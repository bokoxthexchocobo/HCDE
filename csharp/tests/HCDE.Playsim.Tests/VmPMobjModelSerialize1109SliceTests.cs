namespace HCDE.Playsim.Tests;

public class VmPMobjModelSerialize1109SliceTests
{
    [Fact]
    public void ApplyAnimModelOverrideRead_SetsMissingId()
    {
        var ctx = new VmPMobjModelSerialize1109Slice.AnimModelOverrideReadContext
        {
            ObjectPresent = false,
            Id = 4
        };
        VmPMobjModelSerialize1109Slice.ApplyAnimModelOverrideRead(ctx);
        Assert.Equal(VmPMobjModelSerialize1109Slice.MissingAnimModelId, ctx.Id);
    }

    [Fact]
    public void ApplyModelAnimFrameRead_ClearsPrecalculatedIqm()
    {
        var ctx = new VmPMobjModelSerialize1109Slice.ModelAnimFrameReadContext
        {
            TypeTag = "precalcIQM"
        };
        VmPMobjModelSerialize1109Slice.ApplyModelAnimFrameRead(ctx);
        Assert.True(ctx.Cleared);
        Assert.Equal(VmPMobjModelSerialize1109Slice.FrameKind.Null, ctx.Kind);
    }

    [Fact]
    public void FrameKindWriteTag_ArchivesPrecalcAsNull()
    {
        Assert.Equal("nullptr", VmPMobjModelSerialize1109Slice.FrameKindWriteTag(
            VmPMobjModelSerialize1109Slice.FrameKind.PrecalculatedIqm));
        Assert.Equal("interp", VmPMobjModelSerialize1109Slice.FrameKindWriteTag(
            VmPMobjModelSerialize1109Slice.FrameKind.Interp));
    }

    [Fact]
    public void RunModelDataDestroy_ZerosCollections()
    {
        var ctx = new VmPMobjModelSerialize1109Slice.ModelDataDestroyContext
        {
            ModelCount = 3,
            GeneratorCount = 2,
            SkinCount = 1,
            AnimationCount = 4
        };
        VmPMobjModelSerialize1109Slice.RunModelDataDestroy(ctx);
        Assert.Equal(0, ctx.ModelCount);
        Assert.Equal(0, ctx.AnimationCount);
    }
}
