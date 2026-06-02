using System.Text.RegularExpressions;

namespace AppCleaner
{
    public partial class FileScanner
    {
        private async Task NormalizeMethodSignaturesFolderAsync(CancellationToken cancellationToken)
        {
            var files = Directory
                .EnumerateFiles(_store.SearchFolder, "*.cs", SearchOption.AllDirectories)
                .Where(file => !IsDesignerFile(file))
                .ToArray();

            _store.SetProgressMaximum(files.Length);

            AddToLog($"Файлов для нормализации сигнатур: {files.Length}");

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (await NormalizeMethodSignaturesFileAsync(file, cancellationToken))
                        AddToLog($"[Исправлен] {file}");
                    else
                        AddToLog($"[Пропуск] {file}");
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    AddToLog($"[Ошибка] {file} - {ex.Message}");
                }
                finally
                {
                    CountProcessedFile(file);
                }
            }

            AddToLog("Нормализация сигнатур завершена.");
        }

        private async Task<bool> NormalizeMethodSignaturesFileAsync(string filePath, CancellationToken cancellationToken)
        {
            var encoding = DetectFileEncoding(filePath);

            var source = await File.ReadAllTextAsync(filePath, encoding, cancellationToken);
            var normalized = NormalizeMethodSignatures(source);

            if (source == normalized)
                return false;

            CreateBackup(filePath);

            await File.WriteAllTextAsync(filePath, normalized, encoding, cancellationToken);

            return true;
        }

        private static string NormalizeMethodSignatures(string source)
        {
            return Regex.Replace(
                source,
                @"(?<sig>(?:public|private|protected|internal)(?:\s+static)?(?:\s+async)?\s+[^{;\r\n]+\()(?<params>[\s\S]*?)(?<end>\)\s*(?:=>|\{))",
                match =>
                {
                    var signature = match.Groups["sig"].Value;
                    var parameters = match.Groups["params"].Value;
                    var end = match.Groups["end"].Value;

                    parameters = Regex.Replace(parameters, @"\s*\r?\n\s*", " ");
                    parameters = Regex.Replace(parameters, @"\s{2,}", " ").Trim();

                    return $"{signature}{parameters}{end}";
                },
                RegexOptions.Multiline);
        }
    }
}