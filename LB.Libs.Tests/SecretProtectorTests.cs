using System.Security.Cryptography;
using Xunit;

namespace LB.Libs.Tests;

public sealed class SecretProtectorTests
{
    [Fact]
    public void ProtectAndUnprotect_RoundTripsSecretWithoutStoringPlaintext()
    {
        const string secret = "пароль-123";

        string protectedValue = SecretProtector.Protect(secret);

        Assert.True(SecretProtector.IsProtected(protectedValue));
        Assert.DoesNotContain(secret, protectedValue, StringComparison.Ordinal);
        Assert.Equal(secret, SecretProtector.Unprotect(protectedValue));
    }

    [Fact]
    public void Protect_DoesNotEncryptValueTwice()
    {
        string protectedValue = SecretProtector.Protect("secret");

        Assert.Equal(protectedValue, SecretProtector.Protect(protectedValue));
    }

    [Fact]
    public void Unprotect_ReturnsLegacyPlaintextUnchanged()
    {
        Assert.Equal("legacy-password", SecretProtector.Unprotect("legacy-password"));
        Assert.Equal(string.Empty, SecretProtector.Unprotect(string.Empty));
    }

    [Fact]
    public void Unprotect_RejectsInvalidProtectedValue()
    {
        Assert.ThrowsAny<CryptographicException>(() =>
            SecretProtector.Unprotect("dpapi:v1:AQIDBA=="));
    }
}
