namespace HCDE.Client;

/// <summary>Automap follow/grid CCMDs from <c>am_map.cpp</c> (200–212).</summary>
public static class VmAutomapFollowGridCommands
{
    public const string DefaultMarkFont = "AMMNUMx";

    public static bool ToggleFollow(bool current) => !current;

    public static bool ToggleGrid(bool current) => !current;

    public static string ResolveFollowMessageKey(bool followOn) =>
        followOn ? "AMSTR_FOLLOWON" : "AMSTR_FOLLOWOFF";

    public static string ResolveGridMessageKey(bool gridOn) =>
        gridOn ? "AMSTR_GRIDON" : "AMSTR_GRIDOFF";

    public static bool ToggleTextured(bool current) => !current;

    public static string ResolveTexturedMessageKey(bool texturedOn) =>
        texturedOn ? "AMSTR_TEXON" : "AMSTR_TEXOFF";

    public static bool ShouldRunSetMark(bool hasAutomap) => hasAutomap;
}
