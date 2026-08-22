package com.onepass

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.BasicTextField
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.automirrored.outlined.StickyNote2
import androidx.compose.material.icons.outlined.Key
import androidx.compose.material.icons.outlined.Save
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.PrimaryTabRow
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.Tab
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults.topAppBarColors
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.focus.onFocusChanged
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.SolidColor
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.colorResource
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import com.onepass.services.NewAccountDetails
import com.onepass.services.VaultAddResult
import com.onepass.ui.theme.OnePassTheme
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext

class AddAccountActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            OnePassTheme {
                val repository = (application as OnePassApplication).vaultRepository
                AddAccountScreen(
                    onBack = ::finish,
                    onAccountAdded = ::finish,
                    onSaveAccount = { details ->
                        withContext(Dispatchers.IO) {
                            repository.addAccount(details)
                        }
                    },
                )
            }
        }
    }
}

private enum class AddAccountTab {
    Details,
    Notes,
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AddAccountScreen(
    onBack: () -> Unit,
    onAccountAdded: () -> Unit,
    onSaveAccount: suspend (NewAccountDetails) -> VaultAddResult,
) {
    var selectedTabIndex by rememberSaveable { mutableIntStateOf(0) }
    var name by rememberSaveable { mutableStateOf("") }
    var username by rememberSaveable { mutableStateOf("") }
    var emailAddress by rememberSaveable { mutableStateOf("") }
    var password by rememberSaveable { mutableStateOf("") }
    var websiteUrl by rememberSaveable { mutableStateOf("") }
    var notes by rememberSaveable { mutableStateOf("") }
    var isSaving by rememberSaveable { mutableStateOf(false) }
    val snackbarHostState = remember { SnackbarHostState() }
    val coroutineScope = rememberCoroutineScope()
    val saveFailedMessage = stringResource(R.string.account_save_failed)
    val rollbackFailedMessage = stringResource(R.string.account_save_rollback_failed)

    val details = NewAccountDetails(
        name = name,
        username = username,
        emailAddress = emailAddress,
        password = password,
        websiteUrl = websiteUrl,
        notes = notes,
    )

    Scaffold(
        topBar = {
            TopAppBar(
                colors = topAppBarColors(
                    containerColor = colorResource(R.color.deep_blue),
                    titleContentColor = Color.White,
                    actionIconContentColor = Color.White,
                    navigationIconContentColor = Color.White,
                ),
                title = { Text(stringResource(R.string.add_account_details)) },
                navigationIcon = {
                    IconButton(enabled = !isSaving, onClick = onBack) {
                        Icon(
                            Icons.AutoMirrored.Filled.ArrowBack,
                            contentDescription = stringResource(R.string.back),
                        )
                    }
                },
                actions = {
                    IconButton(
                        enabled = name.isNotBlank() && !isSaving,
                        onClick = {
                            isSaving = true
                            coroutineScope.launch {
                                when (val result = onSaveAccount(details)) {
                                    VaultAddResult.Success -> onAccountAdded()
                                    VaultAddResult.VaultLocked -> {
                                        isSaving = false
                                        snackbarHostState.showSnackbar(saveFailedMessage)
                                    }
                                    is VaultAddResult.SaveFailed -> {
                                        isSaving = false
                                        snackbarHostState.showSnackbar(
                                            if (result.rollbackSucceeded) saveFailedMessage
                                            else rollbackFailedMessage,
                                        )
                                    }
                                }
                            }
                        },
                    ) {
                        Icon(
                            Icons.Outlined.Save,
                            contentDescription = stringResource(R.string.save_account),
                        )
                    }
                },
            )
        },
        snackbarHost = { SnackbarHost(snackbarHostState) },
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding),
        ) {
            PrimaryTabRow(selectedTabIndex = selectedTabIndex) {
                AddAccountTab.entries.forEachIndexed { index, tab ->
                    Tab(
                        selected = selectedTabIndex == index,
                        enabled = !isSaving,
                        onClick = { selectedTabIndex = index },
                        text = {
                            Text(
                                stringResource(
                                    if (tab == AddAccountTab.Details) R.string.details
                                    else R.string.notes,
                                ),
                            )
                        },
                        icon = {
                            Icon(
                                if (tab == AddAccountTab.Details) Icons.Outlined.Key
                                else Icons.AutoMirrored.Outlined.StickyNote2,
                                contentDescription = null,
                            )
                        },
                    )
                }
            }

            when (AddAccountTab.entries[selectedTabIndex]) {
                AddAccountTab.Details -> AddDetailsTab(
                    name = name,
                    username = username,
                    emailAddress = emailAddress,
                    password = password,
                    websiteUrl = websiteUrl,
                    enabled = !isSaving,
                    onNameChange = { name = it },
                    onUsernameChange = { username = it },
                    onEmailChange = { emailAddress = it },
                    onPasswordChange = { password = it },
                    onWebsiteChange = { websiteUrl = it },
                )
                AddAccountTab.Notes -> AddNotesTab(
                    notes = notes,
                    enabled = !isSaving,
                    onNotesChange = { notes = it },
                )
            }
        }
    }
}

@Composable
private fun AddDetailsTab(
    name: String,
    username: String,
    emailAddress: String,
    password: String,
    websiteUrl: String,
    enabled: Boolean,
    onNameChange: (String) -> Unit,
    onUsernameChange: (String) -> Unit,
    onEmailChange: (String) -> Unit,
    onPasswordChange: (String) -> Unit,
    onWebsiteChange: (String) -> Unit,
) {
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(start = 20.dp, end = 20.dp, top = 16.dp, bottom = 28.dp),
    ) {
        item {
            EditableCredentialRow(
                label = stringResource(R.string.account_name),
                value = name,
                onValueChange = onNameChange,
                enabled = enabled,
                keyboardType = KeyboardType.Text,
                testTag = "add_name",
            )
        }
        item {
            EditableCredentialRow(
                label = stringResource(R.string.account_username),
                value = username,
                onValueChange = onUsernameChange,
                enabled = enabled,
                keyboardType = KeyboardType.Text,
                testTag = "add_username",
            )
        }
        item {
            EditableCredentialRow(
                label = stringResource(R.string.account_email),
                value = emailAddress,
                onValueChange = onEmailChange,
                enabled = enabled,
                keyboardType = KeyboardType.Email,
                testTag = "add_email",
            )
        }
        item {
            EditableCredentialRow(
                label = stringResource(R.string.account_password),
                value = password,
                onValueChange = onPasswordChange,
                enabled = enabled,
                keyboardType = KeyboardType.Password,
                testTag = "add_password",
                isPassword = true,
            )
        }
        item {
            EditableCredentialRow(
                label = stringResource(R.string.website),
                value = websiteUrl,
                onValueChange = onWebsiteChange,
                enabled = enabled,
                keyboardType = KeyboardType.Uri,
                testTag = "add_website",
            )
        }
    }
}

@Composable
private fun AddNotesTab(
    notes: String,
    enabled: Boolean,
    onNotesChange: (String) -> Unit,
) {
    var focused by remember { mutableStateOf(false) }
    val shape = RoundedCornerShape(10.dp)
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(horizontal = 20.dp, vertical = 16.dp),
    ) {
        Text(
            text = stringResource(R.string.notes),
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            style = MaterialTheme.typography.bodySmall,
        )
        BasicTextField(
            value = notes,
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
                    if (notes.isEmpty()) {
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
}
