namespace HCDE.Playsim;

internal static class ActorDefenseSave
{
    internal static SimDefenseProperties? Capture(Actor actor)
    {
        var flags = (actor.Shootable ? 1 : 0) | (actor.Invulnerable ? 2 : 0) | (actor.NonShootable ? 4 : 0);
        var shootableChanged = actor.ResurrectionCollisionFlags is { } collision
            && (actor.Shootable ? 2 : 0) != (collision & 2);
        var invulnerableChanged = actor.ResurrectionDefenseFlags is { } defense
            && (actor.Invulnerable ? 1 : 0) != (defense & 1);
        return actor.HasDefensePropertyOverride || actor.Mass != 100 || actor.NonShootable
            || shootableChanged || invulnerableChanged
            ? new(actor.Mass, flags) : null;
    }
}
