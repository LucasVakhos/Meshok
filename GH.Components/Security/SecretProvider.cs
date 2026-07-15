#nullable enable
using System.Reflection;

namespace GH.Components;

/// <summary>
/// Reads optional local values from the ignored root Secret.cs file. A clean
/// clone still compiles; missing secrets resolve to safe empty defaults and can
/// then be supplied through the common application INI.
/// </summary>
public static class SecretProvider
{
    private static readonly string[] SecretTypeNames = { "LB.Libs.Secret", "GH.Components.Secret" };
    private static readonly Lazy<Type?> SecretType = new(() =>
        SecretTypeNames.Select(name => typeof(SecretProvider).Assembly.GetType(name, false)).FirstOrDefault(type => type != null));

    public static string EncryptionPass => GetString(nameof(EncryptionPass));

    public static string CdUchetDataSource => GetString(nameof(CdUchetDataSource));
    public static string CdUchetDatabase => GetString(nameof(CdUchetDatabase));
    public static string CdUchetUserId => GetString(nameof(CdUchetUserId));
    public static string CdUchetPassword => GetString(nameof(CdUchetPassword));

    public static string IShopDataSource => GetString(nameof(IShopDataSource));
    public static string IShopDatabasePath => GetString(nameof(IShopDatabasePath));
    public static string IShopUserId => GetString(nameof(IShopUserId));
    public static string IShopPassword => GetString(nameof(IShopPassword));
    public static string IShopUserLogin => GetString(nameof(IShopUserLogin));
    public static string IShopUserPassword => GetString(nameof(IShopUserPassword));

    public static string MeshokUserLogin => GetString(nameof(MeshokUserLogin));
    public static string MeshokUserPassword => GetString(nameof(MeshokUserPassword));

    public static string LegacyIShopDataSource => GetString(nameof(LegacyIShopDataSource));
    public static string LegacyIShopDatabase => GetString(nameof(LegacyIShopDatabase));
    public static string LegacyIShopLogin => GetString(nameof(LegacyIShopLogin));
    public static string LegacyIShopPassword => GetString(nameof(LegacyIShopPassword));
    public static string LegacyMeshokUser => GetString(nameof(LegacyMeshokUser));
    public static string LegacyMeshokPassword => GetString(nameof(LegacyMeshokPassword));
    public static string LegacyBridgeServer => GetString(nameof(LegacyBridgeServer));
    public static string LegacyBridgeDatabase => GetString(nameof(LegacyBridgeDatabase));
    public static string LegacyBridgeUser => GetString(nameof(LegacyBridgeUser));
    public static string LegacyBridgePassword => GetString(nameof(LegacyBridgePassword));

    public static string NewsBridgeServer => GetString(nameof(NewsBridgeServer));
    public static string NewsBridgeDatabase => GetString(nameof(NewsBridgeDatabase));
    public static string NewsBridgeUserId => GetString(nameof(NewsBridgeUserId));
    public static string NewsBridgePassword => GetString(nameof(NewsBridgePassword));
    public static string CoreNewsBridgeServer => GetString(nameof(CoreNewsBridgeServer));
    public static string CoreNewsBridgeDatabase => GetString(nameof(CoreNewsBridgeDatabase));
    public static string CoreNewsBridgeUserId => GetString(nameof(CoreNewsBridgeUserId));
    public static string CoreNewsBridgePassword => GetString(nameof(CoreNewsBridgePassword));
    public static string NewsMyEmail => GetString(nameof(NewsMyEmail));

    public static string NewsSmtpServer => GetString(nameof(NewsSmtpServer));
    public static string NewsSmtpUser => GetString(nameof(NewsSmtpUser));
    public static string NewsSmtpPassword => GetString(nameof(NewsSmtpPassword));
    public static string NewsBridgeEmail => GetString(nameof(NewsBridgeEmail));
    public static string NewsSenderEmail => GetString(nameof(NewsSenderEmail));
    public static string NewsContactEmail => GetString(nameof(NewsContactEmail));
    public static string NewsContactPhone => GetString(nameof(NewsContactPhone));

    public static string SendPulseUserId => GetString(nameof(SendPulseUserId));
    public static string SendPulseSecret => GetString(nameof(SendPulseSecret));
    public static string SendPulseBackEmail => GetString(nameof(SendPulseBackEmail));

    public static int RuSenderId => GetInt(nameof(RuSenderId));
    public static string RuSenderApiKey => GetString(nameof(RuSenderApiKey));
    public static string RuSenderBackEmail => GetString(nameof(RuSenderBackEmail));

    private static string GetString(string propertyName)
    {
        object? value = GetValue(propertyName);
        return value as string ?? string.Empty;
    }

    private static int GetInt(string propertyName)
    {
        object? value = GetValue(propertyName);
        return value is int number ? number : 0;
    }

    private static object? GetValue(string propertyName)
    {
        return SecretType.Value?
            .GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?
            .GetValue(null);
    }
}
