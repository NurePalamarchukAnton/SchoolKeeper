package com.schoolkeeper.app.data.model

/** Mirrors C# enum ordinals for JSON. */
object ApiEnums {
    const val IncidentSeverityLow = 0
    const val IncidentSeverityMedium = 1
    const val IncidentSeverityHigh = 2
    const val IncidentSeverityCritical = 3

    const val IncidentStatusActive = 0
    const val IncidentStatusResolved = 1

    const val DeviceTypeMotion = 0
    const val DeviceTypeAlarm = 1
    const val DeviceTypeAccess = 2

    const val DeviceStatusActive = 0
    const val DeviceStatusInactive = 1
    const val DeviceStatusError = 2

    const val UserRoleAdmin = 0
    const val UserRoleSecurity = 1
    const val UserRoleTeacher = 2
    const val UserRoleParent = 3
    const val UserRoleStudent = 4

    /** Українські підписи для значень API (ординали enum як у бекенді). */
    fun userRoleLabel(role: Int): String = when (role) {
        UserRoleAdmin -> "Адміністратор"
        UserRoleSecurity -> "Безпека"
        UserRoleTeacher -> "Вчитель"
        UserRoleParent -> "Батьки"
        UserRoleStudent -> "Студент"
        else -> "Роль $role"
    }

    fun deviceTypeLabel(v: Int): String = when (v) {
        DeviceTypeMotion -> "Датчик руху"
        DeviceTypeAlarm -> "Тривога"
        DeviceTypeAccess -> "Доступ"
        else -> "Тип $v"
    }

    fun deviceStatusLabel(v: Int): String = when (v) {
        DeviceStatusActive -> "Активний"
        DeviceStatusInactive -> "Неактивний"
        DeviceStatusError -> "Помилка"
        else -> "Статус $v"
    }

    fun incidentSeverityLabel(v: Int): String = when (v) {
        IncidentSeverityLow -> "Низька"
        IncidentSeverityMedium -> "Середня"
        IncidentSeverityHigh -> "Висока"
        IncidentSeverityCritical -> "Критична"
        else -> "Важливість $v"
    }

    fun incidentStatusLabel(v: Int): String = when (v) {
        IncidentStatusActive -> "Активний"
        IncidentStatusResolved -> "Вирішено"
        else -> "Статус $v"
    }
}
