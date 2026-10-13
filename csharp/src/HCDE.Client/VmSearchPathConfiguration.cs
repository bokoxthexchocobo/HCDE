namespace HCDE.Client;

public sealed record VmSearchPathConfiguration(IReadOnlyList<VmFileSearchPath> IwadPaths,
    IReadOnlyList<VmFileSearchPath> FilePaths)
{
    public bool HasIwadSection { get; init; }
    public bool HasFileSection { get; init; }
    public bool HasSoundfontSection { get; init; }
    public IReadOnlyList<VmFileSearchPath> SoundfontPaths { get; init; } = Array.Empty<VmFileSearchPath>();
    public string? LastRunVersion { get; init; }

    public VmSearchPathConfiguration WithWindowsLegacySearchMigration()
    {
        var match = System.Text.RegularExpressions.Regex.Match(LastRunVersion ?? "",
            @"^[\x09-\x0d ]*[+-]?(?:[0-9]+(?:\.[0-9]*)?|\.[0-9]+)(?:[eE][+-]?[0-9]+)?",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        var version = match.Success && double.TryParse(match.Value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
        if (version >= 227) return this;
        return this with
        {
            IwadPaths = HasIwadSection ? Ensure(IwadPaths) : IwadPaths,
            FilePaths = HasFileSection ? Ensure(FilePaths) : FilePaths,
            SoundfontPaths = HasSoundfontSection ? Ensure(SoundfontPaths, SoundfontDefaults()) : SoundfontPaths,
        };

        static IReadOnlyList<VmFileSearchPath> Ensure(IReadOnlyList<VmFileSearchPath> paths, IReadOnlyList<string>? values = null)
        {
            var result = paths.ToList();
            foreach (var value in values ?? ["$HOME", "$PROGDIR"])
                if (!result.Any(path => path.Kind == VmFileSearchPathKind.Path && path.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
                    result.Add(new(VmFileSearchPathKind.Path, value));
            return result.AsReadOnly();
        }
    }

    public VmSearchPathConfiguration WithWindowsDefaults()
    {
        VmFileSearchPath[] defaults = [new(VmFileSearchPathKind.Path, "$HOME"), new(VmFileSearchPathKind.Path, "$PROGDIR")];
        return this with
        {
            IwadPaths = HasIwadSection ? IwadPaths : Array.AsReadOnly(defaults),
            FilePaths = HasFileSection ? FilePaths : Array.AsReadOnly(defaults),
            SoundfontPaths = HasSoundfontSection ? SoundfontPaths : Array.AsReadOnly(SoundfontDefaults()
                .Select(value => new VmFileSearchPath(VmFileSearchPathKind.Path, value)).ToArray()),
            HasIwadSection = true,
            HasFileSection = true,
            HasSoundfontSection = true,
        };
    }

    private static string[] SoundfontDefaults() =>
        ["$HOME/soundfonts", "$HOME/fm_banks", "$PROGDIR/soundfonts", "$PROGDIR/fm_banks"];

    public static VmSearchPathConfiguration ReadFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return ParseBytes(File.ReadAllBytes(path));
    }

    public static VmSearchPathConfiguration ParseBytes(ReadOnlySpan<byte> bytes)
    {
        try
        {
            // Native ReadConfig does not strip a BOM or detect UTF-16.
            return Parse(new System.Text.UTF8Encoding(false, true).GetString(bytes));
        }
        catch (System.Text.DecoderFallbackException error)
        {
            throw new InvalidDataException("Search configuration is not valid UTF-8", error);
        }
    }

    public static VmSearchPathConfiguration Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var iwads = new List<VmFileSearchPath>();
        var files = new List<VmFileSearchPath>();
        var soundfonts = new List<VmFileSearchPath>();
        string? section = null;
        var hasIwadSection = false;
        var hasFileSection = false;
        var hasSoundfontSection = false;
        string? lastRunVersion = null;
        var position = 0;
        while (ReadLine() is { } raw)
        {
            var line = Trim(raw);
            if (line.Length < 2 || line.StartsWith('#') || line.StartsWith("//", StringComparison.Ordinal)) continue;
            if (line[0] == '[')
            {
                section = line.EndsWith(']') ? line[1..^1] : line[1..];
                hasIwadSection |= section.Equals("IWADSearch.Directories", StringComparison.OrdinalIgnoreCase);
                hasFileSection |= section.Equals("FileSearch.Directories", StringComparison.OrdinalIgnoreCase);
                hasSoundfontSection |= section.Equals("SoundfontSearch.Directories", StringComparison.OrdinalIgnoreCase);
                continue;
            }
            if (section is null) throw new InvalidDataException("Configuration entry precedes its section");
            var equal = line.IndexOf('=');
            if (equal <= 0) continue;
            var key = Trim(line[..equal]);
            var value = Trim(line[(equal + 1)..]);
            if (value.StartsWith("<<<", StringComparison.Ordinal) && value.Length > 3)
            {
                var terminator = ">>>" + value[3..];
                var body = new System.Text.StringBuilder();
                var trimLineBreaks = false;
                while (ReadLine() is { } bodyLine)
                {
                    // Native stops on a matching prefix even when trailing text invalidates newline trimming.
                    if (bodyLine.StartsWith(terminator, StringComparison.Ordinal))
                    {
                        var suffix = bodyLine[terminator.Length..];
                        trimLineBreaks = suffix.All(character => character is '\n' or '\r');
                        break;
                    }
                    body.Append(bodyLine);
                }
                value = body.ToString();
                if (trimLineBreaks) value = value.TrimEnd('\n', '\r');
            }
            var isIwad = section.Equals("IWADSearch.Directories", StringComparison.OrdinalIgnoreCase);
            if (lastRunVersion is null && section.Equals("LastRun", StringComparison.OrdinalIgnoreCase)
                && key.Equals("Version", StringComparison.OrdinalIgnoreCase)) lastRunVersion = value;
            var isFile = section.Equals("FileSearch.Directories", StringComparison.OrdinalIgnoreCase);
            var isSoundfont = section.Equals("SoundfontSearch.Directories", StringComparison.OrdinalIgnoreCase);
            if (!isIwad && !isFile && !isSoundfont) continue;
            VmFileSearchPathKind? kind = key.ToLowerInvariant() switch
            {
                "path" => VmFileSearchPathKind.Path,
                "recursivepath" => VmFileSearchPathKind.RecursivePath,
                "pathlist" when isFile => VmFileSearchPathKind.PathList,
                _ => null,
            };
            if (kind is { } recognized) (isIwad ? iwads : isFile ? files : soundfonts).Add(new(recognized, value));
        }
        return new(iwads.AsReadOnly(), files.AsReadOnly())
            { HasIwadSection = hasIwadSection, HasFileSection = hasFileSection, LastRunVersion = lastRunVersion,
                HasSoundfontSection = hasSoundfontSection, SoundfontPaths = soundfonts.AsReadOnly() };

        string? ReadLine()
        {
            var line = new System.Text.StringBuilder();
            while (position < text.Length)
            {
                var character = text[position++];
                if (character == '\0') break;
                if (character == '\r') continue;
                line.Append(character);
                if (character == '\n') break;
            }
            return line.Length == 0 ? null : line.ToString();
        }
    }

    private static string Trim(string text)
    {
        var start = 0;
        var end = text.Length;
        while (start < end && text[start] <= ' ') start++;
        while (end > start && text[end - 1] <= ' ') end--;
        return text[start..end];
    }
}
