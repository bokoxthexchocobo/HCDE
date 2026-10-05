using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RipperLevelTests
{
    [Theory]
    [InlineData(0, 0, 0, true)]
    [InlineData(2, 3, 0, false)]
    [InlineData(3, 3, 0, true)]
    [InlineData(4, 3, 5, true)]
    [InlineData(5, 0, 5, true)]
    [InlineData(6, 0, 5, false)]
    [InlineData(-1, -2, -3, true)]
    [InlineData(4, 5, 3, false)]
    public void InclusivePositiveLimitsControlPassThrough(int level, int min, int max, bool passes)
    {
        var (_, missile, target) = Setup(); missile.RipperLevel = level;
        target.RipLevelMin = min; target.RipLevelMax = max;
        missile.DamageExpression = _ => 7; missile.Tick();
        Assert.Equal(!passes, missile.Destroyed); Assert.Equal(993, target.Health);
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    public void BossRestrictionRequiresBothFlags(bool noBossRip, bool boss, bool passes)
    {
        var (_, missile, target) = Setup(); target.Boss = boss;
        Assert.True(AcsActorFlags.TrySet(missile, "Actor.NOBOSSRIP", noBossRip));
        Assert.True(AcsActorFlags.TryGet(missile, "nobossrip", out var value)); Assert.Equal(noBossRip, value);
        missile.DamageExpression = _ => 7; missile.Tick();
        Assert.Equal(!passes, missile.Destroyed); Assert.Equal(993, target.Health);
    }

    [Fact]
    public void SaveRestoresLevelRestrictionsAndBossFlag()
    {
        var (sim, missile, target) = Setup(); missile.RipperLevel = 4; missile.NoBossRip = true;
        target.RipLevelMin = 3; target.RipLevelMax = 5; target.Boss = true;
        var bytes = SimSavegame.Write(sim); Assert.Equal(59, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var (restored, loaded, victim) = Setup(); restored.RestoreState(state);
        Assert.Equal(4, loaded.RipperLevel); Assert.True(loaded.NoBossRip);
        Assert.Equal(3, victim.RipLevelMin); Assert.Equal(5, victim.RipLevelMax); Assert.True(victim.Boss);
        loaded.Tick(); Assert.True(loaded.Destroyed); Assert.True(victim.Health < 1000);
    }

    [Fact]
    public void OldRipFormatClearsNewFieldsAndPosePreservesSignedValues()
    {
        var (sim, missile, target) = Setup(); var old = SimSavegame.Write(sim);
        missile.NoBossRip = true; missile.RipperLevel = -4; target.RipLevelMin = -2; target.RipLevelMax = 6;
        var pose = sim.CaptureState(); missile.RipperLevel = 0; target.RipLevelMax = 0;
        sim.RestoreState(pose); Assert.Equal(-4, missile.RipperLevel); Assert.Equal(6, target.RipLevelMax);
        Assert.True(SimSavegame.TryRead(old, out var state, out var error), error); sim.RestoreState(state);
        Assert.False(missile.NoBossRip); Assert.Equal(0, missile.RipperLevel);
        Assert.Equal(0, target.RipLevelMin); Assert.Equal(0, target.RipLevelMax); Assert.True(missile.Rip);
        Assert.Equal(old, SimSavegame.Write(sim));
    }

    [Fact]
    public void UnknownExtendedFlagsAreRejected()
    {
        var (sim, missile, _) = Setup(); missile.NoBossRip = true;
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 8);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-rip-flags", error);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); missile.Rip = true;
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
