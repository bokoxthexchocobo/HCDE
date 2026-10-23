using HCDE.Gamedata;

namespace HCDE.Playsim;

/// <summary>Cajun bot movement from <c>b_move.cpp</c>.</summary>
public static class VmBBotMove
{
    public const int DiEast = 0;
    public const int DiNortheast = 1;
    public const int DiNorth = 2;
    public const int DiNorthwest = 3;
    public const int DiWest = 4;
    public const int DiSouthwest = 5;
    public const int DiSouth = 6;
    public const int DiSoutheast = 7;
    public const int DiNodir = 8;

    public const int ForwardRun = 0x3200;
    public const int BtUse = 1 << 1;
    public const int BtJump = 1 << 2;

    public const int DoorLockedRaise = 13;
    public const int GenericDoor = 202;
    public const int SpacUse = 1;
    public const int SpacPush = 1 << 1;

    public const int MfNoclip = 1 << 6;
    public const int MfMissile = 1 << 14;
    public const int MfTeleport = 1 << 18;
    public const int MfDropoff = 1 << 21;
    public const int MfFloat = 1 << 24;

    public const int BifBotExplosive = 2;

    public const double SqrtHalf = 0.70710678118654752440084436210485;
    public static readonly double[] XSpeed = [1, SqrtHalf, 0, -SqrtHalf, -1, -SqrtHalf, 0, SqrtHalf];
    public static readonly double[] YSpeed = [0, SqrtHalf, 1, SqrtHalf, 0, -SqrtHalf, -1, -SqrtHalf];

    private static readonly int[] Opposite = [DiWest, DiSouthwest, DiSouth, DiSoutheast, DiEast, DiNortheast, DiNorth, DiNorthwest, DiNodir];
    private static readonly int[] Diags = [DiNorthwest, DiNortheast, DiSouthwest, DiSoutheast];

    public sealed class UserCmd
    {
        public int Buttons { get; set; }
        public int ForwardMove { get; set; }
        public int SideMove { get; set; }
    }

    public sealed record BotWeaponInfo(int Flags, string? ProjectileTypeName, int MoveCombatDist);

