namespace HCDE.Playsim;
/// <summary>
/// FindNestedMusicFile through StopChannel from s_doomsound.cpp 233-574.
/// Music path matching ends at line 232. Actor-pitched sound starts at line 575.
/// </summary>
public static class VmDoomSound1424Slice
{
    public const int BandStart = 233;
    public const int BandEnd = 574;
    public const int PathEnd = 232;
    public const int ActorSound = 575;
    public const int MusicNamespace = 16;
    public const int SourceUnattached = 0;
    public const int SourceNone = 1;
    public const int SourceActor = 2;
    public const int PlayerReserve = 1;
    public const int Euthanize = 32;
    public const int SectorSilent = 1;
    public const int MaybeLocal = 16;
    public const int SilentPickup = 8;
    public const int MagicSilence = 32;
    public const int Weapon = 1;
    public const int Evicted = 2;
    public sealed class Play
    {
        public int Source;
        public int Channel;
        public int Flags;
        public int Id;
        public float Volume;
        public float Attenuation;
        public float Pitch;
        public float Start;
        public float X;
        public float Y;
        public float Z;
        public bool Position;
    }
    public sealed class State
    {
        public List<string?> Files = new();
        public Dictionary<string, int> Names = new();
        public Dictionary<string, int> Full = new();
        public List<string> Traces = new();
        public bool HasEngine;
        public int Created;
        public bool ArgsMissing;
        public bool NoMusic;
        public bool NoSound;
        public bool MusicDisabled;
        public int SoundInits;
        public int MusicInits;
        public int VolumeCallbacks;
        public int CurveLump = -1;
        public byte[] Curve = Array.Empty<byte>();
        public byte[] CurveLoaded = Array.Empty<byte>();
        public int EngineInits;
        public bool Callbacks;
        public bool GSnd;
        public List<int>? Levels = new();
        public int Sequences;
        public int EngineShutdowns;
        public string LastSong = "old";
        public int SoundClosed;
        public bool MusicStopped;
        public bool LevelMissing;
        public bool HasInfo = true;
        public string SoundInfo = "";
        public string SndSeq = "";
        public string LastInfo = "";
        public string LastSeq = "";
        public int StoppedChannels;
        public int Unloaded;
        public int InfoParses;
        public bool InfoGlobal;
        public int AddedLocal = -2;
        public int SeqLump = -2;
        public bool SameLevel = true;
        public bool LevelNull;
        public List<bool> Actors = new();
        public int MarkedUnused;
        public int VirtualMarks;
        public List<int> GameSounds = new();
        public List<int>? MapSounds = new();
        public List<int> Used = new();
        public int CachedMarked;
        public List<Play> Plays = new();
        public int? SfxFlag;
        public int BaseCaches;
        public int Type = SourceActor;
        public object? Ent = "actor";
        public object? Skinned;
        public int BaseResolves;
        public bool EntNull;
        public bool Euthanize;
        public bool SectorNull;
        public bool SectorSilent;
        public int Flags;
        public int Compat;
        public bool Listener;
        public int Channel;
        public bool Verified;
        public object? Chan = "chan";
        public bool SysChannel = true;
        public int ChanFlags;
        public int SourceType = SourceActor;
        public object? Source = "actor";
        public int BaseStops;
    }
    public static int FindNested(State s, string? music)
    {
        for (int lump = s.Files.Count - 1; lump >= 0; lump--)
        {
            var full = s.Files[lump];
            if (HasMusic(full) && Matches(full, music))
            {
                s.Traces.Add($"music:FindMusic nested match '{music ?? "(null)"}' -> '{full ?? "(null)"}' lump={lump}");
                return lump;
            }
        }
        return -1;
    }
    public static int FindMusic(State s, string? music)
    {
        _ = music ?? throw new NullReferenceException();
        int lump = Get(s.Names, music);
        if (lump == -1) lump = FindNested(s, music);
        if (lump == -1) lump = Get(s.Full, music);
        return lump;
    }
    public static void Init(State s)
    {
        s.Traces.Add("sound:S_Init called");
        s.Callbacks = true;
        if (!s.HasEngine)
        {
            s.HasEngine = true;
            s.Created++;
        }
        s.SoundInits++;
        if (s.ArgsMissing) throw new NullReferenceException();
        s.MusicDisabled = s.NoMusic || s.NoSound;
        s.MusicInits++;
        s.VolumeCallbacks++;
        s.CurveLoaded = s.CurveLump >= 0 ? s.Curve.ToArray() : Array.Empty<byte>();
        s.EngineInits++;
    }
    public static void Shutdown(State s)
    {
        s.MusicStopped = true;
        if (s.Levels == null) throw new NullReferenceException();
        s.Sequences += s.Levels.Count;
        if (s.HasEngine)
        {
            s.EngineShutdowns++;
            s.HasEngine = false;
        }
        s.LastSong = "";
        if (s.GSnd) s.SoundClosed++;
    }
    public static void Start(State s)
    {
        if (!(s.GSnd && s.HasEngine)) return;
        s.StoppedChannels++;
        if (s.LevelMissing) throw new NullReferenceException();
        var info = "";
        var seq = "";
        if (s.HasInfo)
        {
            info = s.SoundInfo;
            seq = s.SndSeq;
        }
        var parse = false;
        if (!Same(s.LastInfo, info) || !s.HasInfo)
        {
            s.Unloaded++;
            s.InfoParses++;
            s.InfoGlobal = true;
            if (info.Length > 0)
            {
                var lump = Get(s.Full, info);
                if (lump >= 0) s.AddedLocal = lump;
            }
            parse = true;
        }
        else if (!Same(s.LastSeq, seq)) parse = true;
        if (parse) s.SeqLump = seq.Length > 0 ? Get(s.Full, seq) : -1;
        s.LastInfo = info;
        s.LastSeq = seq;
    }
    public static void Precache(State s)
    {
        if (!(s.GSnd && s.SameLevel)) return;
        s.MarkedUnused++;
        if (s.LevelNull) throw new NullReferenceException();
        foreach (var virtualMark in s.Actors)
        {
            if (virtualMark) s.VirtualMarks++;
        }
        foreach (var sound in s.GameSounds) s.Used.Add(sound);
        if (s.MapSounds == null) throw new NullReferenceException();
        foreach (var sound in s.MapSounds) s.Used.Add(sound);
        s.CachedMarked++;
    }
    public static void InitData(State s)
    {
        s.LastInfo = s.LastSeq = "";
        s.InfoParses++;
        s.InfoGlobal = false;
        s.SeqLump = -1;
    }
    public static void SoundPitch(State s, int channel, int flags, int id, float volume, float attenuation, float pitch, float start)
    {
        if (!s.HasEngine) throw new NullReferenceException();
        s.Plays.Add(new Play
        {
            Source = SourceNone,
            Channel = channel,
            Flags = flags,
            Id = id,
            Volume = volume,
            Attenuation = attenuation,
            Pitch = pitch,
            Start = start,
        });
    }
    public static void Sound(State s, int channel, int flags, int id, float volume, float attenuation)
    {
        if (!s.HasEngine) throw new NullReferenceException();
        s.Plays.Add(new Play
        {
            Source = SourceNone,
            Channel = channel,
            Flags = flags,
            Id = id,
            Volume = volume,
            Attenuation = attenuation,
        });
    }
    public static int ActionSound(State s, int id, int channel, double volume, double attenuation, double pitch, double start)
    {
        SoundPitch(s, channel & 7, channel & ~7, id, (float)volume, (float)attenuation, (float)pitch, (float)start);
        return 0;
    }
    public static int ActionStart(State s, int id, int channel, int flags, double volume, double attenuation, double pitch, double start)
    {
        SoundPitch(s, channel, flags, id, (float)volume, (float)attenuation, (float)pitch, (float)start);
        return 0;
    }
    public static void StartAt(State s, double x, double y, double z, int id, int channel, int flags, double volume, double attenuation, double pitch, double start)
    {
        if (!s.HasEngine) throw new NullReferenceException();
        s.Plays.Add(new Play
        {
            Source = SourceUnattached,
            Channel = channel,
            Flags = flags,
            Id = id,
            Volume = (float)volume,
            Attenuation = (float)attenuation,
            Pitch = (float)pitch,
            Start = (float)start,
            X = (float)x,
            Z = (float)z,
            Y = (float)y,
            Position = true,
        });
    }
    public static int ActionStartAt(State s, double x, double y, double z, int id, int channel, int flags, double volume, double attenuation, double pitch, double start)
    {
        StartAt(s, x, y, z, id, channel, flags, volume, attenuation, pitch, start);
        return 0;
    }
    public static void Cache(State s)
    {
        if (s.SfxFlag == null) throw new NullReferenceException();
        if ((s.SfxFlag.Value & PlayerReserve) == 0) s.BaseCaches++;
    }
    public static int Resolve(State s, int id)
    {
        if (s.SfxFlag == null) throw new NullReferenceException();
        if ((s.SfxFlag.Value & PlayerReserve) != 0)
        {
            s.Skinned = s.Type == SourceActor ? s.Ent : null;
            return id;
        }
        s.BaseResolves++;
        return id;
    }
    public static bool Verify(State s)
    {
        if (s.EntNull || s.Euthanize) return false;
        if (s.SectorNull) throw new NullReferenceException();
        if (s.SectorSilent || !s.SameLevel) return false;
        if ((s.Flags & MaybeLocal) != 0 && (s.Compat & SilentPickup) != 0 && !s.Listener) return false;
        if ((s.Compat & MagicSilence) != 0) s.Channel = Weapon;
        return true;
    }
    public static void StopChannel(State s)
    {
        if (s.Chan != null && s.SysChannel && (s.ChanFlags & Evicted) == 0 && s.SourceType == SourceActor)
            s.Source = null;
        s.BaseStops++;
    }
    static int Get(Dictionary<string, int> map, string name) => map.TryGetValue(name, out var lump) ? lump : -1;
    static bool Same(string left, string right) => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    static bool HasMusic(string? path)
    {
        var segment = path;
        while (!string.IsNullOrEmpty(segment))
        {
            var slash = segment.IndexOfAny(['/', '\\']);
            var length = slash >= 0 ? slash : segment.Length;
            if (length == 5 && segment.AsSpan(0, length).Equals("music".AsSpan(), StringComparison.OrdinalIgnoreCase)) return true;
            segment = slash >= 0 ? segment[(slash + 1)..] : "";
        }
        return false;
    }
    static bool Matches(string? full, string? music)
    {
        if (string.IsNullOrEmpty(full) || string.IsNullOrEmpty(music)) return false;
        var slash = full.LastIndexOfAny(['/', '\\']);
        var baseName = slash >= 0 ? full[(slash + 1)..] : full;
        var dot = baseName.LastIndexOf('.');
        if (dot <= 0 || dot == baseName.Length - 1) return false;
        var ext = baseName[(dot + 1)..];
        if (!Known(ext)) return false;
        var stem = baseName[..dot];
        return stem.Length == music.Length && string.Equals(stem, music, StringComparison.OrdinalIgnoreCase);
    }
    static bool Known(string ext)
    {
        foreach (var name in new[] { "ogg", "mid", "midi", "mus", "mp3", "flac", "wav", "it", "xm", "s3m", "mod" })
        {
            if (string.Equals(name, ext, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }
}
