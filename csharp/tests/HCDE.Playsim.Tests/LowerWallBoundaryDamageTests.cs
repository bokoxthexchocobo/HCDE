using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LowerWallBoundaryDamageTests
{
    [Theory]
    [InlineData(false, -1, 93)]
    [InlineData(false, 0, 93)]
    [InlineData(false, 1, 100)]
    [InlineData(true, -1, 93)]
    [InlineData(true, 0, 93)]
    [InlineData(true, 1, 100)]
    public void BlockingWallDamagesBackFloorAtInclusiveBoundary(bool reverse, int zDelta, int floorHealth)
    {
        var sim = Room(reverse);
        var source = CenterOrigin(sim); source.Z = new Fixed(zDelta);
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(0), 256);
        Assert.Same(sim.Level.Lines[0], hit.Wall);
        Assert.Equal(reverse ? 1 : 0, hit.Side);
        GeometryLineAttack.Apply(sim, hit, 7);
        Assert.Equal(floorHealth, sim.Level.Sectors[reverse ? 0 : 1].HealthFloor);
        Assert.Equal(93, sim.Level.Lines[0].Health);
    }

    [Fact]
    public void SharedGroupReceivesFloorThenWallDamageAtBoundary()
    {
        var sim = Room(false, shared: true); var source = CenterOrigin(sim);
        AcsLineAttack.Attack(sim, source, 0, 0, 7, "None", 256);
        Assert.Equal(86, sim.HealthGroups[4]);
        Assert.Equal(86, sim.Level.Sectors[1].HealthFloor);
        Assert.Equal(86, sim.Level.Lines[0].Health);
    }

    private static AuthoritySimulation Room(bool reverse, bool shared = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [
            new LevelSector { FloorHeight = reverse ? 28 : 0, CeilingHeight = 128, HealthFloor = 100 },
            new LevelSector { FloorHeight = reverse ? 0 : 28, CeilingHeight = 128, HealthFloor = 100,
                HealthFloorGroup = shared ? 4 : 0 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines = [new LevelLine { X1 = 64, Y1 = 128, X2 = 64, Y2 = -128,
            SideFront = 0, SideBack = 1, Flags = LevelLine.BlockHitscanFlag, Health = 100, HealthGroup = shared ? 4 : 0 }],
        Things = [new LevelThing { Type = 1, X = reverse ? 128 : 0, Angle = reverse ? 180 : 0 }],
    });
    // These geometry fixtures intentionally trace from the actor center.
    private static PlayerPawn CenterOrigin(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); player.AttackZOffset = new Fixed(0); return player;
    }
}
