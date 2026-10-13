namespace HCDE.Client.Tests;

public class VmMusicWildMidiPatchLookupTests
{
    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void DrumTrackSwitchClearsPatchOrLoadsDefaultWithoutChangingBank(int value, bool drum)
    {
        var patch = new Patch();
        var channel = new VmMusicWildMidiChannelState { Bank = 5, Patch = patch };
        VmMusicWildMidiPatchLookup.SetDrumTrack(channel, value, id =>
            { Assert.False(drum); Assert.Equal(0, id); return patch; });
        Assert.Equal(drum, channel.IsDrum);
        Assert.Equal(5, channel.Bank);
        Assert.Same(drum ? null : patch, channel.Patch);
    }

    [Fact]
    public void ResetMatchesNativeDefaultsAndMarksDrumChannelAfterGlobalAdjustment()
    {
        var patch = new Patch();
        var channels = Enumerable.Range(0, 16).Select(_ => new VmMusicWildMidiChannelState
            { NonRegistered = 1, PitchAdjust = 123, IsDrum = true }).ToArray();
        var lookups = 0;
        VmMusicWildMidiPatchLookup.ResetChannels(channels, id =>
            { Assert.Equal(0, id); lookups++; return patch; }, channel =>
            { Assert.Equal(16, channel); Assert.All(channels, c => Assert.False(c.IsDrum)); });
        Assert.Equal(15, lookups);
        for (var i = 0; i < 16; i++)
        {
            var channel = channels[i];
            Assert.Equal(i == 9, channel.IsDrum); Assert.Same(i == 9 ? null : patch, channel.Patch);
            Assert.Equal(100, channel.Volume); Assert.Equal(64, channel.Pan); Assert.Equal(64, channel.Balance);
            Assert.Equal(127, channel.Pressure); Assert.Equal(127, channel.Expression);
            Assert.Equal(200, channel.PitchRange); Assert.Equal(0xffff, channel.RegisteredParameter);
            Assert.Equal(1, channel.NonRegistered); Assert.Equal(123, channel.PitchAdjust);
        }
    }

    private sealed class Patch : IVmMusicWildMidiPatch
    {
        public ushort Id { get; init; }
        public bool Loaded { get; set; }
        public bool HasSamples => true;
        public int UseCount { get; set; }
    }

    [Fact]
    public void MelodicProgramChangeUsesChannelBankAndFallbackThenClearsPatchOnMiss()
    {
        var patch = new Patch { Id = 5 };
        var channel = new VmMusicWildMidiChannelState { Bank = 2 };
        var calls = new List<ushort>();
        VmMusicWildMidiPatchLookup.ChangeProgram(channel, 5, id =>
            { calls.Add(id); return id == 5 ? patch : null; });
        Assert.Same(patch, channel.Patch);
        Assert.Equal(new ushort[] { 0x205, 5 }, calls);
        Assert.Equal(2, channel.Bank);
        VmMusicWildMidiPatchLookup.ChangeProgram(channel, 6, _ => null);
        Assert.Null(channel.Patch);
    }

    [Fact]
    public void PercussionProgramChangeSelectsBankWithoutLookupOrPatchMutation()
    {
        var patch = new Patch();
        var channel = new VmMusicWildMidiChannelState { IsDrum = true, Patch = patch };
        VmMusicWildMidiPatchLookup.ChangeProgram(channel, 300, _ => throw new Exception("unexpected lookup"));
        Assert.Equal(44, channel.Bank);
        Assert.Same(patch, channel.Patch);
    }

    [Fact]
    public void ProgramLookupFailurePreservesExistingPatchAndException()
    {
        var patch = new Patch();
        var channel = new VmMusicWildMidiChannelState { Patch = patch };
        var error = new IOException("lookup failed");
        Assert.Same(error, Assert.Throws<IOException>(() =>
            VmMusicWildMidiPatchLookup.ChangeProgram(channel, 5, _ => throw error)));
        Assert.Same(patch, channel.Patch);
    }

    private sealed record Sample(uint Low, uint High);

    [Theory]
    [InlineData(127, 3, 127, 32767)]
    [InlineData(256, 0, 1, 1)]
    [InlineData(1, -1, 2, 386)]
    public void InstrumentPackingMatchesNativeUnsignedShortNarrowing(int bank, int drum, int program, int expected)
    {
        Assert.Equal(expected, VmMusicWildMidiPatchLookup.InstrumentId(bank, drum, program));
    }

    [Fact]
    public void BankMissFallsBackToBankZeroWithoutDroppingPercussionBit()
    {
        var calls = new List<ushort>();
        var patch = new object();
        Assert.Same(patch, VmMusicWildMidiPatchLookup.Find(0x12ab,
            id => { calls.Add(id); return id == 0xab ? patch : null; }));
        Assert.Equal(new ushort[] { 0x12ab, 0xab }, calls);
        calls.Clear();
        Assert.Null(VmMusicWildMidiPatchLookup.Find<object>(0xab, id => { calls.Add(id); return null; }));
        Assert.Equal(new ushort[] { 0xab }, calls);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, 0)]
    [InlineData(19, 0)]
    [InlineData(20, 0)]
    [InlineData(21, 1)]
    [InlineData(40, 1)]
    [InlineData(41, 2)]
    public void SampleSelectionUsesStrictBoundsAndNativeLastEligibleFallback(uint frequency, int expected)
    {
        Sample[] samples = [new(10, 20), new(20, 30), new(40, 50)];
        Assert.Same(samples[expected], VmMusicWildMidiPatchLookup.SelectSample(samples, frequency, s => s.Low, s => s.High));
    }
}
