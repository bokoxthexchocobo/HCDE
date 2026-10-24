namespace HCDE.Playsim;

/// <summary>Faithful port of look core (<c>p_enemy.cpp</c> 1268–1440, 1789–1933).</summary>
public static class VmPEnemyLookCore
{
    public const int MfShootable = 1;
    public const int MfShadow = 1 << 1;
    public const int MfFriendly = 1;
    public const int Mf3Ghost = 1;
    public const int Mf3Huntplayers = 1 << 1;
    public const int Mf6Seeinvisible = 1;
    public const int Mf8Mvisblocked = 1;
    public const int Mf2Dormant = 1;
    public const int Mf3Ismonster = 1;
    public const int Mf7Nevertarget = 1;
    public const int Mf8Seefriendlymonsters = 1;
    public const int CfNotarget = 1;
    public const int CompatInvisibility = 1;
    public const int LofDontchasegoal = 1;
    public const int Maxplayers = 8;

    public sealed class LookExParams
    {
        public double MaxDist { get; set; }
        public double MinDist { get; set; }
        public double FovDegrees { get; set; }
        public int Flags { get; set; }
    }

    public sealed class IsVisibleState
    {
        public double Distance2D { get; set; }
        public double MaxDist { get; set; }
        public double MinDist { get; set; }
        public double FovDegrees { get; set; }
        public bool AllAround { get; set; }
        public double AngleToTarget { get; set; }
        public double YawDegrees { get; set; }
        public double MeleeRange { get; set; }
        public double Radius { get; set; }
        public bool TargetMvisBlocked { get; set; }
        public Func<bool>? CheckSight { get; set; }
    }

    public static bool IsVisible(IsVisibleState s)
    {
        if (s.TargetMvisBlocked) return false;
        if (s.MaxDist > 0 && s.Distance2D > s.MaxDist) return false;
        if (s.MinDist > 0 && s.Distance2D < s.MinDist) return false;
        var fov = s.AllAround ? 0 : s.FovDegrees;
        if (fov > 0)
        {
            var an = Math.Abs(NormalizeAngle(s.AngleToTarget - s.YawDegrees));
            if (an > fov / 2)
            {
                if (s.MinDist > 0 || s.Distance2D > s.MeleeRange + s.Radius)
                    return false;
            }
        }
        return s.CheckSight?.Invoke() == true;
    }

    static double NormalizeAngle(double a)
    {
        while (a > 180) a -= 360;
        while (a < -180) a += 360;
        return a;
    }

    public sealed class TargetablePlayerState
    {
        public bool MoNull { get; set; }
        public int MoFlags { get; set; }
        public bool IsFriend { get; set; }
        public int Cheats { get; set; }
        public int Health { get; set; }
        public int MoFlags3 { get; set; }
        public int CompatFlags { get; set; }
        public double Distance2D { get; set; }
        public double VelLenSq { get; set; }
        public int ActorFlags6 { get; set; }
        public IsVisibleState Visible { get; } = new();
        public Func<int>? PrLookForPlayers { get; set; }
    }

    public static bool IsTargetablePlayer(TargetablePlayerState s)
    {
        if (s.MoNull) return false;
        if ((s.MoFlags & MfShootable) == 0) return false;
        if (s.IsFriend) return false;
        if ((s.Cheats & CfNotarget) != 0) return false;
        if (s.Health <= 0) return false;
        if (!IsVisible(s.Visible)) return false;
        if ((s.ActorFlags6 & Mf6Seeinvisible) == 0)
        {
            if (((s.MoFlags & MfShadow) != 0 && (s.CompatFlags & CompatInvisibility) == 0) || (s.MoFlags3 & Mf3Ghost) != 0)
            {
                if (s.Distance2D > 128 && s.VelLenSq < 25)
                    return false;
                if ((s.PrLookForPlayers?.Invoke() ?? 0) < 225)
                    return false;
            }
        }
        return true;
    }

    public sealed class ValidEnemyBlockState
    {
        public int Flags { get; set; }
        public int Health { get; set; }
        public int Flags2 { get; set; }
        public int Flags3 { get; set; }
        public int Flags7 { get; set; }
        public int Flags8 { get; set; }
        public bool LookeeFriendly { get; set; }
        public bool OtherFriendly { get; set; }
        public bool IsFriend { get; set; }
        public bool HasTarget { get; set; }
        public bool TargetFriendly { get; set; }
        public int TargetFlags { get; set; }
        public int TargetHealth { get; set; }
        public int TargetFlags2 { get; set; }
        public bool MutualTarget { get; set; }
        public int SkipRoll { get; set; }
        public int TargHealth { get; set; }
        public int SpawnHealth { get; set; }
        public bool Visible { get; set; }
    }

