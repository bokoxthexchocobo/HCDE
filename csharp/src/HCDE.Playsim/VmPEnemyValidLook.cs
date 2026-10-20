namespace HCDE.Playsim;

/// <summary>Look/target helpers from <c>p_enemy.cpp</c> (1365–1764).</summary>
public static class VmPEnemyValidLook
{
    public const int MfShootable = 1;
    public const int MfFriendly = 1;
    public const int Mf2Dormant = 1;
    public const int Mf3Ismonster = 1;
    public const int Mf7Nevertarget = 1;
    public const int Mf8Seefriendlymonsters = 1;

    public static bool ShootableAlive(int flags, int health, int flags2) =>
        (flags & MfShootable) != 0 && health > 0 && (flags2 & Mf2Dormant) == 0;

    public static bool NotSelf(AActorRef a, AActorRef b) => a.Id != b.Id;

    public static bool IsMonster(int flags3) => (flags3 & Mf3Ismonster) != 0;

    public static bool NeverTarget(int flags7) => (flags7 & Mf7Nevertarget) != 0;

    public static bool FriendlyOtherEnemy(bool lookeeFriendly, bool otherFriendly, bool isFriend) =>
        lookeeFriendly && otherFriendly && !isFriend;

    public static bool FriendlyRetargetShootable(bool hasTarget, bool targetNotFriendly, int targetFlags, int targetHealth, int targetFlags2) =>
        hasTarget && targetNotFriendly && ShootableAlive(targetFlags, targetHealth, targetFlags2);

    public static bool NonFriendlyKeepChecking(bool lookeeFriendly, bool otherFriendly) =>
        lookeeFriendly && !otherFriendly;

    public static bool SeeFriendlyMonsters(int flags8, bool otherFriendly) =>
        (flags8 & Mf8Seefriendlymonsters) != 0 && otherFriendly;

    public static bool MbfSkipMutualFight(bool keepChecking, bool mutualTarget, int roll, bool isFriend, int targHealth, int spawnHealth) =>
        keepChecking && mutualTarget && roll > 100 && isFriend && targHealth * 2 >= spawnHealth;

    public static bool VisibleRequired(bool keepChecking, bool visible) =>
        !keepChecking || visible;

    public static bool LookForMonstersSector(bool foundSector) => foundSector;

    public static bool LookTidMatch(int thingTid, int wantTid) => thingTid == wantTid;

    public static bool LookTidShootable(int flags, int health) => (flags & MfShootable) != 0 && health > 0;

    public static bool LookEnemySameSpecies(bool sameSpecies, bool harmSpecies) => sameSpecies && !harmSpecies;

    public static bool LookAllaround(bool allaround) => allaround;

    public static bool LookForPlayersInSector(bool found) => found;

    public static bool PlayerTargetable(bool inGame, bool notDead) => inGame && notDead;

    public static bool NoiseAlertSplash(bool splash) => splash;

    public static bool BlockSound(double maxdist) => maxdist > 0;

    public static bool RecursiveSoundPortal(bool portal) => portal;

    public static bool ValidEnemyPassesAll(
        bool shootable,
        bool notSelf,
        bool monster,
        bool neverTarget,
        bool visible) =>
        shootable && notSelf && monster && !neverTarget && visible;

    public readonly record struct ValidEnemyContext(
        int Flags,
        int Health,
        int Flags2,
        int Flags3,
        int Flags7,
        int Flags8,
        bool LookeeFriendly,
        bool OtherFriendly,
        bool IsFriend,
        bool HasOtherTarget,
        bool TargetFriendly,
        int TargetFlags,
        int TargetHealth,
        int TargetFlags2,
        bool MutualTarget,
        int SkipRoll,
        int TargHealth,
        int SpawnHealth,
        bool Visible);

