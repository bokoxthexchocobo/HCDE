using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Gyroscope input from <c>src/i_input_gyro.cpp</c> lines 1-246, through end of file.
/// The header declaration is not this credit. Windows and the other platform are both selectable.
/// </summary>
public sealed class VmGyroInput1361Slice
{
    public const uint SdlInitSensor = 0x00008000;
    public const int SdlSensorGyro = 2;
    public const string SdlLibrary = "SDL2.dll";

    public static readonly string[] SensorSymbols =
    [
        "SDL_WasInit",
        "SDL_InitSubSystem",
        "SDL_SensorUpdate",
        "SDL_NumSensors",
        "SDL_SensorGetDeviceType",
        "SDL_SensorOpen",
        "SDL_SensorGetData",
        "SDL_SensorClose",
    ];

    public sealed class Sample
    {
        public bool Available { get; set; }
        public bool PlatformQueried { get; set; }
        public string Provider { get; set; } = "none";
        public int SensorCount { get; set; }
        public float YawRate { get; set; }
        public float PitchRate { get; set; }
    }

    public bool Enable { get; set; }
    public float YawScale { get; set; } = 2.5f;
    public float PitchScale { get; set; } = 2.0f;
    public float Deadzone { get; set; } = 0.05f;
    public int Mode { get; set; }
    public bool InvertYaw { get; set; }
    public bool InvertPitch { get; set; }
    public bool Windows { get; set; }
    public List<string> Lines { get; } = [];

    public Func<string, IntPtr>? GetModuleHandle { get; set; }
    public Func<string, IntPtr>? LoadLibrary { get; set; }
    public Func<IntPtr, string, IntPtr>? GetProcAddress { get; set; }
    public Func<uint, uint>? WasInit { get; set; }
    public Func<uint, int>? InitSubSystem { get; set; }
    public Action? SensorUpdate { get; set; }
    public Func<int>? NumSensors { get; set; }
    public Func<int, int>? SensorGetDeviceType { get; set; }
    public Func<int, object?>? SensorOpen { get; set; }
    public Func<object, float[], int, int>? SensorGetData { get; set; }
    public Action<object>? SensorClose { get; set; }

    public bool ModeAllowsContribution() => Mode <= 0;

    public Sample PollPlatformSample()
    {
        var sample = new Sample();
        if (!Enable)
        {
            sample.Provider = "disabled";
            return sample;
        }

        if (!Windows)
        {
            sample.Provider = "platform-sensor-probe-unavailable";
            return sample;
        }

        sample.PlatformQueried = true;
        sample.Provider = "windows-sdl2-dynamic-sensor";
        if (GetModuleHandle is null)
            throw new InvalidOperationException("GetModuleHandleA");
        var sdl = GetModuleHandle(SdlLibrary);
        if (sdl == IntPtr.Zero)
        {
            if (LoadLibrary is null)
                throw new InvalidOperationException("LoadLibraryA");
            sdl = LoadLibrary(SdlLibrary);
        }

        if (sdl == IntPtr.Zero)
        {
            sample.Provider = "windows-sdl2-not-loaded";
            return sample;
        }

        if (GetProcAddress is null)
            throw new InvalidOperationException("GetProcAddress");
        var symbolsPresent = true;
        foreach (var name in SensorSymbols)
        {
            if (GetProcAddress(sdl, name) == IntPtr.Zero)
                symbolsPresent = false;
        }

        if (!symbolsPresent)
        {
            sample.Provider = "windows-sdl2-sensor-symbols-missing";
            return sample;
        }

        if (WasInit is null)
            throw new InvalidOperationException("SDL_WasInit");
        if (InitSubSystem is null)
            throw new InvalidOperationException("SDL_InitSubSystem");
        if ((WasInit(SdlInitSensor) & SdlInitSensor) == 0 && InitSubSystem(SdlInitSensor) != 0)
        {
            sample.Provider = "windows-sdl2-sensor-init-failed";
            return sample;
        }

        if (SensorUpdate is null)
            throw new InvalidOperationException("SDL_SensorUpdate");
        SensorUpdate();
        if (NumSensors is null)
            throw new InvalidOperationException("SDL_NumSensors");
        var sensorCount = NumSensors();
        sample.SensorCount = sensorCount;
        for (var i = 0; i < sensorCount; i++)
        {
            if (SensorGetDeviceType is null)
                throw new InvalidOperationException("SDL_SensorGetDeviceType");
            if (SensorGetDeviceType(i) != SdlSensorGyro)
                continue;
            if (SensorOpen is null)
                throw new InvalidOperationException("SDL_SensorOpen");
            var sensor = SensorOpen(i);
            if (sensor is null)
                continue;
            if (SensorGetData is null)
                throw new InvalidOperationException("SDL_SensorGetData");
            var values = new float[3];
            var ok = SensorGetData(sensor, values, 3);
            if (SensorClose is null)
                throw new InvalidOperationException("SDL_SensorClose");
            SensorClose(sensor);
            if (ok != 0)
                continue;
            sample.Available = true;
            sample.PitchRate = values[0];
            sample.YawRate = values[1];
            return sample;
        }

        return sample;
    }

