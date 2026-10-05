using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ResurrectionDefaultsChecksumTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DifferentRaiseDefaultsAffectChecksumWithIdenticalCurrentDimensions(bool height)
    {
        var normal = Room(); var changed = Room();
        if (height) changed.Actors[1].ResurrectionHeight = Fixed.FromInt(80);
        else changed.Actors[1].ResurrectionRadius = Fixed.FromInt(40);
        normal.Tick(); changed.Tick();
        Assert.Equal(normal.Actors[1].Height, changed.Actors[1].Height);
        Assert.Equal(normal.Actors[1].Radius, changed.Actors[1].Radius);
        Assert.NotEqual(normal.Checksum, changed.Checksum);
    }

    [Fact]
    public void PoseRestoreRetainsOriginalDefaultsForFutureRaise()
    {
        var sim = Room(); var corpse = sim.Actors[1];
        var radius = corpse.Radius; var height = corpse.Height;
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Height = Fixed.FromInt(1); corpse.Radius = Fixed.FromInt(1);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        sim.RestoreState(state);
        Assert.Equal(Fixed.FromInt(1), corpse.Height);
        Assert.Equal(height, corpse.ResurrectionHeight);
        Assert.Equal(radius, corpse.ResurrectionRadius);
        Assert.True(ThingRaise.Execute(sim, 17, corpse, 0, 2));
        Assert.Equal(height, corpse.Height);
        Assert.Equal(radius, corpse.Radius);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 100 }],
    });
}
