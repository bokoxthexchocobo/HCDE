namespace HCDE.Playsim;

/// <summary><c>QueueBody</c> from <c>g_game.cpp</c> (2307–2357).</summary>
public static class VmGGameQueueBody
{
    public const int SvCorpseFilterPlayers = 2;
    public const int DefaultBodyQueueSize = 32;

    public static int ResolveBodyLimit(int svCorpseFilter, int svCorpseQueueSize) =>
        (svCorpseFilter & SvCorpseFilterPlayers) != 0
            ? Math.Clamp(svCorpseQueueSize, 0, DefaultBodyQueueSize)
            : DefaultBodyQueueSize;

    public static bool ShouldDestroyImmediately(int bodyLimit) => bodyLimit <= 0;

    public static int NextBodySlot(int bodyQueueSlot, int bodyLimit) => bodyQueueSlot % bodyLimit;

    public static bool ShouldDestroyPreviousCorpse(int bodyQueueSlot, int bodyLimit) =>
        bodyQueueSlot >= bodyLimit;

    public static bool ShouldCopyPlayerCorpseTranslation(int translationType, int playersType, int playersExtraType) =>
        translationType == playersType || translationType == playersExtraType;

    public static bool ShouldApplySkinScale(int skinIndex, bool noSkinFlag) =>
        skinIndex != 0 && !noSkinFlag;

    public static double ScaleSkinAxis(double bodyScale, double skinScale, double defaultScale) =>
        defaultScale == 0 ? bodyScale : bodyScale * (skinScale / defaultScale);

    public static int AdvanceQueueSlot(int currentSlot) => currentSlot + 1;

    public static bool UsesPlayerCorpseTranslationSlot(int modSlot) => modSlot >= 0;

    public static void ClearBodyPlayerReference() { /* native sets body->player = nullptr */ }
}

