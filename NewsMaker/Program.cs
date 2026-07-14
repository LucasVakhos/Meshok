using System;
namespace NewsMaker
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            // Первый запуск собирает старые разрозненные INI в один файл рядом с exe.
            AppCleaner.IniFile.MigrateLegacyFiles();
            AppContextNM.RunInstance();
        }
    }
}
