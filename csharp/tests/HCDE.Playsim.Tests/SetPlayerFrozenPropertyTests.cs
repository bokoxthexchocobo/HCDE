using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SetPlayerFrozenPropertyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void ActionSetsAndClearsOnlySelectedFreezeMode(int property)
    {
        var sim = Room(); var player = sim.Players.First();
        Assert.True(ActorSpecialActions.CallSpecial(player, 191, 0, -1, property));
        Assert.Equal(property == 0, player.Frozen); Assert.Equal(property == 4, player.TotallyFrozen);
        Assert.False(sim.Players.Last().Frozen); Assert.False(sim.Players.Last().TotallyFrozen);
        Assert.True(PlayerPropertyActions.ExecuteSpecial(sim, 191, null, 1, 0, property));
        Assert.False(player.Frozen); Assert.False(player.TotallyFrozen);
    }

    [Fact]
    public void FrozenMasksMovementButAllowsLookingAndUse()
    {
        var sim = Room(); var player = sim.Players.First(); player.Frozen = true;
        player.Pending = new PlayerCommand { ForwardMove = 1000, SideMove = 1000, UpMove = 100,
            YawDelta = 100, PitchDelta = 100, Use = true };
        player.Tick();
        Assert.Equal(0, player.VelocityX.Raw); Assert.Equal(0, player.VelocityY.Raw);
        Assert.NotEqual(0u, player.Angle.Raw); Assert.NotEqual(0, player.PitchDegrees);
        Assert.True(player.UsePressed);
    }

    [Fact]
    public void TotallyFrozenMasksLookingAttackAndTurnButKeepsUse()
    {
        var sim = Room(); var player = sim.Players.First(); player.TotallyFrozen = true; player.TurnTicks = 5;
        player.Pending = new PlayerCommand { ForwardMove = 1000, SideMove = 1000, UpMove = 100,
            YawDelta = 100, PitchDelta = 100, Attack = true, Jump = true, Turn180 = true, Crouch = true, Use = true };
        player.Tick();
        Assert.Equal(0, player.VelocityX.Raw); Assert.Equal(0, player.VelocityY.Raw);
        Assert.Equal(0u, player.Angle.Raw); Assert.Equal(0, player.PitchDegrees); Assert.Equal(0, player.TurnTicks);
        Assert.False(player.AttackPressed); Assert.True(player.UsePressed); Assert.Equal(1, player.CrouchFactor);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FreezeDoesNotEraseExistingMomentum(bool total)
    {
        var sim = Room(); var player = sim.Players.First(); var reference = sim.Players.Last();
        player.VelocityX = reference.VelocityX = Fixed.FromInt(2);
        player.Frozen = !total; player.TotallyFrozen = total;
        player.Tick(); reference.Tick();
        Assert.Equal(reference.VelocityX, player.VelocityX); Assert.NotEqual(0, player.VelocityX.Raw);
    }

    [Fact]
    public void DirectInventoryUseBlocksButAcsBypassesTotalFreeze()
    {
        var sim = Room(); var player = sim.Players.First(); player.Health = 30;
        ActorInventoryActions.SetInventory(player, "MedPatch", 2); player.TotallyFrozen = true;
        Assert.False(ActorInventoryActions.UseInventory(player, "MedPatch")); Assert.Equal(30, player.Health);
        Assert.Equal(1, AcsPlayerInventory.Use(player, "MedPatch")); Assert.Equal(40, player.Health);
        Assert.True(player.TotallyFrozen);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void SavedModesRoundTripAndOlderSaveClearsThem(int flags)
    {
        var sim = Room(); var player = sim.Players.First(); var older = SimSavegame.Write(sim, true);
        player.Frozen = (flags & 1) != 0; player.TotallyFrozen = (flags & 2) != 0;
        var saved = SimSavegame.Write(sim, true);
        player.Frozen = player.TotallyFrozen = false; SimSavegame.Apply(sim, saved);
        Assert.Equal((flags & 1) != 0, player.Frozen); Assert.Equal((flags & 2) != 0, player.TotallyFrozen);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, older); Assert.False(player.Frozen); Assert.False(player.TotallyFrozen);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void InvalidModesRejectBeforeMutation(int flags)
    {
        var sim = Room(); var state = sim.CaptureState(); state.Actors[0].PlayerFrozenFlags = flags;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.False(sim.Players.First().Frozen);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 128 }] },
        spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
