namespace HCDE.Playsim;

/// <summary>Native <c>DoSetCVar</c>, <c>DoGetCVar</c>, <c>SetCVar</c>, and <c>SetUserCVar</c> from <c>p_acs.cpp</c>.</summary>
internal static class AcsModCvarService
{
    public static int ReadAcsValue(ConsoleVariable? variable, AcsGlobalStrings globalStrings, bool asString)
    {
        if (variable is null)
            return asString ? globalStrings.Add(string.Empty) : 0;

        if (asString)
            return globalStrings.Add(variable.AsString());

        return variable.Kind switch
        {
            ConsoleVariableKind.Float => AcsFixedPoint.FromDouble(variable.AsFloat()),
            ConsoleVariableKind.String => globalStrings.Add(variable.AsString()),
            _ => variable.Int,
        };
    }

    public static ConsoleVariable WriteAcsValue(
        ConsoleVariable? existing,
        int acsValue,
        bool asString,
        string? stringValue)
    {
        if (asString)
            return ConsoleVariable.StringValue(stringValue ?? string.Empty, existing?.Flags ?? ConsoleVariableFlags.None);

        if (existing?.Kind == ConsoleVariableKind.Float)
            return ConsoleVariable.FloatValue((float)(acsValue / 65536.0), existing.Flags);

        return ConsoleVariable.IntValue(acsValue, existing?.Flags ?? ConsoleVariableFlags.None);
    }

    public static int TrySetCvar(
        AuthoritySimulation sim,
        Actor? activator,
        string name,
        int acsValue,
        bool asString,
        string[] stringTable,
        AcsGlobalStrings globalStrings)
    {
        if (string.IsNullOrEmpty(name))
            return 0;

        if (sim.CVars.TryResolve(-1, sim.IsPlayerInGame, sim.ConsolePlayer, name) is { Flags: var flags }
            && flags.HasFlag(ConsoleVariableFlags.UserInfo))
        {
            if (activator is not PlayerPawn { Destroyed: false } player)
                return 0;
            return TrySetUserCvar(sim, player.PlayerNum, name, acsValue, asString, stringTable, globalStrings);
        }

        var text = asString ? AcsStringIds.LookupTerminated(acsValue, stringTable, globalStrings) : null;
        var existing = sim.CVars.TryResolve(-1, sim.IsPlayerInGame, sim.ConsolePlayer, name);
        sim.CVars.Set(name, WriteAcsValue(existing, acsValue, asString, text));
        return 1;
    }

    public static int TrySetUserCvar(
        AuthoritySimulation sim,
        int playerNum,
        string name,
        int acsValue,
        bool asString,
        string[] stringTable,
        AcsGlobalStrings globalStrings)
    {
        if (!sim.IsPlayerInGame(playerNum) || string.IsNullOrEmpty(name))
            return 0;

        var text = asString ? AcsStringIds.LookupTerminated(acsValue, stringTable, globalStrings) : null;
        var existing = sim.CVars.TryResolve(playerNum, sim.IsPlayerInGame, sim.ConsolePlayer, name);
        var written = WriteAcsValue(existing, acsValue, asString, text);
        sim.CVars.SetUser(playerNum, name, written);

        if (sim.ConsolePlayer == playerNum)
        {
            var mirror = sim.CVars.TryResolve(-1, sim.IsPlayerInGame, sim.ConsolePlayer, name);
            sim.CVars.Set(name, WriteAcsValue(mirror, acsValue, asString, text));
        }

        return 1;
    }
}
