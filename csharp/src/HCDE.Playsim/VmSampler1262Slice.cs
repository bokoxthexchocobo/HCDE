using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Hardware, postprocess, shadow, and lightmap samplers from <c>vk_samplers.cpp</c> (<b>1–190</b>).
/// Chunk <b>1262</b>. The filter and clamp tables are the ones in this file. A texture-filter
/// callback that returns true forces table row zero. Anisotropy is applied only when that row
/// wants mipmaps and the anisotropy callback does not disable it. The no-mip and camera slots
/// copy the magnification filter into the minification filter. The destructor body does not
/// hand samplers to the delete list.
/// </summary>
public sealed class VmSampler1262
{
    public const int FilterNearest = 0;
    public const int FilterLinear = 1;
    public const int MipNearest = 0;
    public const int MipLinear = 1;
    public const int Repeat = 0;
    public const int ClampEdge = 2;

    public const int ClampNone = 0;
    public const int ClampX = 1;
    public const int ClampY = 2;
    public const int ClampXy = 3;
    public const int ClampXyNoMip = 4;
    public const int ClampNoFilter = 5;
    public const int ClampNoFilterXy = 8;
    public const int ClampCam = 9;
    public const int SamplerCount = 10;

    public const int PpNearest = 0;
    public const int PpLinear = 1;
    public const int WrapClamp = 0;
    public const int WrapRepeat = 1;

    public readonly List<string> Log = new();
    public readonly Sampler?[] Hw = new Sampler?[SamplerCount];
    public readonly Sampler?[] Post = new Sampler?[4];
    public Sampler? Shadow;
    public Sampler? Light;
    int _nextId = 1;
    int _filter;
    bool _disableFilter;
    bool _disableAniso;
    float _aniso;

    readonly record struct FilterRow(int Min, int Mag, int Mip, bool Mips);
    readonly record struct ClampRow(int U, int V);

    static readonly FilterRow[] Filters =
    {
        new(FilterNearest, FilterNearest, MipNearest, false),
        new(FilterNearest, FilterNearest, MipNearest, true),
        new(FilterLinear, FilterLinear, MipNearest, false),
        new(FilterLinear, FilterLinear, MipNearest, true),
        new(FilterLinear, FilterLinear, MipLinear, true),
        new(FilterNearest, FilterNearest, MipLinear, true),
        new(FilterLinear, FilterNearest, MipLinear, true),
    };

    static readonly ClampRow[] Clamps =
    {
        new(Repeat, Repeat),
        new(ClampEdge, Repeat),
        new(Repeat, ClampEdge),
        new(ClampEdge, ClampEdge),
    };

    public static VmSampler1262 Create(int filter, bool disableFilter, bool disableAniso, float aniso)
    {
        var vm = new VmSampler1262();
        vm.CreateHw(filter, disableFilter, disableAniso, aniso);
        vm.CreateShadow();
        vm.CreateLight();
        return vm;
    }

    public void Reset()
    {
        DeleteHw();
        CreateHw(_filter, _disableFilter, _disableAniso, _aniso);
    }

