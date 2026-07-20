using DevExpress.Export.Xl;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Text;

namespace CsvToExcel;

public sealed record ConversionResult(int Rows, int Columns, string EncodingName);

public static class CsvExcelConverter
{
    private static readonly IReadOnlyDictionary<string, int> RussianMonths =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["янв"] = 1,
            ["фев"] = 2,
            ["мар"] = 3,
            ["апр"] = 4,
            ["май"] = 5,
            ["июн"] = 6,
            ["июл"] = 7,
            ["авг"] = 8,
            ["сен"] = 9,
            ["сент"] = 9,
            ["окт"] = 10,
            ["ноя"] = 11,
            ["дек"] = 12
        };

    public static ConversionResult Convert(string csvPath, string xlsxPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(csvPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(xlsxPath);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding = DetectEncoding(csvPath);
        string temporaryPath = xlsxPath + ".tmp";

        try
        {
            using TextFieldParser parser = new(csvPath, encoding, detectEncoding: true)
            {
                TextFieldType = FieldType.Delimited,
                HasFieldsEnclosedInQuotes = true,
                TrimWhiteSpace = false
            };
            parser.SetDelimiters(";");

            string[]? rawHeaders = parser.ReadFields();
            if (rawHeaders is null || rawHeaders.Length == 0)
                throw new InvalidDataException("CSV-файл не содержит заголовков.");

            string[] headers = RemoveTrailingEmptyColumn(rawHeaders);
            if (headers.Length == 0)
                throw new InvalidDataException("В CSV-файле не найдены столбцы.");

            IXlExporter exporter = XlExport.CreateExporter(XlDocumentFormat.Xlsx);
            using FileStream output = new(temporaryPath, FileMode.Create, FileAccess.ReadWrite);
            using IXlDocument document = exporter.CreateDocument(output);
            using IXlSheet sheet = document.CreateSheet();
            sheet.Name = CreateSheetName(Path.GetFileNameWithoutExtension(csvPath));

            CreateColumns(sheet, headers);
            WriteHeader(sheet, headers);

            int rowCount = 0;
            while (!parser.EndOfData)
            {
                string[]? fields = parser.ReadFields();
                if (fields is null || fields.All(string.IsNullOrEmpty))
                    continue;

                WriteRow(sheet, fields, headers);
                rowCount++;
            }

            sheet.AutoFilterRange = sheet.DataRange;
            sheet.SplitPosition = new XlCellPosition(0, 1);
            document.Dispose();
            output.Dispose();

            File.Move(temporaryPath, xlsxPath, true);
            return new ConversionResult(rowCount, headers.Length, encoding.EncodingName);
        }
        catch (MalformedLineException ex)
        {
            throw new InvalidDataException(
                $"Ошибка структуры CSV в строке {ex.LineNumber}. Проверьте кавычки и разделители.", ex);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private static void CreateColumns(IXlSheet sheet, IReadOnlyList<string> headers)
    {
        foreach (string header in headers)
        {
            using IXlColumn column = sheet.CreateColumn();
            int width = Math.Clamp((header.Length + 3) * 9, 90, 240);
            column.WidthInPixels = width;
        }
    }

    private static void WriteHeader(IXlSheet sheet, IReadOnlyList<string> headers)
    {
        XlCellFormatting formatting = new()
        {
            Fill = XlFill.SolidFill(XlColor.FromArgb(10, 127, 120)),
            Font = new XlFont
            {
                Bold = true,
                Color = XlColor.FromArgb(255, 255, 255)
            },
            Alignment = new XlCellAlignment
            {
                VerticalAlignment = XlVerticalAlignment.Center,
                WrapText = true
            }
        };

        using IXlRow row = sheet.CreateRow();
        row.HeightInPixels = 34;
        foreach (string header in headers)
        {
            using IXlCell cell = row.CreateCell();
            cell.Value = header.Trim();
            cell.ApplyFormatting(formatting);
        }
    }

    private static void WriteRow(IXlSheet sheet, IReadOnlyList<string> fields, int columnCount)
    {
        using IXlRow row = sheet.CreateRow();
        for (int index = 0; index < columnCount; index++)
        {
            using IXlCell cell = row.CreateCell();
            cell.Value = index < fields.Count ? fields[index] : string.Empty;
        }
    }

    private static string[] RemoveTrailingEmptyColumn(string[] fields)
    {
        int length = fields.Length;
        while (length > 0 && string.IsNullOrEmpty(fields[length - 1]))
            length--;
        return fields.Take(length).ToArray();
    }

    private static string CreateSheetName(string source)
    {
        char[] invalid = ['[', ']', ':', '*', '?', '/', '\\'];
        string value = string.Concat(source.Select(character => invalid.Contains(character) ? '_' : character)).Trim();
        if (string.IsNullOrWhiteSpace(value))
            value = "Данные";
        return value.Length <= 31 ? value : value[..31];
    }

    private static Encoding DetectEncoding(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode;
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return Encoding.BigEndianUnicode;

        try
        {
            _ = new UTF8Encoding(false, true).GetString(bytes);
            return new UTF8Encoding(false);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.GetEncoding(1251);
        }
    }
}
