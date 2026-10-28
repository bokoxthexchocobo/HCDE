using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmPSpecTestActivateLineTests
{
    [Fact]
    public void RejectFirstSideOnly_WhenBackSide() =>
        Assert.True(VmPSpecTestActivateLine.RejectFirstSideOnly(1, 1));

    [Fact]
    public void PromoteMissileTeleport_WhenCrossTeleport() =>
        Assert.True(VmPSpecTestActivateLine.PromoteMissileTeleport(70, 2, 4, true));

    [Fact]
    public void DoorRaiseMonsterPath_SlowSpeed() =>
        Assert.True(VmPSpecTestActivateLine.DoorRaiseMonsterPath(12, 32));

    [Fact]
    public void ExpandAnyCross_AddsCrossBits() =>
        Assert.Equal(518, VmPSpecTestActivateLine.ExpandAnyCross(512));

    [Fact]
    public void TestActivateLine_AllowsMonsterCrossTeleport()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Things = [new LevelThing { Type = 3001, X = 0, Y = 0 }],
            Lines =
            [
                new LevelLine
                {
                    Special = 70,
                    MonsterCross = true,
                    Flags = LevelLine.TwoSidedFlag,
                    SideFront = 0,
                    SideBack = 0,
                },
            ],
        });
        var monster = sim.Actors.First(a => a is not PlayerPawn);
        var line = sim.Level.Lines[0];
        Assert.True(VmPSpecTestActivateLine.TestActivateLine(
            sim, line, monster, 0, VmPSpecTestActivateLine.SpacMcross));
    }
}
