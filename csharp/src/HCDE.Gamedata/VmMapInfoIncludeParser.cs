namespace HCDE.Gamedata;

public static class VmMapInfoIncludeParser
{
    public static bool TryParseInclude(IReadOnlyList<string> tokens, ref int i, List<string> includes, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected include path."); return false; }
        includes.Add(tokens[i++].Trim('"'));
        return true;
    }
}
