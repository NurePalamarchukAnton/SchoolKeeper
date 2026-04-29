package com.schoolkeeper.app.data.remote

import com.google.gson.Gson
import com.google.gson.GsonBuilder
import com.schoolkeeper.app.BuildConfig
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import java.util.concurrent.TimeUnit

object NetworkModule {
    fun gson(): Gson = GsonBuilder().setLenient().create()

    fun createOkHttpClient(): OkHttpClient {
        val logging = HttpLoggingInterceptor().apply {
            level = if (BuildConfig.DEBUG) HttpLoggingInterceptor.Level.BASIC
            else HttpLoggingInterceptor.Level.NONE
        }

        return OkHttpClient.Builder()
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(60, TimeUnit.SECONDS)
            .addInterceptor(logging)
            .build()
    }

    fun createApi(client: OkHttpClient, gson: Gson): SchoolKeeperApi =
        SchoolKeeperApi(client, gson, BuildConfig.API_BASE_URL)
}
