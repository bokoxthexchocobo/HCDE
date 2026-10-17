using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StealthVisibilityTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(0)]
    public void TickAppliesNativeDirectionalRateAndClearsOnlyForcedAlphaFlag(int direction)
    {
        var (_, actor) = Room(); actor.Alpha = 0.5; actor.VisibilityDirection = direction;
        actor.RenderStyle = ActorRenderStyle.Translucent with { Flags = 15 };
        actor.Tick();
        Assert.Equal(0.5 + (direction > 0 ? 2.0 / 35 : direction < 0 ? -1.5 / 35 : 0), actor.Alpha, 12);
        Assert.Equal(13, actor.RenderStyle.Flags); Assert.Equal(direction, actor.VisibilityDirection);
    }

    [Fact]
    public void FadeInStopsOnlyAfterPassingOne()
    {
        var (_, actor) = Room(); actor.VisibilityDirection = 1; actor.Alpha = 1 - 2.0 / 35;
        actor.Tick(); Assert.Equal(1, actor.Alpha); Assert.Equal(1, actor.VisibilityDirection);
        actor.Tick(); Assert.Equal(1, actor.Alpha); Assert.Equal(0, actor.VisibilityDirection);
    }

    [Fact]
    public void FadeOutStopsOnlyAfterPassingStealthAlpha()
    {
        var (_, actor) = Room(); actor.VisibilityDirection = -1; actor.Alpha = 1.5 / 35;
        actor.Tick(); Assert.Equal(0, actor.Alpha); Assert.Equal(-1, actor.VisibilityDirection);
        actor.Tick(); Assert.Equal(0, actor.Alpha); Assert.Equal(0, actor.VisibilityDirection);
    }

    [Fact]
    public void FadeOutUsesCustomMinimum()
    {
        var (_, actor) = Room(); actor.VisibilityDirection = -1; actor.Alpha = 0.5; actor.StealthAlpha = 0.49;
        actor.Tick(); Assert.Equal(0.49, actor.Alpha); Assert.Equal(0, actor.VisibilityDirection);
    }

    [Fact]
    public void PulseTakesPrecedenceAndDoesNotClearForcedAlpha()
    {
        var (_, actor) = Room(); actor.VisibilityPulse = true; actor.Alpha = 0.5; actor.VisibilityDirection = 1;
        var style = actor.RenderStyle; actor.Tick();
        Assert.Equal(0.53125, actor.Alpha); Assert.Equal(style, actor.RenderStyle);
    }

    [Fact]
    public void FacingStartsFadeInButMissingSubjectDoesNot()
    {
        var (sim, actor) = Room(); actor.VisibilityDirection = -1;
        ActorOrientationActions.Face(actor, null); Assert.Equal(-1, actor.VisibilityDirection);
        ActorOrientationActions.Face(actor, sim.AddBot(100, 0)); Assert.Equal(1, actor.VisibilityDirection);
        actor.Tick(); Assert.True(actor.Alpha > 0);
    }

    [Fact]
    public void LookAndEnabledChaseStartFadeOutButConversationDoesNot()
    {
        var (sim, actor) = Room();
        ActorLookActions.LookEx(actor, ActorLookFlags.NoSoundCheck | ActorLookFlags.NoSightCheck | ActorLookFlags.NoJump);
        Assert.Equal(-1, actor.VisibilityDirection);
        actor.VisibilityDirection = 1; actor.Brain!.Enabled = true; actor.Brain.Tick(sim, actor);
        Assert.Equal(-1, actor.VisibilityDirection);
        actor.VisibilityDirection = 1; actor.InConversation = true; actor.Brain.Tick(sim, actor);
        Assert.Equal(1, actor.VisibilityDirection);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void DamageRevealsBeforeDormantAndNoDamageGates(int gate)
    {
        var (_, actor) = Room(); actor.Alpha = 0.2; actor.VisibilityDirection = 1;
        if (gate == 1) actor.Dormant = true;
        if (gate == 2) actor.NoDamage = true;
        ActorDamage.Apply(actor, 1);
        Assert.Equal(1, actor.Alpha); Assert.Equal(-1, actor.VisibilityDirection);
        Assert.Equal(gate == 0 ? 19 : 20, actor.Health);
    }

    [Fact]
    public void InvulnerabilityRejectsBeforeStealthReveal()
    {
        var (_, actor) = Room(); actor.Alpha = 0.2; actor.VisibilityDirection = 1; actor.Invulnerable = true;
        ActorDamage.Apply(actor, 1); Assert.Equal(0.2, actor.Alpha); Assert.Equal(1, actor.VisibilityDirection);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TunedStateRoundTripsEvenWhenFlagDisabledAndLegacyResets(bool enabled)
    {
        var (sim, actor) = Room(); actor.Stealth = false; var legacy = SimSavegame.Write(sim, true);
        actor.Stealth = enabled; actor.StealthAlpha = 0.3; actor.Alpha = 0.6; actor.VisibilityDirection = -1;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(331, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.Stealth = !enabled; actor.StealthAlpha = 0;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(enabled, actor.Stealth); Assert.Equal(0.3, actor.StealthAlpha); Assert.Equal(-1, actor.VisibilityDirection);
        Assert.Equal(checksum, sim.Checksum); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, legacy); Assert.False(actor.Stealth); Assert.Equal(0, actor.StealthAlpha);
    }

    [Fact]
    public void FadeContinuesIdenticallyAfterSaveRestore()
    {
        var (sim, actor) = Room(); actor.StealthAlpha = 0.2; actor.Alpha = 1; actor.VisibilityDirection = -1;
        actor.Tick(); var bytes = SimSavegame.Write(sim, true);
        (double, int) Next() { actor.Tick(); return (actor.Alpha, actor.VisibilityDirection); }
        var expected = Enumerable.Range(0, 30).Select(_ => Next()).ToArray();
        SimSavegame.Apply(sim, bytes); Assert.Equal(expected, Enumerable.Range(0, 30).Select(_ => Next()).ToArray());
    }

    [Fact]
    public void AcsCanSetQueryAndClearStealth()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "STEALTH", true));
        Assert.True(AcsActorFlags.TryGet(actor, "stealth", out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, "Stealth", false)); Assert.False(actor.Stealth);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlagAndMinimumAffectChecksum(bool tuning)
    {
        var (sim, actor) = Room(); actor.Stealth = false; sim.RestoreState(sim.CaptureState()); var baseline = sim.Checksum;
        if (tuning) actor.StealthAlpha = 0.2; else actor.Stealth = true;
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidArchiveValuesAreRejected(bool alpha)
    {
        var (sim, _) = Room(); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var item = bytes.AsSpan(bytes.Length - size + 8);
        if (alpha) BinaryPrimitives.WriteDoubleLittleEndian(item[4..], double.NaN);
        else BinaryPrimitives.WriteInt32LittleEndian(item, 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-stealth-value", error);
    }

    private static (AuthoritySimulation Sim, Actor Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [] });
        var actor = sim.AddBot(0, 0); actor.Brain!.Enabled = false; actor.NoGravity = true; actor.Stealth = true;
        return (sim, actor);
    }
}
