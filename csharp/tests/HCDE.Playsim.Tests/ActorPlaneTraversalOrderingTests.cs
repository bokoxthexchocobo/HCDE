using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPlaneTraversalOrderingTests
{
    [Theory]
    [InlineData(false, 150, false)]
    [InlineData(false, 250, true)]
    [InlineData(true, 150, false)]
    [InlineData(true, 250, true)]
    public void EarlierLineStillResolvesPlaneBeforeActorEntry(bool ceiling, int wallX, bool actorFirst)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = wallX, X2 = wallX, Y1 = 128, Y2 = -128,
                SideFront = 0, SideBack = -1 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var source = sim.Players.Single(); var target = sim.AddBot(200, 0);
        target.Z = Fixed.FromInt(ceiling ? 200 : -240);
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, BamAngle.FromDegrees(ceiling ? -45 : 45), 512);
        Assert.Equal(actorFirst ? target : null, hit.Victim);
        Assert.Equal(actorFirst ? -1 : 0, hit.PlaneSector);
        Assert.Equal(actorFirst ? -1 : ceiling ? 1 : 0, hit.PlanePart);
        Assert.Null(hit.Wall);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void OrdinaryPlaneIsFallbackAfterActorTraversal(bool ceiling, bool reverse, bool missActor)
    {
        var sign = reverse ? -1 : 1;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = 100, HealthCeiling = 100,
                FloorPic = "STONE", CeilingPic = "STONE" }],
            Things = [new LevelThing { Type = 1, Angle = reverse ? 180 : 0 }],
        });
        var source = sim.Players.Single();
        var target = sim.AddBot(sign * 200, missActor ? 100 : 0);
        target.Z = Fixed.FromInt(ceiling ? 200 : -240);
        var pitch = BamAngle.FromDegrees(ceiling ? -45 : 45);
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, pitch, 512);
        Assert.True(hit.Hit);
        Assert.Equal(missActor ? null : target, hit.Victim);
        Assert.Null(hit.Wall);
        Assert.Equal(missActor ? 0 : -1, hit.PlaneSector);
        Assert.Equal(missActor ? (ceiling ? 1 : 0) : -1, hit.PlanePart);
        Assert.Equal(missActor ? null : target, CombatTrace.PickActor(sim, source, source.Angle, pitch, 512));
        var health = target.Health;
        AcsLineAttack.Attack(sim, source, unchecked((int)source.Angle.Raw), unchecked((int)pitch.Raw),
            7, "None", 512, absoluteAngles: true);
        Assert.Equal(missActor ? health : health - 7, target.Health);
        Assert.Equal(missActor ? 93 : 100,
            ceiling ? sim.Level.Sectors[0].HealthCeiling : sim.Level.Sectors[0].HealthFloor);
    }
}
