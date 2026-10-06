namespace HCDE.Playsim;

internal static class ActorMovementSave
{
    internal static int? Capture(Actor actor)
    {
        var flags = (actor.Solid ? 1 : 0) | (actor.Floating ? 2 : 0) | (actor.NoGravity ? 4 : 0);
        var movementChanged = actor.ResurrectionMovementFlags is { } movement
            && ((actor.NoGravity ? 1 : 0) | (actor.Floating ? 2 : 0)) != (movement & 3);
        var solidChanged = actor.ResurrectionCollisionFlags is { } collision
            && (actor.Solid ? 1 : 0) != (collision & 1);
        return actor.HasMovementActionOverride || movementChanged || solidChanged ? flags : null;
    }
}
