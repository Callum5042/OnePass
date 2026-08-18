package com.onepass

import android.content.Context
import android.widget.Toast
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.input.PasswordVisualTransformation
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
                    Box(
                        modifier = Modifier.padding(innerPadding)
                    ) {
                        FilePicker()
                    }
                }
            }
        }
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