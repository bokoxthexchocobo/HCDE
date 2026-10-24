namespace HCDE.Gamedata;

public class VmDecalBase
{
    public string Name { get; set; } = "";
}

public sealed class VmDecalGroup : VmDecalBase
{
    private readonly List<(VmDecalBase Decal, ushort Weight)> _choices = [];

    public void AddDecal(VmDecalBase decal, ushort weight) => _choices.Add((decal, weight));

    public VmDecalBase? PickDecal(Func<int, int, int>? weightedIndex = null)
    {
        if (_choices.Count == 0) return null;
        var total = _choices.Aggregate(0, (sum, c) => sum + c.Weight);
        if (total <= 0) return _choices[0].Decal;
        var roll = weightedIndex?.Invoke(0, total - 1) ?? 0;
        var acc = 0;
        foreach (var (decal, weight) in _choices)
        {
            acc += weight;
            if (roll < acc) return decal;
        }

        return _choices[^1].Decal;
    }

    public void ReplaceDecalRef(VmDecalBase from, VmDecalBase to)
    {
        for (var i = 0; i < _choices.Count; i++)
        {
            if (ReferenceEquals(_choices[i].Decal, from))
                _choices[i] = (to, _choices[i].Weight);
        }
    }
}

public sealed class VmDecalTranslation
{
    public uint StartColor { get; set; }
    public uint EndColor { get; set; }
    public VmDecalTranslation? Next { get; set; }
}

/// <summary>Decal library clear/tree/group core from <c>decallib.cpp</c>.</summary>
public sealed class VmDecalLibCore
{
    public VmDecalTreeNode? Root { get; set; }
    public VmDecalTranslation? Translations { get; set; }
    public List<VmDecalAnimatorDefinition> Animators { get; } = [];

    public void Clear()
    {
        DelTree(Root);
        Root = null;
        var trans = Translations;
        while (trans is not null)
        {
            var next = trans.Next;
            trans = next;
        }

        Translations = null;
        Animators.Clear();
    }

    public static void DelTree(VmDecalTreeNode? root)
    {
        if (root is null) return;
        DelTree(root.Left);
        DelTree(root.Right);
    }
}

public sealed class VmDecalAnimatorDefinition
{
    public string Name { get; set; } = "";
}