    public void GyroStatus()
    {
        var sample = PollPlatformSample();
        Lines.Add("HCDE gyro input scaffold:\n");
        Lines.Add($"  joy_gyro_enable      = {(Enable ? "on" : "off")}\n");
        Lines.Add(Format("  joy_gyro_yawscale    = {0:0.000}\n", YawScale));
        Lines.Add(Format("  joy_gyro_pitchscale  = {0:0.000}\n", PitchScale));
        Lines.Add(Format("  joy_gyro_deadzone    = {0:0.000} rad/s\n", Deadzone));
        Lines.Add($"  joy_gyro_mode        = {Mode} (0=always, 1=held, 2=toggle)\n");
        Lines.Add($"  joy_gyro_invertyaw   = {(InvertYaw ? "on" : "off")}\n");
        Lines.Add($"  joy_gyro_invertpitch = {(InvertPitch ? "on" : "off")}\n");
        Lines.Add($"  platform provider    = {sample.Provider}\n");
        Lines.Add($"  platform queried     = {(sample.PlatformQueried ? "yes" : "no")}\n");
        Lines.Add($"  platform sensor count = {sample.SensorCount}\n");
        Lines.Add($"  platform sensor available = {(sample.Available ? "yes" : "no")}\n");
        Lines.Add(Format(
            "  latest platform sample = yaw {0:0.000} rad/s, pitch {1:0.000} rad/s\n",
            sample.YawRate,
            sample.PitchRate));
        Lines.Add($"  mode contribution allowed = {(ModeAllowsContribution() ? "yes" : "no")}\n");
        Lines.Add("  platform sensor plumbing = SDL2 sensor probe when available; otherwise zero contribution\n");
    }

    public void GetTiccmdContribution(out float yawDelta, out float pitchDelta)
    {
        yawDelta = 0f;
        pitchDelta = 0f;
        if (!Enable)
            return;
        if (!ModeAllowsContribution())
            return;

        var sample = PollPlatformSample();
        if (!sample.Available)
            return;

        var yaw = sample.YawRate;
        var pitch = sample.PitchRate;
        if (yaw > -Deadzone && yaw < Deadzone)
            yaw = 0f;
        if (pitch > -Deadzone && pitch < Deadzone)
            pitch = 0f;
        yaw *= YawScale;
        pitch *= PitchScale;
        if (InvertYaw)
            yaw = -yaw;
        if (InvertPitch)
            pitch = -pitch;
        yawDelta = yaw;
        pitchDelta = pitch;
    }

    static string Format(string pattern, float value) =>
        string.Format(CultureInfo.InvariantCulture, pattern, value);

    static string Format(string pattern, float yaw, float pitch) =>
        string.Format(CultureInfo.InvariantCulture, pattern, yaw, pitch);
}
