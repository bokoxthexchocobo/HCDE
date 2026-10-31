using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Texture collection, sprite translations, and the model vertex buffers from
/// <c>hw_precache.cpp</c> (1-330). Chunk 1306.
/// </summary>
public sealed class VmHwPrecache1306
{
    public const int HitWall = 1, HitFlat = 2, HitSky = 4;
    public const int UfTexture = 1, UfSprite = 2;
    public const int CtfExpand = 1, CtfUpscale = 2;
    public const int Md3MaxSurfaces = 32;
    public const int UseFontChar = 9, UseNull = 13, UseSpecial = 15;

    readonly Host _host;

    public VmHwPrecache1306(Host host)
    {
        _host = host;
    }

    public static int ShouldUpscale(Tex tex, int use, int mask)
    {
        if ((mask & use) == 0)
            return 0;
        return tex.UpscaleFlag & 1;
    }

    public void Precache()
    {
        var all = new List<int>();
        var seen = new HashSet<int>();
        int count = _host.Textures.Length;
        for (int i = 1; i < count; i++)
        {
            Tex? game = _host.Textures[i];
            if (game != null && game.UseType != UseNull && game.HasImage && game.UseType != UseFontChar && game.UseType < UseSpecial)
            {
                foreach (int layer in game.Layers)
                {
                    Tex layerTex = _host.Textures[layer] ?? throw new InvalidOperationException("layer");
                    layerTex.CleanMarkers++;
                    if (seen.Add(layer))
                        all.Add(layer);
                }
            }
            if ((_host.Hits[i] & HitSky) != 0)
            {
                if (game == null)
                    throw new InvalidOperationException("sky texture");
                if (game.SkyFaces != null)
                {
                    for (int face = 0; face < 6; face++)
                    {
                        int index = game.SkyFaces[face];
                        if (index >= 0)
                            _host.Hits[index] |= HitFlat;
                    }
                }
            }
        }

        var spriteHits = new List<Dictionary<int, bool>>();
        for (int i = 0; i < _host.Sprites.Length; i++)
            spriteHits.Add(new Dictionary<int, bool>());
        var spriteList = new Dictionary<int, bool>?[count];
        var modelList = new byte[_host.Models.Length];

        foreach (Actor actor in _host.Actors)
        {
            int translation = actor.Remap ?? 0;
            foreach ((int sprite, int frame) in actor.States)
            {
                spriteHits[sprite][translation] = true;
                _host.Steps.Add("find:" + sprite + ":" + frame + ":0");
                if (!actor.Models.TryGetValue((sprite, frame), out ModelFrame? found))
                    found = null;
                if (found != null)
                {
                    for (int slot = 0; slot < found.ModelIds.Length; slot++)
                    {
                        int skin = found.Skins[slot];
                        int modelId = found.ModelIds[slot];
                        if (skin > 0)
                            _host.Hits[skin] |= HitFlat;
                        else if (modelId != -1)
                        {
                            int start = slot * Md3MaxSurfaces;
                            int[]? slice = (uint)start < (uint)found.SurfaceSkins.Length ? found.SurfaceSkins[start..] : null;
                            _host.Models[modelId].AddSkins(_host.Hits, slice);
                            _host.Steps.Add("add-skins:" + modelId + ":" + (slice == null ? "null" : slice.Length.ToString(CultureInfo.InvariantCulture)));
                        }
                        if (modelId != -1)
                            modelList[modelId] = 1;
                    }
                }
            }
        }

        for (int i = _host.Sprites.Length - 1; i >= 0; i--)
        {
            if (spriteHits[i].Count > 0)
            {
                foreach (int[] frame in _host.Sprites[i].Frames)
                {
                    for (int k = 0; k < 16; k++)
                    {
                        int pic = k < frame.Length ? frame[k] : -1;
                        if (pic > 0)
                            spriteList[pic] = spriteHits[i];
                    }
                }
            }
        }

        for (int i = 0; i < modelList.Length; i++)
        {
            if (modelList[i] == 0)
            {
                _host.Models[i].Destroyed++;
                _host.Steps.Add("destroy:" + i);
            }
        }

        _host.Steps.Add("start");
        for (int i = count - 1; i >= 0; i--)
        {
            Tex? tex = _host.Textures[i];
            if (tex == null)
                continue;
            if ((_host.Hits[i] & (HitWall | HitFlat | HitSky)) != 0)
            {
                int scale = 0;
                if (ShouldUpscale(tex, UfTexture, _host.UpscaleMask) != 0)
                    scale |= CtfUpscale;
                Material? material = _host.Validate(i, scale, true);
                MarkLayers(material, 0);
            }
            if (spriteList[i] != null && spriteList[i]!.Count > 0)
            {
                Material? material = _host.Validate(i, 1, true);
                foreach (int translation in spriteList[i]!.Keys)
                    MarkLayers(material, translation);
            }
        }

        foreach (int layer in all)
        {
            Tex layerTex = _host.Textures[layer] ?? throw new InvalidOperationException("layer");
            layerTex.CleanUnused++;
            _host.Steps.Add("clean-unused:" + layer);
        }

        if (_host.GlPrecache)
        {
            _host.Steps.Add("begin");
            for (int i = count - 1; i >= 0; i--)
            {
                Tex? game = _host.Textures[i];
                if (game == null || !game.HasImage)
                    continue;
                if ((_host.Hits[i] & (HitWall | HitFlat | HitSky)) != 0)
                {
                    int flags = ShouldUpscale(game, UfTexture, _host.UpscaleMask);
                    _host.Steps.Add("hw:" + i + ":" + flags);
                    if (game.Hardware == null)
                        _host.Steps.Add("register:" + i + ":" + (_host.TrueColor ? 1 : 0));
                }
                if (spriteList[i] != null && spriteList[i]!.ContainsKey(0))
                    _host.Steps.Add("register:" + i + ":" + (_host.TrueColor ? 1 : 0));
            }
            for (int i = count - 1; i >= 0; i--)
            {
                Tex? game = _host.Textures[i];
                if (game == null)
                    continue;
                PrecacheTexture(i, _host.Hits[i]);
                if (spriteList[i] != null && spriteList[i]!.Count > 0)
                    PrecacheSprite(i, spriteList[i]!);
            }
            _host.Steps.Add("end");
            _host.Steps.Add("renderer:-1");
            for (int i = 0; i < modelList.Length; i++)
            {
                if (modelList[i] != 0)
                {
                    _host.Models[i].Built++;
                    _host.Steps.Add("build:" + i);
                }
            }
            _host.Steps.Add("delete-renderer");
            _host.Prints.Add(FormatPrecache(_host.ElapsedMs));
        }

        _host.SpriteKeys = spriteHits.Select(hits => hits.Keys.ToArray()).ToList();
    }

