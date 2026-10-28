namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseLookupNameTests
{
    [Fact]
    public void TryParseLookupToken_StripsDollarPrefix()
    {
        Assert.True(VmMapInfoParseLookupName.TryParseLookupToken("$MYTEXT", out var dest, out var lookup));
        Assert.Equal("MYTEXT", dest);
        Assert.True(lookup);
    }
}
