namespace HCDE.Playsim;

/// <summary>
/// Automatic uniforms, scene blit, present gamma, and the shadow-map handoff
/// from <c>vk_postprocess.cpp</c> (1-360). Chunk 1301. Clocks, the linear-scale
/// switch, and whether the shadow map wants an update are host inputs. The
/// destructor body adds no extra release; the uniform buffer is the member
/// that goes away with the object. <c>NextEye</c> does not change the pipeline
/// image, so the eye loop records the calls instead of an empty method.
/// </summary>
public sealed class VmVkPostprocess1301
{
    public const int UsageUniform = 0x10;
    public const int MemoryCpuToGpu = 3;
    public const int LayoutColor = 2;
    public const int LayoutDepth = 3;
    public const int LayoutShaderRead = 5;
    public const int LayoutTransferSrc = 6;
    public const int LayoutTransferDst = 7;
    public const int FormatRgba16f = 97;
    public const int SampleCount1 = 1;
    public const int AspectColor = 1;
    public const int FilterNearest = 0;
    public const int FilterLinear = 1;
    public const int WrapRepeat = 1;
    public const int ColorSpaceExtendedSrgbLinear = 1000104002;
    public const int UniformBytes = 16;
    public const bool DeltaUsesCurrentArgument = false;

    public sealed class Host
    {
        public double DeltaTime;
        public uint FrameTime;
        public int WorldTimer;
        public int TicRate = 35;
        public int EyeCount = 1;
        public bool LinearScale;
        public float Gamma = 1f;
        public float Contrast = 1f;
        public float Saturation = 1f;
        public float BlackPoint;
        public float WhitePoint = 1f;
        public int SatFormula;
        public int DitherBpc = -1;
        public int ColorSpace;
        public bool ShadowCollects;
        public int ViewportWidth;
        public int ViewportHeight;
    }

    public sealed class GpuBuffer
    {
        public int Size;
        public int Usage;
        public int Memory;
        public string Name = "";
        public float[] Mapped = new float[4];
        public bool Alive = true;
        public bool MappedOpen;
        public int MapOffset = -1;
        public int MapSize;
    }

    public sealed class Image
    {
        public string Name = "";
        public int Width;
        public int Height;
        public int Layout = -1;
        public bool Undefined;

        public Image(string name, int width, int height)
        {
            Name = name;
            Width = width;
            Height = height;
        }
    }

    public readonly record struct ImageStep(string Name, int Layout, bool Undefined);

    public sealed class TransitionBatch
    {
        public string Queue = "";
        public List<ImageStep> Steps = new();
    }

    public sealed class Region
    {
        public string Kind = "";
        public string Src = "";
        public string Dst = "";
        public int SrcWidth;
        public int SrcHeight;
        public int DstWidth;
        public int DstHeight;
        public int Depth = 1;
        public int Filter = -1;
        public int Aspect = AspectColor;
        public int Mip;
        public int Layer;
        public int LayerCount = 1;
        public int SrcLayout;
        public int DstLayout;
    }

    public readonly record struct Target(string Color, string DepthView, int Width, int Height, int Format, int Samples);

    public readonly record struct Pass(string Name, int FixedCm, float? Flash, int Width, int Height);

    public readonly record struct Box(int X, int Y, int Width, int Height);

    public sealed class PresentUniforms
    {
        public float InvGamma;
        public float Contrast;
        public float Saturation;
        public float BlackPoint;
        public float WhitePoint;
        public float ColorScale;
        public int? GrayFormula;
        public int? WindowPositionParity = null;
        public float ScaleX;
        public float ScaleY;
        public float OffsetX;
        public float OffsetY;
        public int HdrMode;
    }

    public sealed class PresentDraw
    {
        public List<string> Steps = new();
        public PresentUniforms Uniforms = new();
        public string Shader = "";
        public string? ScreenShader;
        public Box Viewport;
        public int InputFilter;
        public string Dither = "";
        public int DitherFilter;
        public int DitherWrap;
        public string Output = "";
        public bool NoBlend;
    }

