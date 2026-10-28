namespace HCDE.Gamedata;

/// <summary>Resolves flat names the way <c>TexMan.CheckForTexture</c> does for <c>ParseFloor</c>.</summary>
public interface ITerrainFlatCatalog
{
    bool TryResolveFlat(string flatName, out int textureIndex, out bool exists);
}

public sealed class VmTerrainFlatNameCatalog : ITerrainFlatCatalog
{
    private readonly Dictionary<string, int> _indices;

    public VmTerrainFlatNameCatalog(IReadOnlyDictionary<string, int> flatIndices)
    {
        _indices = new Dictionary<string, int>(flatIndices, StringComparer.OrdinalIgnoreCase);
    }

    public bool TryResolveFlat(string flatName, out int textureIndex, out bool exists)
    {
        exists = _indices.TryGetValue(flatName, out textureIndex);
        return true;
    }
}
