namespace HCDE.Playsim;

/// <summary>
/// Rail-attack tail, camera offset, and talk facing from <c>p_map.cpp</c> (<b>5568–5821</b>). Chunk <b>1171</b>.
/// </summary>
public static class VmPMapRailTalk1171Slice
{
    public const int PfHitThing = 1;
    public const int PfHitThingBleed = 8;
    public const int TraceHitFloor = 1;
    public const int TraceHitCeiling = 2;
    public const int TraceHitWall = 3;
    public const int MfShootable = 0x00000004;
    public const int MfNoBlood = 0x00080000;
    public const int MlBlockEverything = 0x00008000;
    public const int LineHorizon = 9;
    public const int Mf2Invulnerable = 0x08000000;
    public const int Mf2Dormant = 0x10000000;
    public const int Mf3AlwaysPuff = 0x00000010;
    public const int Mf3SkyExplode = 0x00004000;
    public const int Mf3BloodlessImpact = 0x08000000;
    public const int Mf3FoilInvul = 0x02000000;
    public const int Mf4InCombat = 0x00008000;
    public const int Mf7FoilBuddha = 0x00000080;
    public const int Mf7ForceDecal = 0x00080000;
    public const int Mf8AllowThruBits = 0x00008000;
    public const int DmgInflictorIsPuff = 2;
    public const int DmgPlayerAttack = 32;
    public const int DmgFoilInvul = 64;
    public const int DmgFoilBuddha = 128;
    public const int DmgUseAngle = 512;
    public const int DmgRailgun = 8192;
    public const int AllowReplace = 1;
    public const int AlfForceNoSmart = 1;
    public const int AlfCheckConversation = 8;
    public const int AlfPortalRestrict = 32;
    public const double TalkRange = 128d;
    public const double ChaseHeight = -8d;
    public const double ChaseDist = 90d;
    public const string NetLimitMessage = "Only settings controllers can start conversations with NPCs\n";

    public readonly record struct Vec3(double X, double Y, double Z);

    public sealed class Puff
    {
        public string Species = "None";
        public int ThruBits;
        public int Flags3;
        public int Flags8;
        public int FloorPic;
        public int CeilingPic;
        public bool Destroyed;
    }

    public sealed class PuffDefaults
    {
        public int Flags3;
        public int Flags7;
        public bool HasDecal;
        public int PoisonDamage;
        public int PoisonDuration;
        public int PoisonPeriod;
        public string PoisonDamageType = "None";
    }

    public sealed class RailHit
    {
        public object Actor = "actor";
        public int Flags;
        public int Flags2;
        public Vec3 Pos;
        public double Angle;
    }

    public sealed class TraceEnd
    {
        public int HitType;
        public Vec3 HitPos;
        public double SrcAngle;
        public object? Line = "line";
        public int LineSpecial;
        public bool CrossedWater;
        public bool Crossed3DWater;
        public object? Sector = "sector";
        public bool HeightSec;
    }

    public sealed class RailState
    {
        public bool UseThruBits;
        public int ThruBits;
        public string PuffSpecies = "";
    }

    public sealed class RailHost
    {
        public Puff? Probe;
        public int ReplaceMode = -1;
        public int SpawnFlags3;
        public int SpawnFloorPic;
        public int SpawnCeilingPic = 1;
        public int DamageResult;
        public List<Vec3> SectorLooks { get; } = new();
        public object? TracedSector;
        public Vec3 TracedDirection;
        public double TracedDistance;
        public uint ActorMask;
        public uint WallMask;
        public List<RailHit> Hits { get; } = new();
        public TraceEnd End = new();
        public List<SpawnCall> Spawns { get; } = new();
        public List<string> Order { get; } = new();
        public object? PoisonVictim;
        public object? PoisonInflictor;
        public object? PoisonSource;
        public int PoisonDamage = -1;
        public int AppliedFlags;
        public object? AppliedInflictor;
        public int AppliedDamage = -1;
        public double AppliedAngle;
        public int BloodAmount = -1;
        public Vec3 BloodAt;
        public int BleedAmount = -1;
        public Vec3 BleedAt;
        public bool Geometry;
        public List<(string Who, string Source)> Decals { get; } = new();
        public Puff? FiredPuff;
        public Vec3 FiredFrom;
        public Vec3 FiredHit;
        public int FiredFlags = -1;
        public bool Splash;
        public bool Water;
        public Puff? DestroyedProbe;
        public object? TrailPortals;
        public int TrailColor1;
        public int TrailColor2;
        public int TrailMaxDiff;
        public int TrailDuration;
        public int TrailSparsity;
        public int TrailDrift;
        public int TrailSpiral;
        public string TrailClass = "";
        public double TrailAngle;
        public double TrailPitch;
        public int SkyFlat = 77;

