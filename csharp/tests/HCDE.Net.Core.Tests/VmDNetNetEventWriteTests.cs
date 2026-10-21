namespace HCDE.Net.Core.Tests;

public class VmDNetNetEventWriteTests
{
    [Fact]
    public void NetAdoptServerRoomIdNative_SetsPendingForGuest()
    {
        var pending = 0;
        VmDNetNetEventWrite.NetAdoptServerRoomIdNative(localAuthority: false, room: 7, ref pending);
        Assert.Equal(7, pending);
    }

    [Fact]
    public void NetDynamicBuffer_SetData_RoundsCapacity()
    {
        var buf = new NetDynamicBuffer();
        buf.SetData([1, 2, 3], 3);
        Assert.Equal(3, buf.Length);
        Assert.True(buf.BufferCapacity >= 3);
    }

    [Fact]
    public void RemoveClassNative_CountsDestroyed()
    {
        var snaps = new[]
        {
            new RemoveClassActorSnapshot(false, true, true),
            new RemoveClassActorSnapshot(true, true, true),
        };
        var count = VmDNetNetEventWrite.RemoveClassNative(
            snaps, s => s.IsKind, _ => { }, out var sawPlayer);
        Assert.Equal(1, count);
        Assert.True(sawPlayer);
    }

    [Fact]
    public void SelectWeaponNative_UsesInventoryWhenResolved()
    {
        var weap = new WeaponStub(42, "pistol");
        var ok = VmDNetNetEventWrite.SelectWeaponNative(
            player: 0,
            consolePlayer: 0,
            slot: 0,
            gameState: VmDNetNetEventWrite.GameStateLevel,
            paused: false,
            playerState: VmDNetNetEventWrite.PlayerStateLive,
            mobj: new object(),
            dmflags2: 0,
            df2DontCheckAmmo: 0,
            displayNameTags: 0,
            nameTagColor: 0,
            statusBar: null,
            smallFont: null,
            readyWeapon: null,
            pickWeaponFromSlot: (_, _, _) => weap,
            pickNextWeapon: null,
            pickPrevWeapon: null,
            findInventoryById: (_, id) => id == 42 ? weap : null,
            useInventory: (_, _) => { },
            playWeaponChangeSound: null,
            attachWeaponNameHud: null);
        Assert.True(ok);
    }

    private sealed class WeaponStub(uint id, string tag) : IInventoryIdProvider, IActorTagProvider
    {
        public uint InventoryId => id;
        public string Tag => tag;
    }
}