    public readonly record struct SsaoCall(float M5, int Width, int Height);

    public readonly record struct BloomCall(int Width, int Height, float Amount);

    public Host Services;
    public GpuBuffer Uniforms;
    public int CurrentPipelineImage;
    public int SceneWidth;
    public int SceneHeight;
    public int BufferWidth;
    public int BufferHeight;
    public int SceneSamples = SampleCount1;
    public string DepthView = "PipelineDepthStencil.View";
    public Image SceneColor;
    public Image SceneFog;
    public Image SceneNormal;
    public Image SceneDepth;
    public Image Shadowmap;
    public Image Depth;
    public List<Image> Pipeline = new();
    public float TimeDelta;
    public float Time;
    public float TimeGame;
    public int Ends;
    public int TonemapClears;
    public int ShadowUpdates;
    public int ShadowFinishes;
    public bool MembersReleased;
    public List<string> Trace = new();
    public List<TransitionBatch> Transitions = new();
    public List<Region> Regions = new();
    public List<Target> Targets = new();
    public List<Pass> Passes = new();
    public List<PresentDraw> Presents = new();
    public List<SsaoCall> Ssao = new();
    public List<BloomCall> Blooms = new();
    public List<int> EyeAdvances = new();

    public VmVkPostprocess1301(Host host)
    {
        Services = host;
        Uniforms = new GpuBuffer
        {
            Size = UniformBytes,
            Usage = UsageUniform,
            Memory = MemoryCpuToGpu,
            Name = "AutomaticUniformsBuffer",
        };
        SceneColor = new Image("SceneColor", 0, 0);
        SceneFog = new Image("SceneFog", 0, 0);
        SceneNormal = new Image("SceneNormal", 0, 0);
        SceneDepth = new Image("SceneDepthStencil", 0, 0);
        Shadowmap = new Image("Shadowmap", 0, 0);
        Depth = new Image("PipelineDepthStencil", 0, 0);
    }

    public void Destroy()
    {
        if (MembersReleased)
            return;
        Uniforms.Alive = false;
        MembersReleased = true;
        Trace.Add("release:" + Uniforms.Name);
    }

    public void SetActiveRenderTarget()
    {
        Image color = Pipeline[CurrentPipelineImage];
        Execute((color, LayoutColor, false), (Depth, LayoutDepth, false));
        Targets.Add(new Target(color.Name, DepthView, BufferWidth, BufferHeight, FormatRgba16f, SampleCount1));
        Trace.Add("render-target");
    }

    public void PostProcessScene(int fixedcm, float flash, Action afterBloom)
    {
        float timeDelta = (float)Services.DeltaTime;
        float time = (float)(Services.FrameTime / 1000.0);
        float timeGame = (float)(Services.WorldTimer / (double)Services.TicRate);
        TimeDelta = timeDelta;
        Time = time;
        TimeGame = timeGame;
        Uniforms.MappedOpen = true;
        Uniforms.MapOffset = 0;
        Uniforms.MapSize = UniformBytes;
        Uniforms.Mapped[0] = timeDelta;
        Uniforms.Mapped[1] = time;
        Uniforms.Mapped[2] = timeGame;
        Uniforms.Mapped[3] = 0f;
        Uniforms.MappedOpen = false;
        Trace.Add("map");
        Passes.Add(new Pass("pass1", fixedcm, null, SceneWidth, SceneHeight));
        Trace.Add("pass1");
        SetActiveRenderTarget();
        Trace.Add("callback");
        afterBloom();
        Passes.Add(new Pass("pass2", fixedcm, flash, SceneWidth, SceneHeight));
        Trace.Add("pass2");
    }

