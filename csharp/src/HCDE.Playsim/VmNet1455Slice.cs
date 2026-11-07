using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Mute commands and settings controllers from d_net.cpp 10387-10667.
/// </summary>
public static class VmNet1455Slice
{
    public const int BandStart = 10387;
    public const int BandEnd = 10667;
    public const int MaxPlayers = 64;
    public const int AddController = 48;
    public const int DelController = 49;

    public sealed class World
    {
        public bool Multiplayer;
        public bool Netgame;
        public bool LocalAuthority;
        public int Console = -1;
        public ulong Muted;
        public bool[] Playing = new bool[MaxPlayers];
        public bool[] ClientInGame = new bool[MaxPlayers];
        public bool[] Authority = new bool[MaxPlayers];
        public bool[] Controller = new bool[MaxPlayers];
        public string[] Names = new string[MaxPlayers];
        public List<int> Clients = new();
        public List<int> Writes = new();
        public List<string> Logs = new();
    }

    public static void Mute(World w, params string[] args) => ChangeMute(w, args, true);

    public static void Unmute(World w, params string[] args) => ChangeMute(w, args, false);

    public static void MuteAll(World w)
    {
        if (!w.Multiplayer)
        {
            w.Logs.Add("This command can only be used when playing in multiplayer");
            return;
        }
        for (int i = 0; i < MaxPlayers; i++)
        {
            if (w.Playing[i] && i != w.Console)
                w.Muted |= 1UL << i;
        }
    }

    public static void UnmuteAll(World w)
    {
        if (!w.Multiplayer)
        {
            w.Logs.Add("This command can only be used when playing in multiplayer");
            return;
        }
        w.Muted = 0;
    }

    public static void ListMuted(World w)
    {
        if (!w.Multiplayer)
        {
            w.Logs.Add("This command can only be used when playing in multiplayer");
            return;
        }
        bool found = false;
        for (int i = 0; i < MaxPlayers; i++)
        {
            if ((w.Muted & (1UL << i)) != 0)
            {
                found = true;
                w.Logs.Add(i + ". " + w.Names[i]);
            }
        }
        if (!found)
            w.Logs.Add("No one currently muted");
    }

    public static void AddControllers(World w, params string[] args)
        => EditControllers(w, args, true, "addsettingscontrollers", "Bad client number");

    public static void RemoveControllers(World w, params string[] args)
        => EditControllers(w, args, false, "removesettingscontrollers", "Bad player number");

    public static void RemoveAllControllers(World w)
    {
        var chosen = new List<int>();
        foreach (int client in w.Clients)
        {
            if (!w.Authority[client] && w.Controller[client])
                chosen.Add(client);
        }
        ApplyControllers(w, chosen, false);
    }

    public static void ListControllers(World w)
    {
        if (!w.Netgame)
        {
            w.Logs.Add("This command can only be used when playing in a net game");
            return;
        }
        var chosen = new List<int>();
        foreach (int client in w.Clients)
        {
            if (!w.Authority[client] && w.Controller[client])
                chosen.Add(client);
        }
        if (chosen.Count == 0)
        {
            w.Logs.Add("No other settings controllers");
            return;
        }
        w.Logs.Add("The following players can change the game settings:");
        foreach (int client in chosen)
        {
            string line = client + ". " + w.Names[client];
            if (client == w.Console)
                line += " [*]";
            w.Logs.Add(line);
        }
    }

    static void ChangeMute(World w, string[] args, bool mute)
    {
        if (args.Length == 0)
        {
            w.Logs.Add(mute
                ? "Usage: mute <player numbers>"
                : "Usage: unmute <player numbers>");
            return;
        }
        if (!w.Multiplayer)
        {
            w.Logs.Add("This command can only be used when playing in multiplayer");
            return;
        }
        var chosen = new List<int>();
        foreach (string arg in args)
        {
            if (!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out int player)
                || player < 0 || player >= MaxPlayers)
                w.Logs.Add("Bad player number " + arg);
            else if (player != w.Console && !chosen.Contains(player))
                chosen.Add(player);
        }
        foreach (int player in chosen)
        {
            if (!w.Playing[player])
                w.Logs.Add("Player " + player + " is not in game");
            else if (mute)
            {
                w.Muted |= 1UL << player;
                w.Logs.Add("Muted player " + w.Names[player] + " [" + player + "]");
            }
            else
            {
                w.Muted &= ~(1UL << player);
                w.Logs.Add("Unmuted player " + w.Names[player] + " [" + player + "]");
            }
        }
    }

    static void EditControllers(World w, string[] args, bool add, string command, string badLabel)
    {
        if (args.Length == 0)
        {
            w.Logs.Add("Usage: " + command + " <client numbers>");
            return;
        }
        var chosen = new List<int>();
        foreach (string arg in args)
        {
            if (!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out int client)
                || client < 0 || client >= MaxPlayers)
                w.Logs.Add(badLabel + " " + arg);
            else if (!w.Authority[client] && !chosen.Contains(client))
                chosen.Add(client);
        }
        ApplyControllers(w, chosen, add);
    }

    static void ApplyControllers(World w, List<int> clients, bool add)
    {
        if (!w.Netgame)
        {
            w.Logs.Add("This command can only be used when playing in a net game");
            return;
        }
        if (!w.LocalAuthority)
        {
            w.Logs.Add("This command is only accessible to the authority");
            return;
        }
        foreach (int client in clients)
        {
            if (w.Authority[client])
                w.Logs.Add("The authority cannot change its own settings controller status");
            else if (!w.ClientInGame[client])
                w.Logs.Add("Client " + client + " is not in game");
            else if (w.Controller[client] && add)
                w.Logs.Add("Client " + client + " is already a settings controller");
            else if (!w.Controller[client] && !add)
                w.Logs.Add("Client " + client + " is already not a settings controller");
            else
            {
                w.Writes.Add(add ? AddController : DelController);
                w.Writes.Add(client);
            }
        }
    }
}
