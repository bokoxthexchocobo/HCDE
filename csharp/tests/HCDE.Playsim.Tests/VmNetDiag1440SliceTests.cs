using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmNetDiag1440SliceTests
{
    static VmNetDiag1440Slice.Kind Chain(string name, int index, params string[] bases)
    {
        VmNetDiag1440Slice.Kind? prev = null;
        foreach (string baseName in bases)
            prev = new VmNetDiag1440Slice.Kind { Name = baseName, Base = prev };
        return new VmNetDiag1440Slice.Kind { Name = name, Index = index, Base = prev };
    }

    [Fact]
    public void SlotsFlagsAndReconcile()
    {
        Assert.Equal(319, VmNetDiag1440Slice.BandStart);
        Assert.Equal(668, VmNetDiag1440Slice.BandEnd);
        var slots = new ushort[5];
        VmNetDiag1440Slice.WriteSlots(null, slots);
        Assert.Equal(new ushort[5], slots);
        var armor = Chain("BasicArmor", 3, "Armor", "Inventory");
        var basic = new VmNetDiag1440Slice.Item { Class = armor, Amount = 4, Slots = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 } };
        VmNetDiag1440Slice.WriteSlots(basic, slots);
        Assert.Equal(new ushort[5], slots);
        var hexenKind = Chain("HexenArmor", 4, "Armor", "Inventory");
        var hexen = new VmNetDiag1440Slice.Item { Class = hexenKind, Amount = 9, Slots = new[] { -2.0, 1.9, 65535.0, 70000.0, 0.0 } };
        VmNetDiag1440Slice.WriteSlots(hexen, slots);
        Assert.Equal(new ushort[] { 0, 1, 65535, 65535, 0 }, slots);
        VmNetDiag1440Slice.WriteSlots(new VmNetDiag1440Slice.Item { Class = hexenKind }, slots);
        Assert.Equal(new ushort[5], slots);

        var weapon = Chain("Pistol", 2, "Weapon", "Inventory");
        var ammo = Chain("Clip", 5, "Ammo", "Inventory");
        var key = Chain("Key", 6, "Key", "Inventory");
        Assert.False(VmNetDiag1440Slice.ShouldCopy(null));
        Assert.True(VmNetDiag1440Slice.ShouldCopy(new VmNetDiag1440Slice.Item { Class = ammo }));
        Assert.False(VmNetDiag1440Slice.ShouldCopy(new VmNetDiag1440Slice.Item { Class = Chain("Zombie", 9) }));
        Assert.Equal(0x01, VmNetDiag1440Slice.CopyFlags(new VmNetDiag1440Slice.Item { Class = weapon }));
        Assert.Equal(0, VmNetDiag1440Slice.CopyFlags(new VmNetDiag1440Slice.Item { Class = ammo }));
        Assert.Equal(0x06, VmNetDiag1440Slice.CopyFlags(new VmNetDiag1440Slice.Item { Class = Chain("ArmorKey", 7, "Key", "Armor", "Inventory") }));
        Assert.Throws<NullReferenceException>(() => VmNetDiag1440Slice.CopyFlags(new VmNetDiag1440Slice.Item()));

        var worn = new VmNetDiag1440Slice.Item { Class = hexenKind, Amount = 9, Slots = new double[5] };
        VmNetDiag1440Slice.ApplyArmor(worn, new VmNetDiag1440Slice.Entry { Armor = true, Amount = 50, Slots = new ushort[] { 3, 4, 5, 6, 7 } });
        Assert.Equal(9, worn.Amount);
        Assert.Equal(new[] { 3d, 4d, 5d, 6d, 7d }, worn.Slots);
        VmNetDiag1440Slice.ApplyArmor(basic, new VmNetDiag1440Slice.Entry { Armor = true, Amount = uint.MaxValue });
        Assert.Equal(int.MaxValue, basic.Amount);
        basic.Amount = 1;
        VmNetDiag1440Slice.ApplyArmor(basic, new VmNetDiag1440Slice.Entry { Armor = false, Amount = 8 });
        Assert.Equal(1, basic.Amount);

        var world = new VmNetDiag1440Slice.World { Console = 1 };
        world.Kinds["Pistol"] = weapon;
        world.Kinds["Clip"] = ammo;
        world.Kinds["HexenArmor"] = hexenKind;
        world.Kinds["Zombie"] = Chain("Zombie", 9);
        var body = new VmNetDiag1440Slice.Item();
        var fist = new VmNetDiag1440Slice.Item { Class = Chain("Fist", 1, "Weapon", "Inventory"), Amount = 1 };
        body.Next = fist;
        world.Players[1].Body = body;
        world.Authority = true;
        VmNetDiag1440Slice.Reconcile(world, 1, new List<VmNetDiag1440Slice.Entry> { new() { ClassName = "Pistol", Amount = 1 } });
        Assert.Equal(0, world.Gives);
        world.Authority = false;
        VmNetDiag1440Slice.Reconcile(world, 0, new List<VmNetDiag1440Slice.Entry> { new() { ClassName = "Pistol", Amount = 1 } });
        Assert.Null(body.Next!.Next);
        VmNetDiag1440Slice.Reconcile(world, 1, new List<VmNetDiag1440Slice.Entry>
        {
            new() { ClassName = "Pistol", Amount = 12 },
            new() { ClassName = "Zombie", Amount = 4 },
            new() { ClassName = "HexenArmor", Amount = 99, Armor = true, Slots = new ushort[] { 1, 0, 0, 0, 0 } },
            new() { ClassName = "missing", Amount = 1 },
        });
        Assert.Equal(2, world.Gives);
        Assert.Same(fist, body.Next);
        Assert.Equal("Pistol", body.Next.Next!.Class!.Name);
        Assert.Equal(12, body.Next.Next.Amount);
        var wornGiven = body.Next.Next.Next!;
        Assert.Equal(0, wornGiven.Amount);
        Assert.Equal(1d, wornGiven.Slots![0]);
        VmNetDiag1440Slice.Reconcile(world, 1, new List<VmNetDiag1440Slice.Entry> { new() { ClassName = "Pistol", Amount = 3 } });
        Assert.Equal(2, world.Gives);
        Assert.Equal(3, body.Next.Next.Amount);
    }

    [Fact]
    public void AppendEchoWire()
    {
        var weapon = Chain("Pistol", 2, "Weapon", "Inventory");
        var ammo = Chain("Clip", 8, "Ammo", "Inventory");
        var owner = Chain("Pistol", 2, "Weapon", "Inventory");
        var state = new VmNetDiag1440Slice.State { Name = "Fire", NameIndex = 15, Owner = owner, Offset = 4 };
        var world = new VmNetDiag1440Slice.World { Console = 2 };
        world.Kinds["Pistol"] = weapon;
        var body = new VmNetDiag1440Slice.Item();
        var gun = new VmNetDiag1440Slice.Item { Class = weapon, Amount = -3 };
        var clip = new VmNetDiag1440Slice.Item { Class = ammo, Amount = 9 };
        body.Next = gun;
        gun.Next = clip;
        var who = world.Players[2];
        who.Body = body;
        who.Ready = gun;
        who.Pending = VmNetDiag1440Slice.NoChange;
        who.WeaponState = 1;
        who.PlayerState = 2;
        who.ViewHeight = 200;
        who.Sprites = new VmNetDiag1440Slice.Sprite
        {
            Id = 0,
            Next = new VmNetDiag1440Slice.Sprite { Id = VmNetDiag1440Slice.WeaponLayer, State = state, Tics = -1 },
        };
        var buf = new byte[128];
        int cursor = 0;
        Assert.True(VmNetDiag1440Slice.Append(world, 2, buf, buf.Length, ref cursor, null));
        Assert.Equal(new byte[] { (byte)'E', (byte)'C', (byte)'H', (byte)'O', 8, 1, 2 }, buf.AsSpan(0, 7).ToArray());
        Assert.Equal(2, buf[7] << 8 | buf[8]);
        Assert.Equal(0x01, buf[9]);
        Assert.Equal(0u, (uint)(buf[10] << 24 | buf[11] << 16 | buf[12] << 8 | buf[13]));
        Assert.Equal((byte)'P', buf[16]);
        int at = 33;
        Assert.Equal(2, buf[at]);
        Assert.Equal(2u, Read32(buf, at + 1));
        Assert.Equal(VmNetDiag1440Slice.PendingNone, Read32(buf, at + 5));
        Assert.Equal(15u, Read32(buf, at + 9));
        Assert.Equal(0xFFFF, buf[at + 13] << 8 | buf[at + 14]);
        Assert.Equal(1, buf[at + 15] << 8 | buf[at + 16]);
        Assert.Equal(2, buf[at + 17]);
        Assert.Equal(32767, buf[at + 18] << 8 | buf[at + 19]);
        Assert.Equal(4u, Read32(buf, at + 20));
        Assert.Equal(0x03, buf[cursor - 1]);
        Assert.True(world.Last[2].Init);
        Assert.Equal(cursor, world.Lanes[0].Bytes);
        int first = cursor;
        int lanes = world.Lanes.Count;
        cursor = 0;
        world.Last[2].Init = false;
        Assert.False(VmNetDiag1440Slice.Append(world, 2, buf, 0, ref cursor, null));
        Assert.Equal(0, cursor);
        Assert.True(world.Last[2].Init);
        Assert.Equal(lanes, world.Lanes.Count);
        cursor = first;
        Assert.True(VmNetDiag1440Slice.Append(world, 2, buf, buf.Length, ref cursor, null));
        Assert.Equal(0, buf[first + 5]);
        world.Reserved = 2;
        cursor = 0;
        world.Lanes.Clear();
        Assert.True(VmNetDiag1440Slice.Append(world, 2, buf, buf.Length, ref cursor, null));
        Assert.Equal(0, buf[5]);
        Assert.Equal(VmNetDiag1440Slice.NoSlot, buf[6]);
        world.Reserved = -1;
        world.Debug = 1;
        cursor = 0;
        world.Lanes.Clear();
        Assert.False(VmNetDiag1440Slice.Append(world, 2, buf, buf.Length, ref cursor, new byte[] { 70 }));
        Assert.Empty(world.Lanes);
        Assert.True(cursor > 6);
        world.Debug = 0;
        who.ViewHeight = 10;
        cursor = 0;
        world.Last[2].Init = false;
        Assert.False(VmNetDiag1440Slice.Append(world, 2, buf, 9, ref cursor, null));
        Assert.Equal(9, cursor);
        Assert.Empty(world.Lanes);
        Assert.True(world.Last[2].Init);
    }

    static uint Read32(byte[] buf, int at) => (uint)(buf[at] << 24 | buf[at + 1] << 16 | buf[at + 2] << 8 | buf[at + 3]);
}
