namespace HCDE.Client.Tests;

public class VmNameTableTests
{
    [Fact]
    public void FindName_CreatesAndReuses()
    {
        var table = new VmNameTable();
        var a = table.FindName("Pickup");
        var b = table.FindName("pickup");
        Assert.Equal(a, b);
        Assert.Equal(0, table.FindName("missing", noCreate: true));
    }
}
