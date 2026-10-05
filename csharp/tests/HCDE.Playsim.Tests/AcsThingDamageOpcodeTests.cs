using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsThingDamageOpcodeTests
{
    [Theory]
    [InlineData(7, 0, 1)]
    [InlineData(7, -10, 1)]
    [InlineData(999, 10, 0)]
    [InlineData(0, 10, 1)]
    public void WireOpcodeReturnsShootableActorCount(int tid, int amount, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { Tag = 1, CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, Id = 7, X = 100 },
                new LevelThing { Type = 3004, Id = 7, X = 200 }],
        });
        sim.Actors[^1].Shootable = false;
        int[] words = [3, tid, 3, amount, 3, 0, 335, 3, 1, 217, 5, 112, 1];
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code, StringTable = ["Fire"] });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.Single()));
        sim.Acs.Tick(sim);
        Assert.Equal(expected, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }
}
