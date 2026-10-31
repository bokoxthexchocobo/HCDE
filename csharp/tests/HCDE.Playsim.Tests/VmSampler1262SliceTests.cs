namespace HCDE.Playsim.Tests;

public class VmSampler1262SliceTests
{
    const string BuildGolden =
"""
hw 0 1 VkSamplerManager.mSamplers 0 1 1 0 0 0 16.000000 100.000000
hw 1 2 VkSamplerManager.mSamplers 0 1 1 2 0 0 16.000000 100.000000
hw 2 3 VkSamplerManager.mSamplers 0 1 1 0 2 0 16.000000 100.000000
hw 3 4 VkSamplerManager.mSamplers 0 1 1 2 2 0 16.000000 100.000000
hw 4 5 VkSamplerManager.mSamplers 0 0 0 2 2 0 off 0.250000
hw 5 6 VkSamplerManager.mSamplers 0 0 0 0 0 0 off 0.250000
hw 6 7 VkSamplerManager.mSamplers 0 0 0 2 0 0 off 0.250000
hw 7 8 VkSamplerManager.mSamplers 0 0 0 0 2 0 off 0.250000
hw 8 9 VkSamplerManager.mSamplers 0 0 0 2 2 0 off 0.250000
hw 9 10 VkSamplerManager.mSamplers 0 0 0 0 0 0 off 0.250000
shadow 0 11 VkRenderBuffers.ShadowmapSampler 0 0 0 2 2 2 off -
light 0 12 VkRenderBuffers.LightmapSampler 1 1 1 2 2 2 off -
pp 0 13 VkPostprocess.mSamplers 0 0 0 2 2 2 off -
pp 1 14 VkPostprocess.mSamplers 0 0 0 0 0 0 off -
pp 2 15 VkPostprocess.mSamplers 1 1 0 2 2 2 off -
pp 3 16 VkPostprocess.mSamplers 1 1 0 0 0 0 off -

""";

    const string ResetGolden =
"""
hw 0 1 VkSamplerManager.mSamplers 0 1 1 0 0 0 16.000000 100.000000
hw 1 2 VkSamplerManager.mSamplers 0 1 1 2 0 0 16.000000 100.000000
hw 2 3 VkSamplerManager.mSamplers 0 1 1 0 2 0 16.000000 100.000000
hw 3 4 VkSamplerManager.mSamplers 0 1 1 2 2 0 16.000000 100.000000
hw 4 5 VkSamplerManager.mSamplers 0 0 0 2 2 0 off 0.250000
hw 5 6 VkSamplerManager.mSamplers 0 0 0 0 0 0 off 0.250000
hw 6 7 VkSamplerManager.mSamplers 0 0 0 2 0 0 off 0.250000
hw 7 8 VkSamplerManager.mSamplers 0 0 0 0 2 0 off 0.250000
hw 8 9 VkSamplerManager.mSamplers 0 0 0 2 2 0 off 0.250000
hw 9 10 VkSamplerManager.mSamplers 0 0 0 0 0 0 off 0.250000
shadow 0 11 VkRenderBuffers.ShadowmapSampler 0 0 0 2 2 2 off -
light 0 12 VkRenderBuffers.LightmapSampler 1 1 1 2 2 2 off -
pp 0 13 VkPostprocess.mSamplers 0 0 0 2 2 2 off -
drop 0 1
drop 1 2
drop 2 3
drop 3 4
drop 4 5
drop 5 6
drop 6 7
drop 7 8
drop 8 9
drop 9 10
hw 0 14 VkSamplerManager.mSamplers 0 1 1 0 0 0 16.000000 100.000000
hw 1 15 VkSamplerManager.mSamplers 0 1 1 2 0 0 16.000000 100.000000
hw 2 16 VkSamplerManager.mSamplers 0 1 1 0 2 0 16.000000 100.000000
hw 3 17 VkSamplerManager.mSamplers 0 1 1 2 2 0 16.000000 100.000000
hw 4 18 VkSamplerManager.mSamplers 0 0 0 2 2 0 off 0.250000
hw 5 19 VkSamplerManager.mSamplers 0 0 0 0 0 0 off 0.250000
hw 6 20 VkSamplerManager.mSamplers 0 0 0 2 0 0 off 0.250000
hw 7 21 VkSamplerManager.mSamplers 0 0 0 0 2 0 off 0.250000
hw 8 22 VkSamplerManager.mSamplers 0 0 0 2 2 0 off 0.250000
hw 9 23 VkSamplerManager.mSamplers 0 0 0 0 0 0 off 0.250000
dtor

""";

