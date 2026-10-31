namespace HCDE.Playsim;

/// <summary>
/// Actor light definitions from <c>a_dynlightdata.cpp</c> (1-304). Chunk 1343.
/// Name lookup is case-insensitive. <c>GetAttenuate</c> returns the attenuate
/// byte, so the default -1 scales new definitions. Pulse setup reads
/// <c>FCycler::SetParams</c> before the offset copy. Frame names occupy eight
/// bytes, and a full frame is length 5 or a 0 in byte 5.
/// </summary>
public sealed class VmDynLightData1343
{
    public const int TicRate = 35;
    public const int NameNone = 0;
    public const int PointLight = 0;
    public const int PulseLight = 1;
    public const int LightIntensity = 3;
    public const int LightSecondary = 4;
    public const int LfAttenuate = 8;
    public const int LfSpot = 64;
    public const int CycleSin = 1;

    public static readonly LightDefaults Missing = new(NameNone);

    public int AttenuationIsSet = -1;
    public int ParsedCapacity;
    public int AllocCount;
    public int Orphans;
    public int DehackedPickup { get; }

    public List<LightDefaults> LightDefaults { get; } = new();
    public List<int> ParsedStateLights { get; } = new();
    public List<LightDefaults?> StateLights { get; } = new();
    public List<string> Sprites { get; } = new();
    public List<ActorClass> Actors { get; } = new();

    readonly List<string> nameText = new() { "None" };
    readonly Dictionary<string, int> nameOf = new(StringComparer.Ordinal) { ["none"] = NameNone };

    public VmDynLightData1343()
    {
        DehackedPickup = Intern("DehackedPickup");
    }

