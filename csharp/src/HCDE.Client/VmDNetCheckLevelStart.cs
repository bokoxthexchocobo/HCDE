namespace HCDE.Client;

/// <summary><c>CheckLevelStart</c> from <c>d_net.cpp</c> (5641–5792).</summary>
public static class VmDNetCheckLevelStart
{
    public const int LstWaiting = 0;

    public static bool ShouldResendLevelReady(int status, bool localAuthority, bool isConsole) =>
        status != LstWaiting && localAuthority && !isConsole;

    public static int LevelReadyPacketSize() => 8;

    public static bool IsAuthoritySlot(bool authoritySlot) => authoritySlot;

    public static int ParseGameticFromBuffer(byte b4, byte b5, byte b6, byte b7) =>
        (b4 << 24) | (b5 << 16) | (b6 << 8) | b7;

    public static bool ShouldAlignGametic(int local, int server) => local != server;

    public static bool LateJoinSeat(bool dedicated, bool localAuthority, int gametic, int clientTic) =>
        dedicated && !localAuthority && gametic > clientTic;

    public static int SeatTicDup(int ticDup) => Math.Max(ticDup, 1);

    public static int ClampDesiredLead(int lead) => Math.Clamp(lead, 0, 8);

    public static int SeatClientTic(int gametic, int lead, int ticDup) => gametic + lead * ticDup;

    public static int SeatSequence(int clientTic, int ticDup) => clientTic / ticDup;

    public static int MaxSequence(int current, int seat) => Math.Max(current, seat);

    public static bool ShouldClampPredictionLead(bool dedicated, bool localAuthority) =>
        dedicated && !localAuthority;

    public static int MaxClientTic(int gametic, int desiredLead, int ticDup) =>
        gametic + desiredLead * ticDup;

    public static bool ClientTicAboveMax(int clientTic, int maxClientTic) => clientTic > maxClientTic;

    public static int PlayTic(int gametic, int ticDup) => Math.Max(gametic / ticDup, 0);

    public static int AppliedSequenceAnchor(int currentSequence, int playTic) =>
        Math.Min(currentSequence, playTic);

    public static ulong AckClient(int client) => 1ul << client;

    public static bool WaitingForStart(int status) => status == LstWaiting;

    public static bool ClearLevelStartAckOnAuthority() => true;

    public static int LevelStartDelayFromTics(int delay) => delay;

    public static bool DebugLevelStartDelay(int delay) => delay >= 0;

    public static bool ResetAuthorityWatchdog(string reason) => !string.IsNullOrEmpty(reason);

    public static bool LogAuthorityStart(int gametic, int clientTic) => gametic >= 0 && clientTic >= 0;

    public static bool TryReleaseAfterAck(ulong ack, ulong mask, bool mapLoaded) =>
        mapLoaded && ack != 0;

    public static int ConsolePlayerInRange(int consoleplayer, int maxPlayers) =>
        consoleplayer >= 0 && consoleplayer < maxPlayers ? consoleplayer : -1;

    public static bool ResendUsesRoomId(byte roomId) => true;

    public static byte NcmdLevelReady() => 0;

    public static bool GameticSnapSafeOnMapChange(bool dedicated, bool authority) =>
        !dedicated || authority;

    public static bool PreserveSequenceOnMapChange(int clientTic, int gametic) =>
        clientTic >= gametic;

    public static bool ClearPredictionAfterSeatJump(bool jumped) => jumped;

    public static bool ClearPredictionAfterLeadClamp(bool clamped) => clamped;

    public static bool AppliedSequenceDiagnosticFix(bool dedicated, bool authority) =>
        dedicated && !authority;
}
