namespace HCDE.Playsim;

public static class ActorFreezeActions
{
    /// <summary>A_FreezeDeath timer and represented corpse flags. Rendering, audio,
    /// additional native flags, height defaults, and player/special effects remain absent.</summary>
    public static void FreezeDeath(Actor actor)
    {
        var sim = actor.Simulation ?? throw new InvalidOperationException("FreezeDeath requires an attached simulation.");
        actor.States.ForceRemainingTics(sim.NextFreezeDeathTics());
        actor.Solid = actor.Shootable = actor.IceCorpse = actor.Pushable = true;
    }
}
