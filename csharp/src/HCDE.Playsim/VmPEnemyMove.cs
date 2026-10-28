namespace HCDE.Playsim;

/// <summary>Faithful port of <c>P_Move</c> (<c>p_enemy.cpp</c> 482–717).</summary>
public static class VmPEnemyMove
{
    public const int Mf2Blasted = 1;
    public const int MfNogravity = 1 << 1;
    public const int Mf2Onmobj = 1 << 2;
    public const int MfFloat = 1 << 3;
    public const int MfInfloat = 1 << 4;
    public const int Mf6Jumpdown = 1;
    public const int Mf6Notrigger = 1 << 1;
    public const int Mf4Canusewalls = 1;
    public const int Mf2Pushwall = 1 << 1;
    public const int Mf8Nofriction = 1;
    public const int DiNodir = 8;
    public const double OrigFriction = 1;
    public const double OrigFrictionFactor = 1;

    public sealed class SpechitEntry
    {
        public object? Line { get; set; }
    }

    public sealed class MoveState
    {
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags4 { get; set; }
        public int Flags6 { get; set; }
        public int Flags8 { get; set; }
        public int Movedir { get; set; }
        public double Speed { get; set; }
        public double Radius { get; set; }
        public double Z { get; set; }
        public double FloorZ { get; set; }
        public double MaxStepHeight { get; set; }
        public double FloatSpeed { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double YawDegrees { get; set; }
        public bool TargetNull { get; set; }
        public bool TargetIsFriend { get; set; }
        public double TargetDistance2D { get; set; }
        public bool LevelNoJumpdown { get; set; }
        public bool CompatMbfMonsterMove { get; set; }
        public bool NoMonsterInterpolation { get; set; }
        public object? BlockingLine { get; set; }
        public bool FloatOk { get; set; }
        public double TmFloorZ { get; set; }
        public Stack<SpechitEntry> Spechit { get; } = new();
        public double[] XSpeed { get; set; } = { 1, 0, -1, 0, 1, 0, -1, 0 };
        public double[] YSpeed { get; set; } = { 0, 1, 0, -1, 0, 1, 0, -1 };
        public Func<bool>? CanJump { get; init; }
        public Func<int>? PrDropoff { get; init; }
        public Func<double, double?>? GetMoveFactor { get; init; }
        public Func<double, double, int, bool>? TryMove { get; init; }
        public Func<double, double, bool>? TestMobjZ { get; init; }
        public Func<object?, bool>? ActivateUseLine { get; init; }
        public Func<object?, bool>? ActivatePushLine { get; init; }
        public Func<int>? PrOpendoor { get; init; }
        public Action<double, double, double, bool>? SetOrigin { get; init; }
        public Action<double>? SetZ { get; init; }
        public Action<double>? AddZ { get; init; }
        public Action? ClearInterpolation { get; init; }
        public Func<bool>? TriggerSectorHitFloor { get; init; }
        public Action<double, bool>? CheckFor3DFloorHit { get; init; }
        public Func<double, double, double>? DeltaAngle { get; init; }
        public Func<double, double, (double X, double Y)>? RotateMove { get; init; }
    }

