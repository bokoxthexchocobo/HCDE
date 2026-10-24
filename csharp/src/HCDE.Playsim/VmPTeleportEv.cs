namespace HCDE.Playsim;

/// <summary>Line and group teleport specials from <c>p_teleport.cpp</c> (376–806).</summary>
public static class VmPTeleportEv
{
    public const int Mf2Noteleport = 0x08000000;
    public const int TelfRotateboom = 0x00000100;
    public const int TelfRotateboominverse = 0x00000200;
    public const int TelfDestfog = 0x00000010;
    public const int TelfSourcefog = 0x00000008;
    public const int TelfKeeporientation = 0x00000004;
    public const double OnFloorZ = VmPTeleportCore.OnFloorZ;
    public const double EqualEpsilon = 1.0 / 65536;
    public const int FudgeFactor = 10;

    public sealed class EvTeleportState
    {
        public bool ThingNull { get; set; }
        public int Flags2 { get; set; }
        public int Side { get; set; }
        public int Tid { get; set; }
        public int Tag { get; set; }
        public bool HasLine { get; set; }
        public double LineDeltaAngle { get; set; }
        public bool DestIsTeleportDest2 { get; set; }
        public double DestZ { get; set; }
        public double DestX { get; set; }
        public double DestY { get; set; }
        public double DestYaw { get; set; }
        public bool BadAnglesCompat { get; set; }
        public bool HasPlayerMo { get; set; }
        public bool Predicting { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double YawDegrees { get; set; }
        public int Flags { get; set; }
        public Func<int, int, bool, bool, object?>? SelectTeleDest { get; init; }
        public Func<double, bool>? Teleport { get; init; }
        public Action? PlayIdle { get; init; }
    }

    public static bool EvTeleport(EvTeleportState s)
    {
        if (s.ThingNull)
            return false;
        if ((s.Flags2 & Mf2Noteleport) != 0)
            return false;
        if (s.Side != 0)
            return false;

        var searcher = s.SelectTeleDest?.Invoke(s.Tid, s.Tag, false, s.HasPlayerMo);
        if (searcher == null)
            return false;

        double z;
        double rotateAngle = 0;
        double vx = 0, vy = 0;
        var badangle = 0.0;

        if (s.HasLine && (s.Flags & (TelfRotateboom | TelfRotateboominverse)) != 0)
        {
            rotateAngle = s.LineDeltaAngle - s.DestYaw + 90;
            if ((s.Flags & TelfRotateboominverse) != 0)
                rotateAngle = -rotateAngle;
            vx = s.VelX;
            vy = s.VelY;
            z = s.DestZ;
        }
        else if (s.DestIsTeleportDest2)
            z = s.DestZ;
        else
            z = OnFloorZ;

        if (s.BadAnglesCompat && s.HasPlayerMo)
            badangle = 0.01;

        if (s.Teleport?.Invoke(s.DestYaw + badangle) != true)
            return false;

        if (s.HasLine && (s.Flags & (TelfRotateboom | TelfRotateboominverse)) != 0)
        {
            s.YawDegrees += rotateAngle;
            s.VelX = vx * Math.Cos(rotateAngle * Math.PI / 180) - vy * Math.Sin(rotateAngle * Math.PI / 180);
            s.VelY = vy * Math.Cos(rotateAngle * Math.PI / 180) + vx * Math.Sin(rotateAngle * Math.PI / 180);
        }

        if (vx == 0 && vy == 0 && s.HasPlayerMo && !s.Predicting)
            s.PlayIdle?.Invoke();

        return true;
    }

    public sealed class TeleportOtherState
    {
        public int OtherTid { get; set; }
        public int DestTid { get; set; }
        public bool Fog { get; set; }
        public IEnumerable<object>? Victims { get; set; }
        public Func<object, bool>? TeleportVictim { get; init; }
    }

