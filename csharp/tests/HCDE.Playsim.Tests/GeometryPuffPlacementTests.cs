using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GeometryPuffPlacementTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void WallPuffOffsetUsesOriginalDirectionEvenForNegativeRange(bool reverse, bool negative)
    {
        var aim = reverse ? -1 : 1; var travel = negative ? -aim : aim;
        var sim = Room(travel * 64); var source = CenterOrigin(sim);
        var angle = BamAngle.FromDegrees(reverse ? 180 : 0);
        var range = negative ? -128 : 128;
        var hit = CombatTrace.TraceLineAttack(sim, source, angle, new BamAngle(0), range);
        Assert.Same(sim.Level.Lines[0], hit.Wall);
        Assert.Equal(0, AcsLineAttack.Attack(sim, source, unchecked((int)angle.Raw), 0, 7, "None", range,
            puffTid: 42, absoluteAngles: true));
        var puff = Assert.Single(sim.Actors.OfType<PuffActor>());
        Assert.Equal(travel * 64 - aim * 4, puff.X.ToDouble(), 5);
        Assert.Equal(0, puff.Y.ToDouble(), 5); Assert.Equal(28, puff.Z.ToDouble(), 5);
        Assert.Equal(1, puff.Radius.Raw); Assert.Equal(42, puff.ThingId);
        Assert.Equal(93, sim.Level.Lines[0].Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlanePuffOffsetIncludesVerticalDirection(bool ceiling)
    {
        var sim = Room(200); var source = CenterOrigin(sim); var pitch = BamAngle.FromDegrees(ceiling ? -45 : 45);
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, pitch, 256);
        Assert.Equal(0, hit.PlaneSector);
        AcsLineAttack.Attack(sim, source, 0, unchecked((int)pitch.Raw), 7, "None", 256, puffTid: 42);
        var puff = Assert.Single(sim.Actors.OfType<PuffActor>());
        Assert.Equal(hit.X - 4 / Math.Sqrt(2), puff.X.ToDouble(), 4);
        Assert.Equal(hit.Z + (ceiling ? -1 : 1) * 4 / Math.Sqrt(2), puff.Z.ToDouble(), 4);
        Assert.Equal(1, puff.Radius.Raw); Assert.Equal(42, puff.ThingId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    public void HorizonStopsTraceWithoutSpawningNormalPuff(int puffTid)
    {
        var sim = Room(64); sim.Level.Lines[0].Special = 9; var source = CenterOrigin(sim);
        var count = sim.Actors.Count; var random = sim.CombatRandomState;
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(0), 128);
        Assert.True(hit.Hit); Assert.Same(sim.Level.Lines[0], hit.Wall);
        AcsLineAttack.Attack(sim, source, 0, 0, 7, "None", 128, puffTid);
        Assert.Empty(sim.Actors.OfType<PuffActor>()); Assert.Equal(count, sim.Actors.Count);
        Assert.Equal(random, sim.CombatRandomState); Assert.Equal(100, sim.Level.Lines[0].Health);
    }

    private static AuthoritySimulation Room(int wallX) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 0 }],
        Lines = [new LevelLine { X1 = wallX, X2 = wallX, Y1 = 128, Y2 = -128,
            SideFront = 0, SideBack = -1, Health = 100 }],
        Things = [new LevelThing { Type = 1 }],
    });
    // These geometry fixtures intentionally trace from the actor center.
    private static PlayerPawn CenterOrigin(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); player.AttackZOffset = new Fixed(0); return player;
    }
}
