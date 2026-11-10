package com.onepass.services

import org.json.JSONArray
import org.json.JSONObject
import java.io.ByteArrayInputStream
import java.io.ByteArrayOutputStream
import java.io.DataOutputStream
import java.io.EOFException
import java.io.InputStream
import java.nio.charset.StandardCharsets
import java.security.MessageDigest
import java.security.SecureRandom
import java.util.UUID
import javax.crypto.AEADBadTagException
import javax.crypto.Cipher
import javax.crypto.SecretKeyFactory
import javax.crypto.spec.GCMParameterSpec
import javax.crypto.spec.IvParameterSpec
import javax.crypto.spec.PBEKeySpec
import javax.crypto.spec.SecretKeySpec

data class OnePassData(
    val accounts: List<Account> = emptyList(),
    val deletedAccounts: List<UUID> = emptyList(),
)

data class Account(
    val guid: UUID,
    val dateCreated: String?,
    val dateModified: String?,
    val name: String?,
    val username: String?,
    val emailAddress: String?,
    val password: String?,
    val favourite: Boolean,
    val websiteUrl: String?,
    val mfaEnabled: Boolean,
    val notes: String?,
    val passwordHistory: List<PasswordHistory> = emptyList(),
)

data class PasswordHistory(
    val guid: UUID,
    val password: String?,
    val dateTime: String?,
)

/**
 * Reads and writes OnePass vault files in the same binary format as the .NET FileEncoder.
 *
 * Version 2 layout (all integers little-endian):
 *   ".ONEPASS" | int32 version | byte kdf id | int32 iterations
 *   | int32 salt length | salt | int32 nonce length | nonce
 *   | AES-256-GCM ciphertext | 16-byte tag
 *
 * Everything before the ciphertext is authenticated as associated data. No password hash is
 * stored: a wrong password is detected by the tag failing. Version 1 files are still readable
 * and are rewritten as version 2 on save.
 */
class FileEncoder : VaultEncoder {
    fun load(password: String, input: InputStream): OnePassData {
        val passwordChars = password.toCharArray()
        return try {
            load(passwordChars, input)
        } finally {
            passwordChars.fill('\u0000')
        }
    }

    fun load(password: CharArray, input: InputStream): OnePassData {
        val bytes = input.readBytes()
        val reader = ByteArrayInputStream(bytes)
        val plaintext = when (val version = readVersion(reader)) {
            LEGACY_FILE_VERSION -> decryptLegacy(password, bytes, reader)
            FILE_VERSION -> decryptCurrent(password, bytes, reader)
            else -> throw InvalidOnePassFileException("Unsupported OnePass file version: $version")
        }
        return try {
            parse(JSONObject(String(plaintext, StandardCharsets.UTF_8)))
        } finally {
            plaintext.fill(0)
        }
    }

    override fun save(password: CharArray, data: OnePassData): ByteArray {
        val salt = ByteArray(SALT_LENGTH).also(secureRandom::nextBytes)
        val nonce = ByteArray(GCM_NONCE_LENGTH).also(secureRandom::nextBytes)

        // The header is authenticated so its settings cannot be altered undetected.
        val header = ByteArrayOutputStream().use { output ->
            DataOutputStream(output).use { writer ->
                writer.write(FILE_SIGNATURE)
                writer.writeIntLittleEndian(FILE_VERSION)
                writer.writeByte(KDF_PBKDF2_SHA256)
                writer.writeIntLittleEndian(PBKDF2_ITERATIONS)
                writer.writeLengthPrefixed(salt)
                writer.writeLengthPrefixed(nonce)
            }
            output.toByteArray()
        }

        val plaintext = serialize(data).toByteArray(StandardCharsets.UTF_8)
        val key = deriveKey(password, salt, PBKDF2_ITERATIONS, PBKDF2_ALGORITHM, AES_KEY_BITS)
        val payload = try {
            Cipher.getInstance(GCM_TRANSFORMATION).run {
                init(
                    Cipher.ENCRYPT_MODE,
                    SecretKeySpec(key, "AES"),
                    GCMParameterSpec(GCM_TAG_LENGTH * 8, nonce),
                )
                updateAAD(header)
                doFinal(plaintext)
            }
        } finally {
            key.fill(0)
            plaintext.fill(0)
        }

        return header + payload
    }

