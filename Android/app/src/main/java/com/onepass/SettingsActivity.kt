package com.onepass

import android.os.Bundle
import android.security.keystore.KeyPermanentlyInvalidatedException
import android.util.Log
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.biometric.BiometricManager
import androidx.biometric.BiometricPrompt
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.core.content.ContextCompat
import androidx.fragment.app.FragmentActivity
import com.onepass.services.ActiveVaultSession
import com.onepass.services.BiometricCredentialStore
import com.onepass.services.VaultRepository
import com.onepass.ui.theme.OnePassTheme
import javax.crypto.Cipher

class SettingsActivity : FragmentActivity() {
    private lateinit var repository: VaultRepository
    private lateinit var credentialStore: BiometricCredentialStore

    private var settingsState by mutableStateOf(BiometricSettingsState())
    private var biometricPrompt: BiometricPrompt? = null
    private var pendingEnrollment: PendingEnrollment? = null
    private var pendingDisable: PendingDisable? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()

        repository = (application as OnePassApplication).vaultRepository
        credentialStore = BiometricCredentialStore(this)
        refreshState()

        setContent {
            OnePassTheme {
                SettingsScreen(
                    state = settingsState,
                    onBiometricToggle = ::onBiometricToggle,
                    onBack = ::finish,
                )
            }
        }
    }

    override fun onDestroy() {
        pendingEnrollment?.session?.clearPassword()
        pendingEnrollment = null
        pendingDisable?.credential?.clear()
        pendingDisable = null

        biometricPrompt?.cancelAuthentication()
        biometricPrompt = null
        super.onDestroy()
    }

    private data class PendingEnrollment(
        val session: ActiveVaultSession,
        val cipher: Cipher,
    )

    private data class PendingDisable(
        val credential: BiometricCredentialStore.StoredCredential,
        val cipher: Cipher,
    )

    private enum class BiometricOperation {
        Enroll,
        Disable,
    }

    private fun onBiometricToggle(enabled: Boolean) {
        if (settingsState.isBusy) return
        if (enabled) {
            beginEnrollment()
        } else {
            beginDisable()
        }
    }

    private fun beginEnrollment() {
        if (settingsState.availability != BiometricAvailability.Available) return

        val session = repository.copyActiveSession()
        if (session == null) {
            refreshState(getString(R.string.biometric_unlock_session_unavailable))
            return
        }

        val cipher = try {
            credentialStore.createEncryptionCipher(session.documentUri)
        } catch (error: Throwable) {
            session.clearPassword()
            handleCredentialError(error)
            return
        }

        pendingEnrollment = PendingEnrollment(session, cipher)
        settingsState = settingsState.copy(isBusy = true, message = null)
        try {
            showBiometricPrompt(
                operation = BiometricOperation.Enroll,
                title = R.string.biometric_unlock_enable_title,
                subtitle = R.string.biometric_unlock_enable_subtitle,
                cipher = cipher,
            )
        } catch (error: Throwable) {
            pendingEnrollment?.session?.clearPassword()
            pendingEnrollment = null
            handleCredentialError(error)
        }
    }

    private fun beginDisable() {
        if (settingsState.availability != BiometricAvailability.Available) {
            refreshState(getString(R.string.biometric_unlock_unavailable))
            return
        }

        val credential = try {
            credentialStore.readCredential()
        } catch (_: Throwable) {
            clearCredentialQuietly()
            refreshState(getString(R.string.biometric_unlock_reenroll))
            return
        }
        if (credential == null) {
            refreshState()
            return
        }

        val cipher = try {
            credentialStore.createDecryptionCipher(credential)
        } catch (error: Throwable) {
            credential.clear()
            handleCredentialError(error)
            return
        }

        pendingDisable = PendingDisable(credential, cipher)
        settingsState = settingsState.copy(isBusy = true, message = null)
        try {
            showBiometricPrompt(
                operation = BiometricOperation.Disable,
                title = R.string.biometric_unlock_disable_title,
                subtitle = R.string.biometric_unlock_disable_subtitle,
                cipher = cipher,
            )
        } catch (error: Throwable) {
            pendingDisable?.credential?.clear()
            pendingDisable = null
            handleCredentialError(error)
        }
    }

    private fun showBiometricPrompt(
        operation: BiometricOperation,
        title: Int,
        subtitle: Int,
        cipher: Cipher,
    ) {
        val prompt = BiometricPrompt(
            this,
            ContextCompat.getMainExecutor(this),
            object : BiometricPrompt.AuthenticationCallback() {
                override fun onAuthenticationSucceeded(result: BiometricPrompt.AuthenticationResult) {
                    when (operation) {
                        BiometricOperation.Enroll -> finishEnrollment(result)
                        BiometricOperation.Disable -> finishDisable(result)
                    }
                }

                override fun onAuthenticationError(errorCode: Int, errString: CharSequence) {
                    finishAuthentication(errString.toString())
                }
            },
        )
        biometricPrompt = prompt
        prompt.authenticate(
            BiometricPrompt.PromptInfo.Builder()
                .setTitle(getString(title))
                .setSubtitle(getString(subtitle))
                .setAllowedAuthenticators(BiometricManager.Authenticators.BIOMETRIC_STRONG)
                .setNegativeButtonText(getString(R.string.cancel))
                .build(),
            BiometricPrompt.CryptoObject(cipher),
        )
    }

    private fun finishEnrollment(result: BiometricPrompt.AuthenticationResult) {
        val pending = pendingEnrollment ?: return
        pendingEnrollment = null
        biometricPrompt = null

        try {
            credentialStore.saveCredential(
                vaultUri = pending.session.documentUri,
                password = pending.session.password,
                cipher = checkNotNull(result.cryptoObject?.cipher),
            )
            refreshState(getString(R.string.biometric_unlock_enabled_message))
        } catch (error: Throwable) {
            handleCredentialError(error)
        } finally {
            pending.session.clearPassword()
        }
    }

    private fun finishDisable(result: BiometricPrompt.AuthenticationResult) {
        val pending = pendingDisable ?: return
        pendingDisable = null
        biometricPrompt = null

        var decryptedPassword: CharArray? = null
        try {
            decryptedPassword = credentialStore.decryptCredential(
                pending.credential,
                checkNotNull(result.cryptoObject?.cipher),
            )
            credentialStore.clear()
            refreshState(getString(R.string.biometric_unlock_disabled_message))
        } catch (error: Throwable) {
            handleCredentialError(error)
        } finally {
            decryptedPassword?.fill('\u0000')
            pending.credential.clear()
        }
    }

    private fun finishAuthentication(message: String) {
        pendingEnrollment?.session?.clearPassword()
        pendingEnrollment = null
        pendingDisable?.credential?.clear()
        pendingDisable = null
        biometricPrompt = null
        refreshState(message)
    }

    private fun refreshState(message: String? = null, isBusy: Boolean = false) {
        val availability = biometricAvailability()
        settingsState = BiometricSettingsState(
            enabled = hasStoredCredential(),
            availability = availability,
            isBusy = isBusy,
            message = message,
        )
    }

    private fun hasStoredCredential(): Boolean {
        val credential = try {
            credentialStore.readCredential()
        } catch (_: Throwable) {
            return false
        } ?: return false
        val matchesActiveVault = credential.vaultUri == repository.activeDocumentUri()
        credential.clear()
        return matchesActiveVault
    }

    private fun biometricAvailability(): BiometricAvailability = when (
        runCatching {
            BiometricManager.from(this).canAuthenticate(
                BiometricManager.Authenticators.BIOMETRIC_STRONG,
            )
        }.getOrDefault(BiometricManager.BIOMETRIC_ERROR_HW_UNAVAILABLE)
    ) {
        BiometricManager.BIOMETRIC_SUCCESS -> BiometricAvailability.Available
        BiometricManager.BIOMETRIC_ERROR_NO_HARDWARE -> BiometricAvailability.NoHardware
        BiometricManager.BIOMETRIC_ERROR_NONE_ENROLLED -> BiometricAvailability.NoEnrollment
        else -> BiometricAvailability.Unavailable
    }

    private fun handleCredentialError(error: Throwable) {
        // Log exception types only; never log passwords, vault URIs or credential contents.
        Log.e("OnePassBiometrics", generateSequence(error) { it.cause }
            .joinToString(" -> ") { it.javaClass.simpleName })
        if (error.hasCause<KeyPermanentlyInvalidatedException>()) {
            clearCredentialQuietly()
            refreshState(getString(R.string.biometric_unlock_reenroll))
        } else {
            refreshState(getString(R.string.biometric_unlock_storage_failed))
        }
    }

    private fun clearCredentialQuietly() {
        runCatching { credentialStore.clear() }
    }
}

