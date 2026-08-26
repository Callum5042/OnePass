package com.onepass

import android.net.Uri
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Test

class LoginValidationTest {
    @Test
    fun uriAndPasswordIsValid() {
        val result = validateLoginAccount(true, "password")

        assert(result.isValid)
        assertEquals(null, result.fileError)
        assertEquals(null, result.passwordError)
    }

    @Test
    fun emptyFileIsRejected() {
        val result = validateLoginAccount(false, "vault-password")

        assertFalse(result.isValid)
        assertEquals("Choose a file", result.fileError)
        assertEquals(null, result.passwordError)
    }

    @Test
    fun emptyPasswordIsRejected() {
        val result = validateLoginAccount(true, "")

        assertFalse(result.isValid)
        assertEquals("Password is required", result.passwordError)
    }
}
