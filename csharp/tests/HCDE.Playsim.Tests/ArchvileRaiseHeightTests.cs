using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileRaiseHeightTests
{
    [Theory]
    [InlineData(80, 0, true)]
    [InlineData(0, 80, true)]
    [InlineData(-80, 0, false)]
    [InlineData(240, 0, false)]
    public void FlatWorldRaiseUsesPositionClearanceInsteadOfVerticalDistance(
        int corpseZ, int archvileZ, bool expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Z = Fixed.FromInt(corpseZ);
        var archvile = new Actor { Z = Fixed.FromInt(archvileZ) };

        Assert.Equal(expected, ArchvileActions.TryRaise(sim, archvile, sim.Players.Single()));
        Assert.Equal(!expected, corpse.IsDead);
    }
}
