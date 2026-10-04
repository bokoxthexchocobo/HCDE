using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InventoryTossEligibilityTests
{
    [Fact]
    public void TossDisablesPickupFlagUntilDelayExpires()
    {
        var sim = Room();
        AcsPlayerInventory.Drop(sim.Players.Single(), "Clip");
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Clip);
        Assert.False(drop.SpecialPickup);
        Assert.False(drop.Solid);
        drop.X = Fixed.FromInt(500);
        drop.VelocityX = default; drop.VelocityY = default;
        for (var i = 0; i < 29; i++) sim.Tick();
        Assert.False(drop.SpecialPickup);
        Assert.Equal(1, drop.PickupDelay);
        sim.Tick();
        Assert.True(drop.SpecialPickup);
        Assert.False(drop.Solid);
        Assert.Equal(0, drop.PickupDelay);
    }

    [Fact]
    public void SavedInactivePickupRestoresFlagAndRemainingDelay()
    {
        var sim = Room();
        AcsPlayerInventory.Drop(sim.Players.Single(), "Clip");
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Clip);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        drop.SpecialPickup = true; drop.PickupDelay = 0;
        sim.RestoreState(state);
        Assert.False(drop.SpecialPickup);
        Assert.Equal(30, drop.PickupDelay);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(31)]
    public void InvalidDirectRestoreIsRejectedBeforeMutation(int delay)
    {
        var sim = Room();
        AcsPlayerInventory.Drop(sim.Players.Single(), "Clip");
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Clip);
        var state = sim.CaptureState();
        state.Actors.Single(a => a.Id == drop.Id).PickupDelay = delay;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(30, drop.PickupDelay);
        Assert.False(drop.SpecialPickup);
    }

    [Fact]
    public void DelayOnNonPickupActorIsRejected()
    {
        var sim = Room(); var state = sim.CaptureState();
        state.Actors.Single().PickupDelay = 1;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, sim.Players.Single().PickupDelay);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
