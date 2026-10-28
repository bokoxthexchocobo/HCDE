namespace HCDE.Client;

/// <summary><c>FStringTable::ParseLanguageCSV</c>.</summary>
public static class VmStringTableLanguageCsv
{
    public static bool TryParse(
        VmStringTable table,
        int fileNum,
        string buffer,
        VmStringTableLangId langIds,
        Func<string, bool>? checkGame)
    {
        if (buffer.Length < 11) return false;
        if (!buffer.StartsWith("default,", StringComparison.OrdinalIgnoreCase) &&
            !buffer.StartsWith("identifier,", StringComparison.OrdinalIgnoreCase))
            return false;

        var data = VmStringTableCsv.Parse(buffer);
        var labelCol = -1;
        var filterCol = -1;
        var langRows = new List<(int Column, uint LangId)>();
        var hasDefaultEntry = false;
        if (data.Count == 0) return true;

        for (var column = 0; column < data[0].Count; column++)
        {
            var entry = data[0][column];
            if (string.Equals(entry, "filter", StringComparison.OrdinalIgnoreCase))
                filterCol = column;
            else if (string.Equals(entry, "identifier", StringComparison.OrdinalIgnoreCase))
                labelCol = column;
            else
            {
                foreach (var lang in entry.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    var tag = lang;
                    if (string.Equals(tag, "default", StringComparison.OrdinalIgnoreCase))
                    {
                        tag = "en-US";
                        hasDefaultEntry = true;
                    }

                    langRows.Add((column, langIds.GetId(tag).Normalized));
                }
            }
        }

        if (labelCol < 0) return true;

        for (var i = 1; i < data.Count; i++)
        {
            var row = data[i];
            if (filterCol >= 0 && filterCol < row.Count)
            {
                var filterStr = row[filterCol];
                if (filterStr.Length > 0)
                {
                    var ok = false;
                    if (checkGame is not null)
                    {
                        foreach (var game in filterStr.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (checkGame(game)) { ok = true; break; }
                        }
                    }

                    if (!ok) continue;
                }
            }

            if (labelCol >= row.Count) continue;
            var label = row[labelCol].Trim();
            if (label.Length == 0) continue;
            if (hasDefaultEntry) table.DeleteForLabel(fileNum, label);
            foreach (var (col, langId) in langRows)
            {
                if (col >= row.Count) continue;
                var str = row[col];
                if (str.Length > 0) table.InsertString(fileNum, langId, label, str);
                else table.DeleteString(langId, label);
            }
        }

        return true;
    }
}
