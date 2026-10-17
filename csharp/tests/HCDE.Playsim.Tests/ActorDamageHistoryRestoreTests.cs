using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDamageHistoryRestoreTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlayerDeathViewResumesWatchingSavedKiller(bool reconstructKiller)
    {
        var (sim, killer, decoy) = Room(); var player = sim.Players.Single(); player.DontThrust = true;
        ActorDamage.Apply(player, 1000, killer, inflictor: killer);
        for (var i = 0; i < 3; i++) sim.Tick();
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        Assert.Equal(311, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 8).Select(_ => Turn(sim)).ToArray();
        player.LastDamageSourceId = decoy.Id;
        if (reconstructKiller) { killer.Destroy(); sim.Tick(); }
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(killer.Id, player.LastDamageSourceId); Assert.Equal(1, player.DeathCount);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.Equal(expected, Enumerable.Range(0, 8).Select(_ => Turn(sim)).ToArray());
    }

    [Fact]
    public void RestoringDeathCountDoesNotExecuteAnotherDeath()
    {
        var (sim, victim, source) = Room();
        ActorDamage.Apply(victim, 1000, source, inflictor: source);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        ActorRaise.ReviveSupported(victim); ActorDamage.Apply(victim, 1000, source, inflictor: source);
        Assert.Equal(2, victim.DeathCount);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(1, victim.DeathCount); Assert.Equal(source.Id, victim.LastDamageSourceId);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        sim.Tick(); Assert.Equal(1, victim.DeathCount);
    }

    [Fact]
    public void EmptyHistorySnapshotClearsLaterDamageSource()
    {
        var (sim, victim, source) = Room();
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        ActorDamage.Apply(victim, 1, source, inflictor: source);
        Assert.Equal(source.Id, victim.LastDamageSourceId);
        SimSavegame.Apply(sim, bytes);
        Assert.Null(victim.LastDamageSourceId); Assert.Equal(0, victim.DeathCount);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NegativeDeathHistoryIsRejectedBeforeMutation(bool binary)
    {
        var (sim, victim, source) = Room(); ActorDamage.Apply(victim, 1000, source, inflictor: source);
        var before = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        if (binary)
        {
            var malformed = before.ToArray();
            var historyEnd = malformed.Length - 16; // Skip the outer ActorDie random-state trailer.
            var size = BinaryPrimitives.ReadInt32LittleEndian(malformed.AsSpan(historyEnd - 4));
            BinaryPrimitives.WriteInt32LittleEndian(malformed.AsSpan(historyEnd - size + 8), -1);
            Assert.False(SimSavegame.TryRead(malformed, out _, out var error));
            Assert.Equal("save-damage-history-value", error);
        }
        else
        {
            var state = sim.CaptureState(includeBotIdentities: true, includeThinkerOrder: false);
            state.Actors.Single(actor => actor.Id == victim.Id).DamageHistory = new(-1, source.Id);
            Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
            Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        }
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false));
    }

    [Fact]
    public void RemovedDamageSourceIdRoundTripsWithoutInventingAnActor()
    {
        var (sim, victim, source) = Room(); ActorDamage.Apply(victim, 1, source, inflictor: source);
        source.Destroy(); sim.Tick();
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        victim.LastDamageSourceId = null; SimSavegame.Apply(sim, bytes);
        Assert.Equal(source.Id, victim.LastDamageSourceId);
        Assert.DoesNotContain(sim.Actors, actor => actor.Id == source.Id);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void DefaultSnapshotsDoNotAcquireDamageHistoryMetadata()
    {
        var (sim, victim, source) = Room(); ActorDamage.Apply(victim, 1000, source, inflictor: source);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.False(state.IncludesBotIdentities); Assert.All(state.Actors, actor => Assert.Null(actor.DamageHistory));
        Assert.Equal(bytes, SimSavegame.Write(state));
    }

    [Fact]
    public void DamageHistoryRequiresOptInIdentityState()
    {
        var (sim, victim, _) = Room(); var state = sim.CaptureState();
        state.Actors.Single(actor => actor.Id == victim.Id).DamageHistory = new(1, 0);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    private static uint Turn(AuthoritySimulation sim) { sim.Tick(); return sim.Players.Single().Angle.Raw; }

    private static (AuthoritySimulation, BotPawn, BotPawn) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }]
        });
        var killer = sim.AddBot(0, 200, 3001); var decoy = sim.AddBot(0, -200, 3004);
        killer.Brain!.Enabled = decoy.Brain!.Enabled = false;
        return (sim, killer, decoy);
    }
}
