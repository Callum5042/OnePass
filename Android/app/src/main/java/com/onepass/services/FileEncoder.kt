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
import javax.crypto.Cipher
import javax.crypto.CipherInputStream
import javax.crypto.SecretKeyFactory
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

/** Decodes the binary format produced by the original .NET FileEncoder. */
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
        val header = readHeader(bytes)
        if (!MessageDigest.isEqual(passwordHash(password, header.salt), header.passwordHash)) {
            throw InvalidPasswordException()
        }

        // Match the defaults of the original .NET Rfc2898DeriveBytes and Aes calls.
        val keySpec = PBEKeySpec(password, header.salt, PBKDF2_ITERATIONS, AES_KEY_BITS)
        val key = try {
            SecretKeyFactory.getInstance("PBKDF2WithHmacSHA1").generateSecret(keySpec).encoded
        } finally {
            keySpec.clearPassword()
        }
        val cipher = Cipher.getInstance("AES/CBC/PKCS5Padding").apply {
            init(Cipher.DECRYPT_MODE, SecretKeySpec(key, "AES"), IvParameterSpec(header.iv))
        }
        key.fill(0)
        val json = CipherInputStream(
                ByteArrayInputStream(bytes, header.payloadOffset, bytes.size - header.payloadOffset),
                cipher,
            )
            .bufferedReader(StandardCharsets.UTF_8)
            .use { it.readText() }
        return parse(JSONObject(json))
    }

    override fun save(password: CharArray, data: OnePassData): ByteArray {
        val salt = ByteArray(SALT_LENGTH).also(secureRandom::nextBytes)
        val iv = ByteArray(AES_BLOCK_SIZE).also(secureRandom::nextBytes)
        val keySpec = PBEKeySpec(password, salt, PBKDF2_ITERATIONS, AES_KEY_BITS)
        val key = try {
            SecretKeyFactory.getInstance("PBKDF2WithHmacSHA1").generateSecret(keySpec).encoded
        } finally {
            keySpec.clearPassword()
        }
        val cipher = Cipher.getInstance("AES/CBC/PKCS5Padding").apply {
            init(Cipher.ENCRYPT_MODE, SecretKeySpec(key, "AES"), IvParameterSpec(iv))
        }
        key.fill(0)
        val payload = cipher.doFinal(serialize(data).toByteArray(StandardCharsets.UTF_8))
        val hash = passwordHash(password, salt)

        return ByteArrayOutputStream().use { output ->
            DataOutputStream(output).use { writer ->
                writer.write(FILE_SIGNATURE)
                writer.writeIntLittleEndian(FILE_VERSION)
                writer.writeLengthPrefixed(hash)
                writer.writeLengthPrefixed(salt)
                writer.writeLengthPrefixed(iv)
                writer.write(payload)
            }
            output.toByteArray()
        }
    }

    fun verify(password: String, input: InputStream): Boolean {
        val passwordChars = password.toCharArray()
        return try {
            val header = readHeader(input.readBytes())
            MessageDigest.isEqual(passwordHash(passwordChars, header.salt), header.passwordHash)
        } finally {
            passwordChars.fill('\u0000')
        }
    }

    private fun readHeader(bytes: ByteArray): Header {
        val input = ByteArrayInputStream(bytes)
        if (!input.readExactly(FILE_SIGNATURE.size).contentEquals(FILE_SIGNATURE)) {
            throw InvalidOnePassFileException("Not a valid OnePass file")
        }
        val version = input.readInt32LittleEndian()
        if (version != FILE_VERSION) {
            throw InvalidOnePassFileException("Unsupported OnePass file version: $version")
        }
        val passwordHash = input.readLengthPrefixed(MAX_HASH_LENGTH)
        val salt = input.readLengthPrefixed(MAX_SALT_LENGTH)
        val iv = input.readLengthPrefixed(MAX_IV_LENGTH)
        if (iv.size != AES_BLOCK_SIZE) throw InvalidOnePassFileException("Invalid AES IV length")
        return Header(passwordHash, salt, iv, bytes.size - input.available())
    }

    private fun passwordHash(password: CharArray, salt: ByteArray): ByteArray {
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

    private data class Header(
        val passwordHash: ByteArray,
        val salt: ByteArray,
        val iv: ByteArray,
        val payloadOffset: Int,
    )

    companion object {
        private val FILE_SIGNATURE = ".ONEPASS".toByteArray(StandardCharsets.UTF_8)
        private const val FILE_VERSION = 1
        private const val AES_BLOCK_SIZE = 16
        private const val SALT_LENGTH = 8
        private const val AES_KEY_BITS = 128
        private const val PBKDF2_ITERATIONS = 1_000
        private const val MAX_HASH_LENGTH = 128
        private const val MAX_SALT_LENGTH = 1_024
        private const val MAX_IV_LENGTH = 32
        private val secureRandom = SecureRandom()
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
