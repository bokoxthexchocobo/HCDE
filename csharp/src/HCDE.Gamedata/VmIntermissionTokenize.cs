namespace HCDE.Gamedata;

internal static class VmIntermissionTokenize
{
    public static List<string> Tokenize(string text)
    {
        var tokens = new List<string>();
        var i = 0;
        while (i < text.Length)
        {
            if (char.IsWhiteSpace(text[i])) { i++; continue; }
            if (text[i] is '{' or '}' or '=' or ',')
            {
                tokens.Add(text[i].ToString());
                i++;
                continue;
            }

            if (text[i] == '"')
            {
                i++;
                var start = i;
                while (i < text.Length && text[i] != '"') i++;
                tokens.Add(text[start..i]);
                if (i < text.Length) i++;
                continue;
            }

            var j = i;
            while (j < text.Length && !char.IsWhiteSpace(text[j]) && text[j] is not '{' and not '}' and not '=' and not ',' and not '"')
                j++;
            tokens.Add(text[i..j]);
            i = j;
        }

        return tokens;
    }
}
