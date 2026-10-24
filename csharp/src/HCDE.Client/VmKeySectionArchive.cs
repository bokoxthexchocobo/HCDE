namespace HCDE.Client;

/// <summary><c>M_SaveCustomKeys</c> from <c>keysections.cpp</c>.</summary>
public static class VmKeySectionArchive
{
    public sealed class KeySection(string section, IReadOnlyList<string> actions)
    {
        public string Section { get; } = section;
        public IReadOnlyList<string> Actions { get; } = actions;
    }

    public static void SaveCustomKeys(
        VmConfigFile config,
        string gameName,
        IReadOnlyList<KeySection> sections,
        VmKeyBindings bindings,
        VmKeyBindings doubleBindings)
    {
        foreach (var section in sections)
        {
            SaveSection(config, VmGameConfigGameSetup.GameSection(gameName, $"{section.Section}.Bindings"), section, bindings);
            SaveSection(config, VmGameConfigGameSetup.GameSection(gameName, $"{section.Section}.DoubleBindings"), section, doubleBindings);
        }
    }

    private static void SaveSection(
        VmConfigFile config,
        string fullSection,
        KeySection keySection,
        VmKeyBindings bindings)
    {
        config.SetSection(fullSection, allowCreate: true);
        config.ClearCurrentSection();
        foreach (var action in keySection.Actions)
            bindings.ArchiveBindings(config, action);
    }
}
