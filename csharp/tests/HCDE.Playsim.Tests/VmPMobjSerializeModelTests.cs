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
}
