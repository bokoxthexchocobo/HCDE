using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSimulationTests
{
    [Fact]
    public void Spawn_SkipsTypeZeroAndUsesDehackedSize()
    {
        var patch = DehackedPatch.Apply("""
            Thing 2
            ID # = 3004
            Hit points = 20
            Width = 1310720
            Height = 3670016
            """);
        var level = Level(
            new LevelThing { Type = 0, X = 1, Y = 1 },
            new LevelThing { Type = 1, X = 100, Y = 200, Angle = 90 },
            new LevelThing { Type = 3004, X = 8, Y = 9, Angle = 0 });

        var thinkers = new ThinkerCollection();
        var actors = ActorSpawner.Spawn(level, thinkers, patch);

        Assert.Equal(2, actors.Count);
        var player = Assert.IsType<PlayerPawn>(actors[0]);
        Assert.Equal(0, player.PlayerNum);
        Assert.Equal(100, player.X.ToDouble());
        Assert.Equal(200, player.Y.ToDouble());
        Assert.Equal(BamAngle.Angle90, player.Angle.Raw);
        Assert.Equal(16, player.Radius.ToInt());
        Assert.Equal(ThinkerStat.Player, player.StatNum);

        Assert.Equal(20, actors[1].Health);
        Assert.Equal(20, actors[1].Radius.ToDouble());
        Assert.Equal(56, actors[1].Height.ToDouble());
        Assert.Equal(ThinkerStat.Default, actors[1].StatNum);
    }

    [Fact]
    public void Player_ForwardAtAngleZeroMovesEast()
    {
        var level = OpenLevel();
        var sim = AuthoritySimulation.Start(level);
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192, YawDelta = 0 });
        var player = sim.Players.Single();
        player.Angle = new BamAngle(0);
        sim.Tick();

        Assert.Equal(33, player.X.ToDouble(), 3);
        Assert.Equal(32, player.Y.ToDouble(), 3);
        Assert.Equal(default, player.Pending);
    }

    [Fact]
    public void Player_YawDeltaAddsSixteenBits()
    {
        var level = OpenLevel();
        var sim = AuthoritySimulation.Start(level);
        var player = sim.Players.Single();
        player.Angle = new BamAngle(0);
        sim.QueueCommand(0, new PlayerCommand { YawDelta = 1 });
        sim.Tick();
        Assert.Equal(1u << 16, player.Angle.Raw);
    }

    [Fact]
    public void Slide_WallKeepsTheParallelComponent()
    {
        var level = WallLevel(blocking: true);
        var moved = LineSlide.Move(level, 32, 64, 16, -32, 10);
        Assert.Equal(32, moved.X, 3);
        Assert.Equal(74, moved.Y, 3);

        var stopped = LineSlide.Move(level, 32, 64, 16, -32, 0);
        Assert.Equal(32, stopped.X, 3);
        Assert.Equal(64, stopped.Y, 3);
    }

    [Fact]
    public void Slide_TwoSidedLineDoesNotBlock()
    {
        var level = WallLevel(blocking: false);
        var moved = LineSlide.Move(level, 32, 64, 16, -64, 0);
        Assert.Equal(-32, moved.X, 3);
        Assert.Equal(64, moved.Y, 3);
    }

    [Fact]
    public void Tick_ChecksumChangesWhenThePlayerMovesAndWhenTheTicAdvances()
    {
        var level = OpenLevel();
        var sim = AuthoritySimulation.Start(level, rngSeed: 3);
        var spawned = sim.Checksum;
        sim.Tick();
        var advanced = sim.Checksum;
        Assert.NotEqual(spawned, advanced);

        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 64 });
        sim.Tick();
        Assert.NotEqual(advanced, sim.Checksum);
        Assert.True(sim.Players.Single().X.ToDouble() > 32);
    }

    private static PlayLevel OpenLevel() => new()
    {
        MapName = "MAP01",
        Things = new[] { new LevelThing { Type = 1, X = 32, Y = 32, Angle = 0 } },
    };

    private static PlayLevel WallLevel(bool blocking) => new()
    {
        MapName = "MAP01",
        Lines = new[]
        {
            new LevelLine
            {
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = 128,
                Flags = blocking ? 0 : 0,
                SideFront = 0,
                SideBack = blocking ? LevelLine.NoSide : 0,
            },
        },
    };

    private static PlayLevel Level(params LevelThing[] things) => new()
    {
        MapName = "MAP01",
        Things = things,
    };
}
