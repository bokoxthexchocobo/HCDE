namespace HCDE.Gamedata;

public sealed class VmLockDefKeyRequirement
{
    public string ActorClass { get; set; } = "";
    public int Count { get; set; } = 1;
}

public sealed class VmLockDefKeyGroup
{
    public List<VmLockDefKeyRequirement> AnyKeyList { get; } = [];
}

public sealed class VmLockDefLock
{
    public List<VmLockDefKeyGroup> KeyGroups { get; } = [];
    public List<string> LockSounds { get; } = [];
    public string Message { get; set; } = "";
    public string RemoteMessage { get; set; } = "";
    public int MapColorRgb { get; set; }
}

public sealed class VmLockDefDatabase
{
    public Dictionary<int, VmLockDefLock> Locks { get; } = new();
    public Dictionary<string, int> KeyNumbersByClass { get; } = new(StringComparer.OrdinalIgnoreCase);
}

public interface ILockDefActorCatalog
{
    bool TryGet(string className, out bool isInventory, out bool isKey);
}

public interface ILockDefGameFilter
{
    bool IsActiveGame(string gameToken);
}
