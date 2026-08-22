package com.onepass

import androidx.compose.ui.test.assertCountEquals
import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.assertIsFocused
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onAllNodesWithContentDescription
import androidx.compose.ui.test.onAllNodesWithText
import androidx.compose.ui.test.onNodeWithContentDescription
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performTextInput
import com.onepass.services.Account
import com.onepass.services.OnePassData
import com.onepass.services.VaultState
import com.onepass.ui.theme.OnePassTheme
import kotlinx.coroutines.flow.MutableStateFlow
import org.junit.Rule
import org.junit.Assert.assertEquals
import org.junit.Test
import java.util.UUID

class MainScreenTest {
    @get:Rule
    val composeRule = createComposeRule()

    @Test
    fun populatedVaultShowsSafeAccountSummaryAndControls() {
        setMainContent(
            VaultState.Unlocked(
                OnePassData(
                    accounts = listOf(
                        account(
                            name = "GitHub",
                            username = "callum",
                            emailAddress = "mail@example.com",
                            password = "never-display-this",
                            favourite = true,
                        ),
                    ),
                ),
            ),
        )

        composeRule.onNodeWithText("GitHub").assertIsDisplayed()
        composeRule.onNodeWithText("callum").assertIsDisplayed()
        composeRule.onAllNodesWithText("mail@example.com").assertCountEquals(0)
        composeRule.onAllNodesWithText("never-display-this").assertCountEquals(0)
        composeRule.onAllNodesWithContentDescription("Favourite account").assertCountEquals(1)

        composeRule.onNodeWithContentDescription("Search accounts")
            .assertIsDisplayed()
            .performClick()
        composeRule.onNodeWithTag("account_search_input").assertIsFocused()
        composeRule.onNodeWithContentDescription("Back").performClick()
        composeRule.onNodeWithContentDescription("Settings")
            .assertIsDisplayed()
            .performClick()
        composeRule.onNodeWithContentDescription("Add account")
            .assertIsDisplayed()
            .performClick()

        composeRule.onNodeWithText("GitHub").assertIsDisplayed()
    }

    @Test
    fun searchFiltersClearsAndClosesWithDedicatedEmptyState() {
        setMainContent(
            VaultState.Unlocked(
                OnePassData(
                    accounts = listOf(
                        account(name = "GitHub", username = "callum"),
                        account(name = "Email", emailAddress = "mail@example.com"),
                    ),
                ),
            ),
        )

        composeRule.onNodeWithContentDescription("Search accounts").performClick()
        composeRule.onNodeWithTag("account_search_input").performTextInput("CALL")

        composeRule.onNodeWithText("GitHub").assertIsDisplayed()
        composeRule.onAllNodesWithText("Email").assertCountEquals(0)

        composeRule.onNodeWithContentDescription("Clear search").performClick()
        composeRule.onNodeWithText("GitHub").assertIsDisplayed()
        composeRule.onNodeWithText("Email").assertIsDisplayed()

        composeRule.onNodeWithTag("account_search_input").performTextInput("missing")
        composeRule.onNodeWithText("No accounts match your search").assertIsDisplayed()
        composeRule.onAllNodesWithText("GitHub").assertCountEquals(0)

        composeRule.onNodeWithContentDescription("Back").performClick()
        composeRule.onNodeWithText("OnePass").assertIsDisplayed()
        composeRule.onNodeWithText("GitHub").assertIsDisplayed()
        composeRule.onNodeWithText("Email").assertIsDisplayed()
    }

    @Test
    fun accountUsesEmailAndMissingValueFallbacks() {
        setMainContent(
            VaultState.Unlocked(
                OnePassData(
                    accounts = listOf(
                        account(name = "Email", emailAddress = "mail@example.com"),
                        account(name = null),
                    ),
                ),
            ),
        )

        composeRule.onNodeWithText("mail@example.com").assertIsDisplayed()
        composeRule.onNodeWithText("Unnamed account").assertIsDisplayed()
        composeRule.onNodeWithText("No login details").assertIsDisplayed()
    }

    @Test
    fun emptyAndLockedVaultsHaveDistinctStates() {
        val state = MutableStateFlow<VaultState>(VaultState.Unlocked(OnePassData()))
        setMainContent(state)

        composeRule.onNodeWithText("No accounts yet").assertIsDisplayed()
        composeRule.onNodeWithText("Use Add to create your first account.").assertIsDisplayed()
        composeRule.onAllNodesWithText("Vault is locked").assertCountEquals(0)

        state.value = VaultState.Locked

        composeRule.onNodeWithText("Vault is locked").assertIsDisplayed()
        composeRule.onAllNodesWithText("No accounts yet").assertCountEquals(0)
    }

    @Test
    fun tappingAccountSelectsThatAccount() {
        val selectedAccount = account(name = "GitHub", username = "callum")
        var selected: Account? = null

        composeRule.setContent {
            OnePassTheme {
                MainScreen(
                    data = MutableStateFlow(
                        VaultState.Unlocked(OnePassData(accounts = listOf(selectedAccount))),
                    ),
                    onAccountSelected = { selected = it },
                )
            }
        }

        composeRule.onNodeWithContentDescription("Search accounts").performClick()
        composeRule.onNodeWithTag("account_search_input").performTextInput("call")
        composeRule.onNodeWithText("GitHub").performClick()
        assertEquals(selectedAccount, selected)
    }

    @Test
    fun tappingAddRequestsAccountCreation() {
        var addRequested = false
        composeRule.setContent {
            OnePassTheme {
                MainScreen(
                    data = MutableStateFlow(VaultState.Unlocked(OnePassData())),
                    onAddAccount = { addRequested = true },
                )
            }
        }

        composeRule.onNodeWithContentDescription("Add account").performClick()

        assertEquals(true, addRequested)
    }

    private fun setMainContent(state: VaultState) {
        setMainContent(MutableStateFlow(state))
    }

    private fun setMainContent(state: MutableStateFlow<VaultState>) {
        composeRule.setContent {
            OnePassTheme {
                MainScreen(state)
            }
        }
    }

    private fun account(
        name: String?,
        username: String? = null,
        emailAddress: String? = null,
        password: String? = "secret",
        favourite: Boolean = false,
    ) = Account(
        guid = UUID.randomUUID(),
        dateCreated = null,
        dateModified = null,
        name = name,
        username = username,
        emailAddress = emailAddress,
        password = password,
        favourite = favourite,
        websiteUrl = null,
        mfaEnabled = false,
        notes = null,
        passwordHistory = emptyList(),
    )
}
