package com.onepass

import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.assertIsEnabled
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
    fun availableBiometricSettingIsVisibleAndEnabled() {
        composeRule.setContent {
            OnePassTheme {
                SettingsScreen(
                    state = BiometricSettingsState(
                        availability = BiometricAvailability.Available,
                    ),
                )
            }
        }

        composeRule.onNodeWithText("Settings").assertIsDisplayed()
        composeRule.onNodeWithText("Biometric unlock").assertIsDisplayed()
        composeRule.onNodeWithText("Enable biometric unlock for this vault.").assertIsDisplayed()
        composeRule.onNodeWithTag("biometric_toggle")
            .assertIsDisplayed()
            .assertIsEnabled()
    }

    @Test
    fun unavailableBiometricSettingExplainsWhyItCannotBeEnabled() {
        composeRule.setContent {
            OnePassTheme {
                SettingsScreen(
                    state = BiometricSettingsState(
                        availability = BiometricAvailability.NoEnrollment,
                    ),
                )
            }
        }

        composeRule.onNodeWithText(
            "Set up a strong biometric in Android Settings to enable this.",
        ).assertIsDisplayed()
        composeRule.onNodeWithTag("biometric_toggle")
            .assertIsDisplayed()
            .assertIsNotEnabled()
    }

    @Test
    fun staleEnabledBiometricSettingCanBeDisabledWithoutAvailability() {
        var requestedValue: Boolean? = null
        composeRule.setContent {
            OnePassTheme {
                SettingsScreen(
                    state = BiometricSettingsState(
                        enabled = true,
                        availability = BiometricAvailability.NoEnrollment,
                    ),
                    onBiometricToggle = { requestedValue = it },
                )
            }
        }

        composeRule.onNodeWithTag("biometric_toggle")
            .assertIsEnabled()
            .performClick()

        assertTrue(requestedValue == false)
    }

    @Test
    fun togglingBiometricSettingInvokesCallback() {
        var requestedValue: Boolean? = null
        composeRule.setContent {
            OnePassTheme {
                SettingsScreen(
                    state = BiometricSettingsState(
                        availability = BiometricAvailability.Available,
                    ),
                    onBiometricToggle = { requestedValue = it },
                )
            }
        }

        composeRule.onNodeWithTag("biometric_toggle").performClick()

        assertTrue(requestedValue == true)
    }

    @Test
    fun disablingBiometricSettingInvokesCallback() {
        var requestedValue: Boolean? = null
        composeRule.setContent {
            OnePassTheme {
                SettingsScreen(
                    state = BiometricSettingsState(
                        enabled = true,
                        availability = BiometricAvailability.Available,
                    ),
                    onBiometricToggle = { requestedValue = it },
                )
            }
        }

        composeRule.onNodeWithTag("biometric_toggle").performClick()

        assertTrue(requestedValue == false)
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
