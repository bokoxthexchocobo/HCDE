using HCDE.Gamedata;
using HCDE.MapLoader;
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
    public void CcmdRegister_OnlyPortsFourCommands()
    {
        var table = new Dictionary<string, Action<VmBBotCcmd.CcmdHost>>();
        VmBBotCcmdRegister.Register(table);
        Assert.Equal(4, table.Count);
        Assert.True(table.ContainsKey("addbot"));
        Assert.True(table.ContainsKey("removebots"));
        Assert.True(table.ContainsKey("freeze"));
        Assert.True(table.ContainsKey("listbots"));
        Assert.DoesNotContain("bot_next_color", table.Keys);
    }

    [Fact]
    public void AddBot_RejectsWhenNotInLevel()
    {
        var host = new VmBBotCcmd.CcmdHost { GameState = 0, Argc = 1 };
        VmBBotCcmd.RunAddBotCcmd(host);
        Assert.Contains("not in a game", host.Printed[0]);
        Assert.Null(host.SpawnedBotName);
    }

    [Fact]
    public void AddBot_SpawnsWhenAllowed()
    {
        var host = new VmBBotCcmd.CcmdHost
        {
            GameState = VmBBot.GsLevel,
            CanControlSettings = true,
            Argc = 2,
            Arg1 = "Cajun",
        };
        VmBBotCcmd.RunAddBotCcmd(host);
        Assert.Equal("Cajun", host.SpawnedBotName);
    }

    [Fact]
    public void ClearPlayer_DestroysBotAndClearsInGame()
    {
        var mo = new Actor();
        var bot = new VmBBot.BotState();
        var slot = new VmBBot.PlayerSlot { Mo = mo, Bot = bot, UserName = "A", Team = 3, InGame = true };
        var head = new VmBBot.BotInfoNode { Name = "A", InUse = VmBBot.BotInuseYes };
        VmBBot.ClearPlayer(slot, head, false);
        Assert.Null(slot.Mo);
        Assert.Null(slot.Bot);
        Assert.False(slot.InGame);
        Assert.Equal(VmBBot.BotInuseNo, head.InUse);
        Assert.Equal(VmBBot.TeamNone, head.LastTeam);
        Assert.True(mo.Destroyed);
    }

    [Fact]
    public void RemoveBots_WritesKillWhenAllowed()
    {
        var host = new VmBBotCcmd.CcmdHost { CanControlSettings = true };
        VmBBotCcmd.RunRemoveBotsCcmd(host);
        Assert.Equal([VmBBot.DemKillbots], host.NetWrites);
    }

    [Fact]
    public void Freeze_WritesCheatWhenAllowed()
    {
        var host = new VmBBotCcmd.CcmdHost { NetGame = true, CanControlSettings = true };
        VmBBotCcmd.RunFreezeCcmd(host);
        Assert.Equal([VmBBot.DemGenericcheat, VmBBot.ChtFreeze], host.NetWrites);
    }

    [Fact]
    public void ListBots_PrintsActiveSuffix()
    {
        var host = new VmBBotCcmd.CcmdHost
        {
            BotListHead = new VmBBot.BotInfoNode { Name = "x", InUse = VmBBot.BotInuseYes },
        };
        VmBBotCcmd.RunListBotsCcmd(host);
        Assert.Contains("(active)", host.Printed[0]);
        Assert.Contains("> 1 bots", host.Printed[1]);
    }

    [Fact]
    public void InitBotStuff_ParsesWeaponAndFlags()
    {
        var info = new Dictionary<string, VmBBot.BotWeaponData>();
        var flags = new Dictionary<string, int> { ["PlasmaBall"] = 0 };
        VmBBot.InitBotStuffFromLumps(
            [["RocketLauncher", ",", "128", ",", "BOT_EXPLOSIVE"]],
            info,
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
                new Dictionary<string, int>()));
    }

    [Fact]
    public void ParseBotSupp_UnknownProjectileTokenThrows()
    {
        Assert.Throws<InvalidOperationException>(() =>
            VmBBot.ParseBotSuppWeaponLine(
                "Pistol",
                ["Pistol", ",", "10", ",", "MissingClass"],
                new Dictionary<string, VmBBot.BotWeaponData>()));
    }
}
