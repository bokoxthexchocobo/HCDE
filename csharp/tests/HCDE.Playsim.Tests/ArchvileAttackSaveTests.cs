using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileAttackSaveTests
{
    [Theory]
    [InlineData(5, false)]
    [InlineData(20, true)]
    public void SaveResumesFireAndBlastStages(int calls, bool fireExists)
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 64);
        sim.Players.Single().Health = 1000; monster.ReactionTime = 0;
        for (var i = 0; i < calls; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        var progress = monster.Brain!.CaptureArchvileAttack(); Assert.NotNull(progress);
        Assert.Equal(fireExists, progress!.Value.FireExists);
        var bytes = SimSavegame.Write(sim); Assert.True(ArchiveLayerFixture.TrailerStart(bytes, 142) >= 16);
        sim.DrainSoundRequests();
        for (var i = calls; i < 70; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        var expected = monster.Brain.CaptureArchvileAttack(); var health = sim.Players.Single().Health;
        var velocity = sim.Players.Single().VelocityZ; var random = sim.CombatRandomState;
        Assert.True(health < 980); Assert.True(velocity.Raw > 0);
        var sounds = sim.DrainSoundRequests().Where(s => s.Sound == "vile/stop").ToArray();
        Assert.Single(sounds);
        SimSavegame.Apply(sim, bytes); monster = sim.Actors.Single(a => a.DoomEdNum == 64);
        sim.DrainSoundRequests();
        Assert.Equal(progress, monster.Brain!.CaptureArchvileAttack());
        for (var i = calls; i < 70; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Equal(expected, monster.Brain.CaptureArchvileAttack()); Assert.Equal(health, sim.Players.Single().Health);
        Assert.Equal(velocity, sim.Players.Single().VelocityZ); Assert.Equal(random, sim.CombatRandomState);
        Assert.Equal(sounds, sim.DrainSoundRequests().Where(s => s.Sound == "vile/stop").ToArray());
        var start = ArchiveLayerFixture.TrailerStart(bytes, 142);
        var state = sim.CaptureState(); var index = state.Actors.FindIndex(a => a.Id == monster.Id);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + index * 28 + 24), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-archvileattack-state", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 100 }, new LevelThing { Type = 64 }] });
}
