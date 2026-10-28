namespace HCDE.Playsim;

/// <summary>Native <c>DActiveButton</c> from <c>p_switch.cpp</c>.</summary>
internal sealed class ActiveButton
{
    internal int SideIndex { get; init; }
    internal SwitchWallPart Part { get; init; }
    internal SwitchDefinition Definition { get; init; } = null!;
    internal double PosX { get; init; }
    internal double PosY { get; init; }
    internal bool Flippable { get; set; }
    internal bool Returning { get; set; }
    internal int Frame { get; set; } = -1;
    internal uint Timer { get; set; }

    internal bool AdvanceFrame(AuthoritySimulation sim)
    {
        var def = Returning ? Definition.Pair ?? Definition : Definition;
        var finished = false;
        if (++Frame == def.Frames.Length - 1)
        {
            if (Flippable) Timer = 35;
            else finished = true;
        }
        else
        {
            var frame = def.Frames[Frame];
            Timer = (uint)frame.TimeMin;
            if (frame.TimeRnd != 0) Timer += (uint)sim.NextSwitchAnimByte() % (uint)frame.TimeRnd;
        }
        return finished;
    }
}
