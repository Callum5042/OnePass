package com.onepass.services

import android.content.ContentResolver
import androidx.core.net.toUri

class AndroidVaultDocumentStore(
    private val contentResolver: ContentResolver,
) : VaultDocumentStore {
    override fun read(documentUri: String): ByteArray =
        contentResolver.openInputStream(documentUri.toUri())?.use { it.readBytes() }
            ?: error("Unable to read the selected vault document")

    override fun write(documentUri: String, bytes: ByteArray) {
        contentResolver.openOutputStream(documentUri.toUri(), "wt")?.use { output ->
            output.write(bytes)
            output.flush()
        } ?: error("Unable to write the selected vault document")
    }
}
