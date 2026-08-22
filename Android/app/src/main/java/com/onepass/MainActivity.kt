package com.onepass

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Search
import androidx.compose.material.icons.filled.Settings
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.FloatingActionButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults.topAppBarColors
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.colorResource
import androidx.compose.ui.tooling.preview.Preview
import com.onepass.services.Account
import com.onepass.services.OnePassData
import com.onepass.services.VaultState
import com.onepass.ui.theme.OnePassTheme
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import java.util.UUID
import androidx.compose.foundation.lazy.items

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            OnePassTheme {
                val repository = (application as OnePassApplication).vaultRepository
                MainScreen(repository.state)
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MainScreen(
    data: StateFlow<VaultState>
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
                    Text("OnePass")
                },
                actions = {
                    IconButton(onClick = { /* Search */ }) {
                        Icon(
                            imageVector = Icons.Default.Search,
                            contentDescription = "Search"
                        )
                    }

                    IconButton(onClick = { /* Open settings */ }) {
                        Icon(
                            imageVector = Icons.Default.Settings,
                            contentDescription = "Settings"
                        )
                    }
                },
            )
        },
        floatingActionButton = {
            FloatingActionButton(
                onClick = { /* do something */ },
                containerColor = colorResource(R.color.deep_blue),
                contentColor = Color.White,
                elevation = FloatingActionButtonDefaults.bottomAppBarFabElevation()
            ) {
                Icon(
                    Icons.Filled.Add,
                    "Localized description"
                )
            }
        }
    ) { innerPadding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
        ) {

            when (vaultState) {
                VaultState.Locked -> {
                    Text(text = "Vault is locked")
                }
                is VaultState.Unlocked -> {
                    val onePassData = (vaultState as VaultState.Unlocked).data
                    val accounts = onePassData.accounts

                    LazyColumn {
                        items(accounts) { item ->
                            item.username?.let { Text(it) }
                        }
                    }
                }
            }
        }
    }
}

@Preview(showBackground = false)
@Composable
fun MainScreenPreview() {

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