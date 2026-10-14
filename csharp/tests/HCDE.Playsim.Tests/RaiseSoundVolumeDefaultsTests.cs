using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseSoundVolumeDefaultsTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void RaiseRestoresMappedAndDynamicSoundFlagDefaults(bool patched, bool dynamicActor)
    {
        var sim = Room(patched); Actor actor = dynamicActor ? sim.AddBot(200, 0) : sim.Actors.Single(a => a.DoomEdNum == 3004);
        actor.Health = 0; actor.States.Enter(actor, ActorStateMachine.Corpse);
        actor.FullVolumeSee = actor.FullVolumeDeath = !patched; actor.FullVolumeActive = true;
        Assert.True(ActorRaiseActions.RaiseSelf(actor));
        Assert.Equal(patched, actor.FullVolumeSee); Assert.Equal(patched, actor.FullVolumeDeath);
        Assert.False(actor.FullVolumeActive);
    }

    [Fact]
    public void BlockedRaisePreservesRuntimeSoundFlags()
    {
        var sim = Room(false); var actor = sim.Actors.Single(a => a.IsMonster);
        actor.Health = 0; actor.States.Enter(actor, ActorStateMachine.Corpse);
        actor.FullVolumeDeath = actor.FullVolumeSee = actor.FullVolumeActive = true; sim.Ceilings[0] = 10;
        Assert.False(ActorRaiseActions.RaiseSelf(actor)); Assert.True(actor.FullVolumeDeath);
        Assert.True(actor.FullVolumeSee); Assert.True(actor.FullVolumeActive);
    }

    [Fact]
    public void RaisedPatchedSoundDefaultsPersistThroughReconstruction()
    {
        var sim = Room(true); var actor = sim.AddBot(200, 0); var id = actor.Id;
        actor.FullVolumeDeath = actor.FullVolumeSee = false; ActorRaise.ReviveSupported(actor);
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        var loaded = sim.Actors.Single(a => a.Id == id);
        loaded.FullVolumeDeath = loaded.FullVolumeSee = false; ActorRaise.ReviveSupported(loaded);
        Assert.True(loaded.FullVolumeDeath); Assert.True(loaded.FullVolumeSee);
    }

    private static AuthoritySimulation Room(bool patched)
    {
        var patch = patched ? DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + COUNTKILL + FULLVOLSOUNDS\n") : null;
        if (patch is not null) Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }]
        }, dehacked: patch);
    }
}
