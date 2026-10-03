using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingChangeTidTests
{
    [Theory]
    [InlineData(MapDataFormat.HexenBinary)]
    [InlineData(MapDataFormat.UdmfText)]
    public void MapRetagsEveryMatchAndLaterActionsUseNewTid(MapDataFormat format)
    {
        var sim = Room(format); var player = Assert.Single(sim.Players);
        var targets = sim.Actors.Where(a => a.ThingId == 7).ToArray();
        var line = new LevelLine { Special = 176, Arg0 = 7, Arg1 = 9, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true)); Assert.Equal(0, line.Special);
        Assert.All(targets, a => Assert.Equal(9, a.ThingId)); Assert.Equal(0, player.ThingId);
        Assert.False(ThingActivation.Execute(sim, null, 7, false));
        Assert.True(ThingActivation.Execute(sim, null, 9, false));
        Assert.All(targets, a => Assert.True(a.Dormant));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsZeroTidChangesOnlyActivatorAndScriptContinues(bool stack)
    {
        var sim = Room(MapDataFormat.HexenBinary); var player = Assert.Single(sim.Players);
        int[] call = stack ? [3, 0, 3, -5, 5, 176] : [10, 176, 0, -5];
        int[] words = [.. call, 11, 185, 0, 90, 0, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], player)); sim.Acs.Tick(sim);
        Assert.Equal(-5, player.ThingId);
        Assert.All(sim.Actors.Where(a => a != player), a => Assert.Equal(7, a.ThingId));
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
    }

    [Theory]
    [InlineData(0, 9)]
    [InlineData(99, 9)]
    [InlineData(7, 0)]
    public void MissingTargetsSucceedAndZeroNewTidClearsMembership(int oldTid, int newTid)
    {
        var sim = Room(MapDataFormat.HexenBinary);
        Assert.True(ThingChangeTid.ExecuteSpecial(sim, 176, null, oldTid, newTid));
        Assert.All(sim.Actors.Where(a => a is not PlayerPawn), a => Assert.Equal(oldTid == 7 ? 0 : 7, a.ThingId));
    }

    [Fact]
    public void RetaggingChangesSimulationChecksum()
    {
        var first = Room(MapDataFormat.HexenBinary); var second = Room(MapDataFormat.HexenBinary);
        ThingChangeTid.ExecuteSpecial(first, 176, null, 7, 9);
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(MapDataFormat format) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = format, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, Id = 7, X = 100 },
            new LevelThing { Type = 3004, Id = 7, X = 200 }],
    });
}