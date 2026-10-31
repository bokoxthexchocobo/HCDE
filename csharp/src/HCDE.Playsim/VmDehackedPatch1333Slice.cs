namespace HCDE.Playsim;

/// <summary>
/// DeHackEd sound, frame, sprite, and ammo patches from
/// <c>d_dehacked.cpp</c> (1907-2252). Chunk 1333. GetLine, FindState,
/// and FindSprite stay callees fed by this world.
/// </summary>
public sealed class VmDehackedPatch1333
{
    public const int StfFast = 2;
    public const int StfFullbright = 4;
    public const int StfDehacked = 64;
    public const int WifNoAutofire = 1;
    public const int WifNoAlert = 0x10;
    public const int WifWimpy = 0x200;
    public const int WifMelee = 0x8000;
    public const int WifNoAutoswitch = 0x80000;
    public const int ShortMax = 32767;

    public static readonly int[] Toff = { 129044, 129044, 129044, 129284, 129380 };

    public static readonly (string Name, int Value)[] SkillBits =
    {
        ("SKILL5FAST", StfFast),
    };

    public static readonly (string Name, int Value)[] WeaponBits =
    {
        ("NOTHRUST", -1),
        ("SILENT", WifNoAlert),
        ("NOAUTOFIRE", WifNoAutofire),
        ("FLEEMELEE", WifMelee),
        ("AUTOSWITCHFROM", WifWimpy),
        ("NOAUTOSWITCHTO", WifNoAutoswitch),
    };

    public bool Dsdhacked;
    public int DVersion;
    public int OrgSprOrgSize;
    public bool ChangedStates;
    public int SoundLines;

    public readonly List<string> OrgNames = new();
    public readonly List<string> Unchanged = new();
    public readonly List<string> Sprites = new();
    public readonly Dictionary<int, State> States = new();
    public readonly Dictionary<int, int> SpriteNumbers = new();
    public readonly Dictionary<int, ArgSet> StateArgs = new();
    public readonly List<Actor?> AmmoNames = new();
    public readonly List<Actor> Actors = new();
    public readonly List<string> Prints = new();
    public readonly List<string> Debugs = new();
    public readonly Queue<Line> Lines = new();

    public Actor? WeaponBase;
    public readonly State Dummy = new();
    public string Line1 = "";
    public string Line2 = "";

    public readonly record struct Line(int Code, string Key, string Value);

    public sealed class State
    {
        public int Id;
        public int Tics;
        public int Misc1;
        public int Misc2;
        public int Frame;
        public int StateFlags;
        public State? Next;
    }

    public sealed class ArgSet
    {
        public readonly long[] Args = new long[8];
        public int Used;
    }

    public sealed class Actor
    {
        public string Name = "";
        public Actor? Parent;
        public int MaxAmount;
        public int Amount;
        public int BackpackMax;
        public int BackpackAmount;
        public int AmmoGive1;
        public int AmmoGive2;
        public Actor? AmmoType1;
        public Actor? AmmoType2;
    }

    public State AddState(int id, int tics, int misc1, int frame, int flags)
    {
        var state = new State
        {
            Id = id,
            Tics = tics,
            Misc1 = misc1,
            Frame = frame,
            StateFlags = flags,
        };
        States[id] = state;
        return state;
    }

    public void Push(int code, string key, string value)
    {
        Lines.Enqueue(new Line(code, key, value));
    }

    public int ReadLine()
    {
        if (Lines.Count == 0)
            return 0;
        Line line = Lines.Dequeue();
        Line1 = line.Key;
        Line2 = line.Value;
        return line.Code;
    }

    public State? FindState(int statenum)
    {
        if (statenum <= 0)
            return null;
        States.TryGetValue(statenum, out State? state);
        return state;
    }

    public int FindSprite(string name)
    {
        string key = name.Length >= 4 ? name.Substring(0, 4) : name;
        for (int i = 0; i < Unchanged.Count; i++)
        {
            if (Unchanged[i] == key)
                return i;
        }
        return -1;
    }

    public int PatchSound(int soundNum, int flags)
    {
        int result = ReadLine();
        while (result == 1)
        {
            SoundLines++;
            result = ReadLine();
        }
        return result;
    }

    public int PatchFrame(int frameNum, int flags)
    {
        State? found = FindState(frameNum);
        State info;
        int tics;
        int misc1;
        int frameBits;
        bool live;
        if (found != null)
        {
            info = found;
            live = true;
            Debugs.Add("Frame " + frameNum.ToString() + "\n");
            if (frameNum == 47)
                tics = 5;
            else if (frameNum == 48)
                tics = 4;
            else
                tics = info.Tics;
            misc1 = info.Misc1;
            frameBits = info.Frame;
            if ((info.StateFlags & StfFullbright) != 0)
                frameBits |= 0x8000;
        }
        else
        {
            info = Dummy;
            live = false;
            tics = 0;
            misc1 = 0;
            frameBits = 0;
            Dummy.Misc2 = 0;
            Dummy.Next = null;
            Prints.Add("Frame " + frameNum.ToString() + " out of range\n");
        }

        var args = new ArgSet();
        int result;
        while ((result = ReadLine()) == 1)
            ApplyFrameKey(info, frameNum, ref tics, ref misc1, ref frameBits, args);

        if (live)
        {
            info.StateFlags |= StfDehacked;
            if ((uint)(frameBits & 0x7fff) > 63)
                Prints.Add("Frame " + frameNum.ToString() + ": Subnumber must be in range [0,63]\n");
            info.Tics = tics;
            info.Misc1 = misc1;
            info.Frame = frameBits & 0x3f;
            if ((frameBits & 0x8000) != 0)
                info.StateFlags |= StfFullbright;
            else
                info.StateFlags &= ~StfFullbright;
            StateArgs[info.Id] = args;
        }
        return result;
    }

