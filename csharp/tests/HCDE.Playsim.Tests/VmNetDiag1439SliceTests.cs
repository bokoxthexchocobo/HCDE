using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmNetDiag1439SliceTests
{
    [Fact]
    public void ReadFlagsAndRemember()
    {
        Assert.Equal(43, VmNetDiag1439Slice.BandStart);
        Assert.Equal(316, VmNetDiag1439Slice.BandEnd);
        int cursor = 0;
        Assert.True(VmNetDiag1439Slice.Read(new byte[] { 0, 0 }, 2, ref cursor, out string empty));
        Assert.Equal(2, cursor);
        Assert.Equal("", empty);
        cursor = 0;
        Assert.True(VmNetDiag1439Slice.Read(new byte[] { 0, 2, (byte)'H', (byte)'i' }, 4, ref cursor, out string hi));
        Assert.Equal(4, cursor);
        Assert.Equal("Hi", hi);
        cursor = 0;
        Assert.True(VmNetDiag1439Slice.Read(new byte[] { 0, 1, 0 }, 3, ref cursor, out string nul));
        Assert.Equal(3, cursor);
        Assert.Equal(1, nul.Length);
        Assert.Equal(0, nul[0]);
        cursor = 0;
        Assert.False(VmNetDiag1439Slice.Read(new byte[] { 1, 0, 1 }, 3, ref cursor, out string forged));
        Assert.Equal(2, cursor);
        Assert.Equal("", forged);
        cursor = 0;
        Assert.False(VmNetDiag1439Slice.Read(new byte[] { 0, 3, (byte)'A' }, 3, ref cursor, out _));
        Assert.Equal(2, cursor);
        cursor = 5;
        Assert.False(VmNetDiag1439Slice.Read(new byte[] { 0 }, 1, ref cursor, out _));
        Assert.Equal(5, cursor);

        var world = new VmNetDiag1439Slice.World();
        Assert.Equal(0x03, VmNetDiag1439Slice.Flags(world, 2, "Pistol", "", 0, 1));
        Assert.False(world.Last[2].Init);
        Assert.Equal(0, VmNetDiag1439Slice.Flags(world, 64, "Pistol", "", 0, 1));
        Assert.Equal(0, VmNetDiag1439Slice.Flags(world, -1, "Pistol", "", 0, 1));
        VmNetDiag1439Slice.Remember(world, 64, "Pistol", "Pistol", 4, 1);
        Assert.False(world.Last[0].Init);
        VmNetDiag1439Slice.Remember(world, 2, "Pistol", "Fist", 1, 1);
        Assert.Equal(0, VmNetDiag1439Slice.Flags(world, 2, "Pistol", "", 1, 1));
        Assert.Equal(0x03, VmNetDiag1439Slice.Flags(world, 2, "Shotgun", "Fist", 1, 1));
        Assert.Equal(0x02, VmNetDiag1439Slice.Flags(world, 2, "Pistol", "Pistol", 1, 1));
        Assert.Equal(0x02, VmNetDiag1439Slice.Flags(world, 2, "Pistol", "", 1, 0));
        world.Last[2].Weapon = 0;
        Assert.Equal(0x02, VmNetDiag1439Slice.Flags(world, 2, "Pistol", "Shotgun", 4, 0));
        Assert.Equal(0, VmNetDiag1439Slice.Flags(world, 2, "Pistol", "Shotgun", 1, 0));
    }

    [Fact]
    public void FollowReseatCompareAndPing()
    {
        var pistol = new VmNetDiag1439Slice.Kind { Name = "Pistol" };
        var shotgun = new VmNetDiag1439Slice.Kind { Name = "Shotgun", Base = pistol };
        var flash = new VmNetDiag1439Slice.State { Name = "Flash", Sprite = 9, Frame = 2 };
        var idle = new VmNetDiag1439Slice.State { Name = "Idle", Sprite = VmNetDiag1439Slice.SpriteFixed, Frame = 8 };
        var hold = new VmNetDiag1439Slice.State { Name = "Hold", Sprite = VmNetDiag1439Slice.SpriteNoChange, Frame = 4 };
        var same = new VmNetDiag1439Slice.State { Name = "Same", Sprite = 6, Frame = 1, Flags = VmNetDiag1439Slice.SameFrame };
        pistol.States.Add(flash);
        pistol.States.Add(idle);
        pistol.States.Add(hold);
        pistol.States.Add(same);
        var world = new VmNetDiag1439Slice.World { Console = 1 };
        world.Kinds["Pistol"] = pistol;
        world.Kinds["Shotgun"] = shotgun;
        world.Kinds["Chaingun"] = new VmNetDiag1439Slice.Kind { Name = "Chaingun" };
        var body = new VmNetDiag1439Slice.Item();
        var gun = new VmNetDiag1439Slice.Item { Class = shotgun };
        var who = world.Players[1];
        who.Body = body;
        who.Inv.Add(gun);
        who.WeaponState = 7;
        var layer = new VmNetDiag1439Slice.Sprite { Id = VmNetDiag1439Slice.WeaponLayer, Frame = 3, SpriteId = 5 };
        who.Sprites = new VmNetDiag1439Slice.Sprite { Id = 0, Next = layer };

        VmNetDiag1439Slice.Follow(world, 1, "Ghost", "", 0, 1, 9, VmNetDiag1439Slice.ReadyClass);
        Assert.Equal(7, who.WeaponState);
        Assert.Null(who.Pending);
        VmNetDiag1439Slice.Follow(world, 1, "Chaingun", "", 0, 1, 9, VmNetDiag1439Slice.ReadyClass);
        Assert.Null(who.Pending);
        Assert.Equal(7, who.WeaponState);
        who.Inv.Add(new VmNetDiag1439Slice.Item { Class = pistol });
        VmNetDiag1439Slice.Follow(world, 1, "Pistol", "Pistol", 0, -1, 9, VmNetDiag1439Slice.ReadyClass);
        Assert.Same(gun, who.Ready);
        Assert.Same(VmNetDiag1439Slice.NoChange, who.Pending);
        Assert.Equal(9, who.WeaponState);
        Assert.Null(layer.State);
        Assert.Equal(0, layer.Resets);

        who.Inv.Clear();
        who.Inv.Add(gun);
        VmNetDiag1439Slice.Follow(world, 1, "Pistol", "Pistol", 0, -1, 4, VmNetDiag1439Slice.ForceReseat);
        Assert.Same(gun, who.Ready);
        Assert.Equal(4, who.WeaponState);
        Assert.Same(flash, layer.State);
        Assert.Equal(-1, layer.Tics);
        Assert.Equal(2, layer.Frame);
        Assert.Equal(9, layer.SpriteId);
        Assert.Same(gun, layer.Caller);
        Assert.Equal(1, layer.Resets);

        layer.Frame = 3;
        layer.SpriteId = 5;
        VmNetDiag1439Slice.Follow(world, 1, "Pistol", "Pistol", 1, 2, 4, VmNetDiag1439Slice.ForceReseat);
        Assert.Same(idle, layer.State);
        Assert.Equal(3, layer.Frame);
        Assert.Equal(5, layer.SpriteId);
        Assert.Equal(2, layer.Resets);

        VmNetDiag1439Slice.Follow(world, 1, "Pistol", "Pistol", 2, 2, 4, VmNetDiag1439Slice.ForceReseat);
        Assert.Equal(4, layer.Frame);
        Assert.Equal(5, layer.SpriteId);
        VmNetDiag1439Slice.Follow(world, 1, "Pistol", "Pistol", 3, 2, 4, VmNetDiag1439Slice.ForceReseat);
        Assert.Equal(4, layer.Frame);
        Assert.Equal(6, layer.SpriteId);

        who.WeaponState = 1;
        world.Follow = false;
        VmNetDiag1439Slice.Follow(world, 1, "Shotgun", "Pistol", 0, 1, 8, 0x03);
        Assert.Equal(1, who.WeaponState);
        world.Follow = true;
        world.Authority = true;
        VmNetDiag1439Slice.Follow(world, 1, "Shotgun", "Pistol", 0, 1, 8, 0x03);
        Assert.Equal(1, who.WeaponState);
        world.Authority = false;
        VmNetDiag1439Slice.Follow(world, 0, "Shotgun", "Pistol", 0, 1, 8, 0x03);
        Assert.Equal(1, who.WeaponState);
        who.Body = null;
        VmNetDiag1439Slice.Follow(world, 1, "Shotgun", "Pistol", 0, 1, 8, 0x03);
        Assert.Equal(1, who.WeaponState);

        who.Body = body;
        who.Ready = gun;
        who.WeaponState = 0x10;
        who.PlayerState = 2;
        who.ViewHeight = 10;
        who.Sprites = new VmNetDiag1439Slice.Sprite
        {
            Id = 4,
            Next = new VmNetDiag1439Slice.Sprite { Id = VmNetDiag1439Slice.WeaponLayer, State = flash, Tics = 6 },
        };
        VmNetDiag1439Slice.Compare(world, 9, 44, -1, "Pistol", "", 0, 0, 1, 1, 0);
        VmNetDiag1439Slice.Compare(world, 9, 44, 1, "", "Pistol", 0, 3, 0x000a, 1, 2304);
        Assert.Equal(new[]
        {
            "net.desync [WEAPON DESYNC] player=1 server ReadyWeapon=None local ReadyWeapon=Shotgun tic=44",
            "net.desync [PSPRITE STATE DESYNC] player=1 server state=Flash (tics=3) local state=Flash (tics=6) tic=44",
            "net.desync [WEAPON STATE DESYNC] player=1 server WeaponState=0x000a local WeaponState=0x0010 tic=44",
            "net.desync [PLAYER STATE DESYNC] player=1 server playerstate=1 local playerstate=2 tic=44",
        }, world.Notes);
        world.Notes.Clear();
        who.Sprites.Next!.State = flash;
        who.Sprites.Next.Tics = 3;
        who.Ready = new VmNetDiag1439Slice.Item { Class = pistol };
        who.WeaponState = 0x000a;
        who.PlayerState = 1;
        VmNetDiag1439Slice.Compare(world, 9, 44, 1, "Pistol", "Pistol", 0, 3, 0x000a, 1, 2304);
        Assert.Empty(world.Notes);
        VmNetDiag1439Slice.Compare(world, 9, 44, 1, "Pistol", "Pistol", 0, 3, 0x000a, 1, 2048);
        Assert.Equal("net.desync [VIEWHEIGHT DESYNC] player=1 server viewheight=8.00 local viewheight=10.00 tic=44", Assert.Single(world.Notes));
        world.Notes.Clear();
        world.Console = 0;
        VmNetDiag1439Slice.Compare(world, 9, 44, 1, "Pistol", "Pistol", 0, 3, 0x000a, 1, 2048);
        Assert.Empty(world.Notes);
        VmNetDiag1439Slice.Ping(world, 3, 2, -5, 40, 1, 0, 7);
        Assert.Equal("net.ping client=3 lead_tics=2 lead_ms=-5 rtt_ms=40 TicDup=1 extratics=0 delta=7", Assert.Single(world.Notes));
    }
}
