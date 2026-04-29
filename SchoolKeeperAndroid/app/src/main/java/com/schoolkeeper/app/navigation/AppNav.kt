package com.schoolkeeper.app.navigation

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.People
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.PhoneAndroid
import androidx.compose.material.icons.filled.Report
import androidx.compose.material.icons.filled.School
import androidx.compose.material.icons.filled.Security
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.NavigationBarItemDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.navigation.NavGraphBuilder
import androidx.navigation.NavHostController
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.schoolkeeper.app.SchoolKeeperApplication
import com.schoolkeeper.app.data.model.UserRole
import com.schoolkeeper.app.screens.AdminDataScreen
import com.schoolkeeper.app.screens.DeviceDetailScreen
import com.schoolkeeper.app.screens.DevicesScreen
import com.schoolkeeper.app.screens.HomeScreen
import com.schoolkeeper.app.screens.ImpersonateScreen
import com.schoolkeeper.app.screens.IncidentDetailScreen
import com.schoolkeeper.app.screens.IncidentsScreen
import com.schoolkeeper.app.screens.LoginScreen
import com.schoolkeeper.app.screens.MoreScreen
import com.schoolkeeper.app.screens.ParentScreen
import com.schoolkeeper.app.screens.RegisterScreen
import com.schoolkeeper.app.screens.ReportDetailScreen
import com.schoolkeeper.app.screens.ReportsScreen
import com.schoolkeeper.app.screens.SecurityScreen
import com.schoolkeeper.app.screens.SchoolsScreen
import com.schoolkeeper.app.screens.StudentScreen
import com.schoolkeeper.app.screens.TeacherScreen

private data class BarItem(
    val route: String,
    val label: String,
    val icon: androidx.compose.ui.graphics.vector.ImageVector
)

/**
 * Bottom bar aligned with web navbar ([_Layout] updateNavigation) + Home.
 * Incidents are not a global tab; Reports for all authenticated roles.
 * Non-matching non-empty role string: same as web `else` — Devices + Reports (+ Home).
 */
private fun barItems(roleFromApi: String?): List<BarItem> {
    val home = BarItem(Routes.Home, "Головна", Icons.Filled.Home)
    val rept = BarItem(Routes.Reports, "Звіти", Icons.Filled.Report)
    val dev = BarItem(Routes.Devices, "Пристрої", Icons.Filled.PhoneAndroid)
    val security = BarItem(Routes.Security, "Безпека", Icons.Filled.Security)
    val teacher = BarItem(Routes.Teacher, "Вчитель", Icons.Filled.School)
    val parent = BarItem(Routes.Parent, "Батьки", Icons.Filled.People)
    val student = BarItem(Routes.Student, "Студент", Icons.Filled.Person)
    UserRole.entries.firstOrNull { it.apiName == roleFromApi }?.let { role ->
        return when (role) {
            UserRole.Admin -> emptyList()
            UserRole.Security -> listOf(home, dev, security, rept)
            UserRole.Teacher -> listOf(home, teacher, rept)
            UserRole.Parent -> listOf(home, parent, rept)
            UserRole.Student -> listOf(home, student, rept)
        }
    }
    if (!roleFromApi.isNullOrBlank()) {
        return listOf(home, dev, rept)
    }
    // null/blank: same as UserRole.fromString → Student
    return listOf(home, student, rept)
}

private fun isBarItemSelected(itemRoute: String, current: String): Boolean {
    return when (itemRoute) {
        Routes.Home -> current == Routes.Home
        Routes.Reports -> current == Routes.Reports || current.startsWith("report/")
        Routes.Devices -> current == Routes.Devices || current.startsWith("device/")
        Routes.Security -> current == Routes.Security
        Routes.Teacher -> current == Routes.Teacher
        Routes.Parent -> current == Routes.Parent
        Routes.Student -> current == Routes.Student
        Routes.More ->
            current == Routes.More ||
                current in setOf(Routes.Admin, Routes.AdminData, Routes.Schools, Routes.Impersonate)
        else -> current == itemRoute
    }
}

@Composable
fun AppNav(app: SchoolKeeperApplication) {
    val navController = rememberNavController()
    NavHost(navController, startDestination = Routes.Splash) {
        composable(Routes.Splash) {
            LaunchedEffect(Unit) {
                val s = app.sessionStore.readSession()
                if (s != null) {
                    navController.navigate(Routes.Home) {
                        popUpTo(Routes.Splash) { inclusive = true }
                    }
                } else {
                    navController.navigate(Routes.Login) {
                        popUpTo(Routes.Splash) { inclusive = true }
                    }
                }
            }
            Surface(
                modifier = Modifier.fillMaxSize(),
                color = MaterialTheme.colorScheme.background
            ) {
                Box(contentAlignment = Alignment.Center, modifier = Modifier.fillMaxSize()) {
                    CircularProgressIndicator(color = MaterialTheme.colorScheme.primary)
                }
            }
        }
        composable(Routes.Login) {
            LoginScreen(
                app = app,
                onRegistered = { navController.navigate(Routes.Register) },
                onLoggedIn = {
                    navController.navigate(Routes.Home) {
                        popUpTo(Routes.Login) { inclusive = true }
                    }
                }
            )
        }
        composable(Routes.Register) {
            RegisterScreen(
                app = app,
                onBack = { navController.popBackStack() },
                onSuccess = { navController.navigate(Routes.Login) { popUpTo(Routes.Register) { inclusive = true } } }
            )
        }
        authenticatedGraph(app, navController)
    }
}

