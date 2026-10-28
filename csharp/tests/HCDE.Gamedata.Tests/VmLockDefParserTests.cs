namespace HCDE.Gamedata.Tests;

public class VmLockDefParserTests
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
    public void ParseLock_AssignsKeyNumbersAndMessages()
    {
        var db = new VmLockDefDatabase();
        var actors = new TestCatalog(("RedCard", true, true), ("BlueCard", true, true));
        var errors = new List<string>();
        const string text = """
            LOCK 1
            {
              ANY { RedCard BlueCard }
              MESSAGE "Need keys"
              MAPCOLOR 255 0 0
            }
            """;

        Assert.True(VmLockDefParser.TryParseDocument(text, db, actors, new ActiveGameFilter("Doom"), errors), string.Join("; ", errors));
        var lock1 = Assert.Single(db.Locks);
        Assert.Equal(1, lock1.Key);
        Assert.Equal("Need keys", lock1.Value.RemoteMessage);
        Assert.Equal(0xFF, lock1.Value.MapColorRgb & 0xFF);
        Assert.Equal(1, db.KeyNumbersByClass["RedCard"]);
        Assert.Equal(2, db.KeyNumbersByClass["BlueCard"]);
    }

    [Fact]
    public void ParseLock_IgnoresOtherGameBlock()
    {
        var db = new VmLockDefDatabase();
        var actors = new TestCatalog(("HexenKey", true, true));
        var errors = new List<string>();
        const string text = "LOCK 5 Heretic { ANY { HexenKey } }";
        Assert.True(VmLockDefParser.TryParseDocument(text, db, actors, new ActiveGameFilter("Doom"), errors));
        Assert.Empty(db.Locks);
        Assert.Empty(db.KeyNumbersByClass);
    }
}
