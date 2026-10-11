using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SeekVisibilityFlagTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    public void VisibilityFlagsGateDirectAndPeriodicTracingAndSurviveSave(int flags, bool dontSeek)
    {
        var (sim, missile, target) = Room();
        Assert.True(AcsActorFlags.TrySet(target, "shadow", (flags & 1) != 0));
        Assert.True(AcsActorFlags.TrySet(target, "minvisible", (flags & 2) != 0));
        Assert.True(AcsActorFlags.TryGet(target, "SHADOW", out var shadow));
        Assert.True(AcsActorFlags.TryGet(target, "MINVISIBLE", out var minVisible));
        Assert.Equal((flags & 1) != 0, shadow); Assert.Equal((flags & 2) != 0, minVisible);
        missile.DontSeekInvisible = dontSeek;
        var state = sim.CaptureState(); target.Shadow = target.MinVisible = false;
        sim.RestoreState(state);
        Assert.Equal(shadow, target.Shadow); Assert.Equal(minVisible, target.MinVisible);
        var bytes = SimSavegame.Write(sim);
        var blocked = dontSeek && flags != 0;
        for (var pass = 0; pass < 2; pass++)
        {
            if (pass == 0) missile.Trace(16.875); else sim.Tick();
            Assert.Equal(BamAngle.FromDegrees(blocked ? 0 : 16.875), missile.Angle);
            Assert.Equal(blocked ? 0 : -0.125, missile.VelocityZ.ToDouble());
            target.Shadow = target.MinVisible = false;
            SimSavegame.Apply(sim, bytes);
            Assert.Equal(shadow, target.Shadow); Assert.Equal(minVisible, target.MinVisible);
        }
    }

    [Fact]
    public void LegacyDefaultsAndChecksumsDistinguishFlags()
    {
        var (sim, _, target) = Room(); var bytes = SimSavegame.Write(sim);
        var hashes = new HashSet<uint>();
        for (var flags = 0; flags < 4; flags++)
        {
            target.Shadow = (flags & 1) != 0; target.MinVisible = (flags & 2) != 0;
            sim.RestoreState(sim.CaptureState());
            Assert.True(hashes.Add(sim.Checksum));
        }
        SimSavegame.Apply(sim, bytes);
        Assert.False(target.Shadow); Assert.False(target.MinVisible);
    }

    [Fact]
    public void InvalidVisibilityBitsAreRejectedWithoutApplyingState()
    {
        var (sim, _, target) = Room(); target.Shadow = target.MinVisible = true;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(185, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 4);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-seekvisibility-value", error);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.True(target.Shadow); Assert.True(target.MinVisible);
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
