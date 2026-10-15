using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HateRetentionSightTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RetentionUsesFullHeightApertureAfterCoin(bool visible)
    {
        var (sim, hunter, _, other) = Room(); var (reference, _, _, _) = Room();
        sim.Floors[1] = visible ? 20 : 0; sim.Ceilings[1] = visible ? 25 : 10;
        for (var i = 0; i < 256; i++)
        {
            var retain = reference.NextSwitchTargetByte() < 128 && visible;
            Assert.Equal(!retain, hunter.OkayToSwitchTarget(other, hunter.Brain!));
            Assert.Equal(reference.NativeSwitchTargetRandomState, sim.NativeSwitchTargetRandomState);
        }
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void VisibilityDrawOccursOnlyAfterSuccessfulRetentionCoin(int kind)
    {
        var (sim, hunter, current, other) = Room(); var (reference, _, _, _) = Room();
        current.Invisible = kind == 0; current.MinVisible = kind == 1;
        if (kind == 2) { current.RenderStyle = ActorRenderStyle.Translucent; current.Alpha = 0; }
        for (var i = 0; i < 256; i++)
        {
            var retain = reference.NextSwitchTargetByte() < 128
                && reference.NextCheckSightByte() <= 50;
            Assert.Equal(!retain, hunter.OkayToSwitchTarget(other, hunter.Brain!));
            Assert.Equal(reference.NativeSwitchTargetRandomState, sim.NativeSwitchTargetRandomState);
            Assert.Equal(reference.CaptureState().CheckSightRandomState, sim.CaptureState().CheckSightRandomState);
        }
    }

    [Fact]
    public void MvisBlockedAllowsSwitchWithoutVisibilityDraw()
    {
        var (sim, hunter, current, other) = Room(); current.MVisBlocked = true;
        for (var i = 0; i < 256; i++) Assert.True(hunter.OkayToSwitchTarget(other, hunter.Brain!));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DeadOrUnhatedCurrentTargetSkipsBothDraws(bool dead)
    {
        var (sim, hunter, current, other) = Room(); current.Invisible = true;
        if (dead) current.Health = 0; else current.ThingId = 8;
        var before = sim.NativeSwitchTargetRandomState;
        Assert.True(hunter.OkayToSwitchTarget(other, hunter.Brain!));
        Assert.Equal(before, sim.NativeSwitchTargetRandomState);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void SavedRetentionDecisionsReplayBothStreams()
    {
        var (sim, hunter, current, other) = Room(); current.Invisible = true;
        var saved = SimSavegame.Write(sim, true);
        var expected = Enumerable.Range(0, 64).Select(_ => hunter.OkayToSwitchTarget(other, hunter.Brain!)).ToArray();
        var resultingSave = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        Assert.Equal(expected, Enumerable.Range(0, 64).Select(_ => hunter.OkayToSwitchTarget(other, hunter.Brain!)).ToArray());
        Assert.Equal(resultingSave, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void OlderSaveResetsRetentionStreamToSeed()
    {
        var (sim, _, _, _) = Room(); var initial = sim.SwitchTargetRandomState;
        var saved = SimSavegame.Write(sim, true);
        sim.NextSwitchTargetRandom(); SimSavegame.Apply(sim, saved);
        Assert.Equal(initial, sim.SwitchTargetRandomState);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ZeroRetentionStateSurvivesArchive()
    {
        var (sim, _, _, _) = Room(); var state = sim.CaptureState();
        state.SwitchTargetRandomState = 0;
        var saved = SimSavegame.Write(state); SimSavegame.Apply(sim, saved);
        Assert.Equal(0u, sim.SwitchTargetRandomState);
        Assert.True(SimSavegame.TryRead(saved, out var decoded, out _));
        Assert.Equal(0u, decoded.SwitchTargetRandomState);
        var canonical = SimSavegame.Write(sim, true); SimSavegame.Apply(sim, canonical);
        Assert.Equal(canonical, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedRetentionTrailerRejectsWithoutApplying(bool header)
    {
        var (sim, _, _, _) = Room(); sim.NextSwitchTargetRandom();
        var saved = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - (header ? 12 : 4)), header ? 288 : 11);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal(header ? "save-switchtargetrandom-header" : "save-switchtargetrandom-size", error);
    }

    [Fact]
    public void RetentionStateRequiresGeometryBeforeRestoreMutation()
    {
        var (sim, _, _, _) = Room(); var state = sim.CaptureState();
        state.GeometryHealth = null; state.SwitchTargetRandomState = 0;
        var before = sim.SwitchTargetRandomState;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, sim.SwitchTargetRandomState);
    }

    private static (AuthoritySimulation, Actor, Actor, Actor) Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = geometry.Sectors, Sides = geometry.Sides, Lines = geometry.Lines }, rngSeed: 42);
        var hunter = sim.AddBot(-64, 0); var current = sim.AddBot(64, 0, 3001);
        var other = sim.AddBot(-64, 64, 3002);
        hunter.TidToHate = current.ThingId = 7;
        hunter.SetTargetPointer(current); hunter.SetChaseThreshold(0, false);
        return (sim, hunter, current, other);
    }
}
