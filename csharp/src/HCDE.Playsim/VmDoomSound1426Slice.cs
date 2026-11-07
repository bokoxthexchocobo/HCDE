namespace HCDE.Playsim;
/// <summary>
/// S_SetListener through S_SerializeSounds from s_doomsound.cpp 852-1014.
/// Playing queries end at line 851. Sector sound origins start at line 1016.
/// </summary>
public static class VmDoomSound1426Slice
{
    public const int BandStart = 852;
    public const int BandEnd = 1014;
    public const int PlayingEnd = 851;
    public const int SectorOrigin = 1016;
    public const int SourceUnattached = 0;
    public const int SourceNone = 1;
    public const int SourceActor = 2;
    public const int SourceSector = 3;
    public const int SourcePoly = 4;
    public const int Evicted = 2;
    public const int AbsTime = 1024;
    public const int OldSave = 4560;
    public const string Unknown = "Unknown sound source type ";
    public sealed class Fatal(string message) : Exception(message);
    public sealed class Heard
    {
        public float Angle;
        public float X;
        public float Y;
        public float Z;
        public bool Underwater;
        public object? Environment;
        public bool Valid;
        public object? Actor;
        public bool VelocityCleared;
    }
    public sealed class Chan
    {
        public int SourceType = SourceNone;
        public int SoundId;
        public int OrgId;
        public float Volume;
        public float Distance;
        public float Pitch;
        public int Flags;
        public int EntChannel;
        public int Priority;
        public float Near;
        public ulong Start;
        public int Rolloff;
        public float Min;
        public float Max;
        public int Limit;
        public object? Source;
        public float[]? Point;
    }
    public sealed class State
    {
        public object? Actor;
        public double Yaw;
        public float X;
        public float Y;
        public float Z;
        public int Water;
        public bool SectorNull;
        public List<object?>? Zones = new();
        public int Zone;
        public bool EngineNull;
        public bool LevelNull;
        public int Tics;
        public List<int>? Levels = new();
        public int Timer;
        public int Sequences;
        public int TimerSeen = -1;
        public Heard? Heard;
        public bool OpenObject = true;
        public bool Loading;
        public Chan? Incoming;
        public bool HasReplace;
        public object? Replace;
        public int SaveVersion = OldSave;
        public List<string> Keys = new();
        public List<ulong> Starts = new();
        public float[]? Point;
        public int Closed;
        public bool DeviceNull;
        public bool Writing = true;
        public bool BeginArray = true;
        public bool FailChannel;
        public int ArrayCount;
        public List<bool> Sync = new();
        public int DeviceUpdates;
        public int Stopped;
        public bool ArrayOpened;
        public bool ArrayClosed;
        public int Restart = -1;
        public List<Chan> Channels = new();
        public List<ulong> Positions = new();
        public List<Chan> Restored = new();
    }
    public static void Hear(State s)
    {
        var heard = new Heard { VelocityCleared = true };
        if (s.Actor != null)
        {
            heard.Angle = (float)s.Yaw;
            heard.X = s.X;
            heard.Y = s.Y;
            heard.Z = s.Z;
            heard.Underwater = s.Water == 3;
            if (s.SectorNull) throw new NullReferenceException();
            if (s.Zones == null) throw new NullReferenceException();
            if (s.Zone < 0 || s.Zone >= s.Zones.Count) throw new IndexOutOfRangeException();
            heard.Environment = s.Zones[s.Zone];
            heard.Valid = true;
            heard.Actor = s.Actor;
        }
        if (s.EngineNull) throw new NullReferenceException();
        s.Heard = heard;
    }
    public static void Update(State s)
    {
        Hear(s);
        if (s.Tics > 0)
        {
            if (s.Levels == null) throw new NullReferenceException();
            s.Sequences += s.Tics * s.Levels.Count;
        }
        if (s.LevelNull) throw new NullReferenceException();
        s.TimerSeen = s.Timer;
    }
    public static void Write(State s, Chan c)
    {
        if (!s.OpenObject) return;
        if (s.Loading && s.Incoming != null)
        {
            c.SourceType = s.Incoming.SourceType;
            c.SoundId = s.Incoming.SoundId;
            c.OrgId = s.Incoming.OrgId;
            c.Volume = s.Incoming.Volume;
            c.Distance = s.Incoming.Distance;
            c.Pitch = s.Incoming.Pitch;
            c.Flags = s.Incoming.Flags;
            c.EntChannel = s.Incoming.EntChannel;
            c.Priority = s.Incoming.Priority;
            c.Near = s.Incoming.Near;
            c.Start = s.Incoming.Start;
            c.Rolloff = s.Incoming.Rolloff;
            c.Min = s.Incoming.Min;
            c.Max = s.Incoming.Max;
            c.Limit = s.Incoming.Limit;
            c.Source = s.Incoming.Source;
            c.Point = s.Incoming.Point;
        }
        s.Keys.Add("sourcetype");
        s.Keys.Add("soundid");
        s.Keys.Add("orgid");
        s.Keys.Add("volume");
        s.Keys.Add("distancescale");
        s.Keys.Add("pitch");
        if (s.SaveVersion < OldSave) c.Pitch /= 128f;
        s.Keys.Add("chanflags");
        s.Keys.Add("entchannel");
        s.Keys.Add("priority");
        s.Keys.Add("nearlimit");
        s.Keys.Add("starttime");
        s.Keys.Add("rolloftype");
        s.Keys.Add("rolloffmin");
        s.Keys.Add("rolloffmax");
        s.Keys.Add("limitrange");
        switch (c.SourceType)
        {
            case SourceNone: break;
            case SourceActor: s.Keys.Add("actor"); if (s.HasReplace) c.Source = s.Replace; break;
            case SourceSector: s.Keys.Add("sector"); if (s.HasReplace) c.Source = s.Replace; break;
            case SourcePoly: s.Keys.Add("poly"); if (s.HasReplace) c.Source = s.Replace; break;
            case SourceUnattached:
                if (c.Point == null) throw new NullReferenceException();
                s.Keys.Add("point");
                s.Point = new[] { c.Point[0], c.Point[1], c.Point[2] };
                break;
            default: throw new Fatal(Unknown + c.SourceType + "\n");
        }
        s.Closed++;
        s.Starts.Add(c.Start);
    }
    public static void Save(State s)
    {
        if (s.DeviceNull) throw new NullReferenceException();
        s.Sync.Add(true);
        if (s.Writing) WriteAll(s);
        else ReadAll(s);
        s.Sync.Add(false);
        s.DeviceUpdates++;
    }
    static void WriteAll(State s)
    {
        if (s.EngineNull) throw new NullReferenceException();
        var count = s.Channels.Count;
        if (count == 0 || !s.BeginArray) return;
        s.ArrayOpened = true;
        for (var i = count; i-- != 0;)
        {
            var chan = s.Channels[i];
            var saved = chan.Start;
            chan.Start = s.Positions[i];
            Write(s, chan);
            chan.Start = saved;
        }
        s.ArrayClosed = true;
    }
    static void ReadAll(State s)
    {
        if (s.EngineNull) throw new NullReferenceException();
        s.Stopped++;
        if (s.BeginArray)
        {
            s.ArrayOpened = true;
            for (var i = 0; i < s.ArrayCount; i++)
            {
                if (s.FailChannel) throw new NullReferenceException();
                var chan = new Chan();
                Write(s, chan);
                chan.Flags |= Evicted | AbsTime;
                s.Restored.Add(chan);
            }
            s.ArrayClosed = true;
        }
        if (s.LevelNull) throw new NullReferenceException();
        s.Restart = s.Timer + 2;
    }
}
