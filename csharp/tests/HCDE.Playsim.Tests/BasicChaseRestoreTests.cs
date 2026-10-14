using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BasicChaseRestoreTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OccludedTargetAndBlockedSteeringReplayAfterRestore(bool reconstruct)
    {
        var (sim, bot) = Room(); var player = sim.Players.Single();
        Assert.True(CombatTrace.HasLineOfSight(sim, bot, player));
        for (var i = 0; i < 4; i++) Chase(sim, bot);
        player.Y = Fixed.FromInt(0);
        Assert.False(CombatTrace.HasLineOfSight(sim, bot, player));
        var saved = bot.Brain!.CaptureBasicAttackState()!.Value;
        Assert.True(saved.BlockedTics > 2);
        Assert.Equal(Fixed.FromDouble(16.25).Raw, saved.PreviousX);
        Assert.Equal(Fixed.FromDouble(-8.5).Raw, saved.PreviousY);
        Assert.Equal(Fixed.FromDouble(400.75).Raw, saved.LastSeenY);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true); var id = bot.Id;
        Assert.Equal(216, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 8).Select(_ => Step(sim, bot)).ToArray();
        player.X = Fixed.FromInt(-200); player.Y = Fixed.FromInt(-300);
        Chase(sim, bot);
        Assert.NotEqual(saved.LastSeenY, bot.Brain.CaptureBasicAttackState()!.Value.LastSeenY);
        if (reconstruct) { bot.Destroy(); sim.Tick(); }
        SimSavegame.Apply(sim, bytes);
        bot = Assert.IsType<BotPawn>(sim.Actors.Single(actor => actor.Id == id));
        Assert.Equal(saved, bot.Brain!.CaptureBasicAttackState());
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.False(CombatTrace.HasLineOfSight(sim, bot, player));
        Assert.Equal(expected, Enumerable.Range(0, 8).Select(_ => Step(sim, bot)).ToArray());
    }

    [Fact]
    public void OlderTimerArchiveResetsLaterSteeringHistory()
    {
        var (sim, bot) = Room();
        bot.Brain!.RestoreBasicAttackState(new(MonsterMode.Windup, 3, 0));
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        Assert.Equal(204, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Chase(sim, bot);
        Assert.NotEqual(0, bot.Brain.CaptureBasicAttackState()!.Value.LastSeenY);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(new SimBasicAttackState(MonsterMode.Windup, 3, 0), bot.Brain.CaptureBasicAttackState());
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NegativeBlockedCounterIsRejectedBeforeMutation(bool archive)
    {
        var (sim, bot) = Room(); Chase(sim, bot);
        var before = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        if (archive)
        {
            var bytes = before.ToArray();
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8 + 36 + 16), -1);
            Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
            Assert.Equal("save-basic-attack-value", error);
        }
        else
        {
            var state = sim.CaptureState(includeBotIdentities: true, includeThinkerOrder: false);
            var pose = state.Actors.Single(actor => actor.Id == bot.Id);
            pose.BasicAttackState = pose.BasicAttackState!.Value with { BlockedTics = -1 };
            Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
            Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        }
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false));
    }

    [Fact]
    public void AbsentActorCannotCarryHiddenSteeringData()
    {
        var (sim, bot) = Room(); Chase(sim, bot);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        // The player has no brain state, so its absent record must contain only zeroes.
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8 + 28), 1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-basic-attack-value", error);
    }

    private static (int, int, uint, SimBasicAttackState?) Step(AuthoritySimulation sim, BotPawn bot)
    {
        Chase(sim, bot);
        return (bot.VelocityX.Raw, bot.VelocityY.Raw, bot.Angle.Raw, bot.Brain!.CaptureBasicAttackState());
    }

    private static void Chase(AuthoritySimulation sim, BotPawn bot) => bot.Brain!.Tick(sim, bot,
        MonsterChaseFlags.NoPlayActive, useMelee: false, useMissile: false);

    private static (AuthoritySimulation, BotPawn) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
            Lines = [new LevelLine { X1 = 100, Y1 = -128, X2 = 100, Y2 = 128, SideBack = -1 }]
        });
        var player = sim.Players.Single(); player.X = Fixed.FromDouble(200.125); player.Y = Fixed.FromDouble(400.75);
        var bot = sim.AddBot(16.25, -8.5, 3001);
        bot.Brain = new MonsterBrain(MonsterAttack.Melee); bot.ReactionTime = 0;
        return (sim, bot);
    }
}
