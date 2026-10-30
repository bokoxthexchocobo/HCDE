namespace HCDE.Playsim;

/// <summary>
/// Descriptor pools, the null texture set, and the postprocess input from
/// <c>vk_descriptorset.cpp</c> (1-297). Chunk 1299. A failed try-allocate
/// waits with finish false and reclaims the draw delete list first.
/// </summary>
public sealed class VmVkDescriptorSet1299
{
    public const int MaxSets = 10, MinTextureLayers = 11, ClampXyNoMip = 4;
    public const int LayoutShaderRead = 5, TypeCombined = 1, TypeUniform = 6;
    public const int TypeStorage = 7, TypeUniformDynamic = 8, TypeAccel = 1000150000;
    public const int StageVertex = 0x1, StageFragment = 0x10, AutomaticUniforms = 15;
    public const int LightNodes = 4, LightLines = 5, LightList = 6;
    public const int ViewpointBytes = 260, MatrixBytes = 192, StreamDataBytes = 384;
    public const int MaxStreamData = 65536 / StreamDataBytes, StreamBytes = MaxStreamData * StreamDataBytes, TextureSetsPerPool = 1000;
    public const int TextureDescriptorsPerPool = 2000, PpImageSamplers = 200, PpStorageBuffers = 4;
    public const int PpUniformBuffers = 100, PpMaxSets = 100;
    public sealed class Buffer
    {
        public string Name;
        public int Size;
        public Buffer(string name, int size)
        {
            Name = name;
            Size = size;
        }
    }
    public sealed class Host
    {
        public bool Raytracing;
        public int Waits;
        public bool LastWaitFinish = true;
        public Buffer Viewpoint = new("ViewpointUBO", ViewpointBytes);
        public Buffer Matrix = new("MatrixBuffer", MatrixBytes);
        public Buffer Stream = new("StreamBuffer", StreamBytes);
        public Buffer Light = new("LightBufferSSO", 128);
        public Buffer Bone = new("BoneBufferSSO", 64);
        public Buffer Nodes = new("LightNodes", 16);
        public Buffer Lines = new("LightLines", 24);
        public Buffer List = new("LightList", 32);
        public Buffer Automatic = new("AutomaticUniforms", 48);
        public string NullView = "null-view";
        public string ShadowView = "shadow-view";
        public string LightView = "light-view";
        public string ShadowSampler = "shadow-sampler";
        public string LightSampler = "light-sampler";
        public string Accel = "tlas";
        public Dictionary<(int Type, int Id), Tex> Textures = new();
        public string Sampler(int filter, int wrap)
        {
            return filter.ToString() + ":" + wrap.ToString();
        }
    }
    public sealed class Tex
    {
        public string Name = "";
        public string View = "";
        public string DepthView = "";
    }
    public readonly record struct Binding(int Index, int Type, int Count, int Stages);
    public readonly record struct PoolSize(int Type, int Count);
    public readonly record struct Write(int Set, int Binding, int Type, string Resource, string Sampler, int Offset, int Range, int Layout);
    public readonly record struct Transition(string Name, int Layout, bool Undefined);
    public readonly record struct Input(int Filter, int Wrap, int Type, int Texture);
    public sealed class Layout
    {
        public int Id;
        public string Name = "";
        public List<Binding> Bindings = new();
    }
    public sealed class Set
    {
        public int Id;
        public Layout? Layout;
        public Pool? Pool;
        public string DebugName = "";
        public bool Alive = true;
        public bool Queued;
    }
    public sealed class Pool
    {
        public int Id;
        public string Name = "";
        public int MaxSets;
        public List<PoolSize> Sizes = new();
        public int Allocated;
        public bool Alive = true;
        public bool Queued;
        public Set? TryAllocate(Layout layout, Func<Set> make)
        {
            if (!Alive || Allocated >= MaxSets)
                return null;
            Allocated++;
            Set set = make();
            set.Layout = layout;
            set.Pool = this;
            return set;
        }
    }
    public sealed class Material
    {
        public int Id;
        public VmVkDescriptorSet1299? Fb;
        public List<Set> Sets = new();
        public int Deleted;
        public void DeleteDescriptors()
        {
            Deleted++;
            if (Fb == null)
                return;
            Fb.MaterialDeletes.Add(Id);
            foreach (Set set in Sets)
                Fb.RetireSet(set);
            Sets.Clear();
        }
    }
    public Host Services;
    public Layout HwLayout = new();
    public Layout FixedLayout = new();
    public Pool HwPool = new();
    public Pool FixedPool = new();
    public Pool? PpPool;
    public Set? HwSet;
    public Set? FixedSet;
    public Set? NullSet;
    public int SetsLeft;
    public int DescriptorsLeft;
    public int DeviceExecutes;
    public int DrawExecutes;
    public int NextSetId = 1;
    public int NextLayoutId = 1;
    public int NextPoolId = 1;
    public List<Layout?> TextureLayouts = new();
    public List<Pool> TexturePools = new();
    public List<Material> Materials = new();
    public List<object> DeleteList = new();
    public List<string> Destroyed = new();
    public List<string> Created = new();
    public List<int> MaterialDeletes = new();
    public List<Write> Writes = new();
    public List<Transition> Transitions = new();
    public VmVkDescriptorSet1299(Host host)
    {
        Services = host;
        CreateHwLayout();
        CreateFixedLayout();
        CreateHwPool();
        CreateFixedPool();
    }
    public void Init()
    {
        UpdateFixedSet();
        UpdateHWBufferSet();
    }
    public void Deinit()
    {
        while (Materials.Count > 0)
            RemoveMaterial(Materials[Materials.Count - 1]);
    }
    public void Destroy()
    {
        while (Materials.Count > 0)
            RemoveMaterial(Materials[Materials.Count - 1]);
    }
    public void BeginFrame()
    {
        UpdateFixedSet();
        UpdateHWBufferSet();
    }
    public Set UpdateHWBufferSet()
    {
        RetireSet(HwSet);
        HwSet = null;
        Set? allocated = HwPool.TryAllocate(HwLayout, NewSet);
        if (allocated == null)
        {
            WaitForCommands(false);
            allocated = HwPool.TryAllocate(HwLayout, NewSet);
            if (allocated == null)
                throw new InvalidOperationException("HW buffer descriptor pool is empty after the wait.");
        }
        HwSet = allocated;
        WriteBuffer(HwSet, 0, TypeUniformDynamic, Services.Viewpoint, 0, ViewpointBytes);
        WriteBuffer(HwSet, 1, TypeUniformDynamic, Services.Matrix, 0, MatrixBytes);
        WriteBuffer(HwSet, 2, TypeUniformDynamic, Services.Stream, 0, StreamBytes);
        WriteBuffer(HwSet, 3, TypeStorage, Services.Light, 0, Services.Light.Size);
        WriteBuffer(HwSet, 4, TypeStorage, Services.Bone, 0, Services.Bone.Size);
        DeviceExecutes++;
        return HwSet;
    }
    public Set UpdateFixedSet()
    {
        RetireSet(FixedSet);
        FixedSet = null;
        Set? allocated = FixedPool.TryAllocate(FixedLayout, NewSet);
        if (allocated == null)
        {
            WaitForCommands(false);
            allocated = FixedPool.TryAllocate(FixedLayout, NewSet);
            if (allocated == null)
                throw new InvalidOperationException("Fixed descriptor pool is empty after the wait.");
        }
        FixedSet = allocated;
        WriteImage(FixedSet, 0, Services.ShadowView, Services.ShadowSampler, LayoutShaderRead);
        WriteImage(FixedSet, 1, Services.LightView, Services.LightSampler, LayoutShaderRead);
        if (Services.Raytracing)
            WriteAccel(FixedSet, 2, Services.Accel);
        DeviceExecutes++;
        return FixedSet;
    }
    public void ResetHWTextureSets()
    {
        foreach (Material mat in Materials)
            mat.DeleteDescriptors();
        foreach (Pool pool in TexturePools)
            RetirePool(pool);
        RetireSet(NullSet);
        NullSet = null;
        TexturePools.Clear();
        SetsLeft = 0;
        DescriptorsLeft = 0;
    }
    public Set GetNullTextureDescriptorSet()
    {
        if (NullSet == null)
        {
            NullSet = AllocateTextureDescriptorSet(MinTextureLayers);
            for (int i = 0; i < MinTextureLayers; i++)
                WriteImage(NullSet, i, Services.NullView, "clamp:" + ClampXyNoMip.ToString(), LayoutShaderRead);
            DeviceExecutes++;
        }
        return NullSet;
    }
    public Set AllocateTextureDescriptorSet(int numLayers)
    {
        if (SetsLeft == 0 || DescriptorsLeft < numLayers)
        {
            SetsLeft = TextureSetsPerPool;
            DescriptorsLeft = TextureDescriptorsPerPool;
            Pool pool = NewPool("VkDescriptorSetManager.TextureDescriptorPool", TextureSetsPerPool);
            pool.Sizes.Add(new PoolSize(TypeCombined, DescriptorsLeft));
            TexturePools.Add(pool);
        }
        SetsLeft--;
        DescriptorsLeft -= numLayers;
        Set? allocated = TexturePools[TexturePools.Count - 1].TryAllocate(GetTextureSetLayout(numLayers), NewSet);
        if (allocated == null)
            throw new InvalidOperationException("texture descriptor pool refused an allocate.");
        return allocated;
    }
    public Layout GetTextureSetLayout(int numLayers)
    {
        while (TextureLayouts.Count < numLayers)
            TextureLayouts.Add(null);
        Layout? layout = TextureLayouts[numLayers - 1];
        if (layout != null)
            return layout;
        layout = NewLayout("VkDescriptorSetManager.TextureSetLayout");
        for (int i = 0; i < numLayers; i++)
            layout.Bindings.Add(new Binding(i, TypeCombined, 1, StageFragment));
        TextureLayouts[numLayers - 1] = layout;
        return layout;
    }
    public void AddMaterial(Material texture)
    {
        texture.Fb = this;
        Materials.Add(texture);
    }
    public void RemoveMaterial(Material texture)
    {
        texture.DeleteDescriptors();
        texture.Fb = null;
        Materials.Remove(texture);
    }
    public Set GetInput(Layout passLayout, IReadOnlyList<Input> textures, bool bindShadowMapBuffers)
    {
        Set descriptors = AllocatePp(passLayout);
        descriptors.DebugName = "VkPostprocess.descriptors";
        for (int index = 0; index < textures.Count; index++)
        {
            Input input = textures[index];
            Tex tex = Services.Textures[(input.Type, input.Texture)];
            string view = tex.DepthView.Length > 0 ? tex.DepthView : tex.View;
            WriteImage(descriptors, index, view, Services.Sampler(input.Filter, input.Wrap), LayoutShaderRead);
            Transitions.Add(new Transition(tex.Name, LayoutShaderRead, false));
        }
        WriteBuffer(descriptors, AutomaticUniforms, TypeUniform, Services.Automatic, 0, Services.Automatic.Size);
        if (bindShadowMapBuffers)
        {
            WriteBuffer(descriptors, LightNodes, TypeStorage, Services.Nodes, 0, Services.Nodes.Size);
            WriteBuffer(descriptors, LightLines, TypeStorage, Services.Lines, 0, Services.Lines.Size);
            WriteBuffer(descriptors, LightList, TypeStorage, Services.List, 0, Services.List.Size);
        }
        DeviceExecutes++;
        DrawExecutes++;
        RetireSet(descriptors);
        return descriptors;
    }
    public void WaitForCommands(bool finish)
    {
        Services.Waits++;
        Services.LastWaitFinish = finish;
        foreach (object item in DeleteList)
        {
            if (item is Set set && set.Alive)
            {
                set.Alive = false;
                if (set.Pool != null && set.Pool.Alive)
                    set.Pool.Allocated--;
                Destroyed.Add("set:" + set.Id.ToString());
            }
            else if (item is Pool pool && pool.Alive)
            {
                pool.Alive = false;
                Destroyed.Add("pool:" + pool.Id.ToString());
            }
        }
        DeleteList.Clear();
    }
    Set AllocatePp(Layout layout)
    {
        if (PpPool != null)
        {
            Set? existing = PpPool.TryAllocate(layout, NewSet);
            if (existing != null)
                return existing;
            RetirePool(PpPool);
            PpPool = null;
        }
        PpPool = NewPool("PPDescriptorPool", PpMaxSets);
        PpPool.Sizes.Add(new PoolSize(TypeCombined, PpImageSamplers));
        PpPool.Sizes.Add(new PoolSize(TypeStorage, PpStorageBuffers));
        PpPool.Sizes.Add(new PoolSize(TypeUniform, PpUniformBuffers));
        Set? created = PpPool.TryAllocate(layout, NewSet);
        if (created == null)
            throw new InvalidOperationException("postprocess descriptor pool refused an allocate.");
        return created;
    }
    void CreateHwLayout()
    {
        Layout layout = NewLayout("VkDescriptorSetManager.HWBufferSetLayout");
        int both = StageVertex | StageFragment;
        layout.Bindings.Add(new Binding(0, TypeUniformDynamic, 1, both));
        layout.Bindings.Add(new Binding(1, TypeUniformDynamic, 1, both));
        layout.Bindings.Add(new Binding(2, TypeUniformDynamic, 1, both));
        layout.Bindings.Add(new Binding(3, TypeStorage, 1, StageFragment));
        layout.Bindings.Add(new Binding(4, TypeStorage, 1, StageVertex));
        HwLayout = layout;
    }
    void CreateFixedLayout()
    {
        Layout layout = NewLayout("VkDescriptorSetManager.FixedSetLayout");
        layout.Bindings.Add(new Binding(0, TypeCombined, 1, StageFragment));
        layout.Bindings.Add(new Binding(1, TypeCombined, 1, StageFragment));
        if (Services.Raytracing)
            layout.Bindings.Add(new Binding(2, TypeAccel, 1, StageFragment));
        FixedLayout = layout;
    }
    void CreateHwPool()
    {
        Pool pool = NewPool("VkDescriptorSetManager.HWBufferDescriptorPool", MaxSets);
        pool.Sizes.Add(new PoolSize(TypeUniformDynamic, 3 * MaxSets));
        pool.Sizes.Add(new PoolSize(TypeStorage, 2 * MaxSets));
        HwPool = pool;
    }
    void CreateFixedPool()
    {
        Pool pool = NewPool("VkDescriptorSetManager.FixedDescriptorPool", MaxSets);
        pool.Sizes.Add(new PoolSize(TypeCombined, 2 * MaxSets));
        if (Services.Raytracing)
            pool.Sizes.Add(new PoolSize(TypeAccel, MaxSets));
        FixedPool = pool;
    }
    Set NewSet()
    {
        return new Set { Id = NextSetId++ };
    }
    Layout NewLayout(string name)
    {
        Created.Add(name);
        return new Layout { Id = NextLayoutId++, Name = name };
    }
    Pool NewPool(string name, int maxSets)
    {
        Created.Add(name);
        return new Pool { Id = NextPoolId++, Name = name, MaxSets = maxSets };
    }
    void RetireSet(Set? set)
    {
        if (set == null || !set.Alive || set.Queued)
            return;
        set.Queued = true;
        DeleteList.Add(set);
    }
    void RetirePool(Pool? pool)
    {
        if (pool == null || !pool.Alive || pool.Queued)
            return;
        pool.Queued = true;
        DeleteList.Add(pool);
    }
    void WriteImage(Set set, int binding, string resource, string sampler, int layout)
    {
        Writes.Add(new Write(set.Id, binding, TypeCombined, resource, sampler, 0, 0, layout));
    }
    void WriteBuffer(Set set, int binding, int type, Buffer buffer, int offset, int range)
    {
        Writes.Add(new Write(set.Id, binding, type, buffer.Name, "", offset, range, 0));
    }
    void WriteAccel(Set set, int binding, string resource)
    {
        Writes.Add(new Write(set.Id, binding, TypeAccel, resource, "", 0, 0, 0));
    }
}
