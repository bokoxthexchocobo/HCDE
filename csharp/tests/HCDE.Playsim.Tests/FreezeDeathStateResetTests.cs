using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FreezeDeathStateResetTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0.25)]
    [InlineData(0.75)]
    public void FreezeRestoresNormalFullAlphaAndPassMobjFlag(double alpha)
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.CanPass = false; actor.RenderStyle = ActorRenderStyle.Translucent; actor.Alpha = alpha;
        ActorFreezeActions.FreezeDeath(actor);
        Assert.True(actor.CanPass); Assert.Equal(ActorRenderStyle.Normal, actor.RenderStyle); Assert.Equal(1, actor.Alpha);
        Assert.True(actor.NoBlood); Assert.True(actor.IceCorpse);
    }

    [Fact]
    public void FreezeClearsPlayerDamagePoisonAndBonusCounters()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.DamageCount = 20; player.PoisonCount = 50; player.BonusCount = 30;
        ActorFreezeActions.FreezeDeath(player);
        Assert.Equal(0, player.DamageCount); Assert.Equal(0, player.PoisonCount); Assert.Equal(0, player.BonusCount);
    }

    [Theory]
    [InlineData(false, -1)]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, -1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public void FreezeStopsOnlyStealthFadeAndRestoresSavedDirection(bool stealth, int direction)
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.Stealth = stealth;
        actor.VisibilityDirection = direction; actor.Alpha = 0.25;
        ActorFreezeActions.FreezeDeath(actor);
        Assert.Equal(stealth ? 0 : direction, actor.VisibilityDirection); Assert.Equal(1, actor.Alpha);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
        if (stealth) { Assert.Equal(1, actor.Alpha); Assert.Equal(0, actor.VisibilityDirection); }
    }

    [Fact]
    public void FreezeResetsSurviveSaveAndRestoration()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.CanPass = false; player.RenderStyle = ActorRenderStyle.Translucent; player.Alpha = 0.25;
        player.PoisonCount = 50;
        ActorFreezeActions.FreezeDeath(player);
        var saved = SimSavegame.Write(sim, true);
        player.CanPass = false; player.RenderStyle = ActorRenderStyle.Translucent; player.Alpha = 0.25; player.PoisonCount = 50;
        SimSavegame.Apply(sim, saved);
        Assert.True(player.CanPass); Assert.Equal(ActorRenderStyle.Normal, player.RenderStyle);
        Assert.Equal(1, player.Alpha); Assert.Equal(0, player.PoisonCount);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
