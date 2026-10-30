namespace HCDE.Playsim;

/// <summary>
/// Vulkan pipeline and scene buffers from <c>vk_renderbuffers.cpp</c> (1-305).
/// Chunk 1298. Sample masks, format support, and the swapchain size are host services.
/// Pipeline images stay at one sample. A sample-only change recreates the scene and
/// leaves the pipeline. A null texture uses the swapchain framebuffer and caches it.
/// The destructor does not release the images.
/// </summary>
public sealed class VmVkRenderBuffers1298
{
    public const int Sample1 = 0x1;
    public const int Sample2 = 0x2;
    public const int Sample4 = 0x4;
    public const int Sample8 = 0x8;
    public const int Sample16 = 0x10;
    public const int Sample32 = 0x20;
    public const int Sample64 = 0x40;
    public const int LayoutColor = 2;
    public const int LayoutDepth = 3;
    public const int AspectDepth = 0x2;
    public const int AspectStencil = 0x4;
    public const int UsageTransferSrc = 0x1;
    public const int UsageTransferDst = 0x2;
    public const int UsageSampled = 0x4;
    public const int UsageColor = 0x10;
    public const int UsageDepth = 0x20;
    public const int FeatureSampled = 0x1;
    public const int FeatureColor = 0x80;
    public const int FormatRgba16f = 97;
    public const int FormatRgba8 = 37;
    public const int FormatA2R10 = 58;
    public const int FormatD24S8 = 129;
    public const int FormatD32S8 = 130;
    public const int NextPipeline = 1;
    public const int StencilNone = 0;
    public const int StencilScene = 1;
    public const int StencilPipeline = 2;
    public const int PipelineSlots = 2;

    public sealed class Failure : Exception
    {
        public readonly string Detail;
        public Failure(string message) : base(message)
        {
            Detail = message;
        }
    }

    public sealed class Host
    {
        public int Multisample;
        public int SampleMask = Sample1;
        public int PresentImageIndex;
        public int SwapWidth;
        public int SwapHeight;
        public List<int> DepthQueries = new();
        public List<int> NormalQueries = new();
        public HashSet<int> DepthFormats = new() { FormatD24S8, FormatD32S8 };
        public HashSet<int> NormalFormats = new() { FormatA2R10, FormatRgba8 };
        public int Waits;
        public int Resets;
        public int Draws;

        public bool DepthSupported(int format)
        {
            DepthQueries.Add(format);
            return DepthFormats.Contains(format);
        }

        public bool NormalSupported(int format)
        {
            NormalQueries.Add(format);
            return NormalFormats.Contains(format);
        }
    }

    public sealed class Image
    {
        public int Id;
        public int Width;
        public int Height;
        public int Samples = 1;
        public int Format;
        public int Usage;
        public int Aspect;
        public string Name = "";
        public string ViewName = "";
        public string DepthViewName = "";
        public bool HasImage;
        public bool HasFramebuffer;
        public int FramePass;
        public int FrameAttachments;
    }

    public readonly record struct Transition(string Name, int Layout, bool Undefined);

    public Host Services;
    public int Width;
    public int Height;
    public int SceneWidth;
    public int SceneHeight;
    public int Samples = Sample1;
    public int PipelineDepthFormat = FormatD24S8;
    public int SceneDepthFormat = FormatD24S8;
    public int SceneNormalFormat = FormatA2R10;
    public bool Destroyed;
    public int ImagesReleased;
    public int NextId = 1;
    public int Executes;
    public Image PipelineDepth = new();
    public Image SceneColor = new();
    public Image SceneDepth = new();
    public Image SceneNormal = new();
    public Image SceneFog = new();
    public Image[] Pipeline = { new Image(), new Image() };
    public List<Transition> Transitions = new();
    public Dictionary<int, Image> SwapFramebuffers = new();

    public VmVkRenderBuffers1298(Host host)
    {
        Services = host;
        Samples = Sample1;
    }

    public void Destroy()
    {
        Destroyed = true;
        ImagesReleased = 0;
    }

    public int GetBestSampleCount()
    {
        int requested = Services.Multisample;
        if (requested < 0)
            requested = 0;
        if (requested > 64)
            requested = 64;
        int count = 1;
        int bit = Sample1;
        int best = bit;
        while (count <= requested)
        {
            if ((Services.SampleMask & bit) != 0)
                best = bit;
            count <<= 1;
            bit <<= 1;
        }

        return best;
    }

    public void BeginFrame(int width, int height, int sceneWidth, int sceneHeight)
    {
        int samples = GetBestSampleCount();
        if (width != Width || height != Height || Samples != samples)
        {
            Services.Waits++;
            Services.Resets++;
        }

        if (width != Width || height != Height)
            CreatePipeline(width, height);
        if (width != Width || height != Height || Samples != samples)
            CreateScene(width, height, samples);
        Width = width;
        Height = height;
        Samples = samples;
        SceneWidth = sceneWidth;
        SceneHeight = sceneHeight;
    }

