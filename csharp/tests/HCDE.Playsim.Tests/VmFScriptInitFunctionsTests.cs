namespace HCDE.Playsim.Tests;

public class VmFScriptInitFunctionsTests
{
    [Fact]
    public void ActorNamesInit_MatchesNativeTable()
    {
        Assert.Equal(139, VmFScriptInitFunctions.ActorNameCount);
        Assert.Equal("DoomPlayer", VmFScriptInitFunctions.ActorNamesInit[0]);
        Assert.Equal("PointPuller", VmFScriptInitFunctions.ActorNamesInit[^1]);
    }

    [Fact]
    public void ConstructThinker_RegistersFraggleFunctions()
    {
        var th = new VmFScriptThinker.ThinkerState
        {
            FraggleFindActorClass = name => name == "DoomPlayer" ? new object() : null,
            FraggleLineSpecialAt = i => i == 4
                ? new VmFScriptInitFunctions.LineSpecialDescriptor(4, "DoorRaise", 4)
                : null,
        };
        VmFScriptThinker.ConstructThinker(th);
        Assert.NotNull(th.Fraggle.ActorTypes[0]);
        Assert.True(th.Fraggle.Functions.ContainsKey("wait"));
        Assert.True(th.Fraggle.LineSpecConstants.ContainsKey("DoorRaise"));
        Assert.True(th.Fraggle.Functions.ContainsKey("opendoor"));
    }
}
