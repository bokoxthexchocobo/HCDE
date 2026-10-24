namespace HCDE.Client.Tests;

public class VmStringTableLocaleTests
{
    [Fact]
    public void RemapLegacyLanguage_UpdatesEnu()
    {
        var code = "enu";
        Assert.True(VmStringTableLocale.RemapLegacyLanguage(ref code));
        Assert.Equal("en-US", code);
    }
}
