namespace HCDE.Client;

/// <summary>Portable subset of <c>FGameConfigFile::DoGlobalSetup</c>.</summary>
public static class VmGameConfigGlobalSetup
{
    public static bool IsNewRelease(string? configVersion, string currentVersion) =>
        configVersion is null || !string.Equals(configVersion, currentVersion, StringComparison.Ordinal);

    public static void PurgeWeaponSlotSections(VmConfigFile config, double lastRunVersion)
    {
        if (lastRunVersion >= 208) return;
        while (config.SetFirstSection())
        {
            var name = config.GetCurrentSection();
            if (name is not null && name.EndsWith(".WeaponSlots", StringComparison.OrdinalIgnoreCase))
            {
                if (!config.DeleteCurrentSection()) break;
            }
            else if (!config.SetNextSection()) break;
        }
    }

    public static void ApplyHexenBindingFix(VmConfigFile config, double lastRunVersion)
    {
        if (lastRunVersion >= 210) return;
        if (!config.SetSection("Hexen.Bindings")) return;
        config.SetValueForKey("6", "use ArtiPork");
        config.SetValueForKey("5", "use ArtiInvulnerability2");
    }
}