        public Puff? SpawnProbe(string puffClass, Vec3 pos)
        {
            ReplaceMode = AllowReplace;
            Order.Add("probe:" + puffClass + "@" + pos.X);
            return Probe;
        }

        public Puff SpawnHit(Vec3 pos, double hitDir, double particleDir, int upDown, int flags, object? victim)
        {
            var puff = new Puff { Flags3 = SpawnFlags3, FloorPic = SpawnFloorPic, CeilingPic = SpawnCeilingPic };
            Spawns.Add(new SpawnCall(pos, hitDir, particleDir, upDown, flags, victim, puff));
            return puff;
        }
    }

    public readonly record struct SpawnCall(Vec3 Pos, double HitDir, double ParticleDir, int UpDown, int Flags, object? Victim, Puff Puff);

    public static void RailTail(
        object source,
        bool player,
        Vec3 sourcePos,
        object? sourceSector,
        string? puffClass,
        PuffDefaults defaults,
        RailState state,
        Vec3 start,
        Vec3 direction,
        double distance,
        int traceFlags,
        int puffFlags,
        int damage,
        string damageType,
        double pitch,
        double angle,
        int railFlags,
        int color1,
        int color2,
        int maxDiff,
        int duration,
        int sparsity,
        int drift,
        int spiral,
        string spawnClass,
        object portalHits,
        RailHost host)
    {
        Puff? thepuff = null;
        if (puffClass != null)
        {
            thepuff = host.SpawnProbe(puffClass, sourcePos);
        }

        state.PuffSpecies = thepuff != null ? thepuff.Species : "None";
        if (thepuff != null)
        {
            state.ThruBits = thepuff.ThruBits;
            if (state.UseThruBits)
            {
                state.UseThruBits = (thepuff.Flags8 & Mf8AllowThruBits) != 0;
            }
        }

        host.SectorLooks.Add(start);
        host.TracedSector = sourceSector;
        host.TracedDirection = direction;
        host.TracedDistance = distance;
        host.ActorMask = MfShootable;
        host.WallMask = MlBlockEverything;
        host.Order.Add("trace:" + traceFlags);
        foreach (var hit in host.Hits)
        {
            var spawnPuff = false;
            var bleed = false;
            var actorFlags = puffFlags | PfHitThing;
            var blocked = (hit.Flags & MfNoBlood) != 0
                || (hit.Flags2 & Mf2Dormant) != 0
                || ((hit.Flags2 & Mf2Invulnerable) != 0 && (defaults.Flags3 & Mf3FoilInvul) == 0);
            if (blocked)
            {
                spawnPuff = puffClass != null;
            }
            else
            {
                spawnPuff = puffClass != null && (defaults.Flags3 & Mf3AlwaysPuff) != 0;
                actorFlags |= PfHitThingBleed;
                if ((defaults.Flags3 & Mf3BloodlessImpact) == 0)
                {
                    bleed = true;
                }
            }

            Puff? hitPuff = null;
            if (spawnPuff)
            {
                hitPuff = host.SpawnHit(hit.Pos, hit.Angle, hit.Angle - 90d, 1, actorFlags, hit.Actor);
            }

            if (hitPuff == null)
            {
                hitPuff = thepuff;
            }

            var damageFlags = DmgInflictorIsPuff;
            if (defaults.PoisonDamage > 0 && defaults.PoisonDuration != int.MinValue)
            {
                var inflictor = hitPuff ?? source;
                host.PoisonVictim = hit.Actor;
                host.PoisonInflictor = inflictor;
                host.PoisonSource = source;
                host.PoisonDamage = defaults.PoisonDamage;
                host.Order.Add("poison");
            }

            if ((defaults.Flags3 & Mf3FoilInvul) != 0)
            {
                damageFlags |= DmgFoilInvul;
            }

            if ((defaults.Flags7 & Mf7FoilBuddha) != 0)
            {
                damageFlags |= DmgFoilBuddha;
            }

            if (player)
            {
                damageFlags |= DmgPlayerAttack;
            }

            damageFlags |= DmgUseAngle | DmgRailgun;
            var newDamage = host.DamageResult;
            host.AppliedFlags = damageFlags;
            host.AppliedInflictor = hitPuff ?? source;
            host.AppliedDamage = newDamage;
            host.AppliedAngle = hit.Angle;
            host.Order.Add("damage");
            if (bleed)
            {
                var amount = newDamage > 0 ? newDamage : damage;
                host.BloodAmount = amount;
                host.BloodAt = hit.Pos;
                host.BleedAmount = amount;
                host.BleedAt = hit.Pos;
                host.Order.Add("blood");
            }
        }

        host.Geometry = true;
        host.Order.Add("geometry");
        if (host.End.HitType == TraceHitWall)
        {
            Puff? puff = null;
            if (puffClass != null && (defaults.Flags3 & Mf3AlwaysPuff) != 0)
            {
                puff = host.SpawnHit(host.End.HitPos, host.End.SrcAngle, host.End.SrcAngle - 90d, 1, puffFlags, null);
                if (puff != null && host.End.Line != null && host.End.LineSpecial == LineHorizon && (puff.Flags3 & Mf3SkyExplode) == 0)
                {
                    puff.Destroyed = true;
                }
            }

            if ((defaults.Flags7 & Mf7ForceDecal) != 0 && defaults.HasDecal)
            {
                host.Decals.Add(("defaults", "defaults"));
            }
            else
            {
                host.Decals.Add(("source", "source"));
            }
        }

        if (host.End.HitType == TraceHitFloor || host.End.HitType == TraceHitCeiling)
        {
            if (puffClass != null && (defaults.Flags3 & Mf3AlwaysPuff) != 0)
            {
                var puff = host.SpawnHit(host.End.HitPos, host.End.SrcAngle, host.End.SrcAngle - 90d, 1, puffFlags, null);
                var sky = (host.End.HitType == TraceHitFloor && puff.FloorPic == host.SkyFlat)
                    || (host.End.HitType == TraceHitCeiling && puff.CeilingPic == host.SkyFlat);
                if ((puff.Flags3 & Mf3SkyExplode) == 0 && sky)
                {
                    puff.Destroyed = true;
                }
            }
        }

        host.FiredPuff = thepuff;
        host.FiredFrom = start;
        host.FiredHit = host.End.HitPos;
        host.FiredFlags = railFlags;
        host.Order.Add("fired");
        if (thepuff != null)
        {
            if (host.End.Crossed3DWater || host.End.CrossedWater)
            {
                host.Splash = true;
                host.Order.Add("splash");
            }
            else if (host.End.HitType == TraceHitFloor && !host.End.HeightSec)
            {
                host.Water = true;
                host.Order.Add("water");
            }

            thepuff.Destroyed = true;
            host.DestroyedProbe = thepuff;
            host.Order.Add("destroy");
        }

        host.TrailPortals = portalHits;
        host.TrailColor1 = color1;
        host.TrailColor2 = color2;
        host.TrailMaxDiff = maxDiff;
        host.TrailDuration = duration;
        host.TrailSparsity = sparsity;
        host.TrailDrift = drift;
        host.TrailSpiral = spiral;
        host.TrailClass = spawnClass;
        host.TrailAngle = angle;
        host.TrailPitch = pitch;
        host.Order.Add("trail");
    }

