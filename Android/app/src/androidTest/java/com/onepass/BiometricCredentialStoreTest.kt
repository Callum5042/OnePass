package com.onepass

import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import androidx.test.platform.app.InstrumentationRegistry
import com.onepass.services.BiometricCredentialStore
import org.junit.After
import org.junit.Before
import org.junit.Test
import org.junit.Assert.*
import java.util.UUID
import javax.crypto.KeyGenerator

/** Tests use an isolated Keystore key without authentication to exercise persistence and GCM.
 * Physical fingerprint authorization is deliberately not simulated by these tests. */
class BiometricCredentialStoreTest {
    private val alias = "onepass_test_${UUID.randomUUID()}"
    private lateinit var store: BiometricCredentialStore

    @Before
    fun setUp() {
        val context = InstrumentationRegistry.getInstrumentation().targetContext
        store = BiometricCredentialStore(context, alias, "$alias.json")
        KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES, "AndroidKeyStore").apply {
            init(KeyGenParameterSpec.Builder(alias,
                KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT)
                .setBlockModes(KeyProperties.BLOCK_MODE_GCM)
                .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                .build())
            generateKey()
        }
    }

    @After
    fun cleanUp() { store.clear() }

    @Test
    fun credentialRoundTripsAndCanBeCleared() {
        val uri = "content://test/vault"
        val password = "test-password-秘密".toCharArray()
        val cipher = store.createEncryptionCipher(uri)
        store.saveCredential(uri, password, cipher)
        val record = store.readCredential()!!
        val decrypted = store.decryptCredential(record, store.createDecryptionCipher(record))
        try {
            assertEquals(uri, record.vaultUri)
            assertArrayEquals(password, decrypted)
        } finally {
            password.fill('\u0000')
            decrypted.fill('\u0000')
            record.clear()
        }
        store.clear()
        assertNull(store.readCredential())
    }

    @Test
    fun changingStoredUriFailsAuthentication() {
        val uri = "content://test/vault"
        val password = "test-password".toCharArray()
        try {
            store.saveCredential(uri, password, store.createEncryptionCipher(uri))
        } finally { password.fill('\u0000') }
        val record = store.readCredential()!!.copy(vaultUri = "content://test/another-vault")
        try {
            assertThrows(BiometricCredentialStore.InvalidBiometricCredentialException::class.java) {
                store.decryptCredential(record, store.createDecryptionCipher(record))
            }
        } finally { record.clear() }
    }

    @Test
    fun preparingCipherDoesNotSubmitAadBeforeAuthentication() {
        // Preparing either cipher must only initialize it. A raw round trip with no AAD
        // fails if preparation has already submitted the URI to the Keystore operation.
        val uri = "content://test/vault"
        val cipher = store.createEncryptionCipher(uri)
        val rawCipher = javax.crypto.Cipher.getInstance("AES/GCM/NoPadding")
        val keyStore = java.security.KeyStore.getInstance("AndroidKeyStore").apply { load(null) }
        rawCipher.init(javax.crypto.Cipher.DECRYPT_MODE, keyStore.getKey(alias, null),
            javax.crypto.spec.GCMParameterSpec(128, cipher.iv))
        val encrypted = cipher.doFinal("test-password".toByteArray())
        val record = BiometricCredentialStore.StoredCredential(uri, cipher.iv, encrypted)
        val plaintext = rawCipher.doFinal(encrypted)
        try { assertArrayEquals("test-password".toByteArray(), plaintext) }
        finally { plaintext.fill(0) }
        val preparedDecryption = store.createDecryptionCipher(record).doFinal(encrypted)
        try { assertArrayEquals("test-password".toByteArray(), preparedDecryption) }
        finally { preparedDecryption.fill(0); record.clear() }
    }
}
