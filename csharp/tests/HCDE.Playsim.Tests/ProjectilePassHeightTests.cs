using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectilePassHeightTests
{
    [Theory]
    [InlineData(0, false, 20, false)]
    [InlineData(16, false, 20, true)]
    [InlineData(16, false, 16, false)]
    [InlineData(-16, false, 20, false)]
    [InlineData(-16, true, 20, true)]
    [InlineData(-16, true, 16, false)]
    [InlineData(80, false, 60, false)]
    public void NativeHeightAndCompatibilityRules(int height, bool clip, int z, bool passes)
    {
        var (_, missile, target) = Setup(clip); target.ProjectilePassHeight = Fixed.FromInt(height);
        missile.Z = Fixed.FromInt(z); missile.Tick();
        Assert.Equal(!passes, missile.Destroyed); Assert.Equal(passes ? 1000 : 993, target.Health);
        Assert.Equal(56, target.Height.ToDouble());
    }

    [Fact]
    public void SignedHeightRoundTripsWithRipperRules()
    {
        var (sim, missile, target) = Setup(true); target.ProjectilePassHeight = Fixed.FromInt(-16);
        missile.Rip = true; missile.RipperLevel = 2; target.Pushable = true;
        var bytes = SimSavegame.Write(sim); Assert.Equal(61, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var (restored, loaded, victim) = Setup(true); restored.RestoreState(state);
        Assert.Equal(Fixed.FromInt(-16), victim.ProjectilePassHeight); Assert.True(loaded.Rip);
        Assert.Equal(2, loaded.RipperLevel); Assert.True(victim.Pushable);
        loaded.Tick(); Assert.False(loaded.Destroyed); Assert.Equal(1000, victim.Health);
    }

    [Fact]
    public void SnapshotRestoresAndLegacyClearsHeight()
    {
        var (sim, _, target) = Setup(false); var legacy = SimSavegame.Write(sim);
        target.ProjectilePassHeight = Fixed.FromInt(16); var pose = sim.CaptureState();
        target.ProjectilePassHeight = default; sim.RestoreState(pose); Assert.Equal(Fixed.FromInt(16), target.ProjectilePassHeight);
        Assert.True(SimSavegame.TryRead(legacy, out var state, out var error), error); sim.RestoreState(state);
        Assert.Equal(default, target.ProjectilePassHeight); Assert.Equal(legacy, SimSavegame.Write(sim));
    }

    [Fact]
    public void RecursiveTrailerVersionIsRejected()
    {
        var (sim, _, target) = Setup(false); target.ProjectilePassHeight = Fixed.FromInt(16);
        var bytes = SimSavegame.Write(sim); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size), 61);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-projectile-pass-height-header", error);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Setup(bool clip)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] },
            compat: clip ? CompatSurface.MissileClip : CompatSurface.None);
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); missile.DamageExpression = _ => 7;
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
