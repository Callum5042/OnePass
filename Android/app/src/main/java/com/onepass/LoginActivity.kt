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
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.colorResource
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.lifecycleScope
import com.onepass.services.FileEncoder
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
                Scaffold(modifier = Modifier.fillMaxSize()) { innerPadding ->
                    Surface(
                        modifier = Modifier.padding(paddingValues = innerPadding),
                    ) {
                        LoginView(
                            onLoginSuccess = { data ->
                                // Store
                                val repository = (application as OnePassApplication).vaultRepository
                                repository.unlock(data)

                                // Change activity
                                val activity = Intent(
                                    this@LoginActivity,
                                    MainActivity::class.java
                                )

                                startActivity(activity)
                                finish()
                            }
                        )
                    }
                }
            }
        }
    }
}

@Composable
fun LoginView(
    onLoginSuccess: (data: OnePassData) -> Unit = {}
) {
    val scrollState = rememberScrollState()

    var fileUri by remember { mutableStateOf<Uri?>(null) }
    var password by remember { mutableStateOf("") }

    val context = LocalContext.current
    val activity = context as ComponentActivity
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

            FileInput(
                onFileSelected = { uri ->
                    fileUri = uri
                }
            )

            Spacer(
                modifier = Modifier.height(16.dp)
            )

            PasswordInput(
                onPasswordEntered = {
                    password = it
                }
            )

            Spacer(
                modifier = Modifier.height(16.dp)
            )

            Button(
                modifier = Modifier
                    .fillMaxWidth()
                    .testTag("login_button"),
                shape = RoundedCornerShape(3.dp),
                colors = ButtonDefaults.buttonColors(
                    containerColor = Color(0xFF0080FF),
                ),
                onClick = {
                    isLoading = true
                    activity.lifecycleScope.launch {
                        val result = runCatching {
                            withContext(Dispatchers.IO) {
                                fileUri?.let {
                                    context.contentResolver.openInputStream(it) }?.use { input ->
                                    FileEncoder().load(password, input)
                                } ?: error("Unable to open the selected file")
                            }
                        }
                        isLoading = false
                        result.fold(
                            onSuccess = { data ->
                                onLoginSuccess(data)
                            },
                            onFailure = { error -> error.message ?: "Unable to decode the selected file" },
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
                modifier = Modifier.padding(top = 6.dp, bottom = 24.dp),
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
fun FileInput(
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
                        launcher.launch(arrayOf("application/octet-stream", "*/*"))
                    }
            )
        }
    }
}

@Composable
fun PasswordInput(
    onPasswordEntered: (password: String) -> Unit
) {
    var password by remember { mutableStateOf("") }
    var passwordVisible by remember { mutableStateOf(false) }

    Column {
        Text(
            text = "Password",
            color = Color(0xFFF5F5F5),
            modifier = Modifier.padding(bottom = 6.dp),
        )

        OutlinedTextField(
            modifier = Modifier
                .fillMaxWidth()
                .testTag("password_input"),
            value = password,
            onValueChange = {
                password = it
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
                            Icons.Default.VisibilityOff
                        } else {
                            Icons.Default.Visibility
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
