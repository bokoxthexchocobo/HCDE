namespace HCDE.Playsim;

/// <summary>
/// Pregame net window from <c>i_net.cpp</c> 1639-1811. Chunk 1394.
/// <c>I_ClearClient</c> starts at line 1812 and stays outside this band.
/// This tree compiles the non-Windows <c>I_NetLog</c>.
/// </summary>
public static class VmNetWindow1394Slice
{
    public const uint CflNone = 0;
    public const uint CflConsolePlayer = 1;
    public const uint CflHost = 1u << 1;
    public const string UnknownError = "unknown network error";
    public const string GuardWindow = "network session window";
    public const string GuardMessage = "network session message";

    public sealed class Session
    {
        public bool Dedicated;
        public bool Silent;
        public bool WindowsDedicatedBuild;
        public bool StartRequested;
        public bool AbortRequested;
        public int ConsolePlayer;
        public bool UsesDedicatedSlot;
        public readonly List<string> Prints = new();
        public readonly List<string> Logs = new();
        public readonly List<string> Warnings = new();
        public readonly List<string> Guards = new();
        public readonly List<string> Statuses = new();
        public readonly List<string> WindowMessages = new();
        public readonly List<Connect> Connects = new();
        public readonly List<Update> Updates = new();
        public readonly List<int> Disconnects = new();
        public readonly List<Progress> Progresses = new();
        public int Closes, Dones, RconPolls, Pumps, Sleeps, Inits;
        public bool? WindowLoop;
        public string? InitMessage;
        public bool InitHost;
    }

    public readonly record struct Connect(int Client, string Name, uint Flags, int Status);
    public readonly record struct Update(int Client, int Status);
    public readonly record struct Progress(int Current, int Limit);

    public static string Tag(Session session) => session.Dedicated ? "NetServer" : "NetSession";

    public static void NetError(string? error, Session session, bool appleOrWindowsLog)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        string message = error ?? UnknownError;
        NetLog("Fatal network error: " + message, session, appleOrWindowsLog);
        session.Warnings.Add("fatal network error: " + message);
        if (!session.Dedicated && !session.Silent)
            session.Closes++;
        throw new InvalidOperationException(message);
    }

    public static void NetInit(string msg, bool host, Session session)
    {
        Note(msg, session, GuardWindow);
        if (!session.Dedicated && !session.Silent)
        {
            session.InitMessage = msg;
            session.InitHost = host;
            session.Inits++;
        }
    }

    public static void NetMessage(string msg, Session session)
    {
        Note(msg, session, GuardMessage);
        if (!session.Dedicated && !session.Silent)
            session.WindowMessages.Add(msg);
    }

    public static bool NetLoop(Func<bool> loopCallback, Session session, bool? windowResult = null)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        if (session.Dedicated || session.Silent)
        {
            if (loopCallback == null)
                throw new ArgumentNullException(nameof(loopCallback));
            while (true)
            {
                if (session.Dedicated)
                    session.RconPolls++;
                if (loopCallback())
                    break;
                if (session.WindowsDedicatedBuild && session.Dedicated)
                {
                    session.Pumps++;
                    if (session.AbortRequested)
                        return false;
                }
                session.Sleeps++;
            }
            if (session.WindowsDedicatedBuild && session.Dedicated)
                session.Pumps++;
            return !session.AbortRequested;
        }
        if (windowResult == null)
            throw new ArgumentNullException(nameof(windowResult));
        session.WindowLoop = windowResult;
        return windowResult.Value;
    }

    public static void ClientConnected(int client, Session session, Func<int, uint, string> clientName, Func<int, bool> reserved, Func<int, bool> serviceAuthority, int status, uint charLimit = 0)
    {
        if (session == null || clientName == null || reserved == null || serviceAuthority == null)
            throw new ArgumentNullException(nameof(session));
        if (reserved(client))
        {
            session.Prints.Add(Tag(session) + ":: Dedicated server authority slot ready.\n");
            return;
        }
        session.Prints.Add(Tag(session) + ":: Client '" + clientName(client, 0) + "' connected.\n");
        uint flags = CflNone;
        if (serviceAuthority(client) && !reserved(client))
            flags |= CflHost;
        if (client == session.ConsolePlayer)
            flags |= CflConsolePlayer;
        session.Connects.Add(new Connect(client, clientName(client, charLimit), flags, status));
    }

    public static void ClientUpdated(int client, int status, Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        session.Updates.Add(new Update(client, status));
    }

    public static void ClientDisconnected(int client, Session session, Func<int, uint, string> clientName, string? reason = null)
    {
        if (session == null || clientName == null)
            throw new ArgumentNullException(nameof(session));
        bool hasReason = !string.IsNullOrEmpty(reason);
        string name = clientName(client, 0);
        session.Prints.Add(Tag(session) + ":: Client '" + name + "' disconnected" + (hasReason ? " (" + reason + ")" : "") + ".\n");
        if (hasReason)
            session.Warnings.Add("client disconnected client=" + client.ToString() + " name=" + name + " reason=" + reason);
        session.Disconnects.Add(client);
    }

    public static void UpdatePlayers(int current, int limit, Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        if (session.UsesDedicatedSlot)
        {
            current = Math.Max(current - 1, 0);
            limit = Math.Max(limit - 1, 0);
        }
        session.Progresses.Add(new Progress(current, limit));
    }

    public static bool ShouldStart(Session session, Func<bool>? windowShouldStart)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        if (session.Dedicated)
            return session.StartRequested;
        if (windowShouldStart == null)
            throw new ArgumentNullException(nameof(windowShouldStart));
        return windowShouldStart();
    }

    public static void KickClients(List<int> clients, Func<int> next) => Drain(clients, next);

    public static void BanClients(List<int> clients, Func<int> next) => Drain(clients, next);

    public static void NetDone(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        session.Dones++;
    }

    private static void Drain(List<int> clients, Func<int> next)
    {
        if (clients == null || next == null)
            throw new ArgumentNullException(nameof(clients));
        clients.Clear();
        int c;
        while ((c = next()) != -1)
            clients.Add(c);
    }

    private static void Note(string msg, Session session, string guard)
    {
        if (msg == null || session == null)
            throw new ArgumentNullException(nameof(session));
        session.Prints.Add(Tag(session) + ":: " + msg + "\n");
        if (session.WindowsDedicatedBuild && session.Dedicated)
            session.Statuses.Add(msg);
        if (!session.Dedicated && !session.Silent)
            session.Guards.Add(guard);
    }

    private static void NetLog(string text, Session session, bool appleOrWindowsLog)
    {
        if (appleOrWindowsLog)
            session.Logs.Add(text + "\n");
        else
            session.Logs.Add("\r" + (text.Length >= 40 ? text : text.PadRight(40)) + "\n");
    }
}