    private fun decryptCurrent(
        password: CharArray,
        bytes: ByteArray,
        reader: ByteArrayInputStream,
    ): ByteArray {
        val kdf = reader.readExactly(1)[0].toInt()
        if (kdf != KDF_PBKDF2_SHA256) {
            throw InvalidOnePassFileException("Unsupported OnePass key derivation: $kdf")
        }
        val iterations = reader.readInt32LittleEndian()
        if (iterations !in 1..MAX_PBKDF2_ITERATIONS) {
            throw InvalidOnePassFileException("Invalid OnePass key derivation settings")
        }
        val salt = reader.readLengthPrefixed(MAX_SALT_LENGTH)
        val nonce = reader.readLengthPrefixed(GCM_NONCE_LENGTH)
        if (nonce.size != GCM_NONCE_LENGTH) {
            throw InvalidOnePassFileException("Invalid OnePass nonce length")
        }
        val headerLength = bytes.size - reader.available()
        if (bytes.size - headerLength < GCM_TAG_LENGTH) {
            throw EOFException("Unexpected end of OnePass file")
        }

        val key = deriveKey(password, salt, iterations, PBKDF2_ALGORITHM, AES_KEY_BITS)
        return try {
            Cipher.getInstance(GCM_TRANSFORMATION).run {
                init(
                    Cipher.DECRYPT_MODE,
                    SecretKeySpec(key, "AES"),
                    GCMParameterSpec(GCM_TAG_LENGTH * 8, nonce),
                )
                updateAAD(bytes, 0, headerLength)
                doFinal(bytes, headerLength, bytes.size - headerLength)
            }
        } catch (_: AEADBadTagException) {
            // Also what a damaged or tampered file looks like: both fail the same check.
            throw InvalidPasswordException()
        } finally {
            key.fill(0)
        }
    }

    /**
     * Version 1: SHA-512(password + salt) stored in the header, PBKDF2-HMAC-SHA1 with 1,000
     * iterations and AES-128-CBC. Read-only, kept so old vaults still open.
     */
    private fun decryptLegacy(
        password: CharArray,
        bytes: ByteArray,
        reader: ByteArrayInputStream,
    ): ByteArray {
        val passwordHash = reader.readLengthPrefixed(LEGACY_MAX_HASH_LENGTH)
        val salt = reader.readLengthPrefixed(MAX_SALT_LENGTH)
        val iv = reader.readLengthPrefixed(LEGACY_MAX_IV_LENGTH)
        if (iv.size != LEGACY_IV_LENGTH) throw InvalidOnePassFileException("Invalid AES IV length")
        val payloadOffset = bytes.size - reader.available()

        if (!MessageDigest.isEqual(legacyPasswordHash(password, salt), passwordHash)) {
            throw InvalidPasswordException()
        }

        val key = deriveKey(
            password,
            salt,
            LEGACY_PBKDF2_ITERATIONS,
            LEGACY_PBKDF2_ALGORITHM,
            LEGACY_AES_KEY_BITS,
        )
        return try {
            Cipher.getInstance("AES/CBC/PKCS5Padding").run {
                init(Cipher.DECRYPT_MODE, SecretKeySpec(key, "AES"), IvParameterSpec(iv))
                doFinal(bytes, payloadOffset, bytes.size - payloadOffset)
            }
        } finally {
            key.fill(0)
        }
    }

    private fun deriveKey(
        password: CharArray,
        salt: ByteArray,
        iterations: Int,
        algorithm: String,
        keyBits: Int,
    ): ByteArray {
        val keySpec = PBEKeySpec(password, salt, iterations, keyBits)
        return try {
            SecretKeyFactory.getInstance(algorithm).generateSecret(keySpec).encoded
        } finally {
            keySpec.clearPassword()
        }
    }

    private fun legacyPasswordHash(password: CharArray, salt: ByteArray): ByteArray {
        val passwordBytes = String(password).toByteArray(StandardCharsets.UTF_8)
        return try {
            MessageDigest.getInstance("SHA-512").digest(passwordBytes + salt)
        } finally {
            passwordBytes.fill(0)
        }
    }

    private fun parse(json: JSONObject) = OnePassData(
        accounts = json.arrayOrEmpty("Accounts").objects().map { account ->
            Account(
                guid = UUID.fromString(account.getString("Guid")),
                dateCreated = account.nullableString("DateCreated"),
                dateModified = account.nullableString("DateModified"),
                name = account.nullableString("Name"),
                username = account.nullableString("Username"),
                emailAddress = account.nullableString("EmailAddress"),
                password = account.nullableString("Password"),
                favourite = account.optBoolean("Favourite"),
                websiteUrl = account.nullableString("WebsiteUrl"),
                mfaEnabled = account.optBoolean("MfaEnabled"),
                notes = account.nullableString("Notes"),
                passwordHistory = account.arrayOrEmpty("PasswordHistory").objects().map { history ->
                    PasswordHistory(
                        guid = UUID.fromString(history.getString("Guid")),
                        password = history.nullableString("Password"),
                        dateTime = history.nullableString("DateTime"),
                    )
                }.toList(),
            )
        }.toList(),
        deletedAccounts = json.arrayOrEmpty("DeletedAccounts").strings().map(UUID::fromString).toList(),
    )

