namespace AppCleaner
{
    public partial class FileScanner
    {
        private async Task RestoreCSharpFilesFromBakFolderAsync(CancellationToken cancellationToken)
        {
            var backupFiles = Directory
                .EnumerateFiles(_store.SearchFolder, "*.cs.bak", SearchOption.AllDirectories)
                .Where(file => !IsDesignerFile(file))
                .GroupBy(GetTargetFilePathFromBak)
                .Select(group => group.OrderByDescending(File.GetCreationTimeUtc).First())
                .ToArray();

            _store.SetProgressMaximum(backupFiles.Length);
            AddToLog($"Файлов .cs.bak для восстановления: {backupFiles.Length}");

            foreach (var backupFile in backupFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (await RestoreCSharpFileFromBakAsync(backupFile, cancellationToken))
                        AddToLog($"[Восстановлен] {backupFile}");
                    else
                        AddToLog($"[Пропуск] {backupFile}");
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    AddToLog($"[Ошибка восстановления] {backupFile} - {ex.Message}");
                }
                finally
                {
                    CountProcessedFile(backupFile);
                }
            }

            AddToLog("Восстановление .cs из последнего по времени создания .bak завершено.");
        }

        private async Task<bool> RestoreCSharpFileFromBakAsync(string backupFilePath, CancellationToken cancellationToken)
        {
            if (!backupFilePath.EndsWith(".cs.bak", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!File.Exists(backupFilePath))
                return false;

            var targetFilePath = GetTargetFilePathFromBak(backupFilePath);
            var encoding = DetectFileEncoding(backupFilePath);
            var backupSource = await File.ReadAllTextAsync(backupFilePath, encoding, cancellationToken);

            await File.WriteAllTextAsync(targetFilePath, backupSource, encoding, cancellationToken);

            File.Delete(backupFilePath);

            return true;
        }

        private static string GetTargetFilePathFromBak(string backupFilePath)
        {
            return backupFilePath[..^4];
        }
    }
}