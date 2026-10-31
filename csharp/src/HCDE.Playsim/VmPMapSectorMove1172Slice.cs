namespace HCDE.Playsim;

/// <summary>
/// Floor and ceiling sector moves from <c>p_map.cpp</c> (<b>6940–7271</b>). Chunk <b>1172</b>.
/// </summary>
public static class VmPMapSectorMove1172Slice
{
    public const int MfNoBlockMap = 0x00000010;
    public const int MfNoGravity = 0x00000200;
    public const int MfNoLiftDrop = 0x20000000;
    public const int Mf2PassMobj = 0x00001000;
    public const int Mf4ActLikeBridge = 0x00000080;
    public const int Mf5MoveWithSector = unchecked((int)0x80000000);
    public const int Mf6RelativeToFloor = unchecked((int)0x80000000);
    public const int RfNoInterpolateView = 0x40000000;

    public sealed class Mover
    {
        public double Z;
        public double FloorZ;
        public double CeilingZ;
        public double Height = 16;
        public double PrevZ;
        public double ViewZ;
        public int Flags;
        public int Flags2;
        public int Flags4;
        public int Flags5;
        public int Flags6;
        public int RenderFlags;
        public bool IsPlayerBody;
        public object? Sector;
        public double Top => Z + Height;
    }

    public sealed class Change
    {
        public bool NoFit;
        public int Crush;
        public double MoveAmt;
        public bool MoveMidTex;
        public bool Instant;
        public object? Sector;
    }

    public sealed class Touch
    {
        public Mover Thing = new();
        public bool Visited;
    }

    public sealed class Sec
    {
        public string Name = "";
        public Sec? HeightSec;
        public List<Touch> Touching { get; } = new();
        public List<Sec> Attached { get; } = new();
        public List<Sec> FakeFloors { get; } = new();
    }

    public sealed class MoveHost
    {
        public int PushUp;
        public int PushDown;
        public bool AdjustOk = true;
        public bool TestZOk = true;
        public Mover? OnMover;
        public List<string> Events { get; } = new();
        public List<string> Iterators { get; } = new();
        public List<(string Sector, int Plane)> Portals { get; } = new();
        public List<double> WaterZ { get; } = new();
        public bool MoveMidTex;
        public Action<Mover, Change>? Apply;

        public void Adjust(Mover thing, Change change)
        {
            Events.Add("adjust");
            Apply?.Invoke(thing, change);
        }
    }

    public static void FloorRaise(Mover thing, Change change, MoveHost host)
    {
        var oldFloor = thing.FloorZ;
        var oldZ = thing.Z;
        host.Adjust(thing, change);
        if (oldFloor == thing.FloorZ)
        {
            return;
        }

        if (thing.Z <= thing.FloorZ)
        {
            if ((thing.Flags4 & Mf4ActLikeBridge) != 0)
            {
                change.NoFit = true;
                return;
            }

            host.Events.Add("clear");
            if (change.Instant)
            {
                thing.PrevZ += thing.FloorZ - thing.Z;
                thing.RenderFlags |= RfNoInterpolateView;
            }

            thing.Z = thing.FloorZ;
        }
        else if ((thing.Flags & MfNoGravity) != 0 && (thing.Flags6 & Mf6RelativeToFloor) != 0)
        {
            host.Events.Add("clear");
            var delta = -oldFloor + thing.FloorZ;
            thing.Z += delta;
            if (change.Instant)
            {
                thing.PrevZ += delta;
                thing.RenderFlags |= RfNoInterpolateView;
            }
        }
        else
        {
            return;
        }

        switch (host.PushUp)
        {
            case 1:
                host.Events.Add("crunch");
                host.Events.Add("trigger:" + oldZ);
                host.Events.Add("render");
                break;
            case 2:
                host.Events.Add("crunch");
                thing.Z = oldZ;
                break;
            default:
                host.Events.Add("trigger:" + oldZ);
                host.Events.Add("render");
                break;
        }

        if (thing.IsPlayerBody)
        {
            thing.ViewZ += thing.Z - oldZ;
        }
    }

    public static void CeilingLower(Mover thing, Change change, MoveHost host)
    {
        var onFloor = thing.Z <= thing.FloorZ;
        var oldZ = thing.Z;
        host.Adjust(thing, change);
        if (thing.Top > thing.CeilingZ)
        {
            if ((thing.Flags4 & Mf4ActLikeBridge) != 0)
            {
                change.NoFit = true;
                return;
            }

            host.Events.Add("clear");
            thing.Z = thing.CeilingZ - thing.Height >= thing.FloorZ ? thing.CeilingZ - thing.Height : thing.FloorZ;
            host.Events.Add("render");
            switch (host.PushDown)
            {
                case 1:
                case 2:
                    if (onFloor)
                    {
                        thing.Z = thing.FloorZ;
                    }

                    host.Events.Add("crunch");
                    host.Events.Add("trigger:" + oldZ);
                    host.Events.Add("render");
                    break;
                default:
                    host.Events.Add("trigger:" + oldZ);
                    host.Events.Add("render");
                    break;
            }
        }

        if (thing.IsPlayerBody)
        {
            thing.ViewZ += thing.Z - oldZ;
        }
    }

