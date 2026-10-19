using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using HCDE.MapLoader;
using HCDE.Scripting;

namespace HCDE.ConversionInventory;

public sealed record MatrixEntry(
    string Category,
    string Id,
    string Name,
    SupportStatus Status,
    string? NativeSource,
    string? ManagedSource,
    string? Notes);

public sealed record MatrixSummary(
    string Category,
    int Executed,
    int Partial,
    int DecodedOnly,
    int Absent,
    int Total);

public sealed record SubsystemLocSummary(string Subsystem, string NativePath, int NativeLoc, int ManagedLoc, int ManagedFiles);

public sealed class SupportMatrix
{
    public required IReadOnlyList<MatrixEntry> Entries { get; init; }
    public required IReadOnlyList<MatrixSummary> Summaries { get; init; }
    public required IReadOnlyList<SubsystemLocSummary> SubsystemLoc { get; init; }
    public required DateTimeOffset GeneratedAt { get; init; }
}

public static class SupportMatrixBuilder
{
    private static readonly HashSet<int> AcsPartialOpcodes =
    [
        (int)AcsPcode.BeginPrint, (int)AcsPcode.EndPrint, (int)AcsPcode.EndPrintBold,
        (int)AcsPcode.PrintString, (int)AcsPcode.PrintNumber, (int)AcsPcode.PrintCharacter,
        (int)AcsPcode.MoreHudMessage, (int)AcsPcode.OptHudMessage, (int)AcsPcode.EndHudMessage,
        (int)AcsPcode.EndHudMessageBold, (int)AcsPcode.SetStyle, (int)AcsPcode.SetFont,
        (int)AcsPcode.SpawnDirect, (int)AcsPcode.SpawnSpotDirect,
    ];

    public static SupportMatrix Build(string repositoryRoot)
    {
        var entries = new List<MatrixEntry>();
        entries.AddRange(BuildAcsOpcodes(repositoryRoot));
        entries.AddRange(BuildAcsCallFunctions(repositoryRoot));
        entries.AddRange(BuildVmOpcodes(repositoryRoot));
        entries.AddRange(BuildLineSpecials(repositoryRoot));
        entries.AddRange(BuildActorClasses());
        entries.AddRange(BuildPersistenceArchives(repositoryRoot));

        var summaries = entries
            .GroupBy(entry => entry.Category)
            .Select(group => new MatrixSummary(
                group.Key,
                group.Count(e => e.Status == SupportStatus.Executed),
                group.Count(e => e.Status == SupportStatus.Partial),
                group.Count(e => e.Status == SupportStatus.DecodedOnly),
                group.Count(e => e.Status == SupportStatus.Absent),
                group.Count()))
            .OrderBy(summary => summary.Category, StringComparer.Ordinal)
            .ToArray();

        var subsystemLoc = BuildSubsystemLoc(repositoryRoot);
        return new SupportMatrix
        {
            Entries = entries,
            Summaries = summaries,
            SubsystemLoc = subsystemLoc,
            GeneratedAt = DateTimeOffset.UtcNow,
        };
    }

    private static IEnumerable<MatrixEntry> BuildAcsOpcodes(string root)
    {
        var native = ParseNativeAcsOpcodes(Path.Combine(root, "src/playsim/p_acs.cpp"));
        var decoded = ParseDecodedAcsOpcodes();
        var executed = ParseExecutedAcsOpcodes(Path.Combine(root, "csharp/src/HCDE.Playsim/AcsVm.cs"));
        foreach (var (id, name) in native.OrderBy(pair => pair.Key))
        {
            var status = SupportStatus.Absent;
            if (executed.Contains(id)) status = AcsPartialOpcodes.Contains(id) ? SupportStatus.Partial : SupportStatus.Executed;
            else if (decoded.Contains(id)) status = SupportStatus.DecodedOnly;
            yield return new MatrixEntry(
                "acs_opcode",
                id.ToString(CultureInfo.InvariantCulture),
                name,
                status,
                "src/playsim/p_acs.cpp",
                status is SupportStatus.Executed or SupportStatus.Partial ? "csharp/src/HCDE.Playsim/AcsVm.cs" : null,
                null);
        }
    }

