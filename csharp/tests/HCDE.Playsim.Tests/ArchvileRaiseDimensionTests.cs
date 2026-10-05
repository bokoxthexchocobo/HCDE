using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileRaiseDimensionTests
{
    [Theory]
    [InlineData(10, false)]
    [InlineData(128, true)]
    public void ArchvileChecksOriginalHeightAndRestoresDimensionsOnSuccess(int ceiling, bool raised)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1]; var height = corpse.Height; var radius = corpse.Radius;
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Height = Fixed.FromInt(1); corpse.Radius = Fixed.FromInt(1); corpse.Solid = false;
        sim.Ceilings[0] = ceiling;
        var archvile = new Actor { X = Fixed.FromInt(-5), Radius = Fixed.FromInt(20) };
        Assert.Equal(raised, ArchvileActions.TryRaise(sim, archvile, sim.Players.Single()));
        Assert.Equal(raised ? height : Fixed.FromInt(1), corpse.Height);
        Assert.Equal(raised ? radius : Fixed.FromInt(1), corpse.Radius);
        Assert.Equal(raised, corpse.Solid);
    }
}
