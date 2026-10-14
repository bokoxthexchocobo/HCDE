using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsFriendlySeeBlocksTests
{
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void SignedPropertyAndChecksumSurviveSaveAndOldSaveRestoresDefault(int value)
    {
        var sim = Room(); var actor = sim.Actors.Single();
        var old = SimSavegame.Write(sim, true); var originalHash = sim.Checksum;
        AcsActorProperties.Set(sim, actor, 0, 48, value);
        Assert.Equal(value, AcsActorProperties.Get(sim, actor, 0, 48));
        Assert.True(AcsActorProperties.Check(sim, actor, 0, 48, value));
        sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(originalHash, sim.Checksum);
        var saved = SimSavegame.Write(sim, true); var hash = sim.Checksum;
        actor.FriendlySeeBlocks = 4;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(value, actor.FriendlySeeBlocks); Assert.Equal(hash, sim.Checksum);
        SimSavegame.Apply(sim, old);
        Assert.Equal(10, actor.FriendlySeeBlocks); Assert.Equal(originalHash, sim.Checksum);
    }

    [Fact]
    public void NativeDefaultAndMissingActorChecks()
    {
        var sim = Room(); var actor = sim.Actors.Single();
        Assert.Equal(10, AcsActorProperties.Get(sim, actor, 0, 48));
        Assert.True(AcsActorProperties.Check(sim, actor, 0, 48, 10));
        Assert.False(AcsActorProperties.Check(sim, null, 999, 48, 0));
        AcsActorProperties.Set(sim, null, 7, 48, 17);
        Assert.Equal(17, actor.FriendlySeeBlocks);
    }

    [Fact]
    public void NonDefaultPropertyRequiresGeometryArchive()
    {
        var sim = Room(); sim.Actors.Single().FriendlySeeBlocks = 0;
        var state = sim.CaptureState(); state.GeometryHealth = null;
        var error = Assert.Throws<InvalidOperationException>(() => SimFriendlySeeBlocksArchive.Validate(state));
        Assert.Equal("FriendlySeeBlocks archives require geometry state.", error.Message);
    }

    [Fact]
    public void InvalidTrailerCountRejects()
    {
        var sim = Room(); sim.Actors.Single().FriendlySeeBlocks = 0;
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(261, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 4), int.MaxValue);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal("save-friendlyseeblocks-header", error);
    }

    [Fact]
    public void PlayerPropertyIsSavedAsWellAsMonsterProperty()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
        });
        var player = sim.Players.Single();
        AcsActorProperties.Set(sim, player, 0, 48, -7);
        var saved = SimSavegame.Write(sim, true); player.FriendlySeeBlocks = 10;
        SimSavegame.Apply(sim, saved); Assert.Equal(-7, player.FriendlySeeBlocks);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004, Id = 7 }]
    });
}
