using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseDefaultChecksumTests
{
    [Theory]
    [InlineData(16, false)]
    [InlineData(32, false)]
    [InlineData(64, true)]
    public void DistinctRevivalDefaultsAffectChecksumWithMatchingCurrentFlags(int flag, bool collision)
    {
        var first = Room();
        var second = Room();
        var actor = second.Actors.Single();
        if (collision) actor.ResurrectionCollisionFlags ^= flag;
        else actor.ResurrectionDefenseFlags ^= flag;
        first.Tick(); second.Tick();
        Assert.Equal(first.Actors.Single().Ambush, actor.Ambush);
        Assert.Equal(first.Actors.Single().Boss, actor.Boss);
        Assert.Equal(first.Actors.Single().SpawnCeiling, actor.SpawnCeiling);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004 }],
    });
}
