namespace HCDE.Gamedata.Tests;

public class VmSndInfoPlayerClassRegistryTests
{
    [Fact]
    public void AddPlayerClass_Deduplicates()
    {
        var list = new List<VmSndInfoPlayerClass>();
        Assert.Equal(0, VmSndInfoPlayerClassRegistry.AddPlayerClass(list, "marine"));
        Assert.Equal(0, VmSndInfoPlayerClassRegistry.AddPlayerClass(list, "Marine"));
        Assert.Single(list);
    }

    [Fact]
    public void TryParsePlayerSoundCommon_ReadsTriple()
    {
        var tokens = VmLockDefTokenize.Tokenize("marine male *pain");
        var i = 0;
        var errors = new List<string>();
        Assert.True(VmSndInfoPlayerClassRegistry.TryParsePlayerSoundCommon(tokens, ref i, out var cls, out var gender, out var slot, errors));
        Assert.Equal("marine", cls);
        Assert.Equal(0, gender);
        Assert.Equal("*pain", slot);
    }
}
