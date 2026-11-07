using S = HCDE.Playsim.VmSystemTheme1401Slice;

namespace HCDE.Playsim.Tests;

public class VmSystemTheme1401SliceTests
{
    [Fact]
    public void WindowsAndLinuxThemeFollowTheRegistryAndThePortalText()
    {
        var dark = new S.Probe { RegistryOk = true, RegistryValue = 0, ContrastOk = true, ContrastFlags = S.HighContrastOn };
        Assert.Equal(S.Dark | S.HighContrast, S.GetSystemTheme(S.Platform.Windows, dark));
        Assert.Equal(S.LightThemeKey, dark.RegistryKey);
        Assert.Equal(S.LightThemeValue, dark.RegistryName);
        Assert.Equal(S.RestrictDword, dark.RegistryRestrict);
        var light = new S.Probe { RegistryOk = true, RegistryValue = 0xFFFFFFFF, ContrastOk = true, ContrastFlags = 0 };
        Assert.Equal(S.Light, S.GetSystemTheme(S.Platform.Windows, light));
        var missed = new S.Probe { ContrastOk = false, ContrastFlags = S.HighContrastOn };
        Assert.Equal(S.Default, S.GetSystemTheme(S.Platform.Windows, missed));
        Assert.Equal(1, missed.ContrastCalls);

        var linux = new S.Probe();
        var commands = new List<string>();
        linux.Pipe = command =>
        {
            commands.Add(command);
            if (command.Contains("color-scheme", StringComparison.Ordinal))
                return "uint32 10";
            return "uint32 1";
        };
        Assert.Equal(S.Dark | S.HighContrast, S.GetSystemTheme(S.Platform.Linux, linux));
        Assert.Equal(2, commands.Count);
        Assert.Contains("org.freedesktop.appearance color-scheme 2>/dev/null", commands[0], StringComparison.Ordinal);
        Assert.Contains("org.freedesktop.appearance contrast 2>/dev/null", commands[1], StringComparison.Ordinal);
        Assert.Equal(2, linux.PipeCloses);
        var preferLight = new S.Probe { Pipe = command => command.Contains("color-scheme", StringComparison.Ordinal) ? "uint32 2" : "" };
        Assert.Equal(S.Light, S.GetSystemTheme(S.Platform.Linux, preferLight));
        var closed = new S.Probe();
        Assert.Equal("", S.PipeRead("gdbus", closed));
        Assert.Equal(0, closed.PipeOpens);
    }

    [Fact]
    public void MacReleasesPreferencesAndOtherPlatformsStayDefault()
    {
        var dark = new S.Probe { StylePresent = true, StyleValue = "Dark", ContrastPresent = true, ContrastValue = true };
        Assert.Equal(S.Dark | S.HighContrast, S.GetSystemTheme(S.Platform.Mac, dark));
        Assert.Equal(1, dark.StyleReleases);
        Assert.Equal(1, dark.StyleCompares);
        Assert.Equal(1, dark.ContrastReleases);
        var spelled = new S.Probe { StylePresent = true, StyleValue = "dark", ContrastPresent = true, ContrastTypeMatches = false, ContrastValue = true };
        Assert.Equal(S.Default, S.GetSystemTheme(S.Platform.Mac, spelled));
        Assert.Equal(1, spelled.StyleReleases);
        Assert.Equal(1, spelled.ContrastReleases);
        Assert.Equal(0, spelled.ContrastReads);
        var missing = new S.Probe { ContrastPresent = true, ContrastValue = false };
        Assert.Equal(S.Default, S.GetSystemTheme(S.Platform.Mac, missing));
        Assert.Equal(0, missing.StyleReleases);
        Assert.Equal(1, missing.ContrastReleases);
        Assert.Equal(S.Default, S.GetSystemTheme(S.Platform.Other, new S.Probe()));
        Assert.Throws<ArgumentNullException>(() => S.GetSystemTheme(S.Platform.Mac, null!));
    }
}
