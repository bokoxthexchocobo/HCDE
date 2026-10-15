using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerNoTargetCheatTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void VisualSearchUsesCheatIndependentlyOfActorFlag(bool flag, bool cheat)
    {
        var (sim, hunter, player) = Room(); player.NoTarget = flag; player.NoTargetCheat = cheat;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(cheat ? (uint?)null : player.Id, hunter.TargetId);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void NoiseAndHearingUseCheatIndependentlyOfActorFlag(bool flag, bool cheat)
    {
        var (sim, hunter, player) = Room(); player.NoTarget = flag; player.NoTargetCheat = cheat;
        SoundPropagation.Alert(sim, player);
        Assert.Equal(cheat ? (uint?)null : player.Id, hunter.LastHeardTargetId);
        Assert.Equal(cheat ? (uint?)null : player.Id, hunter.TargetId);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void NativeDamageEligibilityUsesActorFlagRatherThanCheat(bool flag, bool cheat)
    {
        var (_, hunter, player) = Room(); player.Friendly = false;
        player.NoTarget = flag; player.NoTargetCheat = cheat;
        ActorDamage.Apply(hunter, 1, player, DamageFlags.NoPain);
        Assert.Equal(flag ? (uint?)null : player.Id, hunter.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OverrideSurvivesSaveAndOlderSaveRestoresLegacyFallback(bool cheat)
    {
        var (sim, _, player) = Room(); player.NoTarget = !cheat;
        var older = SimSavegame.Write(sim, true);
        player.NoTargetCheat = cheat; var saved = SimSavegame.Write(sim, true);
        Assert.Equal(290, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        player.NoTargetCheat = !cheat; SimSavegame.Apply(sim, saved);
        Assert.Equal(cheat, player.NoTargetCheat); Assert.Equal(!cheat, player.NoTarget);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, older);
        Assert.Null(player.NoTargetCheatOverride); Assert.Equal(player.NoTarget, player.NoTargetCheat);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidStructuredOverrideRejectsBeforeMutation(int value)
    {
        var (sim, _, player) = Room(); var state = sim.CaptureState();
        state.Actors.Single(a => a.Id == player.Id).PlayerNoTargetCheatOverride = value;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Null(player.NoTargetCheatOverride);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedOverrideArchiveRejects(bool header)
    {
        var (sim, _, player) = Room(); player.NoTargetCheat = true;
        var saved = SimSavegame.Write(sim, true);
        var start = saved.Length - BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + (header ? 0 : 8)), header ? 290 : 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal(header ? "save-playernotargetcheat-header" : "save-playernotargetcheat-value", error);
    }

    [Fact]
    public void ExplicitFalseStillRequiresGeometryState()
    {
        var (sim, _, player) = Room(); player.NoTargetCheat = false;
        var state = sim.CaptureState(); state.GeometryHealth = null;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.False(player.NoTargetCheat);
    }

    private static (AuthoritySimulation, Actor, PlayerPawn) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 128 }] });
        var hunter = sim.AddBot(0, 0); hunter.NoPain = true; hunter.Health = 1000;
        hunter.SetChaseThreshold(0, false);
        return (sim, hunter, sim.Players.Single());
    }
}
