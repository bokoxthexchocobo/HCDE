namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchSpriteTests
{
    [Fact]
    public void ComputeNameOffset_SubtractsTableBase()
    {
        var toff = new List<int> { 129044, 129044, 129044, 129284, 129380 };
        var offset = VmDehackedPatchSprite.ComputeNameOffset(129044 + 22044 + 8, 0, 0, toff);
        Assert.Equal(1, offset);
    }
}
