using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace LB.Libs;

[AttributeUsage(AttributeTargets.Property)]
public sealed class SavedAttribute : Attribute
{
}

/// <summary>
/// A process-wide INI store. By default every executable uses one file next to
/// itself: &lt;executable-name&gt;.ini.
/// </summary>
public sealed class IniFile
{
    private static readonly Lazy<IniFile> Default = new(() => new IniFile(DefaultFilePath));
    private readonly object _sync = new();
    private readonly string _filePath;
    private readonly Dictionary<string, Dictionary<string, string>> _sections =
        new(StringComparer.OrdinalIgnoreCase);

    public IniFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = Path.GetFullPath(filePath);
        Load();
    }

    public static string DefaultFilePath
    {
        get
        {
            string entryPath = Assembly.GetEntryAssembly()?.Location ?? string.Empty;
            string processPath = Environment.ProcessPath ?? AppDomain.CurrentDomain.FriendlyName;
            string name = Path.GetFileNameWithoutExtension(
                string.IsNullOrEmpty(entryPath) ? processPath : entryPath);
            return Path.Combine(System.AppContext.BaseDirectory, $"{name}.ini");
        }
    }

    public string FilePath => _filePath;

    public static IniFile DefaultInstance() => Default.Value;

    /// <summary>
    /// On first application start imports every legacy INI/JSON configuration
    /// below the executable directory into the single default INI file.
    /// Source files are kept intact; an old JSON file occupying the target path
    /// is copied to a .legacy backup before conversion.
    /// </summary>
    public static void MigrateLegacyFiles()
    {
        lock (Default.Value._sync)
        {
            string targetPath = DefaultFilePath;
            string? legacyTargetJson = null;

            if (File.Exists(targetPath))
            {
                string existingText = File.ReadAllText(targetPath, Encoding.UTF8).Trim();
                if (existingText.StartsWith('{'))
                {
                    legacyTargetJson = existingText;
                    File.Copy(targetPath, targetPath + ".legacy", true);
                    File.Delete(targetPath);
                }
            }

            IniFile target = DefaultInstance();

            if (!string.IsNullOrEmpty(legacyTargetJson))
                WriteJsonFields(target, "CfgApp", legacyTargetJson);

            foreach (string sourcePath in EnumerateLegacyIniFiles(System.AppContext.BaseDirectory)
                         .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                if (Path.GetFullPath(sourcePath).Equals(Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    string text = File.ReadAllText(sourcePath, Encoding.UTF8).Trim();
                    if (string.IsNullOrEmpty(text))
                        continue;

                    if (text.StartsWith('{'))
                    {
                        string section = Path.GetFileNameWithoutExtension(sourcePath);
                        WriteJsonFields(target, section, text);
                    }
                    else
                    {
                        IniFile source = new IniFile(sourcePath);
                        foreach (var section in source._sections)
                            foreach (var pair in section.Value)
                                target.WriteIfMissing(section.Key, pair.Key, pair.Value);
                    }
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            target.Save();
        }
    }

    private static void WriteJsonFields(IniFile target, string section, string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                string value = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString() ?? string.Empty,
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => string.Empty,
                    JsonValueKind.Number => prop.Value.GetRawText(),
                    JsonValueKind.Array => "[" + string.Join(",", prop.Value.EnumerateArray().Select(e =>
                        e.ValueKind == JsonValueKind.String ? e.GetString() ?? string.Empty : e.GetRawText())) + "]",
                    _ => prop.Value.GetRawText()
                };
                target.WriteIfMissing(section, prop.Name, value);
            }
        }
        catch
        {
            target.WriteIfMissing(section, "Json", json);
        }
    }

    public string Read(string section, string key, string defaultValue = "")
    {
        lock (_sync)
        {
            return _sections.TryGetValue(section, out var values) &&
                   values.TryGetValue(key, out var value)
                ? value
                : defaultValue;
        }
    }

    public bool TryRead(string section, string key, out string value)
    {
        lock (_sync)
        {
            if (_sections.TryGetValue(section, out var values) &&
                values.TryGetValue(key, out string? storedValue))
            {
                value = storedValue;
                return true;
            }

            value = string.Empty;
            return false;
        }
    }

    public void Write(string section, string key, object? value)
    {
        lock (_sync)
        {
            if (!_sections.TryGetValue(section, out var values))
            {
                values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _sections[section] = values;
            }

            values[key] = ConvertToIniString(value);
        }
    }

    public void Remove(string section, string key)
    {
        lock (_sync)
        {
            if (_sections.TryGetValue(section, out var values))
                values.Remove(key);
        }
    }

    private void WriteIfMissing(string section, string key, object? value)
    {
        lock (_sync)
        {
            if (_sections.TryGetValue(section, out var values) && values.ContainsKey(key))
                return;
            Write(section, key, value);
        }
    }

    public void SaveObject(object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        string section = obj.GetType().Name;

        foreach (var property in GetSavedProperties(obj.GetType()))
            Write(section, property.Name, property.GetValue(obj));

        Save();
    }

    public void LoadObject(object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        string section = obj.GetType().Name;

        foreach (var property in GetSavedProperties(obj.GetType()))
        {
            var text = Read(section, property.Name);
            if (string.IsNullOrEmpty(text))
                continue;

            try
            {
                var value = ConvertFromString(text, property.PropertyType);
                if (value is string stringValue && IsPathLikeProperty(property))
                    value = NormalizePathSeparators(stringValue);
                property.SetValue(obj, value);
            }
            catch
            {
                // A malformed user value must not prevent application startup.
            }
        }
    }

    public void Save()
    {
        lock (_sync)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var lines = new List<string>();
            foreach (var section in _sections.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                lines.Add($"[{section.Key}]");
                foreach (var pair in section.Value.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
                    lines.Add($"{pair.Key}={Escape(pair.Value)}");
                lines.Add(string.Empty);
            }

            string tempPath = _filePath + ".tmp";
            File.WriteAllLines(tempPath, lines, new UTF8Encoding(false));
            File.Move(tempPath, _filePath, true);
        }
    }

    private void Load()
    {
        lock (_sync)
        {
            if (!File.Exists(_filePath))
                return;

            string currentSection = string.Empty;
            foreach (var rawLine in File.ReadAllLines(_filePath, Encoding.UTF8))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';') || line.StartsWith('#'))
                    continue;

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    currentSection = line[1..^1].Trim();
                    if (!_sections.ContainsKey(currentSection))
                        _sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    continue;
                }

                var parts = line.Split('=', 2);
                if (parts.Length != 2 || string.IsNullOrEmpty(currentSection))
                    continue;

                if (!_sections.TryGetValue(currentSection, out var values))
                {
                    values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    _sections[currentSection] = values;
                }
                values[parts[0].Trim()] = Unescape(parts[1]);
            }
        }
    }

    private static IEnumerable<string> EnumerateLegacyIniFiles(string rootPath)
    {
        var pending = new Stack<string>();
        pending.Push(rootPath);

        while (pending.Count > 0)
        {
            string directory = pending.Pop();
            string[] files;
            string[] directories;
            try
            {
                files = Directory.GetFiles(directory, "*.ini", SearchOption.TopDirectoryOnly);
                directories = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (string file in files)
                yield return file;
            foreach (string child in directories)
                pending.Push(child);
        }
    }

    private static IEnumerable<PropertyInfo> GetSavedProperties(Type type) => type
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(x => x.CanRead && x.CanWrite)
        .Where(x => x.GetCustomAttribute<SavedAttribute>() is not null)
        .Where(x => IsSupportedIniType(x.PropertyType));

    internal static bool IsSupportedIniType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(string) || type == typeof(int) || type == typeof(bool) ||
               type == typeof(long) || type == typeof(double) || type == typeof(decimal) ||
               type == typeof(DateTime) || type == typeof(Size) || type == typeof(Point) ||
               type == typeof(List<string>) || type == typeof(Dictionary<string, string>) ||
               type.IsEnum;
    }

    internal static string ConvertToIniString(object? value) => value switch
    {
        null => string.Empty,
        List<string> list => string.Join("|", NormalizeStringList(list)),
        Dictionary<string, string> dictionary => string.Join("|", dictionary.Select(pair =>
            $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}")),
        DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
        Size size => $"{size.Width},{size.Height}",
        Point point => $"{point.X},{point.Y}",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    internal static object? ConvertFromString(string text, Type targetType)
    {
        var nullableType = Nullable.GetUnderlyingType(targetType);
        var realType = nullableType ?? targetType;
        if (nullableType != null && string.IsNullOrWhiteSpace(text)) return null;
        if (realType == typeof(string)) return text;
        if (realType == typeof(List<string>)) return ParseStringList(text);
        if (realType == typeof(Dictionary<string, string>)) return ParseStringDictionary(text);
        if (realType == typeof(DateTime))
            return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        if (realType == typeof(Size))
        {
            int[] parts = ParseIntegerPair(text);
            return new Size(parts[0], parts[1]);
        }
        if (realType == typeof(Point))
        {
            int[] parts = ParseIntegerPair(text);
            return new Point(parts[0], parts[1]);
        }
        if (realType.IsEnum)
        {
            if (Enum.TryParse(realType, text, true, out var enumValue)) return enumValue;
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                return Enum.ToObject(realType, index);
            return Activator.CreateInstance(realType);
        }
        if (realType == typeof(bool)) return bool.Parse(text);
        return Convert.ChangeType(text, realType, CultureInfo.InvariantCulture);
    }

    private static int[] ParseIntegerPair(string text)
    {
        string[] parts = text.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            throw new FormatException($"Expected two comma-separated integers, got '{text}'.");

        return
        [
            int.Parse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture),
            int.Parse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture)
        ];
    }

    private static Dictionary<string, string> ParseStringDictionary(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new Dictionary<string, string>();

        if (text.TrimStart().StartsWith('{'))
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(text) ?? new();
            }
            catch
            {
            }
        }

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string item in text.Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] pair = item.Split('=', 2);
            if (pair.Length == 2)
                result[Uri.UnescapeDataString(pair[0])] = Uri.UnescapeDataString(pair[1]);
        }
        return result;
    }

    private static List<string> ParseStringList(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();
        if (text.TrimStart().StartsWith('['))
        {
            try { return NormalizeStringList(JsonSerializer.Deserialize<List<string>>(text)); }
            catch { }
        }
        return NormalizeStringList(text.Split('|', StringSplitOptions.RemoveEmptyEntries));
    }

    private static List<string> NormalizeStringList(IEnumerable<string>? values) => values?
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Select(x => NormalizePathSeparators(x.Trim()))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList() ?? new List<string>();

    private static bool IsPathLikeProperty(PropertyInfo property) =>
        property.Name.Contains("Path", StringComparison.OrdinalIgnoreCase) ||
        property.Name.Contains("Folder", StringComparison.OrdinalIgnoreCase) ||
        property.Name.Contains("File", StringComparison.OrdinalIgnoreCase) ||
        property.Name.Contains("Directory", StringComparison.OrdinalIgnoreCase);

    private static string NormalizePathSeparators(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        bool isUnc = value.StartsWith(@"\\");
        while (value.Contains(@"\\")) value = value.Replace(@"\\", @"\");
        if (isUnc && !value.StartsWith(@"\\")) value = @"\" + value;
        return value;
    }

    public static string Escape(string value) => value
        .Replace("%", "%25")
        .Replace("\r", "%0D")
        .Replace("\n", "%0A");

    public static string Unescape(string value) => value
        .Replace("%0D", "\r", StringComparison.OrdinalIgnoreCase)
        .Replace("%0A", "\n", StringComparison.OrdinalIgnoreCase)
        .Replace("%25", "%", StringComparison.OrdinalIgnoreCase);
}
