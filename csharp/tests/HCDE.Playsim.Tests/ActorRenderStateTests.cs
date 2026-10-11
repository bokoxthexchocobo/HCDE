using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorRenderStateTests
{
    [Theory]
    [InlineData(false, 0, 0, false)]
    [InlineData(true, 0, 0, false)]
    [InlineData(true, 1, 0, true)]
    [InlineData(true, 1, 0.5, false)]
    [InlineData(true, 2, 1, true)]
    [InlineData(false, 2, 1, false)]
    [InlineData(true, 3, 0, false)]
    public void StoredRenderStateControlsTracingAndReplays(bool dontSeek, int style, double alpha, bool blocked)
    {
        var (sim, missile, target) = Room();
        target.RenderStyle = style switch
        {
            1 => ActorRenderStyle.Translucent,
            2 => new(ActorBlendOperation.None, ActorAlphaFactor.One, ActorAlphaFactor.Zero),
            3 => new(ActorBlendOperation.Fuzz, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource),
            _ => ActorRenderStyle.Normal,
        };
        target.Alpha = alpha; missile.DontSeekInvisible = dontSeek;
        var expectedStyle = target.RenderStyle;
        var bytes = SimSavegame.Write(sim);
        for (var pass = 0; pass < 2; pass++)
        {
            if (pass == 0) missile.Trace(16.875); else sim.Tick();
            Assert.Equal(BamAngle.FromDegrees(blocked ? 0 : 16.875), missile.Angle);
            target.Alpha = 1; target.RenderStyle = ActorRenderStyle.Normal;
            SimSavegame.Apply(sim, bytes);
            Assert.Equal(expectedStyle, target.RenderStyle); Assert.Equal(alpha, target.Alpha);
        }
    }

    [Fact]
    public void LegacyDefaultsAndChecksumsIncludeRenderProperties()
    {
        var (sim, _, target) = Room(); var legacy = SimSavegame.Write(sim);
        sim.RestoreState(sim.CaptureState()); var baseline = sim.Checksum;
        target.Alpha = 0; sim.RestoreState(sim.CaptureState()); var alphaHash = sim.Checksum;
        target.RenderStyle = ActorRenderStyle.Translucent; sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(baseline, alphaHash); Assert.NotEqual(alphaHash, sim.Checksum);
        SimSavegame.Apply(sim, legacy);
        Assert.Equal(ActorRenderStyle.Normal, target.RenderStyle); Assert.Equal(1, target.Alpha);
        Assert.Throws<ArgumentOutOfRangeException>(() => target.Alpha = double.NaN);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidRenderTrailerIsRejectedBeforeApply(bool invalidAlpha)
    {
        var (sim, _, target) = Room(); target.Alpha = 0.5;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(187, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var entry = bytes.AsSpan(bytes.Length - size + 8);
        if (invalidAlpha) BinaryPrimitives.WriteDoubleLittleEndian(entry[4..], double.NaN);
        else BinaryPrimitives.WriteUInt32LittleEndian(entry, 255);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-render-value", error);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(0.5, target.Alpha);
    }

    private static (AuthoritySimulation, ProjectileActor, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var owner = sim.AddBot(0, 0); var target = sim.AddBot(800, 0);
        owner.Brain = target.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.RevenantTracer, target);
        target.Y = Fixed.FromInt(800); missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