    public void CreatePipeline(int width, int height)
    {
        for (int i = 0; i < PipelineSlots; i++)
            Pipeline[i] = new Image();
        PipelineDepth = new Image();
        CreatePipelineDepth(width, height);
        Add(PipelineDepth.Name, LayoutDepth, true);
        for (int i = 0; i < PipelineSlots; i++)
        {
            Pipeline[i] = Make(width, height, 1, FormatRgba16f, UsageColor | UsageSampled | UsageTransferSrc | UsageTransferDst, "VkRenderBuffers.PipelineImage", "VkRenderBuffers.PipelineView");
            Add(Pipeline[i].Name, LayoutColor, true);
        }

        Executes++;
        Services.Draws++;
    }

    public void CreateScene(int width, int height, int samples)
    {
        SceneColor = new Image();
        SceneDepth = new Image();
        SceneNormal = new Image();
        SceneFog = new Image();
        SceneColor = Make(width, height, samples, FormatRgba16f, UsageColor | UsageSampled | UsageTransferSrc, "VkRenderBuffers.SceneColor", "VkRenderBuffers.SceneColorView");
        CreateSceneDepth(width, height, samples);
        CreateSceneNormal(width, height, samples);
        SceneFog = Make(width, height, samples, FormatRgba8, UsageColor | UsageSampled, "VkRenderBuffers.SceneFog", "VkRenderBuffers.SceneFogView");
        Add(SceneColor.Name, LayoutColor, true);
        Add(SceneDepth.Name, LayoutDepth, true);
        Add(SceneNormal.Name, LayoutColor, true);
        Add(SceneFog.Name, LayoutColor, true);
        Executes++;
        Services.Draws++;
    }

    public Image GetOutput(Image? tex, int type, int stencil, int pass)
    {
        if (tex != null)
        {
            Add(tex.Name, LayoutColor, type == NextPipeline);
            if (stencil == StencilScene)
                Add(SceneDepth.Name, LayoutDepth, false);
            if (stencil == StencilPipeline)
                Add(PipelineDepth.Name, LayoutDepth, false);
            Executes++;
            Services.Draws++;
            if (!tex.HasFramebuffer)
            {
                tex.HasFramebuffer = true;
                tex.FramePass = pass;
                tex.FrameAttachments = 1;
                if (stencil == StencilScene || stencil == StencilPipeline)
                    tex.FrameAttachments = 2;
            }

            return tex;
        }

        int index = Services.PresentImageIndex;
        if (!SwapFramebuffers.TryGetValue(index, out Image? fb))
        {
            fb = new Image();
            fb.Id = NextId++;
            fb.Name = "PPOutputFB";
            fb.Width = Services.SwapWidth;
            fb.Height = Services.SwapHeight;
            fb.HasFramebuffer = true;
            fb.FramePass = pass;
            fb.FrameAttachments = 1;
            if (stencil == StencilScene || stencil == StencilPipeline)
                fb.FrameAttachments = 2;
            SwapFramebuffers[index] = fb;
        }

        return fb;
    }

    void CreatePipelineDepth(int width, int height)
    {
        if (!Services.DepthSupported(PipelineDepthFormat))
        {
            PipelineDepthFormat = FormatD32S8;
            if (!Services.DepthSupported(PipelineDepthFormat))
                throw new Failure("This device does not support any of the required depth stencil image formats.");
        }

        PipelineDepth = Make(width, height, 1, PipelineDepthFormat, UsageDepth | UsageSampled, "VkRenderBuffers.PipelineDepthStencil", "VkRenderBuffers.PipelineDepthStencilView");
        PipelineDepth.Aspect = AspectDepth | AspectStencil;
        PipelineDepth.DepthViewName = "VkRenderBuffers.PipelineDepthView";
    }

    void CreateSceneDepth(int width, int height, int samples)
    {
        if (!Services.DepthSupported(SceneDepthFormat))
        {
            SceneDepthFormat = FormatD32S8;
            if (!Services.DepthSupported(SceneDepthFormat))
                throw new Failure("This device does not support any of the required depth stencil image formats.");
        }

        SceneDepth = Make(width, height, samples, SceneDepthFormat, UsageDepth | UsageSampled, "VkRenderBuffers.SceneDepthStencil", "VkRenderBuffers.SceneDepthStencilView");
        SceneDepth.Aspect = AspectDepth | AspectStencil;
        SceneDepth.DepthViewName = "VkRenderBuffers.SceneDepthView";
    }

    void CreateSceneNormal(int width, int height, int samples)
    {
        if (!Services.NormalSupported(SceneNormalFormat))
            SceneNormalFormat = FormatRgba8;
        SceneNormal = Make(width, height, samples, SceneNormalFormat, UsageColor | UsageSampled, "VkRenderBuffers.SceneNormal", "VkRenderBuffers.SceneNormalView");
    }

    Image Make(int width, int height, int samples, int format, int usage, string name, string view)
    {
        var image = new Image();
        image.Id = NextId++;
        image.Width = width;
        image.Height = height;
        image.Samples = samples;
        image.Format = format;
        image.Usage = usage;
        image.Name = name;
        image.ViewName = view;
        image.HasImage = true;
        return image;
    }

    void Add(string name, int layout, bool undefined)
    {
        Transitions.Add(new Transition(name, layout, undefined));
    }
}
