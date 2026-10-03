using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingThrustZTests
{
    [Theory]
    [InlineData(0, 0, 2.25)]
    [InlineData(1, 0, -2.25)]
    [InlineData(0, 1, 5.25)]
    [InlineData(-1, -1, 0.75)]
    public void MapScalesReversesAndSetsOrAddsForAllMatches(int down, int add, double expected)
    {
        var sim = Room(); var player = Assert.Single(sim.Players);
        foreach (var actor in sim.Actors) actor.VelocityZ = Fixed.FromInt(3);
        var line = new LevelLine { Special = 128, Arg0 = 7, Arg1 = 9, Arg2 = down, Arg3 = add, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true)); Assert.Equal(0, line.Special);
        Assert.All(sim.Actors.Where(a => a.ThingId == 7), a => Assert.Equal(expected, a.VelocityZ.ToDouble()));
        Assert.Equal(3, player.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsZeroTidSelectsActivatorAndSupportsSignedThrust(bool stack)
    {
        var sim = Room(); var player = Assert.Single(sim.Players);
        int[] words = stack ? [3, 0, 3, -9, 3, 0, 3, 0, 7, 128, 1] : [12, 128, 0, -9, 0, 0, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], player)); sim.Acs.Tick(sim);
        Assert.Equal(-2.25, player.VelocityZ.ToDouble());
        Assert.All(sim.Actors.Where(a => a.ThingId == 7), a => Assert.Equal(0, a.VelocityZ.Raw));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(99, true)]
    public void MissingTargetReturnDistinguishesZeroFromNonzeroTid(int tid, bool expected)
    {
        var sim = Room();
        Assert.Equal(expected, ThingThrustZ.ExecuteSpecial(sim, 128, null, tid, 9, 0, 0));
        Assert.All(sim.Actors, a => Assert.Equal(0, a.VelocityZ.Raw));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, Id = 7, X = 100 },
            new LevelThing { Type = 3004, Id = 7, X = 200 }],
    });
}