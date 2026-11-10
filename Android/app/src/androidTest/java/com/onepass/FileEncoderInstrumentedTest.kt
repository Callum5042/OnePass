package com.onepass

import androidx.test.platform.app.InstrumentationRegistry
import com.onepass.services.Account
import com.onepass.services.FileEncoder
import com.onepass.services.InvalidPasswordException
import com.onepass.services.InvalidOnePassFileException
import com.onepass.services.OnePassData
import com.onepass.services.PasswordHistory
import org.junit.Assert.assertArrayEquals
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertThrows
import org.junit.Assert.assertTrue
import org.junit.Test
import java.io.ByteArrayInputStream
import java.io.EOFException
import java.security.MessageDigest
import java.util.UUID

class FileEncoderInstrumentedTest {
    private val password = "vault-pässword-秘密".toCharArray()
    private val fixturePassword = "fixture-password".toCharArray()

    @Test
    fun saveProducesVersionTwoFileAndRoundTripsAllValues() {
        val accountGuid = UUID.fromString("00000000-0000-0000-0000-000000000010")
        val deletedGuid = UUID.fromString("00000000-0000-0000-0000-000000000020")
        val data = OnePassData(
            accounts = listOf(
                Account(
                    guid = accountGuid,
                    dateCreated = null,
                    dateModified = "2026-08-22T12:00:00Z",
                    name = "Ünicode account",
                    username = "  exact user  ",
                    emailAddress = "",
                    password = "秘密",
                    favourite = true,
                    websiteUrl = null,
                    mfaEnabled = true,
                    notes = "line one\nline two",
                    passwordHistory = listOf(
                        PasswordHistory(
                            UUID.fromString("00000000-0000-0000-0000-000000000030"),
                            "older",
                            null,
                        ),
                    ),
                ),
            ),
            deletedAccounts = listOf(deletedGuid),
        )

        val bytes = FileEncoder().save(password, data)

        assertArrayEquals(".ONEPASS".encodeToByteArray(), bytes.copyOfRange(0, 8))
        assertEquals(2, littleEndianInt(bytes, 8))
        assertEquals(1, bytes[12].toInt())
        assertEquals(600_000, littleEndianInt(bytes, 13))
        assertEquals(16, littleEndianInt(bytes, SALT_OFFSET - 4))
        assertEquals(12, littleEndianInt(bytes, NONCE_OFFSET - 4))
        assertFalse(FileEncoder.needsUpgrade(bytes))
        assertEquals(data, FileEncoder().load(password, ByteArrayInputStream(bytes)))
    }

    @Test
    fun saveDoesNotStoreThePasswordHash() {
        val bytes = FileEncoder().save(password, OnePassData())
        val salt = bytes.copyOfRange(SALT_OFFSET, SALT_OFFSET + 16)
        val legacyHash = MessageDigest.getInstance("SHA-512")
            .digest(String(password).toByteArray(Charsets.UTF_8) + salt)

        assertFalse(bytes.indices.any { start ->
            start + legacyHash.size <= bytes.size &&
                bytes.copyOfRange(start, start + legacyHash.size).contentEquals(legacyHash)
        })
    }

    @Test
    fun eachSaveUsesFreshSaltAndNonce() {
        val encoder = FileEncoder()
        val data = OnePassData()

        val first = encoder.save(password, data)
        val second = encoder.save(password, data)

        assertFalse(
            first.copyOfRange(SALT_OFFSET, SALT_OFFSET + 16)
                .contentEquals(second.copyOfRange(SALT_OFFSET, SALT_OFFSET + 16)),
        )
        assertFalse(
            first.copyOfRange(NONCE_OFFSET, NONCE_OFFSET + 12)
                .contentEquals(second.copyOfRange(NONCE_OFFSET, NONCE_OFFSET + 12)),
        )
    }

    @Test(expected = InvalidPasswordException::class)
    fun wrongPasswordIsRejected() {
        val bytes = FileEncoder().save(password, OnePassData())
        FileEncoder().load("wrong password", ByteArrayInputStream(bytes))
    }

