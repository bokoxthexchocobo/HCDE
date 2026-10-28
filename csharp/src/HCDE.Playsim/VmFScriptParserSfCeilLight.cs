namespace HCDE.Playsim;

/// <summary>Ceiling height/move, light level, and floor texture from <c>t_func.cpp</c> (1587–1875).</summary>
public static partial class VmFScriptParser
{
    static bool SectorCeilingActive(AuthoritySimulation sim, int sectorIndex) =>
        sim.Motions.Any(m => m.SectorIndex == sectorIndex && m.MovesCeiling && !m.Completed);

    static double CenterCeilingHeight(AuthoritySimulation sim, int sectorIndex) =>
        sim.CeilingOf(sectorIndex);

    static int CeilingMoveDirection(double dest, double current) =>
        dest > current ? 1 : -1;

    static int CeilingCrushDamage(int crush) => crush != 0 ? 10 : -1;

    static void SetCeilingHeightReturnFixed(ParserContext ctx, double returnVal)
    {
        ctx.TReturn.Type = SvtFixed;
        ctx.TReturn.FixedValue = (long)(returnVal * 65536.0);
    }

    static int FindFirstTaggedSector(ParserContext ctx, int tag)
    {
        foreach (var sectorIndex in SectorsForTag(ctx, tag))
            return sectorIndex;
        return -1;
    }

    static void SetLightLevelOnTaggedSectors(AuthoritySimulation sim, ParserContext ctx, int tag, int level)
    {
        foreach (var sectorIndex in SectorsForTag(ctx, tag))
            sim.Lights[sectorIndex] = (short)Math.Clamp(level, short.MinValue, short.MaxValue);
    }

    static bool ApplyImmediateCeilingHeight(
        AuthoritySimulation sim,
        int sectorIndex,
        double dest,
        int crush,
        ref double returnVal)
    {
        if (SectorCeilingActive(sim, sectorIndex))
            return false;

        var current = CenterCeilingHeight(sim, sectorIndex);
        var crushDamage = CeilingCrushDamage(crush);
        if (crush != 0 && CeilingMoveDirection(dest, current) > 0
            && TryCrushRaisingCeiling(sim, sectorIndex, dest, crushDamage))
        {
            returnVal = 0;
            return true;
        }

        if (!SectorCeilingActive(sim, sectorIndex))
            sim.Ceilings[sectorIndex] = Math.Max(sim.Floors[sectorIndex], dest);
        return true;
    }

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
                ApplyImmediateCeilingHeight(sim, sectorIndex, dest, crush, ref returnVal);
            SetCeilingHeightReturnFixed(ctx, returnVal);
            return;
        }

        var firstSector = FindFirstTaggedSector(ctx, tag);
        if (firstSector < 0)
        {
            ctx.ScriptError($"sector not found with tagnum {tag}\n");
            return;
        }

        SetCeilingHeightReturnFixed(ctx, CenterCeilingHeight(sim, firstSector));
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
        var sim = ctx.Simulation;
        var tag = ctx.IntValue(ctx.TArgv[0]);
        var firstSector = FindFirstTaggedSector(ctx, tag);
        if (firstSector < 0)
            return;
        if (ctx.TArgc > 1)
        {
            var level = ctx.IntValue(ctx.TArgv[1]);
            SetLightLevelOnTaggedSectors(sim, ctx, tag, level);
        }

        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = sim.LightOf(firstSector);
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
        var firstSector = sectors[0];
        var sector = ctx.Simulation.Level.Sectors[firstSector];
        if (ctx.TArgc > 1)
        {
            var pic = ctx.StringValue(ctx.TArgv[1]);
            foreach (var sectorIndex in sectors)
                ctx.Simulation.Level.Sectors[sectorIndex].FloorPic = pic;
            sector = ctx.Simulation.Level.Sectors[firstSector];
        }

        ctx.TReturn.Type = SvtString;
        ctx.TReturn.String = sector.FloorPic ?? string.Empty;
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
