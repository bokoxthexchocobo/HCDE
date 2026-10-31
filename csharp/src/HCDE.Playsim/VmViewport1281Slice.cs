using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Software world matrix, clip frustum, aspect setup, and the sky angle table
/// from <c>r_viewport.cpp</c> (1-290). Chunk 1281. Fuzz-table and particle
/// setup are host seams. The clip multiply uses the matrix already stored
/// in the world-to-clip slot.
/// </summary>
public sealed class VmMat1281
{
    public float[] M = new float[16];

    public void LoadIdentity()
    {
        for (int i = 0; i < 16; i++)
            M[i] = 0f;
        for (int i = 0; i < 4; i++)
            M[i + i * 4] = 1f;
    }

    public void Translate(float x, float y, float z)
    {
        M[12] = M[0] * x + M[4] * y + M[8] * z + M[12];
        M[13] = M[1] * x + M[5] * y + M[9] * z + M[13];
        M[14] = M[2] * x + M[6] * y + M[10] * z + M[14];
    }

    public void Scale(float x, float y, float z)
    {
        M[0] *= x; M[1] *= x; M[2] *= x; M[3] *= x;
        M[4] *= y; M[5] *= y; M[6] *= y; M[7] *= y;
        M[8] *= z; M[9] *= z; M[10] *= z; M[11] *= z;
    }

    public void Multiply(VmMat1281 other)
    {
        float[] a = other.M;
        float[] res = new float[16];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                float sum = 0f;
                for (int k = 0; k < 4; k++)
                    sum += M[k * 4 + i] * a[j * 4 + k];
                res[j * 4 + i] = sum;
            }
        }
        M = res;
    }

    public void Load(float[] source)
    {
        for (int i = 0; i < 16; i++)
            M[i] = source[i];
    }

    public string Text()
    {
        string[] parts = new string[16];
        for (int i = 0; i < 16; i++)
            parts[i] = M[i].ToString("G9", CultureInfo.InvariantCulture);
        return string.Join(' ', parts);
    }
}

public sealed class VmViewport1281
{
    public const double YAspect = 200.0;
    public const int FlipFlag = 1;

    public int ViewWidth;
    public int ViewHeight;
    public int ViewWindowX;
    public int ViewWindowY;
    public int ViewCenterX;
    public int ViewCenterY;
    public int RenderPitch;
    public int Flags3;
    public float WidescreenRatio;
    public float TrueRatio;
    public double FocalTangent;
    public double Visibility;
    public double? PixelStretch;
    public double Sin;
    public double Cos = 1;
    public double PosX;
    public double PosY;
    public double PosZ;
    public double PitchDegrees;
    public double FieldOfViewDegrees = 90;
    public bool HasCamera;
    public bool RenderingToCanvas;
    public bool Bgra;
    public int MirrorFlags;
    public double CenterX;
    public double CenterY;
    public double FocalLengthX;
    public double FocalLengthY;
    public double YaspectMul;
    public double BaseYaspectMul;
    public double IYaspectMul;
    public double InvZtoScale;
    public double WallTMapScale2;
    public double GlobalUClip;
    public double GlobalDClip;
    public int FuzzViewHeight;
    public int ViewingRangeRecip;
    public short[] ScreenHeight = Array.Empty<short>();
    public uint[] XToViewAngle = Array.Empty<uint>();
    public VmMat1281 WorldToView = new();
    public VmMat1281 ViewToClip = new();
    public VmMat1281 WorldToClip = new();
    public List<string> Log = new();

    public void SetViewport(int fullWidth, int fullHeight, float trueRatio)
    {
        TrueRatio = trueRatio;
        if (!RenderingToCanvas)
        {
            string text = ViewWidth + " x " + ViewHeight;
            if (text.Length > 15)
                text = text.Substring(0, 15);
            Log.Add("viewsize " + text);
        }
        FuzzViewHeight = ViewHeight - 2;
        CenterX = ViewCenterX;
        CenterY = ViewCenterY;
        int virtWidth = fullWidth;
        int virtWidth2 = fullWidth;
        int virtHeight = fullHeight;
        int virtHeight2 = fullHeight;
        if (Taller(trueRatio))
            virtHeight2 = virtHeight2 * Multiplier(trueRatio) / 48;
        else
            virtWidth2 = virtWidth2 * Multiplier(trueRatio) / 48;
        if (Taller(WidescreenRatio))
            virtHeight = virtHeight * Multiplier(WidescreenRatio) / 48;
        else
            virtWidth = virtWidth * Multiplier(WidescreenRatio) / 48;
        double stretch = PixelStretch ?? 1.2;
        BaseYaspectMul = 320.0 * virtHeight2 / (YAspect * virtWidth2);
        YaspectMul = 320.0 * virtHeight / (YAspect * virtWidth) * stretch / 1.2;
        IYaspectMul = (double)virtWidth * YAspect / 320.0 / virtHeight;
        InvZtoScale = YaspectMul * CenterX;
        WallTMapScale2 = IYaspectMul / CenterX * 1.2 / stretch;
        IYaspectMul *= 1.2 / stretch;
        ScreenHeight = new short[ViewWidth];
        short row = unchecked((short)ViewHeight);
        for (int i = 0; i < ViewWidth; i++)
            ScreenHeight[i] = row;
        Log.Add("fill " + ViewWidth + " " + row);
        InitTextureMapping();
        bool noFade = (Flags3 & 8) != 0;
        Log.Add("visibility " + Num(Visibility) + " " + (noFade ? 1 : 0));
        Log.Add("fuzz " + RenderPitch);
        Log.Add("particles");
    }

