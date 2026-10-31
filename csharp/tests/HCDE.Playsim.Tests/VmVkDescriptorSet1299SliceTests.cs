namespace HCDE.Playsim.Tests;

public class VmVkDescriptorSet1299SliceTests
{
    [Fact]
    public void HwSets_WaitBeforeTheEleventhAllocateAndReadRaytracingLive()
    {
        var host = new VmVkDescriptorSet1299.Host { Raytracing = true };
        var sets = new VmVkDescriptorSet1299(host);
        Assert.Equal(
            new[]
            {
                "VkDescriptorSetManager.HWBufferSetLayout",
                "VkDescriptorSetManager.FixedSetLayout",
                "VkDescriptorSetManager.HWBufferDescriptorPool",
                "VkDescriptorSetManager.FixedDescriptorPool",
            },
            sets.Created);
        Assert.Equal(VmVkDescriptorSet1299.TypeAccel, sets.FixedLayout.Bindings[2].Type);
        Assert.Equal(VmVkDescriptorSet1299.StageFragment, sets.FixedLayout.Bindings[2].Stages);
        Assert.Equal(VmVkDescriptorSet1299.TypeCombined, sets.FixedPool.Sizes[0].Type);
        Assert.Equal(20, sets.FixedPool.Sizes[0].Count);
        Assert.Equal(VmVkDescriptorSet1299.TypeAccel, sets.FixedPool.Sizes[1].Type);
        Assert.Equal(VmVkDescriptorSet1299.MaxSets, sets.FixedPool.Sizes[1].Count);
        Assert.Equal(30, sets.HwPool.Sizes[0].Count);
        Assert.Equal(20, sets.HwPool.Sizes[1].Count);
        Assert.Equal(VmVkDescriptorSet1299.StageVertex | VmVkDescriptorSet1299.StageFragment, sets.HwLayout.Bindings[0].Stages);
        Assert.Equal(VmVkDescriptorSet1299.StageFragment, sets.HwLayout.Bindings[3].Stages);
        Assert.Equal(VmVkDescriptorSet1299.StageVertex, sets.HwLayout.Bindings[4].Stages);
        Assert.Equal(65280, VmVkDescriptorSet1299.StreamBytes);
        Assert.Equal(170, VmVkDescriptorSet1299.MaxStreamData);

        host.Raytracing = false;
        host.Viewpoint.Size = 999;
        host.Matrix.Size = 1;
        host.Stream.Size = 2;
        host.Light.Size = 77;
        host.Bone.Size = 55;
        sets.BeginFrame();
        Assert.Equal(0, host.Waits);
        Assert.Equal(2, sets.DeviceExecutes);
        Assert.Equal(0, sets.DrawExecutes);
        Assert.Equal(7, sets.Writes.Count);
        Assert.Equal(VmVkDescriptorSet1299.TypeCombined, sets.Writes[0].Type);
        Assert.Equal("shadow-view", sets.Writes[0].Resource);
        Assert.Equal("shadow-sampler", sets.Writes[0].Sampler);
        Assert.Equal(VmVkDescriptorSet1299.LayoutShaderRead, sets.Writes[0].Layout);
        Assert.Equal("light-view", sets.Writes[1].Resource);
        Assert.DoesNotContain(sets.Writes, write => write.Type == VmVkDescriptorSet1299.TypeAccel);
        Assert.Equal(0, sets.Writes[2].Offset);
        Assert.Equal(VmVkDescriptorSet1299.ViewpointBytes, sets.Writes[2].Range);
        Assert.Equal(VmVkDescriptorSet1299.MatrixBytes, sets.Writes[3].Range);
        Assert.Equal(VmVkDescriptorSet1299.StreamBytes, sets.Writes[4].Range);
        Assert.Equal(77, sets.Writes[5].Range);
        Assert.Equal(55, sets.Writes[6].Range);
        Assert.Equal("LightBufferSSO", sets.Writes[5].Resource);
        Assert.Equal(VmVkDescriptorSet1299.TypeStorage, sets.Writes[5].Type);
        Assert.Equal(VmVkDescriptorSet1299.TypeUniformDynamic, sets.Writes[2].Type);
        Assert.True(sets.FixedSet!.Alive);
        Assert.Equal(1, sets.FixedPool.Allocated);

        var hw = new VmVkDescriptorSet1299(new VmVkDescriptorSet1299.Host());
        var hwIds = new List<int>();
        for (int i = 0; i < 10; i++)
            hwIds.Add(hw.UpdateHWBufferSet().Id);
        Assert.Equal(0, hw.Services.Waits);
        Assert.Equal(10, hw.HwPool.Allocated);
        Assert.Equal(9, hw.DeleteList.Count);
        VmVkDescriptorSet1299.Set eleventh = hw.UpdateHWBufferSet();
        Assert.Equal(1, hw.Services.Waits);
        Assert.False(hw.Services.LastWaitFinish);
        Assert.Equal(hwIds.Select(id => "set:" + id.ToString()), hw.Destroyed);
        Assert.DoesNotContain(eleventh.Id, hwIds);
        Assert.True(eleventh.Alive);
        Assert.Equal(1, hw.HwPool.Allocated);
        Assert.Empty(hw.DeleteList);

        var cold = new VmVkDescriptorSet1299.Host { Raytracing = false };
        var fixedSets = new VmVkDescriptorSet1299(cold);
        Assert.Equal(2, fixedSets.FixedLayout.Bindings.Count);
        Assert.Single(fixedSets.FixedPool.Sizes);
        cold.Raytracing = true;
        var fixedIds = new List<int>();
        for (int i = 0; i < 10; i++)
            fixedIds.Add(fixedSets.UpdateFixedSet().Id);
        Assert.Equal(0, cold.Waits);
        Assert.Contains(fixedSets.Writes, write => write.Set == fixedIds[0] && write.Type == VmVkDescriptorSet1299.TypeAccel && write.Resource == "tlas" && write.Binding == 2);
        VmVkDescriptorSet1299.Set fixedEleventh = fixedSets.UpdateFixedSet();
        Assert.Equal(1, cold.Waits);
        Assert.False(cold.LastWaitFinish);
        Assert.Equal(fixedIds.Select(id => "set:" + id.ToString()), fixedSets.Destroyed);
        Assert.Contains(fixedSets.Writes, write => write.Set == fixedEleventh.Id && write.Binding == 2);
        Assert.Equal(1, fixedSets.FixedPool.Allocated);
    }

