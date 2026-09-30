using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsPlayerInventoryTests
{
    [Fact]
    public void PlayerFrags_ReportsActivatorFragCount()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.FragCount = 12;
        Run(sim, player,
            (int)AcsPcode.PlayerFrags,
            (int)AcsPcode.PushNumber, 12,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void PlayerBlueCard_IsSetWhenTheActivatorHasTheKey()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.BlueKey = true;
        Run(sim, player,
            (int)AcsPcode.PlayerBlueCard,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void GiveActorInventory_TargetsEveryActorWithTid()
    {
        var sim = TwoPlayerRoom();
        var target = sim.Players.Last();
        Assert.Equal(9, target.ThingId);
        target.Inventory.Bullets = 10;
        var activator = sim.Players.First();
        Assert.Equal(2, sim.Players.Count());
        RunProgram(sim, activator, ["Clip"],
            (int)AcsPcode.PushNumber, 9,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 15,
            (int)AcsPcode.GiveActorInventory);
        Assert.Equal(25, target.Inventory.Bullets);
        Assert.Equal(50, activator.Inventory.Bullets);
    }

    [Fact]
    public void CheckActorInventory_ReadsTidTargetAmmo()
    {
        var sim = TwoPlayerRoom();
        var target = sim.Players.Last();
        target.Inventory.Bullets = 77;
        var activator = sim.Players.First();
        RunProgram(sim, activator, ["Clip"],
            (int)AcsPcode.PushNumber, 9,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CheckActorInventory,
            (int)AcsPcode.PushNumber, 77,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void ClearInventory_ResetsActivatorToPistolStart()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.Bullets = 200;
        player.Inventory.BlueKey = true;
        RunProgram(sim, player, ["Clip"],
            (int)AcsPcode.ClearInventory,
            (int)AcsPcode.CheckInventoryDirect, 0,
            (int)AcsPcode.PushNumber, 50,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 32,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
        Assert.False(player.Inventory.BlueKey);
    }

    [Fact]
    public void GiveInventoryDirect_AddsAmmo()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.Bullets = 10;
        RunProgram(sim, player, ["Clip"],
            (int)AcsPcode.GiveInventoryDirect, 0, 25,
            (int)AcsPcode.CheckInventoryDirect, 0,
            (int)AcsPcode.PushNumber, 35,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(35, player.Inventory.Bullets);
    }

    [Fact]
    public void TakeInventory_RemovesAmmoFromActivator()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.Bullets = 50;
        RunProgram(sim, player, ["Clip"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 20,
            (int)AcsPcode.TakeInventory,
            (int)AcsPcode.CheckInventoryDirect, 0,
            (int)AcsPcode.PushNumber, 30,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(30, player.Inventory.Bullets);
    }

    [Fact]
    public void CheckInventoryDirect_ReadsAmmoFromStringTable()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.Bullets = 77;
        RunProgram(sim, player, ["Clip"],
            (int)AcsPcode.CheckInventoryDirect, 0,
            (int)AcsPcode.PushNumber, 77,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 28,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    private static void RunProgram(AuthoritySimulation sim, Actor activator, string[] strings, params int[] words)
    {
        sim.Acs.Add(new AcsProgram { Number = 1, StringTable = strings, Code = Words(words) });
        Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, activator));
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void GetAmmoCapacity_ReturnsPistolStartClipMax()
    {
        var sim = Room();
        var player = sim.Players.Single();
        RunProgram(sim, player, ["Clip"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.GetAmmoCapacity,
            (int)AcsPcode.PushNumber, 200,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SetAmmoCapacity_UpdatesClipMax()
    {
        var sim = Room();
        var player = sim.Players.Single();
        RunProgram(sim, player, ["Clip"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 350,
            (int)AcsPcode.SetAmmoCapacity);
        Assert.Equal(350, player.Inventory.MaxBullets);
    }

    [Fact]
    public void UseActorInventory_TidZeroUsesEveryPlayer()
    {
        var sim = TwoPlayerRoom();
        foreach (var player in sim.Players)
        {
            player.Inventory.Weapons |= WeaponKind.Shotgun;
            player.Inventory.Shells = 8;
            player.Inventory.Selected = WeaponKind.Pistol;
            player.Inventory.Pending = null;
        }

        Assert.Equal(2, sim.Players.Count());
        RunProgram(sim, sim.Players.First(), ["Shotgun"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.UseActorInventory);
        foreach (var player in sim.Players)
            Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
    }

    [Fact]
    public void UseActorInventory_SelectsWeaponOnTidTarget()
    {
        var sim = TwoPlayerRoom();
        var target = sim.Players.Last();
        target.Inventory.Weapons |= WeaponKind.Chaingun;
        target.Inventory.Bullets = 20;
        var activator = sim.Players.First();
        RunProgram(sim, activator, ["Chaingun"],
            (int)AcsPcode.PushNumber, 9,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.UseActorInventory);
        Assert.Equal(WeaponKind.Chaingun, target.Inventory.Pending);
        Assert.Null(activator.Inventory.Pending);
    }

    [Fact]
    public void UseInventory_SelectsOwnedWeapon()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = 4;
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        RunProgram(sim, player, ["Shotgun"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.UseInventory);
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
    }

    [Fact]
    public void UseInventory_ReturnsZeroWhenWeaponNotOwned()
    {
        var sim = Room();
        var player = sim.Players.Single();
        RunProgram(sim, player, ["BFG"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.UseInventory,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 32,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
        Assert.Null(player.Inventory.Pending);
    }

    private static void Run(AuthoritySimulation sim, Actor activator, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, activator));
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static byte[] Words(params int[] values)
    {
        var bytes = new byte[values.Length * 4];
        for (var i = 0; i < values.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), values[i]);
        return bytes;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
    });

    private static AuthoritySimulation TwoPlayerRoom() => AuthoritySimulation.Start(
        new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
            Things =
            [
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = 2, Id = 9, X = 96, Y = 64, Single = false, Coop = true },
            ],
        },
        spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
