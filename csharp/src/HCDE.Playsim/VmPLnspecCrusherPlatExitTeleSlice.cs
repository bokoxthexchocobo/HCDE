namespace HCDE.Playsim;

/// <summary>
/// Generic crushers, plat movers, exits, and teleports from <c>p_lnspec.cpp</c> (**915–1203**).
/// </summary>
public static class VmPLnspecCrusherPlatExitTeleSlice
{
    public const int CrushSlowSpeedThreshold = 24;
    public const int PlatDefaultLip = 8;
    public const int GameHexen = 2;
    public const int GameHeretic = 3;
    public const int GameStrife = 4;

    public const int TelfDestFog = 1;
    public const int TelfFdCompat = 2;
    public const int TelfSourceFog = 4;
    public const int TelfKeepVelocity = 8;
    public const int TelfKeepOrientation = 16;
    public const int TelfRotateBoom = 32;
    public const int TelfRotateBoomInverse = 64;
    public const int TelfKeepHeight = 128;

    public enum CrushMode
    {
        Doom,
        Hexen,
        Slowdown,
    }

    public enum PlatKind
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

    public readonly record struct CrusherCall(
        int Tag,
        double DownSpeed,
        double UpSpeed,
        int Dist,
        int Damage,
        int SilentFlag,
        CrushMode Mode);

    public readonly record struct PlatCall(
        PlatKind Kind,
        int Tag,
        int HeightUnits,
        double Speed,
        int DelayTics,
        int Lip,
        int StayTx);

    public sealed class LineContext
    {
        public Func<CrusherCall, bool>? EvGenericCrusher { get; init; }
        public Func<PlatCall, bool>? EvDoPlat { get; init; }
        public Func<int, bool, bool>? EvStopPlat { get; init; }
        public Func<int, bool, bool>? EvExitLevel { get; init; }
        public Func<int, bool, bool>? EvSecretExit { get; init; }
        public Func<int, bool, bool>? EvChangeLevel { get; init; }
        public Func<int, int, int, bool>? EvTeleport { get; init; }
        public Func<int, int, bool, bool>? EvTeleportOther { get; init; }
        public Func<int, int, int, bool, bool, bool>? EvTeleportGroup { get; init; }
        public Func<int, int, int, bool, int, bool>? EvTeleportSector { get; init; }
        public Func<int, int, bool>? EvSilentLineTeleport { get; init; }
        public Func<bool>? CheckExitGood { get; init; }
        public Func<int, bool>? FindLevelByNum { get; init; }
        public int GameType { get; init; }
    }

    static CrushMode CrusherModeForSpeeds(int downRaw, int upRaw, bool hexen) =>
        hexen ? CrushMode.Hexen :
        downRaw <= CrushSlowSpeedThreshold && upRaw <= CrushSlowSpeedThreshold ? CrushMode.Slowdown : CrushMode.Doom;

    static int SilentFlag(int silent) => silent != 0 ? 2 : 0;

    public static bool GenericCrusher(int tag, int down, int up, int silent, int damage, LineContext ctx) =>
        ctx.EvGenericCrusher?.Invoke(new CrusherCall(tag, VmPLnspecArgMacros.Speed(down), VmPLnspecArgMacros.Speed(up),
            0, damage, SilentFlag(silent), CrusherModeForSpeeds(down, up, false))) == true;

    public static bool GenericCrusher2(int tag, int down, int up, int silent, int damage, LineContext ctx) =>
        ctx.EvGenericCrusher?.Invoke(new CrusherCall(tag, VmPLnspecArgMacros.Speed(down), VmPLnspecArgMacros.Speed(up),
            0, damage, SilentFlag(silent), CrushMode.Hexen)) == true;

    public static bool GenericCrusherDist(int tag, int down, int up, int silent, int damage, LineContext ctx) =>
        ctx.EvGenericCrusher?.Invoke(new CrusherCall(tag, VmPLnspecArgMacros.Speed(down), VmPLnspecArgMacros.Speed(up),
            8, damage, SilentFlag(silent), CrusherModeForSpeeds(down, up, false))) == true;

    static bool DoPlat(PlatCall call, LineContext ctx) =>
        ctx.EvDoPlat?.Invoke(call) == true;

