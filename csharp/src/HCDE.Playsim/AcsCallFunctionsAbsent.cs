using HCDE.MapLoader;

namespace HCDE.Playsim;

internal static partial class AcsCallFunctions
{
    public const int AnnouncerSound = 37;
    public const int SetPointer = 38;
    public const int AcsNamedExecute = 39;
    public const int AcsNamedSuspend = 40;
    public const int AcsNamedTerminate = 41;
    public const int AcsNamedLockedExecute = 42;
    public const int AcsNamedLockedExecuteDoor = 43;
    public const int AcsNamedExecuteWithResult = 44;
    public const int AcsNamedExecuteAlways = 45;
    public const int SetHudClipRect = 51;
    public const int SetHudWrapWidth = 52;
    public const int Warp = 92;
    public const int SetTranslation = 203;
    public const int ScriptCall = 210;
    public const int StartSlideshow = 211;
    public const int SetSubtitleNumber = 214;
    public const int SetSectorGlow = 400;
    public const int SetFogDensity = 401;

    private const int WarpfUsePtr = 1 << 16;
    private static readonly int[] NamedAcsLineSpecials = [80, 81, 82, 83, 84, 85, 226];

    private static bool TryAbsentCallFuncs(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? scriptActivator,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int function,
        int argCount,
        out int result)
    {
        result = 0;
        switch (function)
        {
            case AnnouncerSound:
                return TryAnnouncerSound(sim, stack, stringTable, globalStrings, argCount, out result);
            case SetPointer:
                return TrySetPointer(sim, stack, scriptActivator, argCount, out result);
            case AcsNamedExecute:
            case AcsNamedSuspend:
            case AcsNamedTerminate:
            case AcsNamedLockedExecute:
            case AcsNamedLockedExecuteDoor:
            case AcsNamedExecuteWithResult:
            case AcsNamedExecuteAlways:
                return TryNamedAcs(sim, stack, scriptActivator, stringTable, globalStrings, function, argCount, out result);
            case SetHudClipRect:
                return TrySetHudClipRect(sim, stack, argCount, out result);
            case SetHudWrapWidth:
                return TrySetHudWrapWidth(sim, stack, argCount, out result);
            case Warp:
                return TryWarp(sim, stack, scriptActivator, argCount, out result);
            case SetTranslation:
                return TrySetTranslation(sim, stack, scriptActivator, stringTable, globalStrings, argCount, out result);
            case ScriptCall:
                return TryScriptCall(stack, argCount, out result);
            case StartSlideshow:
                return TryStartSlideshow(sim, stack, stringTable, globalStrings, argCount, out result);
            case SetSubtitleNumber:
                return TrySetSubtitleNumber(sim, stack, argCount, out result);
            case SetSectorGlow:
                return TrySetSectorGlow(sim, stack, argCount, out result);
            case SetFogDensity:
                return TrySetFogDensity(sim, stack, argCount, out result);
            default:
                return false;
        }
    }

    private static bool TryAnnouncerSound(
        AuthoritySimulation sim,
        List<int> stack,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var sound = AcsStringIds.Lookup(stack[start], stringTable, globalStrings);
        stack.RemoveRange(start, argCount);
        if (!string.IsNullOrEmpty(sound)) sim.QueueAnnouncerSound(sound);
        return true;
    }

