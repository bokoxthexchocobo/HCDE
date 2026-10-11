using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseRenderVisibilityTests
{
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1, false, false)]
    [InlineData(2, false, false)]
    [InlineData(0, false, true)]
    [InlineData(1, false, true)]
    [InlineData(2, false, true)]
    [InlineData(0, true, false)]
    [InlineData(1, true, false)]
    [InlineData(2, true, false)]
    [InlineData(0, true, true)]
    [InlineData(1, true, true)]
    [InlineData(2, true, true)]
    public void RaisePreservesRenderVisibilityOnSuccessAndFailureAndSaveReplay(int path, bool blocked, bool invisible)
    {
        var sim = Room(); var corpse = sim.Actors.Single(a => a.DoomEdNum == 3004);
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Invisible = invisible;
        corpse.RenderStyle = ActorRenderStyle.Translucent; corpse.Alpha = 0.25;
        if (blocked) sim.Ceilings[0] = 10;
        var bytes = SimSavegame.Write(sim);
        for (var pass = 0; pass < 2; pass++)
        {
            Assert.Equal(!blocked, Raise(sim, corpse, path));
            Assert.Equal(invisible, corpse.Invisible);
            Assert.Equal(ActorRenderStyle.Translucent, corpse.RenderStyle); Assert.Equal(0.25, corpse.Alpha);
            Assert.Equal(!invisible, new Actor { DontSeekInvisible = true }.CanSeek(corpse));
            var raised = SimSavegame.Write(sim);
            corpse.Invisible = !invisible; corpse.Alpha = 0;
            SimSavegame.Apply(sim, raised);
            Assert.Equal(invisible, corpse.Invisible); Assert.Equal(0.25, corpse.Alpha);
            SimSavegame.Apply(sim, bytes);
            Assert.True(corpse.Corpse); Assert.Equal(invisible, corpse.Invisible);
        }
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    public void RaiseKeepsCurrentVisibilityInsteadOfRestoringPatchedClassValue(int path, bool invisible)
    {
        var sim = Room(DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + TRANSLUC75\n"));
        var corpse = sim.Actors.Single(a => a.DoomEdNum == 3004);
        Assert.True(corpse.Invisible);
        corpse.Invisible = invisible;
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        var bytes = SimSavegame.Write(sim);
        for (var pass = 0; pass < 2; pass++)
        {
            Assert.True(Raise(sim, corpse, path));
            Assert.Equal(invisible, corpse.Invisible);
            Assert.Equal(!invisible, new Actor { DontSeekInvisible = true }.CanSeek(corpse));
            SimSavegame.Apply(sim, bytes);
            Assert.Equal(invisible, corpse.Invisible);
        }
    }

    private static bool Raise(AuthoritySimulation sim, Actor corpse, int path) => path switch
    {
        0 => ActorRaiseActions.RaiseSelf(corpse),
        1 => ThingRaise.Execute(sim, 17, corpse, 0, 0) == true,
        _ => ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single()),
    };

    private static AuthoritySimulation Room(DehackedPatchResult? patch = null) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    }, dehacked: patch);
}
