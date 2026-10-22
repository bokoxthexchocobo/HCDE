namespace HCDE.Playsim.Tests;

public class VmPMobjSerializeModelTests
{
    [Fact]
    public void SerializeModelId_ReadMissingKey_ReturnsNegativeOne()
    {
        var id = VmPMobjSerializeModel.SerializeModelId(new() { Writing = false, HasKey = false }, 3);
        Assert.Equal(-1, id);
    }

    [Fact]
    public void SerializeModelId_WritePositiveId_EmitsPath()
    {
        string? written = null;
        var id = VmPMobjSerializeModel.SerializeModelId(new()
        {
            Writing = true,
            ModelPath = "models/foo.md3",
            WriteModelPath = (_, v) => written = v,
        }, 2);
        Assert.Equal(2, id);
        Assert.Equal("models/foo.md3", written);
    }

    [Fact]
    public void ReadModelAnimFrameType_PrecalcMapsToNull()
    {
        Assert.Equal(VmPMobjSerializeModel.ModelAnimFrameKind.Null,
            VmPMobjSerializeModel.ReadModelAnimFrameType("precalcIQM"));
    }

    [Fact]
    public void SerializeBoneOverrideComponent_WritesModeFields()
    {
        var comp = new VmPMobjSerializeModel.BoneOverrideComponent { Mode = 2 };
        VmPMobjSerializeModel.SerializeBoneOverrideComponent(comp, writing: true);
        Assert.Equal(2, comp.Mode);
    }

    [Fact]
    public void SerializeBoneOverride_OpensObject()
    {
        var fields = new VmPMobjSerializeModel.BoneOverrideFields();
        VmPMobjSerializeModel.SerializeBoneOverride(fields, writing: true);
        Assert.NotNull(fields.Translation);
    }

    [Fact]
    public void SerializeTrs_OpensObject()
    {
        var trs = VmPMobjSerializeModel.SerializeTrs(new() { ScalingX = 2 }, writing: true);
        Assert.Equal(2, trs.ScalingX);
    }

    [Fact]
    public void SerializeModelOverride_EmitsModelId()
    {
        var mo = VmPMobjSerializeModel.SerializeModelOverride(new() { Writing = true, ModelPath = "x" },
            new() { ModelId = 1 });
        Assert.Equal(1, mo.ModelId);
    }

    [Fact]
    public void SerializeAnimModelOverride_ReadGuard()
    {
        var amo = VmPMobjSerializeModel.SerializeAnimModelOverride(new(), new(), objectOk: false);
        Assert.Equal(-1, amo.Id);
    }

    [Fact]
    public void SerializeModelAnim_WritesFrameFields()
    {
        var anim = VmPMobjSerializeModel.SerializeModelAnim(new() { FirstFrame = 3 }, writing: true);
        Assert.Equal(3, anim.FirstFrame);
    }

    [Fact]
    public void WriteModelAnimFrameType_Nullptr()
    {
        Assert.Equal(VmPMobjSerializeModel.ModelAnimFrameKind.Null,
            VmPMobjSerializeModel.WriteModelAnimFrameType(VmPMobjSerializeModel.ModelAnimFrameKind.Null));
    }
}
