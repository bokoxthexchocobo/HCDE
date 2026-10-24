namespace HCDE.Client;

/// <summary>Managed <c>i_time.cpp</c> frame/tic clock (injectable time source).</summary>
public sealed class VmGameClock
{
    private ulong startupTimeNs;
    private ulong firstFrameStartTime;
    private ulong currentFrameStartTime;
    private ulong freezeTime;
    private double lastInputTime;
    private double deltaTime;
    private ulong prevTime;

    public double TimeScale { get; set; } = 1.0;
    public int GameTicRate { get; set; } = 35;

    public Func<ulong> GetTimePoint { get; init; } = () => (ulong)Environment.TickCount64 * 1_000_000;

    public void InitTime() => startupTimeNs = GetTimePoint();

    private ulong GetClockTimeNs()
    {
        var tp = GetTimePoint() - startupTimeNs;
        if (TimeScale == 1.0) return tp;
        return (ulong)(tp / 1000 * TimeScale * 1000);
    }

    public void SetFrameTime()
    {
        if (freezeTime == 0) currentFrameStartTime = GetClockTimeNs();
        if (firstFrameStartTime == 0) firstFrameStartTime = currentFrameStartTime;
    }

    public void WaitVbl(int count)
    {
        Thread.Sleep(1000 * count / 70);
        SetFrameTime();
    }

    public int WaitForTic(int prevTic, double ticRate)
    {
        int time;
        while ((time = GetTime(ticRate)) <= prevTic)
        {
            var next = firstFrameStartTime + VmTimeUnitConversions.TicToNs(prevTic + 1, ticRate);
            var now = NsTime();
            if (next > now)
            {
                var sleepTime = VmTimeUnitConversions.NsToMs(next - now);
                if (sleepTime > 2) Thread.Sleep((int)(sleepTime - 2));
            }

            SetFrameTime();
        }

        return time;
    }

    public ulong NsTime() => GetClockTimeNs();
    public ulong MsTime() => VmTimeUnitConversions.NsToMs(NsTime());
    public double MsTimeF() => NsTime() / 1_000_000.0;
    public ulong MsTimeFromStart() => firstFrameStartTime == 0 ? 0 : VmTimeUnitConversions.NsToMs(NsTime() - firstFrameStartTime);

    public ulong GetTimeNs() => currentFrameStartTime - firstFrameStartTime;
    public int GetTime(double ticRate) => VmTimeUnitConversions.NsToTic(currentFrameStartTime - firstFrameStartTime, ticRate);

    public double GetTimeFrac(double ticRate)
    {
        var elapsed = currentFrameStartTime - firstFrameStartTime;
        var currentTic = VmTimeUnitConversions.NsToTic(elapsed, ticRate);
        var ticStart = firstFrameStartTime + VmTimeUnitConversions.TicToNs(currentTic, ticRate);
        var ticNext = firstFrameStartTime + VmTimeUnitConversions.TicToNs(currentTic + 1, ticRate);
        return (currentFrameStartTime - ticStart) / (double)(ticNext - ticStart);
    }

    public void FreezeTime(bool frozen)
    {
        if (frozen)
        {
            if (freezeTime != 0) throw new InvalidOperationException("Already frozen");
            freezeTime = GetTimePoint();
        }
        else
        {
            if (freezeTime == 0) throw new InvalidOperationException("Not frozen");
            if (firstFrameStartTime != 0) firstFrameStartTime += GetTimePoint() - freezeTime;
            freezeTime = 0;
            SetFrameTime();
        }
    }

    public void ResetFrameTime()
    {
        var ft = currentFrameStartTime;
        SetFrameTime();
        firstFrameStartTime += currentFrameStartTime - ft;
    }

    public double GetInputFrac()
    {
        var now = MsTimeF();
        var result = (now - lastInputTime) * GameTicRate * (1.0 / 1000.0);
        lastInputTime = now;
        return result;
    }

    public void ResetInputTime() => lastInputTime = MsTimeF();

    public void ClearPrevTime() => prevTime = 0;

    public void SetDeltaTime()
    {
        var time = NsTime();
        if (prevTime == 0) prevTime = time;
        deltaTime = Math.Clamp((time - prevTime) * 1e-9, 0.001, 0.2);
        prevTime = time;
    }

    public double GetDeltaTime(bool current)
    {
        var time = NsTime();
        if (prevTime == 0) return 0.0;
        return deltaTime + (current ? (time - prevTime) * 1e-9 : 0.0);
    }

    public double GetPhysicsTimeStep() => 1.0 / GameTicRate;
}
