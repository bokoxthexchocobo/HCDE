namespace HCDE.Playsim;

/// <summary>Damage dispatch wrappers and target-switch predicates from <c>p_interaction.cpp</c> (1573–1821).</summary>
public static class VmPInteractionDamageDispatch
{
    public const int MfShootable = 1;
    public const int Mf4Notargetswitch = 1;
    public const int Mf4Quicktoretaliate = 1;
    public const int Mf4Nohateplayers = 1;
    public const int Mf3Notarget = 1;
    public const int Mf5Noinfighting = 1;
    public const int Mf7Nevertarget = 1;
    public const int Mf7Noinfightspecies = 1;
    public const int Mf7Forceinfighting = 1;

    public sealed class SwitchActor
    {
        public int Flags { get; set; }
        public int Flags3 { get; set; }
        public int Flags4 { get; set; }
        public int Flags5 { get; set; }
        public int Flags7 { get; set; }
        public int Threshold { get; set; }
        public int TidToHate { get; set; }
        public int ThingId { get; set; }
        public bool HasPlayer { get; set; }
        public bool OtherHasPlayer { get; set; }
        public int InfightingGroup { get; set; }
        public int OtherInfightingGroup { get; set; }
        public string? Species { get; set; }
        public string? OtherSpecies { get; set; }
        public bool MasterSameClass { get; set; }
        public bool MinionSameClass { get; set; }
        public object? Target { get; set; }
        public int TargetHealth { get; set; }
        public int TargetTid { get; set; }
    }

    public sealed class DamageDispatchContext
    {
        public bool MirrorTarget { get; init; }
        public bool MirrorInflictor { get; init; }
        public bool MirrorSource { get; init; }
        public bool VisualTarget { get; init; }
        public bool VisualInflictor { get; init; }
        public bool VisualSource { get; init; }
        public bool HasVirtualDamageMobj { get; init; }
        public bool LagCompActive { get; init; }
        public bool LagCompReplaying { get; init; }
        public bool SourceNonNull { get; init; }
        public Func<int, bool, int>? DamageMobj { get; init; }
        public Func<int, int>? VmDamageMobj { get; init; }
        public Action<object?, object?>? RecordInvasionAttack { get; init; }
        public Action<object?, object?>? RecordCoopAttack { get; init; }
        public Action<object?, object?, object?, int, int, int, double>? ReactToDamage { get; init; }
        public Action<object?, object?, object?, int, int, int, double>? WorldThingDamaged { get; init; }
        public Action<object?, object?, object?, int, int, int, double>? RecordLagComp { get; init; }
        public Func<SwitchActor, SwitchActor, bool>? IsHostile { get; init; }
        public Func<SwitchActor, SwitchActor, bool>? IsFriend { get; init; }
        public Func<SwitchActor, int>? GetInfighting { get; init; }
        public Func<SwitchActor, SwitchActor, bool>? CheckSight { get; init; }
        public Func<int>? SwitchRandom { get; init; }
        public Func<SwitchActor, SwitchActor, bool>? CallOkayVirtual { get; init; }
    }

    public static bool MirrorOrVisualSkips(DamageDispatchContext ctx) =>
        ctx.MirrorTarget || ctx.MirrorInflictor || ctx.MirrorSource
        || ctx.VisualTarget || ctx.VisualInflictor || ctx.VisualSource;

    public static int DoDamageMobj(DamageDispatchContext ctx, object target, object? inflictor, object? source,
        int damage, int mod, int flags, double angle)
    {
        if (MirrorOrVisualSkips(ctx))
            return 0;

        var needEvent = true;
        var realDamage = ctx.DamageMobj?.Invoke(damage, needEvent) ?? damage;
        if (realDamage > 0)
        {
            ctx.RecordInvasionAttack?.Invoke(source, target);
            ctx.RecordCoopAttack?.Invoke(source, target);
        }

        if (realDamage >= 0)
            ctx.ReactToDamage?.Invoke(target, inflictor, source, realDamage, mod, flags, angle);

        if (realDamage > 0 && needEvent)
            ctx.WorldThingDamaged?.Invoke(target, inflictor, source, realDamage, mod, flags, angle);

        return realDamage < 0 ? 0 : realDamage;
    }

