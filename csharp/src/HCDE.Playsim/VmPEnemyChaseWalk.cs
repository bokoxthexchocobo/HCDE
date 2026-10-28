namespace HCDE.Playsim;

/// <summary>Faithful port of chase-walk cluster (<c>p_enemy.cpp</c> 725–1099).</summary>
public static class VmPEnemyChaseWalk
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

    public const int Mf2Floorclip = 1;
    public const int Mf5Avoidingdropoff = 1;
    public const int Mf5Chasegoal = 1 << 1;
    public const int Mf6Nofear = 1;
    public const int MfFloat = 1;
    public const int MfDropoff = 1 << 1;
    public const int Mf2Onmobj = 1 << 2;
    public const int Mf3Avoidmelee = 1;
    public const int Mf4Frightened = 1;
    public const int Mf8Avoidhazards = 1;
    public const int Mf8Stayonlift = 1 << 1;
    public const int Mf8Frightening = 1 << 2;
    public const int CfFrightening = 1;
    public const int CompatDropoff = 1;
    public const int CompatStayonlift = 1 << 1;
    public const int CompatAvoidHazards = 1 << 2;
    public const int Level3Avoidmelee = 1;

    private static readonly int[] Opposite = { DiWest, DiSouthwest, DiSouth, DiSoutheast, DiEast, DiNortheast, DiNorth, DiNorthwest, DiNodir };
    private static readonly int[] Diags = { DiNorthwest, DiNortheast, DiSouthwest, DiSoutheast };

    public sealed class SmartMoveState
    {
        public int Flags8 { get; set; }
        public int CompatFlags2 { get; set; }
        public bool TargetNull { get; set; }
        public int TargetHealth { get; set; }
        public bool OnLift { get; set; }
        public bool TargetOnLift { get; set; }
        public bool SameLiftTags { get; set; }
        public bool UnderDamageBefore { get; set; }
        public bool UnderDamageAfter { get; set; }
        public bool UnderDamageAfterNegative { get; set; }
        public int Flags2 { get; set; }
        public int Movedir { get; set; }
        public Func<bool>? Move { get; set; }
        public Func<int>? PrStayOnLift { get; set; }
        public Func<int>? PrAvoidCrush { get; set; }
        public Action? AdjustFloorClip { get; set; }
    }

    public static bool SmartMove(SmartMoveState s)
    {
        var monsterAvoid = (s.CompatFlags2 & CompatAvoidHazards) != 0 || (s.Flags8 & Mf8Avoidhazards) != 0;
        var onLift = ((s.Flags8 & Mf8Stayonlift) != 0 || (s.CompatFlags2 & CompatStayonlift) != 0)
            && !s.TargetNull && s.TargetHealth > 0 && s.OnLift && s.TargetOnLift && s.SameLiftTags;
        var underDamage = monsterAvoid && s.UnderDamageBefore;

        if (s.Move?.Invoke() != true)
            return false;

        if ((onLift && (s.PrStayOnLift?.Invoke() ?? 0) < 230 && !s.OnLift)
            || (monsterAvoid && !underDamage && s.UnderDamageAfter
                && (s.UnderDamageAfterNegative || (s.PrAvoidCrush?.Invoke() ?? 256) < 200)))
            s.Movedir = DiNodir;

        if ((s.Flags2 & Mf2Floorclip) != 0)
            s.AdjustFloorClip?.Invoke();
        return true;
    }

    public sealed class TryWalkState
    {
        public SmartMoveState Smart { get; } = new();
        public int MoveCount { get; set; }
        public Func<int>? PrTrywalk { get; set; }
    }

    public static bool TryWalk(TryWalkState s)
    {
        if (!SmartMove(s.Smart))
            return false;
        s.MoveCount = (s.PrTrywalk?.Invoke() ?? 0) & 15;
        return true;
    }

    public sealed class DoNewChaseDirState
    {
        public int Movedir { get; set; }
        public int Flags5 { get; set; }
        public bool[] Attempts { get; } = new bool[DiNodir];
        public Func<bool>? TryWalk { get; set; }
        public Func<int>? PrNewChaseDir { get; set; }
    }

    public static void DoNewChaseDir(DoNewChaseDirState s, double deltax, double deltay)
    {
        Array.Clear(s.Attempts, 0, s.Attempts.Length);
        var olddir = s.Movedir;
        var turnaround = Opposite[olddir];

        int d0, d1;
        if (deltax > 10) d0 = DiEast;
        else if (deltax < -10) d0 = DiWest;
        else d0 = DiNodir;

        if (deltay < -10) d1 = DiSouth;
        else if (deltay > 10) d1 = DiNorth;
        else d1 = DiNodir;

        if (d0 != DiNodir && d1 != DiNodir)
        {
            s.Movedir = Diags[((deltay < 0 ? 1 : 0) << 1) + (deltax > 0 ? 1 : 0)];
            if (s.Movedir != turnaround)
            {
                s.Attempts[s.Movedir] = true;
                if (s.TryWalk?.Invoke() == true)
                    return;
            }
        }

        if ((s.Flags5 & Mf5Avoidingdropoff) == 0)
        {
            if ((s.PrNewChaseDir?.Invoke() ?? 0) > 200 || Math.Abs(deltay) > Math.Abs(deltax))
                (d0, d1) = (d1, d0);
            if (d0 == turnaround) d0 = DiNodir;
            if (d1 == turnaround) d1 = DiNodir;
        }

        if (d0 != DiNodir && !s.Attempts[d0])
        {
            s.Movedir = d0;
            s.Attempts[d0] = true;
            if (s.TryWalk?.Invoke() == true)
                return;
        }

        if (d1 != DiNodir && !s.Attempts[d1])
        {
            s.Movedir = d1;
            s.Attempts[d1] = true;
            if (s.TryWalk?.Invoke() == true)
                return;
        }

        if ((s.Flags5 & Mf5Avoidingdropoff) == 0 && olddir != DiNodir && !s.Attempts[olddir])
        {
            s.Movedir = olddir;
            s.Attempts[olddir] = true;
            if (s.TryWalk?.Invoke() == true)
                return;
        }

        var roll = s.PrNewChaseDir?.Invoke() ?? 0;
        if ((roll & 1) != 0)
        {
            for (var tdir = DiEast; tdir <= DiSoutheast; tdir++)
            {
                if (tdir != turnaround && !s.Attempts[tdir])
                {
                    s.Movedir = tdir;
                    s.Attempts[tdir] = true;
                    if (s.TryWalk?.Invoke() == true)
                        return;
                }
            }
        }
        else
        {
            for (var tdir = DiSoutheast; tdir >= DiEast; tdir--)
            {
                if (tdir != turnaround && !s.Attempts[tdir])
                {
                    s.Movedir = tdir;
                    s.Attempts[tdir] = true;
                    if (s.TryWalk?.Invoke() == true)
                        return;
                }
            }
        }

        if (turnaround != DiNodir && !s.Attempts[turnaround])
        {
            s.Movedir = turnaround;
            if (s.TryWalk?.Invoke() == true)
                return;
        }

        s.Movedir = DiNodir;
    }

    public readonly record struct DropoffLine(double FrontZ, double BackZ, double AngleSin, double AngleCos);

    public sealed class NewChaseDirState
    {
        public int StrafeCount { get; set; }
        public int Flags3 { get; set; }
        public int Flags5 { get; set; }
        public int Flags6 { get; set; }
        public int Flags { get; set; }
        public int Flags4 { get; set; }
        public int Flags2 { get; set; }
        public int Movedir { get; set; }
        public int MoveCount { get; set; }
        public int LevelCompat { get; set; }
        public int LevelFlags3 { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double Z { get; set; }
        public double FloorZ { get; set; }
        public double DropoffZ { get; set; }
        public double MaxDropOffHeight { get; set; }
        public bool GoalNull { get; set; }
        public bool TargetNull { get; set; }
        public bool TargetIsGoal { get; set; }
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
        public bool TargetHasPlayer { get; set; }
        public int TargetPlayerCheats { get; set; }
        public int TargetFlags8 { get; set; }
        public bool TargetIsFriend { get; set; }
        public int TargetHealth { get; set; }
        public bool TargetMissileStateNull { get; set; }
        public double TargetDistance2D { get; set; }
        public double TargetMeleeRange { get; set; }
        public double TargetRadius { get; set; }
        public bool ReadyWeaponMelee { get; set; }
        public bool AiRegroupConsumed { get; set; }
        public double RegroupX { get; set; }
        public double RegroupY { get; set; }
        public IReadOnlyList<DropoffLine>? DropoffLines { get; set; }
        public Func<bool>? RandomChaseDir { get; set; }
        public Action<double, double>? DoNewChaseDir { get; set; }
        public Func<int>? PrEnemyStrafe { get; set; }
    }

    public static void NewChaseDir(NewChaseDirState s)
    {
        s.StrafeCount = 0;
        double deltaX, deltaY;
        if ((s.Flags5 & Mf5Chasegoal) != 0 || s.TargetIsGoal)
        {
            if (s.GoalNull)
            {
                _ = s.RandomChaseDir?.Invoke();
                return;
            }
            deltaX = s.DeltaX;
            deltaY = s.DeltaY;
        }
        else if (!s.TargetNull)
        {
            deltaX = s.DeltaX;
            deltaY = s.DeltaY;
            if ((s.Flags6 & Mf6Nofear) == 0)
            {
                if ((s.TargetHasPlayer && (s.TargetPlayerCheats & CfFrightening) != 0)
                    || (s.Flags4 & Mf4Frightened) != 0
                    || (s.TargetFlags8 & Mf8Frightening) != 0)
                {
                    deltaX = -deltaX;
                    deltaY = -deltaY;
                }
            }
        }
        else
        {
            _ = s.RandomChaseDir?.Invoke();
            return;
        }

        if (s.FloorZ - s.DropoffZ > s.MaxDropOffHeight
            && s.Z <= s.FloorZ
            && (s.Flags & MfDropoff) == 0
            && (s.Flags2 & Mf2Onmobj) == 0
            && (s.Flags & MfFloat) == 0
            && (s.LevelCompat & CompatDropoff) == 0
            && s.DropoffLines is { Count: > 0 })
        {
            var deltax = 0.0;
            var deltay = 0.0;
            foreach (var line in s.DropoffLines)
            {
                if (line.BackZ == s.Z && line.FrontZ < s.Z - s.MaxDropOffHeight)
                {
                    deltax -= 32 * line.AngleSin;
                    deltay += 32 * line.AngleCos;
                }
                else if (line.FrontZ == s.Z && line.BackZ < s.Z - s.MaxDropOffHeight)
                {
                    deltax -= 32 * line.AngleSin;
                    deltay += 32 * line.AngleCos;
                }
            }

            if (deltax != 0 || deltay != 0)
            {
                s.Flags5 |= Mf5Avoidingdropoff;
                s.DoNewChaseDir?.Invoke(deltax, deltay);
                s.Flags5 &= ~Mf5Avoidingdropoff;
                s.MoveCount = 1;
                return;
            }
        }

        if (!s.TargetNull && s.TargetHealth > 0 && !s.TargetIsFriend && !s.TargetIsGoal)
        {
            if ((s.Flags3 & Mf3Avoidmelee) != 0 || (s.LevelFlags3 & Level3Avoidmelee) != 0)
            {
                var isMelee = !s.TargetHasPlayer
                    ? s.TargetMissileStateNull && s.TargetDistance2D < (s.TargetMeleeRange + s.TargetRadius) * 2
                    : s.ReadyWeaponMelee && s.TargetDistance2D < 192;
                if (isMelee)
                {
                    s.StrafeCount = (s.PrEnemyStrafe?.Invoke() ?? 0) & 15;
                    deltaX = -deltaX;
                    deltaY = -deltaY;
                }
            }
        }

        if (s.StrafeCount == 0 && s.AiRegroupConsumed)
        {
            var regroupDeltaX = s.RegroupX - s.PosX;
            var regroupDeltaY = s.RegroupY - s.PosY;
            if (regroupDeltaX * regroupDeltaX + regroupDeltaY * regroupDeltaY > 64 * 64)
            {
                deltaX = deltaX * 0.75 + regroupDeltaX * 0.25;
                deltaY = deltaY * 0.75 + regroupDeltaY * 0.25;
            }
        }

        s.DoNewChaseDir?.Invoke(deltaX, deltaY);
        if (s.StrafeCount > 0)
            s.MoveCount = s.StrafeCount;
    }
}
