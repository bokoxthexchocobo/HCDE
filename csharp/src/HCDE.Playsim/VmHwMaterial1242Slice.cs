namespace HCDE.Playsim;

/// <summary>
/// Material shader selection, texture layers, and validate from <c>hw_material.cpp</c>
/// (<b>1–218</b>). Chunk <b>1242</b>. The destructor body is empty, so this slice has no
/// method for it. <c>CreateDefaultBrightmap</c> and <c>BestColor</c> live outside the file;
/// the constructor calls them before it chooses the brightmap layer.
/// </summary>
public sealed class VmHwMaterial1242
{
    public const int Expand = 1;
    public const int Indexed = 4;
    public const int ClampXy = 3;
    public const int ClampCam = 9;
    public const int ShaderDefault = 0;
    public const int ShaderSpecular = 3;
    public const int ShaderPbr = 4;
    public const int ShaderPaletted = 5;
    public const int FirstUserShader = 15;
    public const int BrightFlag = 0x10000;
    public const int DetailFlag = 0x20000;
    public const int GlowFlag = 0x40000;
    public const int UseDecal = 7;
    public const int UseMisc = 8;
    public const int UseFont = 9;
    public const int UseNull = 13;
    public const int UseSwCanvas = 16;

    public readonly List<string> Log = new();
    public bool CustomShader = true;
    public readonly List<int> UserTypes = new();
    public readonly Dictionary<int, Tex> Games = new();
    public byte[]? PalR;
    public byte[]? PalG;
    public byte[]? PalB;
    public byte[]? IndexMap;

    int _nextTex = 1;
    int _nextMat = 1;
    Func<int, int, int>? _callback;

    public sealed class Tex
    {
        public int Id;
        public int Use = 1;
        public int ColorFormat = 1;
        public bool HardwareCanvas;
        public int Warped;
        public int ShaderIndex;
        public bool ExpandSprites = true;
        public Tex? Bright;
        public bool BrightChecked;
        public bool HasImage = true;
        public bool UseGamePalette = true;
        public bool HasGlobalBrightmap;
        public byte[]? Pixels;
        public byte[]? Remap;
        public Tex? Normal;
        public Tex? Specular;
        public Tex? Metallic;
        public Tex? Roughness;
        public Tex? Ao;
        public Tex? Detail;
        public Tex? Glow;
        public bool HasLayers;
        public readonly List<Tex?> Custom = new();
        public readonly Mat?[] Material = new Mat?[5];
        public bool Valid => Use != UseNull;
    }

    public sealed class Layer
    {
        public Tex? Tex;
        public int Scale;
        public int Clamp;
    }

    public sealed class Mat
    {
        public int Id;
        public Tex? Source;
        public int Shader;
        public int LayerFlags;
        public int ScaleFlags;
        public readonly List<Layer> Layers = new();
    }

    public Tex NewTex(int use = 1)
    {
        var tex = new Tex { Id = _nextTex, Use = use };
        _nextTex++;
        return tex;
    }

    public void SetCallback(Func<int, int, int>? callback)
    {
        _callback = callback;
        Log.Add(callback == null ? "callback null" : "callback set");
    }

    public int PickWhite()
    {
        if (PalR == null || PalG == null || PalB == null)
        {
            Log.Add("pick white=1");
            return 1;
        }

        int first = IndexMap != null ? 0 : 1;
        int best = first;
        int bestdist = 257 * 257 + 257 * 257 + 257 * 257;
        for (int color = first; color < 255; color++)
        {
            int co = IndexMap != null ? IndexMap[color] : color;
            int x = 255 - PalR[co];
            int y = 255 - PalG[co];
            int z = 255 - PalB[co];
            int dist = x * x + y * y + z * z;
            if (dist < bestdist)
            {
                if (dist == 0)
                {
                    Log.Add("pick white=" + co);
                    return co;
                }

                bestdist = dist;
                best = co;
            }
        }

        Log.Add("pick white=" + best);
        return best;
    }

