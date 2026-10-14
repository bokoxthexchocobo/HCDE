using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RevenantAttackSaveTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void SaveRetainsMeleeSelectionWithoutReplayingSwing(bool melee, bool leavesRange)
    {
        var sim = Room();
        if (!melee) sim.Players.Single().X = Fixed.FromInt(500);
        var monster = sim.Actors.Single(a => a.DoomEdNum == 66); monster.ReactionTime = 0;
        for (var i = 0; i < 5; i++) ActorChaseActions.ExtChase(monster, melee, true, playActive: false);
        var attack = monster.Brain!.CaptureRevenantAttack();
        Assert.NotNull(attack); Assert.Equal(melee, attack!.Value.SeparateMelee);
        if (leavesRange) sim.Players.Single().X = Fixed.FromInt(500);
        var bytes = SimSavegame.Write(sim); Assert.True(ArchiveLayerFixture.TrailerStart(bytes, 141) >= 16);
        sim.DrainSoundRequests();
        for (var i = 0; i < 8; i++) ActorChaseActions.ExtChase(monster, melee, true, playActive: false);
        var expected = sim.CaptureState(); var health = sim.Players.Single().Health;
        var sounds = sim.DrainSoundRequests(); var progress = monster.Brain.CaptureRevenantAttack();
        var projectiles = sim.Actors.Count(a => a is ProjectileActor);
        SimSavegame.Apply(sim, bytes); sim.DrainSoundRequests(); monster = sim.Actors.Single(a => a.DoomEdNum == 66);
        Assert.Equal(attack, monster.Brain!.CaptureRevenantAttack());
        for (var i = 0; i < 8; i++) ActorChaseActions.ExtChase(monster, melee, true, playActive: false);
        Assert.Equal(progress, monster.Brain.CaptureRevenantAttack()); Assert.Equal(health, sim.Players.Single().Health);
        Assert.Equal(expected.SkelFistRandomState, sim.CaptureState().SkelFistRandomState);
        Assert.Equal(expected.CombatRandomState, sim.CaptureState().CombatRandomState);
        Assert.Equal(projectiles, sim.Actors.Count(a => a is ProjectileActor));
        Assert.Equal(sounds, sim.DrainSoundRequests()); Assert.DoesNotContain(sounds, r => r.Sound == "skeleton/swing");
        Assert.Equal(melee && !leavesRange, health < 100);
        Assert.Equal(melee ? 0 : 1, projectiles);
        var start = ArchiveLayerFixture.TrailerStart(bytes, 141);
        var index = expected.Actors.FindIndex(a => a.Id == monster.Id);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + index * 28 + 24), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-revenantattack-state", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 40 }, new LevelThing { Type = 66 }] });
}
