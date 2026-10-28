namespace HCDE.Playsim;

/// <summary><c>DLevelScript::CallFunction</c> ACSF cases <c>p_acs.cpp</c> (**6766–6915**); chunk **924**.</summary>
internal static class VmPAcsCallfunc924Slice
{
    private static Actor? ResolveActor(AuthoritySimulation sim, int tid, Actor? activator) =>
        tid == 0 ? activator is { Destroyed: false } ? activator : null : AcsActorTid.SingleFromTid(sim, tid);

    public static bool TrySetTranslation(AuthoritySimulation sim, List<int> stack, Actor? activator,
        string[] stringTable, AcsGlobalStrings globalStrings, int argCount, out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var tid = stack[start];
        var translation = AcsStringIds.Lookup(stack[start + 1], stringTable, globalStrings);
        stack.RemoveRange(start, argCount);
        var changed = false;
        if (tid == 0)
        {
            if (activator is { Destroyed: false } self) changed = self.ApplyTranslationName(translation);
        }
        else
        {
            foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
                changed |= actor.ApplyTranslationName(translation);
        }
        result = changed ? 1 : 0;
        return true;
    }

    public static bool TrySetSectorGlow(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 6 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var tag = stack[start];
        var ceiling = stack[start + 1] != 0;
        var r = stack[start + 2];
        var g = stack[start + 3];
        var b = stack[start + 4];
        var height = stack[start + 5];
        stack.RemoveRange(start, argCount);
        var color = r < 0 ? 0u : (uint)((r & 255) | ((g & 255) << 8) | ((b & 255) << 16));
        var glowHeight = height / 65536f;
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(tag)) continue;
            if (ceiling)
            {
                sector.CeilingGlowColor = color;
                sector.CeilingGlowHeight = glowHeight;
            }
            else
            {
                sector.FloorGlowColor = color;
                sector.FloorGlowHeight = glowHeight;
            }
        }
        return true;
    }

    public static bool TrySetFogDensity(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var tag = stack[start];
        var density = Math.Clamp(stack[start + 1] / 2, 0, 255);
        stack.RemoveRange(start, argCount);
        foreach (var sector in sim.Level.Sectors)
            if (sector.MatchesTag(tag)) sector.FogDensity = density;
        return true;
    }

    public static bool TryGetActorFloorTexture(AuthoritySimulation sim, List<int> stack, Actor? activator,
        AcsGlobalStrings globalStrings, int argCount, out int result)
    {
        result = globalStrings.Add(string.Empty);
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);
        var actor = ResolveActor(sim, tid, activator);
        if (actor is null || actor.SectorIndex < 0 || actor.SectorIndex >= sim.Level.Sectors.Count) return true;
        result = globalStrings.Add(sim.Level.Sectors[actor.SectorIndex].FloorPic);
        return true;
    }

    public static bool TryGetActorFloorTerrain(AuthoritySimulation sim, List<int> stack, Actor? activator,
        AcsGlobalStrings globalStrings, int argCount, out int result)
    {
        result = globalStrings.Add(string.Empty);
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var tid = stack[start];
        stack.RemoveRange(start, argCount);
        var actor = ResolveActor(sim, tid, activator);
        if (actor is null || actor.SectorIndex < 0 || actor.SectorIndex >= sim.Level.Sectors.Count) return true;
        var terrain = actor.GetFloorTerrain();
        result = globalStrings.Add(terrain?.Name ?? sim.Level.Sectors[actor.SectorIndex].FloorTerrain);
        return true;
    }

    public static bool TryStrArg(AuthoritySimulation sim, List<int> stack, AcsGlobalStrings globalStrings, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var index = stack[start];
        stack.RemoveRange(start, argCount);
        var text = index >= 0 && index < sim.StrArgs.Count ? sim.StrArgs[index] : string.Empty;
        result = globalStrings.Add(text);
        return true;
    }

    public static bool TryFixedQuantize(List<int> stack, int function, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var fixedValue = stack[start];
        stack.RemoveRange(start, argCount);
        const int fractionalMask = 0xFFFF;
        const int fractionalRoundBias = 0x8000;
        var integral = fixedValue & ~fractionalMask;
        result = function switch
        {
            AcsCallFunctions.Floor => integral,
            AcsCallFunctions.Round => (fixedValue + fractionalRoundBias) & ~fractionalMask,
            AcsCallFunctions.Ceil => integral + 0x10000,
            _ => 0,
        };
        return true;
    }

    public static bool TryScriptCall(AuthoritySimulation sim, List<int> stack, Actor? activator,
        string[] stringTable, AcsGlobalStrings globalStrings, int argCount, out int result) =>
        AcsScriptCallService.TryInvoke(sim, stack, activator, stringTable, globalStrings, argCount, out result);

    public static bool TryStartSlideshow(AuthoritySimulation sim, List<int> stack, string[] stringTable,
        AcsGlobalStrings globalStrings, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        sim.AcsSlideshowName = AcsStringIds.Lookup(stack[start], stringTable, globalStrings);
        stack.RemoveRange(start, argCount);
        return true;
    }

    public static bool TryGetDestructibleHealth(AuthoritySimulation sim, List<int> stack, bool lineHealth, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var id = stack[start];
        if (lineHealth)
        {
            stack.RemoveRange(start, argCount);
            result = AcsDestructibleHealth.GetLineHealth(sim, id);
            return true;
        }
        if (argCount < 2) return false;
        var part = stack[start + 1];
        stack.RemoveRange(start, argCount);
        result = AcsDestructibleHealth.GetSectorHealth(sim, id, part);
        return true;
    }
}
