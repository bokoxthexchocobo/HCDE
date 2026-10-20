namespace HCDE.MapLoader.Tests;

public class VmMapLoaderLoadThingsTests
{
    [Fact]
    public void MakeSkill_MapsDoomSpawnBits()
    {
        Assert.Equal(3, VmMapLoaderLoadThings.MakeSkill(1));
        Assert.Equal(4, VmMapLoaderLoadThings.MakeSkill(2));
        Assert.Equal(24, VmMapLoaderLoadThings.MakeSkill(4));
    }

    [Fact]
    public void TranslateDoomThingFlags_AppliesFriendlyBit()
    {
        var flags = VmMapLoaderLoadThings.TranslateDoomThingFlags(
            (ushort)(0xf | VmMapLoaderLoadThings.BtfFriendly), false);
        Assert.NotEqual(0, flags & VmMapLoaderLoadThings.MtfFriendly);
    }

    [Fact]
    public void ApplyHexenHackMask_StripsStrifeBits()
    {
        Assert.Equal(0x7ff, VmMapLoaderLoadThings.ApplyHexenHackMask(0xffff, true));
    }

    [Fact]
    public void CountDoomThings_DividesLumpSize()
    {
        Assert.Equal(10, VmMapLoaderLoadThings.CountDoomThings(200, 20));
    }
}