private fun NavGraphBuilder.authenticatedGraph(app: SchoolKeeperApplication, navController: NavHostController) {
    composable(Routes.Home) {
        Shell(app, navController) { HomeScreen(app, navController) }
    }
    composable(Routes.Schools) {
        Shell(app, navController) { SchoolsScreen(app, navController) }
    }
    composable(Routes.Devices) {
        Shell(app, navController) { DevicesScreen(app, navController) }
    }
    composable(
        Routes.DeviceDetail,
        listOf(navArgument("id") { type = NavType.IntType })
    ) {
        val id = it.arguments?.getInt("id") ?: return@composable
        Shell(app, navController) {
            DeviceDetailScreen(app, navController, id)
        }
    }
    composable(Routes.Incidents) {
        Shell(app, navController) { IncidentsScreen(app, navController) }
    }
    composable(
        Routes.IncidentDetail,
        listOf(navArgument("id") { type = NavType.IntType })
    ) {
        val id = it.arguments?.getInt("id") ?: return@composable
        Shell(app, navController) {
            IncidentDetailScreen(app, navController, id)
        }
    }
    composable(Routes.Reports) {
        Shell(app, navController) { ReportsScreen(app, navController) }
    }
    composable(
        Routes.ReportDetail,
        listOf(navArgument("id") { type = NavType.IntType })
    ) {
        val id = it.arguments?.getInt("id") ?: return@composable
        Shell(app, navController) {
            ReportDetailScreen(app, navController, id)
        }
    }
    composable(Routes.Admin) {
        Shell(app, navController) { HomeScreen(app, navController) }
    }
    composable(Routes.AdminData) {
        Shell(app, navController) { AdminDataScreen(app, navController) }
    }
    composable(Routes.Impersonate) {
        Shell(app, navController) { ImpersonateScreen(app, navController) }
    }
    composable(Routes.Security) {
        Shell(app, navController) { SecurityScreen(app, navController) }
    }
    composable(Routes.Teacher) {
        Shell(app, navController) { TeacherScreen(app, navController) }
    }
    composable(Routes.Parent) {
        Shell(app, navController) { ParentScreen(app, navController) }
    }
    composable(Routes.Student) {
        Shell(app, navController) { StudentScreen(app, navController) }
    }
    composable(Routes.More) {
        Shell(app, navController) { MoreScreen(app, navController) }
    }
}

@Composable
private fun Shell(
    app: SchoolKeeperApplication,
    navController: NavHostController,
    content: @Composable () -> Unit
) {
    val session by app.sessionStore.session.collectAsState(initial = null)
    val backStackEntry by navController.currentBackStackEntryAsState()
    val current = backStackEntry?.destination?.route ?: ""
    val items = barItems(session?.role)
    val showBar = current != Routes.Splash && current != Routes.Login && current != Routes.Register && items.isNotEmpty()
    if (!showBar) {
        content()
        return
    }
    Scaffold(
        containerColor = MaterialTheme.colorScheme.background,
        bottomBar = {
            NavigationBar(
                containerColor = MaterialTheme.colorScheme.surfaceContainer,
                contentColor = MaterialTheme.colorScheme.onSurface
            ) {
                items.forEach { item ->
                    val sel = isBarItemSelected(item.route, current)
                    NavigationBarItem(
                        selected = sel,
                        onClick = {
                            navController.navigate(item.route) {
                                launchSingleTop = true
                                restoreState = true
                                popUpTo(Routes.Home) { saveState = true }
                            }
                        },
                        icon = { Icon(item.icon, contentDescription = null) },
                        label = { Text(item.label) },
                        colors = NavigationBarItemDefaults.colors(
                            selectedIconColor = MaterialTheme.colorScheme.primary,
                            selectedTextColor = MaterialTheme.colorScheme.primary,
                            indicatorColor = MaterialTheme.colorScheme.primaryContainer,
                            unselectedIconColor = MaterialTheme.colorScheme.onSurfaceVariant,
                            unselectedTextColor = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    )
                }
            }
        }
    ) { padding ->
        Box(Modifier.padding(padding)) {
            content()
        }
    }
}
