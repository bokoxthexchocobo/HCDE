using HCDE.Gamedata;

namespace HCDE.Playsim;

/// <summary>Managed class/count/geometry subset of P_Thing_CheckProximity.</summary>
public static class ActorProximity
{
    public static bool Check(Actor actor, string? className, double distance, int count = 1,
        int flags = 0, int pointerSelector = 0)
        => Evaluate(actor, className, distance, count, flags, pointerSelector, counting: false) != 0;

    public static int CountProximity(Actor actor, string? className, double distance, int flags = 0,
        int pointerSelector = 0)
        => Evaluate(actor, className, distance, 0, flags, pointerSelector, counting: true);

    private static int Evaluate(Actor actor, string? className, double distance, int count,
        int flags, int pointerSelector, bool counting)
    {
        const int ancestor = 1, lessOrEqual = 2, noZ = 4, countDead = 8, deadOnly = 16, exact = 32,
            setTarget = 64, farthest = 512, closest = 1024, setOnPointer = 2048, checkSight = 4096;
        const int supported = ancestor | lessOrEqual | noZ | countDead | deadOnly | exact | setTarget
            | farthest | closest | setOnPointer | checkSight;
        if ((flags & ~supported) != 0)
            throw new NotSupportedException("Proximity master/tracer-changing flags are not implemented.");
        var sim = actor.Simulation
            ?? throw new InvalidOperationException("Actor proximity checks require a simulation.");
        var reference = AcsActorPointer.Resolve(sim, actor, pointerSelector);
        if (reference is null || reference.Destroyed || string.IsNullOrEmpty(className) || distance <= 0)
            return 0;
        var actorClass = className.Equals("Actor", StringComparison.OrdinalIgnoreCase);
        var playerClass = className.Equals("PlayerPawn", StringComparison.OrdinalIgnoreCase);
        if (!actorClass && !playerClass && !className.Equals("DoomPlayer", StringComparison.OrdinalIgnoreCase)
            && !DoomActorCatalog.TryEditorNumberForClassName(className, out _)) return 0;
        var changesTarget = (flags & setTarget) != 0;
        var targetOwner = (flags & setOnPointer) != 0 ? reference : actor;
        if (changesTarget && targetOwner.Brain is null)
            throw new NotSupportedException("Proximity target mutation requires a managed monster brain.");
        var distancePreference = (flags & (closest | farthest)) != 0;
        Actor? selected = null;
        var nearer = distance; var farther = 0.0;
        var found = 0; var result = 0;
        foreach (var other in sim.Actors)
        {
            if (other.Destroyed || ReferenceEquals(other, reference)) continue;
            var inherits = (flags & ancestor) != 0;
            var matches = inherits && actorClass || other is PlayerPawn
                && (className.Equals("DoomPlayer", StringComparison.OrdinalIgnoreCase) || inherits && playerClass)
                || other is not PlayerPawn && (inherits
                    ? DoomActorCatalog.IsKindOfClassName(other.ClassDoomEdNum, className)
                    : DoomActorCatalog.MatchesClassName(other.ClassDoomEdNum, className));
            if (!matches) continue;
            if (!ActorJumpActions.CheckIfCloser(reference, other, distance, (flags & noZ) != 0)) continue;
            if ((flags & checkSight) != 0 && !CombatTrace.HasLineOfSight(sim, other, reference)) continue;
            if (other.Killed ? (flags & (countDead | deadOnly)) == 0 : (flags & deadOnly) != 0) continue;
            if (changesTarget)
            {
                var dx = other.X.ToDouble() - reference.X.ToDouble();
                var dy = other.Y.ToDouble() - reference.Y.ToDouble();
                var horizontal = Math.Sqrt(dx * dx + dy * dy);
                if ((flags & closest) != 0 && horizontal < nearer)
                { selected = other; nearer = horizontal; }
                else if ((flags & farthest) != 0 && horizontal > farther)
                { selected = other; farther = horizontal; }
                else selected ??= other;
            }
            found++;
            if (!counting && found > count)
            {
                result = (flags & (lessOrEqual | exact)) == 0 ? 1 : 0;
                if (!(changesTarget && distancePreference)) break;
            }
        }
        if (changesTarget && selected is not null) targetOwner.Brain!.SetSpecialTarget(selected);
        if (counting) return found;
        if (found == count) return 1;
        if (found < count) return (flags & lessOrEqual) != 0 && (flags & exact) == 0 ? 1 : 0;
        return result;
    }
}
