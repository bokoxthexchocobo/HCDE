namespace HCDE.Playsim;

/// <summary>Native <c>DLevelScript::CountPlayers</c> for ACS scripts.</summary>
public static class AcsPlayerPopulation
{
    public const int MaxPlayers = 8;

    public static int CountInGame(AuthoritySimulation sim, Func<int, bool>? isReservedSlot = null)
    {
        var count = 0;
        for (var slot = 0; slot < MaxPlayers; slot++)
        {
            if (isReservedSlot?.Invoke(slot) == true)
                continue;
            if (sim.IsPlayerInGame(slot))
                count++;
        }

        return count;
    }
}
