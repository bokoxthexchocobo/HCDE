namespace HCDE.Playsim;

/// <summary>Native ACS <c>GetPlayerInput</c> / <c>DLevelScript::GetPlayerInput</c>.</summary>
internal static class AcsPlayerInput
{
    public static int Query(AuthoritySimulation sim, Actor? activator, int playerNum, int inputNum)
    {
        PlayerPawn? player;
        if (playerNum < 0)
        {
            player = activator is PlayerPawn { Destroyed: false } pawn ? pawn : null;
        }
        else if (!sim.IsPlayerInGame(playerNum))
            return 0;
        else
            player = sim.Players.FirstOrDefault(candidate => !candidate.Destroyed && candidate.PlayerNum == playerNum);

        return player?.ReadPlayerInput(inputNum) ?? 0;
    }
}
