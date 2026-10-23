using HCDE.Gamedata;

namespace HCDE.Playsim;

/// <summary>Level/mobj query <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (3541–3698).</summary>
public static partial class VmFScriptParser
{
    public static void SfLevelNum(ParserContext ctx)
    {
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = ctx.Simulation?.Level.LevelNumber ?? 0;
    }

    public static void SfMobjRadius(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var mo = AsActor(ctx.ActorValue(ctx.TArgv[0]));
        if (ctx.TArgc > 1 && mo != null)
            mo.Radius = Fixed.FromDouble(ctx.FloatValue(ctx.TArgv[1]));
        SetReturnDouble(ctx, mo == null ? 0 : mo.Radius.ToDouble());
    }

    public static void SfMobjHeight(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var mo = AsActor(ctx.ActorValue(ctx.TArgv[0]));
        if (ctx.TArgc > 1 && mo != null)
            mo.Height = Fixed.FromDouble(ctx.FloatValue(ctx.TArgv[1]));
        SetReturnDouble(ctx, mo == null ? 0 : mo.Height.ToDouble());
    }

    public static void SfThingCount(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var pclass = GetMobjType(ctx, ctx.TArgv[0]);
        if (pclass == null)
            return;
        var sim = ctx.Simulation;
        if (sim == null)
            return;

        var countLivingOnly = ctx.TArgc >= 2 && ctx.IntValue(ctx.TArgv[1]) != 0
            && !IsInventoryClass(pclass);
        var count = CountThings(sim, pclass, countLivingOnly, actor => ctx.GetActorClass(actor));
        if (!TryReplacementClass(pclass, out var replacement) || ReferenceEquals(replacement, pclass))
        {
            ctx.TReturn.Type = SvtInt;
            ctx.TReturn.IntValue = count;
            return;
        }

        count += CountThings(sim, replacement, countLivingOnly, actor => ctx.GetActorClass(actor));
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = count;
    }

    public static void SfSetColor(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2) || ctx.Simulation == null)
            return;
        var tagnum = ctx.IntValue(ctx.TArgv[0]);
        if (!ctx.Simulation.Level.Sectors.Any(s => s.MatchesTag(tagnum)))
            return;
        if (!TryResolveSectorColor(ctx, out var color))
            return;

        foreach (var sector in ctx.Simulation.Level.Sectors)
        {
            if (sector.MatchesTag(tagnum))
                sector.ColormapLightColor = color;
        }
    }

    static bool TryResolveSectorColor(ParserContext ctx, out int color)
    {
        color = 0;
        if (ctx.TArgc > 1 && ctx.TArgc < 4)
        {
            color = ctx.IntValue(ctx.TArgv[1]);
            return true;
        }

        if (ctx.TArgc >= 4)
        {
            color = ctx.IntValue(ctx.TArgv[1])
                | (ctx.IntValue(ctx.TArgv[2]) << 8)
                | (ctx.IntValue(ctx.TArgv[3]) << 16);
            return true;
        }

        return false;
    }

    static readonly HashSet<string> InventoryClassNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Clip", "Shell", "Cell", "RocketAmmo",
        "BlueCard", "YellowCard", "RedCard", "BlueSkull", "YellowSkull", "RedSkull",
        "ArmorBonus", "SoulSphere", "Megasphere", "Backpack",
    };

    static bool IsInventoryClass(object pclass) =>
        pclass is string name && InventoryClassNames.Contains(name);

    static int CountThings(
        AuthoritySimulation sim,
        object pclass,
        bool livingOnly,
        Func<Actor, object?> getActorClass)
    {
        var count = 0;
        foreach (var actor in sim.Actors)
        {
            if (!ActorMatchesClass(actor, pclass, getActorClass))
                continue;
            if (livingOnly)
            {
                if (actor.Health > 0)
                    count++;
                continue;
            }

            if (actor.IsMapActor())
                count++;
        }

        return count;
    }

    static bool ActorMatchesClass(Actor actor, object pclass, Func<Actor, object?> getActorClass) =>
        pclass switch
        {
            string className => DoomActorCatalog.TryEditorNumberForClassName(className, out var ed)
                && actor.DoomEdNum == ed,
            int editorNumber => actor.DoomEdNum == editorNumber,
            _ => ReferenceEquals(getActorClass(actor), pclass),
        };

    static bool TryReplacementClass(object pclass, out object replacement)
    {
        replacement = pclass;
        if (pclass is not string className
            || !DoomActorCatalog.TryEditorNumberForClassName(className, out var editorNumber))
            return false;
        var replaced = AcsThingCountReplacementPolicy.ResolveEditorNumber(editorNumber);
        if (replaced is null || replaced.Value == editorNumber)
            return false;
        if (!DoomActorCatalog.TrySpawnClassName(replaced.Value, out var replacedName))
            return false;
        replacement = replacedName;
        return true;
    }
}
