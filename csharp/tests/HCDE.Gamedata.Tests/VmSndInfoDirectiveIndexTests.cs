namespace HCDE.Gamedata.Tests;

public class VmSndInfoDirectiveIndexTests
{
    [Fact]
    public void SoundSettingDirectives_IncludesAliasAndRandom()
    {
        var list = VmSndInfoDirectiveIndex.SoundSettingDirectives;
        Assert.Contains("$alias", list);
        Assert.Contains("$random", list);
    }
}
