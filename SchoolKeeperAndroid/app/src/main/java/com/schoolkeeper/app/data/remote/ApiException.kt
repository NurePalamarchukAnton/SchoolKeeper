package com.schoolkeeper.app.data.remote

class ApiException(
    val code: Int? = null,
    override val message: String? = null,
    cause: Throwable? = null
) : Exception(message, cause)
