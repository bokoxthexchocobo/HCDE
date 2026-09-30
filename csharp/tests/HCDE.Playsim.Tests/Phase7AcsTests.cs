using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class Phase7AcsTests
{
    [Fact]
    public void MultiplyAndCompare_ExitsWhenTheProductMatches()
    {
        var sim = Scripted(
            (int)AcsPcode.PushNumber, 6,
            (int)AcsPcode.PushNumber, 7,
            (int)AcsPcode.Multiply,
            (int)AcsPcode.PushNumber, 42,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 44);
        sim.Tick();
        Assert.True(sim.Exited);
    }

    [Fact]
    public void DivideByZero_PushesZero()
    {
        var sim = Scripted(
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Divide,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 44);
        sim.Tick();
        Assert.True(sim.Exited);
    }

    [Fact]
    public void ScriptVar_AddsOntoTheStoredValue()
    {
        var sim = Scripted(
            (int)AcsPcode.PushNumber, 9,
            (int)AcsPcode.AssignScriptVar, 0,
            (int)AcsPcode.PushNumber, 4,
            (int)AcsPcode.AddScriptVar, 0,
            (int)AcsPcode.PushScriptVar, 0,
            (int)AcsPcode.PushNumber, 13,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 64);
        sim.Tick();
        Assert.True(sim.Exited);
    }

    [Fact]
    public void MapVar_IsVisibleToTheNextScript()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { MapName = "MAP01" });
        sim.Acs.Add(new AcsProgram
        {
            Number = 1,
            Code = Words((int)AcsPcode.PushNumber, 5, (int)AcsPcode.AssignMapVar, 0, (int)AcsPcode.Terminate),
        });
        sim.Acs.Add(new AcsProgram
        {
            Number = 2,
            Code = Words(
                (int)AcsPcode.PushMapVar, 0,
                (int)AcsPcode.PushNumber, 5,
                (int)AcsPcode.Eq,
                (int)AcsPcode.IfGoto, 32,
                (int)AcsPcode.Terminate,
                (int)AcsPcode.Lspec1Direct, LineSpecials.ExitNormal, 0),
        });
        Assert.True(sim.Acs.Enqueue(1));
        Assert.True(sim.Acs.Enqueue(2));
        sim.Tick();
        Assert.True(sim.Exited);
    }

    [Fact]
    public void ThingCount_CountsEditorNumbersAndIgnoresATid()
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Things = new[]
            {
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = 3001, X = 80, Y = 64 },
            },
        };
        var counted = AuthoritySimulation.Start(level);
        counted.Acs.Add(ExitWhen(
            (int)AcsPcode.ThingCountDirect, 3001, 0,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 36));
        counted.Acs.Enqueue(1);
        counted.Tick();
        Assert.True(counted.Exited);

        var tid = AuthoritySimulation.Start(level);
        tid.Acs.Add(ExitWhen(
            (int)AcsPcode.ThingCountDirect, 3001, 7,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 36));
        tid.Acs.Enqueue(1);
        tid.Tick();
        Assert.True(tid.Exited);
    }

    [Fact]
    public void ThingCount_FiltersByTidWhenSet()
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Things =
            [
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = 3001, X = 80, Y = 64, Id = 7 },
                new LevelThing { Type = 3001, X = 96, Y = 64, Id = 8 },
            ],
        };
        var sim = AuthoritySimulation.Start(level);
        sim.Acs.Add(ExitWhen(
            (int)AcsPcode.ThingCountDirect, 3001, 7,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 36));
        sim.Acs.Enqueue(1);
        sim.Tick();
        Assert.True(sim.Exited);
    }

    [Fact]
    public void ThingCountSector_RespectsSectorTag()
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, FloorHeight = 0, CeilingHeight = 128 }],
            Things =
            [
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = 3001, X = 80, Y = 64 },
            ],
        };
        var sim = AuthoritySimulation.Start(level);
        Assert.Equal(1, sim.Actors.Count(actor => !actor.Destroyed && actor.DoomEdNum == 3001));
        sim.Acs.Add(ExitWhen(
            (int)AcsPcode.PushNumber, 3001,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.ThingCountSector,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 52));
        sim.Acs.Enqueue(1);
        sim.Tick();
        Assert.True(sim.Exited);
    }

    [Fact]
    public void ThingCount_IgnoresDeadActors()
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Things =
            [
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = 3001, X = 80, Y = 64 },
            ],
        };
        var sim = AuthoritySimulation.Start(level);
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        imp.Health = 0;
        sim.Acs.Add(ExitWhen(
            (int)AcsPcode.ThingCountDirect, 3001, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 36));
        sim.Acs.Enqueue(1);
        sim.Tick();
        Assert.True(sim.Exited);
    }

    [Fact]
    public void ThingCountSector_IgnoresActorsOutsideTaggedSector()
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 0, FloorHeight = 0, CeilingHeight = 128 }],
            Things =
            [
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = 3001, X = 80, Y = 64 },
            ],
        };
        var sim = AuthoritySimulation.Start(level);
        sim.Acs.Add(ExitWhen(
            (int)AcsPcode.PushNumber, 3001,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 7,
            (int)AcsPcode.ThingCountSector,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 52));
        sim.Acs.Enqueue(1);
        sim.Tick();
        Assert.True(sim.Exited);
    }

    [Fact]
    public void TimerAndPlayerCount_MatchTheRunningSim()
    {
        var sim = Scripted(
            (int)AcsPcode.Timer,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.PlayerCount,
            (int)AcsPcode.AndLogical,
            (int)AcsPcode.IfGoto, 36);
        sim.Tick();
        Assert.True(sim.Exited);
    }

    private static AuthoritySimulation Scripted(params int[] leading)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = new[] { new LevelThing { Type = 1, X = 32, Y = 64 } },
        });
        sim.Acs.Add(ExitWhen(leading));
        sim.Acs.Enqueue(1);
        return sim;
    }

    private static AcsProgram ExitWhen(params int[] leading)
    {
        var words = new List<int>(leading)
        {
            (int)AcsPcode.Terminate,
            (int)AcsPcode.Lspec1Direct,
            LineSpecials.ExitNormal,
            0,
        };
        return new AcsProgram { Number = 1, Code = Words(words.ToArray()) };
    }

    private static byte[] Words(params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return bytes;
    }
}
