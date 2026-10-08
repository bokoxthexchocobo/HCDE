using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MeleeRangeArchiveTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-65536)]
    [InlineData(688128)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void SignedFixedRangeRoundTripsInFreshSimulation(int raw)
    {
        var sim = Room(); sim.Players.Single().MeleeRange = new Fixed(raw);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(103, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var loaded = Room(); SimSavegame.Apply(loaded, bytes);
        Assert.Equal(raw, loaded.Players.Single().MeleeRange.Raw);
        Assert.Equal(Fixed.FromInt(44), loaded.Actors[1].MeleeRange);
        Assert.Equal(bytes, SimSavegame.Write(loaded));
    }

    [Fact]
    public void ArchiveWithoutRangeClearsLaterRuntimeChanges()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        Assert.NotEqual(103, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        foreach (var actor in sim.Actors) actor.MeleeRange = Fixed.FromInt(100);
        SimSavegame.Apply(sim, bytes);
        Assert.All(sim.Actors, actor => Assert.Equal(Fixed.FromInt(44), actor.MeleeRange));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(WeaponKind.Fist)]
    [InlineData(WeaponKind.Chainsaw)]
    public void LoadedPlayerUsesExtendedReachWhileOmittedActorResets(WeaponKind weapon)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.Weapons |= weapon; player.Inventory.Selected = weapon;
        player.MeleeRange = Fixed.FromInt(100);
        var bytes = SimSavegame.Write(sim);
        var loaded = Room(); loaded.Actors[1].MeleeRange = Fixed.FromInt(1);
        SimSavegame.Apply(loaded, bytes);
        Assert.Equal(Fixed.FromInt(44), loaded.Actors[1].MeleeRange);
        var target = loaded.Actors[1]; target.Brain = null; target.Health = 1000;
        Assert.True(HitscanCombat.Fire(loaded, loaded.Players.Single()));
        Assert.True(target.Health < 1000);
    }

    [Theory]
    [InlineData(0, "save-melee-range-size")]
    [InlineData(1, "save-melee-range-header")]
    [InlineData(2, "save-melee-range-value")]
    public void MalformedTrailerIsRejectedBeforeApply(int corruption, string expected)
    {
        var sim = Room(); sim.Players.Single().MeleeRange = Fixed.FromInt(100);
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var start = bytes.Length - size;
        if (corruption == 0) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), int.MaxValue);
        if (corruption == 1) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), 103);
        if (corruption == 2) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(expected, error);
        Assert.Equal(expected, Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes)).Message);
        Assert.Equal(Fixed.FromInt(100), sim.Players.Single().MeleeRange);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 100 }],
        Sectors = [new LevelSector { CeilingHeight = 512 }],
    }, rngSeed: 42);
}
