using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SetPlayerNoTargetPropertyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-4)]
    public void ActivatorOnlySetsCheatWithoutChangingActorFlag(int value)
    {
        var sim = Room(); var players = sim.Players.ToArray(); players[0].NoTarget = true;
        Assert.True(PlayerPropertyActions.ExecuteSpecial(sim, 191, players[0], 0, value, 1));
        Assert.Equal(value != 0, players[0].NoTargetCheat); Assert.True(players[0].NoTarget);
        Assert.Null(players[1].NoTargetCheatOverride);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(99)]
    public void AnyNonzeroWhoSetsAllPlayersEvenWithoutActivator(int who)
    {
        var sim = Room(); sim.Players.Last().Health = 0;
        Assert.True(PlayerPropertyActions.ExecuteSpecial(sim, 191, null, who, 1, 1));
        Assert.All(sim.Players, p => Assert.True(p.NoTargetCheat));
        Assert.True(PlayerPropertyActions.ExecuteSpecial(sim, 191, null, who, 0, 1));
        Assert.All(sim.Players, p => Assert.False(p.NoTargetCheat));
    }

    [Fact]
    public void MissingOrNonplayerActivatorRejectsLocalAction()
    {
        var sim = Room();
        Assert.False(PlayerPropertyActions.ExecuteSpecial(sim, 191, null, 0, 1, 1));
        Assert.False(PlayerPropertyActions.ExecuteSpecial(sim, 191, sim.AddBot(64, 0), 0, 1, 1));
        Assert.All(sim.Players, p => Assert.Null(p.NoTargetCheatOverride));
        Assert.Null(PlayerPropertyActions.ExecuteSpecial(sim, 19, null, 0, 1, 1));
        Assert.False(PlayerPropertyActions.ExecuteSpecial(sim, 191, sim.Players.First(), 0, 1, 999));
    }

    [Theory]
    [InlineData(MapDataFormat.HexenBinary)]
    [InlineData(MapDataFormat.UdmfText)]
    public void MapLineDispatchesAndConsumesSuccessfulAction(MapDataFormat format)
    {
        var sim = Room(format); var player = sim.Players.First();
        var line = new LevelLine { Special = 191, Arg0 = 0, Arg1 = 1, Arg2 = 1, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true));
        Assert.True(player.NoTargetCheat); Assert.False(player.NoTarget); Assert.Equal(0, line.Special);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsImmediateAndStackFormsSetPropertyAndContinue(bool stack)
    {
        var sim = Room(); var player = sim.Players.First();
        int[] call = stack ? [3, 0, 3, 1, 3, 1, 6, 191] : [11, 191, 0, 1, 1];
        int[] words = [.. call, 11, 185, 0, 90, 0, 1]; var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], player)); sim.Acs.Tick(sim);
        Assert.True(player.NoTargetCheat); Assert.False(player.NoTarget);
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
    }

    [Fact]
    public void ActorCallSpecialUsesPropertyDispatchAndSavedCheatSurvives()
    {
        var sim = Room(); var player = sim.Players.First();
        Assert.True(ActorSpecialActions.CallSpecial(player, 191, 0, 1, 1));
        var saved = SimSavegame.Write(sim, true); player.NoTargetCheat = false;
        SimSavegame.Apply(sim, saved); Assert.True(player.NoTargetCheat);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room(MapDataFormat format = MapDataFormat.HexenBinary) =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Format = format, Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 128 }],
        }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
