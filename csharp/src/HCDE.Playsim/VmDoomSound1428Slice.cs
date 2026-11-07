using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// ReadSound through snd_reset from s_doomsound.cpp 1255-1523.
/// Position limits end at line 1254.
/// </summary>
public static class VmDoomSound1428Slice
{
    public const int BandStart = 1255;
    public const int BandEnd = 1523;
    public const int LimitsEnd = 1254;
    public const int Is3D = 1;
    public const int Evicted = 2;
    public const int ListenerZ = 8;
    public const int MaybeLocal = 16;
    public const int Ui = 32;
    public const int NoPause = 64;
    public const int Area = 128;
    public const int Loop = 256;
    public const int Virtual = 2048;
    public const int PlayerReserve = 1;
    public const int NoLink = -1;
    public const int Auto = 0;
    public const int Body = 4;
    public const int Replace = 1;
    public const float Idle = 1.001f;
    public const int Yellow = 10;
    public const int Gold = 5;
    public const int Brown = 4;
    public const int Grey = 2;
    public const string Green = "\u001cD";
    public const string Black = "\u001cM";
    public const string RngName = "RandSound";

    public sealed class Draw
    {
        public int Color;
        public int X;
        public int Y;
        public string Text = "";
    }

    public sealed class Chan
    {
        public int Next = -1;
        public int Prev = -2;
        public float OriginX;
        public float OriginY;
        public float OriginZ;
        public float Volume = 1f;
        public float DistanceScale = 1f;
        public int Ent;
        public int Priority;
        public int Flags;
        public float Aud;
        public uint Position;
        public string? ShortName = "DS";
    }

    public sealed class Sfx
    {
        public bool Random;
        public int Link = NoLink;
        public int Lump = NoLink;
        public string Name = "";
        public int User;
        public int Pitch;
        public string? ShortName;
    }

    public sealed class RollList
    {
        public int[] Choices = Array.Empty<int>();
    }

    public sealed class Started
    {
        public string Kind = "";
        public int Channel;
        public int Flags;
        public int Id;
        public float Volume;
        public float Atten;
        public string? ClassName;
        public double X;
        public double Y;
        public double Z;
        public int Replace;
    }

    public sealed class State
    {
        public bool ReaderNull;
        public int Length;
        public int ReadCount = -1;
        public byte[]? Bytes = Array.Empty<byte>();
        public int Opened = -1;
        public List<Sfx?> Sounds = new();
        public List<RollList?> Lists = new();
        public List<int> Rolls = new();
        public int RollAt;
        public List<int> RollSizes = new();
        public bool Rolled;
        public List<string> Lines = new();
        public bool EngineNull;
        public string[]? Args = Array.Empty<string>();
        public Dictionary<string, int> Ids = new(StringComparer.Ordinal);
        public int Lookups;
        public List<Started> Plays = new();
        public bool Net;
        public bool PlayerNull = true;
        public double PlayerX;
        public double PlayerY;
        public double PlayerZ;
        public bool SpawnNull;
        public int Spawns;
        public bool LevelNull;
        public int Resets;
        public List<Chan> Channels = new();
        public bool ChannelsNull = true;
        public bool CameraNull;
        public float ListenX;
        public float ListenY;
        public float ListenZ;
        public int CleanY = 1;
        public int Font = 8;
        public int Height = 200;
        public bool Heard;
        public int Calcs;
        public bool AudNull;
        public bool PosNull;
        public List<Draw> Draws = new();
    }

    public static byte[] ReadSound(State s, int lump)
    {
        if (s.ReaderNull) throw new NullReferenceException();
        s.Opened = lump;
        if (s.Length < 0) throw new NullReferenceException();
        if (s.Bytes == null && s.Length > 0) throw new NullReferenceException();
        var buffer = new byte[s.Length];
        int have = s.Bytes?.Length ?? 0;
        int fill = Math.Min(buffer.Length, have);
        if (fill > 0) Array.Copy(s.Bytes!, buffer, fill);
        int count = s.ReadCount < 0 ? s.Length : s.ReadCount;
        if (count < 0) count = 0;
        if (count == buffer.Length) return buffer;
        var sized = new byte[count];
        Array.Copy(buffer, sized, Math.Min(count, buffer.Length));
        return sized;
    }

    public static int PickReplacement(State s, int id)
    {
        while (true)
        {
            if ((uint)id >= (uint)s.Sounds.Count || s.Sounds[id] == null) throw new NullReferenceException();
            var sfx = s.Sounds[id]!;
            if (!sfx.Random) return id;
            if ((uint)sfx.Link >= (uint)s.Lists.Count || s.Lists[sfx.Link] == null) throw new NullReferenceException();
            var choices = s.Lists[sfx.Link]!.Choices;
            s.RollSizes.Add(choices.Length);
            s.Rolled = true;
            if (s.RollAt >= s.Rolls.Count) throw new NullReferenceException();
            int roll = s.Rolls[s.RollAt++];
            if ((uint)roll >= (uint)choices.Length) throw new IndexOutOfRangeException();
            id = choices[roll];
        }
    }

