namespace HCDE.Client;

/// <summary><c>FStringTable::LoadStrings</c>.</summary>
public static class VmStringTableLoadStrings
{
    public static void Load(
        VmStringTable table,
        IVmStringTableLumpSource lumps,
        string? language,
        Func<string, bool>? checkGame = null)
    {
        table.Clear();
        table.ClearMacros();
        foreach (var lump in lumps.EnumerateLumps("LMACROS"))
            VmStringTableMacros.ReadMacros(table, lump.Text);

        foreach (var lump in lumps.EnumerateLumps("LANGUAGE"))
        {
            if (!VmStringTableLanguageCsv.TryParse(table, lump.FileNum, lump.Text, table.LangIds, checkGame))
                VmStringTableLoader.TryLoad(table, lump.FileNum, lump.Text, table.LangIds, checkGame);
        }

        table.UpdateLanguage(language);
        table.ClearMacros();
    }
}
