namespace HCDE.Playsim;

/// <summary>
/// MapLoader::SpawnSpecials from specials.cpp 624-852. The banner at 615-623 stays uncredited.
/// </summary>
public static class VmSpecials1462Slice
{
    public const int BandStart = 624;
    public const int BandEnd = 852;
    public const int ScriptOpen = 1;
    public const int RepeatSpecial = 0x200;
    public const int FakeFloorOnly = 2;
    public const int ClipFakePlanes = 4;
    public const int NoFakeLight = 8;
    public const int IgnoreHeightSec = 16;
    public const int Underwater = 32;
    public const int ForcedUnderwater = 64;
    public const int Lift = 2048;
    public const int SkyFlat = 0x40000000;
    public const int PortsPlane = 4;
    public const int PortsHorizon = 5;
    public const int TransferHeights = 209;
    public const int TransferFloorLight = 210;
    public const int TransferCeilingLight = 211;
    public const int TransferWallLight = 16;
    public const int Attach3dMidtex = 48;
    public const int SectorSetLink = 51;
    public const int SectorSetPortal = 57;
    public const int LineSetPortal = 156;
    public const int LineQuickPortal = 301;
    public const int GenericLift = 203;
    public const int PlatDownWaitUpStay = 62;
    public const int PlatDownWaitUpStayLip = 206;
    public const int PlatUpWaitDownStay = 64;
    public const int PlatUpNearestWaitDownStay = 172;
    public const int StaticInit = 190;
    public const int InitGravity = 0;
    public const int InitDamage = 2;
    public const int InitSectorLink = 3;
    public const int InitTransferSky = 255;

    public sealed class Sector
    {
        public int Special;
        public int Tag;
        public int MoreFlags;
        public double Gravity = 1;
        public int DamageAmount;
        public int LeakyDamage;
        public int DamageInterval;
        public string DamageType = "";
        public int SkyTransfer;
        public Sector? HeightSec;
        public bool FloorClipAdjusted;
        public List<Sector> FakeFloors = new();
    }

    public sealed class Line
    {
        public int Index;
        public int Special;
        public int Flags;
        public int Back = -1;
        public double Delta = 0;
        public int[] Args = new int[5];
        public Sector Front = null!;
    }

    public sealed class Actor
    {
        public string Name = "";
    }

    public sealed class Thinker
    {
        public string Kind = "";
        public Sector? Source;
        public int Tag;
        public int Extra;
        public bool Floor;
    }

    public sealed class Spawned
    {
        public string Kind = "";
        public int A, B, C, D;
        public bool Flag;
        public Sector? Source;
    }

    public sealed class World
    {
        public List<Sector> Sectors = new();
        public List<Line> Lines = new();
        public List<Actor> Actors = new();
        public bool ForceWater;
        public bool HasHeightSecs;
        public int NextPortal;
        public List<string> Calls = new();
        public List<int> Inited = new();
        public List<Thinker> Thinkers = new();
        public List<Spawned> Spawns = new();
        public int OpenScripts;
        public bool OpenFromActor;
        public bool OpenAlways;
    }

    public static void Spawn(World w)
    {
        w.Calls.Add("SetupPortals");
        for (int i = 0; i < w.Sectors.Count; i++)
        {
            if (w.Sectors[i].Special == 0)
                continue;
            w.Inited.Add(i);
            w.Calls.Add("InitSectorSpecial");
        }
        w.Calls.Add("ProcessEDSectors");
        w.Calls.Add("SpawnScrollers");
        w.Calls.Add("SpawnFriction");
        w.Calls.Add("SpawnPushers");
        for (int i = 0; i < w.Actors.Count; i++)
        {
            if (w.Actors[i].Name == "SkyCamCompat")
                w.Calls.Add("SpawnSkybox:" + i);
        }
        foreach (var line in w.Lines)
            ApplyLine(w, line);
        w.OpenScripts++;
        w.OpenFromActor = false;
        w.OpenAlways = false;
        w.Calls.Add("StartTypedScripts");
    }

    static void ApplyLine(World w, Line line)
    {
        switch (line.Special)
        {
            case TransferHeights:
                ApplyHeights(w, line);
                break;
            case TransferFloorLight:
                w.Thinkers.Add(new Thinker { Kind = "DLightTransfer", Source = line.Front, Tag = line.Args[0], Floor = true });
                break;
            case TransferCeilingLight:
                w.Thinkers.Add(new Thinker { Kind = "DLightTransfer", Source = line.Front, Tag = line.Args[0], Floor = false });
                break;
            case TransferWallLight:
                w.Thinkers.Add(new Thinker { Kind = "DWallLightTransfer", Source = line.Front, Tag = line.Args[0], Extra = line.Args[1] });
                break;
            case Attach3dMidtex:
                w.Spawns.Add(new Spawned { Kind = "Attach3dMidtex", Source = line.Front, A = line.Args[0], B = line.Args[1], Flag = line.Args[2] != 0 });
                break;
            case SectorSetLink:
                if (line.Args[0] == 0)
                    w.Spawns.Add(new Spawned { Kind = "SectorLinks", Source = line.Front, A = line.Args[1], B = line.Args[2], C = line.Args[3] });
                break;
            case SectorSetPortal:
                ApplyPortal(w, line);
                break;
            case LineSetPortal:
            case LineQuickPortal:
                w.Spawns.Add(new Spawned { Kind = "LinePortal", Source = line.Front, A = line.Index, B = line.Special });
                break;
            case GenericLift:
                if (line.Args[3] != 1)
                    return;
                MarkLift(w, line);
                break;
            case PlatDownWaitUpStay:
            case PlatDownWaitUpStayLip:
            case PlatUpWaitDownStay:
            case PlatUpNearestWaitDownStay:
                MarkLift(w, line);
                break;
            case StaticInit:
                ApplyStatic(w, line);
                break;
        }
    }

