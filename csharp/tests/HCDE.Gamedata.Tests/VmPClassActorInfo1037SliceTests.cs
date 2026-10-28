namespace HCDE.Gamedata.Tests;

public class VmPClassActorInfo1037SliceTests
{
    [Fact]
    public void CopyParentDefaultsBlock_CopiesAndZeroExtends()
    {
        const int header = 4;
        var parent = new byte[] { 1, 2, 3, 4, 5, 6 };
        var defaults = new byte[8];
        VmPClassActorInfo1037Slice.CopyParentDefaultsBlock(defaults, parent, header, parent.Length);
        Assert.Equal(new byte[] { 0, 0, 0, 0, 5, 6, 0, 0 }, defaults);
    }

    [Fact]
    public void TransferrableObjectFlags_MasksToTransferBits()
    {
        var masked = VmPClassActorInfo1037Slice.TransferrableObjectFlags(0xffu);
        Assert.Equal(VmPClassActorInfo1037Slice.OfTransferrableFlags, masked);
    }

    [Fact]
    public void ShouldRegisterSpawnOrDoomEd_RespectsGameFilter()
    {
        Assert.True(VmPClassActorInfo1037Slice.ShouldRegisterSpawnOrDoomEd(
            VmPClassActorInfo1037Slice.GameFilterAny, currentGameType: 2));
        Assert.True(VmPClassActorInfo1037Slice.ShouldRegisterSpawnOrDoomEd(2, 2));
        Assert.False(VmPClassActorInfo1037Slice.ShouldRegisterSpawnOrDoomEd(4, 2));
    }

    [Fact]
    public void CreateDecorateDoomEdEntry_UsesSpecialMinusTwo()
    {
        var ent = VmPClassActorInfo1037Slice.CreateDecorateDoomEdEntry("Foo");
        Assert.Equal(-2, ent.Special);
    }

    [Fact]
    public void EvaluateSkillReplacement_FailsWhenClassMissing()
    {
        var result = VmPClassActorInfo1037Slice.EvaluateSkillReplacement(
            lookSkill: true,
            skillCount: 5,
            currentSkill: 1,
            typeName: "Baron",
            skillReplaceName: "Knight",
            replaceeExists: false,
            clientSideMatch: true);
        Assert.True(result.Failed);
    }

    [Fact]
    public void EvaluateSkillReplacement_FailsOnClientSideMismatch()
    {
        var result = VmPClassActorInfo1037Slice.EvaluateSkillReplacement(
            lookSkill: true,
            skillCount: 5,
            currentSkill: 1,
            typeName: "A",
            skillReplaceName: "B",
            replaceeExists: true,
            clientSideMatch: false);
        Assert.True(result.Failed);
    }

    [Fact]
    public void ApplySkillReplacementFailureClear_ClearsLookSkill()
    {
        var failed = new VmPClassActorInfo1037Slice.SkillReplacementFailure(
            true, false, "A", "B");
        var cleared = VmPClassActorInfo1037Slice.ApplySkillReplacementFailureClear(failed, "A", "B");
        Assert.True(cleared.ClearLookSkill);
    }

    [Fact]
    public void IsDuplicateDecorateDoomEdEntry_DetectsDecorateMarker()
    {
        var ent = VmPClassActorInfo1037Slice.CreateDecorateDoomEdEntry("X");
        Assert.True(VmPClassActorInfo1037Slice.IsDuplicateDecorateDoomEdEntry(ent));
    }

    [Fact]
    public void CopyMetaBlock_ZeroFillsExtension()
    {
        var parent = new byte[] { 9, 8 };
        var meta = new byte[4];
        VmPClassActorInfo1037Slice.CopyMetaBlock(meta, parent, parentMetaSize: 2, metaSize: 4);
        Assert.Equal(new byte[] { 9, 8, 0, 0 }, meta);
    }
}
