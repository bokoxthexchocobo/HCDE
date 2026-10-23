using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmBBotTests
{
    [Fact]
    public void Clear_ZeroesPointersAndTimers()
    {
        var bot = new VmBBot.BotState
        {
            Player = new object(),
            Dest = new Actor(),
            Enemy = new Actor(),
            TActive = 9,
            FirstShot = false,
        };
        bot.Skill.Aiming = 5;
        VmBBot.Clear(bot);
        Assert.Null(bot.Player);
        Assert.Null(bot.Dest);
        Assert.Null(bot.Enemy);
        Assert.Equal(0, bot.TActive);
        Assert.True(bot.FirstShot);
        Assert.Equal(0, bot.Skill.Aiming);
    }

    [Fact]
    public void WriteBotSnapshot_RecordsAngleAndSkill()
    {
        var bot = new VmBBot.BotState { Angle = 1.5 };
        bot.Skill.Reaction = 42;
        var keys = new List<string>();
        var values = new List<object?>();
        VmBBot.WriteBotSnapshot(bot, keys, values);
        Assert.Contains("angle", keys);
        Assert.Contains("skill", keys);
    }

    [Fact]
    public void Tick_SkipsWhenMoNull()
    {
        var bot = new VmBBot.BotState();
        var called = false;
        VmBBot.Tick(bot, null, false, () => called = true);
        Assert.False(called);
    }

    [Fact]
    public void Tick_SkipsWhenFrozen()
    {
        var bot = new VmBBot.BotState();
        var called = false;
        VmBBot.Tick(bot, new Actor(), true, () => called = true);
        Assert.False(called);
    }

    [Fact]
    public void Tick_InvokesThinkWhenActive()
    {
        var bot = new VmBBot.BotState();
        var called = false;
        VmBBot.Tick(bot, new Actor(), false, () => called = true);
        Assert.True(called);
    }

    [Fact]
    public void AddBot_RejectsWhenNotInLevel()
    {
        var lines = new List<string>();
        var ok = VmBBot.TryAddBot(0, true, 1, null, _ => { }, s => lines.Add(s));
        Assert.False(ok);
        Assert.Contains("not in a game", lines[0]);
    }

    [Fact]
    public void AddBot_SpawnsWhenAllowed()
    {
        string? spawned = "unset";
        var ok = VmBBot.TryAddBot(VmBBot.GsLevel, true, 2, "Cajun", n => spawned = n, _ => { });
        Assert.True(ok);
        Assert.Equal("Cajun", spawned);
    }

    [Fact]
    public void ClearPlayer_DestroysBotAndClearsInGame()
    {
        var mo = new Actor();
        var bot = new VmBBot.BotState();
        var slot = new VmBBot.PlayerSlot { Mo = mo, Bot = bot, UserName = "A", Team = 3, InGame = true };
        var head = new VmBBot.BotInfoNode { Name = "A", InUse = VmBBot.BotInuseYes };
        VmBBot.ClearPlayer(slot, head, 0, false, _ => { }, _ => { }, () => { });
        Assert.Null(slot.Mo);
        Assert.Null(slot.Bot);
        Assert.False(slot.InGame);
        Assert.Equal(VmBBot.BotInuseNo, head.InUse);
        Assert.Equal(VmBBot.TeamNone, head.LastTeam);
    }

    [Fact]
    public void RemoveBots_WritesKillWhenAllowed()
    {
        var wrote = false;
        Assert.True(VmBBot.TryRemoveBots(true, _ => { }, () => wrote = true));
        Assert.True(wrote);
    }

    [Fact]
    public void Freeze_WritesCheatWhenAllowed()
    {
        var wrote = false;
        Assert.True(VmBBot.TryFreeze(false, true, true, _ => { }, () => wrote = true));
        Assert.True(wrote);
    }

    [Fact]
    public void ListBots_PrintsActiveSuffix()
    {
        var lines = new List<string>();
        var head = new VmBBot.BotInfoNode { Name = "x", InUse = VmBBot.BotInuseYes };
        Assert.Equal(1, VmBBot.ListBots(head, lines.Add));
        Assert.Contains("(active)", lines[0]);
    }

    [Fact]
    public void InitBotStuff_ParsesWeaponAndFlags()
    {
        var info = new Dictionary<string, VmBBot.BotWeaponData>();
        var flags = new Dictionary<string, int> { ["PlasmaBall"] = 0 };
        VmBBot.InitBotStuffFromLumps(
            [["RocketLauncher", ",", "128", ",", "BOT_EXPLOSIVE"]],
            info,
            _ => true,
            n => n,
            flags);
        Assert.Equal(128, info["RocketLauncher"].MoveCombatDist);
        Assert.Equal(VmBBot.BifBotExplosive, info["RocketLauncher"].Flags);
        Assert.Equal(VmBBot.Mf3Warnbot, flags["PlasmaBall"]);
    }

    [Fact]
    public void InitBotStuff_RejectsNonWeaponType()
    {
        var info = new Dictionary<string, VmBBot.BotWeaponData>();
        Assert.Throws<InvalidOperationException>(() =>
            VmBBot.InitBotStuffFromLumps(
                [["NotAWeapon", ",", "1"]],
                info,
                _ => false,
                _ => null,
                new Dictionary<string, int>()));
    }

    [Fact]
    public void ParseBotSupp_UnknownProjectileTokenThrows()
    {
        Assert.Throws<InvalidOperationException>(() =>
            VmBBot.ParseBotSuppWeaponLine(
                "Pistol",
                ["Pistol", ",", "10", ",", "MissingClass"],
                new Dictionary<string, VmBBot.BotWeaponData>(),
                _ => true,
                _ => null));
    }
}
