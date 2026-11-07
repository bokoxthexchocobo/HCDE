namespace HCDE.Playsim;

/// <summary>
/// System theme detection from <c>system_theme.cpp</c> 1-162. Chunk 1401.
/// Registry, gdbus, and CoreFoundation stay callees. The file ends at line 162.
/// </summary>
public static class VmSystemTheme1401Slice
{
    public const int Default = 0;
    public const int Light = 1;
    public const int Dark = 2;
    public const int HighContrast = 8;
    public const int HighContrastOn = 1;
    public const uint RestrictDword = 0x10;
    public const int BandStart = 1;
    public const int BandEnd = 162;
    public const string LightThemeKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";
    public const string LightThemeValue = "AppsUseLightTheme";

    public enum Platform
    {
        Windows,
        Linux,
        Mac,
        Other,
    }

    public sealed class Probe
    {
        public bool RegistryOk;
        public uint RegistryValue;
        public int RegistryCalls;
        public string? RegistryKey;
        public string? RegistryName;
        public uint RegistryRestrict;
        public bool ContrastOk;
        public uint ContrastFlags;
        public int ContrastCalls;
        public Func<string, string>? Pipe;
        public int PipeOpens;
        public int PipeCloses;
        public bool StylePresent;
        public bool StyleTypeMatches = true;
        public string StyleValue = "";
        public int StyleReleases;
        public int StyleCompares;
        public bool ContrastPresent;
        public bool ContrastTypeMatches = true;
        public bool ContrastValue;
        public int ContrastReleases;
        public int ContrastReads;
    }

    public static int GetSystemTheme(Platform platform, Probe probe)
    {
        if (probe == null)
            throw new ArgumentNullException(nameof(probe));
        int result = Default;
        if (platform == Platform.Windows)
        {
            probe.RegistryCalls++;
            probe.RegistryKey = LightThemeKey;
            probe.RegistryName = LightThemeValue;
            probe.RegistryRestrict = RestrictDword;
            if (probe.RegistryOk)
                result = probe.RegistryValue == 0 ? Dark : Light;
            probe.ContrastCalls++;
            if (probe.ContrastOk && (probe.ContrastFlags & HighContrastOn) != 0)
                result |= HighContrast;
        }
        else if (platform == Platform.Linux)
        {
            string color = GdbusRead("color-scheme", probe);
            if (color.Contains("uint32 1", StringComparison.Ordinal))
                result = Dark;
            else if (color.Contains("uint32 2", StringComparison.Ordinal))
                result = Light;
            string contrast = GdbusRead("contrast", probe);
            if (contrast.Contains("uint32 1", StringComparison.Ordinal))
                result |= HighContrast;
        }
        else if (platform == Platform.Mac)
        {
            if (Preference(probe.StylePresent, probe.StyleTypeMatches, () =>
            {
                probe.StyleCompares++;
                return string.Equals(probe.StyleValue, "Dark", StringComparison.Ordinal);
            }, () => probe.StyleReleases++))
                result = Dark;
            if (Preference(probe.ContrastPresent, probe.ContrastTypeMatches, () =>
            {
                probe.ContrastReads++;
                return probe.ContrastValue;
            }, () => probe.ContrastReleases++))
                result |= HighContrast;
        }

        return result;
    }

    public static string GdbusRead(string property, Probe probe)
    {
        if (property == null)
            throw new ArgumentNullException(nameof(property));
        string command = "gdbus call --session "
            + "--dest org.freedesktop.portal.Desktop "
            + "--object-path /org/freedesktop/portal/desktop "
            + "--method org.freedesktop.portal.Settings.Read "
            + "org.freedesktop.appearance " + property + " 2>/dev/null";
        return PipeRead(command, probe);
    }

    public static string PipeRead(string command, Probe probe)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        if (probe.Pipe == null)
            return "";
        probe.PipeOpens++;
        string text = probe.Pipe(command) ?? "";
        probe.PipeCloses++;
        return text;
    }

    public static bool Preference(bool present, bool typeMatches, Func<bool> predicate, Action release)
    {
        if (!present)
            return false;
        bool matched = typeMatches && predicate();
        release();
        return matched;
    }
}
