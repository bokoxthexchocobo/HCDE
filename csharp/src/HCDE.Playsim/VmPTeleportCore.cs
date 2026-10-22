namespace HCDE.Playsim;

/// <summary><c>P_SpawnTeleportFog</c> and <c>P_Teleport</c> from <c>p_teleport.cpp</c> (45–255).</summary>
public static class VmPTeleportCore
{
    public const double OnFloorZ = -32768;
    public const int MfMissile = 0x00020000;
    public const int MfNogravity = 0x00000400;
    public const int TelfKeepheight = 0x00000002;
    public const int TelfKeeporientation = 0x00000004;
    public const int TelfSourcefog = 0x00000008;
    public const int TelfDestfog = 0x00000010;
    public const int TelfKeepvelocity = 0x00000020;
    public const int TelfFdcompat = 0x00000040;
    public const double Telefogheight = 32;

    public sealed class SpawnTeleportFogState
    {
        public bool BeforeTele { get; set; }
        public bool SetTarget { get; set; }
        public bool FogTypeNull { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public bool IsMissile { get; set; }
        public object? SpawnedFog { get; set; }
        public Func<double, double, double, object?>? SpawnFog { get; init; }
    }

    public static void SpawnTeleportFog(SpawnTeleportFogState s)
    {
        if (s.FogTypeNull)
        {
            s.SpawnedFog = null;
            return;
        }

        var fogDelta = s.IsMissile ? 0.0 : Telefogheight;
        s.SpawnedFog = s.SpawnFog?.Invoke(s.PosX, s.PosY, s.PosZ + fogDelta);
    }

    public sealed class TeleportState
    {
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double RequestedYawDegrees { get; set; }
        public double YawDegrees { get; set; }
        public double PitchDegrees { get; set; }
        public int Flags { get; set; }
        public double ThingHeight { get; set; }
        public double FloorHeight { get; set; }
        public double CeilingHeight { get; set; }
        public double OldX { get; set; }
        public double OldY { get; set; }
        public double OldZ { get; set; }
        public double AboveFloor { get; set; }
        public double MissileSpeed { get; set; }
        public bool IsMissile { get; set; }
        public bool NoGravity { get; set; }
        public bool HasPlayer { get; set; }
        public bool PlayerIsMo { get; set; }
        public bool Predicting { get; set; }
        public bool CompatFdTeleport { get; set; }
        public bool ResetPitch { get; set; }
        public bool Telezoom { get; set; }
        public bool IsConsolePlayerMo { get; set; }
        public double ViewZ { get; set; }
        public double ViewHeight { get; set; }
        public double DesiredFov { get; set; }
        public double Fov { get; set; }
        public int ReactionTime { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double PlayerVelX { get; set; }
        public double PlayerVelY { get; set; }
        public Func<bool>? PreTeleport { get; init; }
        public Func<double, double, double, bool>? TeleportMove { get; init; }
        public Action? PlayerTeleportedRumble { get; init; }
        public Action<double, double, double>? SpawnSourceFog { get; init; }
        public Func<double, double, double, double, (double X, double Y)>? PortalOffsetFog { get; init; }
        public Action<double, double, double>? SpawnDestFog { get; init; }
        public Func<int>? GetTeleportFreezeTime { get; init; }
        public Action<double>? VelFromAngle { get; init; }
        public Action<double, double, double, double, int>? PostTeleport { get; init; }
    }

    public static bool Teleport(TeleportState s)
    {
        s.OldX = s.PosX;
        s.OldY = s.PosY;
        s.OldZ = s.PosZ;
        s.AboveFloor = s.PosZ - s.FloorHeight;

        if (s.IsMissile)
            s.MissileSpeed = Math.Sqrt(s.VelX * s.VelX + s.VelY * s.VelY);

        if ((s.Flags & TelfKeepheight) != 0)
            s.PosZ = s.FloorHeight + s.AboveFloor;
        else if (s.PosZ == OnFloorZ)
        {
            if (s.HasPlayer && s.PlayerIsMo)
            {
                if (s.NoGravity && s.AboveFloor > 0)
                {
                    s.PosZ = s.FloorHeight + s.AboveFloor;
                    if (s.PosZ + s.ThingHeight > s.CeilingHeight)
                        s.PosZ = s.CeilingHeight - s.ThingHeight;
                }
                else
                {
                    if (!s.CompatFdTeleport || (s.Flags & TelfFdcompat) == 0 || s.FloorHeight > s.PosZ)
                        s.PosZ = s.FloorHeight;
                    else
                        s.PosZ = s.OldZ;

                    if ((s.Flags & TelfKeeporientation) == 0)
                        s.ResetPitch = false;
                }
            }
            else if (s.IsMissile)
            {
                s.PosZ = s.FloorHeight + s.AboveFloor;
                if (s.PosZ + s.ThingHeight > s.CeilingHeight)
                    s.PosZ = s.CeilingHeight - s.ThingHeight;
            }
            else
            {
                if (!s.CompatFdTeleport || (s.Flags & TelfFdcompat) == 0 || !s.NoGravity || s.FloorHeight > s.PosZ)
                    s.PosZ = s.FloorHeight;
                else
                    s.PosZ = s.OldZ;
            }
        }

        if (s.PreTeleport?.Invoke() == false)
            return false;

        if (s.TeleportMove?.Invoke(s.PosX, s.PosY, s.PosZ) != true)
            return false;

        if (s.HasPlayer && s.PlayerIsMo)
        {
            s.ViewZ = s.PosZ + s.ViewHeight;
            if (s.ResetPitch)
                s.PitchDegrees = 0;
            if (s.IsConsolePlayerMo)
                s.PlayerTeleportedRumble?.Invoke();
        }

        if ((s.Flags & TelfKeeporientation) == 0)
            s.YawDegrees = s.RequestedYawDegrees;
        else
            s.RequestedYawDegrees = s.YawDegrees;

        if ((s.Flags & TelfSourcefog) != 0 && !s.Predicting)
            s.SpawnSourceFog?.Invoke(s.OldX, s.OldY, s.OldZ);

        if ((s.Flags & TelfDestfog) != 0)
        {
            if (!s.Predicting)
            {
                var fog = s.PortalOffsetFog?.Invoke(s.PosX, s.PosY, 20, s.YawDegrees) ?? (s.PosX, s.PosY);
                s.SpawnDestFog?.Invoke(fog.X, fog.Y, s.PosZ);
            }

            if (s.HasPlayer && s.PlayerIsMo && s.Telezoom && (s.Flags & TelfKeepvelocity) == 0)
                s.Fov = Math.Min(175, s.DesiredFov + 45);
        }

        if (s.HasPlayer && s.PlayerIsMo &&
            (((s.Flags & TelfDestfog) != 0) || (s.Flags & TelfKeeporientation) == 0) &&
            (s.Flags & TelfKeepvelocity) == 0)
            s.ReactionTime = s.GetTeleportFreezeTime?.Invoke() ?? 18;

        if (s.IsMissile)
            s.VelFromAngle?.Invoke(s.MissileSpeed);
        else if ((s.Flags & TelfKeeporientation) == 0 && (s.Flags & TelfKeepvelocity) == 0)
        {
            s.VelX = 0;
            s.VelY = 0;
            if (s.HasPlayer && s.PlayerIsMo)
            {
                s.PlayerVelX = 0;
                s.PlayerVelY = 0;
            }
        }

        s.PostTeleport?.Invoke(s.PosX, s.PosY, s.PosZ, s.YawDegrees, s.Flags);
        return true;
    }
}
