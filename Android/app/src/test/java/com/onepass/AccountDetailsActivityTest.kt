package com.onepass

import com.onepass.services.Account
import com.onepass.services.PasswordHistory
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Test
import java.time.ZoneOffset
import java.util.Locale
import java.util.UUID

class AccountDetailsActivityTest {
    @Test
    fun accountLookupRequiresMatchingValidGuid() {
        val account = account()

        assertEquals(account, findAccount(listOf(account), account.guid.toString()))
        assertNull(findAccount(listOf(account), "not-a-guid"))
        assertNull(findAccount(listOf(account), UUID.randomUUID().toString()))
        assertNull(findAccount(listOf(account), null))
    }

    @Test
    fun websiteOnlyAllowsAbsoluteHttpUrls() {
        assertEquals("https://example.com/path", normalizedHttpUrl(" https://example.com/path "))
        assertEquals("http://example.com", normalizedHttpUrl("http://example.com"))
        assertNull(normalizedHttpUrl("example.com"))
        assertNull(normalizedHttpUrl("javascript:alert(1)"))
        assertNull(normalizedHttpUrl("https:///missing-host"))
        assertNull(normalizedHttpUrl(" "))
    }

    @Test
    fun passwordHistorySortsNewestFirstAndUnknownDatesLast() {
        val oldest = history("00000000-0000-0000-0000-000000000001", "2024-01-01T10:00:00Z")
        val newest = history("00000000-0000-0000-0000-000000000002", "2026-08-20T09:15:00Z")
        val unknown = history("00000000-0000-0000-0000-000000000003", "legacy date")

        assertEquals(
            listOf(newest, oldest, unknown),
            sortPasswordHistory(listOf(unknown, oldest, newest)),
        )
    }

    @Test
    fun dateFormattingUsesLocaleAndPreservesUnknownValues() {
        val formatted = formatAccountDate(
            value = "2026-08-20T09:15:00Z",
            locale = Locale.UK,
            zoneId = ZoneOffset.UTC,
        )

        checkNotNull(formatted)
        assertTrue(formatted.contains("20"))
        assertTrue(formatted.contains("2026"))
        assertEquals("legacy date", formatAccountDate(" legacy date ", Locale.UK, ZoneOffset.UTC))
        assertNull(formatAccountDate(" ", Locale.UK, ZoneOffset.UTC))
    }

    private fun account() = Account(
        guid = UUID.fromString("00000000-0000-0000-0000-000000000010"),
        dateCreated = null,
        dateModified = null,
        name = "Account",
        username = null,
        emailAddress = null,
        password = null,
        favourite = false,
        websiteUrl = null,
        mfaEnabled = false,
        notes = null,
        passwordHistory = emptyList(),
    )

    private fun history(guid: String, dateTime: String) = PasswordHistory(
        guid = UUID.fromString(guid),
        password = "secret",
        dateTime = dateTime,
    )
}
