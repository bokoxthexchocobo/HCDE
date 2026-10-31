namespace HCDE.Playsim;

/// <summary>
/// Net command tail from <c>d_net.cpp</c> lines 9009-9260: <c>DEM_CROUCH</c>
/// through the unknown-command error, then <c>RunScript</c>.
/// <c>Net_TrySkipCommand</c> starts at 9261. Opcodes whose cases sit above
/// this band take the default error here. <c>cht_Morph</c>, <c>FindActor</c>,
/// <c>Massacre</c>, <c>GetReplacement</c>, <c>RemoveClass</c>,
/// <c>P_ConversationCommand</c>, <c>Net_ReadWeapon</c>,
/// <c>P_IsPlayerTotallyFrozen</c>, <c>HCDEPredator_HandleNetEvent</c>, and
/// <c>NetworkClients.InGame</c> stay injected.
/// </summary>
public static class VmNetDoCommand1358Slice
{
    public const string ReadPastEnd = "Attempted to read past end of stream";

    public const int GsLevel = 0;
    public const int PstLive = 0;
    public const int BtJump = 1 << 2;
    public const int NumWeaponSlots = 10;
    public const int AcsAlways = 2;
    public const int AcsNet = 8;

    public const int DemCrouch = 39;
    public const int DemMorphex = 43;
    public const int DemAddController = 48;
    public const int DemDelController = 49;
    public const int DemKillClassCheat = 50;
    public const int DemAddSlotDefault = 55;
    public const int DemAddSlot = 56;
    public const int DemSetSlot = 57;
    public const int DemConvReply = 59;
    public const int DemConvClose = 60;
    public const int DemConvNull = 61;
    public const int DemSetPitchLimit = 63;
    public const int DemRevertCamera = 66;
    public const int DemSetSlotPnum = 67;
    public const int DemRemove = 68;
    public const int DemFinishGame = 69;
    public const int DemNetEvent = 70;
    public const int DemEndScreenJob = 73;
    public const int DemZscCmd = 74;
    public const int DemChangeSkill = 75;
    public const int DemKick = 76;
    public const int DemReadied = 77;
    public const int DemWeapSelect = 78;
    public const int DemUseFlechette = 79;

    public sealed class NetStream
    {
        public byte[] Buf = Array.Empty<byte>();
        public int Pos;
        public int End;
        public int Size => End - Pos;
    }

    public sealed class Actor
    {
        public object? Level;
    }

    public sealed class ActorClass
    {
        public string TypeName = "";
        public bool IsActor = true;
        public bool IsWeapon;
        public Func<Level, ActorClass?>? Replace;
    }

    public sealed class SlotCall
    {
        public string Kind = "";
        public int Slot;
        public ActorClass? Type;
        public bool Feedback;
    }

    public sealed class Weapons
    {
        public readonly List<ActorClass?>[] Slots = new List<ActorClass?>[NumWeaponSlots];
        public readonly List<SlotCall> Calls = new();

        public Weapons()
        {
            for (int i = 0; i < Slots.Length; i++)
                Slots[i] = new List<ActorClass?>();
        }
    }

    public sealed class Player
    {
        public Actor? Mo;
        public Actor? Camera;
        public int State = PstLive;
        public uint OldButtons;
        public sbyte CrouchDir;
        public sbyte Crouching;
        public bool SettingsController;
        public string Name = "";
        public double MinPitch;
        public double MaxPitch;
        public Weapons Weapons = new();
    }

    public sealed class LevelChange
    {
        public string? Name;
        public int Position;
        public int Flags;
    }

    public sealed class ConsoleCall
    {
        public int Player;
        public string Name = "";
        public int A;
        public int B;
        public int C;
        public bool Manual;
        public bool FromScript;
    }

    public sealed class NetCmd
    {
        public int Player;
        public string Name = "";
        public byte[] Bytes = Array.Empty<byte>();
    }

