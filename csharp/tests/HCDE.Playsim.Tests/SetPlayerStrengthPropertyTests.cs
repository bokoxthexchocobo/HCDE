using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SetPlayerStrengthPropertyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    public void LocalActionGrantsRefreshesOrRemovesPower(int value)
    {
        var sim = Room(); var player = sim.Players.First();
        player.GivePowerStrength(); player.PowerStrengthTics = 400;
        Assert.True(ActorSpecialActions.CallSpecial(player, 191, 0, value, 6));
        Assert.Equal(value == 0 ? 0 : 1, player.PowerStrengthTics);
        Assert.Equal(value == 0 ? 0 : 1, AcsPlayerInventory.Count(player, "PowerStrength", false));
        Assert.False(sim.Players.Last().HasPowerStrength);
        Assert.False(player.Buddha); Assert.False(player.GodMode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void AllPlayerActionSetsPowerWithoutActivator(int value)
    {
        var sim = Room(); foreach (var p in sim.Players) p.GivePowerStrength();
        Assert.True(PlayerPropertyActions.ExecuteSpecial(sim, 191, null, -1, value, 6));
        Assert.All(sim.Players, p => Assert.Equal(value != 0, p.HasPowerStrength));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsFormsGrantStrengthAndContinue(bool stack)
    {
        var sim = Room(); var player = sim.Players.First();
        int[] call = stack ? [3, 0, 3, 1, 3, 6, 6, 191] : [11, 191, 0, 1, 6];
        int[] words = [.. call, 11, 185, 0, 90, 0, 1]; var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], player)); sim.Acs.Tick(sim);
        Assert.True(player.HasPowerStrength);
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
    }

    [Theory]
    [InlineData(MapDataFormat.HexenBinary)]
    [InlineData(MapDataFormat.UdmfText)]
    public void MapLineGrantsPowerAndConsumesSpecial(MapDataFormat format)
    {
        var sim = Room(format); var player = sim.Players.First();
        var line = new LevelLine { Special = 191, Arg1 = 1, Arg2 = 6, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true));
        Assert.True(player.HasPowerStrength); Assert.Equal(0, line.Special);
    }

    [Fact]
    public void ScriptGrantAmplifiesFistDamageWithoutExtraRandomDraws()
    {
        var (sim, player, target) = CombatRoom(); var (reference, ordinary, ordinaryTarget) = CombatRoom();
        Assert.True(ActorSpecialActions.CallSpecial(player, 191, 0, 1, 6));
        Assert.True(HitscanCombat.Fire(reference, ordinary)); Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal((1000 - ordinaryTarget.Health) * 10, 1000 - target.Health);
        Assert.Equal(reference.CaptureState().PunchRandomState, sim.CaptureState().PunchRandomState);
    }

    [Fact]
    public void SavedScriptPowerContinuesTimerExactly()
    {
        var sim = Room(); var player = sim.Players.First();
        ActorSpecialActions.CallSpecial(player, 191, 0, 1, 6); player.Tick();
        var saved = SimSavegame.Write(sim, true); player.Tick();
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); player.Tick();
        Assert.Equal(expected, SimSavegame.Write(sim, true));
        Assert.True(ActorSpecialActions.CallSpecial(player, 191, 0, 0, 6));
        Assert.Equal(0u, player.GetStrengthBlend());
    }

    private static (AuthoritySimulation, PlayerPawn, Actor) CombatRoom()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single(); player.Inventory.Selected = WeaponKind.Fist;
        var target = sim.AddBot(48, 0, 3001); target.Health = 1000; target.Brain!.Enabled = false;
        return (sim, player, target);
    }

    private static AuthoritySimulation Room(MapDataFormat format = MapDataFormat.HexenBinary) =>
        AuthoritySimulation.Start(new PlayLevel
        { Format = format, Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 128 }] },
            spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