    public static bool ValidEnemyInBlock(ValidEnemyBlockState s)
    {
        if ((s.Flags & MfShootable) == 0 || s.Health <= 0 || (s.Flags2 & Mf2Dormant) != 0) return false;
        if ((s.Flags3 & Mf3Ismonster) == 0) return false;
        if ((s.Flags7 & Mf7Nevertarget) != 0) return false;
        var keep = false;
        if (s.LookeeFriendly)
        {
            if (s.OtherFriendly)
            {
                if (!s.IsFriend) keep = true;
                else if (s.HasTarget && !s.TargetFriendly
                         && (s.TargetFlags & MfShootable) != 0 && s.TargetHealth > 0 && (s.TargetFlags2 & Mf2Dormant) == 0)
                    return false;
            }
            else keep = true;
        }
        else if ((s.Flags8 & Mf8Seefriendlymonsters) != 0 && s.OtherFriendly)
            keep = true;

        if (keep && s.MutualTarget && s.SkipRoll > 100 && s.IsFriend && s.TargHealth * 2 >= s.SpawnHealth)
            return false;
        return !keep || s.Visible;
    }

    public sealed class LookForPlayersState
    {
        public int TidToHate { get; set; }
        public int Flags { get; set; }
        public int Flags3 { get; set; }
        public int Flags8 { get; set; }
        public int FriendPlayer { get; set; }
        public bool Deathmatch { get; set; }
        public bool Teamplay { get; set; }
        public int Team { get; set; }
        public bool PrimaryLevel { get; set; }
        public bool Multiplayer { get; set; }
        public int Player0Health { get; set; }
        public bool GoalNull { get; set; }
        public int GameState { get; set; }
        public int LastLookPlayer { get; set; }
        public bool[] PlayerInGame { get; } = new bool[Maxplayers];
        public bool ChaseGoal { get; set; } = true;
        public object? Target { get; set; }
        public object? Goal { get; set; }
        public object? LastEnemy { get; set; }
        public int LastEnemyHealth { get; set; }
        public bool LastEnemyIsFriend { get; set; }
        public int ReactionTime { get; set; }
        public Func<bool>? LookForTid { get; set; }
        public Func<bool>? LookForEnemies { get; set; }
        public Func<bool>? LookForMonsters { get; set; }
        public Func<int>? PrLook2 { get; set; }
        public Func<int, TargetablePlayerState, bool>? IsTargetable { get; set; }
    }

    public static bool LookForPlayers(LookForPlayersState s, bool allaround, LookExParams? parameters)
    {
        var chaseGoal = parameters == null || (parameters.Flags & LofDontchasegoal) == 0;
        if (s.TidToHate != 0)
        {
            if (s.LookForTid?.Invoke() == true) return true;
            if ((s.Flags3 & Mf3Huntplayers) == 0) return false;
        }
        else if ((s.Flags & MfFriendly) != 0)
        {
            var result = s.LookForEnemies?.Invoke() == true;
            if (s.FriendPlayer == 0 && (!s.Teamplay || s.Team == 0)) return result;
            if (result || !s.Deathmatch) return true;
        }
        else if ((s.Flags8 & Mf8Seefriendlymonsters) != 0)
        {
            if (s.LookForEnemies?.Invoke() == true) return true;
        }

        if (!s.Multiplayer && s.PrimaryLevel && s.Player0Health <= 0 && s.GoalNull && s.GameState != 0)
            return s.LookForMonsters?.Invoke() == true;

        var c = 0;
        var pnum = s.TidToHate != 0 ? (s.PrLook2?.Invoke() ?? 0) & (Maxplayers - 1) : s.LastLookPlayer;
        while (true)
        {
            if (c++ < Maxplayers)
            {
                pnum = (pnum + 1) & (Maxplayers - 1);
                if (!s.PlayerInGame[pnum]) continue;
                if (s.TidToHate == 0) s.LastLookPlayer = pnum;
            }
            else
            {
                if (s.Target == null)
                {
                    if (s.Goal != null && chaseGoal)
                    {
                        s.Target = s.Goal;
                        return true;
                    }
                    if (s.LastEnemy != null && s.LastEnemyHealth > 0 && !s.LastEnemyIsFriend)
                    {
                        s.Target = s.LastEnemy;
                        s.LastEnemy = null;
                        return true;
                    }
                    if (s.LastEnemyIsFriend) s.LastEnemy = null;
                }
                return ReferenceEquals(s.Target, s.Goal) && s.Goal != null;
            }

            if (s.IsTargetable?.Invoke(pnum, new TargetablePlayerState()) != true)
                continue;
            if (s.Goal != null && ReferenceEquals(s.Target, s.Goal))
                s.ReactionTime = 0;
            s.Target = pnum;
            return true;
        }
    }
}
