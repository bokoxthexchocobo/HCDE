namespace HCDE.Playsim;

/// <summary><c>P_TeleportMove</c> and <c>P_PlayerStartStomp</c> from <c>p_map.cpp</c> (458–638).</summary>
public static class VmPMapTeleportMove
{
    public const int MfShootable = 1 << 5;
    public const int MfSolid = 1 << 4;
    public const int Mf2Passmobj = 1;
    public const int Mf2Telestomp = 1 << 1;
    public const int Mf2Thruactors = 1 << 2;
    public const int Mf2Floorclip = 1 << 5;
    public const int Mf3Ismonster = 1 << 1;
    public const int Mf3Dontoverlap = 1 << 2;
    public const int Mf4Actlikebridge = 1;
    public const int Mf6Thruspecies = 1;
    public const int Mf6Intrymove = 1 << 1;
    public const int Mf6Notelefrag = 1 << 2;
    public const int Mf7Notelestomp = 1;
    public const int Mf7Alwaystelefrag = 1 << 1;
    public const int Mf8Allowthrubits = 1;
    public const int CompatfNoPassmobj = 1;
    public const int Compatf2Teleport = 1;
    public const int LevelMonsterstelefrag = 1;
    public const int CfPredicting = 1;
    public const int DmgThrustless = 1;
    public const int TelefragDamage = 10000;

    public sealed class MapActor
    {
        public MapSector? Sector { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Height { get; set; } = 41;
        public double Radius { get; set; } = 16;
        public double Top => Z + Height;
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags3 { get; set; }
        public int Flags4 { get; set; }
        public int Flags6 { get; set; }
        public int Flags7 { get; set; }
        public int Flags8 { get; set; }
        public uint ThruBits { get; set; }
        public double FloorZ { get; set; }
        public double CeilingZ { get; set; }
        public double DropoffZ { get; set; }
        public object? FloorPic { get; set; }
        public object? CeilingPic { get; set; }
        public MapSector? FloorSector { get; set; }
        public MapSector? CeilingSector { get; set; }
        public int FloorTerrain { get; set; }
        public object? BlockingLine { get; set; }
        public int RenderFlags { get; set; }
        public PlayerState? Player { get; set; }
        public MapLevel? Level { get; set; }
        public string Species { get; set; } = "";
        public bool IsInventory { get; set; }
    }

    public sealed class PlayerState
    {
        public int Cheats { get; set; }
    }

    public sealed class MapLevel
    {
        public int Flags { get; set; }
        public int CompatFlags { get; set; }
        public int CompatFlags2 { get; set; }
        public Func<double, double, MapSector?> PointInSector { get; set; } = (_, _) => null;
    }

    public sealed class MapSector
    {
        public int PortalGroup { get; set; }
    }

    public sealed class TeleportFloorCeilingTm
    {
        public double FloorZ { get; set; }
        public double CeilingZ { get; set; }
        public double DropoffZ { get; set; }
        public object? FloorPic { get; set; }
        public object? CeilingPic { get; set; }
        public MapSector? FloorSector { get; set; }
        public MapSector? CeilingSector { get; set; }
        public int FloorTerrain { get; set; }
        public bool TouchMidtex { get; set; }
        public bool AboveMidtex { get; set; }
    }

    public sealed class TeleportContext
    {
        public required MapActor Thing { get; init; }
        public Action? ClearSpechit { get; init; }
        public Action<TeleportFloorCeilingTm, int>? GetFloorCeilingZ { get; init; }
        public Action<MapActor, int>? IterateLinesAtDest { get; init; }
        public Func<MapActor, IEnumerable<MapActor>>? IterateThingsAtDest { get; init; }
        public Func<MapActor, MapActor, bool>? CanCollideWith { get; init; }
        public Func<MapActor, MapActor, bool>? ShouldPassThroughPlayer { get; init; }
        public Action<MapActor, MapActor, MapActor, int, string, int>? DamageMobj { get; init; }
        public Action<MapActor>? AdjustFloorClip { get; init; }
        public Action<MapActor, MapSector?>? CheckSectorTransition { get; init; }
    }

