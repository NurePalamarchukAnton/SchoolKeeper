(function () {
    'use strict';

    function skT(key, fallback) {
        var sk = window.SchoolKeeperI18n;
        return sk ? sk.t(key) : fallback;
    }

    function getAuthToken() {
        var cookies = document.cookie.split(';');
        for (var i = 0; i < cookies.length; i++) {
            var parts = cookies[i].trim().split('=');
            if (parts[0] === 'authToken') return parts[1];
        }
        return localStorage.getItem('authToken');
    }

    window.exportReport = async function (reportId, format) {
        var token = getAuthToken();
        if (!token) {
            alert(skT('reports.export.notAuth', 'Помилка: Ви не авторизовані. Будь ласка, увійдіть в систему.'));
            window.location.href = '/Login';
            return;
        }

        try {
            var response = await fetch('/api/Rept/' + reportId + '/export?format=' + format, {
                method: 'GET',
                headers: {
                    'Authorization': 'Bearer ' + token,
                    'Accept': format === 'json' ? 'application/json' : format === 'csv' ? 'text/csv' : 'text/plain'
                }
            });

            if (!response.ok) {
                if (response.status === 401) {
                    alert(skT('reports.export.sessionExpired', 'Помилка: Сесія закінчилася. Будь ласка, увійдіть в систему знову.'));
                    window.location.href = '/Login';
                    return;
                }
                throw new Error(skT('reports.export.loadFailed', 'Помилка завантаження') + ': ' + response.status + ' ' + response.statusText);
            }

            var blob = await response.blob();
            var url = window.URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = url;
            a.download = 'report_' + reportId + '.' + format;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);
        } catch (error) {
            console.error('exportReport', error);
            alert(skT('reports.export.failed', 'Помилка при завантаженні звіту: ') + (error && error.message ? error.message : ''));
        }
    };
})();
