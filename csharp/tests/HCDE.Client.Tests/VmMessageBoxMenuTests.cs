namespace HCDE.Client.Tests;

public class VmMessageBoxMenuTests
{
    [Fact]
    public void CreateMessageBoxMenu_FallsBackToDefaultClass()
    {
        var menu = VmMessageBoxMenu.CreateMessageBoxMenu(
            null,
            "Quit?",
            1,
            true,
            resolveClass: _ => "NotMessageBox");
        Assert.Equal(VmMessageBoxMenu.DefaultMenuClass, menu.ClassName);
        Assert.Equal("Quit?", menu.Message);
    }

    [Fact]
    public void StartMessage_ActivatesChildWithParentLink()
    {
        VmMessageBoxMenuInstance? activated = null;
        var parent = VmMessageBoxMenu.CreateMessageBoxMenu(null, "old", 0, false);
        var child = VmMessageBoxMenu.StartMessage(parent, false, "new", 2, "Ok", menu => activated = menu);
        Assert.Same(parent, child.ParentMenu);
        Assert.NotNull(activated);
    }
}
