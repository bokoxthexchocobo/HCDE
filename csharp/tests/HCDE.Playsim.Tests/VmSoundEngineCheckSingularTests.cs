namespace HCDE.Playsim.Tests;

public class VmSoundEngineCheckSingularTests
{
    [Fact]
    public void CheckSingular_FindsOrgId()
    {
        Assert.True(VmSoundEngineCheckSingular.CheckSingular(5, new[] { 1, 5, 9 }));
    }

    [Fact]
    public void ChangeSoundVolume_ClipsAndUpdates()
    {
        var actor = new object();
        var channels = new List<(int, object?, int, float)> { (1, actor, 0, 0.5f) };
        VmSoundEngineCheckSingular.ChangeSoundVolume(1, actor, -1, 2, channels, (i, v) => channels[i] = (1, actor, 0, v));
        Assert.Equal(1f, channels[0].Item4);
    }
}
