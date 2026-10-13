namespace HCDE.Client;

[Flags]
public enum VmMusicBankFormat { None = 0, Sf2 = 1, Wopl = 2, Wopn = 4, Gus = 8 }

public sealed record VmMusicBank(string Name, string NameWithExtension, string Path, VmMusicBankFormat Format);

public static class VmMusicBankCatalog
{
    public static IReadOnlyList<VmMusicBank> Discover(VmSearchPathConfiguration configuration,
        string? programDirectory = null, Func<string, string?>? environment = null, Action<string>? warning = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        programDirectory ??= AppContext.BaseDirectory;
        var candidates = new List<string>();
        // Native music discovery consumes only Path entries and does not descend into subdirectories.
        var paths = VmFileSearchPaths.Expand(configuration.SoundfontPaths
            .Where(path => path.Kind == VmFileSearchPathKind.Path).ToArray(), environment, programDirectory);
        foreach (var entry in paths)
        {
            var directory = entry.Value.Replace('\\', '/');
            if (!Directory.Exists(directory))
            {
                var message = "Soundfont search directory is unavailable: " + directory;
                if (warning is null) Console.Error.WriteLine(message); else warning(message);
                continue;
            }
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = false, IgnoreInaccessible = false, AttributesToSkip = 0,
            };
            candidates.AddRange(Directory.EnumerateFiles(directory, "*", options));
        }
        var banks = Collect(candidates);
        if (banks.Count != 0) return banks;
        var fallback = System.IO.Path.Combine(programDirectory, "soundfonts", "hcde.sf2");
        return File.Exists(fallback) ? Collect([fallback]) : banks;
    }

    public static VmMusicBankFormat Identify(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 16 && header[..4].SequenceEqual("RIFF"u8)
            && header.Slice(8, 8).SequenceEqual("sfbkLIST"u8)) return VmMusicBankFormat.Sf2;
        if (header.StartsWith("WOPL3-BANK\0"u8)) return VmMusicBankFormat.Wopl;
        if (header.StartsWith("WOPN2-BANK\0"u8) || header.StartsWith("WOPN2-B2NK\0"u8)) return VmMusicBankFormat.Wopn;
        return VmMusicBankFormat.None;
    }

    public static IReadOnlyList<VmMusicBank> Collect(IReadOnlyList<string> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        var banks = new List<VmMusicBank>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (names.Contains(name)) continue;
            using var stream = File.OpenRead(path);
            var header = new byte[16];
            var count = stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);
            var format = Identify(header.AsSpan(0, count));
            if (format == VmMusicBankFormat.None && count >= 2 && header[0] == 'P' && header[1] == 'K')
            {
                // Reuse the resource ZIP directory rules without decoding config or patch payloads.
                var resources = VmArchiveScriptLoader.ReadResourceNames(new(path, File.ReadAllBytes(path), VmScriptArchiveFormat.Pk3));
                if (resources.Count > 1 && resources.Any(resource => resource.FullName?.Equals("timidity.cfg", StringComparison.OrdinalIgnoreCase) == true))
                    format = VmMusicBankFormat.Gus;
            }
            if (format == VmMusicBankFormat.None) continue;
            names.Add(name);
            banks.Add(new(name, System.IO.Path.GetFileName(path), System.IO.Path.GetFullPath(path), format));
        }
        return banks.AsReadOnly();
    }

    public static VmMusicBank? Find(IReadOnlyList<VmMusicBank> banks, string? name, VmMusicBankFormat allowed)
    {
        ArgumentNullException.ThrowIfNull(banks);
        return banks.FirstOrDefault(bank => (bank.Format & allowed) != 0 && (string.IsNullOrEmpty(name)
            || bank.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            || bank.NameWithExtension.Equals(name, StringComparison.OrdinalIgnoreCase)))
            ?? banks.FirstOrDefault(bank => (bank.Format & allowed) != 0);
    }
}
