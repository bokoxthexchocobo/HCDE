using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Prediction-loss debug tick from <c>d_net_predict_impl.cpp</c> (1-375). Chunk 1327.
/// </summary>
public sealed class VmPredictDebug1327
{
    public const int TicRate = 35;
    public const int GsLevel = 0;
    public const int MaxPlayers = 64;
    public const int LocalCmdTics = 35 * 5 * 3;
    public const int BtAttack = 1;
    public const int CfPredicting = 1 << 13;
    public const int PspWeapon = 1;
    public const int OfEuthanizeMe = 1 << 5;
    public const int ActionMelee = 3;
    public const int ActionMissile = 4;
    public const int ActionPain = 5;
    public const string CsvHeader =
        "ms_time,gametic,clienttic,totaltics,availabletics,backlog,lowestseq," +
        "authority,seq,ack,acklag,snap_rx,ctrl_rx,any_rx,snap_count,input_sent," +
        "win_passive_client,win_passive_auth,win_local_repairs,win_hard_repairs,win_faults," +
        "min_available_w,max_acklag_w,max_stale_visual_w,max_backlog_w," +
        "invasion,wave,server_active,tracked_mirrors,blocking_mirrors,projectile_mirrors," +
        "stale_attacking,mirror_delta,last_world_tics,lag_state," +
        "predicting,compat_predicting,attack,move,fwd,side,up,buttons," +
        "pos_x,pos_y,pos_z,vel_x,vel_y,vel_z,bob,health," +
        "viewheight,view_z_off,psprite_x,psprite_y,psprite_alive,room\n";

    public int Level { get; set; }
    public bool NetGame { get; set; }
    public int GameState { get; set; } = GsLevel;
    public bool DemoPlayback { get; set; }
    public bool LocalAuthority { get; set; }
    public int AuthoritySlot { get; set; } = -1;
    public ulong NowMs { get; set; }
    public int ClientTic { get; set; }
    public int GameTic { get; set; }
    public int TicDup { get; set; } = 1;
    public bool ExtraTic { get; set; }
    public int ConsolePlayer { get; set; }
    public int TotalTics { get; set; }
    public int AvailableTics { get; set; }
    public int LowestSequence { get; set; }
    public int Interval { get; set; } = 15;
    public int EnterTic { get; set; }
    public int LastGameUpdate { get; set; }
    public int PauseGraceUntil { get; set; }
    public int AckLagThreshold { get; set; }
    public int MirrorThreshold { get; set; }
    public int PassiveThreshold { get; set; }
    public bool HudDebug { get; set; }
    public bool Predicting { get; set; }
    public int ServerActive { get; set; }
    public int Wave { get; set; }
    public string InvasionName { get; set; } = "";
    public string LagName { get; set; } = "";
    public string AppData { get; set; } = "";
    public uint Room { get; set; }
    public ulong HealthRepairs { get; set; }
    public ulong StateRepairs { get; set; }
    public ulong RespawnRepairs { get; set; }
    public ulong DeathRepairs { get; set; }
    public ulong FaultReports { get; set; }
    public ulong LifetimePassiveClient { get; set; }
    public ulong LifetimePassiveAuth { get; set; }

    public int MinAvailable { get; set; } = int.MaxValue;
    public int MaxAckLag { get; set; }
    public int MaxStaleVisual { get; set; }
    public int MaxBacklog { get; set; }
    public int LastSampleTic { get; set; }
    public ulong WindowPassiveClient { get; set; }
    public ulong WindowPassiveAuth { get; set; }
    public ulong WindowLocalRepairs { get; set; }
    public ulong WindowHardRepairs { get; set; }
    public ulong WindowFaultReports { get; set; }
    public ulong LastSoftWarnMs { get; set; }
    public ulong LastTraceDumpMs { get; set; }
    public bool CsvHeaderProbed { get; set; }
    public ulong LastPingMs { get; set; }
    public ulong MismatchStartMs { get; set; }
    public ulong LastDesyncMs { get; set; }

