using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmGyroInput1361SliceTests
{
    [Fact]
    public void DefaultsStatusAndContribution()
    {
        var slice = new VmGyroInput1361Slice();
        Assert.False(slice.Enable);
        Assert.Equal(2.5f, slice.YawScale);
        Assert.Equal(2.0f, slice.PitchScale);
        Assert.Equal(0.05f, slice.Deadzone);
        Assert.Equal(0, slice.Mode);
        Assert.False(slice.InvertYaw);
        Assert.False(slice.InvertPitch);
        Assert.True(slice.ModeAllowsContribution());
        Assert.Equal(2, VmGyroInput1361Slice.SdlSensorGyro);
        Assert.Equal(0x00008000u, VmGyroInput1361Slice.SdlInitSensor);

        slice.Mode = 1;
        Assert.False(slice.ModeAllowsContribution());
        slice.Mode = 2;
        Assert.False(slice.ModeAllowsContribution());
        slice.Mode = -3;
        Assert.True(slice.ModeAllowsContribution());
        slice.Mode = 0;

        var polls = 0;
        slice.Windows = true;
        slice.GetModuleHandle = _ =>
        {
            polls++;
            return IntPtr.Zero;
        };
        slice.GyroStatus();
        Assert.Equal(0, polls);
        Assert.Equal(
            new[]
            {
                "HCDE gyro input scaffold:\n",
                "  joy_gyro_enable      = off\n",
                "  joy_gyro_yawscale    = 2.500\n",
                "  joy_gyro_pitchscale  = 2.000\n",
                "  joy_gyro_deadzone    = 0.050 rad/s\n",
                "  joy_gyro_mode        = 0 (0=always, 1=held, 2=toggle)\n",
                "  joy_gyro_invertyaw   = off\n",
                "  joy_gyro_invertpitch = off\n",
                "  platform provider    = disabled\n",
                "  platform queried     = no\n",
                "  platform sensor count = 0\n",
                "  platform sensor available = no\n",
                "  latest platform sample = yaw 0.000 rad/s, pitch 0.000 rad/s\n",
                "  mode contribution allowed = yes\n",
                "  platform sensor plumbing = SDL2 sensor probe when available; otherwise zero contribution\n",
            },
            slice.Lines);
        slice.GetTiccmdContribution(out var yaw, out var pitch);
        Assert.Equal(0f, yaw);
        Assert.Equal(0f, pitch);
        Assert.Equal(0, polls);

        slice.Mode = 1;
        slice.Enable = true;
        slice.LoadLibrary = _ => IntPtr.Zero;
        slice.Lines.Clear();
        slice.GyroStatus();
        Assert.Equal(1, polls);
        Assert.Equal("  mode contribution allowed = no\n", slice.Lines[^2]);
        Assert.Contains("  platform provider    = windows-sdl2-not-loaded\n", slice.Lines);
        slice.GetTiccmdContribution(out yaw, out pitch);
        Assert.Equal(0f, yaw);
        Assert.Equal(0f, pitch);
        Assert.Equal(1, polls);

        slice.Mode = 0;
        slice.Windows = false;
        slice.Lines.Clear();
        slice.InvertYaw = true;
        slice.YawScale = -1.25f;
        var pollsBeforeOffPlatform = polls;
        slice.GyroStatus();
        Assert.Equal(pollsBeforeOffPlatform, polls);
        Assert.Contains("  joy_gyro_enable      = on\n", slice.Lines);
        Assert.Contains("  joy_gyro_yawscale    = -1.250\n", slice.Lines);
        Assert.Contains("  joy_gyro_invertyaw   = on\n", slice.Lines);
        Assert.Contains("  platform provider    = platform-sensor-probe-unavailable\n", slice.Lines);
        Assert.Contains("  platform queried     = no\n", slice.Lines);
        slice.GetTiccmdContribution(out yaw, out pitch);
        Assert.Equal(0f, yaw);
        Assert.Equal(0f, pitch);

        slice.Windows = true;
        slice.InvertYaw = false;
        slice.YawScale = 2.5f;
        slice.PitchScale = 2f;
        InstallOneGyro(slice, pitch: 0.5f, yaw: 0.03125f);
        slice.GetTiccmdContribution(out yaw, out pitch);
        Assert.Equal(0f, yaw);
        Assert.Equal(1f, pitch);

        slice.InvertPitch = true;
        slice.InvertYaw = true;
        slice.GetTiccmdContribution(out yaw, out pitch);
        Assert.Equal(0f, yaw);
        Assert.Equal(-1f, pitch);

        slice.Deadzone = 0.25f;
        InstallOneGyro(slice, pitch: -0.25f, yaw: 0.25f);
        slice.InvertPitch = false;
        slice.InvertYaw = false;
        slice.PitchScale = 2f;
        slice.YawScale = 2f;
        slice.GetTiccmdContribution(out yaw, out pitch);
        Assert.Equal(0.5f, yaw);
        Assert.Equal(-0.5f, pitch);

        slice.Deadzone = 0.25f;
        InstallOneGyro(slice, pitch: -0.125f, yaw: 0.125f);
        slice.GetTiccmdContribution(out yaw, out pitch);
        Assert.Equal(0f, yaw);
        Assert.Equal(0f, pitch);

        slice.Deadzone = -1f;
        InstallOneGyro(slice, pitch: 0.25f, yaw: 0.25f);
        slice.YawScale = 2f;
        slice.PitchScale = 2f;
        slice.GetTiccmdContribution(out yaw, out pitch);
        Assert.Equal(0.5f, yaw);
        Assert.Equal(0.5f, pitch);
    }

    [Fact]
    public void WindowsSensorProbe()
    {
        var slice = new VmGyroInput1361Slice { Enable = true, Windows = true };
        var missingModule = Assert.Throws<InvalidOperationException>(() => slice.PollPlatformSample());
        Assert.Equal("GetModuleHandleA", missingModule.Message);

        var loads = new List<string>();
        slice.GetModuleHandle = _ => IntPtr.Zero;
        slice.LoadLibrary = null;
        var missingLoad = Assert.Throws<InvalidOperationException>(() => slice.PollPlatformSample());
        Assert.Equal("LoadLibraryA", missingLoad.Message);

        slice.LoadLibrary = name =>
        {
            loads.Add(name);
            return IntPtr.Zero;
        };
        var unloaded = slice.PollPlatformSample();
        Assert.Equal("windows-sdl2-not-loaded", unloaded.Provider);
        Assert.True(unloaded.PlatformQueried);
        Assert.False(unloaded.Available);
        Assert.Equal(VmGyroInput1361Slice.SdlLibrary, Assert.Single(loads));

        var module = new IntPtr(42);
        var lookedUp = new List<string>();
        slice.GetModuleHandle = name =>
        {
            Assert.Equal(VmGyroInput1361Slice.SdlLibrary, name);
            return module;
        };
        loads.Clear();
        slice.GetProcAddress = (handle, name) =>
        {
            Assert.Equal(module, handle);
            lookedUp.Add(name);
            return name == "SDL_NumSensors" ? IntPtr.Zero : new IntPtr(1);
        };
        var symbols = slice.PollPlatformSample();
        Assert.Empty(loads);
        Assert.Equal(VmGyroInput1361Slice.SensorSymbols, lookedUp);
        Assert.Equal("windows-sdl2-sensor-symbols-missing", symbols.Provider);
        Assert.True(symbols.PlatformQueried);

        slice.GetProcAddress = (handle, _) =>
        {
            Assert.Equal(module, handle);
            return new IntPtr(1);
        };
        var missingWas = Assert.Throws<InvalidOperationException>(() => slice.PollPlatformSample());
        Assert.Equal("SDL_WasInit", missingWas.Message);

        var initCalls = 0;
        slice.WasInit = flag =>
        {
            Assert.Equal(VmGyroInput1361Slice.SdlInitSensor, flag);
            return VmGyroInput1361Slice.SdlInitSensor | 1u;
        };
        slice.InitSubSystem = _ =>
        {
            initCalls++;
            return 0;
        };
        slice.SensorUpdate = () => { };
        slice.NumSensors = () => 0;
        var none = slice.PollPlatformSample();
        Assert.Equal(0, initCalls);
        Assert.Equal("windows-sdl2-dynamic-sensor", none.Provider);
        Assert.Equal(0, none.SensorCount);
        Assert.False(none.Available);
        Assert.True(none.PlatformQueried);

        var updates = 0;
        slice.WasInit = _ => 0;
        slice.InitSubSystem = flag =>
        {
            Assert.Equal(VmGyroInput1361Slice.SdlInitSensor, flag);
            return 7;
        };
        slice.SensorUpdate = () => updates++;
        var failed = slice.PollPlatformSample();
        Assert.Equal("windows-sdl2-sensor-init-failed", failed.Provider);
        Assert.Equal(0, updates);
        Assert.True(failed.PlatformQueried);

        var opened = new List<int>();
        var closed = new List<object>();
        var dataCounts = new List<int>();
        slice.InitSubSystem = _ => 0;
        slice.NumSensors = () => 5;
        slice.SensorGetDeviceType = i => i == 0 ? 1 : VmGyroInput1361Slice.SdlSensorGyro;
        slice.SensorOpen = i =>
        {
            opened.Add(i);
            return i == 1 ? null : $"sensor-{i}";
        };
        slice.SensorGetData = (sensor, values, count) =>
        {
            dataCounts.Add(count);
            values[0] = 0.5f;
            values[1] = -0.25f;
            values[2] = 99f;
            return sensor.Equals("sensor-2") ? 4 : 0;
        };
        slice.SensorClose = sensor => closed.Add(sensor);
        var hit = slice.PollPlatformSample();
        Assert.Equal(1, updates);
        Assert.Equal(new[] { 1, 2, 3 }, opened);
        Assert.Equal(new object[] { "sensor-2", "sensor-3" }, closed);
        Assert.Equal(new[] { 3, 3 }, dataCounts);
        Assert.True(hit.Available);
        Assert.Equal(5, hit.SensorCount);
        Assert.Equal(-0.25f, hit.YawRate);
        Assert.Equal(0.5f, hit.PitchRate);
        Assert.Equal("windows-sdl2-dynamic-sensor", hit.Provider);

        slice.Lines.Clear();
        slice.YawScale = 2.5f;
        slice.PitchScale = 2f;
        slice.Deadzone = 0.05f;
        slice.InvertYaw = false;
        slice.InvertPitch = false;
        slice.GyroStatus();
        Assert.Contains("  platform provider    = windows-sdl2-dynamic-sensor\n", slice.Lines);
        Assert.Contains("  platform queried     = yes\n", slice.Lines);
        Assert.Contains("  platform sensor count = 5\n", slice.Lines);
        Assert.Contains("  platform sensor available = yes\n", slice.Lines);
        Assert.Contains("  latest platform sample = yaw -0.250 rad/s, pitch 0.500 rad/s\n", slice.Lines);
        slice.GetTiccmdContribution(out var yaw, out var pitch);
        Assert.Equal(-0.625f, yaw);
        Assert.Equal(1f, pitch);
    }

    static void InstallOneGyro(VmGyroInput1361Slice slice, float pitch, float yaw)
    {
        slice.Enable = true;
        slice.Windows = true;
        slice.GetModuleHandle = _ => new IntPtr(1);
        slice.GetProcAddress = (_, _) => new IntPtr(1);
        slice.WasInit = _ => VmGyroInput1361Slice.SdlInitSensor;
        slice.InitSubSystem = _ => 0;
        slice.SensorUpdate = () => { };
        slice.NumSensors = () => 1;
        slice.SensorGetDeviceType = _ => VmGyroInput1361Slice.SdlSensorGyro;
        slice.SensorOpen = _ => "gyro";
        slice.SensorGetData = (_, values, count) =>
        {
            Assert.Equal(3, count);
            values[0] = pitch;
            values[1] = yaw;
            return 0;
        };
        slice.SensorClose = _ => { };
    }
}
