namespace HCDE.Playsim;

/// <summary>Whole static <c>P_XYMovement</c> from <c>p_mobj.cpp</c> (2421–2926).</summary>
public static class VmPMobjXYMovement
{
    public const double MaxMove = 30;
    public const double StopSpeed = 0x1000 / 65536.0;
    public const double CarryStopSpeed = (0x1000 * 32.0 / 3.0) / 65536.0;
    public const double CarryFactor = 3.0 / 32.0;
    public const double VelocityThreshold = 5000;
    public const double MinVel = 1e-6;
    public const double OrigFriction = 0xE800 / 65536.0;

    public const int MfMissile = 1;
    public const int MfNoclip = 1 << 1;
    public const int MfSkullfly = 1 << 2;
    public const int MfCorpse = 1 << 3;
    public const int MfNogravity = 1 << 4;
    public const int Mf2Windthrust = 1;
    public const int Mf2Slide = 1 << 1;
    public const int Mf2Blasted = 1 << 2;
    public const int Mf2Onmobj = 1 << 3;
    public const int Mf2Fly = 1 << 4;
    public const int Mf2Dormant = 1 << 5;
    public const int Mf2Rip = 1 << 6;
    public const int Mf3Skyexplode = 1;
    public const int Mf4Scrollmove = 1;
    public const int Mf6Falling = 1;
    public const int Mf8Nofriction = 1;
    public const int Mf9Noautooffskullfly = 1;
    public const int BounceMbf = 1;
    public const int BounceActors = 1 << 1;
    public const int CompatfWallrun = 1;
    public const int CfNovelocity = 1;
    public const int CfPredicting = 1 << 1;
    public const int LineHorizon = 27;

    static int _pushTime;

    public sealed class XYActor
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double FloorZ { get; set; }
        public double DropoffZ { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags3 { get; set; }
        public int Flags4 { get; set; }
        public int Flags6 { get; set; }
        public int Flags8 { get; set; }
        public int Flags9 { get; set; }
        public int BounceFlags { get; set; }
        public int WaterLevel { get; set; }
        public bool HasPlayer { get; set; }
        public int PlayerCheats { get; set; }
        public double PlayerVelX { get; set; }
        public double PlayerVelY { get; set; }
        public double CrouchOffset { get; set; }
        public double CrouchFactor { get; set; } = 1;
        public int CmdForward { get; set; }
        public int CmdSide { get; set; }
        public double YawDeg { get; set; }
        public object? BlockingMobj { get; set; }
        public object? BlockingLine { get; set; }
        public object? BlockingFloor { get; set; }
        public object? BlockingCeiling { get; set; }
        public object? Sector { get; set; }
        public int SectorSpecial { get; set; }
        public object? SeeState { get; set; }
        public int Tics { get; set; }
        public bool Destroyed { get; set; }
        public bool CeilingLineHitsSky { get; set; }
        public int FfloorCount { get; set; }
        public bool FfloorSolidAtFloorZ { get; set; }
        public bool NoClip2 { get; set; }

        public bool IsNoClip2() => NoClip2;
    }

    public sealed class XYCheckTm
    {
        public object? FloorSector { get; set; }
        public object? CeilingSector { get; set; }
        public object? CeilingLine { get; set; }
    }

    public sealed class XYMovementState
    {
        public required XYActor Actor { get; init; }
        public double ScrollX { get; init; }
        public double ScrollY { get; init; }
        public int LevelCompatFlags { get; init; }
        public double AirFriction { get; init; } = 1;
        public int BlockingLineSpecial { get; set; }
        public int LineHorizonSpecial { get; init; } = LineHorizon;
        public XYCheckTm Tm { get; } = new();
        public Func<XYActor, double, double, object?, XYCheckTm, bool>? TryMove { get; init; }
        public Func<XYActor, double, double, int, bool>? SlideMove { get; init; }
        public Func<XYActor, double, double, object?>? CheckSlopeWalk { get; init; }
        public Func<XYActor, object?, bool>? BounceActor { get; init; }
        public Func<XYActor, bool>? BounceWall { get; init; }
        public Func<XYActor, object?, bool>? ReflectOffActor { get; init; }
        public Action<XYActor, object?, object?, bool>? ExplodeMissile { get; init; }
        public Action<XYActor>? Destroy { get; init; }
        public Action<XYActor>? SetSeeState { get; init; }
        public Action<XYActor>? SetIdle { get; init; }
        public Action<XYActor>? PlayIdle { get; init; }
        public Action<XYActor, bool, double>? PlayBounceSound { get; init; }
        public Action<XYActor, double, bool>? Check3DFloorHit { get; init; }
        public Action<XYActor, double, bool>? Check3DCeilingHit { get; init; }
        public Action<XYActor, int>? ProjectileHitPlane { get; init; }
        public Func<XYActor, object?, double>? GetFriction { get; init; }
        public Action<XYActor, double, double>? ThrustWind { get; init; }
        public bool ForceSlide { get; init; }
    }