    public static bool Move(MoveState s)
    {
        if ((s.Flags2 & Mf2Blasted) != 0)
            return true;

        if (s.Movedir >= DiNodir)
        {
            s.Movedir = DiNodir;
            return false;
        }

        if ((s.Flags & MfNogravity) == 0 && s.CanJump?.Invoke() != true && s.Z > s.FloorZ && (s.Flags2 & Mf2Onmobj) == 0)
            return false;

        var dropoff = 0;
        if ((s.Flags6 & Mf6Jumpdown) != 0 && !s.TargetNull && !s.TargetIsFriend && !s.LevelNoJumpdown
            && s.TargetDistance2D < 144 && (s.PrDropoff?.Invoke() ?? 0) < 235)
            dropoff = 2;

        var movefactor = OrigFrictionFactor;
        var friction = OrigFriction;
        var speed = s.Speed;
        if (s.CompatMbfMonsterMove && (s.Flags8 & Mf8Nofriction) == 0)
        {
            var mf = s.GetMoveFactor?.Invoke(friction);
            if (mf != null)
            {
                movefactor = mf.Value;
                if (friction < OrigFriction)
                {
                    speed = speed * ((OrigFrictionFactor - (OrigFrictionFactor - movefactor) / 2)) / OrigFrictionFactor;
                    if (speed == 0)
                        speed = s.Speed;
                }
            }
        }

        var deltax = speed * s.XSpeed[s.Movedir];
        var deltay = speed * s.YSpeed[s.Movedir];
        var origx = s.PosX;
        var origy = s.PosY;
        var tryx = origx + deltax;
        var tryy = origy + deltay;

        var maxmove = s.Radius - 1;
        var steps = 1;
        if (maxmove > 0)
        {
            var xs = Math.Abs(deltax);
            var ys = Math.Abs(deltay);
            if (xs > ys)
            {
                if (xs > maxmove)
                    steps = 1 + (int)(xs / maxmove);
            }
            else if (ys > maxmove)
                steps = 1 + (int)(ys / maxmove);
        }

        var startX = origx;
        var startY = origy;
        var moveX = deltax;
        var moveY = deltay;
        var oldangle = s.YawDegrees;
        var tryOk = true;
        for (var i = 1; i <= steps; i++)
        {
            var ptryX = startX + moveX * i / steps;
            var ptryY = startY + moveY * i / steps;
            tryOk = s.TryMove?.Invoke(ptryX, ptryY, dropoff) == true;
            if (!tryOk)
                break;
            if (steps > 1 && (s.PosX != ptryX || s.PosY != ptryY))
            {
                var anglediff = s.DeltaAngle?.Invoke(oldangle, s.YawDegrees) ?? 0;
                if (anglediff != 0 && s.RotateMove != null)
                {
                    var rotated = s.RotateMove(moveX, moveY);
                    moveX = rotated.X;
                    moveY = rotated.Y;
                    oldangle = s.YawDegrees;
                }

                startX = s.PosX - moveX * i / steps;
                startY = s.PosY - moveY * i / steps;
            }
        }

        if (s.NoMonsterInterpolation)
            s.ClearInterpolation?.Invoke();

        if (tryOk && friction > OrigFriction)
        {
            s.SetOrigin?.Invoke(origx, origy, s.Z, false);
            movefactor *= 1.0 / OrigFrictionFactor / 4;
            s.VelX += deltax * movefactor;
            s.VelY += deltay * movefactor;
        }

        if (tryOk && (s.Flags & MfNogravity) == 0 && s.CanJump?.Invoke() != true && (s.Flags2 & Mf2Onmobj) == 0)
        {
            if (s.Z < s.FloorZ)
                s.SetZ?.Invoke(s.FloorZ);
            else if (s.Z <= s.FloorZ + s.MaxStepHeight)
            {
                var savedz = s.Z;
                s.SetZ?.Invoke(s.FloorZ);
                if (s.TestMobjZ?.Invoke(savedz, s.FloorZ) != true)
                    s.SetZ?.Invoke(savedz);
                else
                {
                    s.TriggerSectorHitFloor?.Invoke();
                    s.CheckFor3DFloorHit?.Invoke(s.Z, true);
                }
            }
        }

        if (!tryOk)
        {
            if ((s.CanJump?.Invoke() == true || (s.Flags & MfFloat) != 0) && s.FloatOk)
            {
                var savedz = s.Z;
                if (s.Z < s.TmFloorZ)
                    s.AddZ?.Invoke(s.FloatSpeed);
                else
                    s.AddZ?.Invoke(-s.FloatSpeed);
                if (s.TestMobjZ?.Invoke(savedz, s.TmFloorZ) == true)
                {
                    s.Flags |= MfInfloat;
                    return true;
                }

                s.SetZ?.Invoke(savedz);
            }

            if (s.Spechit.Count == 0)
                return false;

            s.Movedir = DiNodir;
            var good = 0;
            if ((s.Flags6 & Mf6Notrigger) == 0)
            {
                while (s.Spechit.TryPop(out var spec))
                {
                    if (((s.Flags4 & Mf4Canusewalls) != 0 && s.ActivateUseLine?.Invoke(spec.Line) == true)
                        || ((s.Flags2 & Mf2Pushwall) != 0 && s.ActivatePushLine?.Invoke(spec.Line) == true))
                        good |= ReferenceEquals(spec.Line, s.BlockingLine) ? 1 : 2;
                }
            }
            else
                s.Spechit.Clear();

            var openRoll = (s.PrOpendoor?.Invoke() ?? 0) >= 203;
            return good != 0 && (openRoll ^ (good & 1) != 0);
        }

        s.Flags &= ~MfInfloat;
        return true;
    }
}
