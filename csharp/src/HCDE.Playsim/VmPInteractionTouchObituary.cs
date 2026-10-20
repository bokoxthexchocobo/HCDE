namespace HCDE.Playsim;

/// <summary><c>P_TouchSpecialThing</c>, <c>PronounMessage</c>, and <c>ClientObituary</c> from <c>p_interaction.cpp</c> (85–328).</summary>
public static class VmPInteractionTouchObituary
{
    public const int GenderMax = 4;

    public static bool PickupOutOfReach(double delta, double toucherHeight, double specialHeight) =>
        delta > toucherHeight || delta < Math.Min(-32.0, -specialHeight);

    public static bool DeadToucherSkips(int health) => health <= 0;

    public static bool BotDestCleared(bool hasBot, bool destMatch) => hasBot && destMatch;

    public static bool ShouldShowObituary(bool hasPlayer, bool moIsSelf, bool showObituaries) =>
        hasPlayer && moIsSelf && showObituaries;

    public static bool VoodooDollInflictor(bool inflictorPlayer, bool inflictorMoMatch) =>
        inflictorPlayer && !inflictorMoMatch;

    public static bool AnnounceTelefragBlocks(bool telefrag, bool announced) => telefrag && announced;

    public static bool AnnounceKillBlocks(bool notTelefrag, bool announced) => notTelefrag && announced;

    public static bool UseDamageTypeObituary(bool attackerNull, bool obitNotEmpty) => attackerNull && obitNotEmpty;

    public static bool SuicideMessage(int modIndex, int suicideIndex) => modIndex == suicideIndex;

    public static bool FallbackMessagename(bool messageNull, bool messagenameSet) =>
        messageNull && messagenameSet;

    public static bool DollarStringLookup(char first) => first == '$';

    public static bool SynthesizeMeleeObit(bool meleeMod, bool found) => meleeMod && found;

    public static bool SynthesizeDefaultObit(bool found) => !found;

    public static bool SkipEmptyMessage(bool messageNull, int len) => messageNull || len <= 0;

    public static bool KillfeedHook(bool rKillfeed) => rKillfeed;

    public static int PronounCaseFromMarker(char marker) => marker switch
    {
        'g' => 0,
        'h' => 1,
        'p' => 2,
        's' => 3,
        'r' => 4,
        _ => -1,
    };

    public static bool CopyLiteralPercent(char current, char next) => current == '%' && next != '%';

    public static bool SubstituteVictimOrKiller(char marker) => marker is 'o' or 'k';

    public static string ExpandPronoun(int gender, int grammarCase)
    {
        ReadOnlySpan<string> row = gender switch
        {
            0 => ["he", "him", "his", "his", "he's"],
            1 => ["she", "her", "her", "hers", "she's"],
            2 => ["they", "them", "their", "theirs", "they're"],
            _ => ["it", "it", "its", "its'", "it's"],
        };
        return grammarCase >= 0 && grammarCase < row.Length ? row[grammarCase] : string.Empty;
    }

    public static bool SelfKillObituary(bool attackerIsSelf) => attackerIsSelf;

    public static bool ClassModLookup(bool attackerNotSelf, bool stringFound) => attackerNotSelf && stringFound;

    public static bool AttackerNullUseSelf(bool attackerNull) => attackerNull;

    public static bool NonPlayerAttackerForMessage(bool attackerPlayer) => !attackerPlayer;

    public static bool FallingObit(int modIndex, int fallingIndex) => modIndex == fallingIndex;

    public static bool CrushObit(int modIndex, int crushIndex) => modIndex == crushIndex;

    public static bool ExitObit(int modIndex, int exitIndex) => modIndex == exitIndex;

    public static bool WaterObit(int modIndex, int drowningIndex) => modIndex == drowningIndex;

    public static bool SlimeObit(int modIndex, int slimeIndex) => modIndex == slimeIndex;

    public static bool LavaObit(int modIndex, int fireIndex) => modIndex == fireIndex;

    public static bool VoodooObituaryInflictor(bool inflictorPlayer, bool moMismatch) =>
        inflictorPlayer && moMismatch;

    public static bool GetSelfObituaryVirtual(bool selfKill, bool retNotEmpty) => selfKill && retNotEmpty;

    public static bool GetObituaryVirtual(bool notSelf, bool retNotEmpty) => notSelf && retNotEmpty;

    public static bool LookupClassModString(bool found) => found;

    public static bool LookupClassString(bool found) => found;

    public static bool ResolveGenderedString(bool dollarPrefix, bool resolved) => dollarPrefix && resolved;

    public static bool MeleeDefHitObit(bool melee, bool resolved) => melee && resolved;

    public static bool DefObFallback(bool resolved) => !resolved;

    public static bool DefaultObString(bool stillNull) => stillNull;

    public static int KillfeedTic(bool hook, int levelTime) => hook ? levelTime : 0;

    public static bool PronounShiftLength(int gender, int grammarCase) =>
        gender >= 0 && gender < GenderMax && grammarCase >= 0 && grammarCase < 5;

    public static bool LiteralCopy(char ch) => ch != '%';

    public static bool EscapedPercent(char current, char next) => current == '%' && next == '%';

    public static bool UnknownGrammarCopiesPercent(char current, int grammarCase) =>
        current == '%' && grammarCase < 0;
}