    public static double XYMovement(XYMovementState st)
    {
        var mo = st.Actor;
        var scrollX = st.ScrollX;
        var scrollY = st.ScrollY;
        var bForceSlide = st.ForceSlide || (scrollX != 0 || scrollY != 0);
        var oldFloorZ = mo.FloorZ;
        var oldZ = mo.Z;

        var maxmove = mo.WaterLevel < 1 || (mo.Flags & MfMissile) != 0
                      || (mo.HasPlayer && mo.CrouchOffset < -10)
            ? MaxMove
            : MaxMove / 4;

        if ((mo.Flags2 & Mf2Windthrust) != 0 && mo.WaterLevel < 2 && (mo.Flags & MfNoclip) == 0)
            ApplyWindThrust(st, mo);

        if ((mo.HasPlayer && (st.LevelCompatFlags & CompatfWallrun) != 0)
            || ((mo.WaterLevel >= 1 || (mo.HasPlayer && mo.CrouchFactor < 0.75))
                && (mo.Flags8 & Mf8Nofriction) == 0))
        {
            ClampVelocityPreserveDirection(mo, maxmove);
        }

        var speedSq = mo.VelX * mo.VelX + mo.VelY * mo.VelY;
        if (speedSq >= VelocityThreshold * VelocityThreshold)
            ResizeVelocity(mo, VelocityThreshold);

        var moveX = mo.VelX;
        var moveY = mo.VelY;
        mo.Flags4 &= ~Mf4Scrollmove;
        if (Math.Abs(scrollX) > CarryStopSpeed)
        {
            scrollX *= CarryFactor;
            mo.VelX += scrollX;
            mo.Flags4 |= Mf4Scrollmove;
        }
        if (Math.Abs(scrollY) > CarryStopSpeed)
        {
            scrollY *= CarryFactor;
            mo.VelY += scrollY;
            mo.Flags4 |= Mf4Scrollmove;
        }
        moveX += scrollX;
        moveY += scrollY;

        if (moveX == 0 && moveY == 0)
        {
            HandleSkullflyStop(st, mo);
            return oldFloorZ;
        }

        var startMoveX = moveX;
        var startMoveY = moveY;
        var walkPlane = st.CheckSlopeWalk?.Invoke(mo, moveX, moveY);

        var maxStepMove = mo.Radius - 1;
        if (maxStepMove <= 0)
            maxStepMove = MaxMove;

        var xSpeed = Math.Abs(moveX);
        var ySpeed = Math.Abs(moveY);
        var steps = 1;
        if (xSpeed > ySpeed)
        {
            if (xSpeed > maxStepMove)
                steps = 1 + (int)(xSpeed / maxStepMove);
        }
        else if (ySpeed > maxStepMove)
            steps = 1 + (int)(ySpeed / maxStepMove);

        var oneStepX = startMoveX / steps;
        var oneStepY = startMoveY / steps;
        var startX = mo.X;
        var startY = mo.Y;
        var step = 1;
        var totalSteps = steps;
        _pushTime++;
        var tm = st.Tm;
        var oldAngle = mo.YawDeg;

        do
        {
            if ((st.LevelCompatFlags & CompatfWallrun) != 0)
                _pushTime++;
            var ptryX = startX + moveX * step / steps;
            var ptryY = startY + moveY * step / steps;
            var startVelX = mo.VelX;
            var startVelY = mo.VelY;

            if (st.TryMove?.Invoke(mo, ptryX, ptryY, walkPlane, tm) != true)
            {
                HandleBlockedMove(st, mo, ref steps, ref step, totalSteps, oneStepX, oneStepY, ref moveX, ref moveY,
                    ref startX, ref startY, startVelX, startVelY, bForceSlide, oldFloorZ, tm, ref oldAngle);
                if (mo.Destroyed)
                    return oldFloorZ;
            }
            else if (mo.X != ptryX || mo.Y != ptryY)
            {
                HandleTeleporterAdjust(mo, ref step, steps, ref moveX, ref moveY, ref startX, ref startY, ref oldAngle);
            }
        } while (++step <= steps);

        if (mo.HasPlayer && (mo.PlayerCheats & CfNovelocity) != 0)
        {
            mo.VelX = mo.VelY = 0;
            mo.PlayerVelX = mo.PlayerVelY = 0;
            return oldFloorZ;
        }

        if ((mo.Flags & (MfMissile | MfSkullfly)) != 0 || (mo.Flags8 & Mf8Nofriction) != 0)
            return oldFloorZ;

        if (mo.Z > mo.FloorZ && (mo.Flags2 & Mf2Onmobj) == 0
            && !mo.IsNoClip2()
            && !((mo.Flags2 & Mf2Fly) != 0 && (mo.Flags & MfNogravity) != 0)
            && mo.WaterLevel == 0)
        {
            if (mo.HasPlayer && st.AirFriction != 1)
            {
                mo.VelX *= st.AirFriction;
                mo.VelY *= st.AirFriction;
                mo.PlayerVelX *= st.AirFriction;
                mo.PlayerVelY *= st.AirFriction;
            }
            return oldFloorZ;
        }

        if ((mo.Flags & MfCorpse) != 0
            || ((mo.BounceFlags & BounceMbf) != 0 && mo.Z > mo.DropoffZ)
            || (mo.Flags6 & Mf6Falling) != 0)
        {
            if (Math.Abs(mo.VelX) > 0.25 || Math.Abs(mo.VelY) > 0.25)
            {
                if (mo.FloorZ > mo.Z && mo.DropoffZ != mo.FloorZ && !mo.FfloorSolidAtFloorZ)
                    return oldFloorZ;
            }
        }

        if (Math.Abs(mo.VelX) < StopSpeed && Math.Abs(mo.VelY) < StopSpeed
            && (!mo.HasPlayer || mo.CmdForward == 0 && mo.CmdSide == 0))
        {
            if (mo.HasPlayer && (mo.PlayerCheats & CfPredicting) == 0)
                st.PlayIdle?.Invoke(mo);
            mo.VelX = mo.VelY = 0;
            mo.Flags4 &= ~Mf4Scrollmove;
            if (mo.HasPlayer)
            {
                mo.PlayerVelX = 0;
                mo.PlayerVelY = 0;
            }
        }
        else
        {
            var friction = st.GetFriction?.Invoke(mo, null) ?? OrigFriction;
            mo.VelX *= friction;
            mo.VelY *= friction;
            if (mo.HasPlayer)
            {
                mo.PlayerVelX *= OrigFriction;
                mo.PlayerVelY *= OrigFriction;
            }
            if (Math.Abs(mo.VelX) < MinVel) mo.VelX = 0;
            if (Math.Abs(mo.VelY) < MinVel) mo.VelY = 0;
            if (mo.HasPlayer)
            {
                if (Math.Abs(mo.PlayerVelX) < MinVel) mo.PlayerVelX = 0;
                if (Math.Abs(mo.PlayerVelY) < MinVel) mo.PlayerVelY = 0;
            }
        }

        return oldFloorZ;
    }

