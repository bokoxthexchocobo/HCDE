namespace HCDE.Playsim.Tests;

public class VmPAcsIsClientSideScriptTests
{
    [Fact]
    public void IsClientSideScript_FlagSet()
    {
        Assert.True(VmPAcsIsClientSideScript.IsClientSideScript(VmPAcsIsClientSideScript.ScriptfClientside));
        Assert.False(VmPAcsIsClientSideScript.IsClientSideScript(0));
    }

    [Fact]
    public void UaLong_ReadsLittleEndian()
    {
        ReadOnlySpan<byte> b = [0x78, 0x56, 0x34, 0x12];
        Assert.Equal(0x12345678, VmPAcsIsClientSideScript.UaLong(b));
    }

    [Fact]
    public void LevelScript_PicksClientSideIterator()
    {
        var ls = new VmPAcsIsClientSideScript.LevelScript { ClientSide = true };
        Assert.True(ls.PickClientSideIterator(5, _ => false, _ => true));
    }

    [Fact]
    public void AcsToFloat_ScalesFixed()
    {
        Assert.Equal(1f, VmPAcsIsClientSideScript.AcsToFloat(65536));
    }
}
