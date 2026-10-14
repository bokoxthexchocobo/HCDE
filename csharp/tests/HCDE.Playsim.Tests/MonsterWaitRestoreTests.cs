using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MonsterWaitRestoreTests
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
    public void ResurrectionWaitSurvivesLiveAndRemovedBotRestore(bool basic, bool reconstruct, bool dormant)
    {
        var (sim, bot) = RaisedRoom(basic); bot.Dormant = dormant;
        for (var i = 0; i < 5; i++) sim.Tick();
        var remaining = bot.Brain!.RaiseTics; Assert.True(remaining > 0);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true); var id = bot.Id;
        Assert.Equal(216, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 40).Select(_ => Tick(sim, bot)).ToArray();
        if (reconstruct) { bot.Destroy(); sim.Tick(); }
        SimSavegame.Apply(sim, bytes);
        bot = Assert.IsType<BotPawn>(sim.Actors.Single(actor => actor.Id == id));
        Assert.Equal(remaining, bot.Brain!.RaiseTics);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.Equal(expected, Enumerable.Range(0, 40).Select(_ => Tick(sim, bot)).ToArray());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ArchvileHealingWaitSurvivesRestoreAndMixedBrainArchives(bool basicCorpse, bool reconstruct)
    {
        var sim = Room(); var vile = sim.AddBot(0, 0, 64); var corpse = sim.AddBot(50, 0, 3001);
        if (basicCorpse) corpse.Brain = new MonsterBrain(MonsterAttack.Hitscan);
        vile.Brain!.Enabled = false; corpse.Health = 0;
        for (var i = 0; i < 8; i++) sim.Tick();
        vile.Brain.Enabled = true;
        for (var i = 0; i < 10 && vile.Brain.Mode != MonsterMode.Heal; i++) sim.Tick();
        Assert.Equal(MonsterMode.Heal, vile.Brain.Mode); Assert.True(corpse.Health > 0);
        for (var i = 0; i < 5; i++) Chase(sim, vile);
        var runtime = vile.Brain.CaptureProfileChaseHistory()!.Value.Runtime!.Value;
        Assert.Equal(24, runtime.HealTics);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true); var id = vile.Id;
        Assert.Equal(216, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 40).Select(_ => ChaseStep(sim, vile)).ToArray();
        if (reconstruct) { vile.Destroy(); sim.Tick(); }
        SimSavegame.Apply(sim, bytes);
        vile = Assert.IsType<BotPawn>(sim.Actors.Single(actor => actor.Id == id));
        Assert.Equal(runtime, vile.Brain!.CaptureProfileChaseHistory()!.Value.Runtime);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.Equal(expected, Enumerable.Range(0, 40).Select(_ => ChaseStep(sim, vile)).ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OlderIdleSnapshotClearsLaterResurrectionWait(bool basic)
    {
        var sim = Room(); var bot = sim.AddBot(0, 0, 3001);
        if (basic) bot.Brain = new MonsterBrain(MonsterAttack.Melee);
        var before = SimSavegame.Write(sim, includeBotIdentities: true);
        bot.Brain!.Revive(bot); Assert.True(bot.Brain.RaiseTics > 0);
        SimSavegame.Apply(sim, before);
        Assert.Equal(0, bot.Brain.RaiseTics); Assert.Equal(MonsterMode.Idle, bot.Brain.Mode);
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Theory]
    [InlineData(false, 40)]
    [InlineData(false, 44)]
    [InlineData(true, 36)]
    public void NegativeWaitsAreRejected(bool basic, int offset)
    {
        var (sim, _) = RaisedRoom(basic);
        var state = sim.CaptureState(includeBotIdentities: true, includeThinkerOrder: false);
        foreach (var pose in state.Actors) pose.DamageHistory = null;
        var bytes = SimSavegame.Write(state);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var stride = basic ? 40 : 48;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8 + stride + offset), -1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(basic ? "save-basic-attack-value" : "save-profile-chase-value", error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NegativeStructuredWaitsAreRejectedBeforeMutation(int kind)
    {
        var (sim, bot) = RaisedRoom(kind == 2);
        var before = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true);
        var pose = state.Actors.Single(actor => actor.Id == bot.Id);
        if (kind == 2) pose.BasicAttackState = pose.BasicAttackState!.Value with { RaiseTics = -1 };
        else
        {
            var history = pose.ProfileChaseHistory!.Value; var runtime = history.Runtime!.Value;
            pose.ProfileChaseHistory = history with
            { Runtime = kind == 0 ? runtime with { HealTics = -1 } : runtime with { RaiseTics = -1 } };
        }
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BrainlessDormantIdentityDoesNotAcquireABasicBrain(bool reconstruct)
    {
        var sim = Room(); var bot = sim.AddBot(0, 0, 3001); bot.Brain = null; bot.Dormant = true;
        var id = bot.Id; var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        Assert.Equal(210, BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 12)));
        if (reconstruct) { bot.Destroy(); sim.Tick(); } else bot.Dormant = false;
        SimSavegame.Apply(sim, bytes);
        var restored = sim.Actors.Single(actor => actor.Id == id);
        Assert.True(restored.Dormant); Assert.Null(restored.Brain);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false));
    }

    [Fact]
    public void ActiveIdentitySnapshotClearsLaterDormancy()
    {
        var sim = Room(); var bot = sim.AddBot(0, 0, 3001);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true); bot.Dormant = true;
        SimSavegame.Apply(sim, bytes);
        Assert.False(bot.Dormant); Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void UnknownDormancyLifecycleBitsAreRejected()
    {
        var sim = Room(); var bot = sim.AddBot(0, 0); bot.Dormant = true;
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 20), 4096);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-bot-identity-value", error);
    }

    [Fact]
    public void HealingWaitRequiresArchvileBeforeMutation()
    {
        var sim = Room(); var bot = sim.AddBot(0, 0, 3001);
        var before = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true);
        state.Actors.Single(actor => actor.Id == bot.Id).ProfileChaseHistory =
            new(0, 0, 0, 0, 0, new(MonsterMode.Heal, 0, 0, HealTics: 5));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void ActiveAttackCannotAlsoCarryAResurrectionWait()
    {
        var sim = Room(); var bot = sim.AddBot(0, 0, 3001); bot.ReactionTime = 0; Chase(sim, bot);
        var before = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true);
        var pose = state.Actors.Single(actor => actor.Id == bot.Id);
        var history = pose.ProfileChaseHistory!.Value;
        pose.ProfileChaseHistory = history with { Runtime = history.Runtime!.Value with { RaiseTics = 1 } };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    private static (int, MonsterMode, int, int, int, int) Tick(AuthoritySimulation sim, BotPawn bot)
    {
        sim.Tick();
        return (bot.Brain!.RaiseTics, bot.Brain.Mode, bot.Brain.WindupTics, bot.Brain.AttackCooldown,
            bot.X.Raw, bot.Y.Raw);
    }

    private static (MonsterMode, int, int, int) ChaseStep(AuthoritySimulation sim, BotPawn bot)
    {
        Chase(sim, bot);
        return (bot.Brain!.Mode, bot.Brain.WindupTics, bot.Brain.AttackCooldown,
            bot.Brain.CaptureProfileChaseHistory()?.Runtime?.HealTics ?? 0);
    }

    private static void Chase(AuthoritySimulation sim, BotPawn bot) => bot.Brain!.Tick(sim, bot,
        MonsterChaseFlags.NoPlayActive | MonsterChaseFlags.DontMove);

    private static (AuthoritySimulation, BotPawn) RaisedRoom(bool basic)
    {
        var sim = Room(); var bot = sim.AddBot(0, 0, 3001);
        if (basic) bot.Brain = new MonsterBrain(MonsterAttack.Fireball);
        bot.Health = 0; ActorRaise.ReviveSupported(bot); ActorRaise.EnterRaiseState(bot, null);
        return (sim, bot);
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1, X = 400 }],
            Sides = [new LevelSide { Sector = 0 }]
        });
        sim.Players.Single().Invulnerable = true;
        return sim;
    }
}
