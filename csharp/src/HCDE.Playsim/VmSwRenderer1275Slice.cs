namespace HCDE.Playsim;

/// <summary>
/// Software renderer precache, camera views, and the default colormap from
/// <c>r_swrenderer.cpp</c> (1-271). Chunk 1275. Shade maps, texture pixels, and
/// the scene draw are host inputs. Sprite hits are painted from the last sprite
/// downward. A flat stays out of the column fetch. A missing camera texture
/// returns before the saved light and field of view are touched.
/// </summary>
public sealed class VmSwSprite1275
{
    public int NumFrames;
    public int FrameIndex;
}

public sealed class VmSwFrame1275
{
    public readonly bool[] Valid = new bool[16];
    public readonly int[] Index = new int[16];
}

public sealed class VmSwClass1275
{
    public int[] StateSprites = [];
}

public sealed class VmSwTex1275
{
    public bool Valid = true;
    public bool SoftwareCanvas;
    public bool CheckPixels;
}

public sealed class VmSwCam1275
{
    public int Id;
    public bool Present = true;
    public int Width;
    public int Height;
    public bool FirstUpdate;
    public int Actor;
    public double Fov;
}

public sealed class VmSwRenderer1275
{
    public const int HitSprite = 8;
    public const int HitColumn = 13;
    public const int SavePal = 0;
    public bool UserGame;
    public bool OldFreelook;
    public bool TrueColorValue;
    public int TrueColorReads;
    public int Pitch;
    public int Window;
    public int Sector;
    public double Fov = 90;
    public int ScenePitchAfter;
    public int SceneWindowAfter;
    public int SavePitchAfter;
    public int SaveWindowAfter;
    public int RemainPitchAfter;
    public int RemainWindowAfter;
    public int CameraPitchAfter;
    public int CameraWindowAfter;
    public bool RenderTargetBgra;
    public int LightToken = 1;
    public readonly List<VmSwSprite1275> Sprites = new();
    public readonly List<VmSwFrame1275> Frames = new();
    public readonly List<VmSwTex1275?> Textures = new();
    public readonly List<string> Log = new();

    public void SetOldFreelook(bool value)
    {
        OldFreelook = value;
        if (UserGame)
            Log.Add("pitch " + (value ? "1" : "0"));
    }

    public void Init()
    {
        Log.Add("shade");
        Log.Add("colormaps");
    }

    public void Create()
    {
        Log.Add("create");
    }

    bool ReadTrueColor()
    {
        TrueColorReads++;
        return TrueColorValue;
    }

    public void Prepare(VmSwTex1275? tex, int cache)
    {
        _ = ReadTrueColor();
        if (tex == null || !tex.Valid || tex.SoftwareCanvas)
            return;
        if (tex.CheckPixels)
        {
            if (cache == 0)
                Log.Add("unload");
        }
        else if (cache != 0)
        {
            Log.Add("register " + (ReadTrueColor() ? "1" : "0"));
        }
    }

    public void PrecacheTexture(VmSwTex1275? tex, int cache)
    {
        bool bgra = ReadTrueColor();
        if (tex == null || !tex.Valid || tex.SoftwareCanvas)
            return;
        if ((cache & HitColumn) != 0)
            Log.Add(bgra ? "column bgra" : "column pal");
        else if (cache != 0)
            Log.Add(bgra ? "pixels bgra" : "pixels pal");
    }

    public void Precache(int[] hit, IReadOnlyList<VmSwClass1275> actors)
    {
        var marks = new bool[Sprites.Count];
        foreach (var cls in actors)
        {
            foreach (int sprite in cls.StateSprites)
                marks[sprite] = true;
        }

        for (int i = Sprites.Count - 1; i >= 0; i--)
        {
            if (!marks[i])
                continue;
            var sprite = Sprites[i];
            for (int j = 0; j < sprite.NumFrames; j++)
            {
                var frame = Frames[sprite.FrameIndex + j];
                for (int k = 0; k < 16; k++)
                {
                    if (!frame.Valid[k])
                        continue;
                    hit[frame.Index[k]] = HitSprite;
                    Log.Add(
                        "mark " + i.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                        " " + j.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                        " " + k.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                        " " + frame.Index[k].ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
        }

        Log.Add("begin");
        for (int i = Textures.Count - 1; i >= 0; i--)
            Prepare(Textures[i], hit[i]);
        for (int i = Textures.Count - 1; i >= 0; i--)
            PrecacheTexture(Textures[i], hit[i]);
        Log.Add("end");
    }

    public void RenderView(IReadOnlyList<VmSwCam1275> cameras)
    {
        Log.Add("install " + Pitch.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + Window.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Log.Add("renderview");
        Pitch = ScenePitchAfter;
        Window = SceneWindowAfter;
        Log.Add("restore " + Pitch.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + Window.ToString(System.Globalization.CultureInfo.InvariantCulture));
        foreach (var cam in cameras)
            RenderTextureView(cam);
    }

    public void WriteSavePic(int? actor, int width, int height)
    {
        Log.Add("install " + Pitch.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + Window.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Log.Add("tocanvas " + (actor.HasValue ? actor.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "null") +
            " 0 0 " + width.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + height.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Pitch = SavePitchAfter;
        Window = SaveWindowAfter;
        Log.Add("restore " + Pitch.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + Window.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Log.Add("write " + SavePal.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + width.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + height.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + Sector.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " 0");
    }

    public void DrawRemaining()
    {
        Log.Add("install " + Pitch.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + Window.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Log.Add("playersprites");
        Pitch = RemainPitchAfter;
        Window = RemainWindowAfter;
        Log.Add("restore " + Pitch.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + Window.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public void SetClearColor(int color)
    {
        Log.Add("clear " + color.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public void RenderTextureView(VmSwCam1275 cam)
    {
        Log.Add("cam install " + Pitch.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + Window.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (!cam.Present)
        {
            Log.Add("cam missing " + cam.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return;
        }

        Log.Add((RenderTargetBgra ? "canvas bgra " : "canvas pal ") +
            cam.Width.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " +
            cam.Height.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Log.Add("light save " + LightToken.ToString(System.Globalization.CultureInfo.InvariantCulture));
        double saved = Fov;
        Fov = cam.Fov;
        Log.Add("fov " + Fov.ToString("G17", System.Globalization.CultureInfo.InvariantCulture));
        Log.Add("tocanvas " + cam.Actor.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " 0 0 " + cam.Width.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + cam.Height.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + (cam.FirstUpdate ? "1" : "0"));
        Fov = saved;
        Log.Add("fov " + Fov.ToString("G17", System.Globalization.CultureInfo.InvariantCulture));
        Log.Add("pixels " + (RenderTargetBgra ? "1" : "0"));
        Log.Add("light restore " + LightToken.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Pitch = CameraPitchAfter;
        Window = CameraWindowAfter;
        Log.Add("cam restore " + Pitch.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + Window.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public void SetColormap(int globalColormap, int fadeTo, string custom, string fadeTable)
    {
        Log.Add("maps");
        Log.Add("color 255 255 255 0");
        Log.Add("fade " + fadeTo.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (globalColormap != 0)
            Log.Add("cmap " + custom);
        else if (fadeTo == 0)
            Log.Add("cmap " + fadeTable);
    }
}
