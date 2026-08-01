using System.Drawing;
using Xunit;

namespace LB.Libs.Tests;

public sealed class IniFileTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(), $"Meshok-IniFileTests-{Guid.NewGuid():N}");

    [Fact]
    public void SaveAndLoadObject_RoundTripsSupportedValues()
    {
        var expected = new TestSettings
        {
            Text = "значение",
            Number = 42,
            Enabled = true,
            Timestamp = new DateTime(2026, 7, 31, 12, 34, 56, DateTimeKind.Utc),
            Size = new Size(800, 600),
            Location = new Point(-15, 25),
            Mode = TestMode.Second,
            Paths = [@"C:\Data", @"D:\Archive"],
            Names = new Dictionary<string, string>
            {
                ["name|with=delimiters"] = "value%with\r\na newline"
            }
        };
        IniFile writer = CreateIni();

        writer.SaveObject(expected);
        var actual = new TestSettings();
        new IniFile(writer.FilePath).LoadObject(actual);

        Assert.Equal(expected.Text, actual.Text);
        Assert.Equal(expected.Number, actual.Number);
        Assert.Equal(expected.Enabled, actual.Enabled);
        Assert.Equal(expected.Timestamp, actual.Timestamp);
        Assert.Equal(expected.Size, actual.Size);
        Assert.Equal(expected.Location, actual.Location);
        Assert.Equal(expected.Mode, actual.Mode);
        Assert.Equal(expected.Paths, actual.Paths);
        Assert.Equal(expected.Names, actual.Names);
    }

    [Fact]
    public void LoadObject_AppliesExistingEmptyString()
    {
        IniFile ini = CreateIni();
        ini.Write(nameof(TestSettings), nameof(TestSettings.Text), string.Empty);
        ini.Save();
        var settings = new TestSettings { Text = "old value" };

        new IniFile(ini.FilePath).LoadObject(settings);

        Assert.Equal(string.Empty, settings.Text);
    }

    [Fact]
    public void SaveAndReload_PreservesEscapedCharacters()
    {
        const string expected = "first%0D%0A=1\r\nsecond=2";
        IniFile ini = CreateIni();
        ini.Write("Section", "Value", expected);
        ini.Save();

        string actual = new IniFile(ini.FilePath).Read("Section", "Value");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void LoadObject_IgnoresMalformedValue()
    {
        IniFile ini = CreateIni();
        ini.Write(nameof(TestSettings), nameof(TestSettings.Number), "not-a-number");
        ini.Save();
        var settings = new TestSettings { Number = 17 };

        new IniFile(ini.FilePath).LoadObject(settings);

        Assert.Equal(17, settings.Number);
    }

    [Fact]
    public void Remove_DeletesOnlyRequestedKey()
    {
        IniFile ini = CreateIni();
        ini.Write("Section", "First", "one");
        ini.Write("Section", "Second", "two");
        ini.Remove("Section", "First");
        ini.Save();
        var reloaded = new IniFile(ini.FilePath);

        Assert.False(reloaded.TryRead("Section", "First", out _));
        Assert.Equal("two", reloaded.Read("Section", "Second"));
    }

    [Fact]
    public void Save_CreatesBackupOfPreviousValidVersion()
    {
        IniFile ini = CreateIni();
        ini.Write("Section", "Value", "first");
        ini.Save();
        ini.Write("Section", "Value", "second");

        ini.Save();

        string backupPath = ini.FilePath + ".bak";
        Assert.True(File.Exists(backupPath));
        Assert.Equal("first", new IniFile(backupPath).Read("Section", "Value"));
        Assert.Equal("second", new IniFile(ini.FilePath).Read("Section", "Value"));
    }

    [Fact]
    public void Load_UsesBackupWhenPrimaryFileIsMalformed()
    {
        IniFile ini = CreateIni();
        ini.Write("Section", "Value", "known-good");
        ini.Save();
        ini.Write("Section", "Value", "newer");
        ini.Save();
        File.WriteAllText(ini.FilePath, "broken line without section or equals");

        var recovered = new IniFile(ini.FilePath);

        Assert.Equal("known-good", recovered.Read("Section", "Value"));
    }

    [Fact]
    public void SaveAfterRecovery_DoesNotReplaceGoodBackupWithCorruptPrimary()
    {
        IniFile ini = CreateIni();
        ini.Write("Section", "Value", "known-good");
        ini.Save();
        ini.Write("Section", "Value", "newer");
        ini.Save();
        File.WriteAllText(ini.FilePath, "corrupt");
        var recovered = new IniFile(ini.FilePath);
        recovered.Write("Section", "Value", "recovered-and-updated");

        recovered.Save();

        Assert.Equal("recovered-and-updated", new IniFile(ini.FilePath).Read("Section", "Value"));
        Assert.Equal("known-good", new IniFile(ini.FilePath + ".bak").Read("Section", "Value"));
        Assert.False(File.Exists(ini.FilePath + ".tmp"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, true);
    }

    private IniFile CreateIni() => new(Path.Combine(_directory, "settings.ini"));

    private sealed class TestSettings
    {
        [Saved]
        public string Text { get; set; } = string.Empty;

        [Saved]
        public int Number { get; set; }

        [Saved]
        public bool Enabled { get; set; }

        [Saved]
        public DateTime Timestamp { get; set; }

        [Saved]
        public Size Size { get; set; }

        [Saved]
        public Point Location { get; set; }

        [Saved]
        public TestMode Mode { get; set; }

        [Saved]
        public List<string> Paths { get; set; } = [];

        [Saved]
        public Dictionary<string, string> Names { get; set; } = [];
    }

    private enum TestMode
    {
        First,
        Second
    }
}
