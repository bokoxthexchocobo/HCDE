namespace HCDE.Playsim;

public static class ActorSkullActions
{
    internal static void StopStationaryCharge(Actor actor)
    {
        actor.Brain!.StopCharge(actor);
        if (!actor.Dormant && actor.States.HasState(actor.SeeState))
            actor.States.Enter(actor, actor.SeeState);
        else
        {
            actor.SetIdle();
            if (actor.Dormant) actor.States.ForceRemainingTics(-1);
        }
    }

    internal static bool Slam(Actor actor, Actor victim)
    {
        if (actor.OnlySlamSolid && !victim.Solid && !victim.Shootable) return true;
        if (actor.Brain is { } brain) brain.StopCharge(actor);
        else actor.VelocityX = actor.VelocityY = actor.VelocityZ = default;
        if (actor.IsDead) return false;
        if (actor.Dormant)
        {
            actor.SetIdle();
            actor.States.ForceRemainingTics(-1);
            return false;
        }
        ActorDamage.Apply(victim, actor.GetMissileDamage(7, 1), actor, damageType: "Melee", inflictor: actor);
        if (actor.IsDead || actor.Destroyed) return false;
        if (actor.States.HasState(actor.SlamState)) actor.States.Enter(actor, actor.SlamState);
        else if (!actor.RetargetAfterSlam && actor.States.HasState(actor.SeeState)) actor.States.Enter(actor, actor.SeeState);
        else actor.SetIdle();
        return false;
    }

    public static void BetaSkullAttack(Actor actor)
    {
        if (actor.Destroyed) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Beta skull attack requires an attached simulation.");
        var target = AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Target);
        if (target == null || actor.SharesContactSpecies(target)) return;
        if (actor.ActorSounds.TryGetValue(ActorSoundType.Attack, out var sound) && !string.IsNullOrEmpty(sound))
            ActorSoundActions.PlayWeaponSound(actor, sound);
        ActorOrientationActions.Face(actor, target);
        ActorDamage.Apply(target, actor.GetMissileDamage(7, 1), actor, inflictor: actor);
    }

    public static void SkullAttack(Actor actor, double skullSpeed = 20)
    {
        if (actor.Destroyed) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Skull attack requires an attached simulation.");
        var target = AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Target);
        if (target == null) return;
        if (!double.IsFinite(skullSpeed)) throw new ArgumentOutOfRangeException(nameof(skullSpeed));
        var brain = actor.Brain ?? throw new NotSupportedException("Skull attack requires a managed monster brain.");
        brain.StartCharge(actor, target, skullSpeed);
    }
}
