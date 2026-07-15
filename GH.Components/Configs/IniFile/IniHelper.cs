namespace GH.Components
{
    public static class IniHelper
    {
        private static IniFile _iniFile;
    public static IniFile IniFile
        {
            get
            {
                if (_iniFile == null)
                    _iniFile = new IniFile();
                return _iniFile;
            }
        }
    public static CfgApp CfgAppForm()
        {
            CfgCore cfg = null;
            IniFile.TryGetValue(nameof(CfgApp), out cfg);
            if (cfg == null)
            {
                cfg = RunContext.Instance.GetCfgApp();
            }
            if (cfg is CfgApp app)
            {
                if (RunContext.AppMainForm != null && app.Form == null)
                    app.Form = RunContext.AppMainForm;
                return app;
            }
            throw new Exception($"Чтото пошло не так в {nameof(CfgAppForm)}!!!");
        }

    public static T CoreCfg<T>() where T : CfgCore
        {
            T cfg = GetCoreConfig<T>();
            if (cfg == null)
            {
                try
                {
                    cfg = CreateCoreConfig<T>();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            return cfg;
        }

    public static T Cfg<T>() where T : CfgCoreConnection
        {
            return LB.Libs.IniHelper.Cfg<T>();
        }

    private static T GetCoreConfig<T>() where T : CfgCore
        {
            CfgCore ret;
            IniFile.TryGetValue(typeof(T).Name, out ret);
            return ret as T;
        }

    private static T CreateCoreConfig<T>() where T : CfgCore
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

    internal static void SaveAll()
        {
            IniFile.SaveAll();
        }
    internal static void AddInstance(CfgCoreConnection cfgCoreConnection)
        {
            // LB.Libs.CfgCoreConnection registers itself in the shared cache.
        }
    }
}
