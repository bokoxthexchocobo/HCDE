using HCDE.Scripting;

namespace HCDE.Client;

public sealed record VmScriptPk3(string Name, byte[] Data);

/// <summary>Loads ZScript sources from ordinary, unfiltered PK3 archives.</summary>
public static class VmPk3ScriptLoader
{
    public static IReadOnlyList<VmScriptSourceSet> LoadAll(IReadOnlyList<VmScriptPk3> archives, Version? maximumVersion = null,
        IReadOnlyList<string>? gameTypeFilters = null, IReadOnlyList<string>? blockedPatterns = null,
        IReadOnlyList<string>? embeddings = null, Action<string>? archiveWarning = null)
    {
        ArgumentNullException.ThrowIfNull(archives);
        return VmArchiveScriptLoader.LoadAll(archives.Select(archive =>
        {
            ArgumentNullException.ThrowIfNull(archive);
            return new VmScriptArchive(archive.Name, archive.Data, VmScriptArchiveFormat.Pk3);
        }).ToArray(), maximumVersion, gameTypeFilters, blockedPatterns, embeddings, archiveWarning: archiveWarning);
    }
}
