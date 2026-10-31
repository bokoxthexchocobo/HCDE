namespace HCDE.Playsim;

/// <summary>DAP debug server from <c>DebugServer.cpp</c> 1-194. The restart routine parks while terminate is clear, and Stop joins it.</summary>
public sealed class VmDebugServer1385Slice
{
    public const string Red = "\u001cG";

    public sealed class ExitEvent(int reason) : Exception("The game wants to exit")
    {
        public int Reason => reason;
    }

    public sealed class Connection
    {
        public bool Open = true;
    }

    public sealed class Session
    {
        public Connection? Bound;
        public Action? OnClosed;
        public Action? OnDisconnectSent;
        public void Bind(Connection connection, Action onClosed) { Bound = connection; OnClosed = onClosed; }
        public void RegisterSentHandler(Action onDisconnect) { OnDisconnectSent = onDisconnect; }
    }

    public sealed class NetServer
    {
        public bool AcceptStart = true;
        public string? ErrorDuringStart;
        public bool Running;
        public int StartedPort;
        public int StopCount;
        public Action<Connection?>? OnConnect;
        public Action<string?>? OnError;

        public bool Start(int listenPort, Action<Connection?> onConnect, Action<string?> onError)
        {
            StartedPort = listenPort;
            OnConnect = onConnect;
            OnError = onError;
            if (ErrorDuringStart != null)
                onError(ErrorDuringStart);
            Running = AcceptStart;
            return AcceptStart;
        }

        public void Stop() { StopCount++; Running = false; }
    }

    public sealed class Debugger
    {
        public bool Quitting;
        public bool Ending;
        public int EndCount;
        public bool LastClosed;
        public Session? Active;
        public int StartCount;

        public bool EndSession(bool wasClosed)
        {
            if (Ending)
                return Quitting;
            Ending = true;
            EndCount++;
            LastClosed = wasClosed;
            Ending = false;
            return Quitting;
        }

        public bool IsEndingSession() => Ending;
        public void StartSession(Session session) { StartCount++; Active = session; }
    }

    public readonly List<string> Logs = new();
    public readonly List<string> Trace = new();
    public readonly List<NetServer> Created = new();
    public Func<NetServer> CreateServer = () => new NetServer();
    public Debugger Script { get; private set; }
    public NetServer? Server { get; private set; }
    public int SubscribeCount { get; private set; }
    public int UnsubscribeCount { get; private set; }
    public bool EnabledHandler { get; private set; }
    public bool PortChosen { get; private set; }
    public int Port => port;
    public bool Stopped => stopped;
    public bool TerminateFlag => terminate;
    public bool RestartPending => restartServer;
    public bool Closed => closed;
    public bool Quitting => quitting;
    public bool RestartWaiting => restartArmed && !restartExited && !restartJoined;
    int port;
    bool stopped, terminate, restartServer, closed, quitting, held, restartArmed, restartExited, restartJoined;

    public VmDebugServer1385Slice()
    {
        terminate = false;
        stopped = false;
        quitting = false;
        Script = new Debugger();
        Mark("DAP debug server object created");
    }

    public void RunRestartThread()
    {
        while (true)
        {
            if (!terminate)
                return;
            held = true;
            var leave = false;
            try
            {
                quitting = Script.EndSession(closed);
                terminate = false;
                if (stopped || quitting)
                    leave = true;
                else if (restartServer)
                {
                    Server!.Stop();
                    Server = null;
                    Server = NewServer();
                    if (!StartServer())
                    {
                        Unsubscribe();
                        stopped = true;
                        ErrorTrace("DAP debug server restart failed");
                        leave = true;
                    }
                    else
                        restartServer = false;
                }
            }
            finally { held = false; }
            if (leave)
                break;
        }
        restartExited = true;
        if (quitting)
            throw new ExitEvent(0);
    }

    public bool Listen(int pPort)
    {
        terminate = false;
        stopped = false;
        quitting = false;
        closed = false;
        if (pPort == 0)
        {
            Warning("DAP debug server listen skipped: port is 0");
            return false;
        }
        port = pPort;
        PortChosen = true;
        if (Server == null)
            Server = NewServer();
        else
            Stop();
        Mark($"DAP debug server listen requested port={port}");
        restartArmed = true;
        restartJoined = false;
        restartExited = false;
        return StartServer();
    }

    public void OnClientConnected(Connection? connection)
    {
        if (connection == null || !connection.Open)
        {
            LogInternalError("Client connected but connection is not open");
            return;
        }
        closed = false;
        var sess = new Session();
        sess.Bind(connection, OnConnectionClosed);
        sess.RegisterSentHandler(OnDisconnectSent);
        LogInternal("DAP client connected.");
        Mark("DAP client connected");
        Script.StartSession(sess);
    }

    public void OnError(string? msg)
    {
        LogInternalError("Server error: " + (msg ?? "(null)") + "\n");
        Warning("DAP server error: " + (msg ?? "unknown"));
        if (restartServer)
            LogInternalError("Restart failed! Stopping server...");
        else
        {
            LogInternalError("Restarting server...");
            held = true;
            terminate = true;
            closed = true;
            restartServer = true;
            held = false;
        }
    }

    public void Stop()
    {
        Mark("DAP debug server stop requested");
        Unsubscribe();
        held = true;
        terminate = true;
        stopped = true;
        held = false;
        if (Server != null)
            Server.Stop();
        JoinRestart();
        Mark("DAP debug server stopped");
    }

    public void Destroy() => Stop();
    public void HoldGate() => held = true;
    public void ReleaseGate() => held = false;

    void OnConnectionClosed()
    {
        LogInternal("DAP connection closed.");
        if (!TryLock())
            return;
        try
        {
            if (!terminate && !Script.IsEndingSession())
            {
                LogInternalError("DAP connection closed without terminating session.");
                closed = true;
                terminate = true;
            }
        }
        finally { Unlock(); }
    }

    void OnDisconnectSent()
    {
        LogInternal("Debugger disconnecting...");
        held = true;
        terminate = true;
        held = false;
    }

    bool StartServer()
    {
        if (!Server!.Start(port, OnClientConnected, OnError))
        {
            LogInternalError($"DAP debugging server failed to start on port {port}, debugging will be unavailable!");
            ErrorTrace($"DAP debug server failed to start port={port}");
            return false;
        }
        Subscribe();
        LogInternal($"DAP debugging server started on port {port}");
        Mark($"DAP debug server started port={port}");
        return true;
    }

    void JoinRestart()
    {
        if (!restartArmed || restartJoined)
            return;
        try
        {
            if (!restartExited)
                RunRestartThread();
            restartJoined = true;
        }
        catch
        {
            restartJoined = true;
            throw;
        }
    }

    bool TryLock()
    {
        if (held)
            return false;
        held = true;
        return true;
    }

    void Unlock() => held = false;
    NetServer NewServer() { var created = CreateServer(); Created.Add(created); return created; }
    void Subscribe() { SubscribeCount++; EnabledHandler = true; }
    void Unsubscribe() { UnsubscribeCount++; if (EnabledHandler) EnabledHandler = false; }
    void LogInternal(string text) => Logs.Add(text + "\n");
    void LogInternalError(string text) => Logs.Add(Red + text + "\n");
    void Mark(string text) => Trace.Add("mark|" + text);
    void Warning(string text) => Trace.Add("warning|" + text);
    void ErrorTrace(string text) => Trace.Add("error|" + text);
}