    public static bool PlatPerpetualRaise(int tag, int speed, int delay, LineContext ctx) =>
        DoPlat(new PlatCall(PlatKind.PerpetualRaise, tag, 0, VmPLnspecArgMacros.Speed(speed),
            VmPLnspecArgMacros.PlatTics(delay), PlatDefaultLip, 0), ctx);

    public static bool PlatPerpetualRaiseLip(int tag, int speed, int delay, int lip, LineContext ctx) =>
        DoPlat(new PlatCall(PlatKind.PerpetualRaise, tag, 0, VmPLnspecArgMacros.Speed(speed),
            VmPLnspecArgMacros.PlatTics(delay), lip, 0), ctx);

    public static bool PlatStop(int tag, int arg3, LineContext ctx)
    {
        var remove = arg3 switch
        {
            1 => false,
            2 => true,
            _ => ctx.GameType == GameHexen,
        };
        return ctx.EvStopPlat?.Invoke(tag, remove) == true;
    }

    public static bool PlatDownWaitUpStay(int tag, int speed, int delay, LineContext ctx) =>
        DoPlat(new PlatCall(PlatKind.DownWaitUpStay, tag, 0, VmPLnspecArgMacros.Speed(speed),
            VmPLnspecArgMacros.PlatTics(delay), PlatDefaultLip, 0), ctx);

    public static bool PlatDownWaitUpStayLip(int tag, int speed, int delay, int lip, bool stoneFloorSound, LineContext ctx) =>
        DoPlat(new PlatCall(stoneFloorSound ? PlatKind.DownWaitUpStayStone : PlatKind.DownWaitUpStay, tag, 0,
            VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.PlatTics(delay), lip, 0), ctx);

    public static bool PlatDownByValue(int tag, int speed, int delay, int height, LineContext ctx) =>
        DoPlat(new PlatCall(PlatKind.DownByValue, tag, height * 8, VmPLnspecArgMacros.Speed(speed),
            VmPLnspecArgMacros.PlatTics(delay), 0, 0), ctx);

    public static bool PlatUpByValue(int tag, int speed, int delay, int height, LineContext ctx) =>
        DoPlat(new PlatCall(PlatKind.UpByValue, tag, height * 8, VmPLnspecArgMacros.Speed(speed),
            VmPLnspecArgMacros.PlatTics(delay), 0, 0), ctx);

    public static bool PlatUpWaitDownStay(int tag, int speed, int delay, LineContext ctx) =>
        DoPlat(new PlatCall(PlatKind.UpWaitDownStay, tag, 0, VmPLnspecArgMacros.Speed(speed),
            VmPLnspecArgMacros.PlatTics(delay), 0, 0), ctx);

    public static bool PlatUpNearestWaitDownStay(int tag, int speed, int delay, LineContext ctx) =>
        DoPlat(new PlatCall(PlatKind.UpNearestWaitDownStay, tag, 0, VmPLnspecArgMacros.Speed(speed),
            VmPLnspecArgMacros.PlatTics(delay), 0, 0), ctx);

    static PlatKind ResolveRaiseAndStayType(int arg3, int gameType) => arg3 switch
    {
        1 => PlatKind.RaiseAndStay,
        2 => PlatKind.RaiseAndStayLockout,
        _ => gameType == GameHeretic ? PlatKind.RaiseAndStayLockout : PlatKind.RaiseAndStay,
    };

    public static bool PlatRaiseAndStayTx0(int tag, int speed, int arg3, LineContext ctx) =>
        DoPlat(new PlatCall(ResolveRaiseAndStayType(arg3, ctx.GameType), tag, 0, VmPLnspecArgMacros.Speed(speed), 0, 0, 1), ctx);

    public static bool PlatUpByValueStayTx(int tag, int speed, int height, LineContext ctx) =>
        DoPlat(new PlatCall(PlatKind.UpByValueStay, tag, height * 8, VmPLnspecArgMacros.Speed(speed), 0, 0, 2), ctx);

    public static bool PlatToggleCeiling(int tag, LineContext ctx) =>
        DoPlat(new PlatCall(PlatKind.Toggle, tag, 0, 0, 0, 0, 0), ctx);

    static PlatKind ResolveGenericLiftTarget(int target) => target switch
    {
        1 => PlatKind.DownWaitUpStay,
        2 => PlatKind.DownToNearestFloor,
        3 => PlatKind.DownToLowestCeiling,
        4 => PlatKind.PerpetualRaise,
        _ => PlatKind.UpByValue,
    };

