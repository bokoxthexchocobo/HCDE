using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingRaiseIntegrationTests
{
    [Theory]
    [InlineData(false, 7, true)]
    [InlineData(true, 7, true)]
    [InlineData(false, 999, false)]
    [InlineData(true, 999, false)]
    public void AcsSpecialRaisesMatchingCorpse(bool stack, int tid, bool expected)
    {
        var sim = Room(); var corpse = Prepare(sim);
        int[] words = stack ? [3, tid, 3, 2, 5, 17, 1] : [10, 17, tid, 2, 1];
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.Single()));
        sim.Acs.Tick(sim);
        Assert.Equal(expected, !corpse.IsDead);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(7, true)]
    [InlineData(999, false)]
    public void MapActionConsumesOnlySuccessfulRaise(int tid, bool expected)
    {
        var sim = Room(); var corpse = Prepare(sim);
        var line = new LevelLine { Special = 17, Arg0 = tid, Arg1 = 2, PlayerUse = true };
        Assert.Equal(expected, LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(expected ? 0 : 17, line.Special);
        Assert.Equal(expected, !corpse.IsDead);
    }

    private static Actor Prepare(AuthoritySimulation sim)
    {
        var corpse = sim.Actors[1];
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        return corpse;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, Id = 7, X = 100 }],
    });
}
