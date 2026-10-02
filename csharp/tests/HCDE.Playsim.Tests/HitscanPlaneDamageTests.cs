using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HitscanPlaneDamageTests
{
    private static AuthoritySimulation Room(string texture = "STONE", int wallX = 200) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = 1000, HealthCeiling = 1000,
            HealthFloorGroup = 4, HealthCeilingGroup = 5, FloorPic = texture, CeilingPic = texture }],
        Sides = [new LevelSide { Sector = 0 }],
        Lines = [new LevelLine { X1 = wallX, Y1 = 128, X2 = wallX, Y2 = -128, SideFront = 0,
            SideBack = -1, Health = 1000 }, new LevelLine { Health = 1000, HealthGroup = 4 },
            new LevelLine { Health = 1000, HealthGroup = 5 }],
        Things = [new LevelThing { Type = 1 }],
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsAttackHitsPlaneBeforeFarWall(bool ceiling)
    {
        var sim = Room(); var player = sim.Players.Single(); player.PitchDegrees = ceiling ? -45 : 45;
        var hit = CombatTrace.TraceLineAttack(sim, player, player.Angle, BamAngle.FromDegrees(player.PitchDegrees), 512);
        Assert.Equal(0, hit.PlaneSector);
        Assert.Equal(ceiling ? 1 : 0, hit.PlanePart);
        Assert.Null(hit.Wall);
        Assert.Equal(ceiling ? 128 : 0, hit.Z, 5);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthFloor);
        AcsLineAttack.Attack(sim, player, 0, 0, 30, "None", 512);
        Assert.Equal(970, ceiling ? sim.Level.Sectors[0].HealthCeiling : sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(970, sim.Level.Lines[ceiling ? 2 : 1].Health);
        Assert.Equal(1000, sim.Level.Lines[0].Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PistolDamagesPlaneAlongPitch(bool ceiling)
    {
        var sim = Room(); var player = sim.Players.Single(); player.PitchDegrees = ceiling ? -45 : 45;
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.InRange(ceiling ? sim.Level.Sectors[0].HealthCeiling : sim.Level.Sectors[0].HealthFloor, 985, 995);
        Assert.Equal(1000, sim.Level.Lines[0].Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StandardSkyPlanesBlockGeometryDamage(bool ceiling)
    {
        var sim = Room("F_SKY1"); var player = sim.Players.Single(); player.PitchDegrees = ceiling ? -45 : 45;
        AcsLineAttack.Attack(sim, player, 0, 0, 30, "None", 512);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthCeiling);
        Assert.Equal(1000, sim.Level.Lines[0].Health);
    }

    [Fact]
    public void CloserWallWinsOverPlane()
    {
        var sim = Room(wallX: 10); var player = sim.Players.Single(); player.PitchDegrees = 45;
        AcsLineAttack.Attack(sim, player, 0, 0, 30, "None", 512);
        Assert.Equal(970, sim.Level.Lines[0].Health);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthFloor);
    }

    [Fact]
    public void CloserActorWinsOverPlane()
    {
        var sim = Room(); var player = sim.Players.Single(); player.PitchDegrees = 45;
        var target = sim.AddBot(20, 0); var health = target.Health;
        Assert.Equal(1, AcsLineAttack.Attack(sim, player, 0, 0, 30, "None", 512));
        Assert.Equal(health - 30, target.Health);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthFloor);
    }

    [Fact]
    public void PlaneBeyondRangeIsNotHit()
    {
        var sim = Room(); var player = sim.Players.Single(); player.PitchDegrees = 45;
        Assert.False(CombatTrace.TraceLineAttack(sim, player, player.Angle, BamAngle.FromDegrees(45), 10).Hit);
    }
}
