namespace HCDE.Playsim;

/// <summary><c>P_ActivateLine</c> and exit checks from <c>p_spec.cpp</c> (74–223).</summary>
public static class VmPSpecActivateLine
{
    public const int Df2KillMonsters = 1;
    public const int DfNoExit = 1;
    public const int TelefragDamage = 1000000;

    public static bool ExitGoodWhenSelfNull(object? self) => self == null;

    public static bool ExitBlockedByMonsters(bool killMonstersFlag, int killed, int total) =>
        killMonstersFlag && killed != total;

    public static bool ExitBlockedDeathmatchNoExit(bool deathmatch, bool alwaysDm, int dmflags) =>
        (deathmatch || alwaysDm) && (dmflags & DfNoExit) != 0;

    public static int ExitDamageOnBlock() => TelefragDamage;

    public static bool HubDeadPlayerBlocked(bool dead, bool multiplayer, bool hubCluster) =>
        dead && !multiplayer && hubCluster;

    public readonly record struct ActivateLinePlan(
        bool RemoteLine,
        int LockNumber,
        bool Repeat,
        int Special,
        bool KeysOk);

    public static bool RemoteSpecial(int special) =>
        special != 7 && special != 8 && (special < 11 || special > 14);

    public static ActivateLinePlan BuildActivatePlan(int special, int lockNumber, bool repeat, bool keysOk) =>
        new(RemoteSpecial(special), lockNumber, repeat, special, keysOk);

    public static bool PreActivateEventAllows(bool shouldActivate) => shouldActivate;

    public static bool ExecuteSucceeded(bool buttonSuccess) => buttonSuccess;

    public static bool ClearSpecialWhenNonRepeat(bool repeat, bool success) => !repeat && success;

    public static bool ChangeSwitchOnUse(bool success, int activationType) =>
        success && activationType is 1 or 2 or 3;

    public static bool DummySwitchCompat(
        bool levelDummySwitches,
        bool repeat,
        int special,
        bool hasTag,
        bool sectorFound) =>
        levelDummySwitches && !repeat && special != 0 && hasTag && !sectorFound;

    public static bool GeneralizedBoomSpecial(int special) =>
        special >= 200 && special <= 205;

    public static bool SpammyDeveloperLog(bool success, int developerLevel) =>
        success && developerLevel >= 3;

    public static bool RemoteActivateUsesOptionalPosition(bool hasOpt) => hasOpt;

    public static bool TestActivateFirstSideOnly(int flags, int side) =>
        (flags & 1) != 0 && side == 1;

    public static bool TestActivateZeroSpecial(int special) => special == 0;

    public static bool WalkingCrossRequiresFloor(bool onFloor, bool onMobj) =>
        onFloor || onMobj;

    public static bool UseThroughExpandsActivation(int lineActivation) =>
        (lineActivation & 256) != 0;

    public static bool MissileTeleportPromote(int special, int lineActivation, int activationType, bool isMissile) =>
        special == 70 && (lineActivation & 2) != 0 && activationType == 4 && isMissile;

    public static bool AnyCrossExpands(int lineActivation) =>
        (lineActivation & 512) != 0;

    public static bool MonsterUseWalls(int activationType, bool player, int flags4) =>
        activationType == 1 && !player && (flags4 & 4) != 0;

    public static bool MonsterPushWalls(int activationType, bool player, int flags2) =>
        activationType == 3 && !player && (flags2 & 2) != 0;

    public static bool ActivationTypeMatches(int lineActivation, int activationType) =>
        (lineActivation & activationType) != 0;

    public static bool LaxMonsterActivation(int levelFlags2) => (levelFlags2 & 2) != 0;

    public static bool SecretDoorBlocksMonster(bool secret, int activationType) =>
        secret && activationType is 1 or 3;

    public static bool DoorRaiseMonsterUse(int special, int speedArg) =>
        special == 12 && speedArg < 64;

    public static bool TeleportMonsterUse(int special) =>
        special is 70 or 71 or 215;

    public static bool PlatMonsterUse(int special) =>
        special is 62 or 206;

    public static bool McrossPromoteFromCross(int activationType, int lineActivation) =>
        activationType == 4 && lineActivation == 2;
}
