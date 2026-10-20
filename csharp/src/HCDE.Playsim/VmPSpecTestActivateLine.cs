namespace HCDE.Playsim;

/// <summary><c>P_TestActivateLine</c> from <c>p_spec.cpp</c> (243–356).</summary>
public static class VmPSpecTestActivateLine
{
    public const int SpacCross = 2;
    public const int SpacMcross = 4;
    public const int SpacWalking = 8;
    public const int SpacUseThrough = 256;
    public const int SpacAnyCross = 512;
    public const int MlFirstsideonly = 1;
    public const int MlMonsterscanactivate = 2;
    public const int MlSecret = 32;
    public const int MfMissile = 1;
    public const int Mf2Pushwall = 2;
    public const int Mf4Canusewalls = 4;
    public const int Level2Laxmonsteractivation = 2;
    public const int TeleportLine = 215;
    public const int Teleport = 70;
    public const int TeleportNofog = 71;
    public const int TeleportNostop = 154;
    public const int DoorRaise = 12;
    public const int PlatDownwaitupstay = 62;
    public const int PlatDownwaitupstaylip = 206;
    public const int GenericFloor = 200;
    public const int GenericCrusher = 205;

    public static bool RejectFirstSideOnly(int flags, int side) =>
        (flags & MlFirstsideonly) != 0 && side == 1;

    public static bool RejectZeroSpecial(int special) => special == 0;

    public static bool WalkingCrossNeedsFloor(bool crossOrMcross, int activation, bool aboveFloor, bool onMobj) =>
        crossOrMcross && (activation & SpacWalking) != 0 && aboveFloor && !onMobj;

    public static int ExpandUseThrough(int lineActivation) =>
        (lineActivation & SpacUseThrough) != 0 ? lineActivation | 1 : lineActivation;

    public static bool PromoteMissileTeleport(int special, int lineActivation, int activationType, bool isMissile) =>
        special == Teleport && (lineActivation & SpacCross) != 0 && activationType == 4 && isMissile;

    public static int ExpandAnyCross(int lineActivation) =>
        (lineActivation & SpacAnyCross) != 0 ? lineActivation | SpacCross | SpacMcross : lineActivation;

    public static bool MonsterCanUseWall(int activationType, bool player, int flags4) =>
        activationType == 1 && !player && (flags4 & Mf4Canusewalls) != 0;

    public static bool MonsterCanPushWall(int activationType, bool player, int flags2) =>
        activationType == 3 && !player && (flags2 & Mf2Pushwall) != 0;

    public static bool ActivationMatches(int lineActivation, int activationType) =>
        (lineActivation & activationType) != 0;

    public static bool McrossFallback(int activationType, int lineActivation) =>
        activationType == SpacMcross && lineActivation == SpacCross;

    public static bool MonsterBlockedWithoutLax(int levelFlags2) => (levelFlags2 & Level2Laxmonsteractivation) == 0;

    public static bool MonsterBlockedDefault(bool monster, bool missile, int lineFlags, int activationType, int lineActivation) =>
        monster && !missile && (lineFlags & MlMonsterscanactivate) == 0 &&
        (activationType != SpacMcross || (lineActivation & SpacMcross) == 0);

    public static bool SecretBlocksMonsterUse(int lineFlags, int activationType) =>
        (lineFlags & MlSecret) != 0 && activationType is 1 or 3;

    public static bool DoorRaiseMonsterPath(int special, int speedArg) =>
        special == DoorRaise && speedArg < 64;

    public static bool TeleportMonsterPath(int special) =>
        special is Teleport or TeleportNofog or TeleportLine;

    public static bool PlatMonsterPath(int special) =>
        special is PlatDownwaitupstay or PlatDownwaitupstaylip;

    public static bool McrossRequiresFlag(int activationType, int lineFlags, int lineActivation) =>
        activationType == SpacMcross && (lineActivation & SpacMcross) == 0 && (lineFlags & MlMonsterscanactivate) == 0;

    public static bool BoomGeneralizedDoor(int special) =>
        special >= GenericFloor && special <= GenericCrusher;
}
