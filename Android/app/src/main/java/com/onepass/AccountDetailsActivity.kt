package com.onepass

import android.content.ClipData
import android.content.ClipDescription
import android.content.ClipboardManager
import android.content.Context
import android.content.Intent
import android.net.Uri
import android.os.Build
import android.os.Bundle
import android.os.PersistableBundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.automirrored.outlined.OpenInNew
import androidx.compose.material.icons.automirrored.outlined.StickyNote2
import androidx.compose.material.icons.filled.Star
import androidx.compose.material.icons.outlined.ContentCopy
import androidx.compose.material.icons.outlined.Edit
import androidx.compose.material.icons.outlined.History
import androidx.compose.material.icons.outlined.Info
import androidx.compose.material.icons.outlined.Key
import androidx.compose.material.icons.outlined.Visibility
import androidx.compose.material.icons.outlined.VisibilityOff
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.ModalBottomSheet
import androidx.compose.material3.PrimaryTabRow
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.Tab
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults.topAppBarColors
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.colorResource
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontStyle
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.onepass.services.Account
import com.onepass.services.PasswordHistory
import com.onepass.services.VaultState
import com.onepass.ui.theme.OnePassTheme
import kotlinx.coroutines.launch
import java.net.URI
import java.time.Instant
import java.time.LocalDateTime
import java.time.OffsetDateTime
import java.time.ZoneId
import java.time.ZonedDateTime
import java.time.format.DateTimeFormatter
import java.time.format.FormatStyle
import java.util.Locale
import java.util.UUID

class AccountDetailsActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()

        val accountGuid = intent.getStringExtra(EXTRA_ACCOUNT_GUID)
        setContent {
            OnePassTheme {
                val repository = (application as OnePassApplication).vaultRepository
                val vaultState by repository.state.collectAsState()
                val account = (vaultState as? VaultState.Unlocked)
                    ?.data
                    ?.accounts
                    ?.let { accounts -> findAccount(accounts, accountGuid) }

                if (account == null) {
                    AccountUnavailableScreen(onBack = ::finish)
                } else {
                    AccountDetailsScreen(
                        account = account,
                        onBack = ::finish,
                        onEdit = {},
                        onCopy = ::copySensitive,
                        onOpenWebsite = { url ->
                            startActivity(Intent(Intent.ACTION_VIEW, Uri.parse(url)))
                        },
                    )
                }
            }
        }
    }

    private fun copySensitive(value: String) {
        val clip = ClipData.newPlainText(getString(R.string.credential_clipboard_label), value)
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            clip.description.extras = PersistableBundle().apply {
                putBoolean(ClipDescription.EXTRA_IS_SENSITIVE, true)
            }
        }
        val clipboard = getSystemService(Context.CLIPBOARD_SERVICE) as ClipboardManager
        clipboard.setPrimaryClip(clip)
    }

    companion object {
        const val EXTRA_ACCOUNT_GUID = "com.onepass.extra.ACCOUNT_GUID"
    }
}

