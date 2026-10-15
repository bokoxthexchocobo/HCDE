using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IronFeetSectorTests
{
    [Theory]
    [InlineData(-1, 100)]
    [InlineData(0, 100)]
    [InlineData(256, 95)]
    [InlineData(300, 95)]
    public void LeakExtremesUseNativeByteComparison(int leakiness, int health)
    {
        var sim = Room(leakiness); var player = sim.Players.Single(); player.GivePowerIronFeet();
        SectorDamage.Tick(sim, player);
        Assert.Equal(health, player.Health); Assert.NotNull(sim.SectorDamageRandomState);
    }

    [Fact]
    public void BoundaryLeaksOnlyWhenByteIsStrictlyLessThanLeakiness()
    {
        var reference = Room(); var value = reference.NextSectorDamageByte();
        foreach (var leakiness in new[] { value, value + 1 })
        {
            var sim = Room(leakiness); var player = sim.Players.Single(); player.GivePowerIronFeet();
            SectorDamage.Tick(sim, player);
            Assert.Equal(leakiness == value ? 100 : 95, player.Health);
        }
    }

    [Fact]
    public void MissingPowerDoesNotConsumeLeakStream()
    {
        var sim = Room(); SectorDamage.Tick(sim, sim.Players.Single());
        Assert.Equal(95, sim.Players.Single().Health); Assert.Null(sim.SectorDamageRandomState);
    }

    [Fact]
    public void OffIntervalDrawsAndGodModeEndsBeforeProtection()
    {
        var sim = Room(0); var player = sim.Players.Single(); player.GivePowerIronFeet();
        sim.Tick(); var before = sim.SectorDamageRandomState;
        sim.Level.Sectors[0].DamageInterval = 32; sim.Level.Sectors[0].DamageEndsGodMode = true;
        player.GodMode = true; SectorDamage.Tick(sim, player);
        Assert.False(player.GodMode); Assert.NotEqual(before, sim.SectorDamageRandomState);
        Assert.Equal(100, player.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GodModesStillPreventLeakedDamageButConsumeDraw(bool ultimate)
    {
        var sim = Room(256); var player = sim.Players.Single(); player.GivePowerIronFeet();
        player.GodMode = !ultimate; player.GodMode2 = ultimate;
        SectorDamage.Tick(sim, player);
        Assert.Equal(100, player.Health); Assert.NotNull(sim.SectorDamageRandomState);
    }

    [Fact]
    public void HealingDoesNotConsumeProtectionRandomness()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerIronFeet(); player.Health = 50;
        sim.Level.Sectors[0].DamageAmount = -5; SectorDamage.Tick(sim, player);
        Assert.Equal(55, player.Health); Assert.Null(sim.SectorDamageRandomState);
    }

    [Fact]
    public void AirborneRejectionDoesNotConsumeProtectionRandomness()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerIronFeet(); player.Z = Fixed.FromInt(1);
        SectorDamage.Tick(sim, player);
        Assert.Equal(100, player.Health); Assert.Null(sim.SectorDamageRandomState);
    }

    [Fact]
    public void SavedDamageSequenceAndChecksumContinueExactly()
    {
        var sim = Room(128); var player = sim.Players.Single(); player.GivePowerIronFeet();
        SectorDamage.Tick(sim, player); var saved = SimSavegame.Write(sim, true);
        for (var i = 0; i < 8; i++) sim.Tick();
        var expected = SimSavegame.Write(sim, true); var checksum = sim.Checksum;
        SimSavegame.Apply(sim, saved);
        for (var i = 0; i < 8; i++) sim.Tick();
        Assert.Equal(expected, SimSavegame.Write(sim, true)); Assert.Equal(checksum, sim.Checksum);
    }

    private static AuthoritySimulation Room(int leakiness = 5) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128, DamageAmount = 5, DamageInterval = 1, Leakiness = (short)leakiness }],
        Things = [new LevelThing { Type = 1 }]
    }, rngSeed: 42);
}
