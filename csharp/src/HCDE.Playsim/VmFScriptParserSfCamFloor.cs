namespace HCDE.Playsim;

/// <summary>Sector sounds and floor height/move from <c>t_func.cpp</c> (1456–1586); camera in chunk **908** slice.</summary>
public static partial class VmFScriptParser
{
    static IEnumerable<int> SectorsForTag(ParserContext ctx, int tag)
    {
        if (ctx.FraggleThinker != null)
            return ctx.FraggleThinker.SectorsByTag(tag);
        if (ctx.Simulation == null)
            return Array.Empty<int>();
        var list = new List<int>();
        for (var i = 0; i < ctx.Simulation.Level.Sectors.Count; i++)
            if (ctx.Simulation.Level.Sectors[i].MatchesTag(tag))
                list.Add(i);
        return list;
    }

    static bool SectorFloorActive(AuthoritySimulation sim, int sectorIndex) =>
        sim.Motions.Any(m => m.SectorIndex == sectorIndex && !m.MovesCeiling && !m.Completed);

    public static void SfStartSound(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var mo = AsActor(ctx.ActorValue(ctx.TArgv[0]));
        if (mo == null)
            return;
        var name = ctx.StringValue(ctx.TArgv[1]);
        ActorSoundActions.StartSound(mo, name, channel: 0);
    }

    public static void SfStartSectorSound(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2) || ctx.Simulation == null)
            return;
        var tag = ctx.IntValue(ctx.TArgv[0]);
        var name = ctx.StringValue(ctx.TArgv[1]);
        var sim = ctx.Simulation;
        foreach (var sectorIndex in SectorsForTag(ctx, tag))
        {
            var floor = sim.FloorOf(sectorIndex);
            ActorSoundActions.StartSoundAt(sim, 0, 0, floor, name, channel: 0, volume: 1f, attenuation: 1f);
        }
    }

    public static void SfFloorHeight(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1) || ctx.Simulation == null)
            return;
        var sim = ctx.Simulation;
        var tag = ctx.IntValue(ctx.TArgv[0]);
        if (ctx.TArgc > 1)
        {
            var dest = ctx.FloatValue(ctx.TArgv[1]);
            var crush = ctx.TArgc >= 3 ? ctx.IntValue(ctx.TArgv[2]) : 0;
            var returnVal = 1.0;
            foreach (var sectorIndex in SectorsForTag(ctx, tag))
            {
                if (SectorFloorActive(sim, sectorIndex))
                    continue;
                var current = sim.FloorOf(sectorIndex);
                var crushDamage = crush != 0 ? 10 : -1;
                if (crush != 0 && dest < current && TryCrushLoweringFloor(sim, sectorIndex, dest, crushDamage))
                    returnVal = 0;
                else if (!SectorFloorActive(sim, sectorIndex))
                    sim.Floors[sectorIndex] = Math.Min(sim.Ceilings[sectorIndex], dest);
            }
            ctx.TReturn.Type = SvtFixed;
            ctx.TReturn.FixedValue = (long)(returnVal * 65536.0);
            return;
        }

        var sectors = SectorsForTag(ctx, tag).ToList();
        if (sectors.Count == 0)
        {
            ctx.ScriptError($"sector not found with tagnum {tag}\n");
            return;
        }
        ReturnFixed(ctx, sim.FloorOf(sectors[0]));
    }

    public static void SfMoveFloor(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2) || ctx.Simulation == null)
            return;
        var tag = ctx.IntValue(ctx.TArgv[0]);
        var dest = ctx.FloatValue(ctx.TArgv[1]);
        var speed = ctx.TArgc > 2 ? ctx.FloatValue(ctx.TArgv[2]) : 1.0;
        var crush = ctx.TArgc > 3 ? ctx.IntValue(ctx.TArgv[3]) : -1;
        var crushArg = crush >= 0 ? crush + 1 : 0;
        var speedArg = (int)Math.Clamp(speed * 8.0, 1, int.MaxValue / 8);
        LineSpecials.ExecuteFloorSpecial(ctx.Simulation, 279, tag, speedArg, (int)dest, crushArg, 0);
    }

    static bool TryCrushLoweringFloor(AuthoritySimulation sim, int sectorIndex, double dest, int crushDamage)
    {
        var actors = sim.Actors.Where(a => !a.Destroyed && a is not ProjectileActor
            && ActorPhysics.TouchesSector(sim, a, sectorIndex)).ToArray();
        foreach (var actor in actors)
        {
            var support = ActorPhysics.SectorSupportFloor(sim, actor);
            if (support + actor.Height.ToDouble() > dest + 0.01)
            {
                if (crushDamage > 0)
                {
                    actor.MarkMoverCrush(0);
                    ActorDamage.Apply(actor, crushDamage, damageType: "Crush");
                    ActorSoundActions.CrushPain(actor);
                }
                return true;
            }
        }
        return false;
    }

    static void ReturnFixed(ParserContext ctx, double value)
    {
        ctx.TReturn.Type = SvtFixed;
        ctx.TReturn.FixedValue = (long)(value * 65536.0);
    }
}
