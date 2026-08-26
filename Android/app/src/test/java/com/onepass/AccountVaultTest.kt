package com.onepass

import com.onepass.services.Account
import com.onepass.services.CredentialEdits
import com.onepass.services.NewAccountDetails
import com.onepass.services.OnePassData
import com.onepass.services.PasswordHistory
import com.onepass.services.VaultDocumentStore
import com.onepass.services.VaultAddResult
import com.onepass.services.VaultEncoder
import com.onepass.services.VaultRepository
import com.onepass.services.VaultState
import com.onepass.services.VaultUpdateResult
import kotlinx.coroutines.runBlocking
import org.junit.Assert.assertArrayEquals
import org.junit.Assert.assertEquals
import org.junit.Assert.assertSame
import org.junit.Test
import java.time.Instant
import java.util.UUID

class AccountVaultTest {
    private val accountGuid = UUID.fromString("00000000-0000-0000-0000-000000000001")
    private val historyGuid = UUID.fromString("00000000-0000-0000-0000-000000000002")
    private val timestamp = Instant.parse("2026-08-22T12:00:00Z")

    @Test
    fun successfulUpdatePersistsCredentialsAndNewPasswordHistoryBeforePublishingState() = runBlocking {
        val store = FakeDocumentStore("original".encodeToByteArray())
        val encoder = CapturingEncoder("encrypted".encodeToByteArray())
        val repository = repository(store, encoder)
        val original = account()
        repository.unlock(OnePassData(accounts = listOf(original)), "content://vault", "key".toCharArray())

        val result = repository.updateCredentials(
            accountGuid,
            CredentialEdits(
                username = "  exact user  ",
                emailAddress = "",
                password = "new-secret",
                websiteUrl = " site with spaces ",
                notes = "new-note",
            ),
        )

        assertSame(VaultUpdateResult.Success, result)
        assertArrayEquals("encrypted".encodeToByteArray(), store.bytes)
        val updated = (repository.state.value as VaultState.Unlocked).data.accounts.single()
        assertEquals("Name", updated.name)
        assertEquals(true, updated.favourite)
        assertEquals("  exact user  ", updated.username)
        assertEquals("", updated.emailAddress)
        assertEquals("new-secret", updated.password)
        assertEquals(" site with spaces ", updated.websiteUrl)
        assertEquals("new-note", updated.notes)
        assertEquals(timestamp.toString(), updated.dateModified)
        assertEquals(
            PasswordHistory(historyGuid, "new-secret", timestamp.toString()),
            updated.passwordHistory.single(),
        )
        assertEquals(updated, encoder.savedData?.accounts?.single())
    }

    @Test
    fun unchangedPasswordDoesNotAddHistory() = runBlocking {
        val store = FakeDocumentStore("original".encodeToByteArray())
        val repository = repository(store, CapturingEncoder("encrypted".encodeToByteArray()))
        repository.unlock(OnePassData(accounts = listOf(account())), "content://vault", charArrayOf('k'))

        repository.updateCredentials(
            accountGuid,
            CredentialEdits("user-2", "mail", "old-secret", "website", ""),
        )

        val updated = (repository.state.value as VaultState.Unlocked).data.accounts.single()
        assertEquals(emptyList<PasswordHistory>(), updated.passwordHistory)
    }

    @Test
    fun successfulAddPersistsNewAccountBeforePublishingState() = runBlocking {
        val store = FakeDocumentStore("original".encodeToByteArray())
        val encoder = CapturingEncoder("encrypted".encodeToByteArray())
        val repository = repository(store, encoder)
        repository.unlock(OnePassData(accounts = listOf(account())), "content://vault", charArrayOf('k'))

        val result = repository.addAccount(
            NewAccountDetails(
                name = "  New product  ",
                username = " exact user ",
                emailAddress = "",
                password = "new-secret",
                websiteUrl = " site ",
                notes = " notes ",
            ),
        )

        assertSame(VaultAddResult.Success, result)
        assertArrayEquals("encrypted".encodeToByteArray(), store.bytes)
        val added = (repository.state.value as VaultState.Unlocked).data.accounts.last()
        assertEquals(historyGuid, added.guid)
        assertEquals(timestamp.toString(), added.dateCreated)
        assertEquals(timestamp.toString(), added.dateModified)
        assertEquals("  New product  ", added.name)
        assertEquals(" exact user ", added.username)
        assertEquals("", added.emailAddress)
        assertEquals("new-secret", added.password)
        assertEquals(" site ", added.websiteUrl)
        assertEquals(" notes ", added.notes)
        assertEquals(false, added.favourite)
        assertEquals(emptyList<PasswordHistory>(), added.passwordHistory)
        assertEquals(added, encoder.savedData?.accounts?.last())
    }

