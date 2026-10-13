using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfigBankTests
{
    [Fact]
    public void BankAndDrumsetMaskNumbersAndReplacePreviousSelection()
    {
        var state = new VmMusicWildMidiConfigBank();
        Assert.Equal(0, state.PatchId);
        Assert.True(state.Apply("DRUMSET 257suffix ignored"));
        Assert.Equal(0x180, state.PatchId);
        Assert.Equal(0x185, state.SelectInstrument("133patch"));
        Assert.True(state.Apply("BaNk 258"));
        Assert.Equal(0x200, state.PatchId);
        Assert.Equal(0x27f, state.SelectInstrument("255"));
        Assert.True(state.Apply("bank 256"));
        Assert.Equal(0, state.PatchId);
    }

    [Fact]
    public void InstrumentSelectionPreservesBankAndDrumFlagForCatalogLookup()
    {
        var state = new VmMusicWildMidiConfigBank();
        state.Apply("drumset 12");
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        var patch = catalog.Register(state.SelectInstrument("35"), "kick.pat");
        Assert.Same(patch, catalog.FindExact(0x0ca3));
        Assert.Equal(0x0ca4, state.SelectInstrument("36"));
    }

    [Fact]
    public void UnhandledLinesLeaveSelectionUnchanged()
    {
        var state = new VmMusicWildMidiConfigBank();
        state.Apply("bank 3");
        Assert.False(state.Apply("# comment"));
        Assert.False(state.Apply("dir patches"));
        Assert.Equal(0x300, state.PatchId);
    }

    [Fact]
    public void MalformedAndOverflowingNumbersFailWithoutMutatingSelection()
    {
        var state = new VmMusicWildMidiConfigBank();
        state.Apply("drumset 1");
        foreach (var line in new[] { "bank", "bank -1", "bank +1", "bank x", "bank 2147483648", "bank 999999999999999999999" })
        {
            Assert.Throws<InvalidDataException>(() => state.Apply(line));
            Assert.Equal(0x180, state.PatchId);
        }
        Assert.Throws<InvalidDataException>(() => state.SelectInstrument(""));
        Assert.Throws<InvalidDataException>(() => state.SelectInstrument("2147483648"));
        Assert.Equal(0x180, state.PatchId);
        Assert.True(state.Apply("bank 2147483647tail"));
        Assert.Equal(0xff00, state.PatchId);
    }
}
