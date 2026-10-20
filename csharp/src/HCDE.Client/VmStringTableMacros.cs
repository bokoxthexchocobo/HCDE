namespace HCDE.Client;

/// <summary><c>FStringTable::readMacros</c> and macro expansion in <c>InsertString</c>.</summary>
public static class VmStringTableMacros
{
    public static void ReadMacros(VmStringTable table, string text)
    {
        var data = VmStringTableCsv.Parse(text);
        table.ClearMacros();
        for (var i = 1; i < data.Count; i++)
        {
            var row = data[i];
            if (row.Count < 3) continue;
            var name = row[0];
            var replacements = new string[4];
            for (var k = 0; k < 4; k++)
                replacements[k] = k + 2 < row.Count ? row[k + 2] : "";
            table.RegisterMacro(name, replacements);
        }
    }

    public static string ApplyMacros(VmStringTable table, string value)
    {
        var text = value;
        while (true)
        {
            var index = text.IndexOf("@[", StringComparison.Ordinal);
            if (index < 0) break;
            var end = text.IndexOf(']', index + 2);
            if (end < 0) break;
            var macroName = text[(index + 2)..end];
            var replacee = $"@[{macroName}]";
            if (!table.TryGetMacro(macroName, out var repl))
                repl = ["", "", "", ""];
            text = text.Replace(replacee, repl[0], StringComparison.Ordinal);
        }

        return text;
    }
}
