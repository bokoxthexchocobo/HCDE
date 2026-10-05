using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomDecorationBrightnessTests
{
    [Theory]
    [InlineData(85)]
    [InlineData(86)]
    [InlineData(2028)]
    [InlineData(41)]
    [InlineData(42)]
    [InlineData(44)]
    [InlineData(45)]
    [InlineData(46)]
    [InlineData(55)]
    [InlineData(56)]
    [InlineData(57)]
    [InlineData(34)]
    [InlineData(35)]
    [InlineData(29)]
    [InlineData(70)]
    public void NativeBrightDecorationsRemainBrightUntilCrushed(int type)
    {
        var sim = Room(type); var actor = sim.Actors.Single();
        Assert.True(actor.FullBright);
        for (var i = 0; i < 40; i++) { sim.Tick(); Assert.True(actor.FullBright); }
        actor.States.Enter(actor, actor.GenericCrushState, noFunction: true);
        Assert.False(actor.FullBright);
        actor.States.Enter(actor, actor.SpawnState, noFunction: true);
        Assert.True(actor.FullBright);
        actor.States.Enter(actor, actor.NullState);
        Assert.False(actor.FullBright);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(36)]
    [InlineData(49)]
    [InlineData(24)]
    public void OrdinaryDecorationEntryClearsBrightness(int type)
    {
        var actor = Room(type).Actors.Single(); actor.FullBright = true;
        actor.States.Enter(actor, actor.SpawnState);
        Assert.False(actor.FullBright);
    }

    [Fact]
    public void SaveRestoresBrightAnimationAndOrdinaryCrushState()
    {
        var sim = Room(85); var actor = sim.Actors.Single();
        sim.Tick();
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        var restored = Room(85); restored.RestoreState(state);
        Assert.True(restored.Actors.Single().FullBright);
        actor.States.Enter(actor, actor.GenericCrushState);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out state, out error), error);
        restored.RestoreState(state);
        Assert.False(restored.Actors.Single().FullBright);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 160 }], Things = [new LevelThing { Type = type }],
    });
}
