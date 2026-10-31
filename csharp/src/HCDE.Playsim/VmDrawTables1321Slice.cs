namespace HCDE.Playsim;

/// <summary>
/// Shade maps, the fuzz table, particle textures, and the wall-column lights
/// from <c>r_draw.cpp</c> (1-285). Chunk 1321.
/// </summary>
public sealed class VmDrawTables1321
{
    public const int ColorMaps = 32;
    public const int FuzzTable = 50;
    public const int FuzzRandomSize = 100;
    public const int ParticleTextures = 3;
    public const int ParticleSize = 64;
    public const int MaxLights = 16;
    public const int XFlip = 0x0001;

    public static readonly int[] FuzzRandom =
    {
        75, 76, 21, 91, 56, 33, 62, 99, 61, 79,
        95, 54, 41, 18, 69, 43, 49, 59, 10, 84,
        94, 17, 57, 46, 9, 39, 55, 34, 100, 81,
        73, 88, 92, 3, 63, 36, 7, 28, 13, 80,
        16, 96, 78, 29, 71, 58, 89, 24, 1, 35,
        52, 82, 4, 14, 22, 53, 38, 66, 12, 72,
        90, 44, 77, 83, 6, 27, 48, 30, 42, 32,
        65, 15, 97, 20, 67, 74, 98, 85, 60, 68,
        19, 26, 8, 87, 86, 64, 11, 37, 31, 47,
        25, 5, 50, 51, 23, 2, 93, 70, 40, 45
    };

    private static readonly sbyte[] FuzzInit =
    {
        1, -1, 1, -1, 1, 1, -1,
        1, 1, -1, 1, 1, 1, -1,
        1, 1, 1, -1, -1, -1, -1,
        1, -1, -1, 1, 1, 1, 1, -1,
        1, -1, 1, 1, -1, -1, 1,
        1, -1, -1, -1, -1, 1, 1,
        1, 1, -1, 1, 1, -1, 1
    };

    public byte[] ShadeTables { get; } = new byte[ColorMaps * 16 * 256];
    public uint[] ShadeColor { get; } = new uint[16];
    public uint[] ShadeDesaturate { get; } = new uint[16];
    public byte[] Identity { get; } = new byte[256];
    public int[] FuzzOffset { get; } = new int[FuzzTable + 1];
    public uint[] Particles { get; } = new uint[ParticleTextures * ParticleSize * ParticleSize];
    public int FuzzPos { get; private set; }
    public int FuzzViewHeight { get; set; } = 200;
    public bool FuzzScale { get; set; } = true;
    public bool DynLights { get; private set; } = true;
    public int SyncCount { get; private set; }
    public int ViewWidth { get; set; } = 320;
    public List<DrawerLight> Lights { get; } = new();
    private int _nextRandom;

    public void SetDynLights(bool value)
    {
        DynLights = value;
        SyncCount++;
    }

    public void InitShadeMaps()
    {
        int cursor = 0;
        for (int i = 0; i < 16; i++)
        {
            ShadeColor[i] = ~0u;
            ShadeDesaturate[i] = ~0u;
            for (int j = 0; j < ColorMaps; j++)
            {
                int alpha = (ColorMaps - j) * 256 / ColorMaps * (16 - i);
                for (int k = 0; k < 256; k++)
                {
                    int value = (((k + 2) * alpha) + 256) >> 14;
                    ShadeTables[cursor++] = (byte)Math.Min(value, 64);
                }
            }
        }
        for (int i = 0; i < 256; i++)
            Identity[i] = (byte)i;
    }

    public void InitFuzzTable()
    {
        var count = new sbyte[FuzzTable + 1];
        count[0] = 1;
        for (int i = 1; i < FuzzTable; i++)
            count[i] = (sbyte)(count[i + FuzzInit[i]] + 1);
        float shade = 1f - 6f / ColorMaps;
        for (int i = 0; i < FuzzTable; i++)
        {
            float result = 1f;
            for (int j = 0; j < count[i]; j++)
                result *= shade;
            int offset = (int)((1f - result) * ColorMaps + 0.5f);
            FuzzOffset[i] = Math.Clamp(offset, 0, ColorMaps - 1);
        }
    }

