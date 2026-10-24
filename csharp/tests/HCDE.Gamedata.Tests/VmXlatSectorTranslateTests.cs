namespace HCDE.Gamedata.Tests;

public class VmXlatSectorTranslateTests
{
    [Fact]
    public void TranslateSectorSpecial_AppliesMaskRules()
    {
        var masks = new List<VmSectorMaskRule> { new() { Mask = 0xF, Op = 1, Shift = 4 } };
        var translations = new List<VmSectorTranslation> { new() { NewType = 9, BitmaskAllowed = true } };
        var result = VmXlatSectorTranslate.TranslateSectorSpecial(0x15, masks, translations);
        Assert.NotEqual(0, result);
    }
}
