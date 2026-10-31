namespace HCDE.Playsim.Tests;

public class VmSwitchBand1374SliceTests
{
    [Fact]
    public void ChangeSwitchTextureSelectsPartSoundAndButton()
    {
        var host = new VmSwitchBand1374Slice();
        var top = Def(10, 7, 1, 40, quest: true);
        var mid = Def(11, 8, 1, 50);
        var bottom = Def(12, 9, 2, 60, timeMin: 3);
        var seen = new List<int>();
        host.FindSwitch = id =>
        {
            seen.Add(id);
            if (id == 10) return top;
            if (id == 11) return mid;
            if (id == 12) return bottom;
            return null;
        };
        var side = new WallSide
        {
            Top = 10, Mid = 11, Bottom = 12,
            V1X = 2, V1Y = 4, DeltaX = 6, DeltaY = 8,
            Level = "map",
        };
        var quest = new QuestSlot { Value = false };
        Assert.True(host.ChangeSwitchTexture(side, 0, 0, quest));
        Assert.Equal(new[] { 10 }, seen);
        Assert.Equal((VmSwitchBand1374Slice.Top, 40), Assert.Single(side.Sets));
        Assert.True(quest.Value);
        var played = Assert.Single(host.Sounds);
        Assert.Equal("map", played.Level);
        Assert.Equal(5d, played.X);
        Assert.Equal(8d, played.Y);
        Assert.Equal(0d, played.Z);
        Assert.Equal(VmSwitchBand1374Slice.ChanVoice, played.Channel);
        Assert.Equal(VmSwitchBand1374Slice.ChanfListenerZ, played.Flags);
        Assert.Equal(7, played.Sound);
        Assert.Equal(1f, played.Volume);
        Assert.Equal(3f, played.Attenuation);
        Assert.Equal("DActiveButton", VmSwitchBand1374Slice.ClassName);
        Assert.False(VmSwitchBand1374Slice.ClassIsAbstract);
        Assert.False(VmSwitchBand1374Slice.ClassHasPointers);
        Assert.NotNull(VmSwitchBand1374Slice.InPlaceConstruct());

        seen.Clear();
        side.Sets.Clear();
        host.Sounds.Clear();
        side.Top = 1;
        Assert.True(host.ChangeSwitchTexture(side, 0, 1, null));
        Assert.Equal(new[] { 1, 11 }, seen);
        Assert.Equal((VmSwitchBand1374Slice.Mid, 50), Assert.Single(side.Sets));

        seen.Clear();
        side.Sets.Clear();
        side.Mid = 2;
        bool started = false;
        bool again = true;
        int startedPart = -1;
        host.StartButton = (_, part, _, _, _, useAgain) =>
        {
            started = true;
            again = useAgain;
            startedPart = part;
            return true;
        };
        Assert.True(host.ChangeSwitchTexture(side, 0, 9, null));
        Assert.Equal(new[] { 1, 2, 12 }, seen);
        Assert.True(started);
        Assert.False(again);
        Assert.Equal(VmSwitchBand1374Slice.Bottom, startedPart);
        Assert.Equal(60, Assert.Single(side.Sets).Texture);

        var missed = new QuestSlot { Value = true };
        side.Bottom = 3;
        Assert.False(host.ChangeSwitchTexture(side, 1, 243, missed));
        Assert.False(missed.Value);
        Assert.False(host.ChangeSwitchTexture(side, 1, 243, null));

        host.FindSound = name => name == VmSwitchBand1374Slice.ExitSoundName ? 4 : 5;
        foreach (var special in new byte[] { 243, 244, 74, 75 })
        {
            host.Sounds.Clear();
            side.Top = 10;
            top.Sound = 0;
            Assert.True(host.ChangeSwitchTexture(side, 0, special, null));
            Assert.Equal(4, host.Sounds[^1].Sound);
        }
        host.Sounds.Clear();
        Assert.True(host.ChangeSwitchTexture(side, 0, 1, null));
        Assert.Equal(5, host.Sounds[^1].Sound);

        side.Sets.Clear();
        host.Sounds.Clear();
        top.Sound = 7;
        top.NumFrames = 1;
        again = true;
        Assert.True(host.ChangeSwitchTexture(side, 2, 0, null));
        Assert.True(again);
        Assert.Single(host.Sounds);

        host.Sounds.Clear();
        host.StartButton = (_, _, _, _, _, _) => false;
        Assert.True(host.ChangeSwitchTexture(side, 2, 0, null));
        Assert.Empty(host.Sounds);
        Assert.Equal(40, side.Sets[^1].Texture);
    }

