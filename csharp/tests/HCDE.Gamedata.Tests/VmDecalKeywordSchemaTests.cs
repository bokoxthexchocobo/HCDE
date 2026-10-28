namespace HCDE.Gamedata.Tests;

public class VmDecalKeywordSchemaTests
{
    [Fact]
    public void DecalKeywords_EndsWithTranslatable()
    {
        Assert.Equal("translatable", VmDecalKeywordSchema.DecalKeywords[^1]);
    }
}
