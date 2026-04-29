package com.schoolkeeper.app.screens

import android.widget.Toast
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import com.schoolkeeper.app.SchoolKeeperApplication
import com.schoolkeeper.app.data.model.ApiEnums
import com.schoolkeeper.app.data.model.DeviceDto
import com.schoolkeeper.app.data.model.ImpersonateRequest
import com.schoolkeeper.app.data.model.IncidentDto
import com.schoolkeeper.app.data.model.OverviewStatisticsDto
import com.schoolkeeper.app.data.model.ReptDto
import com.schoolkeeper.app.data.model.SchoolStatisticsDto
import com.schoolkeeper.app.data.model.StopImpersonationRequest
import com.schoolkeeper.app.data.model.UserDto
import com.schoolkeeper.app.data.model.UserRole
import com.schoolkeeper.app.data.model.UserStatisticsDto
import com.schoolkeeper.app.data.session.Session
import com.schoolkeeper.app.navigation.Routes
import com.schoolkeeper.app.ui.components.AppCard
import com.schoolkeeper.app.ui.components.AppPrimaryButton
import com.schoolkeeper.app.ui.components.ErrorText
import com.schoolkeeper.app.ui.components.ScreenScaffold
import com.schoolkeeper.app.ui.components.StatHighlightCard
import com.schoolkeeper.app.ui.components.UserPicker
import kotlinx.coroutines.launch

@Composable
fun DevicesScreen(app: SchoolKeeperApplication, navController: NavHostController) {
    var list by remember { mutableStateOf<List<DeviceDto>>(emptyList()) }
    var loading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    LaunchedEffect(Unit) {
        try {
            val s = app.sessionStore.readSession() ?: return@LaunchedEffect
            list = app.api.getDevices(s.token)
        } catch (e: Exception) {
            error = e.message
        } finally {
            loading = false
        }
    }
    ScreenScaffold {
        Text("Пристрої", style = MaterialTheme.typography.headlineSmall, color = MaterialTheme.colorScheme.onBackground)
        if (loading) CircularProgressIndicator(color = MaterialTheme.colorScheme.primary)
        error?.let { ErrorText(it) }
        LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
            items(list, key = { it.id }) { d ->
                AppCard(onClick = { navController.navigate(Routes.deviceDetail(d.id)) }) {
                    Text(
                        "${d.deviceName} — ${d.location ?: "-"}",
                        style = MaterialTheme.typography.bodyLarge,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                }
            }
        }
    }
}

@Composable
fun DeviceDetailScreen(app: SchoolKeeperApplication, navController: NavHostController, id: Int) {
    var d by remember { mutableStateOf<DeviceDto?>(null) }
    var error by remember { mutableStateOf<String?>(null) }
    LaunchedEffect(id) {
        try {
            val s = app.sessionStore.readSession() ?: return@LaunchedEffect
            d = app.api.getDevice(s.token, id)
        } catch (e: Exception) {
            error = e.message
        }
    }
    ScreenScaffold {
        d?.let {
            AppCard {
                Text("Назва: ${it.deviceName}", color = MaterialTheme.colorScheme.onSurface)
                Text("Тип: ${ApiEnums.deviceTypeLabel(it.deviceType)}", color = MaterialTheme.colorScheme.onSurface)
                Text("Статус: ${ApiEnums.deviceStatusLabel(it.status)}", color = MaterialTheme.colorScheme.onSurface)
                Text("Локація: ${it.location}", color = MaterialTheme.colorScheme.onSurface)
                Text("SchoolId: ${it.schoolId}", color = MaterialTheme.colorScheme.onSurface)
            }
        } ?: if (error == null) {
            CircularProgressIndicator(color = MaterialTheme.colorScheme.primary)
        } else {
            Unit
        }
        error?.let { ErrorText(it) }
        Spacer(Modifier.height(8.dp))
        OutlinedButton(
            onClick = { navController.popBackStack() },
            modifier = Modifier.fillMaxWidth(),
            shape = MaterialTheme.shapes.small
        ) { Text("Назад") }
    }
}