private enum class AccountTab {
    Details,
    Notes,
    History,
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AccountDetailsScreen(
    account: Account,
    onBack: () -> Unit,
    onOpenWebsite: (String) -> Unit,
    onCopy: (String) -> Unit,
    onEdit: () -> Unit,
    editEnabled: Boolean = false,
) {
    var selectedTabIndex by rememberSaveable(account.guid) { mutableIntStateOf(0) }
    var showRecordDetails by rememberSaveable(account.guid) { mutableStateOf(false) }
    val snackbarHostState = remember { SnackbarHostState() }
    val coroutineScope = rememberCoroutineScope()
    val copiedMessage = stringResource(R.string.copied_to_clipboard)
    val copyValue: (String) -> Unit = { value ->
        onCopy(value)
        coroutineScope.launch { snackbarHostState.showSnackbar(copiedMessage) }
    }

    if (showRecordDetails) {
        ModalBottomSheet(onDismissRequest = { showRecordDetails = false }) {
            RecordDetailsSheet(account)
        }
    }

    Scaffold(
        topBar = {
            AccountDetailsTopBar(
                onBack = onBack,
                onEdit = onEdit,
                editEnabled = editEnabled,
                onInfo = { showRecordDetails = true },
            )
        },
        snackbarHost = { SnackbarHost(snackbarHostState) },
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding),
        ) {
            AccountIdentity(account)

            PrimaryTabRow(selectedTabIndex = selectedTabIndex) {
                AccountTab.entries.forEachIndexed { index, tab ->
                    Tab(
                        selected = selectedTabIndex == index,
                        onClick = { selectedTabIndex = index },
                        text = { Text(stringResource(tab.titleResource)) },
                        icon = { Icon(tab.icon, contentDescription = null) },
                    )
                }
            }

            when (AccountTab.entries[selectedTabIndex]) {
                AccountTab.Details -> DetailsTab(account, copyValue, onOpenWebsite)
                AccountTab.Notes -> NotesTab(account.notes)
                AccountTab.History -> HistoryTab(
                    history = sortPasswordHistory(account.passwordHistory),
                    onCopy = copyValue,
                )
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun AccountDetailsTopBar(
    onBack: () -> Unit,
    onEdit: () -> Unit,
    editEnabled: Boolean,
    onInfo: () -> Unit,
) {
    TopAppBar(
        colors = topAppBarColors(
            containerColor = colorResource(R.color.deep_blue),
            titleContentColor = Color.White,
            actionIconContentColor = Color.White,
            navigationIconContentColor = Color.White,
        ),
        title = { Text(stringResource(R.string.account_details)) },
        navigationIcon = {
            IconButton(onClick = onBack) {
                Icon(
                    Icons.AutoMirrored.Filled.ArrowBack,
                    contentDescription = stringResource(R.string.back),
                )
            }
        },
        actions = {
            IconButton(enabled = editEnabled, onClick = onEdit) {
                Icon(
                    Icons.Outlined.Edit,
                    contentDescription = stringResource(R.string.edit_account),
                )
            }
            IconButton(onClick = onInfo) {
                Icon(
                    Icons.Outlined.Info,
                    contentDescription = stringResource(R.string.info_account),
                )
            }
        },
    )
}

@Composable
private fun AccountIdentity(account: Account) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(horizontal = 20.dp, vertical = 20.dp),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        Box(
            modifier = Modifier
                .size(58.dp)
                .clip(RoundedCornerShape(16.dp))
                .background(MaterialTheme.colorScheme.primaryContainer),
            contentAlignment = Alignment.Center,
        ) {
            Text(
                text = accountInitial(account),
                color = MaterialTheme.colorScheme.onPrimaryContainer,
                style = MaterialTheme.typography.headlineSmall,
                fontWeight = FontWeight.Medium,
            )
        }
        Spacer(Modifier.width(16.dp))
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = account.name.normalizedDetailValue()
                    ?: stringResource(R.string.unnamed_account),
                style = MaterialTheme.typography.headlineSmall,
                fontWeight = FontWeight.Medium,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis,
            )
            if (account.favourite) {
                Row(
                    modifier = Modifier.padding(top = 4.dp),
                    verticalAlignment = Alignment.CenterVertically,
                ) {
                    Icon(
                        Icons.Filled.Star,
                        contentDescription = null,
                        modifier = Modifier.size(16.dp),
                        tint = AccountFavouriteGold,
                    )
                    Spacer(Modifier.width(5.dp))
                    Text(
                        text = stringResource(R.string.favourite),
                        color = AccountFavouriteGold,
                        style = MaterialTheme.typography.bodySmall,
                    )
                }
            }
        }
    }
}