    public Mat? Validate(Tex? tex, int scaleflags, bool create)
    {
        if (tex == null || !tex.Valid)
        {
            Log.Add(tex == null ? "validate null" : "validate invalid");
            return null;
        }

        int flags = scaleflags;
        if ((flags & Indexed) != 0)
            flags = Indexed;
        if (!tex.ExpandSprites)
            flags &= ~Expand;
        if (flags < 0 || flags > 4)
        {
            Log.Add("validate oob flags=" + flags);
            return null;
        }

        Mat? hw = tex.Material[flags];
        if (hw == null && create)
            hw = Build(tex, flags);
        Log.Add("validate flags=" + flags + " id=" + (hw == null ? -1 : hw.Id));
        return hw;
    }

    public int GetLayer(Mat mat, int i, int translation, out Layer? info)
    {
        info = null;
        if ((mat.ScaleFlags & Indexed) != 0 && i > 0 && _callback != null)
        {
            if (i == 1 || i == 2)
            {
                info = new Layer { Tex = null, Scale = 0, Clamp = ClampXy };
                int hw = _callback(i, translation);
                Log.Add("layer i=" + i + " callback trans=" + translation + " clamp=3 hw=" + hw);
                return hw;
            }

            Log.Add("layer i=" + i + " indexed none");
            return -1;
        }

        if (i < 0 || i >= mat.Layers.Count)
        {
            Log.Add("layer oob i=" + i);
            return -1;
        }

        Layer layer = mat.Layers[i];
        info = layer;
        int trans = translation;
        if ((mat.ScaleFlags & Indexed) != 0)
            trans = -1;
        if (layer.Tex == null)
        {
            Log.Add("layer i=" + i + " empty trans=" + trans);
            return -1;
        }

        Log.Add("layer i=" + i + " trans=" + trans + " tex=" + layer.Tex.Id + " scale=" + layer.Scale + " hw=" + layer.Tex.Id);
        return layer.Tex.Id;
    }

    public void Delete(Mat mat)
    {
        int slot = -1;
        if (mat.Source != null)
        {
            for (int i = 0; i < mat.Source.Material.Length; i++)
            {
                if (mat.Source.Material[i] == mat)
                    slot = i;
            }
        }

        Log.Add("delete id=" + mat.Id + " slot=" + slot);
    }

    public Mat Build(Tex tx, int scaleflags)
    {
        var mat = new Mat
        {
            Id = _nextMat,
            Source = tx,
            Shader = ShaderDefault,
            ScaleFlags = scaleflags
        };
        _nextMat++;
        mat.Layers.Add(LayerOf(tx, scaleflags, -1));
        if (tx.Use == UseSwCanvas && tx.ColorFormat == 0)
        {
            mat.Shader = ShaderPaletted;
        }
        else if ((scaleflags & Indexed) != 0)
        {
            mat.Layers[0].Scale |= Indexed;
            mat.Shader = ShaderPaletted;
        }
        else if (tx.HardwareCanvas)
        {
            if (tx.ShaderIndex >= FirstUserShader)
                mat.Shader = tx.ShaderIndex;
            mat.Layers[^1].Clamp = ClampCam;
        }
        else
        {
            if (tx.Warped != 0)
                mat.Shader = tx.Warped;
            else if (tx.HasLayers && tx.Normal != null && tx.Specular != null)
            {
                mat.Layers.Add(LayerOf(tx.Normal, 0, -1));
                mat.Layers.Add(LayerOf(tx.Specular, 0, -1));
                mat.Shader = ShaderSpecular;
            }
            else if (tx.HasLayers && tx.Normal != null && tx.Metallic != null && tx.Roughness != null && tx.Ao != null)
            {
                mat.Layers.Add(LayerOf(tx.Normal, 0, -1));
                mat.Layers.Add(LayerOf(tx.Metallic, 0, -1));
                mat.Layers.Add(LayerOf(tx.Roughness, 0, -1));
                mat.Layers.Add(LayerOf(tx.Ao, 0, -1));
                mat.Shader = ShaderPbr;
            }

            DefaultBright(tx);
            Tex? placeholder = Games.TryGetValue(1, out Tex? game) ? game : null;
            if (tx.Bright != null)
            {
                mat.Layers.Add(LayerOf(tx.Bright, scaleflags, -1));
                mat.LayerFlags |= BrightFlag;
            }
            else
            {
                NoteMissing(placeholder);
                mat.Layers.Add(LayerOf(placeholder, 0, -1));
            }

            if (tx.HasLayers && tx.Detail != null)
            {
                mat.Layers.Add(LayerOf(tx.Detail, 0, 0));
                mat.LayerFlags |= DetailFlag;
            }
            else
            {
                NoteMissing(placeholder);
                mat.Layers.Add(LayerOf(placeholder, 0, -1));
            }

            if (tx.HasLayers && tx.Glow != null)
            {
                mat.Layers.Add(LayerOf(tx.Glow, scaleflags, -1));
                mat.LayerFlags |= GlowFlag;
            }
            else
            {
                NoteMissing(placeholder);
                mat.Layers.Add(LayerOf(placeholder, 0, -1));
            }

            if (CustomShader && tx.ShaderIndex >= FirstUserShader)
                ApplyUser(mat, tx);
        }

        if (scaleflags >= 0 && scaleflags < tx.Material.Length)
            tx.Material[scaleflags] = mat;
        else
            Log.Add("slot oob flags=" + scaleflags);
        Log.Add("mat id=" + mat.Id + " shader=" + mat.Shader + " flags=" + mat.LayerFlags + " scale=" + mat.ScaleFlags + " n=" + mat.Layers.Count + " slot=" + SlotOf(tx, mat));
        for (int i = 0; i < mat.Layers.Count; i++)
        {
            Layer layer = mat.Layers[i];
            int id = layer.Tex == null ? 0 : layer.Tex.Id;
            Log.Add("L i=" + i + " tex=" + id + " scale=" + layer.Scale + " clamp=" + layer.Clamp);
        }

        return mat;
    }

