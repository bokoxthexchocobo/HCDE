using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// Inventory reconcile and presentation-echo writer from d_net_diag.cpp 319-668.
/// The replicated-item struct is non-behavior at the front of the span.
/// The reader starts at line 671.
/// </summary>
public static class VmNetDiag1440Slice
{
    public const int BandStart = 319;
    public const int BandEnd = 668;
    public const int MaxPlayers = 64;
    public const int ReadyClass = 0x01;
    public const int ForceReseat = 0x02;
    public const int WeaponReady = 1;
    public const int WeaponLayer = 1;
    public const int Version = 8;
    public const int NoSlot = 0xFF;
    public const uint PendingNone = 0xFFFFFFFFu;

    public static readonly Item NoChange = new();

    public sealed class Kind
    {
        public string Name = "";
        public int Index;
        public Kind? Base;
        public bool Is(string name)
        {
            for (Kind? k = this; k != null; k = k.Base)
                if (string.Equals(k.Name, name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }

    public sealed class State
    {
        public string Name = "";
        public int NameIndex;
        public Kind? Owner;
        public int Offset;
    }

    public sealed class Item
    {
        public Kind? Class;
        public int Amount;
        public double[]? Slots;
        public Item? Next;
    }

    public sealed class Sprite
    {
        public int Id;
        public Sprite? Next;
        public State? State;
        public short Tics;
    }

    public sealed class Player
    {
        public Item? Body;
        public Item? Ready;
        public Item? Pending;
        public ushort WeaponState;
        public byte PlayerState;
        public double ViewHeight;
        public Sprite? Sprites;
    }

    public sealed class Entry
    {
        public string ClassName = "";
        public uint Amount;
        public bool Armor;
        public ushort[] Slots = new ushort[5];
    }

    public sealed class Echo
    {
        public string Ready = "";
        public string Owner = "";
        public uint Offset;
        public ushort Weapon;
        public bool Init;
    }

    public sealed class World
    {
        public int Console;
        public int Reserved = -1;
        public int Debug;
        public bool Authority;
        public int Gives;
        public List<(int Client, int Bytes)> Lanes = new();
        public Echo[] Last = new Echo[MaxPlayers];
        public Player[] Players = new Player[MaxPlayers];
        public Dictionary<string, Kind> Kinds = new(StringComparer.OrdinalIgnoreCase);
        public World()
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                Last[i] = new Echo();
                Players[i] = new Player();
            }
        }
    }

    public static void WriteSlots(Item? item, ushort[] slots)
    {
        for (int i = 0; i < 5; i++) slots[i] = 0;
        if (item == null || item.Class == null || !item.Class.Is("HexenArmor")) return;
        if (item.Slots == null) return;
        for (int i = 0; i < 5; i++)
        {
            double slot = Math.Max(0.0, item.Slots[i]);
            slots[i] = (ushort)Math.Min(slot, 65535.0);
        }
    }

    public static bool ShouldCopy(Item? item)
    {
        if (item?.Class == null) return false;
        Kind kind = item.Class;
        return kind.Is("Weapon") || kind.Is("Ammo") || kind.Is("Armor") || kind.Is("Key") || kind.Is("Powerup") || kind.Is("CustomInventory");
    }

    public static int CopyFlags(Item item)
    {
        if (item.Class == null) throw new NullReferenceException();
        int flags = 0;
        if (item.Class.Is("Weapon")) flags |= 0x01;
        if (item.Class.Is("Armor")) flags |= 0x02;
        if (item.Class.Is("Key")) flags |= 0x04;
        if (item.Class.Is("Powerup")) flags |= 0x08;
        if (item.Class.Is("CustomInventory")) flags |= 0x10;
        return flags;
    }

    public static void ApplyArmor(Item? inv, Entry item)
    {
        if (inv == null || !item.Armor) return;
        if (inv.Class != null && inv.Class.Is("HexenArmor"))
        {
            if (inv.Slots != null)
                for (int i = 0; i < 5; i++) inv.Slots[i] = item.Slots[i];
            return;
        }
        inv.Amount = item.Amount > int.MaxValue ? int.MaxValue : (int)item.Amount;
    }

