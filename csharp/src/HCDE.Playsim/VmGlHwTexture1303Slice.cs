namespace HCDE.Playsim;

/// <summary>
/// GL texture upload, the unpack buffer, and the sampler bind from
/// <c>gl_hwtexture.cpp</c> (1-331). Chunk 1303.
/// </summary>
public sealed class VmGlHwTexture1303
{
    public const int MaxUnits = 16;
    public const int GlNearest = 0x2600, GlLinear = 0x2601, GlNearestMipNearest = 0x2700;
    public const int GlLinearMipNearest = 0x2701, GlNearestMipLinear = 0x2702, GlLinearMipLinear = 0x2703;
    public const int GlTexture2D = 0x0DE1, GlTexture0 = 0x84C0, GlRgba8 = 0x8058;
    public const int GlR8 = 0x8229, GlRg8 = 0x822B, GlRgb8 = 0x8051;
    public const int GlRed = 0x1903, GlRg = 0x8227, GlBgr = 0x80E0, GlBgra = 0x80E1;
    public const int GlUnsignedByte = 0x1401, GlTexture = 0x1702;
    public const int GlPixelUnpack = 0x88EC, GlStreamDraw = 0x88E0, GlWriteOnly = 0x88B9;
    public const int GlRenderbuffer = 0x8D41, GlDepth24Stencil8 = 0x88F0;
    public const int GlFramebuffer = 0x8D40, GlColor0 = 0x8CE0, GlDepthAttach = 0x8D00, GlStencilAttach = 0x8D20;
    public const int ClampXy = 3, ClampNoFilter = 5, CtfIndexed = 4, CtfProcessData = 16;
    public static readonly int[] LastBound = new int[MaxUnits];
    public readonly record struct Filter(int Min, int Mag, bool Mip);
    public static readonly Filter[] Filters =
    {
        new(GlNearest, GlNearest, false),
        new(GlNearestMipNearest, GlNearest, true),
        new(GlLinear, GlLinear, false),
        new(GlLinearMipNearest, GlLinear, true),
        new(GlLinearMipLinear, GlLinear, true),
        new(GlNearestMipLinear, GlNearest, true),
        new(GlLinearMipLinear, GlNearest, true),
    };
    static readonly int[] InternalByBytes = { GlR8, GlRg8, GlRgb8, GlRgba8 };
    static readonly int[] SourceByBytes = { GlRed, GlRg, GlBgr, GlBgra };

    public sealed class Host
    {
        public int MaxTextureSize = 4096;
        public int TextureFilter = 6;
        public int BoundTexture2D;
        public bool DebugApi;
        public int DebugLevel;
        public int NextTex = 1, NextBuffer = 1, NextDepth = 1;
        public int DeletedTextures, DeletedBuffers, Mipmaps;
        public List<int> Stores = new();
        public List<Upload> Uploads = new();
        public List<(int Type, int Id, string? Name)> Labels = new();
        public List<string> Steps = new();
        public List<(int Unit, int Clamp, int Light)> Samplers = new();
        public List<(int Target, int Id)> Framebuffers = new();
        public void Active(int target) => Steps.Add("active:" + target);
        public void BindTex(int id) => Steps.Add("bind:" + id);
        public void BindPbo(int id) => Steps.Add("pbo:" + id);
    }

    public sealed record Upload(bool Sub, int Width, int Height, int InternalFormat, int Source, byte[]? Pixels, bool Freed, bool FromPbo);
    public sealed class Tex
    {
        public int Width, Height;
        public byte[]? Pixels;
        public bool HardwareCanvas;
    }

    readonly Host _host;
    public int TexId, DepthId, BufferId, TextureBytes, BufferPitch;
    public bool ForceNoFilter, Mipmapped;
    public string? LastName;
    public byte[]? Mapped;
    public int DepthWidth, DepthHeight;
    public List<(int Translation, int Flags)> Requests = new();

    public VmGlHwTexture1303(Host host, int channels = 4, bool disableFilter = false)
    {
        _host = host;
        ForceNoFilter = disableFilter;
        TextureBytes = channels;
    }

    public int Dimension(int value) => value > _host.MaxTextureSize ? _host.MaxTextureSize : value;