    static void ApplyHeights(World w, Line line)
    {
        var sec = line.Front;
        int bits = line.Args[1];
        if ((bits & 2) != 0) sec.MoreFlags |= FakeFloorOnly;
        if ((bits & 4) != 0) sec.MoreFlags |= ClipFakePlanes;
        if ((bits & 8) != 0) sec.MoreFlags |= Underwater;
        else if (w.ForceWater) sec.MoreFlags |= ForcedUnderwater;
        if ((bits & 16) != 0) sec.MoreFlags |= IgnoreHeightSec;
        else w.HasHeightSecs = true;
        if ((bits & 32) != 0) sec.MoreFlags |= NoFakeLight;
        foreach (int s in Tagged(w, line.Args[0], null))
        {
            var dest = w.Sectors[s];
            dest.HeightSec = sec;
            sec.FakeFloors.Add(dest);
            dest.MoreFlags |= sec.MoreFlags & IgnoreHeightSec;
            dest.FloorClipAdjusted = true;
        }
    }

    static void ApplyPortal(World w, Line line)
    {
        int kind = line.Args[1];
        if ((kind == 0 || kind == 6) && line.Args[3] == 0)
        {
            w.Spawns.Add(new Spawned
            {
                Kind = "SectorPortal",
                Source = line.Front,
                A = line.Args[0],
                B = line.Args[2],
                C = line.Args[4],
                D = kind,
            });
            return;
        }
        if (kind == 3 || kind == 4)
        {
            int pnum = w.NextPortal++;
            int ptype = kind == 3 ? PortsPlane : PortsHorizon;
            w.Spawns.Add(new Spawned
            {
                Kind = "CopyPortal",
                Source = line.Front,
                A = line.Args[0],
                B = line.Args[2],
                C = pnum,
                D = ptype,
                Flag = true,
            });
        }
    }

    static void MarkLift(World w, Line line)
    {
        if ((line.Flags & RepeatSpecial) == 0)
            return;
        foreach (int s in Tagged(w, line.Args[0], line))
            w.Sectors[s].MoreFlags |= Lift;
    }

    static void ApplyStatic(World w, Line line)
    {
        switch (line.Args[1])
        {
            case InitGravity:
                double grav = line.Delta / 100.0;
                foreach (int s in Tagged(w, line.Args[0], null))
                    w.Sectors[s].Gravity = grav;
                break;
            case InitDamage:
                int damage = (int)line.Delta;
                foreach (int s in Tagged(w, line.Args[0], null))
                {
                    var sec = w.Sectors[s];
                    sec.DamageAmount = damage;
                    sec.DamageType = "None";
                    if (sec.DamageAmount < 20)
                    {
                        sec.LeakyDamage = 0;
                        sec.DamageInterval = 32;
                    }
                    else if (sec.DamageAmount < 50)
                    {
                        sec.LeakyDamage = 5;
                        sec.DamageInterval = 32;
                    }
                    else
                    {
                        sec.LeakyDamage = 256;
                        sec.DamageInterval = 1;
                    }
                }
                break;
            case InitSectorLink:
                if (line.Args[3] == 0)
                    w.Spawns.Add(new Spawned { Kind = "SectorLinksById", Source = line.Front, A = line.Args[0], B = line.Args[2] });
                break;
            case InitTransferSky:
                foreach (int s in Tagged(w, line.Args[0], null))
                    w.Sectors[s].SkyTransfer = (line.Index + 1) | SkyFlat;
                break;
        }
    }

    static IEnumerable<int> Tagged(World w, int tag, Line? line)
    {
        if (line != null && tag == 0)
        {
            if (line.Back >= 0)
                yield return line.Back;
            yield break;
        }
        if (tag == 0)
        {
            for (int i = 0; i < w.Sectors.Count; i++)
            {
                if (w.Sectors[i].Tag == 0)
                    yield return i;
            }
            yield break;
        }
        for (int i = 0; i < w.Sectors.Count; i++)
        {
            if (w.Sectors[i].Tag == tag)
                yield return i;
        }
    }
}
