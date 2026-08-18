package com.onepass

import android.R
import android.content.Context
import android.widget.Toast
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.animation.VectorConverter
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Visibility
import androidx.compose.material.icons.filled.VisibilityOff
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonColors
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.IconButtonDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TextField
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.fromColorLong
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.unit.dp
import androidx.lifecycle.lifecycleScope
import com.onepass.services.FileEncoder
import com.onepass.ui.theme.OnePassTheme
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            OnePassTheme {
                Scaffold(modifier = Modifier.fillMaxSize()) { innerPadding ->
                    Surface(
                        modifier = Modifier.padding(innerPadding),
                        color = Color(0xFF003264)
                    ) {
                        LoginView()
                    }
                }
            }
        }
    }
}

@Composable
fun LoginView() {


    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(horizontal = 24.dp),

        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {

        UsernameInput()

        Spacer(
            modifier = Modifier.height(16.dp)
        )

        PasswordInput()

        Spacer(
            modifier = Modifier.height(16.dp)
        )

        Button(
            modifier = Modifier.fillMaxWidth(),
            shape = RoundedCornerShape(3.dp),
            colors = ButtonDefaults.buttonColors(
                containerColor = Color(0xFF0080FF),
            ),
            onClick = {

            }
        ) {
            Text(
                text = "Login"
            )
        }

        Text(
            modifier = Modifier.padding(top = 6.dp),
            text = "Create Account",
            color = Color.Cyan,
        )
    }
}

@Composable
fun UsernameInput() {
    var username by remember { mutableStateOf("") }

    Column(
        horizontalAlignment = Alignment.Start
    ) {
        Text(
            text = "Username",
            color = Color(0xFFF5F5F5),
            modifier = Modifier.padding(bottom = 6.dp),
        )

        OutlinedTextField(
            modifier = Modifier.fillMaxWidth(),
            value = username,
            shape = RoundedCornerShape(8.dp),
            onValueChange = { username = it },
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

@Composable
fun PasswordInput() {
    var password by remember { mutableStateOf("") }
    var passwordVisible by remember { mutableStateOf(false) }

    Column {
        Text(
            text = "Password",
            color = Color(0xFFF5F5F5),
            modifier = Modifier.padding(bottom = 6.dp),
        )

        OutlinedTextField(
            modifier = Modifier.fillMaxWidth(),
            value = password,
            onValueChange = { password = it },
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
                        imageVector = if (passwordVisible) {
                            Icons.Default.VisibilityOff
                        } else {
                            Icons.Default.Visibility
                        },
                        contentDescription = if (passwordVisible) {
                            "Hide password"
                        } else {
                            "Show password"
                        },
                        tint = Color.Gray
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
fun FilePicker() {
    val context = LocalContext.current
    val activity = context as ComponentActivity
    var password by remember { mutableStateOf("") }
    var status by remember { mutableStateOf("Choose a OnePass file to decode.") }
    var isLoading by remember { mutableStateOf(false) }

    val launcher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.OpenDocument(),
    ) { uri ->
        if (uri != null) {
            isLoading = true
            status = "Decoding."
            activity.lifecycleScope.launch {
                val result = runCatching {
                    withContext(Dispatchers.IO) {
                        context.contentResolver.openInputStream(uri)?.use { input ->
                            FileEncoder().load(password, input)
                        } ?: error("Unable to open the selected file")
                    }
                }
                isLoading = false
                status = result.fold(
                    onSuccess = { data ->
                        val suffix = if (data.accounts.size == 1) "account" else "accounts"
                        "Decoded ${data.accounts.size} $suffix successfully."
                    },
                    onFailure = { error -> error.message ?: "Unable to decode the selected file" },
                )
            }
        }
    }

    Column(
        modifier = Modifier.fillMaxWidth().padding(24.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp),
    ) {
        OutlinedTextField(
            value = password,
            onValueChange = { password = it },
            modifier = Modifier.fillMaxWidth(),
            label = { Text("Password") },
            singleLine = true,
            visualTransformation = PasswordVisualTransformation(),
            enabled = !isLoading,
        )
        Button(
            onClick = { launcher.launch(arrayOf("application/octet-stream", "*/*")) },
            enabled = password.isNotEmpty() && !isLoading,
        ) { Text("Open and decode file") }
        if (isLoading) CircularProgressIndicator()
        Text(status)
    }
}

@Composable
fun TestButton(
    context: Context,
    modifier: Modifier,
    onClick: () -> Unit
) {
    Button(
        modifier = modifier,
        onClick = {
            Toast.makeText(
                context,
                "This is a Sample Toast",
                Toast.LENGTH_LONG
            ).show()

            onClick()
        }
    ) {
        Text(
            text = "Me Button"
        )
    }
}

@Composable
fun Greeting(name: String, modifier: Modifier = Modifier) {
    Text(
        text = "Hello $name!",
        modifier = modifier
    )
}

@Preview(showBackground = true)
@Composable
fun GreetingPreview() {
    OnePassTheme {
        Greeting("Android")
    }
}