    public List<int> NetworkClients { get; } = new();
    public ClientSlot[] Clients { get; } = NewClients();
    public PeerSlot[] Peers { get; } = NewPeers();
    public PlayerSlot?[] Players { get; } = new PlayerSlot?[MaxPlayers];
    public UserCmd[] Commands { get; } = new UserCmd[LocalCmdTics];
    public List<MirrorRef> Mirrors { get; } = new();
    public Dictionary<string, string> Files { get; } = new();
    public List<PingSample> Pings { get; } = new();
    public List<string> DesyncWarnings { get; } = new();
    public List<string> SoftWarnings { get; } = new();
    public List<string> HudLines { get; } = new();
    public List<TraceDump> Traces { get; } = new();

    public readonly record struct PingSample(int Client, int LeadTics, int LeadMs, int Rtt, int TicDup, int Extra, int Delta);
    public readonly record struct TraceDump(string Path, string Channel, string Severity);

    public sealed class ClientSlot
    {
        public int CurrentSequence;
        public int SequenceAck;
        public int AverageLatency;
    }

    public sealed class PeerSlot
    {
        public uint RxServerSnapshotSequence;
        public uint RxControlSequence;
        public uint RxSequence;
        public uint SnapshotReceived;
        public uint ClientCommandSent;
    }

    public struct UserCmd
    {
        public uint Buttons;
        public short Forward;
        public short Side;
        public short Up;
    }

    public sealed class PSprite
    {
        public double X;
        public double Y;
        public bool Alive;
    }

    public sealed class PlayerSlot
    {
        public int Cheats;
        public double Bob;
        public double ViewHeight;
        public double ViewZ;
        public bool HasActor;
        public double X;
        public double Y;
        public double Z;
        public double VelX;
        public double VelY;
        public double VelZ;
        public int Health;
        public PSprite? Weapon;
    }

    public sealed class MirrorRef
    {
        public bool Missing;
        public int ObjectFlags;
        public bool Corpse;
        public bool Projectile;
        public int Action;
        public int VisualTargetTic;
    }