    void ApplyUser(Mat mat, Tex tx)
    {
        int index = tx.ShaderIndex - FirstUserShader;
        if (index < 0 || index >= UserTypes.Count)
        {
            Log.Add("shader oob " + tx.ShaderIndex);
            return;
        }

        if (UserTypes[index] != mat.Shader)
        {
            Log.Add("user skip type=" + UserTypes[index] + " have=" + mat.Shader);
            return;
        }

        if (tx.HasLayers)
        {
            foreach (Tex? texture in tx.Custom)
            {
                if (texture == null)
                    continue;
                mat.Layers.Add(LayerOf(texture, 0, 0));
            }
        }

        mat.Shader = tx.ShaderIndex;
        Log.Add("user shader=" + tx.ShaderIndex);
    }

    void DefaultBright(Tex tx)
    {
        if (tx.BrightChecked)
        {
            Log.Add("brightmap checked");
            return;
        }

        tx.BrightChecked = true;
        if (!(tx.HasImage && tx.UseGamePalette && tx.HasGlobalBrightmap &&
              tx.Use != UseDecal && tx.Use != UseMisc && tx.Use != UseFont &&
              tx.Bright == null))
        {
            Log.Add("brightmap skip");
            return;
        }

        if (tx.Pixels == null || tx.Remap == null)
        {
            Log.Add("brightmap pixels missing");
            return;
        }

        int white = PickWhite();
        for (int i = 0; i < tx.Pixels.Length; i++)
        {
            int pix = tx.Pixels[i];
            if (pix >= tx.Remap.Length)
            {
                Log.Add("remap oob " + pix);
                return;
            }

            if (tx.Remap[pix] == white)
            {
                tx.Bright = new Tex { Id = _nextTex, Use = 1 };
                _nextTex++;
                Log.Add("brightmap created id=" + tx.Bright.Id);
                return;
            }
        }

        Log.Add("brightmap none");
    }

    static Layer LayerOf(Tex? tex, int scale, int clamp)
    {
        return new Layer { Tex = tex, Scale = scale, Clamp = clamp };
    }

    void NoteMissing(Tex? placeholder)
    {
        if (placeholder == null)
            Log.Add("placeholder missing");
    }

    static int SlotOf(Tex tx, Mat mat)
    {
        for (int i = 0; i < tx.Material.Length; i++)
        {
            if (tx.Material[i] == mat)
                return i;
        }

        return -1;
    }
}