    public static void NoiseDebug(State s)
    {
        int y = 32 * s.CleanY;
        Mark(s, Yellow, 0, y, "*** SOUND DEBUG INFO ***");
        y += s.Font;
        Mark(s, Gold, 0, y, "name");
        Mark(s, Gold, 70, y, "x");
        Mark(s, Gold, 120, y, "y");
        Mark(s, Gold, 170, y, "z");
        Mark(s, Gold, 220, y, "vol");
        Mark(s, Gold, 260, y, "dist");
        Mark(s, Gold, 300, y, "chan");
        Mark(s, Gold, 340, y, "pri");
        Mark(s, Gold, 380, y, "flags");
        Mark(s, Gold, 460, y, "aud");
        Mark(s, Gold, 520, y, "pos");
        y += s.Font;
        if (s.ChannelsNull) return;
        if (s.CameraNull || s.Channels.Count == 0) throw new NullReferenceException();
        s.Heard = true;
        int index = 0;
        while (true)
        {
            int next = s.Channels[index].Next;
            if (next < 0) break;
            if ((uint)next >= (uint)s.Channels.Count) throw new NullReferenceException();
            index = next;
        }
        while (y < s.Height - 16)
        {
            var chan = s.Channels[index];
            s.Calcs++;
            int color = (chan.Flags & Loop) != 0 ? Brown : Grey;
            if (chan.ShortName == null) throw new NullReferenceException();
            Mark(s, color, 0, y, chan.ShortName);
            if ((chan.Flags & Is3D) == 0)
            {
                Mark(s, color, 70, y, "---");
                Mark(s, color, 120, y, "---");
                Mark(s, color, 170, y, "---");
                Mark(s, color, 260, y, "---");
            }
            else
            {
                Mark(s, color, 70, y, Fixed0(chan.OriginX));
                Mark(s, color, 120, y, Fixed0(chan.OriginZ));
                Mark(s, color, 170, y, Fixed0(chan.OriginY));
                if (chan.DistanceScale > 0f)
                {
                    float dx = chan.OriginX - s.ListenX;
                    float dy = chan.OriginY - s.ListenY;
                    float dz = chan.OriginZ - s.ListenZ;
                    float sum = dx * dx + dy * dy + dz * dz;
                    Mark(s, color, 260, y, Fixed0(Math.Sqrt(sum)));
                }
                else Mark(s, color, 260, y, "---");
            }
            Mark(s, color, 220, y, VolumeText(chan.Volume));
            Mark(s, color, 300, y, chan.Ent.ToString(CultureInfo.InvariantCulture));
            Mark(s, color, 340, y, chan.Priority.ToString(CultureInfo.InvariantCulture));
            Mark(s, color, 380, y, FlagText(chan.Flags));
            if (s.AudNull) throw new NullReferenceException();
            Mark(s, color, 460, y, chan.Aud.ToString("F4", CultureInfo.InvariantCulture));
            if (s.PosNull) throw new NullReferenceException();
            Mark(s, color, 520, y, chan.Position.ToString(CultureInfo.InvariantCulture));
            y += s.Font;
            if (chan.Prev < 0) break;
            if ((uint)chan.Prev >= (uint)s.Channels.Count) throw new NullReferenceException();
            index = chan.Prev;
        }
    }

    public static string SoundDebug(State s)
    {
        if (s.EngineNull) throw new NullReferenceException();
        NoiseDebug(s);
        return "";
    }

    public static void PrintSoundList(State s)
    {
        for (int i = 0; i < s.Sounds.Count; i++)
        {
            var sfx = s.Sounds[i] ?? throw new NullReferenceException();
            if (sfx.Random)
            {
                if ((uint)sfx.Link >= (uint)s.Lists.Count || s.Lists[sfx.Link] == null) throw new NullReferenceException();
                s.Lines.Add(Width(i) + ". " + sfx.Name + " -> #" + sfx.Link.ToString(CultureInfo.InvariantCulture) + " {");
                foreach (int choice in s.Lists[sfx.Link]!.Choices)
                    s.Lines.Add(" " + SoundName(s, choice) + " ");
                s.Lines.Add("}\n");
            }
            else if ((sfx.User & PlayerReserve) != 0)
                s.Lines.Add(Width(i) + ". " + sfx.Name + " <<player sound " + sfx.Link.ToString(CultureInfo.InvariantCulture) + ">>\n");
            else if (sfx.Lump != NoLink)
                s.Lines.Add(Width(i) + ". " + sfx.Name + " (" + (sfx.ShortName ?? throw new NullReferenceException()) + ")\n");
            else if (sfx.Link != NoLink)
                s.Lines.Add(Width(i) + ". " + sfx.Name + " -> " + SoundName(s, sfx.Link) + "\n");
            else
                s.Lines.Add(Width(i) + ". " + sfx.Name + " **not present**\n");
            s.Lines.Add("    PitchMask = " + sfx.Pitch.ToString(CultureInfo.InvariantCulture) + "\n");
        }
    }

