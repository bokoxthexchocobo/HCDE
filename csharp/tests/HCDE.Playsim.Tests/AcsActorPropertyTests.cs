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
    public void SetAndGetActorProperty_InvulnerableFlag()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 11,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.SetActorProperty);
        Assert.True(imp.Invulnerable);

        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 11,
            (int)AcsPcode.GetActorProperty,
            (int)AcsPcode.PushNumber, 1,
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
    public void GetActorProperty_TargetTidOnMonsterAndSetterIsIgnored()
    {
        var sim = RoomWithImp(playerTid: 3);
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        Assert.Equal(3, player.ThingId);
        imp.Brain!.SetTargetThingId(sim, 3);
        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 26,
            (int)AcsPcode.PushNumber, 999,
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
    public void SetAndGetActorProperty_FriendlyFlag()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 16,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.SetActorProperty);
        Assert.True(imp.Friendly);

        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 16,
            (int)AcsPcode.GetActorProperty,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SetAndGetActorProperty_NoTargetFlag()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 19,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.SetActorProperty);
        Assert.True(imp.NoTarget);

        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 19,
            (int)AcsPcode.GetActorProperty,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SetAndGetActorProperty_SpawnHealth()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 17,
            (int)AcsPcode.PushNumber, 80,
            (int)AcsPcode.SetActorProperty);
        Assert.Equal(80, imp.ResurrectionHealth);

        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 17,
            (int)AcsPcode.GetActorProperty,
            (int)AcsPcode.PushNumber, 80,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SetAndGetActorProperty_MaxStepHeightFixedRaw()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        var raw = Fixed.FromInt(32).Raw;
        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 44,
            (int)AcsPcode.PushNumber, raw,
            (int)AcsPcode.SetActorProperty);
        Assert.Equal(32, imp.MaxStepHeight.ToDouble());

        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 44,
            (int)AcsPcode.GetActorProperty,
            (int)AcsPcode.PushNumber, raw,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void NoTargetOnPlayerBlocksMonsterWakeTarget()
    {
        var baseline = RoomWithImp();
        var baselineImp = baseline.Actors.First(actor => actor.DoomEdNum == 3001);
        var baselinePlayer = baseline.Players.Single();
        ActorDamage.Apply(baselineImp, 5, baselinePlayer);
        Assert.Equal(baselinePlayer.Id, baselineImp.Brain!.TargetId);

        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        Run(sim, player,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 19,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.SetActorProperty);
        Assert.True(player.NoTarget);
        ActorDamage.Apply(imp, 5, player);
        Assert.Null(imp.Brain!.TargetId);
    }

    [Fact]
    public void SetAndGetActorProperty_MaxDropOffHeightFixedRaw()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        var raw = Fixed.FromInt(16).Raw;
        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 45,
            (int)AcsPcode.PushNumber, raw,
            (int)AcsPcode.SetActorProperty);
        Assert.Equal(16, imp.MaxDropOffHeight.ToDouble());

        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 45,
            (int)AcsPcode.GetActorProperty,
            (int)AcsPcode.PushNumber, raw,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SetActorProperty_InvulnerableBlocksOrdinaryDamage()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var health = imp.Health;
        var player = sim.Players.Single();
        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 11,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.SetActorProperty);
        Assert.True(imp.Invulnerable);
        ActorDamage.Apply(imp, 40);
        Assert.Equal(health, imp.Health);
    }

    [Fact]
    public void SetAndGetActorProperty_Mass()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 32,
            (int)AcsPcode.PushNumber, 999,
            (int)AcsPcode.SetActorProperty);
        Assert.Equal(999, imp.Mass);

        Run(sim, player,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 32,
            (int)AcsPcode.GetActorProperty,
            (int)AcsPcode.PushNumber, 999,
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
