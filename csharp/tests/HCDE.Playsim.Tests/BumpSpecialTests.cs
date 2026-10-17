using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BumpSpecialTests
{
    [Theory]
    [InlineData(true, 0, true)]
    [InlineData(false, 0, false)]
    [InlineData(false, 8, true)]
    public void HorizontalContactHonorsTriggerEligibility(bool player, int activation, bool runs)
    {
        var (sim, mover, target) = Room(player); target.ActivationType = activation;
        sim.Thinkers.Clock.Restore(1);
        Assert.True(ActorPhysics.TryMove(sim, mover, 8, 0, out _));
        Assert.Equal(runs ? 200 : 128, sim.LightOf(0)); Assert.Equal(runs ? 36 : 0, target.LastBumpTic);
    }

    [Fact]
    public void InitialTicAndExactCooldownBoundaryDoNotActivate()
    {
        var (sim, mover, target) = Room();
        ActorPhysics.TryMove(sim, mover, 8, 0, out _); Assert.Equal(128, sim.LightOf(0));
        sim.Thinkers.Clock.Restore(1); ActorSpecialActions.TryBump(sim, target, mover);
        Assert.Equal(200, sim.LightOf(0)); target.SpecialArgs[1] = 210;
        sim.Thinkers.Clock.Restore(36); ActorSpecialActions.TryBump(sim, target, mover); Assert.Equal(200, sim.LightOf(0));
        sim.Thinkers.Clock.Restore(37); ActorSpecialActions.TryBump(sim, target, mover);
        Assert.Equal(210, sim.LightOf(0)); Assert.Equal(72, target.LastBumpTic);
    }

    [Fact]
    public void FailureDoesNotConsumeCooldownAndCanRetrySameTic()
    {
        var (sim, mover, target) = Room(); sim.Thinkers.Clock.Restore(1); target.Special = 999;
        ActorSpecialActions.TryBump(sim, target, mover); Assert.Equal(0, target.LastBumpTic);
        target.Special = 112; ActorSpecialActions.TryBump(sim, target, mover); Assert.Equal(36, target.LastBumpTic);
    }

    [Fact]
    public void ClearSpecialAndTargetSwitchingReuseThingActivationRules()
    {
        var (sim, mover, target) = Room(); sim.Thinkers.Clock.Restore(1); target.ActivationType = 32 | 2;
        ActorPhysics.TryMove(sim, mover, 8, 0, out _);
        Assert.Equal(0, target.Special); Assert.Equal(mover.Id, target.TargetId); Assert.Equal(36, target.LastBumpTic);
    }

    [Fact]
    public void StandingOnBumperActivatesWithoutHorizontalMovement()
    {
        var (sim, mover, target) = Room(); sim.Thinkers.Clock.Restore(1);
        target.X = default; target.Solid = true; ActorPropertyActions.SetSize(target, 20, 16);
        mover.Z = Fixed.FromInt(16); mover.VelocityZ = Fixed.FromInt(-1);
        ActorPhysics.Step(sim, mover);
        Assert.Equal(200, sim.LightOf(0)); Assert.Equal(36, target.LastBumpTic);
    }

    [Fact]
    public void TouchDeathReturnsBeforeBumpActivation()
    {
        var (sim, mover, target) = Room(); sim.Thinkers.Clock.Restore(1);
        target.Touchy = target.Armed = true; target.SeeState = -1;
        ActorPhysics.TryMove(sim, mover, 8, 0, out _);
        Assert.True(target.IsDead); Assert.Equal(0, target.LastBumpTic);
    }

    [Fact]
    public void FlagAndCooldownRoundTripIncludingDisabledFlagHistory()
    {
        var (sim, mover, target) = Room(); target.BumpSpecial = false; var older = SimSavegame.Write(sim, true);
        target.BumpSpecial = true; sim.Thinkers.Clock.Restore(1); ActorSpecialActions.TryBump(sim, target, mover);
        target.BumpSpecial = false; var saved = SimSavegame.Write(sim, true);
        Assert.Equal(322, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var (restored, loaded, hit) = Room(); SimSavegame.Apply(restored, saved);
        Assert.False(hit.BumpSpecial); Assert.Equal(36, hit.LastBumpTic); Assert.Equal(saved, SimSavegame.Write(restored, true));
        hit.BumpSpecial = true; sim.Thinkers.Clock.Restore(36); restored.Thinkers.Clock.Restore(36);
        target.BumpSpecial = true; ActorSpecialActions.TryBump(sim, target, mover); ActorSpecialActions.TryBump(restored, hit, loaded);
        sim.RestoreState(sim.CaptureState()); restored.RestoreState(restored.CaptureState()); Assert.Equal(sim.Checksum, restored.Checksum);
        SimSavegame.Apply(restored, older); Assert.False(hit.BumpSpecial); Assert.Equal(0, hit.LastBumpTic);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void InvalidFlagArchiveIsRejected(int value)
    {
        var (sim, _, _) = Room(); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-bumpspecial-value", error);
    }

    [Fact]
    public void AcsExposesBumpSpecialFlag()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "Actor.BumpSpecial", true));
        Assert.True(AcsActorFlags.TryGet(actor, "BUMPSPECIAL", out var flag)); Assert.True(flag);
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Target) Room(bool player = true)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256, LightLevel = 128, Tag = 7 }], Things = player ? [new LevelThing { Type = 1 }] : [] });
        Actor mover = player ? sim.Players.Single() : sim.AddBot(0, 0, 3001);
        var target = sim.AddBot(30, 0); mover.Brain = target.Brain = null; target.Solid = false;
        target.BumpSpecial = true; target.Special = 112; target.SpecialArgs[0] = 7; target.SpecialArgs[1] = 200;
        return (sim, mover, target);
    }
}
