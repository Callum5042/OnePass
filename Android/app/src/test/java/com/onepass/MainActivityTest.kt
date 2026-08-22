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

    @Test
    fun searchMatchesNameUsernameAndEmailIgnoringCaseAndWhitespace() {
        val accounts = listOf(
            account(name = "GitHub"),
            account(name = "Work", username = "Callum"),
            account(name = "Mail", emailAddress = "person@example.com"),
            account(name = "Other"),
        )

        assertEquals(listOf("GitHub"), filterAccounts(accounts, "  HUB ").map(Account::name))
        assertEquals(listOf("Work"), filterAccounts(accounts, "CALL").map(Account::name))
        assertEquals(listOf("Mail"), filterAccounts(accounts, "EXAMPLE").map(Account::name))
    }

    @Test
    fun blankSearchReturnsAllAccountsInNormalSortOrder() {
        val accounts = listOf(
            account(name = "zebra"),
            account(name = "Alpha"),
            account(name = "beta", favourite = true),
        )

        assertEquals(
            listOf("beta", "Alpha", "zebra"),
            filterAccounts(accounts, "   ").map(Account::name),
        )
    }

    @Test
    fun searchDoesNotMatchSensitiveOrUnsupportedFields() {
        val accounts = listOf(
            account(
                name = "Account",
                password = "password-needle",
                websiteUrl = "https://website-needle.example",
                notes = "notes-needle",
            ),
        )

        assertEquals(emptyList<Account>(), filterAccounts(accounts, "password-needle"))
        assertEquals(emptyList<Account>(), filterAccounts(accounts, "website-needle"))
        assertEquals(emptyList<Account>(), filterAccounts(accounts, "notes-needle"))
    }

    private fun account(
        name: String? = "Account",
        username: String? = null,
        emailAddress: String? = null,
        favourite: Boolean = false,
        password: String? = "secret",
        websiteUrl: String? = null,
        notes: String? = null,
    ) = Account(
        guid = UUID.randomUUID(),
        dateCreated = null,
        dateModified = null,
        name = name,
        username = username,
        emailAddress = emailAddress,
        password = password,
        favourite = favourite,
        websiteUrl = websiteUrl,
        mfaEnabled = false,
        notes = notes,
        passwordHistory = emptyList(),
    )
}