    private fun serialize(data: OnePassData): String = JSONObject().apply {
        put("Accounts", JSONArray().apply {
            data.accounts.forEach { account ->
                put(JSONObject().apply {
                    put("Guid", account.guid.toString())
                    putNullable("DateCreated", account.dateCreated)
                    putNullable("DateModified", account.dateModified)
                    putNullable("Name", account.name)
                    putNullable("Username", account.username)
                    putNullable("EmailAddress", account.emailAddress)
                    putNullable("Password", account.password)
                    put("Favourite", account.favourite)
                    putNullable("WebsiteUrl", account.websiteUrl)
                    put("MfaEnabled", account.mfaEnabled)
                    putNullable("Notes", account.notes)
                    put("PasswordHistory", JSONArray().apply {
                        account.passwordHistory.forEach { history ->
                            put(JSONObject().apply {
                                put("Guid", history.guid.toString())
                                putNullable("Password", history.password)
                                putNullable("DateTime", history.dateTime)
                            })
                        }
                    })
                })
            }
        })
        put("DeletedAccounts", JSONArray().apply {
            data.deletedAccounts.forEach { put(it.toString()) }
        })
    }.toString()

    companion object {
        private val FILE_SIGNATURE = ".ONEPASS".toByteArray(StandardCharsets.UTF_8)
        private const val LEGACY_FILE_VERSION = 1
        private const val FILE_VERSION = 2

        private const val KDF_PBKDF2_SHA256 = 1
        private const val PBKDF2_ALGORITHM = "PBKDF2WithHmacSHA256"
        private const val PBKDF2_ITERATIONS = 600_000
        private const val MAX_PBKDF2_ITERATIONS = 10_000_000
        private const val AES_KEY_BITS = 256
        private const val SALT_LENGTH = 16
        private const val MAX_SALT_LENGTH = 1_024
        private const val GCM_TRANSFORMATION = "AES/GCM/NoPadding"
        private const val GCM_NONCE_LENGTH = 12
        private const val GCM_TAG_LENGTH = 16

        private const val LEGACY_PBKDF2_ALGORITHM = "PBKDF2WithHmacSHA1"
        private const val LEGACY_PBKDF2_ITERATIONS = 1_000
        private const val LEGACY_AES_KEY_BITS = 128
        private const val LEGACY_IV_LENGTH = 16
        private const val LEGACY_MAX_IV_LENGTH = 32
        private const val LEGACY_MAX_HASH_LENGTH = 128

        private val secureRandom = SecureRandom()

        /** True when [bytes] is a vault in an older file format that the next save replaces. */
        fun needsUpgrade(bytes: ByteArray): Boolean =
            readVersion(ByteArrayInputStream(bytes)) < FILE_VERSION

        private fun readVersion(input: InputStream): Int {
            if (!input.readExactly(FILE_SIGNATURE.size).contentEquals(FILE_SIGNATURE)) {
                throw InvalidOnePassFileException("Not a valid OnePass file")
            }
            return input.readInt32LittleEndian()
        }
    }
}

class InvalidOnePassFileException(message: String) : IllegalArgumentException(message)
class InvalidPasswordException : IllegalArgumentException("Incorrect password")

private fun InputStream.readExactly(length: Int): ByteArray {
    val result = ByteArray(length)
    var offset = 0
    while (offset < length) {
        val count = read(result, offset, length - offset)
        if (count < 0) throw EOFException("Unexpected end of OnePass file")
        offset += count
    }
    return result
}

private fun InputStream.readInt32LittleEndian(): Int {
    val bytes = readExactly(Int.SIZE_BYTES)
    return (bytes[0].toInt() and 0xff) or
        ((bytes[1].toInt() and 0xff) shl 8) or
        ((bytes[2].toInt() and 0xff) shl 16) or
        ((bytes[3].toInt() and 0xff) shl 24)
}

private fun InputStream.readLengthPrefixed(maxLength: Int): ByteArray {
    val length = readInt32LittleEndian()
    if (length !in 1..maxLength) throw InvalidOnePassFileException("Invalid field length: $length")
    return readExactly(length)
}

private fun JSONObject.arrayOrEmpty(name: String): JSONArray = optJSONArray(name) ?: JSONArray()
private fun JSONObject.nullableString(name: String): String? =
    if (!has(name) || isNull(name)) null else getString(name)

private fun JSONArray.objects(): Sequence<JSONObject> = sequence {
    for (index in 0 until length()) yield(getJSONObject(index))
}

private fun JSONArray.strings(): Sequence<String> = sequence {
    for (index in 0 until length()) yield(getString(index))
}

private fun JSONObject.putNullable(name: String, value: String?) {
    put(name, value ?: JSONObject.NULL)
}

private fun DataOutputStream.writeIntLittleEndian(value: Int) {
    writeByte(value and 0xff)
    writeByte((value ushr 8) and 0xff)
    writeByte((value ushr 16) and 0xff)
    writeByte((value ushr 24) and 0xff)
}

private fun DataOutputStream.writeLengthPrefixed(bytes: ByteArray) {
    writeIntLittleEndian(bytes.size)
    write(bytes)
}
