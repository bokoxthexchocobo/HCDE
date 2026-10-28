namespace HCDE.Client.Tests;

public class VmDMainStartupResourceLoadTests
{
    [Fact]
    public void FatalWhenNoPlayerClasses() =>
        Assert.True(VmDMainStartupResourceLoad.FatalWhenNoPlayerClasses(0));

    [Fact]
    public void LateVideoInit_WhenNotInitialized() =>
        Assert.True(VmDMainStartupResourceLoad.LateVideoInitWhenNeeded(false, false, false, false));

    [Fact]
    public void LockLocalSnd_WhenDehAddsSounds() =>
        Assert.True(VmDMainStartupResourceLoad.LockLocalSndWhenDehAdded(10, 12));

    [Fact]
    public void DefaultDeh_WhenDoomAndNoPatches() =>
        Assert.True(VmDMainStartupResourceLoad.DefaultDehWhenDoom(1, false, false));

    [Fact]
    public void CheckNetGameWhenNotRestart() =>
        Assert.True(VmDMainStartupResourceLoad.CheckNetGameWhenNotRestart(false));

    [Fact]
    public void PlayloopInit() =>
        Assert.True(VmDMainStartupResourceLoad.PlayloopInit(true));
}
