using HCDE.Gamedata;

namespace HCDE.Playsim;

/// <summary><c>DLevelScript::CallFunction</c> ACSF cases <c>p_acs.cpp</c> (**6614–6764**); chunk **923**.</summary>
internal static class VmPAcsCallfunc923Slice
{
    private static double AcsToDouble(int value) => value / 65536.0;
    private static Actor? ResolveActor(AuthoritySimulation sim, int tid, Actor? activator) =>
        tid == 0 ? activator is { Destroyed: false } ? activator : null : AcsActorTid.SingleFromTid(sim, tid);

    public static bool TryGetMaxInventory(AuthoritySimulation sim, List<int> stack, Actor? activator,
        string[] stringTable, AcsGlobalStrings globalStrings, int argCount, out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var tid = stack[start];
        var stringId = stack[start + 1];
        stack.RemoveRange(start, argCount);
        var actor = ResolveActor(sim, tid, activator);
        result = actor is { Destroyed: false }
            ? AcsPlayerInventory.Count(actor, AcsStringIds.Lookup(stringId, stringTable, globalStrings), max: true) : 0;
        return true;
    }

    public static bool TrySetSectorDamage(AuthoritySimulation sim, List<int> stack, string[] stringTable,
        AcsGlobalStrings globalStrings, int argCount, out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var tag = stack[start];
        var amount = stack[start + 1];
        var damageType = argCount > 2 ? AcsStringIds.Lookup(stack[start + 2], stringTable, globalStrings) : "None";
        var interval = argCount > 3 ? Math.Max(1, stack[start + 3]) : 32;
        var leakiness = argCount > 4 ? stack[start + 4] : 0;
        stack.RemoveRange(start, argCount);
        foreach (var sector in sim.Level.Sectors.Where(sector => sector.MatchesTag(tag)))
        {
            sector.DamageAmount = amount;
            sector.DamageType = damageType.Length == 0 ? "None" : damageType;
            sector.DamageInterval = unchecked((short)interval);
            sector.Leakiness = unchecked((short)leakiness);
        }
        return true;
    }

    public static bool TrySetSectorTerrain(AuthoritySimulation sim, List<int> stack, string[] stringTable,
        AcsGlobalStrings globalStrings, int argCount, out int result)
    {
        result = 0;
        if (argCount < 3 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var tag = stack[start];
        var position = stack[start + 1];
        var terrainId = stack[start + 2];
        stack.RemoveRange(start, argCount);
        SectorTerrain.ApplyTagged(sim, tag,
            AcsStringIds.LookupTerminated(terrainId, stringTable, globalStrings), position);
        return true;
    }

    public static bool TrySpawnParticle(List<int> stack, int argCount, out int result)
    {
        result = 1;
        if (argCount < 1 || stack.Count < argCount) return false;
        stack.RemoveRange(stack.Count - argCount, argCount);
        return true;
    }

    public static bool TrySetMusicVolume(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 1;
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        sim.MusicVolume = AcsToDouble(stack[start]);
        stack.RemoveRange(start, argCount);
        return true;
    }

    public static bool TryCheckProximity(AuthoritySimulation sim, List<int> stack, Actor? activator,
        string[] stringTable, AcsGlobalStrings globalStrings, int argCount, out int result)
    {
        result = 0;
        if (argCount < 3 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var tid = stack[start];
        var classStringId = stack[start + 1];
        var distanceFixed = stack[start + 2];
        var required = argCount > 3 ? stack[start + 3] : 1;
        var flags = argCount > 4 ? stack[start + 4] : 0;
        var pointerSelector = argCount > 5 ? stack[start + 5] : 0;
        stack.RemoveRange(start, argCount);
        var actor = ResolveActor(sim, tid, activator);
        if (actor is null) return true;
        var className = AcsStringIds.Lookup(classStringId, stringTable, globalStrings);
        result = VmPThingsRaiseRemoveWarpProximity.Check(actor, className, AcsToDouble(distanceFixed), required, flags, pointerSelector) ? 1 : 0;
        return true;
    }

    public static bool TryCheckActorState(AuthoritySimulation sim, List<int> stack, Actor? activator,
        string[] stringTable, AcsGlobalStrings globalStrings, int argCount, out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var tid = stack[start];
        var stateStringId = stack[start + 1];
        var exact = argCount > 2 && stack[start + 2] != 0;
        stack.RemoveRange(start, argCount);
        var actor = ResolveActor(sim, tid, activator);
        if (actor is null) return true;
        result = AcsActorStates.HasNamedState(actor, AcsStringIds.Lookup(stateStringId, stringTable, globalStrings), exact) ? 1 : 0;
        return true;
    }

    public static bool TryCheckClass(List<int> stack, string[] stringTable, AcsGlobalStrings globalStrings, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var className = AcsStringIds.Lookup(stack[start], stringTable, globalStrings);
        stack.RemoveRange(start, argCount);
        result = DoomActorCatalog.TryEditorNumberForClassName(className, out _)
            || className.Equals("DoomPlayer", StringComparison.OrdinalIgnoreCase)
            || ActorInventoryActions.IsManagedPowerup(className)
            || PickupCatalog.TryEditorNumberForDropName(className, out _)
            || PickupCatalog.TryEditorNumberForDropName(className, out _, strifeHealth: true)
            || PickupCatalog.TryEditorNumberForDropName(className, out _, ravenHealth: true) ? 1 : 0;
        return true;
    }

    public static bool TryDamageActor(AuthoritySimulation sim, List<int> stack, Actor? activator,
        string[] stringTable, AcsGlobalStrings globalStrings, int argCount, out int result)
    {
        result = 0;
        if (argCount < 6 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var targetTid = stack[start];
        var targetPointer = stack[start + 1];
        var inflictorTid = stack[start + 2];
        var inflictorPointer = stack[start + 3];
        var amount = stack[start + 4];
        var damageTypeStringId = stack[start + 5];
        stack.RemoveRange(start, argCount);
        var target = AcsActorPointer.Resolve(sim, ResolveActor(sim, targetTid, activator), targetPointer);
        if (target is not { Destroyed: false }) return true;
        var inflictor = AcsActorPointer.Resolve(sim, ResolveActor(sim, inflictorTid, activator), inflictorPointer);
        var damageType = AcsStringIds.Lookup(damageTypeStringId, stringTable, globalStrings);
        ActorDamage.ApplyWithNativeResult(target, amount, out result, inflictor, DamageFlags.None, damageType, inflictor);
        return true;
    }

    public static bool TrySetActorFlag(AuthoritySimulation sim, List<int> stack, Actor? activator,
        string[] stringTable, AcsGlobalStrings globalStrings, int argCount, out int result)
    {
        result = 0;
        if (argCount < 3 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var tid = stack[start];
        var flagName = AcsStringIds.Lookup(stack[start + 1], stringTable, globalStrings);
        var set = stack[start + 2] != 0;
        stack.RemoveRange(start, argCount);
        if (tid == 0)
        {
            if (activator is { Destroyed: false } && AcsActorFlags.TrySet(activator, flagName, set)) result = 1;
            return true;
        }
        foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
            if (AcsActorFlags.TrySet(actor, flagName, set)) result++;
        return true;
    }
}
