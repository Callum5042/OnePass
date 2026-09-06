package com.onepass

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.selection.toggleable
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.semantics.Role
import androidx.compose.ui.unit.dp

@Composable
internal fun FavouriteSwitchRow(
    favourite: Boolean,
    enabled: Boolean,
    onFavouriteChange: (Boolean) -> Unit,
) {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(top = 10.dp),
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .testTag("account_favourite")
                .toggleable(
                    value = favourite,
                    enabled = enabled,
                    role = Role.Switch,
                    onValueChange = onFavouriteChange,
                )
                .padding(vertical = 12.dp),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically,
        ) {
            Text(stringResource(R.string.favourite))
            Switch(checked = favourite, onCheckedChange = null, enabled = enabled)
        }
        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
    }
}
