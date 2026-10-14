namespace HCDE.Playsim;

[Flags]
public enum AlertMonsterFlags { None = 0, TargetEmitter = 1, TargetNonPlayer = 2, EmitFromTarget = 4 }

public static class ActorNoiseActions
{
    public static void WeaponAlert(Actor actor) => SoundAlert(actor, actor);

    public static void SoundAlert(Actor actor, Actor? target, bool splash = false, double maxDistance = 0)
    {
        var sim = actor.Simulation ?? throw new InvalidOperationException("Sound alert requires an attached simulation.");
        SoundPropagation.Alert(sim, actor, target, maxDistance, splash);
    }

    public static void NoiseAlert(Actor actor)
    {
        var sim = actor.Simulation ?? throw new InvalidOperationException("Noise alert requires an attached simulation.");
        var target = sim.Actors.FirstOrDefault(candidate => candidate.Id == actor.TargetId && !candidate.Destroyed);
        if (target is not null) SoundPropagation.Alert(sim, actor, target);
    }

    public static void AlertMonsters(Actor actor, double maxDistance = 0, AlertMonsterFlags flags = AlertMonsterFlags.None)
    {
        var sim = actor.Simulation ?? throw new InvalidOperationException("Monster alert requires an attached simulation.");
        Actor? target = null;
        if (actor is PlayerPawn || (flags & AlertMonsterFlags.TargetEmitter) != 0) target = actor;
        else
        {
            var current = sim.Actors.FirstOrDefault(candidate => candidate.Id == actor.TargetId && !candidate.Destroyed);
            if (current is PlayerPawn || (flags & AlertMonsterFlags.TargetNonPlayer) != 0) target = current;
        }
        var emitter = (flags & AlertMonsterFlags.EmitFromTarget) != 0 ? target : actor;
        if (target is not null && emitter is not null) SoundPropagation.Alert(sim, emitter, target, maxDistance);
    }
}
