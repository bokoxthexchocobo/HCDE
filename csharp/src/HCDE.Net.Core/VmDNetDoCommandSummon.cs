namespace HCDE.Net.Core;

/// <summary><c>Net_DoCommand</c> summon, spray, mdk, autosave, FOV, script cases (<c>d_net.cpp</c>).</summary>
public static class VmDNetDoCommandSummon
{
    public const ulong MfMissile = 1 << 0;
    public const ulong MfFriendly = 1 << 1;
    public const ulong Mf3NoBlockMonsters = 1 << 2;
    public const ulong MtfConsoleThing = 1 << 3;

    public static void ApplySummonDoCommandNative(
        int cmd,
        int player,
        int demSummon2,
        int demSummonFoe2,
        int demSummonFriend,
        int demSummonFriend2,
        int demSummonMbf,
        int demSummonFoe,
        string actorClassName,
        SummonExtendedFields extended,
        double sourceYaw,
        object? source,
        Func<string, object?>? findActorClass,
        Func<object?, ulong>? getDefaultFlags,
        Func<object?, double>? getDefaultRadius,
        Func<object?, double>? getSourceRadius,
        Func<object?, double, double, (double X, double Y, double Z)>? vec3Angle,
        Func<object?, object?, object?, object?>? spawnActor,
        Func<string, object?>? findClass,
        Func<object?, bool>? isVisualThinkerDescendant,
        Func<object?, object?, object?>? newVisualThinker,
        Action<object?, object?, object?>? spawnPlayerMissile,
        Action<object>? setConsoleThingSpawnFlag,
        Func<object, bool>? countsAsKill,
        Action? decrementTotalMonsters,
        Action<object, int>? setFriendPlayer,
        Action<object>? addFriendlyFlag,
        Action<object, object?>? setLastHeard,
        Action<object>? restoreSpawnHealth,
        Action<object>? clearFriendlyFlag,
        Action<object, double>? setSpawnYaw,
        Action<object, byte>? setSpecial,
        Action<object, int, int>? setSpawnArg,
        Action<object, short>? setTid,
        Action<object>? addNoBlockMonsters,
        Action<object, double, double, double>? setVisualThinkerPosition)
    {
        if (source is null)
            return;

        var typeInfo = findActorClass?.Invoke(actorClassName);
        if (typeInfo is not null)
        {
            var flags = getDefaultFlags?.Invoke(typeInfo) ?? 0;
            if ((flags & MfMissile) != 0)
            {
                spawnPlayerMissile?.Invoke(source, typeInfo, source);
                return;
            }

            var defRadius = getDefaultRadius?.Invoke(typeInfo) ?? 0;
            var sourceRadius = getSourceRadius?.Invoke(source) ?? 0;
            var spawnPos = vec3Angle?.Invoke(source, defRadius * 2 + sourceRadius, 8) ?? (0, 0, 0);
            var spawned = spawnActor?.Invoke(null, typeInfo, spawnPos);
            if (spawned is null)
                return;

            setConsoleThingSpawnFlag?.Invoke(spawned);
            if (cmd == demSummonFriend || cmd == demSummonFriend2 || cmd == demSummonMbf)
            {
                if (countsAsKill?.Invoke(spawned) == true)
                    decrementTotalMonsters?.Invoke();
                setFriendPlayer?.Invoke(spawned, player + 1);
                addFriendlyFlag?.Invoke(spawned);
                setLastHeard?.Invoke(spawned, source);
                restoreSpawnHealth?.Invoke(spawned);
                if (cmd == demSummonMbf)
                    addNoBlockMonsters?.Invoke(spawned);
            }
            else if (cmd == demSummonFoe || cmd == demSummonFoe2)
            {
                setFriendPlayer?.Invoke(spawned, 0);
                clearFriendlyFlag?.Invoke(spawned);
                restoreSpawnHealth?.Invoke(spawned);
            }

            if (cmd >= demSummon2 && cmd <= demSummonFoe2)
            {
                setSpawnYaw?.Invoke(spawned, sourceYaw - extended.Angle);
                setSpecial?.Invoke(spawned, extended.Special);
                for (var i = 0; i < extended.Args.Length && i < 5; i++)
                    setSpawnArg?.Invoke(spawned, i, extended.Args[i]);
                if (extended.Tid != 0)
                    setTid?.Invoke(spawned, extended.Tid);
            }
            return;
        }

        var classInfo = findClass?.Invoke(actorClassName);
        if (classInfo is null || isVisualThinkerDescendant?.Invoke(classInfo) != true)
            return;

        var vtRadius = getSourceRadius?.Invoke(source) ?? 0;
        var vtPos = vec3Angle?.Invoke(source, vtRadius * 4, 8) ?? (0, 0, 0);
        var vt = newVisualThinker?.Invoke(source, classInfo);
        if (vt is null)
            return;
        setVisualThinkerPosition?.Invoke(vt, vtPos.X, vtPos.Y, vtPos.Z);
    }

