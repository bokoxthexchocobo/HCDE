using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsThingActivationTests
{
    [Theory]
    [InlineData(false, 130)]
    [InlineData(true, 130)]
    [InlineData(false, 131)]
    [InlineData(true, 131)]
    public void DirectAndStackCallsSelectTidAndContinue(bool stack, int special)
    {
        var sim = Room(); var target = sim.Actors.Single(a => a.ThingId == 7);
        var other = sim.Actors.Single(a => a.ThingId == 8);
        target.Dormant = other.Dormant = special == 130;
        Run(sim, stack, special, 7, null);
        Assert.Equal(special == 131, target.Dormant);
        Assert.Equal(special == 130, other.Dormant);
        Assert.Equal(special == 130 ? 1 : -1, target.States.RemainingTics);
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ZeroTidSelectsActivator(bool stack)
    {
        var sim = Room(); var target = sim.Actors.Single(a => a.ThingId == 7);
        Run(sim, stack, 131, 0, target);
        Assert.True(target.Dormant);
        Assert.False(sim.Actors.Single(a => a.ThingId == 8).Dormant);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingTargetDoesNotStopScript(bool stack)
    {
        var sim = Room(); Run(sim, stack, 131, 99, null);
        Assert.All(sim.Actors, a => Assert.False(a.Dormant));
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
    }

    private static void Run(AuthoritySimulation sim, bool stack, int special, int tid, Actor? activator)
    {
        // Deliberately use two arguments: only arg0 is meaningful to native activation.
        int[] call = stack ? [3, tid, 3, 99, 5, special] : [10, special, tid, 99];
        int[] words = [.. call, 11, 185, 0, 90, 0, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], activator)); sim.Acs.Tick(sim);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, Id = 7 }, new LevelThing { Type = 3004, Id = 8, X = 100 }],
    });
}