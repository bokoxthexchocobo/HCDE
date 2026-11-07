namespace HCDE.Playsim;

/// <summary>
/// ACS call cases UniqueTID through StopSound from p_acs.cpp 5967-6148.
/// </summary>
public static class VmAcsCall1460Slice
{
    public const int BandStart = 5967;
    public const int BandEnd = 6148;
    public const int UniqueTid = 46;
    public const int IsTidUsed = 47;
    public const int Sqrt = 48;
    public const int FixedSqrt = 49;
    public const int VectorLength = 50;
    public const int SetHudClipRect = 51;
    public const int SetHudWrapWidth = 52;
    public const int SetCVar = 53;
    public const int GetUserCVar = 54;
    public const int SetUserCVar = 55;
    public const int GetCVarString = 56;
    public const int SetCVarString = 57;
    public const int GetUserCVarString = 58;
    public const int SetUserCVarString = 59;
    public const int LineAttack = 60;
    public const int PlaySound = 61;
    public const int StopSound = 62;
    public const int PlayActorSound = 71;
    public const int BodyChan = 4;
    public const double MissileRange = 32 * 64;

    public sealed class Shot
    {
        public int Source;
        public double Angle;
        public double Pitch;
        public int Damage;
        public string Puff = "";
        public string DamageType = "";
        public double Range;
        public int Flags;
        public int PuffId;
        public int PuffTid;
    }

    public sealed class Heard
    {
        public int Actor;
        public int Channel;
        public int Flags;
        public int Sound;
        public float Volume;
        public float Attenuation;
    }

    public sealed class World
    {
        public bool NoArgCheck;
        public bool ClientSide;
        public int Activator = -1;
        public Dictionary<int, string?> Strings = new();
        public Func<int, int, bool, int>? Unique;
        public Func<int, bool, int>? TidUsed;
        public Func<string?, bool, int>? ReadCVar;
        public Func<int, string?, int, bool, int>? WriteCVar;
        public Func<int, int>? ResolvePlayer;
        public Func<int, string?, bool, int>? ReadUserCVar;
        public Func<int, string?, int, bool, int>? WriteUserCVar;
        public Func<int, IEnumerable<int>>? ActorsOf;
        public Func<int, double, double, int, string, string, double, int, int>? Fire;
        public Func<string?, int>? FindSound;
        public Func<int, int, int>? ActorSound;
        public int ClipLeft, ClipTop, ClipWidth, ClipHeight, WrapWidth;
        public bool HandleAspect = true;
        public List<Shot> Shots = new();
        public List<Heard> HeardSounds = new();
        public List<(int Actor, int Channel)> Stops = new();
        public List<(int Puff, int Tid)> TidSets = new();
    }

