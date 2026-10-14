using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BasicAttackRestoreTests
{
    [Theory]
    [InlineData(MonsterAttack.Melee, false, false)]
    [InlineData(MonsterAttack.Melee, false, true)]
    [InlineData(MonsterAttack.Melee, true, false)]
    [InlineData(MonsterAttack.Melee, true, true)]
    [InlineData(MonsterAttack.Hitscan, false, false)]
    [InlineData(MonsterAttack.Hitscan, false, true)]
    [InlineData(MonsterAttack.Hitscan, true, false)]
    [InlineData(MonsterAttack.Hitscan, true, true)]
    [InlineData(MonsterAttack.Fireball, false, false)]
    [InlineData(MonsterAttack.Fireball, false, true)]
    [InlineData(MonsterAttack.Fireball, true, false)]
    [InlineData(MonsterAttack.Fireball, true, true)]
    public void SavedWindupAndCooldownReplayAttackTiming(MonsterAttack attack, bool cooldown, bool reconstruct)
    {
        var (sim, bot) = Room(attack);
        for (var i = 0; i < (cooldown ? 12 : 4); i++) Chase(sim, bot);
        Assert.Equal(cooldown ? 0 : 5, bot.Brain!.WindupTics);
        Assert.Equal(cooldown ? 22 : 0, bot.Brain.AttackCooldown);
        var id = bot.Id; var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        Assert.True(ArchiveLayerFixture.TrailerStart(bytes, 216) >= 16);
        var expected = Enumerable.Range(0, 32).Select(_ => Step(sim, bot)).ToArray();
        if (reconstruct) { bot.Destroy(); sim.Tick(); }
        SimSavegame.Apply(sim, bytes);
        bot = Assert.IsType<BotPawn>(sim.Actors.Single(actor => actor.Id == id));
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        var actual = Enumerable.Range(0, 32).Select(_ => Step(sim, bot)).ToArray();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(4, -1)]
    [InlineData(8, -1)]
    [InlineData(12, -1)]
    public void MalformedRuntimeFieldsAreRejected(int offset, int value)
    {
        var (sim, bot) = Room(MonsterAttack.Hitscan); Chase(sim, bot);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        var start = ArchiveLayerFixture.TrailerStart(bytes, 205);
        // The player is the first actor; the bot occupies the second table entry.
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + 36 + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-basic-attack-value", error);
    }

    [Fact]
    public void IdleIdentitySnapshotClearsLaterAttackTimers()
    {
        var (sim, bot) = Room(MonsterAttack.Fireball);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        Chase(sim, bot); Assert.True(bot.Brain!.WindupTics > 0);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(MonsterMode.Idle, bot.Brain.Mode);
        Assert.Equal(0, bot.Brain.WindupTics); Assert.Equal(0, bot.Brain.AttackCooldown);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void RuntimeStateRequiresBasicBrainIdentityBeforeMutation()
    {
        var (sim, bot) = Room(MonsterAttack.Melee);
        var before = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true);
        state.Actors.Single(actor => actor.Id == bot.Id).BotIdentity = new(3001, 0);
        state.Actors.Single(actor => actor.Id == bot.Id).BasicAttackState = new(MonsterMode.Windup, 5, 0);
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    private static (MonsterMode, int, int, int, int, uint) Step(AuthoritySimulation sim, BotPawn bot)
    {
        Chase(sim, bot);
        return (bot.Brain!.Mode, bot.Brain.WindupTics, bot.Brain.AttackCooldown,
            sim.Players.Single().Health, sim.Actors.OfType<ProjectileActor>().Count(), sim.CombatRandomState);
    }

    private static void Chase(AuthoritySimulation sim, BotPawn bot) => bot.Brain!.Tick(sim, bot,
        MonsterChaseFlags.NoPlayActive | MonsterChaseFlags.DontMove);

    private static (AuthoritySimulation, BotPawn) Room(MonsterAttack attack)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = attack == MonsterAttack.Melee ? 40 : 200 }]
        });
        var bot = sim.AddBot(0, 0, 3001);
        bot.Brain = new MonsterBrain(attack); bot.ReactionTime = 0;
        bot.MeleeRange = Fixed.FromInt(64);
        return (sim, bot);
    }
}
