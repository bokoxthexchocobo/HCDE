namespace HCDE.Playsim;

/// <summary>Target-switch and player poison gates from <c>p_interaction.cpp</c> (1734–1980).</summary>
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

    public const int Mf7Laxtelefragdmg = 1;

    public static bool NullPlayerSkips(bool playerNull) => playerNull;

    public static bool DeadMoSkips(int health) => health <= 0;

    public static bool TelefragDamage(int damage, int threshold, bool lax) =>
        damage >= threshold && !lax;

    public static bool InvulnBlocksPoisonDamage(bool telefrag, bool god2, bool nodamage, bool invuln, bool god) =>
        god2 || nodamage || (!telefrag && (invuln || god));

    public static bool ZeroDamageAfterMods(int damage) => damage <= 0;

    public static bool AutoUseHealthGate(int damage, int health, bool autoUse, bool dm, bool altNull) =>
        damage >= health && (autoUse || dm) && altNull;

    public static bool StrifeHealthLow(bool low, bool dm) => low && !dm;

    public static bool ClampPlayerHealthNegative(int health) => health < 0;

    public static bool BuddhaSave(bool telefrag, int buddha) => telefrag && buddha == 1;

    public static bool BuddhaKeepsAlive(bool buddha) => buddha != 0;

    public static bool FlameDeathMod(bool fireMod, int health, int damage) => fireMod && health > -50 && damage > 25;

    public static bool PainSoundCadence(bool ticMask, bool play) => play && ticMask;

    public static bool VirtualOkaySwitch(bool virtualDefined) => virtualDefined;

    public static bool CallReturnsBool(int ret) => ret != 0;

    public static bool HostileOverridesMaster(bool hostile) => hostile;

    public static bool TidHateMismatch(int a, int b) => a != 0 && a == b;

    public static bool SwitcherRoll(bool sight, int roll) => sight && roll < 128;

    public static bool InfightForceOn(int flags7) => (flags7 & Mf7Forceinfighting) != 0;

    public static bool InfightForceOff(int flags5) => (flags5 & Mf5Noinfighting) != 0;

    public static bool LevelInfightDefault(int level) => level;

    public static bool NonPlayerOther(bool player) => !player;

    public static bool HostileOther(bool hostile) => hostile;

    public static bool TargetAlive(bool alive) => alive;

    public static bool PoisonerNullType(bool null) => null;

    public static bool RecordPoisonType(bool set) => set;

    public static bool RecordPainType(bool set) => set;

    public static bool TeamDamageScale(bool teammate) => teammate;

    public static bool PoisonCountAdd(int poison) => poison > 0;

    public static bool DamageFactorPlayer(double factor) => factor != 1.0;

    public static bool ModifiedDamageZero(int d) => d <= 0;

    public static bool ApplyDamageFactor(int d) => d > 0;

    public static bool MirrorHealthToPlayer(bool mirror) => mirror;

    public static bool SetAttacker(bool set) => set;

    public static bool MoHealthSubtract(bool sub) => sub;

    public static bool DieOnMoDeath(bool dead) => dead;

    public static bool Special1Damage(bool die) => die;

    public static bool AlternativeNull(bool alt) => alt == false;

    public static bool FirePoisonType(bool fire) => fire;

    public static bool UsePoisonPainType(bool custom) => custom;

    public static bool FindPainState(bool found) => found;

    public static bool SetPainState(bool set) => set;

    public static bool TelefragLaxFlag(int flags7) => (flags7 & Mf7Laxtelefragdmg) != 0;

    public static bool TrainerHalfDamage(bool trainer) => trainer;

    public static bool SkillDamageFactor(double f) => f != 1.0;

    public static bool SvDamagePlayer(double f) => f != 1.0;

    public static bool GodmodeBlocks(bool g) => g;

    public static bool Godmode2Blocks(bool g) => g;

    public static bool NodamageBlocks(bool n) => n;

    public static bool InvulnUnlessTelefrag(bool tele, bool inv) => !tele && inv;
}
