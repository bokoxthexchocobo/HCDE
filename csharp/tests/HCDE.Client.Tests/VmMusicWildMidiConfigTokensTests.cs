using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfigTokensTests
{
    [Fact]
    public void SpacesTabsAndCommentsSeparateOrdinaryTokens()
    {
        Assert.Equal(new[] { "bank", "12" }, VmMusicWildMidiConfigTokens.Tokenize("  bank\t12  # comment"));
        Assert.Empty(VmMusicWildMidiConfigTokens.Tokenize("# comment"));
        Assert.Empty(VmMusicWildMidiConfigTokens.Tokenize(" \t"));
    }

    [Fact]
    public void NativeQuoteHandlingRetainsClosingQuote()
    {
        Assert.Equal(new[] { "dir", "two words\"" }, VmMusicWildMidiConfigTokens.Tokenize("dir \"two words\""));
        Assert.Equal(new[] { "abc\" def\"" }, VmMusicWildMidiConfigTokens.Tokenize("abc\" def\""));
    }

    [Fact]
    public void NativeHashStopsScanningWithoutTerminatingCurrentToken()
    {
        Assert.Equal(new[] { "12", "piano#tail words" }, VmMusicWildMidiConfigTokens.Tokenize("12 piano#tail words"));
        Assert.Equal(new[] { "dir", "a#b\" tail" }, VmMusicWildMidiConfigTokens.Tokenize("dir \"a#b\" tail"));
    }

    [Fact]
    public void EmbeddedNullEndsNativeStringAndEmptyQuotesDoNotCreateTokens()
    {
        Assert.Equal(new[] { "bank", "1" }, VmMusicWildMidiConfigTokens.Tokenize("bank 1\0 ignored"));
        Assert.Empty(VmMusicWildMidiConfigTokens.Tokenize("\"\""));
        Assert.Empty(VmMusicWildMidiConfigTokens.Tokenize(""));
    }
}
