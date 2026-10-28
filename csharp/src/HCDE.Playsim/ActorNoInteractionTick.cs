namespace HCDE.Playsim;

/// <summary>Native <c>AActor::Tick</c> <c>MF5_NOINTERACTION</c> minimal physics (p_mobj.cpp).</summary>
public static class ActorNoInteractionTick
{
    public static bool TryTickMinimalPhysics(AuthoritySimulation sim, Actor actor)
    {
        if (!actor.NoInteraction)
            return false;

        if (actor is PlayerPawn player && (player.Frozen || player.TotallyFrozen))
            return true;

        var moving = actor.VelocityX.Raw != 0 || actor.VelocityY.Raw != 0 || actor.VelocityZ.Raw != 0;
        if (moving || actor.NoBlockmap == false)
        {
            actor.NoBlockmap = true;
            actor.X = Fixed.FromDouble(actor.X.ToDouble() + actor.VelocityX.ToDouble());
            actor.Y = Fixed.FromDouble(actor.Y.ToDouble() + actor.VelocityY.ToDouble());
            actor.Z = Fixed.FromDouble(actor.Z.ToDouble() + actor.VelocityZ.ToDouble());
            ActorPhysics.FitToSector(sim, actor, carryFloor: false);
        }

        return true;
    }
}
