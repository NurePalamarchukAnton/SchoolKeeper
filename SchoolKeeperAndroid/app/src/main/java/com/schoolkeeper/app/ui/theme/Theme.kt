package com.schoolkeeper.app.ui.theme

import android.app.Activity
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.SideEffect
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.toArgb
import androidx.compose.ui.platform.LocalView
import androidx.core.view.WindowCompat

private val LightColorScheme = lightColorScheme(
    primary = SkColors.Primary,
    onPrimary = SkColors.OnPrimary,
    primaryContainer = SkColors.PrimaryContainer,
    onPrimaryContainer = SkColors.OnSurface,
    secondary = SkColors.FocusAccent,
    onSecondary = Color.White,
    tertiary = Color(0xFF198754),
    onTertiary = Color.White,
    background = SkColors.Background,
    onBackground = SkColors.OnSurface,
    surface = SkColors.Surface,
    onSurface = SkColors.OnSurface,
    surfaceVariant = SkColors.SurfaceVariant,
    onSurfaceVariant = SkColors.OnSurfaceVariant,
    outline = SkColors.Outline,
    error = SkColors.Error,
    onError = SkColors.OnError,
    errorContainer = SkColors.ErrorContainer,
    onErrorContainer = SkColors.OnErrorContainer,
    surfaceContainer = SkColors.Surface,
    surfaceContainerLow = SkColors.Background,
    surfaceContainerHigh = SkColors.SurfaceVariant
)

private val DarkColorScheme = darkColorScheme(
    primary = Color(0xFF6EA8FE),
    onPrimary = Color(0xFF0A1628),
    primaryContainer = Color(0xFF084298),
    onPrimaryContainer = Color(0xFFE7F1FF),
    secondary = Color(0xFF90CAF9),
    onSecondary = Color(0xFF0A1628),
    tertiary = Color(0xFF81C784),
    onTertiary = Color(0xFF0A1628),
    background = Color(0xFF121212),
    onBackground = Color(0xFFE9ECEF),
    surface = Color(0xFF1E1E1E),
    onSurface = Color(0xFFE9ECEF),
    surfaceVariant = Color(0xFF2C2C2C),
    onSurfaceVariant = Color(0xFFADB5BD),
    outline = Color(0xFF6C757D),
    error = Color(0xFFFFB4AB),
    onError = Color(0xFF690005),
    errorContainer = Color(0xFF93000A),
    onErrorContainer = Color(0xFFFFDAD6),
    surfaceContainer = Color(0xFF1E1E1E),
    surfaceContainerLow = Color(0xFF121212),
    surfaceContainerHigh = Color(0xFF2C2C2C)
)

@Composable
fun SchoolKeeperTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    content: @Composable () -> Unit
) {
    val colorScheme = if (darkTheme) DarkColorScheme else LightColorScheme
    val view = LocalView.current
    if (!view.isInEditMode) {
        SideEffect {
            val window = (view.context as Activity).window
            window.statusBarColor = Color.Transparent.toArgb()
            window.navigationBarColor = colorScheme.surfaceContainer.toArgb()
            WindowCompat.getInsetsController(window, view).apply {
                isAppearanceLightStatusBars = !darkTheme
                isAppearanceLightNavigationBars = !darkTheme
            }
        }
    }
    MaterialTheme(
        colorScheme = colorScheme,
        typography = SchoolKeeperTypography,
        shapes = SchoolKeeperShapes,
        content = content
    )
}