    public void Tick(int totalTics, int availableTics, int lowestSequence)
    {
        if (Level <= 0)
        {
            return;
        }

        if (!NetGame || GameState != GsLevel || DemoPlayback || LocalAuthority)
        {
            return;
        }

        bool slotOk = AuthoritySlot >= 0 && AuthoritySlot < MaxPlayers;
        ClientSlot? authority = slotOk ? Clients[AuthoritySlot] : null;
        PeerSlot? peer = slotOk ? Peers[AuthoritySlot] : null;
        ulong pingNow = NowMs;
        if (pingNow - LastPingMs >= 1000)
        {
            LastPingMs = pingNow;
            foreach (int client in NetworkClients)
            {
                int lead;
                if (client == ConsolePlayer)
                {
                    lead = Max((ClientTic - GameTic) / TicDup, 0);
                }
                else
                {
                    lead = Max(Clients[client].CurrentSequence - (GameTic / TicDup), 0);
                }

                int leadMs = lead * TicDup * 1000 / TicRate;
                int extra = ExtraTic ? 1 : 0;
                Pings.Add(new PingSample(client, lead, leadMs, Clients[client].AverageLatency, TicDup, extra, ClientTic - GameTic));
            }
        }

        int backlog = ClientTic - GameTic;
        int newest = authority != null ? authority.CurrentSequence : -1;
        int ack = authority != null ? authority.SequenceAck : -1;
        int ackLag = newest >= 0 && ack >= 0 ? Max(newest - ack, 0) : 0;
        if (availableTics < MinAvailable)
        {
            MinAvailable = availableTics;
        }

        if (ackLag > MaxAckLag)
        {
            MaxAckLag = ackLag;
        }

        if (backlog > MaxBacklog)
        {
            MaxBacklog = backlog;
        }

        uint tracked = (uint)Mirrors.Count;
        uint blocking = 0;
        uint projectiles = 0;
        uint staleAttacking = 0;
        int maxStale = 0;
        foreach (MirrorRef mirror in Mirrors)
        {
            if (mirror.Missing || (mirror.ObjectFlags & OfEuthanizeMe) != 0 || mirror.Corpse)
            {
                continue;
            }

            if (mirror.Projectile)
            {
                projectiles++;
            }
            else
            {
                blocking++;
            }

            bool attacking = mirror.Action == ActionMelee || mirror.Action == ActionMissile || mirror.Action == ActionPain;
            if (attacking && mirror.VisualTargetTic > 0)
            {
                int age = GameTic - mirror.VisualTargetTic;
                if (age > maxStale)
                {
                    maxStale = age;
                }

                if (age > TicRate / 2)
                {
                    staleAttacking++;
                }
            }
        }

        if (maxStale > MaxStaleVisual)
        {
            MaxStaleVisual = maxStale;
        }

        int mirrorDelta = Math.Abs((int)blocking - ServerActive);
        if (ServerActive != (int)blocking)
        {
            if (MismatchStartMs == 0)
            {
                MismatchStartMs = NowMs;
            }
            else if (NowMs - MismatchStartMs >= 1000)
            {
                if (NowMs - LastDesyncMs >= 1000)
                {
                    LastDesyncMs = NowMs;
                    DesyncWarnings.Add(
                        "[MIRROR PARITY DESYNC] mismatch for >1s: server active monsters=" + ServerActive
                        + ", client blocking mirrors=" + (int)blocking + ", gametic=" + GameTic);
                }
            }
        }
        else
        {
            MismatchStartMs = 0;
        }

        int interval = Max(Interval, 1);
        if (GameTic - LastSampleTic < interval)
        {
            return;
        }

        ulong lifetimeLocal = HealthRepairs + StateRepairs;
        ulong lifetimeHard = RespawnRepairs + DeathRepairs;
        ulong winPassiveClient = LifetimePassiveClient - WindowPassiveClient;
        ulong winPassiveAuth = LifetimePassiveAuth - WindowPassiveAuth;
        ulong winLocal = lifetimeLocal - WindowLocalRepairs;
        ulong winHard = lifetimeHard - WindowHardRepairs;
        ulong winFaults = FaultReports - WindowFaultReports;
        bool playerOk = ConsolePlayer >= 0 && ConsolePlayer < MaxPlayers;
        PlayerSlot? player = playerOk ? Players[ConsolePlayer] : null;
        bool hasActor = player != null && player.HasActor;
        int latest = Max(ClientTic - 1, 0);
        UserCmd cmd = Commands[latest % LocalCmdTics];
        bool attack = (cmd.Buttons & BtAttack) != 0;
        bool move = cmd.Forward != 0 || cmd.Side != 0 || cmd.Up != 0;
        bool compat = player != null && (player.Cheats & CfPredicting) != 0;
        double pspX = 0;
        double pspY = 0;
        bool pspAlive = false;
        if (player != null && player.Weapon != null)
        {
            pspX = player.Weapon.X;
            pspY = player.Weapon.Y;
            pspAlive = player.Weapon.Alive;
        }

        ulong now = NowMs;
        string csvPath = AppData + "/hcde_predict_metrics.csv";
        if (!CsvHeaderProbed)
        {
            CsvHeaderProbed = true;
            bool needsHeader = true;
            if (Files.TryGetValue(csvPath, out string? existing))
            {
                needsHeader = existing.Length <= 0;
            }

            if (needsHeader)
            {
                Files[csvPath] = CsvHeader;
            }
        }

        int minPrinted = MinAvailable == int.MaxValue ? availableTics : MinAvailable;
        string row = string.Join(",", new string[]
        {
            now.ToString(CultureInfo.InvariantCulture),
            GameTic.ToString(CultureInfo.InvariantCulture),
            ClientTic.ToString(CultureInfo.InvariantCulture),
            totalTics.ToString(CultureInfo.InvariantCulture),
            availableTics.ToString(CultureInfo.InvariantCulture),
            backlog.ToString(CultureInfo.InvariantCulture),
            lowestSequence.ToString(CultureInfo.InvariantCulture),
            AuthoritySlot.ToString(CultureInfo.InvariantCulture),
            newest.ToString(CultureInfo.InvariantCulture),
            ack.ToString(CultureInfo.InvariantCulture),
            ackLag.ToString(CultureInfo.InvariantCulture),
            U(peer != null ? peer.RxServerSnapshotSequence : 0),
            U(peer != null ? peer.RxControlSequence : 0),
            U(peer != null ? peer.RxSequence : 0),
            U(peer != null ? peer.SnapshotReceived : 0),
            U(peer != null ? peer.ClientCommandSent : 0),
            U(winPassiveClient),
            U(winPassiveAuth),
            U(winLocal),
            U(winHard),
            U(winFaults),
            minPrinted.ToString(CultureInfo.InvariantCulture),
            MaxAckLag.ToString(CultureInfo.InvariantCulture),
            MaxStaleVisual.ToString(CultureInfo.InvariantCulture),
            MaxBacklog.ToString(CultureInfo.InvariantCulture),
            InvasionName,
            Wave.ToString(CultureInfo.InvariantCulture),
            ServerActive.ToString(CultureInfo.InvariantCulture),
            U(tracked),
            U(blocking),
            U(projectiles),
            U(staleAttacking),
            mirrorDelta.ToString(CultureInfo.InvariantCulture),
            (EnterTic - LastGameUpdate).ToString(CultureInfo.InvariantCulture),
            LagName,
            Predicting ? "1" : "0",
            compat ? "1" : "0",
            attack ? "1" : "0",
            move ? "1" : "0",
            cmd.Forward.ToString(CultureInfo.InvariantCulture),
            cmd.Side.ToString(CultureInfo.InvariantCulture),
            cmd.Up.ToString(CultureInfo.InvariantCulture),
            "0x" + cmd.Buttons.ToString("x8", CultureInfo.InvariantCulture),
            F2(hasActor ? player!.X : 0),
            F2(hasActor ? player!.Y : 0),
            F2(hasActor ? player!.Z : 0),
            F3(hasActor ? player!.VelX : 0),
            F3(hasActor ? player!.VelY : 0),
            F3(hasActor ? player!.VelZ : 0),
            F4(player != null ? player.Bob : 0),
            (hasActor ? player!.Health : 0).ToString(CultureInfo.InvariantCulture),
            F2(player != null ? player.ViewHeight : 0),
            F2(player != null && hasActor ? player.ViewZ - player.Z : 0),
            F2(pspX),
            F2(pspY),
            pspAlive ? "1" : "0",
            U(Room),
        }) + "\n";
        if (Files.TryGetValue(csvPath, out string? prior))
        {
            Files[csvPath] = prior + row;
        }
        else
        {
            Files[csvPath] = row;
        }

        if (Level >= 2 && EnterTic >= PauseGraceUntil)
        {
            bool underPressure = MaxBacklog > Max(TicDup * 2, 2);
            bool warnAck = MaxAckLag >= AckLagThreshold;
            bool warnMirror = mirrorDelta >= MirrorThreshold;
            bool warnAvailable = MinAvailable != int.MaxValue && MinAvailable <= 1 && underPressure;
            bool warnStale = staleAttacking > 0;
            bool warnPassive = winPassiveClient >= (uint)PassiveThreshold;
            bool warnHard = winHard >= 2;
            if ((warnAck || warnMirror || warnAvailable || warnStale || warnPassive || warnHard) && ReportElapsed(LastSoftWarnMs, 250, out ulong softStamp))
            {
                LastSoftWarnMs = softStamp;
                string tags =
                    (warnAck ? "ack-lag " : "") +
                    (warnAvailable ? "available-low " : "") +
                    (warnMirror ? "mirror-drift " : "") +
                    (warnStale ? "stale-attack " : "") +
                    (warnPassive ? "passive-storm " : "") +
                    (warnHard ? "hard-churn " : "");
                string warnPath = AppData + "/hcde_prediction_softwarn.log";
                string line =
                    now + " HCDE predict softwarn triggers=[" + tags + "] ack=" + ack + "/" + newest
                    + " (lag=" + ackLag + " max=" + MaxAckLag + ") avail=" + availableTics
                    + " (min=" + minPrinted + ") backlog=" + backlog + " (max=" + MaxBacklog + ") mirror="
                    + mirrorDelta + " (blocking=" + blocking + " server=" + ServerActive + ") stale-attack="
                    + staleAttacking + " max-stale=" + MaxStaleVisual + " passive-client=" + winPassiveClient
                    + " passive-auth=" + winPassiveAuth + " local-repairs=" + winLocal + " hard-repairs=" + winHard
                    + " fault-reports=" + winFaults + " invasion=" + InvasionName + " wave=" + Wave
                    + " snap-count=" + (peer != null ? peer.SnapshotReceived : 0)
                    + " input-sent=" + (peer != null ? peer.ClientCommandSent : 0)
                    + " viewheight=" + F2(player != null ? player.ViewHeight : 0)
                    + " view-z-off=" + F2(player != null && hasActor ? player.ViewZ - player.Z : 0)
                    + " psprite-y=" + F2(pspY)
                    + " pos=(" + F1(hasActor ? player!.X : 0) + "," + F1(hasActor ? player!.Y : 0) + "," + F1(hasActor ? player!.Z : 0) + ")"
                    + " vel=(" + F2(hasActor ? player!.VelX : 0) + "," + F2(hasActor ? player!.VelY : 0) + "," + F2(hasActor ? player!.VelZ : 0) + ")"
                    + " health=" + (hasActor ? player!.Health : 0) + " room=" + Room + "\n";
                if (Files.TryGetValue(warnPath, out string? warnPrior))
                {
                    Files[warnPath] = warnPrior + line;
                }
                else
                {
                    Files[warnPath] = line;
                }

                SoftWarnings.Add(
                    "HCDE predict softwarn ack=" + ack + "/" + newest + " (lag=" + ackLag + ") avail=" + availableTics
                    + " backlog=" + backlog + " mirror-delta=" + mirrorDelta + " stale-attack=" + staleAttacking
                    + " passive=" + winPassiveClient + " invasion=" + InvasionName + " wave=" + Wave + " triggers=" + tags);
                if (HudDebug)
                {
                    HudLines.Add(
                        "HCDE predict softwarn ack=" + ack + "/" + newest + " (lag=" + ackLag + ") avail=" + availableTics
                        + " backlog=" + backlog + " mirror-delta=" + mirrorDelta + " stale-attack=" + staleAttacking
                        + " passive=" + winPassiveClient + "\n");
                }

                bool dump = warnMirror || warnStale || warnPassive || warnHard;
                if (Level >= 3 && dump && ReportElapsed(LastTraceDumpMs, 5000, out ulong traceStamp))
                {
                    LastTraceDumpMs = traceStamp;
                    string tracePath = AppData + "/hcde_prediction_softwarn_trace_" + now + ".txt";
                    Traces.Add(new TraceDump(tracePath, "net", "Debug"));
                }
            }
        }

        WindowPassiveClient = LifetimePassiveClient;
        WindowPassiveAuth = LifetimePassiveAuth;
        WindowLocalRepairs = lifetimeLocal;
        WindowHardRepairs = lifetimeHard;
        WindowFaultReports = FaultReports;
        MinAvailable = availableTics;
        MaxAckLag = ackLag;
        MaxStaleVisual = maxStale;
        MaxBacklog = backlog;
        LastSampleTic = GameTic;
    }

    bool ReportElapsed(ulong last, ulong interval, out ulong updated)
    {
        ulong now = NowMs;
        if (last != 0 && now - last < interval)
        {
            updated = last;
            return false;
        }

        updated = now;
        return true;
    }

    static ClientSlot[] NewClients()
    {
        var slots = new ClientSlot[MaxPlayers];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ClientSlot();
        }

        return slots;
    }

    static PeerSlot[] NewPeers()
    {
        var slots = new PeerSlot[MaxPlayers];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new PeerSlot();
        }

        return slots;
    }

    static int Max(int a, int b)
    {
        return a > b ? a : b;
    }

    static string U(ulong value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    static string F1(double value)
    {
        return value.ToString("0.0", CultureInfo.InvariantCulture);
    }

    static string F2(double value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }

    static string F3(double value)
    {
        return value.ToString("0.000", CultureInfo.InvariantCulture);
    }

    static string F4(double value)
    {
        return value.ToString("0.0000", CultureInfo.InvariantCulture);
    }
}
