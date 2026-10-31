namespace HCDE.Playsim;

public sealed class VmSwitchBand1374Slice
{
    public const int Top = 0;
    public const int Mid = 1;
    public const int Bottom = 2;
    public const int NoSound = 0;
    public const int ExitNormal = 243;
    public const int ExitSecret = 244;
    public const int TeleportNewMap = 74;
    public const int TeleportEndGame = 75;
    public const int ButtonTime = 35;
    public const int ChanVoice = 2;
    public const int ChanfListenerZ = 8;
    public const float SoundVolume = 1f;
    public const float AttnStatic = 3f;
    public const int InvalidTexture = -1;
    public const string ExitSoundName = "switches/exitbutn";
    public const string NormSoundName = "switches/normbutn";
    public const string ClassName = "DActiveButton";
    public const bool ClassIsAbstract = false;
    public const bool ClassHasPointers = false;

    public Func<int, SwitchDef?>? FindSwitch { get; set; }
    public Func<string, int>? FindSound { get; set; }
    public Func<WallSide, int, SwitchDef, double, double, bool, bool>? StartButton { get; set; }
    public Func<int, uint>? SwitchAnim { get; set; }
    public Action<FieldArc>? SuperSerialize { get; set; }
    public List<SoundPlay> Sounds { get; } = [];

    public static BandButton InPlaceConstruct() => new();

    public bool ChangeSwitchTexture(WallSide side, int useAgain, byte special, QuestSlot? quest)
    {
        var sw = Lookup(side.GetTexture(Top));
        var part = Top;
        if (sw == null)
        {
            sw = Lookup(side.GetTexture(Mid));
            part = Mid;
            if (sw == null)
            {
                sw = Lookup(side.GetTexture(Bottom));
                part = Bottom;
            }
        }
        if (sw == null)
        {
            if (quest != null)
                quest.Value = false;
            return false;
        }

        int soundId;
        if (sw.Sound != NoSound)
            soundId = sw.Sound;
        else
        {
            var exit = special == ExitNormal || special == ExitSecret
                || special == TeleportNewMap || special == TeleportEndGame;
            soundId = RequireSound(exit ? ExitSoundName : NormSoundName);
        }

        var x = side.V1X + side.DeltaX / 2d;
        var y = side.V1Y + side.DeltaY / 2d;
        side.SetTexture(part, sw.Frames[0].Texture);
        var playsound = true;
        if (useAgain != 0 || sw.NumFrames > 1)
        {
            if (StartButton == null)
                throw new InvalidOperationException("P_StartButton");
            playsound = StartButton(side, part, sw, x, y, useAgain != 0);
        }
        if (playsound)
        {
            Sounds.Add(new SoundPlay(
                side.Level, x, y, 0d, ChanVoice, ChanfListenerZ, soundId, SoundVolume, AttnStatic));
        }
        if (quest != null)
            quest.Value = sw.QuestPanel;
        return true;
    }

    public SwitchDef? SerializeSwitch(FieldArc arc, string key, SwitchDef? sw)
    {
        arc.Keys.Add(key);
        if (arc.Writing)
        {
            arc.SwitchTexture = sw!.PreTexture;
            return sw;
        }
        var tex = InvalidTexture;
        arc.TextureBeforeRead = tex;
        tex = arc.SwitchTexture;
        return Lookup(tex);
    }

    public int RequireSound(string name)
    {
        if (FindSound == null)
            throw new InvalidOperationException("S_FindSound");
        return FindSound(name);
    }

    SwitchDef? Lookup(int texture)
    {
        if (FindSwitch == null)
            throw new InvalidOperationException("FindSwitch");
        return FindSwitch(texture);
    }
}

public sealed class WallSide
{
    public int Top, Mid, Bottom;
    public double V1X, V1Y, DeltaX, DeltaY;
    public string? Level;
    public List<(int Part, int Texture)> Sets { get; } = [];

    public int GetTexture(int part)
    {
        if (part == VmSwitchBand1374Slice.Top)
            return Top;
        if (part == VmSwitchBand1374Slice.Mid)
            return Mid;
        if (part == VmSwitchBand1374Slice.Bottom)
            return Bottom;
        throw new InvalidOperationException("GetTexture");
    }

    public void SetTexture(int part, int texture) => Sets.Add((part, texture));
}

public sealed class BandFrame
{
    public int TimeMin;
    public int TimeRnd;
    public int Texture;
}

public sealed class SwitchDef
{
    public int PreTexture;
    public SwitchDef? Pair;
    public int NumFrames;
    public bool QuestPanel;
    public int Sound;
    public List<BandFrame> Frames { get; } = [];
}

public sealed class QuestSlot
{
    public bool Value;
}

