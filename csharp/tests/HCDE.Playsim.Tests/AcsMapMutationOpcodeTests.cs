using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsMapMutationOpcodeTests
{
    [Fact]
    public void ChangeFloorDirectUpdatesTaggedSectors()
    {
        var sim = Room();
        Run(sim, [(int)AcsPcode.ChangeFloorDirect, 7, 0, (int)AcsPcode.Terminate], ["NEWFLAT"]);
        Assert.Equal("NEWFLAT", sim.Level.Sectors[0].FloorPic);
        Assert.Equal("OLD", sim.Level.Sectors[1].FloorPic);
    }

    [Fact]
    public void ChangeCeilingStackOpcodeUpdatesTaggedSectors()
    {
        var sim = Room();
        Run(sim, [3, 7, 3, 0, (int)AcsPcode.ChangeCeiling, (int)AcsPcode.Terminate], ["SKY1"]);
        Assert.Equal("SKY1", sim.Level.Sectors[0].CeilingPic);
    }

    [Fact]
    public void SetLineTextureUpdatesFrontMiddle()
    {
        var sim = RoomWithLine();
        Run(sim, [3, 5, 3, 0, 3, 1, 3, 0, (int)AcsPcode.SetLineTexture, (int)AcsPcode.Terminate], ["BRICK"]);
        Assert.Equal("BRICK", sim.Level.Sides[0].MidTexture);
    }

    [Fact]
    public void ClearLineSpecialZerosTriggerLine()
    {
        var sim = RoomWithLine();
        var line = sim.Level.Lines[0];
        line.Special = 112;
        Run(sim, [(int)AcsPcode.ClearLineSpecial, (int)AcsPcode.Terminate], [], line);
        Assert.Equal(0, line.Special);
    }

    [Fact]
    public void SpawnDirectCreatesActorAndReturnsCount()
    {
        var sim = SingleSectorRoom();
        Run(sim, [(int)AcsPcode.SpawnDirect, 0, 0, 0, 0, 0, 0, (int)AcsPcode.Terminate], ["DoomImp"]);
        Assert.Equal(2, sim.Actors.Count(actor => !actor.Destroyed));
        Assert.Equal(1, sim.Actors.Count(actor => !actor.Destroyed && actor.DoomEdNum == 3001));
    }

    [Fact]
    public void SpawnSpotDirectUsesTidAnchor()
    {
        var sim = SingleSectorRoom();
        var spot = sim.Actors.Single();
        spot.ThingId = 9;
        Run(sim, [(int)AcsPcode.SpawnSpotDirect, 0, 9, 0, 0, (int)AcsPcode.Terminate], ["DoomImp"]);
        Assert.Equal(1, sim.Actors.Count(actor => !actor.Destroyed && actor.DoomEdNum == 3001));
    }

    [Fact]
    public void SetResultValueExposesScriptResult()
    {
        var sim = Room();
        Run(sim, [3, 42, (int)AcsPcode.SetResultValue, (int)AcsPcode.Terminate], []);
        Assert.Equal(42, sim.Acs.ScriptResult);
    }

    private static AuthoritySimulation SingleSectorRoom() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, FloorPic = "OLD", CeilingPic = "OLD" }],
        Things = [new LevelThing { Type = 1 }],
    });

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors =
        [
            new LevelSector { Tag = 7, FloorPic = "OLD", CeilingPic = "OLD" },
            new LevelSector { Tag = 8, FloorPic = "OLD", CeilingPic = "OLD" },
        ],
        Things = [new LevelThing { Type = 1 }],
    });

    private static AuthoritySimulation RoomWithLine()
    {
        var level = new PlayLevel
        {
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }],
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
}