    public static bool TeleportMove(
        TeleportContext ctx,
        double posX,
        double posY,
        double posZ,
        bool telefrag,
        bool modifyActor)
    {
        var thing = ctx.Thing;
        var oldsec = thing.Sector;
        var tmf = new TeleportFloorCeilingTm();
        ctx.GetFloorCeilingZ?.Invoke(tmf, 0);
        ctx.ClearSpechit?.Invoke();

        var stompAlwaysFrags = ((thing.Flags2 & Mf2Telestomp) != 0 || (thing.Level?.Flags & LevelMonsterstelefrag) != 0 || telefrag)
            && (thing.Flags7 & Mf7Notelestomp) == 0;

        var savedz = thing.Z;
        thing.Z = posZ;
        ctx.IterateLinesAtDest?.Invoke(thing, 0);
        thing.Z = savedz;

        if (tmf.TouchMidtex)
            tmf.DropoffZ = tmf.FloorZ;

        foreach (var th in ctx.IterateThingsAtDest?.Invoke(thing) ?? Array.Empty<MapActor>())
        {
            if ((th.Flags & MfShootable) == 0 || th == thing)
                continue;
            if (((th.Flags2 | thing.Flags2) & Mf2Thruactors) != 0)
                continue;
            if ((th.ThruBits & thing.ThruBits) != 0 && ((th.Flags8 | thing.Flags8) & Mf8Allowthrubits) != 0)
                continue;

            var blockdist = th.Radius + thing.Radius;
            if (Math.Abs(th.X - posX) >= blockdist || Math.Abs(th.Y - posY) >= blockdist)
                continue;

            if ((thing.Flags6 & Mf6Thruspecies) != 0 && thing.Species == th.Species)
                continue;

            if ((thing.Flags2 & Mf2Passmobj) != 0 || (th.Flags4 & Mf4Actlikebridge) != 0)
            {
                if ((thing.Level?.CompatFlags & CompatfNoPassmobj) == 0)
                {
                    if ((th.Flags3 & thing.Flags3 & Mf3Dontoverlap) == 0)
                    {
                        if (posZ > th.Top || posZ + thing.Height < th.Z)
                            continue;
                    }
                }
            }

            if (ctx.CanCollideWith?.Invoke(thing, th) == false)
                continue;

            if ((thing.IsInventory || (thing.Flags2 & Mf2Telestomp) != 0) && (thing.Flags & MfSolid) == 0
                && ((th.Flags3 & Mf3Ismonster) != 0 || th.Player is not null))
                continue;

            if (thing.Player is not null && ctx.ShouldPassThroughPlayer?.Invoke(thing, th) == true)
                continue;

            if ((stompAlwaysFrags && (th.Flags6 & Mf6Notelefrag) == 0) || (th.Flags7 & Mf7Alwaystelefrag) != 0)
            {
                if (thing.Player is null || (thing.Player.Cheats & CfPredicting) == 0)
                    ctx.DamageMobj?.Invoke(th, thing, thing, TelefragDamage, "Telefrag", DmgThrustless);
                continue;
            }

            return false;
        }

        if (modifyActor)
        {
            thing.X = posX;
            thing.Y = posY;
            thing.Z = posZ;
            thing.FloorZ = tmf.FloorZ;
            thing.CeilingZ = tmf.CeilingZ;
            thing.FloorSector = tmf.FloorSector;
            thing.FloorPic = tmf.FloorPic;
            thing.FloorTerrain = tmf.FloorTerrain;
            thing.CeilingSector = tmf.CeilingSector;
            thing.CeilingPic = tmf.CeilingPic;
            thing.DropoffZ = tmf.DropoffZ;
            thing.BlockingLine = null;
            if ((thing.Flags2 & Mf2Floorclip) != 0)
                ctx.AdjustFloorClip?.Invoke(thing);
            thing.RenderFlags |= 1;
            if ((thing.Flags6 & Mf6Intrymove) == 0 && (thing.Level?.CompatFlags2 & Compatf2Teleport) == 0)
                ctx.CheckSectorTransition?.Invoke(thing, oldsec);
        }

        return true;
    }

    public static void PlayerStartStomp(
        MapActor actor,
        bool mononly,
        Func<MapActor, IEnumerable<MapActor>>? iterateThings = null,
        Action<MapActor, MapActor, MapActor, int, string, int>? damageMobj = null)
    {
        foreach (var th in iterateThings?.Invoke(actor) ?? Array.Empty<MapActor>())
        {
            if ((th.Flags & MfShootable) == 0)
                continue;
            if (th == actor || (th.Player == actor.Player && th.Player is not null))
                continue;

            var blockdist = th.Radius + actor.Radius;
            if (Math.Abs(th.X - actor.X) >= blockdist || Math.Abs(th.Y - actor.Y) >= blockdist)
                continue;

            if (th.Player is null && (th.Flags3 & Mf3Ismonster) == 0)
                continue;
            if ((th.Flags6 & Mf6Notelefrag) != 0 && (th.Flags7 & Mf7Alwaystelefrag) == 0)
                continue;
            if (th.Player is not null && mononly)
                continue;
            if (actor.Z > th.Top)
                continue;
            if (actor.Top < th.Z)
                continue;

            damageMobj?.Invoke(th, actor, actor, TelefragDamage, "Telefrag", 0);
        }
    }
}
