namespace HCDE.Net.Core;

/// <summary>Vote/toggle slice of <c>d_net.cpp</c> cutscene ready participants.</summary>
public sealed class CutsceneReadyCoordinator
{
    public const int ReadyToggleDebounceTics = 35 / 4;
    private const int MaxPlayers = 8;

    private ulong _readyMask;
    private readonly int[] _lastToggleTic = new int[MaxPlayers];

    public ulong ReadyMask => _readyMask;

    public void ClearReady() => _readyMask = 0;

    public bool IsParticipantReady(int player, bool voteMode) =>
        voteMode && player >= 0 && player < MaxPlayers && (_readyMask & (1ul << player)) != 0;

    public void ToggleReady(int player, int gameTic, int humanParticipants, bool voteMode)
    {
        if (!voteMode || player < 0 || player >= MaxPlayers)
            return;
        if (gameTic - _lastToggleTic[player] < ReadyToggleDebounceTics)
            return;
        _lastToggleTic[player] = gameTic;
        if (humanParticipants <= 1)
            _readyMask |= 1ul << player;
        else if ((_readyMask & (1ul << player)) != 0)
            _readyMask &= ~(1ul << player);
        else
            _readyMask |= 1ul << player;
    }
}
