namespace HCDE.Net.Core;

/// <summary>Net-game CCMD guard mirrors (<c>d_net.cpp</c>).</summary>
public static class VmDNetNetGameCcmd
{
    public static bool RejectWithoutNetgameNative(bool netgame) => !netgame;

    public static bool RejectWithoutMultiplayerNative(bool multiplayer) => !multiplayer;

    public static bool RejectWithoutAuthorityNative(bool authority) => !authority;

    public static bool KickArgcValidNative(int argc) => argc >= 2;

    public static bool PlayerNumberValidNative(int player, int maxPlayers) =>
        player >= 0 && player < maxPlayers;

    public static bool ShouldKickClientNative(int target, int consolePlayer, bool inGame) =>
        target != consolePlayer && inGame;

    public static ulong MuteClientMaskNative(int player) => 1ul << player;

    public static bool IsClientMutedNative(ulong mutedClients, int player) =>
        (mutedClients & (1ul << player)) != 0;
}
