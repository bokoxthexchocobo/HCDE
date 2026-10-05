using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpecialLightTests
{
    [Theory]
    [InlineData(110, false, 160)]
    [InlineData(110, true, 160)]
    [InlineData(111, false, 96)]
    [InlineData(111, true, 96)]
    [InlineData(112, false, 32)]
    [InlineData(112, true, 32)]
    public void ActorActivationRunsTaggedLightAction(int special, bool death, int expected)
    {
        var sim = Room(); var actor = sim.Actors[0]; Configure(actor, special, 7);
        if (death) ActorDamage.Apply(actor, 1000, null);
        else Assert.True(ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.Equal(expected, sim.LightOf(0)); Assert.Equal(220, sim.LightOf(1));
        Assert.Equal(death ? 0 : special, actor.Special);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(99)]
    public void LightResultAllowsClearSpecialEvenWithoutMatchingTag(int tag)
    {
        var sim = Room(); var actor = sim.Actors[0]; Configure(actor, 112, tag); actor.ActivationType = 32;
        Assert.True(ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.Equal(0, actor.Special); Assert.Equal(tag == 7 ? 32 : 128, sim.LightOf(0));
    }

    [Fact]
    public void FadeArgumentsCreateAndAdvanceLightEffect()
    {
        var sim = Room(); var actor = sim.Actors[0]; Configure(actor, 113, 7); actor.SpecialArgs[2] = 4;
        Assert.True(ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.Single(sim.LightEffects);
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(56, sim.LightOf(0));
        sim.Tick();
        Assert.Equal(32, sim.LightOf(0)); Assert.Equal(220, sim.LightOf(1));
    }

    private static void Configure(Actor actor, int special, int tag)
    {
        actor.Special = special; actor.SpecialArgs[0] = tag; actor.SpecialArgs[1] = 32;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 },
            new LevelSector { Tag = 8, CeilingHeight = 128, LightLevel = 220 }],
        Things = [new LevelThing { Type = 3004, X = 20 }],
    });
}
