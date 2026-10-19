namespace HCDE.Net.Core;

/// <summary>Native <c>FHCDERemotePlayerInterpState</c> cleared on client disconnect / full reset.</summary>
public sealed class RemotePlayerInterpResetState
{
    public const int RingCapacity = 16;

    public int WriteIndex { get; set; }
    public int Count { get; set; }
    public int[] SampleTics { get; } = new int[RingCapacity];
    public float[] SampleX { get; } = new float[RingCapacity];
    public float[] SampleY { get; } = new float[RingCapacity];
    public float[] SampleZ { get; } = new float[RingCapacity];
    public float[] SampleAngle { get; } = new float[RingCapacity];

    public void Reset()
    {
        WriteIndex = 0;
        Count = 0;
        Array.Clear(SampleTics);
        Array.Clear(SampleX);
        Array.Clear(SampleY);
        Array.Clear(SampleZ);
        Array.Clear(SampleAngle);
    }

    public void PushSample(int tic, float x, float y, float z, float angle)
    {
        var slot = WriteIndex % RingCapacity;
        SampleTics[slot] = tic;
        SampleX[slot] = x;
        SampleY[slot] = y;
        SampleZ[slot] = z;
        SampleAngle[slot] = angle;
        WriteIndex++;
        Count = Math.Min(Count + 1, RingCapacity);
    }
}

public sealed class RemotePlayerInterpRegistry
{
    private readonly RemotePlayerInterpResetState[] _states;

    public RemotePlayerInterpRegistry(int maxClients = NetClientDisconnectSession.MaxPlayers)
    {
        _states = new RemotePlayerInterpResetState[maxClients];
        for (var i = 0; i < maxClients; i++)
            _states[i] = new RemotePlayerInterpResetState();
    }

    public RemotePlayerInterpResetState this[int client] => _states[client];

    public void ResetClient(int clientNum)
    {
        if (clientNum < 0 || clientNum >= _states.Length)
            return;
        _states[clientNum].Reset();
    }
}
