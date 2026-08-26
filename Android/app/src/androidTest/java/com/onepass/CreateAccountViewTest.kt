package com.onepass

import androidx.compose.ui.test.assertCountEquals
import androidx.compose.ui.test.assertHasClickAction
import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.assertTextEquals
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onAllNodesWithContentDescription
import androidx.compose.ui.test.onNodeWithContentDescription
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performTextReplacement
import com.onepass.ui.theme.OnePassTheme
import org.junit.Assert.assertTrue
import org.junit.Rule
import org.junit.Test

class CreateAccountViewTest {
    @get:Rule
    val composeRule = createComposeRule()

    @Test
    fun createAccountPageDisplaysExpectedControls() {
        setCreateAccountContent()

        composeRule.onNodeWithContentDescription("Logo").assertIsDisplayed()
        composeRule.onNodeWithTag("choose_vault_button")
            .assertIsDisplayed()
            .assertHasClickAction()
        composeRule.onNodeWithText("Create file ...").assertIsDisplayed()
        composeRule.onNodeWithTag("create_password_input").assertIsDisplayed()
        composeRule.onNodeWithTag("repeat_password_input").assertIsDisplayed()
        composeRule.onNodeWithTag("create_account_button")
            .assertIsDisplayed()
            .assertHasClickAction()
        composeRule.onNodeWithTag("back_to_login").assertIsDisplayed()
    }

    @Test
    fun submittingEmptyFormShowsValidationErrors() {
        setCreateAccountContent()

        composeRule.onNodeWithTag("create_account_button").performClick()

        composeRule.onNodeWithText("Choose a file for your vault").assertIsDisplayed()
        composeRule.onNodeWithText("Password must be longer than 10 characters").assertIsDisplayed()
    }

    @Test
    fun shortPasswordShowsPasswordValidationError() {
        setCreateAccountContent()

        composeRule.onNodeWithTag("create_password_input").performTextReplacement("1234567890")
        composeRule.onNodeWithTag("repeat_password_input").performTextReplacement("1234567890")
        composeRule.onNodeWithTag("create_account_button").performClick()

        composeRule.onNodeWithTag("password_error").assertTextEquals(
            "Password must be longer than 10 characters",
        )
        composeRule.onNodeWithTag("repeat_password_error").assertDoesNotExist()
    }

    @Test
    fun mismatchedPasswordsShowRepeatPasswordValidationError() {
        setCreateAccountContent()

        composeRule.onNodeWithTag("create_password_input").performTextReplacement("12345678901")
        composeRule.onNodeWithTag("repeat_password_input").performTextReplacement("different")
        composeRule.onNodeWithTag("create_account_button").performClick()

        composeRule.onNodeWithTag("repeat_password_error")
            .assertTextEquals("Passwords do not match")
    }

    @Test
    fun passwordVisibilityCanBeToggledForBothFields() {
        setCreateAccountContent()

        composeRule.onAllNodesWithContentDescription("Show password").assertCountEquals(2)
        composeRule.onAllNodesWithContentDescription("Show password")[0].performClick()

        composeRule.onAllNodesWithContentDescription("Hide password").assertCountEquals(1)
        composeRule.onAllNodesWithContentDescription("Show password").assertCountEquals(1)
    }

    @Test
    fun backToLoginInvokesCallback() {
        var backRequested = false
        setCreateAccountContent(onBackToLogin = { backRequested = true })

        composeRule.onNodeWithTag("back_to_login").performClick()

        assertTrue(backRequested)
    }

    private fun setCreateAccountContent(onBackToLogin: () -> Unit = {}) {
        composeRule.setContent {
            OnePassTheme {
                CreateAccountView(onBackToLogin = onBackToLogin)
            }
        }
    }
}
