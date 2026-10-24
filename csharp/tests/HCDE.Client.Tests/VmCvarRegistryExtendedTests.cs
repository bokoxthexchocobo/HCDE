namespace HCDE.Client.Tests;

public class VmCvarRegistryExtendedTests
{
    [Fact]
    public void SetArchiveDefaults_RestoresDefaultValue()
    {
        var reg = new VmCvarRegistry();
        reg.Register(new VmCvarEntry("foo", VmCvarFlags.Archive, "changed", "default"));
        reg.SetArchiveDefaults();
        Assert.Equal("default", reg.Find("foo")!.Value);
    }

    [Fact]
    public void Create_RegistersNewCvar()
    {
        var reg = new VmCvarRegistry();
        reg.Create("bar", "1", VmCvarFlags.Archive);
        Assert.Equal("1", reg.Find("bar")!.Value);
    }
}
