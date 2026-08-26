package com.onepass

import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

class CreateAccountValidationTest {
    @Test
    fun validDetailsHaveNoErrors() {
        val result = validateCreateAccount(true, "12345678901", "12345678901")

        assertTrue(result.isValid)
        assertEquals(null, result.fileError)
        assertEquals(null, result.passwordError)
        assertEquals(null, result.repeatPasswordError)
    }

    @Test
    fun fileIsRequired() {
        val result = validateCreateAccount(false, "12345678901", "12345678901")

        assertEquals("Choose a file for your vault", result.fileError)
        assertEquals(false, result.isValid)
    }

    @Test
    fun passwordMustBeLongerThanTenCharacters() {
        val result = validateCreateAccount(true, "123456789", "123456789")

        assertEquals("Password must be at least 10 characters", result.passwordError)
        assertEquals(false, result.isValid)
    }

    @Test
    fun repeatPasswordMustMatch() {
        val result = validateCreateAccount(true, "12345678901", "different-password")

        assertEquals("Passwords do not match", result.repeatPasswordError)
        assertEquals(false, result.isValid)
    }
}
