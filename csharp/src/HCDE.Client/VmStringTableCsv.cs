namespace HCDE.Client;

/// <summary><c>FStringTable::parseCSV</c> for LANGUAGE CSV lumps.</summary>
public static class VmStringTableCsv
{
    public static List<List<string>> Parse(ReadOnlySpan<char> buffer)
    {
        var data = new List<List<string>>();
        var row = new List<string>();
        var cell = new List<char>();
        var quoted = false;
        for (var i = 0; i < buffer.Length; i++)
        {
            var ch = buffer[i];
            if (ch == '"')
            {
                if (quoted && i + 1 < buffer.Length && buffer[i + 1] == '"')
                {
                    cell.Add('"');
                    i++;
                }
                else if (cell.Count == 0 || quoted) quoted = !quoted;
            }
            else if (ch == ',' && !quoted)
            {
                row.Add(VmStringTableEscapes.Process(new string(cell.ToArray())));
                cell.Clear();
            }
            else if (ch == '\r') { }
            else if (ch == '\n' && !quoted)
            {
                row.Add(VmStringTableEscapes.Process(new string(cell.ToArray())));
                data.Add(row);
                row = new List<string>();
                cell.Clear();
            }
            else cell.Add(ch);
        }

        if (cell.Count > 0 || row.Count > 0)
        {
            row.Add(VmStringTableEscapes.Process(new string(cell.ToArray())));
            data.Add(row);
        }

        return data;
    }
}
