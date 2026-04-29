package com.schoolkeeper.app.data.model

import com.google.gson.annotations.SerializedName

data class AuthResponse(
    @SerializedName("token") val token: String,
    @SerializedName("email") val email: String,
    @SerializedName("role") val role: String,
    @SerializedName("userId") val userId: Int,
    @SerializedName("originalAdminId") val originalAdminId: String? = null
)

data class LoginRequest(
    @SerializedName("email") val email: String,
    @SerializedName("password") val password: String
)

data class RegisterRequest(
    @SerializedName("fullName") val fullName: String,
    @SerializedName("email") val email: String,
    @SerializedName("password") val password: String,
    @SerializedName("schoolId") val schoolId: Int,
    @SerializedName("role") val role: Int
)

data class ImpersonateRequest(
    @SerializedName("userId") val userId: Int
)

data class StopImpersonationRequest(
    @SerializedName("originalAdminId") val originalAdminId: String?
)

data class SchoolDto(
    @SerializedName("id") val id: Int,
    @SerializedName("name") val name: String,
    @SerializedName("address") val address: String? = null,
    @SerializedName("region") val region: String? = null,
    @SerializedName("contactNumber") val contactNumber: String? = null
)

data class DeviceDto(
    @SerializedName("id") val id: Int,
    @SerializedName("deviceName") val deviceName: String,
    @SerializedName("deviceType") val deviceType: Int,
    @SerializedName("status") val status: Int,
    @SerializedName("location") val location: String? = null,
    @SerializedName("deviceGuid") val deviceGuid: String? = null,
    @SerializedName("schoolId") val schoolId: Int? = null
)

data class DeviceCreateRequest(
    @SerializedName("deviceName") val deviceName: String,
    @SerializedName("deviceType") val deviceType: Int,
    @SerializedName("status") val status: Int,
    @SerializedName("location") val location: String? = null,
    @SerializedName("deviceGuid") val deviceGuid: String? = null,
    @SerializedName("schoolId") val schoolId: Int? = null
)

data class IncidentDto(
    @SerializedName("id") val id: Int,
    @SerializedName("deviceId") val deviceId: Int? = null,
    @SerializedName("reportedBy") val reportedBy: Int,
    @SerializedName("incidentType") val incidentType: String,
    @SerializedName("severity") val severity: Int,
    @SerializedName("description") val description: String? = null,
    @SerializedName("timestamp") val timestamp: String,
    @SerializedName("status") val status: Int,
    @SerializedName("schoolId") val schoolId: Int? = null,
    @SerializedName("users") val users: List<IncidentUserDto>? = null
)

data class IncidentUserDto(
    @SerializedName("userId") val userId: Int,
    @SerializedName("fullName") val fullName: String,
    @SerializedName("email") val email: String,
    @SerializedName("role") val role: String
)

data class AddUserToIncidentRequest(
    @SerializedName("userId") val userId: Int
)

data class ReptDto(
    @SerializedName("id") val id: Int,
    @SerializedName("schoolId") val schoolId: Int,
    @SerializedName("generatedBy") val generatedBy: Int,
    @SerializedName("periodStart") val periodStart: String,
    @SerializedName("periodEnd") val periodEnd: String,
    @SerializedName("summary") val summary: String? = null,
    @SerializedName("generatedOn") val generatedOn: String
)

data class UserDto(
    @SerializedName("id") val id: Int,
    @SerializedName("fullName") val fullName: String,
    @SerializedName("role") val role: Int,
    @SerializedName("email") val email: String,
    @SerializedName("phoneNumber") val phoneNumber: String? = null,
    @SerializedName("schoolId") val schoolId: Int
)

data class UserCreateRequest(
    @SerializedName("fullName") val fullName: String,
    @SerializedName("role") val role: Int,
    @SerializedName("email") val email: String,
    @SerializedName("password") val password: String,
    @SerializedName("phoneNumber") val phoneNumber: String? = null,
    @SerializedName("schoolId") val schoolId: Int
)

/** Matches [SchoolKeeper.DTO.UserUpdateDto] — partial update. */
data class UserUpdateRequest(
    @SerializedName("fullName") val fullName: String? = null,
    @SerializedName("role") val role: Int? = null,
    @SerializedName("email") val email: String? = null,
    @SerializedName("password") val password: String? = null,
    @SerializedName("phoneNumber") val phoneNumber: String? = null,
    @SerializedName("schoolId") val schoolId: Int? = null
)

data class OverviewStatisticsDto(
    @SerializedName("totalSchools") val totalSchools: Int,
    @SerializedName("totalUsers") val totalUsers: Int,
    @SerializedName("totalDevices") val totalDevices: Int,
    @SerializedName("totalIncidents") val totalIncidents: Int,
    @SerializedName("totalReports") val totalReports: Int,
    @SerializedName("activeIncidents") val activeIncidents: Int,
    @SerializedName("resolvedIncidents") val resolvedIncidents: Int
)

data class SchoolStatisticsDto(
    @SerializedName("schoolId") val schoolId: Int,
    @SerializedName("schoolName") val schoolName: String? = null,
    @SerializedName("totalUsers") val totalUsers: Int,
    @SerializedName("totalDevices") val totalDevices: Int,
    @SerializedName("totalIncidents") val totalIncidents: Int,
    @SerializedName("activeIncidents") val activeIncidents: Int,
    @SerializedName("resolvedIncidents") val resolvedIncidents: Int,
    @SerializedName("totalReports") val totalReports: Int
)

data class UserStatisticsDto(
    @SerializedName("userId") val userId: Int,
    @SerializedName("userName") val userName: String? = null,
    @SerializedName("totalIncidents") val totalIncidents: Int,
    @SerializedName("activeIncidents") val activeIncidents: Int,
    @SerializedName("resolvedIncidents") val resolvedIncidents: Int,
    @SerializedName("totalReports") val totalReports: Int
)

enum class UserRole(val apiName: String) {
    Admin("Admin"),
    Security("Security"),
    Teacher("Teacher"),
    Parent("Parent"),
    Student("Student");

    val displayNameUa: String
        get() = when (this) {
            Admin -> "Адміністратор"
            Security -> "Безпека"
            Teacher -> "Вчитель"
            Parent -> "Батьки"
            Student -> "Студент"
        }

    companion object {
        fun fromString(s: String?) =
            entries.firstOrNull { it.apiName.equals(s?.trim(), ignoreCase = true) } ?: Student
        fun fromInt(i: Int) = entries.getOrNull(i) ?: Student
    }
}
