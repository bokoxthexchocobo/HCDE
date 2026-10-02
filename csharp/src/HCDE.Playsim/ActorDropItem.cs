namespace HCDE.Playsim;

/// <summary>Native <c>P_DropItem</c> subset for ACS <c>DropItem</c>.</summary>
internal static class ActorDropItem
{
    internal static void DropVanillaDeathItem(AuthoritySimulation sim, Actor actor)
    {
        if (actor is PlayerPawn) return;
        var item = actor.DoomEdNum switch
        {
            3004 or 84 => PickupCatalog.Clip,
            9 => PickupCatalog.Shotgun,
            65 => PickupCatalog.Chaingun,
            _ => 0,
        };
        if (item != 0) TryDropOne(sim, actor, item, 0, 256);
    }

    public static int Drop(AuthoritySimulation sim, int tid, Actor? activator, string? typeName, int amount, int chance)
    {
        if (!PickupCatalog.TryEditorNumberForDropName(typeName, out var doomEdNum))
            return 0;

        var count = 0;
        if (tid == 0)
        {
            if (activator is { Destroyed: false } && TryDropOne(sim, activator, doomEdNum, amount, chance))
                count++;
        }
        else
        {
            foreach (var actor in AcsActorTid.AllFromTid(sim, tid).ToArray())
            {
                if (TryDropOne(sim, actor, doomEdNum, amount, chance))
                    count++;
            }
        }

        return count;
    }

    private static bool TryDropOne(AuthoritySimulation sim, Actor dropper, int doomEdNum, int amount, int chance)
    {
        if ((sim.NextCombatRandom() & 255) > chance)
            return false;
        return sim.SpawnDroppedPickup(dropper, doomEdNum, pickupAmount: amount > 0 ? amount : 0);
    }
}
