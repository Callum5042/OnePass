package com.onepass

import android.app.Application
import com.onepass.services.VaultRepository

class OnePassApplication : Application() {
    val vaultRepository by lazy { VaultRepository() }
}