    public sealed class ScriptCall
    {
        public object? PawnLevel;
        public Actor? Pawn;
        public object? Line;
        public int Number;
        public string Map = "";
        public int[] Args = Array.Empty<int>();
        public int Count;
        public int Flags;
    }

    public sealed class Level
    {
        public string MapName = "";
        public Func<bool, string, int>? Massacre;
        public Func<ActorClass, int>? RemoveClass;
        public readonly List<LevelChange> Changes = new();
        public readonly List<ConsoleCall> Consoles = new();
        public readonly List<NetCmd> NetCommands = new();
        public readonly List<ScriptCall> Scripts = new();
    }

    public sealed class World
    {
        public int GameState = GsLevel;
        public int ConsolePlayer;
        public int NextSkill;
        public int GameTic;
        public int ClientTic;
        public uint Room;
        public Level? Primary;
        public List<Player> Players = new();
        public Func<Player, bool>? TotallyFrozen;
        public Func<string, ActorClass?>? FindActor;
        public Func<Player, ActorClass?, bool, string>? Morph;
        public Func<bool>? IsAuthority;
        public Action<int, int, NetStream>? Conversation;
        public Func<NetStream, ActorClass?>? ReadWeapon;
        public Func<int, string, int, int, int, bool>? Predator;
        public Func<int, bool>? InGame;
        public readonly List<string> Prints = new();
        public readonly List<(string Channel, string Text)> Warnings = new();
        public readonly List<int> Disconnects = new();
        public readonly List<int> Readied = new();
        public readonly List<(int Player, int Slot)> WeaponPicks = new();
        public readonly List<int> Flechettes = new();
        public int EndScreens;
        public readonly List<string> Trace = new();
    }

    public static void DoCommand(int cmd, NetStream stream, int player, World world)
    {
        switch (cmd)
        {
            case DemCrouch:
                Crouch(world, player);
                break;
            case DemMorphex:
                MorphEx(world, stream, player);
                break;
            case DemAddController:
                Controller(world, stream, true);
                break;
            case DemDelController:
                Controller(world, stream, false);
                break;
            case DemKillClassCheat:
                KillClass(world, stream);
                break;
            case DemRemove:
                RemoveActors(world, stream);
                break;
            case DemConvReply:
            case DemConvClose:
            case DemConvNull:
                Talk(world, cmd, player, stream);
                break;
            case DemSetSlot:
            case DemSetSlotPnum:
                SetSlot(world, stream, cmd, player);
                break;
            case DemAddSlot:
                AddOne(world, stream, player, false);
                break;
            case DemAddSlotDefault:
                AddOne(world, stream, player, true);
                break;
            case DemSetPitchLimit:
                Pitch(world, stream, player);
                break;
            case DemRevertCamera:
                Revert(world, player);
                break;
            case DemFinishGame:
                Finish(world);
                break;
            case DemNetEvent:
                NetEvent(world, stream, player);
                break;
            case DemEndScreenJob:
                world.EndScreens++;
                Mark(world, "end-screen");
                break;
            case DemReadied:
                world.Readied.Add(player);
                Mark(world, "readied");
                break;
            case DemZscCmd:
                Zsc(world, stream, player);
                break;
            case DemChangeSkill:
                world.NextSkill = ReadInt32(stream);
                break;
            case DemKick:
                Kick(world, stream, player);
                break;
            case DemWeapSelect:
                world.WeaponPicks.Add((player, ReadInt8(stream)));
                Mark(world, "weapon");
                break;
            case DemUseFlechette:
                world.Flechettes.Add(player);
                Mark(world, "flechette");
                break;
            default:
                throw new InvalidOperationException($"Unknown net command: {cmd}");
        }
    }

    /// <summary>
    /// <c>RunScript</c> from lines 9233-9248. A null pawn returns before any read.
    /// </summary>
    public static void RunScript(NetStream stream, Actor? pawn, int snum, int argn, int always, World world)
    {
        if (pawn == null)
            return;

        var arg = new int[4];
        for (int i = 0; i < argn; i++)
        {
            int argval = ReadInt32(stream);
            if ((uint)i < 4)
                arg[i] = argval;
        }

        Level level = world.Primary ?? throw new NullReferenceException("primaryLevel");
        level.Scripts.Add(new ScriptCall
        {
            PawnLevel = pawn.Level,
            Pawn = pawn,
            Line = null,
            Number = snum,
            Map = level.MapName,
            Args = arg,
            Count = Math.Min(4, argn),
            Flags = AcsNet | always,
        });
        Mark(world, "script");
    }