    public sealed class SectorNode
    {
        public bool FloorBlocks;
        public bool CeilingBlocks;
        public double FloorPlane;
        public double CeilingPlane;
        public Vec3 FloorShift;
        public Vec3 CeilingShift;
        public SectorNode? FloorDest;
        public SectorNode? CeilingDest;
        public string Name = "";
    }

    public sealed class ViewPoint
    {
        public Vec3 Pos;
        public object? Sector;
        public double Yaw;
        public bool AllowOutOfBounds;
        public string SectorFrom = "";
        public int PortalHops;
    }

    public sealed class ViewTrace
    {
        public bool Hit;
        public Vec3 HitPos;
        public Vec3 HitVector;
        public double Distance;
        public double SrcAngle;
        public object? Sector;
    }

    public sealed class ViewHost
    {
        public bool Hardware;
        public ViewTrace Trace = new();
        public int Traces;
        public SectorNode Render = new() { Name = "render" };

        public SectorNode RenderSector(Vec3 pos)
        {
            Render.Name = "render@" + pos.Z;
            return Render;
        }
    }

    public static void OffsetView(ViewPoint view, Vec3 dir, double distance, ViewHost host)
    {
        var baseYaw = AngleOf(dir);
        if (view.AllowOutOfBounds && host.Hardware)
        {
            view.Pos = new Vec3(view.Pos.X + (dir.X * distance), view.Pos.Y + (dir.Y * distance), view.Pos.Z + (dir.Z * distance));
            view.Sector = host.RenderSector(view.Pos);
            view.SectorFrom = "render";
        }
        else if (host.Trace.Hit)
        {
            host.Traces++;
            var scale = Math.Min(5d, host.Trace.Distance);
            var hit = host.Trace.HitPos;
            var vector = host.Trace.HitVector;
            view.Pos = new Vec3(hit.X - (vector.X * scale), hit.Y - (vector.Y * scale), hit.Z - (vector.Z * scale));
            view.Sector = host.RenderSector(view.Pos);
            view.SectorFrom = "render";
            view.Yaw += Normalize180(host.Trace.SrcAngle - baseYaw);
        }
        else
        {
            host.Traces++;
            view.Pos = host.Trace.HitPos;
            view.Sector = host.Trace.Sector;
            view.SectorFrom = "trace";
            view.Yaw += Normalize180(host.Trace.SrcAngle - baseYaw);
        }

        if (!view.AllowOutOfBounds || !host.Hardware)
        {
            if (dir.Z < 0d && view.Sector is SectorNode floor)
            {
                while (!floor.FloorBlocks && view.Pos.Z < floor.FloorPlane)
                {
                    view.Pos = new Vec3(view.Pos.X + floor.FloorShift.X, view.Pos.Y + floor.FloorShift.Y, view.Pos.Z + floor.FloorShift.Z);
                    floor = floor.FloorDest ?? floor;
                    view.Sector = floor;
                    view.PortalHops++;
                }
            }
            else if (dir.Z > 0d && view.Sector is SectorNode ceiling)
            {
                while (!ceiling.CeilingBlocks && view.Pos.Z > ceiling.CeilingPlane)
                {
                    view.Pos = new Vec3(view.Pos.X + ceiling.CeilingShift.X, view.Pos.Y + ceiling.CeilingShift.Y, view.Pos.Z + ceiling.CeilingShift.Z);
                    ceiling = ceiling.CeilingDest ?? ceiling;
                    view.Sector = ceiling;
                    view.PortalHops++;
                }
            }
        }
    }

