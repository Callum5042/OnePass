package com.onepass

import android.content.Intent
import android.net.Uri
import android.os.Bundle
import android.provider.OpenableColumns
import androidx.activity.ComponentActivity
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.Image
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.imePadding
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.FolderOpen
import androidx.compose.material.icons.filled.Visibility
import androidx.compose.material.icons.filled.VisibilityOff
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Checkbox
import androidx.compose.material3.CheckboxDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.colorResource
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.semantics.Role
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.onepass.services.FileEncoder
import com.onepass.services.InvalidOnePassFileException
import com.onepass.services.InvalidPasswordException
import com.onepass.services.OnePassData
import com.onepass.ui.theme.OnePassTheme
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext

class LoginActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            OnePassTheme {
                var showCreateAccount by remember { mutableStateOf(false) }
                Scaffold(modifier = Modifier.fillMaxSize()) { innerPadding ->
                    Surface(
                        modifier = Modifier.padding(paddingValues = innerPadding),
                    ) {
                        if (showCreateAccount) {
                            val repository = (application as OnePassApplication).vaultRepository
                            CreateAccountView(
                                onBackToLogin = { showCreateAccount = false },
                                onAccountCreated = { data, documentUri, password ->
                                    loginAndOpenMain(data, documentUri, password)
                                },
                                onSaveAccount = { uri, password ->
                                    withContext(Dispatchers.IO) {
                                        repository.create(uri.toString(), password)
                                    }
                                },
                            )
                        } else {
                            LoginView(
                                onCreateAccount = { showCreateAccount = true },
                                onLoginSuccess = { data, documentUri, password ->
                                    loginAndOpenMain(data, documentUri, password)
                                },
                            )
                        }
                    }
                }
            }
        }
    }

    private fun loginAndOpenMain(data: OnePassData, documentUri: Uri, password: CharArray) {
        val repository = (application as OnePassApplication).vaultRepository
        repository.unlock(data, documentUri.toString(), password)

        val activity = Intent(this@LoginActivity, MainActivity::class.java)
        startActivity(activity)
        finish()
    }
}

@Composable
fun LoginView(
    onCreateAccount: () -> Unit = {},
    onLoginSuccess: (data: OnePassData, documentUri: Uri, password: CharArray) -> Unit = { _, _, _ -> }
) {
    val scrollState = rememberScrollState()

    var fileUri by remember { mutableStateOf<Uri?>(null) }
    var password by remember { mutableStateOf("") }
    var rememberMe by remember { mutableStateOf(false)}

    var fileError by remember { mutableStateOf<String?>(null) }
    var passwordError by remember { mutableStateOf<String?>(null) }

    val context = LocalContext.current
    val coroutineScope = rememberCoroutineScope()
    var isLoading by remember { mutableStateOf(false) }

    Surface(
        color = colorResource(R.color.deep_blue)
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(horizontal = 24.dp)
                .imePadding()
                .verticalScroll(state = scrollState),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {

            Image(
                painter = painterResource(id = R.drawable.onepass_logo),
                contentDescription = "Logo",
                modifier = Modifier.fillMaxWidth(0.45f),
                contentScale = ContentScale.FillWidth,
            )

            Column(horizontalAlignment = Alignment.Start) {
                FileInput(
                    isLoading = isLoading,
                    onFileSelected = { uri ->
                        fileUri = uri
                    }
                )

                fileError?.let {
                    Text(
                        it,
                        color = MaterialTheme.colorScheme.error,
                        modifier = Modifier.testTag("login_file_error")
                    )
                }
            }

            Spacer(
                modifier = Modifier.height(16.dp)
            )

            Column(horizontalAlignment = Alignment.Start) {
                PasswordInput(
                    enabled = !isLoading,
                    onPasswordEntered = {
                        password = it
                    }
                )

                passwordError?.let {
                    Text(
                        it,
                        color = MaterialTheme.colorScheme.error,
                        modifier = Modifier.testTag("login_password_error")
                    )
                }
            }

            Column(horizontalAlignment = Alignment.Start) {
                CheckboxWithLabel(
                    label = "Remember me",
                    value = rememberMe)
                {
                    rememberMe = !rememberMe
                }
            }

            Button(
                modifier = Modifier
                    .fillMaxWidth()
                    .testTag("login_button"),
                shape = RoundedCornerShape(3.dp),
                colors = ButtonDefaults.buttonColors(
                    containerColor = Color(0xFF0080FF),
                ),
                onClick = {
                    val validation = validateLoginAccount(fileUri != null, password)
                    fileError = validation.fileError
                    passwordError = validation.passwordError
                    if (!validation.isValid) return@Button

                    isLoading = true
                    coroutineScope.launch {
                        val passwordChars = password.toCharArray()
                        val result = runCatching {
                            withContext(Dispatchers.IO) {
                                val selectedUri = fileUri ?: error("Select a vault file")
                                val data = selectedUri.let {
                                    context.contentResolver.openInputStream(it) }?.use { input ->
                                        FileEncoder().load(passwordChars, input)
                                } ?: error("Unable to open the selected file")

                                LoginResult(data, selectedUri, passwordChars)
                            }
                        }
                        isLoading = false
                        result.fold(
                            onSuccess = { login ->
                                onLoginSuccess(
                                    login.data,
                                    login.documentUri,
                                    login.password
                                )
                            },
                            onFailure = { error ->
                                when (error) {
                                    is InvalidPasswordException -> {
                                        passwordError = error.message
                                    }
                                    is InvalidOnePassFileException -> {
                                        fileError = error.message
                                    }
                                    else -> {
                                        fileError = error.message ?: "Unable to decode the selected file"
                                    }
                                }
                            },
                        )
                    }
                },
                enabled = !isLoading,
            ) {
                if (isLoading) {
                    CircularProgressIndicator(color = Color.White)
                    Text(
                        text = "Decrypting..."
                    )
                } else {
                    Text(
                        text = "Login"
                    )
                }
            }

            Text(
                modifier = Modifier
                    .padding(top = 6.dp, bottom = 24.dp)
                    .clickable(onClick = {
                        if (isLoading) return@clickable
                        onCreateAccount()
                    }),
                text = "Create Account",
                color = Color.White.copy(alpha = 0.8f),
            )

            Text(
                text = "v${BuildConfig.VERSION_NAME}",
                color = Color.White.copy(alpha = 0.6f),
                style = MaterialTheme.typography.bodySmall
            )
        }
    }
}

