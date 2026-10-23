using System.Runtime.InteropServices;
using System.Text;

namespace HCDE.Playsim;

/// <summary>Cajun master lifecycle and spawn scheduling from <c>b_game.cpp</c>.</summary>
public static class VmBBotGame
{
    public const int NoColor = 11;
    public const int SpawnDelay = 80;
    public const int TeamNone = 255;

    public sealed class CajunMasterState
    {
        public int BotNum { get; set; }
        public int WantedBotNum { get; set; }
        public int SpawnTries { get; set; }
        public int TJoin { get; set; }
        public bool Teamplay { get; set; }
        public bool Deathmatch { get; set; }
        public bool Ctf { get; set; }
        public VmBBot.BotInfoNode? BotListHead { get; set; }
        public List<string> GetSpawned { get; } = new();
    }

    static readonly string[] ColorSuffixes =
    [
        "\\color\\40 cf 00",
        "\\color\\b0 b0 b0",
        "\\color\\50 50 60",
        "\\color\\8f 00 00",
        "\\color\\ff ff ff",
        "\\color\\ff af 3f",
        "\\color\\bf 00 00",
        "\\color\\00 00 ff",
        "\\color\\00 00 7f",
        "\\color\\ff ff 00",
        "\\color\\cf df 90",
    ];

    public static void Init(CajunMasterState cajun, bool ctf, bool teamplay, int wantedBotNum, int getSpawnedCount)
    {
        cajun.BotNum = 0;
        cajun.SpawnTries = 0;
        cajun.Ctf = ctf;
        if (ctf && !teamplay)
            cajun.Teamplay = true;
        else
            cajun.Teamplay = teamplay;

        cajun.TJoin = (wantedBotNum + 1) * SpawnDelay;
        if (cajun.BotListHead == null)
            LoadBots(cajun, _ => { });
        else
        {
            var node = cajun.BotListHead;
            while (node != null)
            {
                node.InUse = VmBBot.BotInuseNo;
                node = node.Next;
            }
        }

        cajun.WantedBotNum = Math.Max(0, wantedBotNum);
        if (cajun.WantedBotNum > getSpawnedCount)
            cajun.WantedBotNum = getSpawnedCount;
    }

    public static void End(CajunMasterState cajun, bool deathmatch, IEnumerable<string> activeBotNames)
    {
        cajun.GetSpawned.Clear();
        if (deathmatch)
        {
            foreach (var name in activeBotNames)
                cajun.GetSpawned.Add(name);
            cajun.WantedBotNum = cajun.BotNum;
        }
        else
            cajun.WantedBotNum = 0;
    }

    public static void Main(
        CajunMasterState cajun,
        AuthoritySimulation sim,
        bool demoPlayback,
        int gameStateLevel,
        bool localAuthority,
        bool levelFrozen,
        Action<string> print,
        Func<int> botSpawnRandom)
    {
        if (demoPlayback || gameStateLevel != VmBBot.GsLevel || !localAuthority)
            return;

        if (cajun.WantedBotNum > cajun.BotNum && !levelFrozen)
        {
            if (cajun.TJoin == (cajun.WantedBotNum - cajun.BotNum) * SpawnDelay)
            {
                string? botName = null;
                if (cajun.SpawnTries >= 0 && cajun.SpawnTries < cajun.GetSpawned.Count)
                    botName = cajun.GetSpawned[cajun.SpawnTries];

                if (!SpawnBot(cajun, sim, botName, NoColor, print, botSpawnRandom))
                    cajun.WantedBotNum--;
                cajun.SpawnTries++;
            }

            cajun.TJoin--;
        }
    }

