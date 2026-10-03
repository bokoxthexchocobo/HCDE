using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActorDimensionTests
{
    [Theory]
    [InlineData(35, true)]
    [InlineData(35, false)]
    [InlineData(36, true)]
    [InlineData(36, false)]
    public void CheckActorPropertyUsesExactFixedDimension(int property, bool matches)
    {
        var sim = Room(); var actor = sim.Players.Single();
        var value = (property == 35 ? actor.Height.Raw : actor.Radius.Raw) + (matches ? 0 : 1);
        Run(sim, actor, [3, 7, 3, 0, 3, property, 3, value, 351, 3, 22,
            3, matches ? 1 : 0, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(35, 12.25)]
    [InlineData(36, 3.5)]
    [InlineData(35, 0)]
    [InlineData(36, -1.25)]
    public void ReadsCurrentFixedDimensions(int property, double dimension)
    {
        var sim = Room(); var player = sim.Players.Single();
        if (property == 35) player.Height = Fixed.FromDouble(dimension);
        else player.Radius = Fixed.FromDouble(dimension);
        Query(sim, player, 0, property, Fixed.FromDouble(dimension).Raw);
    }

    [Theory]
    [InlineData(35)]
    [InlineData(36)]
    public void NativeReadOnlyDimensionsIgnorePropertySetter(int property)
    {
        var sim = Room(); var actor = sim.Players.Single();
        var expected = property == 35 ? actor.Height.Raw : actor.Radius.Raw;
        Run(sim, actor, [3, 0, 3, property, 3, 1, 245, 1]);
        Query(sim, actor, 0, property, expected);
    }

    [Theory]
    [InlineData(35, false)]
    [InlineData(36, false)]
    [InlineData(35, true)]
    [InlineData(36, true)]
    public void MissingOrDestroyedTargetReturnsZero(int property, bool destroyed)
    {
        var sim = Room(); var actor = sim.Players.Single();
        if (destroyed) actor.Destroy();
        Query(sim, actor, destroyed ? 0 : 999, property, 0);
    }

    [Theory]
    [InlineData(35)]
    [InlineData(36)]
    public void TidQueryChoosesNewestMatchingActor(int property)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 }],
            Things = [new LevelThing { Type = 3001, Id = 42 }, new LevelThing { Type = 3001, Id = 42, X = 128 }],
        });
        var newest = sim.Actors.Last(); newest.Height = new Fixed(12345); newest.Radius = new Fixed(23456);
        Query(sim, null, 42, property, property == 35 ? 12345 : 23456);
    }

    private static void Query(AuthoritySimulation sim, Actor? actor, int tid, int property, int expected)
    {
        Run(sim, actor, [3, 7, 3, tid, 3, property, 246, 3, expected, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }
    private static void Run(AuthoritySimulation sim, Actor? actor, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); sim.Acs.TryExecute(1, [], actor); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