    public static void ApplySprayDecalNative(object? mobj, string decalName, Action<object?, string>? sprayDecal)
    {
        if (mobj is null)
            return;
        sprayDecal?.Invoke(mobj, decalName);
    }

    public static void ApplyMdkNative(object? player, string name, Action<object?, string>? doMdk) =>
        doMdk?.Invoke(player, name);

    public static void ApplyCheckAutosaveNative(
        bool netgame,
        bool demoplayback,
        int disableAutosave,
        int autosaveCount,
        int playerState,
        int liveState,
        bool deathmatch,
        Action<byte>? writeDemByte)
    {
        if (netgame || demoplayback || disableAutosave >= 2 || autosaveCount == 0
            || playerState != liveState || deathmatch)
            return;
        writeDemByte?.Invoke(0);
    }

    public static void ApplyDoAutosaveNative(Action<string>? traceSetGameAction) =>
        traceSetGameAction?.Invoke("net-autosave");

    public static void ApplyFovDoCommandNative(
        float newFov,
        float desiredFov,
        int player,
        bool authoritySlot,
        IEnumerable<int> networkClients,
        Action<int, float>? setDesiredFov,
        Action<string>? printFov)
    {
        if (newFov != desiredFov)
        {
            var suffix = authoritySlot ? " for everyone" : "";
            printFov?.Invoke($"FOV{suffix} set to {newFov}");
        }

        foreach (var client in networkClients)
            setDesiredFov?.Invoke(client, newFov);
    }

    public static void ApplyMyFovNative(int player, float newFov, Action<int, float>? setDesiredFov) =>
        setDesiredFov?.Invoke(player, newFov);

    public static void ApplyRunScriptNative(
        int cmd,
        int demRunScript2,
        short scriptNumber,
        byte argCount,
        Action<short, byte, int>? runScript) =>
        runScript?.Invoke(scriptNumber, argCount, cmd == demRunScript2 ? 1 : 0);

    public static void ApplyRunNamedScriptNative(
        string scriptName,
        byte argnByte,
        int nameIndex,
        Action<int, byte, int>? runScript) =>
        runScript?.Invoke(-nameIndex, (byte)(argnByte & 127), (argnByte & 128) != 0 ? 1 : 0);

    public static void ApplyRunSpecialNative(
        short specialNumber,
        byte argCount,
        int player,
        int consolePlayer,
        ReadOnlySpan<int> streamArgs,
        Func<bool, bool>? checkCheatMode,
        Action<short, int, int, int, int, int>? executeSpecial)
    {
        Span<int> args = stackalloc int[5];
        for (var i = 0; i < argCount; i++)
        {
            var argVal = i < streamArgs.Length ? streamArgs[i] : 0;
            if ((uint)i < 5u)
                args[i] = argVal;
        }

        if (checkCheatMode?.Invoke(player == consolePlayer) == true)
            return;

        executeSpecial?.Invoke(specialNumber, args[0], args[1], args[2], args[3], args[4]);
    }
}

public readonly struct SummonExtendedFields(int angle, short tid, byte special, int[] args)
{
    public int Angle { get; } = angle;
    public short Tid { get; } = tid;
    public byte Special { get; } = special;
    public int[] Args { get; } = args;
}
