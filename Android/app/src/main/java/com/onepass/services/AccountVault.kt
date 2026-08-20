package com.onepass.services

import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow

sealed interface VaultState {
    data object Locked : VaultState
    data class Unlocked(val data: OnePassData) : VaultState
}

class VaultRepository {
    private val _state = MutableStateFlow<VaultState>(VaultState.Locked)
    val state = _state.asStateFlow()

    fun unlock(data: OnePassData) {
        _state.value = VaultState.Unlocked(data)
    }

    fun lock() {
        _state.value = VaultState.Locked
    }
}