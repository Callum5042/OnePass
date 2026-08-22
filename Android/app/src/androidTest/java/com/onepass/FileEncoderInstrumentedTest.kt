package com.onepass

import com.onepass.services.Account
import com.onepass.services.FileEncoder
import com.onepass.services.InvalidPasswordException
import com.onepass.services.OnePassData
import com.onepass.services.PasswordHistory
import org.junit.Assert.assertArrayEquals
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test
import java.io.ByteArrayInputStream
import java.util.UUID

class FileEncoderInstrumentedTest {
    private val password = "vault-password".toCharArray()

    @Test
    fun saveProducesVersionOneFileAndRoundTripsAllValues() {
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
        assertEquals(1, littleEndianInt(bytes, 8))
        assertEquals(data, FileEncoder().load(password, ByteArrayInputStream(bytes)))
    }

    @Test
    fun eachSaveUsesFreshSaltAndIv() {
        val encoder = FileEncoder()
        val data = OnePassData()

        val first = encoder.save(password, data)
        val second = encoder.save(password, data)

        assertFalse(first.contentEquals(second))
        assertTrue(encoder.verify(String(password), ByteArrayInputStream(first)))
        assertTrue(encoder.verify(String(password), ByteArrayInputStream(second)))
    }

    @Test(expected = InvalidPasswordException::class)
    fun wrongPasswordIsRejected() {
        val bytes = FileEncoder().save(password, OnePassData())
        FileEncoder().load("wrong password", ByteArrayInputStream(bytes))
    }

    private fun littleEndianInt(bytes: ByteArray, offset: Int): Int =
        (bytes[offset].toInt() and 0xff) or
            ((bytes[offset + 1].toInt() and 0xff) shl 8) or
            ((bytes[offset + 2].toInt() and 0xff) shl 16) or
            ((bytes[offset + 3].toInt() and 0xff) shl 24)
}
