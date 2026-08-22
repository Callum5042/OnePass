package com.onepass.services

import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import java.time.Instant
import java.util.UUID

sealed interface VaultState {
    data object Locked : VaultState
    data class Unlocked(val data: OnePassData) : VaultState
}

data class CredentialEdits(
    val username: String?,
    val emailAddress: String?,
    val password: String?,
    val websiteUrl: String?,
)

sealed interface VaultUpdateResult {
    data object Success : VaultUpdateResult
    data object VaultLocked : VaultUpdateResult
    data object AccountNotFound : VaultUpdateResult
    data class SaveFailed(val rollbackSucceeded: Boolean) : VaultUpdateResult
}

interface VaultDocumentStore {
    fun read(documentUri: String): ByteArray
    fun write(documentUri: String, bytes: ByteArray)
}

fun interface VaultEncoder {
    fun save(password: CharArray, data: OnePassData): ByteArray
}

class VaultRepository(
    private val documentStore: VaultDocumentStore,
    private val encoder: VaultEncoder = FileEncoder(),
    private val now: () -> Instant = Instant::now,
    private val newGuid: () -> UUID = UUID::randomUUID,
) {
    private val _state = MutableStateFlow<VaultState>(VaultState.Locked)
    val state = _state.asStateFlow()
    private val updateMutex = Mutex()
    private var session: VaultSession? = null

    fun unlock(data: OnePassData, documentUri: String, password: CharArray) {
        clearSession()
        session = VaultSession(documentUri, password.copyOf())
        _state.value = VaultState.Unlocked(data)
    }

    fun lock() {
        clearSession()
        _state.value = VaultState.Locked
    }

    suspend fun updateCredentials(
        accountGuid: UUID,
        edits: CredentialEdits,
    ): VaultUpdateResult = updateMutex.withLock {
        val unlocked = _state.value as? VaultState.Unlocked
            ?: return@withLock VaultUpdateResult.VaultLocked
        val activeSession = session ?: return@withLock VaultUpdateResult.VaultLocked
        val accountIndex = unlocked.data.accounts.indexOfFirst { it.guid == accountGuid }
        if (accountIndex < 0) return@withLock VaultUpdateResult.AccountNotFound

        val existing = unlocked.data.accounts[accountIndex]
        val timestamp = now().toString()
        val passwordHistory = if (edits.password != existing.password) {
            existing.passwordHistory + PasswordHistory(
                guid = newGuid(),
                password = edits.password,
                dateTime = timestamp,
            )
        } else {
            existing.passwordHistory
        }
        val updatedAccount = existing.copy(
            dateModified = timestamp,
            username = edits.username,
            emailAddress = edits.emailAddress,
            password = edits.password,
            websiteUrl = edits.websiteUrl,
            passwordHistory = passwordHistory,
        )
        val updatedData = unlocked.data.copy(
            accounts = unlocked.data.accounts.toMutableList().apply {
                this[accountIndex] = updatedAccount
            },
        )

        val encrypted = try {
            encoder.save(activeSession.password, updatedData)
        } catch (_: Exception) {
            return@withLock VaultUpdateResult.SaveFailed(rollbackSucceeded = true)
        }
        val original = try {
            documentStore.read(activeSession.documentUri)
        } catch (_: Exception) {
            return@withLock VaultUpdateResult.SaveFailed(rollbackSucceeded = true)
        }

        try {
            documentStore.write(activeSession.documentUri, encrypted)
        } catch (_: Exception) {
            val restored = try {
                documentStore.write(activeSession.documentUri, original)
                true
            } catch (_: Exception) {
                false
            }
            return@withLock VaultUpdateResult.SaveFailed(rollbackSucceeded = restored)
        }

        _state.value = VaultState.Unlocked(updatedData)
        VaultUpdateResult.Success
    }

    private fun clearSession() {
        session?.password?.fill('\u0000')
        session = null
    }

    private data class VaultSession(
        val documentUri: String,
        val password: CharArray,
    )
}
