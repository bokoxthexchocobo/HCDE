using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StrifeDamageArchiveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LoadedMissileUsesRestoredDice(bool powers)
    {
        var (sim, missile, _) = Setup(); missile.StrifeDamage = true;
        if (powers) sim.Players.Single().GivePowerBuddha();
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(powers ? 52 : 54, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var (restored, loaded, target) = Setup(); restored.RestoreState(state);
        Assert.True(loaded.StrifeDamage);
        var (prediction, _, _) = Setup();
        var expected = 5 * (1 + (int)(prediction.NextCombatRandom() & 3u));
        loaded.Tick(); Assert.Equal(expected, 1000 - target.Health);
        Assert.Equal(powers ? 2100 : 0, restored.Players.Single().PowerBuddhaTics);
    }

    [Fact]
    public void PoseRestoresAndLegacyClearsFlag()
    {
        var (sim, missile, _) = Setup(); var legacy = SimSavegame.Write(sim);
        missile.StrifeDamage = true; var pose = sim.CaptureState();
        missile.StrifeDamage = false; sim.RestoreState(pose); Assert.True(missile.StrifeDamage);
        Assert.True(SimSavegame.TryRead(legacy, out var state, out var error), error);
        sim.RestoreState(state); Assert.False(missile.StrifeDamage);
        Assert.Equal(legacy, SimSavegame.Write(sim));
    }

    [Fact]
    public void UnknownBitsAreRejected()
    {
        var (sim, missile, _) = Setup(); missile.StrifeDamage = true;
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), 32);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-special-fire-flags", error);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }],
        }, rngSeed: 7);
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
