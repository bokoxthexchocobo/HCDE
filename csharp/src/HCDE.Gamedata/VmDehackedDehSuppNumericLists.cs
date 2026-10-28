namespace HCDE.Gamedata;

/// <summary><c>LoadDehSupp</c> <c>OrgHeights</c> and <c>CodePConv</c> lists.</summary>
public static class VmDehackedDehSuppNumericLists
{
    public static List<int> ParseIntList(IEnumerable<string> tokens)
    {
        var list = new List<int>();
        foreach (var raw in tokens)
        {
            var token = raw.Trim();
            if (token == "," || token == "}") continue;
            if (!int.TryParse(token, out var value))
                break;
            list.Add(value);
        }

        return list;
    }
}
