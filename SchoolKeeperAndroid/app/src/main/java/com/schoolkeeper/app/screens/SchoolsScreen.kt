package com.schoolkeeper.app.screens

import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.runtime.collectAsState
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import com.schoolkeeper.app.SchoolKeeperApplication
import com.schoolkeeper.app.data.model.SchoolDto
import com.schoolkeeper.app.data.model.UserRole
import com.schoolkeeper.app.data.remote.ApiException
import com.schoolkeeper.app.ui.components.AppCard
import com.schoolkeeper.app.ui.components.AppOutlinedTextField
import com.schoolkeeper.app.ui.components.AppPrimaryButton
import com.schoolkeeper.app.ui.components.ErrorText
import com.schoolkeeper.app.ui.components.ScreenScaffold
import kotlinx.coroutines.launch

@Composable
fun SchoolsScreen(app: SchoolKeeperApplication, @Suppress("UNUSED_PARAMETER") navController: NavHostController) {
    var list by remember { mutableStateOf<List<SchoolDto>>(emptyList()) }
    var loading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    var newName by remember { mutableStateOf("") }
    val scope = rememberCoroutineScope()
    val session by app.sessionStore.session.collectAsState(initial = null)
    val role = UserRole.fromString(session?.role)

    LaunchedEffect(Unit) {
        try {
            val s = app.sessionStore.readSession() ?: return@LaunchedEffect
            list = app.api.getSchools(s.token)
        } catch (e: ApiException) {
            error = e.message
        } catch (e: Exception) {
            error = e.message
        } finally {
            loading = false
        }
    }

    ScreenScaffold {
        Text("Школи", style = MaterialTheme.typography.headlineSmall, color = MaterialTheme.colorScheme.onBackground)
        if (loading) {
            CircularProgressIndicator(color = MaterialTheme.colorScheme.primary)
        }
        error?.let { ErrorText(it) }
        if (role == UserRole.Admin) {
            Spacer(Modifier.height(8.dp))
            AppOutlinedTextField(newName, { newName = it }, label = { Text("Нова школа") }, modifier = Modifier.fillMaxWidth())
            Spacer(Modifier.height(8.dp))
            AppPrimaryButton(
                onClick = {
                    scope.launch {
                        try {
                            val s = app.sessionStore.readSession() ?: return@launch
                            val created = app.api.createSchool(s.token, SchoolDto(0, newName.trim(), null, null, null))
                            list = list + created
                            newName = ""
                        } catch (e: Exception) {
                            error = e.message
                        }
                    }
                },
                enabled = newName.isNotBlank()
            ) {
                Text("Додати")
            }
        }
        Spacer(Modifier.height(12.dp))
        LazyColumn {
            items(list, key = { it.id }) { school ->
                AppCard {
                    Text(
                        "${school.name} (${school.region ?: "-"})",
                        style = MaterialTheme.typography.bodyLarge,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                }
                Spacer(Modifier.height(8.dp))
            }
        }
    }
}
