using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Line specials and textures from <c>t_func.cpp</c> (2040–2239).</summary>
public static partial class VmFScriptParser
{
    public static void SfLineTrigger(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1) || ctx.Simulation == null)
            return;
        var special = ctx.IntValue(ctx.TArgv[0]);
        var tag = ctx.TArgc > 1 ? ctx.IntValue(ctx.TArgv[1]) : 0;
        var activator = AsActor(ctx.Script?.Trigger) ?? ctx.Simulation.Actors.FirstOrDefault();
        if (activator == null)
            return;
        ActorSpecialActions.CallSpecial(activator, special, tag);
    }

    public static void SfSetLineBlocking(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2) || ctx.Simulation == null)
            return;
        var blocking = ctx.IntValue(ctx.TArgv[1]);
        if (blocking < 0 || blocking > 2)
            return;
        var mask = blocking switch
        {
            0 => 0,
            1 => LevelLine.BlockingFlag,
            _ => LevelLine.BlockEverythingFlag,
        };
        var tag = ctx.IntValue(ctx.TArgv[0]);
        foreach (var line in AcsLineActivation.LinesFromId(ctx.Simulation, tag))
            line.Flags = (line.Flags & ~(LevelLine.BlockingFlag | LevelLine.BlockEverythingFlag)) | mask;
    }

    public static void SfSetLineMonsterBlocking(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2) || ctx.Simulation == null)
            return;
        var blocking = ctx.IntValue(ctx.TArgv[1]) != 0 ? LevelLine.BlockMonstersFlag : 0;
        var tag = ctx.IntValue(ctx.TArgv[0]);
        foreach (var line in AcsLineActivation.LinesFromId(ctx.Simulation, tag))
            line.Flags = (line.Flags & ~LevelLine.BlockMonstersFlag) | blocking;
    }

    public static void SfSetLineTexture(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 4) || ctx.Simulation == null)
            return;
        var tag = ctx.IntValue(ctx.TArgv[0]);
        if (ctx.TArgv[3].Type == SvtString)
        {
            var side = ctx.IntValue(ctx.TArgv[1]);
            if (side < 0 || side > 1)
            {
                ctx.ScriptError("invalid side number for texture change\n");
                return;
            }
            var position = ctx.IntValue(ctx.TArgv[2]);
            if (position < 1 || position > 3)
            {
                ctx.ScriptError("invalid position for texture change\n");
                return;
            }
            var texture = ctx.StringValue(ctx.TArgv[3]);
            var tier = 3 - position;
            foreach (var line in AcsLineActivation.LinesFromId(ctx.Simulation, tag))
                ApplyLineSideTexture(ctx.Simulation, line, side, tier, texture);
            return;
        }
        var pic = ctx.StringValue(ctx.TArgv[1]);
        var legacySide = ctx.IntValue(ctx.TArgv[2]) != 0 ? 1 : 0;
        var sections = ctx.IntValue(ctx.TArgv[3]);
        foreach (var line in AcsLineActivation.LinesFromId(ctx.Simulation, tag))
        {
            if (line.SideFront < 0 || line.SideBack < 0 && legacySide != 0)
                continue;
            var sideIndex = legacySide == 0 ? line.SideFront : line.SideBack;
            if (sideIndex < 0)
                continue;
            var sidedef = ctx.Simulation.Level.Sides[sideIndex];
            if ((sections & 1) != 0)
                sidedef.TopTexture = pic;
            if ((sections & 2) != 0)
                sidedef.MidTexture = pic;
            if ((sections & 4) != 0)
                sidedef.BottomTexture = pic;
        }
    }

    static void ApplyLineSideTexture(AuthoritySimulation sim, LevelLine line, int sideChoice, int tier, string texture)
    {
        var sideIndex = sideChoice == 0 ? line.SideFront : line.SideBack;
        if (sideIndex < 0)
            return;
        var sidedef = sim.Level.Sides[sideIndex];
        switch (tier)
        {
            case 0:
                sidedef.TopTexture = texture;
                break;
            case 1:
                sidedef.MidTexture = texture;
                break;
            case 2:
                sidedef.BottomTexture = texture;
                break;
        }
    }
}
