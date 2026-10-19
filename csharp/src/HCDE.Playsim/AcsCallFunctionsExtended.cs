using HCDE.MapLoader;

namespace HCDE.Playsim;

internal static partial class AcsCallFunctions
{
    public const int SetSkyScrollSpeed = 18;
    public const int SetCVar = 53;
    public const int GetUserCVar = 54;
    public const int SetUserCVar = 55;
    public const int GetCVarString = 56;
    public const int SetCVarString = 57;
    public const int GetUserCVarString = 58;
    public const int SetUserCVarString = 59;
    public const int GetTeamScore = 19620;
    public const int SetTeamScore = 19621;
    public const int InvasionGetState = 19700;
    public const int InvasionGetStateTics = 19701;
    public const int InvasionGetWave = 19702;
    public const int InvasionGetMaxWaves = 19703;
    public const int InvasionGetWaveBudget = 19704;
    public const int InvasionGetWaveSpawned = 19705;
    public const int InvasionGetWaveCleared = 19706;
    public const int InvasionGetActiveMonsterCount = 19707;
    public const int InvasionIsBossWave = 19708;
    public const int InvasionGetSpawnSpotCount = 19709;
    public const int InvasionGetActiveSpawnSpotCount = 19710;
    public const int InvasionGetSpawnPlanBudget = 19711;
    public const int InvasionGetSpawnActiveTag = 19712;
    public const int InvasionIsSpawnUsingFallback = 19713;
    public const int InvasionGetSpawnFallbackSource = 19714;
    public const int InvasionControlAction = 19715;