    private static IEnumerable<MatrixEntry> BuildAcsCallFunctions(string root)
    {
        var native = ParseNativeCallFunctions(Path.Combine(root, "src/playsim/p_acs.cpp"));
        var managed = ParseManagedCallFunctions(Path.Combine(root, "csharp/src/HCDE.Playsim/AcsCallFunctions.cs"));
        foreach (var (id, name) in native.OrderBy(pair => pair.Key))
        {
            var status = managed.Contains(id) ? SupportStatus.Executed : SupportStatus.Absent;
            yield return new MatrixEntry(
                "acs_callfunc",
                id.ToString(CultureInfo.InvariantCulture),
                name,
                status,
                "src/playsim/p_acs.cpp",
                status == SupportStatus.Executed ? "csharp/src/HCDE.Playsim/AcsCallFunctions.cs" : null,
                null);
        }
    }

    private static IEnumerable<MatrixEntry> BuildVmOpcodes(string root)
    {
        var native = ParseNativeVmOps(Path.Combine(root, "src/common/scripting/vm/vmops.h"));
        var executed = ParseExecutedVmOps(Path.Combine(root, "csharp/src/HCDE.Scripting/VmExec.cs"));
        var named = Enum.GetValues<VmOp>().Cast<int>().ToHashSet();
        foreach (var (id, name) in native.OrderBy(pair => pair.Key))
        {
            var status = SupportStatus.Absent;
            if (executed.Contains(id)) status = SupportStatus.Executed;
            else if (named.Contains(id)) status = SupportStatus.DecodedOnly;
            yield return new MatrixEntry(
                "zscript_vm_opcode",
                id.ToString(CultureInfo.InvariantCulture),
                name,
                status,
                "src/common/scripting/vm/vmops.h",
                status == SupportStatus.Executed ? "csharp/src/HCDE.Scripting/VmExec.cs" : null,
                null);
        }
    }

    private static IEnumerable<MatrixEntry> BuildLineSpecials(string root)
    {
        var native = ParseNativeLineSpecials(Path.Combine(root, "src/playsim/p_lnspec.cpp"));
        var managed = ParseManagedLineSpecials(Path.Combine(root, "csharp/src/HCDE.Playsim"));
        foreach (var (id, name) in native.OrderBy(pair => pair.Key))
        {
            var status = managed.Contains(id) ? SupportStatus.Executed : SupportStatus.Absent;
            yield return new MatrixEntry(
                "line_special",
                id.ToString(CultureInfo.InvariantCulture),
                name,
                status,
                "src/playsim/p_lnspec.cpp",
                status == SupportStatus.Executed ? "csharp/src/HCDE.Playsim/LineSpecials.cs" : null,
                null);
        }
    }

    private static IEnumerable<MatrixEntry> BuildActorClasses()
    {
        var catalog = typeof(HCDE.Gamedata.DoomActorCatalog).Assembly.GetType("HCDE.Gamedata.DoomActorCatalog");
        var defs = catalog?.GetField("Definitions", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
        if (defs is System.Collections.IDictionary map)
        {
            foreach (var key in map.Keys.Cast<int>().Order())
                yield return new MatrixEntry("actor_class", key.ToString(CultureInfo.InvariantCulture), $"DoomEdNum_{key}",
                    SupportStatus.Partial, "wadsrc/static/zscript/actors/doom", "csharp/src/HCDE.Gamedata/DoomActorCatalog.cs",
                    "Catalog stats only; ZScript class execution is not ported.");
        }
    }

    private static IEnumerable<MatrixEntry> BuildPersistenceArchives(string root)
    {
        var playsim = Path.Combine(root, "csharp/src/HCDE.Playsim");
        foreach (var file in Directory.EnumerateFiles(playsim, "Sim*Archive.cs").OrderBy(path => path))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            yield return new MatrixEntry(
                "persistence_archive",
                name,
                name,
                SupportStatus.Partial,
                "src/playsim/p_saveg.cpp",
                $"csharp/src/HCDE.Playsim/{Path.GetFileName(file)}",
                "Pose/state slice; not full native save membership.");
        }
    }

