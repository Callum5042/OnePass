package com.onepass.services

import android.content.Context
import android.os.Build
import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import android.util.Base64
import android.util.AtomicFile
import org.json.JSONObject
import java.io.File
import java.nio.charset.StandardCharsets
import java.security.KeyStore
import javax.crypto.Cipher
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey
import javax.crypto.spec.GCMParameterSpec

/**
 * Stores one biometric-protected vault password outside of Android backup data.
 *
 * The password is encrypted with an authentication-per-use Keystore key. The
 * selected vault URI is included as GCM authenticated data so the record cannot
 * be moved to another vault without failing authentication.
 */
class BiometricCredentialStore internal constructor(
    context: Context,
    private val keyAlias: String = KEY_ALIAS,
    credentialFileName: String = CREDENTIAL_FILE_NAME,
) {
    private val noBackupDirectory = context.applicationContext.noBackupFilesDir
    private val credentialFile = File(noBackupDirectory, credentialFileName)

    fun readCredential(): StoredCredential? {
        val file = AtomicFile(credentialFile)
        if (!credentialFile.isFile && !File(credentialFile.path + ".bak").isFile) return null

        return try {
            val record = JSONObject(
                file.openRead().bufferedReader(StandardCharsets.UTF_8).use { it.readText() },
            )
            val fields = record.keys().asSequence().toSet()
            require(fields == RECORD_FIELDS) { "Invalid biometric credential record" }

            val vaultUri = record.getString(FIELD_VAULT_URI)
            val iv = Base64.decode(record.getString(FIELD_IV), Base64.DEFAULT)
            val ciphertext = Base64.decode(record.getString(FIELD_CIPHERTEXT), Base64.DEFAULT)
            require(vaultUri.isNotEmpty()) { "Invalid biometric credential URI" }
            require(iv.size == GCM_IV_LENGTH) { "Invalid biometric credential IV" }
            require(ciphertext.isNotEmpty()) { "Invalid biometric credential ciphertext" }

            StoredCredential(vaultUri, iv, ciphertext)
        } catch (error: Exception) {
            throw InvalidBiometricCredentialException(error)
        }
    }

    fun createEncryptionCipher(vaultUri: String): Cipher {
        require(vaultUri.isNotEmpty())
        return Cipher.getInstance(TRANSFORMATION).apply {
            init(Cipher.ENCRYPT_MODE, getOrCreateKey())
        }
    }

    fun saveCredential(vaultUri: String, password: CharArray, cipher: Cipher) {
        require(vaultUri.isNotEmpty())
        val passwordBytes = String(password).toByteArray(StandardCharsets.UTF_8)
        val ciphertext = try {
            // AAD is a Keystore operation too: submit it only after BiometricPrompt succeeds.
            cipher.updateAAD(vaultUri.toByteArray(StandardCharsets.UTF_8))
            cipher.doFinal(passwordBytes)
        } finally {
            passwordBytes.fill(0)
        }

        try {
            val record = JSONObject()
                .put(FIELD_VAULT_URI, vaultUri)
                .put(FIELD_IV, Base64.encodeToString(cipher.iv, Base64.NO_WRAP))
                .put(FIELD_CIPHERTEXT, Base64.encodeToString(ciphertext, Base64.NO_WRAP))
            credentialFile.parentFile?.mkdirs()
            val file = AtomicFile(credentialFile)
            val output = file.startWrite()
            try {
                output.write(record.toString().toByteArray(StandardCharsets.UTF_8))
                file.finishWrite(output)
            } catch (error: Exception) {
                file.failWrite(output)
                throw error
            }
        } finally {
            ciphertext.fill(0)
        }
    }

    fun createDecryptionCipher(credential: StoredCredential): Cipher {
        require(credential.vaultUri.isNotEmpty())
        return Cipher.getInstance(TRANSFORMATION).apply {
            init(
                Cipher.DECRYPT_MODE,
                getExistingKey(),
                GCMParameterSpec(GCM_TAG_LENGTH_BITS, credential.iv),
            )
        }
    }

    fun decryptCredential(credential: StoredCredential, cipher: Cipher): CharArray {
        val passwordBytes = try {
            cipher.updateAAD(credential.vaultUri.toByteArray(StandardCharsets.UTF_8))
            cipher.doFinal(credential.ciphertext)
        } catch (error: Exception) {
            throw InvalidBiometricCredentialException(error)
        }
        return try {
            String(passwordBytes, StandardCharsets.UTF_8).toCharArray()
        } finally {
            passwordBytes.fill(0)
        }
    }

    fun clear() {
        if (credentialFile.exists() && !credentialFile.delete()) {
            throw IllegalStateException("Unable to clear biometric credential")
        }

        keyStore().deleteEntry(keyAlias)
    }

    private fun getOrCreateKey(): SecretKey {
        val keyStore = keyStore()
        val existing = keyStore.getKey(keyAlias, null) as? SecretKey
        if (existing != null) return existing

        val generator = KeyGenerator.getInstance(
            KeyProperties.KEY_ALGORITHM_AES,
            ANDROID_KEY_STORE,
        )
        val keySpec = KeyGenParameterSpec.Builder(
            keyAlias,
            KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT,
        )
            .setBlockModes(KeyProperties.BLOCK_MODE_GCM)
            .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
            .setUserAuthenticationRequired(true)
            .setInvalidatedByBiometricEnrollment(true)

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            keySpec.setUserAuthenticationParameters(
                0,
                KeyProperties.AUTH_BIOMETRIC_STRONG,
            )
        } else {
            @Suppress("DEPRECATION")
            keySpec.setUserAuthenticationValidityDurationSeconds(-1)
        }

        generator.init(keySpec.build())
        return generator.generateKey()
    }

    private fun getExistingKey(): SecretKey =
        (keyStore().getKey(keyAlias, null) as? SecretKey)
            ?: throw IllegalStateException("Biometric credential key is missing")

    private fun keyStore(): KeyStore = KeyStore.getInstance(ANDROID_KEY_STORE).apply {
        load(null)
    }

    data class StoredCredential(
        val vaultUri: String,
        val iv: ByteArray,
        val ciphertext: ByteArray,
    ) {
        fun clear() {
            iv.fill(0)
            ciphertext.fill(0)
        }
    }

    class InvalidBiometricCredentialException(cause: Throwable) :
        IllegalArgumentException("Invalid biometric credential", cause)

    private companion object {
        const val ANDROID_KEY_STORE = "AndroidKeyStore"
        const val KEY_ALIAS = "onepass_biometric_credential"
        const val CREDENTIAL_FILE_NAME = "biometric_credential.json"
        const val TRANSFORMATION = "AES/GCM/NoPadding"
        const val GCM_TAG_LENGTH_BITS = 128
        const val GCM_IV_LENGTH = 12
        const val FIELD_VAULT_URI = "vaultUri"
        const val FIELD_IV = "iv"
        const val FIELD_CIPHERTEXT = "ciphertext"
        val RECORD_FIELDS = setOf(FIELD_VAULT_URI, FIELD_IV, FIELD_CIPHERTEXT)
    }
}
