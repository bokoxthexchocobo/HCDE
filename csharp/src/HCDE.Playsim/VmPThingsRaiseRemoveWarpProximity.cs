using HCDE.Gamedata;

namespace HCDE.Playsim;

/// <summary>
/// <c>P_RemoveThing</c>, raise/velocity/proximity/warp from <c>p_things.cpp</c> (422–535, 566–811, 818–835).
/// </summary>
public static class VmPThingsRaiseRemoveWarpProximity
{
    public const int MfSolid = 1;
    public const int MfShootable = 1 << 1;
    public const int MfUnmorphed = 1 << 2;
    public const int Mf6Killed = 1;
    public const int Mf2Floatbob = 1 << 19;
    public const int RfTransferfriendliness = 1;
    public const int RfNocheckpositionRaise = 2;

    public const int CpxfAncestor = 1;
    public const int CpxfLessorequal = 1 << 1;
    public const int CpxfNoz = 1 << 2;
    public const int CpxfCountdead = 1 << 3;
    public const int CpxfDeadonly = 1 << 4;
    public const int CpxfExact = 1 << 5;
    public const int CpxfSettarget = 1 << 6;
    public const int CpxfSetmaster = 1 << 7;
    public const int CpxfSettracer = 1 << 8;
    public const int CpxfFarthest = 1 << 9;
    public const int CpxfClosest = 1 << 10;
    public const int CpxfSetonptr = 1 << 11;
    public const int CpxfChecksight = 1 << 12;

    public const int WarpfAbsoluteoffset = 0x1;
    public const int WarpfAbsoluteangle = 0x2;
    public const int WarpfUsecallerangle = 0x4;
    public const int WarpfNocheckposition = 0x8;
    public const int WarpfInterpolate = 0x10;
    public const int WarpfWarpinterpolation = 0x20;
    public const int WarpfCopyinterpolation = 0x40;
    public const int WarpfStop = 0x80;
    public const int WarpfTofloor = 0x100;
    public const int WarpfTestonly = 0x200;
    public const int WarpfAbsoluteposition = 0x400;
    public const int WarpfBob = 0x800;
    public const int WarpfMoveptr = 0x1000;
    public const int WarpfCopyvelocity = 0x4000;
    public const int WarpfCopypitch = 0x8000;
    public const double NullAngleDegrees = 1e37;

    public sealed class RemoveThingActor
    {
        public bool HasPlayer { get; set; }
        public bool PlayerIsMo { get; set; }
        public bool IsMapActor { get; set; } = true;
        public bool Removed { get; set; }
    }

    public static void P_RemoveThing(
        RemoveThingActor actor,
        Action? clearCounters = null,
        Action? destroy = null)
    {
        if (!actor.HasPlayer || !actor.PlayerIsMo)
        {
            if (!actor.IsMapActor)
                return;
            clearCounters?.Invoke();
            destroy?.Invoke();
            actor.Removed = true;
        }
    }

    public sealed class RaiseActor
    {
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public int Flags { get; set; }
        public object? DefaultInfo { get; set; }
        public object? RaiseState { get; set; }
        public bool Raised { get; set; }
    }

    public sealed class RaiseContext
    {
        public required RaiseActor Thing { get; init; }
        public object? Raiser { get; init; }
        public int Flags { get; init; }
        public Func<RaiseActor, object?>? GetRaiseState { get; init; }
        public Func<object?, (double Height, double Radius)>? DefaultDimensions { get; init; }
        public Func<RaiseActor, bool>? CheckPosition { get; init; }
        public Func<object?, RaiseActor, bool>? CanResurrect { get; init; }
        public Action<RaiseActor>? PlayRaiseSound { get; init; }
        public Action<RaiseActor>? Revive { get; init; }
        public Action<RaiseActor, object?, bool>? CopyFriendliness { get; init; }
        public Action<RaiseActor, object?>? SetState { get; init; }
    }

