package com.schoolkeeper.app.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.schoolkeeper.app.SchoolKeeperApplication
import com.schoolkeeper.app.data.model.ApiEnums
import com.schoolkeeper.app.data.model.RegisterRequest
import com.schoolkeeper.app.data.remote.ApiException
import com.schoolkeeper.app.ui.components.AppOutlinedTextField
import com.schoolkeeper.app.ui.components.AppPrimaryButton
import com.schoolkeeper.app.ui.components.ErrorText
import com.schoolkeeper.app.ui.components.UserRolePicker
import com.schoolkeeper.app.ui.theme.SkSpacing
import kotlinx.coroutines.launch

@Composable
fun RegisterScreen(
    app: SchoolKeeperApplication,
    onBack: () -> Unit,
    onSuccess: () -> Unit
) {
    var fullName by remember { mutableStateOf("") }
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var confirm by remember { mutableStateOf("") }
    var schoolId by remember { mutableStateOf("1") }
    var roleInt by remember { mutableIntStateOf(ApiEnums.UserRoleStudent) }
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
                .verticalScroll(rememberScrollState())
                .padding(
                    horizontal = SkSpacing.authHorizontal,
                    vertical = SkSpacing.authVertical
                ),
            verticalArrangement = Arrangement.Top,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Text(
                "Реєстрація",
                style = MaterialTheme.typography.headlineSmall,
                color = MaterialTheme.colorScheme.onBackground
            )
            Spacer(Modifier.height(12.dp))
            AppOutlinedTextField(fullName, { fullName = it }, label = { Text("Повне ім'я") }, modifier = Modifier.fillMaxWidth())
            AppOutlinedTextField(email, { email = it }, label = { Text("Email") }, modifier = Modifier.fillMaxWidth())
            AppOutlinedTextField(password, { password = it }, label = { Text("Пароль") }, modifier = Modifier.fillMaxWidth())
            AppOutlinedTextField(confirm, { confirm = it }, label = { Text("Підтвердження") }, modifier = Modifier.fillMaxWidth())
            AppOutlinedTextField(schoolId, { schoolId = it.filter { ch -> ch.isDigit() } }, label = { Text("ID школи") }, modifier = Modifier.fillMaxWidth())
            Text(
                "Уточніть номер школи в адміністратора (список шкіл під час реєстрації недоступний без входу).",
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                modifier = Modifier.padding(top = 4.dp)
            )
            UserRolePicker(
                selectedRole = roleInt,
                onRoleSelected = { roleInt = it },
                label = "Роль",
                modifier = Modifier.fillMaxWidth()
            )
            error?.let { ErrorText(it, Modifier.padding(top = 8.dp)) }
            Spacer(Modifier.height(12.dp))
            AppPrimaryButton(
                onClick = {
                    if (password != confirm) {
                        error = "Паролі не збігаються"
                    } else {
                        error = null
                        loading = true
                        scope.launch {
                            try {
                                app.api.register(
                                    RegisterRequest(
                                        fullName = fullName.trim(),
                                        email = email.trim(),
                                        password = password,
                                        schoolId = schoolId.toIntOrNull() ?: 1,
                                        role = roleInt.coerceIn(0, 4)
                                    )
                                )
                                onSuccess()
                            } catch (e: ApiException) {
                                error = e.message
                            } catch (e: Exception) {
                                error = e.message
                            } finally {
                                loading = false
                            }
                        }
                    }
                },
                enabled = !loading
            ) {
                Text(if (loading) "…" else "Зареєструватися")
            }
            TextButton(onClick = onBack) {
                Text("Назад до входу", color = MaterialTheme.colorScheme.primary)
            }
        }
    }
}
