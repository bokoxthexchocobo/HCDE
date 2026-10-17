using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerRespawnArchiveTests
{
    [Fact]
    public void DeadlineAndArmedInputRoundTripAndOlderSaveResetsThem()
    {
        var sim = Room(); var player = sim.Players.Single();
        var older = SimSavegame.Write(sim, true);
        ActorDamage.Apply(player, 10000);
        player.RespawnArmed = true;
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(303, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.Equal(player.RespawnEarliestTic, restored.Players.Single().RespawnEarliestTic);
        Assert.True(restored.Players.Single().RespawnArmed);
        Assert.Equal(saved, SimSavegame.Write(restored, true));
        SimSavegame.Apply(restored, older);
        Assert.Equal(int.MaxValue, restored.Players.Single().RespawnEarliestTic);
        Assert.False(restored.Players.Single().RespawnArmed);
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(4, 2)]
    public void InvalidTrailerFieldsAreRejected(int offset, int value)
    {
        var sim = Room(); ActorDamage.Apply(sim.Players.Single(), 10000);
        var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8 + offset), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
