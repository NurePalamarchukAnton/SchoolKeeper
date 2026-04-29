package com.schoolkeeper.app.data.session

import android.content.Context
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map

private val Context.dataStore by preferencesDataStore("session")

class SessionStore(private val context: Context) {
    companion object {
        private val TOKEN = stringPreferencesKey("token")
        private val EMAIL = stringPreferencesKey("email")
        private val ROLE = stringPreferencesKey("role")
        private val USER_ID = stringPreferencesKey("user_id")
        private val ORIGINAL_ADMIN_ID = stringPreferencesKey("original_admin_id")
        private val IMPERSONATING = stringPreferencesKey("impersonating")
    }

    val session: Flow<Session?> = context.dataStore.data.map { prefs ->
        val token = prefs[TOKEN] ?: return@map null
        Session(
            token = token,
            email = prefs[EMAIL].orEmpty(),
            role = prefs[ROLE].orEmpty(),
            userId = prefs[USER_ID]?.toIntOrNull() ?: 0,
            originalAdminId = prefs[ORIGINAL_ADMIN_ID],
            isImpersonating = prefs[IMPERSONATING] == "1"
        )
    }

    suspend fun save(session: Session) {
        context.dataStore.edit { prefs ->
            prefs[TOKEN] = session.token
            prefs[EMAIL] = session.email
            prefs[ROLE] = session.role
            prefs[USER_ID] = session.userId.toString()
            if (session.originalAdminId != null) prefs[ORIGINAL_ADMIN_ID] = session.originalAdminId
            else prefs.remove(ORIGINAL_ADMIN_ID)
            prefs[IMPERSONATING] = if (session.isImpersonating) "1" else "0"
        }
    }

    suspend fun clear() {
        context.dataStore.edit { it.clear() }
    }

    suspend fun readSession(): Session? = session.first()
}

data class Session(
    val token: String,
    val email: String,
    val role: String,
    val userId: Int,
    val originalAdminId: String? = null,
    val isImpersonating: Boolean = false
)
