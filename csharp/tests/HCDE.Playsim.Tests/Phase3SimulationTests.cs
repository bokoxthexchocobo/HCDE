using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class Phase3SimulationTests
{
    [Fact]
    public void DoorRaise_OpensThenClosesTheTaggedSector()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = new[] { new LevelSector { CeilingHeight = 0, Tag = 1 }, new LevelSector { CeilingHeight = 12 } },
            Sides = new[] { new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 } },
            Lines = new[] { new LevelLine { SideFront = 0, SideBack = 1 } },
        });
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.DoorRaise, 1));
        sim.Tick();
        Assert.Equal(8, sim.CeilingOf(0));
        Assert.Equal(0, sim.FloorOf(0));

        for (var i = 0; i < 6; i++)
            sim.Tick();
        Assert.Equal(0, sim.CeilingOf(0));
    }

    [Fact]
    public void Exit_SetsExitedAndClearsAOneShotLine()
    {
        var sim = AuthoritySimulation.Start(DoorLevel(LineSpecials.ExitNormal));
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick();
        Assert.True(sim.Exited);
        Assert.False(sim.SecretExit);
        Assert.Equal(0, sim.Level.Lines[0].Special);
    }

    [Fact]
    public void Teleport_MovesTheActivatorToTheDestThing()
    {
        var level = DoorLevel(LineSpecials.Teleport);
        level = new PlayLevel
        {
            MapName = level.MapName,
            Lines = level.Lines,
            Sectors = level.Sectors,
            Things = new[]
            {
                level.Things[0],
                new LevelThing { Type = LineSpecials.TeleportDestType, X = 200, Y = 180, Angle = 90 },
            },
        };
        var sim = AuthoritySimulation.Start(level);
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick();
        var player = sim.Players.Single();
        Assert.Equal(200, player.X.ToDouble());
        Assert.Equal(180, player.Y.ToDouble());
    }

    [Fact]
    public void AcsExecuteLine_StartsTheTaggedScriptOnTheSameTic()
    {
        var sim = AuthoritySimulation.Start(DoorLevel(LineSpecials.AcsExecute));
        sim.Acs.Add(new AcsProgram
        {
            Number = 1,
            Code = Words((int)AcsPcode.Lspec1Direct, LineSpecials.ExitNormal, 0, (int)AcsPcode.Terminate),
        });
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick();
        Assert.True(sim.Exited);
        Assert.Equal(0, sim.Level.Lines[0].Special);
    }

    [Fact]
    public void Acs_DelayOneThenExitRunsOnTheSecondTic()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { MapName = "MAP01" });
        sim.Acs.Add(new AcsProgram
        {
            Number = 4,
            Code = Words(
                (int)AcsPcode.PushNumber, 2,
                (int)AcsPcode.PushNumber, 3,
                (int)AcsPcode.Add,
                (int)AcsPcode.Drop,
                (int)AcsPcode.DelayDirect, 1,
                (int)AcsPcode.Lspec1Direct, LineSpecials.ExitNormal, 0,
                (int)AcsPcode.Terminate),
        });
        Assert.True(sim.Acs.Enqueue(4));
        sim.Tick();
        Assert.False(sim.Exited);
        Assert.Equal(1, sim.Acs.RunningCount);
        sim.Tick();
        Assert.True(sim.Exited);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void BotWithoutATargetStaysIdle()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { MapName = "MAP01" });
        var bot = sim.AddBot(32, 32);
        sim.Tick();
        for (var i = 0; i < 20; i++) sim.Tick();
        Assert.Equal(32, bot.X.ToDouble());
        Assert.Equal(MonsterMode.Idle, bot.Brain!.Mode);
    }

    [Fact]
    public void Savegame_RestoresPoseFloorAndExit()
    {
        var sim = AuthoritySimulation.Start(DoorLevel(LineSpecials.FloorRaise));
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick();
        var savedX = sim.Players.Single().X.Raw;
        var bytes = SimSavegame.Write(sim);
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick();
        Assert.NotEqual(savedX, sim.Players.Single().X.Raw);

        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.RestoreState(state);
        Assert.Equal(savedX, sim.Players.Single().X.Raw);
        Assert.Equal(8, sim.FloorOf(0));
        Assert.Equal(1, sim.Thinkers.Clock.Tic);
    }

    [Fact]
    public void Savegame_RejectsABadMagic()
    {
        Assert.False(SimSavegame.TryRead(new byte[16], out _, out var error));
        Assert.Equal("save-magic", error);
    }

    [Fact]
    public void Mbf21FloorNudge_RequiresTheCompatFlag()
    {
        var blocked = AuthoritySimulation.Start(DoorLevel(CompatSurfaceRules.Mbf21FloorNudge));
        blocked.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        blocked.Tick();
        Assert.Equal(0, blocked.FloorOf(0));
        Assert.Equal(CompatSurfaceRules.Mbf21FloorNudge, blocked.Level.Lines[0].Special);

        var allowed = AuthoritySimulation.Start(DoorLevel(CompatSurfaceRules.Mbf21FloorNudge), compat: CompatSurface.Mbf21);
        allowed.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        allowed.Tick();
        Assert.Equal(8, allowed.FloorOf(0));
        Assert.Equal(0, allowed.Level.Lines[0].Special);
    }

    [Fact]
    public void Id24SecretExit_RequiresTheCompatFlag()
    {
        var blocked = AuthoritySimulation.Start(DoorLevel(CompatSurfaceRules.Id24SecretExit));
        blocked.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        blocked.Tick();
        Assert.False(blocked.Exited);

        var allowed = AuthoritySimulation.Start(DoorLevel(CompatSurfaceRules.Id24SecretExit), compat: CompatSurface.Id24);
        allowed.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        allowed.Tick();
        Assert.True(allowed.Exited);
        Assert.True(allowed.SecretExit);
    }

    [Fact]
    public void Eternity_SectorSpecialArmsReverb()
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Sectors = new[] { new LevelSector { Special = CompatSurfaceRules.EternityReverbSpecial } },
        };
        Assert.False(AuthoritySimulation.Start(level).ReverbActive);
        Assert.True(AuthoritySimulation.Start(level, compat: CompatSurface.Eternity).ReverbActive);
    }

    [Fact]
    public void Invasion_SpawnsAnEnemyThatWaitsForATarget()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = new[] { new LevelThing { Type = InvasionDirector.SpawnSpotType, X = 10, Y = 12 } },
        });
        sim.Invasion.Enabled = true;
        sim.Tick();
        var bot = Assert.Single(sim.Actors.OfType<BotPawn>());
        Assert.Equal(1, sim.Invasion.Wave);
        Assert.Equal(InvasionPhase.Wave, sim.Invasion.Phase);
        Assert.Equal(10, bot.X.ToDouble());
        sim.Tick();
        Assert.Equal(10, bot.X.ToDouble());
        Assert.NotNull(bot.Brain);
    }

    [Fact]
    public void Rewind_RestoresTheOldestCapturedPose()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = new[] { new LevelThing { Type = 1, X = 32, Y = 32 } },
        });
        sim.RewindEnabled = true;
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick();
        var first = sim.Players.Single().X.Raw;
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick();
        Assert.NotEqual(first, sim.Players.Single().X.Raw);
        Assert.True(sim.Rewind.RestoreOldest(sim));
        Assert.Equal(first, sim.Players.Single().X.Raw);
        Assert.Equal(2, sim.Rewind.Count);
    }

    [Fact]
    public void UdmfLine_CopiesSpecialTagAndArgs()
    {
        var level = LevelBuilder.FromUdmf(new UdmfTextMap
        {
            Linedefs = new[]
            {
                new UdmfLinedef { Special = 80, Id = 7, Arg0 = 3, SideFront = -1, SideBack = -1 },
            },
        }, "MAP02");
        var line = Assert.Single(level.Lines);
        Assert.Equal(80, line.Special);
        Assert.Equal(7, line.Tag);
        Assert.Equal(3, line.Arg0);
    }

    private static PlayLevel DoorLevel(int special) => new()
    {
        MapName = "MAP01",
        Sectors = new[] { new LevelSector { FloorHeight = 0, CeilingHeight = 128, Tag = 1 } },
        Lines = new[]
        {
            new LevelLine
            {
                X1 = 32.5,
                Y1 = 0,
                X2 = 32.5,
                Y2 = 128,
                SideFront = 0,
                SideBack = 0,
                Special = special,
                Tag = 1,
            },
        },
        Things = new[] { new LevelThing { Type = 1, X = 32, Y = 64 } },
    };

    private static byte[] Words(params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return bytes;
    }
}
