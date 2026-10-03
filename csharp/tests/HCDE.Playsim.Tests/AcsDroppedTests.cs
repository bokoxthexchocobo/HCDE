using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsDroppedTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void SetGetCheckNormalizeBoolean(int value)
    {
        var sim = Room(); var actor = sim.Players.Single(); var expected = value != 0 ? 1 : 0;
        Run(sim, actor, [3, 0, 3, 18, 3, value, 245,
            3, 7, 3, 0, 3, 18, 246, 3, expected, 19, 5, 112, 1]);
        Assert.Equal(value != 0, actor.Dropped); Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 18, 3, value, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 18, 3, expected == 0 ? -9 : 0, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpawnedDropIsMarkedAndClearingFlagDoesNotChangePickupFields(bool ignoreSkill)
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.True(sim.SpawnDroppedPickup(player, 2001, ignoreAmmoSkill: ignoreSkill, pickupAmount: 9));
        var drop = sim.Actors.Last(); Assert.True(drop.Dropped);
        var amount = drop.PickupAmount; Assert.Equal(ignoreSkill, drop.IgnoreAmmoSkill);
        Run(sim, drop, [3, 7, 3, 0, 3, 18, 246, 5, 112, 1]); Assert.Equal(1, sim.LightOf(0));
        Run(sim, drop, [3, 0, 3, 18, 3, 0, 245, 1]);
        Assert.False(drop.Dropped); Assert.Equal(amount, drop.PickupAmount); Assert.Equal(ignoreSkill, drop.IgnoreAmmoSkill);
    }

    [Fact]
    public void MapPickupStartsUndropped()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 2001 }],
        });
        Assert.False(sim.Actors.Single().Dropped);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedTargetIgnoresSetter(bool destroyed)
    {
        var sim = Room(); var actor = sim.Players.Single(); if (destroyed) actor.Destroy();
        var tid = destroyed ? 0 : 999;
        Run(sim, actor, [3, tid, 3, 18, 3, 1, 245, 3, 7, 3, tid, 3, 18, 246, 5, 112, 1]);
        Assert.False(actor.Dropped); Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void FlagParticipatesInRestingChecksum()
    {
        var first = Room(); var second = Room(); second.Players.Single().Dropped = true;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static void Run(AuthoritySimulation sim, Actor actor, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); sim.Acs.TryExecute(1, [], actor); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256, LightLevel = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
