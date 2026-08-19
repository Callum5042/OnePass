package com.onepass

import androidx.compose.ui.test.assertHasClickAction
import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.assertTextEquals
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithContentDescription
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performTextInput
import com.onepass.ui.theme.OnePassTheme
import org.junit.Assert.assertEquals
import org.junit.Rule
import org.junit.Test

class LoginViewTest {
    @get:Rule
    val composeRule = createComposeRule()

    @Test
    fun loginPageDisplaysExpectedControls() {
        setLoginContent()

        composeRule.onNodeWithContentDescription("Logo").assertIsDisplayed()
        composeRule.onNodeWithTag("username_input").assertIsDisplayed()
        composeRule.onNodeWithTag("password_input").assertIsDisplayed()
        composeRule.onNodeWithTag("login_button")
            .assertIsDisplayed()
            .assertHasClickAction()
        composeRule.onNodeWithText("Create Account").assertIsDisplayed()
        composeRule.onNodeWithText("v2.0").assertIsDisplayed()
    }

    @Test
    fun usernameAndPasswordAcceptInput() {
        setLoginContent()

        composeRule.onNodeWithTag("username_input")
            .performTextInput("user")
        composeRule.onNodeWithTag("password_input")
            .performTextInput("password")

        composeRule.onNodeWithContentDescription("Show password")
            .performClick()

        composeRule.onNodeWithTag("username_input").assertTextEquals("user")
        composeRule.onNodeWithTag("password_input").assertTextEquals("password")
    }

    @Test
    fun passwordVisibilityButtonTogglesItsAccessibleState() {
        setLoginContent()

        composeRule.onNodeWithContentDescription("Show password")
            .assertIsDisplayed()
            .performClick()

        composeRule.onNodeWithContentDescription("Hide password").assertIsDisplayed()
    }

    @Test
    fun loginButtonInvokesLoginAction() {
        var clickCount = 0
        setLoginContent(onLoginClick = { clickCount++ })

        composeRule.onNodeWithTag("login_button").performClick()

        composeRule.runOnIdle {
            assertEquals(1, clickCount)
        }
    }

    private fun setLoginContent(onLoginClick: () -> Unit = {}) {
        composeRule.setContent {
            OnePassTheme {
                LoginView(onLoginClick = onLoginClick)
            }
        }
    }
}
