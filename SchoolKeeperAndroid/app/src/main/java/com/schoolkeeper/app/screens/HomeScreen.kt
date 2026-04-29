package com.schoolkeeper.app.screens



import androidx.compose.foundation.layout.Spacer

import androidx.compose.foundation.layout.fillMaxWidth

import androidx.compose.foundation.layout.height

import androidx.compose.material3.MaterialTheme

import androidx.compose.material3.OutlinedButton

import androidx.compose.material3.Text

import androidx.compose.runtime.Composable

import androidx.compose.runtime.collectAsState

import androidx.compose.runtime.getValue

import androidx.compose.runtime.rememberCoroutineScope

import androidx.compose.ui.Modifier

import androidx.compose.ui.unit.dp

import androidx.navigation.NavHostController

import com.schoolkeeper.app.SchoolKeeperApplication

import com.schoolkeeper.app.data.model.UserRole

import com.schoolkeeper.app.navigation.Routes

import com.schoolkeeper.app.ui.components.AppCard

import com.schoolkeeper.app.ui.components.AppPrimaryButton

import com.schoolkeeper.app.ui.components.ScreenScaffold

import kotlinx.coroutines.launch



@Composable

fun HomeScreen(app: SchoolKeeperApplication, navController: NavHostController) {

    val session by app.sessionStore.session.collectAsState(initial = null)

    val role = UserRole.fromString(session?.role)

    val scope = rememberCoroutineScope()

    ScreenScaffold {

        Text(

            "Ласкаво просимо до SchoolKeeper",

            style = MaterialTheme.typography.headlineSmall,

            color = MaterialTheme.colorScheme.onBackground

        )

        Spacer(Modifier.height(12.dp))

        session?.let {

            AppCard {

                Text("Email: ${it.email}", style = MaterialTheme.typography.bodyLarge, color = MaterialTheme.colorScheme.onSurface)

                Text(
                    "Роль: ${UserRole.fromString(it.role).displayNameUa}",
                    style = MaterialTheme.typography.bodyLarge,
                    color = MaterialTheme.colorScheme.onSurface
                )

                if (it.isImpersonating) {

                    Text(

                        "Режим входу від імені",

                        style = MaterialTheme.typography.labelLarge,

                        color = MaterialTheme.colorScheme.secondary

                    )

                }

            }

        }

        if (role == UserRole.Admin) {

            Spacer(Modifier.height(16.dp))

            AdminDashboardContent(app)

            Spacer(Modifier.height(8.dp))

            Text("Швидкі дії", style = MaterialTheme.typography.titleMedium, color = MaterialTheme.colorScheme.onBackground)

            Spacer(Modifier.height(8.dp))

            OutlinedButton(

                onClick = { navController.navigate(Routes.AdminData) },

                modifier = Modifier.fillMaxWidth(),

                shape = MaterialTheme.shapes.small

            ) { Text("Управління даними") }

            Spacer(Modifier.height(8.dp))

            OutlinedButton(

                onClick = { navController.navigate(Routes.Reports) },

                modifier = Modifier.fillMaxWidth(),

                shape = MaterialTheme.shapes.small

            ) { Text("Звіти") }

            Spacer(Modifier.height(8.dp))

            OutlinedButton(

                onClick = { navController.navigate(Routes.Impersonate) },

                modifier = Modifier.fillMaxWidth(),

                shape = MaterialTheme.shapes.small

            ) { Text("Вхід від імені") }

        }

        Spacer(Modifier.height(24.dp))

        AppPrimaryButton(

            onClick = {

                scope.launch {

                    app.sessionStore.clear()

                    navController.navigate(Routes.Login) {

                        popUpTo(Routes.Home) { inclusive = true }

                    }

                }

            }

        ) {

            Text("Вихід")

        }

    }

}


