using System.Globalization;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// Echo string read through local echo compare from d_net_diag.cpp 43-316.
/// The CVAR, static slots, and comments inside the span are non-behavior.
/// The replicated-item struct starts at line 319.
/// </summary>
public static class VmNetDiag1439Slice
{
    public const int BandStart = 43;
    public const int BandEnd = 316;
    public const int MaxPlayers = 64;
    public const int ReadyClass = 0x01;
    public const int ForceReseat = 0x02;
    public const int WeaponReady = 1;
    public const int WeaponLayer = 1;
    public const int SpriteFixed = 1;
    public const int SpriteNoChange = 2;
    public const int SameFrame = 16;

    public static readonly Item NoChange = new();

    public sealed class Echo
    {
        public string Ready = "";
        public string Owner = "";
        public uint Offset;
        public ushort Weapon;
        public bool Init;
    }

    public sealed class State
    {
        public string Name = "";
        public int Sprite;
        public int Frame;
        public int Flags;
    }

    public sealed class Kind
    {
        public string Name = "";
        public Kind? Base;
        public List<State> States = new();
        public bool Is(Kind want)
        {
            for (Kind? k = this; k != null; k = k.Base)
                if (ReferenceEquals(k, want)) return true;
            return false;
        }
    }

    public sealed class Item
    {
        public Kind? Class;
    }

    public sealed class Sprite
    {
        public int Id;
        public Sprite? Next;
        public State? State;
        public short Tics;
        public int Frame;
        public int SpriteId;
        public Item? Caller;
        public int Resets;
    }

    public sealed class Player
    {
        public Item? Body;
        public Item? Ready;
        public Item? Pending;
        public ushort WeaponState;
        public byte PlayerState;
        public int ViewHeight;
        public Sprite? Sprites;
        public List<Item> Inv = new();
    }

