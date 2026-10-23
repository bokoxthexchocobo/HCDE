namespace HCDE.Playsim.Tests;

public class VmBBotGameTests
{
    [Fact]
    public void LoadBots_SeedsDefaultBots()
    {
        var cajun = new VmBBotGame.CajunMasterState();
        VmBBotGame.LoadBots(cajun, _ => { });
        Assert.NotNull(cajun.BotListHead);
        Assert.Contains("DefaultBot0", EnumerateBotNames(cajun));
    }

    [Fact]
    public void SpawnBot_QueuesPendingAddForNamedBot()
    {
        var cajun = new VmBBotGame.CajunMasterState();
        VmBBotGame.LoadBots(cajun, _ => { });
        var printed = new List<string>();
        Assert.True(VmBBotGame.SpawnBot(cajun, "DefaultBot1", VmBBotGame.NoColor, printed.Add, () => 0, queueNet: true));
        Assert.Single(cajun.PendingAdds);
    }

    [Fact]
    public void SpawnBot_FailsWhenNamedBotMissing()
    {
        var cajun = new VmBBotGame.CajunMasterState();
        VmBBotGame.LoadBots(cajun, _ => { });
        var printed = new List<string>();
        Assert.False(VmBBotGame.SpawnBot(cajun, "NotInCfg", VmBBotGame.NoColor, printed.Add, () => 0, queueNet: true));
        Assert.Empty(cajun.PendingAdds);
        Assert.Contains("couldn't find", printed[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DoAddBot_AddsCajunPlayerSlot()
    {
        var sim = AuthoritySimulation.Start(GameplayFoundationTests.TwoRooms(0, 128).Level, rngSeed: 1);
        var cajun = new VmBBotGame.CajunMasterState();
        VmBBotGame.LoadBots(cajun, _ => { });
        var skill = new VmBBot.BotSkill { Aiming = 2, Perfection = 2, Reaction = 2, Isp = 2 };
        var info = "\\autoaim\\0\\name\\DefaultBot0\\team\\255";
        var printed = new List<string>();
        Assert.True(VmBBotGame.DoAddBot(sim, info, skill, printed.Add));
        Assert.Contains(sim.Players, p => p.CajunBot != null && p.CajunDisplayName == "DefaultBot0");
    }

    [Fact]
    public void Main_DecrementsJoinTimerAndSchedulesSpawn()
    {
        var cajun = new VmBBotGame.CajunMasterState { WantedBotNum = 1, TJoin = 80, BotNum = 0 };
        VmBBotGame.LoadBots(cajun, _ => { });
        cajun.GetSpawned.Add("DefaultBot0");
        for (var i = 0; i < 80; i++)
            VmBBotGame.Main(cajun, false, VmBBot.GsLevel, true, false, _ => { }, () => 0);
        Assert.Single(cajun.PendingAdds);
    }

    [Fact]
    public void RemoveAllBots_ClearsCajunPlayers()
    {
        var sim = AuthoritySimulation.Start(GameplayFoundationTests.TwoRooms(0, 128).Level, rngSeed: 2);
        var cajun = new VmBBotGame.CajunMasterState();
        VmBBotGame.LoadBots(cajun, _ => { });
        VmBBotGame.DoAddBot(sim, "\\name\\DefaultBot0", new VmBBot.BotSkill(), _ => { });
        VmBBotGame.RemoveAllBots(sim, cajun, fromList: true);
        Assert.DoesNotContain(sim.Players, p => p.CajunBot != null);
        Assert.Equal(0, cajun.BotNum);
    }

    static IEnumerable<string> EnumerateBotNames(VmBBotGame.CajunMasterState cajun)
    {
        var node = cajun.BotListHead;
        while (node != null)
        {
            yield return node.Name;
            node = node.Next;
        }
    }
}
