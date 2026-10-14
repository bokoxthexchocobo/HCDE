using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpawnedSoulRestoreTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void SpawnedSoulReconstructionPreservesDefaultsChargeAndReferences(bool remap, bool remove, bool established)
    {
        var (sim, parent) = Room(remap);
        var soul = Assert.IsType<Actor>(sim.SpawnLostSoul(parent, sim.Players.Single(), 0));
        parent.MasterId = parent.TracerId = soul.Id;
        if (established) sim.Tick();
        var id = soul.Id; var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        Assert.True(soul.SpawnedLostSoul); Assert.True(soul.Brain!.Charging);
        soul.Destroy(); if (remove) sim.Tick();
        SimSavegame.Apply(sim, bytes);
        var restored = Assert.IsType<Actor>(sim.Actors.Single(actor => actor.Id == id));
        Assert.True(restored.SpawnedLostSoul); Assert.True(restored.Brain!.Charging);
        Assert.Equal(remap ? 4000 : 3006, restored.DoomEdNum);
        Assert.Equal(soul.SpawnDamage, restored.SpawnDamage);
        Assert.Equal(soul.SpawnReactionTime, restored.SpawnReactionTime);
        Assert.Equal(id, parent.MasterId); Assert.Equal(id, parent.TracerId);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void SnapshotBeforeSpawnRewindsMembershipAndActorIds()
    {
        var (sim, parent) = Room();
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        var first = Assert.IsType<Actor>(sim.SpawnLostSoul(parent, null, 0));
        SimSavegame.Apply(sim, bytes);
        Assert.True(first.Destroyed); Assert.DoesNotContain(sim.Actors, actor => actor.SpawnedLostSoul);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.Equal(first.Id, sim.SpawnLostSoul(parent, null, 0)!.Id);
    }

    [Fact]
    public void ReconstructedEstablishedSoulDoesNotRepeatSpawnNotification()
    {
        var (sim, parent) = Room(); var soul = sim.SpawnLostSoul(parent, null, 0)!; sim.Tick();
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        soul.Destroy(); sim.Tick(); var notifications = 0; sim.WorldThingSpawned += _ => notifications++;
        SimSavegame.Apply(sim, bytes); sim.Tick();
        Assert.Equal(0, notifications);
    }

    [Fact]
    public void WrongSpawnClassIsRejectedBeforeMutation()
    {
        var (sim, parent) = Room(); var soul = sim.SpawnLostSoul(parent, null, 0)!;
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true);
        var pose = state.Actors.Single(actor => actor.Id == soul.Id);
        pose.BotIdentity = pose.BotIdentity!.Value with { DoomEdNum = 3001 };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void PainElementalAttackReplaysPastSuccessfulSpawn()
    {
        var (sim, parent) = Room(); parent.Brain!.Enabled = true; parent.ReactionTime = 0;
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        var expected = Enumerable.Range(0, 32).Select(_ => AttackStep(sim, parent)).ToArray();
        Assert.Contains(sim.Actors, actor => actor.SpawnedLostSoul);
        SimSavegame.Apply(sim, bytes);
        Assert.DoesNotContain(sim.Actors, actor => actor.SpawnedLostSoul);
        Assert.Equal(expected, Enumerable.Range(0, 32).Select(_ => AttackStep(sim, parent)).ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedSpawnReconstructsWithoutRepeatingDeath(bool remap)
    {
        var (sim, parent) = Room(remap);
        ((List<LevelLine>)sim.Level.Lines).Add(new LevelLine
        { X1 = 50, X2 = 50, Y1 = -200, Y2 = 200, SideBack = -1 });
        Assert.Null(sim.SpawnLostSoul(parent, null, 0));
        var failed = sim.Actors.Single(actor => actor.SpawnedLostSoul);
        Assert.True(failed.FailedLostSoulSpawn); Assert.True(failed.IsDead);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true); var id = failed.Id;
        failed.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        var restored = sim.Actors.Single(actor => actor.Id == id);
        Assert.True(restored.FailedLostSoulSpawn); Assert.True(restored.IsDead);
        Assert.Equal(1, restored.DeathCount); Assert.Equal(parent.Id, restored.LastDamageSourceId);
        Assert.Null(restored.SpawnDamage); Assert.Null(restored.SpawnReactionTime);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        var deaths = restored.DeathCount; sim.Tick(); Assert.Equal(deaths, restored.DeathCount);
    }

    [Fact]
    public void SnapshotRewindsLaterFailedSpawn()
    {
        var (sim, parent) = Room();
        ((List<LevelLine>)sim.Level.Lines).Add(new LevelLine
        { X1 = 50, X2 = 50, Y1 = -200, Y2 = 200, SideBack = -1 });
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        Assert.Null(sim.SpawnLostSoul(parent, null, 0));
        var id = sim.Actors.Single(actor => actor.SpawnedLostSoul).Id;
        SimSavegame.Apply(sim, bytes);
        Assert.DoesNotContain(sim.Actors, actor => actor.SpawnedLostSoul);
        Assert.Null(sim.SpawnLostSoul(parent, null, 0));
        Assert.Equal(id, sim.Actors.Single(actor => actor.SpawnedLostSoul).Id);
    }

    [Fact]
    public void ReconstructedChargeReplaysPhysicalMovement()
    {
        var (sim, parent) = Room(); var soul = sim.SpawnLostSoul(parent, sim.Players.Single(), 0)!; sim.Tick();
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true); var id = soul.Id;
        var expected = Enumerable.Range(0, 12).Select(_ => MoveStep(sim, soul)).ToArray();
        soul.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        soul = sim.Actors.Single(actor => actor.Id == id);
        Assert.Equal(expected, Enumerable.Range(0, 12).Select(_ => MoveStep(sim, soul)).ToArray());
    }

    private static (int, int, int, int, uint) MoveStep(AuthoritySimulation sim, Actor soul)
    {
        sim.Tick(); return (soul.X.Raw, soul.Y.Raw, soul.Z.Raw, soul.Health, soul.Brain!.StateChecksum);
    }

    private static (MonsterMode, int, string, uint) AttackStep(AuthoritySimulation sim, BotPawn parent)
    {
        parent.Brain!.Tick(sim, parent, MonsterChaseFlags.NoPlayActive | MonsterChaseFlags.DontMove);
        return (parent.Brain.Mode, parent.Brain.WindupTics,
            string.Join(',', sim.Actors.Where(actor => actor.SpawnedLostSoul).Select(actor => actor.Id)), sim.CombatRandomState);
    }

    private static (AuthoritySimulation, BotPawn) Room(bool remap = false)
    {
        var patch = remap ? DehackedPatch.Apply("Thing 19\nID # = 4000\nHit points = 235\nWidth = 786432\nHeight = 2621440\nMass = 123\nMissile damage = 9\nReaction time = 17\n") : null;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1, X = 400 }]
        }, dehacked: patch);
        sim.Players.Single().Invulnerable = true;
        var parent = sim.AddBot(0, 0, 71); parent.Brain!.Enabled = false;
        return (sim, parent);
    }
}
