namespace HCDE.Playsim.Tests;

public class VmFScriptInitFunctionsTests
{
    [Fact]
    public void RegisterFraggleFunctions_BindsMoveCamera()
    {
        var map = new Dictionary<string, Action<VmFScriptParser.ParserContext>>(StringComparer.OrdinalIgnoreCase);
        VmFScriptInitFunctions.RegisterFraggleFunctions(map);
        Assert.True(map.ContainsKey("movecamera"));
        Assert.True(map.ContainsKey("spawnmissile"));
        Assert.True(map.ContainsKey("resurrect"));
        Assert.True(map.ContainsKey("setcorona"));
    }

    [Fact]
    public void InitFunctions_ResolvesActorTypesAndLineSpecs()
    {
        var state = VmFScriptInitFunctions.InitFunctions(
            name => name == "DoomPlayer" ? new object() : null,
            i => i == 4
                ? new VmFScriptInitFunctions.LineSpecialMeta("DoorRaise", 4)
                : null,
            maxLineSpecial: 4);
        Assert.NotNull(state.ActorTypes[0]);
        Assert.True(state.LineSpecConstants.ContainsKey("DoorRaise"));
        Assert.True(state.Functions.ContainsKey("wait"));
        Assert.Equal(VmFScriptInitFunctions.ActorNamesInit.Length, state.ActorTypes.Length);
    }
}
