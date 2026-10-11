using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MancubusRaiseSoundTests
{
    [Fact]
    public void AttackEntryFacesAndPlaysRaiseSoundOnceOnWeaponChannel()
    {
        var (sim, monster) = Room();
        monster.Angle = BamAngle.FromDegrees(180);
        monster.Ambush = true;
        Tick(sim, monster);
        Assert.Equal(MonsterMode.Windup, monster.Brain!.Mode);
        Assert.False(monster.Ambush);
        Assert.Equal(0, monster.Angle.ToDegrees(), 5);
        var sound = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("fatso/raiseguns", sound.Sound);
        Assert.Equal(monster.Id, sound.ActorId);
        Assert.Equal(1, sound.Channel);
        Assert.Equal(0, sound.Flags);
        Assert.Equal(1, sound.Volume);
        Assert.Equal(1, sound.Attenuation);
        for (var i = 0; i < 19; i++) Tick(sim, monster);
        Assert.DoesNotContain(sim.DrainSoundRequests(), s => s.Sound == "fatso/raiseguns");
    }

    [Fact]
    public void LoadingMidWindupDoesNotReplayRaiseSound()
    {
        var (sim, monster) = Room();
        Tick(sim, monster);
        sim.DrainSoundRequests();
        for (var i = 0; i < 5; i++) Tick(sim, monster);
        var bytes = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes);
        monster = sim.Actors.Single(a => a.DoomEdNum == 67);
        Assert.Equal(5, monster.Brain!.CaptureProfileAttack()!.Value.AttackTic);
        for (var i = 0; i < 14; i++) Tick(sim, monster);
        Assert.DoesNotContain(sim.DrainSoundRequests(), s => s.Sound == "fatso/raiseguns");
    }

    [Fact]
    public void DisabledMissileAttackDoesNotPlayRaiseSound()
    {
        var (sim, monster) = Room();
        monster.Brain!.Tick(sim, monster, MonsterChaseFlags.NoPlayActive, useMissile: false);
        Assert.DoesNotContain(sim.DrainSoundRequests(), s => s.Sound == "fatso/raiseguns");
        Assert.Null(monster.Brain.CaptureProfileAttack());
    }

    private static void Tick(AuthoritySimulation sim, Actor monster) => monster.Brain!.Tick(sim, monster,
        MonsterChaseFlags.NoDirectionTurn | MonsterChaseFlags.NoPlayActive);

    private static (AuthoritySimulation, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 67 }],
        });
        var monster = sim.Actors.Single(a => a.DoomEdNum == 67);
        monster.ReactionTime = 0;
        return (sim, monster);
    }
}