@Composable
private fun DetailsTab(
    account: Account,
    onCopy: (String) -> Unit,
    onOpenWebsite: (String) -> Unit,
) {
    val website = normalizedHttpUrl(account.websiteUrl)
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = androidx.compose.foundation.layout.PaddingValues(
            start = 20.dp,
            end = 20.dp,
            top = 16.dp,
            bottom = 28.dp,
        ),
    ) {
        item { SectionLabel(R.string.sign_in) }
        item {
            CopyableDetailRow(
                label = stringResource(R.string.account_username),
                value = account.username,
                onCopy = onCopy,
            )
        }
        item {
            CopyableDetailRow(
                label = stringResource(R.string.account_email),
                value = account.emailAddress,
                onCopy = onCopy,
            )
        }
        item {
            PasswordDetailRow(
                label = stringResource(R.string.account_password),
                password = account.password,
                onCopy = onCopy,
            )
        }
        item { Spacer(Modifier.height(20.dp)) }
        item { SectionLabel(R.string.website) }
        item {
            DetailRow(
                label = stringResource(R.string.website),
                value = account.websiteUrl,
                actions = {
                    if (website != null) {
                        IconButton(onClick = { onOpenWebsite(website) }) {
                            Icon(
                                Icons.AutoMirrored.Outlined.OpenInNew,
                                contentDescription = stringResource(R.string.open_website),
                            )
                        }
                    }
                },
            )
        }
    }
}

@Composable
private fun NotesTab(notes: String?) {
    val normalizedNotes = notes.normalizedDetailValue()
    if (normalizedNotes == null) {
        EmptyTabState(
            icon = Icons.AutoMirrored.Outlined.StickyNote2,
            message = stringResource(R.string.no_notes),
        )
    } else {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .verticalScroll(rememberScrollState())
                .padding(horizontal = 20.dp, vertical = 16.dp),
        ) {
            SectionLabel(R.string.notes)
            Text(
                text = normalizedNotes,
                modifier = Modifier.padding(top = 8.dp, bottom = 28.dp),
                style = MaterialTheme.typography.bodyLarge,
            )
        }
    }
}

@Composable
private fun HistoryTab(
    history: List<PasswordHistory>,
    onCopy: (String) -> Unit,
) {
    if (history.isEmpty()) {
        EmptyTabState(
            icon = Icons.Outlined.History,
            message = stringResource(R.string.no_password_history),
        )
        return
    }
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = androidx.compose.foundation.layout.PaddingValues(
            start = 20.dp,
            end = 20.dp,
            top = 16.dp,
            bottom = 28.dp,
        ),
    ) {
        item { SectionLabel(R.string.previous_passwords) }
        items(items = history, key = PasswordHistory::guid) { entry ->
            HistoryRow(entry = entry, onCopy = onCopy)
        }
    }
}

@Composable
private fun CopyableDetailRow(
    label: String,
    value: String?,
    onCopy: (String) -> Unit,
) {
    val normalizedValue = value.normalizedDetailValue()
    DetailRow(
        label = label,
        value = normalizedValue,
        actions = {
            if (normalizedValue != null) {
                IconButton(onClick = { onCopy(normalizedValue) }) {
                    Icon(
                        Icons.Outlined.ContentCopy,
                        contentDescription = stringResource(R.string.copy_value, label),
                    )
                }
            }
        },
    )
}

@Composable
private fun PasswordDetailRow(
    label: String,
    password: String?,
    onCopy: (String) -> Unit,
) {
    val normalizedPassword = password.normalizedDetailValue()
    var visible by rememberSaveable { mutableStateOf(false) }
    DetailRow(
        label = label,
        value = when {
            normalizedPassword == null -> null
            visible -> normalizedPassword
            else -> MaskedPassword
        },
        actions = {
            if (normalizedPassword != null) {
                IconButton(onClick = { visible = !visible }) {
                    Icon(
                        if (visible) Icons.Outlined.VisibilityOff else Icons.Outlined.Visibility,
                        contentDescription = stringResource(
                            if (visible) R.string.hide_password else R.string.show_password,
                        ),
                    )
                }
                IconButton(onClick = { onCopy(normalizedPassword) }) {
                    Icon(
                        Icons.Outlined.ContentCopy,
                        contentDescription = stringResource(R.string.copy_password),
                    )
                }
            }
        },
    )
}

