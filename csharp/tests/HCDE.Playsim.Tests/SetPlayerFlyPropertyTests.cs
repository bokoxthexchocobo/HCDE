using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SetPlayerFlyPropertyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    public void LocalActionChangesAllFlightFlagsButReturnsFalse(int value)
    {
        var sim = Room(); var player = sim.Players.First();
        player.Fly = player.NoGravity = player.FlyCheat = value == 0;
        Assert.False(ActorSpecialActions.CallSpecial(player, 191, 0, value, 3));
        Assert.Equal(value != 0, player.Fly); Assert.Equal(value != 0, player.NoGravity);
        Assert.Equal(value != 0, player.FlyCheat); Assert.False(sim.Players.Last().Fly);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AllPlayerActionPreservesFlyCheatMarkers(bool marker)
    {
        var sim = Room(); foreach (var p in sim.Players) p.FlyCheat = marker;
        Assert.False(PlayerPropertyActions.ExecuteSpecial(sim, 191, null, -1, 1, 3));
        Assert.All(sim.Players, p => { Assert.True(p.Fly); Assert.True(p.NoGravity); Assert.Equal(marker, p.FlyCheat); });
        Assert.False(PlayerPropertyActions.ExecuteSpecial(sim, 191, null, 1, 0, 3));
        Assert.All(sim.Players, p => { Assert.False(p.Fly); Assert.False(p.NoGravity); Assert.Equal(marker, p.FlyCheat); });
    }

    [Theory]
    [InlineData(MapDataFormat.HexenBinary)]
    [InlineData(MapDataFormat.UdmfText)]
    public void FalseResultLeavesMapLineSpecialAvailable(MapDataFormat format)
    {
        var sim = Room(format); var player = sim.Players.First();
        var line = new LevelLine { Special = 191, Arg1 = 1, Arg2 = 3, PlayerUse = true };
        Assert.False(LineSpecials.ActivateMapLine(sim, player, line, true));
        Assert.True(player.Fly); Assert.True(player.FlyCheat); Assert.Equal(191, line.Special);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsFormsContinueAfterMutatingFalseResult(bool stack)
    {
        var sim = Room(); var player = sim.Players.First();
        int[] call = stack ? [3, 0, 3, 1, 3, 3, 6, 191] : [11, 191, 0, 1, 3];
        int[] words = [.. call, 11, 185, 0, 90, 0, 1]; var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], player)); sim.Acs.Tick(sim);
        Assert.True(player.FlyCheat); Assert.True(player.NoGravity);
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
    }

    [Fact]
    public void SavedFlightFlagsRoundTripAndOlderSaveClearsMarker()
    {
        var sim = Room(); var player = sim.Players.First(); var older = SimSavegame.Write(sim, true);
        ActorSpecialActions.CallSpecial(player, 191, 0, 1, 3); var saved = SimSavegame.Write(sim, true);
        ActorSpecialActions.CallSpecial(player, 191, 0, 0, 3); SimSavegame.Apply(sim, saved);
        Assert.True(player.FlyCheat); Assert.True(player.Fly); Assert.True(player.NoGravity);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, older); Assert.False(player.FlyCheat);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidMarkerRejectsBeforeRestore(int value)
    {
        var sim = Room(); var state = sim.CaptureState(); state.Actors[0].FlyCheatFlag = value;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.All(sim.Players, p => Assert.False(p.FlyCheat));
    }

    [Fact]
    public void FlightMarkerRequiresGeometry()
    {
        var sim = Room(); var state = sim.CaptureState(); state.GeometryHealth = null;
        state.Actors[0].FlyCheatFlag = 1;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    private static AuthoritySimulation Room(MapDataFormat format = MapDataFormat.HexenBinary) =>
        AuthoritySimulation.Start(new PlayLevel
        { Format = format, Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 128 }] },
            spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
