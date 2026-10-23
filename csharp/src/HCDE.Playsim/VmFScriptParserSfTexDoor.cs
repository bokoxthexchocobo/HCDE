namespace HCDE.Playsim;

/// <summary>Sector textures, doors, and native no-op colormap from <c>t_func.cpp</c> (1876–2038).</summary>
public static partial class VmFScriptParser
{
    static int FraggleDoorSpeedArg(int speed) => Math.Clamp(speed, 1, 127) * 2;

    static int FraggleDoorWaitTics(ParserContext ctx, int waitArgIndex) =>
        ctx.TArgc > waitArgIndex ? ctx.IntValue(ctx.TArgv[waitArgIndex]) * GameTicClock.TicRate / 100 : 0;

    static bool FraggleDoorTagAllowed(int tag) => tag != 0;

    public static void SfSectorColormap(ParserContext ctx)
    {
        _ = ctx;
    }

    public static void SfCeilingTexture(ParserContext ctx)
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
                ctx.Simulation.Level.Sectors[sectorIndex].CeilingPic = pic;
        }
        ctx.TReturn.Type = SvtString;
        ctx.TReturn.String = sector.CeilingPic;
    }

    public static void SfOpenDoor(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1) || ctx.Simulation == null)
            return;
        var sectag = ctx.IntValue(ctx.TArgv[0]);
        if (!FraggleDoorTagAllowed(sectag))
            return;
        var waitTics = FraggleDoorWaitTics(ctx, 1);
        var speedArg = FraggleDoorSpeedArg(ctx.TArgc > 2 ? ctx.IntValue(ctx.TArgv[2]) : 1);
        if (waitTics > 0)
            LineSpecials.ExecuteDoorSpecial(ctx.Simulation, 12, sectag, speedArg, waitTics, 0, null);
        else
            LineSpecials.ExecuteDoorSpecial(ctx.Simulation, 11, sectag, speedArg, 0, 0, null);
    }

    public static void SfCloseDoor(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1) || ctx.Simulation == null)
            return;
        var sectag = ctx.IntValue(ctx.TArgv[0]);
        if (!FraggleDoorTagAllowed(sectag))
            return;
        var speedArg = FraggleDoorSpeedArg(ctx.TArgc > 1 ? ctx.IntValue(ctx.TArgv[1]) : 1);
        LineSpecials.ExecuteDoorSpecial(ctx.Simulation, 10, sectag, speedArg, 0, 0, null);
    }
}