    @Test
    fun failedAddRestoresFileAndDoesNotPublishAccount() = runBlocking {
        val originalBytes = "original".encodeToByteArray()
        val store = FakeDocumentStore(originalBytes, failuresRemaining = 1)
        val repository = repository(store, CapturingEncoder("encrypted".encodeToByteArray()))
        val original = account()
        repository.unlock(OnePassData(accounts = listOf(original)), "content://vault", charArrayOf('k'))

        val result = repository.addAccount(
            NewAccountDetails("New", "user", null, "password", null, null),
        )

        assertEquals(VaultAddResult.SaveFailed(rollbackSucceeded = true), result)
        assertArrayEquals(originalBytes, store.bytes)
        assertEquals(listOf(original), (repository.state.value as VaultState.Unlocked).data.accounts)
    }

    @Test
    fun failedWriteRestoresOriginalAndKeepsPublishedStateUnchanged() = runBlocking {
        val originalBytes = "original".encodeToByteArray()
        val store = FakeDocumentStore(originalBytes, failuresRemaining = 1)
        val repository = repository(store, CapturingEncoder("encrypted".encodeToByteArray()))
        val originalAccount = account()
        repository.unlock(OnePassData(accounts = listOf(originalAccount)), "content://vault", charArrayOf('k'))

        val result = repository.updateCredentials(
            accountGuid,
            CredentialEdits("changed", null, "old-secret", null, ""),
        )

        assertEquals(VaultUpdateResult.SaveFailed(rollbackSucceeded = true), result)
        assertArrayEquals(originalBytes, store.bytes)
        assertEquals(2, store.writeAttempts)
        assertEquals(originalAccount, (repository.state.value as VaultState.Unlocked).data.accounts.single())
    }

    @Test
    fun failedWriteReportsWhenRollbackAlsoFails() = runBlocking {
        val store = FakeDocumentStore("original".encodeToByteArray(), failuresRemaining = 2)
        val repository = repository(store, CapturingEncoder("encrypted".encodeToByteArray()))
        repository.unlock(OnePassData(accounts = listOf(account())), "content://vault", charArrayOf('k'))

        val result = repository.updateCredentials(
            accountGuid,
            CredentialEdits("changed", null, "old-secret", null, ""),
        )

        assertEquals(VaultUpdateResult.SaveFailed(rollbackSucceeded = false), result)
        assertEquals(2, store.writeAttempts)
    }

    private fun repository(store: FakeDocumentStore, encoder: CapturingEncoder) = VaultRepository(
        documentStore = store,
        encoder = encoder,
        now = { timestamp },
        newGuid = { historyGuid },
    )

    private fun account() = Account(
        guid = accountGuid,
        dateCreated = "2024-01-01T00:00:00Z",
        dateModified = "2025-01-01T00:00:00Z",
        name = "Name",
        username = "user",
        emailAddress = null,
        password = "old-secret",
        favourite = true,
        websiteUrl = null,
        mfaEnabled = true,
        notes = "notes",
        passwordHistory = emptyList(),
    )

    private class CapturingEncoder(private val output: ByteArray) : VaultEncoder {
        var savedData: OnePassData? = null

        override fun save(password: CharArray, data: OnePassData): ByteArray {
            savedData = data
            return output
        }
    }

    private class FakeDocumentStore(
        initialBytes: ByteArray,
        var failuresRemaining: Int = 0,
    ) : VaultDocumentStore {
        var bytes = initialBytes
        var writeAttempts = 0

        override fun read(documentUri: String): ByteArray = bytes.copyOf()

        override fun write(documentUri: String, bytes: ByteArray) {
            writeAttempts++
            if (failuresRemaining-- > 0) error("write failed")
            this.bytes = bytes.copyOf()
        }
    }
}
