package com.schoolkeeper.app.screens

import android.widget.Toast
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.ScrollableTabRow
import androidx.compose.material3.Tab
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.Dialog
import androidx.navigation.NavHostController
import com.schoolkeeper.app.SchoolKeeperApplication
import com.schoolkeeper.app.data.model.ApiEnums
import com.schoolkeeper.app.data.model.DeviceDto
import com.schoolkeeper.app.data.model.IncidentDto
import com.schoolkeeper.app.data.model.ReptDto
import com.schoolkeeper.app.data.model.SchoolDto
import com.schoolkeeper.app.data.model.UserCreateRequest
import com.schoolkeeper.app.data.model.UserDto
import com.schoolkeeper.app.data.model.UserRole
import com.schoolkeeper.app.data.model.UserUpdateRequest
import com.schoolkeeper.app.ui.components.AppCard
import com.schoolkeeper.app.ui.components.DevicePicker
import com.schoolkeeper.app.ui.components.ErrorText
import com.schoolkeeper.app.ui.components.IntEnumPicker
import com.schoolkeeper.app.ui.components.deviceStatusPickerOptions
import com.schoolkeeper.app.ui.components.deviceTypePickerOptions
import com.schoolkeeper.app.ui.components.incidentSeverityPickerOptions
import com.schoolkeeper.app.ui.components.incidentStatusPickerOptions
import com.schoolkeeper.app.ui.components.SchoolPicker
import com.schoolkeeper.app.ui.components.SchoolPickerOptional
import com.schoolkeeper.app.ui.components.UserPicker
import com.schoolkeeper.app.ui.components.UserRolePicker
import kotlinx.coroutines.launch
import java.time.Instant

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AdminDataScreen(app: SchoolKeeperApplication, navController: NavHostController) {
    val session by app.sessionStore.session.collectAsState(initial = null)
    val ctx = LocalContext.current
    // collectAsState(initial = null) emits null until DataStore is read — do not treat as non-admin
    val currentSession = session
    if (currentSession == null) {
        Box(Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
            CircularProgressIndicator(color = MaterialTheme.colorScheme.primary)
        }
        return
    }
    val role = UserRole.fromString(currentSession.role)
    LaunchedEffect(role) {
        if (role != UserRole.Admin) {
            navController.popBackStack()
            Toast.makeText(ctx, "Доступ лише для адміністратора", Toast.LENGTH_SHORT).show()
        }
    }
    if (role != UserRole.Admin) return

    var tabIndex by remember { mutableIntStateOf(0) }
    val titles = listOf("Школи", "Користувачі", "Пристрої", "Інциденти", "Звіти")

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Управління даними") },
                navigationIcon = {
                    IconButton(onClick = { navController.popBackStack() }) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Назад")
                    }
                }
            )
        }
    ) { padding ->
        Column(
            Modifier
                .padding(padding)
                .fillMaxSize()
        ) {
            ScrollableTabRow(selectedTabIndex = tabIndex, edgePadding = 8.dp) {
                titles.forEachIndexed { index, title ->
                    Tab(
                        selected = tabIndex == index,
                        onClick = { tabIndex = index },
                        text = { Text(title, maxLines = 1) }
                    )
                }
            }
            when (tabIndex) {
                0 -> AdminSchoolsTab(app)
                1 -> AdminUsersTab(app)
                2 -> AdminDevicesTab(app)
                3 -> AdminIncidentsTab(app)
                4 -> AdminReportsTab(app)
            }
        }
    }
}