    public static bool P_Thing_Raise(RaiseContext ctx)
    {
        var thing = ctx.Thing;
        if (ctx.GetRaiseState?.Invoke(thing) == null)
            return false;

        var info = thing.DefaultInfo;
        thing.VelX = thing.VelY = 0;
        var oldheight = thing.Height;
        var oldradius = thing.Radius;
        var oldflags = thing.Flags;

        thing.Flags |= MfSolid;
        if (info != null)
        {
            var dims = ctx.DefaultDimensions?.Invoke(info) ?? (thing.Height, thing.Radius);
            thing.Height = dims.Height;
            thing.Radius = dims.Radius;
        }

        if ((ctx.Flags & RfNocheckpositionRaise) == 0 && ctx.CheckPosition?.Invoke(thing) != true)
        {
            thing.Flags = oldflags;
            thing.Radius = oldradius;
            thing.Height = oldheight;
            return false;
        }

        if (ctx.CanResurrect?.Invoke(ctx.Raiser, thing) != true)
            return false;

        ctx.PlayRaiseSound?.Invoke(thing);
        ctx.Revive?.Invoke(thing);

        if ((ctx.Flags & RfTransferfriendliness) != 0 && ctx.Raiser != null)
            ctx.CopyFriendliness?.Invoke(thing, ctx.Raiser, false);

        var raiseState = ctx.GetRaiseState?.Invoke(thing);
        ctx.SetState?.Invoke(thing, raiseState);
        thing.Raised = true;
        return true;
    }

    public static bool P_Thing_CanRaise(RaiseContext ctx)
    {
        var thing = ctx.Thing;
        if (ctx.GetRaiseState?.Invoke(thing) == null)
            return false;

        var info = thing.DefaultInfo;
        var oldflags = thing.Flags;
        var oldheight = thing.Height;
        var oldradius = thing.Radius;

        thing.Flags |= MfSolid;
        if (info != null)
        {
            var dims = ctx.DefaultDimensions?.Invoke(info) ?? (thing.Height, thing.Radius);
            thing.Height = dims.Height;
            thing.Radius = dims.Radius;
        }

        var check = ctx.CheckPosition?.Invoke(thing) == true;

        thing.Flags = oldflags;
        thing.Radius = oldradius;
        thing.Height = oldheight;
        return check;
    }

    public sealed class VelocityActor
    {
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double VelZ { get; set; }
        public bool HasPlayer { get; set; }
        public double PlayerVelX { get; set; }
        public double PlayerVelY { get; set; }
    }

    public static void P_Thing_SetVelocity(
        VelocityActor? actor,
        VmPThingsSpawnMoveIntercept.Vec3 vec,
        bool add,
        bool setbob)
    {
        if (actor == null)
            return;
        if (!add)
        {
            actor.VelX = actor.VelY = actor.VelZ = 0;
            if (actor.HasPlayer)
                actor.PlayerVelX = actor.PlayerVelY = 0;
        }
        actor.VelX += vec.X;
        actor.VelY += vec.Y;
        actor.VelZ += vec.Z;
        if (setbob && actor.HasPlayer)
        {
            actor.PlayerVelX += vec.X;
            actor.PlayerVelY += vec.Y;
        }
    }