    public static byte ReadInt8(NetStream stream)
    {
        Need(stream, 1);
        byte v = stream.Buf[stream.Pos];
        stream.Pos++;
        return v;
    }

    public static short ReadInt16(NetStream stream)
    {
        Need(stream, 2);
        int v = (stream.Buf[stream.Pos] << 8) | stream.Buf[stream.Pos + 1];
        stream.Pos += 2;
        return (short)v;
    }

    public static int ReadInt32(NetStream stream)
    {
        Need(stream, 4);
        int pos = stream.Pos;
        int v = (stream.Buf[pos] << 24)
            | (stream.Buf[pos + 1] << 16)
            | (stream.Buf[pos + 2] << 8)
            | stream.Buf[pos + 3];
        stream.Pos += 4;
        return v;
    }

    public static string ReadStringConst(NetStream stream)
    {
        int len = 0;
        while (len < stream.Size)
        {
            if (stream.Pos < 0 || stream.Pos + len >= stream.Buf.Length)
                throw new InvalidOperationException(ReadPastEnd);
            if (stream.Buf[stream.Pos + len] == 0)
                break;
            len++;
        }

        if (len == stream.Size || stream.Pos < 0 || stream.Pos + len >= stream.Buf.Length)
            throw new InvalidOperationException(ReadPastEnd);
        var chars = new char[len];
        for (int i = 0; i < len; i++)
            chars[i] = (char)stream.Buf[stream.Pos + i];
        stream.Pos += len + 1;
        return new string(chars);
    }

    static void Crouch(World world, int player)
    {
        if (world.GameState != GsLevel)
            return;
        Player who = At(world, player);
        if (who.Mo == null || who.State != PstLive || (who.OldButtons & (uint)BtJump) != 0)
            return;
        if (world.TotallyFrozen == null)
            throw new InvalidOperationException("P_IsPlayerTotallyFrozen");
        Mark(world, "frozen");
        if (world.TotallyFrozen(who))
            return;
        who.Crouching = (sbyte)(who.CrouchDir < 0 ? 1 : -1);
    }

    static void MorphEx(World world, NetStream stream, int player)
    {
        string name = ReadStringConst(stream);
        ActorClass? cls = Find(world, name);
        if ((uint)player >= (uint)world.Players.Count)
            throw new IndexOutOfRangeException("players");
        if (world.Morph == null)
            throw new InvalidOperationException("cht_Morph");
        Mark(world, "morph");
        string msg = world.Morph(world.Players[player], cls, false) ?? "";
        if (player != world.ConsolePlayer)
            return;
        bool shown = msg.Length > 0 && msg[0] != '\0';
        Print(world, (shown ? msg : "Morph failed.") + "\n");
    }

    static void Controller(World world, NetStream stream, bool enable)
    {
        int playernum = ReadInt8(stream);
        Player who = At(world, playernum);
        who.SettingsController = enable;
        if (world.ConsolePlayer == playernum)
        {
            Print(world, enable
                ? "You can now control game settings\n"
                : "You can no longer control game settings\n");
            return;
        }

        if (!Authority(world))
            return;
        Print(world, enable
            ? $"{who.Name} [{playernum}] is now a settings controller\n"
            : $"{who.Name} [{playernum}] is no longer a settings controller\n");
    }

