using HCDE.Gamedata;

namespace HCDE.Playsim;

/// <summary>Native checks.zs jump actions for managed state tables.</summary>
public static class ActorJumpActions
{
    public static int? CheckBlock(Actor actor, int destination, int flags = 0, int pointerSelector = 0,
        double xOffset = 0, double yOffset = 0, double zOffset = 0, double angle = 0)
    {
        if ((flags & ~511) != 0)
            throw new NotSupportedException("Unknown block-check flags.");
        if (!double.IsFinite(xOffset) || !double.IsFinite(yOffset) || !double.IsFinite(zOffset) || !double.IsFinite(angle))
            throw new ArgumentOutOfRangeException(nameof(xOffset), "Block-check offsets must be finite.");
        var subject = ResolveSubject(actor, pointerSelector);
        if (subject is null || subject.Destroyed) return null;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Block checks require a simulation.");
        var oldX = subject.X; var oldY = subject.Y; var oldZ = subject.Z;
        var radians = (angle + ((flags & 256) != 0 ? 0 : actor.Angle.ToDegrees())) * Math.PI / 180;
        var x = (flags & 128) != 0 ? xOffset : oldX.ToDouble() + xOffset * Math.Cos(radians) + yOffset * Math.Sin(radians);
        var y = (flags & 128) != 0 ? yOffset : oldY.ToDouble() + xOffset * Math.Sin(radians) - yOffset * Math.Cos(radians);
        var z = (flags & 128) != 0 ? zOffset : oldZ.ToDouble() + zOffset;
        var probeX = Fixed.FromDouble(x); var probeY = Fixed.FromDouble(y); var probeZ = Fixed.FromDouble(z);
        try
        {
            subject.X = probeX; subject.Y = probeY; subject.Z = probeZ;
            var blocker = ActorPhysics.FindPositionBlocker(sim, subject, out var line, squareBounds: true);
            var blocked = blocker is not null && (flags & 64) == 0 || line && (flags & 1) == 0;
            if ((flags & 32) != 0)
            {
                blocked |= !ActorPhysics.FlatMoveProbeFits(sim, subject);
                if (!blocked) return null;
            }
            if (blocker is not null)
            {
                var setter = (flags & 16) != 0 ? subject : actor;
                if ((flags & 2) != 0) ActorPointerActions.RequireWritable(setter, AcsActorPointer.Target);
                if ((flags & 8) != 0) ActorPointerActions.RequireWritable(setter, AcsActorPointer.Tracer);
                // Native CheckBlock writes raw references without chain verification.
                if ((flags & 2) != 0) ActorPointerActions.Assign(sim, setter, AcsActorPointer.Target, blocker, 3);
                if ((flags & 4) != 0) ActorPointerActions.Assign(sim, setter, AcsActorPointer.Master, blocker, 3);
                if ((flags & 8) != 0) ActorPointerActions.Assign(sim, setter, AcsActorPointer.Tracer, blocker, 3);
            }
            return blocked ? destination : null;
        }
        finally { subject.X = oldX; subject.Y = oldY; subject.Z = oldZ; }
    }

    public static int? Jump(Actor actor, int chance, int destination)
    {
        if (chance >= 256) return destination;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Random jumps require a simulation.");
        return sim.NextJumpRandom() < chance ? destination : null;
    }

    public static int? JumpIf(bool expression, int destination) => expression ? destination : null;

    public static int? CheckProximity(Actor actor, int? destination, string className, double distance,
        int count = 1, int flags = 0, int pointerSelector = 0)
    {
        if (!destination.HasValue && (flags & (64 | 128 | 256)) == 0) return null;
        return ActorProximity.Check(actor, className, distance, count, flags, pointerSelector) ? destination : null;
    }

    public static int? JumpIfInTargetInventory(Actor actor, string itemType, int amount, int destination,
        int forwardPointer = 0)
    {
        var target = ResolveSubject(actor, AcsActorPointer.Target);
        return target is null ? null : JumpIfInventory(target, itemType, amount, destination, forwardPointer);
    }

