namespace HCDE.Client.Tests;

public class VmStringTableLangIdTests
{
    [Fact]
    public void GetId_NormalizesEnUs()
    {
        var ids = new VmStringTableLangId();
        var id = ids.GetId("enu", useSystemLocaleForAuto: false);
        Assert.Equal("en-*-US", id.Name);
    }

    [Fact]
    public void GetFallback_MapsEnAu()
    {
        Assert.Equal("en-GB", VmStringTableLangId.GetFallback("en-*-AU"));
    }
}
