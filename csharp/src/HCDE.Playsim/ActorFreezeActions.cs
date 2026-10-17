namespace HCDE.Playsim;

public static class ActorFreezeActions
{
    /// <summary>A_FreezeDeath timer, sound request and represented corpse flags/render state.
    /// Additional native flags remain absent. Height restoration
    /// uses represented spawn defaults when available.</summary>
    public static void FreezeDeath(Actor actor)
    {
        var sim = actor.Simulation ?? throw new InvalidOperationException("FreezeDeath requires an attached simulation.");
        actor.States.ForceRemainingTics(sim.NextFreezeDeathTics());
        actor.Solid = actor.Shootable = actor.IceCorpse = actor.Pushable = true;
        actor.NoBlood = true;
        actor.CanSlide = true;
        actor.CanPass = true;
        actor.TeleStomp = true;
        actor.Crashed = true;
        actor.RenderStyle = ActorRenderStyle.Normal;
        actor.Alpha = 1;
        if (actor.Stealth) actor.VisibilityDirection = 0;
        actor.Height = actor.ResurrectionHeight ?? actor.Height;
        if (actor is PlayerPawn player) { player.DamageCount = 0; player.PoisonCount = 0; player.BonusCount = 0; }
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, "misc/freeze", 4, 0, 1, false, 1, false), immediate: true);
        if (actor is not PlayerPawn && actor.IsMonster && actor.Special != 0)
        {
            ActorSpecialActions.CallSpecial(actor, actor.Special, actor.SpecialArgs[0], actor.SpecialArgs[1],
                actor.SpecialArgs[2], actor.SpecialArgs[3], actor.SpecialArgs[4]);
            actor.Special = 0;
        }
    }
}
