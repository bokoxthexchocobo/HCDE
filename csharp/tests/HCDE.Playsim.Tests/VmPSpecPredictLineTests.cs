using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmPSpecPredictLineTests
{
    static (AuthoritySimulation Sim, PlayerPawn Player, LevelLine Line) TeleportRoom()
    {
        var line = new LevelLine
        {
            Index = 1,
            X1 = 32, Y1 = -32, X2 = 32, Y2 = 32,
            Special = 70,
            Arg0 = 1,
            Arg1 = 7,
            PlayerCross = true,
            SideFront = 0,
            SideBack = 1,
            Flags = LevelLine.TwoSidedFlag,
        };
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors =
            [
                new LevelSector { CeilingHeight = 128, FloorHeight = 0, Tag = 7 },
                new LevelSector { CeilingHeight = 128, FloorHeight = 0 },
            ],
            Sides =
            [
                new LevelSide { Sector = 0 },
                new LevelSide { Sector = 1 },
            ],
            Things =
            [
                new LevelThing { Type = 1, X = 0, Y = 0 },
                new LevelThing { Type = 14, Id = 1, X = 64, Y = 0 },
            ],
            Lines = [line],
        });
        var player = sim.Players.First();
        player.SectorIndex = 0;
        return (sim, player, line);
    }

    [Fact]
    public void RejectWhenNotPredictableSpecial()
    {
        var (sim, player, _) = TeleportRoom();
        Assert.False(VmPSpecPredictLine.PredictLine(
            sim, new LevelLine { Special = 12 }, player, 0, VmPSpecTestActivateLine.SpacCross));
    }

    [Fact]
    public void RejectWhenLocked()
    {
        var (sim, player, line) = TeleportRoom();
        line.LockNumber = 1;
        Assert.False(VmPSpecPredictLine.PredictLine(sim, line, player, 0, VmPSpecTestActivateLine.SpacCross));
    }

    [Fact]
    public void RejectWhenPredictCvarOff()
    {
        var (sim, player, line) = TeleportRoom();
        sim.ClPredictSpecials = false;
        Assert.False(VmPSpecPredictLine.PredictLine(sim, line, player, 0, VmPSpecTestActivateLine.SpacCross));
    }

    [Fact]
    public void PredictTeleport_WhenActivateAndCvarOk()
    {
        var (sim, player, line) = TeleportRoom();
        Assert.True(VmPSpecPredictLine.PredictLine(sim, line, player, 0, VmPSpecTestActivateLine.SpacCross));
    }
}
