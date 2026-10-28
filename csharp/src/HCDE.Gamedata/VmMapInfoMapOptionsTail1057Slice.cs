namespace HCDE.Gamedata;

/// <summary>
/// `DEFINE_MAP_OPTION` continuation from <c>g_mapinfo.cpp</c> (**1341–1490**). Follows **1028** (**1191–1340**).
/// Uncredited <see cref="VmMapInfoMapOptionsMedia1028Slice"/> wall-shade clamp — seam only.
/// </summary>
public static class VmMapInfoMapOptionsTail1057Slice
{
    public const int NoSound = -1;
    public const double NoGravityMarker = double.MaxValue;

    public sealed class SpecialActionDraft
    {
        public string TypeName { get; set; } = "";
        public int Action { get; set; }
        public int MinArg { get; set; }
        public int[] Args { get; } = new int[5];
        public int ArgCount { get; set; }
    }

    public sealed class MapOptionsTailDraft
    {
        public int WallHorizLight { get; set; }
        public double Gravity { get; set; }
        public double AirControl { get; set; }
        public int AirSupply { get; set; }
        public string ExitPic { get; set; } = "";
        public string EnterPic { get; set; } = "";
        public string ExitAnim { get; set; } = "";
        public string EnterAnim { get; set; } = "";
        public List<int> PrecacheSoundIds { get; } = new();
        public List<string> EventHandlers { get; } = new();
        public List<string> PrecacheTextures { get; } = new();
        public List<string> PrecacheClasses { get; } = new();
        public string RedirectTypeToken { get; set; } = "";
    }

    public static sbyte ApplyHorizWallShade(int rawNumber) =>
        VmMapInfoMapOptionsMedia1028Slice.ClampWallShade(rawNumber);

    public static void ApplyGravityFromParse(MapOptionsTailDraft draft, double value) => draft.Gravity = value;

    public static void ApplyNoGravity(MapOptionsTailDraft draft) => draft.Gravity = NoGravityMarker;

    public static void ApplyAirControl(MapOptionsTailDraft draft, double value) => draft.AirControl = value;

    public static void ApplyAirSupply(MapOptionsTailDraft draft, int value) => draft.AirSupply = value;

    public static void StoreStringMapOption(ref string destination, string? token) =>
        destination = token ?? string.Empty;

    public static bool SpecialActionLookupFailed(int action, int minArg) => action == 0 || minArg < 0;

    public static int CountTrailingSpecialActionArgs(ReadOnlySpan<bool> commaAfterNumber)
    {
        var count = 0;
        foreach (var comma in commaAfterNumber)
        {
            if (!comma || count >= 5)
                break;
            count++;
        }

        return count;
    }

    public static bool ShouldPushPrecacheSound(int soundId) => soundId != NoSound;

    public static int CountCommaSeparatedEntries(ReadOnlySpan<bool> trailingCommas)
    {
        var count = 1;
        foreach (var comma in trailingCommas)
        {
            if (!comma)
                break;
            count++;
        }

        return count;
    }

    public static bool BeginRedirectOption(MapOptionsTailDraft draft, string redirectTypeToken)
    {
        draft.RedirectTypeToken = redirectTypeToken;
        return redirectTypeToken.Length > 0;
    }

    public enum MapOptionTailKeyword
    {
        Unknown,
        HorizWallShade,
        Gravity,
        NoGravity,
        AirControl,
        AirSupply,
        InterPic,
        ExitPic,
        EnterPic,
        ExitAnim,
        EnterAnim,
        SpecialAction,
        PrecacheSounds,
        EventHandlers,
        PrecacheTextures,
        PrecacheClasses,
        Redirect,
    }

    public static MapOptionTailKeyword ClassifyTailOptionKeyword(string keyword)
    {
        return keyword.ToLowerInvariant() switch
        {
            "horizwallshade" => MapOptionTailKeyword.HorizWallShade,
            "gravity" => MapOptionTailKeyword.Gravity,
            "nogravity" => MapOptionTailKeyword.NoGravity,
            "aircontrol" => MapOptionTailKeyword.AirControl,
            "airsupply" => MapOptionTailKeyword.AirSupply,
            "interpic" => MapOptionTailKeyword.InterPic,
            "exitpic" => MapOptionTailKeyword.ExitPic,
            "enterpic" => MapOptionTailKeyword.EnterPic,
            "exitanim" => MapOptionTailKeyword.ExitAnim,
            "enteranim" => MapOptionTailKeyword.EnterAnim,
            "specialaction" => MapOptionTailKeyword.SpecialAction,
            "precachesounds" => MapOptionTailKeyword.PrecacheSounds,
            "eventhandlers" => MapOptionTailKeyword.EventHandlers,
            "precachetextures" => MapOptionTailKeyword.PrecacheTextures,
            "precacheclasses" => MapOptionTailKeyword.PrecacheClasses,
            "redirect" => MapOptionTailKeyword.Redirect,
            _ => MapOptionTailKeyword.Unknown,
        };
    }
}