    public static bool EvaluateValidEnemyInBlock(ValidEnemyContext ctx)
    {
        if (!ShootableAlive(ctx.Flags, ctx.Health, ctx.Flags2)) return false;
        if (!IsMonster(ctx.Flags3)) return false;
        if (NeverTarget(ctx.Flags7)) return false;
        var keep = false;
        if (ctx.LookeeFriendly)
        {
            if (ctx.OtherFriendly)
            {
                if (FriendlyOtherEnemy(true, true, ctx.IsFriend)) keep = true;
                else if (FriendlyRetargetShootable(ctx.HasOtherTarget, !ctx.TargetFriendly, ctx.TargetFlags, ctx.TargetHealth, ctx.TargetFlags2))
                    return false;
            }
            else keep = NonFriendlyKeepChecking(true, false);
        }
        else if (SeeFriendlyMonsters(ctx.Flags8, ctx.OtherFriendly)) keep = true;
        if (MbfSkipMutualFight(keep, ctx.MutualTarget, ctx.SkipRoll, ctx.IsFriend, ctx.TargHealth, ctx.SpawnHealth))
            return false;
        return VisibleRequired(keep, ctx.Visible);
    }

    public readonly record struct LookScanContext(
        bool SectorFound,
        bool TidMatch,
        bool Shootable,
        bool SameSpecies,
        bool HarmSpecies,
        bool Allaround,
        bool PlayerOk);

    public static bool EvaluateLookForMonsters(LookScanContext ctx) =>
        ctx.SectorFound && ctx.Shootable;

    public static bool EvaluateLookForTid(LookScanContext ctx) =>
        ctx.TidMatch && ctx.Shootable;

    public static bool EvaluateLookForEnemies(LookScanContext ctx) =>
        !LookEnemySameSpecies(ctx.SameSpecies, ctx.HarmSpecies) && ctx.Shootable;

    public static bool EvaluateLookForPlayers(LookScanContext ctx) =>
        ctx.PlayerOk && (ctx.Allaround || ctx.SectorFound);

    public static bool NoiseMarkInRange(double dist, double maxdist) => dist <= maxdist;

    public static bool SplashMarksTarget(bool splash, bool hasTarget) => splash || hasTarget;

    public static bool EmitterBlocksSound(int soundblocks) => soundblocks != 0;

    public static bool PortalRecursive(bool linked) => linked;

    public static bool LookExFov(bool useFov, double angle) => !useFov || angle < 90;

    public static bool LookExPitch(bool usePitch, double pitch) => !usePitch || Math.Abs(pitch) < 45;

    public static bool LookExMaxDist(double dist, double max) => max <= 0 || dist <= max;

    public static bool LookExAllaround(bool allaround) => allaround;

    public static bool LookExThroughPortals(bool portals) => portals;

    public static bool LookForTidClassMatch(bool match) => match;

    public static bool LookForTidFriendSkip(bool friendSkip) => !friendSkip;

    public static bool LookForEnemiesSpecies(bool same, bool harm) => !LookEnemySameSpecies(same, harm);

    public static bool LookForPlayersConsole(bool console) => console;

    public static bool LookForPlayersBot(bool bot) => !bot;

    public static bool LookForPlayersDead(bool dead) => !dead;

    public static bool LookForPlayersSpectator(bool spec) => !spec;

    public static bool ValidEnemyMirror(bool mirror) => !mirror;

    public static bool ValidEnemyCorpse(bool corpse) => !corpse;

    public static bool ValidEnemyInvisible(bool invisible) => !invisible;

    public static bool ValidEnemyNotarget(bool notarget) => !notarget;

    public static bool LookSectorEmpty(bool empty) => !empty;

    public static bool LookBlockDone(bool done) => done;

    public static bool LookPickClosest(bool closer) => closer;

    public static bool LookPickFarthest(bool farther) => farther;

    public static bool LookPickRandom(bool random) => random;

    public static bool FriendlyChainRetarget(bool keep, bool retargetOk) => keep && retargetOk;

    public static bool LookRejectDeadTarget(int health) => health <= 0;

    public static bool LookRejectDormant(int flags2) => (flags2 & Mf2Dormant) != 0;

    public static bool LookRejectNevertarget(int flags7) => NeverTarget(flags7);

    public static bool LookRejectNonMonster(int flags3) => !IsMonster(flags3);

    public static bool LookRejectNonShootable(int flags) => (flags & MfShootable) == 0;

    public static bool LookParamsNull(bool nullParams) => !nullParams;

    public static bool LookIteratorNext(bool next) => next;

    public static bool LookBreakOnFound(bool found) => found;

    public static bool LookContinueScan(bool found) => !found;
}