    static void KillClass(World world, NetStream stream)
    {
        string name = ReadStringConst(stream);
        ActorClass? cls = Find(world, name);
        if (cls == null)
        {
            Print(world, $"{name} is not an actor class.\n");
            return;
        }

        Level level = world.Primary ?? throw new NullReferenceException("primaryLevel");
        int killcount = Massacre(world, level, cls.TypeName);
        ActorClass? rep = Replacement(world, cls, level);
        if (!ReferenceEquals(cls, rep))
        {
            if (rep == null)
                throw new NullReferenceException("cls_rep");
            killcount += Massacre(world, level, rep.TypeName);
        }

        Print(world, $"Killed {killcount} monsters of type {name}.\n");
    }

    static void RemoveActors(World world, NetStream stream)
    {
        string name = ReadStringConst(stream);
        ActorClass? cls = Find(world, name);
        if (cls == null || !cls.IsActor)
        {
            Print(world, $"{name} is not an actor class.\n");
            return;
        }

        Level level = world.Primary ?? throw new NullReferenceException("primaryLevel");
        int removed = Remove(world, level, cls);
        ActorClass? rep = Replacement(world, cls, level);
        if (!ReferenceEquals(cls, rep))
        {
            if (rep == null)
                throw new NullReferenceException("cls_rep");
            removed += Remove(world, level, rep);
        }

        Print(world, $"Removed {removed} actors of type {name}.\n");
    }

    static void Talk(World world, int cmd, int player, NetStream stream)
    {
        if (world.Conversation == null)
            throw new InvalidOperationException("P_ConversationCommand");
        Mark(world, "talk");
        world.Conversation(cmd, player, stream);
    }

    static void SetSlot(World world, NetStream stream, int cmd, int player)
    {
        int pnum = player;
        if (cmd == DemSetSlotPnum)
            pnum = ReadInt8(stream);
        int slot = ReadInt8(stream);
        int count = ReadInt8(stream);
        Player who = At(world, pnum);
        if (slot < NumWeaponSlots)
        {
            who.Weapons.Calls.Add(new SlotCall { Kind = "clear", Slot = slot });
            who.Weapons.Slots[slot].Clear();
            Mark(world, "clear");
        }

        bool feedback = pnum == world.ConsolePlayer;
        for (int i = 0; i < count; i++)
        {
            ActorClass? wpn = ReadWeapon(world, stream);
            Place(world, who.Weapons, slot, wpn, feedback, false);
        }
    }

    static void AddOne(World world, NetStream stream, int player, bool asDefault)
    {
        int slot = ReadInt8(stream);
        ActorClass? wpn = ReadWeapon(world, stream);
        Player who = At(world, player);
        bool feedback = player == world.ConsolePlayer;
        Place(world, who.Weapons, slot, wpn, feedback, asDefault);
    }

    static void Place(World world, Weapons weapons, int slot, ActorClass? wpn, bool feedback, bool asDefault)
    {
        weapons.Calls.Add(new SlotCall
        {
            Kind = asDefault ? "default" : "add",
            Slot = slot,
            Type = wpn,
            Feedback = feedback,
        });
        Mark(world, asDefault ? "add-default" : "add");
        if (asDefault)
        {
            if (wpn == null || !wpn.IsWeapon)
                return;
        }
        else if (wpn == null)
        {
            return;
        }

        if ((uint)slot >= (uint)NumWeaponSlots)
            throw new IndexOutOfRangeException("Slots");
        weapons.Slots[slot].Add(wpn);
    }

    static void Pitch(World world, NetStream stream, int player)
    {
        int up = ReadInt8(stream);
        At(world, player).MinPitch = -up;
        int down = ReadInt8(stream);
        At(world, player).MaxPitch = down;
    }

    static void Revert(World world, int player)
    {
        Player who = At(world, player);
        who.Camera = who.Mo;
        Mark(world, "camera");
    }

    static void Finish(World world)
    {
        Level level = world.Primary ?? throw new NullReferenceException("primaryLevel");
        level.Changes.Add(new LevelChange());
        Mark(world, "change-level");
    }

