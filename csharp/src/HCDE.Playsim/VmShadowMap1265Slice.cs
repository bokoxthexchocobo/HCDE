using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Shadow-map counters, cvar clamps, and the upload path from <c>hw_shadowmap.cpp</c>
/// (<b>1–213</b>). Chunk <b>1265</b>. Quality accepts only the powers of two named in the
/// switch and otherwise stores 128. The budget cvars clamp on write. <c>PerformUpdate</c>
/// clears the light counters and sets the row count to one, and it leaves the budget caps
/// alone. A null light collector returns false before any upload. The light buffer is
/// resized to 4096 floats only when its size differs. A new tree uploads static buffers.
/// A later tree update writes sub-ranges and faults if those buffers were never created.
/// </summary>
public sealed class VmShadowMap1265
{
    public const int LightFloats = 1024 * 4;
    public const int NodesBinding = 4;
    public const int LinesBinding = 5;
    public const int ListBinding = 6;

    public readonly List<string> Log = new();
    public bool Enabled;
    public bool Prioritize = true;
    public bool AutoFallback = true;
    public bool AutoBudget;
    public float TargetMs = 1.20f;
    public int MinLights = 64;
    public int Step = 32;
    public int MaxLights;
    public int Quality = 512;

    public double UpdateMs;
    public int LightsProcessed;
    public int LightsEligible;
    public int LightsShadowmapped;
    public int LightsBudgetedOut;
    public int LightRowsUpdated;
    public int LightPriorityEnabled;
    public int BudgetHardCap;
    public int BudgetRuntimeCap;
    public int BudgetAdaptiveEnabled;

    public bool HasTree;
    public bool NewTree;
    public float Ray = 1f;
    public int NodesBytes = 16;
    public int LinesBytes = 8;
    public int DynNodesOffset;
    public int DynNodesBytes;
    public int DynLinesOffset;
    public int DynLinesBytes;
    public bool TreeUpdated;
    public Action<float[]>? Collect;
    public float[] Lights = [];
    public Buffer? LightList;
    public Buffer? Nodes;
    public Buffer? Lines;
    int _nextId = 1;

    public sealed class Buffer
    {
        public int Id;
        public int Binding;
        public bool Storage;
        public bool Index;
        public int Bytes;
        public string Usage = "";
    }

    public void SetTarget(float value)
    {
        if (value < 0.25f)
            value = 0.25f;
        else if (value > 10f)
            value = 10f;
        TargetMs = value;
        Log.Add("target " + value.ToString("F2", CultureInfo.InvariantCulture));
    }

    public void SetMinLights(int value)
    {
        if (value < 0)
            value = 0;
        else if (value > 1024)
            value = 1024;
        MinLights = value;
        Log.Add($"min {value}");
    }

    public void SetStep(int value)
    {
        if (value < 1)
            value = 1;
        else if (value > 256)
            value = 256;
        Step = value;
        Log.Add($"step {value}");
    }

    public void SetMaxLights(int value)
    {
        if (value < 0)
            value = 0;
        else if (value > 1024)
            value = 1024;
        MaxLights = value;
        Log.Add($"max {value}");
    }

    public void SetQuality(int value)
    {
        switch (value)
        {
            case 2 << 6:
            case 2 << 7:
            case 2 << 8:
            case 2 << 9:
            case 2 << 10:
            case 2 << 11:
            case 2 << 12:
                break;
            default:
                value = 128;
                break;
        }

        Quality = value;
        Log.Add($"quality {value}");
    }

    public string Stat()
    {
        string line =
            "upload=" + Ms(UpdateMs) +
            $" ms  lights={LightsProcessed}  eligible={LightsEligible}  shadowmapped={LightsShadowmapped}" +
            $"  dropped={LightsBudgetedOut}  rows={LightRowsUpdated}  cap={BudgetRuntimeCap}/{BudgetHardCap}" +
            $"  adaptive={(BudgetAdaptiveEnabled != 0 ? "on" : "off")}  priority={(LightPriorityEnabled != 0 ? "on" : "off")}";
        Log.Add(line);
        return line;
    }

