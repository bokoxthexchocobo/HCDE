namespace HCDE.Playsim;

/// <summary>Fillcolor shade, pitch clamp, actor/view angles, and FOV from <c>p_mobj.cpp</c> (4021–4189).</summary>
public static class VmPMobjOrientationShade
{
    public const int SpfForceclamp = 1;
    public const int SpfScalednolerp = 2;
    public const int SpfInterpolate = 4;
    public const int CfScalednolerp = 1;
    public const int CfInterpview = 2;
    public const int CfInterpviewangles = 4;
    public const int CfNofovinterp = 8;

    public sealed class PlayerAngles
    {
        public double Pitch { get; set; }
        public double Yaw { get; set; }
        public double Roll { get; set; }
        public double MinPitch { get; set; } = -89;
        public double MaxPitch { get; set; } = 89;
        public int Cheats { get; set; }
        public double Fov { get; set; } = 90;
        public AngleOffsetTargets OffsetTargets { get; } = new();
    }

    public sealed class AngleOffsetTargets
    {
        public double Pitch { get; set; }
        public double Yaw { get; set; }
        public double Roll { get; set; }
    }

    public sealed class OrientationActor
    {
        public PlayerAngles? Player { get; set; }
        public uint Fillcolor { get; set; }
        public double Pitch { get; set; }
        public double Yaw { get; set; }
        public double Roll { get; set; }
        public double ViewPitch { get; set; }
        public double ViewYaw { get; set; }
        public double CameraFov { get; set; } = 90;
        public double PrevFov { get; set; }
    }

    public static uint PackShadeRgb(uint rgb, Func<byte, byte, byte, byte>? colorMatcherPick)
    {
        var r = (byte)(rgb & 0xff);
        var g = (byte)((rgb >> 8) & 0xff);
        var b = (byte)((rgb >> 16) & 0xff);
        var pick = colorMatcherPick?.Invoke(r, g, b) ?? (byte)0;
        return (rgb & 0xffffff) | ((uint)pick << 24);
    }

    public static void SetShade(OrientationActor self, uint rgb, Func<byte, byte, byte, byte>? colorMatcherPick)
    {
        self.Fillcolor = PackShadeRgb(rgb, colorMatcherPick);
    }

    public static void SetShade(OrientationActor self, int r, int g, int b, Func<byte, byte, byte, byte>? colorMatcherPick)
    {
        var pick = colorMatcherPick?.Invoke((byte)r, (byte)g, (byte)b) ?? (byte)0;
        self.Fillcolor = (uint)(((pick << 24) | (r << 16) | (g << 8) | b) & 0xffffffff);
    }

    public static double ClampPitch(OrientationActor self, double pitch)
    {
        if (self.Player != null)
            return Math.Clamp(pitch, self.Player.MinPitch, self.Player.MaxPitch);
        return Math.Clamp(pitch, -89, 89);
    }

    public static void SetPitch(OrientationActor self, double pitch, int fflags, Func<double, double, double>? deltaAngle)
    {
        if (self.Player != null || (fflags & SpfForceclamp) != 0)
            pitch = ClampPitch(self, pitch);

        if (pitch == self.Pitch)
            return;

        if (self.Player != null)
        {
            if ((fflags & SpfScalednolerp) != 0)
            {
                self.Player.OffsetTargets.Pitch = deltaAngle?.Invoke(self.Pitch, pitch) ?? pitch - self.Pitch;
                self.Player.Cheats |= CfScalednolerp;
            }
            else
            {
                self.Pitch = pitch;
                if ((fflags & SpfInterpolate) != 0)
                    self.Player.Cheats |= CfInterpview;
            }
        }
        else
        {
            self.Pitch = pitch;
        }
    }

    public static void SetAngle(OrientationActor self, double ang, int fflags, Func<double, double, double>? deltaAngle)
    {
        if (ang == self.Yaw)
            return;

        if (self.Player != null)
        {
            if ((fflags & SpfScalednolerp) != 0)
            {
                self.Player.OffsetTargets.Yaw = deltaAngle?.Invoke(self.Yaw, ang) ?? ang - self.Yaw;
                self.Player.Cheats |= CfScalednolerp;
            }
            else
            {
                self.Yaw = ang;
                if ((fflags & SpfInterpolate) != 0)
                    self.Player.Cheats |= CfInterpview;
            }
        }
        else
        {
            self.Yaw = ang;
        }
    }

    public static void SetRoll(OrientationActor self, double roll, int fflags, Func<double, double, double>? deltaAngle)
    {
        if (roll == self.Roll)
            return;

        if (self.Player != null)
        {
            if ((fflags & SpfScalednolerp) != 0)
            {
                self.Player.OffsetTargets.Roll = deltaAngle?.Invoke(self.Roll, roll) ?? roll - self.Roll;
                self.Player.Cheats |= CfScalednolerp;
            }
            else
            {
                self.Roll = roll;
                if ((fflags & SpfInterpolate) != 0)
                    self.Player.Cheats |= CfInterpview;
            }
        }
        else
        {
            self.Roll = roll;
        }
    }

    public static void SetViewPitch(OrientationActor self, double pitch, int fflags)
    {
        if (self.Player != null || (fflags & SpfForceclamp) != 0)
            pitch = ClampPitch(self, pitch);

        if (pitch == self.ViewPitch)
            return;

        self.ViewPitch = pitch;
        if (self.Player != null && (fflags & SpfInterpolate) != 0)
            self.Player.Cheats |= CfInterpviewangles;
    }

    public static void SetViewAngle(OrientationActor self, double ang, int fflags)
    {
        if (ang == self.ViewYaw)
            return;

        self.ViewYaw = ang;
        if (self.Player != null && (fflags & SpfInterpolate) != 0)
            self.Player.Cheats |= CfInterpviewangles;
    }

    public static double GetFov(OrientationActor self, double ticFrac)
    {
        if (self.Player != null)
        {
            if ((self.Player.Cheats & CfNofovinterp) != 0)
                return self.Player.Fov;
            return self.PrevFov * (1 - ticFrac) + self.Player.Fov * ticFrac;
        }
        return self.PrevFov * (1 - ticFrac) + self.CameraFov * ticFrac;
    }
}