    [Fact]
    public void Textures_RefillBelowTheDescriptorFloorAndPostprocessRetiresAFullPool()
    {
        var host = new VmVkDescriptorSet1299.Host();
        var sets = new VmVkDescriptorSet1299(host);
        VmVkDescriptorSet1299.Layout wide = sets.GetTextureSetLayout(4);
        VmVkDescriptorSet1299.Layout narrow = sets.GetTextureSetLayout(2);
        Assert.Equal(4, sets.TextureLayouts.Count);
        Assert.Null(sets.TextureLayouts[0]);
        Assert.Same(narrow, sets.TextureLayouts[1]);
        Assert.Null(sets.TextureLayouts[2]);
        Assert.Same(wide, sets.TextureLayouts[3]);
        Assert.Same(narrow, sets.GetTextureSetLayout(2));
        Assert.Equal(4, wide.Bindings.Count);
        Assert.Equal(2, narrow.Bindings.Count);
        Assert.Equal(VmVkDescriptorSet1299.StageFragment, narrow.Bindings[1].Stages);

        VmVkDescriptorSet1299.Set first = sets.AllocateTextureDescriptorSet(1);
        Assert.Equal(999, sets.SetsLeft);
        Assert.Equal(1999, sets.DescriptorsLeft);
        Assert.Equal(2000, sets.TexturePools[0].Sizes[0].Count);
        Assert.Equal(1000, sets.TexturePools[0].MaxSets);
        Assert.Equal("VkDescriptorSetManager.TextureDescriptorPool", sets.TexturePools[0].Name);
        Assert.Same(sets.TextureLayouts[0], first.Layout);
        Assert.Null(sets.TextureLayouts[2]);
        Assert.Single(first.Layout!.Bindings);

        sets.AllocateTextureDescriptorSet(1990);
        Assert.Equal(998, sets.SetsLeft);
        Assert.Equal(9, sets.DescriptorsLeft);
        Assert.Single(sets.TexturePools);
        sets.AllocateTextureDescriptorSet(11);
        Assert.Equal(2, sets.TexturePools.Count);
        Assert.Equal(999, sets.SetsLeft);
        Assert.Equal(1989, sets.DescriptorsLeft);
        Assert.True(sets.TexturePools[0].Alive);
        Assert.Empty(sets.DeleteList);

        VmVkDescriptorSet1299.Set nullSet = sets.GetNullTextureDescriptorSet();
        Assert.Equal(998, sets.SetsLeft);
        Assert.Equal(1978, sets.DescriptorsLeft);
        Assert.Equal(11, sets.Writes.Count(write => write.Set == nullSet.Id));
        Assert.Equal("null-view", sets.Writes.First(write => write.Set == nullSet.Id).Resource);
        Assert.Equal("clamp:4", sets.Writes.First(write => write.Set == nullSet.Id).Sampler);
        Assert.Equal(VmVkDescriptorSet1299.LayoutShaderRead, sets.Writes.First(write => write.Set == nullSet.Id).Layout);
        int writes = sets.Writes.Count;
        Assert.Same(nullSet, sets.GetNullTextureDescriptorSet());
        Assert.Equal(writes, sets.Writes.Count);
        Assert.Equal(1, sets.DeviceExecutes);

        var kept = new VmVkDescriptorSet1299.Material { Id = 7 };
        var dropped = new VmVkDescriptorSet1299.Material { Id = 8 };
        var keptSet = new VmVkDescriptorSet1299.Set { Id = 70 };
        var droppedSet = new VmVkDescriptorSet1299.Set { Id = 80 };
        kept.Sets.Add(keptSet);
        dropped.Sets.Add(droppedSet);
        sets.AddMaterial(kept);
        sets.AddMaterial(dropped);
        VmVkDescriptorSet1299.Pool[] retiredPools = sets.TexturePools.ToArray();
        sets.ResetHWTextureSets();
        Assert.Equal(new[] { 7, 8 }, sets.MaterialDeletes);
        Assert.Same(sets, kept.Fb);
        Assert.Equal(2, sets.Materials.Count);
        Assert.Empty(kept.Sets);
        Assert.Contains(keptSet, sets.DeleteList);
        Assert.Contains(droppedSet, sets.DeleteList);
        Assert.Contains(nullSet, sets.DeleteList);
        Assert.Equal(2, retiredPools.Length);
        Assert.All(retiredPools, pool => Assert.Contains(pool, sets.DeleteList));
        Assert.Equal(0, sets.SetsLeft);
        Assert.Equal(0, sets.DescriptorsLeft);
        Assert.Null(sets.NullSet);
        Assert.Empty(sets.TexturePools);

        VmVkDescriptorSet1299.Set huge = sets.AllocateTextureDescriptorSet(2001);
        Assert.Equal(999, sets.SetsLeft);
        Assert.Equal(-1, sets.DescriptorsLeft);
        Assert.Equal(2000, huge.Pool!.Sizes[0].Count);
        sets.AllocateTextureDescriptorSet(1);
        Assert.Equal(2, sets.TexturePools.Count);
        Assert.Equal(999, sets.SetsLeft);
        Assert.Equal(1999, sets.DescriptorsLeft);

        var owner = new VmVkDescriptorSet1299(new VmVkDescriptorSet1299.Host());
        var firstMat = new VmVkDescriptorSet1299.Material { Id = 1 };
        var middle = new VmVkDescriptorSet1299.Material { Id = 2 };
        var last = new VmVkDescriptorSet1299.Material { Id = 3 };
        var owned = new VmVkDescriptorSet1299.Set { Id = 501 };
        firstMat.Sets.Add(owned);
        owner.AddMaterial(firstMat);
        owner.AddMaterial(middle);
        owner.AddMaterial(last);
        owner.RemoveMaterial(middle);
        Assert.Equal(new[] { 2 }, owner.MaterialDeletes);
        Assert.Null(middle.Fb);
        Assert.Equal(1, middle.Deleted);
        Assert.Equal(new[] { firstMat, last }, owner.Materials);
        owner.Deinit();
        Assert.Equal(new[] { 2, 3, 1 }, owner.MaterialDeletes);
        Assert.Empty(owner.Materials);
        Assert.Null(firstMat.Fb);
        Assert.Contains(owned, owner.DeleteList);
        owner.Deinit();
        Assert.Equal(3, owner.MaterialDeletes.Count);
        var cleared = new VmVkDescriptorSet1299.Material { Id = 4 };
        var stranded = new VmVkDescriptorSet1299.Set { Id = 502 };
        cleared.Sets.Add(stranded);
        owner.AddMaterial(cleared);
        cleared.Fb = null;
        owner.Destroy();
        Assert.Equal(1, cleared.Deleted);
        Assert.Empty(owner.Materials);
        Assert.DoesNotContain(stranded, owner.DeleteList);

        var ppHost = new VmVkDescriptorSet1299.Host();
        ppHost.Textures[(3, 9)] = new VmVkDescriptorSet1299.Tex { Name = "depth-tex", View = "color", DepthView = "depth-only" };
        ppHost.Textures[(1, 4)] = new VmVkDescriptorSet1299.Tex { Name = "color-tex", View = "scene-color", DepthView = "" };
        var pp = new VmVkDescriptorSet1299(ppHost);
        var pass = new VmVkDescriptorSet1299.Layout { Id = 1, Name = "pass" };
        var inputs = new List<VmVkDescriptorSet1299.Input>
        {
            new(1, 0, 3, 9),
            new(0, 1, 1, 4),
        };
        VmVkDescriptorSet1299.Set input = pp.GetInput(pass, inputs, true);
        Assert.Equal("VkPostprocess.descriptors", input.DebugName);
        Assert.Same(pass, input.Layout);
        Assert.True(input.Alive);
        Assert.True(input.Queued);
        Assert.Equal("PPDescriptorPool", input.Pool!.Name);
        Assert.Equal(100, input.Pool.MaxSets);
        Assert.Equal(new[] { VmVkDescriptorSet1299.TypeCombined, VmVkDescriptorSet1299.TypeStorage, VmVkDescriptorSet1299.TypeUniform }, input.Pool.Sizes.Select(size => size.Type));
        Assert.Equal(new[] { 200, 4, 100 }, input.Pool.Sizes.Select(size => size.Count));
        var firstWrites = pp.Writes.Where(write => write.Set == input.Id).ToArray();
        Assert.Equal(6, firstWrites.Length);
        Assert.Equal("depth-only", firstWrites[0].Resource);
        Assert.Equal("1:0", firstWrites[0].Sampler);
        Assert.Equal("scene-color", firstWrites[1].Resource);
        Assert.Equal("0:1", firstWrites[1].Sampler);
        Assert.Equal(VmVkDescriptorSet1299.AutomaticUniforms, firstWrites[2].Binding);
        Assert.Equal(VmVkDescriptorSet1299.TypeUniform, firstWrites[2].Type);
        Assert.Equal(48, firstWrites[2].Range);
        Assert.Equal(VmVkDescriptorSet1299.LightNodes, firstWrites[3].Binding);
        Assert.Equal(16, firstWrites[3].Range);
        Assert.Equal(VmVkDescriptorSet1299.LightLines, firstWrites[4].Binding);
        Assert.Equal(24, firstWrites[4].Range);
        Assert.Equal(VmVkDescriptorSet1299.LightList, firstWrites[5].Binding);
        Assert.Equal(32, firstWrites[5].Range);
        Assert.Equal(new[] { "depth-tex", "color-tex" }, pp.Transitions.Select(item => item.Name));
        Assert.All(pp.Transitions, item => Assert.False(item.Undefined));
        Assert.Equal(VmVkDescriptorSet1299.LayoutShaderRead, pp.Transitions[0].Layout);
        int poolId = input.Pool.Id;
        for (int i = 0; i < 99; i++)
            pp.GetInput(pass, Array.Empty<VmVkDescriptorSet1299.Input>(), false);
        Assert.Equal(poolId, pp.PpPool!.Id);
        Assert.Equal(100, pp.PpPool.Allocated);
        Assert.Equal(100, pp.DrawExecutes);
        VmVkDescriptorSet1299.Set retired = pp.GetInput(pass, Array.Empty<VmVkDescriptorSet1299.Input>(), false);
        Assert.NotEqual(poolId, retired.Pool!.Id);
        Assert.Equal(1, retired.Pool.Allocated);
        Assert.Equal(1, pp.PpPool.Allocated);
        VmVkDescriptorSet1299.Pool oldPool = pp.DeleteList.OfType<VmVkDescriptorSet1299.Pool>().Single(pool => pool.Id == poolId);
        Assert.Equal(100, oldPool.Allocated);
        Assert.True(oldPool.Alive);
        Assert.Single(pp.Writes, write => write.Set == retired.Id);
        Assert.Equal(VmVkDescriptorSet1299.AutomaticUniforms, pp.Writes.Single(write => write.Set == retired.Id).Binding);
        Assert.Contains(pp.DeleteList, item => item is VmVkDescriptorSet1299.Pool old && old.Id == poolId && old.Alive);
        pp.WaitForCommands(false);
        Assert.False(pp.Services.LastWaitFinish);
        Assert.Equal("pool:" + poolId.ToString(), pp.Destroyed[100]);
        Assert.Equal("set:" + retired.Id.ToString(), pp.Destroyed[^1]);
        Assert.False(retired.Alive);
        Assert.Equal(0, retired.Pool.Allocated);
        Assert.True(retired.Pool.Alive);
        Assert.Equal(101, pp.DrawExecutes);
        Assert.Equal(101, pp.DeviceExecutes);
    }
}
