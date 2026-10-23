namespace HCDE.Playsim;

/// <summary>CCMD bodies from <c>b_bot.cpp</c> with direct console registration (no generic callback seams).</summary>
public static class VmBBotCcmd
{
    public sealed class CcmdHost
    {
        public int GameState { get; set; }
        public bool CanControlSettings { get; set; }
        public int Argc { get; set; }
        public string? Arg1 { get; set; }
        public bool CheatMode { get; set; }
        public bool NetGame { get; set; }
        public VmBBot.BotInfoNode? BotListHead { get; set; }
        public VmBBotGame.CajunMasterState? Cajun { get; set; }
        public List<string> Printed { get; } = new();
        public string? SpawnedBotName { get; set; }
        public List<int> NetWrites { get; } = new();

        public void Printf(string message) => Printed.Add(message);

        public void SpawnBot(string? name) => SpawnedBotName = name;

        public void NetWriteInt8(int value) => NetWrites.Add(value);
    }

    public static void RunAddBotCcmd(CcmdHost host)
    {
        if (host.GameState != VmBBot.GsLevel)
        {
            host.Printf("Bots cannot be added when not in a game!\n");
            return;
        }

        if (!host.CanControlSettings)
        {
            host.Printf("Only setting controllers can add bots\n");
            return;
        }

        if (host.Argc > 2)
        {
            host.Printf("addbot [botname] : add a bot to the game\n");
            return;
        }

        var name = host.Argc > 1 ? host.Arg1 : null;
        if (host.Cajun != null)
        {
            VmBBotGame.SpawnBot(host.Cajun, name, VmBBotGame.NoColor, host.Printf, () => 0, queueNet: false);
            host.SpawnedBotName = name;
            return;
        }

        host.SpawnBot(name);
    }

    public static void RunRemoveBotsCcmd(CcmdHost host)
    {
        if (!host.CanControlSettings)
        {
            host.Printf("Only setting controllers can remove bots\n");
            return;
        }

        host.NetWriteInt8(VmBBot.DemKillbots);
    }

    public static void RunFreezeCcmd(CcmdHost host)
    {
        if (host.CheatMode)
            return;

        if (host.NetGame && !host.CanControlSettings)
        {
            host.Printf("Only setting controllers can use freeze mode\n");
            return;
        }

        host.NetWriteInt8(VmBBot.DemGenericcheat);
        host.NetWriteInt8(VmBBot.ChtFreeze);
    }

    public static void RunListBotsCcmd(CcmdHost host)
    {
        var count = 0;
        var node = host.BotListHead;
        while (node != null)
        {
            var suffix = node.InUse == VmBBot.BotInuseYes ? " (active)" : "";
            host.Printf($"{node.Name}{suffix}");
            node = node.Next;
            count++;
        }

        host.Printf($"> {count} bots");
    }
}

public static class VmBBotCcmdRegister
{
    /// <summary>Registers only CCMDs with ported bodies. <c>bot_next_color</c> CVAR is not registered here.</summary>
    public static void Register(IDictionary<string, Action<VmBBotCcmd.CcmdHost>> commands)
    {
        commands["addbot"] = VmBBotCcmd.RunAddBotCcmd;
        commands["removebots"] = VmBBotCcmd.RunRemoveBotsCcmd;
        commands["freeze"] = VmBBotCcmd.RunFreezeCcmd;
        commands["listbots"] = VmBBotCcmd.RunListBotsCcmd;
    }
}
