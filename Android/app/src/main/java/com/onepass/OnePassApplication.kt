package com.onepass

import android.app.Application
import com.onepass.services.AndroidVaultDocumentStore
import com.onepass.services.VaultRepository
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob

class OnePassApplication : Application() {
    val vaultRepository by lazy {
        VaultRepository(AndroidVaultDocumentStore(contentResolver))
    }

    /** For vault work that must not be cancelled when the activity that started it finishes. */
    val applicationScope = CoroutineScope(SupervisorJob() + Dispatchers.IO)
}
