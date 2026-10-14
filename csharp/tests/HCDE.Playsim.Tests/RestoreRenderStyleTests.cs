using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RestoreRenderStyleTests
{
    [Theory]
    [InlineData(3004, false, 1)]
    [InlineData(58, true, 0.5)]
    public void RestoreUsesNativeClassDefaults(int type, bool shadow, double alpha)
    {
        var sim = Room(type); var actor = sim.Actors.Single(); var expectedStyle = actor.DefaultRenderStyle;
        actor.Shadow = !shadow; actor.Ghost = true; actor.RenderStyle = ActorRenderStyle.FromNative(0); actor.Alpha = 0;
        actor.RestoreRenderStyle();
        Assert.Equal(shadow, actor.Shadow); Assert.False(actor.Ghost);
        Assert.Equal(expectedStyle, actor.RenderStyle); Assert.Equal(alpha, actor.Alpha);
    }

    [Fact]
    public void MapOverridesDoNotBecomeClassDefaults()
    {
        var sim = Room(58, mapOverrides: true); var actor = sim.Actors.Single();
        Assert.Equal(0.25, actor.Alpha); Assert.Equal(ActorRenderStyle.Translucent, actor.RenderStyle);
        actor.RestoreRenderStyle();
        Assert.Equal(0.5, actor.Alpha); Assert.Equal(ActorRenderStyle.OptFuzzy, actor.RenderStyle); Assert.True(actor.Shadow);
    }

    [Fact]
    public void UnrelatedVisibilityFlagsArePreserved()
    {
        var sim = Room(3004); var actor = sim.Actors.Single();
        actor.Invisible = actor.MinVisible = actor.MVisBlocked = actor.ThruGhost = true;
        actor.Ghost = true; actor.RestoreRenderStyle();
        Assert.False(actor.Ghost); Assert.True(actor.Invisible); Assert.True(actor.MinVisible);
        Assert.True(actor.MVisBlocked); Assert.True(actor.ThruGhost);
    }

    [Fact]
    public void PatchedPrimaryShadowDefaultIsRestored()
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + SHADOW\n");
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] }, dehacked: patch);
        var actor = sim.Actors.Single(); Assert.True(actor.DefaultShadow);
        actor.Shadow = false; actor.RestoreRenderStyle(); Assert.True(actor.Shadow);
    }

    [Theory]
    [InlineData(ProjectileKind.Rocket)]
    [InlineData(ProjectileKind.Plasma)]
    [InlineData(ProjectileKind.ImpBall)]
    public void ProjectilesRestoreTheirFactoryRenderDefaults(ProjectileKind kind)
    {
        var sim = Room(3004); var missile = sim.SpawnProjectile(sim.Actors.Single(), kind);
        var style = missile.DefaultRenderStyle; var alpha = missile.DefaultAlpha;
        missile.RenderStyle = ActorRenderStyle.FromNative(0); missile.Alpha = 0; missile.Ghost = true;
        missile.RestoreRenderStyle();
        Assert.Equal(style, missile.RenderStyle); Assert.Equal(alpha, missile.Alpha); Assert.False(missile.Ghost);
    }

    [Fact]
    public void StateActionAndSaveRestoreKeepRestoredDefaults()
    {
        var sim = Room(58); var actor = sim.Actors.Single(); actor.Ghost = true; actor.Shadow = false; actor.Alpha = 0;
        actor.States.Configure(actor, [new(-1, 0), new(5, 0, Action: ActorPropertyActions.RestoreRenderStyle)], 0);
        actor.States.Enter(actor, 1); Assert.True(actor.Shadow); Assert.False(actor.Ghost); Assert.Equal(0.5, actor.Alpha);
        var saved = SimSavegame.Write(sim); actor.Shadow = false; actor.Ghost = true; actor.Alpha = 0;
        SimSavegame.Apply(sim, saved); Assert.True(actor.Shadow); Assert.False(actor.Ghost); Assert.Equal(0.5, actor.Alpha);
        Assert.Equal(saved, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room(int type, bool mapOverrides = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = type, Alpha = mapOverrides ? 0.25 : -1, RenderStyle = mapOverrides ? 6 : null }]
    });
}
