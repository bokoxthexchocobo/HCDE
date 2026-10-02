using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CombatTraceMissEndpointTests
{
    [Theory]
    [InlineData(0, 0, 64)]
    [InlineData(180, 0, 64)]
    [InlineData(90, 0, 64)]
    [InlineData(0, 45, 64)]
    [InlineData(0, -45, 64)]
    [InlineData(0, 0, -64)]
    [InlineData(90, -45, -64)]
    [InlineData(0, 45, 0)]
    public void MissReportsSignedRangeEndpointWithoutAttackSideEffects(double yaw, double pitch, double range)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { FloorHeight = -1024, CeilingHeight = 1024 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var source = sim.Players.Single(); source.X = Fixed.FromInt(10); source.Y = Fixed.FromInt(20);
        source.Z = Fixed.FromInt(30);
        var angle = BamAngle.FromDegrees(yaw); var pitchAngle = BamAngle.FromDegrees(pitch);
        var count = sim.Actors.Count; var random = sim.CombatRandomState;
        var hit = CombatTrace.TraceLineAttack(sim, source, angle, pitchAngle, range);
        Assert.False(hit.Hit); Assert.Null(hit.Victim); Assert.Null(hit.Wall);
        Assert.Equal(-1, hit.PlaneSector); Assert.Equal(-1, hit.PlanePart);
        var yawRadians = yaw * Math.PI / 180; var pitchRadians = pitch * Math.PI / 180;
        Assert.Equal(10 + Math.Cos(yawRadians) * Math.Cos(pitchRadians) * range, hit.X, 5);
        Assert.Equal(20 + Math.Sin(yawRadians) * Math.Cos(pitchRadians) * range, hit.Y, 5);
        Assert.Equal(58 - Math.Sin(pitchRadians) * range, hit.Z, 5);
        Assert.Null(CombatTrace.PickActor(sim, source, angle, pitchAngle, range));
        Assert.Equal(0, AcsLineAttack.Attack(sim, source, unchecked((int)angle.Raw), unchecked((int)pitchAngle.Raw),
            7, "None", range, puffTid: 42, absoluteAngles: true));
        Assert.Equal(count, sim.Actors.Count); Assert.Equal(random, sim.CombatRandomState);
    }
}
