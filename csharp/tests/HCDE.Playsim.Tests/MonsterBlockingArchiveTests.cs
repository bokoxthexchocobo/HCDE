using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MonsterBlockingArchiveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ChangedExemptionSurvivesArchiveAndCollision(bool player)
    {
        var sim = Room(player);
        var actor = sim.Actors.Single();
        ActorPropertyActions.ChangeFlag(actor, "NOBLOCKMONST", !player);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(90, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.NoBlockMonsters = player;
        sim.RestoreState(state);
        actor = sim.Actors.Single();
        Assert.Equal(!player, actor.NoBlockMonsters);
        Assert.Equal(player ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, xOffset: 40));
        Assert.Equal(bytes, SimSavegame.Write(sim));
        var fresh = Room(player);
        fresh.RestoreState(state);
        Assert.Equal(!player, fresh.Actors.Single().NoBlockMonsters);
        Assert.Equal(bytes, SimSavegame.Write(fresh));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OmittedFlagRestoresSpawnDefault(bool player)
    {
        var sim = Room(player);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.Null(state.Actors.Single().MonsterBlockingFlag);
        sim.Actors.Single().NoBlockMonsters = !player;
        sim.RestoreState(state);
        Assert.Equal(player, sim.Actors.Single().NoBlockMonsters);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, 90)]
    [InlineData(4, -1)]
    [InlineData(8, 2)]
    [InlineData(12, 0)]
    public void InvalidTrailerIsRejected(int offset, int value)
    {
        var sim = Room(true);
        sim.Actors.Single().NoBlockMonsters = false;
        var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    private static AuthoritySimulation Room(bool player)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines.Take(6).Append(new LevelLine
            {
                X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1,
                Flags = LevelLine.BlockMonstersFlag
            }).ToArray(),
            Things = [new LevelThing { Type = player ? 1 : 3004, X = -40 }]
        });
    }
}
