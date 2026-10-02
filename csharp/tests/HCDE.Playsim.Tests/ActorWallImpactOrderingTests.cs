using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorWallImpactOrderingTests
{
    [Theory]
    [InlineData(false, -1e-10, false)]
    [InlineData(false, 0, false)]
    [InlineData(false, 1e-10, true)]
    [InlineData(true, -1e-10, false)]
    [InlineData(true, 0, false)]
    [InlineData(true, 1e-10, true)]
    public void ActorStrictlyBeforeWallIsHitWithoutArtificialGap(bool reverse, double wallOffset, bool actorFirst)
    {
        var sign = reverse ? -1 : 1;
        var wallX = sign * (80 + wallOffset);
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = wallX, X2 = wallX, Y1 = sign * 128, Y2 = -sign * 128,
                SideFront = 0, SideBack = -1, Health = 100 }],
            Things = [new LevelThing { Type = 1, Angle = reverse ? 180 : 0 }],
        });
        var source = sim.Players.Single(); var target = sim.AddBot(sign * 100, 0);
        target.Radius = Fixed.FromInt(20); var health = target.Health;
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(0), 256);
        Assert.Equal(actorFirst, ReferenceEquals(target, hit.Victim));
        Assert.Equal(actorFirst ? target : null, CombatTrace.PickActor(sim, source, source.Angle, new BamAngle(0), 256));
        Assert.Equal(actorFirst ? 1 : 0, AcsLineAttack.Attack(sim, source, 0, 0, 7, "None", 256));
        Assert.Equal(actorFirst ? health - 7 : health, target.Health);
        Assert.Equal(actorFirst ? 100 : 93, sim.Level.Lines[0].Health);
    }
}