    @Test
    fun tamperedFileFailsAuthentication() {
        val original = FileEncoder().save(password, OnePassData())

        for (offset in listOf(SALT_OFFSET, NONCE_OFFSET, HEADER_LENGTH, original.size - 1)) {
            val tampered = original.copyOf()
            tampered[offset] = (tampered[offset].toInt() xor 0x01).toByte()

            assertThrows("offset $offset", InvalidPasswordException::class.java) {
                FileEncoder().load(password, ByteArrayInputStream(tampered))
            }
        }
    }

    @Test(expected = InvalidOnePassFileException::class)
    fun unsupportedVersionIsRejected() {
        val bytes = FileEncoder().save(password, OnePassData())
        bytes[8] = 3
        FileEncoder().load(password, ByteArrayInputStream(bytes))
    }

    @Test(expected = EOFException::class)
    fun truncatedFileIsRejected() {
        val bytes = FileEncoder().save(password, OnePassData())
        FileEncoder().load(password, ByteArrayInputStream(bytes.copyOf(30)))
    }

    @Test
    fun versionOneFileStillLoadsAndIsFlaggedForUpgrade() {
        val bytes = fixture("vault-v1.bin")

        assertEquals(1, littleEndianInt(bytes, 8))
        assertTrue(FileEncoder.needsUpgrade(bytes))
        assertFixtureVault(FileEncoder().load(fixturePassword, ByteArrayInputStream(bytes)))
    }

    @Test(expected = InvalidPasswordException::class)
    fun versionOneFileRejectsWrongPassword() {
        FileEncoder().load("wrong password", ByteArrayInputStream(fixture("vault-v1.bin")))
    }

    @Test
    fun versionOneFileIsRewrittenAsVersionTwoOnSave() {
        val data = FileEncoder().load(fixturePassword, ByteArrayInputStream(fixture("vault-v1.bin")))

        val upgraded = FileEncoder().save(fixturePassword, data)

        assertFalse(FileEncoder.needsUpgrade(upgraded))
        assertFixtureVault(FileEncoder().load(fixturePassword, ByteArrayInputStream(upgraded)))
    }

    @Test
    fun versionTwoFileWrittenByWindowsLoads() {
        val bytes = fixture("vault-v2-windows.bin")

        assertEquals(2, littleEndianInt(bytes, 8))
        assertFixtureVault(FileEncoder().load(password, ByteArrayInputStream(bytes)))
    }

    @Test(expected = EOFException::class)
    fun emptyFileIsRejected() {
        FileEncoder().load(password, ByteArrayInputStream(ByteArray(0)))
    }

    @Test(expected = InvalidOnePassFileException::class)
    fun nonOnePassFileIsRejected() {
        FileEncoder().load(password, ByteArrayInputStream("not a OnePass file".encodeToByteArray()))
    }

    private fun assertFixtureVault(data: OnePassData) {
        val account = data.accounts.single()
        assertEquals(UUID.fromString("00000000-0000-0000-0000-000000000010"), account.guid)
        assertEquals("Ünicode account", account.name)
        assertEquals("fixture-user", account.username)
        assertEquals("user@example.com", account.emailAddress)
        assertEquals("秘密", account.password)
        assertTrue(account.favourite)
        assertTrue(account.mfaEnabled)
        assertEquals("line one\nline two", account.notes)
        assertEquals("older", account.passwordHistory.single().password)
        assertEquals(
            listOf(UUID.fromString("00000000-0000-0000-0000-000000000020")),
            data.deletedAccounts,
        )
    }

    private fun fixture(name: String): ByteArray =
        InstrumentationRegistry.getInstrumentation().context.assets.open(name).use { it.readBytes() }

    private fun littleEndianInt(bytes: ByteArray, offset: Int): Int =
        (bytes[offset].toInt() and 0xff) or
            ((bytes[offset + 1].toInt() and 0xff) shl 8) or
            ((bytes[offset + 2].toInt() and 0xff) shl 16) or
            ((bytes[offset + 3].toInt() and 0xff) shl 24)

    private companion object {
        // ".ONEPASS" + version + kdf id + iterations + salt length + salt + nonce length + nonce
        const val SALT_OFFSET = 8 + 4 + 1 + 4 + 4
        const val NONCE_OFFSET = SALT_OFFSET + 16 + 4
        const val HEADER_LENGTH = NONCE_OFFSET + 12
    }
}
