package com.schoolkeeper.app.ui.components

import androidx.compose.foundation.clickable
import androidx.compose.foundation.interaction.MutableInteractionSource
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.ColumnScope
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowDropDown
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.ui.draw.rotate
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import com.schoolkeeper.app.data.model.ApiEnums
import com.schoolkeeper.app.data.model.DeviceDto
import com.schoolkeeper.app.data.model.SchoolDto
import com.schoolkeeper.app.data.model.UserDto

@Composable
private fun SimpleExposedDropdown(
    label: String,
    displayText: String,
    expanded: Boolean,
    onExpandedChange: (Boolean) -> Unit,
    modifier: Modifier = Modifier,
    menuContent: @Composable ColumnScope.() -> Unit
) {
    // Без ExposedDropdownMenuBox + menuAnchor (Material3 1.4+) меню часто не показується.
    // Box + DropdownMenu + явне відкриття по кліку на поле та на стрілку.
    Box(modifier.fillMaxWidth()) {
        OutlinedTextField(
            value = displayText,
            onValueChange = {},
            readOnly = true,
            label = { Text(label) },
            trailingIcon = {
                IconButton(onClick = { onExpandedChange(!expanded) }) {
                    Icon(
                        imageVector = Icons.Filled.ArrowDropDown,
                        contentDescription = null,
                        modifier = Modifier.rotate(if (expanded) 180f else 0f)
                    )
                }
            },
            modifier = Modifier
                .fillMaxWidth()
                .clickable(
                    interactionSource = remember { MutableInteractionSource() },
                    indication = null
                ) { onExpandedChange(!expanded) }
        )
        DropdownMenu(
            expanded = expanded,
            onDismissRequest = { onExpandedChange(false) }
        ) {
            menuContent()
        }
    }
}

@Composable
fun SchoolPicker(
    schools: List<SchoolDto>,
    selectedSchoolId: Int,
    onSchoolIdSelected: (Int) -> Unit,
    label: String,
    modifier: Modifier = Modifier
) {
    var expanded by remember(schools, selectedSchoolId) { mutableStateOf(false) }
    val selected = schools.find { it.id == selectedSchoolId }
    val display = selected?.let { "${it.name} (id ${it.id})" } ?: "Оберіть школу"
    SimpleExposedDropdown(
        label = label,
        displayText = display,
        expanded = expanded,
        onExpandedChange = { expanded = it },
        modifier = modifier
    ) {
        schools.forEach { s ->
            DropdownMenuItem(
                text = { Text("${s.name} (id ${s.id})") },
                onClick = {
                    onSchoolIdSelected(s.id)
                    expanded = false
                }
            )
        }
    }
}

@Composable
fun SchoolPickerOptional(
    schools: List<SchoolDto>,
    selectedSchoolId: Int?,
    onSchoolIdSelected: (Int?) -> Unit,
    label: String,
    modifier: Modifier = Modifier
) {
    var expanded by remember(schools, selectedSchoolId) { mutableStateOf(false) }
    val display = when (val id = selectedSchoolId) {
        null -> "Не вказано"
        else -> schools.find { it.id == id }?.let { "${it.name} (id ${it.id})" } ?: "Школа id $id"
    }
    SimpleExposedDropdown(
        label = label,
        displayText = display,
        expanded = expanded,
        onExpandedChange = { expanded = it },
        modifier = modifier
    ) {
        DropdownMenuItem(
            text = { Text("Не вказано") },
            onClick = {
                onSchoolIdSelected(null)
                expanded = false
            }
        )
        schools.forEach { s ->
            DropdownMenuItem(
                text = { Text("${s.name} (id ${s.id})") },
                onClick = {
                    onSchoolIdSelected(s.id)
                    expanded = false
                }
            )
        }
    }
}

@Composable
fun UserPicker(
    users: List<UserDto>,
    selectedUserId: Int,
    onUserIdSelected: (Int) -> Unit,
    label: String,
    modifier: Modifier = Modifier
) {
    var expanded by remember(users, selectedUserId) { mutableStateOf(false) }
    val selected = users.find { it.id == selectedUserId }
    val display = selected?.let { "${it.fullName} · ${it.email} (id ${it.id})" } ?: "Оберіть користувача"
    SimpleExposedDropdown(
        label = label,
        displayText = display,
        expanded = expanded,
        onExpandedChange = { expanded = it },
        modifier = modifier
    ) {
        users.forEach { u ->
            DropdownMenuItem(
                text = { Text("${u.fullName} · ${u.email} (id ${u.id})") },
                onClick = {
                    onUserIdSelected(u.id)
                    expanded = false
                }
            )
        }
    }
}

