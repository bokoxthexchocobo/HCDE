namespace HCDE.Playsim;

/// <summary>Poison and target-switch gates from <c>p_interaction.cpp</c> (1660–1864).</summary>
public static class VmPInteractionPoisonSwitch
{
    public const int Mf2Invulnerable = 1;
    public const int Mf3Foilinvul = 1;
    public const int Mf6Poisonalways = 1;
    public const int Mf6Additivepoisondamage = 1;
    public const int Mf6Additivepoisonduration = 1;
    public const int MfShootable = 1;
    public const int Mf4Notargetswitch = 1;
    public const int Mf3Notarget = 1;
    public const int Mf4Nohateplayers = 1;
    public const int Mf4Quicktoretaliate = 1;
    public const int Mf5Noinfighting = 1;
    public const int Mf7Forceinfighting = 1;
    public const int Mf7Noinfightspecies = 1;
    public const int Mf7Nevertarget = 1;
    public const int CfGodmode = 1;
    public const int CfGodmode2 = 1;
    public const int Mf5Nodamage = 1;

    public static bool MirrorActorSkips(bool mirrorTarget, bool mirrorInflictor, bool mirrorSource) =>
        mirrorTarget || mirrorInflictor || mirrorSource;

    public static bool CoopVisualSkips(bool visualTarget, bool visualInflictor, bool visualSource) =>
        visualTarget || visualInflictor || visualSource;

    public static bool InvulnBlocksPoison(bool poisonAlways, bool invuln, bool foilInvul, bool isPlayer) =>
        !poisonAlways && invuln && (!foilInvul || isPlayer);

    public static bool PlayerInvulnBlocks(bool isPlayer, bool invuln) => isPlayer && invuln;

    public static bool AdditivePoisonDamage(bool flag) => flag;

    public static bool AdditivePoisonDuration(bool flag) => flag;

    public static bool OkayNullOther(bool otherNull) => otherNull;

    public static bool OkaySelf(bool same) => same;

    public static bool OkayNeverTarget(int flags7) => (flags7 & Mf7Nevertarget) != 0;

    public static bool OkayNotShootable(int flags) => (flags & MfShootable) == 0;

    public static bool OkayNoTargetSwitch(int flags4, bool hasTarget) =>
        (flags4 & Mf4Notargetswitch) != 0 && hasTarget;

    public static bool MasterMinionBlock(bool masterMatch, bool minionMatch, bool hostile, int tidHate, int otherTidHate) =>
        (masterMatch || minionMatch) && !hostile && otherTidHate == tidHate;

    public static bool InfightingSameGroup(int myGroup, int otherGroup) =>
        myGroup != 0 && myGroup == otherGroup;

    public static bool NoInfightSpecies(bool flag, bool sameSpecies) => flag && sameSpecies;

    public static bool NotargetOther(int flags3, int tidHate, int otherTid, bool hostile) =>
        (flags3 & Mf3Notarget) != 0 && otherTid != tidHate && !hostile;

    public static bool ThresholdBlocks(int threshold, int flags4) =>
        threshold != 0 && (flags4 & Mf4Quicktoretaliate) == 0;

    public static bool FriendBlocksSwitch(bool isFriend) => isFriend;

    public static int ResolveInfight(int force, int noInfight, int levelInfight) =>
        force != 0 ? 1 : noInfight != 0 ? -1 : levelInfight;

    public static bool InfightOffBlocksNonFriendly(int infight, bool otherPlayer, bool hostile) =>
        infight < 0 && !otherPlayer && !hostile;

    public static bool TeammateTidBlock(int tidHate, int otherTidHate) =>
        tidHate != 0 && tidHate == otherTidHate;

    public static bool NoHatePlayers(int flags4, bool otherPlayer) =>
        (flags4 & Mf4Nohateplayers) != 0 && otherPlayer;

    public static bool HateTargetSticky(bool hasLiveTarget, int tidHate, int targetTid, int roll, bool sight) =>
        hasLiveTarget && tidHate != 0 && targetTid == tidHate && roll < 128 && sight;

    public static bool PoisonPlayerGodmode(int cheats, int flags2, int flags5) =>
        (cheats & (CfGodmode | CfGodmode2)) != 0 || (flags2 & Mf2Invulnerable) != 0 || (flags5 & Mf5Nodamage) != 0;

    public static bool TeammatePoisonScale(bool sourcePlayer, bool teammate, double teamDamage) =>
        sourcePlayer && teammate && teamDamage != 1.0;

    public static int ClampPoisonCount(int count) => count > 100 ? 100 : count;

    public static bool PoisonPositive(int poison) => poison > 0;

    public static bool NullPoisonerUsesNone(bool poisonerNull) => poisonerNull;
}
