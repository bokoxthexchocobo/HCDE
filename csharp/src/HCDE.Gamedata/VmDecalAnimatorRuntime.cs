namespace HCDE.Gamedata;

public enum VmDecalThinkerKind { Fader, Stretcher, Slider, Colorer, Combiner }

public sealed record VmDecalThinkerSchedule(
    VmDecalThinkerKind Kind,
    ulong StartMapTime,
    ulong EndMapTime,
    double GoalX = -1,
    double GoalY = -1,
    double DistY = 0,
    string GoalColor = "",
    IReadOnlyList<VmDecalThinkerSchedule>? Children = null);

public static class VmDecalAnimatorRuntime
{
    public static VmDecalAnimator? FindAnimator(IReadOnlyList<VmDecalAnimator> animators, string name)
    {
        for (var i = animators.Count - 1; i >= 0; i--)
        {
            if (animators[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return animators[i];
        }

        return null;
    }

    public static VmDecalDefinition? ResolveGroupChoice(
        IReadOnlyList<(VmDecalDefinition Decal, int Weight)> choices,
        Random? random = null)
    {
        if (choices.Count == 0) return null;
        var rng = random ?? Random.Shared;
        var total = choices.Sum(c => Math.Max(c.Weight, 0));
        if (total <= 0) return choices[0].Decal;
        var pick = rng.Next(total);
        foreach (var (decal, weight) in choices)
        {
            if (weight <= 0) continue;
            if (pick < weight) return ResolveDecalChain(decal);
            pick -= weight;
        }

        return ResolveDecalChain(choices[^1].Decal);
    }

    public static VmDecalDefinition ResolveDecalChain(VmDecalDefinition decal) => decal;

    public static IReadOnlyList<VmDecalThinkerSchedule> BuildThinkerSchedules(
        VmDecalAnimator animator,
        ulong mapTime,
        IReadOnlyDictionary<string, VmDecalAnimator> byName)
    {
        var list = new List<VmDecalThinkerSchedule>();
        BuildRecursive(animator, mapTime, byName, list);
        return list;
    }

    private static void BuildRecursive(
        VmDecalAnimator animator,
        ulong mapTime,
        IReadOnlyDictionary<string, VmDecalAnimator> byName,
        List<VmDecalThinkerSchedule> output)
    {
        switch (animator.Kind)
        {
            case VmDecalAnimatorKind.Fader:
                output.Add(new VmDecalThinkerSchedule(
                    VmDecalThinkerKind.Fader,
                    mapTime + (ulong)animator.DecayStartTics,
                    mapTime + (ulong)(animator.DecayStartTics + animator.DecayTimeTics)));
                break;
            case VmDecalAnimatorKind.Stretcher:
                output.Add(new VmDecalThinkerSchedule(
                    VmDecalThinkerKind.Stretcher,
                    mapTime + (ulong)animator.StretchStartTics,
                    mapTime + (ulong)(animator.StretchStartTics + animator.StretchTimeTics),
                    animator.GoalX,
                    animator.GoalY));
                break;
            case VmDecalAnimatorKind.Slider:
                output.Add(new VmDecalThinkerSchedule(
                    VmDecalThinkerKind.Slider,
                    mapTime + (ulong)animator.SlideStartTics,
                    mapTime + (ulong)(animator.SlideStartTics + animator.SlideTimeTics),
                    DistY: animator.DistY));
                break;
            case VmDecalAnimatorKind.ColorChanger:
                output.Add(new VmDecalThinkerSchedule(
                    VmDecalThinkerKind.Colorer,
                    mapTime + (ulong)animator.ColorFadeStartTics,
                    mapTime + (ulong)(animator.ColorFadeStartTics + animator.ColorFadeTimeTics),
                    GoalColor: animator.GoalColor));
                break;
            case VmDecalAnimatorKind.Combiner:
                var children = new List<VmDecalThinkerSchedule>();
                foreach (var childName in animator.CombinedAnimatorNames)
                {
                    if (!byName.TryGetValue(childName, out var child)) continue;
                    BuildRecursive(child, mapTime, byName, children);
                }

                output.Add(new VmDecalThinkerSchedule(VmDecalThinkerKind.Combiner, mapTime, mapTime, Children: children));
                break;
        }
    }
}

public enum VmDecalAnimatorKind { Fader, Stretcher, Slider, ColorChanger, Combiner }

public sealed class VmDecalAnimator
{
    public string Name { get; init; } = "";
    public VmDecalAnimatorKind Kind { get; init; }
    public int DecayStartTics { get; init; }
    public int DecayTimeTics { get; init; }
    public int StretchStartTics { get; init; }
    public int StretchTimeTics { get; init; }
    public double GoalX { get; init; } = -1;
    public double GoalY { get; init; } = -1;
    public int SlideStartTics { get; init; }
    public int SlideTimeTics { get; init; }
    public double DistY { get; init; }
    public int ColorFadeStartTics { get; init; }
    public int ColorFadeTimeTics { get; init; }
    public string GoalColor { get; init; } = "";
    public IReadOnlyList<string> CombinedAnimatorNames { get; init; } = Array.Empty<string>();

    public static IEnumerable<VmDecalAnimator> EnumerateFromDatabase(VmDecalDatabase db)
    {
        foreach (var f in db.Faders)
        {
            yield return new VmDecalAnimator
            {
                Name = f.Name,
                Kind = VmDecalAnimatorKind.Fader,
                DecayStartTics = f.DecayStartTics,
                DecayTimeTics = f.DecayTimeTics
            };
        }

        foreach (var s in db.Stretchers)
        {
            yield return new VmDecalAnimator
            {
                Name = s.Name,
                Kind = VmDecalAnimatorKind.Stretcher,
                StretchStartTics = s.StretchStartTics,
                StretchTimeTics = s.StretchTimeTics,
                GoalX = s.GoalX,
                GoalY = s.GoalY
            };
        }

        foreach (var s in db.Sliders)
        {
            yield return new VmDecalAnimator
            {
                Name = s.Name,
                Kind = VmDecalAnimatorKind.Slider,
                SlideStartTics = s.SlideStartTics,
                SlideTimeTics = s.SlideTimeTics,
                DistY = s.DistY
            };
        }

        foreach (var c in db.ColorChangers)
        {
            yield return new VmDecalAnimator
            {
                Name = c.Name,
                Kind = VmDecalAnimatorKind.ColorChanger,
                ColorFadeStartTics = c.FadeStartTics,
                ColorFadeTimeTics = c.FadeTimeTics,
                GoalColor = c.GoalColor
            };
        }

        foreach (var c in db.Combiners)
        {
            yield return new VmDecalAnimator
            {
                Name = c.Name,
                Kind = VmDecalAnimatorKind.Combiner,
                CombinedAnimatorNames = c.AnimatorNames.ToArray()
            };
        }
    }
}

public static class VmDecalTranslationTable
{
    public static bool TryLocate(IReadOnlyList<(uint Start, uint End, int Index)> tables, uint start, uint end, out int index)
    {
        foreach (var table in tables)
        {
            if (table.Start == start && table.End == end)
            {
                index = table.Index;
                return true;
            }
        }

        index = -1;
        return false;
    }

    public static int Register(List<(uint Start, uint End, int Index)> tables, uint start, uint end)
    {
        if (TryLocate(tables, start, end, out var existing)) return existing;
        if (tables.Count >= 256 * 256)
            return -1;
        var index = tables.Count;
        tables.Add((start, end, index));
        return index;
    }
}
