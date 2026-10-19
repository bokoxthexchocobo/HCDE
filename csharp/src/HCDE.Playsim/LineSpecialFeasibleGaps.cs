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
            FloorWaggle => ExecuteFloorWaggle(sim, a0, a1),
            ThingSpawnFacing => ThingSpawnLineSpecials.ExecuteSpawnFacing(sim, a0, a1, a2 != 0, a3),
            SetGlobalFogParameter => true,
            SendToCommunicator => ExecuteCommunicator(sim, a0, a1),
            ChangeSkill => a0 is >= 0 and <= 5,
            ThingSetTranslation => ExecuteThingTranslation(sim, a0, a1),
            SectorSetWind => ExecuteSectorWind(sim, a0, a1, a2),
            _ => null,
        };
    }

    private static bool ExecuteFloorWaggle(AuthoritySimulation sim, int tag, int heightFixed)
    {
        if (tag == 0) return false;
        var delta = heightFixed / 65536.0;
        foreach (var sector in sim.Level.Sectors.Where(candidate => candidate.MatchesTag(tag)))
        {
            if ((uint)sector.Index < (uint)sim.Floors.Length)
                sim.Floors[sector.Index] += delta;
        }
        return true;
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

    private static bool ExecuteSectorWind(AuthoritySimulation sim, int tag, int amount, int angleByte)
    {
        if (tag == 0) return false;
        var radians = angleByte * (Math.PI * 2 / 256);
        var pushX = Math.Cos(radians) * amount / 8.0;
        var pushY = Math.Sin(radians) * amount / 8.0;
        foreach (var sector in sim.Level.Sectors.Where(candidate => candidate.MatchesTag(tag)))
        {
            sector.PushForceX = pushX;
            sector.PushForceY = pushY;
        }
        return true;
    }
}
