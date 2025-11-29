// Auth.js - Управління автентифікацією та API викликами

// Базовий URL для API
const API_BASE_URL = '/api';

// Отримання токена з localStorage
function getAuthToken() {
    return localStorage.getItem('authToken');
}

// Перевірка чи користувач автентифікований
function isAuthenticated() {
    return !!getAuthToken();
}

// Збереження даних користувача після логіну
function saveUserData(userData) {
    if (userData.token) {
        localStorage.setItem('authToken', userData.token);
    }
    if (userData.email) {
        localStorage.setItem('userEmail', userData.email);
    }
    if (userData.role) {
        localStorage.setItem('userRole', userData.role);
    }
    if (userData.userId) {
        localStorage.setItem('userId', userData.userId);
    }
    
    // Очищаем данные impersonation при обычном логине (если они не переданы явно)
    if (!userData.originalAdminId) {
        localStorage.removeItem('isImpersonating');
        localStorage.removeItem('originalAdminId');
    } else {
        // Если есть originalAdminId, значит это impersonation
        localStorage.setItem('isImpersonating', 'true');
        localStorage.setItem('originalAdminId', userData.originalAdminId);
    }
    
    // Встановлюємо cookie для серверної перевірки
    if (userData.token) {
        document.cookie = `authToken=${userData.token}; path=/; max-age=43200`; // 12 годин
    }
}

// Очищення даних користувача
function clearUserData() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('userEmail');
    localStorage.removeItem('userRole');
    localStorage.removeItem('userId');
    localStorage.removeItem('isImpersonating');
    localStorage.removeItem('originalAdminId');
    
    // Видаляємо cookie
    document.cookie = 'authToken=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
    document.cookie = 'isImpersonating=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
    document.cookie = 'originalAdminId=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
}

// Вихід з системи
function logout() {
    clearUserData();
    window.location.href = '/Login';
}

// Оновлення навігації на основі стану автентифікації
// Примітка: основна функція updateNavigation() знаходиться в _Layout.cshtml
// Ця функція залишена для сумісності зі старим кодом, але не викликається автоматично
function updateNavigation() {
    // Перевіряємо, чи існує основна функція updateNavigation в глобальній області
    // Якщо вона визначена в _Layout.cshtml, вона має пріоритет
    if (typeof window.updateNavigation === 'function' && window.updateNavigation.toString().includes('userDropdownNavItem')) {
        // Використовуємо основну функцію з layout
        window.updateNavigation();
        return;
    }
    
    const isAuth = isAuthenticated();
    const loginNavItem = document.getElementById('loginNavItem');
    const registerNavItem = document.getElementById('registerNavItem');
    const logoutNavItem = document.getElementById('logoutNavItem');
    const userInfoNavItem = document.getElementById('userInfoNavItem');
    const userEmail = document.getElementById('userEmail');
    
    if (isAuth) {
        if (loginNavItem) loginNavItem.style.display = 'none';
        if (registerNavItem) registerNavItem.style.display = 'none';
        if (logoutNavItem) logoutNavItem.style.display = 'block';
        if (userInfoNavItem) userInfoNavItem.style.display = 'block';
        if (userEmail) {
            userEmail.textContent = localStorage.getItem('userEmail') || '';
        }
    } else {
        if (loginNavItem) loginNavItem.style.display = 'block';
        if (registerNavItem) registerNavItem.style.display = 'block';
        if (logoutNavItem) logoutNavItem.style.display = 'none';
        if (userInfoNavItem) userInfoNavItem.style.display = 'none';
    }
}

// Виконання API запиту з автентифікацією
async function apiRequest(endpoint, options = {}) {
    const token = getAuthToken();
    
    if (!token) {
        console.warn('No auth token found in localStorage');
        clearUserData();
        window.location.href = '/Login';
        return null;
    }
    
    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
        }
    };
    
    defaultOptions.headers['Authorization'] = `Bearer ${token}`;
    
    const finalOptions = {
        ...defaultOptions,
        ...options,
        headers: {
            ...defaultOptions.headers,
            ...options.headers
        }
    };
    
    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, finalOptions);
        
        // Якщо токен недійсний, виходимо
        if (response.status === 401) {
            clearUserData();
            window.location.href = '/Login';
            return null;
        }
        
        // Пытаемся получить JSON, но если это не JSON, возвращаем текст
        let data;
        const contentType = response.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            try {
                data = await response.json();
            } catch (jsonError) {
                console.error('Error parsing JSON:', jsonError);
                data = { message: 'Помилка парсингу відповіді сервера' };
            }
        } else {
            const text = await response.text();
            data = { message: text || 'Невідома помилка' };
        }
        
        return { response, data };
    } catch (error) {
        console.error('API request error:', error);
        throw error;
    }
}

// Ініціалізація при завантаженні сторінки
document.addEventListener('DOMContentLoaded', function() {
    // Оновлюємо навігацію тільки якщо основна функція з layout ще не визначена
    // Основна функція updateNavigation() викликається в _Layout.cshtml
    // Тому тут не викликаємо, щоб уникнути конфліктів
    
    // Додаємо обробник для кнопки виходу (якщо вона існує)
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', function(e) {
            e.preventDefault();
            logout();
        });
    }
    
    // Перевірка автентифікації на захищених сторінках
    const currentPath = window.location.pathname;
    const protectedPaths = ['/'];
    
    if (protectedPaths.includes(currentPath) && !isAuthenticated()) {
        // Якщо на головній сторінці і не автентифікований, не перенаправляємо
        // (головна сторінка показує різний контент)
    }
});

// Експорт функцій для використання в інших скриптах
window.auth = {
    getAuthToken,
    isAuthenticated,
    saveUserData,
    clearUserData,
    logout,
    updateNavigation,
    apiRequest
};

