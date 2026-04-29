package com.schoolkeeper.app.screens



import androidx.compose.foundation.layout.Spacer

import androidx.compose.foundation.layout.fillMaxWidth

import androidx.compose.foundation.layout.height

import androidx.compose.material3.ButtonDefaults

import androidx.compose.material3.MaterialTheme

import androidx.compose.material3.OutlinedButton

import androidx.compose.material3.Text

import androidx.compose.runtime.Composable

import androidx.compose.runtime.collectAsState

import androidx.compose.runtime.getValue

import androidx.compose.ui.Modifier

import androidx.compose.ui.unit.dp

import androidx.navigation.NavHostController

import com.schoolkeeper.app.SchoolKeeperApplication

import com.schoolkeeper.app.data.model.UserRole

import com.schoolkeeper.app.navigation.Routes

import com.schoolkeeper.app.ui.components.ScreenScaffold



/**

 * Overflow menu for Admin-only actions (matches web: admin panel, schools, impersonation).

 * Other roles do not show a More tab; this screen is only reachable for Admin.

 */

@Composable

fun MoreScreen(app: SchoolKeeperApplication, navController: NavHostController) {

    val session by app.sessionStore.session.collectAsState(initial = null)

    val role = UserRole.fromString(session?.role)

    ScreenScaffold {

        Text("Меню", style = MaterialTheme.typography.titleLarge, color = MaterialTheme.colorScheme.onBackground)

        Spacer(Modifier.height(16.dp))

        when (role) {

            UserRole.Admin -> {

                MenuOutlineButton({ navController.navigate(Routes.Home) }, "Головна")

                MenuOutlineButton({ navController.navigate(Routes.AdminData) }, "Управління даними")

                MenuOutlineButton({ navController.navigate(Routes.Reports) }, "Звіти")

                MenuOutlineButton({ navController.navigate(Routes.Impersonate) }, "Вхід від імені")

            }

            else -> {

                Text(

                    "Немає додаткових пунктів",

                    style = MaterialTheme.typography.bodyMedium,

                    color = MaterialTheme.colorScheme.onSurfaceVariant

                )

            }

        }

    }

}



@Composable

private fun MenuOutlineButton(onClick: () -> Unit, label: String) {

    OutlinedButton(

        onClick = onClick,

        modifier = Modifier.fillMaxWidth(),

        shape = MaterialTheme.shapes.small,

        colors = ButtonDefaults.outlinedButtonColors(

            contentColor = MaterialTheme.colorScheme.primary

        )

    ) {

        Text(label)

    }

    Spacer(Modifier.height(8.dp))

}


