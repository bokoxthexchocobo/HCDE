namespace HCDE.Playsim;

/// <summary>
/// Camera-to-canvas assignments from <c>r_canvastexture.cpp</c> (<b>1–200</b>).
/// Chunk <b>1191</b>. Texture lookup is a host result. A texture id is valid when it is greater than zero.
/// </summary>
public static class VmCanvasTexture1191Slice
{
    public const int TextureWall = 1;
    public const int TexmanOverridable = 2;

    public static readonly Dictionary<int, CanvasTexture> Textures = new();
    public static readonly List<string> Messages = new();
    public static Func<string, int, int, int> CheckForTexture = (_, _, _) => 0;

    public sealed class CanvasTexture
    {
        public string Name = "";
        public bool HasCanvas;
        public bool HardwareCanvas;
        public bool FirstUpdate;
        public bool NeedsUpdate;
    }

    public sealed class Actor
    {
        public int Id;
        public Level Level = new();
    }

    public sealed class Level
    {
        public readonly Info Canvas = new();
    }

    public sealed class Probe
    {
        public Actor? Viewpoint;
        public CanvasTexture? Texture;
        public int PicNum;
        public double Fov;
    }

    public sealed class Info
    {
        public readonly List<Probe> List = new();
        public readonly List<Actor?> Marked = new();

        public void Add(Actor? viewpoint, int picNum, double fov)
        {
            if (picNum <= 0)
                return;
            if (!Textures.TryGetValue(picNum, out var texture))
                throw new NullReferenceException();

            if (!texture.HasCanvas)
            {
                Messages.Add(texture.Name + " is not a valid target for a camera\n");
                return;
            }

            for (int i = 0; i < List.Count; i++)
            {
                if (!ReferenceEquals(List[i].Texture, texture))
                    continue;
                Probe probe = List[i];
                if (probe.Viewpoint != viewpoint || probe.Fov != fov)
                    texture.FirstUpdate = true;
                probe.Viewpoint = viewpoint;
                probe.Fov = fov;
                return;
            }

            List.Add(new Probe
            {
                Viewpoint = viewpoint,
                Texture = texture,
                PicNum = picNum,
                Fov = fov
            });
            texture.FirstUpdate = true;
        }

        public void UpdateAll(Action<Actor, CanvasTexture, double> callback)
        {
            foreach (Probe probe in List)
            {
                CanvasTexture texture = probe.Texture!;
                if (probe.Viewpoint != null && texture.NeedsUpdate)
                    callback(probe.Viewpoint, texture, probe.Fov);
            }
        }

        public void EmptyList()
        {
            List.Clear();
        }

        public void Serialize(Archive arc)
        {
            if (arc.Writing)
            {
                if (List.Count == 0)
                    return;
                if (!arc.BeginArray("canvastextures"))
                    return;
                foreach (Probe probe in List)
                {
                    if (probe.Texture == null || probe.Viewpoint == null)
                        continue;
                    if (!arc.BeginObject())
                        continue;
                    arc.Viewpoint(probe.Viewpoint);
                    arc.Fov(probe.Fov);
                    arc.Texture(probe.PicNum);
                    arc.EndObject();
                }

                arc.EndArray();
                return;
            }

            if (!arc.BeginArray("canvastextures"))
                return;
            while (arc.BeginObject())
            {
                Actor? viewpoint = arc.ReadViewpoint();
                double fov = arc.ReadFov();
                int picNum = arc.ReadTexture();
                arc.EndObject();
                Add(viewpoint, picNum, fov);
            }

            arc.EndArray();
        }

        public void Mark()
        {
            foreach (Probe info in List)
                Marked.Add(info.Viewpoint);
        }
    }

    public sealed class Archive
    {
        public bool Writing = true;
        public bool AllowArray = true;
        public bool AllowObject = true;
        public readonly List<string> Trace = new();
        public readonly List<Saved> Incoming = new();
        int _read;

        public bool BeginArray(string name)
        {
            if (!AllowArray)
                return false;
            if (Writing)
                Trace.Add("array " + name);
            return true;
        }

        public void EndArray()
        {
            if (Writing)
                Trace.Add("end-array");
        }

        public bool BeginObject()
        {
            if (Writing)
            {
                if (!AllowObject)
                    return false;
                Trace.Add("object");
                return true;
            }

            if (_read >= Incoming.Count)
                return false;
            _read++;
            return true;
        }

        public void EndObject()
        {
            if (Writing)
                Trace.Add("end-object");
        }

        public void Viewpoint(Actor actor)
        {
            Trace.Add("viewpoint " + actor.Id);
        }

        public void Fov(double fov)
        {
            Trace.Add("fov " + fov.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public void Texture(int picNum)
        {
            Trace.Add("texture " + picNum);
        }

        public Actor? ReadViewpoint()
        {
            return Incoming[_read - 1].Viewpoint;
        }

        public double ReadFov()
        {
            return Incoming[_read - 1].Fov;
        }

        public int ReadTexture()
        {
            return Incoming[_read - 1].PicNum;
        }
    }

    public sealed class Saved
    {
        public Actor? Viewpoint;
        public double Fov;
        public int PicNum;
    }

    public static void Clear()
    {
        Textures.Clear();
        Messages.Clear();
        CheckForTexture = (_, _, _) => 0;
    }

    public static void SetCameraToTexture(Actor viewpoint, string name, double fov)
    {
        int textureId = CheckForTexture(name, TextureWall, TexmanOverridable);
        if (textureId <= 0)
            return;
        if (!Textures.TryGetValue(textureId, out var tex) || tex == null || !tex.HardwareCanvas)
            return;
        viewpoint.Level.Canvas.Add(viewpoint, textureId, fov);
    }
}
