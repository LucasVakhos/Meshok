using System.Security.Cryptography;
using System.Text;

namespace LB.Libs;

internal static class SecretProtector
{
    private const string Prefix = "dpapi:v1:";
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("LB.Libs.Configuration.v1");

    public static bool IsProtected(string? value) =>
        value?.StartsWith(Prefix, StringComparison.Ordinal) == true;

    public static string Protect(string? value)
    {
        if (string.IsNullOrEmpty(value) || IsProtected(value))
            return value ?? string.Empty;

        byte[] plaintext = Encoding.UTF8.GetBytes(value);
        byte[] protectedData = ProtectedData.Protect(
            plaintext, Entropy, DataProtectionScope.CurrentUser);
        return Prefix + Convert.ToBase64String(protectedData);
    }

    public static string Unprotect(string? value)
    {
        if (string.IsNullOrEmpty(value) || !IsProtected(value))
            return value ?? string.Empty;

        byte[] protectedData = Convert.FromBase64String(value[Prefix.Length..]);
        byte[] plaintext = ProtectedData.Unprotect(
            protectedData, Entropy, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(plaintext);
    }
}
