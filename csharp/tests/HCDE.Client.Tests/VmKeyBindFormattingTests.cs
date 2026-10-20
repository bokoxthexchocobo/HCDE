namespace HCDE.Client.Tests;

public class VmKeyBindFormattingTests
{
    [Fact]
    public void NameKeys_FormatsTwoKeys()
    {
        var text = VmKeyBindFormatting.NameKeys(
            VmKeyNames.GetKeyFromName("F11"),
            VmKeyNames.GetKeyFromName("F12"));
        Assert.Contains("F11", text);
        Assert.Contains("F12", text);
    }
}
