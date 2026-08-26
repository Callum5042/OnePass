package com.onepass

import androidx.compose.ui.test.assertCountEquals
import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.assertIsEnabled
import androidx.compose.ui.test.assertIsNotEnabled
import androidx.compose.ui.test.assertIsSelected
import androidx.compose.ui.test.assertTextEquals
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onAllNodesWithContentDescription
import androidx.compose.ui.test.onAllNodesWithTag
import androidx.compose.ui.test.onAllNodesWithText
import androidx.compose.ui.test.onNodeWithContentDescription
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performTextReplacement
import com.onepass.services.Account
import com.onepass.services.CredentialEdits
import com.onepass.services.PasswordHistory
import com.onepass.services.VaultUpdateResult
import com.onepass.ui.theme.OnePassTheme
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Rule
import org.junit.Test
import java.util.UUID

class AccountDetailsScreenTest {
    @get:Rule
    val composeRule = createComposeRule()

    @Test
    fun detailsShowSafeRowsAndToolbarActions() {
        var wentBack = false
        var openedWebsite: String? = null
        setAccountContent(
            account = account(),
            onBack = { wentBack = true },
            onOpenWebsite = { openedWebsite = it },
        )

        composeRule.onNodeWithText("GitHub").assertIsDisplayed()
        composeRule.onNodeWithText("Favourite").assertIsDisplayed()
        composeRule.onNodeWithText("Details").assertIsSelected()
        composeRule.onNodeWithText("callum").assertIsDisplayed()
        composeRule.onNodeWithText("callum@example.com").assertIsDisplayed()
        composeRule.onAllNodesWithText("current-secret").assertCountEquals(0)
        composeRule.onNodeWithContentDescription("Edit account details").assertIsEnabled()

        composeRule.onNodeWithContentDescription("Open website").performClick()
        assertEquals("https://github.com", openedWebsite)

        composeRule.onNodeWithContentDescription("Account information").performClick()
        composeRule.onNodeWithText("Record details").assertIsDisplayed()
        composeRule.onNodeWithText("Created").assertIsDisplayed()

        composeRule.onNodeWithContentDescription("Back").performClick()
        assertTrue(wentBack)
    }

    @Test
    fun currentPasswordCanRevealAndCopyUnderlyingValue() {
        var copied: String? = null
        setAccountContent(account = account(), onCopy = { copied = it })

        composeRule.onNodeWithContentDescription("Show password").performClick()
        composeRule.onNodeWithText("current-secret").assertIsDisplayed()
        composeRule.onNodeWithContentDescription("Hide password").assertIsDisplayed()

        composeRule.onNodeWithContentDescription("Copy password").performClick()
        assertEquals("current-secret", copied)
    }

    @Test
    fun notesTabShowsPopulatedState() {
        setAccountContent(account = account())
        composeRule.onNodeWithText("Notes").performClick()
        composeRule.onNodeWithText("Recovery codes are stored offline.").assertIsDisplayed()
    }

    @Test
    fun notesTabShowsEmptyState() {
        setAccountContent(account = account(notes = " "))
        composeRule.onNodeWithText("Notes").performClick()
        composeRule.onNodeWithText("No notes have been added to this account.").assertIsDisplayed()
    }

    @Test
    fun historyTabSortsMasksRevealsAndCopies() {
        var copied: String? = null
        setAccountContent(account = account(), onCopy = { copied = it })

        composeRule.onNodeWithText("History").performClick()
        composeRule.onNodeWithText("History").assertIsSelected()
        composeRule.onAllNodesWithText("new-secret").assertCountEquals(0)
        composeRule.onAllNodesWithText("old-secret").assertCountEquals(0)

        composeRule.onAllNodesWithContentDescription("Show password")[0].performClick()
        composeRule.onNodeWithText("new-secret").assertIsDisplayed()
        composeRule.onAllNodesWithContentDescription("Copy password")[0].performClick()
        assertEquals("new-secret", copied)
    }

    @Test
    fun historyTabShowsEmptyState() {
        setAccountContent(account = account(history = emptyList()))

        composeRule.onNodeWithText("History").performClick()
        composeRule.onNodeWithText("No previous passwords are stored for this account.")
            .assertIsDisplayed()
    }

    @Test
    fun missingValuesUseFallbacksAndInvalidWebsiteHasNoOpenAction() {
        setAccountContent(
            account = account(
                username = null,
                email = " ",
                password = null,
                website = "example.com",
            ),
        )

        composeRule.onAllNodesWithText("Not set").assertCountEquals(3)
        composeRule.onAllNodesWithText("example.com").assertCountEquals(1)
        composeRule.onAllNodesWithContentDescription("Open website").assertCountEquals(0)
    }

    @Test
    fun editShowsCredentialFieldsAndKeepsDraftAcrossTabs() {
        setAccountContent(account = account())

        composeRule.onNodeWithContentDescription("Edit account details").performClick()

        composeRule.onNodeWithTag("edit_username").assertTextEquals("callum")
        composeRule.onNodeWithTag("edit_email").assertTextEquals("callum@example.com")
        composeRule.onNodeWithTag("edit_website").assertTextEquals("https://github.com")
        composeRule.onNodeWithContentDescription("Show password").assertIsDisplayed()
        composeRule.onNodeWithContentDescription("Save account details").assertIsNotEnabled()

        composeRule.onNodeWithTag("edit_username").performTextReplacement("  changed user  ")
        composeRule.onNodeWithContentDescription("Save account details").assertIsEnabled()
        composeRule.onNodeWithText("Notes").performClick()
        composeRule.onNodeWithText("Details").performClick()
        composeRule.onNodeWithTag("edit_username").assertTextEquals("  changed user  ")
    }

