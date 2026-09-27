using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StairActionsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TagWaitReleasesCompletedStepWhileChainRemainsLocked(bool direct)
    {
        var sim = Room(); Assert.True(Start(sim)); RegisterWait(sim, direct);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        for (var i = 0; i < 31; i++) sim.Tick();
        Assert.Equal(1, sim.Acs.RunningCount);
        sim.Tick(); Assert.Equal(0, sim.Acs.RunningCount);
        Assert.Equal(8, sim.FloorOf(0)); Assert.Equal(8, sim.FloorOf(2));
        Assert.False(Start(sim));
    }

    [Fact]
    public void OrdinaryFloorCanMoveCompletedStepWithoutUnlockingStairChain()
    {
        var sim = Room(); Assert.True(Start(sim));
        for (var i = 0; i < 32; i++) sim.Tick();
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(),
            new LevelLine { Special = 58, Tag = 7 }, false));
        Assert.False(Start(sim));
        sim.Tick(); Assert.Equal(9, sim.FloorOf(0));
        for (var i = 0; i < 63; i++) sim.Tick();
        Assert.Equal(32, sim.FloorOf(0)); Assert.Equal(24, sim.FloorOf(2));
        Assert.True(Start(sim));
    }

    [Fact]
    public void NewFloorMovementStillBlocksTagWaitOnCompletedStair()
    {
        var sim = Room(); Assert.True(Start(sim));
        for (var i = 0; i < 32; i++) sim.Tick();
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(),
            new LevelLine { Special = 58, Tag = 7 }, false));
        RegisterWait(sim, true); Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        for (var i = 0; i < 23; i++) sim.Tick();
        Assert.Equal(1, sim.Acs.RunningCount);
        sim.Tick(); Assert.Equal(0, sim.Acs.RunningCount); Assert.False(Start(sim));
    }

    [Fact]
    public void InternalFloorActionAlsoUsesPlaneOwnershipInsteadOfStairLock()
    {
        var sim = Room(); Assert.True(Start(sim));
        Assert.False(LineSpecials.Execute(sim, sim.Players.Single(), LineSpecials.FloorRaise, 7));
        for (var i = 0; i < 32; i++) sim.Tick();
        Assert.True(LineSpecials.Execute(sim, sim.Players.Single(), LineSpecials.FloorRaise, 7));
        sim.Tick(); Assert.Equal(16, sim.FloorOf(0)); Assert.False(Start(sim));
    }

    [Fact]
    public void IncompleteStepStillRejectsOrdinaryFloorAction()
    {
        var sim = Room(); Assert.True(Start(sim)); sim.Tick();
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(),
            new LevelLine { Special = 58, Tag = 7 }, false));
    }

    private static void RegisterWait(AuthoritySimulation sim, bool direct)
    {
        int[] words = direct ? [62, 7, 1] : [3, 7, 61, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
    }

    private static AuthoritySimulation Room(bool differentTexture = false, bool reversed = false, bool cycle = false, bool twoSided = true) =>
        AuthoritySimulation.Start(new PlayLevel {
            Format = MapDataFormat.DoomBinary,
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, FloorPic = "STEP" },
                new LevelSector { Index = 1, CeilingHeight = 128, FloorPic = differentTexture ? "OTHER" : "step" },
                new LevelSector { Index = 2, CeilingHeight = 128, FloorPic = "STEP" }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }, new LevelSide { Sector = 2 }],
            Lines = [new LevelLine { Flags = twoSided ? LevelLine.TwoSidedFlag : 0, SideFront = reversed ? 1 : 0, SideBack = reversed ? 0 : 1 },
                new LevelLine { Flags = LevelLine.TwoSidedFlag, SideFront = 1, SideBack = 2 },
                new LevelLine { Flags = LevelLine.TwoSidedFlag, SideFront = 2, SideBack = cycle ? 0 : -1 }],
            Things = [new LevelThing { Type = 1 }],
        });

    private static bool Start(AuthoritySimulation sim, int special = 256, bool use = false) =>
        LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine { Special = special, Tag = 7 }, use);

    [Fact]
    public void BackSideReferenceWithoutTwoSidedFlagDoesNotExtendStairChain()
    {
        var sim = Room(twoSided: false); Assert.True(Start(sim));
        for (var i = 0; i < 96; i++) sim.Tick();
        Assert.Equal(8, sim.FloorOf(0)); Assert.Equal(0, sim.FloorOf(1)); Assert.Equal(0, sim.FloorOf(2));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UdmfTwoSidedFlagSurvivesLoadingAndControlsStairTraversal(bool twoSided)
    {
        var text = """
            namespace = "ZDoom";
            vertex { x = 0; y = 0; } vertex { x = 0; y = 128; }
            sector { id = 7; heightceiling = 128; texturefloor = "STEP"; }
            sector { heightceiling = 128; texturefloor = "STEP"; }
            sidedef { sector = 0; } sidedef { sector = 1; }
            linedef { v1 = 0; v2 = 1; sidefront = 0; sideback = 1; twosided = TWO_SIDED; }
            thing { type = 1; x = 32; y = 32; skill3 = true; single = true; coop = true; dm = true; }
            """.Replace("TWO_SIDED", twoSided ? "true" : "false");
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var level = LevelBuilder.FromUdmf(map, "MAP01");
        Assert.Equal(twoSided, (level.Lines[0].Flags & LevelLine.TwoSidedFlag) != 0);
        var sim = AuthoritySimulation.Start(level);
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine
            { Special = 217, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true }, true));
        for (var i = 0; i < 16; i++) sim.Tick();
        Assert.Equal(8, sim.FloorOf(0)); Assert.Equal(twoSided ? 16 : 0, sim.FloorOf(1));
    }

    [Theory]
    [InlineData(8, false, 8, 0.25)]
    [InlineData(7, true, 8, 0.25)]
    [InlineData(100, false, 16, 4)]
    [InlineData(127, true, 16, 4)]
    public void DoomStairsBuildAscendingChain(int special, bool use, double height, double speed)
    {
        var sim = Room();
        Assert.True(Start(sim, special, use));
        sim.Tick();
        Assert.Equal(speed, sim.FloorOf(0)); Assert.Equal(speed, sim.FloorOf(1)); Assert.Equal(speed, sim.FloorOf(2));
        for (var i = 0; i < height * 3 / speed; i++) sim.Tick();
        Assert.Equal(height, sim.FloorOf(0)); Assert.Equal(height * 2, sim.FloorOf(1)); Assert.Equal(height * 3, sim.FloorOf(2));
    }

    [Fact]
    public void WholeChainStaysLockedUntilLastStepCompletes()
    {
        var sim = Room(); Assert.True(Start(sim));
        for (var i = 0; i < 32; i++) sim.Tick();
        Assert.Equal(8, sim.FloorOf(0)); Assert.False(Start(sim));
        for (var i = 0; i < 64; i++) sim.Tick();
        Assert.Equal(24, sim.FloorOf(2)); Assert.True(Start(sim));
        sim.Tick(); Assert.Equal(8.25, sim.FloorOf(0));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void TextureAndSidednessStopChainTraversal(bool different, bool reversed)
    {
        var sim = Room(different, reversed); Assert.True(Start(sim));
        for (var i = 0; i < 96; i++) sim.Tick();
        Assert.Equal(8, sim.FloorOf(0)); Assert.Equal(0, sim.FloorOf(1)); Assert.Equal(0, sim.FloorOf(2));
    }

    [Fact]
    public void CyclicSectorGraphBuildsEachStepOnce()
    {
        var sim = Room(cycle: true); Assert.True(Start(sim));
        for (var i = 0; i < 96; i++) sim.Tick();
        Assert.Equal(8, sim.FloorOf(0)); Assert.Equal(16, sim.FloorOf(1)); Assert.Equal(24, sim.FloorOf(2));
    }
}
