namespace HCDE.Playsim;

/// <summary>
/// Vulkan texture manager, shadowmap size, and lightmap alpha from <c>vk_texture.cpp</c>
/// (1-253). Chunk 1295. Pipeline images and scene images are host slots.
/// </summary>
public sealed class VmVkTexture1295
{
    public const int PipelineImages = 2;
    public const int CurrentPipeline = 0;
    public const int NextPipeline = 1;
    public const int PostTexture = 2;
    public const int SceneColor = 3;
    public const int SceneFog = 4;
    public const int SceneNormal = 5;
    public const int SceneDepth = 6;
    public const int SwapChain = 7;
    public const int ShadowMap = 8;
    public const int Rgba8 = 37;
    public const int Rgba16f = 97;
    public const int R32f = 100;
    public const int Sampled = 0x4;
    public const int TransferDst = 0x2;
    public const int ColorAttachment = 0x10;
    public const int LayoutUndefined = 0;
    public const int LayoutShaderRead = 5;
    public const int LayoutTransferDst = 7;
    public const int View2DArray = 5;
    public const ushort HalfOne = 0x3c00;

    public sealed class Failure : Exception
    {
        public readonly string Detail;

        public Failure(string message) : base(message)
        {
            Detail = message;
        }
    }

    public sealed class Image
    {
        public int Format;
        public int Width;
        public int Height;
        public int Layers = 1;
        public int Usage;
        public string Name = "";
        public int ViewType;
    }

    public sealed class HardwareTexture
    {
        public string Name = "";
        public bool Reset;
        public bool Detached;
    }

    public sealed class PpTexture
    {
        public string Name = "";
        public bool Reset;
        public bool Detached;
        public bool HasBackend;
        public int Format = Rgba8;
    }

    public sealed class Transition
    {
        public string Image = "";
        public int Layout;
        public bool Undefined;
        public int BaseMip;
        public int LevelCount;
    }

    public int ShadowQuality;
    public int PipelineIndex;
    public Image? NullImage;
    public Image? NullView;
    public Image? ShadowImage;
    public Image? ShadowView;
    public Image? LightImage;
    public Image? LightView;
    public int ShadowResets;
    public int LightResets;
    public ushort[] LightTexels = Array.Empty<ushort>();
    public int LightSourceRead;
    public int StagingBytes;
    public List<HardwareTexture> Textures = new();
    public List<PpTexture> PostTextures = new();
    public List<string> Removed = new();
    public List<Transition> Transitions = new();
    public List<string> DeletedStaging = new();
    public int CopyLayers;
    public int CopyWidth;
    public int CopyHeight;
    public int CopyDepth;

    public VmVkTexture1295(int shadowQuality)
    {
        ShadowQuality = shadowQuality;
        CreateNull();
        CreateShadow();
        CreateLight();
    }

    public void Destroy()
    {
        Drain();
    }

    public void Deinit()
    {
        Drain();
    }

    public void BeginFrame()
    {
        if (ShadowImage == null || ShadowImage.Width != ShadowQuality)
        {
            ShadowResets++;
            CreateShadow();
        }
    }

    public void AddTexture(HardwareTexture texture)
    {
        Textures.Add(texture);
    }

    public void RemoveTexture(HardwareTexture texture)
    {
        texture.Reset = true;
        texture.Detached = true;
        Textures.Remove(texture);
        Removed.Add(texture.Name);
    }

    public void AddPost(PpTexture texture)
    {
        PostTextures.Add(texture);
    }

    public void RemovePost(PpTexture texture)
    {
        texture.Reset = true;
        texture.Detached = true;
        PostTextures.Remove(texture);
        Removed.Add(texture.Name);
    }

    public string? GetTexture(int type, PpTexture? texture)
    {
        if (type == CurrentPipeline || type == NextPipeline)
        {
            int index = PipelineIndex;
            if (type == NextPipeline)
                index = (index + 1) % PipelineImages;
            return "pipeline-" + index;
        }

        if (type == PostTexture)
            return "pp:" + GetPost(texture!).Name;
        if (type == SceneColor)
            return "scene-color";
        if (type == SceneNormal)
            return "scene-normal";
        if (type == SceneFog)
            return "scene-fog";
        if (type == SceneDepth)
            return "scene-depth";
        if (type == ShadowMap)
            return "shadow";
        if (type == SwapChain)
            return null;
        throw new Failure("VkPPRenderState::GetTexture not implemented yet for this texture type");
    }

    public int GetFormat(PpTexture texture)
    {
        return GetPost(texture).Format;
    }

    public PpTexture GetPost(PpTexture texture)
    {
        texture.HasBackend = true;
        return texture;
    }

    public void SetLightmap(int size, int count, ushort[] source)
    {
        LightResets++;
        LightImage = new Image
        {
            Format = Rgba16f,
            Width = size,
            Height = size,
            Layers = count,
            Usage = Sampled | TransferDst,
            Name = "VkRenderBuffers.Lightmap",
        };
        LightView = new Image
        {
            Format = Rgba16f,
            ViewType = View2DArray,
            Name = "VkRenderBuffers.LightmapView",
        };
        int pixels = size * size * count;
        StagingBytes = pixels * 8;
        var texels = new ushort[pixels * 4];
        int read = 0;
        int write = 0;
        for (int i = pixels; i > 0; i--)
        {
            texels[write++] = source[read++];
            texels[write++] = source[read++];
            texels[write++] = source[read++];
            texels[write++] = HalfOne;
        }

        LightSourceRead = read;
        LightTexels = texels;
        Transitions.Add(new Transition
        {
            Image = "light",
            Layout = LayoutTransferDst,
            Undefined = true,
            BaseMip = 0,
            LevelCount = count,
        });
        CopyLayers = count;
        CopyWidth = size;
        CopyHeight = size;
        CopyDepth = 1;
        Transitions.Add(new Transition
        {
            Image = "light",
            Layout = LayoutShaderRead,
            Undefined = false,
            BaseMip = 0,
            LevelCount = count,
        });
        DeletedStaging.Add("VkHardwareTexture.mStagingBuffer");
    }

    void Drain()
    {
        while (Textures.Count > 0)
            RemoveTexture(Textures[^1]);
        while (PostTextures.Count > 0)
            RemovePost(PostTextures[^1]);
    }

    void CreateNull()
    {
        NullImage = new Image
        {
            Format = Rgba8,
            Width = 1,
            Height = 1,
            Usage = Sampled,
            Name = "VkDescriptorSetManager.NullTexture",
        };
        NullView = new Image
        {
            Format = Rgba8,
            Name = "VkDescriptorSetManager.NullTextureView",
        };
        Transitions.Add(new Transition
        {
            Image = "null",
            Layout = LayoutShaderRead,
            Undefined = true,
            BaseMip = 0,
            LevelCount = 1,
        });
    }

    void CreateShadow()
    {
        ShadowImage = new Image
        {
            Format = R32f,
            Width = ShadowQuality,
            Height = 1024,
            Usage = ColorAttachment | Sampled,
            Name = "VkRenderBuffers.Shadowmap",
        };
        ShadowView = new Image
        {
            Format = R32f,
            Name = "VkRenderBuffers.ShadowmapView",
        };
        Transitions.Add(new Transition
        {
            Image = "shadow",
            Layout = LayoutShaderRead,
            Undefined = true,
            BaseMip = 0,
            LevelCount = 1,
        });
    }

    void CreateLight()
    {
        SetLightmap(1, 1, new ushort[] { 0, 0, 0, HalfOne });
    }
}
