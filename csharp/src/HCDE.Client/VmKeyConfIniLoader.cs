using HCDE.Gamedata;

namespace HCDE.Client;

/// <summary><c>LoadKeys</c> from <c>keysections.cpp</c>.</summary>
public static class VmKeyConfIniLoader
{
    public static void LoadKeys(
        string gameConfigName,
        string modIniName,
        bool doubleBindings,
        VmConfigFile gameConfig,
        VmKeyBindings bindings,
        VmKeyBindings doubleBindingsTable)
    {
        var section = VmKeyConfParser.BuildBindingsSectionName(gameConfigName, modIniName, doubleBindings);
        var dest = doubleBindings ? doubleBindingsTable : bindings;
        if (!gameConfig.SetSection(section)) return;
        while (gameConfig.NextInSection(out var key, out var value))
            dest.DoBind(key, value);
    }

    public static void LoadKeysForSection(
        string gameConfigName,
        VmKeyConfSection section,
        VmConfigFile gameConfig,
        VmKeyBindings bindings,
        VmKeyBindings doubleBindingsTable)
    {
        LoadKeys(gameConfigName, section.IniName, false, gameConfig, bindings, doubleBindingsTable);
        LoadKeys(gameConfigName, section.IniName, true, gameConfig, bindings, doubleBindingsTable);
    }
}
