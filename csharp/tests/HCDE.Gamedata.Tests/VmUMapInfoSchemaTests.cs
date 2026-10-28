namespace HCDE.Gamedata.Tests;

public class VmUMapInfoSchemaTests
{
    [Fact]
    public void LumpNameFields_MatchesNativeLayout()
    {
        Assert.Equal(12, VmUMapInfoSchema.LumpNameFields.Count);
        Assert.Equal("interbackdrop", VmUMapInfoSchema.LumpNameFields[^2]);
    }
}
