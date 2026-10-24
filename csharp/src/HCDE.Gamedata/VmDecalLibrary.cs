namespace HCDE.Gamedata;

/// <summary><c>FDecalLib::AddDecal</c> / lookup scans from <c>decallib.cpp</c>.</summary>
public sealed class VmDecalLibrary
{
    private VmDecalTreeNode? _root;

    public void Clear() => _root = null;

    public void AddDecal(VmDecalDefinition definition)
    {
        _root = VmDecalTreeNode.Insert(_root, definition);
        var stored = GetDecalByName(definition.Name);
        if (stored is null || definition.Id == 0)
            return;
        var existing = ScanTreeForNum(definition.Id, _root);
        if (existing is not null && !ReferenceEquals(existing, stored))
            existing.Id = 0;
        stored.Id = definition.Id;
    }

    public VmDecalDefinition? GetDecalByName(string name) => ScanTreeForName(name, _root);

    public VmDecalDefinition? GetDecalByNum(ushort num)
    {
        if (num == 0) return null;
        return ScanTreeForNum(num, _root);
    }

    internal static VmDecalDefinition? ScanTreeForName(string name, VmDecalTreeNode? root)
    {
        while (root is not null)
        {
            var cmp = string.Compare(name, root.Definition.Name, StringComparison.OrdinalIgnoreCase);
            if (cmp == 0) return root.Definition;
            root = cmp < 0 ? root.Left : root.Right;
        }

        return null;
    }

    internal static VmDecalDefinition? ScanTreeForNum(ushort num, VmDecalTreeNode? root)
    {
        while (root is not null)
        {
            if (root.Definition.Id == num) return root.Definition;
            var left = ScanTreeForNum(num, root.Left);
            if (left is not null) return left;
            root = root.Right;
        }

        return null;
    }
}

public sealed class VmDecalTreeNode
{
    public VmDecalDefinition Definition { get; }
    public VmDecalTreeNode? Left { get; set; }
    public VmDecalTreeNode? Right { get; set; }

    public VmDecalTreeNode(VmDecalDefinition definition) => Definition = definition;

    private static void CopyTemplate(VmDecalTemplate from, VmDecalTemplate to)
    {
        to.ScaleX = from.ScaleX;
        to.ScaleY = from.ScaleY;
        to.PicName = from.PicName;
        to.RenderStyle = from.RenderStyle;
        to.Alpha = from.Alpha;
        to.Translatable = from.Translatable;
        to.AnimatorName = from.AnimatorName;
    }

    public static VmDecalTreeNode? Insert(VmDecalTreeNode? root, VmDecalDefinition definition)
    {
        if (root is null) return new VmDecalTreeNode(definition);
        var cmp = string.Compare(definition.Name, root.Definition.Name, StringComparison.OrdinalIgnoreCase);
        if (cmp == 0)
        {
            root.Definition.Id = definition.Id;
            CopyTemplate(definition.Template, root.Definition.Template);
            return root;
        }

        if (cmp < 0) root.Left = Insert(root.Left, definition);
        else root.Right = Insert(root.Right, definition);
        return root;
    }
}
