using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorDamageTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(32)]
    public void DamageUsesSharedClockIncludingTicZero(int interval)
    {
        var sim = Room(5, interval); var player = sim.Players.Single();
        sim.Tick(); Assert.Equal(95, player.Health);
        for (var i = 1; i < interval; i++) { sim.Tick(); Assert.Equal(95, player.Health); }
        sim.Tick(); Assert.Equal(90, player.Health);
    }

    [Theory]
    [InlineData(false, 90, 90)]
    [InlineData(true, 100, 100)]
    public void DamageRespectsArmorAndInvulnerability(bool invulnerable, int health, int armor)
    {
        var sim = Room(20, 1); var player = sim.Players.Single();
        player.Inventory.Armor = 100; player.Inventory.ArmorSavePercent = 50;
        player.Invulnerable = invulnerable;
        sim.Tick(); Assert.Equal(health, player.Health); Assert.Equal(armor, player.Inventory.Armor);
        Assert.Null(player.LastDamageSourceId);
    }

    [Fact]
    public void AirbornePlayerWaitsForFloorContact()
    {
        var sim = Room(5, 1); var player = sim.Players.Single();
        player.NoGravity = true; player.Z = Fixed.FromInt(16);
        sim.Tick(); Assert.Equal(100, player.Health);
        player.Z = Fixed.FromInt(0);
        sim.Tick(); Assert.Equal(95, player.Health);
    }

    [Theory]
    [InlineData(-5, 98, 100)]
    [InlineData(-5, 80, 85)]
    [InlineData(int.MinValue, 1, 100)]
    [InlineData(-5, 150, 150)]
    [InlineData(-5, 0, 0)]
    public void HealingCapsAtOneHundredWithoutResurrecting(int amount, int initial, int expected)
    {
        var sim = Room(amount, 1); var player = sim.Players.Single(); player.Health = initial;
        sim.Tick(); Assert.Equal(expected, player.Health);
    }

    [Fact]
    public void LethalDamageRunsDeathOnlyOnce()
    {
        var sim = Room(100, 1); var player = sim.Players.Single();
        sim.Tick(); sim.Tick();
        Assert.True(player.IsDead); Assert.Equal(1, player.DeathCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void InvalidRuntimeIntervalIsRepaired(int interval)
    {
        var sim = Room(5, interval);
        sim.Tick(); Assert.Equal(32, sim.Level.Sectors[0].DamageInterval);
        sim.Tick(); Assert.Equal(95, sim.Players.Single().Health);
    }

    [Fact]
    public void LandingDoesNotRestartDamageClock()
    {
        var sim = Room(5, 4); var player = sim.Players.Single();
        player.NoGravity = true; player.Z = Fixed.FromInt(16);
        sim.Tick(); sim.Tick();
        player.Z = Fixed.FromInt(0);
        sim.Tick(); sim.Tick(); Assert.Equal(100, player.Health);
        sim.Tick(); Assert.Equal(95, player.Health);
    }

    [Fact]
    public void RaiseAndChangeWithoutTriggerClearsActiveDamage()
    {
        var sim = Room(5, 1); var player = sim.Players.Single();
        sim.Tick(); Assert.Equal(95, player.Health);
        int[] words = [11, 239, 7, 8, 4, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        sim.Tick(); Assert.Equal(95, player.Health);
    }

    private static AuthoritySimulation Room(int amount, int interval) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, DamageAmount = amount, DamageInterval = interval }],
        Things = [new LevelThing { Type = 1 }],
    });
}
