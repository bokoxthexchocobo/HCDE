namespace HCDE.Playsim;

/// <summary>
/// `G_ProcessIFFDemo` tail, `G_DoPlayDemo`, `G_TimeDemo`, and `G_CheckDemoStatus` head from <c>g_game.cpp</c> (**3500–3649**).
/// </summary>
public static class VmGGameDemoPlayback1008Slice
{
    public const int GaLoadGame = 5;
    public const int GaLoadGamePlayDemo = 7;
    public const int GaPlayDemo = 11;
    public const int GaNothing = 0;
    public const int FormId = 0x464F524D;
    public const int ZLibOk = 0;
    public const int MinDemoFormBytes = 4;
    public const string DefaultDemoExtension = ".lmp";

    public enum DemoIffTailFailure
    {
        None,
        NoPlayers,
        NoBody,
    }

    public enum SingleDemoMessagePolicy
    {
        Silent,
        PrintPlaying,
    }

    public enum DemoStatusUserInfoPolicy
    {
        RestorePlayerInfo,
        Skip,
    }

    public static DemoIffTailFailure ResolveNoPlayersFailure(int numPlayers) =>
        numPlayers == 0 ? DemoIffTailFailure.NoPlayers : DemoIffTailFailure.None;

    public static DemoIffTailFailure ResolveNoBodyFailure(bool bodyHit) =>
        bodyHit ? DemoIffTailFailure.None : DemoIffTailFailure.NoBody;

    public static bool ShouldMarkNetgameFromDemoPlayerCount(int numPlayers) =>
        numPlayers > 1;

    public static bool ShouldDecompressDemoBody(int uncompSize) =>
        uncompSize > 0;

    public static bool DecompressDemoBodySucceeded(int zlibResult) =>
        zlibResult == ZLibOk;

    public static bool IsDemoLumpInFilesystem(int lumpIndex) =>
        lumpIndex >= 0;

    public static string AppendDemoExtensionForFileOpen(string path, string extension) =>
        path.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? path : path + extension;

    public static bool IsDemoBufferTooSmallForForm(int bufferSize) =>
        bufferSize < MinDemoFormBytes;

    public static SingleDemoMessagePolicy ResolveSingleDemoMessagePolicy(bool singleDemo) =>
        singleDemo ? SingleDemoMessagePolicy.PrintPlaying : SingleDemoMessagePolicy.Silent;

    public static bool IsNonZdoomFormDemo(int formId) =>
        formId != FormId;

    public static bool ShouldFatalErrorOnNonFormSingleDemo(bool singleDemo, int formId) =>
        singleDemo && formId != FormId;

    public static bool ShouldRestoreCvarsAfterIffFailure(bool iffParseFailed, bool cvarsWereBackedUp) =>
        iffParseFailed && cvarsWereBackedUp;

    public static bool ShouldInitNewLevelFromDemoMap(string? mapName) =>
        !string.IsNullOrEmpty(mapName);

    public static bool RequiresSavegameWhenMapNameEmpty(int sectorCount) =>
        sectorCount == 0;

    public static (bool Nodrawers, bool Noblit) ResolveTimeDemoDrawFlags(bool nodrawArg, bool noblitArg) =>
        (nodrawArg, noblitArg);

    public static int ResolveTimeDemoGameAction(int currentGameAction) =>
        currentGameAction == GaLoadGame ? GaLoadGamePlayDemo : GaPlayDemo;

    public static DemoStatusUserInfoPolicy ResolveDemoStatusUserInfoPolicy(bool demoRecording) =>
        demoRecording ? DemoStatusUserInfoPolicy.Skip : DemoStatusUserInfoPolicy.RestorePlayerInfo;

    public static bool ShouldComputeTimingDemoElapsed(bool demoPlayback, bool timingDemo) =>
        demoPlayback && timingDemo;
}
