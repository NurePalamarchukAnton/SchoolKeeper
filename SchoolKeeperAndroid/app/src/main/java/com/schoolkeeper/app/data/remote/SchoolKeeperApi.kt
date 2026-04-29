package com.schoolkeeper.app.data.remote

import com.google.gson.Gson
import com.google.gson.JsonObject
import com.google.gson.JsonParser
import com.google.gson.reflect.TypeToken
import com.schoolkeeper.app.data.model.AddUserToIncidentRequest
import com.schoolkeeper.app.data.model.AuthResponse
import com.schoolkeeper.app.data.model.DeviceDto
import com.schoolkeeper.app.data.model.ImpersonateRequest
import com.schoolkeeper.app.data.model.IncidentDto
import com.schoolkeeper.app.data.model.LoginRequest
import com.schoolkeeper.app.data.model.OverviewStatisticsDto
import com.schoolkeeper.app.data.model.ReptDto
import com.schoolkeeper.app.data.model.RegisterRequest
import com.schoolkeeper.app.data.model.SchoolDto
import com.schoolkeeper.app.data.model.SchoolStatisticsDto
import com.schoolkeeper.app.data.model.StopImpersonationRequest
import com.schoolkeeper.app.data.model.UserCreateRequest
import com.schoolkeeper.app.data.model.UserDto
import com.schoolkeeper.app.data.model.UserUpdateRequest
import com.schoolkeeper.app.data.model.UserStatisticsDto
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import java.lang.reflect.Type

