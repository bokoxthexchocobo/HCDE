namespace HCDE.Playsim.Tests;

public class VmPAcsBehaviorInitTests
{
    [Fact]
    public void Init_NullLevel_ReturnsFalse()
    {
        var mod = new VmPAcsBehaviorInit.BehaviorModule();
        var ctx = new VmPAcsBehaviorInit.InitContext
        {
            Module = mod,
            LevelNull = true,
            LumpNum = 1,
            Len = 64,
        };
        Assert.False(VmPAcsBehaviorInit.Init(ctx));
    }

    [Fact]
    public void Init_ValidHeader_ReturnsTrue()
    {
        var data = new byte[64];
        data[0] = (byte)'A';
        data[1] = (byte)'C';
        data[2] = (byte)'S';
        data[3] = 0;
        BitConverter.TryWriteBytes(data.AsSpan(4), 32u);

        var mod = new VmPAcsBehaviorInit.BehaviorModule();
        var ctx = new VmPAcsBehaviorInit.InitContext
        {
            Module = mod,
            LevelNull = false,
            LumpNum = 1,
            ReaderBuffer = data,
            Len = data.Length,
            PushStaticModule = _ => 1,
            LoadScriptsDirectory = () => { },
        };
        Assert.True(VmPAcsBehaviorInit.Init(ctx));
        Assert.Equal(AcsFormat.Old, mod.Format);
        Assert.Equal("BEHAVIOR", mod.ModuleName);
    }

    [Fact]
    public void Init_RejectsTinyLump()
    {
        var mod = new VmPAcsBehaviorInit.BehaviorModule();
        var ctx = new VmPAcsBehaviorInit.InitContext
        {
            Module = mod,
            LevelNull = false,
            LumpNum = 1,
            Len = 8,
        };
        Assert.False(VmPAcsBehaviorInit.Init(ctx));
    }

    [Fact]
    public void ReleaseBehavior_ClearsFlags()
    {
        var mod = new VmPAcsBehaviorInit.BehaviorModule
        {
            HasScripts = true,
            HasData = true,
            NumArrays = 2,
        };
        var released = 0;
        VmPAcsBehaviorInit.ReleaseBehavior(mod, () => released++, null, _ => released++, null, null, () => released++);
        Assert.False(mod.HasScripts);
        Assert.False(mod.HasData);
        Assert.Equal(2, released);
    }
}