    public void InitParticles()
    {
        double center = ParticleSize * 0.5;
        for (int y = 0; y < ParticleSize; y++)
        {
            for (int x = 0; x < ParticleSize; x++)
            {
                double dx = (center - x - 0.5) / center;
                double dy = (center - y - 0.5) / center;
                double dist2 = dx * dx + dy * dy;
                double round = Math.Clamp(1.7 - dist2 * 1.7, 0, 1);
                double smooth = Math.Clamp(1.1 - dist2 * 1.1, 0, 1);
                int index = x + y * ParticleSize;
                Particles[index] = 128;
                Particles[ParticleSize * ParticleSize + index] = (uint)(round * 128.0 + 0.5);
                Particles[2 * ParticleSize * ParticleSize + index] = (uint)(smooth * 128.0 + 0.5);
            }
        }
    }

    public void UpdateFuzzFrame()
    {
        if (!FuzzScale)
            return;
        FuzzPos = (FuzzPos + FuzzRandom[_nextRandom] * FuzzTable / 100) % FuzzTable;
        _nextRandom++;
        if (_nextRandom == FuzzRandomSize)
            _nextRandom = 0;
    }

    public void UpdateFuzzColumn(int y1, int y2)
    {
        if (FuzzScale)
            return;
        int top = Math.Max(y1, 1);
        int bottom = Math.Min(y2, FuzzViewHeight);
        if (top <= bottom)
            FuzzPos = (FuzzPos + bottom - top + 1) % FuzzTable;
    }

    public void SetLights(Wall wall, int x, int y1)
    {
        Lights.Clear();
        int tx = x;
        if ((wall.MirrorFlags & XFlip) != 0)
            tx = ViewWidth - tx - 1;
        float w1 = 1f / wall.Sz1;
        float w2 = 1f / wall.Sz2;
        float t = (x - wall.Sx1 + 0.5f) / (wall.Sx2 - wall.Sx1);
        float wcol = w1 * (1f - t) + w2 * t;
        float zcol = 1f / wcol;
        wall.ViewX = (float)((tx + 0.5 - wall.CenterX) / wall.CenterX * zcol);
        wall.ViewY = zcol;
        wall.ViewZ = (float)((wall.CenterY - y1 - 0.5) / wall.InvZ * zcol);
        wall.StepZ = (float)(-zcol / wall.InvZ);
        if (wall.Lights == null)
            return;
        foreach (DynLight? light in wall.Lights)
        {
            if (light == null || !light.Active)
                continue;
            float lx = (float)((light.X - wall.PosX) * wall.Sin - (light.Y - wall.PosY) * wall.Cos) - wall.ViewX;
            float ly = (float)((light.X - wall.PosX) * wall.TanCos + (light.Y - wall.PosY) * wall.TanSin) - wall.ViewY;
            float lz = (float)(light.Z - wall.PosZ);
            float constant = lx * lx + ly * ly;
            float normal = light.Attenuated ? lx * wall.NormalX + ly * wall.NormalY : 0f;
            if (light.Radius * light.Radius < constant || normal < 0f)
                continue;
            Lights.Add(new DrawerLight
            {
                X = constant,
                Y = normal,
                Z = lz,
                Radius = 256f / light.Radius,
                Color = (light.Red << 16) | (light.Green << 8) | light.Blue
            });
            if (Lights.Count == MaxLights)
                break;
        }
    }

    public sealed class Wall
    {
        public int MirrorFlags, Sx1, Sx2 = 2;
        public float Sz1 = 1, Sz2 = 1, CenterX = 160, CenterY = 100, InvZ = 160;
        public double PosX, PosY, PosZ, Sin, Cos = 1, TanCos = 1, TanSin;
        public float NormalX, NormalY = 1;
        public float ViewX, ViewY, ViewZ, StepZ;
        public List<DynLight?>? Lights { get; set; }
    }

    public sealed class DynLight
    {
        public double X, Y, Z;
        public float Radius = 1;
        public int Red, Green, Blue;
        public bool Active = true, Attenuated;
    }

    public sealed class DrawerLight
    {
        public float X, Y, Z, Radius;
        public int Color;
    }
}