    public sealed class World
    {
        public bool Follow = true;
        public int Console;
        public bool Authority;
        public Echo[] Last = new Echo[MaxPlayers];
        public Player[] Players = new Player[MaxPlayers];
        public Dictionary<string, Kind> Kinds = new(StringComparer.OrdinalIgnoreCase);
        public List<string> Notes = new();
        public World()
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                Last[i] = new Echo();
                Players[i] = new Player();
            }
        }
    }

    public static bool Read(byte[]? data, int size, ref int cursor, out string text)
    {
        text = "";
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
        if ((data == null && size > 0) || (data != null && size > data.Length)) throw new NullReferenceException();
        if (cursor >= size || size - cursor < 2) return false;
        int len = (data![cursor] << 8) | data[cursor + 1];
        cursor += 2;
        if (len == 0) return true;
        if (len > 255) return false;
        if (cursor > size || size - cursor < len) return false;
        text = Encoding.Latin1.GetString(data, cursor, len);
        cursor += len;
        return true;
    }

    public static int Flags(World w, int client, string ready, string owner, uint offset, ushort weapon)
    {
        if (client < 0 || client >= MaxPlayers) return 0;
        Echo last = w.Last[client];
        if (!last.Init) return ReadyClass | ForceReseat;
        int flags = 0;
        if (!string.Equals(ready, last.Ready, StringComparison.Ordinal)) flags |= ReadyClass;
        bool force = (flags & ReadyClass) != 0;
        if (!force && owner.Length != 0 && string.Equals(ready, owner, StringComparison.Ordinal) && !string.Equals(owner, last.Owner, StringComparison.Ordinal))
            force = true;
        if (!force && (weapon & WeaponReady) == 0 && (last.Weapon & WeaponReady) != 0)
            force = true;
        if (!force && owner.Length != 0 && !string.Equals(owner, last.Owner, StringComparison.Ordinal) && offset != last.Offset)
            force = true;
        if (force) flags |= ForceReseat;
        return flags;
    }

    public static void Remember(World w, int client, string ready, string owner, uint offset, ushort weapon)
    {
        if (client < 0 || client >= MaxPlayers) return;
        Echo last = w.Last[client];
        last.Ready = ready;
        last.Owner = owner;
        last.Offset = offset;
        last.Weapon = weapon;
        last.Init = true;
    }

    public static void Follow(World w, int player, string ready, string owner, uint offset, short tics, ushort weapon, int change)
    {
        if (!w.Follow) return;
        if (player != w.Console) return;
        if (player < 0 || player >= MaxPlayers) return;
        if (w.Authority) return;
        if (change == 0) return;
        Player who = w.Players[player];
        if (who.Body == null) return;
        bool classChanged = (change & ReadyClass) != 0;
        bool reseat = (change & ForceReseat) != 0;
        if (ready.Length == 0) return;
        string local = who.Ready?.Class?.Name ?? "";
        if (!string.Equals(ready, local, StringComparison.Ordinal) || classChanged)
        {
            if (!w.Kinds.TryGetValue(ready, out Kind? kind)) return;
            Item? found = null;
            foreach (Item item in who.Inv)
                if (item.Class != null && item.Class.Is(kind)) { found = item; break; }
            if (found == null) return;
            who.Pending = NoChange;
            who.Ready = found;
        }
        if (reseat || classChanged) who.WeaponState = weapon;
        if (!reseat) return;
        Sprite? sp = who.Sprites;
        while (sp != null && sp.Id != WeaponLayer) sp = sp.Next;
        if (sp == null || owner.Length == 0) return;
        if (!w.Kinds.TryGetValue(owner, out Kind? ownerKind) || offset >= (uint)ownerKind.States.Count) return;
        State server = ownerKind.States[(int)offset];
        if (who.Ready != null) sp.Caller = who.Ready;
        sp.State = server;
        sp.Tics = tics;
        if (server.Sprite != SpriteFixed)
        {
            if ((server.Flags & SameFrame) == 0) sp.Frame = server.Frame;
            if (server.Sprite != SpriteNoChange) sp.SpriteId = server.Sprite;
        }
        sp.Resets++;
    }

    public static void Ping(World w, int client, int leadTics, int leadMs, int rttMs, int ticDup, int extraTics, int delta)
    {
        w.Notes.Add("net.ping client=" + client + " lead_tics=" + leadTics + " lead_ms=" + leadMs + " rtt_ms=" + rttMs + " TicDup=" + ticDup + " extratics=" + extraTics + " delta=" + delta);
    }

    public static void Compare(World w, int client, uint tic, int player, string ready, string owner, uint offset, short tics, ushort weapon, byte playerState, short viewHeight)
    {
        if (player < 0 || player >= MaxPlayers) return;
        Player who = w.Players[player];
        string localReady = who.Ready?.Class?.Name ?? "";
        State? localState = null;
        short localTics = 0;
        Sprite? sp = who.Sprites;
        while (sp != null && sp.Id != WeaponLayer) sp = sp.Next;
        if (sp != null && sp.State != null)
        {
            localState = sp.State;
            localTics = sp.Tics;
        }
        State? serverState = null;
        if (owner.Length != 0 && w.Kinds.TryGetValue(owner, out Kind? kind) && offset < (uint)kind.States.Count)
            serverState = kind.States[(int)offset];
        if (!string.Equals(ready, localReady, StringComparison.Ordinal))
            w.Notes.Add("net.desync [WEAPON DESYNC] player=" + player + " server ReadyWeapon=" + (ready.Length == 0 ? "None" : ready) + " local ReadyWeapon=" + (localReady.Length == 0 ? "None" : localReady) + " tic=" + tic);
        if (!ReferenceEquals(localState, serverState) || (serverState != null && localTics != tics))
            w.Notes.Add("net.desync [PSPRITE STATE DESYNC] player=" + player + " server state=" + (serverState != null ? serverState.Name : "None") + " (tics=" + tics + ") local state=" + (localState != null ? localState.Name : "None") + " (tics=" + localTics + ") tic=" + tic);
        if (who.WeaponState != weapon)
            w.Notes.Add("net.desync [WEAPON STATE DESYNC] player=" + player + " server WeaponState=0x" + weapon.ToString("x4") + " local WeaponState=0x" + who.WeaponState.ToString("x4") + " tic=" + tic);
        if (who.PlayerState != playerState)
            w.Notes.Add("net.desync [PLAYER STATE DESYNC] player=" + player + " server playerstate=" + playerState + " local playerstate=" + who.PlayerState + " tic=" + tic);
        short localView = unchecked((short)who.ViewHeight);
        if (player == w.Console && Math.Abs(localView - (viewHeight / 256.0)) > 1.0)
            w.Notes.Add("net.desync [VIEWHEIGHT DESYNC] player=" + player + " server viewheight=" + (viewHeight / 256.0).ToString("0.00", CultureInfo.InvariantCulture) + " local viewheight=" + ((double)localView).ToString("0.00", CultureInfo.InvariantCulture) + " tic=" + tic);
        _ = client;
    }
}
