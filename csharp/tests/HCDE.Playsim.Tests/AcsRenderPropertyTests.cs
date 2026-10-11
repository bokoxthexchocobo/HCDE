using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsRenderPropertyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(64)]
    [InlineData(65)]
    [InlineData(66)]
    [InlineData(67)]
    [InlineData(68)]
    [InlineData(69)]
    public void NativeStyleIndicesSetGetCheckAndSave(int index)
    {
        var sim = Room(); var actor = sim.AddBot(100, 0);
        AcsActorProperties.Set(sim, actor, 0, 4, index);
        Assert.Equal(index, AcsActorProperties.Get(sim, actor, 0, 4));
        Assert.True(AcsActorProperties.Check(sim, actor, 0, 4, index));
        Assert.False(AcsActorProperties.Check(sim, actor, 0, 4, 99));
        var style = actor.RenderStyle; var bytes = SimSavegame.Write(sim);
        actor.RenderStyle = ActorRenderStyle.Translucent;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(style, actor.RenderStyle);
    }

    [Theory]
    [InlineData(-32768)]
    [InlineData(0)]
    [InlineData(16384)]
    [InlineData(65536)]
    [InlineData(131072)]
    public void AlphaUsesUnclampedFixedPointAndTidSelection(int value)
    {
        var sim = Room(); var first = sim.AddBot(100, 0); var second = sim.AddBot(300, 0);
        first.ThingId = second.ThingId = 9;
        AcsActorProperties.Set(sim, null, 9, 3, value);
        Assert.Equal(value / 65536.0, first.Alpha); Assert.Equal(first.Alpha, second.Alpha);
        Assert.Equal(value, AcsActorProperties.Get(sim, null, 9, 3));
        Assert.True(AcsActorProperties.Check(sim, null, 9, 3, value));
        var bytes = SimSavegame.Write(sim); first.Alpha = second.Alpha = 1;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(value, AcsActorProperties.Get(sim, first, 0, 3));
        Assert.Equal(value, AcsActorProperties.Get(sim, second, 0, 3));
    }

    [Fact]
    public void InvalidStyleWritesAndCustomStyleReadFollowNativeFallback()
    {
        var sim = Room(); var actor = sim.AddBot(100, 0);
        actor.RenderStyle = ActorRenderStyle.OptFuzzy;
        foreach (var invalid in new[] { -1, 8, 63, 70, int.MaxValue })
        {
            AcsActorProperties.Set(sim, actor, 0, 4, invalid);
            Assert.Equal(ActorRenderStyle.OptFuzzy, actor.RenderStyle);
        }
        actor.RenderStyle = new(ActorBlendOperation.Subtract, ActorAlphaFactor.One, ActorAlphaFactor.Zero);
        Assert.Equal(1, AcsActorProperties.Get(sim, actor, 0, 4));
        Assert.True(AcsActorProperties.Check(sim, actor, 0, 4, 1));
        Assert.Equal(0, AcsActorProperties.Get(sim, null, 0, 3));
        Assert.False(AcsActorProperties.Check(sim, null, 0, 4, 0));
    }

    [Theory]
    [InlineData(0.5, 0)]
    [InlineData(1.5, 2)]
    [InlineData(-0.5, 0)]
    [InlineData(-1.5, -2)]
    public void AlphaReadRoundsFixedPointTiesToEven(double units, int expected)
    {
        var sim = Room(); var actor = sim.AddBot(100, 0); actor.Alpha = units / 65536;
        Assert.Equal(expected, AcsActorProperties.Get(sim, actor, 0, 3));
    }

    [Fact]
    public void AcsBytecodeSetsRenderPropertiesAndChangesSeeking()
    {
        var sim = Room(); var actor = sim.AddBot(100, 0);
        int[] words = [(int)AcsPcode.PushNumber, 0, (int)AcsPcode.PushNumber, 3,
            (int)AcsPcode.PushNumber, 0, (int)AcsPcode.SetActorProperty,
            (int)AcsPcode.PushNumber, 0, (int)AcsPcode.PushNumber, 4,
            (int)AcsPcode.PushNumber, 64, (int)AcsPcode.SetActorProperty];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, actor));
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
        Assert.Equal(0, actor.Alpha); Assert.Equal(ActorRenderStyle.Translucent, actor.RenderStyle);
        Assert.False(new Actor { DontSeekInvisible = true }.CanSeek(actor));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }] });
}
