package com.onepass

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.AccountCircle
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Lock
import androidx.compose.material.icons.filled.Search
import androidx.compose.material.icons.filled.Settings
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.FloatingActionButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults.topAppBarColors
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.colorResource
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.onepass.services.Account
import com.onepass.services.OnePassData
import com.onepass.services.VaultState
import com.onepass.ui.theme.OnePassTheme
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import java.util.UUID

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            OnePassTheme {
                val repository = (application as OnePassApplication).vaultRepository
                MainScreen(
                    data = repository.state,
                    onAccountSelected = { account ->
                        startActivity(
                            Intent(this, AccountDetailsActivity::class.java).apply {
                                putExtra(AccountDetailsActivity.EXTRA_ACCOUNT_GUID, account.guid.toString())
                            },
                        )
                    },
                )
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MainScreen(
    data: StateFlow<VaultState>,
    onAccountSelected: (Account) -> Unit = {},
) {
    val vaultState by data.collectAsState()

    Scaffold(
        topBar = {
            TopAppBar(
                colors = topAppBarColors(
                    containerColor = colorResource(R.color.deep_blue),
                    titleContentColor = Color.White,
                    actionIconContentColor = Color.White,
                    navigationIconContentColor = Color.White,
                    subtitleContentColor = Color.Gray,
                ),
                title = {
                    Text(stringResource(R.string.app_name))
                },
                actions = {
                    IconButton(onClick = { /* Search will be implemented later. */ }) {
                        Icon(
                            imageVector = Icons.Default.Search,
                            contentDescription = stringResource(R.string.search_accounts),
                        )
                    }

                    IconButton(onClick = { /* Settings will be implemented later. */ }) {
                        Icon(
                            imageVector = Icons.Default.Settings,
                            contentDescription = stringResource(R.string.settings),
                        )
                    }
                },
            )
        },
        floatingActionButton = {
            FloatingActionButton(
                onClick = { /* Account creation will be implemented later. */ },
                containerColor = colorResource(R.color.deep_blue),
                contentColor = Color.White,
                elevation = FloatingActionButtonDefaults.bottomAppBarFabElevation(),
            ) {
                Icon(
                    imageVector = Icons.Filled.Add,
                    contentDescription = stringResource(R.string.add_account),
                )
            }
        }
    ) { innerPadding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
        ) {

            when (val state = vaultState) {
                VaultState.Locked -> LockedVaultState()
                is VaultState.Unlocked -> AccountContent(
                    accounts = state.data.accounts,
                    onAccountSelected = onAccountSelected,
                )
            }
        }
    }
}

@Composable
private fun AccountContent(
    accounts: List<Account>,
    onAccountSelected: (Account) -> Unit,
) {
    if (accounts.isEmpty()) {
        EmptyVaultState()
    } else {
        AccountList(
            accounts = sortAccounts(accounts),
            onAccountSelected = onAccountSelected,
        )
    }
}

@Composable
private fun AccountList(
    accounts: List<Account>,
    onAccountSelected: (Account) -> Unit,
) {
    LazyColumn(modifier = Modifier.fillMaxSize()) {
        items(
            items = accounts,
            key = { account -> account.guid },
        ) { account ->
            AccountRow(
                account = account,
                onClick = { onAccountSelected(account) },
            )
            HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
        }
    }
}

@Composable
private fun AccountRow(
    account: Account,
    onClick: () -> Unit,
) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .heightIn(min = 72.dp)
            .clickable(onClick = onClick)
            .padding(horizontal = 16.dp, vertical = 12.dp),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        Box(
            modifier = Modifier
                .size(48.dp)
                .clip(RoundedCornerShape(12.dp))
                .background(MaterialTheme.colorScheme.primaryContainer),
            contentAlignment = Alignment.Center,
        ) {
            Text(
                text = accountInitial(account),
                color = MaterialTheme.colorScheme.onPrimaryContainer,
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.SemiBold,
            )
        }

        Spacer(modifier = Modifier.width(16.dp))

        Column(
            modifier = Modifier.weight(1f),
            verticalArrangement = Arrangement.Center,
        ) {
            Text(
                text = account.name.normalizedOrNull()
                    ?: stringResource(R.string.unnamed_account),
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.Medium,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis,
            )
            Text(
                text = displayLogin(account)
                    ?: stringResource(R.string.no_login_details),
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                style = MaterialTheme.typography.bodyMedium,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis,
            )
        }

        if (account.favourite) {
            Spacer(modifier = Modifier.width(12.dp))
            Icon(
                imageVector = Icons.Filled.Star,
                contentDescription = stringResource(R.string.favourite_account),
                tint = FavouriteGold,
            )
        }
    }
}

