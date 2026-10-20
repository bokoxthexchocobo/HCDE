using HCDE.Scripting;

namespace HCDE.Client;

/// <summary><c>FStringTable::LoadLanguage</c> text lump parser.</summary>
public static class VmStringTableLoader
{
    public static void Load(
        VmStringTable table,
        int fileNum,
        string text,
        VmStringTableLangId langIds,
        Func<string, bool>? checkGame)
    {
        var sc = new VmFScanner();
        sc.OpenString("LANGUAGE", text);
        sc.SetCMode(true);
        var activeMaps = new List<uint>();
        while (sc.GetString())
        {
            if (sc.Compare("["))
            {
                activeMaps.Clear();
                sc.MustGetString();
                while (!sc.Compare("]"))
                {
                    if (sc.String.Length == 1 && sc.String[0] == '~')
                    {
                        sc.MustGetString();
                        continue;
                    }

                    var langTag = ReadLangTag(sc);
                    var id = langIds.GetId(langTag);
                    if (langTag == "*")
                    {
                        activeMaps.Clear();
                        activeMaps.Add(table.GlobalTableId);
                    }
                    else if (id.Normalized == table.DefaultTableId)
                    {
                        activeMaps.Clear();
                        activeMaps.Add(id.Normalized);
                    }
                    else if (activeMaps.Count != 1 || (activeMaps[0] != table.DefaultTableId && activeMaps[0] != table.GlobalTableId))
                    {
                        activeMaps.Add(id.Normalized);
                    }

                    sc.MustGetString();
                }
            }
            else
            {
                if (activeMaps.Count == 0)
                    throw new InvalidOperationException("Found a string without a language specified.");
                var skip = false;
                if (sc.Compare("$"))
                {
                    sc.MustGetStringName("ifgame");
                    sc.MustGetStringName("(");
                    sc.MustGetString();
                    skip = checkGame is not null && !checkGame(sc.String);
                    sc.MustGetStringName(")");
                    sc.MustGetString();
                }

                var label = sc.String;
                sc.MustGetStringName("=");
                sc.MustGetString();
                var strText = VmStringTableEscapes.Process(sc.String);
                sc.MustGetString();
                while (!sc.Compare(";"))
                {
                    strText += VmStringTableEscapes.Process(sc.String);
                    sc.MustGetString();
                }

                if (skip) continue;
                foreach (var map in activeMaps)
                    table.InsertString(fileNum, map, label, strText);
            }
        }
    }

    private static string ReadLangTag(VmFScanner sc)
    {
        var tag = sc.String;
        if (sc.CheckString("-"))
        {
            sc.MustGetString();
            tag += "-" + sc.String;
            if (sc.CheckString("-"))
            {
                sc.MustGetString();
                tag += "-" + sc.String;
            }
        }

        return tag;
    }
}
