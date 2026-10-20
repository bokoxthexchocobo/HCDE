namespace HCDE.Playsim;

/// <summary><c>P_UnPredictClient</c> and boss-exit tail from <c>p_user.cpp</c> / <c>p_enemy.cpp</c>.</summary>
public static class VmPUserUnpredictBoss
{
    public const int CfPredicting = 1;
    public const int Maxplayers = 64;

    public static bool UnpredictAlreadyDisabled(bool predicting) => !predicting;

    public static bool ClearPredictingCheatInRange(int consoleplayer) =>
        consoleplayer >= 0 && consoleplayer < Maxplayers;

    public static bool CaptureWeaponBeforeRollback(bool predictWeapon) => predictWeapon;

    public static bool RestoreWeaponAfterRollback(bool predictWeapon, bool inRange) =>
        predictWeapon && inRange;

    public static bool RollbackReaderOpened(bool opened) => opened;

    public static bool ClearPredictingAfterRollback(bool inRange) => inRange;

    public static bool BossExitSpeckillMonsters(int levelFlags, int flag) => (levelFlags & flag) != 0;

    public static bool BossExitMap07Special(int levelFlags, int map07Flag) =>
        (levelFlags & map07Flag) != 0;

    public static bool BossMap07Boss1(int flags8, int boss1) => (flags8 & boss1) != 0;

    public static bool BossMap07Boss2(int flags8, int boss2) => (flags8 & boss2) != 0;

    public static bool BossSamereplacement(bool typeFatsoOrArach, bool bothFlagged, bool sameReplace) =>
        typeFatsoOrArach && bothFlagged && sameReplace;

    public static bool BossNoExitDmflags(bool deathmatch, bool alwaysDmflags, int dmflags, int noExit) =>
        (deathmatch || alwaysDmflags) && (dmflags & noExit) != 0;

    public static bool MassacreSkipsDormant(int flags2, int dormant) => (flags2 & dormant) != 0;

    public static bool MassacreRequiresMonster(int flags3, int isMonster) => (flags3 & isMonster) != 0;

    public static bool MassacreFriendlyFilter(bool baddiesOnly, int flags, int friendly) =>
        !baddiesOnly || (flags & friendly) == 0;

    public static bool BossSpecActionsMask(int levelFlags, int mask) => (levelFlags & mask) != 0;

    public readonly record struct BossFloorDoorPick(int SpecMask, int SpecLower, int SpecLowerHighest, int SpecOpenDoor);

    public static bool TryPickBossSpecAction(BossFloorDoorPick ctx, out int actionKind)
    {
        actionKind = 0;
        var masked = ctx.SpecMask;
        if (masked == ctx.SpecLower)
        {
            actionKind = 1;
            return true;
        }
        if (masked == ctx.SpecLowerHighest)
        {
            actionKind = 2;
            return true;
        }
        if (masked == ctx.SpecOpenDoor)
        {
            actionKind = 3;
            return true;
        }
        return false;
    }

    public readonly record struct MassacreActorGate(
        int Flags2,
        int Flags3,
        int Flags,
        int Dormant,
        int IsMonster,
        int Friendly,
        bool BaddiesOnly);

    public static bool MassacreActorEligible(MassacreActorGate g) =>
        !MassacreSkipsDormant(g.Flags2, g.Dormant)
        && MassacreRequiresMonster(g.Flags3, g.IsMonster)
        && MassacreFriendlyFilter(g.BaddiesOnly, g.Flags, g.Friendly);

    public static bool ClearPredictionDisablesNet(bool predicting) => predicting;

    public static bool ClearPredictionResetsPos(bool always) => always;

    public static bool ClearPredictionClearsBackup(bool always) => always;

    public static bool ClearPredictionResetsFlag(bool always) => always;

    public static bool ClearPredictionUsesAuthoritativeBase(bool baseValid) => baseValid;

    public static int ClearPredictionLastTicFromBase(int baseSequence) => Math.Max(baseSequence, 0);

    public static int ClearPredictionLastTicFromGametic(int gametic) => gametic;

    public static bool ClearPredictionClearsCheat(bool inRange) => inRange;

    public static bool ClearPredictionLogsDebug(bool debugEnabled) => debugEnabled;

    public readonly record struct ClearPredictionSeatContext(
        bool InRange,
        bool BaseValid,
        int BaseSequence,
        int Gametic);

    public static int ResolveLastPredictedTic(ClearPredictionSeatContext ctx) =>
        !ctx.InRange ? 0
        : ctx.BaseValid ? ClearPredictionLastTicFromBase(ctx.BaseSequence)
        : ClearPredictionLastTicFromGametic(ctx.Gametic);
}
