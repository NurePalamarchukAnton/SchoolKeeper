// Admin Panel JavaScript Module
// Управління адмін-панеллю з вкладками та CRUD операціями

const AdminPanel = {
    currentPage: {},
    pageSize: 50,
    
    // Ініціалізація адмін-панелі
    init() {
        this.setupTabs();
        this.loadInitialData();
    },
    
    // Налаштування вкладок
    setupTabs() {
        const tabButtons = document.querySelectorAll('[data-bs-toggle="tab"]');
        tabButtons.forEach(button => {
            button.addEventListener('shown.bs.tab', (e) => {
                const targetTab = e.target.getAttribute('data-bs-target');
                const entityType = targetTab.replace('#', '');
                this.loadTableData(entityType);
            });
        });
        
        // Завантажуємо дані для активної вкладки
        const activeTab = document.querySelector('.nav-link.active');
        if (activeTab) {
            const targetTab = activeTab.getAttribute('data-bs-target');
            const entityType = targetTab.replace('#', '');
            this.loadTableData(entityType);
        }
    },
    
    // Завантаження початкових даних (школи для випадаючих списків)
    async loadInitialData() {
        await this.loadSchools();
    },
    
    // Завантаження шкіл для випадаючих списків
    async loadSchools() {
        try {
            const { response, data } = await auth.apiRequest('/School?pageSize=1000');
            if (response.ok && data.data) {
                window.schoolsList = data.data;
            }
        } catch (error) {
            console.error('Error loading schools:', error);
        }
    },
    
    // Завантаження даних таблиці
    async loadTableData(entityType, page = 1) {
        const tableBody = document.querySelector(`#${entityType}Table tbody`);
        const loadingRow = tableBody.querySelector('.loading-row');
        
        if (!loadingRow) {
            tableBody.innerHTML = '<tr class="loading-row"><td colspan="100%" class="text-center"><div class="spinner-border" role="status"><span class="visually-hidden">Завантаження...</span></div></td></tr>';
        }
        
        try {
            const { response, data } = await auth.apiRequest(`/${entityType}?page=${page}&pageSize=${this.pageSize}`);
            
            if (response.ok && data.data) {
                this.renderTable(entityType, data.data);
                this.currentPage[entityType] = page;
            } else {
                this.showError(`Помилка завантаження даних: ${data.message || 'Невідома помилка'}`);
                tableBody.innerHTML = '<tr><td colspan="100%" class="text-center text-danger">Помилка завантаження даних</td></tr>';
            }
        } catch (error) {
            console.error(`Error loading ${entityType}:`, error);
            this.showError('Помилка завантаження даних');
            tableBody.innerHTML = '<tr><td colspan="100%" class="text-center text-danger">Помилка завантаження даних</td></tr>';
        }
    },
    
    // Відображення таблиці
    renderTable(entityType, items) {
        const tableBody = document.querySelector(`#${entityType}Table tbody`);
        if (!tableBody) return;
        
        if (items.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="100%" class="text-center">Немає даних</td></tr>';
            return;
        }
        
        let html = '';
        items.forEach(item => {
            html += this.renderTableRow(entityType, item);
        });
        
        tableBody.innerHTML = html;
    },
    
    // Відображення рядка таблиці
    renderTableRow(entityType, item) {
        switch (entityType) {
            case 'School':
                return `
                    <tr>
                        <td>${item.id || ''}</td>
                        <td>${this.escapeHtml(item.name || '')}</td>
                        <td>${this.escapeHtml(item.address || '')}</td>
                        <td>${this.escapeHtml(item.region || '')}</td>
                        <td>${this.escapeHtml(item.contactNumber || '')}</td>
                        <td>
                            <button class="btn btn-sm btn-warning" onclick="AdminPanel.editItem('${entityType}', ${item.id})">Редагувати</button>
                            <button class="btn btn-sm btn-danger" onclick="AdminPanel.deleteItem('${entityType}', ${item.id})">Видалити</button>
                        </td>
                    </tr>
                `;
            case 'User':
                return `
                    <tr>
                        <td>${item.id || ''}</td>
                        <td>${this.escapeHtml(item.fullName || '')}</td>
                        <td>${this.escapeHtml(item.email || '')}</td>
                        <td><span class="badge bg-info">${item.roleString || item.role || ''}</span></td>
                        <td>${this.escapeHtml(item.phoneNumber || '')}</td>
                        <td>${item.schoolId || ''}</td>
                        <td>
                            <button class="btn btn-sm btn-warning" onclick="AdminPanel.editItem('${entityType}', ${item.id})">Редагувати</button>
                            <button class="btn btn-sm btn-danger" onclick="AdminPanel.deleteItem('${entityType}', ${item.id})">Видалити</button>
                        </td>
                    </tr>
                `;
            case 'Device':
                const deviceStatus = item.statusString || item.status || '';
                return `
                    <tr>
                        <td>${item.id || ''}</td>
                        <td>${this.escapeHtml(item.deviceName || '')}</td>
                        <td><span class="badge bg-secondary">${item.deviceTypeString || item.deviceType || ''}</span></td>
                        <td><span class="badge ${deviceStatus === 'Active' ? 'bg-success' : 'bg-danger'}">${deviceStatus}</span></td>
                        <td>${this.escapeHtml(item.location || '')}</td>
                        <td>${item.schoolId || ''}</td>
                        <td>
                            <button class="btn btn-sm btn-warning" onclick="AdminPanel.editItem('${entityType}', ${item.id})">Редагувати</button>
                            <button class="btn btn-sm btn-danger" onclick="AdminPanel.deleteItem('${entityType}', ${item.id})">Видалити</button>
                        </td>
                    </tr>
                `;
            case 'Incident':
                const incidentStatus = item.statusString || item.status || '';
                return `
                    <tr>
                        <td>${item.id || ''}</td>
                        <td>${item.deviceId || ''}</td>
                        <td>${item.reportedBy || ''}</td>
                        <td>${this.escapeHtml(item.incidentType || '')}</td>
                        <td><span class="badge bg-warning">${item.severityString || item.severity || ''}</span></td>
                        <td>${this.escapeHtml((item.description || '').substring(0, 50))}${(item.description || '').length > 50 ? '...' : ''}</td>
                        <td>${new Date(item.timestamp).toLocaleString('uk-UA')}</td>
                        <td><span class="badge ${incidentStatus === 'Active' ? 'bg-danger' : 'bg-success'}">${incidentStatus}</span></td>
                        <td>${item.schoolId || ''}</td>
                        <td>
                            <button class="btn btn-sm btn-warning" onclick="AdminPanel.editItem('${entityType}', ${item.id})">Редагувати</button>
                            <button class="btn btn-sm btn-danger" onclick="AdminPanel.deleteItem('${entityType}', ${item.id})">Видалити</button>
                        </td>
                    </tr>
                `;
            case 'Rept':
                return `
                    <tr>
                        <td>${item.id || ''}</td>
                        <td>${item.schoolId || ''}</td>
                        <td>${item.generatedBy || ''}</td>
                        <td>${item.periodStart || ''}</td>
                        <td>${item.periodEnd || ''}</td>
                        <td>${this.escapeHtml((item.summary || '').substring(0, 50))}${(item.summary || '').length > 50 ? '...' : ''}</td>
                        <td>${new Date(item.generatedOn).toLocaleString('uk-UA')}</td>
                        <td>
                            <button class="btn btn-sm btn-warning" onclick="AdminPanel.editItem('${entityType}', ${item.id})">Редагувати</button>
                            <button class="btn btn-sm btn-danger" onclick="AdminPanel.deleteItem('${entityType}', ${item.id})">Видалити</button>
                        </td>
                    </tr>
                `;
            default:
                return '';
        }
    },
    
    // Відкриття модального вікна для створення
    createItem(entityType) {
        const modal = new bootstrap.Modal(document.getElementById(`${entityType}Modal`));
        const form = document.getElementById(`${entityType}Form`);
        if (form) {
            form.reset();
            form.dataset.mode = 'create';
            form.dataset.id = '';
        }
        
        // Скрываем секцию участников при создании нового инцидента
        if (entityType === 'Incident') {
            const usersSection = document.getElementById('incidentUsersSection');
            if (usersSection) {
                usersSection.style.display = 'none';
            }
        }
        
        this.populateFormDropdowns(entityType);
        modal.show();
    },
    
    // Відкриття модального вікна для редагування
    async editItem(entityType, id) {
        try {
            const { response, data } = await auth.apiRequest(`/${entityType}/${id}`);
            
            if (response.ok && data.data) {
                const modal = new bootstrap.Modal(document.getElementById(`${entityType}Modal`));
                const form = document.getElementById(`${entityType}Form`);
                
                if (form) {
                    this.populateForm(form, data.data, entityType);
                    form.dataset.mode = 'edit';
                    form.dataset.id = id;
                }
                
                this.populateFormDropdowns(entityType);
                modal.show();
            } else {
                this.showError('Помилка завантаження даних для редагування');
            }
        } catch (error) {
            console.error('Error loading item for edit:', error);
            this.showError('Помилка завантаження даних');
        }
    },
    
    // Заповнення форми даними
    populateForm(form, data, entityType) {
        Object.keys(data).forEach(key => {
            const input = form.querySelector(`[name="${key}"]`);
            if (input) {
                if (input.type === 'checkbox') {
                    input.checked = data[key];
                } else if (input.type === 'datetime-local') {
                    // Конвертуємо ISO дату в формат datetime-local
                    if (data[key]) {
                        const date = new Date(data[key]);
                        const year = date.getFullYear();
                        const month = String(date.getMonth() + 1).padStart(2, '0');
                        const day = String(date.getDate()).padStart(2, '0');
                        const hours = String(date.getHours()).padStart(2, '0');
                        const minutes = String(date.getMinutes()).padStart(2, '0');
                        input.value = `${year}-${month}-${day}T${hours}:${minutes}`;
                    }
                } else if (input.type === 'date') {
                    // Для DateOnly просто використовуємо значення
                    if (data[key]) {
                        input.value = data[key];
                    }
                } else {
                    input.value = data[key] || '';
                }
            }
        });
        
        // Використовуємо строкові значення enum'ів для випадаючих списків
        if (entityType === 'User' && data.roleString) {
            const roleInput = form.querySelector('[name="role"]');
            if (roleInput) {
                roleInput.value = data.roleString;
            }
        }
        
        if (entityType === 'Device') {
            if (data.deviceTypeString) {
                const deviceTypeInput = form.querySelector('[name="deviceType"]');
                if (deviceTypeInput) {
                    deviceTypeInput.value = data.deviceTypeString;
                }
            }
            if (data.statusString) {
                const statusInput = form.querySelector('[name="status"]');
                if (statusInput) {
                    statusInput.value = data.statusString;
                }
            }
        }
        
        if (entityType === 'Incident') {
            if (data.severityString) {
                const severityInput = form.querySelector('[name="severity"]');
                if (severityInput) {
                    severityInput.value = data.severityString;
                }
            }
            if (data.statusString) {
                const statusInput = form.querySelector('[name="status"]');
                if (statusInput) {
                    statusInput.value = data.statusString;
                }
            }
            
            // Отображаем участников инцидента
            this.displayIncidentUsers(data.users || []);
        }
        
        // Для User при редагуванні очищаємо поле пароля
        if (entityType === 'User' && form.dataset.mode === 'edit') {
            const passwordInput = form.querySelector('[name="password"]');
            if (passwordInput) {
                passwordInput.value = '';
            }
        }
    },
    
    // Отображение участников инцидента
    displayIncidentUsers(users) {
        const usersSection = document.getElementById('incidentUsersSection');
        const usersList = document.getElementById('incidentUsersList');
        
        if (!usersSection || !usersList) return;
        
        if (users && users.length > 0) {
            usersSection.style.display = 'block';
            let html = '<div class="list-group list-group-flush">';
            users.forEach(user => {
                html += `
                    <div class="list-group-item d-flex justify-content-between align-items-center">
                        <div>
                            <strong>${this.escapeHtml(user.fullName)}</strong>
                            <br>
                            <small class="text-muted">${this.escapeHtml(user.email)}</small>
                            <span class="badge bg-info ms-2">${this.escapeHtml(user.role)}</span>
                        </div>
                    </div>
                `;
            });
            html += '</div>';
            usersList.innerHTML = html;
        } else {
            usersSection.style.display = 'block';
            usersList.innerHTML = '<p class="text-muted mb-0">Немає учасників, пов\'язаних з цим інцидентом</p>';
        }
    },
    
    // Отображение участников инцидента
    displayIncidentUsers(users) {
        const usersSection = document.getElementById('incidentUsersSection');
        const usersList = document.getElementById('incidentUsersList');
        
        if (!usersSection || !usersList) return;
        
        if (users && users.length > 0) {
            usersSection.style.display = 'block';
            let html = '<div class="list-group list-group-flush">';
            users.forEach(user => {
                html += `
                    <div class="list-group-item d-flex justify-content-between align-items-center">
                        <div>
                            <strong>${this.escapeHtml(user.fullName)}</strong>
                            <br>
                            <small class="text-muted">${this.escapeHtml(user.email)}</small>
                            <span class="badge bg-info ms-2">${this.escapeHtml(user.role)}</span>
                        </div>
                    </div>
                `;
            });
            html += '</div>';
            usersList.innerHTML = html;
        } else {
            usersSection.style.display = 'block';
            usersList.innerHTML = '<p class="text-muted mb-0">Немає учасників, пов\'язаних з цим інцидентом</p>';
        }
    },
    
    // Заповнення випадаючих списків
    populateFormDropdowns(entityType) {
        // Заповнюємо список шкіл
        const schoolSelects = document.querySelectorAll(`#${entityType}Form select[name="schoolId"]`);
        schoolSelects.forEach(select => {
            if (window.schoolsList) {
                select.innerHTML = '<option value="">Оберіть школу</option>';
                window.schoolsList.forEach(school => {
                    select.innerHTML += `<option value="${school.id}">${this.escapeHtml(school.name)}</option>`;
                });
            }
        });
        
        // Заповнюємо список користувачів для ReportedBy/GeneratedBy
        if (entityType === 'Incident' || entityType === 'Rept') {
            this.loadUsersForDropdown(entityType);
        }
        
        // Заповнюємо список пристроїв для DeviceId
        if (entityType === 'Incident') {
            this.loadDevicesForDropdown();
        }
    },
    
    // Завантаження користувачів для випадаючого списку
    async loadUsersForDropdown(entityType) {
        try {
            const { response, data } = await auth.apiRequest('/User?pageSize=1000');
            if (response.ok && data.data) {
                const selectName = entityType === 'Incident' ? 'reportedBy' : 'generatedBy';
                const select = document.querySelector(`#${entityType}Form select[name="${selectName}"]`);
                if (select) {
                    select.innerHTML = '<option value="">Оберіть користувача</option>';
                    data.data.forEach(user => {
                        select.innerHTML += `<option value="${user.id}">${this.escapeHtml(user.fullName)} (${user.email})</option>`;
                    });
                }
            }
        } catch (error) {
            console.error('Error loading users:', error);
        }
    },
    
    // Завантаження пристроїв для випадаючого списку
    async loadDevicesForDropdown() {
        try {
            const { response, data } = await auth.apiRequest('/Device?pageSize=1000');
            if (response.ok && data.data) {
                const select = document.querySelector('#IncidentForm select[name="deviceId"]');
                if (select) {
                    select.innerHTML = '<option value="">Оберіть пристрій</option>';
                    data.data.forEach(device => {
                        select.innerHTML += `<option value="${device.id}">${this.escapeHtml(device.deviceName)}</option>`;
                    });
                }
            }
        } catch (error) {
            console.error('Error loading devices:', error);
        }
    },
    
    // Конвертація строкових enum'ів в числові значення
    convertEnumStringToNumber(entityType, key, value) {
        // DeviceType enum: MotionSensor=0, AlarmButton=1, AccessControl=2
        if (key === 'deviceType') {
            const deviceTypeMap = {
                'MotionSensor': 0,
                'AlarmButton': 1,
                'AccessControl': 2
            };
            return deviceTypeMap[value] !== undefined ? deviceTypeMap[value] : value;
        }
        
        // DeviceStatus enum: Active=0, Inactive=1, Error=2
        if (key === 'status' && entityType === 'Device') {
            const deviceStatusMap = {
                'Active': 0,
                'Inactive': 1,
                'Error': 2
            };
            return deviceStatusMap[value] !== undefined ? deviceStatusMap[value] : value;
        }
        
        // IncidentSeverity enum: Low=0, Medium=1, High=2, Critical=3
        if (key === 'severity') {
            const severityMap = {
                'Low': 0,
                'Medium': 1,
                'High': 2,
                'Critical': 3
            };
            return severityMap[value] !== undefined ? severityMap[value] : value;
        }
        
        // IncidentStatus enum: Active=0, Resolved=1
        if (key === 'status' && entityType === 'Incident') {
            const incidentStatusMap = {
                'Active': 0,
                'Resolved': 1
            };
            return incidentStatusMap[value] !== undefined ? incidentStatusMap[value] : value;
        }
        
        // UserRole enum: Admin=0, Security=1, Teacher=2, Parent=3, Student=4
        if (key === 'role') {
            const roleMap = {
                'Admin': 0,
                'Security': 1,
                'Teacher': 2,
                'Parent': 3,
                'Student': 4
            };
            return roleMap[value] !== undefined ? roleMap[value] : value;
        }
        
        return value;
    },
    
    // Збереження форми
    async saveForm(entityType) {
        const form = document.getElementById(`${entityType}Form`);
        if (!form) return;
        
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        const formData = new FormData(form);
        const data = {};
        
        // Обробка полів форми
        formData.forEach((value, key) => {
            // Для User: пропускаємо порожній пароль при редагуванні
            if (entityType === 'User' && key === 'password' && form.dataset.mode === 'edit' && !value) {
                return; // Не додаємо порожній пароль
            }
            
            // Пропускаємо порожні значення для необов'язкових полів
            if (value && value.trim() !== '') {
                // Конвертуємо числа
                if (key.includes('Id') || key === 'reportedBy' || key === 'generatedBy' || key === 'deviceId') {
                    data[key] = parseInt(value);
                }
                // Конвертуємо дати
                else if (key === 'timestamp' || key === 'generatedOn') {
                    if (value) {
                        data[key] = new Date(value).toISOString();
                    }
                }
                // Конвертуємо DateOnly
                else if (key === 'periodStart' || key === 'periodEnd') {
                    data[key] = value; // DateOnly приймає формат YYYY-MM-DD
                }
                // Конвертуємо enum'и з строк в числа
                else if (key === 'deviceType' || key === 'status' || key === 'severity' || key === 'role') {
                    data[key] = this.convertEnumStringToNumber(entityType, key, value);
                }
                else {
                    data[key] = value;
                }
            }
        });
        
        const mode = form.dataset.mode;
        const id = form.dataset.id;
        
        try {
            let response, result;
            
            if (mode === 'create') {
                ({ response, data: result } = await auth.apiRequest(`/${entityType}`, {
                    method: 'POST',
                    body: JSON.stringify(data)
                }));
            } else {
                ({ response, data: result } = await auth.apiRequest(`/${entityType}/${id}`, {
                    method: 'PUT',
                    body: JSON.stringify(data)
                }));
            }
            
            if (response.ok) {
                this.showSuccess(mode === 'create' ? 'Запис успішно створено' : 'Запис успішно оновлено');
                const modal = bootstrap.Modal.getInstance(document.getElementById(`${entityType}Modal`));
                if (modal) modal.hide();
                this.loadTableData(entityType, this.currentPage[entityType] || 1);
            } else {
                this.showError(result.message || 'Помилка збереження даних');
            }
        } catch (error) {
            console.error('Error saving form:', error);
            this.showError('Помилка збереження даних');
        }
    },
    
    // Видалення запису
    async deleteItem(entityType, id) {
        if (!confirm('Ви впевнені, що хочете видалити цей запис?')) {
            return;
        }
        
        try {
            const { response, data } = await auth.apiRequest(`/${entityType}/${id}`, {
                method: 'DELETE'
            });
            
            if (response.ok) {
                this.showSuccess('Запис успішно видалено');
                this.loadTableData(entityType, this.currentPage[entityType] || 1);
            } else {
                this.showError(data.message || 'Помилка видалення запису');
            }
        } catch (error) {
            console.error('Error deleting item:', error);
            this.showError('Помилка видалення запису');
        }
    },
    
    // Показ повідомлення про успіх
    showSuccess(message) {
        this.showNotification(message, 'success');
    },
    
    // Показ повідомлення про помилку
    showError(message) {
        this.showNotification(message, 'danger');
    },
    
    // Показ повідомлення
    showNotification(message, type) {
        // Створюємо або знаходимо контейнер для повідомлень
        let alertContainer = document.getElementById('alertContainer');
        if (!alertContainer) {
            alertContainer = document.createElement('div');
            alertContainer.id = 'alertContainer';
            alertContainer.className = 'position-fixed top-0 end-0 p-3';
            alertContainer.style.zIndex = '9999';
            document.body.appendChild(alertContainer);
        }
        
        const alertId = `alert-${Date.now()}`;
        const alert = document.createElement('div');
        alert.id = alertId;
        alert.className = `alert alert-${type} alert-dismissible fade show`;
        alert.role = 'alert';
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        alertContainer.appendChild(alert);
        
        // Автоматично приховуємо через 5 секунд
        setTimeout(() => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 5000);
    },
    
    // Екранування HTML
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
};

// Ініціалізація при завантаженні сторінки
document.addEventListener('DOMContentLoaded', function() {
    if (document.getElementById('adminPanel')) {
        AdminPanel.init();
    }
});

// Експорт для глобального використання
window.AdminPanel = AdminPanel;

