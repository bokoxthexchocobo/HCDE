namespace HCDE.Playsim;

/// <summary>
/// Player log/subtitle/FOV/color/pain-flash/pitch-limit helpers from <c>p_user.cpp</c> (840–1146).
/// </summary>
public static class VmPUserLogFovPitch
{
    public const int GiShareware = 1;
    public const int DfNoFov = 1;
    public const int DemMyfov = 1;
    public const int DemSetpitchlimit = 2;
    public const int NoSound = -1;
    public const int Ticrate = 35;
    public const int NameNone = 0;

    public sealed class PlayerLogState
    {
        public string LogText { get; set; } = "";
        public string SubtitleText { get; set; } = "";
        public int SubtitleCounter { get; set; }
        public object? Mo { get; set; }
        public int CurrentPlayerClass { get; set; }
        public double MinPitch { get; set; }
        public double MaxPitch { get; set; }
    }

    public sealed class LogNumberContext
    {
        public Func<string, string?>? CheckString { get; init; }
        public Func<string, int>? CheckNumForName { get; init; }
        public Func<int, string>? GetResourceFileName { get; init; }
        public Func<int, string>? ReadLumpString { get; init; }
        public Action<string>? SetLogText { get; init; }
    }

    public static void SetLogNumber(LogNumberContext ctx, int num)
    {
        var stringKey = $"$TXT_LOGTEXT{num}";
        if (ctx.CheckString?.Invoke(stringKey[1..]) is { } text && text.Length > 0)
        {
            ctx.SetLogText?.Invoke(stringKey);
            return;
        }

        var lumpname = $"LOG{num}";
        var lumpnum = ctx.CheckNumForName?.Invoke(lumpname) ?? -1;
        if (lumpnum == -1)
            return;

        var wadname = ctx.GetResourceFileName?.Invoke(lumpnum) ?? "";
        if (IsStrifeIwad(wadname))
        {
            var ilogKey = $"$TXT_ILOG{num}";
            if (ctx.CheckString?.Invoke(ilogKey[1..]) is { } ilog && ilog.Length > 0)
            {
                ctx.SetLogText?.Invoke(ilogKey);
                return;
            }
        }

        var lump = ctx.ReadLumpString?.Invoke(lumpnum);
        if (lump is not null)
            ctx.SetLogText?.Invoke(lump);
    }

    static bool IsStrifeIwad(string wadname) =>
        string.Equals(wadname, "STRIFE0.WAD", StringComparison.OrdinalIgnoreCase)
        || string.Equals(wadname, "STRIFE1.WAD", StringComparison.OrdinalIgnoreCase)
        || string.Equals(wadname, "SVE.WAD", StringComparison.OrdinalIgnoreCase);

    public static void SetLogText(PlayerLogState player, string text, bool checkLocalView, Action<string>? printGold = null)
    {
        player.LogText = text;
        if (checkLocalView && printGold is not null)
        {
            var resolved = text.Length > 0 && text[0] == '$'
                ? text[1..]
                : text;
            printGold(resolved);
        }
    }

    public sealed class SubtitleContext
    {
        public int GameFlags { get; init; }
        public Func<string, string?>? GetLanguageString { get; init; }
        public Func<int, int>? GetSoundLengthMs { get; init; }
    }

    public static void SetSubtitle(PlayerLogState player, SubtitleContext ctx, int num, int soundid)
    {
        if ((ctx.GameFlags & GiShareware) != 0)
            return;

        var lumpname = $"$TXT_SUB_LOG{num}";
        var text = ctx.GetLanguageString?.Invoke(lumpname[1..]);
        if (text is null)
            return;

        player.SubtitleText = lumpname;
        var sl = soundid == NoSound ? 7000 : Math.Max(7000, ctx.GetSoundLengthMs?.Invoke(soundid) ?? 7000);
        player.SubtitleCounter = sl * Ticrate / 1000;
    }

    public static int GetSpawnClass(IReadOnlyList<object?> playerClasses, int currentClass, Func<object?, int>? spawnMaskReader)
    {
        var type = playerClasses[currentClass];
        return spawnMaskReader?.Invoke(type) ?? 0;
    }

    public readonly record struct FovWritePlan(bool WriteNetwork, bool PrintDisabled, float ClampedFov);

    public static FovWritePlan PlanSetFov(
        bool consolePlayerValid,
        int dmflags,
        bool isLocalServiceAuthority,
        float fov)
    {
        if (!consolePlayerValid)
            return new FovWritePlan(false, false, fov);

        if ((dmflags & DfNoFov) != 0 && !isLocalServiceAuthority)
            return new FovWritePlan(false, true, fov);

        return new FovWritePlan(true, (dmflags & DfNoFov) != 0 && isLocalServiceAuthority, Math.Clamp(fov, 5f, 179f));
    }