    [Fact]
    public void Build_UsesFilterRowSixAndCachesPostprocess()
    {
        var vm = VmSampler1262.Create(6, false, false, 16f);
        var cached = vm.GetPost(VmSampler1262.PpNearest, VmSampler1262.WrapClamp);
        Assert.Same(cached, vm.GetPost(VmSampler1262.PpNearest, VmSampler1262.WrapClamp));
        vm.GetPost(VmSampler1262.PpNearest, VmSampler1262.WrapRepeat);
        vm.GetPost(VmSampler1262.PpLinear, VmSampler1262.WrapClamp);
        vm.GetPost(VmSampler1262.PpLinear, VmSampler1262.WrapRepeat);

        Assert.Equal(0, vm.Hw[0]!.Mag);
        Assert.Equal(1, vm.Hw[0]!.Min);
        Assert.Equal(1, vm.Hw[0]!.Mip);
        Assert.True(vm.Hw[0]!.AnisoOn);
        Assert.Equal(16f, vm.Hw[0]!.Aniso);
        Assert.Equal(100f, vm.Hw[0]!.Lod);
        Assert.Equal(2, vm.Hw[1]!.U);
        Assert.Equal(0, vm.Hw[1]!.V);
        Assert.Equal(0, vm.Hw[4]!.Mag);
        Assert.Equal(0, vm.Hw[4]!.Min);
        Assert.False(vm.Hw[4]!.AnisoOn);
        Assert.Equal(0.25f, vm.Hw[4]!.Lod);
        Assert.Equal(0, vm.Hw[9]!.Mag);
        Assert.Equal(0, vm.Hw[9]!.W);
        Assert.False(vm.Shadow!.LodSet);
        Assert.Equal(2, vm.Shadow.W);
        Assert.Equal(1, vm.Light!.Mip);
        Assert.False(vm.Light.LodSet);
        Assert.Same(cached, vm.Post[0]);
        Assert.Equal(BuildGolden, Dump(vm));
    }

    [Fact]
    public void Reset_DropsHardwareSlotsAndKeepsShadow()
    {
        var vm = VmSampler1262.Create(6, false, false, 16f);
        var post = vm.GetPost(VmSampler1262.PpNearest, VmSampler1262.WrapClamp);
        int shadow = vm.Shadow!.Id;
        vm.Reset();
        vm.Destroy();
        Assert.Equal(shadow, vm.Shadow.Id);
        Assert.Same(post, vm.Post[0]);
        Assert.Equal(14, vm.Hw[0]!.Id);
        Assert.NotNull(vm.Hw[9]);
        Assert.Equal(ResetGolden, Dump(vm));

        var forced = VmSampler1262.Create(6, true, false, 16f);
        Assert.Equal(0, forced.Hw[0]!.Mag);
        Assert.Equal(0, forced.Hw[0]!.Min);
        Assert.False(forced.Hw[0]!.AnisoOn);
        Assert.Equal(0.25f, forced.Hw[0]!.Lod);
        Assert.Equal(0, forced.Hw[4]!.Min);
        Assert.Equal(0, forced.Hw[9]!.Min);

        var quiet = VmSampler1262.Create(4, false, true, 16f);
        Assert.Equal(1, quiet.Hw[0]!.Mag);
        Assert.Equal(1, quiet.Hw[0]!.Min);
        Assert.Equal(1, quiet.Hw[0]!.Mip);
        Assert.False(quiet.Hw[0]!.AnisoOn);
        Assert.Equal(0.25f, quiet.Hw[0]!.Lod);
        Assert.Equal(1, quiet.Hw[4]!.Mag);
        Assert.Equal(1, quiet.Hw[4]!.Min);
        Assert.Equal(0, quiet.Hw[4]!.Mip);
        Assert.Equal(1, quiet.Hw[9]!.Min);
        Assert.Equal(0, quiet.Hw[9]!.Mip);
    }

    static string Dump(VmSampler1262 vm) => string.Join('\n', vm.Log) + "\n";
}
