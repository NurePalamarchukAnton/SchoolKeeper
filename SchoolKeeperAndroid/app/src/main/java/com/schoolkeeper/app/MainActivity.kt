package com.schoolkeeper.app

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import com.schoolkeeper.app.navigation.AppNav
import com.schoolkeeper.app.ui.theme.SchoolKeeperTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            SchoolKeeperTheme {
                AppNav(application as SchoolKeeperApplication)
            }
        }
    }
}
