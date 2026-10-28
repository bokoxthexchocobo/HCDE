namespace HCDE.Playsim;

/// <summary><c>EV_Thing_Projectile</c> and <c>EV_Thing_Damage</c> from <c>p_things.cpp</c> (250–420).</summary>
public static class VmPThingsProjectileDamage
{
    public const int Mf3Ismonster = VmPThingsSpawnMoveIntercept.Mf3Ismonster;
    public const int Mf3Floorhugger = 1;
    public const int Mf3Ceilinghugger = 1 << 1;
    public const int Mf2Seekermissile = 1 << 7;
    public const int MfNogravity = VmPThingsSpawnMoveIntercept.MfNogravity;
    public const int MfMissile = 1 << 17;
    public const int MfSpecial = VmPThingsSpawnMoveIntercept.MfSpecial;
    public const int MfDropped = VmPThingsSpawnMoveIntercept.MfDropped;
    public const int MfShootable = 1;
    public const int DfNoMonsters = VmPThingsSpawnMoveIntercept.DfNoMonsters;
    public const int Level2Nomonsters = VmPThingsSpawnMoveIntercept.Level2Nomonsters;
    public const double OnFloorZ = VmPMobjSpawnMissileVariants.OnFloorZ;
    public const double OnCeilingZ = VmPMobjSpawnMissileVariants.OnCeilingZ;

    public sealed class ProjectileSpot
    {
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double Floorclip { get; set; }
        public double Radius { get; set; }
        public object? TargetSpot { get; set; }
    }

    public sealed class ProjectileMobj
    {
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags3 { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double VelZ { get; set; }
        public double YawDegrees { get; set; }
        public double Speed { get; set; }
        public object? Target { get; set; }
        public object? Tracer { get; set; }
        public double Gravity { get; set; } = 1;
        public int NewTid { get; set; }
        public bool Destroyed { get; set; }
    }

    public sealed class EvThingProjectileContext
    {
        public int Tid { get; init; }
        public object? Source { get; init; }
        public int Type { get; init; }
        public string? TypeName { get; init; }
        public double AngleDegrees { get; init; }
        public double Speed { get; init; }
        public double VSpeed { get; init; }
        public int DestTid { get; init; }
        public object? ForceDest { get; init; }
        public int Gravity { get; init; }
        public int NewTid { get; init; }
        public bool LeadTarget { get; init; }
        public int DmFlags { get; init; }
        public int LevelFlags2 { get; init; }
        public Func<int, string?, object?>? ResolveKind { get; init; }
        public Func<object?, object?>? GetReplacement { get; init; }
        public Func<object?, int>? GetDefaultFlags3 { get; init; }
        public Func<int, Func<object?>>? CreateSpotIterator { get; init; }
        public Func<int, Func<object?>>? CreateDestIterator { get; init; }
        public Func<ProjectileSpot, object?, double, ProjectileMobj?>? SpawnAtZ { get; init; }
        public Action<ProjectileMobj, ProjectileSpot>? PlaySpawnSound { get; init; }
        public Action<ProjectileMobj, object?, double, bool>? VelIntercept { get; init; }
        public Action<ProjectileMobj, double>? VelFromAngle { get; init; }
        public Func<ProjectileMobj, double>? VelToSpeed { get; init; }
        public Func<ProjectileMobj, ProjectileSpot, bool>? CheckMissileSpawn { get; init; }
        public Func<ProjectileMobj, bool>? TestMobjLocation { get; init; }
        public Action<ProjectileMobj>? ClearCounters { get; init; }
        public Action<ProjectileMobj>? DestroyMobj { get; init; }
        public Func<object?, ProjectileSpot>? AsSpot { get; init; }
        public Func<object?, VmPThingsSpawnMoveIntercept.InterceptTarget>? AsInterceptTarget { get; init; }
    }

