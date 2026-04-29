package com.schoolkeeper.app

import android.app.Application
import com.google.gson.Gson
import com.schoolkeeper.app.data.remote.NetworkModule
import com.schoolkeeper.app.data.remote.SchoolKeeperApi
import com.schoolkeeper.app.data.session.SessionStore
import okhttp3.OkHttpClient

class SchoolKeeperApplication : Application() {
    lateinit var sessionStore: SessionStore
        private set
    lateinit var okHttp: OkHttpClient
        private set
    lateinit var gson: Gson
        private set
    lateinit var api: SchoolKeeperApi
        private set

    override fun onCreate() {
        super.onCreate()
        sessionStore = SessionStore(this)
        gson = NetworkModule.gson()
        okHttp = NetworkModule.createOkHttpClient()
        api = NetworkModule.createApi(okHttp, gson)
    }
}
