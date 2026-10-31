namespace HCDE.Playsim;
/// <summary>
/// Hardware texture upload, wipe image, and material descriptor sets from
/// <c>vk_hwtexture.cpp</c> (1-386). Chunk 1302.
/// </summary>
public sealed class VmVkHwTexture1302
{
    public const int CtfIndexed = 4, CtfProcessData = 16, FormatR8 = 9;
    public const int FormatRgba = 37, FormatBgra = 44, FormatRgba32f = 109;
    public const int UsageTransferSrc = 0x1, UsageTransferDst = 0x2, UsageSampled = 0x4;
    public const int UsageColor = 0x10, UsageDepth = 0x20, MemoryCpuOnly = 2;
    public const int MemoryGpuOnly = 1, MemoryUnknown = 0, AllocDedicated = 0x1;
    public const int AllocMapped = 0x4, MemHostVisible = 0x2, MemHostCoherent = 0x4;
    public const int MemDeviceLocal = 0x1, AspectColor = 1, AspectDepth = 2;
    public const int AspectStencil = 4, LayoutUndefined = 0, LayoutGeneral = 1;
    public const int LayoutDepth = 3, LayoutShaderRead = 5, LayoutTransferDst = 7;
    public const int SampleCount1 = 1, UploadLimit = 64 * 1024 * 1024, MinTextureLayers = 11;
    public const int ClampNone = 0, ClampXy = 3, ClampNoFilter = 5;
    public const int ClampCamtex = 9, FirstUserShader = 15;
    public sealed class Services
    {
        public List<VmVkHwTexture1302> Textures = new();
        public List<VmVkMaterial1302> Materials = new();
        public int TransferBytes;
        public List<GpuBuffer> Transfer = new();
        public List<string> DrawRetired = new();
        public List<Descriptor> DrawDescriptors = new();
        public int Waits;
        public bool LastWaitFinish = true;
        public bool LastWaitUpload;
        public int BufferWidth;
        public int BufferHeight;
        public int SceneDepthFormat;
        public long NextAllocatedBytes;
        public int PaletteLookups;
        public int DeviceExecutes;
        public int NextDescriptor;
        public string NullView = "null-view";
        public List<string> Trace = new();
        public List<Transition> Transitions = new();
        public List<Copy> Copies = new();
        public List<(string Image, int Layout)> Blits = new();
        public List<Clear> Clears = new();
        public int PaletteToken(int type, int index)
        {
            PaletteLookups++;
            return 50000 + type * 1000 + index;
        }
        public void WaitForCommands(bool finish, bool uploadOnly)
        {
            Waits++;
            LastWaitFinish = finish;
            LastWaitUpload = uploadOnly;
            foreach (GpuBuffer buffer in Transfer)
                buffer.Alive = false;
            Transfer.Clear();
            TransferBytes = 0;
            Trace.Add(uploadOnly ? "wait:upload" : "wait");
        }
    }
    public sealed class GpuImage
    {
        public string Name = "";
        public int Width;
        public int Height;
        public int Levels = 1;
        public int Format;
        public int Usage;
        public int Samples = SampleCount1;
        public int Memory = -1;
        public int AllocFlags;
        public bool Linear;
        public int RequiredMemory;
        public int PreferredMemory;
        public long AllocatedBytes;
        public bool Alive = true;
        public bool Queued;
        public bool MappedOpen;
    }
    public sealed class GpuBuffer
    {
        public string Name = "";
        public int Size;
        public int Usage;
        public int Memory;
        public byte[] Data = Array.Empty<byte>();
        public bool Alive = true;
        public bool Queued;
    }
    public sealed class View
    {
        public string Name = "";
        public int Format;
        public int Aspect = AspectColor;
        public bool Alive = true;
        public bool Queued;
    }
    public sealed class TexImage
    {
        public GpuImage? Image;
        public View? View;
        public View? DepthOnly;
        public string? PostFramebuffer;
        public List<(int Key, string Framebuffer)> Passes = new();
        public int Layout = LayoutUndefined;
        public int Aspect = AspectColor;
    }
    public sealed class Tex
    {
        public int Width;
        public int Height;
        public bool HardwareCanvas;
        public bool Hdr;
        public byte[] Pixels = Array.Empty<byte>();
    }
    public readonly record struct Transition(string Queue, string Image, int Layout, bool Undefined);
    public readonly record struct Copy(string Src, string Dst, int Width, int Height, int Depth, int Aspect, int Layers);
    public sealed class Clear
    {
        public string Image = "";
        public int Layout;
        public float R;
        public float G;
        public float B;
        public float A;
        public int Aspect = AspectColor;
        public int Layers = 1;
        public int Levels = 1;
    }
    public sealed class Descriptor
    {
        public int Id;
        public string Name = "";
        public int Slots;
        public List<Write> Writes = new();
        public bool Alive = true;
        public bool Queued;
    }
    public readonly record struct Write(int Binding, string View, string Sampler, int Layout);
    public Services Host;
    public bool Live = true;
    public int TexelSize;
    public byte[]? Mapped;
    public TexImage Color = new();
    public TexImage Depth = new();
    public int BufferPitch = -1;
    public int MipmapCalls;
    public List<(int Translation, int Flags)> TexBuffers = new();
    public VmVkHwTexture1302(Services host, int channels)
    {
        Host = host;
        TexelSize = channels;
        host.Textures.Add(this);
    }
    public void Destroy()
    {
        if (Live)
            Host.Textures.Remove(this);
        DropImage(Color);
        DropImage(Depth);
        Mapped = null;
    }
    public void Reset()
    {
        if (!Live)
            return;
        if (Mapped != null)
        {
            if (Color.Image != null)
                Color.Image.MappedOpen = false;
            Mapped = null;
            Host.Trace.Add("unmap");
        }
        Retire(Color);
        Retire(Depth);
    }
    public TexImage GetImage(Tex tex, int translation, int flags)
    {
        if (Color.Image == null)
            CreateImage(tex, translation, flags);
        return Color;
    }
    public TexImage GetDepthStencil(Tex tex)
    {
        if (Depth.View != null)
            return Depth;
        var image = new GpuImage
        {
            Name = "VkHardwareTexture.DepthStencil",
            Width = tex.Width,
            Height = tex.Height,
            Format = Host.SceneDepthFormat,
            Samples = SampleCount1,
            Usage = UsageDepth,
        };
        Depth.Image = image;
        Depth.Aspect = AspectDepth | AspectStencil;
        Depth.View = new View
        {
            Name = "VkHardwareTexture.DepthStencilView",
            Format = Host.SceneDepthFormat,
            Aspect = AspectDepth | AspectStencil,
        };
        TransitionTo(Depth, image.Name, LayoutDepth, true, "transfer");
        return Depth;
    }
    public static int GetMipLevels(int width, int height)
    {
        int levels = 1;
        while (width > 1 || height > 1)
        {
            width = Math.Max(width >> 1, 1);
            height = Math.Max(height >> 1, 1);
            levels++;
        }
        return levels;
    }
    public void AllocateBuffer(int width, int height, int texelSize)
    {
        if (Color.Image != null && (Color.Image.Width != width || Color.Image.Height != height || TexelSize != texelSize))
            Reset();
        if (Color.Image != null)
            return;
        int format = texelSize == 4 ? FormatBgra : FormatR8;
        long allocated = Host.NextAllocatedBytes;
        var image = new GpuImage
        {
            Name = "VkHardwareTexture.mImage",
            Width = width,
            Height = height,
            Format = format,
            Usage = UsageSampled,
            Memory = MemoryUnknown,
            AllocFlags = AllocDedicated | AllocMapped,
            Linear = true,
            RequiredMemory = MemHostVisible | MemHostCoherent,
            PreferredMemory = MemHostVisible | MemHostCoherent | MemDeviceLocal,
            AllocatedBytes = allocated,
        };
        Color.Image = image;
        TexelSize = texelSize;
        Color.View = new View { Name = "VkHardwareTexture.mImageView", Format = format };
        TransitionTo(Color, image.Name, LayoutGeneral, true, "transfer");
        BufferPitch = (int)(allocated / height / texelSize);
    }
    public byte[] MapBuffer()
    {
        if (Mapped == null)
        {
            GpuImage image = Color.Image!;
            Mapped = new byte[image.Width * image.Height * TexelSize];
            image.MappedOpen = true;
            Host.Trace.Add("map");
        }
        return Mapped;
    }
    public int CreateTexture(byte[]? buffer, int width, int height, int texunit, bool mipmap, string name)
    {
        _ = (texunit, name);
        if (buffer != null)
        {
            int format = TexelSize == 4 ? FormatBgra : FormatR8;
            CreateTexture(width, height, TexelSize, format, buffer, mipmap);
        }
        return 0;
    }
    public void CreateWipeTexture(int width, int height, string name)
    {
        var image = new GpuImage
        {
            Name = name,
            Width = width,
            Height = height,
            Format = FormatBgra,
            Usage = UsageSampled | UsageTransferDst,
            Memory = MemoryGpuOnly,
        };
        Replace(Color, image, new View { Name = name, Format = FormatBgra });
        TexelSize = 4;
        if (Host.BufferWidth > 0 && Host.BufferHeight > 0)
        {
            Host.Blits.Add((image.Name, LayoutShaderRead));
            Color.Layout = LayoutShaderRead;
            Host.Trace.Add("blit");
            return;
        }
        TransitionTo(Color, image.Name, LayoutTransferDst, true, "transfer");
        Host.Clears.Add(new Clear
        {
            Image = image.Name,
            Layout = Color.Layout,
            A = 1f,
        });
        Host.Trace.Add("clear");
        TransitionTo(Color, image.Name, LayoutShaderRead, false, "transfer");
    }
    void CreateImage(Tex tex, int translation, int flags)
    {
        if (!tex.HardwareCanvas)
        {
            TexBuffers.Add((translation, flags | CtfProcessData));
            bool indexed = (flags & CtfIndexed) != 0;
            int pixels = indexed ? 1 : 4;
            int format = indexed ? FormatR8 : FormatBgra;
            CreateTexture(tex.Width, tex.Height, pixels, format, tex.Pixels, !indexed);
            return;
        }
        int canvasFormat = tex.Hdr ? FormatRgba32f : FormatRgba;
        var image = new GpuImage
        {
            Name = "VkHardwareTexture.mImage",
            Width = tex.Width,
            Height = tex.Height,
            Format = canvasFormat,
            Usage = UsageColor | UsageSampled,
        };
        Replace(Color, image, new View { Name = "VkHardwareTexture.mImageView", Format = canvasFormat });
        TransitionTo(Color, image.Name, LayoutShaderRead, true, "transfer");
    }
    void CreateTexture(int width, int height, int pixelSize, int format, byte[] pixels, bool mipmap)
    {
        if (width <= 0 || height <= 0)
            throw new InvalidOperationException("Trying to create zero size texture");
        int total = width * height * pixelSize;
        var staging = new GpuBuffer
        {
            Name = "VkHardwareTexture.mStagingBuffer",
            Size = total,
            Usage = UsageTransferSrc,
            Memory = MemoryCpuOnly,
        };
        staging.Data = new byte[total];
        Buffer.BlockCopy(pixels, 0, staging.Data, 0, total);
        int levels = mipmap ? GetMipLevels(width, height) : 1;
        var image = new GpuImage
        {
            Name = "VkHardwareTexture.mImage",
            Width = width,
            Height = height,
            Levels = levels,
            Format = format,
            Usage = UsageTransferSrc | UsageTransferDst | UsageSampled,
        };
        Replace(Color, image, new View { Name = "VkHardwareTexture.mImageView", Format = format });
        TransitionTo(Color, image.Name, LayoutTransferDst, true, "transfer");
        Host.Copies.Add(new Copy(staging.Name, image.Name, width, height, 1, AspectColor, 1));
        Host.Trace.Add("copy");
        if (mipmap)
        {
            MipmapCalls++;
            Color.Layout = LayoutShaderRead;
            Host.Trace.Add("mipmap");
        }
        staging.Queued = true;
        Host.Transfer.Add(staging);
        Host.TransferBytes += staging.Size;
        if (Host.TransferBytes > UploadLimit)
            Host.WaitForCommands(false, true);
    }
    static void DropImage(TexImage image)
    {
        if (image.Image is { Queued: false })
            image.Image.Alive = false;
        if (image.View is { Queued: false })
            image.View.Alive = false;
        image.Image = null;
        image.View = null;
    }
    void Replace(TexImage slot, GpuImage image, View view)
    {
        if (slot.Image is { Queued: false })
            slot.Image.Alive = false;
        if (slot.View is { Queued: false })
            slot.View.Alive = false;
        slot.Image = image;
        slot.View = view;
    }
    void Retire(TexImage image)
    {
        image.Aspect = AspectColor;
        image.Layout = LayoutUndefined;
        if (image.PostFramebuffer != null)
        {
            Host.DrawRetired.Add(image.PostFramebuffer);
            image.PostFramebuffer = null;
        }
        foreach ((int _, string framebuffer) in image.Passes.OrderBy(pass => pass.Key))
            Host.DrawRetired.Add(framebuffer);
        image.Passes.Clear();
        RetireView(image.DepthOnly);
        image.DepthOnly = null;
        RetireView(image.View);
        image.View = null;
        RetireGpu(image.Image);
        image.Image = null;
    }
    void RetireView(View? view)
    {
        if (view == null)
            return;
        view.Queued = true;
        Host.DrawRetired.Add(view.Name);
    }
    void RetireGpu(GpuImage? image)
    {
        if (image == null)
            return;
        image.Queued = true;
        Host.DrawRetired.Add(image.Name);
    }
    void TransitionTo(TexImage image, string name, int layout, bool undefined, string queue)
    {
        image.Layout = layout;
        Host.Transitions.Add(new Transition(queue, name, layout, undefined));
        Host.Trace.Add("transition");
    }
}
public sealed class VmVkMaterial1302
{
    public sealed class Layer
    {
        public VmVkHwTexture1302 Hw = null!;
        public VmVkHwTexture1302.Tex Source = new();
        public int ScaleFlags;
    }
    public sealed class Entry
    {
        public int Clamp;
        public int Remap;
        public VmVkHwTexture1302.Descriptor? Descriptor;
    }
    public readonly record struct State(int ClampMode, int Translation);
    public VmVkHwTexture1302.Services Host;
    public bool Live = true;
    public bool SwCanvas;
    public bool HardwareCanvas;
    public bool Warped;
    public int ShaderIndex;
    public List<Layer> Layers = new();
    public List<Entry> Entries = new();
    public List<(int Index, int LayerTranslation, int ImageTranslation)> LayerCalls = new();
    public VmVkMaterial1302(VmVkHwTexture1302.Services host)
    {
        Host = host;
        host.Materials.Add(this);
    }
    public void Destroy()
    {
        if (Live)
            Host.Materials.Remove(this);
        foreach (Entry entry in Entries)
        {
            if (entry.Descriptor is { Queued: false })
                entry.Descriptor.Alive = false;
        }
        Entries.Clear();
    }
    public void DeleteDescriptors()
    {
        if (!Live)
            return;
        foreach (Entry entry in Entries)
        {
            if (entry.Descriptor == null)
                continue;
            entry.Descriptor.Queued = true;
            Host.DrawDescriptors.Add(entry.Descriptor);
        }
        Entries.Clear();
    }
    public int ResolvedClamp(int clampmode)
    {
        if (SwCanvas)
            return VmVkHwTexture1302.ClampNoFilter;
        if (HardwareCanvas)
            return VmVkHwTexture1302.ClampCamtex;
        if ((Warped || ShaderIndex >= VmVkHwTexture1302.FirstUserShader) && clampmode <= VmVkHwTexture1302.ClampXy)
            return VmVkHwTexture1302.ClampNone;
        return clampmode;
    }
    public VmVkHwTexture1302.Descriptor GetDescriptorSet(State state)
    {
        int translation = state.Translation;
        int remap = IsLuminosity(translation)
            ? translation
            : Host.PaletteToken(TranslationType(translation), TranslationIndex(translation));
        int clampmode = ResolvedClamp(state.ClampMode);
        foreach (Entry entry in Entries)
        {
            if (entry.Descriptor != null && entry.Clamp == clampmode && entry.Remap == remap)
                return entry.Descriptor;
        }
        int numLayers = Layers.Count;
        var descriptor = new VmVkHwTexture1302.Descriptor
        {
            Id = ++Host.NextDescriptor,
            Slots = Math.Max(numLayers, VmVkHwTexture1302.MinTextureLayers),
            Name = "VkHardwareTexture.mDescriptorSets",
        };
        string sampler = "sampler:" + clampmode;
        Layer layer = Layers[0];
        LayerCalls.Add((0, translation, translation));
        VmVkHwTexture1302.TexImage image = layer.Hw.GetImage(layer.Source, translation, layer.ScaleFlags);
        descriptor.Writes.Add(new VmVkHwTexture1302.Write(0, image.View!.Name, sampler, image.Layout));
        if ((layer.ScaleFlags & VmVkHwTexture1302.CtfIndexed) == 0)
        {
            for (int i = 1; i < numLayers; i++)
                BindLayer(descriptor, i, 0, sampler);
        }
        else
        {
            for (int i = 1; i < 3; i++)
                BindLayer(descriptor, i, translation, sampler);
            numLayers = 3;
        }
        for (int i = numLayers; i < VmVkHwTexture1302.MinTextureLayers; i++)
        {
            descriptor.Writes.Add(new VmVkHwTexture1302.Write(
                i, Host.NullView, sampler, VmVkHwTexture1302.LayoutShaderRead));
        }
        Host.DeviceExecutes++;
        Entries.Add(new Entry { Clamp = clampmode, Remap = remap, Descriptor = descriptor });
        return descriptor;
    }
    public static bool IsLuminosity(int translation)
    {
        return translation > 0 && (translation & (1 << 30)) != 0;
    }
    public static int TranslationType(int translation)
    {
        return (translation & (255 << 16)) >> 16;
    }
    public static int TranslationIndex(int translation)
    {
        return translation & 0xffff;
    }
    void BindLayer(VmVkHwTexture1302.Descriptor descriptor, int index, int translation, string sampler)
    {
        Layer layer = Layers[index];
        LayerCalls.Add((index, translation, 0));
        VmVkHwTexture1302.TexImage image = layer.Hw.GetImage(layer.Source, 0, layer.ScaleFlags);
        descriptor.Writes.Add(new VmVkHwTexture1302.Write(index, image.View!.Name, sampler, image.Layout));
    }
}
