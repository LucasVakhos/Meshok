using System.ComponentModel;
using System.Text.Json;

namespace LB.Libs;

/// <summary>
/// Base class for JSON-backed application settings stored in the shared INI.
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

            string json = IniFile.DefaultInstance().Read(GetName(), "Json");
            if (string.IsNullOrWhiteSpace(json))
            {
                Save(true);
                return;
            }

            try
            {
                if (JsonSerializer.Deserialize(json, GetType(), SerializerOptions) is CfgCore loaded)
                    Assigne(loaded);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                Save(true);
            }
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
                .FirstOrDefault(x => x.GetType().FullName == "GH.Components.UpdatablePropertyAttribute");
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
            string json = JsonSerializer.Serialize(this, GetType(), SerializerOptions);
            IniFile ini = IniFile.DefaultInstance();
            ini.Write(GetName(), "Json", json);
            ini.Save();
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
        }

        EndEdit();
    }
}
