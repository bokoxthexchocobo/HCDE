namespace HCDE.Gamedata;

/// <summary><c>GetSpriteIndex</c> from <c>info.cpp</c>.</summary>
public sealed class VmSpriteCatalog
{
    private readonly List<string> _sprites = ["TNT1", "----", "####"];
    private string _lastName = "";
    private int _lastIndex;

    public int GetSpriteIndex(string spriteName, bool add)
    {
        var upper = NormalizeSpriteName(spriteName);
        if (upper == _lastName) return _lastIndex;
        for (var i = 0; i < _sprites.Count; i++)
        {
            if (_sprites[i] == upper)
            {
                _lastName = upper;
                _lastIndex = i;
                return i;
            }
        }

        if (!add) return _lastIndex = -1;
        _sprites.Add(upper);
        _lastName = upper;
        _lastIndex = _sprites.Count - 1;
        return _lastIndex;
    }

    public static string NormalizeSpriteName(string name)
    {
        var upper = new char[4];
        for (var i = 0; i < 4; i++)
            upper[i] = i < name.Length ? char.ToUpperInvariant(name[i]) : '\0';
        return new string(upper).TrimEnd('\0');
    }
}
