namespace HCDE.Gamedata.Tests;

public class VmDecalAnimatorParserTests
{
    [Fact]
    public void ParseFaderAndCombiner_LinkAnimators()
    {
        var db = new VmDecalDatabase();
        var errors = new List<string>();
        var tokens = VmLockDefTokenize.Tokenize("fade1 { DecayStart 1.0 DecayTime 2.0 }");
        var i = 0;
        Assert.True(VmDecalAnimatorParser.TryParseFader(tokens, ref i, db, errors), string.Join("; ", errors));
        tokens = VmLockDefTokenize.Tokenize("combo1 { fade1 }");
        i = 0;
        Assert.True(VmDecalAnimatorParser.TryParseCombiner(tokens, ref i, db, errors), string.Join("; ", errors));
        Assert.Single(db.Faders);
        Assert.Single(db.Combiners);
    }

    [Fact]
    public void ParseGenerator_OptionalActor()
    {
        var db = new VmDecalDatabase();
        var errors = new List<string>();
        var tokens = VmLockDefTokenize.Tokenize("optional UnknownActor None");
        var i = 0;
        Assert.True(VmDecalAnimatorParser.TryParseGenerator(tokens, ref i, db, errors));
        Assert.Single(db.Generators);
        Assert.Null(db.Generators[0].DecalName);
    }
}
