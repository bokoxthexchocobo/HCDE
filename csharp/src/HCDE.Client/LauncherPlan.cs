namespace HCDE.Client;

/// <summary>
/// Headless launcher widget. It validates a start selection and builds the client argument list.
/// There is no Avalonia or ImGui window in this phase.
/// </summary>
public sealed class LauncherSelection
{
    public string IwadPath { get; init; } = "";
    public string? SearchPathConfigPath { get; init; }
    public IReadOnlyList<VmFileSearchPath> OrderedFileSearchPaths { get; init; } = Array.Empty<VmFileSearchPath>();
    public string? CoreScriptPath { get; init; }
    public IReadOnlyList<string>? GameTypeFilters { get; init; }
    public VmStartupGameType? GameType { get; init; }
    public string? IwadFilter { get; init; }
    public string? IwadInfoPath { get; init; }
    public string? RequiredIwadPath { get; init; }
    public IReadOnlyList<string> IwadSearchDirectories { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RecursiveIwadSearchDirectories { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> FileSearchDirectories { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RecursiveFileSearchDirectories { get; init; } = Array.Empty<string>();
    public bool LoadSupportWad { get; init; } = true;
    public IReadOnlyList<string> BlockedScriptPatterns { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> EmbeddedScriptArchives { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ScriptPaths { get; init; } = Array.Empty<string>();
    public string MapName { get; init; } = "MAP01";
    public int Skill { get; init; } = 3;
    public int Width { get; init; } = 320;
    public int Height { get; init; } = 200;
    public bool SoftwareRenderer { get; init; } = true;
    public string? ViewPreferencesPath { get; init; }
}

public static class LauncherPlan
{
    public static bool TryValidate(LauncherSelection selection, out string? error)
    {
        if (selection.SearchPathConfigPath is not null && string.IsNullOrWhiteSpace(selection.SearchPathConfigPath))
        { error = "search-config-path"; return false; }
        if (selection.OrderedFileSearchPaths is null || selection.OrderedFileSearchPaths.Any(entry =>
            entry is null || !Enum.IsDefined(entry.Kind) || string.IsNullOrWhiteSpace(entry.Value)))
        { error = "file-search-directory"; return false; }
        if (selection.IwadSearchDirectories is null || selection.RecursiveIwadSearchDirectories is null
            || selection.IwadSearchDirectories.Any(string.IsNullOrWhiteSpace)
            || selection.RecursiveIwadSearchDirectories.Any(string.IsNullOrWhiteSpace))
        {
            error = "iwad-search-directory";
            return false;
        }
        if (selection.FileSearchDirectories is null || selection.RecursiveFileSearchDirectories is null
            || selection.FileSearchDirectories.Any(string.IsNullOrWhiteSpace)
            || selection.RecursiveFileSearchDirectories.Any(string.IsNullOrWhiteSpace))
        { error = "file-search-directory"; return false; }
        if (string.IsNullOrWhiteSpace(selection.IwadPath))
        {
            error = "iwad-required";
            return false;
        }

        if (string.IsNullOrWhiteSpace(selection.MapName))
        {
            error = "map-required";
            return false;
        }

        if (selection.Skill is < 1 or > 5)
        {
            error = "skill-range";
            return false;
        }

        if (selection.Width < 16 || selection.Height < 16)
        {
            error = "view-size";
            return false;
        }

        if (selection.ViewPreferencesPath is not null && string.IsNullOrWhiteSpace(selection.ViewPreferencesPath))
        {
            error = "view-preferences-path";
            return false;
        }
        if (selection.ScriptPaths is null || selection.ScriptPaths.Any(string.IsNullOrWhiteSpace))
        {
            error = "script-path";
            return false;
        }
        if (selection.CoreScriptPath is not null && string.IsNullOrWhiteSpace(selection.CoreScriptPath))
        {
            error = "core-script-path";
            return false;
        }
        if (selection.GameTypeFilters is not null && selection.GameTypeFilters.Any(string.IsNullOrWhiteSpace))
        {
            error = "game-filter";
            return false;
        }
        if (selection.GameType is { } game && !Enum.IsDefined(game))
        {
            error = "game-type";
            return false;
        }
        if (selection.IwadInfoPath is not null && string.IsNullOrWhiteSpace(selection.IwadInfoPath))
        {
            error = "iwad-info-path";
            return false;
        }
        if (selection.RequiredIwadPath is not null && (selection.IwadInfoPath is null
            && Path.GetExtension(selection.IwadPath).ToLowerInvariant() is not (".iwad" or ".ipk3" or ".ipk7")
            || string.IsNullOrWhiteSpace(selection.RequiredIwadPath)))
        {
            error = "required-iwad-path";
            return false;
        }
        if (selection.IwadFilter is not null && (selection.GameType is null && selection.IwadInfoPath is null || string.IsNullOrWhiteSpace(selection.IwadFilter)))
        {
            error = "iwad-filter";
            return false;
        }
        if (selection.BlockedScriptPatterns is null || selection.BlockedScriptPatterns.Any(string.IsNullOrWhiteSpace))
        {
            error = "script-block";
            return false;
        }
        if (selection.EmbeddedScriptArchives is null || selection.EmbeddedScriptArchives.Any(string.IsNullOrWhiteSpace))
        {
            error = "script-embed";
            return false;
        }
        error = null;
        return true;
    }

    public static string[] BuildArgs(LauncherSelection selection)
    {
        if (!TryValidate(selection, out var error))
            throw new InvalidOperationException(error);
        string[] args =
        [
            "--iwad", selection.IwadPath,
            "--map", selection.MapName,
            "--skill", selection.Skill.ToString(),
            "--width", selection.Width.ToString(),
            "--height", selection.Height.ToString(),
            "--renderer", selection.SoftwareRenderer ? "software" : "hardware",
        ];
        var output = args.ToList();
        if (selection.CoreScriptPath is { } corePath) output.AddRange(["--core-script-file", corePath]);
        if (selection.ViewPreferencesPath is { } path) output.AddRange(["--view-preferences", path]);
        foreach (var scriptPath in selection.ScriptPaths) output.AddRange(["--script-file", scriptPath]);
        foreach (var pattern in selection.BlockedScriptPatterns) output.AddRange(["--script-block", pattern]);
        foreach (var archive in selection.EmbeddedScriptArchives) output.AddRange(["--script-embed", archive]);
        if (selection.GameType is { } game) output.AddRange(["--game-type", game.ToString().ToLowerInvariant()]);
        if (selection.IwadFilter is { } iwadFilter) output.AddRange(["--iwad-filter", iwadFilter]);
        if (selection.IwadInfoPath is { } infoPath) output.AddRange(["--iwad-info", infoPath]);
        if (selection.RequiredIwadPath is { } requiredPath) output.AddRange(["--required-iwad", requiredPath]);
        foreach (var directory in selection.FileSearchDirectories) output.AddRange(["--file-search-dir", directory]);
        if (selection.SearchPathConfigPath is { } searchConfig) output.AddRange(["--search-config", searchConfig]);
        foreach (var entry in selection.OrderedFileSearchPaths)
            output.AddRange([entry.Kind switch
            {
                VmFileSearchPathKind.Path => "--search-path",
                VmFileSearchPathKind.RecursivePath => "--search-recursive-path",
                _ => "--search-path-list",
            }, entry.Value]);
        foreach (var directory in selection.RecursiveFileSearchDirectories) output.AddRange(["--file-recursive-dir", directory]);
        if (!selection.LoadSupportWad) output.Add("--no-support-wad");
        foreach (var directory in selection.IwadSearchDirectories) output.AddRange(["--iwad-search-dir", directory]);
        foreach (var directory in selection.RecursiveIwadSearchDirectories) output.AddRange(["--iwad-recursive-dir", directory]);
        if (selection.GameTypeFilters is { } filters)
        {
            output.Add("--game-filters");
            foreach (var filter in filters) output.AddRange(["--game-filter", filter]);
        }
        return output.ToArray();
    }

    public static ClientHost CreateHost(LauncherSelection selection, HCDE.Playsim.AuthoritySimulation simulation)
    {
        if (!TryValidate(selection, out var error)) throw new InvalidOperationException(error);
        VmIwadDefinition? identified = null;
        IReadOnlyList<string> companionPaths = Array.Empty<string>();
        string? dependencyPath = null;
        string? supportPath = null;
        var deletedLumps = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        var iwadExtension = Path.GetExtension(selection.IwadPath).ToLowerInvariant();
        if (selection.IwadInfoPath is not null || iwadExtension is ".iwad" or ".ipk3" or ".ipk7")
        {
            var wadBytes = File.ReadAllBytes(selection.IwadPath);
            VmIwadInfoCatalog catalog;
            if (selection.IwadInfoPath is { } catalogPath)
            {
                catalog = VmIwadInfoParser.Parse(File.ReadAllText(catalogPath));
                identified = VmIwadIdentifier.IdentifyArchive(catalog.Entries.Select(entry => entry.Definition).ToArray(), new(selection.IwadPath, wadBytes))
                    ?? throw new InvalidDataException("IWAD does not match the supplied IWADINFO catalog");
            }
            else
            {
                var embedded = VmIwadInfoParser.ReadEmbeddedArchive(new(selection.IwadPath, wadBytes));
                identified = embedded.Definition;
                catalog = new([embedded], [], [identified.Name]);
            }
            var entry = catalog.Entries.First(entry => ReferenceEquals(entry.Definition, identified));
            var configured = selection.SearchPathConfigPath is { } configPath
                ? VmSearchPathConfiguration.Parse(File.ReadAllText(configPath)) : new VmSearchPathConfiguration([], []);
            var configIwads = VmFileSearchPaths.Expand(configured.IwadPaths);
            var iwadDirectories = VmSearchPathExpansion.ExpandDirectories(selection.IwadSearchDirectories)
                .Concat(configIwads.Where(path => path.Kind == VmFileSearchPathKind.Path).Select(path => path.Value)).ToArray();
            var recursiveIwadDirectories = VmSearchPathExpansion.ExpandDirectories(selection.RecursiveIwadSearchDirectories)
                .Concat(configIwads.Where(path => path.Kind == VmFileSearchPathKind.RecursivePath).Select(path => path.Value)).ToArray();
            dependencyPath = VmIwadRequiredResource.Resolve(entry, catalog, selection.IwadPath, selection.RequiredIwadPath,
                iwadDirectories, recursiveIwadDirectories);
            if (entry.Properties.TryGetValue("deletelumps", out var deleted))
            {
                deletedLumps[Path.GetFullPath(selection.IwadPath)] = deleted;
                if (dependencyPath is not null) deletedLumps[Path.GetFullPath(dependencyPath)] = deleted;
            }
            supportPath = VmIwadSupportResource.Resolve(entry,
                [Path.GetDirectoryName(Path.GetFullPath(selection.IwadPath))!, .. iwadDirectories],
                selection.LoadSupportWad, recursiveDirectories: recursiveIwadDirectories);
            companionPaths = VmIwadCompanionResources.Resolve(entry, selection.IwadPath,
                searchDirectories: VmSearchPathExpansion.ExpandDirectories(selection.FileSearchDirectories),
                recursiveDirectories: VmSearchPathExpansion.ExpandDirectories(selection.RecursiveFileSearchDirectories),
                orderedSearchPaths: VmFileSearchPaths.Expand(selection.OrderedFileSearchPaths.Concat(configured.FilePaths).ToArray()));
        }
        string[] iwadPaths = dependencyPath is null ? [selection.IwadPath] : [dependencyPath, selection.IwadPath];
        if (supportPath is not null) iwadPaths = [supportPath, .. iwadPaths];
        string[] paths = selection.CoreScriptPath is { } corePath
            ? [corePath, .. iwadPaths, .. companionPaths, .. selection.ScriptPaths]
            : [.. iwadPaths, .. companionPaths, .. selection.ScriptPaths];
        paths = paths.DistinctBy(path => Path.GetFullPath(path).Replace('\\', '/'), StringComparer.OrdinalIgnoreCase).ToArray();
        IReadOnlyList<string>? filters = (selection.GameType ?? identified?.Game) is { } game
            ? Array.AsReadOnly(VmStartupGameFilters.Build(game, selection.IwadFilter ?? identified?.AutoName ?? "")
                .Concat(selection.GameTypeFilters ?? Array.Empty<string>()).ToArray())
            : selection.GameTypeFilters;
        return new ClientHost(simulation, selection.Width, selection.Height, selection.ViewPreferencesPath, paths,
            reserveCoreContainer: selection.CoreScriptPath is null, gameTypeFilters: filters,
            blockedPatterns: selection.BlockedScriptPatterns, embeddings: selection.EmbeddedScriptArchives,
            deletedGlobalLumps: deletedLumps)
        { UseHardware = !selection.SoftwareRenderer };
    }

    public static bool TryParse(string[] args, out LauncherSelection selection, out string? error)
    {
        selection = new LauncherSelection();
        error = null;
        string? iwad = null;
        var map = "MAP01";
        var skill = 3;
        var width = 320;
        var height = 200;
        var software = true;
        string? preferencesPath = null;
        string? coreScriptPath = null;
        var scriptPaths = new List<string>();
        var blockedPatterns = new List<string>();
        var embeddings = new List<string>();
        List<string>? gameFilters = null;
        VmStartupGameType? gameType = null;
        string? iwadFilter = null;
        string? iwadInfoPath = null;
        string? requiredIwadPath = null;
        var fileDirectories = new List<string>();
        string? searchConfigPath = null;
        var orderedPaths = new List<VmFileSearchPath>();
        var recursiveFileDirectories = new List<string>();
        var loadSupportWad = true;
        var searchDirectories = new List<string>();
        var recursiveDirectories = new List<string>();
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--search-config":
                    if (!Take(args, ref i, out searchConfigPath) || string.IsNullOrWhiteSpace(searchConfigPath)
                        || searchConfigPath.StartsWith("--", StringComparison.Ordinal))
                    { error = "search-config-path"; return false; }
                    break;
                case "--search-path":
                case "--search-recursive-path":
                case "--search-path-list":
                    var pathKind = args[i] switch
                    {
                        "--search-path" => VmFileSearchPathKind.Path,
                        "--search-recursive-path" => VmFileSearchPathKind.RecursivePath,
                        _ => VmFileSearchPathKind.PathList,
                    };
                    if (!Take(args, ref i, out var orderedPath) || string.IsNullOrWhiteSpace(orderedPath)
                        || orderedPath.StartsWith("--", StringComparison.Ordinal))
                    { error = "file-search-directory"; return false; }
                    orderedPaths.Add(new(pathKind, orderedPath));
                    break;
                case "--iwad-search-dir":
                case "--iwad-recursive-dir":
                    var recursive = args[i] == "--iwad-recursive-dir";
                    if (!Take(args, ref i, out var searchDirectory) || string.IsNullOrWhiteSpace(searchDirectory)
                        || searchDirectory.StartsWith("--", StringComparison.Ordinal))
                    {
                        error = "iwad-search-directory";
                        return false;
                    }
                    (recursive ? recursiveDirectories : searchDirectories).Add(searchDirectory);
                    break;
                case "--file-search-dir":
                case "--file-recursive-dir":
                    var recursiveFile = args[i] == "--file-recursive-dir";
                    if (!Take(args, ref i, out var fileDirectory) || string.IsNullOrWhiteSpace(fileDirectory)
                        || fileDirectory.StartsWith("--", StringComparison.Ordinal))
                    { error = "file-search-directory"; return false; }
                    (recursiveFile ? recursiveFileDirectories : fileDirectories).Add(fileDirectory);
                    break;
                case "--no-support-wad":
                    loadSupportWad = false;
                    break;
                case "--required-iwad":
                    if (!Take(args, ref i, out requiredIwadPath) || string.IsNullOrWhiteSpace(requiredIwadPath)
                        || requiredIwadPath.StartsWith("--", StringComparison.Ordinal))
                    {
                        error = "required-iwad-path";
                        return false;
                    }
                    break;
                case "--iwad-info":
                    if (!Take(args, ref i, out iwadInfoPath) || string.IsNullOrWhiteSpace(iwadInfoPath)
                        || iwadInfoPath.StartsWith("--", StringComparison.Ordinal))
                    {
                        error = "iwad-info-path";
                        return false;
                    }
                    break;
                case "--game-type":
                    if (!Take(args, ref i, out var gameName))
                    {
                        error = "game-type";
                        return false;
                    }
                    gameType = gameName?.ToLowerInvariant() switch
                    {
                        "doom" => VmStartupGameType.Doom,
                        "heretic" => VmStartupGameType.Heretic,
                        "hexen" => VmStartupGameType.Hexen,
                        "strife" => VmStartupGameType.Strife,
                        "chex" => VmStartupGameType.Chex,
                        _ => null,
                    };
                    if (gameType is null) { error = "game-type"; return false; }
                    break;
                case "--iwad-filter":
                    if (!Take(args, ref i, out iwadFilter) || string.IsNullOrWhiteSpace(iwadFilter)
                        || iwadFilter.StartsWith("--", StringComparison.Ordinal))
                    {
                        error = "iwad-filter";
                        return false;
                    }
                    break;
                case "--script-block":
                case "--script-embed":
                    var option = args[i];
                    if (!Take(args, ref i, out var resourceRule) || string.IsNullOrWhiteSpace(resourceRule)
                        || resourceRule.StartsWith("--", StringComparison.Ordinal))
                    {
                        error = option == "--script-block" ? "script-block" : "script-embed";
                        return false;
                    }
                    (option == "--script-block" ? blockedPatterns : embeddings).Add(resourceRule);
                    break;
                case "--game-filters":
                    gameFilters ??= new List<string>();
                    break;
                case "--game-filter":
                    if (!Take(args, ref i, out var gameFilter) || string.IsNullOrWhiteSpace(gameFilter)
                        || gameFilter.StartsWith("--", StringComparison.Ordinal))
                    {
                        error = "game-filter";
                        return false;
                    }
                    (gameFilters ??= new List<string>()).Add(gameFilter);
                    break;
                case "--core-script-file":
                    if (!Take(args, ref i, out coreScriptPath) || string.IsNullOrWhiteSpace(coreScriptPath)
                        || coreScriptPath.StartsWith("--", StringComparison.Ordinal))
                    {
                        error = "core-script-path";
                        return false;
                    }
                    break;
                case "--script-file":
                    if (!Take(args, ref i, out var scriptPath) || string.IsNullOrWhiteSpace(scriptPath) || scriptPath.StartsWith("--", StringComparison.Ordinal))
                    {
                        error = "script-path";
                        return false;
                    }
                    scriptPaths.Add(scriptPath);
                    break;
                case "--view-preferences":
                    if (!Take(args, ref i, out preferencesPath) || string.IsNullOrWhiteSpace(preferencesPath)
                        || preferencesPath.StartsWith("--", StringComparison.Ordinal))
                    {
                        error = "view-preferences-path";
                        return false;
                    }
                    break;
                case "--iwad":
                    if (!Take(args, ref i, out iwad))
                    {
                        error = "iwad-required";
                        return false;
                    }
                    break;
                case "--map":
                    if (!Take(args, ref i, out var mapArg) || string.IsNullOrWhiteSpace(mapArg))
                    {
                        error = "map-required";
                        return false;
                    }
                    map = mapArg;
                    break;
                case "--skill":
                    if (!Take(args, ref i, out var skillText) || !int.TryParse(skillText, out skill))
                    {
                        error = "skill-range";
                        return false;
                    }
                    break;
                case "--width":
                    if (!Take(args, ref i, out var widthText) || !int.TryParse(widthText, out width))
                    {
                        error = "view-size";
                        return false;
                    }
                    break;
                case "--height":
                    if (!Take(args, ref i, out var heightText) || !int.TryParse(heightText, out height))
                    {
                        error = "view-size";
                        return false;
                    }
                    break;
                case "--renderer":
                    if (!Take(args, ref i, out var renderer))
                    {
                        error = "renderer";
                        return false;
                    }
                    software = renderer == "software";
                    if (renderer is not ("software" or "hardware"))
                    {
                        error = "renderer";
                        return false;
                    }
                    break;
                default:
                    error = "unknown-arg";
                    return false;
            }
        }

        selection = new LauncherSelection
        {
            IwadPath = iwad ?? "",
            CoreScriptPath = coreScriptPath,
            GameTypeFilters = gameFilters?.AsReadOnly(),
            GameType = gameType,
            IwadFilter = iwadFilter,
            IwadInfoPath = iwadInfoPath,
            RequiredIwadPath = requiredIwadPath,
            IwadSearchDirectories = searchDirectories.AsReadOnly(),
            RecursiveIwadSearchDirectories = recursiveDirectories.AsReadOnly(),
            FileSearchDirectories = fileDirectories.AsReadOnly(),
            SearchPathConfigPath = searchConfigPath,
            OrderedFileSearchPaths = orderedPaths.AsReadOnly(),
            RecursiveFileSearchDirectories = recursiveFileDirectories.AsReadOnly(),
            LoadSupportWad = loadSupportWad,
            BlockedScriptPatterns = blockedPatterns.AsReadOnly(),
            EmbeddedScriptArchives = embeddings.AsReadOnly(),
            MapName = map,
            Skill = skill,
            Width = width,
            Height = height,
            SoftwareRenderer = software,
            ViewPreferencesPath = preferencesPath,
            ScriptPaths = scriptPaths.AsReadOnly(),
        };
        return TryValidate(selection, out error);
    }

    private static bool Take(string[] args, ref int index, out string? value)
    {
        value = null;
        if (index + 1 >= args.Length)
            return false;
        value = args[++index];
        return true;
    }
}
