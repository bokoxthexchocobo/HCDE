namespace HCDE.Playsim;

/// <summary>Faithful port of range/chase helpers (<c>p_enemy.cpp</c> 253–365, 384–399, 1107–1257).</summary>
public static class VmPEnemyChaseRandom
{
    public const int MfFriendly = 1;
    public const int MfJusthit = 1 << 1;
    public const int Mf5Noverticalmeleerange = 1;
    public const int SecfNoattack = 1;
    public const int DiNodir = 8;
    public const int DiEast = 0;
    public const int DiNorth = 2;
    public const int DiSouth = 6;
    public const int DiWest = 4;
    public const int DiSoutheast = 7;
    public const int Maxplayers = 8;
    public const int SfSeepastblockeverything = 1;

    private static readonly int[] Opposite = { 4, 5, 6, 7, 0, 1, 2, 3, 8 };
    private static readonly int[] Diags = { 3, 1, 5, 7 };

    public sealed class MeleeRangeState
    {
        public bool TargetNull { get; set; }
        public double Distance2D { get; set; }
        public double MeleeRange { get; set; }
        public double TargetRadius { get; set; }
        public bool TargetIsGoal { get; set; }
        public int SectorFlags { get; set; }
        public int Flags5 { get; set; }
        public double TargetZ { get; set; }
        public double ActorTop { get; set; }
        public double ActorZ { get; set; }
        public bool IsFriend { get; set; }
        public Func<bool>? CheckSight { get; set; }
    }

    public static bool CheckMeleeRange(MeleeRangeState s, double range)
    {
        if (s.TargetNull) return false;
        if (range < 0) range = s.MeleeRange;
        if (s.Distance2D >= range + s.TargetRadius) return false;
        if (s.TargetIsGoal) return true;
        if ((s.SectorFlags & SecfNoattack) != 0) return false;
        if ((s.Flags5 & Mf5Noverticalmeleerange) == 0)
        {
            if (s.TargetZ > s.ActorTop) return false;
            if (s.TargetZ + s.TargetRadius < s.ActorZ) return false;
        }
        if (s.IsFriend) return false;
        return s.CheckSight?.Invoke() == true;
    }

    public sealed class MissileRangeState
    {
        public int SectorFlags { get; set; }
        public int Flags { get; set; }
        public int ReactionTime { get; set; }
        public bool TargetIsFriend { get; set; }
        public bool HitFriend { get; set; }
        public bool MeleeStateNull { get; set; }
        public double Distance2D { get; set; }
        public double MaxTargetRange { get; set; }
        public double MeleeThreshold { get; set; }
        public double MissileChanceMult { get; set; }
        public double MinMissileChance { get; set; }
        public double SkillAggressiveness { get; set; }
        public int TargetHealth { get; set; }
        public bool TargetHasPlayer { get; set; }
        public int TargetFlags { get; set; }
        public Func<bool>? CheckSight { get; set; }
        public Func<int>? PrDefect { get; set; }
        public Func<int>? PrCheckMissileRange { get; set; }
    }

    public static bool CheckMissileRange(MissileRangeState s)
    {
        if ((s.SectorFlags & SecfNoattack) != 0) return false;
        if (s.CheckSight?.Invoke() != true) return false;

        if ((s.Flags & MfJusthit) != 0)
        {
            s.Flags &= ~MfJusthit;
            if ((s.Flags & MfFriendly) == 0) return true;
            if (s.TargetHealth <= 0) return false;
            if (!s.TargetIsFriend) return true;
            if (s.TargetHasPlayer) return (s.PrDefect?.Invoke() ?? 0) > 128;
            return (s.TargetFlags & MfJusthit) == 0 && (s.PrDefect?.Invoke() ?? 0) > 128;
        }

        if (s.ReactionTime > 0) return false;
        if (s.TargetIsFriend) return false;
        if ((s.Flags & MfFriendly) != 0 && s.HitFriend) return false;

        var dist = s.Distance2D - 64;
        if (s.MeleeStateNull) dist -= 128;
        if (s.MaxTargetRange > 0 && dist > s.MaxTargetRange) return false;
        if (!s.MeleeStateNull && dist < s.MeleeThreshold) return false;
        dist *= s.MissileChanceMult;
        var mmc = (int)(s.MinMissileChance * s.SkillAggressiveness);
        return (s.PrCheckMissileRange?.Invoke() ?? 256) >= Math.Min((int)dist, mmc);
    }

    public sealed class HitFriendState
    {
        public int Flags { get; set; }
        public bool TargetNull { get; set; }
        public bool LineTargetIsFriend { get; set; }
        public bool LineTargetDiffersFromTarget { get; set; }
    }

    public static bool HitFriend(HitFriendState s) =>
        (s.Flags & MfFriendly) != 0 && !s.TargetNull && s.LineTargetDiffersFromTarget && s.LineTargetIsFriend;

    public sealed class RandomChaseDirState
    {
        public int Movedir { get; set; }
        public int MoveCount { get; set; }
        public int Flags { get; set; }
        public int Flags8 { get; set; }
        public int FriendPlayer { get; set; }
        public bool Multiplayer { get; set; }
        public bool[] PlayerInGame { get; } = new bool[Maxplayers];
        public bool PlayerMoNull { get; set; }
        public double PlayerDeltaX { get; set; }
        public double PlayerDeltaY { get; set; }
        public Func<bool>? TryWalk { get; set; }
        public Func<bool>? CheckSight { get; set; }
        public Func<int>? PrNewChaseDir { get; set; }
    }

    public static void RandomChaseDir(RandomChaseDirState s)
    {
        var olddir = s.Movedir;
        var turnaround = Opposite[olddir];

        if ((s.Flags & MfFriendly) != 0 && (s.Flags8 & (1 << 3)) == 0 && !s.PlayerMoNull)
        {
            if (((s.PrNewChaseDir?.Invoke() ?? 0) & 1) != 0 || s.CheckSight?.Invoke() == false)
            {
                int d1, d2;
                if (s.PlayerDeltaX > 128) d1 = DiEast;
                else if (s.PlayerDeltaX < -128) d1 = DiWest;
                else d1 = DiNodir;
                if (s.PlayerDeltaY < -128) d2 = DiSouth;
                else if (s.PlayerDeltaY > 128) d2 = DiNorth;
                else d2 = DiNodir;

                if (d1 != DiNodir && d2 != DiNodir)
                {
                    s.Movedir = Diags[((s.PlayerDeltaY < 0 ? 1 : 0) << 1) + (s.PlayerDeltaX > 0 ? 1 : 0)];
                    if (s.Movedir != turnaround && s.TryWalk?.Invoke() == true)
                        return;
                }
            }
        }

        if ((s.PrNewChaseDir?.Invoke() ?? 256) < 150 && s.TryWalk?.Invoke() == true)
            return;

        var turndir = ((s.PrNewChaseDir?.Invoke() ?? 0) & 1) != 0 ? -1 : 1;
        if (olddir == DiNodir)
            olddir = (s.PrNewChaseDir?.Invoke() ?? 0) & 7;

        for (var tdir = (olddir + turndir) & 7; tdir != olddir; tdir = (tdir + turndir) & 7)
        {
            if (tdir != turnaround)
            {
                s.Movedir = tdir;
                if (s.TryWalk?.Invoke() == true)
                    return;
            }
        }

        if (turnaround != DiNodir)
        {
            s.Movedir = turnaround;
            if (s.TryWalk?.Invoke() == true)
            {
                s.MoveCount = (s.PrNewChaseDir?.Invoke() ?? 0) & 15;
                return;
            }
        }

        s.Movedir = DiNodir;
    }
}
