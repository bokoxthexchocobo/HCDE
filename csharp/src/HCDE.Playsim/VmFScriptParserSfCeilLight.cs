namespace HCDE.Playsim;

/// <summary>Ceiling height/move, light level, and floor texture from <c>t_func.cpp</c> (1587–1875).</summary>
public static partial class VmFScriptParser
{
    static bool SectorCeilingActive(AuthoritySimulation sim, int sectorIndex) =>
        sim.Motions.Any(m => m.SectorIndex == sectorIndex && m.MovesCeiling && !m.Completed);

    public static void SfCeilingHeight(ParserContext ctx)
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
                if (SectorCeilingActive(sim, sectorIndex))
                    continue;
                var current = sim.CeilingOf(sectorIndex);
                var crushDamage = crush != 0 ? 10 : -1;
                if (crush != 0 && dest > current && TryCrushRaisingCeiling(sim, sectorIndex, dest, crushDamage))
                    returnVal = 0;
                else if (!SectorCeilingActive(sim, sectorIndex))
                    sim.Ceilings[sectorIndex] = Math.Max(sim.Floors[sectorIndex], dest);
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
        ReturnFixed(ctx, sim.CeilingOf(sectors[0]));
    }

    public static void SfMoveCeiling(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2) || ctx.Simulation == null)
            return;
        var tag = ctx.IntValue(ctx.TArgv[0]);
        var dest = ctx.FloatValue(ctx.TArgv[1]);
        var speed = ctx.TArgc > 2 ? ctx.FloatValue(ctx.TArgv[2]) : 1.0;
        var crush = ctx.TArgc > 3 ? ctx.IntValue(ctx.TArgv[3]) : -1;
        var silent = ctx.TArgc > 4 ? ctx.IntValue(ctx.TArgv[4]) : 1;
        var crushArg = crush >= 0 ? crush : 0;
        var speedArg = (int)Math.Clamp(speed * 8.0, 1, int.MaxValue / 8);
        LineSpecials.ExecuteCeilingSpecial(ctx.Simulation, 280, tag, speedArg, (int)dest, crushArg, silent, null);
    }

    public static void SfLightLevel(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1) || ctx.Simulation == null)
            return;
        var tag = ctx.IntValue(ctx.TArgv[0]);
        var sectors = SectorsForTag(ctx, tag).ToList();
        if (sectors.Count == 0)
            return;
        if (ctx.TArgc > 1)
        {
            var level = ctx.IntValue(ctx.TArgv[1]);
            foreach (var sectorIndex in sectors)
                ctx.Simulation.Lights[sectorIndex] = (short)Math.Clamp(level, short.MinValue, short.MaxValue);
        }
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = ctx.Simulation.LightOf(sectors[0]);
    }

    public static void SfFloorTexture(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1) || ctx.Simulation == null)
            return;
        var tag = ctx.IntValue(ctx.TArgv[0]);
        var sectors = SectorsForTag(ctx, tag).ToList();
        if (sectors.Count == 0)
        {
            ctx.ScriptError($"sector not found with tagnum {tag}\n");
            return;
        }
        var sector = ctx.Simulation.Level.Sectors[sectors[0]];
        if (ctx.TArgc > 1)
        {
            var pic = ctx.StringValue(ctx.TArgv[1]);
            foreach (var sectorIndex in sectors)
                ctx.Simulation.Level.Sectors[sectorIndex].FloorPic = pic;
        }
        ctx.TReturn.Type = SvtString;
        ctx.TReturn.String = sector.FloorPic;
    }

    static bool TryCrushRaisingCeiling(AuthoritySimulation sim, int sectorIndex, double dest, int crushDamage)
    {
        var actors = sim.Actors.Where(a => !a.Destroyed && a is not ProjectileActor
            && ActorPhysics.TouchesSector(sim, a, sectorIndex)).ToArray();
        foreach (var actor in actors)
        {
            var head = actor.Z.ToDouble() + actor.Height.ToDouble();
            if (head > dest - 0.01)
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
}