    public sealed record ColorSetEntry(object Class, int SetNum, bool IsNamed, int NameIndex = 1);

    public static Func<object?, object?, bool>? IsAncestorOfFunc { get; set; }

    public static void EnumColorSets(IReadOnlyList<ColorSetEntry> colorSets, object cls, IList<int> outList)
    {
        var deleteds = new List<int>();
        outList.Clear();

        for (var i = colorSets.Count - 1; i >= 0; i--)
        {
            var entry = colorSets[i];
            if (!IsAncestorOf(entry.Class, cls))
                continue;

            var v = entry.SetNum;
            if (!outList.Contains(v) && !deleteds.Contains(v))
            {
                if (!entry.IsNamed)
                    deleteds.Add(v);
                else
                    outList.Add(v);
            }
        }

        var sorted = outList.OrderBy(x => x).ToList();
        outList.Clear();
        foreach (var v in sorted)
            outList.Add(v);
    }

    public static bool IsAncestorOf(object? registered, object? cls) =>
        IsAncestorOfFunc?.Invoke(registered, cls)
        ?? (registered is not null && cls is not null && ReferenceEquals(registered, cls));

    public static ColorSetEntry? GetColorSet(IReadOnlyList<ColorSetEntry> colorSets, object cls, int setnum)
    {
        for (var i = colorSets.Count - 1; i >= 0; i--)
        {
            var entry = colorSets[i];
            if (entry.SetNum == setnum && IsAncestorOf(entry.Class, cls))
                return entry.IsNamed ? entry : null;
        }

        return null;
    }

    public static int GetColorSetNameIndex(ColorSetEntry? entry) => entry is null ? 0 : entry.NameIndex;

    public sealed record PainFlashEntry(object Class, int Type, int Color);

    public static int GetPainFlash(IReadOnlyList<PainFlashEntry> flashes, object actorClass, int type)
    {
        for (var i = flashes.Count - 1; i >= 0; i--)
        {
            var entry = flashes[i];
            if (entry.Type == type && IsAncestorOf(entry.Class, actorClass))
                return entry.Color;
        }

        return 0;
    }

    public static int GetSoftPitch(bool down, float maxviewpitch, bool clOldFreelookLimit)
    {
        var maxDn = Math.Min(56, (int)maxviewpitch);
        var maxUp = Math.Min(32, (int)maxviewpitch);
        return down ? maxDn : clOldFreelookLimit ? maxUp : maxDn;
    }

    public readonly record struct PitchLimitPlan(int UpPitch, int DownPitch, bool ApplyLocal);

    public static PitchLimitPlan PlanSendPitchLimits(
        bool isConsolePlayer,
        bool hardwareRenderer,
        float maxviewpitch,
        bool clOldFreelookLimit)
    {
        if (!isConsolePlayer)
            return new PitchLimitPlan(0, 0, false);

        int uppitch;
        int downpitch;
        if (!hardwareRenderer)
        {
            uppitch = GetSoftPitch(false, maxviewpitch, clOldFreelookLimit);
            downpitch = GetSoftPitch(true, maxviewpitch, clOldFreelookLimit);
        }
        else
        {
            uppitch = downpitch = (int)maxviewpitch;
        }

        return new PitchLimitPlan(uppitch, downpitch, true);
    }

    public static void ApplyPitchLimits(PlayerLogState player, PitchLimitPlan plan)
    {
        if (!plan.ApplyLocal)
            return;

        player.MinPitch = -(double)(sbyte)(byte)plan.UpPitch;
        player.MaxPitch = (double)(sbyte)(byte)plan.DownPitch;
    }

    public static int SetLogNumberAction(LogNumberContext ctx, int log) { SetLogNumber(ctx, log); return 0; }
    public static int SetLogTextAction(PlayerLogState self, string log, bool localView) { SetLogText(self, log, localView); return 0; }
    public static int SetSubtitleNumberAction(PlayerLogState self, SubtitleContext ctx, int log, int soundid) { SetSubtitle(self, ctx, log, soundid); return 0; }
    public static int SetFovAction(FovWritePlan plan) => plan.WriteNetwork ? 0 : 0;
    public static void EnumColorSetsAction(IReadOnlyList<ColorSetEntry> colorSets, object cls, IList<int> outList) => EnumColorSets(colorSets, cls, outList);
    public static int GetColorSetNameAction(ColorSetEntry? entry) => GetColorSetNameIndex(entry);
    public static int GetPainFlashForTypeAction(IReadOnlyList<PainFlashEntry> flashes, object actorClass, int type) => GetPainFlash(flashes, actorClass, type);
    public static void SendPitchLimitsAction(PlayerLogState self, PitchLimitPlan plan) => ApplyPitchLimits(self, plan);
}