    private static bool TryExtendedInvoke(
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
            case SetSkyScrollSpeed:
                return TrySetSkyScrollSpeed(sim, stack, argCount, out result);
            case SetCVar:
                return TrySetCVar(sim, stack, scriptActivator, stringTable, globalStrings, argCount, out result);
            case GetUserCVar:
                return TryGetUserCVar(sim, stack, stringTable, globalStrings, argCount, false, out result);
            case SetUserCVar:
                return TrySetUserCVar(sim, stack, stringTable, globalStrings, argCount, false, out result);
            case GetCVarString:
                return TryGetCVarString(sim, stack, scriptActivator, stringTable, globalStrings, argCount, out result);
            case SetCVarString:
                return TrySetCVarString(sim, stack, scriptActivator, stringTable, globalStrings, argCount, out result);
            case GetUserCVarString:
                return TryGetUserCVar(sim, stack, stringTable, globalStrings, argCount, true, out result);
            case SetUserCVarString:
                return TrySetUserCVar(sim, stack, stringTable, globalStrings, argCount, true, out result);
            case GetTeamScore:
                return TryGetTeamScore(sim, stack, argCount, out result);
            case SetTeamScore:
                return TrySetTeamScore(sim, stack, argCount, out result);
            case InvasionGetState:
            case InvasionGetStateTics:
            case InvasionGetWave:
            case InvasionGetMaxWaves:
            case InvasionGetWaveBudget:
            case InvasionGetWaveSpawned:
            case InvasionGetWaveCleared:
            case InvasionGetActiveMonsterCount:
            case InvasionIsBossWave:
            case InvasionGetSpawnSpotCount:
            case InvasionGetActiveSpawnSpotCount:
            case InvasionGetSpawnPlanBudget:
            case InvasionGetSpawnActiveTag:
            case InvasionIsSpawnUsingFallback:
            case InvasionGetSpawnFallbackSource:
            case InvasionControlAction:
                return TryInvasionCall(sim, stack, function, argCount, out result);
            default:
                return TryAbsentCallFuncs(sim, stack, scriptActivator, stringTable, globalStrings, function, argCount, out result);
        }
    }

    private static bool TrySetSkyScrollSpeed(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var layer = stack[start];
        var speedFixed = stack[start + 1];
        stack.RemoveRange(start, argCount);
        var speed = speedFixed / 65536.0;
        switch (layer)
        {
            case 1: sim.Level.SkySpeed1 = speed; break;
            case 2: sim.Level.SkySpeed2 = speed; break;
            case 3: sim.Level.SkyMistSpeed = speed; break;
            default: return true;
        }

        result = 1;
        return true;
    }

    private static bool TrySetCVar(
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
        var name = AcsStringIds.Lookup(stack[start], stringTable, globalStrings);
        var value = stack[start + 1];
        stack.RemoveRange(start, argCount);
        if (string.IsNullOrEmpty(name)) return true;
        sim.CVars.Set(name, ConsoleVariable.IntValue(value));
        result = 1;
        return true;
    }

    private static bool TrySetCVarString(
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
        var name = AcsStringIds.Lookup(stack[start], stringTable, globalStrings);
        var textId = stack[start + 1];
        stack.RemoveRange(start, argCount);
        var text = AcsStringIds.LookupTerminated(textId, stringTable, globalStrings);
        if (string.IsNullOrEmpty(name)) return true;
        sim.CVars.Set(name, ConsoleVariable.StringValue(text));
        result = 1;
        return true;
    }

    private static bool TryGetCVarString(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = globalStrings.Add(string.Empty);
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var name = AcsStringIds.Lookup(stack[start], stringTable, globalStrings);
        stack.RemoveRange(start, argCount);
        if (string.IsNullOrEmpty(name)) return true;
        var playerNum = ResolveAcsContextPlayerNum(sim, activator);
        var text = activator?.GetCVarString(name)
            ?? sim.CVars.ReadString(playerNum, sim.IsPlayerInGame, sim.ConsolePlayer, name);
        result = globalStrings.Add(text);
        return true;
    }

    private static bool TryGetUserCVar(
        AuthoritySimulation sim,
        List<int> stack,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        bool asString,
        out int result)
    {
        result = asString ? globalStrings.Add(string.Empty) : 0;
        if (argCount < 2 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var playerNum = stack[start];
        var name = AcsStringIds.Lookup(stack[start + 1], stringTable, globalStrings);
        stack.RemoveRange(start, argCount);
        if (!sim.IsPlayerInGame(playerNum) || string.IsNullOrEmpty(name))
            return true;
        var player = sim.Players.FirstOrDefault(p => p.PlayerNum == playerNum);
        if (player is null) return true;
        if (asString)
        {
            result = globalStrings.Add(player.GetCVarString(name));
            return true;
        }

        result = AcsFixedPoint.FromDouble(player.GetCVar(name));
        return true;
    }

    private static bool TrySetUserCVar(
        AuthoritySimulation sim,
        List<int> stack,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        bool asString,
        out int result)
    {
        result = 0;
        if (argCount < 3 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var playerNum = stack[start];
        var name = AcsStringIds.Lookup(stack[start + 1], stringTable, globalStrings);
        var value = stack[start + 2];
        stack.RemoveRange(start, argCount);
        if (!sim.IsPlayerInGame(playerNum) || string.IsNullOrEmpty(name))
            return true;
        if (asString)
        {
            var text = AcsStringIds.LookupTerminated(value, stringTable, globalStrings);
            sim.CVars.SetUser(playerNum, name, ConsoleVariable.StringValue(text));
        }
        else sim.CVars.SetUser(playerNum, name, ConsoleVariable.IntValue(value));

        result = 1;
        return true;
    }

    private static bool TryGetTeamScore(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 1 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var team = stack[start];
        stack.RemoveRange(start, argCount);
        result = sim.GetTeamScore(team);
        return true;
    }

    private static bool TrySetTeamScore(AuthoritySimulation sim, List<int> stack, int argCount, out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount) return false;
        var start = stack.Count - argCount;
        var team = stack[start];
        var score = stack[start + 1];
        stack.RemoveRange(start, argCount);
        sim.SetTeamScore(team, score);
        result = 1;
        return true;
    }

    private static bool TryInvasionCall(AuthoritySimulation sim, List<int> stack, int function, int argCount, out int result)
    {
        result = 0;
        if (function == InvasionControlAction)
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

    private static int ResolveAcsContextPlayerNum(AuthoritySimulation sim, Actor? activator)
    {
        if (activator is PlayerPawn { Destroyed: false } player) return player.PlayerNum;
        return sim.IsPlayerInGame(sim.ConsolePlayer) ? sim.ConsolePlayer : -1;
    }
}
