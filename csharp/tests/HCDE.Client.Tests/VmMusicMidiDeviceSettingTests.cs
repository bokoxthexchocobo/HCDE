namespace HCDE.Client.Tests;

public class VmMusicMidiDeviceSettingTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SparseValidSelectionCommitsBeforeBackendAndRestartsOnlyOnChange(bool changed)
    {
        var setting = new VmMusicMidiDeviceSetting();
        var calls = new List<string>();
        setting.Apply(12, [-5, 2, 12], device =>
        { Assert.Equal(12, setting.Device); Assert.Equal(12, device); calls.Add("configure"); return changed; },
            device => { Assert.Equal(12, device); calls.Add("restart"); },
            _ => throw new InvalidOperationException());
        Assert.Equal(changed ? new[] { "configure", "restart" } : new[] { "configure" }, calls);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(3, true)]
    public void InvalidSelectionConfiguresAvailableDefaultWithNativeWarningRules(int requested, bool warns)
    {
        var setting = new VmMusicMidiDeviceSetting();
        var reports = new List<string>();
        var configured = new List<int>();
        setting.Apply(requested, [-5, 12], device => { configured.Add(device); return true; },
            device => Assert.Equal(-5, device), reports.Add);
        Assert.Equal(-5, setting.Device);
        Assert.Equal(new[] { -5 }, configured);
        Assert.Equal(warns ? new[] { "ID out of range. Using default device.\n" } : Array.Empty<string>(), reports);
    }

    [Theory]
    [InlineData(-5, 1)]
    [InlineData(9, 2)]
    [InlineData(0, 1)]
    public void MissingDefaultTerminatesWithoutBackendCalls(int requested, int warnings)
    {
        var setting = new VmMusicMidiDeviceSetting();
        var reports = new List<string>();
        setting.Apply(requested, [], _ => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException(), reports.Add);
        Assert.Equal(-5, setting.Device);
        Assert.Equal(warnings, reports.Count);
    }

    [Fact]
    public void BackendFailurePropagatesAfterCommittingSelectionWithoutRestart()
    {
        var setting = new VmMusicMidiDeviceSetting();
        Assert.Throws<InvalidOperationException>(() => setting.Apply(12, [12],
            _ => throw new InvalidOperationException("configuration failed"),
            _ => throw new Exception("unexpected restart"), _ => throw new Exception("unexpected report")));
        Assert.Equal(12, setting.Device);
    }
}