    public static void Resize(int sw, int sh, int dw, int dh, byte[] src, byte[] dst)
    {
        (int Start, int End)[] rows = Boxes(dh, sh), cols = Boxes(dw, sw);
        int at = 0;
        for (int y = 0; y < dh; y++)
        {
            for (int x = 0; x < dw; x++)
            {
                double sr = 0, sg = 0, sb = 0, sa = 0;
                int pixels = 0, alphas = 0;
                for (int j = rows[y].Start; j <= rows[y].End; j++)
                {
                    for (int i = cols[x].Start; i <= cols[x].End; i++)
                    {
                        int p = (j * sw + i) * 4;
                        int a = src[p + 3];
                        if (a > 0)
                        {
                            sr += src[p];
                            sg += src[p + 1];
                            sb += src[p + 2];
                            sa += a;
                            pixels++;
                        }
                        alphas++;
                    }
                }
                dst[at++] = pixels == 0 ? (byte)0 : Even(sr / pixels);
                dst[at++] = pixels == 0 ? (byte)0 : Even(sg / pixels);
                dst[at++] = pixels == 0 ? (byte)0 : Even(sb / pixels);
                dst[at++] = alphas == 0 ? (byte)0 : Even(sa / alphas);
            }
        }
    }

    public uint CreateTexture(byte[]? buffer, int w, int h, int unit, bool mipmap, string? name)
    {
        LastName = name;
        int format = GlRgba8;
        bool deleteBuffer = false, fromPbo = false, first = TexId == 0;
        byte[]? shot = null;
        if (first)
        {
            TexId = _host.NextTex++;
            _host.Steps.Add("gen-tex:" + TexId);
        }
        int saved = _host.BoundTexture2D;
        if (unit == -1) _host.Steps.Add("get-binding:" + saved);
        if (unit > 0) _host.Active(GlTexture0 + unit);
        if (unit >= 0) LastBound[unit] = TexId;
        _host.BindTex(TexId);
        if (_host.DebugApi && _host.DebugLevel != 0)
        {
            _host.Labels.Add((GlTexture, TexId, name));
            _host.Steps.Add($"label:{GlTexture}:{TexId}:{name}");
        }
        int rw = Dimension(w), rh = Dimension(h);
        if (BufferId > 0)
        {
            _host.Steps.Add("unmap");
            shot = Mapped == null ? null : (byte[])Mapped.Clone();
            buffer = null;
            fromPbo = true;
        }
        else if (buffer == null)
        {
            Mipmapped = false;
            buffer = new byte[4 * rw * (rh + 1)];
            deleteBuffer = true;
            _host.Steps.Add("calloc:" + buffer.Length);
        }
        else if (rw < w || rh < h)
        {
            var scaled = new byte[4 * rw * (rh + 1)];
            Resize(w, h, rw, rh, buffer, scaled);
            buffer = scaled;
            deleteBuffer = true;
            _host.Steps.Add($"resize:{w}x{h}->{rw}x{rh}");
        }
        int source;
        if (TextureBytes > 0)
        {
            if (TextureBytes < 4)
            {
                _host.Stores.Add(1);
                _host.Steps.Add("align:1");
            }
            format = InternalByBytes[TextureBytes - 1];
            source = SourceByBytes[TextureBytes - 1];
        }
        else source = GlBgra;
        bool sub = !first && BufferId > 0;
        _host.Uploads.Add(new Upload(sub, rw, rh, format, source, fromPbo ? shot : buffer, deleteBuffer, fromPbo));
        _host.Steps.Add(sub ? "tex-sub" : "tex-image");
        if (deleteBuffer) _host.Steps.Add("free");
        else if (BufferId != 0)
        {
            _host.Stores.Add(4);
            _host.Steps.Add("align:4");
            _host.BindPbo(0);
        }
        if (mipmap && Filters[_host.TextureFilter].Mip)
        {
            _host.Mipmaps++;
            Mipmapped = true;
            _host.Steps.Add("mipmap");
        }
        if (unit > 0) _host.Active(GlTexture0);
        else if (unit == -1)
        {
            _host.BindTex(saved);
            _host.Steps.Add("restore:" + saved);
        }
        return (uint)TexId;
    }

    public void AllocateBuffer(int w, int h, int texelSize)
    {
        int rw = Dimension(w), rh = Dimension(h);
        if (texelSize < 1 || texelSize > 4) texelSize = 4;
        TextureBytes = texelSize;
        BufferPitch = w;
        if (rw != w && rh != h) return;
        BufferId = _host.NextBuffer++;
        Mapped = new byte[w * h * texelSize];
        _host.Steps.Add("gen-buf:" + BufferId);
        _host.BindPbo(BufferId);
        _host.Steps.Add($"buffer-data:{Mapped.Length}:{GlStreamDraw}");
        _host.BindPbo(0);
    }

    public byte[]? MapBuffer()
    {
        _host.BindPbo(BufferId);
        _host.Steps.Add("map:" + GlWriteOnly);
        return BufferId == 0 ? null : Mapped;
    }

