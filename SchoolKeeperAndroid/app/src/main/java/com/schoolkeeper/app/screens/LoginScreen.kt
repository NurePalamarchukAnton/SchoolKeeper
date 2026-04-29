package com.schoolkeeper.app.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.schoolkeeper.app.SchoolKeeperApplication
import com.schoolkeeper.app.data.model.LoginRequest
import com.schoolkeeper.app.data.remote.ApiException
import com.schoolkeeper.app.data.session.Session
import com.schoolkeeper.app.ui.components.AppOutlinedTextField
import com.schoolkeeper.app.ui.components.AppPrimaryButton
import com.schoolkeeper.app.ui.components.ErrorText
import com.schoolkeeper.app.ui.theme.SkSpacing
import kotlinx.coroutines.launch

@Composable
fun LoginScreen(
    app: SchoolKeeperApplication,
    onRegistered: () -> Unit,
    onLoggedIn: () -> Unit
) {
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var error by remember { mutableStateOf<String?>(null) }
    var loading by remember { mutableStateOf(false) }
    val scope = rememberCoroutineScope()

    Surface(
        modifier = Modifier.fillMaxSize(),
        color = MaterialTheme.colorScheme.background
    ) {
        Column(
            Modifier
                .fillMaxSize()
                .padding(
                    horizontal = SkSpacing.authHorizontal,
                    vertical = SkSpacing.authVertical
                ),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Text(
                "Увійти в SchoolKeeper",
                style = MaterialTheme.typography.headlineSmall,
                color = MaterialTheme.colorScheme.onBackground
            )
            Spacer(Modifier.height(SkSpacing.screenVertical))
            AppOutlinedTextField(
                value = email,
                onValueChange = { email = it },
                label = { Text("Email") },
                modifier = Modifier.fillMaxWidth()
            )
            Spacer(Modifier.height(8.dp))
            AppOutlinedTextField(
                value = password,
                onValueChange = { password = it },
                label = { Text("Пароль") },
                modifier = Modifier.fillMaxWidth()
            )
            error?.let {
                Spacer(Modifier.height(8.dp))
                ErrorText(it)
            }
            Spacer(Modifier.height(SkSpacing.screenVertical))
            AppPrimaryButton(
                onClick = {
                    error = null
                    loading = true
                    scope.launch {
                        try {
                            val auth = app.api.login(LoginRequest(email.trim(), password))
                            app.sessionStore.save(
                                Session(
                                    token = auth.token,
                                    email = auth.email,
                                    role = auth.role,
                                    userId = auth.userId
                                )
                            )
                            onLoggedIn()
                        } catch (e: ApiException) {
                            error = e.message ?: "Помилка входу"
                        } catch (e: Exception) {
                            error = e.message ?: "Мережа недоступна"
                        } finally {
                            loading = false
                        }
                    }
                },
                enabled = !loading && email.isNotBlank() && password.isNotBlank()
            ) {
                Text(if (loading) "Зачекайте…" else "Увійти")
            }
            TextButton(onClick = onRegistered) {
                Text("Реєстрація", color = MaterialTheme.colorScheme.primary)
            }
        }
    }
}
