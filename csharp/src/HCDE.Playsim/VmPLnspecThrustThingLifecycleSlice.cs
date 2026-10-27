namespace HCDE.Playsim;

/// <summary>
/// Thrust, thing special/TID, damage/heal, and activate/deactivate from <c>p_lnspec.cpp</c> (**1205–1479**).
/// </summary>
public static class VmPLnspecThrustThingLifecycleSlice
{
    public const double MaxMove = 30.0;
    public const int TelefragDamage = 1_000_000;
    public const int Level2HexenHack = 1;
    public const int ThingSpecActivate = 1;
    public const int ThingSpecDeactivate = 2;
    public const int ThingSpecSwitch = 4;

    public sealed class LineContext
    {
        public Func<int, int, int, bool, bool>? ThrustByTid { get; init; }
        public Func<int, int, bool, bool>? ThrustActivator { get; init; }
        public Func<int, double, bool, bool>? ThrustZByTid { get; init; }
        public Func<double, bool, bool>? ThrustZActivator { get; init; }
        public Func<int, int, int, int, int, bool>? ApplySpecialToActivator { get; init; }
        public Func<int, int, int, int, int, bool>? ApplySpecialByTid { get; init; }
        public Func<int, bool>? SetTidOnActivator { get; init; }
        public Func<int, int, bool>? SetTidByOldTid { get; init; }
        public Func<int, int, bool>? DamageActivator { get; init; }
        public Func<int, int, bool>? HealActivator { get; init; }
        public Func<int, int, bool>? ActivateByTid { get; init; }
        public Func<bool>? ActivateActivator { get; init; }
        public Func<int, int, bool>? DeactivateByTid { get; init; }
        public Func<bool>? DeactivateActivator { get; init; }
        public int LevelFlags2 { get; init; }
        public bool HasActivator { get; init; }
        public bool BackSide { get; init; }
        public int DehMaxSoulsphere { get; init; }
    }

    public static double ByteAngleToDegrees(int byteAngle) => byteAngle * (360.0 / 256.0);

    public static (double vx, double vy) ThrustClampVelocity(double vx, double vy, bool noLimit)
    {
        if (noLimit)
            return (vx, vy);
        return (Math.Clamp(vx, -MaxMove, MaxMove), Math.Clamp(vy, -MaxMove, MaxMove));
    }

    public static void ThrustThingHelper(double vx, double vy, double angleDeg, double force, bool noLimit, out double outVx, out double outVy)
    {
        var rad = angleDeg * Math.PI / 180.0;
        var nx = vx + Math.Cos(rad) * force;
        var ny = vy + Math.Sin(rad) * force;
        (outVx, outVy) = ThrustClampVelocity(nx, ny, noLimit);
    }

    public static bool ThrustThing(int angle, int force, bool noLimit, int tid, LineContext ctx)
    {
        if (tid != 0)
            return ctx.ThrustByTid?.Invoke(tid, angle, force, noLimit) == true;
        if (!ctx.HasActivator)
            return false;
        if ((ctx.LevelFlags2 & Level2HexenHack) != 0 && ctx.BackSide)
            return false;
        return ctx.ThrustActivator?.Invoke(angle, force, noLimit) == true;
    }

    public static double ThrustZValue(int zThrust, bool downUp) =>
        (downUp ? -1 : 1) * (zThrust / 4.0);

    public static double ApplyThrustZ(double currentZ, double thrust, bool setMode) =>
        setMode ? currentZ + thrust : thrust;

    public static bool ThrustThingZ(int tid, int zThrust, bool downUp, bool setMode, LineContext ctx)
    {
        var thrust = ThrustZValue(zThrust, downUp);
        if (tid != 0)
            return ctx.ThrustZByTid?.Invoke(tid, thrust, setMode) == true;
        if (!ctx.HasActivator)
            return false;
        return ctx.ThrustZActivator?.Invoke(thrust, setMode) == true;
    }

    public static bool ThingSetSpecial(int tid, int special, int a0, int a1, int a2, LineContext ctx)
    {
        if (tid == 0 && ctx.HasActivator)
            ctx.ApplySpecialToActivator?.Invoke(special, a0, a1, a2, 0);
        else if (tid != 0)
            ctx.ApplySpecialByTid?.Invoke(tid, special, a0, a1, a2);
        return true;
    }

    public static bool ThingChangeTid(int oldTid, int newTid, LineContext ctx)
    {
        if (oldTid == 0 && ctx.HasActivator)
            ctx.SetTidOnActivator?.Invoke(newTid);
        else if (oldTid != 0)
            ctx.SetTidByOldTid?.Invoke(oldTid, newTid);
        return true;
    }

    public static bool DamageThing(int damage, int mod, LineContext ctx)
    {
        if (!ctx.HasActivator)
            return false;
        if (damage < 0)
            return ctx.HealActivator?.Invoke(-damage, mod) == true;
        if (damage > 0)
            return ctx.DamageActivator?.Invoke(damage, mod) == true;
        return ctx.DamageActivator?.Invoke(TelefragDamage, mod) == true;
    }

    public static int ResolveHealMax(int maxArg, int dehMaxSoulsphere) =>
        maxArg == 1 ? dehMaxSoulsphere : maxArg;

    public static int ApplyHealCap(int health, int amount, int max)
    {
        if (max != 0 && health >= max)
            return health;
        var nh = health + amount;
        if (nh > max && max > 0)
            nh = max;
        return nh;
    }

    public static bool HealThing(int amount, int maxArg, LineContext ctx)
    {
        if (!ctx.HasActivator)
            return false;
        return ctx.HealActivator?.Invoke(amount, maxArg) == true;
    }

    public static int ToggleActivationType(int type, bool activate)
    {
        if (activate)
        {
            if ((type & ThingSpecActivate) == 0)
                return type;
            type &= ~ThingSpecActivate;
            if ((type & ThingSpecSwitch) != 0)
                type |= ThingSpecDeactivate;
            return type;
        }
        if ((type & ThingSpecDeactivate) == 0)
            return type;
        type &= ~ThingSpecDeactivate;
        if ((type & ThingSpecSwitch) != 0)
            type |= ThingSpecActivate;
        return type;
    }

    public static bool ThingActivate(int tid, LineContext ctx)
    {
        if (tid != 0)
            return ctx.ActivateByTid?.Invoke(tid, 0) == true;
        if (!ctx.HasActivator)
            return false;
        return ctx.ActivateActivator?.Invoke() == true;
    }

    public static bool ThingDeactivate(int tid, LineContext ctx)
    {
        if (tid != 0)
            return ctx.DeactivateByTid?.Invoke(tid, 0) == true;
        if (!ctx.HasActivator)
            return false;
        return ctx.DeactivateActivator?.Invoke() == true;
    }
}
