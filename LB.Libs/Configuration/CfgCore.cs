using LB.Libs.Utils;
using System.ComponentModel;
using System.Text.Json;

namespace LB.Libs;

/// <summary>
/// Base class for application settings stored as keys in the shared INI.
/// </summary>
public class CfgCore : AbstractEntity
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    [ThreadStatic]
    private static bool _loading;
    private bool _loaded;

    internal string ConfigPath => IniFile.DefaultFilePath;

    public CfgCore()
    {
        if (!IniHelper.TryGet(GetName(), out _))
            IniHelper.AddInstance(this);
    }

    public virtual string GetName() => GetType().Name;

    public void Load()
    {
        if (_loaded || _loading)
            return;

        _loading = true;
        try
        {
            LoadDefauls();
            IniFile.MigrateLegacyFiles();

            IniFile ini = IniFile.DefaultInstance();
            string section = GetName();
            string json = ini.Read(section, "Json");
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    if (JsonSerializer.Deserialize(json, GetType(), SerializerOptions) is CfgCore loaded)
                        Assigne(loaded);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }

                Save(true);
                return;
            }

            bool loadedAny = false;
            bool migrateSecrets = false;
            foreach (System.Reflection.PropertyInfo property in GetIniProperties())
            {
                if (!ini.TryRead(section, property.Name, out string text))
                    continue;

                try
                {
                    object? value = IniFile.ConvertFromString(text, property.PropertyType);
                    if (IsSecret(property) && value is string secret)
                    {
                        migrateSecrets |= !string.IsNullOrEmpty(secret) && !SecretProtector.IsProtected(secret);
                        value = SecretProtector.Unprotect(secret);
                    }

                    property.SetValue(this, value);
                    loadedAny = true;
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Cannot load [{section}] {property.Name}: {ex}");
                }
            }

            if (!loadedAny || migrateSecrets)
                Save(true);
        }
        finally
        {
            _loading = false;
            _loaded = true;
        }
    }

    internal void EnsureLoaded() => Load();

    protected virtual void LoadDefauls()
    {
        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this, false))
        {
            if (property.Attributes[typeof(UpdatablePropertyAttribute)] is UpdatablePropertyAttribute attribute)
            {
                Default(property, attribute.Default);
                continue;
            }

            Attribute? legacy = property.Attributes.Cast<Attribute>()
                .FirstOrDefault(x => x.GetType().FullName == "LB.Libs.UpdatablePropertyAttribute");
            if (legacy is not null)
                Default(property, legacy.GetType().GetProperty("Default")?.GetValue(legacy));
        }
    }

    protected void Default(PropertyDescriptor property, object? value)
    {
        if (property.GetValue(this) is not null)
            return;

        property.SetValue(this, value ?? GetDefault(property.Name));
    }

    public virtual object? GetDefault(string name)
    {
        throw new NotImplemented(nameof(GetDefault), this);
    }

    protected virtual void CreateSomething()
    {
        throw new NotImplemented(nameof(CreateSomething), this);
    }

    public void Save(bool anything = false)
    {
        if (!anything && !HasChanges)
            return;

        try
        {
            IniFile ini = IniFile.DefaultInstance();
            string section = GetName();
            foreach (System.Reflection.PropertyInfo property in GetIniProperties())
            {
                object? value = property.GetValue(this);
                if (IsSecret(property) && value is string secret)
                    value = SecretProtector.Protect(secret);
                ini.Write(section, property.Name, value);
            }

            ini.Remove(section, "Json");
            ini.Save();
            EndEdit();
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
        }
    }

    private IEnumerable<System.Reflection.PropertyInfo> GetIniProperties() => GetType()
        .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
        .Where(property => property.CanRead && property.CanWrite)
        .Where(property => property.GetCustomAttributes(
            typeof(System.Runtime.Serialization.DataMemberAttribute), true).Length > 0)
        .Where(property => IniFile.IsSupportedIniType(property.PropertyType));

    private static bool IsSecret(System.Reflection.PropertyInfo property) =>
        property.PropertyType == typeof(string) &&
        property.Name.Contains("Password", StringComparison.OrdinalIgnoreCase);
}
