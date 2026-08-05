using System.ComponentModel.DataAnnotations;

namespace NewsWave.Configuration;

/// <summary>
/// Настройки программы рассылки NewsMaker
/// </summary>
public class NewsMakerOptions
{
    public const string SectionName = "NewsMaker";

    /// <summary>
    /// Настройки расписания запуска
    /// </summary>
    [Required]
    public ProgramOptions Program { get; set; } = new();

    /// <summary>
    /// Настройки подключения к базе данных BridgeNote
    /// </summary>
    [Required]
    public BridgeNoteOptions BridgeNote { get; set; } = new();

    /// <summary>
    /// Настройки SMTP сервера для отправки почты
    /// </summary>
    [Required]
    public SmtpOptions Post { get; set; } = new();

    /// <summary>
    /// Лимит отправки писем за один запуск
    /// </summary>
    [Range(1, 10000)]
    public int SendLimit { get; set; } = 10;

    /// <summary>
    /// Путь для экспорта файлов
    /// </summary>
    [Required]
    public string ExportPath { get; set; } = "App_Data/exports";
}

public class ProgramOptions
{
    /// <summary>
    /// День недели для автоматического запуска (1-7, где 7 = воскресенье)
    /// </summary>
    [Range(1, 7)]
    public int RunDay { get; set; } = 7;

    /// <summary>
    /// Время автоматического запуска
    /// </summary>
    public TimeSpan RunTime { get; set; } = TimeSpan.Parse("18:00:00");
}

public class BridgeNoteOptions
{
    /// <summary>
    /// Адрес MySQL сервера
    /// </summary>
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Имя базы данных
    /// </summary>
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string UserID { get; set; } = string.Empty;

    /// <summary>
    /// Пароль (из User Secrets или Environment Variables)
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Порт MySQL сервера
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 3306;

    /// <summary>
    /// Кодировка соединения
    /// </summary>
    public string CharacterSet { get; set; } = "utf8";

    /// <summary>
    /// Протокол соединения
    /// </summary>
    public string ConnectionProtocol { get; set; } = "Tcp";

    /// <summary>
    /// Режим SSL
    /// </summary>
    public string SslMode { get; set; } = "None";

    /// <summary>
    /// Проверяет, настроено ли подключение
    /// </summary>
    public bool IsConfigured => 
        !string.IsNullOrWhiteSpace(Server) && 
        !string.IsNullOrWhiteSpace(Database) &&
        !string.IsNullOrWhiteSpace(UserID);

    /// <summary>
    /// Строка подключения к MySQL
    /// </summary>
    public string ConnectionString =>
        $"Server={Server};Port={Port};Database={Database};Uid={UserID};Pwd={Password};" +
        $"CharSet={CharacterSet};ConnectionProtocol={ConnectionProtocol};SslMode={SslMode}";
}

public class SmtpOptions
{
    /// <summary>
    /// SMTP сервер
    /// </summary>
    public string Smtp { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя SMTP
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Пароль SMTP (из User Secrets или Environment Variables)
    /// </summary>
    public string PassWrd { get; set; } = string.Empty;

    /// <summary>
    /// Email отправителя (BridgeNote)
    /// </summary>
    [EmailAddress]
    public string BridgeEmail { get; set; } = string.Empty;

    /// <summary>
    /// Контактный телефон
    /// </summary>
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>
    /// Порт SMTP
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 25;

    /// <summary>
    /// Использовать SSL
    /// </summary>
    public bool UseSSL { get; set; } = true;

    /// <summary>
    /// Email разработчика для тестовых отправок
    /// </summary>
    [EmailAddress]
    public string DeveloperEmail { get; set; } = string.Empty;

    /// <summary>
    /// Проверяет, настроен ли SMTP
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Smtp) &&
        !string.IsNullOrWhiteSpace(User) &&
        !string.IsNullOrWhiteSpace(BridgeEmail);
}