@Composable
fun IncidentsScreen(app: SchoolKeeperApplication, navController: NavHostController) {
    var list by remember { mutableStateOf<List<IncidentDto>>(emptyList()) }
    var loading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    LaunchedEffect(Unit) {
        try {
            val s = app.sessionStore.readSession() ?: return@LaunchedEffect
            list = app.api.getIncidents(s.token)
        } catch (e: Exception) {
            error = e.message
        } finally {
            loading = false
        }
    }
    ScreenScaffold {
        Text("Інциденти", style = MaterialTheme.typography.headlineSmall, color = MaterialTheme.colorScheme.onBackground)
        if (loading) CircularProgressIndicator(color = MaterialTheme.colorScheme.primary)
        error?.let { ErrorText(it) }
        LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
            items(list, key = { it.id }) { inc ->
                AppCard(onClick = { navController.navigate(Routes.incidentDetail(inc.id)) }) {
                    Text(
                        "#${inc.id} ${inc.incidentType} — ${inc.timestamp}",
                        style = MaterialTheme.typography.bodyLarge,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                }
            }
        }
    }
}

@Composable
fun IncidentDetailScreen(app: SchoolKeeperApplication, navController: NavHostController, id: Int) {
    var inc by remember { mutableStateOf<IncidentDto?>(null) }
    var error by remember { mutableStateOf<String?>(null) }
    val scope = rememberCoroutineScope()
    val session by app.sessionStore.session.collectAsState(initial = null)
    val role = UserRole.fromString(session?.role)
    LaunchedEffect(id) {
        try {
            val s = app.sessionStore.readSession() ?: return@LaunchedEffect
            inc = app.api.getIncident(s.token, id)
        } catch (e: Exception) {
            error = e.message
        }
    }
    ScreenScaffold {
        inc?.let { i ->
            AppCard {
                Text("Тип: ${i.incidentType}", color = MaterialTheme.colorScheme.onSurface)
                Text("Важливість: ${ApiEnums.incidentSeverityLabel(i.severity)}", color = MaterialTheme.colorScheme.onSurface)
                Text("Статус: ${ApiEnums.incidentStatusLabel(i.status)}", color = MaterialTheme.colorScheme.onSurface)
                Text("Час: ${i.timestamp}", color = MaterialTheme.colorScheme.onSurface)
                Text(i.description ?: "", color = MaterialTheme.colorScheme.onSurface)
                i.users?.forEach { u ->
                    Text(
                        "— ${u.fullName} (${UserRole.fromString(u.role).displayNameUa})",
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }
            if (role == UserRole.Admin || role == UserRole.Teacher) {
                Spacer(Modifier.height(8.dp))
                AppPrimaryButton(
                    onClick = {
                        scope.launch {
                            try {
                                val s = app.sessionStore.readSession() ?: return@launch
                                inc = app.api.resolveIncident(s.token, id)
                            } catch (e: Exception) {
                                error = e.message
                            }
                        }
                    }
                ) { Text("Вирішити") }
            }
        } ?: if (error == null) {
            CircularProgressIndicator(color = MaterialTheme.colorScheme.primary)
        } else {
            Unit
        }
        error?.let { ErrorText(it) }
        Spacer(Modifier.height(8.dp))
        OutlinedButton(
            onClick = { navController.popBackStack() },
            modifier = Modifier.fillMaxWidth(),
            shape = MaterialTheme.shapes.small
        ) { Text("Назад") }
    }
}

@Composable
fun ReportsScreen(app: SchoolKeeperApplication, navController: NavHostController) {
    var list by remember { mutableStateOf<List<ReptDto>>(emptyList()) }
    var loading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    LaunchedEffect(Unit) {
        try {
            val s = app.sessionStore.readSession() ?: return@LaunchedEffect
            list = app.api.getReports(s.token)
        } catch (e: Exception) {
            error = e.message
        } finally {
            loading = false
        }
    }
    ScreenScaffold {
        Text("Звіти", style = MaterialTheme.typography.headlineSmall, color = MaterialTheme.colorScheme.onBackground)
        if (loading) CircularProgressIndicator(color = MaterialTheme.colorScheme.primary)
        error?.let { ErrorText(it) }
        LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
            items(list, key = { it.id }) { r ->
                AppCard(onClick = { navController.navigate(Routes.reportDetail(r.id)) }) {
                    Text(
                        "Звіт #${r.id} ${r.periodStart} — ${r.periodEnd}",
                        style = MaterialTheme.typography.bodyLarge,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                }
            }
        }
    }
}

@Composable
fun ReportDetailScreen(app: SchoolKeeperApplication, navController: NavHostController, id: Int) {
    var r by remember { mutableStateOf<ReptDto?>(null) }
    var error by remember { mutableStateOf<String?>(null) }
    val ctx = LocalContext.current
    val scope = rememberCoroutineScope()
    LaunchedEffect(id) {
        try {
            val s = app.sessionStore.readSession() ?: return@LaunchedEffect
            r = app.api.getReport(s.token, id)
        } catch (e: Exception) {
            error = e.message
        }
    }
    ScreenScaffold {
        r?.let { rep ->
            AppCard {
                Text(rep.summary ?: "", style = MaterialTheme.typography.bodyLarge)
                Text(
                    "Період: ${rep.periodStart} — ${rep.periodEnd}",
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
            Spacer(Modifier.height(8.dp))
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                listOf("json", "csv", "txt").forEach { fmt ->
                    Button(
                        onClick = {
                            scope.launch {
                                try {
                                    val s = app.sessionStore.readSession() ?: return@launch
                                    val bytes = app.api.exportReport(s.token, id, fmt)
                                    ctx.openFileOutput("report_${id}.$fmt", 0).use { out -> out.write(bytes) }
                                    Toast.makeText(ctx, "Збережено report_${id}.$fmt", Toast.LENGTH_SHORT).show()
                                } catch (e: Exception) {
                                    Toast.makeText(ctx, e.message, Toast.LENGTH_LONG).show()
                                }
                            }
                        },
                        shape = MaterialTheme.shapes.small
                    ) { Text(fmt.uppercase()) }
                }
            }
        } ?: if (error == null) {
            CircularProgressIndicator(color = MaterialTheme.colorScheme.primary)
        } else {
            Unit
        }
        error?.let { ErrorText(it) }
        Spacer(Modifier.height(8.dp))
        OutlinedButton(
            onClick = { navController.popBackStack() },
            modifier = Modifier.fillMaxWidth(),
            shape = MaterialTheme.shapes.small
        ) { Text("Назад") }
    }
}

/** Огляд статистики для головної адміністратора (раніше окремий екран «Адмін-панель»). */
@Composable
fun AdminDashboardContent(app: SchoolKeeperApplication) {
    var stats by remember { mutableStateOf<OverviewStatisticsDto?>(null) }
    var error by remember { mutableStateOf<String?>(null) }
    LaunchedEffect(Unit) {
        try {
            val s = app.sessionStore.readSession() ?: return@LaunchedEffect
            stats = app.api.getStatisticsOverview(s.token)
        } catch (e: Exception) {
            error = e.message
        }
    }
    Text("Адмін-панель", style = MaterialTheme.typography.headlineSmall, color = MaterialTheme.colorScheme.onBackground)
    Spacer(Modifier.height(12.dp))
    stats?.let { s ->
        StatHighlightCard("Школи", "${s.totalSchools}")
        Spacer(Modifier.height(8.dp))
        StatHighlightCard(
            "Користувачі",
            "${s.totalUsers}",
            containerColor = MaterialTheme.colorScheme.secondary,
            contentColor = MaterialTheme.colorScheme.onSecondary
        )
        Spacer(Modifier.height(8.dp))
        StatHighlightCard("Пристрої", "${s.totalDevices}", containerColor = Color(0xFF084298))
        Spacer(Modifier.height(8.dp))
        StatHighlightCard(
            "Інциденти",
            "${s.totalIncidents} (акт. ${s.activeIncidents}, виріш. ${s.resolvedIncidents})",
            containerColor = MaterialTheme.colorScheme.tertiary,
            contentColor = MaterialTheme.colorScheme.onTertiary
        )
        Spacer(Modifier.height(8.dp))
        StatHighlightCard("Звіти", "${s.totalReports}", containerColor = Color(0xFF055160))
    } ?: if (error == null) {
        CircularProgressIndicator(color = MaterialTheme.colorScheme.primary)
    } else {
        Unit
    }
    error?.let { ErrorText(it) }
}

@Composable
fun ImpersonateScreen(app: SchoolKeeperApplication, navController: NavHostController) {
    var users by remember { mutableStateOf<List<UserDto>>(emptyList()) }
    var usersLoading by remember { mutableStateOf(true) }
    var selectedUserId by remember { mutableStateOf(0) }
    var error by remember { mutableStateOf<String?>(null) }
    val scope = rememberCoroutineScope()

    LaunchedEffect(Unit) {
        usersLoading = true
        try {
            val s = app.sessionStore.readSession() ?: return@LaunchedEffect
            users = app.api.getUsers(s.token, pageSize = 500).filter { it.id != s.userId }
        } catch (e: Exception) {
            error = e.message
        } finally {
            usersLoading = false
        }
    }

    ScreenScaffold {
        Text(
            "Вхід від імені (лише Admin)",
            style = MaterialTheme.typography.titleLarge,
            color = MaterialTheme.colorScheme.onBackground
        )
        Spacer(Modifier.height(8.dp))
        if (usersLoading) {
            CircularProgressIndicator(color = MaterialTheme.colorScheme.primary)
        } else {
            UserPicker(
                users = users,
                selectedUserId = selectedUserId,
                onUserIdSelected = { selectedUserId = it },
                label = "Користувач",
                modifier = Modifier.fillMaxWidth()
            )
        }
        error?.let { ErrorText(it) }
        Spacer(Modifier.height(8.dp))
        AppPrimaryButton(
            onClick = {
                scope.launch {
                    try {
                        val s = app.sessionStore.readSession() ?: return@launch
                        val auth = app.api.impersonate(s.token, ImpersonateRequest(selectedUserId))
                        app.sessionStore.save(
                            Session(
                                token = auth.token,
                                email = auth.email,
                                role = auth.role,
                                userId = auth.userId,
                                originalAdminId = auth.originalAdminId,
                                isImpersonating = true
                            )
                        )
                        navController.popBackStack(Routes.Home, inclusive = false)
                    } catch (e: Exception) {
                        error = e.message
                    }
                }
            },
            enabled = !usersLoading && selectedUserId > 0
        ) { Text("Увійти") }
        Spacer(Modifier.height(8.dp))
        OutlinedButton(
            onClick = {
                scope.launch {
                    try {
                        val s = app.sessionStore.readSession() ?: return@launch
                        val auth = app.api.stopImpersonation(s.token, StopImpersonationRequest(s.originalAdminId))
                        app.sessionStore.save(
                            Session(
                                token = auth.token,
                                email = auth.email,
                                role = auth.role,
                                userId = auth.userId,
                                isImpersonating = false,
                                originalAdminId = null
                            )
                        )
                        navController.popBackStack(Routes.Home, inclusive = false)
                    } catch (e: Exception) {
                        error = e.message
                    }
                }
            },
            modifier = Modifier.fillMaxWidth(),
            shape = MaterialTheme.shapes.small
        ) { Text("Вийти з режиму від імені") }
    }
}

@Composable
fun SecurityScreen(app: SchoolKeeperApplication, navController: NavHostController) {
    var stats by remember { mutableStateOf<SchoolStatisticsDto?>(null) }
    var error by remember { mutableStateOf<String?>(null) }
    LaunchedEffect(Unit) {
        try {
            val s = app.sessionStore.readSession() ?: return@LaunchedEffect
            val me = app.api.getUser(s.token, s.userId)
            stats = app.api.getSchoolStatistics(s.token, me.schoolId)
        } catch (e: Exception) {
            error = e.message
        }
    }
    ScreenScaffold {
        Text("Безпека школи", style = MaterialTheme.typography.headlineSmall, color = MaterialTheme.colorScheme.onBackground)
        stats?.let { s ->
            StatHighlightCard("Користувачі", "${s.totalUsers}")
            Spacer(Modifier.height(8.dp))
            StatHighlightCard(
                "Пристрої",
                "${s.totalDevices}",
                containerColor = MaterialTheme.colorScheme.secondary,
                contentColor = MaterialTheme.colorScheme.onSecondary
            )
            Spacer(Modifier.height(8.dp))
            StatHighlightCard(
                "Інциденти",
                "${s.totalIncidents}",
                containerColor = MaterialTheme.colorScheme.tertiary,
                contentColor = MaterialTheme.colorScheme.onTertiary
            )
        } ?: if (error == null) {
            CircularProgressIndicator(color = MaterialTheme.colorScheme.primary)
        } else {
            Unit
        }
        error?.let { ErrorText(it) }
        Spacer(Modifier.height(12.dp))
        OutlinedButton(
            onClick = { navController.navigate(Routes.Incidents) },
            modifier = Modifier.fillMaxWidth(),
            shape = MaterialTheme.shapes.small
        ) { Text("Управління інцидентами") }
    }
}

@Composable
fun TeacherScreen(app: SchoolKeeperApplication, navController: NavHostController) {
    var users by remember { mutableStateOf<List<UserDto>>(emptyList()) }
    var incidents by remember { mutableStateOf<List<IncidentDto>>(emptyList()) }
    var error by remember { mutableStateOf<String?>(null) }
    LaunchedEffect(Unit) {
        try {
            val s = app.sessionStore.readSession() ?: return@LaunchedEffect
            users = app.api.getUsers(s.token).filter { it.role == ApiEnums.UserRoleStudent || it.role == ApiEnums.UserRoleParent }
            incidents = app.api.getIncidents(s.token)
        } catch (e: Exception) {
            error = e.message
        }
    }
    ScreenScaffold {
        Text("Панель вчителя", style = MaterialTheme.typography.headlineSmall, color = MaterialTheme.colorScheme.onBackground)
        error?.let { ErrorText(it) }
        OutlinedButton(
            onClick = { navController.navigate(Routes.Incidents) },
            modifier = Modifier.fillMaxWidth(),
            shape = MaterialTheme.shapes.small
        ) { Text("Переглянути всі інциденти") }
        Spacer(Modifier.height(12.dp))
        Text("Користувачі (фільтр)", style = MaterialTheme.typography.titleMedium, color = MaterialTheme.colorScheme.onBackground)
        users.take(30).forEach { u ->
            AppCard {
                Text("${u.fullName} — ${u.email}", color = MaterialTheme.colorScheme.onSurface)
            }
            Spacer(Modifier.height(8.dp))
        }
        Spacer(Modifier.height(12.dp))
        Text("Інциденти", style = MaterialTheme.typography.titleMedium, color = MaterialTheme.colorScheme.onBackground)
        incidents.take(20).forEach { i ->
            AppCard {
                Text("#${i.id} ${i.incidentType}", color = MaterialTheme.colorScheme.onSurface)
            }
            Spacer(Modifier.height(8.dp))
        }
    }
}

@Composable
fun ParentScreen(app: SchoolKeeperApplication, navController: NavHostController) {
    var stats by remember { mutableStateOf<UserStatisticsDto?>(null) }
    var error by remember { mutableStateOf<String?>(null) }
    LaunchedEffect(Unit) {
        try {
            val s = app.sessionStore.readSession() ?: return@LaunchedEffect
            stats = app.api.getUserStatistics(s.token, s.userId)
        } catch (e: Exception) {
            error = e.message
        }
    }
    ScreenScaffold {
        Text("Батьки", style = MaterialTheme.typography.headlineSmall, color = MaterialTheme.colorScheme.onBackground)
        stats?.let {
            StatHighlightCard(
                "Інциденти (за звітами)",
                "${it.totalIncidents}",
                containerColor = Color(0xFF0DCAF0),
                contentColor = Color(0xFF055160)
            )
        } ?: if (error == null) {
            CircularProgressIndicator(color = MaterialTheme.colorScheme.primary)
        } else {
            Unit
        }
        error?.let { ErrorText(it) }
        Spacer(Modifier.height(12.dp))
        OutlinedButton(
            onClick = { navController.navigate(Routes.Incidents) },
            modifier = Modifier.fillMaxWidth(),
            shape = MaterialTheme.shapes.small
        ) { Text("Перегляд інцидентів") }
    }
}

@Composable
fun StudentScreen(app: SchoolKeeperApplication, navController: NavHostController) {
    var teachers by remember { mutableStateOf<List<UserDto>>(emptyList()) }
    var incidents by remember { mutableStateOf<List<IncidentDto>>(emptyList()) }
    var error by remember { mutableStateOf<String?>(null) }
    LaunchedEffect(Unit) {
        try {
            val s = app.sessionStore.readSession() ?: return@LaunchedEffect
            teachers = app.api.getMyTeachers(s.token)
            incidents = app.api.getIncidents(s.token)
        } catch (e: Exception) {
            error = e.message
        }
    }
    ScreenScaffold {
        Text("Студент", style = MaterialTheme.typography.headlineSmall, color = MaterialTheme.colorScheme.onBackground)
        error?.let { ErrorText(it) }
        Text("Вчителі", style = MaterialTheme.typography.titleMedium, color = MaterialTheme.colorScheme.onBackground)
        teachers.forEach { t ->
            AppCard {
                Text(t.fullName, color = MaterialTheme.colorScheme.onSurface)
            }
            Spacer(Modifier.height(8.dp))
        }
        Spacer(Modifier.height(12.dp))
        OutlinedButton(
            onClick = { navController.navigate(Routes.Incidents) },
            modifier = Modifier.fillMaxWidth(),
            shape = MaterialTheme.shapes.small
        ) { Text("Переглянути всі інциденти") }
        Spacer(Modifier.height(12.dp))
        Text("Мої інциденти (список школи/фільтр API)", style = MaterialTheme.typography.titleMedium, color = MaterialTheme.colorScheme.onBackground)
        incidents.take(15).forEach { i ->
            AppCard {
                Text("#${i.id} ${i.incidentType}", color = MaterialTheme.colorScheme.onSurface)
            }
            Spacer(Modifier.height(8.dp))
        }
    }
}
