namespace HCDE.Gamedata;

/// <summary><c>FMapInfoParser::ParseIntermission</c> from <c>intermission_parse.cpp</c>.</summary>
public static class VmIntermissionDocumentParser
{
    public static bool TryParse(string name, string document, VmIntermissionRegistry registry, List<string> errors)
    {
        var desc = new VmIntermissionDescriptor();
        var tokens = VmIntermissionTokenize.Tokenize(document);
        var i = 0;
        while (i < tokens.Count)
        {
            if (tokens[i] == "{") { i++; continue; }
            if (tokens[i] == "}") break;
            var type = tokens[i++];
            if (type.Equals("Link", StringComparison.OrdinalIgnoreCase))
            {
                i = SkipEquals(tokens, i);
                if (i >= tokens.Count)
                {
                    errors.Add("Expected link target.");
                    return false;
                }

                desc.Link = tokens[i++];
                continue;
            }

            var body = "";
            if (i < tokens.Count && tokens[i] == "{")
            {
                i++;
                var bodyStart = i;
                var depth = 1;
                while (i < tokens.Count && depth > 0)
                {
                    if (tokens[i] == "{") depth++;
                    else if (tokens[i] == "}") depth--;
                    i++;
                }

                var bodyEnd = i - 1;
                body = string.Join(' ', tokens.Skip(bodyStart).Take(bodyEnd - bodyStart));
            }

            if (!VmIntermissionDescriptorParser.TryAddAction(type, body, desc, errors))
                return false;
        }

        registry.Replace(name, desc);
        return errors.Count == 0;
    }

    private static int SkipEquals(IReadOnlyList<string> tokens, int i)
    {
        if (i < tokens.Count && tokens[i] == "=") i++;
        return i;
    }
}