public sealed class FieldArc
{
    public bool Writing;
    public bool SuperCalled;
    public int SwitchTexture = VmSwitchBand1374Slice.InvalidTexture;
    public int TextureBeforeRead = int.MinValue;
    public WallSide? Side;
    public sbyte Part;
    public int Frame;
    public uint Timer;
    public bool Fippable;
    public double X;
    public double Y;
    public bool Returning;
    public List<string> Keys { get; } = [];
}

public readonly record struct SoundPlay(
    string? Level, double X, double Y, double Z,
    int Channel, int Flags, int Sound, float Volume, float Attenuation);

public sealed class BandButton
{
    public WallSide? Side;
    public sbyte Part;
    public double PosX;
    public double PosY;
    public bool Flippable;
    public bool Returning;
    public SwitchDef? SwitchDef;
    public int Frame;
    public uint Timer;
    public string? Level;
    public int DestroyCount;
    public List<SoundPlay> Sounds { get; } = [];

    public void Construct(WallSide side, int where, SwitchDef sw, double x, double y, bool useAgain, VmSwitchBand1374Slice host)
    {
        Side = side;
        Part = (sbyte)where;
        PosX = x;
        PosY = y;
        Flippable = useAgain;
        Returning = false;
        SwitchDef = sw;
        Frame = -1;
        AdvanceFrame(host);
    }

    public void Serialize(FieldArc arc, VmSwitchBand1374Slice host)
    {
        if (host.SuperSerialize == null)
            throw new InvalidOperationException("Serialize");
        host.SuperSerialize(arc);
        arc.SuperCalled = true;
        if (arc.Writing)
        {
            arc.Keys.Add("side");
            arc.Side = Side;
            arc.Keys.Add("part");
            arc.Part = Part;
            host.SerializeSwitch(arc, "switchdef", SwitchDef);
            arc.Keys.Add("frame");
            arc.Frame = Frame;
            arc.Keys.Add("timer");
            arc.Timer = Timer;
            arc.Keys.Add("fippable");
            arc.Fippable = Flippable;
            arc.Keys.Add("pos");
            arc.X = PosX;
            arc.Y = PosY;
            arc.Keys.Add("returning");
            arc.Returning = Returning;
            return;
        }

        arc.Keys.Add("side");
        Side = arc.Side;
        arc.Keys.Add("part");
        Part = arc.Part;
        SwitchDef = host.SerializeSwitch(arc, "switchdef", SwitchDef);
        arc.Keys.Add("frame");
        Frame = arc.Frame;
        arc.Keys.Add("timer");
        Timer = arc.Timer;
        arc.Keys.Add("fippable");
        Flippable = arc.Fippable;
        arc.Keys.Add("pos");
        PosX = arc.X;
        PosY = arc.Y;
        arc.Keys.Add("returning");
        Returning = arc.Returning;
    }

    public void Tick(VmSwitchBand1374Slice host)
    {
        if (SwitchDef == null)
        {
            Destroy();
            return;
        }

        var def = Returning ? SwitchDef.Pair : SwitchDef;
        uint next;
        unchecked
        {
            next = Timer - 1;
        }
        Timer = next;
        if (next != 0)
            return;

        if (Frame == def!.NumFrames - 1)
        {
            Returning = true;
            def = SwitchDef.Pair;
            if (def != null)
            {
                Frame = -1;
                var soundId = def.Sound != VmSwitchBand1374Slice.NoSound
                    ? def.Sound
                    : host.RequireSound(VmSwitchBand1374Slice.NormSoundName);
                Sounds.Add(new SoundPlay(
                    Level, PosX, PosY, 0d,
                    VmSwitchBand1374Slice.ChanVoice, VmSwitchBand1374Slice.ChanfListenerZ,
                    soundId, VmSwitchBand1374Slice.SoundVolume, VmSwitchBand1374Slice.AttnStatic));
                Flippable = false;
            }
            else
            {
                Destroy();
                return;
            }
        }

        var kill = AdvanceFrame(host);
        Side!.SetTexture(Part, def.Frames[Frame].Texture);
        if (kill)
            Destroy();
    }

    public bool AdvanceFrame(VmSwitchBand1374Slice host)
    {
        var ret = false;
        var def = Returning ? SwitchDef!.Pair! : SwitchDef!;
        Frame++;
        if (Frame == def.NumFrames - 1)
        {
            if (Flippable)
                Timer = (uint)VmSwitchBand1374Slice.ButtonTime;
            else
                ret = true;
        }
        else
        {
            var frame = def.Frames[Frame];
            Timer = (uint)frame.TimeMin;
            if (frame.TimeRnd != 0)
            {
                if (host.SwitchAnim == null)
                    throw new InvalidOperationException("pr_switchanim");
                Timer += host.SwitchAnim(frame.TimeRnd);
            }
        }
        return ret;
    }

    void Destroy() => DestroyCount++;
}