class SchoolKeeperApi(
    private val client: OkHttpClient,
    private val gson: Gson,
    private val baseUrl: String
) {
    private val jsonMedia = "application/json; charset=utf-8".toMediaType()

    private fun url(path: String): String {
        val b = baseUrl.trimEnd('/')
        val p = path.trimStart('/')
        return "$b/$p"
    }

    private suspend fun raw(
        method: String,
        path: String,
        token: String?,
        body: String? = null
    ): String = withContext(Dispatchers.IO) {
        val builder = Request.Builder().url(url(path))
        if (token != null) builder.header("Authorization", "Bearer $token")
        when (method) {
            "GET" -> builder.get()
            "POST" -> builder.post((body ?: "{}").toRequestBody(jsonMedia))
            "PUT" -> builder.put((body ?: "{}").toRequestBody(jsonMedia))
            "DELETE" -> {
                if (body != null) builder.delete(body.toRequestBody(jsonMedia))
                else builder.delete()
            }
            else -> error("Unsupported $method")
        }
        val response = client.newCall(builder.build()).execute()
        val text = response.body?.string().orEmpty()
        if (!response.isSuccessful) {
            val msg = try {
                JsonParser.parseString(text).asJsonObject.get("message")?.asString
            } catch (_: Exception) {
                null
            } ?: response.message
            throw ApiException(response.code, msg)
        }
        text
    }

    private fun ensureSuccessEnvelope(json: String) {
        if (json.isBlank()) return
        val obj = JsonParser.parseString(json).asJsonObject
        val status = obj.get("statusCode")?.asInt ?: 200
        if (status !in 200..299) throw ApiException(status, obj.get("message")?.asString)
    }

    private fun <T> parseWrapper(json: String, dataType: Type): T {
        val obj = JsonParser.parseString(json).asJsonObject
        val status = obj.get("statusCode")?.asInt ?: 200
        if (status !in 200..299) {
            throw ApiException(status, obj.get("message")?.asString)
        }
        val dataEl = obj.get("data") ?: throw ApiException(status, "Empty data")
        return gson.fromJson(dataEl, dataType)
    }

    suspend fun login(req: LoginRequest): AuthResponse {
        val json = raw("POST", "api/Auth/login", null, gson.toJson(req))
        return parseWrapper(json, object : TypeToken<AuthResponse>() {}.type)
    }

    suspend fun register(req: RegisterRequest) {
        val json = raw("POST", "api/Auth/register", null, gson.toJson(req))
        val obj = JsonParser.parseString(json).asJsonObject
        val status = obj.get("statusCode")?.asInt ?: 200
        if (status !in 200..299) throw ApiException(status, obj.get("message")?.asString)
    }

    suspend fun impersonate(token: String, req: ImpersonateRequest): AuthResponse {
        val json = raw("POST", "api/Auth/impersonate", token, gson.toJson(req))
        return parseWrapper(json, object : TypeToken<AuthResponse>() {}.type)
    }

    suspend fun stopImpersonation(token: String, req: StopImpersonationRequest): AuthResponse {
        val json = raw("POST", "api/Auth/stop-impersonation", token, gson.toJson(req))
        return parseWrapper(json, object : TypeToken<AuthResponse>() {}.type)
    }

    suspend fun getSchools(token: String, page: Int = 1, pageSize: Int = 100): List<SchoolDto> {
        val json = raw("GET", "api/School?page=$page&pageSize=$pageSize", token)
        return parseWrapper(json, object : TypeToken<List<SchoolDto>>() {}.type)
    }

    suspend fun createSchool(token: String, dto: SchoolDto): SchoolDto {
        val json = raw("POST", "api/School", token, gson.toJson(dto))
        return parseWrapper(json, object : TypeToken<SchoolDto>() {}.type)
    }

    suspend fun updateSchool(token: String, id: Int, dto: SchoolDto): SchoolDto {
        val json = raw("PUT", "api/School/$id", token, gson.toJson(dto))
        return parseWrapper(json, object : TypeToken<SchoolDto>() {}.type)
    }

    suspend fun deleteSchool(token: String, id: Int) {
        ensureSuccessEnvelope(raw("DELETE", "api/School/$id", token))
    }

    suspend fun getDevices(token: String, page: Int = 1, pageSize: Int = 100): List<DeviceDto> {
        val json = raw("GET", "api/Device?page=$page&pageSize=$pageSize", token)
        return parseWrapper(json, object : TypeToken<List<DeviceDto>>() {}.type)
    }

    suspend fun getDevice(token: String, id: Int): DeviceDto {
        val json = raw("GET", "api/Device/$id", token)
        return parseWrapper(json, object : TypeToken<DeviceDto>() {}.type)
    }

    suspend fun createDevice(token: String, req: DeviceDto): DeviceDto {
        val json = raw("POST", "api/Device", token, gson.toJson(req))
        return parseWrapper(json, object : TypeToken<DeviceDto>() {}.type)
    }

    suspend fun updateDevice(token: String, id: Int, dto: DeviceDto): DeviceDto {
        val json = raw("PUT", "api/Device/$id", token, gson.toJson(dto))
        return parseWrapper(json, object : TypeToken<DeviceDto>() {}.type)
    }

    suspend fun deleteDevice(token: String, id: Int) {
        ensureSuccessEnvelope(raw("DELETE", "api/Device/$id", token))
    }

    suspend fun getIncidents(token: String, page: Int = 1, pageSize: Int = 100): List<IncidentDto> {
        val json = raw("GET", "api/Incident?page=$page&pageSize=$pageSize", token)
        return parseWrapper(json, object : TypeToken<List<IncidentDto>>() {}.type)
    }

    suspend fun getIncident(token: String, id: Int): IncidentDto {
        val json = raw("GET", "api/Incident/$id", token)
        return parseWrapper(json, object : TypeToken<IncidentDto>() {}.type)
    }

    suspend fun createIncident(token: String, dto: IncidentDto): IncidentDto {
        val json = raw("POST", "api/Incident", token, gson.toJson(dto))
        return parseWrapper(json, object : TypeToken<IncidentDto>() {}.type)
    }

    suspend fun updateIncident(token: String, id: Int, dto: IncidentDto): IncidentDto {
        val json = raw("PUT", "api/Incident/$id", token, gson.toJson(dto))
        return parseWrapper(json, object : TypeToken<IncidentDto>() {}.type)
    }

    suspend fun resolveIncident(token: String, id: Int): IncidentDto {
        val json = raw("POST", "api/Incident/$id/resolve", token, "{}")
        return parseWrapper(json, object : TypeToken<IncidentDto>() {}.type)
    }

    suspend fun addUserToIncident(token: String, id: Int, req: AddUserToIncidentRequest) {
        ensureSuccessEnvelope(raw("POST", "api/Incident/$id/add-user", token, gson.toJson(req)))
    }

    suspend fun getReports(token: String, page: Int = 1, pageSize: Int = 100): List<ReptDto> {
        val json = raw("GET", "api/Rept?page=$page&pageSize=$pageSize", token)
        return parseWrapper(json, object : TypeToken<List<ReptDto>>() {}.type)
    }

    suspend fun getReport(token: String, id: Int): ReptDto {
        val json = raw("GET", "api/Rept/$id", token)
        return parseWrapper(json, object : TypeToken<ReptDto>() {}.type)
    }

    suspend fun createReport(token: String, req: ReptDto): ReptDto {
        val json = raw("POST", "api/Rept", token, gson.toJson(req))
        return parseWrapper(json, object : TypeToken<ReptDto>() {}.type)
    }

    suspend fun updateReport(token: String, id: Int, dto: ReptDto): ReptDto {
        val json = raw("PUT", "api/Rept/$id", token, gson.toJson(dto))
        return parseWrapper(json, object : TypeToken<ReptDto>() {}.type)
    }

    suspend fun deleteReport(token: String, id: Int) {
        ensureSuccessEnvelope(raw("DELETE", "api/Rept/$id", token))
    }

    suspend fun exportReport(token: String, id: Int, format: String): ByteArray =
        withContext(Dispatchers.IO) {
            val request = Request.Builder()
                .url(url("api/Rept/$id/export?format=$format"))
                .header("Authorization", "Bearer $token")
                .get()
                .build()
            val response = client.newCall(request).execute()
            if (!response.isSuccessful) {
                throw ApiException(response.code, response.message)
            }
            response.body?.bytes() ?: byteArrayOf()
        }

    suspend fun getUsers(token: String, page: Int = 1, pageSize: Int = 200): List<UserDto> {
        val json = raw("GET", "api/User?page=$page&pageSize=$pageSize", token)
        return parseWrapper(json, object : TypeToken<List<UserDto>>() {}.type)
    }

    suspend fun getUser(token: String, id: Int): UserDto {
        val json = raw("GET", "api/User/$id", token)
        return parseWrapper(json, object : TypeToken<UserDto>() {}.type)
    }

    suspend fun createUser(token: String, req: UserCreateRequest): UserDto {
        val json = raw("POST", "api/User", token, gson.toJson(req))
        return parseWrapper(json, object : TypeToken<UserDto>() {}.type)
    }

    suspend fun updateUser(token: String, id: Int, req: UserUpdateRequest): UserDto {
        val json = raw("PUT", "api/User/$id", token, gson.toJson(req))
        return parseWrapper(json, object : TypeToken<UserDto>() {}.type)
    }

    suspend fun deleteUser(token: String, id: Int) {
        ensureSuccessEnvelope(raw("DELETE", "api/User/$id", token))
    }

    suspend fun deleteIncident(token: String, id: Int) {
        ensureSuccessEnvelope(raw("DELETE", "api/Incident/$id", token))
    }

    suspend fun getMyTeachers(token: String): List<UserDto> {
        val json = raw("GET", "api/User/MyTeachers", token)
        return parseWrapper(json, object : TypeToken<List<UserDto>>() {}.type)
    }

    suspend fun getStatisticsOverview(token: String): OverviewStatisticsDto {
        val json = raw("GET", "api/Statistics/overview", token)
        return parseWrapper(json, object : TypeToken<OverviewStatisticsDto>() {}.type)
    }

    suspend fun getSchoolStatistics(token: String, schoolId: Int): SchoolStatisticsDto {
        val json = raw("GET", "api/Statistics/school/$schoolId", token)
        return parseWrapper(json, object : TypeToken<SchoolStatisticsDto>() {}.type)
    }

    suspend fun getUserStatistics(token: String, userId: Int): UserStatisticsDto {
        val json = raw("GET", "api/Statistics/user/$userId", token)
        return parseWrapper(json, object : TypeToken<UserStatisticsDto>() {}.type)
    }
}