@Composable
private fun HistoryRow(
    entry: PasswordHistory,
    onCopy: (String) -> Unit,
) {
    val password = entry.password.normalizedDetailValue()
    var visible by rememberSaveable(entry.guid) { mutableStateOf(false) }
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .heightIn(min = 76.dp),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = when {
                    password == null -> stringResource(R.string.not_set)
                    visible -> password
                    else -> MaskedPassword
                },
                color = if (password == null) {
                    MaterialTheme.colorScheme.onSurfaceVariant
                } else {
                    MaterialTheme.colorScheme.onSurface
                },
                fontStyle = if (password == null) FontStyle.Italic else FontStyle.Normal,
                style = MaterialTheme.typography.bodyLarge,
            )
            Text(
                text = formatAccountDate(entry.dateTime)
                    ?: stringResource(R.string.date_not_recorded),
                modifier = Modifier.padding(top = 5.dp),
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                style = MaterialTheme.typography.bodySmall,
            )
        }
        if (password != null) {
            IconButton(onClick = { visible = !visible }) {
                Icon(
                    if (visible) Icons.Outlined.VisibilityOff else Icons.Outlined.Visibility,
                    contentDescription = stringResource(
                        if (visible) R.string.hide_password else R.string.show_password,
                    ),
                )
            }
            IconButton(onClick = { onCopy(password) }) {
                Icon(
                    Icons.Outlined.ContentCopy,
                    contentDescription = stringResource(R.string.copy_password),
                )
            }
        }
    }
    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
}

@Composable
private fun DetailRow(
    label: String,
    value: String?,
    actions: @Composable () -> Unit = {},
) {
    val normalizedValue = value.normalizedDetailValue()
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .heightIn(min = 66.dp),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = label,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                style = MaterialTheme.typography.bodySmall,
            )
            Text(
                text = normalizedValue ?: stringResource(R.string.not_set),
                modifier = Modifier.padding(top = 4.dp),
                color = if (normalizedValue == null) {
                    MaterialTheme.colorScheme.onSurfaceVariant
                } else {
                    MaterialTheme.colorScheme.onSurface
                },
                fontStyle = if (normalizedValue == null) FontStyle.Italic else FontStyle.Normal,
                style = MaterialTheme.typography.bodyLarge,
            )
        }
        Row(verticalAlignment = Alignment.CenterVertically) { actions() }
    }
    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
}

@Composable
private fun SectionLabel(resource: Int) {
    Text(
        text = stringResource(resource).uppercase(),
        color = MaterialTheme.colorScheme.primary,
        style = MaterialTheme.typography.labelMedium,
        fontWeight = FontWeight.Medium,
    )
}

@Composable
private fun EmptyTabState(
    icon: androidx.compose.ui.graphics.vector.ImageVector,
    message: String,
) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(32.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center,
    ) {
        Icon(
            imageVector = icon,
            contentDescription = null,
            modifier = Modifier.size(48.dp),
            tint = MaterialTheme.colorScheme.onSurfaceVariant,
        )
        Text(
            text = message,
            modifier = Modifier.padding(top = 14.dp),
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            style = MaterialTheme.typography.bodyMedium,
        )
    }
}

@Composable
private fun RecordDetailsSheet(account: Account) {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(start = 20.dp, end = 20.dp, bottom = 28.dp),
    ) {
        Text(
            text = stringResource(R.string.record_details),
            style = MaterialTheme.typography.titleLarge,
            fontWeight = FontWeight.Medium,
        )
        Spacer(Modifier.height(12.dp))
        DetailRow(
            label = stringResource(R.string.created),
            value = formatAccountDate(account.dateCreated),
        )
        DetailRow(
            label = stringResource(R.string.last_modified),
            value = formatAccountDate(account.dateModified),
        )
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun AccountUnavailableScreen(onBack: () -> Unit) {
    Scaffold(
        topBar = {
            TopAppBar(
                colors = topAppBarColors(
                    containerColor = colorResource(R.color.deep_blue),
                    titleContentColor = Color.White,
                    navigationIconContentColor = Color.White,
                ),
                title = { Text(stringResource(R.string.account_details)) },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(
                            Icons.AutoMirrored.Filled.ArrowBack,
                            contentDescription = stringResource(R.string.back),
                        )
                    }
                },
            )
        },
    ) { innerPadding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
                .padding(32.dp),
            contentAlignment = Alignment.Center,
        ) {
            Text(
                text = stringResource(R.string.account_unavailable),
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                style = MaterialTheme.typography.bodyLarge,
            )
        }
    }
}

