using HCDE.MapLoader;
using HCDE.Net.Core;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class WeaponSelectionTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });

    private static byte[] Events(params byte[] slots)
    {
        var records = slots.Select(slot => new EventRecord((byte)DemoCommand.WeapSelect, new byte[] { slot })).ToArray();
        var buffer = new byte[2 + 4 * slots.Length];
        Assert.Equal(buffer.Length, EventRecordsCodec.Write(buffer, records));
        return buffer;
    }

    [Fact]
    public void SelectionRunsWithItsQueuedTicAndDoesNotBypassCooldown()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = 5;
        var sink = new SimulationCommandSink(sim);
        Assert.True(sink.ApplyCommand(0, 0, 1, new UserCmd(1, 0, 0, 0, 0, 0, 0), Events(3)));
        Assert.True(sink.ApplyCommand(0, 0, 2, new UserCmd(1, 0, 0, 0, 0, 0, 0), Events(2)));
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        sim.Tick();
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Selected);
        Assert.Equal(4, player.Inventory.Shells);
        Assert.Equal(35, player.WeaponCooldown);
        sim.Tick();
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.Equal(34, player.WeaponCooldown);
    }

    [Fact]
    public void SlotAndNextPreviousCyclingRespectOwnershipAndAmmo()
    {
        var sim = Room(); var player = sim.Players.Single(); var inventory = player.Inventory;
        inventory.Weapons |= WeaponKind.Shotgun | WeaponKind.SuperShotgun | WeaponKind.Chainsaw;
        inventory.Shells = 2;
        var sink = new SimulationCommandSink(sim);
        void Select(params byte[] slots)
        {
            Assert.True(sink.ApplyCommand(0, 0, 1, default, Events(slots)));
            sim.Tick();
        }
        Select(3); Assert.Equal(WeaponKind.SuperShotgun, inventory.Selected);
        Select(3); Assert.Equal(WeaponKind.Shotgun, inventory.Selected);
        Select(3, 3); Assert.Equal(WeaponKind.Shotgun, inventory.Selected); // Preserve every event in a tic.
        inventory.Shells = 1;
        Select(3); Assert.Equal(WeaponKind.Shotgun, inventory.Selected); // SSG needs two shells.
        Select(12); Assert.Equal(WeaponKind.Fist, inventory.Selected); // Wrap past unowned weapons.
        Select(12); Assert.Equal(WeaponKind.Chainsaw, inventory.Selected);
        Select(11); Assert.Equal(WeaponKind.Fist, inventory.Selected);
        Select(11); Assert.Equal(WeaponKind.Shotgun, inventory.Selected);
        Select(0, 8, 9, 10, 255); Assert.Equal(WeaponKind.Shotgun, inventory.Selected);
    }

    [Fact]
    public void FullQueueRejectsSelectionUntilRetryAndCopiesEventData()
    {
        var sim = Room(); var player = sim.Players.Single();
        for (var i = 0; i < PlayerPawn.CommandQueueCapacity; i++) sim.QueueCommand(0, default);
        var sink = new SimulationCommandSink(sim); var events = Events(1);
        Assert.False(sink.ApplyCommand(0, 0, 1, default, events));
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        sim.Tick();
        Assert.True(sink.ApplyCommand(0, 0, 1, default, events));
        events[^1] = 2;
        for (var i = 0; i < PlayerPawn.CommandQueueCapacity; i++) sim.Tick();
        Assert.Equal(WeaponKind.Fist, player.Inventory.Selected);
    }

    [Theory]
    [InlineData(new byte[] { 0 })]
    [InlineData(new byte[] { 0, 1, 78, 0, 0 })]
    [InlineData(new byte[] { 0, 1, 78, 0, 2, 1, 2 })]
    [InlineData(new byte[] { 0, 0, 1 })]
    public void MalformedEventsDoNotAdmitMovementOrAttack(byte[] events)
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.False(new SimulationCommandSink(sim).ApplyCommand(0, 0, 1,
            new UserCmd(1, 0, 123, 0, 0, 0, 0), events));
        Assert.Equal(0, player.BufferedCommandCount);
        sim.Tick();
        Assert.Equal(0u, player.Angle.Raw);
        Assert.Equal(50, player.Inventory.Bullets);
    }

    [Fact]
    public void DeadPlayersDoNotRetainSelectionForResurrection()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 0;
        Assert.True(new SimulationCommandSink(sim).ApplyCommand(0, 0, 1, default, Events(1)));
        player.Health = 100; sim.Tick();
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }
}
