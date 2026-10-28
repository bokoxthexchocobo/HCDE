namespace HCDE.Playsim;

/// <summary>
/// <c>FLevelLocals</c> ctor/dtor, <c>Init</c> head, and <c>CalcMapName</c> from <c>g_level.cpp</c> (**1959–2108**).
/// </summary>
public static class VmGLevelInitCtor1014Slice
{
    public const int MaxPlayers = 8;
    public const uint GameInfoMapxx = 0x00000001;
    public const uint LevelHasFadeTable = 0x00000008;
    public const int TicRate = 35;
    public const double GravityNumerator = 35.0;

    public readonly struct InitScalarsSnapshot
    {
        public double Gravity { get; init; }
        public float AirControl { get; init; }
        public float TeamDamage { get; init; }
        public uint Flags { get; init; }
        public uint Flags2 { get; init; }
        public uint Flags3 { get; init; }
        public int ImpactDecalCount { get; init; }
        public int FrozenState { get; init; }
        public bool FromSnapshot { get; init; }
        public float MusicVolume { get; init; }
        public bool HasHeightSecs { get; init; }
    }

    public readonly struct InitGlPropsSnapshot
    {
        public bool BrightFog { get; init; }
        public bool LightAdditiveSurfaces { get; init; }
        public bool NoTextureFill { get; init; }
    }

    public static bool ShouldWirePlayersArrayIndex(int playerIndex) =>
        playerIndex >= 0 && playerIndex < MaxPlayers;

    public static int CountNonNullDestructorTargets(object? localEventManager, object? aabbTree)
    {
        var count = 0;
        if (localEventManager is not null)
            count++;
        if (aabbTree is not null)
            count++;
        return count;
    }

    public static InitScalarsSnapshot CreateInitScalarReset(float svGravity, float svAirControl, float globalTeamDamage) =>
        new()
        {
            Gravity = svGravity * GravityNumerator / TicRate,
            AirControl = svAirControl,
            TeamDamage = globalTeamDamage,
            Flags = 0,
            Flags2 = 0,
            Flags3 = 0,
            ImpactDecalCount = 0,
            FrozenState = 0,
            FromSnapshot = false,
            MusicVolume = 1f,
            HasHeightSecs = false,
        };

    public static bool ShouldSetLevelHasFadeTable(int fadeto, string? fadeTableName)
    {
        if (fadeto != 0 || string.IsNullOrEmpty(fadeTableName))
            return false;
        return !fadeTableName.StartsWith("COLORMAP", StringComparison.OrdinalIgnoreCase);
    }

    public static uint ApplyLevelHasFadeTableFlag(uint flags, bool shouldSet) =>
        shouldSet ? flags | LevelHasFadeTable : flags;

    public static int ScaleWallLight(int infoLight) => infoLight * 2;

    public static double ResolveInitGravity(double svGravity, float infoGravity)
    {
        if (infoGravity == 0f)
            return svGravity * GravityNumerator / TicRate;
        if (infoGravity >= float.MaxValue * 0.5f)
            return 0;
        return infoGravity * GravityNumerator / TicRate;
    }

    public static float ResolveInitAirControl(float svAirControl, float infoAirControl) =>
        infoAirControl != 0f ? infoAirControl : svAirControl;

    public static float ResolveInitTeamDamage(float globalTeamDamage, float infoTeamDamage) =>
        infoTeamDamage != 0f ? infoTeamDamage : globalTeamDamage;

    public static uint ResolveClusterFlags(bool clusterFound, uint clusterFlags) =>
        clusterFound ? clusterFlags : 0;

    public static uint MergeLevelInfoFlags(uint levelFlags, uint infoFlags) => levelFlags | infoFlags;

    public static int ResolveAirSupplyTicks(int infoAirSupplySeconds) => infoAirSupplySeconds * TicRate;

    public static bool ResolveInitGlBool(int infoField, bool glCvarValue) =>
        infoField < 0 ? glCvarValue : infoField != 0;

    public static InitGlPropsSnapshot ResolveInitGlProps(
        int infoBrightFog,
        int infoLightAdditiveSurfaces,
        int infoNoTextureFill,
        bool glBrightFog,
        bool glLightAdditiveSurfaces,
        bool glNoTextureFill) =>
        new()
        {
            BrightFog = ResolveInitGlBool(infoBrightFog, glBrightFog),
            LightAdditiveSurfaces = ResolveInitGlBool(infoLightAdditiveSurfaces, glLightAdditiveSurfaces),
            NoTextureFill = ResolveInitGlBool(infoNoTextureFill, glNoTextureFill),
        };

    public static bool UsesMapxxNaming(uint gameInfoFlags) => (gameInfoFlags & GameInfoMapxx) != 0;

    public static string CalcMapName(uint gameInfoFlags, int episode, int level) =>
        UsesMapxxNaming(gameInfoFlags)
            ? $"MAP{level:D2}"
            : $"E{episode}M{level}";
}
