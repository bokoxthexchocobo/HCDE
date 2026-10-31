using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Stereo viewport and eye projection from <c>hw_vrmodes.cpp</c> (<b>1–202</b>). Chunk <b>1264</b>.
/// A missing screen, or a disable-filter callback that returns true, forces mono. The checker
/// scales are the literal square-root-of-one-half constant in this file. Orthographic isometric
/// projection uses a near plane of negative three over the fov ratio. A zero shift factor uses
/// a symmetric perspective matrix. Any other shift builds an asymmetric frustum. The weapon
/// matrix is an ortho with the screen height as the bottom and zero as the top.
/// </summary>
public sealed class VmVrMode1264
{
    public const float ISqrt2 = 0.7071067812f;
    public const float ZNear = 5f;
    public const float ZFar = 65536f;

    public const int Mono = 0;
    public const int GreenMagenta = 1;
    public const int RedCyan = 2;
    public const int SideBySideFull = 3;
    public const int SideBySideSquished = 4;
    public const int LeftEye = 5;
    public const int RightEye = 6;
    public const int Quad = 7;
    public const int SideBySideLetter = 8;
    public const int AmberBlue = 9;
    public const int TopBottom = 11;
    public const int RowInterleaved = 12;
    public const int ColumnInterleaved = 13;
    public const int Checker = 14;

    public readonly List<string> Log = new();
    public int EyeCount;
    public float Horizontal = 1f;
    public float Vertical = 1f;
    public float Weapon = 1f;
    public readonly float[] Shift = new float[2];
    public readonly float[] Scale = new float[2];
    public int SceneLeft, SceneTop, SceneWidth, SceneHeight;
    public int ScreenLeft, ScreenTop, ScreenWidth, ScreenHeight;
    public int Width = 640;
    public int Height = 480;
    public float Ipd = 0.062f;
    public float ScreenDist = 0.80f;
    public float UnitsPerMeter = 41f;
    public bool SwapEyes;

    public static VmVrMode1264 Select(bool toScreen, bool filterCallback, bool filterDisabled, int mode)
    {
        int picked = !toScreen || (filterCallback && filterDisabled) ? 0 : mode;
        var vm = FromMode(picked);
        vm.Log.Add($"mode {picked} {vm.EyeCount} {F(vm.Horizontal)} {F(vm.Vertical)} {F(vm.Weapon)} {F(vm.Shift[0])} {F(vm.Scale[0])} {F(vm.Shift[1])} {F(vm.Scale[1])}");
        return vm;
    }

    public static VmVrMode1264 FromMode(int mode)
    {
        var vm = new VmVrMode1264();
        switch (mode)
        {
            case GreenMagenta:
            case RedCyan:
            case Quad:
            case AmberBlue:
            case SideBySideLetter:
                vm.Set(2, 1f, 1f, 1f, -0.5f, 1f, 0.5f, 1f);
                break;
            case SideBySideSquished:
            case ColumnInterleaved:
                vm.Set(2, 0.5f, 1f, 1f, -0.5f, 1f, 0.5f, 1f);
                break;
            case SideBySideFull:
                vm.Set(2, 0.5f, 1f, 2f, -0.5f, 0.5f, 0.5f, 0.5f);
                break;
            case TopBottom:
            case RowInterleaved:
                vm.Set(2, 1f, 0.5f, 1f, -0.5f, 1f, 0.5f, 1f);
                break;
            case LeftEye:
                vm.Set(1, 1f, 1f, 1f, -0.5f, 1f, 0f, 0f);
                break;
            case RightEye:
                vm.Set(1, 1f, 1f, 1f, 0.5f, 1f, 0f, 0f);
                break;
            case Checker:
                vm.Set(2, ISqrt2, ISqrt2, 1f, -0.5f, 1f, 0.5f, 1f);
                break;
            default:
                vm.Set(1, 1f, 1f, 1f, 0f, 1f, 0f, 0f);
                break;
        }

        return vm;
    }

    public void Adjust(int sceneLeft, int sceneTop, int sceneWidth, int sceneHeight, int screenLeft, int screenTop, int screenWidth, int screenHeight)
    {
        SceneHeight = (int)(sceneHeight * Vertical);
        SceneTop = (int)(sceneTop * Vertical);
        SceneWidth = (int)(sceneWidth * Horizontal);
        SceneLeft = (int)(sceneLeft * Horizontal);
        ScreenHeight = (int)(screenHeight * Vertical);
        ScreenTop = (int)(screenTop * Vertical);
        ScreenWidth = (int)(screenWidth * Horizontal);
        ScreenLeft = (int)(screenLeft * Horizontal);
        Log.Add($"view {SceneLeft} {SceneTop} {SceneWidth} {SceneHeight} {ScreenLeft} {ScreenTop} {ScreenWidth} {ScreenHeight}");
    }