    public void Destroy()
    {
        if (TexId != 0)
        {
            _host.DeletedTextures++;
            _host.Steps.Add("del-tex:" + TexId);
            TexId = 0;
        }
        if (BufferId != 0)
        {
            _host.DeletedBuffers++;
            _host.Steps.Add("del-buf:" + BufferId);
            BufferId = 0;
        }
    }

    public uint Bind(int unit, bool needMip)
    {
        if (TexId == 0) return 0;
        if (LastBound[unit] == TexId) return (uint)TexId;
        LastBound[unit] = TexId;
        if (unit != 0) _host.Active(GlTexture0 + unit);
        _host.BindTex(TexId);
        if (needMip && !Mipmapped && Filters[_host.TextureFilter].Mip)
        {
            _host.Mipmaps++;
            Mipmapped = true;
            _host.Steps.Add("mipmap");
        }
        if (unit != 0) _host.Active(GlTexture0);
        return (uint)TexId;
    }

    public static void Unbind(Host host, int unit)
    {
        if (LastBound[unit] == 0) return;
        if (unit != 0) host.Active(GlTexture0 + unit);
        host.BindTex(0);
        if (unit != 0) host.Active(GlTexture0);
        LastBound[unit] = 0;
    }

    public static void UnbindAll(Host host)
    {
        for (int unit = 0; unit < MaxUnits; unit++) Unbind(host, unit);
    }

    public int GetDepthBuffer(int width, int height)
    {
        if (DepthId == 0)
        {
            DepthId = _host.NextDepth++;
            DepthWidth = Dimension(width);
            DepthHeight = Dimension(height);
            _host.Steps.Add("gen-rb:" + DepthId);
            _host.Steps.Add("rb:" + DepthId);
            _host.Steps.Add($"storage:{GlDepth24Stencil8}:{DepthWidth}x{DepthHeight}");
            _host.Steps.Add("rb:0");
        }
        return DepthId;
    }

    public void BindToFrameBuffer(int width, int height)
    {
        width = Dimension(width);
        height = Dimension(height);
        _host.Framebuffers.Add((GlColor0, TexId));
        _host.Steps.Add($"fb-tex:{GlColor0}:{TexId}");
        int depth = GetDepthBuffer(width, height);
        _host.Framebuffers.Add((GlDepthAttach, depth));
        _host.Steps.Add($"fb-rb:{GlDepthAttach}:{depth}");
        int stencil = GetDepthBuffer(width, height);
        _host.Framebuffers.Add((GlStencilAttach, stencil));
        _host.Steps.Add($"fb-rb:{GlStencilAttach}:{stencil}");
    }

    public bool BindOrCreate(Tex tex, int unit, int clamp, int translation, int flags)
    {
        bool need = clamp <= ClampXy && !ForceNoFilter;
        if (Bind(unit, need) == 0)
        {
            if ((flags & CtfIndexed) != 0)
            {
                TextureBytes = 1;
                ForceNoFilter = true;
                need = false;
            }
            byte[]? pixels;
            int w, h;
            if (!tex.HardwareCanvas)
            {
                Requests.Add((translation, flags | CtfProcessData));
                w = tex.Width;
                h = tex.Height;
                pixels = tex.Pixels;
            }
            else
            {
                w = tex.Width;
                h = tex.Height;
                pixels = null;
            }
            if (CreateTexture(pixels, w, h, unit, need, "FHardwareTexture.BindOrCreate") == 0)
                return false;
        }
        if (ForceNoFilter && clamp <= ClampXy) clamp += ClampNoFilter;
        _host.Samplers.Add((unit, clamp, 255));
        _host.Steps.Add($"sampler:{unit}:{clamp}:255");
        return true;
    }

    static (int Start, int End)[] Boxes(int newDim, int oldDim)
    {
        var boxes = new (int Start, int End)[newDim];
        double scale = (double)oldDim / newDim;
        int half = (int)(scale / 2);
        for (int dst = 0; dst < newDim; dst++)
        {
            int src = (int)(dst * scale);
            int start = Clamp((int)(src - scale / 2.0 + 1), 0, oldDim - 1);
            boxes[dst] = (start, Clamp(Math.Max(start + 1, src + half), 0, oldDim - 1));
        }
        return boxes;
    }

    static int Clamp(int value, int lo, int hi) => value < lo ? lo : value > hi ? hi : value;
    static byte Even(double value) => (byte)Convert.ToInt32(Math.Round(value, MidpointRounding.ToEven));
}
