namespace HCDE.Playsim.Tests;

public class VmPLnspecGlassConversation933SliceTests
{
    [Fact]
    public void BuildGlassShard_DefaultJunk_ComputesStateOffset()
    {
        var shard = VmPLnspecGlassConversation933Slice.BuildGlassShard(0, null, 17, 5);
        Assert.False(shard.SpawnCustom);
        Assert.Equal(2, shard.StateOffset);
    }

    [Fact]
    public void BuildQuestGrant_SkipsWhenNotSwitched()
    {
        var grant = VmPLnspecGlassConversation933Slice.BuildQuestGrant(
            switched: false, noJunkArg: false, quest1: true, quest2: false, activator: new(), firstPlayerMo: null);
        Assert.False(grant.Grant);
    }

    [Fact]
    public void BuildQuestGrant_ReturnsInventoryList()
    {
        var grant = VmPLnspecGlassConversation933Slice.BuildQuestGrant(
            switched: true, noJunkArg: false, quest1: true, quest2: false, activator: new(), firstPlayerMo: null);
        Assert.True(grant.Grant);
        Assert.Equal(3, grant.Items.Count);
    }

    [Fact]
    public void TryStartConversation_RejectsDeadPlayer()
    {
        var invoke = VmPLnspecGlassConversation933Slice.TryStartConversation(
            new object(), new object(), new object(), true, 0, 1);
        Assert.Null(invoke);
    }

    [Fact]
    public void BuildConversationAssign_RejectsMissingDialog()
    {
        Assert.Null(VmPLnspecGlassConversation933Slice.BuildConversationAssign(5, -1));
    }

    [Fact]
    public void BuildPlaneReflection_SkipsSlopedFloor()
    {
        var invoke = VmPLnspecGlassConversation933Slice.BuildPlaneReflection(128, 64, floorSlope: true, ceilingSlope: false);
        Assert.False(invoke.ApplyFloor);
        Assert.True(invoke.ApplyCeiling);
    }

    [Fact]
    public void BuildGlobalFog_DensityHalvesValue()
    {
        var invoke = VmPLnspecGlassConversation933Slice.BuildGlobalFog(0, 10);
        Assert.NotNull(invoke);
        Assert.Equal(5, invoke!.Value.Value);
    }
}