    public bool ShadowTest(double lx, double ly, double lz, double x, double y, double z)
    {
        if (!(HasTree && Enabled))
        {
            Log.Add("open");
            return true;
        }

        bool hit = Ray >= 1f;
        Log.Add($"{(hit ? "lit" : "shadow")} {F(lx)} {F(ly)} {F(lz)} {F(x)} {F(y)} {F(z)} {F(Ray)}");
        return hit;
    }

    public bool PerformUpdate()
    {
        UpdateMs = 0;
        LightsProcessed = 0;
        LightsEligible = 0;
        LightsShadowmapped = 0;
        LightsBudgetedOut = 0;
        LightRowsUpdated = 1;
        LightPriorityEnabled = 0;
        Log.Add($"reset rows {LightRowsUpdated} cap {BudgetRuntimeCap}/{BudgetHardCap} adaptive {BudgetAdaptiveEnabled}");
        if (Collect != null)
        {
            Log.Add("clock");
            UploadTree();
            UploadLights();
            return true;
        }

        Log.Add("skip");
        return false;
    }

    public void UploadLights()
    {
        if (Lights.Length != LightFloats)
        {
            var next = new float[LightFloats];
            int keep = Math.Min(Lights.Length, next.Length);
            Array.Copy(Lights, next, keep);
            Lights = next;
            Log.Add($"resize {LightFloats}");
        }

        if (Collect == null)
            throw new NullReferenceException();
        Collect(Lights);
        LightList ??= Create(ListBinding, true, false);
        int bytes = sizeof(float) * Lights.Length;
        LightList.Bytes = bytes;
        LightList.Usage = "stream";
        Log.Add($"data list {LightList.Id} {bytes} stream");
    }

    public void UploadTree()
    {
        if (NewTree)
        {
            NewTree = false;
            Nodes ??= Create(NodesBinding, true, false);
            Nodes.Bytes = NodesBytes;
            Nodes.Usage = "static";
            Log.Add($"data nodes {Nodes.Id} {NodesBytes} static");
            Lines ??= Create(LinesBinding, true, false);
            Lines.Bytes = LinesBytes;
            Lines.Usage = "static";
            Log.Add($"data lines {Lines.Id} {LinesBytes} static");
            return;
        }

        if (!HasTree)
            throw new NullReferenceException();
        if (!TreeUpdated)
        {
            Log.Add("tree idle");
            return;
        }

        Buffer nodes = Nodes ?? throw new NullReferenceException();
        Buffer lines = Lines ?? throw new NullReferenceException();
        Log.Add($"sub nodes {nodes.Id} {DynNodesOffset} {DynNodesBytes}");
        Log.Add($"sub lines {lines.Id} {DynLinesOffset} {DynLinesBytes}");
    }

    public void Reset()
    {
        Drop(ref LightList, "list");
        Drop(ref Nodes, "nodes");
        Drop(ref Lines, "lines");
    }

    public void Destroy()
    {
        Reset();
        Log.Add("dtor");
    }

    Buffer Create(int binding, bool storage, bool index)
    {
        var buffer = new Buffer
        {
            Id = _nextId++,
            Binding = binding,
            Storage = storage,
            Index = index,
        };
        Log.Add($"buf {binding} {(storage ? 1 : 0)} {(index ? 1 : 0)} {buffer.Id}");
        return buffer;
    }

    void Drop(ref Buffer? buffer, string name)
    {
        if (buffer == null)
            return;
        Log.Add($"drop {name} {buffer.Id}");
        buffer = null;
    }

    static string F(double value) => ((float)value).ToString("F6", CultureInfo.InvariantCulture);

    static string Ms(double value)
    {
        string text = value.ToString("0.00", CultureInfo.InvariantCulture);
        return text.Length < 4 ? text.PadLeft(4, '0') : text;
    }
}
