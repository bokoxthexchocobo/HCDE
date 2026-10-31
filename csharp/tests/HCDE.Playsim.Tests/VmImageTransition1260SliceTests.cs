namespace HCDE.Playsim.Tests;

public class VmImageTransition1260SliceTests
{
    const string AddGolden =
"""
same 1
barrier 1 5 6 20 800 3 2 3 0 1
layout 1 6
barrier 1 0 2 800 180 3 0 1 0 1
layout 1 2
same 1
barrier 2 0 1 0 0 1 0 1 0 1
layout 2 1
same 3
barrier 4 2 3 180 600 1 0 1 0 1
layout 4 3
barrier 8 7 5 1000 20 5 4 2 0 1
layout 8 5
barrier 9 5 7 20 1000 1 0 1 0 1
layout 9 7
execute 1481 1781 6

""";

    const string WideGolden =
"""
barrier 11 5 6 1000 800 1 0 1 0 1
barrier 11 0 7 1000 800 1 1 1 0 1
execute 1000 1000 2
blit 11 6 7 0 0 0 4 2 1 0 0 0 0 2 1 1 1 1 0 1 1
barrier 11 6 5 800 20 1 0 1 0 1
execute 1000 80 1
barrier 11 7 6 1000 800 1 1 1 0 1
barrier 11 0 7 1000 800 1 2 1 0 1
execute 1000 1000 2
blit 11 6 7 0 0 0 2 1 1 1 0 0 0 1 1 1 2 1 0 1 1
barrier 11 6 5 800 20 1 1 1 0 1
execute 1000 80 1
barrier 11 7 5 800 20 1 2 1 0 1
execute 1000 80 1

""";

    const string OddGolden =
"""
barrier 13 6 6 1000 800 1 0 1 0 1
barrier 13 0 7 1000 800 1 1 1 0 1
execute 1000 1000 2
blit 13 6 7 0 0 0 3 5 1 0 0 0 0 1 2 1 1 1 0 1 1
barrier 13 6 5 800 20 1 0 1 0 1
execute 1000 80 1
barrier 13 7 6 1000 800 1 1 1 0 1
barrier 13 0 7 1000 800 1 2 1 0 1
execute 1000 1000 2
blit 13 6 7 0 0 0 1 2 1 1 0 0 0 1 1 1 2 1 0 1 1
barrier 13 6 5 800 20 1 1 1 0 1
execute 1000 80 1
barrier 13 7 5 800 20 1 2 1 0 1
execute 1000 80 1

""";

    const string OneGolden =
"""
barrier 12 7 5 800 20 1 0 1 0 1
execute 1000 80 1

""";