    public void BlitSceneToPostprocess()
    {
        EndPass();
        CurrentPipelineImage = 0;
        Image dst = Pipeline[0];
        Execute((SceneColor, LayoutTransferSrc, false), (dst, LayoutTransferDst, true));
        if (SceneSamples != SampleCount1)
            AddResolve(SceneColor, dst);
        else
            AddBlit(SceneColor, dst, SceneColor.Width, SceneColor.Height, SceneColor.Width, SceneColor.Height);
    }

    public void ImageTransitionScene(bool undefinedSrcLayout)
    {
        Execute(
            (SceneColor, LayoutColor, undefinedSrcLayout),
            (SceneFog, LayoutColor, undefinedSrcLayout),
            (SceneNormal, LayoutColor, undefinedSrcLayout),
            (SceneDepth, LayoutDepth, undefinedSrcLayout));
    }

    public void BlitCurrentToImage(Image dst, int finalLayout = LayoutShaderRead)
    {
        EndPass();
        Image src = Pipeline[CurrentPipelineImage];
        Execute((src, LayoutTransferSrc, false), (dst, LayoutTransferDst, true));
        AddBlit(src, dst, src.Width, src.Height, dst.Width, dst.Height);
        Execute((dst, finalLayout, false));
    }

    public void CopyCurrentToImage(Image dst, int finalLayout = LayoutShaderRead)
    {
        EndPass();
        Image src = Pipeline[CurrentPipelineImage];
        if (src.Width != dst.Width || src.Height != dst.Height)
            return;
        Execute((src, LayoutTransferSrc, false), (dst, LayoutTransferDst, true));
        Regions.Add(new Region
        {
            Kind = "copy",
            Src = src.Name,
            Dst = dst.Name,
            SrcWidth = src.Width,
            SrcHeight = src.Height,
            DstWidth = src.Width,
            DstHeight = src.Height,
            Depth = 1,
            Aspect = AspectColor,
            LayerCount = 1,
            SrcLayout = LayoutTransferSrc,
            DstLayout = LayoutTransferDst,
        });
        Trace.Add("copy");
        Execute((src, LayoutColor, false), (dst, finalLayout, false));
    }

    public void DrawPresentTexture(Box box, bool applyGamma, bool screenshot)
    {
        var draw = new PresentDraw();
        if (!screenshot)
        {
            draw.ScreenShader = "screen";
            draw.Steps.Add("screen");
        }

        var uniforms = new PresentUniforms();
        if (!applyGamma)
        {
            uniforms.InvGamma = 1f;
            uniforms.Contrast = 1f;
            uniforms.Saturation = 1f;
            uniforms.BlackPoint = 0f;
            uniforms.WhitePoint = 1f;
        }
        else
        {
            uniforms.InvGamma = 1f / Clamp(Services.Gamma, 0.1f, 4f);
            uniforms.Contrast = Clamp(Services.Contrast, 0.1f, 3f);
            uniforms.Saturation = Clamp(Services.Saturation, -15f, 15f);
            uniforms.BlackPoint = Clamp(Services.BlackPoint, 0f, 1f);
            uniforms.WhitePoint = Clamp(Services.WhitePoint, 0f, 5f);
            uniforms.GrayFormula = Services.SatFormula;
        }

        uniforms.ColorScale = Services.DitherBpc == -1 ? 255f : (1 << Services.DitherBpc) - 1;
        float scaleX = Services.ViewportWidth / (float)BufferWidth;
        float scaleY = Services.ViewportHeight / (float)BufferHeight;
        if (screenshot)
        {
            uniforms.ScaleX = scaleX;
            uniforms.ScaleY = scaleY;
            uniforms.OffsetX = 0f;
            uniforms.OffsetY = 0f;
        }
        else
        {
            uniforms.ScaleX = scaleX;
            uniforms.ScaleY = -scaleY;
            uniforms.OffsetX = 0f;
            uniforms.OffsetY = 1f;
        }

        if (applyGamma && Services.ColorSpace == ColorSpaceExtendedSrgbLinear && !screenshot)
            uniforms.HdrMode = 1;
        else
            uniforms.HdrMode = 0;

        draw.Uniforms = uniforms;
        draw.Steps.Add("uniforms");
        draw.Steps.Add("clear");
        draw.Shader = "present";
        draw.Steps.Add("shader");
        draw.Steps.Add("bind-uniforms");
        draw.Viewport = box;
        draw.Steps.Add("viewport");
        draw.InputFilter = Services.LinearScale ? FilterLinear : FilterNearest;
        draw.Steps.Add("input-current");
        draw.Dither = "dither";
        draw.DitherFilter = FilterNearest;
        draw.DitherWrap = WrapRepeat;
        draw.Steps.Add("input-dither");
        draw.Output = screenshot ? "next" : "swapchain";
        draw.Steps.Add(screenshot ? "output-next" : "output-swapchain");
        draw.NoBlend = true;
        draw.Steps.Add("noblend");
        draw.Steps.Add("draw");
        Presents.Add(draw);
    }

