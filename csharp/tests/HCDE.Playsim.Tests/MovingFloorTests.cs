using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MovingFloorTests
{
    [Fact]
    public void BlockedReturningLiftReversesWithoutDamage()
    {
        var sim = Room(floor: 64, neighbor: 0); var player = sim.Players.Single();
        Assert.True(LineSpecials.ActivateMapLine(sim, player, new LevelLine { Special = 62, Tag = 7 }, true));
        for (var tick = 0; tick < 16 + 105; tick++) sim.Tick();
        player.Height = Fixed.FromInt(80);
        for (var tick = 0; tick < 12; tick++) sim.Tick();
        Assert.Equal(48, sim.FloorOf(0));
        sim.Tick(); Assert.Equal(48, sim.FloorOf(0));
        sim.Tick(); Assert.Equal(44, sim.FloorOf(0));
        Assert.Equal(100, player.Health);
    }

    [Fact]
    public void MoverTargetsParticipateInChecksumBeforePositionsDiverge()
    {
        var first = Room(); var second = Room();
        LineSpecials.ActivateMapLine(first, first.Players.Single(), new LevelLine { Special = 18, Tag = 7 }, true);
        LineSpecials.ActivateMapLine(second, second.Players.Single(), new LevelLine { Special = 58, Tag = 7 }, false);
        first.Tick(); second.Tick();
        Assert.Equal(first.FloorOf(0), second.FloorOf(0));
        Assert.NotEqual(first.Checksum, second.Checksum);
    }
    private static AuthoritySimulation Room(short floor = 0, short neighbor = 64, short ceiling = 128,
        MapDataFormat format = MapDataFormat.DoomBinary) => AuthoritySimulation.Start(new PlayLevel {
            Format = format, Namespace = "ZDoom",
            Sectors = [new LevelSector { Tag = 7, FloorHeight = floor, CeilingHeight = ceiling },
                new LevelSector { Index = 1, FloorHeight = neighbor, CeilingHeight = 256 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [
                new LevelLine { X1=-128, Y1=-128, X2=128, Y2=-128, SideBack=-1 },
                new LevelLine { X1=128, Y1=-128, X2=128, Y2=128, SideBack=1 },
                new LevelLine { X1=128, Y1=128, X2=-128, Y2=128, SideBack=-1 },
                new LevelLine { X1=-128, Y1=128, X2=-128, Y2=-128, SideBack=-1 },
            ], Things = [new LevelThing { Type = 1 }],
        });

    [Fact]
    public void DoomFloorRaisesToNeighborAndCarriesPlayer()
    {
        var sim = Room(); var player = sim.Players.Single();
        var line = new LevelLine { Special = 18, Tag = 7 };
        Assert.False(LineSpecials.ActivateMapLine(sim, player, line, false));
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true));
        Assert.Equal(0, line.Special);
        for (var tick = 0; tick < 64; tick++) sim.Tick();
        Assert.Equal(64, sim.FloorOf(0));
        Assert.Equal(64, player.Z.ToDouble());
        Assert.True(player.OnGround);
    }

    [Fact]
    public void LiftLowersWaitsAndReturnsWithRider()
    {
        var sim = Room(floor: 64, neighbor: 0); var player = sim.Players.Single();
        var line = new LevelLine { Special = 62, Tag = 7 };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true));
        Assert.False(LineSpecials.ActivateMapLine(sim, player, line, true)); // No duplicate mover.
        for (var tick = 0; tick < 16; tick++) sim.Tick();
        Assert.Equal(0, sim.FloorOf(0)); Assert.Equal(0, player.Z.ToDouble());
        for (var tick = 0; tick < 105; tick++) sim.Tick();
        Assert.Equal(0, sim.FloorOf(0));
        for (var tick = 0; tick < 16; tick++) sim.Tick();
        Assert.Equal(64, sim.FloorOf(0)); Assert.Equal(64, player.Z.ToDouble());
        Assert.Equal(62, line.Special);
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true));
    }

    [Fact]
    public void NonCrushingFloorWaitsForObstructionToClear()
    {
        var sim = Room(neighbor: 24, ceiling: 64); var player = sim.Players.Single();
        Assert.True(LineSpecials.ActivateMapLine(sim, player, new LevelLine { Special = 18, Tag = 7 }, true));
        for (var tick = 0; tick < 30; tick++) sim.Tick();
        Assert.Equal(8, sim.FloorOf(0)); Assert.Equal(100, player.Health);
        Assert.Equal(8, player.Z.ToDouble());
        player.Health = 0;
        for (var tick = 0; tick < 20; tick++) sim.Tick();
        Assert.Equal(24, sim.FloorOf(0));
    }

    [Fact]
    public void CrushingFloorUsesDamageLifecycleAndStopsBelowCeiling()
    {
        var sim = Room(ceiling: 64); var player = sim.Players.Single();
        Assert.True(LineSpecials.ActivateMapLine(sim, player, new LevelLine { Special = 55, Tag = 7 }, true));
        for (var tick = 0; tick < 12; tick++) sim.Tick();
        Assert.Equal(90, player.Health);
        for (var tick = 0; tick < 60; tick++) sim.Tick();
        Assert.True(player.IsDead); Assert.Equal(1, player.DeathCount);
        Assert.Equal(56, sim.FloorOf(0));
    }

    [Fact]
    public void ZdoomFloorUsesArgsInsteadOfLineIdAndDoesNotWrapLargeHeight()
    {
        var sim = Room(ceiling: 256, format: MapDataFormat.UdmfText); var player = sim.Players.Single();
        var line = new LevelLine { Special = 23, Tag = 999, Arg0 = 7, Arg1 = 16, Arg2 = int.MaxValue, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true));
        sim.Tick(); Assert.Equal(2, sim.FloorOf(0));
        player.Health = 0;
        for (var tick = 0; tick < 130; tick++) sim.Tick();
        Assert.Equal(256, sim.FloorOf(0));
    }

    [Fact]
    public void MissingNeighborDoesNotConsumeOneShotRaise()
    {
        var sim = Room(neighbor: -16); var line = new LevelLine { Special = 18, Tag = 7 };
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(18, line.Special);
    }
}
