package com.onepass

import androidx.compose.runtime.Composable
import androidx.compose.ui.test.assertIsEnabled
import androidx.compose.ui.test.assertIsNotEnabled
import androidx.compose.ui.test.assertIsOff
import androidx.compose.ui.test.assertIsOn
import androidx.compose.ui.test.hasScrollToIndexAction
import androidx.compose.ui.test.hasTestTag
import androidx.compose.ui.test.junit4.StateRestorationTester
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithContentDescription
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performScrollToNode
import androidx.compose.ui.test.performTextReplacement
import com.onepass.services.Account
import com.onepass.services.CredentialEdits
import com.onepass.services.NewAccountDetails
import com.onepass.services.VaultAddResult
import com.onepass.services.VaultUpdateResult
import com.onepass.ui.theme.OnePassTheme
import kotlinx.coroutines.CompletableDeferred
import org.junit.Assert.assertEquals
import org.junit.Rule
import org.junit.Test
import java.util.UUID

class FavouriteScreenTest {
    @get:Rule
    val rule = createComposeRule()

    private fun toggle(): androidx.compose.ui.test.SemanticsNodeInteraction {
        rule.onNode(hasScrollToIndexAction()).performScrollToNode(hasTestTag("account_favourite"))
        return rule.onNodeWithTag("account_favourite")
    }
    private fun save() = rule.onNodeWithContentDescription("Save account details")
    private fun edit() = rule.onNodeWithContentDescription("Edit account details").performClick()

    @Test
    fun createDefaultsOffAndPreservesSelectionAcrossTabsAndRestoration() {
        var saved: NewAccountDetails? = null
        val restoration = StateRestorationTester(rule)
        restoration.setContent {
            OnePassTheme {
                AddAccountScreen({}, {}, { saved = it; VaultAddResult.Success })
            }
        }
        rule.onNodeWithTag("add_name").performTextReplacement("New account")
        toggle().assertIsOff().performClick()
        rule.onNodeWithText("Notes").performClick()
        restoration.emulateSavedInstanceStateRestore()
        rule.onNodeWithText("Details").performClick()
        toggle().assertIsOn()
        save().performClick()
        rule.runOnIdle { assertEquals(true, saved?.favourite) }
    }

    @Test
    fun createCanSaveWithFavouriteOff() {
        var saved: NewAccountDetails? = null
        rule.setContent {
            OnePassTheme {
                AddAccountScreen({}, {}, { saved = it; VaultAddResult.Success })
            }
        }
        rule.onNodeWithTag("add_name").performTextReplacement("New account")
        toggle().performClick().performClick().assertIsOff()
        save().performClick()
        rule.runOnIdle { assertEquals(false, saved?.favourite) }
    }

    @Test
    fun editFavouriteAloneControlsDirtyStateAndCancelRestoresSavedValue() {
        rule.setContent { EditContent() }
        edit()
        toggle().assertIsOn().performClick()
        save().assertIsEnabled()
        toggle().performClick()
        save().assertIsNotEnabled()
        toggle().performClick()
        rule.onNodeWithContentDescription("Back").performClick()
        edit()
        toggle().assertIsOn()
        save().assertIsNotEnabled()
    }

    @Test
    fun editPreservesDraftAcrossTabsAndRestorationAndSavesFavouriteOff() {
        var saved: CredentialEdits? = null
        val restoration = StateRestorationTester(rule)
        restoration.setContent {
            EditContent { saved = it; VaultUpdateResult.Success }
        }
        edit()
        toggle().performClick()
        rule.onNodeWithText("Notes").performClick()
        restoration.emulateSavedInstanceStateRestore()
        rule.onNodeWithText("Details").performClick()
        toggle().assertIsOff()
        save().assertIsEnabled().performClick()
        rule.runOnIdle { assertEquals(false, saved?.favourite) }
    }

    @Test
    fun editCanTurnFavouriteOn() {
        var saved: CredentialEdits? = null
        rule.setContent {
            EditContent(favourite = false) { saved = it; VaultUpdateResult.Success }
        }
        edit()
        toggle().assertIsOff().performClick()
        save().performClick()
        rule.runOnIdle { assertEquals(true, saved?.favourite) }
    }

    @Test
    fun editDisablesSwitchDuringSaveAndRetainsFailedDraftForRetry() {
        val pending = CompletableDeferred<VaultUpdateResult>()
        val submissions = mutableListOf<CredentialEdits>()
        rule.setContent {
            EditContent {
                submissions.add(it)
                if (submissions.size == 1) pending.await() else VaultUpdateResult.Success
            }
        }
        edit()
        toggle().performClick()
        save().performClick()
        toggle().assertIsNotEnabled()
        rule.runOnIdle { pending.complete(VaultUpdateResult.SaveFailed(true)) }
        toggle().assertIsEnabled().assertIsOff()
        save().performClick()
        rule.runOnIdle {
            assertEquals(2, submissions.size)
            assertEquals(false, submissions.last().favourite)
            assertEquals(submissions.first(), submissions.last())
        }
    }

    @Test
    fun createDisablesSwitchDuringSaveAndRetainsFailedDraftForRetry() {
        val pending = CompletableDeferred<VaultAddResult>()
        val submissions = mutableListOf<NewAccountDetails>()
        rule.setContent {
            OnePassTheme {
                AddAccountScreen({}, {}, {
                    submissions.add(it)
                    if (submissions.size == 1) pending.await() else VaultAddResult.Success
                })
            }
        }
        rule.onNodeWithTag("add_name").performTextReplacement("New account")
        toggle().performClick()
        save().performClick()
        toggle().assertIsNotEnabled()
        rule.runOnIdle { pending.complete(VaultAddResult.SaveFailed(true)) }
        toggle().assertIsEnabled().assertIsOn()
        save().performClick()
        rule.runOnIdle {
            assertEquals(2, submissions.size)
            assertEquals(true, submissions.last().favourite)
            assertEquals(submissions.first(), submissions.last())
        }
    }

    @Composable
    private fun EditContent(
        favourite: Boolean = true,
        onSave: suspend (CredentialEdits) -> VaultUpdateResult = { VaultUpdateResult.Success },
    ) {
        OnePassTheme {
            AccountDetailsScreen(
                account = Account(
                    guid = UUID.fromString("00000000-0000-0000-0000-000000000001"),
                    name = "Account", dateCreated = null, dateModified = null,
                    username = "user", emailAddress = null, password = "secret",
                    websiteUrl = null, notes = null, favourite = favourite,
                    mfaEnabled = false, passwordHistory = emptyList(),
                ),
                onBack = {}, onOpenWebsite = {}, onCopy = {}, onSaveCredentials = onSave,
            )
        }
    }
}
