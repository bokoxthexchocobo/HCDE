using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TraceParallelPlaneOpeningTests
{
    [Theory]
    [InlineData(false, false, false, 0)]
    [InlineData(false, true, false, 0)]
    [InlineData(true, false, false, 0)]
    [InlineData(true, true, false, 0)]
    [InlineData(false, false, true, 0)]
    [InlineData(false, true, true, 0)]
    [InlineData(true, false, true, 0)]
    [InlineData(true, true, true, 0)]
    [InlineData(false, false, false, LevelLine.BlockHitscanFlag)]
    [InlineData(false, true, false, LevelLine.BlockHitscanFlag)]
    [InlineData(true, false, false, LevelLine.BlockHitscanFlag)]
    [InlineData(true, true, false, LevelLine.BlockHitscanFlag)]
    [InlineData(false, false, true, LevelLine.BlockHitscanFlag)]
    [InlineData(false, true, true, LevelLine.BlockHitscanFlag)]
    [InlineData(true, false, true, LevelLine.BlockHitscanFlag)]
    [InlineData(true, true, true, LevelLine.BlockHitscanFlag)]
    [InlineData(false, false, false, LevelLine.BlockEverythingFlag)]
    [InlineData(false, true, false, LevelLine.BlockEverythingFlag)]
    [InlineData(true, false, false, LevelLine.BlockEverythingFlag)]
    [InlineData(true, true, false, LevelLine.BlockEverythingFlag)]
    [InlineData(false, false, true, LevelLine.BlockEverythingFlag)]
    [InlineData(false, true, true, LevelLine.BlockEverythingFlag)]
    [InlineData(true, false, true, LevelLine.BlockEverythingFlag)]
    [InlineData(true, true, true, LevelLine.BlockEverythingFlag)]
    public void HorizontalNearPlaneFallsBackToDestinationOpening(bool reverse, bool ceiling, bool closed, int flags)
    {
        var near = new LevelSector { FloorHeight = ceiling ? 0 : 28, CeilingHeight = ceiling ? 28 : 128 };
        var far = new LevelSector { FloorHeight = closed && !ceiling ? 28 : 0,
            CeilingHeight = closed && ceiling ? 28 : 128 };
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = reverse ? [far, near] : [near, far],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [new LevelLine { X1 = 64, Y1 = 128, X2 = 64, Y2 = -128, SideFront = 0, SideBack = 1, Flags = flags }],
            Things = [new LevelThing { Type = 1, X = reverse ? 128 : 0, Angle = reverse ? 180 : 0 }],
        });
        var source = CenterOrigin(sim); source.Z = Fixed.FromInt(0);
        var target = sim.AddBot(reverse ? -64 : 192, 0); target.Z = Fixed.FromInt(0);
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(0), 256);
        Assert.Equal(closed, hit.Wall is not null);
        if (closed) Assert.Null(hit.Victim); else Assert.Same(target, hit.Victim);
        Assert.Equal(closed ? null : target, CombatTrace.PickActor(sim, source, source.Angle, new BamAngle(0), 256));
    }
    // These geometry fixtures intentionally trace from the actor center.
    private static PlayerPawn CenterOrigin(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); player.AttackZOffset = new Fixed(0); return player;
    }
}
