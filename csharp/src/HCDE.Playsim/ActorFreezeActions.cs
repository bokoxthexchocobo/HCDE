namespace HCDE.Playsim;

public static class ActorFreezeActions
{
    /// <summary>A_FreezeDeath timer and represented corpse flags. Rendering, audio,
    /// additional native flags and player effects remain absent. Height restoration
    /// uses represented spawn defaults when available.</summary>
    public static void FreezeDeath(Actor actor)
    {
        var sim = actor.Simulation ?? throw new InvalidOperationException("FreezeDeath requires an attached simulation.");
        actor.States.ForceRemainingTics(sim.NextFreezeDeathTics());
        actor.Solid = actor.Shootable = actor.IceCorpse = actor.Pushable = true;
        actor.CanSlide = true;
        actor.Height = actor.ResurrectionHeight ?? actor.Height;
        if (actor is not PlayerPawn && actor.IsMonster && actor.Special != 0)
        {
            ActorSpecialActions.CallSpecial(actor, actor.Special, actor.SpecialArgs[0], actor.SpecialArgs[1],
                actor.SpecialArgs[2], actor.SpecialArgs[3], actor.SpecialArgs[4]);
            actor.Special = 0;
        }
    }
}
