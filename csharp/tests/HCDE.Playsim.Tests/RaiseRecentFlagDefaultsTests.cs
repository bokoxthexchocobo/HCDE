using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseRecentFlagDefaultsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RaiseRestoresRecentFlagDefaultsButBlockedRaiseLeavesThemIntact(bool blocked)
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.IsMonster);
        actor.Health = 0; actor.States.Enter(actor, ActorStateMachine.Corpse);
        actor.Vulnerable = actor.NoBlood = actor.IsPuff = actor.DontCorpse = actor.DontFall = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, ActorRaiseActions.RaiseSelf(actor));
        Assert.Equal(blocked, actor.Vulnerable); Assert.Equal(blocked, actor.NoBlood);
        Assert.Equal(blocked, actor.IsPuff); Assert.Equal(blocked, actor.DontCorpse); Assert.Equal(blocked, actor.DontFall);
    }

    [Fact]
    public void PatchedNoBloodDefaultIsRestored()
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = 524294\n");
        Assert.Empty(patch.Errors); var sim = Room(patch); var actor = sim.Actors.Single(a => a.DoomEdNum == 3004);
        Assert.True(actor.DefaultNoBlood); actor.NoBlood = false;
        ActorRaise.ReviveSupported(actor); Assert.True(actor.NoBlood);
    }

    [Fact]
    public void NativeLostSoulDontFallDefaultIsRestored()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0, 3006);
        actor.DontFall = false; ActorRaise.ReviveSupported(actor); Assert.True(actor.DontFall);
    }

    [Fact]
    public void SpawnedPuffMarkerDefaultIsRestored()
    {
        var sim = Room(); var puff = sim.SpawnHitscanPuff(200, 0, 20);
        puff.IsPuff = false; ActorRaise.ReviveSupported(puff); Assert.True(puff.IsPuff);
    }

    [Fact]
    public void RaisedDefaultFlagsRoundTripWithoutOldOverrides()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.IsMonster);
        actor.Health = 0; actor.States.Enter(actor, ActorStateMachine.Corpse);
        actor.Vulnerable = actor.NoBlood = actor.IsPuff = actor.DontCorpse = actor.DontFall = true;
        Assert.True(ActorRaiseActions.RaiseSelf(actor)); var saved = SimSavegame.Write(sim, true);
        actor.Vulnerable = actor.NoBlood = actor.IsPuff = actor.DontCorpse = actor.DontFall = true;
        SimSavegame.Apply(sim, saved);
        Assert.False(actor.Vulnerable); Assert.False(actor.NoBlood); Assert.False(actor.IsPuff);
        Assert.False(actor.DontCorpse); Assert.False(actor.DontFall); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room(DehackedPatchResult? patch = null) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }]
    }, dehacked: patch);
}