    public static int? JumpIfInventory(Actor actor, string itemType, int amount, int destination, int pointerSelector = 0)
    {
        var subject = ResolveSubject(actor, pointerSelector);
        if (subject is null || subject.Destroyed || string.IsNullOrEmpty(itemType)
            || itemType.Equals("Health", StringComparison.OrdinalIgnoreCase)) return null;
        var count = AcsPlayerInventory.Count(subject, itemType, max: false);
        if (count <= 0) return null;
        var required = amount > 0 ? amount : AcsPlayerInventory.Count(subject, itemType, max: true);
        return count >= required ? destination : null;
    }

    public static bool CheckArmorType(Actor actor, string armorType, int amount = 1)
        => actor is PlayerPawn { Destroyed: false } player
            && player.Inventory.ArmorType.Equals(armorType, StringComparison.OrdinalIgnoreCase)
            && player.Inventory.Armor >= amount;

    public static int? JumpIfArmorType(Actor actor, string armorType, int destination, int amount = 1)
        => CheckArmorType(actor, armorType, amount) ? destination : null;

    public static int? CheckSpecies(Actor actor, int destination, string species = "None", int pointerSelector = 0)
    {
        var subject = ResolveSubject(actor, pointerSelector);
        if (subject is null || subject.Destroyed) return null;
        return subject.SpeciesName.Equals(species, StringComparison.OrdinalIgnoreCase) ? destination : null;
    }

    public static int? JumpIfHigherOrLower(Actor actor, int? high, int? low,
        double offsetHigh = 0, double offsetLow = 0, bool includeHeight = true, int pointerSelector = 2)
    {
        var subject = ResolveSubject(actor, pointerSelector);
        if (subject is null || ReferenceEquals(subject, actor)) return null;
        var z = actor.Z.ToDouble(); var subjectZ = subject.Z.ToDouble();
        if (high.HasValue && subjectZ > z + (includeHeight ? actor.Height.ToDouble() : 0) + offsetHigh)
            return high;
        if (low.HasValue && subjectZ + (includeHeight ? subject.Height.ToDouble() : 0) < z + offsetLow)
            return low;
        return null;
    }

    public static int? CheckFlag(Actor actor, string flagName, int destination, int pointerSelector = 0)
    {
        var subject = ResolveSubject(actor, pointerSelector);
        return subject is not null && AcsActorFlags.TryGet(subject, flagName, out var enabled) && enabled
            ? destination : null;
    }

    public static int? CheckFloor(Actor actor, int destination)
    {
        var sim = actor.Simulation
            ?? throw new InvalidOperationException("Actor floor checks require a simulation.");
        return actor.Z.ToDouble() <= sim.FloorOf(actor.SectorIndex) ? destination : null;
    }

    public static int? CheckRange(Actor actor, double distance, int destination, bool twoDimension = false)
        => CheckPlayerObservation(actor, destination, distance, twoDimension, checkSight: false);

    public static int? CheckSight(Actor actor, int destination)
        => CheckPlayerObservation(actor, destination, null, false, checkSight: true);

    public static int? CheckSightOrRange(Actor actor, double distance, int destination, bool twoDimension = false)
        => CheckPlayerObservation(actor, destination, distance, twoDimension, checkSight: true);

    private static int? CheckPlayerObservation(Actor actor, int destination, double? distance,
        bool twoDimension, bool checkSight)
    {
        var sim = actor.Simulation
            ?? throw new InvalidOperationException("Actor player observation checks require a simulation.");
        var rangeSquared = distance * distance;
        foreach (var player in sim.Players)
        {
            if (player.Destroyed) continue;
            var dx = actor.X.ToDouble() - player.X.ToDouble();
            var dy = actor.Y.ToDouble() - player.Y.ToDouble();
            var eyeZ = player.Z.ToDouble() + player.Height.ToDouble() * 0.5;
            var bottom = actor.Z.ToDouble();
            var top = bottom + actor.Height.ToDouble();
            var dz = twoDimension ? 0 : eyeZ > top ? top - eyeZ : eyeZ < bottom ? bottom - eyeZ : 0;
            if (rangeSquared.HasValue && dx * dx + dy * dy + dz * dz <= rangeSquared.Value) return null;
            if (checkSight && CombatTrace.HasLineOfSight(sim, player, actor)) return null;
        }
        return destination;
    }

