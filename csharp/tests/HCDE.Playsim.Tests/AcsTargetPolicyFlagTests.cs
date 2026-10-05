using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsTargetPolicyFlagTests
{
    [Theory]
    [InlineData("nevertarget")]
    [InlineData("notargetswitch")]
    [InlineData("quicktoretaliate")]
    [InlineData("nohateplayers")]
    public void FlagsSupportCaseInsensitiveSetQueryAndClear(string name)
    {
        var actor = new Actor();
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name.ToUpperInvariant(), out var enabled));
        Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, name.ToUpperInvariant(), false));
        Assert.True(AcsActorFlags.TryGet(actor, name, out enabled));
        Assert.False(enabled);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScriptNoHatePlayersControlsDamageRetaliation(bool excluded)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var monster = sim.Actors[1];
        Assert.True(AcsActorFlags.TrySet(monster, "NOHATEPLAYERS", excluded));
        Assert.Equal(1, ActorDamage.Apply(monster, 1, sim.Players.Single()).HealthLost);
        Assert.Equal(excluded ? null : (uint?)sim.Players.Single().Id, monster.Brain!.TargetId);
    }
}