    private static SubsystemLocSummary[] BuildSubsystemLoc(string root)
    {
        (string name, string nativeRel, string managedRel)[] rows =
        [
            ("playsim", "src/playsim", "csharp/src/HCDE.Playsim"),
            ("maploader", "src/maploader", "csharp/src/HCDE.MapLoader"),
            ("scripting_vm", "src/common/scripting", "csharp/src/HCDE.Scripting"),
            ("rendering", "src/rendering", "csharp/src/HCDE.Client"),
            ("net", "src/d_net.cpp", "csharp/src/HCDE.Net.Core"),
            ("gamedata", "src/gamedata", "csharp/src/HCDE.Gamedata"),
        ];
        return rows.Select(row => new SubsystemLocSummary(
            row.name,
            row.nativeRel,
            CountNativeLoc(Path.Combine(root, row.nativeRel)),
            CountManagedLoc(Path.Combine(root, row.managedRel)),
            CountManagedFiles(Path.Combine(root, row.managedRel)))).ToArray();
    }

    private static int CountNativeLoc(string path)
    {
        if (File.Exists(path)) return CountLines(path);
        if (!Directory.Exists(path)) return 0;
        return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".cpp", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".c", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".h", StringComparison.OrdinalIgnoreCase))
            .Sum(CountLines);
    }

    private static int CountManagedLoc(string path)
    {
        if (!Directory.Exists(path)) return 0;
        return Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories).Sum(CountLines);
    }

    private static int CountManagedFiles(string path) =>
        Directory.Exists(path) ? Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories).Count() : 0;

    private static int CountLines(string file)
    {
        try { return File.ReadLines(file).Count(); }
        catch { return 0; }
    }

    private static Dictionary<int, string> ParseNativeAcsOpcodes(string path)
    {
        var text = File.ReadAllText(path);
        var match = Regex.Match(text, @"enum\s*\{[^}]*PCD_NOP", RegexOptions.Singleline);
        if (!match.Success) return new Dictionary<int, string>();
        var start = text.IndexOf("PCD_NOP", match.Index, StringComparison.Ordinal);
        var end = text.IndexOf("};", start, StringComparison.Ordinal);
        var block = text[start..end];
        var lines = block.Split('\n');
        var result = new Dictionary<int, string>();
        var index = 0;
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.StartsWith("/*", StringComparison.Ordinal)) continue;
            var nameMatch = Regex.Match(line, @"(?:PCD_)?([A-Z0-9_]+)");
            if (!nameMatch.Success) continue;
            var name = nameMatch.Groups[1].Value;
            if (name is "NOP" or "TERMINATE") { /* first opcode */ }
            if (line.Contains("/*", StringComparison.Ordinal) && line.IndexOf("/*", StringComparison.Ordinal) < line.IndexOf("PCD_", StringComparison.Ordinal))
            {
                var comment = Regex.Match(line, @"/\*\s*(\d+)\*/");
                if (comment.Success) index = int.Parse(comment.Groups[1].Value, CultureInfo.InvariantCulture);
            }
            result[index] = name;
            index++;
        }
        return result;
    }

    private static HashSet<int> ParseDecodedAcsOpcodes()
    {
        var decoded = new HashSet<int>();
        foreach (AcsPcode opcode in Enum.GetValues(typeof(AcsPcode)))
            decoded.Add((int)opcode);
        foreach (var wire in new[] { 359, 360, 361, 362, 363, 376, 377, 378, 379, 380, 383, 384, 393, 394, 395, 396, 397, 398, 399, 400, 401, 402, 403, 404, 405, 406, 444, 465, 466 })
            decoded.Add(wire);
        return decoded;
    }

    private static HashSet<int> ParseExecutedAcsOpcodes(string path)
    {
        var text = File.ReadAllText(path);
        var executed = new HashSet<int>();
        foreach (Match match in Regex.Matches(text, @"case\s*\(int\)AcsPcode\.(\w+)\s*:"))
        {
            if (Enum.TryParse<AcsPcode>(match.Groups[1].Value, out var opcode))
                executed.Add((int)opcode);
        }
        foreach (Match match in Regex.Matches(text, @"case\s+(\d+)\s*:"))
            executed.Add(int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
        foreach (Match match in Regex.Matches(text, @"case\s*>=\s*\(int\)AcsPcode\.(\w+)\s+and\s*<=\s*\(int\)AcsPcode\.(\w+)\s*:"))
        {
            if (Enum.TryParse<AcsPcode>(match.Groups[1].Value, out var start)
                && Enum.TryParse<AcsPcode>(match.Groups[2].Value, out var end))
            {
                for (var value = (int)start; value <= (int)end; value++) executed.Add(value);
            }
        }
        if (text.Contains("RunArray(fiber, opcode)", StringComparison.Ordinal))
        {
            for (var value = (int)AcsPcode.PushGlobalArray; value <= (int)AcsPcode.DecGlobalArray; value++) executed.Add(value);
            for (var value = (int)AcsPcode.PushMapArray; value <= (int)AcsPcode.DecMapArray; value++) executed.Add(value);
            for (var value = (int)AcsPcode.PushWorldArray; value <= (int)AcsPcode.DecWorldArray; value++) executed.Add(value);
            for (var value = (int)AcsPcode.AndMapArray; value <= (int)AcsPcode.RsGlobalArray; value++) executed.Add(value);
            for (var value = (int)AcsPcode.AssignScriptArray; value <= (int)AcsPcode.RsScriptArray; value++) executed.Add(value);
        }
        return executed;
    }

    private static Dictionary<int, string> ParseNativeCallFunctions(string path)
    {
        var text = File.ReadAllText(path);
        var result = new Dictionary<int, string>();
        var match = Regex.Match(text, @"ACSF_GetLineUDMFInt\s*=\s*1,", RegexOptions.Singleline);
        if (!match.Success) return result;
        var start = text.IndexOf("ACSF_GetLineUDMFInt", match.Index, StringComparison.Ordinal);
        var end = text.IndexOf("};", start, StringComparison.Ordinal);
        var index = 1;
        foreach (var line in text[start..end].Split('\n'))
        {
            var name = Regex.Match(line, @"ACSF_([A-Za-z0-9_]+)");
            if (!name.Success) continue;
            result[index] = name.Groups[1].Value;
            if (!line.TrimEnd().EndsWith(',')) continue;
            index++;
        }
        return result;
    }

    private static HashSet<int> ParseManagedCallFunctions(string path)
    {
        var text = File.ReadAllText(path);
        var result = new HashSet<int>();
        foreach (Match match in Regex.Matches(text, @"public const int (\w+) = (\d+);"))
        {
            if (match.Groups[1].Value is "FixedMax") continue;
            result.Add(int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture));
        }
        return result;
    }

    private static Dictionary<int, string> ParseNativeVmOps(string path)
    {
        var text = File.ReadAllText(path);
        var result = new Dictionary<int, string>();
        var index = 0;
        foreach (Match match in Regex.Matches(text, @"xx\(([A-Z0-9_]+),"))
        {
            result[index] = match.Groups[1].Value;
            index++;
        }
        return result;
    }

    private static HashSet<int> ParseExecutedVmOps(string path)
    {
        var text = File.ReadAllText(path);
        var executed = new HashSet<int>();
        foreach (Match match in Regex.Matches(text, @"case\s*\(int\)VmOp\.(\w+)\s*:"))
        {
            if (Enum.TryParse<VmOp>(match.Groups[1].Value, out var opcode))
                executed.Add((int)opcode);
        }
        foreach (Match match in Regex.Matches(text, @"op\s*>=\s*\(int\)VmOp\.(\w+)\s*&&\s*op\s*<=\s*\(int\)VmOp\.(\w+)"))
        {
            if (Enum.TryParse<VmOp>(match.Groups[1].Value, out var start)
                && Enum.TryParse<VmOp>(match.Groups[2].Value, out var end))
            {
                for (var value = (int)start; value <= (int)end; value++) executed.Add(value);
            }
        }
        return executed;
    }

    private static Dictionary<int, string> ParseNativeLineSpecials(string path)
    {
        var header = Path.Combine(Path.GetDirectoryName(path)!, "actionspecials.h");
        var text = File.ReadAllText(header);
        var result = new Dictionary<int, string>();
        foreach (Match match in Regex.Matches(text, @"DEFINE_SPECIAL\(([^,]+),\s*(\d+)"))
            result[int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture)] = match.Groups[1].Value.Trim();
        return result;
    }

    private static HashSet<int> ParseManagedLineSpecials(string playsimDirectory)
    {
        var text = string.Join('\n', Directory.EnumerateFiles(playsimDirectory, "*.cs", SearchOption.TopDirectoryOnly)
            .Select(File.ReadAllText));
        var result = new HashSet<int>();
        foreach (Match match in Regex.Matches(text, @"\b(?:special|line\.Special)\s*==\s*(\d+)"))
            result.Add(int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
        foreach (Match match in Regex.Matches(text, @"\b(\d+)\s+or\s+\d+\s+or"))
            result.Add(int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
        foreach (Match match in Regex.Matches(text, @"Execute\w+Special\(sim,\s*(?:line\.Special|special)"))
            _ = match;
        foreach (Match match in Regex.Matches(text, @"public const int \w+ = (\d+);"))
            result.Add(int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
        return result;
    }

    public static string FormatMarkdown(SupportMatrix matrix)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# HCDE native-to-managed support matrix");
        builder.AppendLine();
        builder.AppendLine($"Generated: {matrix.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC.");
        builder.AppendLine("Regenerate: `dotnet run --project csharp/src/HCDE.ConversionInventory -- ../../`");
        builder.AppendLine();
        builder.AppendLine("## Headline counts");
        builder.AppendLine();
        builder.AppendLine("| Category | Executed | Partial | Decoded-only | Absent | Total |");
        builder.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: |");
        foreach (var summary in matrix.Summaries)
            builder.AppendLine(CultureInfo.InvariantCulture,
                $"| {summary.Category} | {summary.Executed} | {summary.Partial} | {summary.DecodedOnly} | {summary.Absent} | {summary.Total} |");
        var totals = matrix.Summaries.Aggregate(
            (Executed: 0, Partial: 0, DecodedOnly: 0, Absent: 0, Total: 0),
            (acc, row) => (acc.Executed + row.Executed, acc.Partial + row.Partial, acc.DecodedOnly + row.DecodedOnly, acc.Absent + row.Absent, acc.Total + row.Total));
        builder.AppendLine(CultureInfo.InvariantCulture,
            $"| **all rows** | **{totals.Executed}** | **{totals.Partial}** | **{totals.DecodedOnly}** | **{totals.Absent}** | **{totals.Total}** |");
        builder.AppendLine();
        builder.AppendLine("## Native LOC without managed equivalent (estimate)");
        builder.AppendLine();
        builder.AppendLine("| Subsystem | Native path | Native LOC | Managed LOC | Managed files |");
        builder.AppendLine("| --- | --- | ---: | ---: | ---: |");
        foreach (var row in matrix.SubsystemLoc)
        {
            var unconverted = Math.Max(0, row.NativeLoc - row.ManagedLoc);
            builder.AppendLine(CultureInfo.InvariantCulture,
                $"| {row.Subsystem} | `{row.NativePath}` | {row.NativeLoc} | {row.ManagedLoc} | {row.ManagedFiles} |");
            builder.AppendLine(CultureInfo.InvariantCulture, $"| → est. unconverted | | **{unconverted}** | | |");
        }
        builder.AppendLine();
        builder.AppendLine("Machine-readable export: [`HCDE_NATIVE_MANAGED_MATRIX.json`](HCDE_NATIVE_MANAGED_MATRIX.json), [`HCDE_NATIVE_MANAGED_MATRIX.csv`](HCDE_NATIVE_MANAGED_MATRIX.csv).");
        return builder.ToString();
    }

    public static void WriteArtifacts(SupportMatrix matrix, string docsDirectory)
    {
        Directory.CreateDirectory(docsDirectory);
        File.WriteAllText(Path.Combine(docsDirectory, "HCDE_NATIVE_MANAGED_MATRIX.md"), FormatMarkdown(matrix));
        var export = new
        {
            matrix.GeneratedAt,
            matrix.Summaries,
            matrix.SubsystemLoc,
            Entries = matrix.Entries.Select(entry => new
            {
                entry.Category,
                entry.Id,
                entry.Name,
                Status = entry.Status.ToString(),
                entry.NativeSource,
                entry.ManagedSource,
                entry.Notes,
            }),
        };
        var json = JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(docsDirectory, "HCDE_NATIVE_MANAGED_MATRIX.json"), json);
        var csv = new StringBuilder();
        csv.AppendLine("category,id,name,status,native_source,managed_source,notes");
        foreach (var entry in matrix.Entries)
        {
            static string Esc(string? value) => $"\"{(value ?? "").Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
            csv.AppendLine(string.Join(',', Esc(entry.Category), Esc(entry.Id), Esc(entry.Name), Esc(entry.Status.ToString()),
                Esc(entry.NativeSource), Esc(entry.ManagedSource), Esc(entry.Notes)));
        }
        File.WriteAllText(Path.Combine(docsDirectory, "HCDE_NATIVE_MANAGED_MATRIX.csv"), csv.ToString());
    }
}
