namespace HCDE.Client.Tests;

public class VmCvarSearchTests
{
    [Fact]
    public void ContainsNoCase_MatchesSubstring()
    {
        Assert.True(VmCvarSearch.ContainsNoCase("gl_texture", "texture"));
        Assert.False(VmCvarSearch.ContainsNoCase("gl", "texture"));
    }
}
