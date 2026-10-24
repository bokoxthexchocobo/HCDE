namespace HCDE.Client.Tests;

public class VmKeyBindingsPerformBindTests
{
    [Fact]
    public void PerformBind_SetsAndQueriesBinding()
    {
        var binds = new VmKeyBindings();
        var lines = new List<string>();
        binds.PerformBind(["bind", "f1", "save"], lines.Add);
        binds.PerformBind(["bind", "f1"], lines.Add);
        Assert.Contains("save", lines[0]);
    }

    [Fact]
    public void PerformBind_ListsActiveBindings()
    {
        var binds = new VmKeyBindings();
        binds.DoBind("f2", "quit");
        var lines = new List<string>();
        binds.PerformBind(["bind"], lines.Add);
        Assert.Contains(lines, s => s.Contains("quit"));
    }
}
