using System.Diagnostics;
using System.IO;
using LB.Libs;

namespace MehokBrowser.Controls
{
    /// <summary>Вспомогательные методы для работы с файлами.</summary>
    public static class FileHelper
    {
        /// <summary>Открывает указанный файл через ассоциированное приложение.</summary>
        public static void OpenFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = fileName,
                    Verb = "Open",
                    WindowStyle = ProcessWindowStyle.Normal
                })?.Dispose();
            }
            catch { }
        }

        /// <summary>Открывает папку с выделением указанного файла.</summary>
        public static void OpenFolder(string fileName)
        {
            try
            {
                Process.Start("explorer", $"/n, /select, {fileName}")?.Dispose();
            }
            catch { }
        }

        /// <summary>Проверяет, сжат ли файл/папка на уровне NTFS.</summary>
        public static bool IsCompressed(string path) =>
            (File.GetAttributes(path) & FileAttributes.Compressed) == FileAttributes.Compressed;
    }
}
