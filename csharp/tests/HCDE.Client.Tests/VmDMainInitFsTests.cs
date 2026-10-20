namespace HCDE.Client.Tests;

public class VmDMainInitFsTests
{
    [Fact]
    public void FsPrintf_ErrorUsesRedLevel() =>
        Assert.True(VmDMainInitFs.FsPrintfError(VmDMainInitFs.FsLevel.Error));

    [Fact]
    public void DInit_RejectsNullIwad() =>
        Assert.False(VmDMainInitFs.DInitRequiresIwad(null));

    [Fact]
    public void DInit_StaticRngWhenArgProvided() =>
        Assert.True(VmDMainInitFs.UseStaticRng(VmDMainInitFs.RngSeedFromArg("42")));

    [Fact]
    public void LumpFilter_AddsGameBlanket() =>
        Assert.True(VmDMainInitFs.BlanketMatches(1, 1));

    [Fact]
    public void WadInit_FailsWhenNoFiles() =>
        Assert.True(VmDMainInitFs.WadInitFatal(false));

    [Fact]
    public void FsPrintf_RoutesErrorToRed() =>
        Assert.True(VmDMainInitFs.RouteFsPrintf(VmDMainInitFs.FsLevel.Error).UseRed);

    [Fact]
    public void LumpFilter_DoomIdWorkaround() =>
        Assert.True(VmDMainInitFs.DoomIdDoomWorkaround("doom.id.doom2", out var rep) &&
                    rep.StartsWith("doom.doom"));

    [Fact]
    public void StartupProgress_HeadroomWhenGuessPositive() =>
        Assert.Equal(8192, VmDMainInitFs.StartupTextureProgressGuess(2048));
}