    private static bool TrySetPointer(AuthoritySimulation sim, List<int> stack, Actor? activator, int argCount, out int result)
    {
        result = 0;
        if (activator is null || argCount < 2 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var slot = stack[start];
        var tid = stack[start + 1];
        var pointer = argCount > 2 ? stack[start + 2] : AcsActorPointer.Default;
        stack.RemoveRange(start, argCount);
        var actor = AcsActorPointer.Resolve(sim, ResolveActor(sim, tid, activator), pointer);
        if (actor == activator) actor = null;
        result = activator.AssignAcsPointer(slot, actor) ? 1 : 0;
        return true;
    }

    private static bool TryNamedAcs(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int function,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var name = AcsStringIds.Lookup(stack[start], stringTable, globalStrings);
        var arg1 = argCount > 1 ? stack[start + 1] : 0;
        var arg2 = argCount > 2 ? stack[start + 2] : 0;
        var arg3 = argCount > 3 ? stack[start + 3] : 0;
        stack.RemoveRange(start, argCount);
        var script = AcsNamedScriptIndex.FromName(name);
        var special = NamedAcsLineSpecials[function - AcsNamedExecute];
        var line = sim.AcsTriggerLine;
        var backSide = sim.AcsTriggerBackSide;
        if (special is 83 or 84 or 85 && line is null) return true;
        var ok = special switch
        {
            80 => sim.Acs.TryExecute(script, [arg1, arg2, arg3], activator, line, backSide),
            81 => sim.Acs.SuspendScript(script),
            82 => sim.Acs.TerminateScript(script),
            83 => line is { } locked && LineSpecialMisc.ExecuteLockedAcsPublic(sim, 83, locked, activator, backSide),
            84 => line is { } lockedDoor && LineSpecialMisc.ExecuteLockedAcsPublic(sim, 84, lockedDoor, activator, backSide),
            85 => line is { } withResult && LineSpecialMisc.ExecuteLockedAcsPublic(sim, 85, withResult, activator, backSide),
            226 => sim.Acs.ExecuteAlways(script, [arg1, arg2, arg3], activator, line, backSide),
            _ => false,
        };
        result = ok ? 1 : 0;
        return true;
    }

    private static bool TrySetHudClipRect(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        sim.AcsHudClipLeft = argCount > 0 ? stack[start] : 0;
        sim.AcsHudClipTop = argCount > 1 ? stack[start + 1] : 0;
        sim.AcsHudClipWidth = argCount > 2 ? stack[start + 2] : 0;
        sim.AcsHudClipHeight = argCount > 3 ? stack[start + 3] : 0;
        sim.AcsHudWrapWidth = argCount > 4 ? stack[start + 4] : 0;
        stack.RemoveRange(start, argCount);
        return true;
    }

    private static bool TrySetHudWrapWidth(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        sim.AcsHudWrapWidth = stack[start];
        stack.RemoveRange(start, argCount);
        return true;
    }

    private static bool TryWarp(AuthoritySimulation sim, List<int> stack, Actor? activator, int argCount, out int result)
    {
        result = 0;
        if (activator is null || argCount < 6 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var dest = stack[start];
        var flags = stack[start + 5];
        Actor? reference;
        if ((flags & WarpfUsePtr) != 0 && dest != AcsActorPointer.Default)
            reference = AcsActorPointer.Resolve(sim, activator, dest);
        else
            reference = ResolveActor(sim, dest, activator);
        var xofs = AcsToDouble(stack[start + 1]);
        var yofs = AcsToDouble(stack[start + 2]);
        var zofs = AcsToDouble(stack[start + 3]);
        stack.RemoveRange(start, argCount);
        if (reference is null || reference.Destroyed) return true;
        activator.X = Fixed.FromDouble(reference.X.ToDouble() + xofs);
        activator.Y = Fixed.FromDouble(reference.Y.ToDouble() + yofs);
        activator.Z = Fixed.FromDouble(reference.Z.ToDouble() + zofs);
        ActorPhysics.RefreshOnMobj(sim, activator);
        result = 1;
        return true;
    }

    private static bool TrySetTranslation(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
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
            if (activator is { Destroyed: false } self)
                changed = self.ApplyTranslationName(translation);
        }
        else
        {
            foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
                changed |= actor.ApplyTranslationName(translation);
        }
        result = changed ? 1 : 0;
        return true;
    }

    private static bool TryScriptCall(List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (stack.Count < argCount) return false;
        stack.RemoveRange(stack.Count - argCount, argCount);
        return true;
    }

    private static bool TryStartSlideshow(
        AuthoritySimulation sim,
        List<int> stack,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        sim.AcsSlideshowName = AcsStringIds.Lookup(stack[start], stringTable, globalStrings);
        stack.RemoveRange(start, argCount);
        return true;
    }

    private static bool TrySetSubtitleNumber(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        sim.AcsSubtitleNumber = stack[start];
        stack.RemoveRange(start, argCount);
        return true;
    }

    private static bool TrySetSectorGlow(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
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

    private static bool TrySetFogDensity(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var tag = stack[start];
        var density = Math.Clamp(stack[start + 1] / 2, 0, 255);
        stack.RemoveRange(start, argCount);
        foreach (var sector in sim.Level.Sectors)
            if (sector.MatchesTag(tag))
                sector.FogDensity = density;
        return true;
    }
}

internal static class AcsNamedScriptIndex
{
    internal static int FromName(string name)
    {
        if (string.IsNullOrEmpty(name)) return 0;
        unchecked
        {
            var hash = 2166136261u;
            foreach (var ch in name.ToUpperInvariant())
                hash = (hash ^ ch) * 16777619u;
            var index = (int)(hash | 0x80000000u);
            return index == 0 ? -1 : index;
        }
    }
}