    @Test
    fun notesCanBeEditedAndSavedWithExactValue() {
        var saved: CredentialEdits? = null
        setAccountContent(
            account = account(),
            onSaveCredentials = {
                saved = it
                VaultUpdateResult.Success
            },
        )

        composeRule.onNodeWithContentDescription("Edit account details").performClick()
        composeRule.onNodeWithText("Notes").performClick()
        composeRule.onNodeWithTag("add_notes")
            .performTextReplacement("  updated recovery notes  ")

        composeRule.onNodeWithText("Details").performClick()
        composeRule.onNodeWithText("Notes").performClick()
        composeRule.onNodeWithTag("add_notes")
            .assertTextEquals("  updated recovery notes  ")

        composeRule.onNodeWithContentDescription("Save account details").performClick()
        composeRule.waitForIdle()

        assertEquals("  updated recovery notes  ", saved?.notes)
        composeRule.onNodeWithText("Account saved").assertIsDisplayed()
    }

    @Test
    fun backDiscardsDraftWithoutSaving() {
        var saved = false
        setAccountContent(
            account = account(),
            onSaveCredentials = {
                saved = true
                VaultUpdateResult.Success
            },
        )
        composeRule.onNodeWithContentDescription("Edit account details").performClick()
        composeRule.onNodeWithTag("edit_username").performTextReplacement("changed")

        composeRule.onNodeWithContentDescription("Back").performClick()

        composeRule.onNodeWithText("callum").assertIsDisplayed()
        composeRule.onAllNodesWithTag("edit_username").assertCountEquals(0)
        assertEquals(false, saved)
    }

    @Test
    fun successfulSaveSubmitsExactDraftAndReturnsToReadMode() {
        var saved: CredentialEdits? = null
        setAccountContent(
            account = account(),
            onSaveCredentials = {
                saved = it
                VaultUpdateResult.Success
            },
        )
        composeRule.onNodeWithContentDescription("Edit account details").performClick()
        composeRule.onNodeWithTag("edit_email").performTextReplacement("  exact@example.com  ")

        composeRule.onNodeWithContentDescription("Save account details").performClick()
        composeRule.waitForIdle()

        assertEquals("  exact@example.com  ", saved?.emailAddress)
        assertEquals("callum", saved?.username)
        composeRule.onNodeWithContentDescription("Edit account details").assertIsDisplayed()
        composeRule.onNodeWithText("Account saved").assertIsDisplayed()
    }

    @Test
    fun failedSaveKeepsDraftForRetry() {
        setAccountContent(
            account = account(),
            onSaveCredentials = { VaultUpdateResult.SaveFailed(rollbackSucceeded = true) },
        )
        composeRule.onNodeWithContentDescription("Edit account details").performClick()
        composeRule.onNodeWithTag("edit_website").performTextReplacement(" exact site ")

        composeRule.onNodeWithContentDescription("Save account details").performClick()
        composeRule.waitForIdle()

        composeRule.onNodeWithTag("edit_website").assertTextEquals(" exact site ")
        composeRule.onNodeWithText("Couldn’t save the account. The original vault was restored.")
            .assertIsDisplayed()
    }

    private fun setAccountContent(
        account: Account,
        onBack: () -> Unit = {},
        onOpenWebsite: (String) -> Unit = {},
        onCopy: (String) -> Unit = {},
        onSaveCredentials: suspend (CredentialEdits) -> VaultUpdateResult = {
            VaultUpdateResult.Success
        },
    ) {
        composeRule.setContent {
            OnePassTheme {
                AccountDetailsScreen(
                    account = account,
                    onBack = onBack,
                    onOpenWebsite = onOpenWebsite,
                    onCopy = onCopy,
                    onSaveCredentials = onSaveCredentials,
                )
            }
        }
    }

    private fun account(
        username: String? = "callum",
        email: String? = "callum@example.com",
        password: String? = "current-secret",
        website: String? = "https://github.com",
        notes: String? = "Recovery codes are stored offline.",
        history: List<PasswordHistory> = listOf(
            PasswordHistory(
                guid = UUID.fromString("00000000-0000-0000-0000-000000000101"),
                password = "old-secret",
                dateTime = "2024-01-01T10:00:00Z",
            ),
            PasswordHistory(
                guid = UUID.fromString("00000000-0000-0000-0000-000000000102"),
                password = "new-secret",
                dateTime = "2026-08-20T09:15:00Z",
            ),
        ),
    ) = Account(
        guid = UUID.fromString("00000000-0000-0000-0000-000000000100"),
        dateCreated = "2024-03-14T18:32:00Z",
        dateModified = "2026-08-20T09:15:00Z",
        name = "GitHub",
        username = username,
        emailAddress = email,
        password = password,
        favourite = true,
        websiteUrl = website,
        mfaEnabled = true,
        notes = notes,
        passwordHistory = history,
    )
}
