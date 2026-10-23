namespace HCDE.Playsim;

/// <summary>
/// <c>FLevelLocals::RecalculateDrawnSubsectors</c>, <c>SerializeSubsectors</c>, and savegame portal/poly/reverb
/// serializers from <c>p_saveg.cpp</c> (346–553).
/// </summary>
public static class VmPSavegSubsectorsPortals
{
    public const uint MlMapped = 0x00000100;
    public const uint SsecmfDrawn = 2;

    public sealed class LineDef
    {
        public uint Flags { get; set; }
    }

    public sealed class SubsectorSeg
    {
        public LineDef? Linedef { get; set; }
    }

    public sealed class Subsector
    {
        public uint Flags { get; set; }
        public int NumLines { get; set; }
        public List<SubsectorSeg> FirstLine { get; } = new();
    }

    public sealed class LevelLocals
    {
        public List<Subsector> Subsectors { get; } = new();
        public List<object> Vertexes { get; } = new();
    }

    public static void RecalculateDrawnSubsectors(LevelLocals level)
    {
        foreach (var sub in level.Subsectors)
        {
            for (var j = 0u; j < sub.NumLines; j++)
            {
                if (j >= sub.FirstLine.Count)
                    break;
                var linedef = sub.FirstLine[(int)j].Linedef;
                if (linedef is not null && (linedef.Flags & MlMapped) != 0)
                    sub.Flags |= SsecmfDrawn;
            }
        }
    }

    public sealed class SavegArchive
    {
        public bool Writing { get; init; }
        public int ArrayVertexCount { get; set; }
        public int ArraySubsectorCount { get; set; }
        public string? ArrayDrawnEncoding { get; set; }
        private readonly Dictionary<string, object?> _objectFields = new(StringComparer.Ordinal);
        private readonly Stack<string> _objectStack = new();
        private int _arrayFieldIndex;

        public bool BeginArray(string? key)
        {
            _arrayFieldIndex = 0;
            return true;
        }

        public SavegArchive Arc(string? key, int value)
        {
            if (key is not null)
                return this;
            if (Writing)
            {
                if (_arrayFieldIndex == 0)
                    ArrayVertexCount = value;
                else if (_arrayFieldIndex == 1)
                    ArraySubsectorCount = value;
            }
            else
            {
                if (_arrayFieldIndex == 0)
                    ArrayVertexCount = value;
                else if (_arrayFieldIndex == 1)
                    ArraySubsectorCount = value;
            }

            _arrayFieldIndex++;
            return this;
        }

        public SavegArchive StringPtr(string? key, string? value)
        {
            if (Writing)
                ArrayDrawnEncoding = value;
            else if (value is not null)
                ArrayDrawnEncoding = value;
            return this;
        }

        public void EndArray() => _arrayFieldIndex = 0;

        public bool BeginObject(string key)
        {
            _objectStack.Push(key);
            if (Writing)
                _objectFields.Clear();
            return true;
        }

        public SavegArchive Arc(string key, object? value)
        {
            if (Writing)
                _objectFields[key] = value;
            return this;
        }

        public void EndObject()
        {
            if (_objectStack.Count > 0)
                _objectStack.Pop();
        }

        public T Field<T>(string key)
        {
            if (_objectFields.TryGetValue(key, out var v) && v is T t)
                return t;
            return default!;
        }

        public void SeedField(string key, object? value) => _objectFields[key] = value;
    }

