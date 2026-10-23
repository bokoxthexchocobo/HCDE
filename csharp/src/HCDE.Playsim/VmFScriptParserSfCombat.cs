namespace HCDE.Playsim;

/// <summary>Deferred combat <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (3259–3316).</summary>
public static partial class VmFScriptParser
{
    const double MissileRange = 32 * 64 * 6;

    public static void SfResurrect(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var mo = AsActor(ctx.ActorValue(ctx.TArgv[0]));
        if (mo == null || mo.RaiseState < 0 || !mo.States.HasState(mo.RaiseState))
            return;
        mo.States.Enter(mo, mo.RaiseState);
        ActorRaise.ReviveSupported(mo, restoreDimensions: true);
        mo.SetTargetPointer(null);
    }

    public static void SfLineAttack(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 3) || ctx.Simulation == null)
            return;
        var mo = AsActor(ctx.ActorValue(ctx.TArgv[0]));
        if (mo == null)
            return;
        var angleDeg = ctx.FloatValue(ctx.TArgv[1]);
        var damage = ctx.IntValue(ctx.TArgv[2]);
        var angleRaw = (int)(angleDeg / 360.0 * 4294967296.0);
        _ = AcsLineAttack.Attack(ctx.Simulation, mo, angleRaw, 0, damage, "Hitscan", MissileRange);
    }
}
