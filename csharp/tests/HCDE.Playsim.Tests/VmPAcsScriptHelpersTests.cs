namespace HCDE.Playsim.Tests;

public class VmPAcsScriptHelpersTests
{
    [Fact]
    public void AcsToFloat_Uses65536Scale()
    {
        Assert.Equal(1f, VmPAcsScriptHelpers.AcsToFloat(65536));
    }

    [Fact]
    public void UaLong_ReadsLittleEndian()
    {
        var b = new byte[] { 0x78, 0x56, 0x34, 0x12 };
        Assert.Equal(0x12345678, VmPAcsScriptHelpers.UaLong(b));
    }

    [Fact]
    public void ShouldIgnoreClientSideScript_SuppressesOtherPlayer()
    {
        var owner = new VmPAcsScriptHelpers.Activator { ClientSide = true, Player = new() };
        var level = new VmPAcsScriptHelpers.LevelRef { IsConsolePlayer = _ => false };
        Assert.True(VmPAcsScriptHelpers.ShouldIgnoreClientSideScript(owner, level));
    }

    [Fact]
    public void StringBuilderStack_PushesAndPops()
    {
        var ls = new VmPAcsScriptHelpers.LevelScript();
        var builder = "outer";
        ls.StringBuilderStart(ref builder);
        Assert.Equal("", builder);
        ls.StringBuilderFinish(ref builder);
        Assert.Equal("outer", builder);
    }
}