    static void NetEvent(World world, NetStream stream, int player)
    {
        string name = ReadStringConst(stream);
        int argn = ReadInt8(stream);
        int a = ReadInt32(stream);
        int b = ReadInt32(stream);
        int c = ReadInt32(stream);
        bool manual = ReadInt8(stream) != 0;
        if (world.Predator == null)
            throw new InvalidOperationException("HCDEPredator_HandleNetEvent");
        Mark(world, "predator");
        bool handled = world.Predator(player, name, a, b, c);
        if (handled)
        {
            _ = argn;
            return;
        }

        Level level = world.Primary ?? throw new NullReferenceException("primaryLevel");
        level.Consoles.Add(new ConsoleCall
        {
            Player = player,
            Name = name,
            A = a,
            B = b,
            C = c,
            Manual = manual,
            FromScript = false,
        });
        Mark(world, "console");
    }

    static void Zsc(World world, NetStream stream, int player)
    {
        string name = ReadStringConst(stream);
        uint size = unchecked((uint)ReadInt16(stream));
        var buffer = new List<byte>();
        for (uint i = 0; i < size; i++)
            buffer.Add(ReadInt8(stream));
        Level level = world.Primary ?? throw new NullReferenceException("primaryLevel");
        level.NetCommands.Add(new NetCmd
        {
            Player = player,
            Name = name,
            Bytes = buffer.ToArray(),
        });
        Mark(world, "net-cmd");
    }

    static void Kick(World world, NetStream stream, int player)
    {
        int pNum = ReadInt8(stream);
        if (pNum == world.ConsolePlayer)
            throw new InvalidOperationException("You have been kicked from the game");
        if (world.InGame == null)
            throw new InvalidOperationException("NetworkClients.InGame");
        Mark(world, "in-game");
        if (!world.InGame(pNum))
            return;
        Player who = At(world, pNum);
        Print(world, $"{who.Name} [{pNum}] has been kicked from the game\n");
        world.Warnings.Add(("net",
            $"kick command disconnecting player={pNum} by={player} gametic={world.GameTic} clienttic={world.ClientTic} room={world.Room}"));
        Mark(world, "warn");
        world.Disconnects.Add(pNum);
        Mark(world, "disconnect");
    }

    static Player At(World world, int index)
    {
        if ((uint)index >= (uint)world.Players.Count)
            throw new IndexOutOfRangeException("players");
        return world.Players[index];
    }

    static ActorClass? Find(World world, string name)
    {
        if (world.FindActor == null)
            throw new InvalidOperationException("FindActor");
        Mark(world, "find");
        return world.FindActor(name);
    }

    static bool Authority(World world)
    {
        if (world.IsAuthority == null)
            throw new InvalidOperationException("I_IsLocalHCDEServiceAuthority");
        Mark(world, "authority");
        return world.IsAuthority();
    }

    static int Massacre(World world, Level level, string typeName)
    {
        if (level.Massacre == null)
            throw new InvalidOperationException("Massacre");
        Mark(world, "massacre");
        return level.Massacre(false, typeName);
    }

    static int Remove(World world, Level level, ActorClass cls)
    {
        if (level.RemoveClass == null)
            throw new InvalidOperationException("RemoveClass");
        Mark(world, "remove");
        return level.RemoveClass(cls);
    }

    static ActorClass? Replacement(World world, ActorClass cls, Level level)
    {
        if (cls.Replace == null)
            throw new InvalidOperationException("GetReplacement");
        Mark(world, "replace");
        return cls.Replace(level);
    }

    static ActorClass? ReadWeapon(World world, NetStream stream)
    {
        if (world.ReadWeapon == null)
            throw new InvalidOperationException("Net_ReadWeapon");
        Mark(world, "weapon-read");
        return world.ReadWeapon(stream);
    }

    static void Need(NetStream stream, int n)
    {
        if (n < 1 || stream.Pos < 0 || stream.Size < n || stream.Pos > stream.Buf.Length - n)
            throw new InvalidOperationException(ReadPastEnd);
    }

    static void Print(World world, string text)
    {
        world.Prints.Add(text);
        Mark(world, "print");
    }

    static void Mark(World world, string token)
    {
        world.Trace.Add(token);
    }
}