    public sealed class Mover
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Yaw { get; set; }
        public double Pitch { get; set; }
        public int Movedir { get; set; }
        public int Movecount { get; set; }
        public int Flags { get; set; }
        public double Height { get; set; }
        public double MaxStepHeight { get; set; }
        public double MaxDropOffHeight { get; set; }
        public double Top() => Z + Height;
        public Actor? BlockingLine { get; set; }
        public double SectorFloorZ { get; set; }
    }

    public sealed class BotMoveState
    {
        public required AuthoritySimulation Sim { get; init; }
        public required PlayerPawn Player { get; init; }
        public Mover Mo { get; } = new();
        public Actor? Dest { get; set; }
        public Actor? Enemy { get; set; }
        public Actor? Missile { get; set; }
        public double Angle { get; set; }
        public int AngleBams { get; set; }
        public int TRoam { get; set; }
        public BotWeaponInfo ReadyWeapon { get; set; } = new(0, null, 0);
        public readonly List<BotLineHit> Spechit = new();
    }

    public sealed record BotLineHit(int Special, int[] Args, Actor? LineActor, bool IsBlockingLine);

    public static void Roam(BotMoveState bot, UserCmd cmd)
    {
        if (bot.Dest is not null && bot.Sim.CajunReachable(bot.Player, bot.Dest))
        {
            bot.Angle = AngleTo(bot.Mo, bot.Dest!);
            bot.AngleBams = DegreesToBams(bot.Angle);
        }
        else if (bot.Mo.Movedir < 8)
        {
            var angle = bot.AngleBams & (7 << 29);
            var delta = angle - (bot.Mo.Movedir << 29);
            if (delta > 0)
                bot.Angle -= 45;
            else if (delta < 0)
                bot.Angle += 45;
        }

        if (--bot.Mo.Movecount < 0 || !Move(bot, cmd))
            NewChaseDir(bot, cmd);
    }

    public static bool Move(BotMoveState bot, UserCmd cmd)
    {
        var mo = bot.Mo;
        if (mo.Movedir >= DiNodir)
        {
            mo.Movedir = DiNodir;
            return false;
        }

        var tryx = mo.X + 8 * XSpeed[mo.Movedir];
        var tryy = mo.Y + 8 * YSpeed[mo.Movedir];
        var tryOk = CajunCleanAhead(bot.Sim, bot.Player, bot.Mo, tryx, tryy, cmd);

        if (!tryOk)
        {
            if (bot.Spechit.Count == 0)
                return false;

            mo.Movedir = DiNodir;
            var good = 0;
            while (bot.Spechit.Count > 0)
            {
                var hit = bot.Spechit[^1];
                bot.Spechit.RemoveAt(bot.Spechit.Count - 1);
                var tryit = true;
                if (hit.Special == DoorLockedRaise && !CajunCheckKeys(bot.Player, hit.Args.ElementAtOrDefault(3), 0, false))
                    tryit = false;
                else if (hit.Special == GenericDoor && !CajunCheckKeys(bot.Player, hit.Args.ElementAtOrDefault(4), 0, false))
                    tryit = false;

                if (tryit &&
                    (CajunTestActivateLine(bot, hit, SpacUse) ||
                     CajunTestActivateLine(bot, hit, SpacPush)))
                {
                    good |= hit.LineActor == mo.BlockingLine ? 1 : 2;
                }
            }

            if (good != 0 && (bot.Sim.BotOpenDoorRandom() >= 203) ^ (good & 1) != 0)
            {
                cmd.Buttons |= BtUse;
                cmd.ForwardMove = ForwardRun;
                return true;
            }

            return false;
        }

        cmd.ForwardMove = ForwardRun;
        return true;
    }

    public static bool TryWalk(BotMoveState bot, UserCmd cmd)
    {
        if (!Move(bot, cmd))
            return false;
        bot.Mo.Movecount = bot.Sim.BotTryWalkRandom() & 60;
        return true;
    }

    public static void NewChaseDir(BotMoveState bot, UserCmd cmd)
    {
        if (bot.Dest == null)
            return;

        var mo = bot.Mo;
        var olddir = mo.Movedir;
        var turnaround = Opposite[olddir];
        var deltaX = bot.Dest.X.ToDouble() - mo.X;
        var deltaY = bot.Dest.Y.ToDouble() - mo.Y;

        int d1, d2;
        if (deltaX > 10) d1 = DiEast;
        else if (deltaX < -10) d1 = DiWest;
        else d1 = DiNodir;

        if (deltaY < -10) d2 = DiSouth;
        else if (deltaY > 10) d2 = DiNorth;
        else d2 = DiNodir;

        if (d1 != DiNodir && d2 != DiNodir)
        {
            mo.Movedir = Diags[((deltaY < 0 ? 1 : 0) << 1) + (deltaX > 0 ? 1 : 0)];
            if (mo.Movedir != turnaround && TryWalk(bot, cmd))
                return;
        }

        if (bot.Sim.BotNewChaseDirRandom() > 200 || Math.Abs(deltaY) > Math.Abs(deltaX))
            (d1, d2) = (d2, d1);

        if (d1 == turnaround) d1 = DiNodir;
        if (d2 == turnaround) d2 = DiNodir;

        if (d1 != DiNodir)
        {
            mo.Movedir = d1;
            if (TryWalk(bot, cmd))
                return;
        }

        if (d2 != DiNodir)
        {
            mo.Movedir = d2;
            if (TryWalk(bot, cmd))
                return;
        }

        if (olddir != DiNodir)
        {
            mo.Movedir = olddir;
            if (TryWalk(bot, cmd))
                return;
        }

        if ((bot.Sim.BotNewChaseDirRandom() & 1) != 0)
        {
            for (var tdir = DiEast; tdir <= DiSoutheast; tdir++)
            {
                if (tdir != turnaround)
                {
                    mo.Movedir = tdir;
                    if (TryWalk(bot, cmd))
                        return;
                }
            }
        }
        else
        {
            for (var tdir = DiSoutheast; tdir >= DiEast; tdir--)
            {
                if (tdir != turnaround)
                {
                    mo.Movedir = tdir;
                    if (TryWalk(bot, cmd))
                        return;
                }
            }
        }

        if (turnaround != DiNodir)
        {
            mo.Movedir = turnaround;
            if (TryWalk(bot, cmd))
                return;
        }

        mo.Movedir = DiNodir;
    }

    public sealed record CheckPositionTm(
        double CeilingZ,
        double FloorZ,
        double DropoffZ);

    public static bool CleanAhead(
        Mover thing,
        double x,
        double y,
        UserCmd cmd,
        CheckPositionTm tm,
        Func<Mover, double, double, CheckPositionTm, bool> safeCheckPosition)
    {
        if (!safeCheckPosition(thing, x, y, tm))
            return false;

        if ((thing.Flags & MfNoclip) == 0)
        {
            if (tm.CeilingZ - tm.FloorZ < thing.Height)
                return false;

            const double maxMoveHeight = 32;
            if ((thing.Flags & MfMissile) == 0)
            {
                if (tm.FloorZ > thing.SectorFloorZ + maxMoveHeight)
                    return false;

                if (tm.FloorZ > thing.SectorFloorZ + thing.MaxStepHeight)
                    cmd.Buttons |= BtJump;

                if ((thing.Flags & MfTeleport) == 0 && tm.CeilingZ < thing.Top())
                    return false;

                if ((thing.Flags & MfTeleport) == 0 && tm.FloorZ - thing.Z > thing.MaxStepHeight)
                    return false;

                if ((thing.Flags & (MfDropoff | MfFloat)) == 0 &&
                    tm.FloorZ - tm.DropoffZ > thing.MaxDropOffHeight)
                    return false;
            }
        }

        return true;
    }

    public static void TurnToAng(BotMoveState bot)
    {
        var maxturn = 15.0;
        var weapon = bot.ReadyWeapon;
        if ((weapon.Flags & BifBotExplosive) != 0 && bot.TRoam != 0 && bot.Missile == null)
            return;

        if (bot.Enemy != null && bot.Dest == null && weapon.ProjectileTypeName == null && weapon.MoveCombatDist > 0 &&
            VmBBotFunc.CheckLos(bot.Sim, bot.Player, bot.Enemy, 65))
            maxturn = 3;

        var distance = DeltaAngle(bot.Mo.Yaw, bot.Angle);
        if (Math.Abs(distance) < 5 && bot.Enemy == null)
            return;

        distance /= 3;
        if (Math.Abs(distance) > maxturn)
            distance = distance < 0 ? -maxturn : maxturn;

        bot.Mo.Yaw += distance;
    }

    public static void Pitch(Mover mo, Actor target)
    {
        var diff = target.Z.ToDouble() - mo.Z;
        var aim = Math.Atan2(diff, Distance2D(mo.X, mo.Y, target));
        mo.Pitch = aim * (180.0 / Math.PI);
    }

    public static int DegreesToBams(double degrees) =>
        (int)(degrees / 360.0 * (1u << 32));

    public static bool IsDangerous(int damageAmount) => damageAmount > 0;

    public static double DeltaAngle(double current, double wanted)
    {
        var diff = wanted - current;
        while (diff > 180) diff -= 360;
        while (diff < -180) diff += 360;
        return diff;
    }

    public static double AngleTo(Mover from, Actor dest) =>
        Math.Atan2(dest.Y.ToDouble() - from.Y, dest.X.ToDouble() - from.X) * (180.0 / Math.PI);

    public static double Distance2D(double x, double y, Actor dest) =>
        Math.Sqrt(Math.Pow(dest.X.ToDouble() - x, 2) + Math.Pow(dest.Y.ToDouble() - y, 2));

    public static bool CajunCleanAhead(AuthoritySimulation sim, PlayerPawn player, Mover mo, double x, double y, UserCmd cmd)
    {
        var tm = new CheckPositionTm(0, 0, 0);
        if (!CajunSafeCheckPosition(sim, player, mo, x, y, ref tm))
            return false;
        return CleanAhead(mo, x, y, cmd, tm, (_, _, _, _) => true);
    }

    static bool CajunSafeCheckPosition(AuthoritySimulation sim, PlayerPawn player, Mover mo, double x, double y, ref CheckPositionTm tm)
    {
        if (!VmBBotFuncReachFire.SafeCheckPosition(sim, player, x, y))
            return false;

        var sector = ActorPhysics.SectorAt(sim.Level, x, y);
        var floor = sector >= 0 ? sim.FloorOf(sector) : mo.SectorFloorZ;
        var ceiling = sector >= 0 ? sim.CeilingOf(sector) : floor + mo.Height;
        tm = new CheckPositionTm(ceiling, floor, floor);
        return true;
    }

    static bool CajunCheckKeys(PlayerPawn player, int lockNumber, int cardType, bool _) =>
        lockNumber == 0 || cardType is >= 1 and <= 5 && player.Inventory.HasKey((PickupCatalog.KeyColor)cardType);

    static bool CajunTestActivateLine(BotMoveState _, BotLineHit __, int ___) => false;
}
