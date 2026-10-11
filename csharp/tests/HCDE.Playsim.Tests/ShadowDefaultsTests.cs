using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ShadowDefaultsTests
{
    [Theory]
    [InlineData(58, null, true)]
    [InlineData(3004, null, false)]
    [InlineData(58, "Thing 14\nBits = 6\n", false)]
    [InlineData(3004, "Thing 2\nBits = 262150\n", true)]
    [InlineData(3004, "Thing 2\nBits = SOLID + SHOOTABLE + SHADOW\n", true)]
    public void MapAndDynamicSpawnsUseClassBaselineAndSavesPreserveRuntimeOverride(int type, string? patch, bool shadow)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = type }],
        }, dehacked: patch == null ? null : DehackedPatch.Apply(patch));
        var actor = sim.Actors.Single();
        Assert.Equal(shadow, actor.Shadow);
        var legacy = SimSavegame.Write(sim);
        actor.Shadow = !shadow;
        var bytes = SimSavegame.Write(sim);
        actor.Shadow = shadow;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(!shadow, actor.Shadow);
        SimSavegame.Apply(sim, legacy);
        Assert.Equal(shadow, actor.Shadow);
        Assert.Equal(shadow, sim.AddBot(500, 0, type).Shadow);
    }

    [Theory]
    [InlineData(58, true)]
    [InlineData(3004, false)]
    public void ResurrectionRestoresDefaultShadow(int type, bool shadow)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = type }],
        });
        var corpse = sim.Actors.Single();
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Shadow = !shadow;
        Assert.True(ThingRaise.Execute(sim, 17, corpse, 0, 0));
        Assert.Equal(shadow, corpse.Shadow);
    }

    [Fact]
    public void ClassBaselineAffectsChecksumWhenRuntimeShadowIsCleared()
    {
        AuthoritySimulation Room(int bits) => AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }],
        }, dehacked: DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"));
        var first = Room(6); var second = Room(262150);
        first.Actors.Single().Shadow = second.Actors.Single().Shadow = false;
        first.RestoreState(first.CaptureState()); second.RestoreState(second.CaptureState());
        Assert.NotEqual(first.Checksum, second.Checksum);
    }
}
