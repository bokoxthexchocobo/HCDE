using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StrifeDoorTokenTests
{
    [Theory]
    [InlineData("OpenDoor222", 222, false)]
    [InlineData("CloseDoor222", 222, true)]
    [InlineData("OpenDoor224", 224, false)]
    public void AllGrantPathsStartNativeDoorMotionAndReplay(string name, int tag, bool close)
    {
        for (var path = 0; path < 3; path++)
        {
            var sim = Room(tag, close); var player = sim.Players.Single();
            var saved = SimSavegame.Write(sim, true);
            Grant(player, name, path);
            var motion = Assert.Single(sim.Motions);
            Assert.Equal(MotionKind.Door, motion.Kind); Assert.Equal(0, motion.SectorIndex);
            Assert.Equal(2, motion.Speed); Assert.Equal(close, motion.Closing); Assert.Equal(close, motion.CloseOnly);
            Assert.False(motion.CloseAfterOpen);
            Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
            Assert.Equal(1, AcsPlayerInventory.Count(player, name, true));
            var expected = SimSavegame.Write(sim, true);
            SimSavegame.Apply(sim, saved); Grant(player, name.ToLowerInvariant(), path);
            Assert.Equal(expected, SimSavegame.Write(sim, true));
        }
    }

    [Theory]
    [InlineData("OpenDoor222")]
    [InlineData("CloseDoor222")]
    [InlineData("OpenDoor224")]
    public void MissingTagStillAcceptsTokenWithoutStartingMotion(string name)
    {
        var sim = Room(7, false); var player = sim.Players.Single();
        Assert.True(ActorInventoryActions.GiveInventory(player, name));
        Assert.True(ActorInventoryActions.SetInventory(player, name, 3));
        Assert.Empty(sim.Motions); Assert.Empty(player.Inventory.HealthPickups);
    }

    [Fact]
    public void OpeningMotionAdvancesAndSurvivesSavedContinuation()
    {
        var sim = Room(222, false); var player = sim.Players.Single();
        Assert.True(ActorPickupActions.UseDoorToken(player, "OpenDoor222"));
        sim.Tick(); Assert.Equal(102, sim.CeilingOf(0));
        var saved = SimSavegame.Write(sim, true);
        sim.Tick(); Assert.Equal(104, sim.CeilingOf(0)); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); sim.Tick(); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void BusyDoorStillAcceptsTokenAndDoesNotDuplicateMotion()
    {
        var sim = Room(222, false); var player = sim.Players.Single();
        Assert.True(ActorPickupActions.UseDoorToken(player, "OpenDoor222"));
        Assert.True(ActorPickupActions.UseDoorToken(player, "CloseDoor222"));
        Assert.Single(sim.Motions);
    }

    [Fact]
    public void ZeroSetterAndAcsGiveDoNotActivateButStateActionDoes()
    {
        var sim = Room(222, false); var player = sim.Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, "OpenDoor222", 0));
        AcsPlayerInventory.Give(player, "OpenDoor222", 0); Assert.Empty(sim.Motions);
        Assert.True(ActorInventoryActions.GiveInventory(player, "OpenDoor222", 0)); Assert.Single(sim.Motions);
    }

    [Fact]
    public void InvalidTokenAndMissingSimulationAreReported()
    {
        Assert.Throws<ArgumentException>(() => ActorPickupActions.UseDoorToken(new Actor(), "Unknown"));
        Assert.Throws<InvalidOperationException>(() => ActorPickupActions.UseDoorToken(new Actor(), "OpenDoor222"));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(12)]
    public void ClosingAndWaitingDoorTimelinesReplay(int special)
    {
        var sim = Room(222, special == 10); var player = sim.Players.Single();
        var old = SimSavegame.Write(sim, true);
        Assert.True(ActorSpecialActions.CallSpecial(player, special, 222, 16, special == 12 ? 3 : 0));
        sim.Tick();
        if (special == 12) { sim.Tick(); Assert.Equal(3, Assert.Single(sim.Motions).Wait); }
        var saved = SimSavegame.Write(sim, true);
        for (var i = 0; i < 4; i++) sim.Tick();
        var expected = SimSavegame.Write(sim, true); var checksum = sim.Checksum;
        SimSavegame.Apply(sim, saved);
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(expected, SimSavegame.Write(sim, true)); Assert.Equal(checksum, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.Empty(sim.Motions);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void MalformedDoorStateRejectsBeforeSimulationMutation(int field)
    {
        var sim = Room(222, false); var player = sim.Players.Single();
        ActorPickupActions.UseDoorToken(player, "OpenDoor222");
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(257, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        var start = saved.Length - size + 8;
        if (field == 0) BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start), int.MaxValue);
        else if (field == 1) BinaryPrimitives.WriteDoubleLittleEndian(saved.AsSpan(start + 28), double.NaN);
        else if (field == 2) BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + 40), -1);
        else BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + 44), 64);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
        var state = sim.CaptureState();
        state.DoorMotions = [Assert.Single(state.DoorMotions!) with { Flags = 64 }];
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Single(sim.Motions); Assert.Equal(100, sim.CeilingOf(0));
    }

    private static void Grant(PlayerPawn player, string name, int path)
    {
        if (path == 0) AcsPlayerInventory.Give(player, name, 7);
        else if (path == 1) Assert.True(ActorInventoryActions.GiveInventory(player, name, 7));
        else Assert.True(ActorInventoryActions.SetInventory(player, name, 7));
    }

    private static AuthoritySimulation Room(int tag, bool close) => AuthoritySimulation.Start(new PlayLevel {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = tag, CeilingHeight = close ? 104 : 100 },
            new LevelSector { Index = 1, CeilingHeight = 108 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines = [new LevelLine { SideFront = 0, SideBack = 1 }], Things = [new LevelThing { Type = 1 }]
    });
}
