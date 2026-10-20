namespace HCDE.Gamedata;

/// <summary><c>FMapInfoParser::ParseIntermissionAction</c> factory from <c>intermission_parse.cpp</c>.</summary>
public static class VmIntermissionDescriptorParser
{
    public static bool TryAddAction(string type, string body, VmIntermissionDescriptor desc, List<string> errors)
    {
        if (type.Equals("Link", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Link must be parsed outside action blocks.");
            return false;
        }

        if (type.Equals("GotoTitle", StringComparison.OrdinalIgnoreCase))
        {
            desc.Actions.Add(new VmIntermissionAction { GotoTitle = true });
            return true;
        }

        VmIntermissionAction? action = type switch
        {
            _ when type.Equals("image", StringComparison.OrdinalIgnoreCase) => new VmIntermissionAction(),
            _ when type.Equals("scroller", StringComparison.OrdinalIgnoreCase) => new VmIntermissionActionScroller(),
            _ when type.Equals("cast", StringComparison.OrdinalIgnoreCase) => new VmIntermissionActionCast(),
            _ when type.Equals("cutscene", StringComparison.OrdinalIgnoreCase) => new VmIntermissionActionCutscene(),
            _ when type.Equals("fader", StringComparison.OrdinalIgnoreCase) => new VmIntermissionActionFader(),
            _ when type.Equals("wiper", StringComparison.OrdinalIgnoreCase) => new VmIntermissionActionWiper(),
            _ when type.Equals("textscreen", StringComparison.OrdinalIgnoreCase) => new VmIntermissionActionTextscreen(),
            _ => null,
        };

        if (action is null)
        {
            errors.Add($"Unknown intermission type '{type}'.");
            return false;
        }

        var parseErrors = new List<string>();
        if (action is VmIntermissionActionCutscene cutscene)
            VmIntermissionActionParser.TryParseCutsceneBody(body, cutscene);
        else if (!VmIntermissionActionParser.TryParseBlock(body, action, parseErrors))
        {
            errors.AddRange(parseErrors);
            return false;
        }

        desc.Actions.Add(action);
        return true;
    }

    public static bool TrySetLink(string target, VmIntermissionDescriptor desc)
    {
        if (string.IsNullOrWhiteSpace(target)) return false;
        desc.Link = target;
        return true;
    }
}