    public int PatchSprite(int sprNum, int flags)
    {
        int index = sprNum;
        if ((uint)sprNum < (uint)OrgSprOrgSize)
            Debugs.Add("Sprite " + sprNum.ToString() + "\n");
        else
        {
            Prints.Add("Sprite " + sprNum.ToString() + " out of range.\n");
            index = -1;
        }

        int offset = 0;
        int result;
        while ((result = ReadLine()) == 1)
        {
            if (Line1.Equals("Offset", StringComparison.OrdinalIgnoreCase))
                offset = Atoi(Line2);
            else
                Prints.Add(Unknown("Sprite", index));
        }

        if (offset > 0 && index != -1 && !Dsdhacked)
            RemapSprite(ref index, offset);
        return result;
    }

    public int PatchAmmo(int ammoNum, int flags)
    {
        Actor? ammo = null;
        bool indexed = ammoNum >= 0 && ammoNum < 4 && (uint)ammoNum <= (uint)AmmoNames.Count;
        if (indexed)
        {
            Debugs.Add("Ammo " + ammoNum.ToString() + ".\n");
            if (ammoNum < AmmoNames.Count)
                ammo = AmmoNames[ammoNum];
        }
        if (ammo == null)
            Prints.Add("Ammo " + ammoNum.ToString() + " out of range.\n");

        int oldClip = ammo == null ? 0 : ammo.Amount;
        int result;
        while ((result = ReadLine()) == 1)
        {
            if (Line1.Equals("Max ammo", StringComparison.OrdinalIgnoreCase))
            {
                if (ammo != null)
                    ammo.MaxAmount = Atoi(Line2);
            }
            else if (Line1.Equals("Per ammo", StringComparison.OrdinalIgnoreCase))
            {
                if (ammo != null)
                    ammo.Amount = Atoi(Line2);
            }
            else
            {
                Prints.Add(Unknown("Ammo", ammoNum));
            }
        }

        if (ammo != null)
        {
            ammo.BackpackMax = ammo.MaxAmount * 2;
            ammo.BackpackAmount = ammo.Amount;
            if (oldClip != ammo.Amount)
                ScaleDescendants(ammo, oldClip, ammo.Amount);
        }
        return result;
    }

    void ApplyFrameKey(State info, int frameNum, ref int tics, ref int misc1, ref int frameBits, ArgSet args)
    {
        long val64 = Atoll(Line2);
        int val = unchecked((int)val64);
        int keylen = Line1.Length;
        if (keylen == 8 && Line1.Equals("Duration", StringComparison.OrdinalIgnoreCase))
            tics = ClampTics(val);
        else if (keylen == 9 && Line1.Equals("Unknown 1", StringComparison.OrdinalIgnoreCase))
            misc1 = val;
        else if (keylen == 9 && Line1.Equals("Unknown 2", StringComparison.OrdinalIgnoreCase))
            info.Misc2 = val;
        else if (keylen == 13 && Line1.Equals("Sprite number", StringComparison.OrdinalIgnoreCase))
            SpriteNumbers[info.Id] = val;
        else if (keylen == 10 && Line1.Equals("Next frame", StringComparison.OrdinalIgnoreCase))
        {
            info.Next = FindState(val);
            ChangedStates = true;
        }
        else if (keylen == 16 && Line1.Equals("Sprite subnumber", StringComparison.OrdinalIgnoreCase))
            frameBits = val;
        else if (keylen == 5 && Line1.StartsWith("Args", StringComparison.OrdinalIgnoreCase))
            StoreArg(args, val64);
        else if (Line1.Equals("MBF21 Bits", StringComparison.OrdinalIgnoreCase))
            ApplyMbf(info, Line2);
        else
            Prints.Add(Unknown("Frame", frameNum));
    }

    void StoreArg(ArgSet args, long val64)
    {
        int arg = Line1[4] - '1';
        if (arg < 0 || arg >= 8)
        {
            Prints.Add("Invalid frame arg " + arg.ToString() + "\n");
            return;
        }
        args.Args[arg] = val64;
        args.Used |= 1 << arg;
    }

