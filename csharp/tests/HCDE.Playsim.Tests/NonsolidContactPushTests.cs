using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NonsolidContactPushTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 6)]
    [InlineData(2, 6)]
    [InlineData(3, 6)]
    [InlineData(4, 6)]
    public void NonsolidMoverPushesOnlyNativeContactTargets(int kind, double expected)
    {
        var (sim, mover, target) = Room();
        target.Solid = kind == 1; target.Shootable = kind == 2;
        target.SpecialPickup = kind == 3; target.Touchy = kind == 4;
        ActorPhysics.Step(sim, mover);
        Assert.Equal(expected, target.VelocityX.ToDouble());
        Assert.Equal(24, mover.X.ToDouble());
        Assert.Equal(kind != 0, target.LastPushFromMovement);
        Assert.Equal(kind == 0 ? 0 : 1, sim.PendingSoundRequests.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NonsolidMoverRetainsPushOptOuts(int mode)
    {
        var (sim, mover, target) = Room(); target.Solid = true;
        if (mode == 0) mover.CannotPush = true;
        if (mode == 1) mover.ThruActors = true;
        if (mode == 2) target.NoBlockmap = true;
        ActorPhysics.Step(sim, mover);
        Assert.Equal(0, target.VelocityX.Raw); Assert.Empty(sim.PendingSoundRequests);
        Assert.Equal(24, mover.X.ToDouble());
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3004, X = 40 }],
        });
        var mover = sim.Actors[0]; var target = sim.Actors[1]; mover.Brain = target.Brain = null;
        mover.Solid = false; mover.VelocityX = Fixed.FromInt(24);
        target.Pushable = true;
        target.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Push] = "push/test" };
        return (sim, mover, target);
    }
}
