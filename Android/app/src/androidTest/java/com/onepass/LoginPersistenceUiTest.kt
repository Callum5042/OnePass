package com.onepass

import android.net.Uri
import androidx.compose.ui.test.assertIsOn
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.junit4.createComposeRule
import com.onepass.ui.theme.OnePassTheme
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Rule
import org.junit.Test

class LoginPersistenceUiTest {
    @get:Rule
    val composeRule = createComposeRule()

    @Test
    fun loginRestoresRememberMeFromSavedState() {
        composeRule.setContent {
            OnePassTheme {
                LoginView(initialRememberMe = true)
            }
        }

        composeRule.onNodeWithTag("remember_me").assertIsOn()
    }

    @Test
    fun togglingRememberMeReportsTheStateToThePersistenceLayer() {
        var savedRememberMe: Boolean? = null
        var savedVaultUri: Uri? = Uri.parse("content://com.onepass.test/not-set")

        composeRule.setContent {
            OnePassTheme {
                LoginView(
                    onLoginPreferencesChanged = { rememberMe, vaultUri ->
                        savedRememberMe = rememberMe
                        savedVaultUri = vaultUri
                    },
                )
            }
        }

        val rememberMe = composeRule.onNodeWithTag("remember_me")
        rememberMe.performClick()
        assertEquals(true, savedRememberMe)
        assertNull(savedVaultUri)

        rememberMe.performClick()
        assertEquals(false, savedRememberMe)
        assertNull(savedVaultUri)
    }
}