    public static bool SpawnBot(
        CajunMasterState cajun,
        AuthoritySimulation sim,
        string? name,
        int color,
        Action<string> print,
        Func<int> botSpawnRandom)
    {
        var thebot = cajun.BotListHead;
        var botshift = 0;
        if (name != null)
        {
            while (thebot != null && !string.Equals(thebot.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                botshift++;
                thebot = thebot.Next;
            }

            if (thebot == null)
            {
                print($"couldn't find {name} in bots.cfg\n");
                return false;
            }

            if (thebot.InUse == VmBBot.BotInuseWaiting)
                return false;
            if (thebot.InUse == VmBBot.BotInuseYes)
            {
                print($"{name} is already in the thick\n");
                return false;
            }
        }
        else
        {
            var available = new List<VmBBot.BotInfoNode>();
            var scan = cajun.BotListHead;
            while (scan != null)
            {
                if (scan.InUse == VmBBot.BotInuseNo)
                    available.Add(scan);
                scan = scan.Next;
            }

            if (available.Count == 0)
            {
                print("Couldn't spawn bot; no bot left in bots.cfg\n");
                return false;
            }

            thebot = available[botSpawnRandom() % available.Count];
            botshift = 0;
            scan = cajun.BotListHead;
            while (scan != null && scan != thebot)
            {
                botshift++;
                scan = scan.Next;
            }
        }

        thebot.InUse = VmBBot.BotInuseWaiting;

        sim.NetWriteInt8(VmBBot.DemAddBot);
        sim.NetWriteInt8(botshift);
        var concat = thebot.Info;
        if (color != NoColor && color >= 0 && color < ColorSuffixes.Length)
            concat += ColorSuffixes[color];
        if (thebot.LastTeam != TeamNone)
            concat += $"\\team\\{thebot.LastTeam}\n";
        sim.NetWriteString(concat);
        sim.NetWriteInt8(thebot.Skill.Aiming);
        sim.NetWriteInt8(thebot.Skill.Perfection);
        sim.NetWriteInt8(thebot.Skill.Reaction);
        sim.NetWriteInt8(thebot.Skill.Isp);

        return true;
    }

    /// <summary>Native <c>Net_DoCommand</c> <c>DEM_ADDBOT</c> → <c>FCajunMaster::TryAddBot</c> for buffered demo bytes.</summary>
    public static void DrainNetDemoCommands(
        AuthoritySimulation sim,
        CajunMasterState cajun,
        int consolePlayer,
        Action<string> print)
    {
        var bytes = sim.NetDemoEventBytes;
        var span = CollectionsMarshal.AsSpan(bytes);
        var index = 0;
        while (index < span.Length)
        {
            var command = span[index++];
            if (command == VmBBot.DemAddBot)
                TryAddBot(cajun, sim, span, ref index, consolePlayer, print);
            else
                break;
        }

        bytes.Clear();
    }

    public static void TryAddBot(
        CajunMasterState cajun,
        AuthoritySimulation sim,
        ReadOnlySpan<byte> stream,
        ref int index,
        int consolePlayer,
        Action<string> print)
    {
        if (index >= stream.Length)
            return;
        var botShift = stream[index++];
        var info = ReadNetString(stream, ref index);
        if (index + 4 > stream.Length)
            return;
        var skill = new VmBBot.BotSkill
        {
            Aiming = stream[index++],
            Perfection = stream[index++],
            Reaction = stream[index++],
            Isp = stream[index++],
        };
        TryAddBot(cajun, sim, botShift, info, skill, consolePlayer, print);
    }

    public static void TryAddBot(
        CajunMasterState cajun,
        AuthoritySimulation sim,
        int botShift,
        string info,
        VmBBot.BotSkill skill,
        int consolePlayer,
        Action<string> print)
    {
        VmBBot.BotInfoNode? thebot = null;
        if (consolePlayer >= 0)
        {
            thebot = cajun.BotListHead;
            var shift = botShift;
            while (thebot != null && shift > 0)
            {
                thebot = thebot.Next;
                shift--;
            }
        }

        if (DoAddBot(sim, info, skill, print))
        {
            cajun.BotNum++;
            if (thebot != null)
                thebot.InUse = VmBBot.BotInuseYes;
        }
        else if (thebot != null)
            thebot.InUse = VmBBot.BotInuseNo;
    }

    public static bool DoAddBot(AuthoritySimulation sim, string info, VmBBot.BotSkill skill, Action<string> print)
    {
        if (!sim.TryAddCajunBotPlayer(info, skill, out _, out var displayName))
        {
            print($"The maximum of {PlayerPawn.MaxPlayers} players/bots has been reached\n");
            return false;
        }

        print($"{displayName} joined the game\n");
        return true;
    }

    public static void RemoveAllBots(AuthoritySimulation sim, CajunMasterState cajun, bool fromList)
    {
        foreach (var player in sim.Players.ToArray())
        {
            if (player.CajunBot == null)
                continue;
            ClearCajunPlayer(sim, cajun, player, keepTeam: !fromList);
        }

        if (fromList)
            cajun.WantedBotNum = 0;
        cajun.BotNum = 0;
    }

    public static void ClearCajunPlayer(AuthoritySimulation sim, CajunMasterState cajun, PlayerPawn player, bool keepTeam)
    {
        if (!player.Destroyed)
            player.Destroy();

        var node = cajun.BotListHead;
        var name = player.CajunDisplayName ?? "";
        while (node != null && !string.Equals(name, node.Name, StringComparison.OrdinalIgnoreCase))
            node = node.Next;

        if (node != null)
        {
            node.InUse = VmBBot.BotInuseNo;
            if (!keepTeam)
                node.LastTeam = TeamNone;
        }

        if (player.CajunBot is { } bot)
        {
            VmBBot.Clear(bot);
            player.CajunBot = null;
        }
    }

    public static void ForgetBots(CajunMasterState cajun) => cajun.BotListHead = null;

    public static bool LoadBots(CajunMasterState cajun, Action<string> print)
    {
        ForgetBots(cajun);
        for (var i = 0; i < 5; i++)
        {
            var defaultBot = new VmBBot.BotInfoNode
            {
                Name = $"DefaultBot{i}",
                Info = $"\\autoaim\\0\\movebob\\.25\\name\\DefaultBot{i}\\team\\255\\playerclass\\random",
                Skill = new VmBBot.BotSkill { Aiming = i, Perfection = i, Reaction = i, Isp = i },
                Next = cajun.BotListHead,
                LastTeam = TeamNone,
            };
            cajun.BotListHead = defaultBot;
        }

        return false;
    }

    static string ReadNetString(ReadOnlySpan<byte> stream, ref int index)
    {
        var start = index;
        while (index < stream.Length && stream[index] != 0)
            index++;
        var text = index > start ? Encoding.UTF8.GetString(stream.Slice(start, index - start)) : "";
        if (index < stream.Length)
            index++;
        return text;
    }
}
