package com.onepass

import androidx.compose.ui.test.assertCountEquals
import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.assertIsEnabled
import androidx.compose.ui.test.assertIsNotEnabled
import androidx.compose.ui.test.assertIsSelected
import androidx.compose.ui.test.assertTextEquals
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onAllNodesWithText
import androidx.compose.ui.test.onNodeWithContentDescription
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performTextReplacement
import com.onepass.services.NewAccountDetails
import com.onepass.services.VaultAddResult
import com.onepass.ui.theme.OnePassTheme
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Rule
import org.junit.Test

class AddAccountScreenTest {
    @get:Rule
    val composeRule = createComposeRule()

    @Test
    fun addUsesEditFieldsAndHasNoHistoryTab() {
        setAddContent()

        composeRule.onNodeWithText("Add account details").assertIsDisplayed()
        composeRule.onNodeWithText("Details").assertIsSelected()
        composeRule.onNodeWithTag("add_name").assertIsDisplayed()
        composeRule.onNodeWithTag("add_username").assertIsDisplayed()
        composeRule.onNodeWithTag("add_email").assertIsDisplayed()
        composeRule.onNodeWithTag("add_password").assertIsDisplayed()
        composeRule.onNodeWithTag("add_website").assertIsDisplayed()
        composeRule.onAllNodesWithText("History").assertCountEquals(0)
        composeRule.onNodeWithContentDescription("Save account details").assertIsNotEnabled()

        composeRule.onNodeWithTag("add_name").performTextReplacement("GitHub")
        composeRule.onNodeWithContentDescription("Save account details").assertIsEnabled()
        composeRule.onNodeWithContentDescription("Show password").assertIsDisplayed()
    }

    @Test
    fun notesDraftSurvivesTabChangesAndSuccessfulSaveReturnsExactValues() {
        var saved: NewAccountDetails? = null
        var finished = false
        setAddContent(
            onSaveAccount = {
                saved = it
                VaultAddResult.Success
            },
            onAccountAdded = { finished = true },
        )

        composeRule.onNodeWithTag("add_name").performTextReplacement("  Product  ")
        composeRule.onNodeWithTag("add_email").performTextReplacement(" exact@example.com ")
        composeRule.onNodeWithText("Notes").performClick()
        composeRule.onNodeWithTag("add_notes").performTextReplacement("  exact notes  ")
        composeRule.onNodeWithText("Details").performClick()
        composeRule.onNodeWithText("Notes").performClick()
        composeRule.onNodeWithTag("add_notes").assertTextEquals("  exact notes  ")

        composeRule.onNodeWithContentDescription("Save account details").performClick()
        composeRule.waitForIdle()

        assertEquals("  Product  ", saved?.name)
        assertEquals(" exact@example.com ", saved?.emailAddress)
        assertEquals("  exact notes  ", saved?.notes)
        assertTrue(finished)
    }

    private fun setAddContent(
        onSaveAccount: suspend (NewAccountDetails) -> VaultAddResult = {
            VaultAddResult.Success
        },
        onAccountAdded: () -> Unit = {},
    ) {
        composeRule.setContent {
            OnePassTheme {
                AddAccountScreen(
                    onBack = {},
                    onAccountAdded = onAccountAdded,
                    onSaveAccount = onSaveAccount,
                )
            }
        }
    }
}
