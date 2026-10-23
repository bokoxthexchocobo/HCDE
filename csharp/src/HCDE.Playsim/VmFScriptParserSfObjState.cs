namespace HCDE.Playsim;

/// <summary>Mobj state <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (1041–1200).</summary>
public static partial class VmFScriptParser
{
    public sealed partial class ParserContext
    {
        public Action<int, object?> EvSilentTeleport { get; init; } = (_, _) => { };
        public Action<object?, object?, object?, int> DamageActor { get; init; } = (_, _, _, _) => { };
        public Func<object?, int> SectorFirstTag { get; init; } = _ => 0;
        public Func<object?, uint> GetActorFlags { get; init; } = _ => 0;
        public Action<object?, uint> SetActorFlags { get; init; } = (_, _) => { };
        public int TicRate { get; init; } = 35;
    }

    public static void SfSilentTeleport(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        object? mo;
        int tag;
        if (ctx.TArgc == 1)
        {
            mo = ctx.Script?.Trigger;
            tag = ctx.IntValue(ctx.TArgv[0]);
        }
        else
        {
            mo = ctx.ActorValue(ctx.TArgv[0]);
            tag = ctx.IntValue(ctx.TArgv[1]);
        }
        if (mo != null)
            ctx.EvSilentTeleport(tag, mo);
    }

    public static void SfDamageObj(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        object? mo;
        int damage;
        if (ctx.TArgc == 1)
        {
            mo = ctx.Script?.Trigger;
            damage = ctx.IntValue(ctx.TArgv[0]);
        }
        else
        {
            mo = ctx.ActorValue(ctx.TArgv[0]);
            damage = ctx.IntValue(ctx.TArgv[1]);
        }
        if (mo != null)
            ctx.DamageActor(mo, null, ctx.Script?.Trigger, damage);
    }

    public static void SfObjSector(ParserContext ctx)
    {
        var mo = OptionalActor(ctx);
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = mo == null ? 0 : ctx.SectorFirstTag(mo);
    }

    public static void SfObjHealth(ParserContext ctx)
    {
        var mo = OptionalActor(ctx);
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = mo == null ? 0 : ctx.ActorHealth(mo);
    }

    public static void SfObjFlag(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        object? mo;
        int flagnum;
        if (ctx.TArgc == 1)
        {
            mo = ctx.Script?.Trigger;
            flagnum = ctx.IntValue(ctx.TArgv[0]);
        }
        else if (ctx.TArgc == 2)
        {
            mo = ctx.ActorValue(ctx.TArgv[0]);
            flagnum = ctx.IntValue(ctx.TArgv[1]);
        }
        else
        {
            mo = ctx.ActorValue(ctx.TArgv[0]);
            flagnum = ctx.IntValue(ctx.TArgv[1]);
            if (mo != null && flagnum < 26)
            {
                var temp = ctx.GetActorFlags(mo);
                temp &= ~(1u << flagnum);
                temp |= (uint)((ctx.IntValue(ctx.TArgv[2]) != 0 ? 1 : 0) << flagnum);
                ctx.SetActorFlags(mo, temp);
            }
        }
        ctx.TReturn.Type = SvtInt;
        if (mo != null && flagnum < 26)
            ctx.TReturn.IntValue = (ctx.GetActorFlags(mo) & (1u << flagnum)) != 0 ? 1 : 0;
        else
            ctx.TReturn.IntValue = 0;
    }
}