@Composable
fun CreateAccountView(
    onBackToLogin: () -> Unit = {},
    onAccountCreated: (data: OnePassData, documentUri: Uri, password: CharArray) -> Unit = { _, _, _ -> },
    onSaveAccount: suspend (uri: Uri, password: CharArray) -> Boolean = { _, _ -> false },
) {
    val context = LocalContext.current
    val coroutineScope = rememberCoroutineScope()
    val scrollState = rememberScrollState()
    var fileUri by remember { mutableStateOf<Uri?>(null) }
    var fileName by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var repeatPassword by remember { mutableStateOf("") }
    var fileError by remember { mutableStateOf<String?>(null) }
    var passwordError by remember { mutableStateOf<String?>(null) }
    var repeatPasswordError by remember { mutableStateOf<String?>(null) }
    var generalError by remember { mutableStateOf<String?>(null) }
    var isLoading by remember { mutableStateOf(false) }

    val launcher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.CreateDocument("application/octet-stream"),
    ) { uri ->
        if (uri != null) {
            fileUri = uri
            fileName = uri.lastPathSegment?.substringAfterLast('/') ?: "Vault file"
            context.contentResolver.query(
                uri,
                arrayOf(OpenableColumns.DISPLAY_NAME),
                null,
                null,
                null,
            )?.use { cursor ->
                if (cursor.moveToFirst()) {
                    val column = cursor.getColumnIndex(OpenableColumns.DISPLAY_NAME)
                    if (column >= 0) fileName = cursor.getString(column)
                }
            }
            fileError = null
        }
    }

    Surface(color = colorResource(R.color.deep_blue)) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(horizontal = 24.dp)
                .imePadding()
                .verticalScroll(scrollState),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally,
        ) {
            Image(
                painter = painterResource(id = R.drawable.onepass_logo),
                contentDescription = "Logo",
                modifier = Modifier.fillMaxWidth(0.45f),
                contentScale = ContentScale.FillWidth,
            )

            Spacer(Modifier.height(18.dp))

            Column(horizontalAlignment = Alignment.Start) {
                Text("Filename", color = Color(0xFFF5F5F5), modifier = Modifier.padding(bottom = 6.dp))
                Box {
                    OutlinedTextField(
                        modifier = Modifier
                            .fillMaxWidth(),
                        value = fileName,
                        onValueChange = {},
                        placeholder = { Text("Create file ...", color = Color.Gray) },
                        trailingIcon = {
                            Icon(Icons.Default.FolderOpen, contentDescription = "Choose file", tint = Color.Gray)
                        },
                        readOnly = true,
                        shape = RoundedCornerShape(8.dp),
                        colors = OutlinedTextFieldDefaults.colors(
                            focusedTextColor = Color.Black,
                            unfocusedTextColor = Color.Black,
                            focusedContainerColor = Color.White,
                            unfocusedContainerColor = Color.White,
                            focusedBorderColor = Color(0xFF0080FF),
                            unfocusedBorderColor = Color(0xFFABADB3),
                            cursorColor = Color.Black,
                        ),
                    )
                    Box(
                        modifier = Modifier
                            .matchParentSize()
                            .clickable {
                                if (isLoading) return@clickable
                                launcher.launch("OnePass Vault.onepass")
                            }
                            .testTag("choose_vault_button"),
                    )
                }
                fileError?.let { Text(it, color = MaterialTheme.colorScheme.error, modifier = Modifier.testTag("file_error")) }
            }

            Spacer(Modifier.height(12.dp))
            PasswordInput(
                value = password,
                onPasswordEntered = { password = it },
                label = "Password",
                testTag = "create_password_input",
                enabled = !isLoading
            )

            passwordError?.let { Text(it, color = MaterialTheme.colorScheme.error, modifier = Modifier.fillMaxWidth().testTag("password_error")) }
            Spacer(Modifier.height(12.dp))

            PasswordInput(
                value = repeatPassword,
                onPasswordEntered = { repeatPassword = it },
                label = "Repeat password",
                testTag = "repeat_password_input",
                enabled = !isLoading
            )

            repeatPasswordError?.let { Text(it, color = MaterialTheme.colorScheme.error, modifier = Modifier.fillMaxWidth().testTag("repeat_password_error")) }
            generalError?.let { Text(it, color = MaterialTheme.colorScheme.error, modifier = Modifier.fillMaxWidth().testTag("create_error")) }
            Spacer(Modifier.height(16.dp))

            Button(
                modifier = Modifier.fillMaxWidth().testTag("create_account_button"),
                enabled = !isLoading,
                onClick = {
                    val validation = validateCreateAccount(fileUri != null, password, repeatPassword)
                    fileError = validation.fileError
                    passwordError = validation.passwordError
                    repeatPasswordError = validation.repeatPasswordError
                    generalError = null
                    if (validation.isValid) {
                        isLoading = true
                        val passwordChars = password.toCharArray()
                        coroutineScope.launch {
                            val uri = fileUri!!
                            val created = onSaveAccount(uri, passwordChars)
                            isLoading = false
                            if (created) onAccountCreated(OnePassData(), uri, passwordChars)
                            else generalError = "Unable to create the vault file"
                        }
                    }
                },
                shape = RoundedCornerShape(3.dp),
                colors = ButtonDefaults.buttonColors(containerColor = Color(0xFF0080FF)),
            ) { if (isLoading) CircularProgressIndicator(color = Color.White) else Text("Create Account") }

            if (!isLoading) {
                Text(
                    "Back to Login",
                    color = Color.White.copy(alpha = 0.8f),
                    modifier = Modifier
                        .padding(top = 14.dp)
                        .clickable(onClick = onBackToLogin)
                        .testTag("back_to_login")
                )
            }

            Text(
                "v${BuildConfig.VERSION_NAME}",
                color = Color.White.copy(alpha = 0.6f),
                style = MaterialTheme.typography.bodySmall,
                modifier = Modifier.padding(top = 8.dp, bottom = 24.dp)
            )
        }
    }
}

