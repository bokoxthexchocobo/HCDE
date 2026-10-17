using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TouchyCrushTests
{
    [Theory]
    [InlineData(true, true, false, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    public void TouchyCrushRequiresArmedOrSentientAndDisarmsBeforeForcedDeath(bool touchy, bool armed, bool sentient, bool dies)
    {
        var actor = new Actor { Health = 100, Shootable = true, Invulnerable = true,
            Touchy = touchy, Armed = armed, SeeState = sentient ? 0 : -1 };
        Assert.Equal(dies, ActorPhysics.GrindActor(actor));
        Assert.Equal(dies, actor.IsDead);
        Assert.Equal(armed && !dies, actor.Armed);
    }

    [Fact]
    public void SectorPinchKillsImmediatelyOffOrdinaryDamageCadence()
    {
        var sim = Room(); sim.Tick(); var actor = sim.Players.Single();
        actor.Touchy = actor.Armed = actor.Invulnerable = true;
        sim.Ceilings[0] = 32;
        ActorPhysics.FitToSector(sim, actor, sectorPinchCrush: true);
        Assert.True(actor.IsDead); Assert.False(actor.Armed);
    }

    [Theory]
    [InlineData(MotionKind.Ceiling)]
    [InlineData(MotionKind.Door)]
    [InlineData(MotionKind.Floor)]
    public void MoversGrindActorsEvenWithOrdinaryCrushDamageDisabled(MotionKind kind)
    {
        var sim = Room(); sim.Tick(); var actor = sim.Players.Single();
        actor.Touchy = actor.Armed = actor.Invulnerable = true;
        sim.Motions.Add(new SectorMotion
        {
            SectorIndex = 0, Kind = kind, Speed = 16, CrushDamage = -1,
            Closing = kind == MotionKind.Door, FloorDirection = 1, TargetFloor = 32,
            CeilingDirection = -1, TargetCeiling = 32, ClosedCeiling = 0,
        });
        LineSpecials.TickMotions(sim);
        Assert.True(actor.IsDead); Assert.False(actor.Armed);
        Assert.Equal(kind == MotionKind.Floor ? 16 : 48,
            kind == MotionKind.Floor ? sim.FloorOf(0) : sim.CeilingOf(0));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 64 }], Things = [new LevelThing { Type = 1 }],
    });
}
