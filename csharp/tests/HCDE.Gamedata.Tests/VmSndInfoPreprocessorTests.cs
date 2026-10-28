namespace HCDE.Gamedata.Tests;

public class VmSndInfoPreprocessorTests
{
    private sealed class DoomOnly : ISndInfoGameFilter
    {
        public bool IsActiveGame(string gameSuffix) => gameSuffix.Equals("doom", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FilterGameBlocks_SkipsInactiveIfBlocks()
    {
        var tokens = VmLockDefTokenize.Tokenize("$ifheretic removed $endif kept");
        var filtered = VmSndInfoPreprocessor.FilterGameBlocks(tokens, new DoomOnly());
        Assert.Equal(["kept"], filtered);
    }
}
