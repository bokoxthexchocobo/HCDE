using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary><c>P_TestActivateLine</c> from <c>p_spec.cpp</c> (243–356).</summary>
public static class VmPSpecTestActivateLine
{
    public const int SpacUse = 1;
    public const int SpacPush = 2;
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

    public static int LineActivationMask(LevelLine line)
    {
        var activation = 0;
        if (line.PlayerUse) activation |= SpacUse;
        if (line.PlayerCross) activation |= SpacCross;
        if (line.MonsterCross) activation |= SpacMcross;
        if (line.UseThrough) activation |= SpacUseThrough;
        return activation;
    }

    public static bool TestActivateLine(AuthoritySimulation sim, LevelLine line, Actor mo, int side, int activationType)
    {
        var lineActivation = LineActivationMask(line);
        if (RejectFirstSideOnly(line.Flags, side) || RejectZeroSpecial(line.Special))
            return false;

        var crossOrMcross = (activationType & (SpacCross | SpacMcross)) != 0;
        var floorZ = ActorPhysics.SectorSupportFloor(sim, mo);
        var aboveFloor = mo.Z.ToDouble() > floorZ;
        var onMobj = mo.OnMobj;
        if (WalkingCrossNeedsFloor(crossOrMcross, lineActivation, aboveFloor, onMobj))
            return false;

        lineActivation = ExpandUseThrough(lineActivation);
        if (PromoteMissileTeleport(line.Special, lineActivation, activationType, mo is ProjectileActor))
            lineActivation |= 16;
        lineActivation = ExpandAnyCross(lineActivation);

        if (activationType is SpacUse or 1024)
        {
            if (!SwitchRangeAllowsUse(sim, line, mo, side))
                return false;
        }

        if (MonsterCanUseWall(activationType, mo is PlayerPawn, 0))
            return true;
        if (MonsterCanPushWall(activationType, mo is PlayerPawn, 0))
            return true;

        if (!ActivationMatches(lineActivation, activationType))
        {
            if (!McrossFallback(activationType, lineActivation))
                return false;
        }

        if (activationType == SpacAnyCross && (lineActivation & activationType) != 0)
            return true;

        var levelFlags2 = 0;
        var monster = mo is not PlayerPawn;
        var missile = mo is ProjectileActor;
        if (MonsterBlockedDefault(monster, missile, line.Flags, activationType, lineActivation))
        {
            if (MonsterBlockedWithoutLax(levelFlags2))
            {
                if (SecretBlocksMonsterUse(line.Flags, activationType))
                    return false;

                var noway = true;
                if (activationType is SpacUse or SpacPush)
                {
                    switch (line.Special)
                    {
                        case DoorRaise when DoorRaiseMonsterPath(line.Special, line.Arg1):
                            noway = false;
                            break;
                        case Teleport or TeleportNofog:
                            noway = false;
                            break;
                    }
                }
                else if (activationType == SpacMcross)
                {
                    if ((lineActivation & SpacMcross) != 0)
                        noway = false;
                    else
                    {
                        switch (line.Special)
                        {
                            case DoorRaise when line.Arg1 >= 64:
                                break;
                            case DoorRaise:
                            case Teleport:
                            case TeleportNofog:
                            case TeleportLine:
                            case PlatDownwaitupstay:
                            case PlatDownwaitupstaylip:
                                noway = false;
                                break;
                        }
                    }
                }
                else
                    noway = false;

                return !noway;
            }
        }

        if (McrossRequiresFlag(activationType, line.Flags, lineActivation))
            return false;
        return true;
    }

    static bool SwitchRangeAllowsUse(AuthoritySimulation sim, LevelLine line, Actor mo, int side)
    {
        if ((line.Flags & LevelLine.CheckSwitchRangeFlag) == 0)
            return true;
        var user = new VmPSwitchCheckRange.User
        {
            X = mo.X.ToDouble(),
            Y = mo.Y.ToDouble(),
            Z = mo.Z.ToDouble(),
            Top = mo.Z.ToDouble() + mo.Height.ToDouble(),
            CompatFlags2 = 0,
        };
        var sample = new VmPSwitchCheckRange.Line
        {
            Flags = line.Flags,
            OneSided = line.OneSided,
            FloorZ = sim.FloorOf(mo.SectorIndex),
            CeilingZ = sim.CeilingOf(mo.SectorIndex),
            OpenRange = 999,
            OpenTop = 999,
            OpenBottom = -999,
        };
        return VmPSwitchCheckRange.CheckSwitchRange(user, sample, side, new VmPSwitchCheckRange.CheckContext(), false);
    }
}
