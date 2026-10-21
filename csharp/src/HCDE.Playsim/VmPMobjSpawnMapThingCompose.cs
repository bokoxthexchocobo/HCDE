namespace HCDE.Playsim;

/// <summary><c>SpawnMapThing</c> ShouldSpawn gate and placement/UDMF tail from <c>p_mobj.cpp</c> (6871–6881, 6942–7095).</summary>
public static class VmPMobjSpawnMapThingCompose
{
    public const int MfSpawnceiling = 1;
    public const int Mf2Spawnfloat = 1;
    public const int Mf2Floatbob = 1 << 1;
    public const int Mf2Floorclip = 1 << 2;
    public const int Mf2Argsdefined = 1 << 3;
    public const int MfNogravity = 1 << 4;
    public const int BcompatfFloatbob = 1;
    public const int StyleCount = -1;
    public const int LfSpot = 1;
    public const int LfNoshadowmap = 1 << 1;
    public const int LfDontlightactors = 1 << 2;
    public const int LfDontlightmap = 1 << 3;
    public const double Onfloorz = -1;
    public const double Onceilingz = -2;
    public const double Floatrandz = -3;

    public static double ResolveMapthingSpawnZ(int flags, int flags2)
    {
        if ((flags & MfSpawnceiling) != 0) return Onceilingz;
        if ((flags2 & Mf2Spawnfloat) != 0) return Floatrandz;
        return Onfloorz;
    }

    public static bool ApplyConversation(int conversationId, int resolvedRoot) =>
        conversationId > 0 && resolvedRoot != -1;

    public static bool ApplyAlpha(double alpha) => alpha >= 0;

    public static bool ApplyRenderStyle(int style) => style != StyleCount;

    public static bool ApplyScaleX(double scaleX) => scaleX != 0;

    public static bool ApplyScaleY(double scaleY) => scaleY != 0;

    public static bool ApplyPitch(double pitch) => pitch != 0;

    public static bool ApplyRoll(double roll) => roll != 0;

    public static bool ApplyScore(int score) => score != 0;

    public static bool ApplyFillColor(int color) => color != 0;

    public static bool ReshuffleSpotRgb(bool spotLight, bool hasArg0Str) =>
        spotLight && !hasArg0Str;

    public static int ApplyLightFlagOverride(bool defined, bool value, int flags, int flagBit) =>
        defined ? (value ? flags | flagBit : flags & ~flagBit) : flags;

    public static bool PropagateUdmfLightToAttached(bool isLight, bool hasSoftRadius) =>
        isLight && hasSoftRadius;

    public static bool LevelSpawnedWhenAlive(bool euthanizeMe) => !euthanizeMe;

    public static bool ScaleHealthFromMapthing(int mapHealth) => mapHealth > 0;

    public static int PositiveScaledHealth(int actorHealth, int mapHealth) =>
        (int)(actorHealth * mapHealth);

    public static int NegativeScaledHealth(int actorHealth, int mapHealth) =>
        - (int)(actorHealth * mapHealth);

    public static bool ScheduleDieFromSpawn(int mapHealth, bool hasDieState) =>
        mapHealth == 0 && hasDieState;

    public static bool CaptureStartHealth(int mapHealth) => mapHealth != 1;

    public static bool AddFloorZOnSpawn(double spawnZ) => spawnZ == Onfloorz;

    public static bool SubtractCeilingZOnSpawn(double spawnZ) => spawnZ == Onceilingz;

    public static bool RecordFloatbobSpecial(double spawnZ, bool floatbob, bool compat) =>
        spawnZ == Onfloorz && floatbob && compat;

    public static bool AdjustFloorClipOnSpawn(int flags2) => (flags2 & Mf2Floorclip) != 0;

    public static bool CopyMapthingSpecial(bool argsDefined) => !argsDefined;

    public static bool ZeroGravitySpawn(int gravity) => gravity == 0;
}
