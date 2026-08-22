package com.onepass

import com.onepass.services.Account
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Test
import java.util.UUID

class MainActivityTest {
    @Test
    fun accountsAreSortedByFavouriteThenNameWithUnnamedLast() {
        val accounts = listOf(
            account(name = null),
            account(name = "zebra"),
            account(name = "beta", favourite = true),
            account(name = "Alpha"),
            account(name = "alpha", favourite = true),
        )

        assertEquals(
            listOf("alpha", "beta", "Alpha", "zebra", null),
            sortAccounts(accounts).map(Account::name),
        )
    }

    @Test
    fun loginPrefersUsernameThenFallsBackToEmail() {
        assertEquals(
            "user",
            displayLogin(account(username = " user ", emailAddress = "mail@example.com")),
        )
        assertEquals(
            "mail@example.com",
            displayLogin(account(username = " ", emailAddress = " mail@example.com ")),
        )
        assertNull(displayLogin(account(username = null, emailAddress = "")))
    }

    @Test
    fun initialUsesFirstLetterOrDigitAndFallsBackToQuestionMark() {
        assertEquals("G", accountInitial(account(name = " github")))
        assertEquals("4", accountInitial(account(name = "--4shared")))
        assertEquals("?", accountInitial(account(name = "---")))
        assertEquals("?", accountInitial(account(name = null)))
    }

    private fun account(
        name: String? = "Account",
        username: String? = null,
        emailAddress: String? = null,
        favourite: Boolean = false,
    ) = Account(
        guid = UUID.randomUUID(),
        dateCreated = null,
        dateModified = null,
        name = name,
        username = username,
        emailAddress = emailAddress,
        password = "secret",
        favourite = favourite,
        websiteUrl = null,
        mfaEnabled = false,
        notes = null,
        passwordHistory = emptyList(),
    )
}
