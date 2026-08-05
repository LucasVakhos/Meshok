# Meshok Solution

Комплексное решение для управления интернет-магазином, включающее desktop и web приложения.

## 📦 Состав решения

### [MehokBrowser](MehokBrowser/) 
**Основное Windows desktop приложение для управления заказами и клиентами**

- ✨ Модернизировано на .NET 8
- 🏗️ Generic Host + Dependency Injection
- ⚡ Async/await поддержка
- 📝 Structured logging
- ✅ Unit тесты

📚 **Документация**:
- [Обзор модернизации](MehokBrowser/MODERNIZATION.md)
- [Руководство по миграции](MehokBrowser/MIGRATION_GUIDE.md)
- [Примеры кода](MehokBrowser/EXAMPLES.md)
- [Итоговый отчёт](MehokBrowser/SUMMARY.md)

### [NewsWave](NewsWave/)
**Web-версия NewsMaker на ASP.NET Core и DevExtreme**

- Рассылка новостей
- Управление подписчиками
- Excel экспорт
- SMTP интеграция

### [LB.Libs](LB.Libs/)
**Базовая библиотека инфраструктуры**

- Legacy framework classes
- Configuration management
- Database abstractions
- Common utilities

### NewsMaker
**Legacy Windows приложение для email рассылок**

## 🚀 Быстрый старт

### Требования
- .NET 8.0 SDK
- Windows 10/11
- Firebird или MySQL
- Visual Studio 2022 (рекомендуется)

### Установка

```bash
# Клонирование
git clone https://github.com/LucasVakhos/Meshok.git
cd Meshok

# Восстановление зависимостей
dotnet restore

# Сборка всего решения
dotnet build

# Запуск MehokBrowser
dotnet run --project MehokBrowser

# Запуск NewsWave
cd NewsWave
npm install
dotnet run
```

### Конфигурация

Настройте connection strings в `MehokBrowser/appsettings.json`:

```json
{
  "ConnectionStrings": {
	"Firebird": "DataSource=localhost;Port=3050;Database=path/to/db.fdb;...",
	"MySQL": "Server=localhost;Database=db;Uid=user;Pwd=password;"
  }
}
```

## 📊 Статус проектов

| Проект | Технология | .NET Version | Статус |
|--------|-----------|-------------|---------|
| **MehokBrowser** | WinForms | .NET 8.0 | ✅ Модернизирован |
| **NewsWave** | ASP.NET Core + Razor Pages | .NET 8.0 | ✅ Актуален |
| **LB.Libs** | Class Library | .NET 8.0 | ✅ Стабилен |
| **NewsMaker** | WinForms (legacy) | .NET Framework | ⚠️ Legacy |

## 🎯 Основные возможности

### MehokBrowser
- 📋 Управление заказами
- 👥 Управление клиентами
- 📊 Отчёты и статистика
- 🔄 Синхронизация с сайтами
- 📨 Автоматические уведомления
- 📦 Управление лотами

### NewsWave
- 📧 Email рассылки
- 👥 Управление подписчиками
- 📊 Статистика рассылок
- ⏰ Автоматические отправки
- 📝 Шаблоны сообщений

## 🧪 Тестирование

```bash
# Все тесты
dotnet test

# Только MehokBrowser tests
dotnet test MehokBrowser.Tests

# С покрытием кода
dotnet test --collect:"XPlat Code Coverage"
```

## 📚 Документация

### MehokBrowser (Основная)
- [📖 Модернизация MehokBrowser](MehokBrowser/MODERNIZATION.md) - полное описание изменений
- [🔄 Migration Guide](MehokBrowser/MIGRATION_GUIDE.md) - как использовать новые возможности
- [💡 Examples](MehokBrowser/EXAMPLES.md) - практические примеры кода
- [📝 Summary](MehokBrowser/SUMMARY.md) - итоговый отчёт

### NewsWave
- [📖 README](NewsWave/README.md) - описание и инструкции

## 🛠️ Разработка

### Структура

```
Meshok/
├── MehokBrowser/           # Desktop приложение
│   ├── Application/        # App infrastructure
│   ├── Data/              # Data access
│   ├── Forms/             # WinForms UI
│   ├── Services/          # Business services
│   └── appsettings.json   # Configuration
├── MehokBrowser.Tests/     # Unit tests
├── NewsWave/              # Web application
├── LB.Libs/               # Shared library
└── NewsMaker/             # Legacy app
```

### Технологический стек

#### MehokBrowser
- **Framework**: .NET 8.0 WinForms
- **DI**: Microsoft.Extensions.DependencyInjection
- **Logging**: Microsoft.Extensions.Logging + Log4Net
- **Configuration**: Microsoft.Extensions.Configuration
- **Data Access**: Dapper
- **Database**: Firebird, MySQL
- **UI**: DevExpress WinForms
- **Testing**: xUnit, FluentAssertions, Moq

#### NewsWave
- **Framework**: ASP.NET Core 8.0
- **UI**: Razor Pages + DevExtreme
- **Database**: Firebird
- **Email**: SMTP

#### LB.Libs
- **Framework**: .NET 8.0
- **Type**: Class Library

## 🔐 Безопасность

### Connection Strings

❌ **Не коммитить** production connection strings в Git!

✅ **Используйте**:
- `appsettings.Development.json` для dev (добавлен в .gitignore)
- Environment Variables для prod
- Azure Key Vault для cloud

```bash
# Пример Environment Variables
set ConnectionStrings__Firebird="..."
set ConnectionStrings__MySQL="..."
```

## 🤝 Вклад в проект

1. Fork проект
2. Создайте feature branch (`git checkout -b feature/Feature`)
3. Commit изменения (`git commit -m 'Add Feature'`)
4. Push в branch (`git push origin feature/Feature`)
5. Откройте Pull Request

### Стандарты кода
- Async/await для I/O операций
- Unit тесты для новой функциональности
- XML комментарии для public API
- Следуйте существующему code style

## 📝 История версий

### 2.0.0 (Апрель 2026) - MehokBrowser Modernization
- ✨ .NET 8 модернизация
- ✨ Generic Host + DI
- ✨ Async/await support
- ✨ Modern logging
- ✨ Unit tests
- ✅ Обратная совместимость

### 1.x - Initial Release
- Базовая функциональность
- WinForms desktop app
- Firebird/MySQL support

## 📄 Лицензия

MIT License - см. [LICENSE](LICENSE) для деталей.

## 👥 Авторы

- **LucasVakhos** - [GitHub](https://github.com/LucasVakhos)

## 🔗 Полезные ссылки

- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [DevExpress WinForms](https://www.devexpress.com/products/net/controls/winforms/)
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)
- [Firebird](https://firebirdsql.org/)

---

**⚡ Powered by .NET 8 | 🎨 Enterprise Ready | 💼 Production Proven**