    public static void SoundList(State s)
    {
        if (s.EngineNull) throw new NullReferenceException();
        PrintSoundList(s);
    }

    public static void PlaySound(State s)
    {
        if (s.Args == null) throw new NullReferenceException();
        if (s.Args.Length <= 1) return;
        if (!Find(s, s.Args[1], out int id))
        {
            s.Lines.Add("'" + s.Args[1] + "' is not a sound\n");
            return;
        }
        s.Plays.Add(new Started { Kind = "ui", Channel = Auto, Flags = Ui, Id = id, Volume = 1f, Atten = 0f });
    }

    public static void LoopSound(State s)
    {
        if (s.Args == null) throw new NullReferenceException();
        if (s.PlayerNull || s.Net || s.Args.Length <= 1) return;
        if (!Find(s, s.Args[1], out int id))
        {
            s.Lines.Add("'" + s.Args[1] + "' is not a sound\n");
            return;
        }
        if (s.LevelNull) throw new NullReferenceException();
        s.Spawns++;
        if (s.SpawnNull) return;
        s.Plays.Add(new Started
        {
            Kind = "body",
            Channel = Body,
            Flags = Loop,
            Id = id,
            Volume = 1f,
            Atten = Idle,
            ClassName = "SpeakerIcon",
            X = s.PlayerX,
            Y = s.PlayerY,
            Z = s.PlayerZ + 32d,
            Replace = Replace,
        });
    }

    public static void ResetSounds(State s) => s.Resets++;

    public static string VolumeText(float value)
    {
        if (float.IsNaN(value)) return "nan";
        if (float.IsPositiveInfinity(value)) return "inf";
        if (float.IsNegativeInfinity(value)) return "-inf";
        if (value == 0f) return float.IsNegative(value) ? "-0" : "0";
        bool neg = value < 0f;
        double v = Math.Abs((double)value);
        int exp = (int)Math.Floor(Math.Log10(v));
        double pow10 = Math.Pow(10, exp);
        if (v < pow10 * (1d - 1e-12)) exp--;
        else if (v >= pow10 * 10d * (1d - 1e-12)) exp++;
        double scaled = v / Math.Pow(10, exp - 1);
        double rounded = RoundHalfEven(scaled);
        if (rounded >= 100d)
        {
            rounded /= 10d;
            exp++;
        }
        if (exp < -4 || exp >= 2)
        {
            int lead = (int)Math.Round(rounded);
            int whole = lead / 10;
            int frac = lead % 10;
            string digits = frac == 0 ? whole.ToString(CultureInfo.InvariantCulture) : whole.ToString(CultureInfo.InvariantCulture) + "." + frac.ToString(CultureInfo.InvariantCulture);
            return (neg ? "-" : "") + digits + "e" + exp.ToString("+00;-00", CultureInfo.InvariantCulture);
        }
        double magnitude = rounded * Math.Pow(10, exp - 1);
        string text = magnitude.ToString("F" + Math.Max(1 - exp, 0), CultureInfo.InvariantCulture);
        if (text.Contains('.')) text = text.TrimEnd('0').TrimEnd('.');
        if (text.Length == 0) text = "0";
        return (neg ? "-" : "") + text;
    }

    public static string Fixed0(double value) =>
        Math.Round(value, MidpointRounding.ToEven).ToString("F0", CultureInfo.InvariantCulture);

    static string FlagText(int flags)
    {
        string Bit(int mask) => (flags & mask) != 0 ? Green : Black;
        return Bit(Is3D) + "3" + Bit(ListenerZ) + "Z" + Bit(Ui) + "U" + Bit(MaybeLocal) + "M" + Bit(NoPause) + "N" + Bit(Area) + "A" + Bit(Loop) + "L" + Bit(Evicted) + "E" + Bit(Virtual) + "V";
    }

    static bool Find(State s, string name, out int id)
    {
        s.Lookups++;
        if (s.Ids.TryGetValue(name, out id) && id >= 0) return true;
        id = -1;
        return false;
    }

    static string SoundName(State s, int index)
    {
        if ((uint)index >= (uint)s.Sounds.Count || s.Sounds[index] == null) throw new NullReferenceException();
        return s.Sounds[index]!.Name;
    }

    static string Width(int value) => value.ToString(CultureInfo.InvariantCulture).PadLeft(3);

    static void Mark(State s, int color, int x, int y, string text) =>
        s.Draws.Add(new Draw { Color = color, X = x, Y = y, Text = text });

    static double RoundHalfEven(double value)
    {
        double floor = Math.Floor(value + 1e-10);
        double diff = value - floor;
        if (diff > 0.5d + 1e-8) return floor + 1d;
        if (diff < 0.5d - 1e-8) return floor;
        return ((long)floor % 2L) == 0L ? floor : floor + 1d;
    }
}
