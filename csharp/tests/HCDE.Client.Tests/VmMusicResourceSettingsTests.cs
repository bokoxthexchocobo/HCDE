namespace HCDE.Client.Tests;

public class VmMusicResourceSettingsTests
{
    [Fact]
    public void ResourceOverridesSelectDeviceArgumentsModuleAndVolume()
    {
        var result = VmMusicResourceSettings.Resolve(3, 9,
            new Dictionary<int, VmMusicDeviceSetting> { [3] = new(4, "bank.sf2") },
            new Dictionary<int, int> { [3] = 2 }, new Dictionary<int, float> { [3] = .5f });
        Assert.Equal(new VmMusicResolvedSettings(4, "bank.sf2", 2, .5f), result);
    }

    [Fact]
    public void MissingOverridesUseNativeDefaultDeviceAndPreferredModule()
    {
        var result = VmMusicResourceSettings.Resolve(-1, 9, new Dictionary<int, VmMusicDeviceSetting>(),
            new Dictionary<int, int>(), new Dictionary<int, float>());
        Assert.Equal(new VmMusicResolvedSettings(-1, "", 9, null), result);
        var calls = 0;
        VmMusicResourceSettings.ApplyVolume(result, _ => throw new InvalidOperationException(),
            (device, args) => { Assert.Equal(-1, device); Assert.Equal("", args); calls++; });
        Assert.Equal(1, calls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void ExplicitVolumeBypassesReplayGainWithoutClamping(float volume)
    {
        var applied = new List<float>();
        VmMusicResourceSettings.ApplyVolume(new(4, "bank", 0, volume), applied.Add,
            (_, _) => throw new InvalidOperationException());
        Assert.Equal(new[] { volume }, applied);
    }
}
