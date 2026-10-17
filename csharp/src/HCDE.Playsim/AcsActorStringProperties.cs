namespace HCDE.Playsim;

internal static class AcsActorStringProperties
{
    internal static bool IsSupported(int property) => property is >= 5 and <= 9 or 20 or 46 or 47;

    internal static void Set(AuthoritySimulation sim, Actor? activator, int tid, int property, string value)
    {
        var targets = tid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : AcsActorTid.AllFromTid(sim, tid);
        foreach (var actor in targets)
        {
            if (property == 20) ActorPropertyActions.SetSpecies(actor, value);
            else if (property == 47)
            {
                if (actor is PlayerPawn player) player.SoundClass = string.IsNullOrEmpty(value) ? null : value;
            }
            else if (property == 46)
                actor.DamageType = string.IsNullOrEmpty(value) || value.Equals("None", StringComparison.OrdinalIgnoreCase) ? null : value;
            else if (property is >= 5 and <= 9)
            {
                var sounds = new Dictionary<ActorSoundType, string>(actor.ActorSounds);
                sounds[(ActorSoundType)(property - 5)] = NormalizeSoundName(value);
                actor.ActorSounds = sounds;
            }
            else throw new ArgumentOutOfRangeException(nameof(property));
        }
    }

    internal static string? Get(AuthoritySimulation sim, Actor? activator, int tid, int property)
    {
        var actor = tid == 0 ? activator : AcsActorTid.SingleFromTid(sim, tid);
        if (actor is null || actor.Destroyed) return null;
        return property switch {
            20 => actor.SpeciesName,
            47 => actor is PlayerPawn { SoundClass: { Length: > 0 } soundClass } ? soundClass : "player",
            46 => string.IsNullOrEmpty(actor.DamageType) ? "None" : actor.DamageType,
            >= 5 and <= 9 => NormalizeSoundName(actor.ActorSounds.GetValueOrDefault((ActorSoundType)(property - 5)) ?? ""),
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
    }

    private static string NormalizeSoundName(string value) => value.Equals("None", StringComparison.OrdinalIgnoreCase) ? "" : value;
}
