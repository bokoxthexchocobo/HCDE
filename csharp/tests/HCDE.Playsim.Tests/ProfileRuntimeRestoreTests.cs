using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProfileRuntimeRestoreTests
{
    [Theory]
    [InlineData(3001, false)]
    [InlineData(3001, true)]
    [InlineData(3004, false)]
    [InlineData(3004, true)]
    [InlineData(66, false)]
    [InlineData(66, true)]
    [InlineData(4000, false)]
    [InlineData(4000, true)]
    public void CompletedAttackTimersSurviveLiveAndRemovedBotRestore(int type, bool reconstruct)
    {
        var (sim, bot) = Room(type);
        for (var i = 0; i < 100 && bot.Brain!.AttackCooldown != 1; i++) Step(sim, bot);
        Assert.Equal(1, bot.Brain!.AttackCooldown);
        Assert.Null(bot.Brain.CaptureProfileAttack()); Assert.Null(bot.Brain.CaptureRevenantAttack());
        var runtime = bot.Brain.CaptureProfileChaseHistory()!.Value.Runtime;
        Assert.NotNull(runtime); Assert.Equal(MonsterMode.Chase, runtime.Value.Mode);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true); var id = bot.Id;
        Assert.True(ArchiveLayerFixture.TrailerStart(bytes, 216) >= 16);
        var expected = Enumerable.Range(0, 32).Select(_ => Step(sim, bot)).ToArray();
        if (reconstruct) { bot.Destroy(); sim.Tick(); }
        SimSavegame.Apply(sim, bytes);
        bot = Assert.IsType<BotPawn>(sim.Actors.Single(actor => actor.Id == id));
        Assert.Equal(runtime, bot.Brain!.CaptureProfileChaseHistory()!.Value.Runtime);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.Equal(expected, Enumerable.Range(0, 32).Select(_ => Step(sim, bot)).ToArray());
    }

    [Theory]
    [InlineData(24, 2)]
    [InlineData(28, -1)]
    [InlineData(32, -1)]
    [InlineData(36, -1)]
    public void InvalidRuntimeFieldsAreRejected(int offset, int value)
    {
        var (sim, bot) = Room(3001); Step(sim, bot);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        var start = ArchiveLayerFixture.TrailerStart(bytes, 207);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + 40 + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-profile-chase-value", error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RuntimeCannotContradictSavedAttackBeforeMutation(int field)
    {
        var (sim, bot) = Room(3001); Step(sim, bot);
        var before = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true);
        var pose = state.Actors.Single(actor => actor.Id == bot.Id);
        var history = pose.ProfileChaseHistory!.Value; var runtime = history.Runtime!.Value;
        runtime = field switch
        {
            0 => runtime with { Mode = MonsterMode.Chase },
            1 => runtime with { WindupTics = runtime.WindupTics + 1 },
            _ => runtime with { Cooldown = runtime.Cooldown + 1 }
        };
        pose.ProfileChaseHistory = history with { Runtime = runtime };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void BinaryRuntimeAttackConflictIsRejected()
    {
        var (sim, bot) = Room(3001); Step(sim, bot);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        var start = ArchiveLayerFixture.TrailerStart(bytes, 207);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + 40 + 36), 1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-profile-chase-state", error);
    }

    [Fact]
    public void OlderChaseArchivePreservesExistingAttackTimers()
    {
        var (sim, bot) = Room(3001); Step(sim, bot);
        var expected = bot.Brain!.CaptureProfileAttack(); Assert.NotNull(expected);
        var state = sim.CaptureState(includeBotIdentities: true, includeThinkerOrder: false);
        var pose = state.Actors.Single(actor => actor.Id == bot.Id);
        pose.ProfileChaseHistory = pose.ProfileChaseHistory!.Value with { Runtime = null };
        var legacy = SimSavegame.Write(state);
        Assert.True(ArchiveLayerFixture.TrailerStart(legacy, 206) >= 16);
        Assert.True(SimSavegame.TryRead(legacy, out var parsed, out var error), error);
        Assert.Equal(legacy, SimSavegame.Write(parsed));
        var id = bot.Id; bot.Destroy(); sim.Tick(); SimSavegame.Apply(sim, legacy);
        bot = Assert.IsType<BotPawn>(sim.Actors.Single(actor => actor.Id == id));
        Assert.Equal(expected, bot.Brain!.CaptureProfileAttack());
    }

    [Fact]
    public void IdleSnapshotClearsLaterClassRuntime()
    {
        var (sim, bot) = Room(3001);
        var idle = SimSavegame.Write(sim, includeBotIdentities: true);
        Step(sim, bot); Assert.Equal(MonsterMode.Windup, bot.Brain!.Mode);
        SimSavegame.Apply(sim, idle);
        Assert.Equal(MonsterMode.Idle, bot.Brain.Mode);
        Assert.Equal(0, bot.Brain.WindupTics); Assert.Equal(0, bot.Brain.AttackCooldown);
        Assert.Equal(idle, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    private static (MonsterMode, int, int, int, int, uint) Step(AuthoritySimulation sim, BotPawn bot)
    {
        bot.Brain!.Tick(sim, bot, MonsterChaseFlags.NoPlayActive | MonsterChaseFlags.DontMove);
        return (bot.Brain.Mode, bot.Brain.WindupTics, bot.Brain.AttackCooldown,
            sim.Players.Single().Health, sim.Actors.OfType<ProjectileActor>().Count(), sim.CombatRandomState);
    }

    private static (AuthoritySimulation, BotPawn) Room(int type)
    {
        var patch = type == 4000 ? DehackedPatch.Apply("Thing 2\nID # = 4000\n") : null;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 300 }]
        }, dehacked: patch);
        sim.Players.Single().Invulnerable = true;
        var bot = sim.AddBot(0, 0, type); bot.ReactionTime = 0;
        return (sim, bot);
    }
}