    public static bool GenericLift(int tag, int speed, int delay, int target, int height, LineContext ctx) =>
        DoPlat(new PlatCall(ResolveGenericLiftTarget(target), tag, height * 8, VmPLnspecArgMacros.Speed(speed),
            VmPLnspecArgMacros.Octics(delay), 0, 0), ctx);

    public static bool ExitNormal(int position, LineContext ctx) =>
        ctx.CheckExitGood?.Invoke() == true && ctx.EvExitLevel?.Invoke(position, false) == true;

    public static bool ExitSecret(int position, LineContext ctx) =>
        ctx.CheckExitGood?.Invoke() == true && ctx.EvSecretExit?.Invoke(position, true) == true;

    public static bool TeleportNewMap(int mapNum, int position, int keepFacing, bool backSide, LineContext ctx)
    {
        if (backSide && ctx.GameType != GameStrife)
            return false;
        if (ctx.FindLevelByNum?.Invoke(mapNum) != true)
            return false;
        if (ctx.CheckExitGood?.Invoke() != true)
            return false;
        return ctx.EvChangeLevel?.Invoke(position, keepFacing != 0) == true;
    }

    public static int TeleportFlags(bool noSourceFog) =>
        TelfDestFog | TelfFdCompat | (noSourceFog ? 0 : TelfSourceFog);

    public static int TeleportNoStopFlags(bool noSourceFog) =>
        TelfDestFog | TelfKeepVelocity | (noSourceFog ? 0 : TelfSourceFog);

    public static int TeleportNoFogOrientationFlags(int useAng, bool hasLine) => useAng switch
    {
        0 => TelfKeepOrientation,
        1 => 0,
        2 => hasLine ? TelfKeepOrientation | TelfRotateBoom : 0,
        3 => hasLine ? TelfKeepOrientation | TelfRotateBoomInverse : 0,
        _ => 0,
    };

    public static int ApplyKeepHeight(int flags, bool keepHeight) =>
        keepHeight ? flags | TelfKeepHeight : flags;

    public static bool Teleport(int tid, int sectorTag, bool noSourceFog, LineContext ctx) =>
        ctx.EvTeleport?.Invoke(tid, sectorTag, TeleportFlags(noSourceFog)) == true;

    public static bool TeleportNoStop(int tid, int sectorTag, bool noSourceFog, LineContext ctx) =>
        ctx.EvTeleport?.Invoke(tid, sectorTag, TeleportNoStopFlags(noSourceFog)) == true;

    public static bool TeleportNoFog(int tid, int useAng, int sectorTag, bool keepHeight, bool hasLine, LineContext ctx)
    {
        var flags = ApplyKeepHeight(TeleportNoFogOrientationFlags(useAng, hasLine), keepHeight);
        return ctx.EvTeleport?.Invoke(tid, sectorTag, flags) == true;
    }

    public static bool TeleportZombieChanger(int tid, int sectorTag, bool hasActivator, int health, LineContext ctx) =>
        hasActivator && ctx.EvTeleport?.Invoke(tid, sectorTag, 0) == true && health >= 0;

    public static bool TeleportOther(int otherTid, int destTid, bool fog, LineContext ctx) =>
        ctx.EvTeleportOther?.Invoke(otherTid, destTid, fog) == true;

    public static bool TeleportGroup(int groupTid, int sourceTid, int destTid, bool moveSource, bool fog, LineContext ctx) =>
        ctx.EvTeleportGroup?.Invoke(groupTid, sourceTid, destTid, moveSource, fog) == true;

    public static bool TeleportInSector(int tag, int sourceTid, int destTid, bool fog, int groupTid, LineContext ctx) =>
        ctx.EvTeleportSector?.Invoke(tag, sourceTid, destTid, fog, groupTid) == true;

    public static bool TeleportEndGame(bool backSide, LineContext ctx) =>
        !backSide && ctx.CheckExitGood?.Invoke() == true && ctx.EvChangeLevel?.Invoke(0, false) == true;

    public static bool TeleportLine(int destId, int reversed, LineContext ctx) =>
        ctx.EvSilentLineTeleport?.Invoke(destId, reversed) == true;
}
