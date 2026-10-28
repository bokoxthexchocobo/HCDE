namespace HCDE.Gamedata.Tests;

public class DecorateScriptPropertyDispatcherTests
{
    [Fact]
    public void DispatchesBoolIntAndStringFields()
    {
        var target = new DecorateScriptPropertyDispatcher.DecorateScriptPropertyTarget();
        var fields = new[]
        {
            new DecorateScriptPropertyDispatcher.ScriptFieldBinding { Name = "Ambush", Kind = DecorateScriptPropertyDispatcher.ScriptFieldKind.Bool },
            new DecorateScriptPropertyDispatcher.ScriptFieldBinding { Name = "Mass", Kind = DecorateScriptPropertyDispatcher.ScriptFieldKind.Int },
            new DecorateScriptPropertyDispatcher.ScriptFieldBinding { Name = "Tag", Kind = DecorateScriptPropertyDispatcher.ScriptFieldKind.String },
        };
        Assert.True(DecorateScriptPropertyDispatcher.TryDispatch(fields, ["0", ",", "100", ",", "Pickup"], target, out _));
        Assert.False(target.MetaBools["Ambush"]);
        Assert.Equal(100, target.Ints["Mass"]);
        Assert.Equal("Pickup", target.Strings["Tag"]);
    }
}
