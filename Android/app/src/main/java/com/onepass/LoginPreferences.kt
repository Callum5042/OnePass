package com.onepass

import android.content.Context
import android.net.Uri
import androidx.core.content.edit

internal data class LoginSettings(
    val rememberMe: Boolean,
    val vaultUri: Uri?,
)

internal class LoginPreferences(context: Context) {
    private val preferences = context.getSharedPreferences(PREFERENCES_NAME, Context.MODE_PRIVATE)

    fun load(): LoginSettings {
        val rememberMe = preferences.getBoolean(KEY_REMEMBER_ME, false)
        return LoginSettings(
            rememberMe = rememberMe,
            vaultUri = if (rememberMe) {
                preferences.getString(KEY_VAULT_URI, null)?.let(Uri::parse)
            } else {
                null
            },
        )
    }

    fun save(rememberMe: Boolean, vaultUri: Uri?) {
        preferences.edit {
            putBoolean(KEY_REMEMBER_ME, rememberMe)

            if (rememberMe && vaultUri != null) {
                putString(KEY_VAULT_URI, vaultUri.toString())
            } else {
                remove(KEY_VAULT_URI)
            }
        }
    }

    private companion object {
        const val PREFERENCES_NAME = "login_preferences"
        const val KEY_REMEMBER_ME = "remember_me"
        const val KEY_VAULT_URI = "vault_uri"
    }
}