    public int Intern(string? text)
    {
        if (text == null)
        {
            return NameNone;
        }

        var chars = text.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] >= 'A' && chars[i] <= 'Z')
            {
                chars[i] = (char)(chars[i] + 32);
            }
        }

        var key = new string(chars);
        if (nameOf.TryGetValue(key, out var index))
        {
            return index;
        }

        index = nameText.Count;
        nameText.Add(text);
        nameOf.Add(key, index);
        return index;
    }

    public ActorClass AddActor(string name, bool dehacked)
    {
        var actor = new ActorClass(Intern(name), DehackedPickup, dehacked);
        Actors.Add(actor);
        return actor;
    }

    public ActorClass? FindActor(int name)
    {
        foreach (var actor in Actors)
        {
            if (actor.Name == name)
            {
                return actor;
            }
        }

        return null;
    }

    public ActorClass GetRealType(ActorClass ti)
    {
        var rep = ti.GetReplacement(null, false);
        if (!ReferenceEquals(rep, ti) && rep != null && rep.IsDescendantOf(DehackedPickup))
        {
            return rep;
        }

        return ti;
    }

    public Arc SerializeDefaults(Arc arc, string? key, LightDefaults value)
    {
        if (arc.BeginObject(key))
        {
            arc.Field("name", ref value.Name);
            arc.Args("args", value.Args);
            arc.Field("param", ref value.Param);
            arc.Pos("pos", value);
            arc.Field("type", ref value.Type);
            arc.Field("attenuate", ref value.Attenuate);
            arc.Field("flags", ref value.Flags);
            arc.Field("swapped", ref value.Swapped);
            arc.Field("spot", ref value.Spot);
            arc.Field("explicitpitch", ref value.ExplicitPitch);
            arc.Field("spotinner", ref value.SpotInner);
            arc.Field("spotouter", ref value.SpotOuter);
            arc.Field("pitch", ref value.Pitch);
            arc.Field("lightdefintensity", ref value.LightDefIntensity);
            arc.EndObject();
        }

        return arc;
    }

    public Arc SerializeList(Arc arc, string? key, List<LightDefaults> value)
    {
        if (arc.Writing && value.Count == 0)
        {
            return arc;
        }

        var opened = arc.BeginArray(key);
        if (arc.Reading)
        {
            if (!opened)
            {
                value.Clear();
                return arc;
            }

            var count = arc.ArraySize();
            while (value.Count < count)
            {
                value.Add(new LightDefaults(NameNone));
            }

            if (value.Count > count)
            {
                value.RemoveRange(count, value.Count - count);
            }
        }

        for (var i = 0; i < value.Count; i++)
        {
            SerializeDefaults(arc, null, value[i]);
        }

        arc.EndArray();
        return arc;
    }

    public void SetAttenuationForLevel(bool yes)
    {
        var flag = yes ? 1 : 0;
        if (AttenuationIsSet == flag)
        {
            return;
        }

        foreach (var ldef in LightDefaults)
        {
            if (ldef.Attenuate != -1)
            {
                continue;
            }

            if (yes)
            {
                ldef.Flags |= LfAttenuate;
            }
            else
            {
                ldef.Flags &= ~LfAttenuate;
            }
        }

        AttenuationIsSet = flag;
    }

    public void AddLightDefaults(LightDefaults defaults, double factor)
    {
        for (var i = 0; i < LightDefaults.Count; i++)
        {
            if (LightDefaults[i].Name == defaults.Name)
            {
                LightDefaults.RemoveAt(i);
                break;
            }
        }

        if (defaults.GetAttenuate() != 0)
        {
            defaults.Args[LightIntensity] = (int)(defaults.Args[LightIntensity] * factor);
            defaults.Args[LightSecondary] = (int)(defaults.Args[LightSecondary] * factor);
        }

        LightDefaults.Add(defaults);
    }

    public Association Bind(LightAssoc asso)
    {
        AllocCount++;
        var result = new Association();
        for (var i = 0; i < LightDefaults.Count; i++)
        {
            if (LightDefaults[i].Name == asso.LightName)
            {
                result.Light = LightDefaults[i];
                break;
            }
        }

        for (var i = 0; i < Sprites.Count; i++)
        {
            if (Prefix4(Sprites[i], asso.Frame))
            {
                result.Sprite = i;
                break;
            }
        }

        var length = FrameLength(asso.Frame);
        if (length == 5 || asso.Frame[5] == (byte)'0')
        {
            result.Frame = Upper(asso.Frame[4]) - 'A';
        }

        return result;
    }

    public void InitializeActorLights(IReadOnlyList<LightAssoc> associations)
    {
        foreach (var asso in associations)
        {
            var ti = FindActor(asso.ActorName);
            if (ti == null)
            {
                continue;
            }

            ti = GetRealType(ti);
            var bound = Bind(asso);
            if (bound.Light != null)
            {
                ti.Lights.Add(bound);
            }
            else
            {
                Orphans++;
            }
        }

        var amount = ParsedStateLights.Count + 1;
        if (StateLights.Count < amount)
        {
            while (StateLights.Count < amount)
            {
                StateLights.Add(null);
            }
        }
        else if (StateLights.Count != amount)
        {
            StateLights.RemoveRange(amount, StateLights.Count - amount);
        }

        for (var i = 0; i < ParsedStateLights.Count; i++)
        {
            if (ParsedStateLights[i] != NameNone)
            {
                StateLights[i] = Missing;
                for (var j = 0; j < LightDefaults.Count; j++)
                {
                    if (LightDefaults[j].Name == ParsedStateLights[i])
                    {
                        StateLights[i] = LightDefaults[j];
                        break;
                    }
                }
            }
            else
            {
                StateLights[i] = null;
            }
        }

        StateLights[StateLights.Count - 1] = null;
        ParsedStateLights.Clear();
        if (ParsedCapacity > ParsedStateLights.Count)
        {
            ParsedCapacity = ParsedStateLights.Count;
        }
    }

    public void AddStateLight(ActorState state, string? lname)
    {
        if (state.Light == 0)
        {
            PushParsed(NameNone);
            state.Light = (short)PushParsed(Intern(lname));
        }
        else
        {
            PushParsed(Intern(lname));
        }
    }

    int PushParsed(int name)
    {
        if (ParsedStateLights.Count + 1 > ParsedCapacity)
        {
            var choiceA = ParsedStateLights.Count + 1;
            var choiceB = ParsedCapacity >= 16 ? ParsedCapacity + ParsedCapacity / 2 : 16;
            ParsedCapacity = choiceA > choiceB ? choiceA : choiceB;
        }

        var index = ParsedStateLights.Count;
        ParsedStateLights.Add(name);
        return index;
    }

    static bool Prefix4(string sprite, byte[] frame)
    {
        for (var i = 0; i < 4; i++)
        {
            var left = i < sprite.Length ? (byte)sprite[i] : (byte)0;
            if (left != frame[i])
            {
                return false;
            }
        }

        return true;
    }

    static int FrameLength(byte[] frame)
    {
        var length = 0;
        while (length < 8 && frame[length] != 0)
        {
            length++;
        }

        return length;
    }

    static int Upper(byte value)
    {
        if (value >= (byte)'a' && value <= (byte)'z')
        {
            return value - 32;
        }

        return value;
    }
}

