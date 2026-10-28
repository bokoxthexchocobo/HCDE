namespace HCDE.Playsim;

/// <summary><c>FDynamicLight::ReleaseLight</c>, <c>Activate</c>, <c>Tick</c>, and <c>UpdateLocation</c> from <c>a_dynlight.cpp</c>.</summary>
public static class VmADynLightTick
{
    public const int PulseLight = 0;
    public const int FlickerLight = 1;
    public const int RandomFlickerLight = 2;
    public const int SectorLight = 3;
    public const int PointLight = 4;
    public const int Ticrate = 35;
    public const int MfUnmorphed = 1 << 20;
    public const int Mf5Nointeraction = 1;

    public sealed class LightState
    {
        public object? Target { get; set; }
        public bool Owned { get; set; }
        public bool Active { get; set; }
        public int LightType { get; set; }
        public float CurrentRadius { get; set; }
        public int TickCount { get; set; }
        public int LastUpdate { get; set; }
        public float SpecialF1 { get; set; }
        public bool Swapped { get; set; }
        public object? Sector { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public float Radius { get; set; }
        public bool IsSpot { get; set; }
        public bool ExplicitPitch { get; set; }
    }

    public static void ReleaseLight(Action unlinkList, Action destroyInstance, Action returnToPool)
    {
        unlinkList();
        destroyInstance();
        returnToPool();
    }

    public static void Activate(LightState light, Func<int> intensity, Func<int> secondaryIntensity, Func<int> timer, Action<float, float, float, bool> cyclerSet, Func<float>? cyclerVal = null)
    {
        light.Active = true;
        light.CurrentRadius = intensity();
        light.TickCount = 0;

        if (light.LightType == PulseLight)
        {
            var pulseTime = light.SpecialF1 / Ticrate;
            light.LastUpdate = timer();
            if (!light.Swapped)
                cyclerSet(secondaryIntensity(), intensity(), pulseTime, true);
            else
                cyclerSet(intensity(), secondaryIntensity(), pulseTime, true);
            if (cyclerVal != null)
                light.CurrentRadius = cyclerVal();
        }

        if (light.CurrentRadius <= 0)
            light.CurrentRadius = 1;
    }

    public static bool Tick(
        LightState light,
        Func<bool> targetMissing,
        Action unlink,
        Action release,
        Func<bool> shouldRenderLocally,
        Func<bool> hasState,
        Func<int> targetFlags,
        Func<bool> visibleToPlayer,
        Func<int> intensity,
        Func<int> secondaryIntensity,
        Func<int> timer,
        Action<double> cyclerUpdate,
        Func<float> cyclerVal,
        Func<int, int> randLight,
        Func<int> sectorLightLevel,
        Action updateLocation)
    {
        if (targetMissing())
        {
            unlink();
            release();
            return false;
        }

        if (light.Owned)
        {
            if (!hasState() || !shouldRenderLocally())
                return false;
            if ((targetFlags() & MfUnmorphed) != 0)
            {
                light.Active = false;
                return true;
            }

            visibleToPlayer();
        }

        if (!light.Active)
            return true;

        switch (light.LightType)
        {
            case PulseLight:
            {
                var t = timer();
                var diff = (t - light.LastUpdate) / (float)Ticrate;
                light.LastUpdate = t;
                cyclerUpdate(diff);
                light.CurrentRadius = cyclerVal();
                break;
            }
            case FlickerLight:
            {
                var rnd = randLight(360);
                light.CurrentRadius = rnd < (int)light.SpecialF1 ? intensity() : secondaryIntensity();
                break;
            }
            case RandomFlickerLight:
            {
                var flickerRange = secondaryIntensity() - intensity();
                var amt = randLight(0) / 255f;
                if (light.TickCount > light.SpecialF1)
                    light.TickCount = 0;
                if (light.TickCount++ == 0 || light.CurrentRadius > secondaryIntensity())
                    light.CurrentRadius = intensity() + amt * flickerRange;
                break;
            }
            case SectorLight:
            {
                var scale = intensity() / 8f;
                if (scale == 0)
                    scale = 1;
                var lvl = light.Sector != null ? sectorLightLevel() * scale : 0;
                light.CurrentRadius = Math.Clamp(lvl, 0, 255);
                break;
            }
            case PointLight:
                light.CurrentRadius = intensity();
                break;
        }

        if (light.CurrentRadius <= 0)
            light.CurrentRadius = 1;
        updateLocation();
        return true;
    }

    public static (double x, double y) RotateOffset(double yawSin, double yawCos, double offX, double offY) =>
        (offX * yawCos + offY * yawSin, offX * yawSin - offY * yawCos);

    public static double ClampZBetweenPlanes(double z, double floorZ, double ceilingZ, bool noInteraction)
    {
        if (noInteraction)
            return z;
        if (z < floorZ + 5)
            return floorZ + 5;
        if (z > ceilingZ - 5)
            return ceilingZ - 5;
        return z;
    }

    public static float RelinkRadius(int lightType, int intensity, int secondaryIntensity, float currentRadius)
    {
        float intensityMax;
        if (lightType == FlickerLight || lightType == RandomFlickerLight || lightType == PulseLight)
            intensityMax = Math.Max(intensity, secondaryIntensity);
        else
            intensityMax = currentRadius;

        var radius = intensityMax * 2.0f;
        if (radius < currentRadius * 2)
            radius = currentRadius * 2;
        return radius;
    }

    public static bool ShouldRelink(double oldX, double oldY, float oldRadius, double newX, double newY, float newRadius) =>
        newX != oldX || newY != oldY || newRadius != oldRadius;

    public static void UpdateLocation(
        LightState light,
        Func<bool> isActive,
        Func<(double x, double y, double z, double floorZ, double ceilingZ, int targetFlags, double yawSin, double yawCos, double bobZ, double offX, double offY, double offZ)> targetSample,
        Func<int> intensity,
        Func<int> secondaryIntensity,
        Action<double, double, float, float> relink)
    {
        var oldX = light.X;
        var oldY = light.Y;
        var oldRadius = light.Radius;
        if (!isActive())
            return;

        var sample = targetSample();
        var (rotX, rotY) = RotateOffset(sample.yawSin, sample.yawCos, sample.offX, sample.offY);
        light.X = sample.x + rotX;
        light.Y = sample.y + rotY;
        light.Z = ClampZBetweenPlanes(sample.z + sample.offZ + sample.bobZ, sample.floorZ, sample.ceilingZ,
            (sample.targetFlags & Mf5Nointeraction) != 0);

        light.Radius = RelinkRadius(light.LightType, intensity(), secondaryIntensity(), light.CurrentRadius);
        if (ShouldRelink(oldX, oldY, oldRadius, light.X, light.Y, light.Radius))
            relink(oldX, oldY, oldRadius, light.Radius);
    }
}