internal data class CreateAccountValidation(
    val fileError: String? = null,
    val passwordError: String? = null,
    val repeatPasswordError: String? = null,
) {
    val isValid: Boolean
        get() = fileError == null && passwordError == null && repeatPasswordError == null
}

internal data class LoginAccountValidation(
    val fileError: String? = null,
    val passwordError: String? = null
) {
    val isValid: Boolean
        get() = fileError == null && passwordError == null
}

internal fun validateCreateAccount(
    fileSelected: Boolean,
    password: String,
    repeatPassword: String,
): CreateAccountValidation = CreateAccountValidation(
    fileError = if (fileSelected) null else "Choose a file for your vault",
    passwordError = if (password.length >= 10) null else "Password must be at least 10 characters",
    repeatPasswordError = if (password == repeatPassword) null else "Passwords do not match",
)

internal fun validateLoginAccount(
    fileSelected: Boolean,
    password: String?,
): LoginAccountValidation = LoginAccountValidation(
    fileError = if (fileSelected) null else "Choose a file",
    passwordError = if (!password.isNullOrEmpty()) null else "Password is required"
)

@Composable
fun FileInput(
    isLoading: Boolean,
    onFileSelected: (Uri) -> Unit
) {
    var fileName by remember { mutableStateOf("") }
    var fileUri by remember { mutableStateOf<Uri?>(null) }

    val context = LocalContext.current
    val launcher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.OpenDocument(),
    ) { uri ->
        uri ?: return@rememberLauncherForActivityResult

        fileUri = uri
        onFileSelected(uri)

        runCatching {
            context.contentResolver.takePersistableUriPermission(
                uri,
                Intent.FLAG_GRANT_READ_URI_PERMISSION or Intent.FLAG_GRANT_WRITE_URI_PERMISSION,
            )
        }

        context.contentResolver.query(
            uri,
            arrayOf(OpenableColumns.DISPLAY_NAME),
            null,
            null,
            null
        )?.use { cursor ->
            if (cursor.moveToFirst()) {
                val column = cursor.getColumnIndex(OpenableColumns.DISPLAY_NAME)

                if (column >= 0) {
                    fileName = cursor.getString(column)
                }
            }
        }
    }

    Column(
        horizontalAlignment = Alignment.Start
    ) {
        Text(
            text = "Filename",
            color = Color(0xFFF5F5F5),
            modifier = Modifier.padding(bottom = 6.dp),
        )

        Box {
            OutlinedTextField(
                modifier = Modifier
                    .fillMaxWidth()
                    .testTag("username_input"),
                placeholder = { Text("Select file ...", color = Color.Gray) },
                value = fileName,
                onValueChange = {},
                trailingIcon = {
                    Icon(
                        Icons.Default.FolderOpen,
                        contentDescription = "Choose file",
                        tint = Color.Gray,
                    )
                },
                readOnly = true,
                shape = RoundedCornerShape(8.dp),
                colors = OutlinedTextFieldDefaults.colors(
                    focusedTextColor = Color.Black,
                    unfocusedTextColor = Color.Black,

                    focusedContainerColor = Color.White,
                    unfocusedContainerColor = Color.White,

                    focusedBorderColor = Color(0xFF0080FF),
                    unfocusedBorderColor = Color(0xFFABADB3),

                    cursorColor = Color.Black
                )
            )

            Box(
                modifier = Modifier
                    .matchParentSize()
                    .clickable {
                        if (isLoading) return@clickable
                        launcher.launch(arrayOf("application/octet-stream", "*/*"))
                    }
            )
        }
    }
}

