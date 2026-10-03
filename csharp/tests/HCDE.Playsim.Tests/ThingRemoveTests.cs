using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingRemoveTests
{
    [Theory]
    [InlineData(MapDataFormat.HexenBinary)]
    [InlineData(MapDataFormat.UdmfText)]
    public void MapRemovesAllMatchesAndProtectsPlayer(MapDataFormat format)
    {
        var sim = Room(format); var player = Assert.Single(sim.Players); player.ThingId = 7;
        var targets = sim.Actors.Where(a => a is not PlayerPawn).ToArray();
        var line = new LevelLine { Special = 132, Arg0 = 7, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true)); Assert.Equal(0, line.Special);
        Assert.All(targets, a => Assert.True(a.Destroyed)); Assert.False(player.Destroyed);
        player.ThingId = 0;
        Assert.False(ThingActivation.Execute(sim, null, 7, false));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsZeroTidRemovesActivatorAndContinuesWithWorldActions(bool stack)
    {
        var sim = Room(MapDataFormat.HexenBinary); var actor = sim.Actors.First(a => a.ThingId == 7);
        Run(sim, stack, 0, actor);
        Assert.True(actor.Destroyed);
        Assert.False(sim.Actors.Last(a => a.ThingId == 7).Destroyed);
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingTargetsSucceedAndDoNotStopScript(bool stack)
    {
        var sim = Room(MapDataFormat.HexenBinary); Run(sim, stack, 99, null);
        Assert.All(sim.Actors, a => Assert.False(a.Destroyed));
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
        Assert.True(ThingRemove.ExecuteSpecial(sim, 132, null, 0));
    }

    [Fact]
    public void ZeroTidPlayerIsProtected()
    {
        var sim = Room(MapDataFormat.HexenBinary); var player = Assert.Single(sim.Players);
        Assert.True(ThingRemove.ExecuteSpecial(sim, 132, player, 0));
        Assert.False(player.Destroyed); Assert.False(player.IsDead);
    }
    private static void Run(AuthoritySimulation sim, bool stack, int tid, Actor? activator)
    {
        int[] call = stack ? [3, tid, 4, 132] : [9, 132, tid];
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