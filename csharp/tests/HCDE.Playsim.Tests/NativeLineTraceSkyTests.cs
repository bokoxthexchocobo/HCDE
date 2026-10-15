using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeLineTraceSkyTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void PlaneSkyIsReportedUnlessNoSkyIsRequested(bool ceiling, bool noSky)
    {
        var sim = PlaneRoom("F_SKY1"); var source = sim.Players.Single();
        var hit = CombatTrace.LineTrace(sim, source, default, 256, BamAngle.FromDegrees(ceiling ? -90 : 90),
            noSky ? ActorLineTraceFlags.NoSky : 0, offsetZ: 28);
        Assert.Equal(!noSky, hit.Hit); Assert.Equal(ceiling ? 128 : 0, hit.Z, 8);
        Assert.Equal(noSky ? -1 : 0, hit.PlaneSector); Assert.Equal(noSky ? -1 : ceiling ? 1 : 0, hit.PlanePart);
    }

    [Theory]
    [InlineData("F_SKY1", "f_sky1", false, true)]
    [InlineData("F_SKY1", "f_sky1", true, false)]
    [InlineData("STONE", "F_SKY1", true, true)]
    [InlineData("F_SKY1", "STONE", true, true)]
    public void UpperWallSkyPolicyRequiresTwoSkyCeilings(string near, string far, bool noSky, bool expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128, CeilingPic = near }, new LevelSector { CeilingHeight = 20, CeilingPic = far }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [new LevelLine { X1 = 64, X2 = 64, Y1 = 128, Y2 = -128, SideFront = 0, SideBack = 1 }],
            Things = [new LevelThing { Type = 1 }]
        });
        var hit = CombatTrace.LineTrace(sim, sim.Players.Single(), default, 256,
            flags: noSky ? ActorLineTraceFlags.NoSky : 0, offsetZ: 36);
        Assert.Equal(expected, hit.Hit); Assert.Equal(expected ? sim.Level.Lines[0] : null, hit.Wall);
        Assert.Equal(64, hit.X, 8);
    }

    [Fact]
    public void NoSkyDoesNotSuppressOrdinaryPlaneHits()
    {
        var sim = PlaneRoom("STONE"); var hit = CombatTrace.LineTrace(sim, sim.Players.Single(), default, 256,
            BamAngle.FromDegrees(-90), ActorLineTraceFlags.NoSky, offsetZ: 28);
        Assert.True(hit.Hit); Assert.Equal(1, hit.PlanePart);
    }

    [Fact]
    public void AttackSkySuppressionRemainsSeparateFromDefaultLineTrace()
    {
        var sim = PlaneRoom("F_SKY1"); var source = sim.Players.Single(); var pitch = BamAngle.FromDegrees(-90);
        Assert.False(CombatTrace.TraceLineAttack(sim, source, default, pitch, 256).Hit);
        Assert.True(CombatTrace.LineTrace(sim, source, default, 256, pitch, offsetZ: source.AttackOffset()).Hit);
    }

    [Fact]
    public void SkyTraceDoesNotMutateDamageOrSavedState()
    {
        var sim = PlaneRoom("F_SKY1"); var saved = SimSavegame.Write(sim, true);
        CombatTrace.LineTrace(sim, sim.Players.Single(), default, 256, BamAngle.FromDegrees(-90), offsetZ: 28);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation PlaneRoom(string texture) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128, FloorPic = texture, CeilingPic = texture, HealthFloor = 100, HealthCeiling = 100 }],
        Things = [new LevelThing { Type = 1 }]
    });
}
