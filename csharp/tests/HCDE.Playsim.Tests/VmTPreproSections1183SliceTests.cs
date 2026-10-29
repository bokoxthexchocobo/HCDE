namespace HCDE.Playsim.Tests;

public class VmTPreproSections1183SliceTests
{
    [Fact]
    public void RegistrationAndSerialize_RecordThePointerAndRunSuperFirst()
    {
        Assert.Equal("DFsSection", VmTPreproSections1183Slice.ClassName);
        Assert.Equal("DObject", VmTPreproSections1183Slice.ParentClassName);
        Assert.False(VmTPreproSections1183Slice.IsAbstract);
        Assert.True(VmTPreproSections1183Slice.HasPointers);
        Assert.Equal(new[] { "next" }, VmTPreproSections1183Slice.PointerFields);
        Assert.Equal(17, VmTPreproSections1183Slice.SectionSlots);

        var next = new VmTPreproSections1183Slice.Section();
        var section = new VmTPreproSections1183Slice.Section
        {
            Type = 1,
            StartIndex = 4,
            EndIndex = 9,
            LoopIndex = 6,
            Next = next,
        };
        var sawSuper = false;
        var archive = VmTPreproSections1183Slice.Serialize(section, () =>
        {
            sawSuper = true;
            section.Type = 4;
        });
        Assert.True(sawSuper);
        Assert.Equal(4, archive.Type);
        Assert.Equal(4, archive.StartIndex);
        Assert.Equal(9, archive.EndIndex);
        Assert.Equal(6, archive.LoopIndex);
        Assert.Same(next, archive.Next);
        Assert.Equal(4, VmTPreproSections1183Slice.SectionStart(section));
        Assert.Equal(9, VmTPreproSections1183Slice.SectionEnd(section));
        Assert.Equal(6, VmTPreproSections1183Slice.SectionLoop(section));
    }

    [Fact]
    public void NewSection_PushesTheFrontOfTheHashChain()
    {
        var script = new VmTPreproSections1183Slice.Script();
        var first = VmTPreproSections1183Slice.NewSection(script, 0);
        var second = VmTPreproSections1183Slice.NewSection(script, 17);
        Assert.Equal(0, first.StartIndex);
        Assert.Equal(17, second.StartIndex);
        Assert.Same(second, script.Sections[0]);
        Assert.Same(first, second.Next);
        Assert.Equal(2, script.WriteBarrierCount);
        Assert.Same(first, VmTPreproSections1183Slice.FindSectionStart(script, 0));
        Assert.Same(second, VmTPreproSections1183Slice.FindSectionStart(script, 17));
        Assert.Null(VmTPreproSections1183Slice.FindSectionStart(script, 1));

        first.EndIndex = 40;
        second.EndIndex = 41;
        var other = VmTPreproSections1183Slice.NewSection(script, 3);
        other.EndIndex = 99;
        Assert.Same(first, VmTPreproSections1183Slice.FindSectionEnd(script, 40));
        Assert.Same(second, VmTPreproSections1183Slice.FindSectionEnd(script, 41));
        Assert.Same(other, VmTPreproSections1183Slice.FindSectionEnd(script, 99));
        Assert.Null(VmTPreproSections1183Slice.FindSectionEnd(script, 7));
    }

    [Fact]
    public void ClearSections_DestroysTheSavedLinkBeforeNullingTheSlot()
    {
        var script = new VmTPreproSections1183Slice.Script();
        var older = VmTPreproSections1183Slice.NewSection(script, 0);
        var newer = VmTPreproSections1183Slice.NewSection(script, 17);
        VmTPreproSections1183Slice.ClearSections(script);
        Assert.True(newer.Destroyed);
        Assert.True(older.Destroyed);
        Assert.Null(newer.Next);
        Assert.All(script.Sections, Assert.Null);
        Assert.Null(VmTPreproSections1183Slice.FindSectionStart(script, 0));
    }
}
