namespace HCDE.Playsim;

/// <summary>
/// Exit check and line activation from <c>src/playsim/p_spec.cpp</c> lines 1-242.
/// <c>P_TestActivateLine</c> stops at the <c>SPAC_UseThrough</c> test. The
/// <c>|= SPAC_Use</c> body at line 243 belongs to chunk 884.
/// </summary>
public sealed class VmSpecActivate1363Slice
{
    public const int Df2KillMonsters = 1 << 17;
    public const int DfNoExit = 1 << 10;
    public const int TelefragDamage = 1000000;
    public const int ClusterHub = 1;
    public const int MlRepeatSpecial = 0x200;
    public const int MlFirstSideOnly = 0x00800000;
    public const int Mf2OnMobj = 0x800;
    public const int DmsgSpammy = 4;
    public const int Level2DummySwitches = 0x00400000;
    public const int GaCompleted = 12;
    public const int SpacCross = 1 << 0;
    public const int SpacUse = 1 << 1;
    public const int SpacMCross = 1 << 2;
    public const int SpacImpact = 1 << 3;
    public const int SpacPush = 1 << 4;
    public const int SpacUseThrough = 1 << 6;
    public const int SpacUseBack = 1 << 10;
    public const int SpacWalking = 1 << 13;
    public const int GenericFloor = 200;
    public const int GenericCrusher = 205;
    public const string NameExit = "Exit";

    public sealed class Actor
    {
        public int Health { get; set; }
        public Player? Player { get; set; }
        public double Z { get; set; }
        public double FloorZ { get; set; }
        public int Flags2 { get; set; }
    }

    public sealed class Player
    {
        public int Index { get; set; }
        public Actor? Mo { get; set; }
        public Actor? Camera { get; set; }
        public string Name { get; set; } = "";
    }

    public sealed class SpecLevel
    {
        public int LevelNum { get; set; }
        public int Flags2 { get; set; }
    }

    public sealed class MapLine
    {
        public int Flags { get; set; }
        public int Special { get; set; }
        public int Activation { get; set; }
        public int LockNumber { get; set; }
        public int[] Args { get; } = new int[5];
        public object? SideDef0 { get; set; }
        public int Index { get; set; }
        public int LineNum { get; set; }
        public SpecLevel? Level { get; set; }
    }

    public sealed class LevelInfo
    {
        public int Cluster { get; set; }
    }

    public sealed class ClusterInfo
    {
        public int Flags { get; set; }
    }

