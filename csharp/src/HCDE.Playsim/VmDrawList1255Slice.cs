namespace HCDE.Playsim;

/// <summary>
/// Hardware draw-list routing from <c>hw_drawlistadd.cpp</c> (<b>1–153</b>).
/// Chunk <b>1255</b>. Pass flags, the segment normal, and vertex build are the helpers those calls use.
/// Dynamic lights and decals are host steps on the mirror copy.
/// </summary>
public sealed class VmDrawList1255
{
    public const int PlainWalls = 0;
    public const int PlainFlats = 1;
    public const int MaskedWalls = 2;
    public const int MaskedFlats = 3;
    public const int MaskedWallsOfs = 4;
    public const int Models = 5;
    public const int Translucent = 6;
    public const int TranslucentBorder = 7;

    public const int RenderNone = 0;
    public const int RenderTop = 1;
    public const int RenderM1S = 2;
    public const int RenderM2S = 3;
    public const int RenderBottom = 4;
    public const int RenderFogBoundary = 5;
    public const int RenderMirror = 6;
    public const int RenderM2Snf = 7;
    public const int RenderColor = 8;
    public const int RenderFfBlock = 9;

    public const int SkyHack = 4;
    public const int TranslucentFlag = 128;
    public const int Render3dPlanes = 4;
    public const int StyleTranslucent = 6;
    public const int SpriteTypeMask = 0x7000;
    public const int WallSprite = 0x1000;
    public const float FltEpsilon = 1.1920928955078125e-7f;

    public static readonly int[] PassFlag = { 0, 1, 1, 2, 1, 3, 1, 2, 3, 2 };

    public readonly List<string> Log = new();
    public readonly List<Wall>[] Walls = new List<Wall>[8];
    public readonly List<Flat>[] Flats = new List<Flat>[8];
    public readonly List<Sprite>[] Sprites = new List<Sprite>[8];
    public bool HasDynamicLights = true;
    public bool FullBright;

    public VmDrawList1255()
    {
        for (int i = 0; i < 8; i++)
        {
            Walls[i] = new();
            Flats[i] = new();
            Sprites[i] = new();
        }
    }
    public void AddWall(Wall wall)
    {
        int list;
        if ((wall.Flags & TranslucentFlag) != 0)
        {
            list = Translucent;
        }
        else
        {
            bool masked = PassFlag[wall.Type] == 1 ? false : wall.Texture != null && wall.Texture.Masked;
            if ((wall.Flags & SkyHack) != 0 && wall.Type == RenderM2S)
                list = MaskedWallsOfs;
            else
                list = masked ? MaskedWalls : PlainWalls;
        }
        Wall copy = wall.Copy();
        Walls[list].Add(copy);
        Log.Add($"wall list={list} type={copy.Type} maskedtex={(wall.Texture != null && wall.Texture.Masked ? 1 : 0)}");
    }
    public void AddMirror(Wall wall)
    {
        wall.Type = RenderMirror;
        Wall copy = wall.Copy();
        copy.VertCount = 0;
        (float x, float y, float z) = Normal(copy.X1, copy.Y1, copy.X2, copy.Y2);
        for (int i = 0; i < 4; i++)
        {
            copy.U[i] = x;
            copy.V[i] = z;
        }
        Log.Add($"normal {Fmt(x)} {Fmt(y)} {Fmt(z)}");
        Log.Add("makevertices 0");
        bool decals = copy.HasDecals;
        if (decals && HasDynamicLights && !FullBright)
        {
            copy.DynLightIndex = 5;
            Log.Add("lights");
        }
        Log.Add("decals");
        copy.DynLightIndex = -1;
        Walls[TranslucentBorder].Add(copy);
        Log.Add($"mirror list={TranslucentBorder} vert={copy.VertCount} light={copy.DynLightIndex} u={Fmt(copy.U[0])} v={Fmt(copy.V[0])}");
    }

    public void AddFlat(Flat flat, bool fog)
    {
        int list;
        bool missing = flat.Texture == null;
        if (flat.RenderStyle != StyleTranslucent || flat.Alpha < 1f - FltEpsilon || fog || missing)
        {
            list = (flat.RenderFlags & Render3dPlanes) != 0 ? Translucent : TranslucentBorder;
        }
        else if (flat.Texture!.Translucent)
        {
            if (flat.Stack)
                list = TranslucentBorder;
            else if ((flat.RenderFlags & Render3dPlanes) != 0 && !flat.Slope)
                list = Translucent;
            else
                list = PlainFlats;
        }
        else
        {
            bool masked = flat.Texture.Masked && ((flat.RenderFlags & Render3dPlanes) != 0 || flat.Stack);
            list = masked ? MaskedFlats : PlainFlats;
        }
        Flats[list].Add(flat.Copy());
        Log.Add($"flat list={list} fog={(fog ? 1 : 0)} alpha={Fmt(flat.Alpha)}");
    }

    public void AddSprite(Sprite sprite, bool translucent)
    {
        int list;
        if (translucent || sprite.Actor == null || (!sprite.ModelFrame && (sprite.RenderFlags & SpriteTypeMask) != WallSprite))
            list = Translucent;
        else
            list = Models;
        Sprites[list].Add(sprite.Copy());
        Log.Add($"sprite list={list} model={(sprite.ModelFrame ? 1 : 0)} flags={sprite.RenderFlags}");
    }

    public static (float X, float Y, float Z) Normal(float x1, float y1, float x2, float y2)
    {
        float x = y2 - y1;
        float y = x1 - x2;
        float ilength = 1f / MathF.Sqrt(x * x + y * y);
        return (x * ilength, 0f, y * ilength);
    }

    public static string Fmt(float value)
    {
        return value.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
    }

    public sealed class Texture
    {
        public bool Masked;
        public bool Translucent;
    }

    public sealed class Wall
    {
        public int Flags;
        public int Type;
        public Texture? Texture;
        public int VertCount = 4;
        public bool HasDecals;
        public int DynLightIndex;
        public float X1, Y1, X2, Y2;
        public float[] U = new float[4];
        public float[] V = new float[4];

        public Wall Copy()
        {
            return new Wall
            {
                Flags = Flags,
                Type = Type,
                Texture = Texture,
                VertCount = VertCount,
                HasDecals = HasDecals,
                DynLightIndex = DynLightIndex,
                X1 = X1, Y1 = Y1, X2 = X2, Y2 = Y2,
                U = (float[])U.Clone(),
                V = (float[])V.Clone(),
            };
        }
    }

    public sealed class Flat
    {
        public int RenderStyle = StyleTranslucent;
        public float Alpha = 1f;
        public Texture? Texture;
        public int RenderFlags;
        public bool Stack;
        public bool Slope;

        public Flat Copy()
        {
            return new Flat
            {
                RenderStyle = RenderStyle,
                Alpha = Alpha,
                Texture = Texture,
                RenderFlags = RenderFlags,
                Stack = Stack,
                Slope = Slope,
            };
        }
    }

    public sealed class Sprite
    {
        public Actor? Actor = new();
        public bool ModelFrame;
        public int RenderFlags;

        public Sprite Copy()
        {
            return new Sprite
            {
                Actor = Actor,
                ModelFrame = ModelFrame,
                RenderFlags = RenderFlags,
            };
        }
    }

    public sealed class Actor
    {
    }
}
