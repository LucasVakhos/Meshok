using DevExpress.Export.Xl;
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
        WriteExcel(table, excelPath);

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

    private static void WriteExcel(DataTable table, string path)
    {
        (string Name, int Length, bool Link)[] columns =
        [
            ("Here", 10, false),
            ("Barcode", 13, false),
            ("Artist", 80, false),
            ("Title", 80, true),
            ("Year_of", 4, false),
            ("Media", 10, false),
            ("Label_name", 20, false),
            ("Origin", 5, false),
            ("Genere", 20, false),
            ("Quality", 10, false)
        ];
        var selected = columns.Where(x => table.Columns.Contains(x.Name)).ToArray();
        IXlExporter exporter = XlExport.CreateExporter(XlDocumentFormat.Xls);
        using FileStream stream = new(path, FileMode.Create, FileAccess.ReadWrite);
        using IXlDocument document = exporter.CreateDocument(stream);
        using IXlSheet sheet = document.CreateSheet();
        sheet.Name = table.TableName;

        List<XlCellFormatting> rowFormats = [];
        for (int index = 0; index < 3; index++)
        {
            XlCellFormatting formatting = new();
            formatting.Font = new XlFont { Name = "Arial Cyr", SchemeStyle = XlFontSchemeStyles.None };
            rowFormats.Add(formatting);
        }
        rowFormats[0].Fill = XlFill.SolidFill(XlColor.FromTheme(XlThemeColor.Accent4, 0.7));
        rowFormats[1].Fill = XlFill.SolidFill(XlColor.FromTheme(XlThemeColor.Accent6, 0.8));

        XlCellFormatting header = new();
        header.CopyFrom(rowFormats[2]);
        header.Font.Bold = true;
        header.Font.Color = XlColor.FromTheme(XlThemeColor.Light1, 0.0);
        header.Fill = XlFill.SolidFill(XlColor.FromTheme(XlThemeColor.Accent2, 0.0));

        foreach (var columnInfo in selected)
        {
            using IXlColumn column = sheet.CreateColumn();
            column.WidthInPixels = Math.Min(255, columnInfo.Length * 10);
            DataColumn dataColumn = table.Columns[columnInfo.Name]!;
            if (dataColumn.DataType == typeof(int))
                column.Formatting = new XlCellFormatting { NumberFormat = "#,##0" };
            else if (dataColumn.DataType == typeof(double))
                column.Formatting = new XlCellFormatting { NumberFormat = "#,##0.00" };
            else if (dataColumn.DataType == typeof(DateTime))
                column.Formatting = new XlCellFormatting
                {
                    NumberFormat = "dd.mm.yyyy",
                    Alignment = new XlCellAlignment { HorizontalAlignment = XlHorizontalAlignment.Left }
                };
        }

        using (IXlRow row = sheet.CreateRow())
        {
            foreach (var columnInfo in selected)
            {
                using IXlCell cell = row.CreateCell();
                cell.Value = table.Columns[columnInfo.Name]!.Caption;
                cell.ApplyFormatting(header);
            }
        }

        foreach (DataRow dataRow in table.Rows)
        {
            object?[] values = dataRow.ItemArray;
            string link = Convert.ToString(values[^1]) ?? string.Empty;
            int formatIndex = Convert.ToInt32(values[0]);
            using IXlRow row = sheet.CreateRow();
            foreach (var columnInfo in selected)
            {
                using IXlCell cell = row.CreateCell();
                cell.Value = XlVariantValue.FromObject(dataRow[columnInfo.Name]);
                cell.ApplyFormatting(rowFormats[formatIndex]);
                if (columnInfo.Link)
                {
                    cell.Formatting.Font.Underline = XlUnderlineType.Single;
                    cell.Formatting.Font.Color = XlColor.FromArgb(0, 0, 255);
                    sheet.Hyperlinks.Add(new XlHyperlink
                    {
                        DisplayText = cell.Value.TextValue,
                        Reference = new XlCellRange(cell.Position),
                        TargetUri = link,
                        Tooltip = "Перейти на сайт"
                    });
                }
            }
        }
        sheet.AutoFilterRange = sheet.DataRange;
        sheet.SplitPosition = new XlCellPosition(0, 1);
    }

    private static string ReadTemplate()    {
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