    private static readonly HashSet<string> ProximityProjectileClasses = Enum.GetValues<ProjectileKind>()
        .Select(ProjectileActor.NativeClassNameOf).ToHashSet(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> ProximityPickupParents = new(StringComparer.OrdinalIgnoreCase)
    {
        "Inventory", "Ammo", "Weapon", "DoomWeapon", "StateProvider", "Health", "HealthPickup",
        "Armor", "BasicArmorBonus", "BasicArmorPickup", "Key", "DoomKey", "BackpackItem",
        "CustomInventory", "PowerupGiver",
    };

    /// <summary>ACS/action proximity check (`P_Thing_CheckProximity`, counting false).</summary>
    public static bool Check(Actor actor, string? className, double distance, int count = 1,
        int flags = 0, int pointerSelector = 0) =>
        P_Thing_CheckProximity(actor, className, distance, count, flags, pointerSelector, counting: false) != 0;

    /// <summary>Counting variant of <c>P_Thing_CheckProximity</c>.</summary>
    public static int CountProximity(Actor actor, string? className, double distance, int flags = 0,
        int pointerSelector = 0) =>
        P_Thing_CheckProximity(actor, className, distance, count: 0, flags, pointerSelector, counting: true);

    /// <summary>Faithful port of <c>P_Thing_CheckProximity</c> (<c>p_things.cpp</c> 566–690).</summary>
    public static int P_Thing_CheckProximity(
        Actor self,
        string? className,
        double distance,
        int count,
        int flags,
        int pointerSelector,
        bool counting)
    {
        const int supported = CpxfAncestor | CpxfLessorequal | CpxfNoz | CpxfCountdead | CpxfDeadonly | CpxfExact
            | CpxfSettarget | CpxfSetmaster | CpxfSettracer | CpxfFarthest | CpxfClosest | CpxfSetonptr | CpxfChecksight;
        if ((flags & ~supported) != 0)
            throw new NotSupportedException("Unsupported proximity flags.");

        var sim = self.Simulation
            ?? throw new InvalidOperationException("Actor proximity checks require a simulation.");
        var reference = AcsActorPointer.Resolve(sim, self, pointerSelector);
        if (reference is null || reference.Destroyed || string.IsNullOrEmpty(className) || distance <= 0)
            return 0;

        var actorClass = className.Equals("Actor", StringComparison.OrdinalIgnoreCase);
        var playerClass = className.Equals("PlayerPawn", StringComparison.OrdinalIgnoreCase);
        if (!actorClass && !playerClass && !className.Equals("DoomPlayer", StringComparison.OrdinalIgnoreCase)
            && !DoomActorCatalog.TryEditorNumberForClassName(className, out _)
            && !PickupCatalog.TryEditorNumberForDropName(className, out _, sim.StrifeHealthPickups, sim.RavenHealthPickups)
            && !DoomDecorationDefaults.IsKnownClassName(className)
            && !ProximityProjectileClasses.Contains(className)
            && !ProximityPickupParents.Contains(className)
            && !className.Equals("BulletPuff", StringComparison.OrdinalIgnoreCase)
            && !className.Equals("IceChunk", StringComparison.OrdinalIgnoreCase))
            return 0;

        var refX = reference.X.ToDouble();
        var refY = reference.Y.ToDouble();
        var refZ = reference.Z.ToDouble();
        var refTop = refZ + reference.Height.ToDouble();

        var ptrWillChange = (flags & (CpxfSettarget | CpxfSetmaster | CpxfSettracer)) != 0;
        var ptrDistPref = (flags & (CpxfClosest | CpxfFarthest)) != 0;
        var writeActor = (flags & CpxfSetonptr) != 0 ? reference : self;

        var counter = 0;
        var result = 0;
        var closer = distance;
        var farther = 0.0;
        Actor? dist = null;

        foreach (var mo in sim.Actors)
        {
            if (mo.Destroyed || ReferenceEquals(mo, reference))
                continue;

            if ((flags & CpxfAncestor) != 0)
            {
                if (!(actorClass || mo is PlayerPawn && playerClass
                        || AcsCallFunctions.ClassNameOf(mo).Equals(className, StringComparison.OrdinalIgnoreCase)
                        || DoomActorCatalog.IsKindOfClassName(mo.ClassDoomEdNum, className)
                        || DoomDecorationDefaults.IsKindOfClassName(mo.ClassDoomEdNum, className)
                        || PickupCatalog.IsKindOfClassName(mo.ClassDoomEdNum, className, sim.StrifeHealthPickups, sim.RavenHealthPickups)
                        || mo is ProjectileActor { Kind: ProjectileKind.CyberRocket }
                            && className.Equals("Rocket", StringComparison.OrdinalIgnoreCase)))
                    continue;
            }
            else if (!AcsCallFunctions.ClassNameOf(mo).Equals(className, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!mo.IsMapActor())
                continue;

            var moX = mo.X.ToDouble();
            var moY = mo.Y.ToDouble();
            var moZ = mo.Z.ToDouble();
            var moTop = moZ + mo.Height.ToDouble();
            if (!ProximityInRange(refX, refY, refZ, refTop, moX, moY, moZ, moTop, distance, (flags & CpxfNoz) != 0))
                continue;

            if ((flags & CpxfChecksight) != 0
                && !CombatTrace.CheckSight(sim, mo, reference, ActorSightFlags.IgnoreVisibility))
                continue;

            if (mo.Killed)
            {
                if ((flags & (CpxfCountdead | CpxfDeadonly)) == 0)
                    continue;
            }
            else if ((flags & CpxfDeadonly) != 0)
                continue;

            if (ptrWillChange)
            {
                var dx = moX - refX;
                var dy = moY - refY;
                var current = Math.Sqrt(dx * dx + dy * dy);
                if ((flags & CpxfClosest) != 0 && current < closer)
                {
                    dist = mo;
                    closer = current;
                }
                else if ((flags & CpxfFarthest) != 0 && current > farther)
                {
                    dist = mo;
                    farther = current;
                }
                else
                    dist ??= mo;
            }
            counter++;

            if (!counting && counter > count)
            {
                result = (flags & (CpxfLessorequal | CpxfExact)) == 0 ? 1 : 0;
                if (ptrWillChange && ptrDistPref)
                    continue;
                break;
            }
        }

        if (ptrWillChange && dist is not null)
        {
            if ((flags & CpxfSettarget) != 0)
                writeActor.SetTargetPointer(dist);
            if ((flags & CpxfSetmaster) != 0)
                writeActor.MasterId = dist.Id;
            if ((flags & CpxfSettracer) != 0)
                writeActor.TracerId = dist.Id;
        }

        if (!counting)
        {
            if (counter == count)
                return 1;
            if (counter < count)
                return (flags & CpxfLessorequal) != 0 && (flags & CpxfExact) == 0 ? 1 : 0;
            return result;
        }
        return counter;
    }

    public sealed class WarpActor
    {
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double PrevX { get; set; }
        public double PrevY { get; set; }
        public double PrevZ { get; set; }
        public int PrevPortalGroup { get; set; }
        public double YawDegrees { get; set; }
        public double PitchDegrees { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double VelZ { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double FloorZ { get; set; }
        public int Flags2 { get; set; }
        public int SectorPortalGroup { get; set; }
        public object? Sector { get; set; }
    }

    public sealed class WarpContext
    {
        public required WarpActor Caller { get; init; }
        public required WarpActor Reference { get; init; }
        public double Xofs { get; init; }
        public double Yofs { get; init; }
        public double Zofs { get; init; }
        public double AngleDegrees { get; init; }
        public int Flags { get; init; }
        public double Heightoffset { get; init; }
        public double Radiusoffset { get; init; }
        public double PitchDegrees { get; init; }
        public Func<int, int, VmPThingsSpawnMoveIntercept.Vec3>? DisplacementOffset { get; init; }
        public Func<WarpActor, bool>? TestMobjLocation { get; init; }
        public Action<WarpActor>? ClearInterpolation { get; init; }
        public Func<WarpActor, double, double, bool>? TryMove { get; init; }
        public Func<WarpActor, double>? BobOffset { get; init; }
    }

    public static bool P_Thing_Warp(WarpContext ctx)
    {
        var caller = ctx.Caller;
        var reference = ctx.Reference;
        if ((ctx.Flags & WarpfMoveptr) != 0)
            (caller, reference) = (reference, caller);

        var old = new VmPThingsSpawnMoveIntercept.Vec3(caller.PosX, caller.PosY, caller.PosZ);
        var oldpgroup = caller.SectorPortalGroup;

        var zofs = ctx.Zofs + reference.Height * ctx.Heightoffset;
        var angle = ctx.AngleDegrees;
        if ((ctx.Flags & WarpfAbsoluteangle) == 0)
            angle += (ctx.Flags & WarpfUsecallerangle) != 0 ? caller.YawDegrees : reference.YawDegrees;

        var rad = ctx.Radiusoffset * reference.Radius;
        var s = Math.Sin(angle * Math.PI / 180);
        var c = Math.Cos(angle * Math.PI / 180);
        var xofs = ctx.Xofs;
        var yofs = ctx.Yofs;

        if ((ctx.Flags & WarpfAbsoluteposition) == 0)
        {
            if ((ctx.Flags & WarpfAbsoluteoffset) == 0)
            {
                var xofs1 = xofs;
                xofs = xofs1 * c + yofs * s;
                yofs = xofs1 * s - yofs * c;
            }

            if ((ctx.Flags & WarpfTofloor) != 0)
            {
                SetOrigin(caller, Vec3Offset(reference, xofs + rad * c, yofs + rad * s, 0));
                caller.PosZ = caller.FloorZ + zofs;
            }
            else
                SetOrigin(caller, Vec3Offset(reference, xofs + rad * c, yofs + rad * s, zofs));
        }
        else
        {
            SetOrigin(caller, new VmPThingsSpawnMoveIntercept.Vec3(
                xofs + rad * c, yofs + rad * s, zofs));
            if ((ctx.Flags & WarpfTofloor) != 0)
                caller.PosZ = caller.FloorZ + zofs;
        }

        if ((ctx.Flags & WarpfNocheckposition) != 0 || ctx.TestMobjLocation?.Invoke(caller) == true)
        {
            if ((ctx.Flags & WarpfTestonly) != 0)
            {
                caller.PosX = old.X;
                caller.PosY = old.Y;
                caller.PosZ = old.Z;
            }
            else
            {
                caller.YawDegrees = angle;
                if ((ctx.Flags & WarpfCopypitch) != 0)
                    caller.PitchDegrees = reference.PitchDegrees;
                if (ctx.PitchDegrees != NullAngleDegrees)
                    caller.PitchDegrees += ctx.PitchDegrees;

                if ((ctx.Flags & WarpfCopyvelocity) != 0)
                {
                    caller.VelX = reference.VelX;
                    caller.VelY = reference.VelY;
                    caller.VelZ = reference.VelZ;
                }
                if ((ctx.Flags & WarpfStop) != 0)
                    caller.VelX = caller.VelY = caller.VelZ = 0;

                if ((ctx.Flags & WarpfWarpinterpolation) != 0)
                {
                    var displacedOld = old + (ctx.DisplacementOffset?.Invoke(oldpgroup, caller.SectorPortalGroup)
                        ?? default);
                    caller.PrevX += caller.PosX - displacedOld.X;
                    caller.PrevY += caller.PosY - displacedOld.Y;
                    caller.PrevZ += caller.PosZ - displacedOld.Z;
                    caller.PrevPortalGroup = caller.SectorPortalGroup;
                }
                else if ((ctx.Flags & WarpfCopyinterpolation) != 0)
                {
                    var displacedOld = old + (ctx.DisplacementOffset?.Invoke(reference.PrevPortalGroup, caller.SectorPortalGroup)
                        ?? default);
                    var displacedRef = old + (ctx.DisplacementOffset?.Invoke(reference.SectorPortalGroup, caller.SectorPortalGroup)
                        ?? default);
                    caller.PrevX = caller.PosX + displacedOld.X - displacedRef.X;
                    caller.PrevY = caller.PosY + displacedOld.Y - displacedRef.Y;
                    caller.PrevZ = caller.PosZ + displacedOld.Z - displacedRef.Z;
                    caller.PrevPortalGroup = caller.SectorPortalGroup;
                }
                else if ((ctx.Flags & WarpfInterpolate) == 0)
                    ctx.ClearInterpolation?.Invoke(caller);

                if ((ctx.Flags & WarpfBob) != 0 && (reference.Flags2 & Mf2Floatbob) != 0)
                    caller.PosZ += ctx.BobOffset?.Invoke(reference) ?? 0;

                ctx.TryMove?.Invoke(caller, caller.PosX, caller.PosY);
            }
            return true;
        }
        caller.PosX = old.X;
        caller.PosY = old.Y;
        caller.PosZ = old.Z;
        return false;
    }

    public static int WarpAction(object? self, object? destination, double xofs, double yofs, double zofs,
        double angleDegrees, int flags, double heightoffset, double radiusoffset, double pitchDegrees,
        Func<object?, WarpActor>? asWarpActor,
        WarpContext? template)
    {
        if (destination == null || template == null || asWarpActor == null)
            return 0;
        var caller = asWarpActor(self);
        var reference = asWarpActor(destination);
        if (caller == null || reference == null)
            return 0;
        var ctx = new WarpContext
        {
            Caller = caller,
            Reference = reference,
            Xofs = xofs,
            Yofs = yofs,
            Zofs = zofs,
            AngleDegrees = angleDegrees,
            Flags = flags,
            Heightoffset = heightoffset,
            Radiusoffset = radiusoffset,
            PitchDegrees = pitchDegrees,
            DisplacementOffset = template.DisplacementOffset,
            TestMobjLocation = template.TestMobjLocation,
            ClearInterpolation = template.ClearInterpolation,
            TryMove = template.TryMove,
            BobOffset = template.BobOffset,
        };
        return P_Thing_Warp(ctx) ? 1 : 0;
    }

    static void SetOrigin(WarpActor actor, VmPThingsSpawnMoveIntercept.Vec3 pos)
    {
        actor.PosX = pos.X;
        actor.PosY = pos.Y;
        actor.PosZ = pos.Z;
    }

    static VmPThingsSpawnMoveIntercept.Vec3 Vec3Offset(WarpActor reference, double x, double y, double z) =>
        new(reference.PosX + x, reference.PosY + y, reference.PosZ + z);

    public static bool ProximityInRange(
        double refX, double refY, double refZ, double refTop,
        double moX, double moY, double moZ, double moTop,
        double distance,
        bool noZ)
    {
        var dx = refX - moX;
        var dy = refY - moY;
        if (Math.Sqrt(dx * dx + dy * dy) >= distance)
            return false;
        if (noZ)
            return true;
        return refZ > moZ
            ? refZ - moTop < distance
            : moZ - refTop < distance;
    }
}
