namespace HCDE.Playsim;

/// <summary><c>CheckBossDeath</c> and <c>A_BossDeath</c> from <c>p_enemy.cpp</c> (3271–3444).</summary>
public static class VmPEnemyBossDeath
{
    public const int MfIcecorpse = 1;
    public const int LevelMap07Special = 1;
    public const int LevelSpeckillmonsters = 1 << 1;
    public const int LevelSpecactionsmask = 0x7;
    public const int LevelSpeclowerfloor = 1;
    public const int LevelSpeclowerfloortohighest = 2;
    public const int LevelSpecopendoor = 3;
    public const int CompatfAnybossdeath = 1;
    public const int Mf8Map07Boss1 = 1;
    public const int Mf8Map07Boss2 = 1 << 1;
    public const int DfNoExit = 1;
    public const int SaDoomFormatMask = 0x40000000;
    public const int SaActionMask = 0x3fffffff;

    public sealed class BossActor
    {
        public required string TypeName { get; init; }
        public required string ReplaceeTypeName { get; init; }
        public int Health { get; init; }
        public int Flags { get; init; }
        public int Flags8 { get; init; }
        public int ReplaceeFlags8 { get; init; }
        public bool IsSelf { get; init; }
    }

    public sealed class LevelBossContext
    {
        public int LevelFlags { get; init; }
        public int LevelFlags3 { get; init; }
        public int CompatFlags { get; init; }
        public bool Deathmatch { get; init; }
        public bool AlwaysApplyDmflags { get; init; }
        public int Dmflags { get; init; }
        public IReadOnlyList<BossSpecialAction> SpecialActions { get; init; } = Array.Empty<BossSpecialAction>();
        public Func<int, bool>? PlayerInGame { get; init; }
        public Func<int, int>? PlayerHealth { get; init; }
        public IEnumerable<BossActor>? LivingBosses { get; init; }
        public bool Map07FatsoBoss1 { get; init; }
        public bool Map07ArachBoss2 { get; init; }
        public bool SameReplacement { get; init; }
    }

    public sealed class BossSpecialAction
    {
        public required string Type { get; init; }
        public int Action { get; init; }
        public int[] Args { get; init; } = Array.Empty<int>();
    }

    public static bool AnyPlayerAlive(LevelBossContext ctx)
    {
        for (var i = 0; i < 8; i++)
        {
            if (ctx.PlayerInGame?.Invoke(i) == true && (ctx.PlayerHealth?.Invoke(i) ?? 0) > 0)
                return true;
        }

        return false;
    }

    public static bool LivingBossRemains(BossActor self, BossActor other) =>
        !other.IsSelf
        && (other.Health > 0 || (other.Flags & MfIcecorpse) != 0)
        && (other.TypeName == self.TypeName || other.ReplaceeTypeName == self.ReplaceeTypeName);

    public static bool CheckBossDeath(BossActor self, LevelBossContext ctx)
    {
        if (!AnyPlayerAlive(ctx))
            return false;
        foreach (var other in ctx.LivingBosses ?? Array.Empty<BossActor>())
        {
            if (LivingBossRemains(self, other))
                return false;
        }

        return true;
    }

    public static int MergeReplaceeFlags8(int flags8, int replaceeFlags8, bool typeDiffers) =>
        typeDiffers ? flags8 | replaceeFlags8 : flags8;

    public static bool SpecialActionMatches(string myType, string replaceeType, string actionType) =>
        replaceeType == actionType || myType == actionType;

    public static bool DoomFormatSpecial(int action) => (action & SaDoomFormatMask) != 0;

    public static int TranslateDoomSpecial(int action) => action & SaActionMask;

    public static bool LevelHasBossSpecial(int flags, int flags3) =>
        (flags & (LevelMap07Special | 0x7e)) != 0
        || (flags3 & 0x1f) != 0;

    public static bool Map07BossFlags(int flags8) => (flags8 & (Mf8Map07Boss1 | Mf8Map07Boss2)) != 0;

    public static bool ShouldRunBossExit(BossActor actor, LevelBossContext ctx, int flags8)
    {
        if ((ctx.CompatFlags & CompatfAnybossdeath) != 0)
            return true;
        if ((ctx.LevelFlags & LevelMap07Special) != 0 && Map07BossFlags(flags8))
            return true;
        if ((ctx.LevelFlags & 0x7e) != 0)
            return true;
        if ((ctx.LevelFlags3 & 0x1f) != 0)
            return true;
        return false;
    }

    public static bool ExitBlockedByDmflags(LevelBossContext ctx) =>
        (ctx.Deathmatch || ctx.AlwaysApplyDmflags) && (ctx.Dmflags & DfNoExit) != 0;

    public static BossDeathPlan PlanBossDeath(BossActor self, LevelBossContext ctx)
    {
        var flags8 = MergeReplaceeFlags8(self.Flags8, self.ReplaceeFlags8, self.TypeName != self.ReplaceeTypeName);
        var plan = new BossDeathPlan();
        if (!CheckBossDeath(self, ctx))
            return plan;

        foreach (var sa in ctx.SpecialActions)
        {
            if (!SpecialActionMatches(self.TypeName, self.ReplaceeTypeName, sa.Type))
                continue;
            plan.RunSpecialActions = true;
            plan.SpecialActions.Add(sa);
        }

        if (!LevelHasBossSpecial(ctx.LevelFlags, ctx.LevelFlags3))
            return plan;

        if (!ShouldRunBossExit(self, ctx, flags8))
            return plan;

        plan.RunVictory = true;
        plan.Speckill = (ctx.LevelFlags & LevelSpeckillmonsters) != 0;
        plan.Map07Lower666 = (ctx.LevelFlags & LevelMap07Special) != 0
            && ((flags8 & Mf8Map07Boss1) != 0 || ctx.SameReplacement);
        plan.Map07Raise667 = (ctx.LevelFlags & LevelMap07Special) != 0
            && ((flags8 & Mf8Map07Boss2) != 0 || ctx.SameReplacement);
        plan.SpecAction = ctx.LevelFlags & LevelSpecactionsmask;
        plan.ExitLevel = !ExitBlockedByDmflags(ctx);
        return plan;
    }

    public sealed class BossDeathPlan
    {
        public bool RunSpecialActions { get; set; }
        public List<BossSpecialAction> SpecialActions { get; } = new();
        public bool RunVictory { get; set; }
        public bool Speckill { get; set; }
        public bool Map07Lower666 { get; set; }
        public bool Map07Raise667 { get; set; }
        public int SpecAction { get; set; }
        public bool ExitLevel { get; set; }
    }
}