@Composable
private fun EmptyVaultState() {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(32.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center,
    ) {
        Icon(
            imageVector = Icons.Filled.AccountCircle,
            contentDescription = null,
            modifier = Modifier.size(72.dp),
            tint = MaterialTheme.colorScheme.primary,
        )
        Text(
            text = stringResource(R.string.no_accounts_title),
            modifier = Modifier.padding(top = 16.dp),
            style = MaterialTheme.typography.headlineSmall,
            fontWeight = FontWeight.SemiBold,
        )
        Text(
            text = stringResource(R.string.no_accounts_guidance),
            modifier = Modifier.padding(top = 8.dp),
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            style = MaterialTheme.typography.bodyMedium,
        )
    }
}

@Composable
private fun LockedVaultState() {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(32.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center,
    ) {
        Icon(
            imageVector = Icons.Filled.Lock,
            contentDescription = null,
            modifier = Modifier.size(64.dp),
            tint = MaterialTheme.colorScheme.onSurfaceVariant,
        )
        Text(
            text = stringResource(R.string.vault_locked),
            modifier = Modifier.padding(top = 16.dp),
            style = MaterialTheme.typography.headlineSmall,
            fontWeight = FontWeight.SemiBold,
        )
    }
}

internal fun sortAccounts(accounts: List<Account>): List<Account> =
    accounts.sortedWith(
        compareByDescending<Account> { it.favourite }
            .thenComparator { left, right -> compareAccountNames(left.name, right.name) }
            .thenBy { it.guid.toString() },
    )

internal fun displayLogin(account: Account): String? =
    account.username.normalizedOrNull() ?: account.emailAddress.normalizedOrNull()

internal fun accountInitial(account: Account): String =
    account.name
        .normalizedOrNull()
        ?.firstOrNull(Char::isLetterOrDigit)
        ?.uppercaseChar()
        ?.toString()
        ?: "?"

private fun compareAccountNames(left: String?, right: String?): Int {
    val leftName = left.normalizedOrNull()
    val rightName = right.normalizedOrNull()
    return when {
        leftName == null && rightName == null -> 0
        leftName == null -> 1
        rightName == null -> -1
        else -> leftName.compareTo(rightName, ignoreCase = true)
    }
}

private fun String?.normalizedOrNull(): String? = this?.trim()?.takeIf(String::isNotEmpty)

private val FavouriteGold = Color(0xFFFFB300)

@Preview(name = "Populated vault", showBackground = true)
@Composable
private fun PopulatedMainScreenPreview() {
    val previewData = OnePassData(
        accounts = listOf(
            Account(
                guid = UUID.randomUUID(),
                dateCreated = null,
                dateModified = null,
                name = "GitHub",
                username = "callum",
                emailAddress = "callum@example.com",
                password = "not-a-real-password",
                favourite = true,
                websiteUrl = "https://github.com",
                mfaEnabled = true,
                notes = "My GitHub account",
                passwordHistory = emptyList()
            ),
            Account(
                guid = UUID.randomUUID(),
                dateCreated = null,
                dateModified = null,
                name = "Netflix",
                username = "callum@example.com",
                emailAddress = "callum@example.com",
                password = "another-fake-password",
                favourite = false,
                websiteUrl = "https://netflix.com",
                mfaEnabled = false,
                notes = null,
                passwordHistory = emptyList()
            ),
            Account(
                guid = UUID.randomUUID(),
                dateCreated = null,
                dateModified = null,
                name = null,
                username = null,
                emailAddress = null,
                password = "never-shown-in-the-list",
                favourite = false,
                websiteUrl = null,
                mfaEnabled = false,
                notes = null,
                passwordHistory = emptyList()
            )
        )
    )

    OnePassTheme {
        MainScreen(
            data = MutableStateFlow(
                VaultState.Unlocked(previewData)
            )
        )
    }
}

@Preview(name = "Empty vault", showBackground = true)
@Composable
private fun EmptyMainScreenPreview() {
    OnePassTheme {
        MainScreen(MutableStateFlow(VaultState.Unlocked(OnePassData())))
    }
}

@Preview(name = "Locked vault", showBackground = true)
@Composable
private fun LockedMainScreenPreview() {
    OnePassTheme {
        MainScreen(MutableStateFlow(VaultState.Locked))
    }
}
