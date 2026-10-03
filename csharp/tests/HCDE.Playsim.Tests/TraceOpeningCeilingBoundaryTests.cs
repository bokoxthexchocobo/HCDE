using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TraceOpeningCeilingBoundaryTests
{
    [Theory]
    [InlineData(false, -1, false)]
    [InlineData(false, 0, false)]
    [InlineData(false, 1, true)]
    [InlineData(true, -1, false)]
    [InlineData(true, 0, false)]
    [InlineData(true, 1, true)]
    public void FarCeilingBoundaryIsInclusiveInBothDirections(bool reverse, int zDelta, bool blocked)
    {
        var sim = Room(reverse, false); var source = CenterOrigin(sim); source.Z = new Fixed(zDelta);
        var target = sim.AddBot(reverse ? -64 : 192, 0); target.Height = Fixed.FromInt(56);
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(0), 256);
        Assert.Equal(blocked, hit.Wall is not null);
        if (blocked) Assert.Null(hit.Victim); else Assert.Same(target, hit.Victim);
        Assert.Equal(blocked ? null : target, CombatTrace.PickActor(sim, source, source.Angle, new BamAngle(0), 256));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExplicitBlockingFlagStillStopsExactBoundaryAndDamagesUpperTier(bool reverse)
    {
        var sim = Room(reverse, true); var source = CenterOrigin(sim);
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(0), 256);
        Assert.Same(sim.Level.Lines[0], hit.Wall);
        GeometryLineAttack.Apply(sim, hit, 7);
        Assert.Equal(93, sim.Level.Sectors[reverse ? 0 : 1].HealthCeiling);
        Assert.Equal(93, sim.Level.Lines[0].Health);
    }

    private static AuthoritySimulation Room(bool reverse, bool blocked) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = reverse ? 28 : 128, HealthCeiling = 100 },
            new LevelSector { CeilingHeight = reverse ? 128 : 28, HealthCeiling = 100 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines = [new LevelLine { X1 = 64, Y1 = 128, X2 = 64, Y2 = -128, SideFront = 0, SideBack = 1,
            Flags = blocked ? LevelLine.BlockHitscanFlag : 0, Health = 100 }],
        Things = [new LevelThing { Type = 1, X = reverse ? 128 : 0, Angle = reverse ? 180 : 0 }],
    });
    // These geometry fixtures intentionally trace from the actor center.
    private static PlayerPawn CenterOrigin(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); player.AttackZOffset = new Fixed(0); return player;
    }
}