    public static bool EvThingProjectile(EvThingProjectileContext ctx)
    {
        var rtn = false;
        object? kind = ctx.TypeName == null
            ? ctx.ResolveKind?.Invoke(ctx.Type, null)
            : ctx.ResolveKind?.Invoke(0, ctx.TypeName);
        if (kind == null)
            return false;
        kind = ctx.GetReplacement?.Invoke(kind) ?? kind;
        var defflags3 = ctx.GetDefaultFlags3?.Invoke(kind) ?? 0;
        if ((defflags3 & Mf3Ismonster) != 0
            && ((ctx.DmFlags & DfNoMonsters) != 0 || (ctx.LevelFlags2 & Level2Nomonsters) != 0))
            return false;

        var nextSpot = ctx.CreateSpotIterator?.Invoke(ctx.Tid);
        object? spotObj = ctx.Tid == 0 ? ctx.Source : nextSpot?.Invoke();
        var forcedest = ctx.ForceDest;
        while (spotObj != null)
        {
            var destIter = ctx.CreateDestIterator?.Invoke(ctx.DestTid);
            object? targ = forcedest;
            if (ctx.DestTid == 0 || (targ = destIter?.Invoke()) != null)
            {
                do
                {
                    var spot = ctx.AsSpot?.Invoke(spotObj) ?? new ProjectileSpot();
                    var z = spot.PosZ;
                    if ((defflags3 & Mf3Floorhugger) != 0)
                        z = OnFloorZ;
                    else if ((defflags3 & Mf3Ceilinghugger) != 0)
                        z = OnCeilingZ;
                    else if (z != OnFloorZ)
                        z -= spot.Floorclip;

                    var mobj = ctx.SpawnAtZ?.Invoke(spot, kind, z);
                    if (mobj != null)
                    {
                        mobj.NewTid = ctx.NewTid;
                        ctx.PlaySpawnSound?.Invoke(mobj, spot);
                        if (ctx.Gravity != 0)
                        {
                            mobj.Flags &= ~MfNogravity;
                            if ((mobj.Flags3 & Mf3Ismonster) == 0 && ctx.Gravity == 1)
                                mobj.Gravity = 1.0 / 8;
                        }
                        else
                            mobj.Flags |= MfNogravity;

                        mobj.Target = spot.TargetSpot ?? spotObj;

                        if (targ != null)
                        {
                            ctx.VelIntercept?.Invoke(mobj, targ, ctx.Speed, ctx.LeadTarget);
                            if ((mobj.Flags2 & Mf2Seekermissile) != 0)
                                mobj.Tracer = targ;
                        }
                        else
                        {
                            mobj.YawDegrees = ctx.AngleDegrees;
                            ctx.VelFromAngle?.Invoke(mobj, ctx.Speed);
                            mobj.VelZ = ctx.VSpeed;
                        }

                        if ((mobj.Flags & MfMissile) != 0)
                            mobj.Speed = ctx.VelToSpeed?.Invoke(mobj) ?? mobj.Speed;

                        if ((mobj.Flags3 & (Mf3Floorhugger | Mf3Ceilinghugger)) != 0)
                            mobj.VelZ = 0;

                        if ((mobj.Flags & MfSpecial) != 0)
                            mobj.Flags |= MfDropped;

                        if ((mobj.Flags & MfMissile) != 0)
                        {
                            if (ctx.CheckMissileSpawn?.Invoke(mobj, spot) == true)
                                rtn = true;
                        }
                        else if (ctx.TestMobjLocation?.Invoke(mobj) != true)
                        {
                            ctx.ClearCounters?.Invoke(mobj);
                            ctx.DestroyMobj?.Invoke(mobj);
                        }
                        else
                            rtn = true;
                    }
                } while (ctx.DestTid != 0 && (targ = destIter?.Invoke()) != null);
            }
            spotObj = ctx.Tid == 0 ? null : nextSpot?.Invoke();
        }
        return rtn;
    }

    public sealed class DamageActor
    {
        public int Flags { get; set; }
        public int Health { get; set; }
        public int SpawnHealth { get; set; }
        public bool HasPlayer { get; set; }
        public int PlayerHealth { get; set; }
    }

    public sealed class EvThingDamageContext
    {
        public int Tid { get; init; }
        public object? WhoFor { get; init; }
        public int Amount { get; init; }
        public object? DamageType { get; init; }
        public Func<int, Func<object?>>? CreateIterator { get; init; }
        public Action<DamageActor, object?, object?, int, object?>? DamageMobj { get; init; }
        public Func<object?, DamageActor>? AsDamageActor { get; init; }
    }

    public static int EvThingDamage(EvThingDamageContext ctx)
    {
        var iterator = ctx.CreateIterator?.Invoke(ctx.Tid);
        var count = 0;
        object? actorObj = ctx.Tid == 0 ? ctx.WhoFor : iterator?.Invoke();
        while (actorObj != null)
        {
            var next = ctx.Tid == 0 ? null : iterator?.Invoke();
            var actor = ctx.AsDamageActor?.Invoke(actorObj);
            if (actor != null && (actor.Flags & MfShootable) != 0)
            {
                if (ctx.Amount > 0)
                    ctx.DamageMobj?.Invoke(actor, actorObj, ctx.WhoFor, ctx.Amount, ctx.DamageType);
                else if (actor.Health < actor.SpawnHealth)
                {
                    actor.Health -= ctx.Amount;
                    if (actor.Health > actor.SpawnHealth)
                        actor.Health = actor.SpawnHealth;
                    if (actor.HasPlayer)
                        actor.PlayerHealth = actor.Health;
                }
                count++;
            }
            actorObj = next;
        }
        return count;
    }
}
