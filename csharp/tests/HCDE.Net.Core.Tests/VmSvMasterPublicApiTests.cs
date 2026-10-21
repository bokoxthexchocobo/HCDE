namespace HCDE.Net.Core.Tests;

public class VmSvMasterPublicApiTests
{
    [Fact]
    public void ShouldShutdownWhenDisabledNative_WhenMastersOffButSocketOpen()
    {
        Assert.True(VmSvMasterPublicApi.ShouldShutdownWhenDisabledNative(svUseMasters: false, masterSocket: 3));
    }

    [Fact]
    public void ShouldReresolveAddressesNative_RespectsInterval()
    {
        Assert.True(VmSvMasterPublicApi.ShouldReresolveAddressesNative(10_000, 0, 1000, force: false));
        Assert.False(VmSvMasterPublicApi.ShouldReresolveAddressesNative(1500, 1000, 1000, force: false));
    }

    [Fact]
    public void FormatRegisteredMasterLineNative_IncludesErrorWhenPresent()
    {
        var line = VmSvMasterPublicApi.FormatRegisteredMasterLineNative(
            "host", "1.2.3.4", 5029, 60, 5, "timeout");
        Assert.Contains("last=\"timeout\"", line);
    }

    [Fact]
    public void InitMastersNative_SkipsWhenAdvertisingDisabled()
    {
        var printed = "";
        var ctx = new InitMastersContext
        {
            ResolveStartupAdvertising = () => false,
            DebugMark = _ => { },
            Print = s => printed = s,
            MasterSocket = -1,
            CreateMasterSocket = () => 1,
            AddStartupMasters = () => { },
            MasterCount = 0,
            DefaultMasters = Array.Empty<string>(),
            AddMaster = _ => { },
            UpdateMaster = _ => { },
        };
        Assert.False(VmSvMasterPublicApi.InitMastersNative(ctx));
        Assert.Contains("disabled", printed);
    }

    [Fact]
    public void UpdateMasterNative_RunsKeepAliveWhenIntervalElapsed()
    {
        var cycles = 0;
        var ctx = new UpdateMasterContext
        {
            SvUseMasters = true,
            MasterSocket = 1,
            MasterCount = 1,
            NowMs = () => 20_000,
            ReresolveIntervalMs = 100_000,
            KeepAliveIntervalMs = 1000,
            LastKeepAlive = 0,
            DebugMark = _ => { },
            ReresolveMaster = _ => { },
            UpdateMasterServersInternal = _ => cycles++,
        };
        VmSvMasterPublicApi.UpdateMasterNative(ctx, force: false);
        Assert.Equal(1, cycles);
    }
}
