namespace HCDE.Playsim;

/// <summary>Water splash/update and <c>ConstructActor</c> from <c>p_mobj.cpp</c> (5380–5628).</summary>
public static class VmPMobjWaterSpawnConstruct
{
    public const int MfNogravity = 1;
    public const int Mf5Alwaysfast = 1;
    public const int Mf3Ismonster = 1;
    public const int Mf2Floorclip = 1;
    public const int MfFriendly = 1;
    public const int MfCountitem = 1;
    public const int Mf5Countsecret = 1;
    public const int Mf5Nointeraction = 1;
    public const int Mf8Inscrollsec = 1;
    public const int LevelNoallies = 1;
    public const double Onfloorz = -32768;
    public const double Onceilingz = -32767;
    public const double Floatrandz = -32766;

    public static bool ShouldSplashOnBoomTransition(int boomWaterLevel, int waterLevel) =>
        boomWaterLevel == 0 && waterLevel != 0;

    public readonly record struct UpdateWaterLevelFlow(bool DoSplash, bool HasPlayer, int OldLevel);

    public static bool RunSplashCheck(UpdateWaterLevelFlow flow) => flow.DoSplash;

    public static bool PlayDiveSoundsForPlayer(UpdateWaterLevelFlow flow) => flow.HasPlayer;

    public static bool PuffTypeLookupFailed(bool found) => !found;

    public static int PickSpawnRng(bool clientSide, bool botThinking) =>
        clientSide ? 1 : botThinking ? 2 : 0;

    public static bool ZeroGravitySetsFlag(double gravity) => gravity == 0;

    public static bool InstantReactionApplies(bool skillInstant, int flags5, int dmflags, int flags3) =>
        (skillInstant || (flags5 & Mf5Alwaysfast) != 0 || dmflags != 0)
        && (flags3 & Mf3Ismonster) != 0;

    public static int MonsterReactionTime(bool applies) => applies ? 0 : -1;

    public static bool IsMonster(int flags3) => (flags3 & Mf3Ismonster) != 0;

    public static bool FastMonstersSpeed(bool skillFast, double floatSpeed) =>
        skillFast && floatSpeed >= 0;

    public static bool DelayFloorFind(bool spawningMapThing, bool isPlayerPawn) =>
        spawningMapThing || !isPlayerPawn;

    public static bool NointeractionFloorCache(int flags5) => (flags5 & Mf5Nointeraction) != 0;

    public static double ResolveSpawnZ(
        double posZ,
        double floorZ,
        double ceilingZ,
        double height,
        int rngByte)
    {
        if (posZ == Onfloorz) return floorZ;
        if (posZ == Onceilingz) return ceilingZ - height;
        if (posZ != Floatrandz) return posZ;
        var space = ceilingZ - height - floorZ;
        if (space > 48)
        {
            var adjusted = space - 40;
            return adjusted * rngByte / 256.0 + floorZ + 40;
        }
        return floorZ;
    }

    public static int AssignFloatBobPhase(int phase, int rngByte) =>
        phase == 255 ? rngByte : phase;

    public static double FloorClipOrZero(bool useFloorClip, double clip) =>
        useFloorClip ? clip : 0;

    public static bool UseFloorClip(int flags2) => (flags2 & Mf2Floorclip) != 0;

    public static bool CallBeginPlayAfterConstruct(bool spawningMapThing) => !spawningMapThing;

    public static bool EuthanizeAfterBeginPlay(int objectFlags) => (objectFlags & 1) != 0;

    public static bool StripFriendlyOnNoAllies(int levelFlags, bool isPlayerPawn) =>
        (levelFlags & LevelNoallies) != 0 && !isPlayerPawn;

    public static int ClearFriendlyFlag(int flags) => flags & ~MfFriendly;

    public static bool IncrementItemCount(int flags) => (flags & MfCountitem) != 0;

    public static bool IncrementSecretCount(int flags5) => (flags5 & Mf5Countsecret) != 0;

    public static int ForceScrollerCheckFlag(int flags8) => flags8 | Mf8Inscrollsec;

    public readonly record struct ConstructActorContext(
        bool ClientSide,
        bool BotThinking,
        double Gravity,
        bool SkillInstantReaction,
        int Flags5,
        int Dmflags,
        int Flags3,
        bool SpawningMapThing,
        bool IsPlayerPawn,
        double PosZ,
        double FloorZ,
        double CeilingZ,
        double Height,
        int RngByte,
        int Flags2,
        int LevelFlags,
        bool CountsAsKill,
        int Flags,
        int Flags5Secret);

    public static ConstructActorResult EvaluateConstructTail(ConstructActorContext ctx)
    {
        var z = ResolveSpawnZ(ctx.PosZ, ctx.FloorZ, ctx.CeilingZ, ctx.Height, ctx.RngByte);
        var bob = AssignFloatBobPhase(255, ctx.RngByte);
        var clip = FloorClipOrZero(UseFloorClip(ctx.Flags2), 0);
        var flags = ctx.Flags;
        if (StripFriendlyOnNoAllies(ctx.LevelFlags, ctx.IsPlayerPawn))
            flags = ClearFriendlyFlag(flags);
        var flags8 = ForceScrollerCheckFlag(0);
        return new ConstructActorResult(z, bob, clip, flags, flags8,
            ctx.CountsAsKill,
            IncrementItemCount(ctx.Flags),
            IncrementSecretCount(ctx.Flags5Secret));
    }

    public readonly record struct ConstructActorResult(
        double SpawnZ,
        int FloatBobPhase,
        double FloorClip,
        int Flags,
        int Flags8,
        bool AddKill,
        bool AddItem,
        bool AddSecret);
}
