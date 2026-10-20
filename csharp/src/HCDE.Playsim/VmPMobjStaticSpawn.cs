namespace HCDE.Playsim;

/// <summary>Static spawn, class resolution, and mapthing flags from <c>p_mobj.cpp</c> (5630–5823).</summary>
public static class VmPMobjStaticSpawn
{
    public const int MtfAmbush = 1;
    public const int MtfDormant = 1 << 1;
    public const int MtfStandstill = 1 << 2;
    public const int MtfNoinfighting = 1 << 3;
    public const int MtfFriendly = 1 << 4;
    public const int MtfShadow = 1 << 5;
    public const int MtfAltshadow = 1 << 6;
    public const int MtfSecret = 1 << 7;
    public const int MtfNocount = 1 << 8;
    public const int MfCountkill = 1;
    public const int MfCountitem = 1 << 1;
    public const int MfFriendly = 1 << 2;
    public const int MfShadow = 1 << 3;
    public const int MfDropped = 1 << 4;
    public const int Mf4Synchronized = 1;
    public const int Mf4Standstill = 1;
    public const int Mf5Noinfighting = 1;
    public const int Mf5Countsecret = 1 << 1;
    public const int Mf2Dormant = 1;
    public const int MtfMapthing = 1;

    public static bool RejectNullType(bool isNull) => isNull;

    public static bool RejectAbstractType(bool isAbstract) => isAbstract;

    public static bool ApplyReplacement(bool allow) => allow;

    public static bool UseClientSideThinker(bool clientSide) => clientSide;

    public static bool EnableNetworkingOnServerThinker(bool clientSide) => !clientSide;

    public static bool ClientSideSpawnRequiresClientClass(bool clientSide) => clientSide;

    public static bool ClassForSpawnIsActor(bool isDescendant) => isDescendant;

    public static int LevelSpawnedTics(int tics, bool synchronized, int rngMod) =>
        tics > 0 && !synchronized ? 1 + rngMod % tics : tics;

    public static int ClearDroppedFlag(int flags, bool defaultHasDropped) =>
        defaultHasDropped ? flags : flags & ~MfDropped;

    public static int SetMapthingSpawnFlag(int spawnFlags) => spawnFlags | MtfMapthing;

    public readonly record struct HandleSpawnFlagsContext(
        int SpawnFlags,
        int Flags,
        int Flags4,
        int Flags5,
        bool DecrementMonsters,
        bool DecrementItems,
        bool IncrementSecrets);

    public static HandleSpawnFlagsResult ApplyHandleSpawnFlags(HandleSpawnFlagsContext ctx)
    {
        var flags = ctx.Flags;
        var flags4 = ctx.Flags4;
        var flags5 = ctx.Flags5;
        var monsters = 0;
        var items = 0;
        var secrets = 0;
        if ((ctx.SpawnFlags & MtfAmbush) != 0) flags |= 1 << 5;
        if ((ctx.SpawnFlags & MtfStandstill) != 0) flags4 |= Mf4Standstill;
        if ((ctx.SpawnFlags & MtfNoinfighting) != 0) flags5 |= Mf5Noinfighting;
        if ((ctx.SpawnFlags & MtfFriendly) != 0)
        {
            flags |= MfFriendly;
            if ((flags & MfCountkill) != 0)
            {
                flags &= ~MfCountkill;
                monsters = -1;
            }
        }
        if ((ctx.SpawnFlags & MtfShadow) != 0)
        {
            flags |= MfShadow;
        }
        if ((ctx.SpawnFlags & MtfSecret) != 0 && (flags5 & Mf5Countsecret) == 0)
        {
            flags5 |= Mf5Countsecret;
            secrets = 1;
        }
        if ((ctx.SpawnFlags & MtfNocount) != 0)
        {
            if ((flags & MfCountkill) != 0) { flags &= ~MfCountkill; monsters = -1; }
            if ((flags & MfCountitem) != 0) { flags &= ~MfCountitem; items = -1; }
        }
        return new HandleSpawnFlagsResult(flags, flags4, flags5, monsters, items, secrets);
    }

    public readonly record struct HandleSpawnFlagsResult(
        int Flags,
        int Flags4,
        int Flags5,
        int MonsterDelta,
        int ItemDelta,
        int SecretDelta);

    public static bool BeginPlayClearsDormant(int flags2) => (flags2 & Mf2Dormant) != 0;

    public static int ClearDormantFlag(int flags2) => flags2 & ~Mf2Dormant;

    public static bool CallBeginPlayUsesVirtual(bool hasVirtual) => hasVirtual;

    public static (int RenderStyle, double Alpha) ShadowSpawnStyle(int spawnFlags)
    {
        if ((spawnFlags & MtfShadow) != 0) return (2, 0.25);
        if ((spawnFlags & MtfAltshadow) != 0) return (3, 1.0);
        return (0, 1.0);
    }

    public static bool DormantSpawnFlag(int spawnFlags) => (spawnFlags & MtfDormant) != 0;

    public static bool NocountStripsKillAndItem(int spawnFlags) => (spawnFlags & MtfNocount) != 0;

    public static int PostBeginPlaySetsHandleNoDelay(int flags7) => flags7 | 1;

    public static bool RecreateLightsWhenAssociated(bool hasAssociations, bool stateLight) =>
        hasAssociations || stateLight;
}
