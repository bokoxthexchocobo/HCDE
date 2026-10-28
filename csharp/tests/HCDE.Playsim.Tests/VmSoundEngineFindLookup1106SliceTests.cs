namespace HCDE.Playsim.Tests;

public class VmSoundEngineFindLookup1106SliceTests
{
    [Fact]
    public void ScanFindByLump_ReturnsIndexWhenPresent()
    {
        var id = VmSoundEngineFindLookup1106Slice.ScanFindByLump(42, new[] { 0, 10, 42 });
        Assert.Equal(2, id);
    }

    [Fact]
    public void ResolveFindSoundTentative_CreatesWhenMissing()
    {
        var id = VmSoundEngineFindLookup1106Slice.ResolveFindSoundTentative(
            VmSoundEngineFindLookup1106Slice.NoSound,
            () => 3);
        Assert.Equal(3, id);
    }

    [Fact]
    public void RunCacheRandomSound_InvokesCacheForEachChoice()
    {
        var cached = new List<int>();
        VmSoundEngineFindLookup1106Slice.RunCacheRandomSound(
            new VmSoundEngineFindLookup1106Slice.CacheRandomSoundContext
            {
                RandomHeader = true,
                ChoiceIndices = new[] { 1, 2 },
                CacheChoice = cached.Add
            });
        Assert.Equal(new[] { 1, 2 }, cached);
    }

    [Fact]
    public void ResolveMsLength_PicksLongestRandomChoice()
    {
        var len = VmSoundEngineFindLookup1106Slice.ResolveMsLength(
            new VmSoundEngineFindLookup1106Slice.MsLengthContext
            {
                ValidSoundId = true,
                HasLink = true,
                RandomHeader = true,
                RandomChoices = new[] { 1, 2 },
                ResolveLinkedLength = id => id == 2 ? 500 : 100
            });
        Assert.Equal(500, len);
    }
}