    [Fact]
    public void Add_AccumulatesStageMasksAndSkipsAMatchingLayout()
    {
        var vm = new VmImageTransition1260();
        var color = new VmImageTransition1260.Texture
        {
            Id = 1,
            Layout = VmImageTransition1260.LayoutShaderRead,
            Aspect = 3,
        };
        vm.Add(color, VmImageTransition1260.LayoutShaderRead, true);
        Assert.Equal(0, vm.SrcStage);
        Assert.Equal(0, vm.DstStage);
        Assert.False(vm.NeedBarrier);
        vm.Execute();

        vm.Add(color, VmImageTransition1260.LayoutTransferSrc, false, 2, 3);
        vm.Add(color, VmImageTransition1260.LayoutColor, true);
        vm.Add(color, VmImageTransition1260.LayoutColor, true);
        var general = new VmImageTransition1260.Texture
        {
            Id = 2,
            Layout = VmImageTransition1260.LayoutUndefined,
            Aspect = 1,
        };
        vm.Add(general, VmImageTransition1260.LayoutGeneral, false);
        var depth = new VmImageTransition1260.Texture
        {
            Id = 3,
            Layout = VmImageTransition1260.LayoutDepth,
        };
        vm.Add(depth, VmImageTransition1260.LayoutDepth, false);
        var attachment = new VmImageTransition1260.Texture
        {
            Id = 4,
            Layout = VmImageTransition1260.LayoutColor,
        };
        vm.Add(attachment, VmImageTransition1260.LayoutDepth, false);
        var sampled = new VmImageTransition1260.Texture
        {
            Id = 8,
            Layout = VmImageTransition1260.LayoutTransferDst,
            Aspect = 5,
        };
        vm.Add(sampled, VmImageTransition1260.LayoutShaderRead, false, 4, 2);
        var upload = new VmImageTransition1260.Texture
        {
            Id = 9,
            Layout = VmImageTransition1260.LayoutShaderRead,
        };
        vm.Add(upload, VmImageTransition1260.LayoutTransferDst, false);
        vm.Execute();

        Assert.Equal(VmImageTransition1260.LayoutColor, color.Layout);
        Assert.Equal(3, color.Aspect);
        Assert.Equal(VmImageTransition1260.LayoutGeneral, general.Layout);
        Assert.Equal(VmImageTransition1260.LayoutDepth, depth.Layout);
        Assert.Equal(VmImageTransition1260.LayoutDepth, attachment.Layout);
        Assert.Equal(VmImageTransition1260.LayoutShaderRead, sampled.Layout);
        Assert.Equal(VmImageTransition1260.LayoutTransferDst, upload.Layout);
        Assert.Equal(0x1481, vm.SrcStage);
        Assert.Equal(0x1781, vm.DstStage);
        Assert.Equal("", vm.Fatal);
        Assert.Equal(AddGolden, Dump(vm));

        var missingSrc = new VmImageTransition1260();
        var held = new VmImageTransition1260.Texture { Id = 5, Layout = 8 };
        missingSrc.Add(held, VmImageTransition1260.LayoutTransferSrc, false);
        missingSrc.Execute();
        Assert.Equal("Unimplemented src image layout transition", missingSrc.Fatal);
        Assert.Equal(8, held.Layout);
        Assert.Equal(0, missingSrc.SrcStage);
        Assert.Equal(0, missingSrc.DstStage);
        Assert.False(missingSrc.NeedBarrier);
        Assert.Equal("fatal src\n", Dump(missingSrc));

        var missingDst = new VmImageTransition1260();
        var source = new VmImageTransition1260.Texture
        {
            Id = 6,
            Layout = VmImageTransition1260.LayoutTransferSrc,
        };
        missingDst.Add(source, 8, true);
        missingDst.Execute();
        Assert.Equal("Unimplemented dst image layout transition", missingDst.Fatal);
        Assert.Equal(VmImageTransition1260.LayoutTransferSrc, source.Layout);
        Assert.Equal(VmImageTransition1260.StageTransfer, missingDst.SrcStage);
        Assert.Equal(0, missingDst.DstStage);
        Assert.False(missingDst.NeedBarrier);
        Assert.Equal("fatal dst\n", Dump(missingDst));
    }

    [Fact]
    public void Mipmaps_HalvesUntilOneAndLeavesTheLastLevelReadable()
    {
        var wide = new VmImageTransition1260();
        var image = new VmImageTransition1260.Texture
        {
            Id = 11,
            Width = 4,
            Height = 2,
            Layout = VmImageTransition1260.LayoutShaderRead,
            Aspect = 9,
        };
        wide.GenerateMipmaps(image);
        Assert.Equal(4, image.Width);
        Assert.Equal(2, image.Height);
        Assert.Equal(9, image.Aspect);
        Assert.Equal(VmImageTransition1260.LayoutShaderRead, image.Layout);
        Assert.Equal(0, wide.SrcStage);
        Assert.Equal(0, wide.DstStage);
        Assert.False(wide.NeedBarrier);
        Assert.Equal(WideGolden, Dump(wide));

        var odd = new VmImageTransition1260();
        var tall = new VmImageTransition1260.Texture
        {
            Id = 13,
            Width = 3,
            Height = 5,
            Layout = VmImageTransition1260.LayoutTransferSrc,
            Aspect = 9,
        };
        odd.GenerateMipmaps(tall);
        Assert.Equal(3, tall.Width);
        Assert.Equal(5, tall.Height);
        Assert.Equal(VmImageTransition1260.LayoutShaderRead, tall.Layout);
        Assert.Equal(OddGolden, Dump(odd));

        var single = new VmImageTransition1260();
        var one = new VmImageTransition1260.Texture
        {
            Id = 12,
            Width = 1,
            Height = 1,
            Layout = VmImageTransition1260.LayoutUndefined,
        };
        single.GenerateMipmaps(one);
        Assert.Equal(VmImageTransition1260.LayoutShaderRead, one.Layout);
        Assert.Equal(OneGolden, Dump(single));
    }

    static string Dump(VmImageTransition1260 vm) => string.Join('\n', vm.Log) + "\n";
}