public sealed class LightDefaults
{
    public LightDefaults(int name, int type = VmDynLightData1343.PointLight)
    {
        Name = name;
        Type = type;
    }

    public int Name;
    public int[] Args = new int[5];
    public double Param;
    public double PosX;
    public double PosY;
    public double PosZ;
    public int Type;
    public sbyte Attenuate = -1;
    public int Flags;
    public bool Swapped;
    public bool Spot;
    public bool ExplicitPitch;
    public double SpotInner = 10;
    public double SpotOuter = 25;
    public double Pitch;
    public double LightDefIntensity = 1;

    public byte GetAttenuate()
    {
        return unchecked((byte)Attenuate);
    }

    public void ApplyProperties(DynLight light)
    {
        var oldType = light.LightType;
        light.Trace.Add("assign");
        light.Active = true;
        light.LightType = Type;
        light.Special = Param;
        light.LightDefIntensity = LightDefIntensity;
        light.Args = Args;
        light.FlagOwner = this;
        if ((Flags & VmDynLightData1343.LfSpot) != 0)
        {
            light.Trace.Add("spot");
            light.SpotOwner = this;
            light.ExplicitPitch = ExplicitPitch;
            if (light.Target == null)
            {
                throw new NullReferenceException("target");
            }

            light.Yaw = light.Target.Yaw;
            light.Pitch = ExplicitPitch ? Pitch : light.Target.Pitch;
        }

        light.TickCount = 0;
        if (Type == VmDynLightData1343.PulseLight)
        {
            light.Trace.Add("pulse");
            var pulseTime = (float)(Param / VmDynLightData1343.TicRate);
            light.LastUpdate = light.Timer;
            if (Swapped)
            {
                light.Cycler.SetParams(Args[VmDynLightData1343.LightSecondary], Args[VmDynLightData1343.LightIntensity], pulseTime, oldType == VmDynLightData1343.PulseLight);
            }
            else
            {
                light.Cycler.SetParams(Args[VmDynLightData1343.LightIntensity], Args[VmDynLightData1343.LightSecondary], pulseTime, oldType == VmDynLightData1343.PulseLight);
            }

            light.Cycler.ShouldCycle(true);
            light.Cycler.SetCycleType(VmDynLightData1343.CycleSin);
            light.Radius = (float)light.Cycler.GetVal();
            if (light.Radius <= 0)
            {
                light.Radius = 1;
            }

            light.Swapped = Swapped;
        }

        light.SetOffset(PosX, PosY, PosZ);
    }
}

public sealed class DynLight
{
    public bool Active;
    public int LightType;
    public double Special;
    public double LightDefIntensity;
    public int[]? Args;
    public LightDefaults? FlagOwner;
    public LightDefaults? SpotOwner;
    public bool ExplicitPitch;
    public double Yaw;
    public double Pitch;
    public int TickCount;
    public int LastUpdate;
    public int Timer;
    public float Radius;
    public bool Swapped;
    public LightTarget? Target;
    public double OffX;
    public double OffY;
    public double OffZ;
    public Cycler Cycler { get; } = new();
    public List<string> Trace { get; } = new();

    public void SetOffset(double x, double y, double z)
    {
        Trace.Add("offset");
        OffX = x;
        OffY = y;
        OffZ = z;
    }
}

public sealed class LightTarget
{
    public double Yaw;
    public double Pitch;
}

public sealed class Cycler
{
    public double Start;
    public double End;
    public double Current;
    public double Time;
    public double Cycle;
    public bool Increment;
    public bool Should;
    public int CycleType;

