using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HitscanSkySuppressionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SkyPlaneIsDiscardedAtImpactWithoutPuffOrDamage(bool ceiling)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, FloorPic = "F_SKY1", CeilingPic = "f_sky1",
                HealthFloor = 100, HealthCeiling = 100 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var source = sim.Players.Single(); var pitch = BamAngle.FromDegrees(ceiling ? -45 : 45);
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, pitch, 256);
        Assert.False(hit.Hit); Assert.Equal(ceiling ? 128 : 0, hit.Z, 5);
        Assert.Null(hit.Wall); Assert.Equal(-1, hit.PlaneSector);
        var count = sim.Actors.Count;
        AcsLineAttack.Attack(sim, source, 0, unchecked((int)pitch.Raw), 7, "None", 256, puffTid: 42);
        Assert.Equal(count, sim.Actors.Count);
        Assert.Equal(100, sim.Level.Sectors[0].HealthFloor); Assert.Equal(100, sim.Level.Sectors[0].HealthCeiling);
    }

    [Theory]
    [InlineData("F_SKY1", "F_SKY1", true)]
    [InlineData("f_sky1", "F_SKY1", true)]
    [InlineData("STONE", "F_SKY1", false)]
    [InlineData("F_SKY1", "STONE", false)]
    public void UpperWallRequiresBothSkyCeilingsForSuppression(string frontTexture, string backTexture, bool sky)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, CeilingPic = frontTexture },
                new LevelSector { CeilingHeight = 20, CeilingPic = backTexture, HealthCeiling = 100 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [new LevelLine { X1 = 64, X2 = 64, Y1 = 128, Y2 = -128,
                SideFront = 0, SideBack = 1, Health = 100 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var source = sim.Players.Single(); var target = sim.AddBot(100, 0); target.Z = Fixed.FromInt(0);
        var health = target.Health; var count = sim.Actors.Count;
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(0), 256);
        Assert.Equal(!sky, hit.Hit); Assert.Null(hit.Victim); Assert.Equal(64, hit.X, 5);
        Assert.Equal(sky ? null : sim.Level.Lines[0], hit.Wall);
        Assert.Null(CombatTrace.PickActor(sim, source, source.Angle, new BamAngle(0), 256));
        AcsLineAttack.Attack(sim, source, 0, 0, 7, "None", 256, puffTid: 42);
        Assert.Equal(health, target.Health); Assert.Equal(sky ? count : count + 1, sim.Actors.Count);
        Assert.Equal(sky ? 100 : 93, sim.Level.Lines[0].Health);
        Assert.Equal(sky ? 100 : 93, sim.Level.Sectors[1].HealthCeiling);
    }
}