    public static int? CheckCeiling(Actor actor, int destination)
    {
        var sim = actor.Simulation
            ?? throw new InvalidOperationException("Actor ceiling checks require a simulation.");
        var ceiling = actor.SectorIndex >= 0 ? sim.CeilingOf(actor.SectorIndex) : double.PositiveInfinity;
        return actor.Z.ToDouble() + actor.Height.ToDouble() >= ceiling ? destination : null;
    }

    public static int? JumpIfTargetInsideMeleeRange(Actor actor, int destination)
        => TargetInMeleeRange(actor) ? destination : null;

    public static int? JumpIfTargetInLOS(Actor actor, int destination, double fov = 0, int flags = 0,
        double maxDistance = 0, double closeDistance = 0)
    {
        const int projectile = 1, noSight = 2, closeNoFov = 4, closeNoSight = 8,
            closeNoJump = 16, deadNoJump = 32, checkMaster = 64, targetLos = 128,
            flipFov = 256, allyNoJump = 512, combatantOnly = 1024, checkTracer = 4096;
        var sim = actor.Simulation
            ?? throw new InvalidOperationException("Actor target sight checks require a simulation.");
        Actor? target;
        var checkSight = (flags & noSight) == 0;
        if (actor is PlayerPawn)
        {
            target = CombatTrace.FindTarget(sim, actor, HitscanCombat.HitscanRange);
            var viewFlags = flags & (targetLos | flipFov);
            if (viewFlags == 0 || viewFlags == (targetLos | flipFov)) fov = 0;
            if ((viewFlags & targetLos) == 0) checkSight = false;
        }
        else
        {
            target = (flags & checkMaster) != 0 ? ResolveSubject(actor, AcsActorPointer.Master)
                : (flags & checkTracer) != 0 || actor is ProjectileActor && (flags & projectile) != 0
                    ? actor is ProjectileActor missile
                        && (missile.Kind == ProjectileKind.RevenantTracer || (flags & checkTracer) != 0)
                            ? sim.Actors.FirstOrDefault(candidate => !candidate.Destroyed && candidate.Id == missile.TracerTargetId)
                            : null
                    : ResolveSubject(actor, AcsActorPointer.Target);
            if (target is not null && (flags & deadNoJump) != 0 && target.Health <= 0) return null;
        }
        if (target is null || (flags & combatantOnly) != 0 && target is not PlayerPawn && !target.IsMonster
            || (flags & allyNoJump) != 0 && actor.IsFriend(target)) return null;
        var distance = PositionDistance(actor, target);
        if (maxDistance != 0 && distance > maxDistance) return null;
        if (closeDistance != 0 && distance < closeDistance)
        {
            if ((flags & closeNoJump) != 0) return null;
            if ((flags & closeNoFov) != 0) fov = 0;
            if ((flags & closeNoSight) != 0) checkSight = false;
        }
        var viewport = (flags & targetLos) != 0 ? target : actor;
        var viewed = ReferenceEquals(viewport, actor) ? target : actor;
        if (checkSight && !CombatTrace.HasLineOfSight(sim, viewport, viewed)) return null;
        if ((flags & flipFov) != 0) (viewport, viewed) = (viewed, viewport);
        return InFieldOfView(viewport, viewed, fov) ? destination : null;
    }

    public static int? JumpIfInTargetLOS(Actor actor, int destination, double fov = 0, int flags = 0,
        double maxDistance = 0, double closeDistance = 0)
    {
        const int projectile = 1, noSight = 2, closeNoFov = 4, closeNoSight = 8,
            closeNoJump = 16, deadNoJump = 32, checkMaster = 64;
        var sim = actor.Simulation
            ?? throw new InvalidOperationException("Actor target sight checks require a simulation.");
        var target = (flags & checkMaster) != 0 ? ResolveSubject(actor, AcsActorPointer.Master)
            : actor is ProjectileActor missile && (flags & projectile) != 0
                ? missile.Kind == ProjectileKind.RevenantTracer
                    ? sim.Actors.FirstOrDefault(candidate => !candidate.Destroyed && candidate.Id == missile.TracerTargetId)
                    : null
                : ResolveSubject(actor, AcsActorPointer.Target);
        if (target is null || (flags & deadNoJump) != 0 && target.Health <= 0) return null;
        var distance = PositionDistance(actor, target);
        if (maxDistance != 0 && distance > maxDistance) return null;
        var checkSight = (flags & noSight) == 0;
        if (closeDistance != 0 && distance < closeDistance)
        {
            if ((flags & closeNoJump) != 0) return null;
            if ((flags & closeNoFov) != 0) fov = 0;
            if ((flags & closeNoSight) != 0) checkSight = false;
        }
        if (!InFieldOfView(target, actor, fov)) return null;
        return !checkSight || CombatTrace.HasLineOfSight(sim, target, actor) ? destination : null;
    }

