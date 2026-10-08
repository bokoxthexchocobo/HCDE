namespace HCDE.Playsim;

public static class ActorUnblockActions
{
    /// <summary>Native A_NoBlocking/A_Fall for represented actors and vanilla death drops.</summary>
    public static void NoBlocking(Actor actor, bool drop = true)
    {
        actor.Solid = false;
        if (!drop || actor is PlayerPawn) return;
        ActorDropItem.DropVanillaDeathItem(actor.Simulation, actor);
    }
}
