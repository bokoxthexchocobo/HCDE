namespace HCDE.Playsim;

/// <summary>`SF_PushThing` from <c>fragglescript/t_func.cpp</c> (1204–1215); exclusive to chunk **906** (momentum band remains in `VmFScriptParserSfMom.cs`).</summary>
public static partial class VmFScriptParser
{
    public static void SfPushThing(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 3))
            return;
        var mo = ctx.ActorValue(ctx.TArgv[0]) as Actor;
        if (mo == null)
            return;
        var angle = BamAngle.FromDegrees(ctx.FloatValue(ctx.TArgv[1]));
        var force = ctx.FloatValue(ctx.TArgv[2]);
        mo.Thrust(angle, force);
    }
}