    public void SetupFreelook()
    {
        double dy = 0;
        if (HasCamera)
            dy = FocalLengthY * Math.Tan(-PitchDegrees * (Math.PI / 180.0));
        CenterY = (ViewHeight / 2.0) + dy;
        ViewCenterY = (int)CenterY;
        GlobalUClip = -CenterY / InvZtoScale;
        GlobalDClip = (ViewHeight - CenterY) / InvZtoScale;
    }

    public void SetupPolyViewport()
    {
        WorldToView = SoftwareWorldToView();
        if ((MirrorFlags & FlipFlag) != 0)
            WorldToView.Scale(-1f, 1f, 1f);
        ViewToClip = SoftwareViewToClip();
        WorldToClip.Multiply(ViewToClip);
    }

    public int GetDest(int x, int y)
    {
        x += ViewWindowX;
        y += ViewWindowY;
        int pixels = Bgra ? 4 : 1;
        return (x + y * RenderPitch) * pixels;
    }

    public (double X, double Y) WorldToView2(double x, double y)
    {
        double tx = x - PosX;
        double ty = y - PosY;
        return (tx * Sin - ty * Cos, tx * Cos + ty * Sin);
    }

    public (double X, double Y, double Z) WorldToView3(double x, double y, double z)
    {
        double tx = x - PosX;
        double ty = y - PosY;
        double tz = z - PosZ;
        return (tx * Sin - ty * Cos, tz, tx * Cos + ty * Sin);
    }

    public (double X, double Y, double Z) ViewToScreen(double x, double y, double z)
    {
        double screenX = CenterX + x / z * CenterX;
        double screenY = CenterY - y / z * InvZtoScale;
        return (screenX, screenY, z);
    }

    public (double X, double Y, double Z) WorldToScreen(double x, double y, double z)
    {
        (double vx, double vy, double vz) = WorldToView3(x, y, z);
        return ViewToScreen(vx, vy, vz);
    }

    public (double X, double Y) ScaleView(double x, double y, double viewZ, bool pixelStretch = true)
    {
        double screenX = x / viewZ * CenterX;
        double screenY = y / viewZ * InvZtoScale;
        if (!pixelStretch)
            screenY /= YaspectMul;
        return (screenX, screenY);
    }

    public void InitTextureMapping()
    {
        FocalLengthX = CenterX / FocalTangent;
        FocalLengthY = FocalLengthX * YaspectMul;
        double half = FieldOfViewDegrees * (Math.PI / 180.0) / 2.0;
        ViewingRangeRecip = FloatToFixed(1.0 / Math.Tan(half));
        double slopeStep = FocalTangent / ViewCenterX;
        XToViewAngle = new uint[ViewWidth + 1];
        double slope = 0;
        for (int i = ViewCenterX; i <= ViewWidth; i++)
        {
            XToViewAngle[i] = ToAngle(slope);
            slope += slopeStep;
        }
        for (int i = 0; i < ViewCenterX; i++)
            XToViewAngle[i] = unchecked(0u - XToViewAngle[ViewWidth - i - 1]);
    }

    public VmMat1281 SoftwareWorldToView()
    {
        float[] m = new float[16];
        m[0] = (float)Sin;
        m[4] = (float)-Cos;
        m[9] = 1f;
        m[2] = (float)-Cos;
        m[6] = (float)-Sin;
        m[15] = 1f;
        var matrix = new VmMat1281();
        matrix.LoadIdentity();
        matrix.Translate((float)-PosX, (float)-PosY, (float)-PosZ);
        matrix.Multiply(new VmMat1281 { M = m });
        return matrix;
    }

    public VmMat1281 SoftwareViewToClip()
    {
        float near = 5f;
        float far = 65536f;
        float width = (float)(CenterX / FocalLengthX);
        float height = (float)(ViewHeight * 0.5 / FocalLengthY);
        float offset = (float)(CenterY / FocalLengthY - height);
        width *= near;
        height *= near;
        offset *= near;
        float left = -width;
        float right = width;
        float bottom = -height + offset;
        float top = height + offset;
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2f * far) / (far - near);
        float[] m = new float[16];
        m[0] = 2f * near / (right - left);
        m[5] = 2f * near / (top - bottom);
        m[8] = a;
        m[9] = b;
        m[10] = c;
        m[14] = d;
        m[11] = -1f;
        var matrix = new VmMat1281();
        matrix.Load(m);
        return matrix;
    }

    public static bool Taller(float aspect)
    {
        return aspect < 1.333f;
    }

    public static int Multiplier(float aspect)
    {
        if (!Taller(aspect))
            return RoundAway(320f / (BaseWidth(aspect) / 3f) * 48f);
        return RoundAway(200f / (BaseHeight(aspect) / 3f) * 48f);
    }

    public static int BaseWidth(float aspect)
    {
        return RoundAway(240f * aspect * 3f);
    }

    public static int BaseHeight(float aspect)
    {
        if (!Taller(aspect))
            return RoundAway(200f * (320f / (BaseWidth(aspect) / 3f)) * 3f);
        return RoundAway((200f * (4f / 3f)) / aspect * 3f);
    }

    public static int RoundAway(float value)
    {
        return (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    public static int FloatToFixed(double value)
    {
        return (int)Math.Round(value * 65536.0, MidpointRounding.ToEven);
    }

    public static uint ToAngle(double slope)
    {
        const double pi = 3.14159265358979323846;
        double v = (2 * pi - Math.Atan(slope)) * (2147483648.0 / pi);
        return WrapTrunc(v);
    }

    public static uint WrapTrunc(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return 0;
        double truncated = Math.Truncate(value);
        truncated %= 4294967296.0;
        if (truncated < 0)
            truncated += 4294967296.0;
        return (uint)truncated;
    }

    public static string Num(double value)
    {
        return value.ToString("G9", CultureInfo.InvariantCulture);
    }
}
