namespace HCDE.Playsim;

/// <summary>Inventory drop, friendship copy, and health grant helpers from <c>p_mobj.cpp</c> (1045–1309).</summary>
public static class VmPMobjInventoryGive
{
    public const int MfFriendly = 1;
    public const int Mf3Nosightcheck = 1;
    public const int Mf3Huntplayers = 1 << 1;
    public const int Mf3Notarget = 1 << 2;
    public const int Mf4Nohateplayers = 1;
    public const int Mf4Bossspawned = 1 << 1;
    public const int Mf7Nevertarget = 1;
    public const int MorphFullhealth = 1;
    public const int MorphAddstamina = 1 << 1;
    public const int Maxmorphhealth = 50;
    public const int PstDead = 4;

    public sealed class DropInventoryContext
    {
        public Func<object?, object?, int, object?>? VmDropInventory { get; init; }
    }

    public static object? DropInventory(DropInventoryContext ctx, object actor, object? item, int amt) =>
        ctx.VmDropInventory?.Invoke(actor, item, amt);

    public sealed class FriendActor
    {
        public int Health { get; set; }
        public int Flags { get; set; }
        public int Flags3 { get; set; }
        public int Flags4 { get; set; }
        public int TidToHate { get; set; }
        public int LastLookActorId { get; set; }
        public int LastLookPlayerNumber { get; set; }
        public int FriendPlayer { get; set; }
        public int DesignatedTeam { get; set; }
        public object? Target { get; set; }
        public int TargetFlags3 { get; set; }
        public int TargetFlags7 { get; set; }
        public object? LastHeard { get; set; }
    }

    public sealed class FriendlinessContext
    {
        public Func<FriendActor, bool>? CountsAsKill { get; init; }
        public Func<FriendActor, int>? SpawnHealth { get; init; }
        public Action<bool>? AdjustTotalMonsters { get; init; }
    }

    public static void CopyFriendliness(
        FriendlinessContext ctx,
        FriendActor self,
        FriendActor other,
        bool changeTarget,
        bool resetHealth)
    {
        if (self.Health > 0)
            ctx.AdjustTotalMonsters?.Invoke(false);
        self.TidToHate = other.TidToHate;
        self.LastLookActorId = other.LastLookActorId;
        self.LastLookPlayerNumber = other.LastLookPlayerNumber;
        self.Flags = (self.Flags & ~MfFriendly) | (other.Flags & MfFriendly);
        self.Flags3 = (self.Flags3 & ~(Mf3Nosightcheck | Mf3Huntplayers))
                      | (other.Flags3 & (Mf3Nosightcheck | Mf3Huntplayers));
        self.Flags4 = (self.Flags4 & ~(Mf4Nohateplayers | Mf4Bossspawned))
                      | (other.Flags4 & (Mf4Nohateplayers | Mf4Bossspawned));
        self.FriendPlayer = other.FriendPlayer;
        self.DesignatedTeam = other.DesignatedTeam;
        if (changeTarget && other.Target != null
            && (other.TargetFlags3 & Mf3Notarget) == 0
            && (other.TargetFlags7 & Mf7Nevertarget) == 0)
        {
            self.LastHeard = other.Target;
            self.Target = other.Target;
        }

        if (resetHealth)
            self.Health = ctx.SpawnHealth?.Invoke(self) ?? self.Health;
        if (self.Health > 0)
            ctx.AdjustTotalMonsters?.Invoke(true);
    }

    public sealed class MaxHealthQuery
    {
        public bool HasPlayer { get; init; }
        public int MorphStyle { get; init; }
        public bool HasAlternative { get; init; }
        public int Stamina { get; init; }
        public int BonusHealth { get; init; }
        public int MaxPickupHealth { get; init; }
        public int ExplicitMax { get; init; }
        public Func<int>? DefaultMaxHealth { get; init; }
    }

    public static int GetRealMaxHealth(MaxHealthQuery q)
    {
        var max = q.ExplicitMax;
        var origMax = max;
        if (max <= 0)
        {
            max = q.DefaultMaxHealth?.Invoke() ?? 0;
            if (q.HasAlternative)
            {
                if ((q.MorphStyle & MorphFullhealth) != 0)
                {
                    if ((q.MorphStyle & MorphAddstamina) == 0)
                        max -= q.Stamina + q.BonusHealth;
                }
                else
                {
                    max = Maxmorphhealth;
                    if ((q.MorphStyle & MorphAddstamina) != 0)
                        max += q.Stamina + q.BonusHealth;
                }
            }
        }
        else if (!q.HasAlternative || (q.MorphStyle & MorphAddstamina) != 0)
        {
            max += q.BonusHealth;
        }

        if (q.MaxPickupHealth != 0)
        {
            max = q.MaxPickupHealth;
            if (origMax > 0 && origMax > max)
                max = origMax;
        }

        return max;
    }

    public sealed class GiveBodyActor
    {
        public int Health { get; set; }
        public bool HasPlayer { get; set; }
        public int PlayerHealth { get; set; }
        public int PlayerState { get; set; }
    }

    public sealed class GiveBodyContext
    {
        public Func<GiveBodyActor, int, int>? ResolveMaxHealth { get; init; }
        public Func<int, double>? SkillHealthFactor { get; init; }
        public Func<GiveBodyActor, int>? MonsterSpawnHealth { get; init; }
    }

    public static bool GiveBody(GiveBodyContext ctx, GiveBodyActor actor, int num, int max)
    {
        if (actor.Health <= 0 || (actor.HasPlayer && actor.PlayerState == PstDead))
            return false;

        num = Math.Clamp(num, -65536, 65536);
        if (actor.HasPlayer)
        {
            max = ctx.ResolveMaxHealth?.Invoke(actor, max) ?? max;
            if (num < 0)
            {
                num = max * -num / 100;
                if (actor.PlayerHealth < num)
                {
                    actor.PlayerHealth = num;
                    actor.Health = num;
                    return true;
                }
            }
            else if (num > 0)
            {
                if (actor.PlayerHealth < max)
                {
                    var factor = ctx.SkillHealthFactor?.Invoke(num) ?? 1.0;
                    num = (int)(num * factor);
                    if (num < 1) num = 1;
                    actor.PlayerHealth += num;
                    if (actor.PlayerHealth > max)
                        actor.PlayerHealth = max;
                    actor.Health = actor.PlayerHealth;
                    return true;
                }
            }
        }
        else
        {
            max = ctx.MonsterSpawnHealth?.Invoke(actor) ?? max;
            if (num < 0)
            {
                num = max * -num / 100;
                if (actor.Health < num)
                {
                    actor.Health = num;
                    return true;
                }
            }
            else if (actor.Health < max)
            {
                actor.Health += num;
                if (actor.Health > max)
                    actor.Health = max;
                return true;
            }
        }

        return false;
    }

    public static bool GiveBodyAction(GiveBodyContext ctx, GiveBodyActor actor, int num, int max) =>
        GiveBody(ctx, actor, num, max);
}
