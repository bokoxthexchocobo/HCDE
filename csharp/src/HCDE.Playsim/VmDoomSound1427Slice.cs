namespace HCDE.Playsim;
/// <summary>
/// CalcSectorSoundOrg through ValidatePosVel from s_doomsound.cpp 1015-1254.
/// Sound saving ends at line 1014. Lump reads start at line 1255.
/// </summary>
public static class VmDoomSound1427Slice
{
    public const int BandStart = 1015;
    public const int BandEnd = 1254;
    public const int SaveEnd = 1014;
    public const int ReadSound = 1255;
    public const int Floor = 1;
    public const int Ceiling = 2;
    public const int Interior = 4;
    public const int SectorSounds = 1 << 18;
    public const int ListenerZ = 8;
    public const int Area = 128;
    public const int TicRate = 35;
    public const int Unattached = 0;
    public const int None = 1;
    public const int Actor = 2;
    public const int Sector = 3;
    public const int Poly = 4;
    public const float PositionLimit = 1024f * 1024f;
    public const float VelocityLimit = 2f * 1024f * 1024f * 35f;
    public const string Red = "\u001cG";
    public const string White = "\u001cJ";
    public sealed class Vec
    {
        public float X;
        public float Y;
        public float Z;
    }
    public sealed class State
    {
        public double ListenX;
        public double ListenY;
        public double ListenZ;
        public object? Sector = "sector";
        public bool Inside = true;
        public double ClosestX;
        public double ClosestY;
        public double CenterX;
        public double CenterY;
        public double FloorZ;
        public double CeilingZ;
        public int Compat;
        public bool LevelNull;
        public bool PrimaryNull;
        public int Channel = Interior;
        public Vec Pos = new();
        public bool PolyNull;
        public bool SideNull;
        public double PolyX;
        public double PolyY;
        public bool HasPos = true;
        public Vec? Vel = new();
        public object? Camera = "camera";
        public bool CameraSectorNull;
        public int CameraGroup;
        public double CamX;
        public double CamY;
        public double CamZ;
        public float SoundX;
        public float SoundY;
        public float SoundZ;
        public int Type = None;
        public float[]? Point;
        public int Flags;
        public double DispX;
        public double DispY;
        public bool SourceNull;
        public bool SourceSectorNull;
        public double SourceX;
        public double SourceY;
        public double SourceZ;
        public double MoveX;
        public double MoveY;
        public double MoveZ;
        public bool CenterNull;
        public bool HitNull;
        public string? ClassName = "DoomImp";
        public bool ClassNull;
        public List<string> Lines = new();
    }
    public static void SectorOrigin(State s)
    {
        if (s.Sector == null || s.LevelNull) throw new NullReferenceException();
        if ((s.Compat & SectorSounds) == 0)
        {
            if (s.PrimaryNull) throw new NullReferenceException();
            if (s.Inside)
            {
                s.Pos.X = (float)s.ListenX;
                s.Pos.Z = (float)s.ListenY;
            }
            else
            {
                s.Pos.X = (float)s.ClosestX;
                s.Pos.Z = (float)s.ClosestY;
            }
        }
        else
        {
            s.Pos.X = (float)s.CenterX;
            s.Pos.Z = (float)s.CenterY;
        }
        if (s.Channel == Floor) s.Pos.Y = (float)Math.Min(s.FloorZ, s.ListenZ);
        else if (s.Channel == Ceiling) s.Pos.Y = (float)Math.Max(s.CeilingZ, s.ListenZ);
        else if (s.Channel == Interior) s.Pos.Y = (float)Clamp(s.ListenZ, s.FloorZ, s.CeilingZ);
    }
    public static void PolyOrigin(State s)
    {
        if (s.PolyNull || s.SideNull || s.Sector == null) throw new NullReferenceException();
        s.Pos.X = (float)s.PolyX;
        s.Pos.Z = (float)s.PolyY;
        s.Pos.Y = (float)Clamp(s.ListenZ, s.FloorZ, s.CeilingZ);
    }
    public static void Calc(State s)
    {
        if (!s.HasPos) return;
        double listenX = 0, listenY = 0, listenZ = 0;
        if (s.Camera != null)
        {
            if (s.CameraSectorNull) throw new NullReferenceException();
            listenX = s.CamX;
            listenY = s.CamY;
            listenZ = s.CamZ;
            s.Pos.X = s.SoundX;
            s.Pos.Y = s.SoundY;
            s.Pos.Z = s.SoundZ;
        }
        else s.Pos.X = s.Pos.Y = s.Pos.Z = 0;
        if (s.Vel != null) s.Vel.X = s.Vel.Y = s.Vel.Z = 0;
        var flags = s.Flags;
        if (s.Type == Unattached)
        {
            if (s.Point == null || s.PrimaryNull || s.HitNull) throw new NullReferenceException();
            s.Pos.X = s.Point[0] - (float)s.DispX;
            s.Pos.Y = (flags & ListenerZ) == 0 ? s.Point[1] : (float)listenZ;
            s.Pos.Z = s.Point[2] - (float)s.DispY;
            return;
        }
        switch (s.Type)
        {
            case Actor when !s.SourceNull:
                if (s.SourceSectorNull || s.PrimaryNull) throw new NullReferenceException();
                s.Pos.X = (float)(s.SourceX - s.DispX);
                s.Pos.Y = (float)s.SourceZ;
                s.Pos.Z = (float)(s.SourceY - s.DispY);
                if (s.Vel != null)
                {
                    s.Vel.X = (float)(s.MoveX * TicRate);
                    s.Vel.Y = (float)(s.MoveZ * TicRate);
                    s.Vel.Z = (float)(s.MoveY * TicRate);
                }
                break;
            case Sector when !s.SourceNull:
                if (s.PrimaryNull) throw new NullReferenceException();
                if ((flags & Area) != 0)
                {
                    var x = s.ListenX;
                    var y = s.ListenY;
                    var z = s.ListenZ;
                    s.ListenX = listenX + s.DispX;
                    s.ListenY = listenY + s.DispY;
                    s.ListenZ = listenZ;
                    SectorOrigin(s);
                    s.ListenX = x;
                    s.ListenY = y;
                    s.ListenZ = z;
                    s.Pos.X -= (float)s.DispX;
                    s.Pos.Z -= (float)s.DispY;
                }
                else
                {
                    s.Pos.X = (float)(s.CenterX - s.DispX);
                    s.Pos.Z = (float)(s.CenterY - s.DispY);
                    flags |= ListenerZ;
                }
                break;
            case Poly when !s.SourceNull:
                if (s.CenterNull || s.PrimaryNull) throw new NullReferenceException();
                var keepX = s.ListenX;
                var keepY = s.ListenY;
                var keepZ = s.ListenZ;
                s.ListenX = listenX + s.DispX;
                s.ListenY = listenY + s.DispY;
                s.ListenZ = listenZ;
                PolyOrigin(s);
                s.ListenX = keepX;
                s.ListenY = keepY;
                s.ListenZ = keepZ;
                s.Pos.X -= (float)s.DispX;
                s.Pos.Z -= (float)s.DispY;
                break;
        }
        if ((flags & ListenerZ) != 0 && s.Camera != null) s.Pos.Y = (float)listenZ;
    }
    public static bool Check(State s, Vec value, float limit, string name)
    {
        var valid = Within(value.X, limit) && Within(value.Y, limit) && Within(value.Z, limit);
        if (!valid)
        {
            s.Lines.Add(Red + "Invalid sound " + name + " " + White + "(" + Text(value.X) + ", " + Text(value.Z) + ", " + Text(value.Y) + ")");
            if (s.SourceNull) s.Lines.Add("\n");
            else
            {
                if (s.ClassNull) throw new NullReferenceException();
                s.Lines.Add(Red + " for actor of class " + White + s.ClassName + "\n");
            }
        }
        return valid;
    }
    public static bool CheckPair(State s)
    {
        if (s.Type != Actor) return true;
        var position = Check(s, s.Pos, PositionLimit, "position");
        var velocity = Check(s, s.Vel ?? throw new NullReferenceException(), VelocityLimit, "velocity");
        return velocity && position;
    }
    static bool Within(float value, float limit) => value >= -limit && value <= limit;
    static double Clamp(double value, double low, double high)
    {
        if (value < low) return low;
        if (value > high) return high;
        return value;
    }
    static string Text(float value) => value.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
}