    public static void CeilingRaise(Mover thing, Change change, MoveHost host)
    {
        host.Adjust(thing, change);
        var isGood = host.AdjustOk;
        var oldZ = thing.Z;
        if ((thing.Flags4 & Mf4ActLikeBridge) != 0)
        {
            return;
        }

        if (thing.Z < thing.FloorZ && thing.Top >= thing.CeilingZ - change.MoveAmt && (thing.Flags & MfNoLiftDrop) == 0)
        {
            thing.Z = thing.FloorZ;
            if (thing.Top > thing.CeilingZ)
            {
                thing.Z = thing.CeilingZ - thing.Height;
            }

            host.Events.Add("trigger:" + oldZ);
            host.Events.Add("render");
        }
        else if ((thing.Flags2 & Mf2PassMobj) != 0 && !isGood && thing.Top < thing.CeilingZ)
        {
            if (!host.TestZOk && host.OnMover != null && host.OnMover.Z <= thing.Z)
            {
                var ceilingTop = thing.CeilingZ - thing.Height;
                thing.Z = ceilingTop < host.OnMover.Top ? ceilingTop : host.OnMover.Top;
                host.Events.Add("render");
            }
        }

        if (thing.IsPlayerBody)
        {
            thing.ViewZ += thing.Z - oldZ;
        }
    }

    public static bool ChangeSector(Sec sector, int crunch, double amt, int floorOrCeil, bool isReset, bool instant, MoveHost host)
    {
        var change = new Change
        {
            Crush = crunch,
            MoveAmt = Math.Abs(amt),
            Sector = sector,
            Instant = instant,
        };
        if (sector.Attached.Count > 0 && floorOrCeil != 2)
        {
            var attached = floorOrCeil == 1
                ? amt >= 0d ? "floor-raise" : "floor-drop"
                : amt >= 0d ? "ceiling-raise" : "ceiling-lower";
            foreach (var attachedSector in sector.Attached)
            {
                host.Events.Add("recalc:" + attachedSector.Name);
                if (ReferenceEquals(attachedSector.HeightSec, sector))
                {
                    continue;
                }

                Visit(attachedSector.Touching, node =>
                {
                    if (Moves(node.Thing))
                    {
                        Dispatch(attached, node.Thing, change, host);
                    }
                });
                host.Portals.Add((attachedSector.Name, floorOrCeil == 0 ? 1 : 0));
            }
        }

        host.Events.Add("recalc:" + sector.Name);
        string? primary;
        string? secondary = null;
        switch (floorOrCeil)
        {
            case 0:
                primary = amt < 0d ? "floor-drop" : "floor-raise";
                break;
            case 1:
                primary = amt < 0d ? "ceiling-lower" : "ceiling-raise";
                break;
            case 2:
                primary = amt < 0d ? "floor-drop" : "floor-raise";
                secondary = amt < 0d ? "ceiling-lower" : "ceiling-raise";
                change.MoveMidTex = true;
                host.MoveMidTex = true;
                break;
            default:
                return false;
        }

        Visit(sector.Touching, node =>
        {
            if (!Moves(node.Thing))
            {
                return;
            }

            Dispatch(primary, node.Thing, change, host);
            if (secondary != null)
            {
                Dispatch(secondary, node.Thing, change, host);
            }
        });
        if (floorOrCeil != 2)
        {
            host.Portals.Add((sector.Name, floorOrCeil));
        }

        if (!change.NoFit && !isReset)
        {
            foreach (var fake in sector.FakeFloors)
            {
                foreach (var node in fake.Touching)
                {
                    node.Visited = false;
                }

                foreach (var node in fake.Touching)
                {
                    if (!node.Visited && ReferenceEquals(node.Thing.Sector, fake))
                    {
                        node.Visited = true;
                        host.Events.Add("water:" + fake.Name);
                        host.WaterZ.Add(node.Thing.Z - amt);
                        host.Events.Add("trigger:" + (node.Thing.Z - amt));
                    }
                }
            }
        }

        return change.NoFit;
    }

    private static bool Moves(Mover thing)
    {
        return (thing.Flags & MfNoBlockMap) == 0 || (thing.Flags5 & Mf5MoveWithSector) != 0;
    }

    private static void Visit(List<Touch> list, Action<Touch> visit)
    {
        foreach (var node in list)
        {
            node.Visited = false;
        }

        while (true)
        {
            Touch? found = null;
            foreach (var node in list)
            {
                if (!node.Visited)
                {
                    node.Visited = true;
                    found = node;
                    break;
                }
            }

            if (found == null)
            {
                return;
            }

            visit(found);
        }
    }

    private static void Dispatch(string name, Mover thing, Change change, MoveHost host)
    {
        host.Iterators.Add(name);
        if (name == "floor-raise")
        {
            FloorRaise(thing, change, host);
        }
        else if (name == "ceiling-lower")
        {
            CeilingLower(thing, change, host);
        }
        else if (name == "ceiling-raise")
        {
            CeilingRaise(thing, change, host);
        }
    }
}
