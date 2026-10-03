using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingStopTests
{
    [Theory]
    [InlineData(MapDataFormat.HexenBinary)]
    [InlineData(MapDataFormat.UdmfText)]
    public void MapStopsAllMatchingActorsWithoutChangingOthers(MapDataFormat format)
    {
        var sim = Room(format); SetVelocities(sim);
        var line = new LevelLine { Special = 19, Arg0 = 7, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, Assert.Single(sim.Players), line, true));
        Assert.Equal(0, line.Special);
        Assert.All(sim.Actors.Where(a => a.ThingId == 7), AssertStopped);
        Assert.Equal(3, Assert.Single(sim.Players).VelocityX.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsZeroTidStopsActivatorAndContinues(bool stack)
    {
        var sim = Room(MapDataFormat.HexenBinary); SetVelocities(sim);
        var player = Assert.Single(sim.Players);
        Run(sim, stack, 0, player);
        AssertStopped(player);
        Assert.All(sim.Actors.Where(a => a.ThingId == 7), a => Assert.Equal(3, a.VelocityX.ToDouble()));
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingTargetLeavesMotionAndScriptRunning(bool stack)
    {
        var sim = Room(MapDataFormat.HexenBinary); SetVelocities(sim);
        Run(sim, stack, 99, null);
        Assert.All(sim.Actors, a => Assert.Equal(3, a.VelocityX.ToDouble()));
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
        Assert.False(ThingStop.ExecuteSpecial(sim, 19, null, 0));
    }

    private static void AssertStopped(Actor actor)
    {
        Assert.Equal(0, actor.VelocityX.Raw); Assert.Equal(0, actor.VelocityY.Raw); Assert.Equal(0, actor.VelocityZ.Raw);
        Assert.Equal(7, actor.ReactionTime); Assert.Equal(100, actor.Health);
    }
    private static void SetVelocities(AuthoritySimulation sim)
    {
        foreach (var actor in sim.Actors)
        {
            actor.VelocityX = Fixed.FromInt(3); actor.VelocityY = Fixed.FromInt(-4); actor.VelocityZ = Fixed.FromInt(2);
            actor.ReactionTime = 7; actor.Health = 100;
        }
    }
    private static void Run(AuthoritySimulation sim, bool stack, int tid, Actor? activator)
    {
        int[] call = stack ? [3, tid, 4, 19] : [9, 19, tid];
        int[] words = [.. call, 11, 185, 0, 90, 0, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], activator)); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room(MapDataFormat format) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = format, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, Id = 7, X = 100 },
            new LevelThing { Type = 3004, Id = 7, X = 200 }],
    });
}