    public sealed class OptPos
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double Z { get; init; }
    }

    public sealed class ShouldFlag
    {
        public bool Value { get; set; }
    }

    public sealed class ExecuteCall
    {
        public int Special { get; init; }
        public Actor? Mo { get; init; }
        public bool BackSide { get; init; }
        public int Arg0 { get; init; }
        public int Arg1 { get; init; }
        public int Arg2 { get; init; }
        public int Arg3 { get; init; }
        public int Arg4 { get; init; }
    }

    public delegate bool Continuation(
        MapLine line,
        Actor? mo,
        int side,
        int activationType,
        OptPos? optpos,
        int lineActivation);

    public int DmFlags2 { get; set; }
    public int DmFlags { get; set; }
    public int KilledMonsters { get; set; }
    public int TotalMonsters { get; set; }
    public bool Deathmatch { get; set; }
    public bool AlwaysApplyDmFlags { get; set; }
    public bool Multiplayer { get; set; }
    public int Cluster { get; set; }
    public int GameAction { get; set; }
    public int Developer { get; set; }
    public int ConsolePlayer { get; set; }
    public bool HasRumble { get; set; }
    public int LevelReads { get; private set; }
    public int PreCalls { get; private set; }
    public int UseThroughCalls { get; private set; }
    public int TailCalls { get; private set; }
    public OptPos? ObservedOpt { get; private set; }
    public List<Player> Players { get; } = new();
    public List<string> Messages { get; } = new();
    public List<(Actor Target, Actor Inflictor, Actor Source, int Amount, string Type)> Damages { get; } = new();
    public List<int> ClusterQueries { get; } = new();
    public List<(Actor? Mo, int Lock, bool Remote)> KeyChecks { get; } = new();
    public List<ExecuteCall> Executes { get; } = new();
    public List<(int Special, Actor? Mo, int Activation)> Activated { get; } = new();
    public List<(object Side, int UseAgain, int Special)> Switches { get; } = new();
    public List<(Actor? Mo, int Activation, int LevelNum, int LineNum, int Special)> Rumbles { get; } = new();
    public List<(int Player, int Index, int Special, int Arg0, bool Success, bool FromServer)> Diags { get; } = new();
    public List<(int Index, int Special, bool Success)> Checksums { get; } = new();
    public List<int> LineIdQueries { get; } = new();
    public List<int> TagQueries { get; } = new();

    public Func<int, ClusterInfo?>? FindClusterInfo { get; set; }
    public Continuation? UseThroughTail { get; set; }
    public Continuation? Tail { get; set; }
    public Action<MapLine, Actor?, int, ShouldFlag>? WorldLinePreActivated { get; set; }
    public Action<MapLine, Actor?, int>? WorldLineActivated { get; set; }
    public Func<Actor?, int, bool, bool>? CheckKeys { get; set; }
    public Func<MapLine, Actor?, bool, int>? ExecuteSpecial { get; set; }
    public Func<MapLine, int, bool>? LineHasId { get; set; }
    public Func<int, int>? FindFirstSectorFromTag { get; set; }

    public bool CheckIfExitIsGood(Actor? self, LevelInfo? info)
    {
        if (self == null)
            return true;

        if ((DmFlags2 & Df2KillMonsters) != 0 && KilledMonsters != TotalMonsters)
            return false;

        if ((Deathmatch || AlwaysApplyDmFlags) && (DmFlags & DfNoExit) != 0)
        {
            Damages.Add((self, self, self, TelefragDamage, NameExit));
            return false;
        }

        if (self.Health <= 0 && !Multiplayer && info != null && info.Cluster == Cluster)
        {
            if (FindClusterInfo == null)
                throw new InvalidOperationException("FindClusterInfo");
            ClusterQueries.Add(Cluster);
            var clusterdef = FindClusterInfo(Cluster);
            if (clusterdef != null && (clusterdef.Flags & ClusterHub) != 0)
                return false;
        }

        if (Deathmatch && GameAction != GaCompleted)
        {
            if (self.Player == null)
                throw new InvalidOperationException("GetName");
            Messages.Add($"{self.Player.Name} exited the level\n");
        }

        return true;
    }

    public bool Activate(MapLine line, Actor? mo, int side, int activationType)
    {
        return ActivateLine(line, mo, side, activationType, null);
    }

    public bool RemoteActivate(
        MapLine line,
        Actor? mo,
        int side,
        int activationType,
        double optx,
        double opty,
        double optz)
    {
        var optpos = new OptPos { X = optx, Y = opty, Z = optz };
        return ActivateLine(line, mo, side, activationType, optpos);
    }

    public bool TestActivateLine(MapLine line, Actor? mo, int side, int activationType, OptPos? optpos)
    {
        _ = ReadLevel(line);
        var lineActivation = line.Activation;
        if (((line.Flags & MlFirstSideOnly) != 0 && side == 1) || line.Special == 0)
            return false;

        if ((activationType & (SpacCross | SpacMCross)) != 0 && (lineActivation & SpacWalking) != 0)
        {
            if (mo == null)
                throw new InvalidOperationException("mo");
            if (mo.Z > mo.FloorZ && (mo.Flags2 & Mf2OnMobj) == 0)
                return false;
        }

        if ((lineActivation & SpacUseThrough) != 0)
        {
            if (UseThroughTail == null)
                throw new InvalidOperationException("P_TestActivateLine");
            UseThroughCalls++;
            return UseThroughTail(line, mo, side, activationType, optpos, lineActivation);
        }

        if (Tail == null)
            throw new InvalidOperationException("P_TestActivateLine");
        TailCalls++;
        return Tail(line, mo, side, activationType, optpos, lineActivation);
    }

    public bool ActivateLine(MapLine line, Actor? mo, int side, int activationType, OptPos? optpos)
    {
        ObservedOpt = optpos;
        if (!TestActivateLine(line, mo, side, activationType, optpos))
            return false;

        var level = ReadLevel(line);
        if (level == null)
            throw new InvalidOperationException("GetLevel");

        if (WorldLinePreActivated == null)
            throw new InvalidOperationException("WorldLinePreActivated");
        var should = new ShouldFlag { Value = true };
        WorldLinePreActivated(line, mo, activationType, should);
        PreCalls++;
        if (!should.Value)
            return false;

        var remote = RemoteSpecial(line.Special);
        if (line.LockNumber > 0)
        {
            if (CheckKeys == null)
                throw new InvalidOperationException("P_CheckKeys");
            KeyChecks.Add((mo, line.LockNumber, remote));
            if (!CheckKeys(mo, line.LockNumber, remote))
                return false;
        }

        if (line.Args.Length < 5)
            throw new IndexOutOfRangeException("args");
        LevelReads++;
        if (line.Level == null)
            throw new InvalidOperationException("GetLevel");
        if (ExecuteSpecial == null)
            throw new InvalidOperationException("P_ExecuteSpecial");
        var repeat = line.Flags & MlRepeatSpecial;
        Executes.Add(new ExecuteCall
        {
            Special = line.Special,
            Mo = mo,
            BackSide = side == 1,
            Arg0 = line.Args[0],
            Arg1 = line.Args[1],
            Arg2 = line.Args[2],
            Arg3 = line.Args[3],
            Arg4 = line.Args[4],
        });
        var buttonSuccess = ExecuteSpecial(line, mo, side == 1);
        if (buttonSuccess != 0)
        {
            if (WorldLineActivated == null)
                throw new InvalidOperationException("WorldLineActivated");
            WorldLineActivated(line, mo, activationType);
            Activated.Add((line.Special, mo, activationType));
        }

        var special = (int)(byte)line.Special;
        if (repeat == 0 && buttonSuccess != 0)
            line.Special = 0;

        if (buttonSuccess != 0)
        {
            if (SwitchActivation(activationType))
                ChangeSwitch(line, repeat, special);
            if ((uint)ConsolePlayer >= (uint)Players.Count)
                throw new IndexOutOfRangeException("players");
            var console = Players[ConsolePlayer];
            if (mo == console.Mo || mo == console.Camera)
            {
                if (RumbleActivation(activationType))
                {
                    if (mo == null)
                        throw new InvalidOperationException("GetClass");
                    if (HasRumble)
                    {
                        Rumbles.Add((mo, activationType, level.LevelNum, line.LineNum, line.Special));
                    }
                }
            }
        }
        else if (activationType == SpacImpact
            && (level.Flags2 & Level2DummySwitches) != 0
            && repeat == 0
            && (special < GenericFloor || special > GenericCrusher)
            && special != 0
            && QueryLineHasId(line, line.Args[0])
            && line.Args[0] != 0
            && QueryTag(line.Args[0]) == -1)
        {
            ChangeSwitch(line, repeat, special);
            line.Special = 0;
        }

        if (Developer >= DmsgSpammy && buttonSuccess != 0)
            Messages.Add($"Line special {special} activated on line {line.Index}\n");

        var playerNum = mo != null && mo.Player != null ? mo.Player.Index : -1;
        Diags.Add((playerNum, line.Index, special, line.Args[0], buttonSuccess != 0, false));
        Checksums.Add((line.Index, special, buttonSuccess != 0));
        return true;
    }

    SpecLevel? ReadLevel(MapLine line)
    {
        LevelReads++;
        return line.Level;
    }

    static bool RemoteSpecial(int special)
    {
        return special != 7 && special != 8 && (special < 11 || special > 14);
    }

    static bool SwitchActivation(int activationType)
    {
        return activationType == SpacUse || activationType == SpacImpact || activationType == SpacPush;
    }

    static bool RumbleActivation(int activationType)
    {
        return activationType == SpacUse
            || activationType == SpacPush
            || activationType == SpacUseThrough
            || activationType == SpacUseBack;
    }

    void ChangeSwitch(MapLine line, int useAgain, int special)
    {
        if (line.SideDef0 == null)
            throw new InvalidOperationException("P_ChangeSwitchTexture");
        Switches.Add((line.SideDef0, useAgain, special));
    }

    bool QueryLineHasId(MapLine line, int id)
    {
        if (LineHasId == null)
            throw new InvalidOperationException("LineHasId");
        LineIdQueries.Add(id);
        return LineHasId(line, id);
    }

    int QueryTag(int tag)
    {
        if (FindFirstSectorFromTag == null)
            throw new InvalidOperationException("FindFirstSectorFromTag");
        TagQueries.Add(tag);
        return FindFirstSectorFromTag(tag);
    }
}