private data class LoginResult(
    val data: OnePassData,
    val documentUri: Uri,
    val password: CharArray,
)

@Composable
fun PasswordInput(
    value: String? = null,
    onPasswordEntered: (password: String) -> Unit,
    label: String = "Password",
    testTag: String = "password_input",
    enabled: Boolean = true
) {
    var internalPassword by remember { mutableStateOf("") }
    val password = value ?: internalPassword
    var passwordVisible by remember { mutableStateOf(false) }

    Column {
        Text(
            text = label,
            color = Color(0xFFF5F5F5),
            modifier = Modifier.padding(bottom = 6.dp),
        )

        OutlinedTextField(
            modifier = Modifier
                .fillMaxWidth()
                .testTag(testTag),
            value = password,
            onValueChange = {
                if (!enabled) return@OutlinedTextField

                internalPassword = it
                onPasswordEntered(it)
            },
            singleLine = true,
            shape = RoundedCornerShape(8.dp),
            visualTransformation = if (passwordVisible) {
                VisualTransformation.None
            } else {
                PasswordVisualTransformation()
            },
            trailingIcon = {
                IconButton(
                    onClick = { passwordVisible = !passwordVisible },
                ) {
                    Icon(
                        tint = Color.Gray,
                        imageVector = if (passwordVisible) {
                            Icons.Default.Visibility
                        } else {
                            Icons.Default.VisibilityOff
                        },
                        contentDescription = if (passwordVisible) {
                            "Hide password"
                        } else {
                            "Show password"
                        }
                    )
                }
            },

            colors = OutlinedTextFieldDefaults.colors(
                focusedTextColor = Color.Black,
                unfocusedTextColor = Color.Black,

                focusedContainerColor = Color.White,
                unfocusedContainerColor = Color.White,

                focusedBorderColor = Color(0xFF0080FF),
                unfocusedBorderColor = Color(0xFFABADB3),

                cursorColor = Color.Black
            )
        )
    }
}

@Preview(showBackground = true)
@Composable
fun LoginViewPreview() {
    OnePassTheme {
        LoginView()
    }
}

@Composable
private fun CheckboxWithLabel(
    label: String,
    value: Boolean,
    onChecked: () -> Unit
) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 16.dp)
            .clickable(
                role = Role.Checkbox,
                onClick = onChecked,
            ),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        Checkbox(
            checked = value,
            onCheckedChange = null,
        )

        Text(
            modifier = Modifier.padding(start = 8.dp),
            text = label,
            color = Color(0xFFF5F5F5),
        )
    }
}

@Preview(showBackground = true)
@Composable
private fun PreviewCheckboxWithLabelChecked() {
    OnePassTheme {
        Surface(
            color = colorResource(R.color.deep_blue)
        ) {
            CheckboxWithLabel(
                "Checkbox Label",
                true,
                onChecked = {}
            )
        }
    }
}

@Preview(showBackground = true)
@Composable
private fun PreviewCheckboxWithLabelUnchecked() {
    OnePassTheme {
        Surface(
            color = colorResource(R.color.deep_blue)
        ) {
            CheckboxWithLabel(
                "Checkbox Label",
                false,
                onChecked = {}
            )
        }
    }
}