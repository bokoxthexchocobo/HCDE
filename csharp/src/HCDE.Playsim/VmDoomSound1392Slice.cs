namespace HCDE.Playsim;

/// <summary>
/// Doom sound engine declaration and <c>LookupMusic</c> from
/// <c>s_doomsound.cpp</c> 1-180. Chunk 1392.
/// <c>IsKnownMusicExtension</c> starts at line 181 and stays outside this band.
/// </summary>
public static class VmDoomSound1392Slice
{
    public const int SourceAny = -1;
    public const int SourceUnattached = 0;
    public const int SourceNone = 1;
    public const int SourceActor = 2;
    public const int SourceSector = 3;
    public const int SourcePolyobj = 4;
    public const int GsLevel = 0;
    public const int GsIntermission = 1;
    public const int GsTitleLevel = 7;
    public const int NameNone = 0;
    public const string TextColorRed = "\u001cG";
    public const string CdWarning = TextColorRed + "CD Audio no longer supported\n";
    public const string BaseClass = "SoundEngine";
    public const string HapticsDoAction = "haptics_do_action";
    public const string AddLocalSndInfo = "void S_AddLocalSndInfo(int lump)";

    public static string LastLocalSndInfo = "";
    public static string LastLocalSndSeq = "";
    public static bool CdAudioWarned;

    public readonly record struct MethodSpec(string Name, string Signature, string Access);

    public static readonly MethodSpec[] Methods =
    {
        new("CalcPosVel", "void CalcPosVel(int type, const void* source, const float pt[3], int channum, int chanflags, FSoundID soundid, FVector3* pos, FVector3* vel, FSoundChan *) override", "private"),
        new("ValidatePosVel", "bool ValidatePosVel(int sourcetype, const void* source, const FVector3& pos, const FVector3& vel)", "private"),
        new("ReadSound", "TArray<uint8_t> ReadSound(int lumpnum)", "private"),
        new("PickReplacement", "FSoundID PickReplacement(FSoundID refid)", "private"),
        new("ResolveSound", "FSoundID ResolveSound(const void *ent, int type, FSoundID soundid, float &attenuation) override", "private"),
        new("CacheSound", "void CacheSound(sfxinfo_t* sfx) override", "private"),
        new("StopChannel", "void StopChannel(FSoundChan* chan) override", "private"),
        new("AddSoundLump", "FSoundID AddSoundLump(const char* logicalname, int lump, int CurrentPitchMask, int resid = -1, int nearlimit = 2) override", "private"),
        new("CheckSoundLimit", "bool CheckSoundLimit(sfxinfo_t* sfx, const FVector3& pos, int near_limit, float limit_range, int sourcetype, const void* actor, int channel, float attenuation) override", "private"),
        new("DoomSoundEngine", "DoomSoundEngine() = default", "public"),
        new("NoiseDebug", "void NoiseDebug(void)", "public"),
        new("PrintSoundList", "void PrintSoundList()", "public"),
    };

    public static MethodSpec Method(string name)
    {
        foreach (var spec in Methods)
        {
            if (spec.Name == name)
                return spec;
        }
        throw new InvalidOperationException(name);
    }

    public sealed class Level
    {
        public string Music = "";
        public int MusicOrder;
    }

    public sealed class MusicWorld
    {
        public int GameState;
        public Level? PrimaryLevel;
        public Dictionary<string, string> Strings = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Alias> Aliases = new(StringComparer.OrdinalIgnoreCase);
    }

    public readonly record struct Alias(int Index, string Chars);

    public readonly record struct TraceMark(string Channel, string Text);

    public readonly record struct LumpForward(string? LogicalName, int Lump, int CurrentPitchMask, int ResId, int NearLimit);

    public readonly record struct LimitForward(
        object? Sfx,
        object? Pos,
        int NearLimit,
        float LimitRange,
        int SourceType,
        object? Actor,
        int Channel,
        float Attenuation);

    public sealed class SfxEntry
    {
        public List<int> UserData { get; } = new();
    }

    public static int AddSoundLump(
        string? logicalName,
        int lump,
        int currentPitchMask,
        Func<LumpForward, int> baseAdd,
        IList<SfxEntry> sfx,
        int resId = -1,
        int nearLimit = 2)
    {
        if (baseAdd == null)
            throw new ArgumentNullException(nameof(baseAdd));
        if (sfx == null)
            throw new ArgumentNullException(nameof(sfx));
        var forward = new LumpForward(logicalName, lump, currentPitchMask, resId, nearLimit);
        int index = baseAdd(forward);
        SfxEntry entry = sfx[index];
        List<int> user = entry.UserData;
        if (user.Count == 0)
            user.Add(0);
        else
        {
            if (user.Count > 1)
                user.RemoveRange(1, user.Count - 1);
            user[0] = 0;
        }
        return index;
    }

    public static bool CheckSoundLimit(
        object? sfx,
        object? pos,
        int nearLimit,
        float limitRange,
        int sourceType,
        object? actor,
        int channel,
        float attenuation,
        Func<LimitForward, bool> baseCheck)
    {
        if (baseCheck == null)
            throw new ArgumentNullException(nameof(baseCheck));
        if (sourceType != SourceActor)
            actor = null;
        return baseCheck(new LimitForward(sfx, pos, nearLimit, limitRange, sourceType, actor, channel, attenuation));
    }

    public static string LookupMusic(string? musicName, ref int order, MusicWorld world, List<string> prints, List<TraceMark> traces)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        if (prints == null)
            throw new ArgumentNullException(nameof(prints));
        if (traces == null)
            throw new ArgumentNullException(nameof(traces));
        string? original = musicName;
        if (musicName != null && musicName == "*")
        {
            if (world.GameState == GsLevel || world.GameState == GsTitleLevel)
            {
                Level level = world.PrimaryLevel ?? throw new NullReferenceException(nameof(world.PrimaryLevel));
                musicName = level.Music;
                order = level.MusicOrder;
            }
            else
            {
                musicName = null;
            }
        }
        if (string.IsNullOrEmpty(musicName))
            return "";
        if (musicName.Length >= 4 && musicName.AsSpan(0, 4).Equals(",CD,", StringComparison.OrdinalIgnoreCase))
        {
            if (!CdAudioWarned)
                prints.Add(CdWarning);
            CdAudioWarned = true;
            return "";
        }
        if (musicName[0] == '/')
            musicName = musicName[1..];
        if (musicName.Length > 0 && musicName[0] == '$')
        {
            string? mus = CheckString(world, musicName[1..]);
            if (mus != null)
                musicName = "D_" + mus;
        }
        if (world.Aliases.TryGetValue(musicName, out Alias alias))
        {
            if (alias.Index == NameNone)
            {
                order = -1;
                traces.Add(new TraceMark("music", "LookupMusic '" + Display(original) + "' -> '' (alias suppressed)"));
                return "";
            }
            musicName = alias.Chars;
        }
        traces.Add(new TraceMark("music", "LookupMusic in='" + Display(original) + "' out='" + musicName + "' order=" + order.ToString()));
        return musicName;
    }

    private static string? CheckString(MusicWorld world, string name)
    {
        if (name.Length == 0 || name.Equals("none", StringComparison.OrdinalIgnoreCase))
            return null;
        if (!world.Strings.TryGetValue(name, out string? value))
            return null;
        return value;
    }

    private static string Display(string? text)
    {
        if (text == null)
            return "(null)";
        return text;
    }
}
