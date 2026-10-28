namespace HCDE.Playsim;

/// <summary>Native <c>G_Ticker</c> game-action and player lifecycle slice for headless authority.</summary>
public static class HeadlessGameFlow
{
    public static void ProcessPlayerLifecycle(AuthoritySimulation sim, IReadOnlyDictionary<byte, HeadlessPlayerState> states)
    {
        foreach (var player in sim.Players)
        {
            if (player.Destroyed)
                continue;
            var state = states.TryGetValue(player.PlayerNum, out var tracked) ? tracked : HeadlessPlayerState.Live;
            if (state == HeadlessPlayerState.Enter || state == HeadlessPlayerState.Reborn)
                sim.TryPlayerRespawn(player);
        }
    }

    /// <summary>Native <c>G_DoReborn</c> reload path for single-player map restart.</summary>
    public static bool TryConsumeReloadRequest(HeadlessGameSession session)
    {
        var sim = session.Simulation;
        if (sim is null || !sim.ConsumeReloadRequest())
            return false;
        session.InitNew(session.CurrentMapName, sim.RngSeed + 1, newGame: false);
        return true;
    }

    public static void ApplyTicCommand(AuthoritySimulation sim, byte playerNum, PlayerCommand command) =>
        sim.QueueCommand(playerNum, command);

    public static PlayerCommand BuildMovementCommand(short forward, short side, short yaw = 0, bool use = false) =>
        new() { ForwardMove = forward, SideMove = side, YawDelta = yaw, Use = use };
}