    private static double PositionDistance(Actor actor, Actor target)
    {
        var dx = actor.X.ToDouble() - target.X.ToDouble();
        var dy = actor.Y.ToDouble() - target.Y.ToDouble();
        var dz = actor.Z.ToDouble() - target.Z.ToDouble();
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private static bool InFieldOfView(Actor viewport, Actor target, double fov)
    {
        if (!(fov > 0 && fov < 360)) return true;
        var angle = Math.Atan2(target.Y.ToDouble() - viewport.Y.ToDouble(),
            target.X.ToDouble() - viewport.X.ToDouble()) * 180 / Math.PI;
        return Math.Abs(Math.IEEERemainder(angle - viewport.Angle.ToDegrees(), 360)) <= fov * 0.5;
    }

    public static int? JumpIfTargetOutsideMeleeRange(Actor actor, int destination)
        => TargetInMeleeRange(actor) ? null : destination;

    public static bool CheckMeleeRange(Actor actor, double range = -1) => TargetInMeleeRange(actor, range);

    private static bool TargetInMeleeRange(Actor actor, double range = -1)
    {
        var sim = actor.Simulation
            ?? throw new InvalidOperationException("Actor target selection requires a simulation.");
        var target = AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Target);
        return target is not null && MonsterBrain.CheckMeleeRange(actor, target,
            CombatTrace.HasLineOfSight(sim, actor, target), range);
    }

    public static bool CheckIfCloser(Actor actor, Actor? target, double distance, bool noZ = false)
    {
        if (target is null) return false;
        var dx = actor.X.ToDouble() - target.X.ToDouble();
        var dy = actor.Y.ToDouble() - target.Y.ToDouble();
        if (!(Math.Sqrt(dx * dx + dy * dy) < distance)) return false;
        if (noZ) return true;
        var z = actor.Z.ToDouble(); var targetZ = target.Z.ToDouble();
        return z > targetZ
            ? z - targetZ - target.Height.ToDouble() < distance
            : targetZ - z - actor.Height.ToDouble() < distance;
    }

    public static int? JumpIfCloser(Actor actor, double distance, int destination, bool noZ = false)
    {
        var sim = actor.Simulation
            ?? throw new InvalidOperationException("Actor target selection requires a simulation.");
        var target = actor is PlayerPawn
            ? CombatTrace.FindTarget(sim, actor, HitscanCombat.HitscanRange)
            : AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Target);
        return CheckIfCloser(actor, target, distance, noZ) ? destination : null;
    }

    public static int? JumpIfMasterCloser(Actor actor, double distance, int destination, bool noZ = false)
        => CheckIfCloser(actor, ResolveSubject(actor, AcsActorPointer.Master), distance, noZ) ? destination : null;

    public static int? JumpIfTracerCloser(Actor actor, double distance, int destination, bool noZ = false)
        => CheckIfCloser(actor, ResolveSubject(actor, AcsActorPointer.Tracer), distance, noZ) ? destination : null;

    public static int? JumpIfHealthLower(Actor actor, int health, int destination, int pointerSelector = 0)
    {
        var subject = ResolveSubject(actor, pointerSelector);
        return subject is not null && subject.Health < health ? destination : null;
    }

    private static Actor? ResolveSubject(Actor actor, int pointerSelector)
    {
        if (pointerSelector == AcsActorPointer.Default) return actor;
        if (pointerSelector == AcsActorPointer.Null) return null;
        var sim = actor.Simulation
            ?? throw new InvalidOperationException("Actor pointer selection requires a simulation.");
        return AcsActorPointer.Resolve(sim, actor, pointerSelector);
    }
}