internal fun findAccount(accounts: List<Account>, guid: String?): Account? {
    val parsedGuid = runCatching { UUID.fromString(guid) }.getOrNull() ?: return null
    return accounts.firstOrNull { account -> account.guid == parsedGuid }
}

internal fun normalizedHttpUrl(value: String?): String? {
    val normalized = value.normalizedDetailValue() ?: return null
    val uri = runCatching { URI(normalized) }.getOrNull() ?: return null
    if (uri.scheme?.lowercase(Locale.ROOT) !in setOf("http", "https")) return null
    if (uri.host.isNullOrBlank()) return null
    return normalized
}

internal fun sortPasswordHistory(history: List<PasswordHistory>): List<PasswordHistory> =
    history.sortedWith(
        compareByDescending<PasswordHistory> { parseAccountInstant(it.dateTime) }
            .thenBy { it.guid.toString() },
    )

internal fun formatAccountDate(
    value: String?,
    locale: Locale = Locale.getDefault(),
    zoneId: ZoneId = ZoneId.systemDefault(),
): String? {
    val normalized = value.normalizedDetailValue() ?: return null
    val dateTime = parseAccountDateTime(normalized, zoneId) ?: return normalized
    return DateTimeFormatter
        .ofLocalizedDateTime(FormatStyle.MEDIUM)
        .withLocale(locale)
        .format(dateTime)
}

private fun parseAccountInstant(value: String?): Instant? {
    val normalized = value.normalizedDetailValue() ?: return null
    return runCatching { Instant.parse(normalized) }.getOrNull()
        ?: runCatching { OffsetDateTime.parse(normalized).toInstant() }.getOrNull()
        ?: runCatching { ZonedDateTime.parse(normalized).toInstant() }.getOrNull()
        ?: runCatching {
            LocalDateTime.parse(normalized).atZone(ZoneId.systemDefault()).toInstant()
        }.getOrNull()
}

private fun parseAccountDateTime(value: String, zoneId: ZoneId): ZonedDateTime? =
    runCatching { Instant.parse(value).atZone(zoneId) }.getOrNull()
        ?: runCatching { OffsetDateTime.parse(value).atZoneSameInstant(zoneId) }.getOrNull()
        ?: runCatching { ZonedDateTime.parse(value).withZoneSameInstant(zoneId) }.getOrNull()
        ?: runCatching { LocalDateTime.parse(value).atZone(zoneId) }.getOrNull()

private fun String?.normalizedDetailValue(): String? = this?.trim()?.takeIf(String::isNotEmpty)

private val AccountTab.titleResource: Int
    get() = when (this) {
        AccountTab.Details -> R.string.details
        AccountTab.Notes -> R.string.notes
        AccountTab.History -> R.string.history
    }

private val AccountTab.icon: androidx.compose.ui.graphics.vector.ImageVector
    get() = when (this) {
        AccountTab.Details -> Icons.Outlined.Key
        AccountTab.Notes -> Icons.AutoMirrored.Outlined.StickyNote2
        AccountTab.History -> Icons.Outlined.History
    }

private const val MaskedPassword = "••••••••••••"
private val AccountFavouriteGold = Color(0xFFFFB300)

@Preview(showBackground = true)
@Composable
private fun AccountDetailsScreenPreview() {
    OnePassTheme {
        AccountDetailsScreen(
            account = previewAccount(),
            onBack = {},
            onOpenWebsite = {},
            onCopy = {},
            onEdit = {},
        )
    }
}

private fun previewAccount() = Account(
    guid = UUID.fromString("00000000-0000-0000-0000-000000000001"),
    dateCreated = "2024-03-14T18:32:00Z",
    dateModified = "2026-08-20T09:15:00Z",
    name = "GitHub",
    username = "callum",
    emailAddress = "callum@example.com",
    password = "not-a-real-password",
    favourite = true,
    websiteUrl = "https://github.com",
    mfaEnabled = false,
    notes = "Personal development account.\n\nRecovery codes are stored offline.",
    passwordHistory = listOf(
        PasswordHistory(
            guid = UUID.fromString("00000000-0000-0000-0000-000000000002"),
            password = "old-password",
            dateTime = "2025-01-03T21:06:00Z",
        ),
    ),
)
