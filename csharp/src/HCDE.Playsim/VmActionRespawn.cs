namespace HCDE.Playsim;

/// <summary><c>A_Respawn</c> branches from <c>p_actionfunctions.cpp</c> (2132–2194).</summary>
public static class VmActionRespawn
{
    public const int RsfFog = 1;
    public const int RsfKeepTarget = 2;
    public const int RsfTelefrag = 4;
    public const int MfSolid = 1 << 1;
    public const int MfFriendly = 1 << 26;
    public const int RfInvisible = 1 << 0;
    public const int Mf3HuntPlayers = 1 << 1;
    public const int Mf4NohatePlayers = 1 << 0;

    public static bool UseTelefragMove(int flags) => (flags & RsfTelefrag) != 0;

    public static bool ShouldClearTarget(int flags) => (flags & RsfKeepTarget) == 0;

    public static bool ShouldClearSelfTarget(object? target, object self) =>
        ReferenceEquals(target, self);

    public static bool ShouldSpawnFog(int flags) => (flags & RsfFog) != 0;

    public static int MergeFriendlyFlags(int defFlags, int selfFlags) =>
        (defFlags & ~MfFriendly) | (selfFlags & MfFriendly);

    public static int ApplySolidFlag(int flags) => flags | MfSolid;

    public static double RestoreDefaultHeight(double defaultHeight) => defaultHeight;

    public static int CopyDefFlags3(int defFlags3, int selfFlags3) =>
        (defFlags3 & ~Mf3HuntPlayers) | (selfFlags3 & Mf3HuntPlayers);

    public static int CopyDefFlags4(int defFlags4, int selfFlags4) =>
        (defFlags4 & ~Mf4NohatePlayers) | (selfFlags4 & Mf4NohatePlayers);

    public static int ClearInvisibleRenderFlag(int renderFlags) => renderFlags & ~RfInvisible;

    public static (bool ok, int health, int flags, int renderFlags) ApplyRespawnSuccess(
        bool positionOk,
        int defaultHealth,
        int defFlags,
        int selfFlags,
        int renderFlags)
    {
        if (!positionOk)
            return (false, 0, defFlags, renderFlags);

        return (
            true,
            defaultHealth,
            MergeFriendlyFlags(defFlags, selfFlags),
            ClearInvisibleRenderFlag(renderFlags));
    }
}