    static void ApplyWindThrust(XYMovementState st, XYActor mo)
    {
        var special = mo.SectorSpecial;
        double[] windTab = { 5 / 32.0, 10 / 32.0, 25 / 32.0 };
        if (special is >= 40 and <= 42)
            st.ThrustWind?.Invoke(mo, 0, windTab[special - 40]);
        else if (special is >= 43 and <= 45)
            st.ThrustWind?.Invoke(mo, 90, windTab[special - 43]);
        else if (special is >= 46 and <= 48)
            st.ThrustWind?.Invoke(mo, 270, windTab[special - 46]);
        else if (special is >= 49 and <= 51)
            st.ThrustWind?.Invoke(mo, 180, windTab[special - 49]);
    }

    static void ClampVelocityPreserveDirection(XYActor mo, double maxmove)
    {
        var cx = mo.VelX == 0 ? 1 : Math.Clamp(mo.VelX, -maxmove, maxmove) / mo.VelX;
        var cy = mo.VelY == 0 ? 1 : Math.Clamp(mo.VelY, -maxmove, maxmove) / mo.VelY;
        var fac = Math.Min(cx, cy);
        mo.VelX *= fac;
        mo.VelY *= fac;
    }

    static void ResizeVelocity(XYActor mo, double limit)
    {
        var len = Math.Sqrt(mo.VelX * mo.VelX + mo.VelY * mo.VelY);
        if (len <= 0) return;
        mo.VelX = mo.VelX / len * limit;
        mo.VelY = mo.VelY / len * limit;
    }

