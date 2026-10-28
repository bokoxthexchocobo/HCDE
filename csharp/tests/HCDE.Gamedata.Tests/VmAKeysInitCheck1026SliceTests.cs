namespace HCDE.Gamedata.Tests;

public class VmAKeysInitCheck1026SliceTests
{
    private sealed class TestCatalog : ILockDefActorCatalog
    {
        private readonly Dictionary<string, (bool inv, bool key)> _map;

        public TestCatalog(params (string name, bool inv, bool key)[] entries)
        {
            _map = entries.ToDictionary(e => e.name, e => (e.inv, e.key), StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGet(string className, out bool isInventory, out bool isKey)
        {
            if (_map.TryGetValue(className, out var v))
            {
                isInventory = v.inv;
                isKey = v.key;
                return true;
            }

            isInventory = isKey = false;
            return false;
        }
    }

    private sealed class ActiveGameFilter(string game) : ILockDefGameFilter
    {
        public bool IsActiveGame(string gameToken) => gameToken.Equals(game, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ClearLocks_ResetsKeyNumbersAndDatabase()
    {
        var state = new VmAKeysInitCheck1026Slice.KeyInitState();
        var keys = new List<VmAKeysInitCheck1026Slice.KeyTypeEntry>
        {
            new() { ClassName = "RedCard", KeyNumber = 3 },
        };
        state.Database.Locks[1] = new VmLockDefLock();
        state.Database.KeyNumbersByClass["RedCard"] = 3;
        state.KeysDone = true;

        VmAKeysInitCheck1026Slice.ClearLocks(keys, state);

        Assert.Equal(0, keys[0].KeyNumber);
        Assert.Empty(state.Database.Locks);
        Assert.False(state.KeysDone);
    }

    [Fact]
    public void CreateSortedKeyList_OrdersAssignedThenUnassigned()
    {
        var state = new VmAKeysInitCheck1026Slice.KeyInitState();
        var keys = new List<VmAKeysInitCheck1026Slice.KeyTypeEntry>
        {
            new() { ClassName = "BlueCard", KeyNumber = 0 },
            new() { ClassName = "RedCard", KeyNumber = 2 },
            new() { ClassName = "YellowCard", KeyNumber = 1 },
        };

        VmAKeysInitCheck1026Slice.CreateSortedKeyList(keys, state);

        Assert.Equal(["YellowCard", "RedCard", "BlueCard"], state.SortedKeyTypes.Select(k => k.ClassName));
    }

    [Fact]
    public void TryInitializeFromLockDefTexts_BuildsLocksAndSortedList()
    {
        var state = new VmAKeysInitCheck1026Slice.KeyInitState();
        var keys = new List<VmAKeysInitCheck1026Slice.KeyTypeEntry>
        {
            new() { ClassName = "RedCard" },
            new() { ClassName = "BlueCard" },
        };
        var errors = new List<string>();
        const string text = """
            LOCK 1
            {
              ANY { RedCard BlueCard }
              MESSAGE "Need both"
            }
            """;

        Assert.True(VmAKeysInitCheck1026Slice.TryInitializeFromLockDefTexts(
            keys, [text], state, new TestCatalog(("RedCard", true, true), ("BlueCard", true, true)),
            new ActiveGameFilter("Doom"), errors), string.Join("; ", errors));

        Assert.True(state.KeysDone);
        Assert.Single(state.Database.Locks);
        Assert.Equal(1, keys.First(k => k.ClassName == "RedCard").KeyNumber);
        Assert.Equal(2, keys.First(k => k.ClassName == "BlueCard").KeyNumber);
        Assert.Equal(2, state.SortedKeyTypes.Count);
    }

    [Fact]
    public void TryEvaluateCheckKeysHead_AllowsNonPositiveKeyNum()
    {
        var owner = new VmInventoryItem { ClassName = "Player" };
        var db = new VmLockDefDatabase();
        Assert.True(VmAKeysInitCheck1026Slice.TryEvaluateCheckKeysHead(
            owner, 0, keysDone: false, 0, db, remote: false, out var ok, out _));
        Assert.NotNull(ok);
        Assert.True(ok!.Allowed);
    }

    [Fact]
    public void TryEvaluateCheckKeysHead_RejectsNullOwner()
    {
        var db = new VmLockDefDatabase();
        Assert.False(VmAKeysInitCheck1026Slice.TryEvaluateCheckKeysHead(
            null, 1, keysDone: true, 0, db, remote: false, out _, out _));
    }

    [Fact]
    public void TryEvaluateCheckKeysHead_MissingLockUsesSharewareText()
    {
        var owner = new VmInventoryItem { ClassName = "Player" };
        var db = new VmLockDefDatabase();
        Assert.True(VmAKeysInitCheck1026Slice.TryEvaluateCheckKeysHead(
            owner, 103, keysDone: true, VmAKeysInitCheck1026Slice.GiShareware, db, remote: false,
            out _, out var fail));
        Assert.Equal("$TXT_RETAIL_ONLY", fail!.FailText);
    }

    [Fact]
    public void TryEvaluateCheckKeysHead_SucceedsWhenLockMatchesInventory()
    {
        var owner = new VmInventoryItem { ClassName = "Player" };
        owner.Inventory.Add(new VmInventoryItem { ClassName = "RedCard" });
        var db = new VmLockDefDatabase();
        var lockDef = new VmLockDefLock();
        lockDef.KeyGroups.Add(new VmLockDefKeyGroup());
        lockDef.KeyGroups[0].AnyKeyList.Add(new VmLockDefKeyRequirement { ActorClass = "RedCard" });
        db.Locks[5] = lockDef;

        Assert.True(VmAKeysInitCheck1026Slice.TryEvaluateCheckKeysHead(
            owner, 5, keysDone: true, 0, db, remote: false, out var ok, out _));
        Assert.True(ok!.Allowed);
    }

    [Fact]
    public void TryEvaluateCheckKeysHead_FailureUsesRemoteMessageWhenRequested()
    {
        var owner = new VmInventoryItem { ClassName = "Player" };
        var db = new VmLockDefDatabase();
        var lockDef = new VmLockDefLock { Message = "Local", RemoteMessage = "Remote" };
        lockDef.KeyGroups.Add(new VmLockDefKeyGroup());
        lockDef.KeyGroups[0].AnyKeyList.Add(new VmLockDefKeyRequirement { ActorClass = "RedCard" });
        db.Locks[2] = lockDef;

        Assert.True(VmAKeysInitCheck1026Slice.TryEvaluateCheckKeysHead(
            owner, 2, keysDone: true, 0, db, remote: true, out _, out var fail));
        Assert.Equal("Remote", fail!.FailText);
    }
}
