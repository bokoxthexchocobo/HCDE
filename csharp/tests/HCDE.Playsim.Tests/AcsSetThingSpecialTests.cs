using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSetThingSpecialTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(7, true)]
    [InlineData(99, true)]
    [InlineData(0, false)]
    public void NumericOpcodeSetsAllArgumentsAndContinues(int tid, bool activator)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.VelocityX = Fixed.FromInt(8);
        Run(sim, activator ? player : null, [3, tid, 3, 19, 3, 11, 3, -12, 3, 13, 3, 14, 3, 15, 180, 9, 19, 0, 1]);
        foreach (var actor in sim.Actors)
        {
            var selected = tid == 0 ? activator && actor == player : actor.ThingId == tid;
            Assert.Equal(selected ? 19 : 80, actor.Special);
            Assert.Equal(selected ? new[] { 11, -12, 13, 14, 15 } : new[] { 1, 2, 3, 4, 5 }, actor.SpecialArgs);
        }
        Assert.Equal(activator ? 0 : 8, player.VelocityX.ToDouble());
    }

    [Fact]
    public void ShortStackDoesNotPartiallyMutateOrContinue()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.VelocityX = Fixed.FromInt(8);
        Run(sim, player, [3, 0, 3, 19, 180, 9, 19, 0, 1]);
        Assert.Equal(80, player.Special);
        Assert.Equal(8, player.VelocityX.ToDouble());
    }

    private static void Run(AuthoritySimulation sim, Actor? activator, int[] words)
    {
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.TryExecute(1, [], activator)); sim.Acs.Tick(sim);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500, Special = 80, Args = [1, 2, 3, 4, 5] },
            new LevelThing { Type = 3004, Id = 7, X = 20, Special = 80, Args = [1, 2, 3, 4, 5] },
            new LevelThing { Type = 3004, Id = 7, X = 200, Special = 80, Args = [1, 2, 3, 4, 5] }],
    });
}
