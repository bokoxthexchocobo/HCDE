namespace HCDE.Playsim;

/// <summary>`SF_RemoveObj` / `SF_KillObj` from <c>fragglescript/t_func.cpp</c> (877–918); exclusive to chunk **905** (remainder of mobj band stays in `VmFScriptParserSfObj.cs`).</summary>
public static partial class VmFScriptParser
{
    public static void SfRemoveObj(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var mo = ctx.ActorValue(ctx.TArgv[0]);
        if (mo == null)
            return;
        ctx.ClearActorCounters(mo);
        ctx.DestroyActor(mo);
    }

    public static void SfKillObj(ParserContext ctx)
    {
        object? mo;
        if (ctx.TArgc > 0)
            mo = ctx.ActorValue(ctx.TArgv[0]);
        else
            mo = ctx.Script?.Trigger;

        if (mo == null)
            return;
        ctx.MarkActorMassacreKillable(mo);
        ctx.DamageActorMassacre(mo, null, null, ctx.ActorHealth(mo));
    }
}
