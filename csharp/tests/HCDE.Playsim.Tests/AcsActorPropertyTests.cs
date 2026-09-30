using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActorPropertyTests
{
    [Fact]
    public void SetAndGetActorProperty_HealthByTid()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        Assert.Equal(5, imp.ThingId);
        var player = sim.Players.Single();
        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 42,
            (int)AcsPcode.SetActorProperty);
        Assert.Equal(42, imp.Health);

        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.GetActorProperty,
            (int)AcsPcode.PushNumber, 42,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SetAndGetActorProperty_AmbushFlag()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        Assert.False(imp.Ambush);
        var player = sim.Players.Single();
        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 10,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.SetActorProperty);
        Assert.True(imp.Ambush);

        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 10,
            (int)AcsPcode.GetActorProperty,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SetAndGetActorProperty_TargetTidOnMonster()
    {
        var sim = RoomWithImp(playerTid: 3);
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        Assert.Equal(3, player.ThingId);
        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 26,
            (int)AcsPcode.PushNumber, 3,
            (int)AcsPcode.SetActorProperty);
        Assert.Equal(player.Id, imp.Brain!.TargetId);

        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 26,
            (int)AcsPcode.GetActorProperty,
            (int)AcsPcode.PushNumber, 3,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SetActorProperty_TidZeroUsesActivator()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Health = 80;
        Run(sim, player,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 60,
            (int)AcsPcode.SetActorProperty);
        Assert.Equal(60, player.Health);
    }

    private static void Run(AuthoritySimulation sim, Actor activator, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, activator));
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
    });

    private static AuthoritySimulation RoomWithImp(int playerTid = 0) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things =
        [
            new LevelThing { Type = 1, Id = playerTid, X = 32, Y = 64 },
            new LevelThing { Type = 3001, Id = 5, X = 96, Y = 64 },
        ],
    });
}
