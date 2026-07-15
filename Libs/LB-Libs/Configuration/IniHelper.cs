namespace LB.Libs;

/// <summary>
/// Keeps one configuration instance per concrete type. Persistence is handled
/// exclusively by the shared <see cref="IniFile"/> next to the executable.
/// </summary>
public static class IniHelper
{
    private static readonly object Sync = new();
    private static readonly Dictionary<string, CfgCore> Configurations =
        new(StringComparer.Ordinal);

    public static T CoreCfg<T>() where T : CfgCore
    {
        return GetOrCreate<T>();
    }

    public static T Cfg<T>() where T : CfgCore
    {
        return GetOrCreate<T>();
    }

    internal static bool TryGet(string name, out CfgCore? configuration)
    {
        lock (Sync)
            return Configurations.TryGetValue(name, out configuration);
    }

    internal static void AddInstance(CfgCore configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        lock (Sync)
            Configurations.TryAdd(configuration.GetName(), configuration);
    }

    internal static void SaveAll()
    {
        CfgCore[] snapshot;
        lock (Sync)
            snapshot = Configurations.Values.ToArray();

        foreach (CfgCore configuration in snapshot)
            configuration.Save();
    }

    private static T GetOrCreate<T>() where T : CfgCore
    {
        string name = typeof(T).Name;
        lock (Sync)
        {
            if (Configurations.TryGetValue(name, out CfgCore? existing))
                return (T)existing;
        }

        try
        {
            return (T)(Activator.CreateInstance(typeof(T))
                ?? throw new InvalidOperationException($"Не удалось создать конфигурацию {typeof(T).FullName}."));
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            throw;
        }
    }
}