    public static int Call(World w, int func, int argCount, int[] args, out int need)
    {
        need = 0;
        int Arg(int i) => i < argCount ? args[i] : 0;
        int Short(int min) => argCount < min && !w.NoArgCheck ? min : 0;
        string? Name(int id) => w.Strings.TryGetValue(id, out var text) ? text : null;
        IEnumerable<int> Actors(int tid) => w.ActorsOf == null ? Array.Empty<int>() : w.ActorsOf(tid);
        switch (func)
        {
            case UniqueTid:
                return (w.Unique ?? ((_, _, _) => 0))(argCount > 0 ? Arg(0) : 0, argCount > 1 && Arg(1) >= 0 ? Arg(1) : 0, w.ClientSide);
            case IsTidUsed:
                if ((need = Short(1)) != 0) return 0;
                return (w.TidUsed ?? ((_, _) => 0))(Arg(0), w.ClientSide);
            case Sqrt:
                if ((need = Short(1)) != 0) return 0;
                return (int)Math.Floor(Math.Sqrt(Arg(0)));
            case FixedSqrt:
                if ((need = Short(1)) != 0) return 0;
                return ToAcs(Math.Sqrt(ToDouble(Arg(0))));
            case VectorLength:
                if ((need = Short(2)) != 0) return 0;
                double x = ToDouble(Arg(0));
                double y = ToDouble(Arg(1));
                return ToAcs(Math.Sqrt(x * x + y * y));
            case SetHudClipRect:
                w.ClipLeft = argCount > 0 ? Arg(0) : 0;
                w.ClipTop = argCount > 1 ? Arg(1) : 0;
                w.ClipWidth = argCount > 2 ? Arg(2) : 0;
                w.ClipHeight = argCount > 3 ? Arg(3) : 0;
                w.WrapWidth = argCount > 4 ? Arg(4) : 0;
                w.HandleAspect = argCount > 5 ? Arg(5) != 0 : true;
                return 0;
            case SetHudWrapWidth:
                w.WrapWidth = argCount > 0 ? Arg(0) : 0;
                return 0;
            case GetCVarString:
                if ((need = Short(1)) != 0) return 0;
                return (w.ReadCVar ?? ((_, _) => 0))(Name(Arg(0)), true);
            case SetCVar:
                if ((need = Short(2)) != 0) return 0;
                return (w.WriteCVar ?? ((_, _, _, _) => 0))(w.Activator, Name(Arg(0)), Arg(1), false);
            case SetCVarString:
                if ((need = Short(2)) != 0) return 0;
                return (w.WriteCVar ?? ((_, _, _, _) => 0))(w.Activator, Name(Arg(0)), Arg(1), true);
            case GetUserCVar:
            case GetUserCVarString:
                if ((need = Short(2)) != 0) return 0;
                int player = 0;
                if (w.ResolvePlayer == null || (player = w.ResolvePlayer(Arg(0))) < 0)
                    return 0;
                return (w.ReadUserCVar ?? ((_, _, _) => 0))(player, Name(Arg(1)), func == GetUserCVarString);
            case SetUserCVar:
                if ((need = Short(3)) != 0) return 0;
                return (w.WriteUserCVar ?? ((_, _, _, _) => 0))(Arg(0), Name(Arg(1)), Arg(2), false);
            case SetUserCVarString:
                if ((need = Short(3)) != 0) return 0;
                return (w.WriteUserCVar ?? ((_, _, _, _) => 0))(Arg(0), Name(Arg(1)), Arg(2), true);
            case LineAttack:
                if ((need = Short(4)) != 0) return 0;
                double angle = Arg(1) * (90.0 / 16384);
                double pitch = Arg(2) * (90.0 / 16384);
                string puff = argCount > 4 && Arg(4) != 0 ? Name(Arg(4)) ?? "" : "BulletPuff";
                string kind = argCount > 5 && Arg(5) != 0 ? Name(Arg(5)) ?? "" : "None";
                double range = argCount > 6 && Arg(6) != 0 ? ToDouble(Arg(6)) : MissileRange;
                int flags = argCount > 7 && Arg(7) != 0 ? Arg(7) : 0;
                int puffTid = argCount > 8 && Arg(8) != 0 ? Arg(8) : 0;
                int fh = 0;
                if ((flags & 1) != 0) fh |= 2;
                if ((flags & 2) != 0) fh |= 4;
                void Shoot(int source)
                {
                    int puffId = w.Fire == null ? -1 : w.Fire(source, angle, pitch, Arg(3), puff, kind, range, fh);
                    w.Shots.Add(new Shot { Source = source, Angle = angle, Pitch = pitch, Damage = Arg(3), Puff = puff, DamageType = kind, Range = range, Flags = fh, PuffId = puffId, PuffTid = puffTid });
                    if (puffId >= 0 && puffTid != 0)
                        w.TidSets.Add((puffId, puffTid));
                }
                if (Arg(0) == 0) Shoot(w.Activator);
                else foreach (int source in Actors(Arg(0))) Shoot(source);
                return 0;
            case PlaySound:
            case PlayActorSound:
                if ((need = Short(2)) != 0) return 0;
                int sid = 0;
                if (func == PlaySound)
                {
                    string? lookup = Name(Arg(1));
                    if (lookup != null)
                        sid = w.FindSound == null ? 0 : w.FindSound(lookup);
                }
                if (sid != 0 || func == PlayActorSound)
                {
                    int chan = argCount > 2 ? Arg(2) : BodyChan;
                    float vol = argCount > 3 ? Arg(3) / 65536f : 1f;
                    bool looping = argCount > 4 && Arg(4) != 0;
                    float atten = argCount > 5 ? Arg(5) / 65536f : 1f;
                    bool local = argCount > 6 && Arg(6) != 0;
                    void Hear(int actor)
                    {
                        int use = func == PlayActorSound ? (w.ActorSound == null ? 0 : w.ActorSound(actor, Arg(1))) : sid;
                        if (use == 0) return;
                        int mixed = chan;
                        if (local) mixed |= 16384;
                        if (looping) mixed |= 256 | 4096;
                        w.HeardSounds.Add(new Heard { Actor = actor, Channel = mixed & 7, Flags = mixed & ~7, Sound = use, Volume = vol, Attenuation = atten });
                    }
                    if (Arg(0) == 0) Hear(w.Activator);
                    foreach (int actor in Actors(Arg(0))) Hear(actor);
                }
                return 0;
            case StopSound:
                if ((need = Short(1)) != 0) return 0;
                int stopChan = argCount > 1 ? Arg(1) : BodyChan;
                if (Arg(0) == 0) w.Stops.Add((w.Activator, stopChan));
                else foreach (int actor in Actors(Arg(0))) w.Stops.Add((actor, stopChan));
                return 0;
            default:
                return 0;
        }
    }

    public static double ToDouble(int value) => value / 65536.0;

    public static int ToAcs(double value) => (int)Math.Round(value * 65536.0, MidpointRounding.ToEven);
}
