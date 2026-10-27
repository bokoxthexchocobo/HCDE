namespace HCDE.Playsim;

/// <summary><c>DLevelScript::CallFunction</c> tail and script helpers <c>p_acs.cpp</c> (**6916–7085**); chunk **925**.</summary>
internal static class VmPAcsCallfunc925Slice
{
    public const int PrintNameLevelName = -1;
    public const int PrintNameLevel = -2;
    public const int PrintNameSkill = -3;
    public const int PrintNameNextLevel = -4;
    public const int PrintNameNextSecret = -5;

    private static double AcsToDouble(int value) => value / 65536.0;

    public static bool TryGetLineCoordinate(AuthoritySimulation sim, List<int> stack, bool yAxis, int argCount, out int result)
    {
        result = 0;
        if (argCount < 3 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var lineId = stack[start];
        var along = AcsToDouble(stack[start + 1]);
        var perpRaw = stack[start + 2];
        stack.RemoveRange(start, argCount);
        var line = AcsLineActivation.FirstLineFromId(sim, lineId);
        if (line is null) return true;
        var deltaX = line.X2 - line.X1;
        var deltaY = line.Y2 - line.Y1;
        var value = (yAxis ? line.Y1 : line.X1) + (yAxis ? deltaY : deltaX) * along;
        if (perpRaw != 0)
        {
            var perpDistance = AcsToDouble(perpRaw);
            var length = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if (length > 0)
            {
                var normalX = deltaY / length;
                var normalY = -deltaX / length;
                value += (yAxis ? normalY : normalX) * perpDistance;
            }
        }
        result = AcsFixedPoint.FromDouble(value);
        return true;
    }

    public static bool TrySetSubtitleNumber(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        sim.AcsSubtitleNumber = stack[start];
        stack.RemoveRange(start, argCount);
        return true;
    }

    public static bool TryInvasionCall(AuthoritySimulation sim, List<int> stack, int function, int argCount, out int result)
    {
        result = 0;
        if (function == AcsCallFunctions.InvasionControlAction)
        {
            if (argCount < 1 || stack.Count < argCount) return false;
            var start = stack.Count - argCount;
            _ = stack[start];
            stack.RemoveRange(start, argCount);
            result = 0;
            return true;
        }
        if (argCount != 0) return false;
        result = sim.Invasion.QueryAcs(function) ?? 0;
        return true;
    }

    public static int ReadEnhancedByte(ref int pc, byte[] scriptBytes)
    {
        var res = scriptBytes[pc];
        pc += 1;
        return res;
    }

    public static int ReadEnhancedShort(ref int pc, byte[] scriptBytes)
    {
        var res = scriptBytes[pc] | (scriptBytes[pc + 1] << 8);
        pc += 2;
        return (short)res;
    }

    public static bool ShouldBlockScriptConsoleCommand(string? command)
    {
        if (string.IsNullOrEmpty(command)) return false;
        foreach (var ch in command)
            if (ch is '\r' or '\n') return true;
        return false;
    }

    public static void RunScriptConsoleCommand(string? command, Action<string> addCommandString)
    {
        if (string.IsNullOrEmpty(command) || ShouldBlockScriptConsoleCommand(command)) return;
        addCommandString(command);
    }

    public static bool TryPopCharArrayParms(List<int> stack, ref int sp, bool ranged, out int capacity, out int offset, out int index)
    {
        capacity = 0;
        offset = 0;
        index = 0;
        if (sp < (ranged ? 4 : 2)) return false;
        if (ranged)
        {
            capacity = stack[sp - 1];
            offset = stack[sp - 2];
            if (capacity < 1 || offset < 0)
            {
                sp -= 4;
                return false;
            }
            sp -= 2;
        }
        else
        {
            capacity = int.MaxValue;
            offset = 0;
        }
        index = stack[sp - 1];
        offset += stack[sp - 2];
        sp -= 2;
        return true;
    }
}
