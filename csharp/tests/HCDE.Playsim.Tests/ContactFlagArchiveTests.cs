using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ContactFlagArchiveTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.Clip }],
    });

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void ChangedFlagsRoundTripAndControlCollection(bool pickup, bool special)
    {
        var original = Room(); original.Players.Single().CanPickupItems = pickup;
        original.Actors[^1].SpecialPickup = special;
        var bytes = SimSavegame.Write(original);
        Assert.Equal(18, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, bytes);
        Assert.Equal(pickup, restored.Players.Single().CanPickupItems);
        Assert.Equal(special, restored.Actors[^1].SpecialPickup);
        original.Tick(); restored.Tick(); Assert.Equal(original.Checksum, restored.Checksum);
        Assert.False(restored.Actors[^1].Destroyed);
        restored.Players.Single().CanPickupItems = true; restored.Actors[^1].SpecialPickup = true;
        restored.Tick(); Assert.DoesNotContain(restored.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void InvalidFlagBitsAreRejected(int flags)
    {
        var sim = Room(); sim.Players.Single().CanPickupItems = false;
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), flags);
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-contact-flags", error); Assert.Empty(state.Actors);
    }

    [Fact]
    public void TruncationsAreRejected()
    {
        var sim = Room(); sim.Players.Single().CanPickupItems = false;
        var bytes = SimSavegame.Write(sim);
        for (var length = 0; length < bytes.Length; length++)
            Assert.False(SimSavegame.TryRead(bytes.AsSpan(0, length), out _, out _));
    }

    [Fact]
    public void CurrentArchiveRestoresDefaultContactFlags()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        Assert.Equal(18, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Players.Single().CanPickupItems = false; sim.Actors[^1].SpecialPickup = false;
        SimSavegame.Apply(sim, bytes);
        Assert.True(sim.Players.Single().CanPickupItems); Assert.True(sim.Actors[^1].SpecialPickup);
        sim.Tick();
        Assert.DoesNotContain(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip);
    }

    [Fact]
    public void LegacyArchivePreservesCurrentContactFlags()
    {
        var sim = Room(); var state = sim.CaptureState();
        foreach (var pose in state.Actors) pose.ContactFlags = null;
        var bytes = SimSavegame.Write(state);
        Assert.Equal(17, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Players.Single().CanPickupItems = false; sim.Actors[^1].SpecialPickup = false;
        SimSavegame.Apply(sim, bytes);
        Assert.False(sim.Players.Single().CanPickupItems); Assert.False(sim.Actors[^1].SpecialPickup);
    }
}