    public static void Reconcile(World w, int player, List<Entry> items)
    {
        if (w.Authority) return;
        if (player != w.Console || player < 0 || player >= MaxPlayers) return;
        Item? mo = w.Players[player].Body;
        if (mo == null) return;
        foreach (Entry entry in items)
        {
            if (!w.Kinds.TryGetValue(entry.ClassName, out Kind? kind) || !kind.Is("Inventory")) continue;
            if (Find(mo, kind) == null)
            {
                Give(mo, kind);
                w.Gives++;
            }
        }
        foreach (Entry entry in items)
        {
            if (!w.Kinds.TryGetValue(entry.ClassName, out Kind? kind) || !kind.Is("Inventory")) continue;
            Item? inv = Find(mo, kind);
            if (inv == null) continue;
            if (entry.Armor) ApplyArmor(inv, entry);
            else inv.Amount = entry.Amount > int.MaxValue ? int.MaxValue : (int)entry.Amount;
        }
    }

    public static bool Append(World w, int client, byte[] output, int capacity, ref int cursor, byte[]? nums)
    {
        int start = cursor;
        var echoNums = new byte[MaxPlayers];
        var echoFlags = new byte[MaxPlayers];
        int count = 0;
        bool own = client >= 0 && client < MaxPlayers && client != w.Reserved && w.Players[client].Body != null;
        if (w.Debug != 0)
        {
            int n = nums?.Length ?? 0;
            for (int i = 0; i < n && count < MaxPlayers; i++)
            {
                echoNums[count] = nums![i];
                echoFlags[count] = (byte)(ReadyClass | ForceReseat);
                count++;
            }
            if (own)
            {
                bool found = false;
                for (int i = 0; i < count; i++)
                    if (echoNums[i] == (byte)client) { found = true; break; }
                if (!found && count < MaxPlayers)
                {
                    echoNums[count] = (byte)client;
                    echoFlags[count] = (byte)(ReadyClass | ForceReseat);
                    count++;
                }
            }
        }
        else if (own)
        {
            Player who = w.Players[client];
            string ownerName = "";
            uint offset = 0;
            Sprite? sp = who.Sprites;
            while (sp != null && sp.Id != WeaponLayer) sp = sp.Next;
            if (sp?.State != null && sp.State.Owner != null)
            {
                ownerName = sp.State.Owner.Name;
                offset = (uint)sp.State.Offset;
            }
            string readyName = who.Ready?.Class?.Name ?? "";
            int change = Flags(w, client, readyName, ownerName, offset, who.WeaponState);
            if (change != 0)
            {
                echoNums[count] = (byte)client;
                echoFlags[count] = (byte)change;
                count++;
                Remember(w, client, readyName, ownerName, offset, who.WeaponState);
            }
        }
        if (!PutBytes(output, capacity, ref cursor, new byte[] { (byte)'E', (byte)'C', (byte)'H', (byte)'O' })
            || !Put(output, capacity, ref cursor, Version)
            || !Put(output, capacity, ref cursor, count))
            return false;
        bool local = own;
        if (!Put(output, capacity, ref cursor, local ? client : NoSlot)) return false;
        if (local)
        {
            Item? head = w.Players[client].Body;
            int items = 0;
            for (Item? item = head?.Next; item != null && items < 255; item = item.Next)
                if (ShouldCopy(item)) items++;
            if (!Put16(output, capacity, ref cursor, (ushort)items)) return false;
            int emitted = 0;
            for (Item? item = head?.Next; item != null && emitted < items; item = item.Next)
            {
                if (!ShouldCopy(item)) continue;
                bool armor = item.Class!.Is("Armor");
                ushort[] slots = new ushort[5];
                if (armor) WriteSlots(item, slots);
                int amount = Math.Max(0, item.Amount);
                if (!Put(output, capacity, ref cursor, CopyFlags(item))
                    || !Put32(output, capacity, ref cursor, (uint)amount)
                    || !PutString(output, capacity, ref cursor, item.Class.Name))
                    return false;
                if (armor)
                    for (int s = 0; s < 5; s++)
                        if (!Put16(output, capacity, ref cursor, slots[s])) return false;
                emitted++;
            }
        }
        for (int i = 0; i < count; i++)
        {
            int player = echoNums[i];
            if (player >= MaxPlayers) return false;
            Player who = w.Players[player];
            uint readyIndex = who.Ready?.Class != null ? (uint)who.Ready.Class.Index : 0;
            uint pendingIndex = 0;
            if (who.Pending != null && !ReferenceEquals(who.Pending, NoChange))
                pendingIndex = who.Pending.Class != null ? (uint)who.Pending.Class.Index : 0;
            else if (ReferenceEquals(who.Pending, NoChange))
                pendingIndex = PendingNone;
            uint stateIndex = 0;
            short tics = 0;
            string ownerName = "";
            uint offset = 0;
            Sprite? sp = who.Sprites;
            while (sp != null && sp.Id != WeaponLayer) sp = sp.Next;
            if (sp?.State != null)
            {
                stateIndex = (uint)sp.State.NameIndex;
                tics = sp.Tics;
                if (sp.State.Owner != null)
                {
                    ownerName = sp.State.Owner.Name;
                    offset = (uint)sp.State.Offset;
                }
            }
            string readyName = who.Ready?.Class?.Name ?? "";
            int scaled = (int)(who.ViewHeight * 256.0);
            if (scaled > 32767) scaled = 32767;
            if (scaled < -32768) scaled = -32768;
            if (!Put(output, capacity, ref cursor, player)
                || !Put32(output, capacity, ref cursor, readyIndex)
                || !Put32(output, capacity, ref cursor, pendingIndex)
                || !Put32(output, capacity, ref cursor, stateIndex)
                || !Put16(output, capacity, ref cursor, unchecked((ushort)tics))
                || !Put16(output, capacity, ref cursor, who.WeaponState)
                || !Put(output, capacity, ref cursor, who.PlayerState)
                || !Put16(output, capacity, ref cursor, unchecked((ushort)(short)scaled))
                || !Put32(output, capacity, ref cursor, offset)
                || !PutString(output, capacity, ref cursor, ownerName)
                || !PutString(output, capacity, ref cursor, readyName)
                || !Put(output, capacity, ref cursor, echoFlags[i]))
                return false;
        }
        w.Lanes.Add((client, cursor - start));
        return true;
    }

