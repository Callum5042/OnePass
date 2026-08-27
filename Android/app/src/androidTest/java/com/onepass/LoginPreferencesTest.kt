package com.onepass

import android.content.Context
import android.net.Uri
import androidx.test.platform.app.InstrumentationRegistry
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test

class LoginPreferencesTest {
    private val context: Context
        get() = InstrumentationRegistry.getInstrumentation().targetContext

    private val preferences
        get() = context.getSharedPreferences("login_preferences", Context.MODE_PRIVATE)

    @Before
    fun clearPreferences() {
        preferences.edit().clear().commit()
    }

    @After
    fun restorePreferences() {
        preferences.edit().clear().commit()
    }

    @Test
    fun savesAndRestoresRememberedVault() {
        val vaultUri = Uri.parse("content://com.onepass.test/vault")

        LoginPreferences(context).save(rememberMe = true, vaultUri = vaultUri)

        val settings = LoginPreferences(context).load()

        assertTrue(settings.rememberMe)
        assertEquals(vaultUri, settings.vaultUri)
    }

    @Test
    fun disablingRememberMeClearsTheRememberedVault() {
        val vaultUri = Uri.parse("content://com.onepass.test/vault")
        val loginPreferences = LoginPreferences(context)
        loginPreferences.save(rememberMe = true, vaultUri = vaultUri)

        loginPreferences.save(rememberMe = false, vaultUri = vaultUri)

        val settings = loginPreferences.load()

        assertFalse(settings.rememberMe)
        assertNull(settings.vaultUri)
    }
}
