namespace HCDE.Gamedata;

public static class HcdeEdfDocumentScanner
{
    public static bool TryParseTopLevelBlock(string keyword, IReadOnlyList<string> bodyLines, List<string> errors)
    {
        var k = HcdeEdfParseUtilities.LowerCopy(keyword);
        switch (k)
        {
            case "thingtype":
            case "thingdelta":
                return HcdeEdfThingParser.TryParseThingBlock(keyword, bodyLines, out _);
            case "reverb":
                var rev = new HcdeEdfReverbDefinition();
                foreach (var line in bodyLines)
                    HcdeEdfReverbParser.TryApplyPropertyLine(rev, line);
                return true;
            case "include":
            case "edfinclude":
                return true;
            default:
                errors.Add($"Unknown EDF block '{keyword}'.");
                return false;
        }
    }
}
