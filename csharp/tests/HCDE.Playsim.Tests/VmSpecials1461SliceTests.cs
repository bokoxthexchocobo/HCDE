using Xunit;

namespace HCDE.Playsim.Tests;

public class VmSpecials1461SliceTests
{
    static VmSpecials1461Slice.Sector Run(int special, bool mbf21, out VmSpecials1461Slice.World world)
    {
        world = new VmSpecials1461Slice.World { Mbf21 = mbf21 };
        var sector = new VmSpecials1461Slice.Sector { Special = special };
        VmSpecials1461Slice.InitTail(world, sector);
        return sector;
    }

    [Fact]
    public void BoomDamageAndDoomSpecials()
    {
        var sector = Run(0x100, false, out var world);
        Assert.Equal(0, sector.Special);
        Assert.True(world.Lights);
        var damage = Assert.Single(world.Damages);
        Assert.Equal(5, damage.Amount);
        Assert.Equal("Fire", damage.Type);

        sector = Run(0x8100, false, out world);
        Assert.Equal("Fire", Assert.Single(world.Damages).Type);
        sector = Run(0x8100, true, out world);
        Assert.Equal(VmSpecials1461Slice.Telefrag, Assert.Single(world.Damages).Amount);
        sector = Run(0x8300, true, out world);
        Assert.Equal(8192, sector.Flags);
        Assert.Empty(world.Damages);

        sector = Run(68, false, out world);
        Assert.Equal(0, sector.Special);
        Assert.Equal(20, Assert.Single(world.Damages).Amount);
        sector = Run(84, false, out world);
        Assert.Equal(84, sector.Special);
        Assert.Equal(-4, Assert.Single(world.Scrolls).X);
        Assert.Equal(128, world.Damages[0].Flags);
        sector = Run(79, false, out world);
        Assert.Equal(0xf900 / 65536.0, sector.Friction);
        Assert.Equal(0x269 / 65536.0, sector.MoveFactor);
        Assert.Equal(16, sector.Flags);
        sector = Run(74, false, out world);
        Assert.Equal(VmSpecials1461Slice.DoorWaitClose, Assert.Single(world.Doors).Type);
        Assert.Equal(1050, world.Doors[0].Wait);
        sector = Run(78, false, out world);
        Assert.Equal(150, world.Doors[0].Delay);
        Assert.Equal(10500, world.Doors[0].Wait);
    }

    [Fact]
    public void HexenScrollHereticCarryAndSkyKeepOrClear()
    {
        var sector = Run(201, false, out var world);
        Assert.Equal(201, sector.Special);
        Assert.Equal(0, world.Scrolls[0].X);
        Assert.Equal(0.5, world.Scrolls[0].Y);
        sector = Run(224, false, out world);
        Assert.Equal(2, world.Scrolls[0].X);
        Assert.Equal(-2, world.Scrolls[0].Y);
        sector = Run(225, false, out world);
        Assert.Equal(-0.5, world.Scrolls[0].X);
        Assert.Equal(225, sector.Special);
        sector = Run(229, false, out world);
        Assert.Equal(-8, world.Scrolls[0].X);
        sector = Run(195, false, out world);
        Assert.Equal(256, sector.MoreFlags);
        Assert.Equal(0, sector.Special);
        sector = Run(200, false, out world);
        Assert.Equal(0x40000000, sector.Sky);
        Assert.Equal(0, sector.Special);
        sector = Run(1, false, out world);
        Assert.Equal(1, sector.Special);
        Assert.Empty(world.Scrolls);
    }
}
