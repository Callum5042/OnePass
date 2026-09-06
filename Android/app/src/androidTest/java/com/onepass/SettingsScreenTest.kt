package com.onepass

import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.assertIsNotEnabled
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import com.onepass.ui.theme.OnePassTheme
import org.junit.Assert.assertTrue
import org.junit.Rule
import org.junit.Test

class SettingsScreenTest {
    @get:Rule
    val composeRule = createComposeRule()

    @Test
    fun placeholderBiometricSettingIsVisibleAndDisabled() {
        composeRule.setContent {
            OnePassTheme {
                SettingsScreen()
            }
        }

        composeRule.onNodeWithText("Settings").assertIsDisplayed()
        composeRule.onNodeWithText("Biometric unlock").assertIsDisplayed()
        composeRule.onNodeWithText("Biometric unlock will be available here.").assertIsDisplayed()
        composeRule.onNodeWithText("Coming soon").assertIsDisplayed()
        composeRule.onNodeWithTag("biometric_toggle")
            .assertIsDisplayed()
            .assertIsNotEnabled()
    }

    @Test
    fun backCallbackIsInvoked() {
        var backRequested = false
        composeRule.setContent {
            OnePassTheme {
                SettingsScreen(onBack = { backRequested = true })
            }
        }

        composeRule.onNodeWithTag("settings_back").performClick()

        assertTrue(backRequested)
    }
}
