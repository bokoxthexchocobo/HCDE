using System.Globalization;

namespace HCDE.Gamedata;

/// <summary>Subset of <c>hcde_edf.cpp</c> thingtype / thingdelta block parsing.</summary>
public static class HcdeEdfThingParser
{
    public static bool TryParseThingBlock(string header, IEnumerable<string> bodyLines, out HcdeEdfThingDefinition? definition)
    {
        definition = null;
        var thing = new HcdeEdfThingDefinition();
        ParseThingHeader(thing, header);
        if (string.IsNullOrEmpty(thing.Name))
            return false;

        foreach (var raw in bodyLines)
        {
            var trimmed = raw.Trim();
            if (trimmed.Length == 0)
                continue;
            ParseThingPropertyLine(thing, trimmed);
        }

        definition = thing;
        return true;
    }

    private static void ParseThingHeader(HcdeEdfThingDefinition thing, string header)
    {
        var brace = header.IndexOf('{');
        var headerOnly = brace < 0 ? header : header[..brace];
        var tokens = TokenizeSimple(headerOnly);
        if (tokens.Count < 2)
            return;

        thing.Name = tokens[1];
        for (var i = 2; i < tokens.Count; i++)
        {
            if (tokens[i] != ":")
                continue;
            if (i + 1 < tokens.Count)
                thing.BaseName = tokens[i + 1];
            if (TryParseFirstIntAfter(tokens, i + 2, out var value))
                thing.DoomedNum = value;
            break;
        }
    }

    private static void ParseThingPropertyLine(HcdeEdfThingDefinition thing, string line)
    {
        var tokens = TokenizeSimple(line);
        if (tokens.Count == 0)
            return;

        var key = tokens[0].ToLowerInvariant();
        switch (key)
        {
            case "doomednum" when TryParseAssignmentInt(tokens, out var doomed):
                thing.DoomedNum = doomed;
                break;
            case "dehackednum" when TryParseAssignmentInt(tokens, out var deh):
                thing.DehackedNum = deh;
                break;
            case "spawnid" when TryParseAssignmentInt(tokens, out var spawn):
                thing.SpawnId = spawn;
                break;
            case "inherits":
                if (TryParseAssignmentValue(tokens, out var inheritName))
                    thing.BaseName = inheritName;
                break;
            case "compatname":
                if (TryParseAssignmentValue(tokens, out var compatName))
                    thing.CompatName = compatName;
                break;
            default:
                thing.HasUnsupportedBehavior = true;
                break;
        }
    }

    private static List<string> TokenizeSimple(string text)
    {
        var tokens = new List<string>();
        foreach (var part in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            tokens.Add(part);
        return tokens;
    }

    private static bool TryParseAssignmentValue(IReadOnlyList<string> tokens, out string value)
    {
        value = "";
        var start = tokens.Count > 1 && tokens[1] == "=" ? 2 : 1;
        if (start >= tokens.Count)
            return false;
        value = tokens[start].TrimEnd(';');
        return value.Length > 0;
    }

    private static bool TryParseAssignmentInt(IReadOnlyList<string> tokens, out int value)
    {
        value = -1;
        var start = tokens.Count > 1 && tokens[1] == "=" ? 2 : 1;
        return TryParseFirstIntAfter(tokens, start, out value);
    }

    private static bool TryParseFirstIntAfter(IReadOnlyList<string> tokens, int startIndex, out int value)
    {
        value = -1;
        for (var i = startIndex; i < tokens.Count; i++)
        {
            var token = tokens[i].TrimEnd(';');
            if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return true;
        }
        return false;
    }
}

public sealed class HcdeEdfThingDefinition
{
    public string Name { get; set; } = "";
    public string CompatName { get; set; } = "";
    public string BaseName { get; set; } = "";
    public int DoomedNum { get; set; } = -1;
    public int DehackedNum { get; set; } = -1;
    public int SpawnId { get; set; } = -1;
    public bool HasUnsupportedBehavior { get; set; }
}
