using HCDE.Gamedata;

namespace HCDE.Playsim;

/// <summary>Managed class/count/geometry subset of P_Thing_CheckProximity.</summary>
public static class ActorProximity
{
    private static readonly HashSet<string> ProjectileClasses = Enum.GetValues<ProjectileKind>()
        .Select(ProjectileActor.NativeClassNameOf).ToHashSet(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> PickupParents = new(StringComparer.OrdinalIgnoreCase)
        { "Inventory", "Ammo", "Weapon", "DoomWeapon", "StateProvider", "Health", "HealthPickup",
            "Armor", "BasicArmorBonus", "BasicArmorPickup", "Key", "DoomKey", "BackpackItem",
            "CustomInventory", "PowerupGiver" };
    public static bool Check(Actor actor, string? className, double distance, int count = 1,
        int flags = 0, int pointerSelector = 0)
        => Evaluate(actor, className, distance, count, flags, pointerSelector, counting: false) != 0;

    public static int CountProximity(Actor actor, string? className, double distance, int flags = 0,
        int pointerSelector = 0)
        => Evaluate(actor, className, distance, 0, flags, pointerSelector, counting: true);

    private static int Evaluate(Actor actor, string? className, double distance, int count,
        int flags, int pointerSelector, bool counting)
    {
        const int ancestor = VmPThingsRaiseRemoveWarpProximity.CpxfAncestor;
        const int lessOrEqual = VmPThingsRaiseRemoveWarpProximity.CpxfLessorequal;
        const int noZ = VmPThingsRaiseRemoveWarpProximity.CpxfNoz;
        const int countDead = VmPThingsRaiseRemoveWarpProximity.CpxfCountdead;
        const int deadOnly = VmPThingsRaiseRemoveWarpProximity.CpxfDeadonly;
        const int exact = VmPThingsRaiseRemoveWarpProximity.CpxfExact;
        const int setTarget = VmPThingsRaiseRemoveWarpProximity.CpxfSettarget;
        const int setMaster = VmPThingsRaiseRemoveWarpProximity.CpxfSetmaster;
        const int setTracer = VmPThingsRaiseRemoveWarpProximity.CpxfSettracer;
        const int setOnPointer = VmPThingsRaiseRemoveWarpProximity.CpxfSetonptr;
        const int checkSight = VmPThingsRaiseRemoveWarpProximity.CpxfChecksight;
        const int supported = ancestor | lessOrEqual | noZ | countDead | deadOnly | exact | setTarget
            | setMaster | setTracer | VmPThingsRaiseRemoveWarpProximity.CpxfFarthest
            | VmPThingsRaiseRemoveWarpProximity.CpxfClosest | setOnPointer | checkSight;
        if ((flags & ~supported) != 0)
            throw new NotSupportedException("Unsupported proximity flags.");
        var sim = actor.Simulation
            ?? throw new InvalidOperationException("Actor proximity checks require a simulation.");
        var reference = AcsActorPointer.Resolve(sim, actor, pointerSelector);
        if (reference is null || reference.Destroyed || string.IsNullOrEmpty(className) || distance <= 0)
            return 0;
        var actorClass = className.Equals("Actor", StringComparison.OrdinalIgnoreCase);
        var playerClass = className.Equals("PlayerPawn", StringComparison.OrdinalIgnoreCase);
        if (!actorClass && !playerClass && !className.Equals("DoomPlayer", StringComparison.OrdinalIgnoreCase)
            && !DoomActorCatalog.TryEditorNumberForClassName(className, out _)
            && !PickupCatalog.TryEditorNumberForDropName(className, out _, sim.StrifeHealthPickups, sim.RavenHealthPickups)
            && !DoomDecorationDefaults.IsKnownClassName(className)
            && !ProjectileClasses.Contains(className)
            && !PickupParents.Contains(className)
            && !className.Equals("BulletPuff", StringComparison.OrdinalIgnoreCase)
            && !className.Equals("IceChunk", StringComparison.OrdinalIgnoreCase)) return 0;

        var refX = reference.X.ToDouble();
        var refY = reference.Y.ToDouble();
        var refZ = reference.Z.ToDouble();
        var refTop = refZ + reference.Height.ToDouble();
        var candidates = new List<VmPThingsRaiseRemoveWarpProximity.ProximityCandidate>();
        foreach (var other in sim.Actors)
        {
            if (other.Destroyed || ReferenceEquals(other, reference)) continue;
            var inherits = (flags & ancestor) != 0;
            var matches = AcsCallFunctions.ClassNameOf(other).Equals(className, StringComparison.OrdinalIgnoreCase)
                || inherits && (actorClass || other is PlayerPawn && playerClass
                    || DoomActorCatalog.IsKindOfClassName(other.ClassDoomEdNum, className)
                    || DoomDecorationDefaults.IsKindOfClassName(other.ClassDoomEdNum, className)
                    || PickupCatalog.IsKindOfClassName(other.ClassDoomEdNum, className, sim.StrifeHealthPickups, sim.RavenHealthPickups)
                    || other is ProjectileActor { Kind: ProjectileKind.CyberRocket }
                        && className.Equals("Rocket", StringComparison.OrdinalIgnoreCase));
            var moX = other.X.ToDouble();
            var moY = other.Y.ToDouble();
            var moZ = other.Z.ToDouble();
            var moTop = moZ + other.Height.ToDouble();
            var dx = moX - refX;
            var dy = moY - refY;
            var horizontal = Math.Sqrt(dx * dx + dy * dy);
            candidates.Add(new VmPThingsRaiseRemoveWarpProximity.ProximityCandidate
            {
                Actor = other,
                Unmorphed = false,
                IsMapActor = other.IsMapActor(),
                Killed = other.Killed,
                ExactClassMatch = AcsCallFunctions.ClassNameOf(other).Equals(className, StringComparison.OrdinalIgnoreCase),
                AncestorMatch = matches,
                Distance2D = horizontal,
                InRange = VmPThingsRaiseRemoveWarpProximity.ProximityInRange(
                    refX, refY, refZ, refTop, moX, moY, moZ, moTop, distance, (flags & noZ) != 0),
            });
        }

        var vmSelf = new VmPThingsRaiseRemoveWarpProximity.ProximityPointerActor();
        var vmRef = new VmPThingsRaiseRemoveWarpProximity.ProximityPointerActor();
        var result = VmPThingsRaiseRemoveWarpProximity.P_Thing_CheckProximity(
            new VmPThingsRaiseRemoveWarpProximity.CheckProximityContext
            {
                Self = vmSelf,
                Ref = vmRef,
                ClassValid = true,
                Distance = distance,
                Count = count,
                Flags = flags,
                Counting = counting,
                Candidates = candidates,
                CheckSight = mo => mo.Actor is Actor cand
                    && CombatTrace.CheckSight(sim, cand, reference, ActorSightFlags.IgnoreVisibility),
            });

        var pointerBox = (flags & setOnPointer) != 0 ? vmRef : vmSelf;
        var writeActor = (flags & setOnPointer) != 0 ? reference : actor;
        if ((flags & setTarget) != 0 && pointerBox.Target is Actor target)
            writeActor.SetTargetPointer(target);
        if ((flags & setMaster) != 0 && pointerBox.Master is Actor master)
            writeActor.MasterId = master.Id;
        if ((flags & setTracer) != 0 && pointerBox.Tracer is Actor tracer)
            writeActor.TracerId = tracer.Id;

        return result;
    }
}
