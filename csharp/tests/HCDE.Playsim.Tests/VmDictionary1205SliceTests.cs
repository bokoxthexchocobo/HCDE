namespace HCDE.Playsim.Tests;

public class VmDictionary1205SliceTests
{
    [Fact]
    public void Insert_ReplacesInPlaceAndIteratorStopsOnTheLastPair()
    {
        var dict = VmDictionary1205Slice.Create();
        Assert.Equal("{}", dict.ToJson());
        Assert.Equal("", dict.At("missing"));
        dict.Insert("a", "1");
        dict.Insert("b", "2");
        dict.Insert("a", "9");
        dict.Remove("nope");
        Assert.Equal("{\"a\":\"9\",\"b\":\"2\"}", dict.ToJson());
        Assert.Equal("9", dict.At("a"));

        var cursor = VmDictionary1205Slice.Iterator.Create(dict);
        Assert.Equal("", cursor.Key());
        Assert.True(cursor.Next());
        Assert.Equal("a", cursor.Key());
        Assert.Equal("9", cursor.Value());
        Assert.True(cursor.Next());
        Assert.Equal("b", cursor.Key());
        Assert.False(cursor.Next());
        Assert.Equal("b", cursor.Key());
        Assert.Equal("2", cursor.Value());

        dict.Remove("a");
        Assert.Equal("{\"b\":\"2\"}", dict.ToJson());
        var fresh = new VmDictionary1205Slice.Iterator();
        Assert.Equal("", fresh.Key());
        Assert.Throws<InvalidOperationException>(() => fresh.Next());
        var save = Assert.Throws<VmDictionary1205Slice.ScriptError>(() => fresh.Serialize(true));
        Assert.Equal("Attempt to save pointer to unhandled type DictionaryIterator", save.Detail);
        fresh.Serialize(false);
        Assert.Equal("", fresh.Key());
    }

    [Fact]
    public void Json_RoundTripAndRejectsNullAndNonStrings()
    {
        var parsed = VmDictionary1205Slice.FromJson(" { \"b\" : \"2\", \"a\" : \"x\\\"y\" } ");
        Assert.NotNull(parsed);
        Assert.Equal("{\"b\":\"2\",\"a\":\"x\\\"y\"}", parsed!.ToJson());
        Assert.Equal("\u0001", VmDictionary1205Slice.FromJson("{\"a\":\"\\u0001\"}")!.At("a"));
        Assert.Equal("{\"a\":\"\\n\"}", VmDictionary1205Slice.FromJson("{\"a\":\"\\n\"}")!.ToJson());
        Assert.Null(VmDictionary1205Slice.FromJson("null"));
        Assert.Equal("{}", VmDictionary1205Slice.FromJson("")!.ToJson());

        var notObject = Assert.Throws<VmDictionary1205Slice.ScriptError>(() => VmDictionary1205Slice.FromJson("[]"));
        Assert.Equal("Dictionary is expected to be a JSON object.", notObject.Detail);
        var badValue = Assert.Throws<VmDictionary1205Slice.ScriptError>(() => VmDictionary1205Slice.FromJson("{\"a\":1}"));
        Assert.Equal("Dictionary value is expected to be a JSON string.", badValue.Detail);
        Assert.Throws<VmDictionary1205Slice.ScriptError>(() => VmDictionary1205Slice.FromJson("NULL"));

        var dict = VmDictionary1205Slice.Create();
        dict.Insert("keep", "me");
        string slot = "null";
        Assert.Throws<NullReferenceException>(() => dict.Serialize(false, ref slot));
        Assert.Equal("me", dict.At("keep"));
        slot = "{\"c\":\"3\"}";
        dict.Serialize(false, ref slot);
        Assert.Equal("", dict.At("keep"));
        Assert.Equal("3", dict.At("c"));
        dict.Serialize(true, ref slot);
        Assert.Equal("{\"c\":\"3\"}", slot);
    }
}