@Composable
private fun AdminSchoolsTab(app: SchoolKeeperApplication) {
    val token = rememberToken(app) ?: return
    val scope = rememberCoroutineScope()
    val ctx = LocalContext.current
    var list by remember { mutableStateOf<List<SchoolDto>>(emptyList()) }
    var loading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    var form by remember { mutableStateOf<SchoolDto?>(null) }
    var deleteId by remember { mutableStateOf<Int?>(null) }

    fun reload() = scope.launch {
        try {
            loading = true
            list = app.api.getSchools(token)
            error = null
        } catch (e: Exception) {
            error = e.message
        } finally {
            loading = false
        }
    }

    LaunchedEffect(Unit) { reload() }

    deleteId?.let { id ->
        AlertDialog(
            onDismissRequest = { deleteId = null },
            title = { Text("Видалити школу?") },
            confirmButton = {
                TextButton(onClick = {
                    scope.launch {
                        try {
                            app.api.deleteSchool(token, id)
                            Toast.makeText(ctx, "Видалено", Toast.LENGTH_SHORT).show()
                            deleteId = null
                            reload()
                        } catch (e: Exception) {
                            Toast.makeText(ctx, e.message ?: "Помилка", Toast.LENGTH_LONG).show()
                        }
                    }
                }) { Text("Видалити") }
            },
            dismissButton = { TextButton(onClick = { deleteId = null }) { Text("Скасувати") } }
        )
    }

    form?.let { s ->
        var name by remember(s.id) { mutableStateOf(s.name) }
        var address by remember(s.id) { mutableStateOf(s.address.orEmpty()) }
        var region by remember(s.id) { mutableStateOf(s.region.orEmpty()) }
        var contact by remember(s.id) { mutableStateOf(s.contactNumber.orEmpty()) }
        AlertDialog(
            onDismissRequest = { form = null },
            title = { Text(if (s.id == 0) "Нова школа" else "Редагування школи") },
            text = {
                Column(Modifier.verticalScroll(rememberScrollState()), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    OutlinedTextField(name, { name = it }, label = { Text("Назва") }, modifier = Modifier.fillMaxWidth())
                    OutlinedTextField(address, { address = it }, label = { Text("Адреса") }, modifier = Modifier.fillMaxWidth())
                    OutlinedTextField(region, { region = it }, label = { Text("Регіон") }, modifier = Modifier.fillMaxWidth())
                    OutlinedTextField(contact, { contact = it }, label = { Text("Контакт") }, modifier = Modifier.fillMaxWidth())
                }
            },
            confirmButton = {
                Button(onClick = {
                    scope.launch {
                        try {
                            val dto = SchoolDto(s.id, name.trim(), address.ifBlank { null }, region.ifBlank { null }, contact.ifBlank { null })
                            if (s.id == 0) app.api.createSchool(token, dto) else app.api.updateSchool(token, s.id, dto)
                            Toast.makeText(ctx, "Збережено", Toast.LENGTH_SHORT).show()
                            form = null
                            reload()
                        } catch (e: Exception) {
                            Toast.makeText(ctx, e.message ?: "Помилка", Toast.LENGTH_LONG).show()
                        }
                    }
                }) { Text("OK") }
            },
            dismissButton = { TextButton(onClick = { form = null }) { Text("Скасувати") } }
        )
    }

    Column(Modifier.padding(16.dp)) {
        Button(onClick = { form = SchoolDto(0, "", null, null, null) }, modifier = Modifier.fillMaxWidth()) {
            Text("Додати школу")
        }
        Spacer(Modifier.height(8.dp))
        error?.let { ErrorText(it) }
        if (loading) {
            CircularProgressIndicator(Modifier.align(Alignment.CenterHorizontally))
        } else {
            LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                items(list, key = { it.id }) { school ->
                    AppCard {
                        Text("${school.name} (id ${school.id})", style = MaterialTheme.typography.titleSmall)
                        Text(
                            listOfNotNull(school.address, school.region).joinToString(" · "),
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                            TextButton(onClick = { form = school }) { Text("Змінити") }
                            TextButton(onClick = { deleteId = school.id }) { Text("Видалити") }
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun AdminUsersTab(app: SchoolKeeperApplication) {
    val token = rememberToken(app) ?: return
    val scope = rememberCoroutineScope()
    val ctx = LocalContext.current
    var list by remember { mutableStateOf<List<UserDto>>(emptyList()) }
    var schools by remember { mutableStateOf<List<SchoolDto>>(emptyList()) }
    var loading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    var deleteId by remember { mutableStateOf<Int?>(null) }
    var showCreate by remember { mutableStateOf(false) }
    var editUser by remember { mutableStateOf<UserDto?>(null) }

    fun reload() = scope.launch {
        try {
            loading = true
            list = app.api.getUsers(token)
            schools = app.api.getSchools(token, pageSize = 500)
            error = null
        } catch (e: Exception) {
            error = e.message
        } finally {
            loading = false
        }
    }

    LaunchedEffect(Unit) { reload() }

    deleteId?.let { id ->
        AlertDialog(
            onDismissRequest = { deleteId = null },
            title = { Text("Видалити користувача?") },
            confirmButton = {
                TextButton(onClick = {
                    scope.launch {
                        try {
                            app.api.deleteUser(token, id)
                            Toast.makeText(ctx, "Видалено", Toast.LENGTH_SHORT).show()
                            deleteId = null
                            reload()
                        } catch (e: Exception) {
                            Toast.makeText(ctx, e.message ?: "Помилка", Toast.LENGTH_LONG).show()
                        }
                    }
                }) { Text("Видалити") }
            },
            dismissButton = { TextButton(onClick = { deleteId = null }) { Text("Скасувати") } }
        )
    }

    if (showCreate) {
        var fullName by remember { mutableStateOf("") }
        var email by remember { mutableStateOf("") }
        var password by remember { mutableStateOf("") }
        var phone by remember { mutableStateOf("") }
        var roleInt by remember { mutableIntStateOf(ApiEnums.UserRoleStudent) }
        var schoolId by remember { mutableIntStateOf(schools.firstOrNull()?.id ?: 0) }
        AlertDialog(
            onDismissRequest = { showCreate = false },
            title = { Text("Новий користувач") },
            text = {
                Column(Modifier.verticalScroll(rememberScrollState()), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    OutlinedTextField(fullName, { fullName = it }, label = { Text("ПІБ") }, modifier = Modifier.fillMaxWidth())
                    OutlinedTextField(email, { email = it }, label = { Text("Email") }, modifier = Modifier.fillMaxWidth())
                    OutlinedTextField(password, { password = it }, label = { Text("Пароль") }, modifier = Modifier.fillMaxWidth())
                    OutlinedTextField(phone, { phone = it }, label = { Text("Телефон") }, modifier = Modifier.fillMaxWidth())
                    UserRolePicker(
                        selectedRole = roleInt,
                        onRoleSelected = { roleInt = it },
                        label = "Роль",
                        modifier = Modifier.fillMaxWidth()
                    )
                    if (schools.isEmpty()) {
                        Text(
                            "Немає шкіл у списку — спочатку додайте школу.",
                            color = MaterialTheme.colorScheme.error,
                            style = MaterialTheme.typography.bodySmall
                        )
                    } else {
                        SchoolPicker(
                            schools = schools,
                            selectedSchoolId = schoolId,
                            onSchoolIdSelected = { schoolId = it },
                            label = "Школа",
                            modifier = Modifier.fillMaxWidth()
                        )
                    }
                }
            },
            confirmButton = {
                Button(onClick = {
                    scope.launch {
                        try {
                            if (schools.isEmpty()) {
                                Toast.makeText(ctx, "Додайте хоча б одну школу", Toast.LENGTH_LONG).show()
                                return@launch
                            }
                            app.api.createUser(
                                token,
                                UserCreateRequest(
                                    fullName = fullName.trim(),
                                    role = roleInt,
                                    email = email.trim(),
                                    password = password,
                                    phoneNumber = phone.ifBlank { null },
                                    schoolId = schoolId
                                )
                            )
                            Toast.makeText(ctx, "Створено", Toast.LENGTH_SHORT).show()
                            showCreate = false
                            reload()
                        } catch (e: Exception) {
                            Toast.makeText(ctx, e.message ?: "Помилка", Toast.LENGTH_LONG).show()
                        }
                    }
                }) { Text("Створити") }
            },
            dismissButton = { TextButton(onClick = { showCreate = false }) { Text("Скасувати") } }
        )
    }

    editUser?.let { u ->
        var fullName by remember(u.id) { mutableStateOf(u.fullName) }
        var email by remember(u.id) { mutableStateOf(u.email) }
        var phone by remember(u.id) { mutableStateOf(u.phoneNumber.orEmpty()) }
        var roleInt by remember(u.id) { mutableIntStateOf(u.role) }
        var schoolId by remember(u.id) { mutableIntStateOf(u.schoolId) }
        var newPassword by remember(u.id) { mutableStateOf("") }
        AlertDialog(
            onDismissRequest = { editUser = null },
            title = { Text("Редагування користувача #${u.id}") },
            text = {
                Column(Modifier.verticalScroll(rememberScrollState()), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    OutlinedTextField(fullName, { fullName = it }, label = { Text("ПІБ") }, modifier = Modifier.fillMaxWidth())
                    OutlinedTextField(email, { email = it }, label = { Text("Email") }, modifier = Modifier.fillMaxWidth())
                    OutlinedTextField(phone, { phone = it }, label = { Text("Телефон") }, modifier = Modifier.fillMaxWidth())
                    UserRolePicker(
                        selectedRole = roleInt,
                        onRoleSelected = { roleInt = it },
                        label = "Роль",
                        modifier = Modifier.fillMaxWidth()
                    )
                    if (schools.isEmpty()) {
                        Text(
                            "Немає шкіл у списку.",
                            color = MaterialTheme.colorScheme.error,
                            style = MaterialTheme.typography.bodySmall
                        )
                    } else {
                        SchoolPicker(
                            schools = schools,
                            selectedSchoolId = schoolId,
                            onSchoolIdSelected = { schoolId = it },
                            label = "Школа",
                            modifier = Modifier.fillMaxWidth()
                        )
                    }
                    OutlinedTextField(newPassword, { newPassword = it }, label = { Text("Новий пароль (необов'язково)") }, modifier = Modifier.fillMaxWidth())
                }
            },
            confirmButton = {
                Button(onClick = {
                    scope.launch {
                        try {
                            val req = UserUpdateRequest(
                                fullName = fullName.trim(),
                                role = roleInt,
                                email = email.trim(),
                                phoneNumber = phone.ifBlank { null },
                                schoolId = schoolId,
                                password = newPassword.ifBlank { null }
                            )
                            app.api.updateUser(token, u.id, req)
                            Toast.makeText(ctx, "Збережено", Toast.LENGTH_SHORT).show()
                            editUser = null
                            reload()
                        } catch (e: Exception) {
                            Toast.makeText(ctx, e.message ?: "Помилка", Toast.LENGTH_LONG).show()
                        }
                    }
                }) { Text("Зберегти") }
            },
            dismissButton = { TextButton(onClick = { editUser = null }) { Text("Скасувати") } }
        )
    }

    Column(Modifier.padding(16.dp)) {
        Button(onClick = { showCreate = true }, modifier = Modifier.fillMaxWidth()) {
            Text("Додати користувача")
        }
        Spacer(Modifier.height(8.dp))
        error?.let { ErrorText(it) }
        if (loading) {
            CircularProgressIndicator(Modifier.align(Alignment.CenterHorizontally))
        } else {
            LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                items(list, key = { it.id }) { u ->
                    AppCard {
                        Text("${u.fullName} (${u.email})", style = MaterialTheme.typography.titleSmall)
                        Text(
                            "Роль: ${ApiEnums.userRoleLabel(u.role)}, школа: ${u.schoolId}",
                            style = MaterialTheme.typography.bodySmall
                        )
                        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                            TextButton(onClick = { editUser = u }) { Text("Змінити") }
                            TextButton(onClick = { deleteId = u.id }) { Text("Видалити") }
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun AdminDevicesTab(app: SchoolKeeperApplication) {
    val token = rememberToken(app) ?: return
    val scope = rememberCoroutineScope()
    val ctx = LocalContext.current
    var list by remember { mutableStateOf<List<DeviceDto>>(emptyList()) }
    var schools by remember { mutableStateOf<List<SchoolDto>>(emptyList()) }
    var loading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    var form by remember { mutableStateOf<DeviceDto?>(null) }
    var deleteId by remember { mutableStateOf<Int?>(null) }

    fun reload() = scope.launch {
        try {
            loading = true
            list = app.api.getDevices(token)
            schools = app.api.getSchools(token, pageSize = 500)
            error = null
        } catch (e: Exception) {
            error = e.message
        } finally {
            loading = false
        }
    }

    LaunchedEffect(Unit) { reload() }

    deleteId?.let { id ->
        AlertDialog(
            onDismissRequest = { deleteId = null },
            title = { Text("Видалити пристрій?") },
            confirmButton = {
                TextButton(onClick = {
                    scope.launch {
                        try {
                            app.api.deleteDevice(token, id)
                            Toast.makeText(ctx, "Видалено", Toast.LENGTH_SHORT).show()
                            deleteId = null
                            reload()
                        } catch (e: Exception) {
                            Toast.makeText(ctx, e.message ?: "Помилка", Toast.LENGTH_LONG).show()
                        }
                    }
                }) { Text("Видалити") }
            },
            dismissButton = { TextButton(onClick = { deleteId = null }) { Text("Скасувати") } }
        )
    }

    form?.let { d ->
        var name by remember(d.id) { mutableStateOf(d.deviceName) }
        var type by remember(d.id) { mutableIntStateOf(d.deviceType) }
        var status by remember(d.id) { mutableIntStateOf(d.status) }
        var location by remember(d.id) { mutableStateOf(d.location.orEmpty()) }
        var guid by remember(d.id) { mutableStateOf(d.deviceGuid.orEmpty()) }
        var schoolIdSel by remember(d.id) { mutableStateOf<Int?>(d.schoolId) }
        AlertDialog(
            onDismissRequest = { form = null },
            title = { Text(if (d.id == 0) "Новий пристрій" else "Редагування пристрою") },
            text = {
                Column(Modifier.verticalScroll(rememberScrollState()), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    OutlinedTextField(name, { name = it }, label = { Text("Назва") }, modifier = Modifier.fillMaxWidth())
                    IntEnumPicker(
                        options = deviceTypePickerOptions,
                        selectedValue = type,
                        onValueSelected = { type = it },
                        label = "Тип пристрою",
                        modifier = Modifier.fillMaxWidth()
                    )
                    IntEnumPicker(
                        options = deviceStatusPickerOptions,
                        selectedValue = status,
                        onValueSelected = { status = it },
                        label = "Статус пристрою",
                        modifier = Modifier.fillMaxWidth()
                    )
                    OutlinedTextField(location, { location = it }, label = { Text("Локація") }, modifier = Modifier.fillMaxWidth())
                    OutlinedTextField(guid, { guid = it }, label = { Text("GUID") }, modifier = Modifier.fillMaxWidth())
                    if (schools.isEmpty()) {
                        Text("Немає шкіл — спочатку додайте школу.", color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.bodySmall)
                    } else {
                        SchoolPickerOptional(
                            schools = schools,
                            selectedSchoolId = schoolIdSel,
                            onSchoolIdSelected = { schoolIdSel = it },
                            label = "Школа",
                            modifier = Modifier.fillMaxWidth()
                        )
                    }
                }
            },
            confirmButton = {
                Button(onClick = {
                    scope.launch {
                        try {
                            val sid = schoolIdSel
                            val dto = DeviceDto(
                                id = d.id,
                                deviceName = name.trim(),
                                deviceType = type,
                                status = status,
                                location = location.ifBlank { null },
                                deviceGuid = guid.ifBlank { null },
                                schoolId = sid
                            )
                            if (d.id == 0) app.api.createDevice(token, dto) else app.api.updateDevice(token, d.id, dto)
                            Toast.makeText(ctx, "Збережено", Toast.LENGTH_SHORT).show()
                            form = null
                            reload()
                        } catch (e: Exception) {
                            Toast.makeText(ctx, e.message ?: "Помилка", Toast.LENGTH_LONG).show()
                        }
                    }
                }) { Text("OK") }
            },
            dismissButton = { TextButton(onClick = { form = null }) { Text("Скасувати") } }
        )
    }

    Column(Modifier.padding(16.dp)) {
        Button(
            onClick = {
                form = DeviceDto(0, "", ApiEnums.DeviceTypeMotion, ApiEnums.DeviceStatusActive, null, null, null)
            },
            modifier = Modifier.fillMaxWidth()
        ) { Text("Додати пристрій") }
        Spacer(Modifier.height(8.dp))
        error?.let { ErrorText(it) }
        if (loading) {
            CircularProgressIndicator(Modifier.align(Alignment.CenterHorizontally))
        } else {
            LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                items(list, key = { it.id }) { d ->
                    AppCard {
                        Text(d.deviceName, style = MaterialTheme.typography.titleSmall)
                        Text(
                            "Тип: ${ApiEnums.deviceTypeLabel(d.deviceType)}, статус: ${ApiEnums.deviceStatusLabel(d.status)}, школа ${d.schoolId}",
                            style = MaterialTheme.typography.bodySmall
                        )
                        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                            TextButton(onClick = { form = d }) { Text("Змінити") }
                            TextButton(onClick = { deleteId = d.id }) { Text("Видалити") }
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun AdminIncidentsTab(app: SchoolKeeperApplication) {
    val token = rememberToken(app) ?: return
    val scope = rememberCoroutineScope()
    val ctx = LocalContext.current
    var list by remember { mutableStateOf<List<IncidentDto>>(emptyList()) }
    var users by remember { mutableStateOf<List<UserDto>>(emptyList()) }
    var devices by remember { mutableStateOf<List<DeviceDto>>(emptyList()) }
    var schools by remember { mutableStateOf<List<SchoolDto>>(emptyList()) }
    var loading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    var form by remember { mutableStateOf<IncidentDto?>(null) }
    var deleteId by remember { mutableStateOf<Int?>(null) }

    fun reload() = scope.launch {
        try {
            loading = true
            list = app.api.getIncidents(token)
            users = app.api.getUsers(token, pageSize = 500)
            devices = app.api.getDevices(token, pageSize = 500)
            schools = app.api.getSchools(token, pageSize = 500)
            error = null
        } catch (e: Exception) {
            error = e.message
        } finally {
            loading = false
        }
    }

    LaunchedEffect(Unit) { reload() }

    deleteId?.let { id ->
        AlertDialog(
            onDismissRequest = { deleteId = null },
            title = { Text("Видалити інцидент?") },
            confirmButton = {
                TextButton(onClick = {
                    scope.launch {
                        try {
                            app.api.deleteIncident(token, id)
                            Toast.makeText(ctx, "Видалено", Toast.LENGTH_SHORT).show()
                            deleteId = null
                            reload()
                        } catch (e: Exception) {
                            Toast.makeText(ctx, e.message ?: "Помилка", Toast.LENGTH_LONG).show()
                        }
                    }
                }) { Text("Видалити") }
            },
            dismissButton = { TextButton(onClick = { deleteId = null }) { Text("Скасувати") } }
        )
    }

    form?.let { inc ->
        var deviceIdSel by remember(inc.id) { mutableStateOf<Int?>(inc.deviceId) }
        var reportedBy by remember(inc.id) { mutableIntStateOf(inc.reportedBy) }
        var type by remember(inc.id) { mutableStateOf(inc.incidentType) }
        var severity by remember(inc.id) { mutableIntStateOf(inc.severity) }
        var description by remember(inc.id) { mutableStateOf(inc.description.orEmpty()) }
        var status by remember(inc.id) { mutableIntStateOf(inc.status) }
        var schoolIdSel by remember(inc.id) { mutableStateOf<Int?>(inc.schoolId) }
        var ts by remember(inc.id) { mutableStateOf(inc.timestamp) }
        AlertDialog(
            onDismissRequest = { form = null },
            title = { Text(if (inc.id == 0) "Новий інцидент" else "Редагування інциденту") },
            text = {
                Column(Modifier.verticalScroll(rememberScrollState()), verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    if (devices.isEmpty()) {
                        Text("Немає пристроїв — додайте пристрій у вкладці «Пристрої».", color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.bodySmall)
                    } else {
                        DevicePicker(
                            devices = devices,
                            selectedDeviceId = deviceIdSel,
                            onDeviceIdSelected = { deviceIdSel = it },
                            label = "Пристрій",
                            allowNone = true,
                            modifier = Modifier.fillMaxWidth()
                        )
                    }
                    if (users.isEmpty()) {
                        Text("Немає користувачів.", color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.bodySmall)
                    } else {
                        UserPicker(
                            users = users,
                            selectedUserId = reportedBy,
                            onUserIdSelected = { reportedBy = it },
                            label = "Хто повідомив",
                            modifier = Modifier.fillMaxWidth()
                        )
                    }
                    OutlinedTextField(type, { type = it }, label = { Text("Тип (текст)") }, modifier = Modifier.fillMaxWidth())
                    IntEnumPicker(
                        options = incidentSeverityPickerOptions,
                        selectedValue = severity,
                        onValueSelected = { severity = it },
                        label = "Важливість",
                        modifier = Modifier.fillMaxWidth()
                    )
                    OutlinedTextField(description, { description = it }, label = { Text("Опис") }, modifier = Modifier.fillMaxWidth())
                    IntEnumPicker(
                        options = incidentStatusPickerOptions,
                        selectedValue = status,
                        onValueSelected = { status = it },
                        label = "Статус інциденту",
                        modifier = Modifier.fillMaxWidth()
                    )
                    if (schools.isNotEmpty()) {
                        SchoolPickerOptional(
                            schools = schools,
                            selectedSchoolId = schoolIdSel,
                            onSchoolIdSelected = { schoolIdSel = it },
                            label = "Школа",
                            modifier = Modifier.fillMaxWidth()
                        )
                    } else {
                        Text("Немає шкіл у списку.", color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.bodySmall)
                    }
                    OutlinedTextField(ts, { ts = it }, label = { Text("Час (ISO)") }, modifier = Modifier.fillMaxWidth())
                }
            },
            confirmButton = {
                Button(onClick = {
                    scope.launch {
                        try {
                            val dto = IncidentDto(
                                id = inc.id,
                                deviceId = deviceIdSel,
                                reportedBy = reportedBy,
                                incidentType = type.trim(),
                                severity = severity,
                                description = description.ifBlank { null },
                                timestamp = ts.ifBlank { Instant.now().toString() },
                                status = status,
                                schoolId = schoolIdSel,
                                users = inc.users
                            )
                            if (inc.id == 0) app.api.createIncident(token, dto) else app.api.updateIncident(token, inc.id, dto)
                            Toast.makeText(ctx, "Збережено", Toast.LENGTH_SHORT).show()
                            form = null
                            reload()
                        } catch (e: Exception) {
                            Toast.makeText(ctx, e.message ?: "Помилка", Toast.LENGTH_LONG).show()
                        }
                    }
                }) { Text("OK") }
            },
            dismissButton = { TextButton(onClick = { form = null }) { Text("Скасувати") } }
        )
    }

    Column(Modifier.padding(16.dp)) {
        Button(
            onClick = {
                val uid = users.firstOrNull()?.id ?: 1
                val devId = devices.firstOrNull()?.id
                if (devId == null) {
                    Toast.makeText(ctx, "Спочатку додайте пристрій (вкладка Пристрої)", Toast.LENGTH_LONG).show()
                    return@Button
                }
                form = IncidentDto(
                    0,
                    devId,
                    uid,
                    "Інше",
                    ApiEnums.IncidentSeverityLow,
                    null,
                    Instant.now().toString(),
                    ApiEnums.IncidentStatusActive,
                    null,
                    null
                )
            },
            modifier = Modifier.fillMaxWidth()
        ) { Text("Додати інцидент") }
        Spacer(Modifier.height(8.dp))
        error?.let { ErrorText(it) }
        if (loading) {
            CircularProgressIndicator(Modifier.align(Alignment.CenterHorizontally))
        } else {
            LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                items(list, key = { it.id }) { i ->
                    AppCard {
                        Text("#${i.id} ${i.incidentType}", style = MaterialTheme.typography.titleSmall)
                        Text(i.description ?: "", style = MaterialTheme.typography.bodySmall, maxLines = 2)
                        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                            TextButton(onClick = { form = i }) { Text("Змінити") }
                            TextButton(onClick = { deleteId = i.id }) { Text("Видалити") }
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun AdminReportsTab(app: SchoolKeeperApplication) {
    val token = rememberToken(app) ?: return
    val scope = rememberCoroutineScope()
    val ctx = LocalContext.current
    var list by remember { mutableStateOf<List<ReptDto>>(emptyList()) }
    var users by remember { mutableStateOf<List<UserDto>>(emptyList()) }
    var schools by remember { mutableStateOf<List<SchoolDto>>(emptyList()) }
    var loading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    var form by remember { mutableStateOf<ReptDto?>(null) }
    var deleteId by remember { mutableStateOf<Int?>(null) }

    fun reload() = scope.launch {
        try {
            loading = true
            list = app.api.getReports(token)
            users = app.api.getUsers(token, pageSize = 500)
            schools = app.api.getSchools(token, pageSize = 500)
            error = null
        } catch (e: Exception) {
            error = e.message
        } finally {
            loading = false
        }
    }

    LaunchedEffect(Unit) { reload() }

    deleteId?.let { id ->
        AlertDialog(
            onDismissRequest = { deleteId = null },
            title = { Text("Видалити звіт?") },
            confirmButton = {
                TextButton(onClick = {
                    scope.launch {
                        try {
                            app.api.deleteReport(token, id)
                            Toast.makeText(ctx, "Видалено", Toast.LENGTH_SHORT).show()
                            deleteId = null
                            reload()
                        } catch (e: Exception) {
                            Toast.makeText(ctx, e.message ?: "Помилка", Toast.LENGTH_LONG).show()
                        }
                    }
                }) { Text("Видалити") }
            },
            dismissButton = { TextButton(onClick = { deleteId = null }) { Text("Скасувати") } }
        )
    }

    form?.let { r ->
        var schoolId by remember(r.id) { mutableIntStateOf(r.schoolId) }
        var generatedBy by remember(r.id) { mutableIntStateOf(r.generatedBy) }
        var periodStart by remember(r.id) { mutableStateOf(r.periodStart) }
        var periodEnd by remember(r.id) { mutableStateOf(r.periodEnd) }
        var summary by remember(r.id) { mutableStateOf(r.summary.orEmpty()) }
        var generatedOn by remember(r.id) { mutableStateOf(r.generatedOn) }
        // AlertDialog + DropdownMenu: спливаюче меню часто перекриває слот confirmButton — кліки не доходять.
        Dialog(onDismissRequest = { form = null }) {
            Surface(
                shape = MaterialTheme.shapes.large,
                tonalElevation = 6.dp,
                modifier = Modifier.fillMaxWidth()
            ) {
                Column(
                    modifier = Modifier.padding(24.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    Text(
                        text = if (r.id == 0) "Новий звіт" else "Редагування звіту",
                        style = MaterialTheme.typography.titleLarge,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                    Column(
                        modifier = Modifier
                            .verticalScroll(rememberScrollState())
                            .heightIn(max = 420.dp),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        if (schools.isEmpty()) {
                            Text("Немає шкіл у списку.", color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.bodySmall)
                        } else {
                            SchoolPicker(
                                schools = schools,
                                selectedSchoolId = schoolId,
                                onSchoolIdSelected = { schoolId = it },
                                label = "Школа",
                                modifier = Modifier.fillMaxWidth()
                            )
                        }
                        if (users.isEmpty()) {
                            Text("Немає користувачів.", color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.bodySmall)
                        } else {
                            UserPicker(
                                users = users,
                                selectedUserId = generatedBy,
                                onUserIdSelected = { generatedBy = it },
                                label = "Згенеровано (користувач)",
                                modifier = Modifier.fillMaxWidth()
                            )
                        }
                        OutlinedTextField(periodStart, { periodStart = it }, label = { Text("Період з (yyyy-MM-dd)") }, modifier = Modifier.fillMaxWidth())
                        OutlinedTextField(periodEnd, { periodEnd = it }, label = { Text("Період по") }, modifier = Modifier.fillMaxWidth())
                        OutlinedTextField(summary, { summary = it }, label = { Text("Підсумок") }, modifier = Modifier.fillMaxWidth())
                        OutlinedTextField(generatedOn, { generatedOn = it }, label = { Text("Час генерації (ISO)") }, modifier = Modifier.fillMaxWidth())
                    }
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.End
                    ) {
                        TextButton(onClick = { form = null }) { Text("Скасувати") }
                        Spacer(Modifier.width(8.dp))
                        Button(
                            onClick = {
                                scope.launch {
                                    try {
                                        val dto = ReptDto(
                                            id = r.id,
                                            schoolId = schoolId,
                                            generatedBy = generatedBy,
                                            periodStart = periodStart.trim(),
                                            periodEnd = periodEnd.trim(),
                                            summary = summary.ifBlank { null },
                                            generatedOn = generatedOn.ifBlank { Instant.now().toString() }
                                        )
                                        if (r.id == 0) app.api.createReport(token, dto) else app.api.updateReport(token, r.id, dto)
                                        Toast.makeText(ctx, "Збережено", Toast.LENGTH_SHORT).show()
                                        form = null
                                        reload()
                                    } catch (e: Exception) {
                                        Toast.makeText(ctx, e.message ?: "Помилка", Toast.LENGTH_LONG).show()
                                    }
                                }
                            }
                        ) { Text("Зберегти") }
                    }
                }
            }
        }
    }

    Column(Modifier.padding(16.dp)) {
        Button(
            onClick = {
                val s = schools.firstOrNull()?.id ?: 1
                val u = users.firstOrNull()?.id ?: 1
                val today = java.time.LocalDate.now().toString()
                form = ReptDto(0, s, u, today, today, null, Instant.now().toString())
            },
            modifier = Modifier.fillMaxWidth()
        ) { Text("Додати звіт") }
        Spacer(Modifier.height(8.dp))
        error?.let { ErrorText(it) }
        if (loading) {
            CircularProgressIndicator(Modifier.align(Alignment.CenterHorizontally))
        } else {
            LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                items(list, key = { it.id }) { r ->
                    AppCard {
                        Text("Звіт #${r.id}", style = MaterialTheme.typography.titleSmall)
                        Text("${r.periodStart} — ${r.periodEnd}", style = MaterialTheme.typography.bodySmall)
                        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                            TextButton(onClick = { form = r }) { Text("Змінити") }
                            TextButton(onClick = { deleteId = r.id }) { Text("Видалити") }
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun rememberToken(app: SchoolKeeperApplication): String? {
    val session by app.sessionStore.session.collectAsState(initial = null)
    return session?.token
}
