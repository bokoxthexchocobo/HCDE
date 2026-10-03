using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LostSoulTargetCopyTests
{
    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    public void SpawnCopiesOnlyEligibleTargetAndHearingMemory(bool noTarget, bool neverTarget, bool copied)
    {
        var (sim, parent, target) = Room();
        target.NoTarget = noTarget; target.NeverTarget = neverTarget;
        var soul = sim.SpawnLostSoul(parent, target, 0);
        Assert.NotNull(soul);
        Assert.Equal(copied, soul.Brain!.Charging);
        Assert.Equal(copied ? (uint?)target.Id : null, soul.Brain.TargetId);
        Assert.Equal(copied ? (uint?)target.Id : null, soul.LastHeardTargetId);
    }

    [Fact]
    public void NullTargetDoesNotInventHearingMemory()
    {
        var (sim, parent, _) = Room();
        var soul = sim.SpawnLostSoul(parent, null, 0);
        Assert.NotNull(soul);
        Assert.Null(soul.LastHeardTargetId);
        Assert.Null(soul.Brain!.TargetId);
        Assert.False(soul.Brain.Charging);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, false)]
    public void TargetCopyDoesNotRequireDamageEligibility(bool dead, bool nonShootable, bool invulnerable)
    {
        var (sim, parent, target) = Room();
        if (dead) target.Health = 0;
        if (nonShootable) target.Shootable = false;
        target.Invulnerable = invulnerable;
        var soul = sim.SpawnLostSoul(parent, target, 0);
        Assert.NotNull(soul);
        Assert.True(soul.Brain!.Charging);
        Assert.Equal(target.Id, soul.Brain.TargetId);
        Assert.Equal(target.Id, soul.LastHeardTargetId);
        Assert.Equal(20, soul.VelocityX.ToDouble());
    }

    [Fact]
    public void DestroyedTargetIsNotCopied()
    {
        var (sim, parent, target) = Room();
        target.Destroy();
        var soul = sim.SpawnLostSoul(parent, target, 0);
        Assert.NotNull(soul);
        Assert.False(soul.Brain!.Charging);
        Assert.Null(soul.Brain.TargetId);
        Assert.Null(soul.LastHeardTargetId);
    }

    [Fact]
    public void CopiedNoiseCanRestoreTargetAfterChargeStopsAndTargetClears()
    {
        var (sim, parent, target) = Room();
        var soul = sim.SpawnLostSoul(parent, target, 0);
        Assert.NotNull(soul);
        soul.Brain!.StopCharge(soul);
        soul.Brain.ClearTarget();
        soul.ReactionTime = 0;
        soul.Brain.Tick(sim, soul);
        Assert.Equal(target.Id, soul.Brain.TargetId);
    }

    private static (AuthoritySimulation Sim, Actor Parent, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1, X = 4096 }, new LevelThing { Type = 71 },
                new LevelThing { Type = 3001, X = 800 }],
        });
        return (sim, sim.Actors.Single(actor => actor.DoomEdNum == 71),
            sim.Actors.Single(actor => actor.DoomEdNum == 3001));
    }
}