    void PrecacheTexture(int index, int cache)
    {
        if ((cache & (HitWall | HitFlat | HitSky)) == 0)
            return;
        Tex tex = _host.Textures[index]!;
        int scale = 0;
        if (ShouldUpscale(tex, UfTexture, _host.UpscaleMask) != 0)
            scale |= CtfUpscale;
        Material? material = _host.Validate(index, scale, true);
        if (material != null)
            _host.Steps.Add("material:" + index + ":0");
    }

    void PrecacheSprite(int index, Dictionary<int, bool> hits)
    {
        Tex tex = _host.Textures[index]!;
        int scale = CtfExpand;
        if (ShouldUpscale(tex, UfSprite, _host.UpscaleMask) != 0)
            scale |= CtfUpscale;
        Material? material = _host.Validate(index, scale, true);
        if (material == null)
            return;
        foreach (int translation in hits.Keys)
            _host.Steps.Add("material:" + index + ":" + translation);
    }

    void MarkLayers(Material? material, int translation)
    {
        if (material == null)
            return;
        foreach (Layer layer in material.Layers)
        {
            if (layer.TextureId is int id)
            {
                Tex marked = _host.Textures[id] ?? throw new InvalidOperationException("layer");
                marked.Marks.Add((translation, layer.ScaleFlags));
                _host.Steps.Add("mark:" + id + ":" + translation + ":" + layer.ScaleFlags);
            }
        }
    }

    public static string FormatPrecache(double milliseconds)
    {
        return "Textures precached in " + milliseconds.ToString("0.000", CultureInfo.InvariantCulture) + " ms\n";
    }

    public sealed class Tex
    {
        public int UseType = 1;
        public bool HasImage = true;
        public int UpscaleFlag = 1;
        public int[] Layers = Array.Empty<int>();
        public int[]? SkyFaces;
        public object? Hardware;
        public int CleanMarkers, CleanUnused;
        public readonly List<(int Translation, int Flags)> Marks = new();
    }

    public sealed class Layer
    {
        public int? TextureId;
        public int ScaleFlags;
    }

    public sealed class Material
    {
        public Layer[] Layers = Array.Empty<Layer>();
    }

    public sealed class ModelFrame
    {
        public int[] Skins = Array.Empty<int>();
        public int[] ModelIds = Array.Empty<int>();
        public int[] SurfaceSkins = Array.Empty<int>();
    }

    public sealed class Actor
    {
        public int? Remap;
        public (int Sprite, int Frame)[] States = Array.Empty<(int, int)>();
        public readonly Dictionary<(int Sprite, int Frame), ModelFrame> Models = new();
    }

    public sealed class SpriteDef
    {
        public int[][] Frames = Array.Empty<int[]>();
    }

    public sealed class Model
    {
        public int Destroyed, Built;
        public readonly List<int[]?> SkinCalls = new();

        public void AddSkins(byte[] hits, int[]? skins)
        {
            SkinCalls.Add(skins);
            _ = hits;
        }
    }

    public sealed class Host
    {
        public Tex?[] Textures = Array.Empty<Tex?>();
        public byte[] Hits = Array.Empty<byte>();
        public SpriteDef[] Sprites = Array.Empty<SpriteDef>();
        public List<Actor> Actors = new();
        public Model[] Models = Array.Empty<Model>();
        public int UpscaleMask;
        public bool GlPrecache;
        public bool TrueColor;
        public double ElapsedMs;
        public readonly List<string> Steps = new();
        public readonly List<string> Prints = new();
        public List<int[]> SpriteKeys = new();

        public Material? Validate(int index, int scaleFlags, bool create)
        {
            Steps.Add("validate:" + index + ":" + scaleFlags + ":" + (create ? 1 : 0));
            return Materials.TryGetValue(index, out Material? material) ? material : new Material();
        }

        public readonly Dictionary<int, Material?> Materials = new();
    }
}
