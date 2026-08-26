package com.onepass

import android.content.ClipData
import android.content.ClipDescription
import android.content.ClipboardManager
import android.content.Intent
import android.os.Build
import android.os.Bundle
import android.os.PersistableBundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.BackHandler
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.text.BasicTextField
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
import androidx.compose.material.icons.outlined.Save
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
import androidx.compose.ui.graphics.SolidColor
import androidx.compose.ui.focus.onFocusChanged
import androidx.compose.ui.res.colorResource
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontStyle
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.core.net.toUri
import com.onepass.services.Account
import com.onepass.services.CredentialEdits
import com.onepass.services.PasswordHistory
import com.onepass.services.VaultState
import com.onepass.services.VaultUpdateResult
import com.onepass.ui.theme.OnePassTheme
import kotlinx.coroutines.launch
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
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
                        onSaveCredentials = { edits ->
                            withContext(Dispatchers.IO) {
                                repository.updateCredentials(account.guid, edits)
                            }
                        },
                        onCopy = ::copySensitive,
                        onOpenWebsite = { url ->
                            startActivity(
                                Intent(
                                    Intent.ACTION_VIEW,
                                    url.toUri()
                                )
                            )
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
        val clipboard = getSystemService(CLIPBOARD_SERVICE) as ClipboardManager
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
    onSaveCredentials: suspend (CredentialEdits) -> VaultUpdateResult = {
        VaultUpdateResult.Success
    },
) {
    var selectedTabIndex by rememberSaveable(account.guid) { mutableIntStateOf(0) }
    var showRecordDetails by rememberSaveable(account.guid) { mutableStateOf(false) }
    val snackBarHostState = remember { SnackbarHostState() }
    val coroutineScope = rememberCoroutineScope()
    val copiedMessage = stringResource(R.string.copied_to_clipboard)
    val savedMessage = stringResource(R.string.account_saved)
    val saveFailedMessage = stringResource(R.string.account_save_failed)
    val rollbackFailedMessage = stringResource(R.string.account_save_rollback_failed)
    val copyValue: (String) -> Unit = { value ->
        onCopy(value)
        coroutineScope.launch { snackBarHostState.showSnackbar(copiedMessage) }
    }

    var isEditing by rememberSaveable(account.guid) { mutableStateOf(false) }
    var isSaving by rememberSaveable(account.guid) { mutableStateOf(false) }
    var username by rememberSaveable(account.guid) { mutableStateOf(account.username.orEmpty()) }
    var emailAddress by rememberSaveable(account.guid) { mutableStateOf(account.emailAddress.orEmpty()) }
    var password by rememberSaveable(account.guid) { mutableStateOf(account.password.orEmpty()) }
    var websiteUrl by rememberSaveable(account.guid) { mutableStateOf(account.websiteUrl.orEmpty()) }
    var notes by rememberSaveable(account.guid) { mutableStateOf(account.notes.orEmpty()) }

    var usernameTouched by rememberSaveable(account.guid) { mutableStateOf(false) }
    var emailTouched by rememberSaveable(account.guid) { mutableStateOf(false) }
    var passwordTouched by rememberSaveable(account.guid) { mutableStateOf(false) }
    var websiteTouched by rememberSaveable(account.guid) { mutableStateOf(false) }
    var notesTouched by rememberSaveable(account.guid) { mutableStateOf(false) }

    fun beginEditing() {
        username = account.username.orEmpty()
        emailAddress = account.emailAddress.orEmpty()
        password = account.password.orEmpty()
        websiteUrl = account.websiteUrl.orEmpty()
        notes = account.notes.orEmpty()

        usernameTouched = false
        emailTouched = false
        passwordTouched = false
        websiteTouched = false
        notesTouched = false
        isEditing = true
    }

    fun cancelEditing() {
        if (isSaving) return
        isEditing = false
        usernameTouched = false
        emailTouched = false
        passwordTouched = false
        websiteTouched = false
        notesTouched = false
    }

    val edits = CredentialEdits(
        username = if (usernameTouched) username else account.username,
        emailAddress = if (emailTouched) emailAddress else account.emailAddress,
        password = if (passwordTouched) password else account.password,
        websiteUrl = if (websiteTouched) websiteUrl else account.websiteUrl,
        notes = if (notesTouched) notes else account.notes,
    )
    val isDirty = edits != CredentialEdits(
        username = account.username,
        emailAddress = account.emailAddress,
        password = account.password,
        websiteUrl = account.websiteUrl,
        notes = account.notes,
    )

    BackHandler(enabled = isEditing) { cancelEditing() }

    if (showRecordDetails) {
        ModalBottomSheet(onDismissRequest = { showRecordDetails = false }) {
            RecordDetailsSheet(account)
        }
    }

    Scaffold(
        topBar = {
            AccountDetailsTopBar(
                isEditing = isEditing,
                isSaving = isSaving,
                saveEnabled = isDirty,
                onBack = { if (isEditing) cancelEditing() else onBack() },
                onEdit = ::beginEditing,
                onSave = {
                    if (!isDirty || isSaving) return@AccountDetailsTopBar
                    isSaving = true
                    coroutineScope.launch {
                        when (val result = onSaveCredentials(edits)) {
                            VaultUpdateResult.Success -> {
                                isSaving = false
                                isEditing = false
                                snackBarHostState.showSnackbar(savedMessage)
                            }
                            is VaultUpdateResult.SaveFailed -> {
                                isSaving = false
                                snackBarHostState.showSnackbar(
                                    if (result.rollbackSucceeded) saveFailedMessage
                                    else rollbackFailedMessage,
                                )
                            }
                            VaultUpdateResult.AccountNotFound,
                            VaultUpdateResult.VaultLocked -> {
                                isSaving = false
                                snackBarHostState.showSnackbar(saveFailedMessage)
                            }
                        }
                    }
                },
                onInfo = { showRecordDetails = true },
            )
        },
        snackbarHost = { SnackbarHost(snackBarHostState) },
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
                        enabled = !isSaving,
                        onClick = { selectedTabIndex = index },
                        text = { Text(stringResource(tab.titleResource)) },
                        icon = { Icon(tab.icon, contentDescription = null) },
                    )
                }
            }

            when (AccountTab.entries[selectedTabIndex]) {
                AccountTab.Details -> DetailsTab(
                    account = account,
                    isEditing = isEditing,
                    enabled = !isSaving,
                    username = username,
                    emailAddress = emailAddress,
                    password = password,
                    websiteUrl = websiteUrl,
                    onUsernameChange = { usernameTouched = true; username = it },
                    onEmailChange = { emailTouched = true; emailAddress = it },
                    onPasswordChange = { passwordTouched = true; password = it },
                    onWebsiteChange = { websiteTouched = true; websiteUrl = it },
                    onCopy = copyValue,
                    onOpenWebsite = onOpenWebsite,
                )
                AccountTab.Notes -> NotesTab(
                    account = account,
                    notes,
                    isEditing = isEditing,
                    enabled = !isSaving,
                    onNotesChange = { notesTouched = true; notes = it }
                )
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
    isEditing: Boolean,
    isSaving: Boolean,
    saveEnabled: Boolean,
    onBack: () -> Unit,
    onEdit: () -> Unit,
    onSave: () -> Unit,
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
            IconButton(enabled = !isSaving, onClick = onBack) {
                Icon(
                    Icons.AutoMirrored.Filled.ArrowBack,
                    contentDescription = stringResource(R.string.back),
                )
            }
        },
        actions = {
            if (isEditing) {
                IconButton(enabled = saveEnabled && !isSaving, onClick = onSave) {
                    Icon(
                        Icons.Outlined.Save,
                        contentDescription = stringResource(R.string.save_account),
                    )
                }
            } else {
                IconButton(onClick = onEdit) {
                    Icon(
                        Icons.Outlined.Edit,
                        contentDescription = stringResource(R.string.edit_account),
                    )
                }
            }
            IconButton(enabled = !isSaving, onClick = onInfo) {
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
    isEditing: Boolean,
    enabled: Boolean,
    username: String,
    emailAddress: String,
    password: String,
    websiteUrl: String,
    onUsernameChange: (String) -> Unit,
    onEmailChange: (String) -> Unit,
    onPasswordChange: (String) -> Unit,
    onWebsiteChange: (String) -> Unit,
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
            if (isEditing) {
                EditableCredentialRow(
                    label = stringResource(R.string.account_username),
                    value = username,
                    onValueChange = onUsernameChange,
                    enabled = enabled,
                    keyboardType = KeyboardType.Text,
                    testTag = "edit_username",
                )
            } else {
                CopyableDetailRow(
                    label = stringResource(R.string.account_username),
                    value = account.username,
                    onCopy = onCopy,
                )
            }
        }
        item {
            if (isEditing) {
                EditableCredentialRow(
                    label = stringResource(R.string.account_email),
                    value = emailAddress,
                    onValueChange = onEmailChange,
                    enabled = enabled,
                    keyboardType = KeyboardType.Email,
                    testTag = "edit_email",
                )
            } else {
                CopyableDetailRow(
                    label = stringResource(R.string.account_email),
                    value = account.emailAddress,
                    onCopy = onCopy,
                )
            }
        }
        item {
            if (isEditing) {
                EditableCredentialRow(
                    label = stringResource(R.string.account_password),
                    value = password,
                    onValueChange = onPasswordChange,
                    enabled = enabled,
                    keyboardType = KeyboardType.Password,
                    isPassword = true,
                    testTag = "edit_password",
                )
            } else {
                PasswordDetailRow(
                    label = stringResource(R.string.account_password),
                    password = account.password,
                    onCopy = onCopy,
                )
            }
        }
        item { Spacer(Modifier.height(20.dp)) }
        item { SectionLabel(R.string.website) }
        item {
            if (isEditing) {
                EditableCredentialRow(
                    label = stringResource(R.string.website),
                    value = websiteUrl,
                    onValueChange = onWebsiteChange,
                    enabled = enabled,
                    keyboardType = KeyboardType.Uri,
                    testTag = "edit_website",
                )
            } else {
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
}

@Composable
internal fun EditableCredentialRow(
    label: String,
    value: String,
    onValueChange: (String) -> Unit,
    enabled: Boolean,
    keyboardType: KeyboardType,
    testTag: String,
    isPassword: Boolean = false,
) {
    var focused by remember { mutableStateOf(false) }
    var passwordVisible by remember { mutableStateOf(false) }
    val shape = RoundedCornerShape(10.dp)

    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(top = 10.dp),
    ) {
        Text(
            text = label,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            style = MaterialTheme.typography.bodySmall,
        )
        BasicTextField(
            value = value,
            onValueChange = onValueChange,
            modifier = Modifier
                .fillMaxWidth()
                .padding(top = 6.dp, bottom = 10.dp)
                .heightIn(min = 44.dp)
                .clip(shape)
                .background(MaterialTheme.colorScheme.surfaceVariant)
                .border(
                    width = 1.dp,
                    color = if (focused) {
                        MaterialTheme.colorScheme.primary
                    } else {
                        Color.Transparent
                    },
                    shape = shape,
                )
                .onFocusChanged { focused = it.isFocused }
                .testTag(testTag),
            enabled = enabled,
            singleLine = true,
            textStyle = MaterialTheme.typography.bodyLarge.copy(
                color = MaterialTheme.colorScheme.onSurface,
            ),
            keyboardOptions = KeyboardOptions(keyboardType = keyboardType),
            visualTransformation = if (isPassword && !passwordVisible) {
                PasswordVisualTransformation()
            } else {
                VisualTransformation.None
            },
            cursorBrush = SolidColor(MaterialTheme.colorScheme.primary),
            decorationBox = { innerTextField ->
                Row(
                    modifier = Modifier.padding(start = 12.dp, end = 4.dp),
                    verticalAlignment = Alignment.CenterVertically,
                ) {
                    Box(modifier = Modifier.weight(1f)) { innerTextField() }
                    if (isPassword) {
                        IconButton(
                            enabled = enabled,
                            onClick = { passwordVisible = !passwordVisible },
                        ) {
                            Icon(
                                if (passwordVisible) Icons.Outlined.Visibility
                                else Icons.Outlined.VisibilityOff,
                                contentDescription = stringResource(
                                    if (passwordVisible) R.string.hide_password
                                    else R.string.show_password,
                                ),
                            )
                        }
                    } else {
                        Spacer(Modifier.width(8.dp))
                    }
                }
            },
        )
    }
    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
}

@Composable
private fun NotesTab(
    account: Account,
    notes: String?,
    isEditing: Boolean,
    enabled: Boolean,
    onNotesChange: (String) -> Unit,
) {
    if (isEditing) {
        var focused by remember { mutableStateOf(false) }
        val shape = RoundedCornerShape(10.dp)

        Column(
            modifier = Modifier
                .fillMaxSize()
                .verticalScroll(rememberScrollState())
                .padding(horizontal = 20.dp, vertical = 16.dp),
        ) {
            SectionLabel(R.string.notes)
            BasicTextField(
                value = notes.orEmpty(),
                onValueChange = onNotesChange,
                enabled = enabled,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(top = 6.dp)
                    .heightIn(min = 160.dp)
                    .clip(shape)
                    .background(MaterialTheme.colorScheme.surfaceVariant)
                    .border(
                        width = 1.dp,
                        color = if (focused) MaterialTheme.colorScheme.primary else Color.Transparent,
                        shape = shape,
                    )
                    .onFocusChanged { focused = it.isFocused }
                    .padding(12.dp)
                    .testTag("add_notes"),
                textStyle = MaterialTheme.typography.bodyLarge.copy(
                    color = MaterialTheme.colorScheme.onSurface,
                ),
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Text),
                cursorBrush = SolidColor(MaterialTheme.colorScheme.primary),
                decorationBox = { innerTextField ->
                    Box {
                        if (notes.orEmpty().isEmpty()) {
                            Text(
                                stringResource(R.string.account_notes_hint),
                                color = MaterialTheme.colorScheme.onSurfaceVariant,
                            )
                        }
                        innerTextField()
                    }
                },
            )
        }
    } else {
        val normalizedNotes = account.notes.normalizedDetailValue()
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
                        if (visible) Icons.Outlined.Visibility else Icons.Outlined.VisibilityOff,
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
                    if (visible) Icons.Outlined.Visibility else Icons.Outlined.VisibilityOff,
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
        )
    }
}

private fun previewAccount() = Account(
    guid = UUID.fromString("00000000-0000-0000-0000-000000000001"),
    dateCreated = "2024-03-14T18:32:00Z",
    dateModified = "2026-08-20T09:15:00Z",
    name = "GitHub",
    username = "Username",
    emailAddress = "user@example.com",
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
