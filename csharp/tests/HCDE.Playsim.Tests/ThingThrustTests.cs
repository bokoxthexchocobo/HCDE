using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingThrustTests
{
    [Theory]
    [InlineData(0, 0, 30, 3)]
    [InlineData(0, 1, 42, 3)]
    [InlineData(128, 0, -30, 3)]
    [InlineData(320, 1, 2, 43)]
    public void MapAddsImpulseAndClampsComponentsUnlessUnlimited(int angle, int noLimit, double x, double y)
    {
        var sim = Room(); var player = Assert.Single(sim.Players);
        foreach (var actor in sim.Actors)
        { actor.VelocityX = Fixed.FromInt(2); actor.VelocityY = Fixed.FromInt(3); actor.VelocityZ = Fixed.FromInt(5); }
        var line = new LevelLine { Special = 72, Arg0 = angle, Arg1 = 40, Arg2 = noLimit, Arg3 = 7, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true)); Assert.Equal(0, line.Special);
        Assert.All(sim.Actors.Where(a => a.ThingId == 7), a =>
        {
            Assert.Equal(x, a.VelocityX.ToDouble(), 4); Assert.Equal(y, a.VelocityY.ToDouble(), 4);
            Assert.Equal(5, a.VelocityZ.ToDouble());
        });
        Assert.Equal(2, player.VelocityX.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsZeroTidSelectsActivatorWithSignedForce(bool stack)
    {
        var sim = Room(); var player = Assert.Single(sim.Players);
        int[] words = stack ? [3, 0, 3, -7, 3, 1, 3, 0, 7, 72, 1] : [12, 72, 0, -7, 1, 0, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], player)); sim.Acs.Tick(sim);
        Assert.Equal(-7, player.VelocityX.ToDouble());
        Assert.All(sim.Actors.Where(a => a.ThingId == 7), a => Assert.Equal(0, a.VelocityX.Raw));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(99, true)]
    public void MissingTargetReturnsNativeResult(int tid, bool expected)
    {
        Assert.Equal(expected, ThingThrust.ExecuteSpecial(Room(), 72, null, 0, 7, 0, tid));
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, Id = 7, X = 100 },
            new LevelThing { Type = 3004, Id = 7, X = 200 }],
    });
}