using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BotProfileRestoreTests
{
    [Theory]
    [InlineData(3004, false)]
    [InlineData(3004, true)]
    [InlineData(65, false)]
    [InlineData(65, true)]
    [InlineData(66, false)]
    [InlineData(66, true)]
    [InlineData(64, false)]
    [InlineData(64, true)]
    [InlineData(3006, false)]
    [InlineData(3006, true)]
    [InlineData(71, false)]
    [InlineData(71, true)]
    public void ReplacementProfileSurvivesLiveReplacementAndReconstruction(int profile, bool reconstruct)
    {
        var (sim, bot) = Room(); bot.Brain = MonsterBrain.ForType(profile); Step(sim, bot);
        Assert.Equal(MonsterMode.Windup, bot.Brain!.Mode);
        var id = bot.Id; var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        Assert.True(ArchiveLayerFixture.TrailerStart(bytes, 216) >= 16);
        var count = 32;
        var expected = Enumerable.Range(0, count).Select(_ => Step(sim, bot)).ToArray();
        if (reconstruct) { bot.Destroy(); sim.Tick(); }
        else bot.Brain = new MonsterBrain(MonsterAttack.Melee);
        SimSavegame.Apply(sim, bytes);
        bot = Assert.IsType<BotPawn>(sim.Actors.Single(actor => actor.Id == id));
        Assert.Equal(3001, bot.DoomEdNum); Assert.Equal(profile, bot.Brain!.ProfileType);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.Equal(expected, Enumerable.Range(0, count).Select(_ => Step(sim, bot)).ToArray());
    }

    [Fact]
    public void ClassDefaultSnapshotReplacesLaterForeignProfile()
    {
        var (sim, bot) = Room(); bot.Brain!.Enabled = false;
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        bot.Brain = MonsterBrain.ForType(65); SimSavegame.Apply(sim, bytes);
        Assert.Equal(3001, bot.Brain!.ProfileType); Assert.False(bot.Brain.Enabled);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9999)]
    public void UnknownSerializedProfilesAreRejected(int profile)
    {
        var (sim, bot) = Room(); bot.Brain = MonsterBrain.ForType(65);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), profile);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-bot-profile-value", error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void IncompatibleProfileIdentityIsRejectedBeforeMutation(int kind)
    {
        var (sim, bot) = Room(); bot.Brain = MonsterBrain.ForType(65);
        if (kind == 3) Step(sim, bot);
        var before = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true);
        var pose = state.Actors.Single(actor => actor.Id == bot.Id);
        pose.BotIdentity = kind switch
        {
            0 => pose.BotIdentity!.Value with { ProfileType = 9999 },
            1 => pose.BotIdentity!.Value with { BrainPresent = false },
            2 => pose.BotIdentity!.Value with { BasicAttack = MonsterAttack.Hitscan },
            _ => pose.BotIdentity!.Value with { ProfileType = 3004 }
        };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void MissingOverrideWithForeignAttackIsRejectedBeforeMutation()
    {
        var (sim, bot) = Room(); bot.Brain = MonsterBrain.ForType(65); Step(sim, bot);
        var before = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true);
        var pose = state.Actors.Single(actor => actor.Id == bot.Id);
        pose.BotIdentity = pose.BotIdentity!.Value with { ProfileType = null };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void ProfileMetadataCannotAttachToAPlayer()
    {
        var (sim, bot) = Room(); bot.Brain = MonsterBrain.ForType(65);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 65);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-bot-profile-state", error);
    }

    [Fact]
    public void RecursiveProfileWrapperVersionIsRejected()
    {
        var (sim, bot) = Room(); bot.Brain = MonsterBrain.ForType(65);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size), 211);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-bot-profile-header", error);
    }

    [Fact]
    public void ReplacementProfileCoexistsWithDormancyAndMixedResurrectionWaits()
    {
        var (sim, bot) = Room(); bot.Brain = MonsterBrain.ForType(65); bot.Brain!.Revive(bot); bot.Dormant = true;
        var basic = sim.AddBot(-200, 0, 3001); basic.Brain = new MonsterBrain(MonsterAttack.Fireball);
        basic.Brain.Revive(basic);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        var id = bot.Id; var basicId = basic.Id;
        bot.Destroy(); basic.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        var restored = sim.Actors.Single(actor => actor.Id == id);
        Assert.True(restored.Dormant); Assert.Equal(65, restored.Brain!.ProfileType);
        Assert.Equal(34, restored.Brain.RaiseTics);
        Assert.Equal(34, sim.Actors.Single(actor => actor.Id == basicId).Brain!.RaiseTics);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void MissingPainElementalOverrideIsRejectedBeforeMutation()
    {
        var (sim, bot) = Room(); bot.Brain = MonsterBrain.ForType(71);
        var before = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true);
        var pose = state.Actors.Single(actor => actor.Id == bot.Id);
        pose.BotIdentity = pose.BotIdentity!.Value with { ProfileType = null };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    private static (int?, MonsterMode, int, int, bool, int, int, uint) Step(AuthoritySimulation sim, BotPawn bot)
    {
        bot.Brain!.Tick(sim, bot, MonsterChaseFlags.NoPlayActive | MonsterChaseFlags.DontMove);
        return (bot.Brain.ProfileType, bot.Brain.Mode, bot.Brain.WindupTics, bot.Brain.AttackCooldown,
            bot.Brain.Charging, sim.Players.Single().Health, sim.Actors.OfType<ProjectileActor>().Count(), sim.CombatRandomState);
    }

    private static (AuthoritySimulation, BotPawn) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1, X = 400 }]
        });
        sim.Players.Single().Invulnerable = true;
        var bot = sim.AddBot(0, 0, 3001); bot.ReactionTime = 0;
        return (sim, bot);
    }
}