    public static int PDamageMobj(DamageDispatchContext ctx, object target, object? inflictor, object? source,
        int damage, int mod, int flags, double angle)
    {
        if (MirrorOrVisualSkips(ctx))
            return 0;

        var retval = 0;
        if (ctx.HasVirtualDamageMobj && ctx.VmDamageMobj != null)
            retval = ctx.VmDamageMobj(damage);
        else
            retval = DoDamageMobj(ctx, target, inflictor, source, damage, mod, flags, angle);

        if (retval > 0)
        {
            if (ctx.LagCompActive && !ctx.LagCompReplaying)
                ctx.RecordLagComp?.Invoke(target, inflictor, source, damage, mod, flags, angle);
            if (ctx.SourceNonNull && (!ctx.LagCompActive || ctx.LagCompReplaying))
            {
                ctx.RecordInvasionAttack?.Invoke(source, target);
                ctx.RecordCoopAttack?.Invoke(source, target);
            }
        }

        return retval;
    }

    public static bool OkayToSwitchTarget(SwitchActor self, SwitchActor? other, DamageDispatchContext ctx)
    {
        if (other == null)
            return false;
        if (ReferenceEquals(self, other))
            return false;
        if ((other.Flags7 & Mf7Nevertarget) != 0)
            return false;
        if ((other.Flags & MfShootable) == 0)
            return false;
        if ((self.Flags4 & Mf4Notargetswitch) != 0 && self.Target != null)
            return false;

        if ((self.MasterSameClass || self.MinionSameClass)
            && !(ctx.IsHostile?.Invoke(self, other) ?? false)
            && (other.ThingId != self.TidToHate || self.TidToHate == 0)
            && other.TidToHate == self.TidToHate)
            return false;

        if (self.InfightingGroup != 0 && self.InfightingGroup == other.InfightingGroup)
            return false;

        if ((self.Flags7 & Mf7Noinfightspecies) != 0
            && string.Equals(self.Species, other.OtherSpecies, StringComparison.Ordinal))
            return false;

        if ((other.Flags3 & Mf3Notarget) != 0
            && (other.ThingId != self.TidToHate || self.TidToHate == 0)
            && !(ctx.IsHostile?.Invoke(self, other) ?? false))
            return false;

        if (self.Threshold != 0 && (self.Flags4 & Mf4Quicktoretaliate) == 0)
            return false;

        if (ctx.IsFriend?.Invoke(self, other) ?? false)
            return false;

        var infight = (self.Flags7 & Mf7Forceinfighting) != 0 ? 1
            : (self.Flags5 & Mf5Noinfighting) != 0 ? -1
            : ctx.GetInfighting?.Invoke(self) ?? 0;

        if (infight < 0 && !other.OtherHasPlayer && !(ctx.IsHostile?.Invoke(self, other) ?? false))
            return false;

        if (self.TidToHate != 0 && self.TidToHate == other.TidToHate)
            return false;

        if (other.OtherHasPlayer && (self.Flags4 & Mf4Nohateplayers) != 0)
            return false;

        if (self.Target != null && self.TargetHealth > 0 && self.TidToHate != 0 && self.TargetTid == self.TidToHate
            && (ctx.SwitchRandom?.Invoke() ?? 255) < 128
            && (ctx.CheckSight?.Invoke(self, other) ?? false))
            return false;

        return true;
    }

    public static bool CallOkayToSwitchTarget(SwitchActor self, SwitchActor? other, DamageDispatchContext ctx)
    {
        if (ctx.CallOkayVirtual != null)
            return ctx.CallOkayVirtual(self, other);
        return OkayToSwitchTarget(self, other, ctx);
    }
}