    public void SetParams(double start, double end, double cycle, bool update)
    {
        if (!update || cycle != Cycle)
        {
            Cycle = cycle;
            Time = 0;
            Increment = true;
            Current = start;
        }
        else
        {
            var fact = (Current - Start) / (End - Start);
            Current = start + fact * (end - start);
        }

        Start = start;
        End = end;
    }

    public void ShouldCycle(bool cycle)
    {
        Should = cycle;
    }

    public void SetCycleType(int cycleType)
    {
        CycleType = cycleType;
    }

    public double GetVal()
    {
        return Current;
    }
}

public sealed class ActorClass
{
    public ActorClass(int name, int dehackedName, bool dehacked)
    {
        Name = name;
        DehackedName = dehackedName;
        Dehacked = dehacked;
    }

    public int Name;
    public int DehackedName;
    public bool Dehacked;
    public ActorClass? Replacement;
    public bool ReplacementReturnsNull;
    public bool SawReplacement;
    public object? SeenLevel;
    public bool SeenLook;
    public int DescendantChecks;
    public List<Association> Lights { get; } = new();

    public ActorClass? GetReplacement(object? level, bool lookSkill)
    {
        SawReplacement = true;
        SeenLevel = level;
        SeenLook = lookSkill;
        if (ReplacementReturnsNull)
        {
            return null;
        }

        return Replacement ?? this;
    }

    public bool IsDescendantOf(int name)
    {
        DescendantChecks++;
        return Dehacked && name == DehackedName;
    }
}

public sealed class ActorState
{
    public short Light;
}

public sealed class LightAssoc
{
    public LightAssoc(int actorName, string frameName, int lightName)
    {
        ActorName = actorName;
        LightName = lightName;
        var count = frameName.Length < 8 ? frameName.Length : 8;
        for (var i = 0; i < count; i++)
        {
            Frame[i] = (byte)frameName[i];
        }
    }

    public int ActorName;
    public int LightName;
    public byte[] Frame { get; } = new byte[8];
}

public sealed class Association
{
    public int Sprite = -1;
    public int Frame = -1;
    public LightDefaults? Light;
}

public sealed class Arc
{
    public bool Writing = true;
    public bool Reading;
    public bool OpenObject = true;
    public bool OpenArray = true;
    public int ReadArraySize;
    public List<string> Ops { get; } = new();
    public List<object?> Values = new();
    public int Cursor;

    public bool BeginObject(string? key)
    {
        Ops.Add(key == null ? "O" : "O:" + key);
        return OpenObject;
    }

    public void EndObject()
    {
        Ops.Add("o");
    }

    public bool BeginArray(string? key)
    {
        Ops.Add(key == null ? "A" : "A:" + key);
        return OpenArray;
    }

    public void EndArray()
    {
        Ops.Add("a");
    }

    public int ArraySize()
    {
        return ReadArraySize;
    }

    public void Field<T>(string key, ref T value)
    {
        Ops.Add(key);
        if (Writing)
        {
            Values.Add(value);
            return;
        }

        var boxed = Values[Cursor++];
        if (boxed == null)
        {
            throw new InvalidOperationException(key);
        }

        value = (T)boxed;
    }

    public void Args(string key, int[] args)
    {
        Ops.Add(key);
        if (Writing)
        {
            for (var i = 0; i < 5; i++)
            {
                Values.Add(args[i]);
            }

            return;
        }

        for (var i = 0; i < 5; i++)
        {
            var boxed = Values[Cursor++];
            if (boxed == null)
            {
                throw new InvalidOperationException(key);
            }

            args[i] = (int)boxed;
        }
    }

    public void Pos(string key, LightDefaults value)
    {
        Ops.Add(key);
        if (Writing)
        {
            Values.Add(value.PosX);
            Values.Add(value.PosY);
            Values.Add(value.PosZ);
            return;
        }

        value.PosX = TakeDouble(key);
        value.PosY = TakeDouble(key);
        value.PosZ = TakeDouble(key);
    }

    double TakeDouble(string key)
    {
        var boxed = Values[Cursor++];
        if (boxed == null)
        {
            throw new InvalidOperationException(key);
        }

        return (double)boxed;
    }
}
