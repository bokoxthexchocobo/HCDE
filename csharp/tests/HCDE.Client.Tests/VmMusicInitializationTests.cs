namespace HCDE.Client.Tests;

public class VmMusicInitializationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InitializationCallbacksSeeNativeNoMusicAssignmentOrder(bool noMusic)
    {
        var enablement = new VmMusicEnableState { NoMusic = !noMusic };
        var calls = new List<string>();
        VmMusicInitialization.Initialize(enablement, noMusic, () => calls.Add("fonts"),
            () => { Assert.Equal(!noMusic, enablement.NoMusic); calls.Add("volume"); },
            () => { Assert.Equal(!noMusic, enablement.NoMusic); calls.Add("enabled"); },
            () => { Assert.Equal(noMusic, enablement.NoMusic); calls.Add("midi"); },
            () => calls.Add("callbacks"), () => calls.Add("resources"));
        Assert.Equal(new[] { "fonts", "volume", "enabled", "midi", "callbacks", "resources" }, calls);
    }

    [Fact]
    public void LiteInitializationSkipsResourceSetupButKeepsCallbacks()
    {
        var registered = false;
        VmMusicInitialization.Initialize(new(), true, () => { }, () => { }, () => { }, () => { },
            () => registered = true, () => throw new InvalidOperationException(), lite: true);
        Assert.True(registered);
    }

    [Fact]
    public void ResourceSetupUsesConvertedStartupDataInNativeOrder()
    {
        var calls = new List<string>();
        var gen = new byte[8 + 175 * 36];
        "#OPL_II#"u8.CopyTo(gen);
        var startup = VmMusicStartupData.Load(name => name == "GENMIDI" ? gen : name == "DMXGUSC" ? [1] : null,
            _ => [2], _ => throw new InvalidOperationException());
        VmMusicInitialization.Initialize(new(), false, () => { }, () => { }, () => { }, () => { },
            () => calls.Add("callbacks"), () => startup.Apply(_ => calls.Add("genmidi"),
                _ => calls.Add("gus"), _ => calls.Add("opn")));
        Assert.Equal(new[] { "callbacks", "genmidi", "gus", "opn" }, calls);
    }

    [Fact]
    public void EarlierCallbackFailurePropagatesBeforeNoMusicAssignment()
    {
        var enablement = new VmMusicEnableState();
        Assert.Throws<InvalidOperationException>(() => VmMusicInitialization.Initialize(enablement, true,
            () => { }, () => throw new InvalidOperationException("volume failed"),
            () => throw new Exception("unexpected"), () => throw new Exception("unexpected"),
            () => throw new Exception("unexpected"), () => throw new Exception("unexpected")));
        Assert.False(enablement.NoMusic);
    }
}
