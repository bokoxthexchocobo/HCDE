namespace HCDE.Playsim;

/// <summary>Legacy map/missile <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (3048–3237).</summary>
public static partial class VmFScriptParser
{
    static readonly string[] ObjStateLabels =
    [
        "Spawn", "See", "Missile", "Melee", "Pain", "Death", "Raise", "Death.Extreme", "Crash",
    ];

    public static void SfSpawnMissile(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 3) || ctx.Simulation == null)
            return;
        var pclass = GetMobjType(ctx, ctx.TArgv[2]);
        if (pclass == null || !TryProjectileKind(pclass, out var kind))
            return;
        var mobj = AsActor(ctx.ActorValue(ctx.TArgv[0]));
        var target = AsActor(ctx.ActorValue(ctx.TArgv[1]));
        if (mobj == null || target == null)
            return;
        ctx.Simulation.SpawnProjectile(mobj, kind, target);
    }

    public static void SfMapThingNumExist(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1) || ctx.Simulation == null)
            return;
        var intval = ctx.IntValue(ctx.TArgv[0]);
        var spawned = ctx.Simulation.FraggleScriptSpawnedThings;
        ctx.TReturn.Type = SvtInt;
        if (intval < 0 || intval >= spawned.Count || spawned[intval] is not Actor actor)
        {
            ctx.TReturn.IntValue = 0;
            return;
        }

        ctx.TReturn.IntValue = actor.IsMapActor() ? 1 : 0;
    }

    public static void SfMapThings(ParserContext ctx)
    {
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = ctx.Simulation?.FraggleScriptSpawnedThings.Count ?? 0;
    }

    public static void SfObjState(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        Actor? mo;
        int state;
        if (ctx.TArgc == 1)
        {
            mo = AsActor(ctx.Script?.Trigger);
            state = ctx.IntValue(ctx.TArgv[0]);
        }
        else
        {
            mo = AsActor(ctx.ActorValue(ctx.TArgv[0]));
            state = ctx.IntValue(ctx.TArgv[1]);
        }

        if (mo == null)
            return;
        if (state < 1 || state > ObjStateLabels.Length)
        {
            ctx.ScriptError("objstate: invalid state");
            return;
        }

        var label = ObjStateLabels[state - 1];
        if (!AcsActorStates.TryFindNamedState(mo, label, exact: true, out var stateIndex) || !mo.States.HasState(stateIndex))
            return;
        mo.States.Enter(mo, stateIndex);
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = stateIndex;
    }

    public static void SfLineFlag(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2) || ctx.Simulation == null)
            return;
        var linenum = ctx.IntValue(ctx.TArgv[0]);
        if (linenum >= ctx.Simulation.Level.Lines.Count)
        {
            ctx.ScriptError("LineFlag: Invalid line number.\n");
            return;
        }

        var line = ctx.Simulation.Level.Lines[linenum];
        var flagnum = ctx.IntValue(ctx.TArgv[1]);
        if (flagnum < 0 || (flagnum > 8 && flagnum != 15))
        {
            ctx.ScriptError("LineFlag: Invalid flag number.\n");
            return;
        }

        if (ctx.TArgc > 2)
        {
            line.Flags &= ~(1 << flagnum);
            if (ctx.IntValue(ctx.TArgv[2]) != 0)
                line.Flags |= 1 << flagnum;
        }

        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = (line.Flags & (1 << flagnum)) != 0 ? 1 : 0;
    }

    public static void SfPlayerAddFrag(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1) || ctx.Simulation == null)
            return;
        if (ctx.TArgc == 1)
        {
            var player = GetPlayerActor(ctx, ctx.TArgv[0]) as PlayerPawn;
            if (player == null)
                return;
            player.FragCount++;
            ctx.TReturn.Type = SvtInt;
            ctx.TReturn.IntValue = player.FragCount;
            return;
        }

        var killer = GetPlayerActor(ctx, ctx.TArgv[0]) as PlayerPawn;
        var p2 = GetPlayerNum(ctx, ctx.TArgv[1]);
        if (killer == null || p2 < 0)
            return;
        killer.Frags[p2]++;
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = killer.Frags[p2];
    }

    static bool TryProjectileKind(object? pclass, out ProjectileKind kind)
    {
        kind = default;
        if (pclass is not string name)
            return false;
        foreach (ProjectileKind candidate in Enum.GetValues<ProjectileKind>())
        {
            if (string.Equals(ProjectileActor.NativeClassNameOf(candidate), name, StringComparison.OrdinalIgnoreCase))
            {
                kind = candidate;
                return true;
            }
        }

        if (string.Equals(name, "Rocket", StringComparison.OrdinalIgnoreCase))
        {
            kind = ProjectileKind.Rocket;
            return true;
        }

        return false;
    }
}
