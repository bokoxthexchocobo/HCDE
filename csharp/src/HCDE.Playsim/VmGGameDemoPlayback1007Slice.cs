namespace HCDE.Playsim;

/// <summary>
/// `G_DeferedPlayDemo`, `playdemo`/`timedemo` CCMDs, and `G_ProcessIFFDemo` head from <c>g_game.cpp</c> (**3350–3499**).
/// </summary>
public static class VmGGameDemoPlayback1007Slice
{
    public const int GaLoadGame = 5;
    public const int GaLoadGamePlayDemo = 7;
    public const int GaPlayDemo = 11;
    public const int MaxPlayers = 8;
    public const int MinDemoVersion = 0x221;
    public const int DemoGameVersion = 0x221;
    public const int LegacyMapNameDemoVersion = 0x21a;

    public enum PlayDemoRejectReason
    {
        None,
        NetGame,
        DemoRecording,
    }

    public enum DemoIffParseFailure
    {
        None,
        NotZdemFile,
        MangledChunk,
        RequiresNewerEngine,
        MissingHeader,
    }

    public static int ResolveDeferredPlayDemoGameAction(int currentGameAction) =>
        currentGameAction == GaLoadGame ? GaLoadGamePlayDemo : GaPlayDemo;

    public static PlayDemoRejectReason ResolvePlayDemoRejectReason(bool netGame, bool demoRecording)
    {
        if (netGame)
            return PlayDemoRejectReason.NetGame;
        if (demoRecording)
            return PlayDemoRejectReason.DemoRecording;
        return PlayDemoRejectReason.None;
    }

    public static bool HasDemoFilenameArgument(int argc) => argc > 1;

    public static long ComputeZdemFormEndOffset(long streamDataOffset, int formLength) =>
        streamDataOffset + formLength + (formLength & 1);

    public static DemoIffParseFailure ResolveZdemMagicFailure(int chunkId, int expectedZdemId) =>
        chunkId != expectedZdemId ? DemoIffParseFailure.NotZdemFile : DemoIffParseFailure.None;

    public static bool IsDemoChunkLayoutMangled(long nextChunkOffset, long formEndOffset) =>
        nextChunkOffset > formEndOffset;

    public static bool RequiresNewerEngineForDemo(int minimumVersionField, int engineDemoVersion) =>
        minimumVersionField > engineDemoVersion;

    public static bool UsesStringMapNameFromDemoHeader(int demoVersion) =>
        demoVersion >= LegacyMapNameDemoVersion;

    public static int LegacyFixedMapNameByteCount() => 8;

    public static bool ShouldClearRngForDemoMapStart(string? mapName) =>
        !string.IsNullOrEmpty(mapName);

    public static bool ShouldRegisterDemoPlayer(ref int numPlayers, bool alreadyInGame)
    {
        if (alreadyInGame)
            return false;
        numPlayers++;
        return true;
    }

    public static bool ActivatesMultiplayerFromNetdemoChunk(int chunkId, int netdemoChunkId) =>
        chunkId == netdemoChunkId;

    public static bool IsDemoBodyChunk(int chunkId, int bodyChunkId) =>
        chunkId == bodyChunkId;

    public static DemoIffParseFailure ResolveMissingDemoHeaderFailure(bool headerSeen) =>
        headerSeen ? DemoIffParseFailure.None : DemoIffParseFailure.MissingHeader;
}