    static void HandleSkullflyStop(XYMovementState st, XYActor mo)
    {
        if ((mo.Flags & MfSkullfly) == 0 || (mo.Flags9 & Mf9Noautooffskullfly) != 0)
            return;
        mo.Flags &= ~MfSkullfly;
        mo.VelX = mo.VelY = 0;
        if ((mo.Flags2 & Mf2Dormant) == 0)
        {
            if (mo.SeeState != null)
                st.SetSeeState?.Invoke(mo);
            else
                st.SetIdle?.Invoke(mo);
        }
        else
        {
            st.SetIdle?.Invoke(mo);
            mo.Tics = -1;
        }
    }

    static void HandleBlockedMove(
        XYMovementState st,
        XYActor mo,
        ref int steps,
        ref int step,
        int totalSteps,
        double oneStepX,
        double oneStepY,
        ref double moveX,
        ref double moveY,
        ref double startX,
        ref double startY,
        double startVelX,
        double startVelY,
        bool bForceSlide,
        double oldFloorZ,
        XYCheckTm tm,
        ref double oldAngle)
    {
        var blockingMobj = mo.BlockingMobj;
        var blockingLine = mo.BlockingLine;
        if (blockingLine == null && blockingMobj == null)
        {
            st.Check3DFloorHit?.Invoke(mo, mo.FloorZ, false);
            st.Check3DCeilingHit?.Invoke(mo, mo.Z + mo.Height, false);
        }

        if ((mo.Flags & MfMissile) == 0 && (mo.BounceFlags & BounceMbf) != 0
            && (blockingMobj != null
                ? st.BounceActor?.Invoke(mo, blockingMobj) == true
                : st.BounceWall?.Invoke(mo) == true))
        {
            return;
        }

        if (((mo.Flags2 & (Mf2Slide | Mf2Blasted)) != 0 || bForceSlide) && (mo.Flags & MfMissile) == 0)
        {
            if (blockingMobj == null)
                SlideAgainstWall(st, mo, ref steps, ref step, totalSteps, oneStepX, oneStepY, ref moveX, ref moveY,
                    ref startX, ref startY, startVelX, startVelY);
            else
                SlideAgainstActor(st, mo, oneStepX, oneStepY, ref steps);
            return;
        }

        if ((mo.Flags & MfMissile) != 0)
        {
            steps = 0;
            HandleMissileBlocked(st, mo, blockingMobj, blockingLine, tm, oldFloorZ);
            return;
        }

        mo.VelX = mo.VelY = 0;
        steps = 0;
    }

    static void SlideAgainstWall(
        XYMovementState st,
        XYActor mo,
        ref int steps,
        ref int step,
        int totalSteps,
        double oneStepX,
        double oneStepY,
        ref double moveX,
        ref double moveY,
        ref double startX,
        ref double startY,
        double startVelX,
        double startVelY)
    {
        if (mo.VelX == startVelX && mo.VelY == startVelY)
        {
            if (mo.HasPlayer && (st.LevelCompatFlags & CompatfWallrun) != 0)
                st.SlideMove?.Invoke(mo, mo.VelX, mo.VelY, 1);
            else
                st.SlideMove?.Invoke(mo, oneStepX, oneStepY, totalSteps);
            if (mo.VelX == 0 && mo.VelY == 0)
                steps = 0;
            else if (!mo.HasPlayer || (st.LevelCompatFlags & CompatfWallrun) == 0)
            {
                moveX = mo.VelX;
                moveY = mo.VelY;
                st.CheckSlopeWalk?.Invoke(mo, moveX, moveY);
                startX = mo.X - moveX * step / steps;
                startY = mo.Y - moveY * step / steps;
            }
        }
        else
            steps = 0;
    }

