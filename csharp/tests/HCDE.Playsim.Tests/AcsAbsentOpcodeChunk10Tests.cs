using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsAbsentOpcodeChunk10Tests
{
    [Fact]
    public void SetLineBlocking_ClearsCreatureBlock()
    {
        var sim = RoomWithLine();
        var line = sim.Level.Lines[0];
        line.Flags = LevelLine.BlockingFlag;
        Run(sim, [3, 5, 3, 0, (int)AcsPcode.SetLineBlocking, (int)AcsPcode.Terminate], [], line);
        Assert.Equal(0, line.Flags & LevelLine.BlockingFlag);
    }

    [Fact]
    public void SetLineSpecial_WritesTriggerLineFields()
    {
        var sim = RoomWithLine();
        var line = sim.Level.Lines[0];
        Run(sim,
            [3, 5, 3, 112, 3, 1, 3, 2, 3, 3, 3, 4, 3, 99, (int)AcsPcode.SetLineSpecial, (int)AcsPcode.Terminate],
            [], line);
        Assert.Equal(112, line.Special);
        Assert.Equal(1, line.Arg0);
        Assert.Equal(99, line.Arg4);
    }

    [Fact]
    public void SectorSound_QueuesOnTriggerSector()
    {
        var sim = RoomWithLine();
        var line = sim.Level.Lines[0];
        Run(sim, [3, 0, 3, 100, (int)AcsPcode.SectorSound, (int)AcsPcode.Terminate], ["world/drip"], line);
        var request = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("world/drip", request.Sound);
        Assert.Equal(100 / 127f, request.Volume, precision: 3);
    }

    [Fact]
    public void SoundSequence_StoresOnFrontSector()
    {
        var sim = RoomWithLine();
        var line = sim.Level.Lines[0];
        Run(sim, [3, 0, (int)AcsPcode.SoundSequence, (int)AcsPcode.Terminate], ["Water"], line);
        Assert.Equal("Water", sim.SectorAcsSoundSequence(0));
    }

    [Fact]
    public void PrintBinaryAndHex_AppendToBuffer()
    {
        var sim = Room();
        Run(sim, [3, 5, (int)AcsPcode.PrintBinary, 3, 255, (int)AcsPcode.PrintHex, (int)AcsPcode.Terminate], []);
        Assert.Equal("101FF", sim.AcsPrintBuffer);
    }

    [Fact]
    public void GetPlayerInputOpcode_ReadsPlayerButtons()
    {
        var sim = Room();
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 100 });
        sim.Tick();
        Run(sim, [3, 0, 3, 5, (int)AcsPcode.GetPlayerInput, (int)AcsPcode.SetResultValue, (int)AcsPcode.Terminate], []);
        Assert.Equal(100, sim.Acs.ScriptResult);
    }

    [Fact]
    public void CheckActorFloorTexture_MatchesSectorFlat()
    {
        var sim = Room();
        sim.Level.Sectors[0].FloorPic = "FLOOR1";
        Run(sim, [3, 0, 3, 0, (int)AcsPcode.CheckActorFloorTexture, (int)AcsPcode.SetResultValue, (int)AcsPcode.Terminate], ["FLOOR1"]);
        Assert.Equal(1, sim.Acs.ScriptResult);
    }

    [Fact]
    public void SetMugshotState_UpdatesPlayer()
    {
        var sim = Room();
        Run(sim, [3, 0, (int)AcsPcode.SetMugshotState, (int)AcsPcode.Terminate], ["ouch"]);
        Assert.Equal("ouch", sim.Players.Single().MugshotState);
    }

    [Fact]
    public void CallFunc_SetSkyScrollSpeed_UpdatesLevel()
    {
        var sim = Room();
        RunCall(sim, 2, AcsCallFunctions.SetSkyScrollSpeed, [1, 65536]);
        Assert.Equal(1, sim.Level.SkySpeed1, precision: 4);
    }

    [Fact]
    public void CallFunc_SetAndGetTeamScore()
    {
        var sim = Room();
        RunCall(sim, 2, AcsCallFunctions.SetTeamScore, [2, 42]);
        RunCall(sim, 1, AcsCallFunctions.GetTeamScore, [2], lightLevel: 42);
        Assert.Equal(42, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_InvasionGetWave_Wire19702()
    {
        var sim = Room();
        sim.Invasion.Enabled = true;
        sim.Invasion.Configure(3, 0, 1);
        sim.Invasion.Tick(sim);
        RunCall(sim, 0, AcsCallFunctions.InvasionGetWave, []);
        Assert.Equal(1, sim.LightOf(0));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });

    private static AuthoritySimulation RoomWithLine()
    {
        var level = new PlayLevel
        {
            Sectors = [new LevelSector { Index = 0, Tag = 7, CeilingHeight = 128 }],
            Sides = [new LevelSide { Index = 0, Sector = 0, MidTexture = "-" }],
            Lines =
            [
                new LevelLine
                {
                    Index = 0, V1 = 0, V2 = 1, SideFront = 0, SideBack = -1, Tag = 5,
                    X1 = 0, Y1 = 0, X2 = 64, Y2 = 0,
                },
            ],
            Vertices = [new LevelVertex { Index = 0 }, new LevelVertex { Index = 1 }],
            Things = [new LevelThing { Type = 1 }],
        };
        return AuthoritySimulation.Start(level);
    }

    private static void Run(AuthoritySimulation sim, int[] words, string[] strings, LevelLine? triggerLine = null)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes, StringTable = strings });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.Single(), triggerLine));
        sim.Acs.Tick(sim);
    }

    private static void RunCall(AuthoritySimulation sim, int argCount, int function, int[] callArgs, int lightLevel = 1)
    {
        var words = new List<int>();
        foreach (var arg in callArgs) words.AddRange([(int)AcsPcode.PushNumber, arg]);
        words.Add((int)AcsPcode.CallFunc);
        words.Add(argCount);
        words.Add(function);
        words.Add((int)AcsPcode.Lspec2Direct);
        words.Add(112);
        words.Add(7);
        words.Add(lightLevel);
        Run(sim, words.ToArray(), []);
    }
}
