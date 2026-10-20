namespace HCDE.Gamedata;

/// <summary><c>PatchMisc</c> dispatch helpers from <c>d_dehacked.cpp</c> (2537–2730).</summary>
public static class VmDehackedPatchMisc
{
    public static bool LineReady(int getLineResult) => getLineResult == 1;

    public static bool HandleKeyMatched(bool handled) => handled;

    public static bool BfgCellsKey(string line1) =>
        string.Equals(line1, "BFG Cells/Shot", StringComparison.OrdinalIgnoreCase);

    public static bool RocketStyleKey(string line1) =>
        string.Equals(line1, "Rocket Explosion Style", StringComparison.OrdinalIgnoreCase);

    public static bool RocketAlphaKey(string line1) =>
        string.Equals(line1, "Rocket Explosion Alpha", StringComparison.OrdinalIgnoreCase);

    public static bool MonstersInfightKey(string line1) =>
        string.Equals(line1, "Monsters Infight", StringComparison.OrdinalIgnoreCase);

    public static bool MonstersIgnoreKey(string line1) =>
        string.Equals(line1, "Monsters Ignore Each Other", StringComparison.OrdinalIgnoreCase);

    public static bool PowerupColorPrefix(string line1) =>
        line1.StartsWith("Powerup Color ", StringComparison.OrdinalIgnoreCase);

    public static string PowerupColorSuffix(string line1) => line1.Length > 14 ? line1[14..] : string.Empty;

    public static bool KnownPowerupName(bool found) => found;

    public static bool UnknownMiscLine(bool found) => !found;

    public static int InfightingFromIgnore(bool ignore, int raw) => ignore && raw != 0 ? -1 : raw;

    public static int InfightingDirect(int raw) => raw;

    public static bool ExplosionStyleFound(int style) => style >= 0;

    public static double ParseExplosionAlpha(string line2) =>
        double.TryParse(line2, out var v) ? v : 0;

    public readonly record struct PowerupColorParse(bool Found, int R, int G, int B, float A);

    public static bool PowerupColorValid(PowerupColorParse p) => p.Found && p.A >= 0;

    public static bool NoAutofreezeKey(string key) =>
        string.Equals(key, "No Autofreeze", StringComparison.OrdinalIgnoreCase);

    public static bool StartHealthKey(string key) =>
        string.Equals(key, "Initial Health", StringComparison.OrdinalIgnoreCase);

    public static bool MaxSoulsphereKey(string key) =>
        string.Equals(key, "Max Soulsphere", StringComparison.OrdinalIgnoreCase);

    public static bool GodModeHealthKey(string key) =>
        string.Equals(key, "God Mode Health", StringComparison.OrdinalIgnoreCase);

    public static int LoopContinue(int result) => result;

    public static bool SpammyLogEnabled(bool spammy) => spammy;

    public static bool IdfaArmorKey(string key) =>
        string.Equals(key, "IDFA Armor", StringComparison.OrdinalIgnoreCase);

    public static bool IdkfaArmorClassKey(string key) =>
        string.Equals(key, "IDKFA Armor Class", StringComparison.OrdinalIgnoreCase);

    public static bool GreenArmorClassKey(string key) =>
        string.Equals(key, "Green Armor Class", StringComparison.OrdinalIgnoreCase);

    public static bool BlueArmorClassKey(string key) =>
        string.Equals(key, "Blue Armor Class", StringComparison.OrdinalIgnoreCase);

    public static bool MegasphereHealthKey(string key) =>
        string.Equals(key, "Megasphere Health", StringComparison.OrdinalIgnoreCase);

    public static int ParseIntField(string line2) =>
        int.TryParse(line2, out var v) ? v : 0;

    public static bool StripWhitespace(bool always) => always;

    public readonly record struct MiscLineLoop(int GetLineResult, bool Handled);

    public static bool ShouldContinueMiscLoop(MiscLineLoop ctx) =>
        LineReady(ctx.GetLineResult);

    public static bool ApplyHandledMisc(MiscLineLoop ctx) => ctx.Handled;

    public static bool PowerupColorRgbaParsed(int r, int g, int b, float a) =>
        r >= 0 && g >= 0 && b >= 0 && a >= 0;

    public static bool UnknownPowerupName(bool matched) => !matched;

    public static string FormatUnknownMisc(string line1) => line1;

    public static bool DehInfoOffsetValid(int offset) => offset > 0;
}