    public void AmbientOccludeScene(float m5)
    {
        Ssao.Add(new SsaoCall(m5, SceneWidth, SceneHeight));
        Trace.Add("ssao");
        ImageTransitionScene(false);
    }

    public void BlurScene(float amount)
    {
        int eyeCount = Services.EyeCount;
        for (int i = 0; i < eyeCount; i++)
        {
            Blooms.Add(new BloomCall(SceneWidth, SceneHeight, amount));
            if (eyeCount - i > 1)
                EyeAdvances.Add(eyeCount);
        }
    }

    public void ClearTonemapPalette()
    {
        TonemapClears++;
        Trace.Add("tonemap-clear");
    }

    public void UpdateShadowMap()
    {
        if (!Services.ShadowCollects)
            return;
        ShadowUpdates++;
        Trace.Add("shadow-update");
        Execute((Shadowmap, LayoutShaderRead, false));
        ShadowFinishes++;
        Trace.Add("shadow-finish");
    }

    static float Clamp(float value, float lo, float hi)
    {
        if (value < lo)
            return lo;
        if (value > hi)
            return hi;
        return value;
    }

    void EndPass()
    {
        Ends++;
        Trace.Add("end-pass");
    }

    void Execute(params (Image Image, int Layout, bool Undefined)[] images)
    {
        var batch = new TransitionBatch { Queue = "draw" };
        foreach ((Image image, int layout, bool undefined) in images)
        {
            image.Layout = layout;
            image.Undefined = undefined;
            batch.Steps.Add(new ImageStep(image.Name, layout, undefined));
        }

        Transitions.Add(batch);
        Trace.Add("transition");
    }

    void AddResolve(Image src, Image dst)
    {
        Regions.Add(new Region
        {
            Kind = "resolve",
            Src = src.Name,
            Dst = dst.Name,
            SrcWidth = src.Width,
            SrcHeight = src.Height,
            DstWidth = src.Width,
            DstHeight = src.Height,
            Depth = 1,
            Aspect = AspectColor,
            LayerCount = 1,
            SrcLayout = LayoutTransferSrc,
            DstLayout = LayoutTransferDst,
        });
        Trace.Add("resolve");
    }

    void AddBlit(Image src, Image dst, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
    {
        Regions.Add(new Region
        {
            Kind = "blit",
            Src = src.Name,
            Dst = dst.Name,
            SrcWidth = srcWidth,
            SrcHeight = srcHeight,
            DstWidth = dstWidth,
            DstHeight = dstHeight,
            Depth = 1,
            Filter = FilterNearest,
            Aspect = AspectColor,
            LayerCount = 1,
            SrcLayout = LayoutTransferSrc,
            DstLayout = LayoutTransferDst,
        });
        Trace.Add("blit");
    }
}