    [Fact]
    public void ButtonTickSerializeAndAdvance()
    {
        var host = new VmSwitchBand1374Slice();
        var sw = Def(3, 0, 2, 70, timeMin: 5);
        var side = new WallSide { Level = "map" };
        var button = new BandButton();
        button.Construct(side, 128, sw, 1.5, 2.5, true, host);
        Assert.Equal((sbyte)-128, button.Part);
        Assert.Equal(0, button.Frame);
        Assert.Equal(5u, button.Timer);
        Assert.True(button.Flippable);
        Assert.False(button.Returning);
        Assert.Same(sw, button.SwitchDef);
        Assert.Equal(0, button.DestroyCount);

        button.Timer = 2;
        button.Tick(host);
        Assert.Equal(1u, button.Timer);
        Assert.Empty(side.Sets);
        button.Tick(host);
        Assert.Equal(1, button.Frame);
        Assert.Equal((uint)VmSwitchBand1374Slice.ButtonTime, button.Timer);
        Assert.Equal((-128, 71), Assert.Single(side.Sets));
        Assert.Equal(0, button.DestroyCount);

        var wrapped = new BandButton { SwitchDef = Def(1, 1, 1, 8), Timer = 0, Frame = 0, Flippable = false };
        wrapped.Tick(host);
        Assert.Equal(uint.MaxValue, wrapped.Timer);
        Assert.Equal(0, wrapped.DestroyCount);

        var lost = new BandButton { Timer = 4 };
        lost.Tick(host);
        Assert.Equal(1, lost.DestroyCount);
        Assert.Equal(4u, lost.Timer);

        var stuck = new BandButton
        {
            SwitchDef = Def(1, 1, 1, 8),
            Frame = 0,
            Timer = 1,
            Flippable = true,
            Side = side,
        };
        side.Sets.Clear();
        stuck.Tick(host);
        Assert.Equal(1, stuck.DestroyCount);
        Assert.True(stuck.Returning);
        Assert.Equal(0, stuck.Frame);
        Assert.Empty(side.Sets);

        var pair = Def(9, 0, 2, 90, timeMin: 4, timeRnd: 3);
        var paired = Def(4, 6, 1, 80);
        paired.Pair = pair;
        var animBound = -1;
        host.SwitchAnim = bound =>
        {
            animBound = bound;
            return 2;
        };
        string? asked = null;
        host.FindSound = name =>
        {
            asked = name;
            return 11;
        };
        var returning = new BandButton
        {
            SwitchDef = paired,
            Frame = 0,
            Timer = 1,
            Flippable = true,
            Side = side,
            Level = "thinker",
            PosX = 3,
            PosY = 4,
        };
        side.Sets.Clear();
        returning.Tick(host);
        Assert.Equal(3, animBound);
        Assert.True(returning.Returning);
        Assert.False(returning.Flippable);
        Assert.Equal(0, returning.Frame);
        Assert.Equal(6u, returning.Timer);
        Assert.Equal(0, returning.DestroyCount);
        Assert.Equal((0, 90), Assert.Single(side.Sets));
        var heard = Assert.Single(returning.Sounds);
        Assert.Equal("thinker", heard.Level);
        Assert.Equal(3d, heard.X);
        Assert.Equal(4d, heard.Y);
        Assert.Equal(0d, heard.Z);
        Assert.Equal(11, heard.Sound);
        Assert.Equal(VmSwitchBand1374Slice.NormSoundName, asked);

        var where = new BandButton();
        where.Construct(side, 255, sw, 0, 0, false, host);
        Assert.Equal((sbyte)-1, where.Part);

        var super = 0;
        host.SuperSerialize = arc =>
        {
            super++;
            Assert.Empty(arc.Keys);
        };
        var source = new BandButton
        {
            Side = side,
            Part = -128,
            SwitchDef = paired,
            Frame = 4,
            Timer = 9,
            Flippable = true,
            PosX = 1.25,
            PosY = -2,
            Returning = true,
        };
        var arc = new FieldArc { Writing = true };
        source.Serialize(arc, host);
        Assert.Equal(1, super);
        Assert.True(arc.SuperCalled);
        Assert.Equal(new[] { "side", "part", "switchdef", "frame", "timer", "fippable", "pos", "returning" }, arc.Keys);
        Assert.Same(side, arc.Side);
        Assert.Equal((sbyte)-128, arc.Part);
        Assert.Equal(paired.PreTexture, arc.SwitchTexture);
        Assert.Equal(4, arc.Frame);
        Assert.Equal(9u, arc.Timer);
        Assert.True(arc.Fippable);
        Assert.Equal(1.25, arc.X);
        Assert.Equal(-2d, arc.Y);
        Assert.True(arc.Returning);

        var restoredSide = new WallSide();
        var read = new FieldArc
        {
            Writing = false,
            Side = restoredSide,
            Part = 2,
            SwitchTexture = 42,
            Frame = 6,
            Timer = 8,
            Fippable = false,
            X = 9,
            Y = 10,
            Returning = true,
        };
        host.FindSwitch = id =>
        {
            Assert.Equal(42, id);
            return paired;
        };
        var loaded = new BandButton();
        loaded.Serialize(read, host);
        Assert.Equal(-1, read.TextureBeforeRead);
        Assert.Same(restoredSide, loaded.Side);
        Assert.Equal((sbyte)2, loaded.Part);
        Assert.Same(paired, loaded.SwitchDef);
        Assert.Equal(6, loaded.Frame);
        Assert.Equal(8u, loaded.Timer);
        Assert.False(loaded.Flippable);
        Assert.Equal(9d, loaded.PosX);
        Assert.Equal(10d, loaded.PosY);
        Assert.True(loaded.Returning);
        Assert.Contains("fippable", read.Keys);
    }

    static SwitchDef Def(int pre, int sound, int frames, int texture, int timeMin = 1, int timeRnd = 0, bool quest = false)
    {
        var def = new SwitchDef
        {
            PreTexture = pre,
            Sound = sound,
            NumFrames = frames,
            QuestPanel = quest,
        };
        for (var i = 0; i < frames; i++)
        {
            def.Frames.Add(new BandFrame
            {
                Texture = texture + i,
                TimeMin = timeMin,
                TimeRnd = timeRnd,
            });
        }
        return def;
    }
}
