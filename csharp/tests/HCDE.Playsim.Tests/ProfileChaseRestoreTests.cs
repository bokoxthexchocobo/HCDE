using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProfileChaseRestoreTests
{
    [Theory]
    [InlineData(3001, false)]
    [InlineData(3001, true)]
    [InlineData(3004, false)]
    [InlineData(3004, true)]
    [InlineData(66, false)]
    [InlineData(66, true)]
    [InlineData(65, false)]
    [InlineData(65, true)]
    [InlineData(4000, false)]
    [InlineData(4000, true)]
    public void ClassChaseDecisionsSurviveOcclusionAndReconstruction(int type, bool reconstruct)
    {
        var (sim, bot) = Room(type); var player = sim.Players.Single();
        Assert.True(CombatTrace.HasLineOfSight(sim, bot, player));
        for (var i = 0; i < 4; i++) Chase(sim, bot);
        player.Y = Fixed.FromInt(0);
        Assert.False(CombatTrace.HasLineOfSight(sim, bot, player));
        var history = bot.Brain!.CaptureProfileChaseHistory()!.Value;
        Assert.True(history.BlockedTics > 2);
        Assert.Equal(Fixed.FromDouble(400.75).Raw, history.LastSeenY);
        var id = bot.Id; var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        Assert.Equal(216, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 8).Select(_ => Step(sim, bot)).ToArray();
        player.X = Fixed.FromInt(-200); player.Y = Fixed.FromInt(-300); Chase(sim, bot);
        if (reconstruct) { bot.Destroy(); sim.Tick(); }
        SimSavegame.Apply(sim, bytes);
        bot = Assert.IsType<BotPawn>(sim.Actors.Single(actor => actor.Id == id));
        Assert.Equal(history, bot.Brain!.CaptureProfileChaseHistory());
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.Equal(expected, Enumerable.Range(0, 8).Select(_ => Step(sim, bot)).ToArray());
    }

    [Fact]
    public void OlderIdentityArchiveClearsLaterClassChaseHistory()
    {
        var (sim, bot) = Room(3001); var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        Assert.Equal(202, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Chase(sim, bot); Assert.NotNull(bot.Brain!.CaptureProfileChaseHistory());
        SimSavegame.Apply(sim, bytes);
        Assert.Null(bot.Brain.CaptureProfileChaseHistory());
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false));
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(4, -1)]
    public void MalformedClassChaseFieldsAreRejected(int offset, int value)
    {
        var (sim, bot) = Room(3001); Chase(sim, bot);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8 + 40 + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-profile-chase-value", error);
    }

    [Fact]
    public void UnsupportedClassHistoryIsRejectedBeforeMutation()
    {
        var (sim, bot) = Room(9999);
        var before = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true);
        var pose = state.Actors.Single(actor => actor.Id == bot.Id);
        pose.BotIdentity = new(9999, 0);
        pose.ProfileChaseHistory = new(1, 0, 0, 0, 0);
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void InvalidHistoryPairingsAreRejectedBeforeMutation(int kind)
    {
        var (sim, bot) = Room(3001); Chase(sim, bot);
        var before = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true);
        var pose = state.Actors.Single(actor => actor.Id == bot.Id);
        if (kind == 0) pose.BotIdentity = new(3001, 0, BrainPresent: false);
        else if (kind == 1) pose.BotIdentity = new(3001, 0, BasicAttack: MonsterAttack.Melee);
        else pose.ProfileChaseHistory = pose.ProfileChaseHistory!.Value with { BlockedTics = -1 };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void AbsentHistoryEntryMustHaveZeroPayload()
    {
        var (sim, bot) = Room(3001); Chase(sim, bot);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8 + 20), 1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-profile-chase-value", error);
    }

    [Theory]
    [InlineData(3001)]
    [InlineData(65)]
    public void ChaseHistoryCoexistsWithRestoredAttackSequence(int type)
    {
        var (sim, bot) = Room(type);
        for (var i = 0; i < 4; i++) Chase(sim, bot);
        bot.Brain!.Tick(sim, bot, MonsterChaseFlags.NoPlayActive | MonsterChaseFlags.DontMove);
        Assert.Equal(MonsterMode.Windup, bot.Brain.Mode);
        var id = bot.Id; var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        var expected = Enumerable.Range(0, 32).Select(_ => AttackStep(sim, bot)).ToArray();
        bot.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        bot = Assert.IsType<BotPawn>(sim.Actors.Single(actor => actor.Id == id));
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.Equal(expected, Enumerable.Range(0, 32).Select(_ => AttackStep(sim, bot)).ToArray());
    }

    private static (MonsterMode, int, int, int, int, uint, SimProfileChaseHistory?) AttackStep(AuthoritySimulation sim, BotPawn bot)
    {
        bot.Brain!.Tick(sim, bot, MonsterChaseFlags.NoPlayActive | MonsterChaseFlags.DontMove);
        return (bot.Brain.Mode, bot.Brain.WindupTics, bot.Brain.AttackCooldown, sim.Players.Single().Health,
            sim.Actors.OfType<ProjectileActor>().Count(), sim.CombatRandomState, bot.Brain.CaptureProfileChaseHistory());
    }

    private static (int, int, uint, SimProfileChaseHistory?) Step(AuthoritySimulation sim, BotPawn bot)
    {
        Chase(sim, bot);
        return (bot.VelocityX.Raw, bot.VelocityY.Raw, bot.Angle.Raw, bot.Brain!.CaptureProfileChaseHistory());
    }

    private static void Chase(AuthoritySimulation sim, BotPawn bot) => bot.Brain!.Tick(sim, bot,
        MonsterChaseFlags.NoPlayActive, useMelee: false, useMissile: false);

    private static (AuthoritySimulation, BotPawn) Room(int type)
    {
        var patch = type == 4000 ? DehackedPatch.Apply("Thing 2\nID # = 4000\n") : null;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
            Lines = [new LevelLine { X1 = 100, Y1 = -128, X2 = 100, Y2 = 128, SideBack = -1 }]
        }, dehacked: patch);
        var player = sim.Players.Single(); player.X = Fixed.FromDouble(200.125); player.Y = Fixed.FromDouble(400.75);
        var bot = sim.AddBot(16.25, -8.5, type); bot.ReactionTime = 0;
        return (sim, bot);
    }
}
