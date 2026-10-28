namespace HCDE.Playsim;

/// <summary>Non-poly line specials that only need lightweight managed parity.</summary>
internal static class LineSpecialFeasibleGaps
{
    internal const int FloorWaggle = 138;
    internal const int ThingSpawnFacing = 139;
    internal const int SetGlobalFogParameter = 157;
    internal const int SendToCommunicator = 174;
    internal const int ChangeSkill = 179;
    internal const int ThingSetTranslation = 180;
    internal const int SectorSetWind = 218;

    internal static bool? Execute(AuthoritySimulation sim, int special, Actor? activator,
        int a0, int a1, int a2, int a3, int a4)
    {
        return special switch
        {
            FloorWaggle => WaggleActions.Start(sim, null, a0, a1, a2, a3, a4, ceiling: false),
            ThingSpawnFacing => ThingSpawnLineSpecials.ExecuteSpawnFacing(sim, a0, a1, a2 != 0, a3),
            SetGlobalFogParameter => true,
            SendToCommunicator => ExecuteCommunicator(sim, a0, a1),
            ChangeSkill => a0 is >= 0 and <= 5,
            ThingSetTranslation => ExecuteThingTranslation(sim, a0, a1),
            SectorSetWind => a3 == 0 && PusherActions.AdjustWind(sim, a0, a1, a2),
            _ => null,
        };
    }

    private static bool ExecuteCommunicator(AuthoritySimulation sim, int tid, int message)
    {
        var changed = false;
        foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
        {
            actor.SpecialArgs[0] = message;
            actor.SpecialChanged = true;
            changed = true;
        }
        return changed;
    }

    private static bool ExecuteThingTranslation(AuthoritySimulation sim, int tid, int translation)
    {
        var changed = false;
        foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
        {
            actor.SpecialArgs[1] = translation;
            actor.SpecialChanged = true;
            changed = true;
        }
        return changed;
    }

}