@Composable
fun DevicePicker(
    devices: List<DeviceDto>,
    selectedDeviceId: Int?,
    onDeviceIdSelected: (Int?) -> Unit,
    label: String,
    allowNone: Boolean = false,
    modifier: Modifier = Modifier
) {
    var expanded by remember(devices, selectedDeviceId) { mutableStateOf(false) }
    val display = when (val id = selectedDeviceId) {
        null -> if (allowNone) "Не вказано" else "Оберіть пристрій"
        else -> devices.find { it.id == id }?.let { "${it.deviceName} (id ${it.id})" } ?: "Пристрій id $id"
    }
    SimpleExposedDropdown(
        label = label,
        displayText = display,
        expanded = expanded,
        onExpandedChange = { expanded = it },
        modifier = modifier
    ) {
        if (allowNone) {
            DropdownMenuItem(
                text = { Text("Не вказано") },
                onClick = {
                    onDeviceIdSelected(null)
                    expanded = false
                }
            )
        }
        devices.forEach { d ->
            DropdownMenuItem(
                text = { Text("${d.deviceName} (id ${d.id})") },
                onClick = {
                    onDeviceIdSelected(d.id)
                    expanded = false
                }
            )
        }
    }
}

/** Варіанти для дропдаунів за ординалами API (пристрої, інциденти). */
val deviceTypePickerOptions: List<Pair<Int, String>> = listOf(
    ApiEnums.DeviceTypeMotion to ApiEnums.deviceTypeLabel(ApiEnums.DeviceTypeMotion),
    ApiEnums.DeviceTypeAlarm to ApiEnums.deviceTypeLabel(ApiEnums.DeviceTypeAlarm),
    ApiEnums.DeviceTypeAccess to ApiEnums.deviceTypeLabel(ApiEnums.DeviceTypeAccess)
)

val deviceStatusPickerOptions: List<Pair<Int, String>> = listOf(
    ApiEnums.DeviceStatusActive to ApiEnums.deviceStatusLabel(ApiEnums.DeviceStatusActive),
    ApiEnums.DeviceStatusInactive to ApiEnums.deviceStatusLabel(ApiEnums.DeviceStatusInactive),
    ApiEnums.DeviceStatusError to ApiEnums.deviceStatusLabel(ApiEnums.DeviceStatusError)
)

val incidentSeverityPickerOptions: List<Pair<Int, String>> = listOf(
    ApiEnums.IncidentSeverityLow to ApiEnums.incidentSeverityLabel(ApiEnums.IncidentSeverityLow),
    ApiEnums.IncidentSeverityMedium to ApiEnums.incidentSeverityLabel(ApiEnums.IncidentSeverityMedium),
    ApiEnums.IncidentSeverityHigh to ApiEnums.incidentSeverityLabel(ApiEnums.IncidentSeverityHigh),
    ApiEnums.IncidentSeverityCritical to ApiEnums.incidentSeverityLabel(ApiEnums.IncidentSeverityCritical)
)

val incidentStatusPickerOptions: List<Pair<Int, String>> = listOf(
    ApiEnums.IncidentStatusActive to ApiEnums.incidentStatusLabel(ApiEnums.IncidentStatusActive),
    ApiEnums.IncidentStatusResolved to ApiEnums.incidentStatusLabel(ApiEnums.IncidentStatusResolved)
)

@Composable
fun IntEnumPicker(
    options: List<Pair<Int, String>>,
    selectedValue: Int,
    onValueSelected: (Int) -> Unit,
    label: String,
    modifier: Modifier = Modifier
) {
    var expanded by remember(options, selectedValue) { mutableStateOf(false) }
    val display = options.find { it.first == selectedValue }?.second ?: "Значення $selectedValue"
    SimpleExposedDropdown(
        label = label,
        displayText = display,
        expanded = expanded,
        onExpandedChange = { expanded = it },
        modifier = modifier
    ) {
        options.forEach { (value, name) ->
            DropdownMenuItem(
                text = { Text(name) },
                onClick = {
                    onValueSelected(value)
                    expanded = false
                }
            )
        }
    }
}

private val roleLabels = listOf(
    ApiEnums.UserRoleAdmin to "Адміністратор",
    ApiEnums.UserRoleSecurity to "Безпека",
    ApiEnums.UserRoleTeacher to "Вчитель",
    ApiEnums.UserRoleParent to "Батьки",
    ApiEnums.UserRoleStudent to "Студент"
)

@Composable
fun UserRolePicker(
    selectedRole: Int,
    onRoleSelected: (Int) -> Unit,
    label: String,
    modifier: Modifier = Modifier
) {
    var expanded by remember(selectedRole) { mutableStateOf(false) }
    val display = roleLabels.find { it.first == selectedRole }?.second ?: "Роль $selectedRole"
    SimpleExposedDropdown(
        label = label,
        displayText = display,
        expanded = expanded,
        onExpandedChange = { expanded = it },
        modifier = modifier
    ) {
        roleLabels.forEach { (value, name) ->
            DropdownMenuItem(
                text = { Text(name) },
                onClick = {
                    onRoleSelected(value)
                    expanded = false
                }
            )
        }
    }
}
