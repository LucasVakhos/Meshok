using HtmlAgilityPack;
using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using System.Resources;
using System.Text;

namespace NewsWave.NewsMaker;

public sealed record NewsletterArchive(string FileName, byte[] Bytes);

public sealed class NewsletterArchiveBuilder
{
    private static readonly string[] Columns =
        ["Here", "Barcode", "Artist", "Title", "Year_of", "Media", "Label_name", "Origin", "Genere", "Quality"];
    private readonly IWebHostEnvironment _environment;
    private readonly NewsMakerSettingsStore _settings;

    public NewsletterArchiveBuilder(IWebHostEnvironment environment, NewsMakerSettingsStore settings)
    {
        _environment = environment;
        _settings = settings;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public NewsletterArchive Build(DataTable table, DateTime date)
    {
        NewsMakerSettings settings = _settings.Current;
        string directory = Path.IsPathRooted(settings.ExportPath)
            ? settings.ExportPath
            : Path.Combine(_environment.ContentRootPath, settings.ExportPath);
        Directory.CreateDirectory(directory);
        string baseName = $"news{date:d}".Replace('.', '_');
        string excelPath = Path.Combine(directory, baseName + ".xls");
        File.WriteAllText(excelPath, BuildExcelHtml(table), Encoding.UTF8);

        string zipName = baseName + ".zip";
        using MemoryStream output = new();
        using (ZipArchive archive = new(output, ZipArchiveMode.Create, true))
        {
            ZipArchiveEntry entry = archive.CreateEntry(Path.GetFileName(excelPath), CompressionLevel.Optimal);
            using Stream target = entry.Open();
            using FileStream source = File.OpenRead(excelPath);
            source.CopyTo(target);
        }
        return new(zipName, output.ToArray());
    }

    public (string Text, string Html) Personalize(string name, string unsubscribeUrl)
    {
        string html = ReadTemplate();
        HtmlDocument document = new();
        document.LoadHtml(html);
        StringBuilder text = new();
        foreach (HtmlNode paragraph in document.DocumentNode.SelectNodes("//p") ?? Enumerable.Empty<HtmlNode>())
        {
            string value = WebUtility.HtmlDecode(paragraph.InnerText).Trim();
            if (!string.IsNullOrWhiteSpace(value))
                text.AppendLine(value).AppendLine();
        }
        return (Replace(text.ToString().Trim(), name, unsubscribeUrl, false),
                Replace(html, name, unsubscribeUrl, true));
    }

    private string Replace(string source, string name, string unsubscribeUrl, bool html)
    {
        PostSettings post = _settings.Current.Post;
        string link = html
            ? $"<a href=\"{WebUtility.HtmlEncode(unsubscribeUrl)}\">этой ссылке</a>"
            : $"этой ссылке: {unsubscribeUrl}";
        return source
            .Replace("#test_label", string.Empty)
            .Replace("#hello_name", name)
            .Replace("#unsubscribe_url", link)
            .Replace("#contact_email", post.BridgeEmail ?? string.Empty)
            .Replace("#contact_phone", post.ContactPhone ?? string.Empty)
            .Trim();
    }

    private static string BuildExcelHtml(DataTable table)
    {
        string[] columns = Columns.Where(table.Columns.Contains).ToArray();
        StringBuilder html = new();
        html.AppendLine("<html><head><meta charset=\"utf-8\"><style>table{border-collapse:collapse;font-family:Arial}th{background:#c0504d;color:white;font-weight:bold}th,td{border:1px solid #bbb;padding:4px}tr:nth-child(even){background:#eaf2f8}</style></head><body><table>");
        html.Append("<tr>");
        foreach (string column in columns)
            html.Append("<th>").Append(WebUtility.HtmlEncode(table.Columns[column]!.Caption)).Append("</th>");
        html.AppendLine("</tr>");
        foreach (DataRow row in table.Rows)
        {
            html.Append("<tr>");
            string link = Convert.ToString(row.ItemArray.LastOrDefault()) ?? string.Empty;
            foreach (string column in columns)
            {
                string value = Convert.ToString(row[column], CultureInfo.CurrentCulture) ?? string.Empty;
                html.Append("<td>");
                if (column == "Title" && Uri.TryCreate(link, UriKind.Absolute, out _))
                    html.Append("<a href=\"").Append(WebUtility.HtmlEncode(link)).Append("\">").Append(WebUtility.HtmlEncode(value)).Append("</a>");
                else
                    html.Append(WebUtility.HtmlEncode(value));
                html.Append("</td>");
            }
            html.AppendLine("</tr>");
        }
        html.AppendLine("</table></body></html>");
        return html.ToString();
    }

    private static string ReadTemplate()
    {
        ResourceManager resources = new("NewsWave.NewsMakerResources", typeof(NewsletterArchiveBuilder).Assembly);
        return resources.GetString("html_code") ?? throw new InvalidOperationException("В ресурсах NewsMaker не найден html_code.");
    }
}

public sealed class NewsletterSmtpSender
{
    private readonly NewsMakerSettingsStore _settings;
    public NewsletterSmtpSender(NewsMakerSettingsStore settings) => _settings = settings;

    public async Task SendNewsletterAsync(
        BridgeRecipient recipient,
        NewsletterArchive archive,
        string text,
        string html,
        CancellationToken token)
    {
        PostSettings post = _settings.Current.Post;
        using MailMessage message = new()
        {
            From = new MailAddress(post.BridgeEmail ?? string.Empty, "Bridgenote.com", Encoding.UTF8),
            Subject = "Обновления на Bridgenote для " + recipient.Name,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };
        message.To.Add(new MailAddress(recipient.Email, recipient.Name, Encoding.UTF8));
        message.Headers.Add("X-Idempotency-Key", recipient.IdempotencyKey);
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, Encoding.UTF8, "text/plain"));
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, "text/html"));
        message.Attachments.Add(new Attachment(new MemoryStream(archive.Bytes, false), archive.FileName, "application/zip"));
        await SendAsync(message, post, token);
    }

    public async Task SendReportAsync(string report, CancellationToken token)
    {
        PostSettings post = _settings.Current.Post;
        if (string.IsNullOrWhiteSpace(post.DeveloperEmail))
            throw new InvalidOperationException("Не задан адрес для команды «Отправить мне».");
        using MailMessage message = new()
        {
            From = new MailAddress(post.BridgeEmail ?? string.Empty, "BridgeNote.com", Encoding.UTF8),
            Subject = "NewsWave - bug report",
            Body = report,
            BodyEncoding = Encoding.UTF8
        };
        message.To.Add(post.DeveloperEmail);
        await SendAsync(message, post, token);
    }

    private static async Task SendAsync(MailMessage message, PostSettings post, CancellationToken token)
    {
        if (!post.IsConfigured)
            throw new InvalidOperationException("Почтовый сервер не настроен.");
        using SmtpClient client = new(post.Smtp ?? string.Empty, post.Port)
        {
            EnableSsl = post.UseSSL,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };
        if (!string.IsNullOrWhiteSpace(post.User))
            client.Credentials = new NetworkCredential(post.User, post.PassWrd ?? string.Empty);
        await client.SendMailAsync(message, token);
    }
}
