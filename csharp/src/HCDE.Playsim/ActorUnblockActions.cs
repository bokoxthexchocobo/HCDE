namespace HCDE.Playsim;

public static class ActorUnblockActions
{
    public static void Fall(Actor actor) => NoBlocking(actor);

    public static void ScreamAndUnblock(Actor actor)
    {
        ActorSoundActions.Scream(actor);
        NoBlocking(actor);
    }

    public static void ActiveAndUnblock(Actor actor)
    {
        ActorSoundActions.ActiveSound(actor);
        NoBlocking(actor);
    }

    /// <summary>Native A_NoBlocking/A_Fall for represented actors and vanilla death drops.</summary>
    public static void NoBlocking(Actor actor, bool drop = true)
    {
        actor.Solid = false;
        if (!drop || actor is PlayerPawn) return;
        ActorDropItem.DropVanillaDeathItem(actor.Simulation, actor);
    }
}
