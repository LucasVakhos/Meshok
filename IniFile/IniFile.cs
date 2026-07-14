using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AppCleaner;

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
            return Path.Combine(AppContext.BaseDirectory, $"{name}.ini");
        }
    }

    public string FilePath => _filePath;

    public static IniFile DefaultInstance() => Default.Value;

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

    private static IEnumerable<PropertyInfo> GetSavedProperties(Type type) => type
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(x => x.CanRead && x.CanWrite)
        .Where(x => x.GetCustomAttribute<SavedAttribute>() is not null)
        .Where(x => IsSupportedIniType(x.PropertyType));

    private static bool IsSupportedIniType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(string) || type == typeof(int) || type == typeof(bool) ||
               type == typeof(long) || type == typeof(double) || type == typeof(decimal) ||
               type == typeof(List<string>) || type.IsEnum;
    }

    private static string ConvertToIniString(object? value) => value switch
    {
        null => string.Empty,
        List<string> list => string.Join("|", NormalizeStringList(list)),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static object? ConvertFromString(string text, Type targetType)
    {
        var nullableType = Nullable.GetUnderlyingType(targetType);
        var realType = nullableType ?? targetType;
        if (nullableType != null && string.IsNullOrWhiteSpace(text)) return null;
        if (realType == typeof(string)) return text;
        if (realType == typeof(List<string>)) return ParseStringList(text);
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
