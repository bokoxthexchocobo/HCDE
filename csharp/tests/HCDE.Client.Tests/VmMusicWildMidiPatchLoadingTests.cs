namespace HCDE.Client.Tests;

public class VmMusicWildMidiPatchLoadingTests
{
    [Fact]
    public void ReleaseBalancesDuplicateFallbackReferencesAndFreesOnlyAtZero()
    {
        var patch = new Patch { Id = 1, Loaded = true, HasSamples = true, UseCount = 2 };
        var patches = new List<Patch> { patch, patch };
        var freed = 0;
        VmMusicWildMidiPatchLoading.Release(patches, input =>
        {
            Assert.Same(patch, input); Assert.Equal(0, input.UseCount);
            Assert.True(input.Loaded); input.HasSamples = false; freed++;
        });
        Assert.Empty(patches);
        Assert.False(patch.Loaded);
        Assert.False(patch.HasSamples);
        Assert.Equal(1, freed);
        VmMusicWildMidiPatchLoading.Release(patches, _ => Assert.Fail("unexpected second free"));
    }

    [Fact]
    public void SharedPatchRemainsLoadedUntilLastDeviceReleasesIt()
    {
        var patch = new Patch { Loaded = true, HasSamples = true, UseCount = 2 };
        VmMusicWildMidiPatchLoading.Release(new List<Patch> { patch }, _ => Assert.Fail("still shared"));
        Assert.Equal(1, patch.UseCount);
        Assert.True(patch.Loaded);
        VmMusicWildMidiPatchLoading.Release(new List<Patch> { patch }, input => input.HasSamples = false);
        Assert.Equal(0, patch.UseCount);
        Assert.False(patch.Loaded);
    }

    [Fact]
    public void FailedSampleCleanupPropagatesWithoutRepeatingReferenceDecrement()
    {
        var patch = new Patch { Loaded = true, UseCount = 1 };
        var pending = new Patch { Loaded = true, UseCount = 1 };
        var patches = new List<Patch> { patch, pending };
        var error = new IOException("sample cleanup failed");
        Assert.Same(error, Assert.Throws<IOException>(() =>
            VmMusicWildMidiPatchLoading.Release(patches, _ => throw error)));
        Assert.Equal(0, patch.UseCount);
        Assert.True(patch.Loaded);
        Assert.Same(pending, Assert.Single(patches));
        VmMusicWildMidiPatchLoading.Release(patches, _ => { });
        Assert.Equal(0, patch.UseCount);
        Assert.Equal(0, pending.UseCount);
    }

    private sealed class Patch : IVmMusicWildMidiPatch
    {
        public ushort Id { get; init; }
        public bool Loaded { get; set; }
        public bool HasSamples { get; set; }
        public int UseCount { get; set; }
    }

    [Fact]
    public void SuccessfulSampleLoadAddsPatchAndDuplicateDoesNotReloadOrIncrement()
    {
        var patch = new Patch { Id = 1 };
        var patches = new List<Patch>();
        Assert.Equal(VmMusicWildMidiPatchLoadResult.Added, VmMusicWildMidiPatchLoading.Load(1, patches, _ => patch,
            input => { Assert.Same(patch, input); input.Loaded = true; input.HasSamples = true; return 0; }));
        Assert.Equal(1, patch.UseCount);
        Assert.Equal(VmMusicWildMidiPatchLoadResult.AlreadyPresent, VmMusicWildMidiPatchLoading.Load<Patch>(1, patches,
            _ => throw new Exception(), _ => throw new Exception()));
        Assert.Equal(1, patch.UseCount);
        Assert.Same(patch, Assert.Single(patches));
    }

    [Theory]
    [InlineData(-1, true, VmMusicWildMidiPatchLoadResult.SampleLoadFailed)]
    [InlineData(0, false, VmMusicWildMidiPatchLoadResult.NoSamples)]
    [InlineData(-2, true, VmMusicWildMidiPatchLoadResult.Added)]
    public void OnlyMinusOneSignalsSampleLoadFailure(int result, bool hasSamples, VmMusicWildMidiPatchLoadResult expected)
    {
        var patch = new Patch { HasSamples = hasSamples };
        var patches = new List<Patch>();
        Assert.Equal(expected, VmMusicWildMidiPatchLoading.Load(0, patches, _ => patch, _ => result));
        Assert.Equal(expected == VmMusicWildMidiPatchLoadResult.Added ? 1 : 0, patches.Count);
        Assert.Equal(patches.Count, patch.UseCount);
    }

    [Fact]
    public void BankFallbackMayAddSamePatchAgainBecauseNativeDuplicateCheckUsesRequestedId()
    {
        var patch = new Patch { Id = 1, Loaded = true, HasSamples = true };
        var patches = new List<Patch>();
        for (var i = 0; i < 2; i++)
            Assert.Equal(VmMusicWildMidiPatchLoadResult.Added, VmMusicWildMidiPatchLoading.Load(0x101, patches,
                id => id == 1 ? patch : null, _ => throw new Exception()));
        Assert.Equal(2, patch.UseCount);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void MissingPatchAndThrowingLoaderLeaveDeviceListAndUseCountUnchanged()
    {
        var patches = new List<Patch>();
        Assert.Equal(VmMusicWildMidiPatchLoadResult.Missing,
            VmMusicWildMidiPatchLoading.Load<Patch>(0, patches, _ => null, _ => throw new Exception()));
        var patch = new Patch();
        var error = new IOException("sample failure");
        Assert.Same(error, Assert.Throws<IOException>(() =>
            VmMusicWildMidiPatchLoading.Load(0, patches, _ => patch, _ => throw error)));
        Assert.Empty(patches);
        Assert.Equal(0, patch.UseCount);
    }
}
