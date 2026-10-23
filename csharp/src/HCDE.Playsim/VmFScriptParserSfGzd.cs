namespace HCDE.Playsim;

/// <summary>GZDoom-era <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (3700–4008).</summary>
public static partial class VmFScriptParser
{
    public sealed partial class ParserContext
    {
        public VmFScriptThinker.ThinkerState? FraggleThinker { get; init; }
        public readonly record struct FraggleLineSpec(int Number, int MinArgs);
    }

    public static void SfSpawnShot2(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2) || ctx.Simulation == null)
            return;
        Actor? source;
        if (ctx.TArgv[1].Type == SvtInt && ctx.TArgv[1].IntValue < 0)
            source = AsActor(ctx.Script?.Trigger);
        else
            source = AsActor(ctx.ActorValue(ctx.TArgv[1]));
        if (source == null)
            return;
        var z = ctx.TArgc > 2 ? ctx.FloatValue(ctx.TArgv[2]) : 0;
        var pclass = GetMobjType(ctx, ctx.TArgv[0]);
        if (pclass == null || !TryProjectileKind(pclass, out var kind))
            return;
        ctx.TReturn.Type = SvtMobj;
        var missile = ctx.Simulation.SpawnProjectile(source, kind, null);
        if (z != 0)
            missile.Z = Fixed.FromDouble(missile.Z.ToDouble() + z);
        ctx.TReturn.Mobj = missile;
    }

    public static void SfKillInSector(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1) || ctx.Simulation == null)
            return;
        var tag = ctx.IntValue(ctx.TArgv[0]);
        foreach (var actor in ctx.Simulation.Actors)
        {
            if (!actor.IsMonster || actor.Destroyed)
                continue;
            var sector = ctx.Simulation.Level.Sectors[actor.SectorIndex];
            if (!sector.MatchesTag(tag))
                continue;
            actor.Massacre();
        }
    }

    public static void SfSetLineTrigger(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2) || ctx.Simulation == null)
            return;
        var id = ctx.IntValue(ctx.TArgv[0]);
        var spec = ctx.IntValue(ctx.TArgv[1]);
        var tag = ctx.TArgc > 2 ? ctx.IntValue(ctx.TArgv[2]) : 0;
        foreach (var line in AcsLineActivation.LinesFromId(ctx.Simulation, id))
        {
            line.Special = spec;
            line.Arg0 = tag;
        }
    }

    public static void SfRunLineSpecial(ParserContext ctx, ParserContext.FraggleLineSpec spec)
    {
        if (!CheckArgs(ctx, spec.MinArgs) || ctx.Simulation == null)
            return;
        var args = new int[5];
        for (var i = 0; i < 5; i++)
            args[i] = ctx.TArgc > i ? ctx.IntValue(ctx.TArgv[i]) : 0;
        var activator = AsActor(ctx.Script?.Trigger);
        var ok = LineSpecials.Execute(ctx.Simulation, activator, spec.Number, args[0]);
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = ok ? 1 : 0;
    }

    public static VmFScriptScript.RunningScript? SaveCurrentScript(ParserContext ctx)
    {
        var th = ctx.FraggleThinker;
        if (th == null || ctx.Script == null)
            return null;
        var runscr = th.CreateRunningScript(ctx.Script.Trigger, ctx.Script, ctx.Rover);
        VmFScriptThinker.AddRunningScript(th, runscr);
        return runscr;
    }

}
