using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseAttackControlTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MeleeEntryEmitsConfiguredAttackSoundOnlyForMeleeAdmission(bool melee)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 40 }, new LevelThing { Type = 66 }] });
        var monster = sim.Actors.Single(a => a.DoomEdNum == 66); monster.ReactionTime = 0;
        monster.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "attack" };
        ActorChaseActions.ExtChase(monster, melee, true, playActive: false);
        if (!melee) { Assert.Empty(sim.PendingSoundRequests); return; }
        var request = Assert.Single(sim.DrainSoundRequests(), r => r.Sound == "attack");
        Assert.Equal("attack", request.Sound); Assert.Equal(1, request.Channel);
        Assert.Equal(0, request.Flags); Assert.Equal(1, request.Volume); Assert.Equal(1, request.Attenuation);
        ActorChaseActions.ExtChase(monster, true, true, playActive: false);
        Assert.Empty(sim.PendingSoundRequests);
    }

    [Theory]
    [InlineData(3004, true, false, false)]
    [InlineData(9, true, false, false)]
    [InlineData(3002, false, true, false)]
    [InlineData(3002, true, false, true)]
    [InlineData(3004, false, true, true)]
    [InlineData(66, true, false, true)]
    public void ExtendedChaseRequiresAnAvailableEnabledAttackState(int type, bool melee, bool missile, bool attacks)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 40 }, new LevelThing { Type = type }] });
        var monster = sim.Actors.Single(a => a.DoomEdNum == type); monster.ReactionTime = 0;
        ActorChaseActions.ExtChase(monster, melee, missile, playActive: false);
        Assert.Equal(attacks ? MonsterMode.Windup : MonsterMode.Chase, monster.Brain!.Mode);
        Assert.Equal(100, sim.Players.Single().Health);
    }

    [Theory]
    [InlineData(true, true, 40, true)]
    [InlineData(true, false, 40, true)]
    [InlineData(false, true, 40, true)]
    [InlineData(false, false, 40, false)]
    [InlineData(true, false, 500, false)]
    [InlineData(false, true, 500, true)]
    public void AttackControlsSelectPermittedBranches(bool melee, bool missile, int distance, bool attacks)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = distance }, new LevelThing { Type = 3001 }] });
        var monster = sim.Actors.Single(a => a.DoomEdNum == 3001); monster.ReactionTime = 0;
        ActorChaseActions.ExtChase(monster, melee, missile, playActive: false);
        Assert.Equal(attacks ? MonsterMode.Windup : MonsterMode.Chase, monster.Brain!.Mode);
        Assert.Null(sim.CaptureState().ChaseRandomState);
    }
}