    static void SlideAgainstActor(XYMovementState st, XYActor mo, double oneStepX, double oneStepY, ref int steps)
    {
        var tX = 0.0;
        var tY = oneStepY;
        st.CheckSlopeWalk?.Invoke(mo, tX, tY);
        if (st.TryMove?.Invoke(mo, mo.X + tX, mo.Y + tY, null, st.Tm) == true)
            mo.VelX = 0;
        else
        {
            tX = oneStepX;
            tY = 0;
            st.CheckSlopeWalk?.Invoke(mo, tX, tY);
            if (st.TryMove?.Invoke(mo, mo.X + tX, mo.Y + tY, null, st.Tm) == true)
                mo.VelY = 0;
            else
                mo.VelX = mo.VelY = 0;
        }
        if (mo.HasPlayer)
        {
            if (mo.VelX == 0) mo.PlayerVelX = 0;
            if (mo.VelY == 0) mo.PlayerVelY = 0;
        }
        steps = 0;
    }

    static void HandleMissileBlocked(
        XYMovementState st,
        XYActor mo,
        object? blockingMobj,
        object? blockingLine,
        XYCheckTm tm,
        double oldFloorZ)
    {
        if (blockingMobj != null)
        {
            if ((mo.BounceFlags & BounceActors) != 0)
            {
                if (st.BounceActor?.Invoke(mo, blockingMobj) != true)
                    st.ExplodeMissile?.Invoke(mo, null, blockingMobj, false);
                return;
            }
        }
        else if (st.BounceWall?.Invoke(mo) == true)
        {
            st.PlayBounceSound?.Invoke(mo, false, 1.0);
            return;
        }

        if (blockingMobj != null && st.ReflectOffActor?.Invoke(mo, blockingMobj) == true)
            return;

        var onSky = false;
        if (tm.CeilingLine != null && mo.CeilingLineHitsSky)
        {
            if ((mo.Flags3 & Mf3Skyexplode) == 0)
            {
                st.Destroy?.Invoke(mo);
                mo.Destroyed = true;
                return;
            }
            onSky = true;
        }

        if (blockingLine != null && st.BlockingLineSpecial == st.LineHorizonSpecial)
        {
            if ((mo.Flags3 & Mf3Skyexplode) == 0)
            {
                st.Destroy?.Invoke(mo);
                mo.Destroyed = true;
                return;
            }
            onSky = true;
        }

        if (mo.BlockingCeiling != null)
            st.ProjectileHitPlane?.Invoke(mo, 1);
        if (mo.BlockingFloor != null)
            st.ProjectileHitPlane?.Invoke(mo, 0);
        st.ExplodeMissile?.Invoke(mo, blockingLine, blockingMobj, onSky);
        mo.Destroyed = true;
    }

    static void HandleTeleporterAdjust(
        XYActor mo,
        ref int step,
        int steps,
        ref double moveX,
        ref double moveY,
        ref double startX,
        ref double startY,
        ref double oldAngle)
    {
        if (mo.VelX == 0 && mo.VelY == 0)
            step = steps;
        else
        {
            var angleDiff = mo.YawDeg - oldAngle;
            if (Math.Abs(angleDiff) > 1e-9)
            {
                (moveX, moveY) = Rotate(moveX, moveY, angleDiff);
                oldAngle = mo.YawDeg;
            }
            startX = mo.X - moveX * step / steps;
            startY = mo.Y - moveY * step / steps;
        }
    }

    static (double X, double Y) Rotate(double x, double y, double degrees)
    {
        var rad = degrees * Math.PI / 180;
        var cos = Math.Cos(rad);
        var sin = Math.Sin(rad);
        return (x * cos - y * sin, x * sin + y * cos);
    }

    public static int SubstepsForMove(double moveX, double moveY, double radius) =>
        radius <= 1 ? 1 : Math.Max(1, (int)(1 + Math.Max(Math.Abs(moveX), Math.Abs(moveY)) / Math.Max(radius - 1, MaxMove)));
}
