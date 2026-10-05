using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseSplashDefaultsTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SuccessfulRaiseRestoresSplashVulnerability(bool archvile, bool blocked)
    {
        var sim = Room();
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.NoRadiusDamage = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.NoRadiusDamage);
        if (!blocked)
        {
            ArchvileActions.Attack(sim, new Actor { X = Fixed.FromInt(-1000) },
                new Actor { X = Fixed.FromInt(44) }, fireExists: true);
            Assert.True(corpse.IsDead);
        }
    }

    [Fact]
    public void SplashRevivalDefaultsAffectChecksum()
    {
        var first = Room(); var second = Room();
        second.Actors[1].ResurrectionDefenseFlags = 4;
        first.Tick(); second.Tick();
        Assert.Equal(first.Actors[1].NoRadiusDamage, second.Actors[1].NoRadiusDamage);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    });
}
