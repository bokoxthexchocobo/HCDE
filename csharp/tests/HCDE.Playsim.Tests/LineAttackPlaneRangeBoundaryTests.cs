using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LineAttackPlaneRangeBoundaryTests
{
    [Theory]
    [InlineData(16384, 28 * 65536 - 1, false)]
    [InlineData(16384, 28 * 65536, false)]
    [InlineData(16384, 28 * 65536 + 1, true)]
    [InlineData(-16384, 100 * 65536 - 1, false)]
    [InlineData(-16384, 100 * 65536, false)]
    [InlineData(-16384, 100 * 65536 + 1, true)]
    public void AcsPlaneHitMustPrecedeRangeEndpoint(int pitch, int range, bool hit)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = 100, HealthCeiling = 100 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var stack = new List<int> { 987, 0, 0, pitch, 5, 0, 0, range, 0, 19 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack,
            new AcsActivatorBinding { Value = CenterOrigin(sim) }, [], new AcsGlobalStrings(),
            AcsCallFunctions.LineAttack, 9, out var result));
        Assert.Equal(0, result);
        Assert.Equal(new[] { 987 }, stack);
        Assert.Equal(hit && pitch > 0 ? 95 : 100, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(hit && pitch < 0 ? 95 : 100, sim.Level.Sectors[0].HealthCeiling);
        Assert.Equal(hit ? 1 : 0, sim.Actors.Count(actor => actor.ThingId == 19));
    }

    [Theory]
    [InlineData(16384, 0)]
    [InlineData(-16384, 128)]
    public void PlaneAtTraceOriginIsExcluded(int pitch, int height)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var player = CenterOrigin(sim);
        player.Z = Fixed.FromInt(height - 28);
        Assert.False(CombatTrace.TraceLineAttack(sim, player, new BamAngle(0),
            new BamAngle(unchecked((uint)(pitch << 16))), 256).Hit);
    }
    // These geometry fixtures intentionally trace from the actor center.
    private static PlayerPawn CenterOrigin(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); player.AttackZOffset = new Fixed(0); return player;
    }
}
