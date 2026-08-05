# User Secrets Configuration

Для безопасного хранения паролей и чувствительных данных используйте User Secrets в режиме разработки.

## Инициализация User Secrets

```bash
cd NewsWave
dotnet user-secrets init
```

## Настройка паролей

### BridgeNote MySQL пароль

```bash
dotnet user-secrets set "NewsMaker:BridgeNote:Password" "your_mysql_password"
```

### SMTP пароль

```bash
dotnet user-secrets set "NewsMaker:Post:PassWrd" "your_smtp_password"
```

### Пароль администратора NewsWave (optional)

```bash
dotnet user-secrets set "NewsWave:AdminPassword" "your_admin_password"
```

## Просмотр текущих секретов

```bash
dotnet user-secrets list
```

## Удаление секрета

```bash
dotnet user-secrets remove "NewsMaker:BridgeNote:Password"
```

## Очистка всех секретов

```bash
dotnet user-secrets clear
```

## Production окружение

В production используйте:
- **Azure Key Vault** для секретов
- **Environment Variables** на сервере
- **Azure App Configuration** для централизованной конфигурации

Пример environment variables:

```bash
NewsMaker__BridgeNote__Password=production_password
NewsMaker__Post__PassWrd=smtp_password
NewsWave__AdminPassword=admin_password
```

> Обратите внимание: в environment variables используется двойное подчеркивание (`__`) вместо двоеточия (`:`).
