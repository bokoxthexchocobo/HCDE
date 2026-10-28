namespace HCDE.Client;

/// <summary><c>C_ArchiveCVars</c> from <c>c_cvars.cpp</c>.</summary>
public static class VmCvarArchive
{
    public static void ArchiveCvars(VmConfigFile config, VmCvarRegistry registry, uint filter)
    {
        var list = registry.All
            .Where(c => (c.Flags & VmCvarFlags.ArchiveMask) == filter)
            .OrderBy(c => c.Name, StringComparer.Ordinal)
            .ToList();
        foreach (var cv in list)
            config.SetValueForKey(cv.Name, cv.ArchiveValue);
    }
}