    public void CreateHw(int filter, bool disableFilter, bool disableAniso, float aniso)
    {
        _filter = filter;
        _disableFilter = disableFilter;
        _disableAniso = disableAniso;
        _aniso = aniso;
        int row = disableFilter ? 0 : filter;
        bool anisoAvailable = !disableAniso;
        FilterRow spec = Filters[row];

        for (int i = ClampNone; i <= ClampXy; i++)
        {
            var sampler = Make("VkSamplerManager.mSamplers", spec.Mag, spec.Min, spec.Mip, Clamps[i].U, Clamps[i].V, Repeat);
            if (spec.Mips && anisoAvailable)
                EnableAniso(sampler, aniso, 100f);
            else
                SetLod(sampler, 0.25f);
            Hw[i] = sampler;
            Note("hw", i, sampler);
        }

        var nomip = Make("VkSamplerManager.mSamplers", spec.Mag, spec.Mag, MipNearest, ClampEdge, ClampEdge, Repeat);
        SetLod(nomip, 0.25f);
        Hw[ClampXyNoMip] = nomip;
        Note("hw", ClampXyNoMip, nomip);

        for (int i = ClampNoFilter; i <= ClampNoFilterXy; i++)
        {
            var sampler = Make(
                "VkSamplerManager.mSamplers",
                FilterNearest,
                FilterNearest,
                MipNearest,
                Clamps[i - ClampNoFilter].U,
                Clamps[i - ClampNoFilter].V,
                Repeat);
            SetLod(sampler, 0.25f);
            Hw[i] = sampler;
            Note("hw", i, sampler);
        }

        var camera = Make("VkSamplerManager.mSamplers", spec.Mag, spec.Mag, MipNearest, Repeat, Repeat, Repeat);
        SetLod(camera, 0.25f);
        Hw[ClampCam] = camera;
        Note("hw", ClampCam, camera);
    }

    public void DeleteHw()
    {
        for (int i = 0; i < Hw.Length; i++)
        {
            if (Hw[i] != null)
            {
                Log.Add($"drop {i} {Hw[i]!.Id}");
                Hw[i] = null;
            }
        }
    }

    public Sampler GetPost(int filter, int wrap)
    {
        int index = (filter << 1) | wrap;
        Sampler? existing = Post[index];
        if (existing != null)
            return existing;

        int mode = filter == PpNearest ? FilterNearest : FilterLinear;
        int address = wrap == WrapClamp ? ClampEdge : Repeat;
        var sampler = Make("VkPostprocess.mSamplers", mode, mode, MipNearest, address, address, address);
        Post[index] = sampler;
        Note("pp", index, sampler);
        return sampler;
    }

    public void CreateShadow()
    {
        Shadow = Make("VkRenderBuffers.ShadowmapSampler", FilterNearest, FilterNearest, MipNearest, ClampEdge, ClampEdge, ClampEdge);
        Note("shadow", 0, Shadow);
    }

    public void CreateLight()
    {
        Light = Make("VkRenderBuffers.LightmapSampler", FilterLinear, FilterLinear, MipLinear, ClampEdge, ClampEdge, ClampEdge);
        Note("light", 0, Light);
    }

    public void Destroy()
    {
        Log.Add("dtor");
    }

    public Sampler? HwAt(int index)
    {
        return Hw[index];
    }

    Sampler Make(string name, int mag, int min, int mip, int u, int v, int w)
    {
        return new Sampler
        {
            Id = _nextId++,
            Name = name,
            Mag = mag,
            Min = min,
            Mip = mip,
            U = u,
            V = v,
            W = w,
        };
    }

    static void EnableAniso(Sampler sampler, float aniso, float lod)
    {
        sampler.AnisoOn = true;
        sampler.Aniso = aniso;
        SetLod(sampler, lod);
    }

    static void SetLod(Sampler sampler, float lod)
    {
        sampler.Lod = lod;
        sampler.LodSet = true;
    }

    void Note(string kind, int index, Sampler sampler)
    {
        string aniso = sampler.AnisoOn
            ? Fmt(sampler.Aniso)
            : "off";
        string lod = sampler.LodSet ? Fmt(sampler.Lod) : "-";
        Log.Add($"{kind} {index} {sampler.Id} {sampler.Name} {sampler.Mag} {sampler.Min} {sampler.Mip} {sampler.U} {sampler.V} {sampler.W} {aniso} {lod}");
    }

    static string Fmt(float value)
    {
        return value.ToString("F6", CultureInfo.InvariantCulture);
    }

    public sealed class Sampler
    {
        public int Id;
        public string Name = "";
        public int Mag;
        public int Min;
        public int Mip;
        public int U;
        public int V;
        public int W;
        public bool AnisoOn;
        public float Aniso;
        public float Lod;
        public bool LodSet;
    }
}