    public float[] Hud()
    {
        float scaled = Width / Weapon;
        float left = (Width - scaled) / 2f;
        var mat = Ortho(left, left + scaled, Height, 0f, -1f, 1f);
        Log.Add("hud " + Dump(mat));
        return mat;
    }

    public float ShiftOf(int eye) 
    {
        float amount = Shift[eye] * Ipd;
        return SwapEyes ? -amount : amount;
    }

    public float[] Projection(int eye, float fov, float aspect, float fovRatio, bool iso)
    {
        float[] mat;
        if (iso)
        {
            double zNear = -3.0 / fovRatio;
            float fH = MathF.Tan(Deg2Rad(fov) / 2f) / fovRatio;
            double fW = fH * aspect * Scale[eye];
            mat = Ortho((float)(-fW), (float)fW, -fH, fH, (float)zNear, ZFar);
            Log.Add("iso " + Dump(mat));
        }
        else if (Shift[eye] == 0f)
        {
            float fovy = 2f * Rad2Deg(MathF.Atan(MathF.Tan(Deg2Rad(fov) / 2f) / fovRatio));
            mat = Perspective(fovy, aspect, ZNear, ZFar);
            Log.Add("persp " + Dump(mat));
        }
        else
        {
            float shift = ShiftOf(eye);
            double zNear = ZNear;
            double frustumShift = zNear * shift / ScreenDist;
            double fH = zNear * MathF.Tan(Deg2Rad(fov) / 2f) / fovRatio;
            double fW = fH * aspect * Scale[eye];
            double left = -fW - frustumShift;
            double right = fW - frustumShift;
            mat = Frustum((float)left, (float)right, (float)(-fH), (float)fH, (float)zNear, ZFar);
            Log.Add("frustum " + Dump(mat));
        }

        return mat;
    }

    public void ViewShift(int eye, float yaw)
    {
        if (Shift[eye] == 0f)
        {
            Log.Add("shift 0.000000 0.000000 0.000000");
            return;
        }

        float dx = -MathF.Cos(Deg2Rad(yaw)) * UnitsPerMeter * ShiftOf(eye);
        float dy = MathF.Sin(Deg2Rad(yaw)) * UnitsPerMeter * ShiftOf(eye);
        Log.Add($"shift {F(dx)} {F(dy)} 0.000000");
    }

    void Set(int eyes, float horizontal, float vertical, float weapon, float s0, float c0, float s1, float c1)
    {
        EyeCount = eyes;
        Horizontal = horizontal;
        Vertical = vertical;
        Weapon = weapon;
        Shift[0] = s0;
        Scale[0] = c0;
        Shift[1] = s1;
        Scale[1] = c1;
    }

    static float[] Ortho(float left, float right, float bottom, float top, float nearp, float farp)
    {
        var m = Identity();
        m[0] = 2f / (right - left);
        m[5] = 2f / (top - bottom);
        m[10] = -2f / (farp - nearp);
        m[12] = -(right + left) / (right - left);
        m[13] = -(top + bottom) / (top - bottom);
        m[14] = -(farp + nearp) / (farp - nearp);
        return m;
    }

    static float[] Perspective(float fov, float ratio, float nearp, float farp)
    {
        float f = 1f / MathF.Tan(fov * (MathF.PI / 360f));
        var m = Identity();
        m[0] = f / ratio;
        m[5] = f;
        m[10] = (farp + nearp) / (nearp - farp);
        m[14] = (2f * farp * nearp) / (nearp - farp);
        m[11] = -1f;
        m[15] = 0f;
        return m;
    }

    static float[] Frustum(float left, float right, float bottom, float top, float nearp, float farp)
    {
        var m = Identity();
        m[0] = 2f * nearp / (right - left);
        m[5] = 2f * nearp / (top - bottom);
        m[8] = (right + left) / (right - left);
        m[9] = (top + bottom) / (top - bottom);
        m[10] = -(farp + nearp) / (farp - nearp);
        m[11] = -1f;
        m[14] = -2f * farp * nearp / (farp - nearp);
        m[15] = 0f;
        return m;
    }

    static float[] Identity()
    {
        var m = new float[16];
        m[0] = m[5] = m[10] = m[15] = 1f;
        return m;
    }

    static float Deg2Rad(float deg) => deg * (float)(Math.PI / 180.0);

    static float Rad2Deg(float rad) => rad * (float)(180.0 / Math.PI);

    static string Dump(float[] m)
    {
        var parts = new string[16];
        for (int i = 0; i < 16; i++)
            parts[i] = F(m[i]);
        return string.Join(' ', parts);
    }

    static string F(float value) => value.ToString("F6", CultureInfo.InvariantCulture);
}
