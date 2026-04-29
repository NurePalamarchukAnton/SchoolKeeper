(function () {
    'use strict';

    var STORAGE_KEY = 'schoolkeeper-ui-lang';
    var TEXT_DIR_STORAGE_KEY = 'schoolkeeper-text-dir';
    /** Поля, для яких керуємо атрибутом dir (ієрогліфи, арабська, іврит тощо). */
    var TEXT_DIR_INPUT_SELECTOR = 'input[type="text"], input[type="search"], input[type="tel"], input[type="url"], input[type="email"], input[type="password"], textarea';

    var DICT = {
        uk: {
            'nav.devices': 'Пристрої',
            'nav.admin': 'Адмін-панель',
            'nav.security': 'Безпека',
            'nav.teacher': 'Вчитель',
            'nav.parent': 'Батьки',
            'nav.student': 'Студент',
            'nav.reports': 'Звіти',
            'nav.login': 'Вхід',
            'nav.register': 'Реєстрація',
            'nav.logout': 'Вихід',
            'nav.impersonation': 'Від імені',
            'nav.stopImpersonation': 'Вийти з режиму входу від імені',
            'nav.toggleNavAria': 'Відкрити меню',
            'nav.textDir': 'Напрям тексту',
            'nav.textDirAuto': 'Авто',
            'nav.textDirLtr': 'Зліва направо',
            'nav.textDirRtl': 'Справа наліво',
            'nav.textDirHint': 'Напрямок набору тексту в полях форми',
            'footer.tagline': 'Система управління безпекою школи.',
            'pageTitle.home': 'Головна',
            'pageTitle.login': 'Вхід',
            'pageTitle.register': 'Реєстрація',
            'pageTitle.admin': 'Адмін-панель',
            'admin.title': 'Панель адміністратора',
            'admin.impersonate': 'Вхід від імені користувача',
            'admin.tab.schools': 'Школи',
            'admin.tab.users': 'Користувачі',
            'admin.tab.devices': 'Пристрої',
            'admin.tab.incidents': 'Інциденти',
            'admin.tab.reports': 'Звіти',
            'admin.loading': 'Завантаження...',
            'admin.schools.title': 'Школи',
            'admin.schools.add': 'Додати школу',
            'admin.users.title': 'Користувачі',
            'admin.users.add': 'Додати користувача',
            'admin.devices.title': 'Пристрої',
            'admin.devices.add': 'Додати пристрій',
            'admin.incidents.title': 'Інциденти',
            'admin.incidents.add': 'Додати інцидент',
            'admin.reports.title': 'Звіти',
            'admin.reports.add': 'Додати звіт',
            'admin.th.id': 'ID',
            'admin.th.name': 'Назва',
            'admin.th.address': 'Адреса',
            'admin.th.region': 'Регіон',
            'admin.th.contact': 'Контактний номер',
            'admin.th.actions': 'Дії',
            'admin.th.fullName': 'ПІБ',
            'admin.th.email': 'Email',
            'admin.th.role': 'Роль',
            'admin.th.phone': 'Телефон',
            'admin.th.schoolId': 'ID Школи',
            'admin.th.deviceId': 'ID Пристрою',
            'admin.th.reportedBy': 'Повідомив',
            'admin.th.type': 'Тип',
            'admin.th.severity': 'Серйозність',
            'admin.th.description': 'Опис',
            'admin.th.date': 'Дата',
            'admin.th.status': 'Статус',
            'admin.th.generatedBy': 'Створив',
            'admin.th.periodStart': 'Початок періоду',
            'admin.th.periodEnd': 'Кінець періоду',
            'admin.th.summary': 'Резюме',
            'admin.th.createdAt': 'Дата створення',
            'admin.edit': 'Редагувати',
            'admin.delete': 'Видалити',
            'admin.cancel': 'Скасувати',
            'admin.save': 'Зберегти',
            'admin.modal.school': 'Школа',
            'admin.modal.user': 'Користувач',
            'admin.modal.device': 'Пристрій',
            'admin.modal.incident': 'Інцидент',
            'admin.modal.rept': 'Звіт',
            'admin.lbl.nameRequired': 'Назва *',
            'admin.lbl.address': 'Адреса',
            'admin.lbl.region': 'Регіон',
            'admin.lbl.contactNumber': 'Контактний номер',
            'admin.lbl.fullNameRequired': 'ПІБ *',
            'admin.lbl.emailRequired': 'Email *',
            'admin.lbl.passwordOptional': 'Пароль',
            'admin.lbl.passwordHint': '(залиште порожнім, щоб не змінювати)',
            'admin.lbl.roleRequired': 'Роль *',
            'admin.lbl.phone': 'Телефон',
            'admin.lbl.schoolRequired': 'Школа *',
            'admin.lbl.deviceNameRequired': 'Назва *',
            'admin.lbl.deviceTypeRequired': 'Тип *',
            'admin.lbl.deviceStatusRequired': 'Статус *',
            'admin.lbl.location': 'Розташування',
            'admin.lbl.deviceRequired': 'Пристрій *',
            'admin.lbl.reportedByRequired': 'Повідомив *',
            'admin.lbl.incidentTypeRequired': 'Тип інциденту *',
            'admin.lbl.severityRequired': 'Серйозність *',
            'admin.lbl.description': 'Опис',
            'admin.lbl.dateTime': 'Дата та час',
            'admin.lbl.statusRequired': 'Статус *',
            'admin.lbl.incidentParticipants': 'Учасники інциденту',
            'admin.lbl.periodStartRequired': 'Початок періоду *',
            'admin.lbl.periodEndRequired': 'Кінець періоду *',
            'admin.lbl.summary': 'Резюме',
            'admin.lbl.generatedByRequired': 'Створив *',
            'admin.lbl.generatedOn': 'Дата створення',
            'admin.placeholder.selectSchool': 'Оберіть школу',
            'admin.placeholder.selectUser': 'Оберіть користувача',
            'admin.placeholder.selectDevice': 'Оберіть пристрій',
            'admin.empty.participants': 'Немає учасників, пов\'язаних з цим інцидентом',
            'admin.err.load': 'Помилка завантаження даних',
            'admin.err.loadPrefix': 'Помилка завантаження даних: ',
            'admin.err.loadEdit': 'Помилка завантаження даних для редагування',
            'admin.err.save': 'Помилка збереження даних',
            'admin.err.delete': 'Помилка видалення запису',
            'admin.success.created': 'Запис успішно створено',
            'admin.success.updated': 'Запис успішно оновлено',
            'admin.success.deleted': 'Запис успішно видалено',
            'admin.emptyTable': 'Немає даних',
            'admin.confirmDelete': 'Ви впевнені, що хочете видалити цей запис?',
            'home.title': 'Ласкаво просимо до SchoolKeeper',
            'home.user': 'Користувач',
            'home.role': 'Роль:',
            'home.quickActions': 'Швидкі дії',
            'home.viewSchools': 'Переглянути школи',
            'home.apiDocs': 'API Документація',
            'home.unauthTitle': 'Для доступу до системи необхідно увійти',
            'home.login': 'Увійти',
            'home.register': 'Зареєструватися',
            'home.unknownRole': 'Невідома',
            'login.heading': 'Вхід',
            'login.password': 'Пароль',
            'login.submit': 'Увійти',
            'login.submitting': 'Вхід...',
            'login.noAccount': 'Немає акаунту?',
            'login.registerLink': 'Зареєструватися',
            'login.errorGeneric': 'Помилка входу. Перевірте email та пароль.',
            'login.errorNetwork': 'Помилка з\'єднання з сервером.',
            'register.heading': 'Реєстрація',
            'register.fullName': 'Повне ім\'я',
            'register.password': 'Пароль',
            'register.confirmPassword': 'Підтвердження пароля',
            'register.school': 'Школа',
            'register.selectSchool': 'Оберіть школу',
            'register.role': 'Роль',
            'register.submit': 'Зареєструватися',
            'register.submitting': 'Реєстрація...',
            'register.haveAccount': 'Вже маєте акаунт?',
            'register.loginLink': 'Увійти',
            'register.roleAdmin': 'Адміністратор',
            'register.roleSecurity': 'Охорона',
            'register.roleTeacher': 'Вчитель',
            'register.roleParent': 'Батько',
            'register.roleStudent': 'Студент',
            'register.errMismatch': 'Паролі не співпадають.',
            'register.errShort': 'Пароль повинен містити мінімум 6 символів.',
            'register.errNetwork': 'Помилка з\'єднання з сервером.',
            'register.errGeneric': 'Помилка реєстрації. Спробуйте ще раз.',
            'register.successDefault': 'Реєстрація успішна! Перенаправлення на сторінку входу...',
            'nav.impersonationStopError': 'Помилка виходу з режиму входу від імені: ',
            'nav.impersonationStopNetwork': 'Помилка підключення до сервера',
            'common.unknownError': 'Невідома помилка',
            'common.details': 'Деталі',
            'common.edit': 'Редагувати',
            'common.backToList': 'Назад до списку',
            'common.create': 'Створити',
            'common.save': 'Зберегти',
            'common.cancel': 'Скасувати',
            'common.resolve': 'Вирішити',
            'common.noDescription': 'Немає опису',
            'role.unknown': 'Користувач',
            'role.Admin': 'Адмін',
            'role.Security': 'Охорона',
            'role.Teacher': 'Вчитель',
            'role.Parent': 'Батько',
            'role.Student': 'Учень',
            'role.User': 'Користувач',
            'enum.DeviceStatus.Active': 'Активний',
            'enum.DeviceStatus.Inactive': 'Неактивний',
            'enum.DeviceStatus.Error': 'Помилка',
            'enum.IncidentStatus.Active': 'Активний',
            'enum.IncidentStatus.Resolved': 'Вирішено',
            'enum.IncidentSeverity.Low': 'Низька',
            'enum.IncidentSeverity.Medium': 'Середня',
            'enum.IncidentSeverity.High': 'Висока',
            'enum.IncidentSeverity.Critical': 'Критична',
            'pageTitle.reports': 'Звіти',
            'pageTitle.reportCreate': 'Створити звіт',
            'pageTitle.reportDetails': 'Деталі звіту',
            'pageTitle.reportEdit': 'Редагувати звіт',
            'pageTitle.devices': 'Пристрої',
            'pageTitle.deviceCreate': 'Додати пристрій',
            'pageTitle.deviceEdit': 'Редагувати пристрій',
            'pageTitle.deviceDetails': 'Деталі пристрою',
            'pageTitle.incidents': 'Інциденти',
            'pageTitle.incidentCreate': 'Створити інцидент',
            'pageTitle.incidentDetails': 'Деталі інциденту',
            'pageTitle.incidentEdit': 'Редагувати інцидент',
            'pageTitle.security': 'Панель охорони',
            'pageTitle.impersonate': 'Вхід від імені',
            'pageTitle.teacher': 'Панель вчителя',
            'pageTitle.parent': 'Панель батька',
            'pageTitle.student': 'Панель учня',
            'reports.title': 'Звіти',
            'reports.create': 'Створити звіт',
            'reports.th.periodStart': 'Період початку',
            'reports.th.periodEnd': 'Період кінця',
            'reports.th.created': 'Створено',
            'reports.th.author': 'Автор',
            'reports.th.actions': 'Дії',
            'reports.downloadTxt': 'Скачати TXT',
            'reports.createPageTitle': 'Створити звіт',
            'reports.autoInfo': 'Звіт буде автоматично сформовано на основі статистики за вказаний період.',
            'reports.lbl.periodStart': 'Період початку',
            'reports.lbl.periodEnd': 'Період кінця',
            'reports.lbl.summaryOptional': 'Додаткова інформація (опціонально)',
            'reports.placeholder.summary': 'Додайте додаткову інформацію до автоматично згенерованого звіту...',
            'reports.hint.stats': 'Основна статистика буде автоматично додана до звіту.',
            'reports.detailsTitle': 'Деталі звіту',
            'reports.dt.school': 'Школа',
            'reports.dt.summary': 'Зміст звіту',
            'reports.downloadTxtLong': 'Скачати звіт (TXT)',
            'reports.downloadJson': 'Скачати звіт (JSON)',
            'reports.downloadCsv': 'Скачати звіт (CSV)',
            'reports.editTitle': 'Редагувати звіт',
            'reports.selectSchool': '-- Оберіть школу --',
            'reports.lbl.description': 'Опис',
            'reports.export.notAuth': 'Помилка: Ви не авторизовані. Будь ласка, увійдіть в систему.',
            'reports.export.sessionExpired': 'Помилка: Сесія закінчилася. Будь ласка, увійдіть в систему знову.',
            'reports.export.loadFailed': 'Помилка завантаження',
            'reports.export.failed': 'Помилка при завантаженні звіту: ',
            'security.title': 'Панель охорони',
            'security.card.totalDevices': 'Всього пристроїв',
            'security.card.activeDevices': 'Активні пристрої',
            'security.card.inactiveDevices': 'Неактивні пристрої',
            'security.card.activeIncidents': 'Активні інциденти',
            'security.card.resolvedIncidents': 'Вирішені інциденти',
            'security.card.reports': 'Звіти',
            'security.quickActions': 'Швидкі дії',
            'security.btn.devices': 'Управління пристроями',
            'security.btn.incidents': 'Управління інцидентами',
            'security.btn.createReport': 'Створити звіт',
            'security.btn.viewReports': 'Перегляд звітів',
            'devices.title': 'Пристрої',
            'devices.add': 'Додати пристрій',
            'devices.detailsTitle': 'Деталі пристрою',
            'devices.dt.name': 'Назва:',
            'devices.dt.type': 'Тип:',
            'devices.dt.status': 'Статус:',
            'devices.dt.location': 'Розташування:',
            'devices.createTitle': 'Додати пристрій',
            'devices.editTitle': 'Редагувати пристрій',
            'devices.lbl.name': 'Назва пристрою',
            'devices.lbl.type': 'Тип пристрою',
            'devices.lbl.status': 'Статус',
            'devices.lbl.location': 'Розташування',
            'devices.lbl.school': 'Школа',
            'devices.selectType': '-- Оберіть тип --',
            'devices.selectStatus': '-- Оберіть статус --',
            'incidents.title': 'Інциденти',
            'incidents.create': 'Створити інцидент',
            'incidents.th.type': 'Тип',
            'incidents.th.severity': 'Серйозність',
            'incidents.th.device': 'Пристрій',
            'incidents.th.date': 'Дата',
            'incidents.th.status': 'Статус',
            'incidents.th.actions': 'Дії',
            'incidents.createPageTitle': 'Створити інцидент',
            'incidents.editTitle': 'Редагувати інцидент',
            'incidents.selectDevice': '-- Оберіть пристрій --',
            'incidents.selectSeverity': '-- Оберіть серйозність --',
            'incidents.selectStatus': '-- Оберіть статус --',
            'incidents.lbl.device': 'Пристрій',
            'incidents.lbl.incidentType': 'Тип інциденту',
            'incidents.detailsTitle': 'Деталі інциденту',
            'incidents.dt.type': 'Тип інциденту',
            'incidents.dt.severity': 'Серйозність',
            'incidents.dt.device': 'Пристрій',
            'incidents.dt.reporter': 'Повідомив',
            'incidents.dt.date': 'Дата',
            'incidents.dt.status': 'Статус',
            'impersonate.cardTitle': 'Вхід від імені користувача',
            'impersonate.alert': 'Ви будете працювати від імені обраного користувача. Всі дії будуть виконуватися від його імені.',
            'impersonate.attention': 'Увага!',
            'impersonate.lbl.user': 'Оберіть користувача *',
            'impersonate.loadingUsers': 'Завантаження користувачів...',
            'impersonate.hint': 'Виберіть користувача, від імені якого хочете увійти',
            'impersonate.submit': 'Увійти від імені користувача',
            'impersonate.back': 'Назад до адмін-панелі',
            'impersonate.errorPrefix': 'Помилка!',
            'impersonate.activeTitle': 'Режим входу від імені користувача активний',
            'impersonate.workingAs': 'Ви працюєте від імені:',
            'impersonate.role': 'Роль:',
            'impersonate.stop': 'Вийти з режиму входу від імені',
            'impersonate.selectPlaceholder': 'Оберіть користувача',
            'impersonate.loadUsersError': 'Помилка завантаження користувачів',
            'impersonate.pickUser': 'Будь ласка, оберіть користувача',
            'impersonate.signingIn': 'Вхід...',
            'impersonate.impersonateError': 'Помилка входу від імені користувача',
            'impersonate.networkError': 'Помилка підключення до сервера',
            'impersonate.stopping': 'Вихід...',
            'impersonate.stopError': 'Помилка виходу з режиму входу від імені',
            'teacher.title': 'Панель вчителя',
            'teacher.tab.students': 'Учні',
            'teacher.tab.parents': 'Батьки',
            'teacher.tab.incidents': 'Інциденти',
            'teacher.studentsTitle': 'Мої учні',
            'teacher.parentsTitle': 'Батьки моїх учнів',
            'teacher.incidentsTitle': 'Інциденти',
            'teacher.th.children': 'Діти',
            'teacher.emptyStudents': 'У вас поки немає учнів',
            'teacher.emptyParents': 'Немає інформації про батьків',
            'teacher.emptyIncidents': 'Немає інцидентів',
            'parent.title': 'Панель батька',
            'parent.card.incidents': 'Інциденти',
            'parent.card.activeIncidents': 'Активні інциденти',
            'parent.card.reports': 'Звіти',
            'parent.quickActions': 'Швидкі дії',
            'parent.btn.viewIncidents': 'Перегляд інцидентів',
            'parent.btn.viewReports': 'Перегляд звітів',
            'student.title': 'Панель учня',
            'student.card.myIncidents': 'Мої інциденти',
            'student.card.activeIncidents': 'Активні інциденти',
            'student.card.teachers': 'Мої вчителі',
            'student.teachersSection': 'Мої вчителі',
            'student.incidentsSection': 'Мої інциденти',
            'student.th.incidentType': 'Тип інциденту',
            'student.emptyTeachers': 'У вас поки що немає призначених вчителів.',
            'student.emptyIncidents': 'У вас поки що немає інцидентів з вашою участю.',
            'pageTitle.schools': 'Школи',
            'schools.title': 'Школи, підключені до системи',
            'schools.address': 'Адреса:',
            'schools.region': 'Регіон:',
            'schools.contact': 'Контактний номер:',
            'schools.empty': 'На даний момент немає шкіл, підключених до системи.',
            'incidents.participantsTitle': 'Учасники інциденту',
            'incidents.linkedUsersTitle': 'Користувачі, пов\'язані з інцидентом',
            'incidents.empty.noLinkedUsers': 'Немає користувачів, пов\'язаних з цим інцидентом',
            'incidents.addUserLabel': 'Додати користувача до інциденту:',
            'incidents.btn.add': 'Додати',
            'incidents.alert.pickUser': 'Будь ласка, оберіть користувача',
            'incidents.alert.errorPrefix': 'Помилка: ',
            'incidents.alert.network': 'Помилка підключення до сервера: ',
            'incidents.confirm.removeUser': 'Ви впевнені, що хочете видалити цього користувача з інциденту?'
        },
        en: {
            'nav.devices': 'Devices',
            'nav.admin': 'Admin',
            'nav.security': 'Security',
            'nav.teacher': 'Teacher',
            'nav.parent': 'Parents',
            'nav.student': 'Student',
            'nav.reports': 'Reports',
            'nav.login': 'Log in',
            'nav.register': 'Sign up',
            'nav.logout': 'Log out',
            'nav.impersonation': 'Impersonating',
            'nav.stopImpersonation': 'Stop impersonation',
            'nav.toggleNavAria': 'Toggle navigation',
            'nav.textDir': 'Text direction',
            'nav.textDirAuto': 'Auto',
            'nav.textDirLtr': 'Left to right',
            'nav.textDirRtl': 'Right to left',
            'nav.textDirHint': 'Typing direction in form fields',
            'footer.tagline': 'School safety management system.',
            'pageTitle.home': 'Home',
            'pageTitle.login': 'Log in',
            'pageTitle.register': 'Sign up',
            'pageTitle.admin': 'Admin',
            'admin.title': 'Administrator panel',
            'admin.impersonate': 'Sign in as user',
            'admin.tab.schools': 'Schools',
            'admin.tab.users': 'Users',
            'admin.tab.devices': 'Devices',
            'admin.tab.incidents': 'Incidents',
            'admin.tab.reports': 'Reports',
            'admin.loading': 'Loading...',
            'admin.schools.title': 'Schools',
            'admin.schools.add': 'Add school',
            'admin.users.title': 'Users',
            'admin.users.add': 'Add user',
            'admin.devices.title': 'Devices',
            'admin.devices.add': 'Add device',
            'admin.incidents.title': 'Incidents',
            'admin.incidents.add': 'Add incident',
            'admin.reports.title': 'Reports',
            'admin.reports.add': 'Add report',
            'admin.th.id': 'ID',
            'admin.th.name': 'Name',
            'admin.th.address': 'Address',
            'admin.th.region': 'Region',
            'admin.th.contact': 'Contact number',
            'admin.th.actions': 'Actions',
            'admin.th.fullName': 'Full name',
            'admin.th.email': 'Email',
            'admin.th.role': 'Role',
            'admin.th.phone': 'Phone',
            'admin.th.schoolId': 'School ID',
            'admin.th.deviceId': 'Device ID',
            'admin.th.reportedBy': 'Reported by',
            'admin.th.type': 'Type',
            'admin.th.severity': 'Severity',
            'admin.th.description': 'Description',
            'admin.th.date': 'Date',
            'admin.th.status': 'Status',
            'admin.th.generatedBy': 'Created by',
            'admin.th.periodStart': 'Period start',
            'admin.th.periodEnd': 'Period end',
            'admin.th.summary': 'Summary',
            'admin.th.createdAt': 'Created at',
            'admin.edit': 'Edit',
            'admin.delete': 'Delete',
            'admin.cancel': 'Cancel',
            'admin.save': 'Save',
            'admin.modal.school': 'School',
            'admin.modal.user': 'User',
            'admin.modal.device': 'Device',
            'admin.modal.incident': 'Incident',
            'admin.modal.rept': 'Report',
            'admin.lbl.nameRequired': 'Name *',
            'admin.lbl.address': 'Address',
            'admin.lbl.region': 'Region',
            'admin.lbl.contactNumber': 'Contact number',
            'admin.lbl.fullNameRequired': 'Full name *',
            'admin.lbl.emailRequired': 'Email *',
            'admin.lbl.passwordOptional': 'Password',
            'admin.lbl.passwordHint': '(leave blank to keep unchanged)',
            'admin.lbl.roleRequired': 'Role *',
            'admin.lbl.phone': 'Phone',
            'admin.lbl.schoolRequired': 'School *',
            'admin.lbl.deviceNameRequired': 'Name *',
            'admin.lbl.deviceTypeRequired': 'Type *',
            'admin.lbl.deviceStatusRequired': 'Status *',
            'admin.lbl.location': 'Location',
            'admin.lbl.deviceRequired': 'Device *',
            'admin.lbl.reportedByRequired': 'Reported by *',
            'admin.lbl.incidentTypeRequired': 'Incident type *',
            'admin.lbl.severityRequired': 'Severity *',
            'admin.lbl.description': 'Description',
            'admin.lbl.dateTime': 'Date and time',
            'admin.lbl.statusRequired': 'Status *',
            'admin.lbl.incidentParticipants': 'Incident participants',
            'admin.lbl.periodStartRequired': 'Period start *',
            'admin.lbl.periodEndRequired': 'Period end *',
            'admin.lbl.summary': 'Summary',
            'admin.lbl.generatedByRequired': 'Created by *',
            'admin.lbl.generatedOn': 'Created at',
            'admin.placeholder.selectSchool': 'Select a school',
            'admin.placeholder.selectUser': 'Select a user',
            'admin.placeholder.selectDevice': 'Select a device',
            'admin.empty.participants': 'No participants linked to this incident',
            'admin.err.load': 'Failed to load data',
            'admin.err.loadPrefix': 'Failed to load data: ',
            'admin.err.loadEdit': 'Failed to load item for editing',
            'admin.err.save': 'Failed to save data',
            'admin.err.delete': 'Failed to delete record',
            'admin.success.created': 'Record created successfully',
            'admin.success.updated': 'Record updated successfully',
            'admin.success.deleted': 'Record deleted successfully',
            'admin.emptyTable': 'No data',
            'admin.confirmDelete': 'Are you sure you want to delete this record?',
            'home.title': 'Welcome to SchoolKeeper',
            'home.user': 'User',
            'home.role': 'Role:',
            'home.quickActions': 'Quick actions',
            'home.viewSchools': 'View schools',
            'home.apiDocs': 'API documentation',
            'home.unauthTitle': 'Sign in to access the system',
            'home.login': 'Log in',
            'home.register': 'Sign up',
            'home.unknownRole': 'Unknown',
            'login.heading': 'Log in',
            'login.password': 'Password',
            'login.submit': 'Log in',
            'login.submitting': 'Signing in...',
            'login.noAccount': 'No account?',
            'login.registerLink': 'Sign up',
            'login.errorGeneric': 'Sign-in failed. Check your email and password.',
            'login.errorNetwork': 'Could not reach the server.',
            'register.heading': 'Sign up',
            'register.fullName': 'Full name',
            'register.password': 'Password',
            'register.confirmPassword': 'Confirm password',
            'register.school': 'School',
            'register.selectSchool': 'Select a school',
            'register.role': 'Role',
            'register.submit': 'Sign up',
            'register.submitting': 'Signing up...',
            'register.haveAccount': 'Already have an account?',
            'register.loginLink': 'Log in',
            'register.roleAdmin': 'Administrator',
            'register.roleSecurity': 'Security',
            'register.roleTeacher': 'Teacher',
            'register.roleParent': 'Parent',
            'register.roleStudent': 'Student',
            'register.errMismatch': 'Passwords do not match.',
            'register.errShort': 'Password must be at least 6 characters.',
            'register.errNetwork': 'Could not reach the server.',
            'register.errGeneric': 'Registration failed. Please try again.',
            'register.successDefault': 'Registration successful! Redirecting to sign-in...',
            'nav.impersonationStopError': 'Failed to stop impersonation: ',
            'nav.impersonationStopNetwork': 'Server connection error',
            'common.unknownError': 'Unknown error',
            'common.details': 'Details',
            'common.edit': 'Edit',
            'common.backToList': 'Back to list',
            'common.create': 'Create',
            'common.save': 'Save',
            'common.cancel': 'Cancel',
            'common.resolve': 'Resolve',
            'common.noDescription': 'No description',
            'role.unknown': 'User',
            'role.Admin': 'Admin',
            'role.Security': 'Security',
            'role.Teacher': 'Teacher',
            'role.Parent': 'Parent',
            'role.Student': 'Student',
            'role.User': 'User',
            'enum.DeviceStatus.Active': 'Active',
            'enum.DeviceStatus.Inactive': 'Inactive',
            'enum.DeviceStatus.Error': 'Error',
            'enum.IncidentStatus.Active': 'Active',
            'enum.IncidentStatus.Resolved': 'Resolved',
            'enum.IncidentSeverity.Low': 'Low',
            'enum.IncidentSeverity.Medium': 'Medium',
            'enum.IncidentSeverity.High': 'High',
            'enum.IncidentSeverity.Critical': 'Critical',
            'pageTitle.reports': 'Reports',
            'pageTitle.reportCreate': 'Create report',
            'pageTitle.reportDetails': 'Report details',
            'pageTitle.reportEdit': 'Edit report',
            'pageTitle.devices': 'Devices',
            'pageTitle.deviceCreate': 'Add device',
            'pageTitle.deviceEdit': 'Edit device',
            'pageTitle.deviceDetails': 'Device details',
            'pageTitle.incidents': 'Incidents',
            'pageTitle.incidentCreate': 'Create incident',
            'pageTitle.incidentDetails': 'Incident details',
            'pageTitle.incidentEdit': 'Edit incident',
            'pageTitle.security': 'Security dashboard',
            'pageTitle.impersonate': 'Sign in as user',
            'pageTitle.teacher': 'Teacher dashboard',
            'pageTitle.parent': 'Parent dashboard',
            'pageTitle.student': 'Student dashboard',
            'reports.title': 'Reports',
            'reports.create': 'Create report',
            'reports.th.periodStart': 'Start period',
            'reports.th.periodEnd': 'End period',
            'reports.th.created': 'Created',
            'reports.th.author': 'Author',
            'reports.th.actions': 'Actions',
            'reports.downloadTxt': 'Download TXT',
            'reports.createPageTitle': 'Create report',
            'reports.autoInfo': 'The report will be generated automatically from statistics for the selected period.',
            'reports.lbl.periodStart': 'Start period',
            'reports.lbl.periodEnd': 'End period',
            'reports.lbl.summaryOptional': 'Additional information (optional)',
            'reports.placeholder.summary': 'Add optional notes to the auto-generated report...',
            'reports.hint.stats': 'Core statistics will be added to the report automatically.',
            'reports.detailsTitle': 'Report details',
            'reports.dt.school': 'School',
            'reports.dt.summary': 'Report content',
            'reports.downloadTxtLong': 'Download report (TXT)',
            'reports.downloadJson': 'Download report (JSON)',
            'reports.downloadCsv': 'Download report (CSV)',
            'reports.editTitle': 'Edit report',
            'reports.selectSchool': '-- Select a school --',
            'reports.lbl.description': 'Description',
            'reports.export.notAuth': 'Error: you are not signed in. Please log in.',
            'reports.export.sessionExpired': 'Error: session expired. Please sign in again.',
            'reports.export.loadFailed': 'Download failed',
            'reports.export.failed': 'Error while downloading the report: ',
            'security.title': 'Security dashboard',
            'security.card.totalDevices': 'Total devices',
            'security.card.activeDevices': 'Active devices',
            'security.card.inactiveDevices': 'Inactive devices',
            'security.card.activeIncidents': 'Active incidents',
            'security.card.resolvedIncidents': 'Resolved incidents',
            'security.card.reports': 'Reports',
            'security.quickActions': 'Quick actions',
            'security.btn.devices': 'Device management',
            'security.btn.incidents': 'Incident management',
            'security.btn.createReport': 'Create report',
            'security.btn.viewReports': 'View reports',
            'devices.title': 'Devices',
            'devices.add': 'Add device',
            'devices.detailsTitle': 'Device details',
            'devices.dt.name': 'Name:',
            'devices.dt.type': 'Type:',
            'devices.dt.status': 'Status:',
            'devices.dt.location': 'Location:',
            'devices.createTitle': 'Add device',
            'devices.editTitle': 'Edit device',
            'devices.lbl.name': 'Device name',
            'devices.lbl.type': 'Device type',
            'devices.lbl.status': 'Status',
            'devices.lbl.location': 'Location',
            'devices.lbl.school': 'School',
            'devices.selectType': '-- Select type --',
            'devices.selectStatus': '-- Select status --',
            'incidents.title': 'Incidents',
            'incidents.create': 'Create incident',
            'incidents.th.type': 'Type',
            'incidents.th.severity': 'Severity',
            'incidents.th.device': 'Device',
            'incidents.th.date': 'Date',
            'incidents.th.status': 'Status',
            'incidents.th.actions': 'Actions',
            'incidents.createPageTitle': 'Create incident',
            'incidents.editTitle': 'Edit incident',
            'incidents.selectDevice': '-- Select device --',
            'incidents.selectSeverity': '-- Select severity --',
            'incidents.selectStatus': '-- Select status --',
            'incidents.lbl.device': 'Device',
            'incidents.lbl.incidentType': 'Incident type',
            'incidents.detailsTitle': 'Incident details',
            'incidents.dt.type': 'Incident type',
            'incidents.dt.severity': 'Severity',
            'incidents.dt.device': 'Device',
            'incidents.dt.reporter': 'Reported by',
            'incidents.dt.date': 'Date',
            'incidents.dt.status': 'Status',
            'impersonate.cardTitle': 'Sign in as user',
            'impersonate.alert': 'You will work on behalf of the selected user. All actions will be performed as that user.',
            'impersonate.attention': 'Attention!',
            'impersonate.lbl.user': 'Select user *',
            'impersonate.loadingUsers': 'Loading users...',
            'impersonate.hint': 'Choose the account you want to sign in as',
            'impersonate.submit': 'Sign in as user',
            'impersonate.back': 'Back to admin panel',
            'impersonate.errorPrefix': 'Error!',
            'impersonate.activeTitle': 'Impersonation is active',
            'impersonate.workingAs': 'You are signed in as:',
            'impersonate.role': 'Role:',
            'impersonate.stop': 'Stop impersonation',
            'impersonate.selectPlaceholder': 'Select a user',
            'impersonate.loadUsersError': 'Failed to load users',
            'impersonate.pickUser': 'Please select a user',
            'impersonate.signingIn': 'Signing in...',
            'impersonate.impersonateError': 'Could not start impersonation',
            'impersonate.networkError': 'Server connection error',
            'impersonate.stopping': 'Signing out...',
            'impersonate.stopError': 'Could not stop impersonation',
            'teacher.title': 'Teacher dashboard',
            'teacher.tab.students': 'Students',
            'teacher.tab.parents': 'Parents',
            'teacher.tab.incidents': 'Incidents',
            'teacher.studentsTitle': 'My students',
            'teacher.parentsTitle': 'Parents of my students',
            'teacher.incidentsTitle': 'Incidents',
            'teacher.th.children': 'Children',
            'teacher.emptyStudents': 'You have no students yet',
            'teacher.emptyParents': 'No parent information',
            'teacher.emptyIncidents': 'No incidents',
            'parent.title': 'Parent dashboard',
            'parent.card.incidents': 'Incidents',
            'parent.card.activeIncidents': 'Active incidents',
            'parent.card.reports': 'Reports',
            'parent.quickActions': 'Quick actions',
            'parent.btn.viewIncidents': 'View incidents',
            'parent.btn.viewReports': 'View reports',
            'student.title': 'Student dashboard',
            'student.card.myIncidents': 'My incidents',
            'student.card.activeIncidents': 'Active incidents',
            'student.card.teachers': 'My teachers',
            'student.teachersSection': 'My teachers',
            'student.incidentsSection': 'My incidents',
            'student.th.incidentType': 'Incident type',
            'student.emptyTeachers': 'You have no assigned teachers yet.',
            'student.emptyIncidents': 'You have no incidents involving you yet.',
            'pageTitle.schools': 'Schools',
            'schools.title': 'Schools connected to the system',
            'schools.address': 'Address:',
            'schools.region': 'Region:',
            'schools.contact': 'Contact number:',
            'schools.empty': 'There are no schools connected to the system yet.',
            'incidents.participantsTitle': 'Incident participants',
            'incidents.linkedUsersTitle': 'Users linked to this incident',
            'incidents.empty.noLinkedUsers': 'No users linked to this incident',
            'incidents.addUserLabel': 'Add user to incident:',
            'incidents.btn.add': 'Add',
            'incidents.alert.pickUser': 'Please select a user',
            'incidents.alert.errorPrefix': 'Error: ',
            'incidents.alert.network': 'Server connection error: ',
            'incidents.confirm.removeUser': 'Remove this user from the incident?'
        }
    };

    function normalizeLang(lang) {
        return lang === 'en' ? 'en' : 'uk';
    }

    function getLang() {
        try {
            return normalizeLang(localStorage.getItem(STORAGE_KEY) || 'uk');
        } catch (e) {
            return 'uk';
        }
    }

    function setLang(lang) {
        lang = normalizeLang(lang);
        try {
            localStorage.setItem(STORAGE_KEY, lang);
        } catch (e) { /* ignore */ }
        apply();
        try {
            window.dispatchEvent(new CustomEvent('schoolkeeper:langchange', { detail: { lang: lang } }));
        } catch (e) { /* ignore */ }
        updateLangToggleUi();
    }

    function toggleLang() {
        setLang(getLang() === 'en' ? 'uk' : 'en');
    }

    function t(key) {
        var lang = getLang();
        var table = DICT[lang] || DICT.uk;
        if (table[key] !== undefined) return table[key];
        return (DICT.uk[key] !== undefined ? DICT.uk[key] : key);
    }

    function formatRole(role) {
        if (!role) return t('role.unknown');
        var k = 'role.' + String(role);
        var lang = getLang();
        var tr = (DICT[lang] && DICT[lang][k] !== undefined) ? DICT[lang][k] : DICT.uk[k];
        return tr !== undefined ? tr : role;
    }

    function normalizeTextDir(dir) {
        if (dir === 'rtl' || dir === 'ltr') return dir;
        return 'auto';
    }

    function getTextDir() {
        try {
            return normalizeTextDir(localStorage.getItem(TEXT_DIR_STORAGE_KEY) || 'auto');
        } catch (e) {
            return 'auto';
        }
    }

    function setTextDir(dir) {
        dir = normalizeTextDir(dir);
        try {
            localStorage.setItem(TEXT_DIR_STORAGE_KEY, dir);
        } catch (e) { /* ignore */ }
        applyTextDir();
        updateTextDirSelectUi();
        try {
            window.dispatchEvent(new CustomEvent('schoolkeeper:textdirchange', { detail: { textDir: dir } }));
        } catch (e) { /* ignore */ }
    }

    function shouldApplyDirToField(el) {
        if (!el || !el.matches || !el.matches(TEXT_DIR_INPUT_SELECTOR)) return false;
        if (el.getAttribute('data-text-dir') === 'fixed') return false;
        return true;
    }

    function applyTextDirToElement(el) {
        if (!shouldApplyDirToField(el)) return;
        var d = getTextDir();
        if (d === 'auto') el.setAttribute('dir', 'auto');
        else el.setAttribute('dir', d);
    }

    function applyTextDir() {
        document.querySelectorAll(TEXT_DIR_INPUT_SELECTOR).forEach(function (el) {
            applyTextDirToElement(el);
        });
    }

    function updateTextDirSelectUi() {
        var sel = document.getElementById('textDirSelect');
        if (!sel) return;
        var v = getTextDir();
        if (sel.value !== v) sel.value = v;
    }

    var textDirMutationTimer;
    var textDirObserver;

    function setupTextDirObserver() {
        if (textDirObserver || !document.body) return;
        textDirObserver = new MutationObserver(function () {
            clearTimeout(textDirMutationTimer);
            textDirMutationTimer = setTimeout(applyTextDir, 150);
        });
        textDirObserver.observe(document.body, { childList: true, subtree: true });
    }

    function apply() {
        var lang = getLang();
        document.documentElement.lang = lang === 'en' ? 'en' : 'uk';

        document.querySelectorAll('[data-i18n]').forEach(function (el) {
            if (el.id === 'langToggleBtn') return;
            var key = el.getAttribute('data-i18n');
            if (!key) return;
            var text = (DICT[lang] && DICT[lang][key] !== undefined) ? DICT[lang][key] : DICT.uk[key];
            if (text !== undefined) el.textContent = text;
        });

        document.querySelectorAll('[data-i18n-aria-label]').forEach(function (el) {
            var key = el.getAttribute('data-i18n-aria-label');
            if (!key) return;
            var text = (DICT[lang] && DICT[lang][key] !== undefined) ? DICT[lang][key] : DICT.uk[key];
            if (text !== undefined) el.setAttribute('aria-label', text);
        });

        document.querySelectorAll('[data-i18n-placeholder]').forEach(function (el) {
            var key = el.getAttribute('data-i18n-placeholder');
            if (!key) return;
            var text = (DICT[lang] && DICT[lang][key] !== undefined) ? DICT[lang][key] : DICT.uk[key];
            if (text !== undefined) el.setAttribute('placeholder', text);
        });

        document.querySelectorAll('[data-i18n-title]').forEach(function (el) {
            var key = el.getAttribute('data-i18n-title');
            if (!key) return;
            var text = (DICT[lang] && DICT[lang][key] !== undefined) ? DICT[lang][key] : DICT.uk[key];
            if (text !== undefined) el.setAttribute('title', text);
        });

        var page = document.body && document.body.getAttribute('data-i18n-page');
        if (page) {
            var titleKey = 'pageTitle.' + page;
            var titleText = (DICT[lang] && DICT[lang][titleKey] !== undefined) ? DICT[lang][titleKey] : DICT.uk[titleKey];
            if (titleText) document.title = titleText + ' - SchoolKeeper';
        }

        try {
            var roleBadge = document.getElementById('userRoleBadge');
            var storedRole = localStorage.getItem('userRole');
            if (roleBadge && storedRole) roleBadge.textContent = formatRole(storedRole);
        } catch (eRole) { /* ignore */ }

        updateTextDirSelectUi();
        applyTextDir();
        updateLangToggleUi();
    }

    function updateLangToggleUi() {
        var btn = document.getElementById('langToggleBtn');
        if (!btn) return;
        var lang = getLang();
        btn.textContent = lang === 'uk' ? 'English' : 'Українська';
        btn.setAttribute('aria-pressed', lang === 'en' ? 'true' : 'false');
        btn.setAttribute('aria-label', lang === 'uk' ? 'Switch interface to English' : 'Перемкнути інтерфейс на українську');
    }

    function init() {
        apply();
        var btn = document.getElementById('langToggleBtn');
        if (btn && !btn.getAttribute('data-i18n-listener')) {
            btn.setAttribute('data-i18n-listener', '1');
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                toggleLang();
            });
        }
        var dirSel = document.getElementById('textDirSelect');
        if (dirSel && !dirSel.getAttribute('data-text-dir-listener')) {
            dirSel.setAttribute('data-text-dir-listener', '1');
            dirSel.addEventListener('change', function () {
                setTextDir(dirSel.value);
            });
        }
        document.addEventListener('focusin', function (e) {
            var t = e.target;
            if (t && t.nodeType === 1 && typeof t.matches === 'function' && shouldApplyDirToField(t)) {
                applyTextDirToElement(t);
            }
        }, true);
        setupTextDirObserver();
    }

    window.SchoolKeeperI18n = {
        getLang: getLang,
        setLang: setLang,
        toggleLang: toggleLang,
        t: t,
        apply: apply,
        formatRole: formatRole,
        getTextDir: getTextDir,
        setTextDir: setTextDir,
        applyTextDir: applyTextDir
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