    public sealed class Talker
    {
        public object? Conversation;
        public int Health = 1;
        public int Flags4;
        public bool HasPlayer;
        public bool IsOwnBody = true;
        public bool SettingsController;
        public double Yaw;
    }

    public sealed class TalkHost
    {
        public bool Netgame;
        public bool LimitConversations;
        public bool HapticsMenus;
        public Talker? ConsoleBody;
        public List<string> Messages { get; } = new();
        public List<string> Rumbles { get; } = new();
        public List<int> Animations { get; } = new();
        public List<(object Self, object Player, bool Face, bool Save)> Starts { get; } = new();
        public List<(double Angle, double Pitch, double Range, int Flags)> Aims { get; } = new();
        public Queue<object?> Targets { get; } = new();
    }

    public static bool CanTalk(Talker self)
    {
        return self.Conversation != null && self.Health > 0 && (self.Flags4 & Mf4InCombat) == 0;
    }

    public static bool CanTalkAction(Talker self)
    {
        return CanTalk(self);
    }

    public static bool HasConversation(Talker self)
    {
        return self.Conversation != null;
    }

    public static bool HasConversationAction(Talker self)
    {
        return HasConversation(self);
    }

    public static bool StartConversation(Talker self, object player, bool faceTalker, bool saveAngle, bool rumble, TalkHost host)
    {
        if (!CanTalk(self))
        {
            return false;
        }

        if (host.Netgame && host.LimitConversations && self.HasPlayer && self.IsOwnBody && !self.SettingsController)
        {
            if (ReferenceEquals(self, host.ConsoleBody))
            {
                host.Messages.Add(NetLimitMessage);
            }

            return false;
        }

        if (rumble && host.HapticsMenus)
        {
            host.Rumbles.Add("menu/activate");
        }

        host.Animations.Add(0);
        host.Starts.Add((self, player, faceTalker, saveAngle));
        return true;
    }

    public static bool StartConversationAction(Talker self, object player, bool faceTalker, bool saveAngle, bool rumble, TalkHost host)
    {
        return StartConversation(self, player, faceTalker, saveAngle, rumble, host);
    }

    public static bool TalkFacing(Talker player, TalkHost host)
    {
        var offsets = new[] { 0d, 90d / 16d, -90d / 16d };
        foreach (var offset in offsets)
        {
            var flags = AlfForceNoSmart | AlfCheckConversation | AlfPortalRestrict;
            host.Aims.Add((player.Yaw + offset, 35d, TalkRange, flags));
            var target = host.Targets.Count > 0 ? host.Targets.Dequeue() : null;
            if (target != null)
            {
                return StartConversation((Talker)target, player, true, true, true, host);
            }
        }

        return false;
    }

    public static double AngleOf(Vec3 dir)
    {
        return Math.Atan2(dir.Y, dir.X) * (180d / Math.PI);
    }

    public static double Normalize180(double degrees)
    {
        var rounded = Math.Round(degrees * (0x40000000 / 90d), MidpointRounding.ToEven);
        var bams = unchecked((uint)(long)rounded);
        return (90d / 0x40000000) * unchecked((int)bams);
    }
}