    void ApplyMbf(State info, string text)
    {
        uint value = 0;
        bool changed = false;
        foreach (string token in SplitBits(text))
        {
            if (IsNum(token))
            {
                value |= unchecked((uint)Atoll(token));
                changed = true;
                continue;
            }
            int i = 0;
            for (; i < SkillBits.Length; i++)
            {
                if (token.Equals(SkillBits[i].Name, StringComparison.OrdinalIgnoreCase))
                {
                    changed = true;
                    value |= 1u << i;
                    break;
                }
            }
            if (i == SkillBits.Length)
                Debugs.Add("Unknown bit mnemonic " + token + "\n");
        }
        if (changed)
        {
            int mask = 0;
            int flags = 0;
            for (int i = 0; i < SkillBits.Length; i++)
            {
                mask |= SkillBits[i].Value;
                if ((value & (1u << i)) != 0)
                    flags |= SkillBits[i].Value;
            }
            info.StateFlags = (info.StateFlags & ~mask) | flags;
        }
        int shown = info.StateFlags;
        Debugs.Add("MBF21 Bits: " + shown.ToString() + " (0x" + unchecked((uint)shown).ToString("x8") + ")\n");
    }

    void RemapSprite(ref int index, int offset)
    {
        if ((uint)DVersion >= (uint)Toff.Length)
            throw new InvalidOperationException("dversion is outside toff");
        int mapped = (offset - Toff[DVersion] - 22044) / 8;
        if ((uint)mapped < (uint)OrgSprOrgSize)
        {
            string org = OrgNames[index];
            index = FindSprite(org);
            if (index != -1)
                Sprites[index] = Four(OrgNames[mapped]);
        }
        else
        {
            Prints.Add("Sprite name " + mapped.ToString() + " out of range.\n");
        }
    }

    void ScaleDescendants(Actor ammo, int oldClip, int newPer)
    {
        if (oldClip == 0)
            throw new InvalidOperationException("ammo scale divisor is zero");
        for (int i = 0; i < Actors.Count; i++)
        {
            Actor type = Actors[i];
            if (ReferenceEquals(type, ammo))
                continue;
            if (IsDescendant(type, ammo))
            {
                type.MaxAmount = ammo.MaxAmount;
                type.Amount = Scale(type.Amount, newPer, oldClip);
            }
            else if (IsDescendant(type, WeaponBase))
            {
                if (ReferenceEquals(type.AmmoType1, ammo))
                    type.AmmoGive1 = Scale(type.AmmoGive1, newPer, oldClip);
                if (ReferenceEquals(type.AmmoType2, ammo))
                    type.AmmoGive2 = Scale(type.AmmoGive2, newPer, oldClip);
            }
        }
    }

    static bool IsDescendant(Actor type, Actor? ancestor)
    {
        if (ancestor == null)
            return false;
        for (Actor? cursor = type; cursor != null; cursor = cursor.Parent)
        {
            if (ReferenceEquals(cursor, ancestor))
                return true;
        }
        return false;
    }

    static int Scale(int current, int newPer, int oldClip)
    {
        return (int)((long)current * newPer / oldClip);
    }

    static int ClampTics(int val)
    {
        if (val < -1)
            return -1;
        if (val > ShortMax)
            return ShortMax;
        return val;
    }

    static string Four(string name)
    {
        if (name.Length >= 4)
            return name.Substring(0, 4);
        return name.PadRight(4, '\0');
    }

    string Unknown(string kind, int num)
    {
        return "Unknown key " + Line1 + " encountered in " + kind + " " + num.ToString() + ".\n";
    }

    static bool IsNum(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if ((c < '0' || c > '9') && c != '-')
                return false;
        }
        return true;
    }

    static List<string> SplitBits(string text)
    {
        var parts = new List<string>();
        int i = 0;
        while (i < text.Length)
        {
            while (i < text.Length && IsSep(text[i]))
                i++;
            if (i >= text.Length)
                break;
            int start = i;
            while (i < text.Length && !IsSep(text[i]))
                i++;
            parts.Add(text.Substring(start, i - start));
        }
        return parts;
    }
    static bool IsSep(char c)
    {
        return c == ',' || c == '+' || c == '|' || c == ' ' || c == '\t' || c == '\f' || c == '\r';
    }
    static int Atoi(string text)
    {
        return unchecked((int)Atoll(text));
    }
    static long Atoll(string text)
    {
        int i = 0;
        while (i < text.Length && IsSpace(text[i]))
            i++;
        int sign = 1;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
        {
            if (text[i] == '-')
                sign = -1;
            i++;
        }
        bool any = false;
        bool overflow = false;
        ulong mag = 0;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            any = true;
            int d = text[i] - '0';
            if (overflow || mag > (ulong.MaxValue - (ulong)d) / 10UL)
                overflow = true;
            else
                mag = mag * 10UL + (ulong)d;
            i++;
        }
        if (!any)
            return 0;
        if (overflow)
            return sign < 0 ? long.MinValue : long.MaxValue;
        if (sign < 0)
        {
            if (mag >= (ulong)long.MaxValue + 1UL)
                return long.MinValue;
            return -(long)mag;
        }
        if (mag > (ulong)long.MaxValue)
            return long.MaxValue;
        return (long)mag;
    }
    static bool IsSpace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
    }
}
