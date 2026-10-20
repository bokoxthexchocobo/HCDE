namespace HCDE.Gamedata;

/// <summary><c>PatchStrings</c> line folding from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchStrings
{
    public static string FoldContinuationLines(IEnumerable<string> rawLines)
    {
        var hold = "";
        foreach (var raw in rawLines)
        {
            var line = raw.TrimEnd();
            if (line.EndsWith('\\'))
            {
                hold += line[..^1];
                continue;
            }

            hold += line;
            break;
        }

        return hold;
    }

    public static string NormalizeStringTableKey(string line1) =>
        line1.Equals("GOTREDSKULL", StringComparison.OrdinalIgnoreCase) ? "GOTREDSKUL" : line1;

    public static string ApplySpecialChars(string input) =>
        VmDehackedPatchLexer.ReplaceSpecialChars(input);
}
