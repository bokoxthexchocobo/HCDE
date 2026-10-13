namespace HCDE.Playsim;

internal static class ActorMissileDamage
{
    internal static DamageResult Apply(Actor actor, Actor victim, bool ripper)
    {
        ArgumentNullException.ThrowIfNull(victim);
        var damage = ripper ? actor.GetMissileDamage(3, 2) : actor.GetMissileDamage(actor.StrifeDamage ? 3 : 7, 1);
        if (damage > 0 || actor.ForcePain || actor.CausePain)
        {
            if (ripper) PlayRipSound(actor);
            var source = actor is ProjectileActor projectile ? projectile.Owner
                : actor.Simulation?.Actors.FirstOrDefault(candidate => candidate.Id == actor.TargetId && !candidate.Destroyed);
            return ActorDamage.Apply(victim, damage, source, damageType: actor.DamageType, inflictor: actor);
        }
        victim.GiveBody((int)Math.Min(-(long)damage, int.MaxValue));
        return default;
    }

    private static void PlayRipSound(Actor actor)
    {
        if (!actor.ActorSounds.TryGetValue(ActorSoundType.Rip, out var sound) || string.IsNullOrEmpty(sound)
            || sound.Equals("None", StringComparison.OrdinalIgnoreCase)) return;
        if (!actor.Destroyed)
        {
            ActorSoundActions.StartSound(actor, sound, channel: 4, attenuation: 1.001f);
            return;
        }
        // Managed impact commits removal before callbacks; retain the native body sound at contact.
        var sim = actor.Simulation ?? throw new InvalidOperationException("Missile impact sounds require a simulation.");
        var position = actor.SoundPos();
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, sound, 4, 0, 1, false, 1.001f, false,
            DetachedOrigin: new SimulationSoundOrigin(position.X, position.Y, position.Z,
                ActorPhysics.SectorAt(sim.Level, actor.X.ToDouble(), actor.Y.ToDouble()))), immediate: true);
    }
}
