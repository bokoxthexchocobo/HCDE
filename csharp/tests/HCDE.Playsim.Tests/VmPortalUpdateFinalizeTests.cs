namespace HCDE.Playsim.Tests;

public class VmPortalUpdateFinalizeTests
{
    [Fact]
    public void UpdatePortal_ClearsFlagsWithoutDestination()
    {
        var port = new VmPortalUpdateFinalize.LinePortalState { DefFlags = 7, DestinationLineIndex = null };
        VmPortalUpdateFinalize.UpdatePortal(port, null);
        Assert.Equal(0, port.Flags);
    }

    [Fact]
    public void FinalizePortals_InvokesBuildAndCreate()
    {
        var built = false;
        var created = false;
        var ctx = new VmPortalUpdateFinalize.FinalizePortalsContext
        {
            BuildPortalBlockmap = () => built = true,
            CreateLinkedPortals = () => created = true,
        };
        ctx.LinePortals.Add(new VmPortalUpdateFinalize.LinePortalState());
        VmPortalUpdateFinalize.FinalizePortals(ctx);
        Assert.True(built && created);
    }
}
