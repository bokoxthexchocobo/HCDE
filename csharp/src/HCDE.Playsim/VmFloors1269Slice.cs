using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Software 3D-floor clip heights from <c>r_3dfloors.cpp</c> (1-199). Chunk 1269.
/// Heights outside the sector are ignored. A repeat height still marks the floor
/// active and does not insert. Entering a skybox saves the list without freeing it.
/// Leaving an empty stack drops the live list without freeing it. Cleanup frees the
/// current list, restores each saved list, and frees that too.
/// </summary>
public sealed class VmFloors1269
{
    public readonly List<string> Log = new();
    public bool FakeActive;
    public int CurrentSkybox;
    public int HeightMax = -1;
    public VmFloorNode1269? Top, Tail;

    readonly Dictionary<int, VmFakeFloor1269> _floors = new();
    readonly List<VmFloorSaved1269> _stack = new();
    VmFloorNode1269? _clipTop;
    int _seq;

    public VmFakeFloor1269? FakeFloor { get; private set; }

    public void SetFakeFloor(int key)
    {
        if (!_floors.TryGetValue(key, out VmFakeFloor1269? clip))
        {
            clip = new VmFakeFloor1269 { Key = key, Id = ++_seq };
            _floors[key] = clip;
            Log.Add("floor new " + clip.Id.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            Log.Add("floor reuse " + clip.Id.ToString(CultureInfo.InvariantCulture));
        }

        FakeFloor = clip;
    }

    public void Cleanup()
    {
        _floors.Clear();
        FakeActive = false;
        while (CurrentSkybox != 0)
        {
            DeleteHeights();
            LeaveSkybox();
        }

        ResetClip();
        DeleteHeights();
        Log.Add("cleanup");
    }

    public void DeleteHeights()
    {
        VmFloorNode1269? cur = Top;
        while (cur != null)
        {
            VmFloorNode1269 doomed = cur;
            cur = cur.Next;
            doomed.Freed = true;
            Log.Add("free " + doomed.Id.ToString(CultureInfo.InvariantCulture));
        }

        HeightMax = -1;
        Top = Tail = null;
    }

    public void AddHeight(double height, double floor, double ceiling)
    {
        if (height >= ceiling || height <= floor)
            return;

        FakeActive = true;
        if (HeightMax >= 0)
        {
            VmFloorNode1269? near = Top;
            while (near != null && near.Height < height)
                near = near.Next;
            if (near != null)
            {
                if (near.Height == height)
                    return;
                var curr = NewNode(height);
                curr.Prev = near.Prev;
                curr.Next = near;
                if (near.Prev != null)
                    near.Prev.Next = curr;
                else
                    Top = curr;
                near.Prev = curr;
            }
            else
            {
                var curr = NewNode(height);
                curr.Prev = Tail;
                curr.Next = null;
                Tail!.Next = curr;
                Tail = curr;
            }
        }
        else
        {
            Top = Tail = NewNode(height);
            Top.Prev = null;
            Top.Next = null;
        }

        HeightMax++;
    }

    public void NewClip(short[] floor, short[] ceiling)
    {
        if (FakeFloor == null)
            throw new NullReferenceException();
        var curr = new VmFloorNode1269
        {
            Id = ++_seq,
            Floor = (short[])floor.Clone(),
            Ceiling = (short[])ceiling.Clone(),
            FloorRef = FakeFloor,
        };
        FakeFloor.Floor = curr.Floor;
        FakeFloor.Ceiling = curr.Ceiling;
        if (_clipTop != null)
        {
            TailClip()!.Next = curr;
            _clipTail = curr;
        }
        else
        {
            _clipTop = _clipTail = curr;
        }

        Log.Add("clip " + curr.Id.ToString(CultureInfo.InvariantCulture) + " " + FakeFloor.Id.ToString(CultureInfo.InvariantCulture));
    }

    public void ResetClip()
    {
        VmFloorNode1269? cur = _clipTop;
        while (cur != null)
        {
            if (cur.FloorRef != null)
            {
                cur.FloorRef.Floor = null;
                cur.FloorRef.Ceiling = null;
            }

            VmFloorNode1269 doomed = cur;
            cur = cur.Next;
            doomed.Freed = true;
            Log.Add("free clip " + doomed.Id.ToString(CultureInfo.InvariantCulture));
        }

        _clipTop = _clipTail = null;
    }

    public void EnterSkybox()
    {
        _stack.Add(new VmFloorSaved1269 { Top = Top, Tail = Tail, Max = HeightMax });
        Top = null;
        Tail = null;
        HeightMax = -1;
        CurrentSkybox++;
        Log.Add("enter " + CurrentSkybox.ToString(CultureInfo.InvariantCulture));
    }

    public void LeaveSkybox()
    {
        var current = new VmFloorSaved1269 { Top = null, Tail = null, Max = -1 };
        if (_stack.Count > 0)
        {
            current = _stack[^1];
            _stack.RemoveAt(_stack.Count - 1);
        }

        Top = current.Top;
        Tail = current.Tail;
        HeightMax = current.Max;
        CurrentSkybox--;
        Log.Add("leave " + CurrentSkybox.ToString(CultureInfo.InvariantCulture));
    }

    public string Chain()
    {
        var text = "";
        for (VmFloorNode1269? n = Top; n != null; n = n.Next)
            text += (text.Length == 0 ? "" : ",") + n.Height.ToString("G", CultureInfo.InvariantCulture);
        return text;
    }

    VmFloorNode1269 NewNode(double height) => new() { Id = ++_seq, Height = height };

    VmFloorNode1269? TailClip() => _clipTail;

    VmFloorNode1269? _clipTail;
}

public sealed class VmFloorNode1269
{
    public int Id;
    public double Height;
    public bool Freed;
    public VmFloorNode1269? Prev, Next;
    public short[]? Floor, Ceiling;
    public VmFakeFloor1269? FloorRef;
}

public sealed class VmFakeFloor1269
{
    public int Key;
    public int Id;
    public short[]? Floor, Ceiling;
}

public sealed class VmFloorSaved1269
{
    public VmFloorNode1269? Top, Tail;
    public int Max;
}
