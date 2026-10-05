using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileRaiseSightTests
{
    [Fact]
    public void FlatWorldRaiseDoesNotRequireSightWhenCorpseFits()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Lines = [new LevelLine { X1 = 0, Y1 = -100, X2 = 0, Y2 = 100,
                SideFront = 0, SideBack = -1, Flags = 1 }],
            Sides = [new LevelSide { Sector = 0 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 21 }],
        });
        var corpse = sim.Actors[1]; corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        var archvile = new Actor { X = Fixed.FromInt(-21), MovementSpeed = Fixed.FromInt(15) };
        Assert.False(CombatTrace.HasLineOfSight(sim, archvile, corpse));
        Assert.True(ActorRaise.CanRaise(sim, corpse));
        Assert.True(ArchvileActions.TryRaise(sim, archvile, sim.Players.Single()));
        Assert.False(corpse.IsDead);
    }
}