    public static SavegArchive SerializeSubsectors(LevelLocals level, SavegArchive arc, string key)
    {
        var numsubsectors = level.Subsectors.Count;
        if (arc.Writing)
        {
            var encoded = new char[1 + (numsubsectors + 5) / 6];
            var p = 0;
            for (var i = 0u; i < numsubsectors; i += 6)
            {
                byte by = 0;
                for (var j = 0u; j < 6; j++)
                {
                    if (i + j < numsubsectors && (level.Subsectors[(int)(i + j)].Flags & SsecmfDrawn) != 0)
                        by |= (byte)(1 << (int)j);
                }

                if (by < 10) by += (byte)'0';
                else if (by < 36) by += (byte)('A' - 10);
                else if (by < 62) by += (byte)('a' - 36);
                else if (by == 62) by = (byte)'-';
                else if (by == 63) by = (byte)'+';
                encoded[p++] = (char)by;
            }

            encoded[p] = '\0';
            var str = new string(encoded, 0, p);
            if (arc.BeginArray(key))
            {
                var numvertexes = level.Vertexes.Count;
                arc.Arc(null, numvertexes)
                    .Arc(null, numsubsectors)
                    .StringPtr(null, str)
                    .EndArray();
            }
        }
        else
        {
            var success = false;
            if (arc.BeginArray(key))
            {
                var numVerts = arc.ArrayVertexCount;
                var numSubs = arc.ArraySubsectorCount;
                arc.Arc(null, numVerts)
                    .Arc(null, numSubs)
                    .StringPtr(null, arc.ArrayDrawnEncoding);
                var str = arc.ArrayDrawnEncoding ?? string.Empty;

                if (numVerts == level.Vertexes.Count && numSubs == numsubsectors)
                {
                    success = true;
                    var sub = 0;
                    for (var i = 0; i < str.Length && str[i] != '\0'; i++)
                    {
                        var by = (byte)str[i];
                        if (by >= '0' && by <= '9') by -= (byte)'0';
                        else if (by >= 'A' && by <= 'Z') by -= (byte)('A' - 10);
                        else if (by >= 'a' && by <= 'z') by -= (byte)('a' - 36);
                        else if (by == '-') by = 62;
                        else if (by == '+') by = 63;
                        else
                        {
                            success = false;
                            break;
                        }

                        for (var s = 0; s < 6; s++)
                        {
                            if (sub + s < numsubsectors && (by & (1 << s)) != 0)
                                level.Subsectors[sub + s].Flags |= SsecmfDrawn;
                        }

                        sub += 6;
                    }
                }

                if (!success)
                    RecalculateDrawnSubsectors(level);
            }
        }

        return arc;
    }

    public sealed class LinePortal
    {
        public int Origin { get; set; }
        public int Destination { get; set; }
        public double DisplacementX { get; set; }
        public double DisplacementY { get; set; }
        public int Type { get; set; }
        public uint Flags { get; set; }
        public uint DefFlags { get; set; }
        public int Align { get; set; }
    }

    public static SavegArchive SerializeLinePortal(SavegArchive arc, string key, LinePortal port)
    {
        if (arc.BeginObject(key))
        {
            if (!arc.Writing)
            {
                port.Origin = arc.Field<int>("origin");
                port.Destination = arc.Field<int>("destination");
                var disp = arc.Field<(double X, double Y)>("displacement");
                port.DisplacementX = disp.X;
                port.DisplacementY = disp.Y;
                port.Type = arc.Field<int>("type");
                port.Flags = arc.Field<uint>("flags");
                port.DefFlags = arc.Field<uint>("defflags");
                port.Align = arc.Field<int>("align");
            }

            arc.Arc("origin", port.Origin)
                .Arc("destination", port.Destination)
                .Arc("displacement", (port.DisplacementX, port.DisplacementY))
                .Arc("type", port.Type)
                .Arc("flags", port.Flags)
                .Arc("defflags", port.DefFlags)
                .Arc("align", port.Align)
                .EndObject();
        }

        return arc;
    }

    public sealed class SectorPortal
    {
        public int Type { get; set; }
        public uint Flags { get; set; }
        public int Partner { get; set; }
        public int Plane { get; set; }
        public int Origin { get; set; }
        public int Destination { get; set; }
        public double DisplacementX { get; set; }
        public double DisplacementY { get; set; }
        public double PlaneZ { get; set; }
        public int Skybox { get; set; }
    }