private inline fun <reified T : Throwable> Throwable.hasCause(): Boolean =
    generateSequence(this) { it.cause }.any { T::class.java.isInstance(it) }

@OptIn(ExperimentalMaterial3Api::class)
@Composable
internal fun SettingsScreen(
    state: BiometricSettingsState = BiometricSettingsState(),
    onBiometricToggle: (Boolean) -> Unit = {},
    onBack: () -> Unit = {},
) {
    val descriptionResource = when {
        state.enabled -> R.string.biometric_unlock_enabled
        state.availability == BiometricAvailability.Available -> R.string.biometric_unlock_ready
        state.availability == BiometricAvailability.NoHardware -> R.string.biometric_unlock_no_hardware
        state.availability == BiometricAvailability.NoEnrollment -> R.string.biometric_unlock_no_enrollment
        else -> R.string.biometric_unlock_unavailable
    }
    val toggleEnabled = state.availability == BiometricAvailability.Available && !state.isBusy

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(stringResource(R.string.settings)) },
                navigationIcon = {
                    IconButton(
                        onClick = onBack,
                        modifier = Modifier.testTag("settings_back"),
                    ) {
                        Icon(
                            imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                            contentDescription = stringResource(R.string.back),
                        )
                    }
                },
            )
        },
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
                .padding(24.dp),
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically,
            ) {
                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = stringResource(R.string.biometric_unlock),
                        style = MaterialTheme.typography.titleMedium,
                    )
                    Spacer(Modifier.height(4.dp))
                    Text(
                        text = stringResource(descriptionResource),
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        style = MaterialTheme.typography.bodyMedium,
                    )
                }
                if (state.isBusy) {
                    CircularProgressIndicator(
                        modifier = Modifier
                            .size(32.dp)
                            .testTag("biometric_progress"),
                        strokeWidth = 3.dp,
                    )
                } else {
                    Switch(
                        checked = state.enabled,
                        onCheckedChange = onBiometricToggle,
                        enabled = toggleEnabled,
                        modifier = Modifier.testTag("biometric_toggle"),
                    )
                }
            }

            state.message?.let { message ->
                Text(
                    text = message,
                    modifier = Modifier
                        .padding(top = 16.dp)
                        .testTag("biometric_status"),
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    style = MaterialTheme.typography.bodySmall,
                )
            }
        }
    }
}

internal enum class BiometricAvailability {
    Available,
    NoHardware,
    NoEnrollment,
    Unavailable,
}

internal data class BiometricSettingsState(
    val enabled: Boolean = false,
    val availability: BiometricAvailability = BiometricAvailability.Unavailable,
    val isBusy: Boolean = false,
    val message: String? = null,
)