    public static bool EvTeleportOther(TeleportOtherState s)
    {
        var did = false;
        if (s.OtherTid == 0 || s.DestTid == 0)
            return false;
        foreach (var victim in s.Victims ?? Array.Empty<object>())
            did |= s.TeleportVictim?.Invoke(victim) == true;
        return did;
    }

    public sealed class DoGroupForOneState
    {
        public double VictimX { get; set; }
        public double VictimY { get; set; }
        public double VictimZ { get; set; }
        public double VictimYaw { get; set; }
        public double SourceX { get; set; }
        public double SourceY { get; set; }
        public double SourceZ { get; set; }
        public double SourceYaw { get; set; }
        public double DestX { get; set; }
        public double DestY { get; set; }
        public double DestZ { get; set; }
        public double DestYaw { get; set; }
        public bool FloorZ { get; set; }
        public bool Fog { get; set; }
        public bool TeleportOk { get; set; }
        public Func<double, double, double, int, bool>? Teleport { get; init; }
    }

    public static bool DoGroupForOne(DoGroupForOneState s)
    {
        var an = (s.DestYaw - s.SourceYaw) * Math.PI / 180;
        var offX = s.VictimX - s.SourceX;
        var offY = s.VictimY - s.SourceY;
        var cos = Math.Cos(an);
        var sin = Math.Sin(an);
        var newX = offX * cos - offY * sin;
        var newY = offX * sin + offY * cos;
        var z = s.FloorZ ? OnFloorZ : s.DestZ + s.VictimZ - s.SourceZ;
        var flags = s.Fog
            ? TelfDestfog | TelfSourcefog | TelfKeeporientation
            : TelfKeeporientation;
        s.TeleportOk = s.Teleport?.Invoke(s.DestX + newX, s.DestY + newY, z, flags) == true;
        s.VictimYaw = s.DestYaw + s.VictimYaw - s.SourceYaw;
        return s.TeleportOk;
    }

    public sealed class TeleportGroupState
    {
        public int GroupTid { get; set; }
        public int SourceTid { get; set; }
        public int DestTid { get; set; }
        public bool MoveSource { get; set; }
        public bool Fog { get; set; }
        public bool DestIsTeleportDest2 { get; set; }
        public object? SourceOrigin { get; set; }
        public object? DestOrigin { get; set; }
        public object? SingleVictim { get; set; }
        public IEnumerable<object>? GroupVictims { get; set; }
        public Func<int, object?>? FindActor { get; init; }
        public Func<int, object?>? FindTeleportDest { get; init; }
        public Func<int, int, bool, bool>? TeleportOtherFallback { get; init; }
        public Func<object, object, object, bool, bool, bool>? GroupOne { get; init; }
        public Func<object, object, bool>? MoveSourceOrigin { get; init; }
    }

    public static bool EvTeleportGroup(TeleportGroupState s)
    {
        s.SourceOrigin = s.FindActor?.Invoke(s.SourceTid);
        if (s.SourceOrigin == null)
            return s.TeleportOtherFallback?.Invoke(s.GroupTid, s.DestTid, s.Fog) == true;

        s.DestOrigin = s.FindTeleportDest?.Invoke(s.DestTid);
        if (s.DestOrigin == null)
            return false;

        var did = false;
        var floorz = !s.DestIsTeleportDest2;

        if (s.GroupTid == 0 && s.SingleVictim != null)
            did = s.GroupOne?.Invoke(s.SingleVictim, s.SourceOrigin, s.DestOrigin, floorz, s.Fog) == true;
        else
        {
            foreach (var victim in s.GroupVictims ?? Array.Empty<object>())
                did |= s.GroupOne?.Invoke(victim, s.SourceOrigin, s.DestOrigin, floorz, s.Fog) == true;
        }

        if (s.MoveSource && did)
            did |= s.MoveSourceOrigin?.Invoke(s.SourceOrigin, s.DestOrigin) == true;

        return did;
    }
}