    public static SavegArchive SerializeSectorPortal(SavegArchive arc, string key, SectorPortal port)
    {
        if (arc.BeginObject(key))
        {
            if (!arc.Writing)
            {
                port.Type = arc.Field<int>("type");
                port.Flags = arc.Field<uint>("flags");
                port.Partner = arc.Field<int>("partner");
                port.Plane = arc.Field<int>("plane");
                port.Origin = arc.Field<int>("origin");
                port.Destination = arc.Field<int>("destination");
                var disp = arc.Field<(double X, double Y)>("displacement");
                port.DisplacementX = disp.X;
                port.DisplacementY = disp.Y;
                port.PlaneZ = arc.Field<double>("planez");
                port.Skybox = arc.Field<int>("skybox");
            }

            arc.Arc("type", port.Type)
                .Arc("flags", port.Flags)
                .Arc("partner", port.Partner)
                .Arc("plane", port.Plane)
                .Arc("origin", port.Origin)
                .Arc("destination", port.Destination)
                .Arc("displacement", (port.DisplacementX, port.DisplacementY))
                .Arc("planez", port.PlaneZ)
                .Arc("skybox", port.Skybox)
                .EndObject();
        }

        return arc;
    }

    public sealed class PolyObj
    {
        public double Angle { get; set; }
        public double StartX { get; set; }
        public double StartY { get; set; }
        public int Interpolation { get; set; }
        public bool Blocked { get; set; }
        public bool HasPortals { get; set; }
        public int SpecialData { get; set; }
        public int Level { get; set; }
        public int OriginalPtsCount { get; set; }
        public int RotateCalls { get; private set; }
        public int MoveCalls { get; private set; }
        public double LastMoveDx { get; private set; }
        public double LastMoveDy { get; private set; }

        public void RotatePolyobj(double angle, bool fromSave)
        {
            RotateCalls++;
            Angle = angle;
        }

        public void MovePolyobj(double dx, double dy, bool fromSave)
        {
            MoveCalls++;
            LastMoveDx = dx;
            LastMoveDy = dy;
            StartX += dx;
            StartY += dy;
        }
    }

    public static SavegArchive SerializePolyObj(SavegArchive arc, string key, PolyObj poly)
    {
        if (arc.BeginObject(key))
        {
            var angle = poly.Angle;
            var deltaX = poly.StartX;
            var deltaY = poly.StartY;
            if (!arc.Writing)
            {
                angle = arc.Field<double>("angle");
                var pos = arc.Field<(double X, double Y)>("pos");
                deltaX = pos.X;
                deltaY = pos.Y;
                poly.Interpolation = arc.Field<int>("interpolation");
                poly.Blocked = arc.Field<bool>("blocked");
                poly.HasPortals = arc.Field<bool>("hasportals");
                poly.SpecialData = arc.Field<int>("specialdata");
                poly.Level = arc.Field<int>("level");
            }

            arc.Arc("angle", angle)
                .Arc("pos", (deltaX, deltaY))
                .Arc("interpolation", poly.Interpolation)
                .Arc("blocked", poly.Blocked)
                .Arc("hasportals", poly.HasPortals)
                .Arc("specialdata", poly.SpecialData)
                .Arc("level", poly.Level)
                .EndObject();

            if (!arc.Writing && poly.OriginalPtsCount > 0)
            {
                poly.RotatePolyobj(angle, true);
                var moveX = deltaX - poly.StartX;
                var moveY = deltaY - poly.StartY;
                poly.MovePolyobj(moveX, moveY, true);
            }
        }

        return arc;
    }

    public sealed class ReverbContainer
    {
        public int Id { get; set; }
    }

    public static ReverbContainer? FindEnvironment(int id) =>
        id == 0 ? null : EnvironmentCatalog.TryGetValue(id, out var c) ? c : null;

    internal static readonly Dictionary<int, ReverbContainer> EnvironmentCatalog = new();

    public static SavegArchive SerializeReverbContainer(SavegArchive arc, string key, ref ReverbContainer? container)
    {
        var id = !arc.Writing || container is null ? 0 : container.Id;
        SerializeInt(arc, key, ref id);
        if (!arc.Writing)
            container = FindEnvironment(id);
        return arc;
    }

    public sealed class Zone
    {
        public ReverbContainer? Environment { get; set; }
    }

    public static SavegArchive SerializeZone(SavegArchive arc, string key, Zone zone)
    {
        var env = zone.Environment;
        SerializeReverbContainer(arc, key, ref env);
        zone.Environment = env;
        return arc;
    }

    public static SavegArchive SerializeInt(SavegArchive arc, string key, ref int value)
    {
        if (arc.Writing)
            arc.Arc(key, value);
        else
            value = arc.Field<int>(key);
        return arc;
    }

}
