namespace HCDE.Playsim;
/// <summary>
/// Actor, poly, sector, and point sound from s_doomsound.cpp 575-851.
/// StopChannel ends at line 574. The listener update starts at line 852.
/// </summary>
public static class VmDoomSound1425Slice
{
    public const int BandStart = 575;
    public const int BandEnd = 851;
    public const int StopEnd = 574;
    public const int Listener = 852;
    public const int SourceUnattached = 0;
    public const int SourceNone = 1;
    public const int SourceActor = 2;
    public const int SourceSector = 3;
    public const int SourcePoly = 4;
    public const int NoRumble = 0x80000;
    public const int Rumble = 0x40000;
    public const int Missile = 0x10000;
    public const int MaybeLocal = 16;
    public const int Local = 16384;
    public const int NoStop = 4096;
    public const int Loop = 256;
    public const int SilentPickup = 8;
    public const int Magic = 32;
    public const int Cutoff = 4;
    public const int Weapon = 1;
    public const int Auto = 0;
    public const int RolloffLinear = 1;
    public sealed class Emit
    {
        public int Source;
        public int Channel;
        public int Flags;
        public int Id;
        public float Volume;
        public float Attenuation;
        public float Pitch;
        public float Start;
        public bool Rolloff;
        public int RolloffType;
        public float Min;
        public float Max;
        public bool Position;
        public float X;
        public float Y;
        public float Z;
        public string Kind = "";
        public bool PassedPosition;
        public int ChanMin;
        public int ChanMax;
        public double Delta;
        public bool Playing;
    }
    public sealed class State
    {
        public int Flags;
        public bool Haptics;
        public bool EntNull;
        public bool PlayerSelf;
        public int ActorFlags;
        public bool TargetNull = true;
        public bool TargetSelf;
        public bool Euthanize;
        public bool SectorNull;
        public bool SectorSilent;
        public bool SameLevel = true;
        public int Compat;
        public int Compat2;
        public bool IsListener;
        public bool ListenerView;
        public bool HasEngine = true;
        public int Channel;
        public int Id;
        public float Volume = 1;
        public float Attenuation = 1;
        public float Pitch;
        public float Start;
        public float Min = 1;
        public float Max = 100;
        public bool PolyNull;
        public bool SectorObjNull;
        public bool FromNull;
        public float PosX = 1;
        public float PosY = 2;
        public float PosZ = 3;
        public bool ActorBusy;
        public bool NoneBusy;
        public bool PlayingResult = true;
        public int Looping;
        public int Local;
        public int ChanMin;
        public int ChanMax;
        public double Delta = 0.5;
        public List<Emit> Plays = new();
    }
    public static void PitchActor(State s)
    {
        if ((s.Flags & NoRumble) != 0)
        {
            if ((s.Flags & Rumble) != 0) s.Flags -= Rumble;
        }
        else if ((s.Flags & Rumble) == 0 && s.Haptics && Owns(s)) s.Flags |= Rumble;
        if (!Verify(s)) return;
        Start(s, SourceActor, s.Attenuation, s.Pitch, s.Start, false);
    }
    public static void SoundActor(State s)
    {
        var pitch = s.Pitch;
        s.Pitch = 0;
        try { PitchActor(s); }
        finally { s.Pitch = pitch; }
    }
    public static void MinMax(State s)
    {
        if (!Verify(s)) return;
        Start(s, SourceActor, 1, 0, 0, true);
    }
    public static void SoundPoly(State s)
    {
        if (s.PolyNull) throw new NullReferenceException();
        if (!s.SameLevel) return;
        Start(s, SourcePoly, s.Attenuation, 0, 0, false);
    }
    public static void SoundPoint(State s)
    {
        if (!s.SameLevel) return;
        var play = Start(s, SourceUnattached, s.Attenuation, 0, 0, false);
        play.Position = true;
        play.X = s.PosX;
        play.Z = s.PosZ;
        play.Y = s.PosY;
    }
    public static void SoundSector(State s)
    {
        if (s.SectorObjNull) throw new NullReferenceException();
        if (!s.SameLevel) return;
        Start(s, SourceSector, s.Attenuation, 0, 0, false);
    }
    public static void PlayPitch(State s)
    {
        if (s.EntNull) return;
        if (s.SectorNull) throw new NullReferenceException();
        if (s.SectorSilent || !s.SameLevel) return;
        if ((s.Flags & Local) == 0)
        {
            if ((s.Flags & NoStop) == 0 || !ActorPlaying(s)) PitchActor(s);
            return;
        }
        if (!s.ListenerView) return;
        if ((s.Flags & NoStop) == 0 || !NonePlaying(s))
        {
            var atten = s.Attenuation;
            s.Attenuation = 0;
            Start(s, SourceNone, 0, s.Pitch, s.Start, false);
            s.Attenuation = atten;
        }
    }
    public static void Play(State s)
    {
        var pitch = s.Pitch;
        var start = s.Start;
        s.Pitch = 0;
        s.Start = 0;
        try { PlayPitch(s); }
        finally
        {
            s.Pitch = pitch;
            s.Start = start;
        }
    }
    public static void ActorStart(State s) => PlayPitch(s);
    public static void Decorate(State s)
    {
        if (s.Looping != 0) s.Flags |= Loop | NoStop;
        if (s.Local != 0) s.Flags |= Local;
        var flags = s.Flags;
        s.Channel = flags & 7;
        s.Flags = flags & ~7;
        s.Start = 0;
        PlayPitch(s);
    }
    public static void StopActor(State s) => Stop(s, SourceActor);
    public static void StopActors(State s)
    {
        Engine(s);
        s.Plays.Add(new Emit { Kind = "actors", Source = SourceActor, ChanMin = s.ChanMin, ChanMax = s.ChanMax });
    }
    public static void StopSector(State s) => Stop(s, SourceSector);
    public static void StopPoly(State s) => Stop(s, SourcePoly);
    public static void Relink(State s)
    {
        if (s.FromNull) throw new NullReferenceException();
        Engine(s);
        var play = new Emit { Kind = "relink", Source = SourceActor, X = s.PosX, Y = s.PosY, Z = s.PosZ };
        play.PassedPosition = (s.Compat2 & Cutoff) == 0;
        s.Plays.Add(play);
    }
    public static void ChangeVolume(State s)
    {
        Engine(s);
        s.Plays.Add(new Emit
        {
            Kind = "volume",
            Source = SourceActor,
            Channel = (s.Compat & Magic) != 0 ? -1 : s.Channel,
            Delta = s.Delta,
        });
    }
    public static void ChangePitch(State s)
    {
        Engine(s);
        s.Plays.Add(new Emit { Kind = "pitch", Source = SourceActor, Channel = s.Channel, Pitch = s.Pitch });
    }
    public static bool PlayingActor(State s) => Query(s, SourceActor);
    public static bool PlayingSector(State s) => Query(s, SourceSector);
    public static bool PlayingPoly(State s) => Query(s, SourcePoly);
    public static bool ActorPlaying(State s)
    {
        Engine(s);
        var channel = (s.Compat & Magic) != 0 ? Auto : s.Channel;
        s.Plays.Add(new Emit { Kind = "actor", Source = SourceActor, Channel = channel, Id = s.Id, Playing = s.PlayingResult });
        return s.PlayingResult;
    }
    static bool Owns(State s)
    {
        if (s.EntNull) throw new NullReferenceException();
        if (s.PlayerSelf) return true;
        return (s.ActorFlags & Missile) != 0 && !s.TargetNull && s.TargetSelf;
    }
    static bool Verify(State s)
    {
        if (s.EntNull || s.Euthanize) return false;
        if (s.SectorNull) throw new NullReferenceException();
        if (s.SectorSilent || !s.SameLevel) return false;
        if ((s.Flags & MaybeLocal) != 0 && (s.Compat & SilentPickup) != 0 && !s.IsListener) return false;
        if ((s.Compat & Magic) != 0) s.Channel = Weapon;
        return true;
    }
    static Emit Start(State s, int source, float attenuation, float pitch, float start, bool rolloff)
    {
        Engine(s);
        var play = new Emit
        {
            Kind = "start",
            Source = source,
            Channel = s.Channel,
            Flags = s.Flags,
            Id = s.Id,
            Volume = s.Volume,
            Attenuation = attenuation,
            Pitch = pitch,
            Start = start,
            Rolloff = rolloff,
            RolloffType = RolloffLinear,
            Min = s.Min,
            Max = s.Max,
        };
        s.Plays.Add(play);
        return play;
    }
    static void Stop(State s, int source)
    {
        Engine(s);
        s.Plays.Add(new Emit
        {
            Kind = "stop",
            Source = source,
            Channel = (s.Compat & Magic) != 0 ? -1 : s.Channel,
        });
    }
    static bool NonePlaying(State s)
    {
        Engine(s);
        s.Plays.Add(new Emit { Kind = "none", Source = SourceNone, Channel = s.Channel, Id = s.Id, Playing = s.NoneBusy });
        return s.NoneBusy;
    }
    static bool Query(State s, int source)
    {
        Engine(s);
        s.Plays.Add(new Emit { Kind = "info", Source = source, Id = s.Id, Playing = s.PlayingResult });
        return s.PlayingResult;
    }
    static void Engine(State s)
    {
        if (!s.HasEngine) throw new NullReferenceException();
    }
}
