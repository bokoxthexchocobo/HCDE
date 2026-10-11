using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileStartSoundTests
{
    [Fact]
    public void AttackEntryFacesAndPlaysStartSoundOnceOnVoiceChannel()
    {
        var (sim, monster) = Room();
        monster.Angle = BamAngle.FromDegrees(180);
        monster.Ambush = true;
        Tick(sim, monster);
        Assert.Equal(MonsterMode.Windup, monster.Brain!.Mode);
        Assert.False(monster.Ambush);
        Assert.Equal(0, monster.Angle.ToDegrees(), 5);
        var sound = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("vile/start", sound.Sound);
        Assert.Equal(monster.Id, sound.ActorId);
        Assert.Equal(2, sound.Channel);
        Assert.Equal(0, sound.Flags);
        Assert.Equal(1, sound.Volume);
        Assert.Equal(1, sound.Attenuation);
        for (var i = 0; i < 9; i++) Tick(sim, monster);
        Assert.DoesNotContain(sim.DrainSoundRequests(), s => s.Sound == "vile/start");
    }

    [Fact]
    public void LoadingMidWindupDoesNotReplayStartSound()
    {
        var (sim, monster) = Room();
        Tick(sim, monster);
        sim.DrainSoundRequests();
        for (var i = 0; i < 5; i++) Tick(sim, monster);
        var bytes = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes);
        monster = sim.Actors.Single(a => a.DoomEdNum == 64);
        Assert.Equal(5, monster.Brain!.CaptureArchvileAttack()!.Value.AttackTic);
        for (var i = 0; i < 4; i++) Tick(sim, monster);
        Assert.DoesNotContain(sim.DrainSoundRequests(), s => s.Sound == "vile/start");
    }

    [Fact]
    public void DisabledMissileAttackDoesNotPlayStartSound()
    {
        var (sim, monster) = Room();
        monster.Brain!.Tick(sim, monster, MonsterChaseFlags.NoPlayActive, useMissile: false);
        Assert.DoesNotContain(sim.DrainSoundRequests(), s => s.Sound == "vile/start");
        Assert.Null(monster.Brain.CaptureArchvileAttack());
    }

    private static void Tick(AuthoritySimulation sim, Actor monster) => monster.Brain!.Tick(sim, monster,
        MonsterChaseFlags.NoDirectionTurn | MonsterChaseFlags.NoPlayActive);

    private static (AuthoritySimulation, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 64 }],
        });
        var monster = sim.Actors.Single(a => a.DoomEdNum == 64);
        monster.ReactionTime = 0;
        return (sim, monster);
    }
}
