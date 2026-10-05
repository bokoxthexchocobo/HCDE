using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseDimensionTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(2, true)]
    public void RaiseUsesOriginalDimensionsAndRestoresOnFailedCheck(int flags, bool success)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 100 }],
        });
        var corpse = sim.Actors[1]; var originalHeight = corpse.Height; var originalRadius = corpse.Radius;
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Solid = false;
        corpse.Height = Fixed.FromInt(1); corpse.Radius = Fixed.FromInt(1);
        sim.Ceilings[0] = 10;
        Assert.False(ActorRaise.CanRaise(sim, corpse));
        Assert.Equal(Fixed.FromInt(1), corpse.Height);
        Assert.False(corpse.Solid);
        Assert.Equal(success, ThingRaise.Execute(sim, 17, corpse, 0, flags));
        Assert.Equal(success ? originalHeight : Fixed.FromInt(1), corpse.Height);
        Assert.Equal(success ? originalRadius : Fixed.FromInt(1), corpse.Radius);
    }
}
