using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.ComponentModel;
namespace GH.Components
{
    public class CfgCore : AbstractEntity
    {
        internal static bool _loading = false;
    internal string ConfigPath => LB.Libs.IniFile.DefaultFilePath;
    private string LegacyConfigPath
        {
            get
            {
                if (this is CfgApp cfgApp)
                    return Path.ChangeExtension(RunContext.ExeFullName, ".ini");
                return Path.Combine(RunContext.ConfigsPath, GetName() + ".ini");
            }
        }
    public virtual string GetName()
        {
            return GetType().Name;
        }

    public CfgCore()
        {
            IniHelper.IniFile.TryGetValue(this.GetName(), out CfgCore cfg);
            if (cfg == null)
            {
                IniHelper.IniFile.AddInstance(this);
                Load();
            }
            else
                Assigne(cfg);
        }
    public void Load()
        {
            if (_loading)
                return;
            _loading = true;
            try
            {
                LoadDefauls();
                CfgCore cfg = null;
                Type type = GetType();
                LB.Libs.IniFile ini = LB.Libs.IniFile.DefaultInstance();
                string json = ini.Read(GetName(), "Json");
                bool migrated = false;

                if (string.IsNullOrEmpty(json) && File.Exists(LegacyConfigPath))
                {
                    string legacy = File.ReadAllText(LegacyConfigPath, System.Text.Encoding.UTF8).Trim();
                    if (legacy.StartsWith("{"))
                    {
                        json = legacy;
                        migrated = true;
                    }
                }

                if (string.IsNullOrEmpty(json))
                {
                    Save(true);
                    return;
                }
                JsonSerializerSettings ser = new JsonSerializerSettings();
                ser.ContractResolver = new CamelCasePropertyNamesContractResolver();
                try
                {
                    cfg = JsonConvert.DeserializeObject(json, type, ser) as CfgCore;
                    Assigne(cfg);
                    if (migrated)
                        Save(true);
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                    Save(true);
                }
            }
            finally
            {
                _loading = false;
            }
        }
    protected virtual void LoadDefauls()
        {
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this, false))
            {
                if (property.Attributes[typeof(UpdatablePropertyAttribute)] is UpdatablePropertyAttribute att)
                    Default(property, att.Default);
            }
        }
    protected void Default(PropertyDescriptor property, object val)
        {
            if (property.GetValue(this) != null)
                return;
            if (val != null)
                property.SetValue(this, val);
            else
                property.SetValue(this, GetDefault(property.Name));
        }
    public virtual object GetDefault(string name)
        {
            throw new NotImplemented(nameof(GetDefault), this);
        }
    protected virtual void CreateSomething()
        {
            throw new NotImplemented(nameof(CreateSomething), this);
        }
    public void Save(bool anything = false)
        {
            if (!(anything || HasChanges))
                return;
            Type type = GetType();
            JsonSerializerSettings ser = new JsonSerializerSettings();
            ser.ContractResolver = new CamelCasePropertyNamesContractResolver();
            try
            {
                string json = JsonConvert.SerializeObject(this, ser);
                LB.Libs.IniFile ini = LB.Libs.IniFile.DefaultInstance();
                ini.Write(GetName(), "Json", json);
                ini.Save();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
            EndEdit();
        }
    }
}
