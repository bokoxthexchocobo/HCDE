using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorLinkMoverTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CeilingObstructionUsesSectorMembership(bool unlinked)
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorPropertyActions.ChangeLinkFlags(actor, sector: unlinked ? 1 : 0);
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Ceiling,
            TargetCeiling = 0, CeilingDirection = -1, Speed = 16, CrushDamage = -1 });
        LineSpecials.TickMotions(sim);
        Assert.Equal(unlinked ? 48 : 64, sim.CeilingOf(0));
        Assert.Equal(100, actor.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CrusherDamageUsesSectorMembership(bool unlinked)
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorPropertyActions.ChangeLinkFlags(actor, sector: unlinked ? 1 : 0);
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Ceiling,
            TargetCeiling = 0, CeilingDirection = -1, Speed = 16, CrushDamage = 10 });
        LineSpecials.TickMotions(sim);
        Assert.Equal(48, sim.CeilingOf(0));
        Assert.Equal(unlinked ? 100 : 90, actor.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FloorObstructionUsesSectorMembership(bool unlinked)
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorPropertyActions.ChangeLinkFlags(actor, sector: unlinked ? 1 : 0);
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Floor,
            TargetFloor = 32, FloorDirection = 1, Speed = 16 });
        LineSpecials.TickMotions(sim);
        Assert.Equal(unlinked ? 16 : 0, sim.FloorOf(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClosingDoorReopensOnlyForSectorMembers(bool unlinked)
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorPropertyActions.ChangeLinkFlags(actor, sector: unlinked ? 1 : 0);
        var motion = new SectorMotion { SectorIndex = 0, Kind = MotionKind.Door,
            ClosedCeiling = 0, TargetCeiling = 64, Closing = true, Speed = 16 };
        sim.Motions.Add(motion); LineSpecials.TickMotions(sim);
        Assert.Equal(unlinked ? 48 : 64, sim.CeilingOf(0));
        Assert.Equal(unlinked, motion.Closing);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StationarySectorPinchUsesSectorMembership(bool unlinked)
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorPropertyActions.ChangeLinkFlags(actor, sector: unlinked ? 1 : 0);
        for (var tick = 0; tick < 4; tick++) sim.Tick();
        sim.Ceilings[0] = 32;
        ActorPhysics.FitToSector(sim, actor, sectorPinchCrush: true);
        Assert.Equal(unlinked ? 100 : 90, actor.Health);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 64 }], Things = [new LevelThing { Type = 1 }] });
}
