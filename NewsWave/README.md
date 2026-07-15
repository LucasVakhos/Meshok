# NewsWave

Веб-продолжение NewsMaker на ASP.NET Core и DevExtreme.

## Возможности

- почтовая очередь с персональной отправкой каждому адресату;
- постоянная адресная книга с группами и признаком активности;
- библиотека текстовых и HTML-шаблонов;
- история рассылок, сохраняемая между перезапусками;
- быстрый перенос активных контактов и шаблонов в редактор письма.

## Запуск

```powershell
cd NewsWave
npm install
dotnet run
```

`npm install` устанавливает зафиксированный DevExtreme 25.2.7 и готовит локальные клиентские файлы.

## Почта

SMTP настраивается через секцию `Mail` в `appsettings.json`. Секреты удобнее передавать
переменными окружения:

```powershell
$env:Mail__Host = "smtp.example.ru"
$env:Mail__Port = "587"
$env:Mail__UseSsl = "true"
$env:Mail__UserName = "sender@example.ru"
$env:Mail__Password = "<пароль приложения>"
$env:Mail__FromAddress = "sender@example.ru"
$env:Mail__FromName = "NewsWave"
dotnet run --project NewsWave
```

Получатели обрабатываются по одному, поэтому их адреса не раскрываются другим адресатам.
Контакты, шаблоны и последние 200 отправок сохраняются в `App_Data/newswave-data.json`.
Прерванная перезапуском рассылка отмечается ошибкой и не отправляется повторно автоматически.
