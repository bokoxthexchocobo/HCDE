namespace HCDE.Client.Tests;

public class VmMiscResponseUncommentTests
{
    [Fact]
    public void Uncomment_StripsCommentsAndWhitespace()
    {
        var result = VmMiscResponseUncomment.Uncomment("# comment\n  -file foo.wad\n", VmMiscResponseUncomment.UncommentFlags.StripWhitespace | VmMiscResponseUncomment.UncommentFlags.SkipEmpty);
        Assert.Contains("-file foo.wad", result);
        Assert.DoesNotContain("#", result);
    }
}
