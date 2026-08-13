using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public static class SnapshotPlayerMask
{
    public static ulong Build(IReadOnlyList<ServerSnapshotPlayerRecord> players)
    {
        ulong mask = 0;
        foreach (var player in players)
            mask |= 1UL << player.PlayerNum;

        return mask;
    }

    public static bool Contains(ulong mask, byte playerNum) =>
        playerNum < NetConstants.MaxPlayers && (mask & (1UL << playerNum)) != 0;
}
