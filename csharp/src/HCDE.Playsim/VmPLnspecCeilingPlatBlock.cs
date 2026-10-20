namespace HCDE.Playsim;

/// <summary>Ceiling texture/generic, crushers, and plat movers from <c>p_lnspec.cpp</c> (836–841, 884–1065).</summary>
public static class VmPLnspecCeilingPlatBlock
{
    public const int GameHexen = 2;
    public const int GameHeretic = 3;
    public const int CrushSlowSpeedThreshold = 24;
    public const int PlatDefaultLip = 8;

    public enum GenericCeilingTarget
    {
        RaiseToHighest = 1,
        RaiseToLowest = 2,
        RaiseToNearest = 3,
        RaiseToHighestFloor = 4,
        RaiseToFloor = 5,
        RaiseByTexture = 6,
        RaiseByValue = 0,
        LowerToHighest = 1,
        LowerToLowest = 2,
        LowerToNearest = 3,
        LowerToHighestFloor = 4,
        LowerToFloor = 5,
        LowerByTexture = 6,
        LowerByValue = 0,
    }

    public enum CrushMode
    {
        Doom,
        Hexen,
        Slowdown,
    }

    public enum PlatType
    {
        PerpetualRaise,
        DownWaitUpStay,
        DownWaitUpStayStone,
        DownByValue,
        UpByValue,
        UpWaitDownStay,
        UpNearestWaitDownStay,
        RaiseAndStay,
        RaiseAndStayLockout,
        UpByValueStay,
        Toggle,
        DownToNearestFloor,
        DownToLowestCeiling,
    }

    public readonly record struct CeilingRaiseByTexture(int Tag, double Speed, int Change);

    public static CeilingRaiseByTexture PackCeilingRaiseByTexture(int tag, int speed, int change) =>
        new(tag, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.Change(change));

    public readonly record struct GenericCeilingPlan(bool RaiseMode, GenericCeilingTarget Target, int CrushOrModel, int ChangeBits);

    public static GenericCeilingPlan PlanGenericCeiling(int arg3, int arg4)
    {
        var raise = (arg4 & 8) != 0;
        var target = (GenericCeilingTarget)(arg3 == 0 ? 0 : arg3);
        var crush = (arg4 & 16) != 0 ? 20 : -1;
        var change = arg4 & 7;
        return new(raise, target, crush, change);
    }

    public static bool GenericCeilingUsesDoubleSpeed() => true;

    public readonly record struct CrusherInvoke(double DownSpeed, double UpSpeed, int Damage, int SilentFlag, CrushMode Mode);

    public static CrushMode CrusherModeForSpeeds(int downRaw, int upRaw, bool hexenVariant)
    {
        if (hexenVariant)
            return CrushMode.Hexen;
        if (downRaw <= CrushSlowSpeedThreshold && upRaw <= CrushSlowSpeedThreshold)
            return CrushMode.Slowdown;
        return CrushMode.Doom;
    }

    public static CrusherInvoke PackGenericCrusher(int down, int up, int silent, int damage) =>
        new(VmPLnspecArgMacros.Speed(down), VmPLnspecArgMacros.Speed(up), damage, silent != 0 ? 2 : 0,
            CrusherModeForSpeeds(down, up, false));

    public static CrusherInvoke PackGenericCrusher2(int down, int up, int silent, int damage) =>
        new(VmPLnspecArgMacros.Speed(down), VmPLnspecArgMacros.Speed(up), damage, silent != 0 ? 2 : 0, CrushMode.Hexen);

    public static CrusherInvoke PackGenericCrusherDist(int down, int up, int silent, int damage) =>
        new(VmPLnspecArgMacros.Speed(down), VmPLnspecArgMacros.Speed(up), damage, silent != 0 ? 2 : 0,
            CrusherModeForSpeeds(down, up, false));

    public static int CrusherDistArg() => 8;

    public readonly record struct PlatInvoke(PlatType Type, int Tag, int HeightUnits, double Speed, int DelayTics, int Lip, int StayTx);

    public static PlatInvoke PackPerpetualRaise(int tag, int speed, int delay) =>
        new(PlatType.PerpetualRaise, tag, 0, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.PlatTics(delay), PlatDefaultLip, 0);

    public static PlatInvoke PackPerpetualRaiseLip(int tag, int speed, int delay, int lip) =>
        new(PlatType.PerpetualRaise, tag, 0, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.PlatTics(delay), lip, 0);

    public static bool PlatStopRemove(int arg3, int gameType)
    {
        return arg3 switch
        {
            1 => false,
            2 => true,
            _ => gameType == GameHexen,
        };
    }

    public static PlatInvoke PackDownWaitUpStay(int tag, int speed, int delay) =>
        new(PlatType.DownWaitUpStay, tag, 0, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.PlatTics(delay), PlatDefaultLip, 0);

    public static PlatInvoke PackDownWaitUpStayLip(int tag, int speed, int delay, int lip, bool stone) =>
        new(stone ? PlatType.DownWaitUpStayStone : PlatType.DownWaitUpStay, tag, 0,
            VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.PlatTics(delay), lip, 0);

    public static PlatInvoke PackDownByValue(int tag, int speed, int delay, int height) =>
        new(PlatType.DownByValue, tag, height * 8, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.PlatTics(delay), 0, 0);

    public static PlatInvoke PackUpByValue(int tag, int speed, int delay, int height) =>
        new(PlatType.UpByValue, tag, height * 8, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.PlatTics(delay), 0, 0);

    public static PlatInvoke PackUpWaitDownStay(int tag, int speed, int delay) =>
        new(PlatType.UpWaitDownStay, tag, 0, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.PlatTics(delay), 0, 0);

    public static PlatInvoke PackUpNearestWaitDownStay(int tag, int speed, int delay) =>
        new(PlatType.UpNearestWaitDownStay, tag, 0, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.PlatTics(delay), 0, 0);

    public static PlatType ResolveRaiseAndStayType(int arg3, int gameType) => arg3 switch
    {
        1 => PlatType.RaiseAndStay,
        2 => PlatType.RaiseAndStayLockout,
        _ => gameType == GameHeretic ? PlatType.RaiseAndStayLockout : PlatType.RaiseAndStay,
    };

    public static PlatInvoke PackRaiseAndStayTx0(int tag, int speed, int arg3, int gameType) =>
        new(ResolveRaiseAndStayType(arg3, gameType), tag, 0, VmPLnspecArgMacros.Speed(speed), 0, 0, 1);

    public static PlatInvoke PackUpByValueStayTx(int tag, int speed, int height) =>
        new(PlatType.UpByValueStay, tag, height * 8, VmPLnspecArgMacros.Speed(speed), 0, 0, 2);

    public static PlatInvoke PackToggleCeiling(int tag) =>
        new(PlatType.Toggle, tag, 0, 0, 0, 0, 0);

    public static PlatType ResolveGenericLiftTarget(int arg3) => arg3 switch
    {
        1 => PlatType.DownWaitUpStay,
        2 => PlatType.DownToNearestFloor,
        3 => PlatType.DownToLowestCeiling,
        4 => PlatType.PerpetualRaise,
        _ => PlatType.UpByValue,
    };

    public static PlatInvoke PackGenericLift(int tag, int speed, int delay, int target, int height) =>
        new(ResolveGenericLiftTarget(target), tag, height * 8, VmPLnspecArgMacros.Speed(speed),
            VmPLnspecArgMacros.Octics(delay), 0, 0);

    public static bool PlatReturnsTrue() => true;

    public static bool CeilingStopReturns(bool ok) => ok;
}