    static int Flags(World w, int client, string ready, string owner, uint offset, ushort weapon)
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

    static void Remember(World w, int client, string ready, string owner, uint offset, ushort weapon)
    {
        Echo last = w.Last[client];
        last.Ready = ready;
        last.Owner = owner;
        last.Offset = offset;
        last.Weapon = weapon;
        last.Init = true;
    }

    static Item? Find(Item mo, Kind kind)
    {
        for (Item? item = mo.Next; item != null; item = item.Next)
            if (item.Class != null && item.Class.Is(kind.Name)) return item;
        return null;
    }

    static void Give(Item mo, Kind kind)
    {
        Item made = new() { Class = kind };
        if (kind.Is("HexenArmor")) made.Slots = new double[5];
        Item tail = mo;
        while (tail.Next != null) tail = tail.Next;
        tail.Next = made;
    }

    static bool Put(byte[] output, int cap, ref int cursor, int value)
    {
        if (cursor >= cap) return false;
        output[cursor++] = (byte)value;
        return true;
    }

    static bool Put16(byte[] output, int cap, ref int cursor, ushort value)
    {
        if (cursor >= cap || cap - cursor < 2) return false;
        output[cursor++] = (byte)(value >> 8);
        output[cursor++] = (byte)value;
        return true;
    }

    static bool Put32(byte[] output, int cap, ref int cursor, uint value)
    {
        if (cursor >= cap || cap - cursor < 4) return false;
        output[cursor++] = (byte)(value >> 24);
        output[cursor++] = (byte)(value >> 16);
        output[cursor++] = (byte)(value >> 8);
        output[cursor++] = (byte)value;
        return true;
    }

    static bool PutBytes(byte[] output, int cap, ref int cursor, byte[] data)
    {
        if (cursor >= cap || data.Length > cap - cursor) return false;
        data.CopyTo(output, cursor);
        cursor += data.Length;
        return true;
    }

    static bool PutString(byte[] output, int cap, ref int cursor, string? text)
    {
        int len = text == null ? 0 : Math.Min(text.Length, 255);
        if (!Put16(output, cap, ref cursor, (ushort)len)) return false;
        if (len == 0) return true;
        byte[] raw = Encoding.Latin1.GetBytes(text!);
        return PutBytes(output, cap, ref cursor, raw.Length == len ? raw : raw[..len]);
    }
}
