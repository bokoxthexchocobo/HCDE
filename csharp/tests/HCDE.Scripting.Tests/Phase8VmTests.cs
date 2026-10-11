using HCDE.Scripting;

namespace HCDE.Scripting.Tests;

public class Phase8VmTests
{
    [Theory]
    [InlineData("\u00a0version 4.13")]
    [InlineData("\u2003version 4.13")]
    public void UnicodeWhitespaceDoesNotExposeLeadingVersionDirective(string source)
        => Assert.Equal(new Version(2, 3, 0), VmScriptVersion.Read(source));

    [Theory]
    [InlineData("version\u00a04.13")]
    [InlineData("version\u20034.13")]
    [InlineData("version\u00e9 4.13")]
    public void NativeVersionKeywordRejectsNonAsciiArgumentSeparators(string source)
        => Assert.Throws<FormatException>(() => VmScriptVersion.Read(source));

    [Fact]
    public void VersionReaderAcceptsNativeControlWhitespace()
    {
        Assert.Equal(new Version(4, 13, 0), VmScriptVersion.Read("\0\v\fversion\0\t4.13"));
        Assert.Equal(new Version(2, 3, 0), VmScriptVersion.Read("version_suffix 4.13"));
    }

    [Theory]
    [InlineData("#include_suffix \"child.zs\"")]
    [InlineData("#include1 \"child.zs\"")]
    [InlineData("#include\u00a0\"child.zs\"")]
    [InlineData("#include\u2003\"child.zs\"")]
    public void IncludeTokensRejectSuffixesAndNonNativeWhitespace(string source)
    {
        var queue = new VmScriptIncludeQueue();
        Assert.Throws<FormatException>(() => queue.AddSource(source, "root.zs"));
        Assert.Equal(0, queue.PendingCount);
    }

    [Fact]
    public void IncludeScannerAcceptsNativeControlWhitespaceAndAdjacentStrings()
    {
        var include = Assert.Single(VmScriptIncludes.Extract("#INCLUDE\0\t\v\f\r\"child\"\".zs\"", "root.zs"));
        Assert.Equal("child.zs", include.Path);
        Assert.Equal(1, include.SourceLine);
    }

    [Fact]
    public void NameConstantClosingQuoteIsNotEscapedByBackslash()
    {
        var includes = VmScriptIncludes.Extract("'name\\'\n#include \"child.zs\"", "mod.pk3:root.zs");
        Assert.Equal(new VmScriptInclude("child.zs", "mod.pk3:root.zs", 2), Assert.Single(includes));
        Assert.Empty(VmScriptIncludes.Extract("'#include \"ignored.zs\"'", "root.zs"));
    }

    [Theory]
    [InlineData("'name\nconstant'\n#include \"child.zs\"")]
    [InlineData("'name\r\nconstant'\n#include \"child.zs\"")]
    [InlineData("'unterminated")]
    public void InvalidNameConstantsDoNotRegisterIncludes(string source)
    {
        var queue = new VmScriptIncludeQueue();
        Assert.Throws<FormatException>(() => queue.AddSource(source, "root.zs"));
        Assert.Equal(0, queue.PendingCount);
    }

    [Theory]
    [InlineData("", "\n")]
    [InlineData("text", "text\n")]
    [InlineData("text\0", "text\n")]
    [InlineData("text\r\n", "text\r\n")]
    [InlineData("\uFEFFtext", "text\n")]
    [InlineData("\uFEFF", "\uFEFF\n")]
    [InlineData("\0", "\n")]
    public void ScriptTextPreparationMatchesNativeBufferRules(string source, string expected)
        => Assert.Equal(expected, VmScriptText.Prepare(source));

    [Fact]
    public void TerminalNullVersionAndRegionCommentUsePreparedInput()
    {
        Assert.Equal(new Version(4, 13, 0), VmScriptVersion.Read("\uFEFFversion 4.13\0"));
        Assert.Equal(new Version(2, 3, 0), VmScriptVersion.Read("#region version 99.0"));
        Assert.Empty(VmScriptIncludes.Extract("#region #include \"ignored.zs\"\0", "root.zs"));
        Assert.Equal(2, Assert.Single(VmScriptIncludes.Extract("\uFEFF//header\n#include \"child.zs\"\0", "root.zs")).SourceLine);
        Assert.Throws<ArgumentNullException>(() => VmScriptText.Prepare(null!));
    }

    [Fact]
    public void MissingIncludesAccumulateAcrossLoadedSourcesInDiscoveryOrder()
    {
        var root = new VmScriptSource("mod.pk3:root.zs", "#include \"missing-a.zs\"\n#include \"child.zs\"\n#include \"missing-b.zs\"", 1);
        var requests = new List<string>();
        var error = Assert.Throws<VmScriptIncludeException>(() => VmScriptSourceLoader.Load(root, path =>
        {
            requests.Add(path);
            return path == "child.zs" ? new VmScriptSource("mod.pk3:child.zs", "#include \"missing-c.zs\"\n#include \"missing-a.zs\"", 1) : null;
        }));
        Assert.Equal(new[] { "missing-a.zs", "child.zs", "missing-b.zs", "missing-c.zs" }, requests);
        Assert.Equal(new[] { new VmScriptIncludeError("missing-a.zs", root.Name, 1), new VmScriptIncludeError("missing-b.zs", root.Name, 3),
            new VmScriptIncludeError("missing-c.zs", "mod.pk3:child.zs", 1) }, error.Errors);
        Assert.StartsWith("3 errors while parsing mod.pk3:root.zs\n", error.Message);
        Assert.Throws<NotSupportedException>(() => ((IList<VmScriptIncludeError>)error.Errors).Clear());
    }

    [Fact]
    public void FatalIncludeFailuresStillStopWithoutBecomingMissingDiagnostics()
    {
        var root = new VmScriptSource("core.pk3:root.zs", "#include \"missing.zs\"\n#include \"override.zs\"\n#include \"later.zs\"", 0);
        var requests = new List<string>();
        var error = Assert.Throws<InvalidOperationException>(() => VmScriptSourceLoader.Load(root, path =>
        {
            requests.Add(path);
            return path == "missing.zs" ? null : new VmScriptSource("mod.pk3:override.zs", "", 1);
        }));
        Assert.Contains("overriding core lump override.zs", error.Message);
        Assert.Equal(new[] { "missing.zs", "override.zs" }, requests);
    }

    [Fact]
    public void ScriptSourceLoaderPreservesDiscoveryOrderAndRootVersion()
    {
        var root = new VmScriptSource("mod.pk3:scripts/root.zs", "version 4.12\n#include \"./a.zs\"\n#include \"./b.zs\"", 1);
        var catalog = new Dictionary<string, VmScriptSource>
        {
            ["scripts/a.zs"] = new("mod.pk3:scripts/a.zs", "#include \"./c.zs\"", 1),
            ["scripts/b.zs"] = new("mod.pk3:scripts/b.zs", "#include \"./a.zs\"", 1),
            ["scripts/c.zs"] = new("mod.pk3:scripts/c.zs", "#include \"./b.zs\"", 1),
        };
        var requests = new List<string>();
        var result = VmScriptSourceLoader.Load(root, path => { requests.Add(path); return catalog.GetValueOrDefault(path); });
        Assert.Equal(new Version(4, 12, 0), result.Version);
        Assert.Equal(new[] { "scripts/a.zs", "scripts/b.zs", "scripts/c.zs" }, requests);
        Assert.Equal(new[] { root.Name, catalog["scripts/a.zs"].Name, catalog["scripts/b.zs"].Name, catalog["scripts/c.zs"].Name }, result.Sources.Select(source => source.Name));
        Assert.Throws<NotSupportedException>(() => ((IList<VmScriptSource>)result.Sources).Clear());
    }

    [Fact]
    public void ScriptSourceLoaderReportsIncludeLocationAndBlocksCoreOverrides()
    {
        var root = new VmScriptSource("core.pk3:root.zs", "\n#include \"child.zs\"", 0);
        var missing = Assert.Throws<VmScriptIncludeException>(() => VmScriptSourceLoader.Load(root, path => null));
        Assert.Contains("core.pk3:root.zs, line 2", missing.Message);
        Assert.Contains("Include script lump child.zs not found", missing.Message);
        var replacement = new VmScriptSource("mod.pk3:child.zs", "", 1);
        Assert.Contains("overriding core lump child.zs", Assert.Throws<InvalidOperationException>(() => VmScriptSourceLoader.Load(root, path => replacement)).Message);
        Assert.Equal(2, VmScriptSourceLoader.Load(root, path => new("core.pk3:child.zs", "", 0)).Sources.Count);
        Assert.Equal(2, VmScriptSourceLoader.Load(root with { ContainerIndex = 1 }, path => replacement).Sources.Count);
    }

    [Fact]
    public void ScriptSourceLoaderIsolatesSessionsAndPropagatesResolverFailures()
    {
        var root = new VmScriptSource("mod.pk3:root.zs", "#include \"child.zs\"", 1);
        var error = new IOException("lookup failed");
        Assert.Same(error, Assert.Throws<IOException>(() => VmScriptSourceLoader.Load(root, path => throw error)));
        Assert.Equal(2, VmScriptSourceLoader.Load(root, path => new("mod.pk3:child.zs", "", 1)).Sources.Count);
        Assert.Equal(2, VmScriptSourceLoader.Load(root, path => new("mod.pk3:child.zs", "", 1)).Sources.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => VmScriptSourceLoader.Load(root with { ContainerIndex = -1 }, path => null));
        Assert.Throws<NotSupportedException>(() => VmScriptSourceLoader.Load(root with { Text = "version 4.14" }, path => null, new Version(4, 13)));
    }

    [Fact]
    public void SourceIncludesConcatenateConstantsAndTrackFirstStringLocation()
    {
        var includes = VmScriptIncludes.Extract("version \"4.13\"\n#include\n\"./actors/\" /*join*/ \"base.zs\"\n#include \"shared.zs\"", "mod.pk3:scripts/root.zs");
        Assert.Equal(new[] { new VmScriptInclude("scripts/actors/base.zs", "mod.pk3:scripts/root.zs", 3),
            new VmScriptInclude("shared.zs", "mod.pk3:scripts/root.zs", 4) }, includes);
    }

    [Fact]
    public void SourceIncludeExtractionIgnoresCommentsAndQuotedDirectiveText()
    {
        var source = "// #include \"bad\"\n/* #include \"bad\" */\nclass C { string s = \"#include \\\"bad\\\"\"; }\n#include \"good.zs\"";
        Assert.Equal("good.zs", Assert.Single(VmScriptIncludes.Extract(source, "mod.pk3:root.zs")).Path);
        Assert.Throws<FormatException>(() => VmScriptIncludes.Extract("#included \"not-a-directive\"", "source"));
    }

    [Theory]
    [InlineData("#include missing")]
    [InlineData("#include \"unfinished")]
    [InlineData("class C { #include \"child.zs\" }")]
    [InlineData("#include \"valid.zs\" /* unfinished")]
    public void InvalidIncludeSourcesDoNotPartiallyRegister(string source)
    {
        var queue = new VmScriptIncludeQueue();
        Assert.Throws<FormatException>(() => queue.AddSource(source, "mod.pk3:root.zs"));
        Assert.Equal(0, queue.PendingCount);
    }

    [Fact]
    public void SourceIncludeQueueIntegratesRelativePathsAndDeduplication()
    {
        var queue = new VmScriptIncludeQueue();
        queue.AddSource("#include \"./child.zs\"\n#include \"scripts/child.zs\"", "mod.pk3:scripts/root.zs");
        Assert.Equal(1, queue.PendingCount);
        Assert.True(queue.TryTake(out var child));
        queue.AddSource("#include \"./leaf.zs\"", "mod.pk3:" + child.Path);
        Assert.True(queue.TryTake(out var leaf));
        Assert.Equal("scripts/leaf.zs", leaf.Path);
    }

    [Fact]
    public void IncludeQueueDeduplicatesResolvedPathsAndRetainsFirstLocation()
    {
        var queue = new VmScriptIncludeQueue();
        Assert.True(queue.Add("./child.zs", "mod.pk3:scripts/root.zs", 7));
        Assert.False(queue.Add("scripts/child.zs", "other.pk3:root.zs", 99));
        Assert.True(queue.Add("scripts/Child.zs", "mod.pk3:root.zs", 11));
        Assert.Equal(2, queue.PendingCount);
        Assert.True(queue.TryTake(out var first));
        Assert.Equal(new VmScriptInclude("scripts/child.zs", "mod.pk3:scripts/root.zs", 7), first);
        Assert.True(queue.TryTake(out var second));
        Assert.Equal("scripts/Child.zs", second.Path);
        Assert.False(queue.TryTake(out _));
        Assert.False(queue.Add("scripts/child.zs", "mod.pk3:root.zs", 12));
    }

    [Fact]
    public void IncludeQueueProcessesNewDiscoveriesInNativeOrderAndStopsCycles()
    {
        var queue = new VmScriptIncludeQueue();
        queue.Add("a.zs", "mod.pk3:root.zs", 1);
        queue.Add("b.zs", "mod.pk3:root.zs", 2);
        var visited = new List<string>();
        while (queue.TryTake(out var include))
        {
            visited.Add(include.Path);
            if (include.Path == "a.zs") queue.Add("c.zs", "mod.pk3:a.zs", 3);
            if (include.Path == "c.zs") Assert.False(queue.Add("a.zs", "mod.pk3:c.zs", 4));
        }
        Assert.Equal(new[] { "a.zs", "b.zs", "c.zs" }, visited);
        Assert.Equal(0, queue.PendingCount);
    }

    [Fact]
    public void IncludeQueueClearsAllCompilationStateAndInvalidInputsDoNotRegister()
    {
        var queue = new VmScriptIncludeQueue();
        queue.Add("child.zs", "mod.pk3:root.zs", 1);
        queue.Clear();
        Assert.False(queue.TryTake(out _));
        Assert.True(queue.Add("child.zs", "mod.pk3:root.zs", 2));
        Assert.Throws<ArgumentNullException>(() => queue.Add(null!, "source", 1));
        Assert.Throws<ArgumentNullException>(() => queue.Add("path", null!, 1));
        Assert.Equal(1, queue.PendingCount);
        Assert.True(queue.TryTake(out var entry));
        Assert.Equal(2, entry.SourceLine);
    }

    [Theory]
    [InlineData("./child.zs", "mod.pk3:scripts/base.zs", "scripts/child.zs")]
    [InlineData("../child.zs", "mod.pk3:scripts/actors/base.zs", "scripts/child.zs")]
    [InlineData("../child.zs", "mod.pk3:scripts/base.zs", "/child.zs")]
    [InlineData("../../child.zs", "mod.pk3:scripts/base.zs", "../../child.zs")]
    [InlineData("./child.zs", "mod.pk3:base.zs", "child.zs")]
    [InlineData("../child.zs", "mod.pk3:base.zs", "../child.zs")]
    [InlineData("./child.zs", "/mods/project/:base.zs", "child.zs")]
    [InlineData("./child.zs", "scripts/base.zs", "./child.zs")]
    [InlineData("scripts/child.zs", "mod.pk3:folder/base.zs", "scripts/child.zs")]
    [InlineData("./nested/../child.zs", "mod.pk3:scripts/base.zs", "scripts/nested/../child.zs")]
    [InlineData(".\\child.zs", "mod.pk3:scripts/base.zs", ".\\child.zs")]
    public void ScriptIncludesMatchNativeArchiveRelativePathRules(string path, string source, string expected)
        => Assert.Equal(expected, VmScriptIncludes.ResolvePath(path, source));

    [Fact]
    public void ScriptIncludeResolverRejectsNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() => VmScriptIncludes.ResolvePath(null!, "mod.pk3:base.zs"));
        Assert.Throws<ArgumentNullException>(() => VmScriptIncludes.ResolvePath("child.zs", null!));
    }

    [Theory]
    [InlineData("+4.+13.+2", 4, 13, 2)]
    [InlineData(" 4. 13. 2", 4, 13, 2)]
    [InlineData("4..2", 4, 0, 2)]
    [InlineData(".13", 0, 13, 0)]
    [InlineData("4.13.", 4, 13, 0)]
    [InlineData("-0.13", 0, 13, 0)]
    public void ScriptVersionComponentsFollowNativeNumericConversion(string text, int major, int minor, int revision)
        => Assert.Equal(new Version(major, minor, revision), VmScriptVersion.Read("version \"" + text + "\""));

    [Theory]
    [InlineData("4.-1")]
    [InlineData("4.+")]
    [InlineData("4.13 ")]
    [InlineData("4.999999999999999999999999999")]
    [InlineData("4.13.32768")]
    public void ScriptVersionRejectsInvalidOrUnrepresentableNumericComponents(string text)
        => Assert.Throws<FormatException>(() => VmScriptVersion.Read("version \"" + text + "\""));

    [Theory]
    [InlineData("#region header\nversion \"4.13\"")]
    [InlineData("#region header\r\n#endregion\r\nversion 4.13")]
    [InlineData("version #region metadata\n4.13")]
    [InlineData("/* comment */ #endregion\nversion \"4.13\"")]
    public void ScriptVersionSkipsNativeRegionDirectiveLines(string source)
        => Assert.Equal(new Version(4, 13, 0), VmScriptVersion.Read(source));

    [Theory]
    [InlineData("version \"4.13\\\"extra\"")]
    [InlineData("version \"4.13\\\"")]
    [InlineData("version ;")]
    [InlineData("version ;\n")]
    public void ScriptVersionHandlesEscapedQuotesAndShortestCommentsWithoutIndexErrors(string source)
        => Assert.Throws<FormatException>(() => VmScriptVersion.Read(source));

    [Theory]
    [InlineData("version 4.13", 4, 13, 0)]
    [InlineData("version 4.12.2; next", 4, 12, 2)]
    [InlineData("version /*header*/ 4.13/*tail*/", 4, 13, 0)]
    [InlineData("version ; comment\n4.13", 4, 13, 0)]
    [InlineData("version 4.13{}", 4, 13, 0)]
    public void ScriptVersionAcceptsNativeUnquotedStringTokens(string source, int major, int minor, int revision)
        => Assert.Equal(new Version(major, minor, revision), VmScriptVersion.Read(source));

    [Theory]
    [InlineData("version 4.13suffix")]
    [InlineData("version 4.13/extra")]
    [InlineData("version ; comment")]
    [InlineData("version {}")]
    public void ScriptVersionRejectsInvalidUnquotedStringTokens(string source)
        => Assert.Throws<FormatException>(() => VmScriptVersion.Read(source));

    [Theory]
    [InlineData("version \"4.13\"", 4, 13, 0)]
    [InlineData("/*header*/ VERSION // comment\n \"4.12.2\"", 4, 12, 2)]
    [InlineData("\uFEFF // header\nversion \"4.13.0\"; class Example {}", 4, 13, 0)]
    [InlineData("class Example {} version \"4.13\"", 2, 3, 0)]
    [InlineData("// version \"4.13\"", 2, 3, 0)]
    public void ScriptVersionReadsOnlyLeadingDirective(string source, int major, int minor, int revision)
        => Assert.Equal(new Version(major, minor, revision), VmScriptVersion.Read(source));

    [Theory]
    [InlineData("version \"4\"")]
    [InlineData("version \"4.13.0.1\"")]
    [InlineData("version \"4.bad\"")]
    [InlineData("version \"65535.0\"")]
    [InlineData("version \"4.13")]
    [InlineData("/* unfinished")]
    public void ScriptVersionRejectsMalformedHeaders(string source)
        => Assert.Throws<FormatException>(() => VmScriptVersion.Read(source));

    [Fact]
    public void DeclaredScriptVersionControlsNamedBindingAndChecksSupportedMaximum()
    {
        var function = new VmScriptFunction([VmCode.RetInt(0)], [], [VmValueType.Integer], parameterNames: ["value"]);
        var args = new[] { new VmCallArgument(VmValue.FromInteger(42), "value") };
        Assert.Equal(VmFault.InvalidCall, VmExec.RunNamedFunction(function, args, scriptVersion: VmScriptVersion.Read("version \"4.12\"")).Fault);
        Assert.Equal(42, VmExec.RunNamedFunction(function, args, scriptVersion: VmScriptVersion.Read("version \"4.13\"")).Values[0].Value);
        Assert.Equal(new Version(4, 13, 0), VmScriptVersion.Read("version \"4.13\"", new Version(4, 13)));
        Assert.Throws<NotSupportedException>(() => VmScriptVersion.Read("version \"4.14\"", new Version(4, 13)));
        Assert.Throws<ArgumentNullException>(() => VmScriptVersion.Read(null!));
    }

    [Theory]
    [InlineData(12, false)]
    [InlineData(13, true)]
    public void NamedRequiredArgumentsFollowNativeVersionBoundary(int minor, bool relaxed)
    {
        var function = new VmScriptFunction([VmCode.RetInt(0)], [], [VmValueType.Integer], parameterNames: ["value"]);
        var result = VmExec.RunNamedFunction(function, [new(VmValue.FromInteger(42), "value")], scriptVersion: new Version(4, minor));
        Assert.Equal(relaxed, result.Returned);
        Assert.Equal(relaxed ? VmFault.None : VmFault.InvalidCall, result.Fault);
        Assert.Equal(42, VmExec.RunNamedFunction(function, [new(VmValue.FromInteger(42))], scriptVersion: new Version(4, minor)).Values[0].Value);
    }

    [Fact]
    public void LegacyNamedArgumentsAllowOptionalForwardSkipsAndPositionalContinuation()
    {
        var function = new VmScriptFunction([VmCode.Rrr(VmOp.Ret, 0, 0, 0), VmCode.Rrr(VmOp.Ret, 1, 0, 1),
            VmCode.Rrr(VmOp.Ret, 2, 0, 2), VmCode.Rrr(VmOp.Ret, VmCode.RetFinal | 3, 0, 3)], [],
            [VmValueType.Integer, VmValueType.Integer, VmValueType.Integer, VmValueType.Integer],
            trailingDefaults: [VmValue.FromInteger(20), VmValue.FromInteger(30), VmValue.FromInteger(40)], parameterNames: ["a", "b", "c", "d"]);
        var result = VmExec.RunNamedFunction(function, [new(VmValue.FromInteger(1)), new(VmValue.FromInteger(3), "c"), new(VmValue.FromInteger(4))], 4,
            scriptVersion: new Version(4, 12));
        Assert.Equal(new[] { 1, 20, 3, 4 }, result.Values.Select(value => value.Value));
        Assert.Equal(VmFault.InvalidCall, VmExec.RunNamedFunction(function,
            [new(VmValue.FromInteger(1)), new(VmValue.FromInteger(3), "c"), new(VmValue.FromInteger(2), "b")], scriptVersion: new Version(4, 12)).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.RunNamedFunction(function,
            [new(VmValue.FromInteger(1)), new(VmValue.FromInteger(2), "b"), new(VmValue.FromInteger(3), "b")], scriptVersion: new Version(4, 12)).Fault);
    }

    [Fact]
    public void LegacyNamedOptionalArgumentsStillRequireEarlierRequiredValues()
    {
        var function = new VmScriptFunction([VmCode.RetInt(0)], [], [VmValueType.Integer, VmValueType.Integer],
            trailingDefaults: [VmValue.FromInteger(7)], parameterNames: ["required", "optional"]);
        Assert.Equal(VmFault.ParameterRange, VmExec.RunNamedFunction(function, [new(VmValue.FromInteger(42), "optional")], scriptVersion: new Version(4, 12)).Fault);
        Assert.Equal(42, VmExec.RunNamedFunction(function, [new(VmValue.FromInteger(42)), new(VmValue.FromInteger(9), "optional")], scriptVersion: new Version(4, 12)).Values[0].Value);
    }

    [Fact]
    public void NamedScriptArgumentsReorderRequiredValuesAndFillSkippedDefaults()
    {
        var names = new[] { "first", "second", "third" };
        var function = new VmScriptFunction([VmCode.Rrr(VmOp.Ret, 0, 0, 0), VmCode.Rrr(VmOp.Ret, 1, 0, 1),
            VmCode.Rrr(VmOp.Ret, VmCode.RetFinal | 2, 0, 2)], [], [VmValueType.Integer, VmValueType.Integer, VmValueType.Integer],
            trailingDefaults: [VmValue.FromInteger(20), VmValue.FromInteger(30)], parameterNames: names);
        names[0] = "changed";
        var result = VmExec.RunNamedFunction(function, [new(VmValue.FromInteger(33), "third"), new(VmValue.FromInteger(11), "FIRST")], 3);
        Assert.Equal(new[] { 11, 20, 33 }, result.Values.Select(value => value.Value));
        result = VmExec.RunNamedFunction(function, [new(VmValue.FromInteger(11), "first"), new(VmValue.FromInteger(22))], 3);
        Assert.Equal(new[] { 11, 22, 30 }, result.Values.Select(value => value.Value));
        result = VmExec.RunNamedFunction(function, [new(VmValue.FromInteger(1), "first"), new(VmValue.FromInteger(2), "first")], 3);
        Assert.Equal(new[] { 2, 20, 30 }, result.Values.Select(value => value.Value));
    }

    [Fact]
    public void NamedScriptArgumentsBindVectorsAsLogicalParameters()
    {
        var function = new VmScriptFunction([VmCode.RetVector(0, 3)], [], [VmValueType.Integer, VmValueType.Vector3],
            trailingDefaults: [VmValue.FromVector3(1, 2, 3)], parameterNames: ["number", "vector"]);
        var result = VmExec.RunNamedFunction(function, [new(VmValue.FromVector3(-0.0, 5, 6), "vector"), new(VmValue.FromInteger(42), "number")]);
        Assert.Equal(new VmVectorResult(3, -0.0, 5, 6, 0), result.Values[0].VectorValue);
        Assert.Equal(long.MinValue, BitConverter.DoubleToInt64Bits(result.Values[0].VectorValue!.Value.X));
        Assert.Equal(VmFault.InvalidCall, VmExec.RunNamedFunction(function, [new(VmValue.FromInteger(1)), new(VmValue.FromVector2(1, 2), "vector")]).Fault);
    }

    [Fact]
    public void NamedScriptArgumentsRejectUnknownNamesMissingRequiredAndOverflow()
    {
        var function = new VmScriptFunction([VmCode.Reti(42)], [], [VmValueType.Integer, VmValueType.Integer], parameterNames: ["a", "b"]);
        Assert.Equal(VmFault.InvalidCall, VmExec.RunNamedFunction(function, [new(VmValue.FromInteger(1), "unknown")]).Fault);
        Assert.Equal(VmFault.ParameterRange, VmExec.RunNamedFunction(function, [new(VmValue.FromInteger(1), "b")]).Fault);
        Assert.Equal(VmFault.ParameterRange, VmExec.RunNamedFunction(function, [new(VmValue.FromInteger(1), "b"), new(VmValue.FromInteger(2))]).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.RunNamedFunction(new VmScriptFunction([], [], [VmValueType.Integer]), [new(VmValue.FromInteger(1), "a")]).Fault);
        Assert.Throws<ArgumentException>(() => new VmScriptFunction([], [], [VmValueType.Integer], parameterNames: []));
        Assert.Throws<ArgumentException>(() => new VmScriptFunction([], [], [VmValueType.Integer, VmValueType.Integer], parameterNames: ["a", "A"]));
    }

    [Fact]
    public void VariadicMetadataMatchesNativeHiddenArgumentLayout()
    {
        var pointer = new object();
        var packed = VmNativeParameters.PackVariadicArguments([VmValue.FromInteger(42), VmValue.FromVector3(-0.0, 2, 3),
            VmValue.FromString("text"), VmValue.FromObject(pointer)]);
        Assert.Equal(7, packed.Count);
        Assert.Equal(new byte[] { 0, 1, 1, 1, 2, 3 }, Assert.IsType<byte[]>(packed[^1].Object));
        var types = VmNativeParameters.ReadVariadicTypes(packed);
        Assert.Equal(new[] { VmValueType.Integer, VmValueType.Float, VmValueType.Float, VmValueType.Float, VmValueType.String, VmValueType.Object }, types);
        Assert.Equal(long.MinValue, BitConverter.DoubleToInt64Bits(packed[1].Float));
        Assert.Same(pointer, packed[5].Object);
        Assert.Empty(VmNativeParameters.ReadVariadicTypes(VmNativeParameters.PackVariadicArguments([])));
        Assert.Throws<NotSupportedException>(() => ((IList<VmValueType>)types)[0] = VmValueType.Object);
    }

    [Fact]
    public void VariadicMetadataFeedsDirectAndBytecodeCallbacks()
    {
        var function = new VmFunction((args, count) => [VmValue.FromInteger(VmNativeParameters.ReadVariadicTypes(args).Count)]);
        var packed = VmNativeParameters.PackVariadicArguments([VmValue.FromString("format"), VmValue.FromInteger(7)]);
        Assert.Equal(2, VmExec.RunNativeFunction(function, packed).Values[0].Value);
        var code = new[] { VmCode.Rrr(VmOp.Param, 6, 0, 0), VmCode.Rrr(VmOp.ParamImmediate, 7, 0, 0),
            VmCode.Rrr(VmOp.Param, 7, 1, 0), VmCode.Rrr(VmOp.CallK, 0, 3, 1), VmCode.Rrr(VmOp.Result, 0, 0, 0), VmCode.RetInt(0) };
        Assert.Equal(2, VmExec.Run(code, [], [], ["format"], objectConstants: [function, packed[^1].Object]).Value);
    }

    [Fact]
    public void VariadicMetadataRejectsMalformedTablesAndReservesHiddenSlot()
    {
        Assert.Throws<ArgumentException>(() => VmNativeParameters.PackVariadicArguments([VmValue.Nil()]));
        Assert.Throws<ArgumentException>(() => VmNativeParameters.PackVariadicArguments(Enumerable.Repeat(VmValue.FromInteger(1), 256).ToArray()));
        Assert.Equal(256, VmNativeParameters.PackVariadicArguments(Enumerable.Repeat(VmValue.FromInteger(1), 255).ToArray()).Count);
        Assert.Throws<ArgumentException>(() => VmNativeParameters.ReadVariadicTypes([]));
        Assert.Throws<ArgumentException>(() => VmNativeParameters.ReadVariadicTypes([VmValue.FromObject(new byte[] { 0 })]));
        Assert.Throws<ArgumentException>(() => VmNativeParameters.ReadVariadicTypes([VmValue.FromFloat(1), VmValue.FromObject(new byte[] { 0 })]));
        Assert.Throws<ArgumentException>(() => VmNativeParameters.ReadVariadicTypes([VmValue.Nil(), VmValue.FromObject(new byte[] { 128 })]));
    }

    [Fact]
    public void NativeObjectParametersAcceptDerivedIdentityAndOptionalNull()
    {
        var instance = new CastDerived();
        Assert.Same(instance, VmNativeParameters.ReadObject<CastBase>(VmValue.FromObject(instance)));
        Assert.Same(instance, VmNativeParameters.RequireObject<CastDerived>(VmValue.FromObject(instance), "self"));
        Assert.Null(VmNativeParameters.ReadObject<CastBase>(VmValue.FromObject(null)));
        var abort = Assert.Throws<VmAbortException>(() => VmNativeParameters.RequireObject<CastBase>(VmValue.FromObject(null), "self"));
        Assert.Equal("In function parameter self", abort.Context);
        Assert.Throws<ArgumentException>(() => VmNativeParameters.ReadObject<CastDerived>(VmValue.FromObject(new CastBase())));
        Assert.Throws<ArgumentException>(() => VmNativeParameters.ReadObject<CastBase>(VmValue.FromObject(new CastUnrelated())));
        Assert.Throws<ArgumentException>(() => VmNativeParameters.ReadObject<object>(VmValue.FromObject(typeof(CastBase))));
        Assert.Throws<ArgumentException>(() => VmNativeParameters.ReadObject<object>(VmValue.FromObject(42)));
        Assert.Throws<ArgumentException>(() => VmNativeParameters.ReadObject<IDisposable>(VmValue.FromObject(null)));
    }

    [Fact]
    public void NativeClassParametersCheckDescendantsAndOptionalNull()
    {
        Assert.Same(typeof(CastDerived), VmNativeParameters.ReadClass(VmValue.FromObject(typeof(CastDerived)), typeof(CastBase)));
        Assert.Same(typeof(CastBase), VmNativeParameters.RequireClass(VmValue.FromObject(typeof(CastBase)), typeof(CastBase), "type"));
        Assert.Null(VmNativeParameters.ReadClass(VmValue.FromObject(null), typeof(CastBase)));
        var abort = Assert.Throws<VmAbortException>(() => VmNativeParameters.RequireClass(VmValue.FromObject(null), typeof(CastBase), "type"));
        Assert.Equal("In function parameter type", abort.Context);
        Assert.Throws<ArgumentException>(() => VmNativeParameters.ReadClass(VmValue.FromObject(typeof(CastBase)), typeof(CastDerived)));
        Assert.Throws<ArgumentException>(() => VmNativeParameters.ReadClass(VmValue.FromObject(new CastBase()), typeof(CastBase)));
        Assert.Throws<ArgumentException>(() => VmNativeParameters.ReadClass(VmValue.FromObject(typeof(int)), typeof(object)));
        Assert.Throws<ArgumentException>(() => VmNativeParameters.ReadClass(VmValue.FromObject(null), typeof(IDisposable)));
        Assert.Throws<ArgumentNullException>(() => VmNativeParameters.ReadClass(VmValue.FromObject(null), null!));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NativeTypedNullChecksRetainFramesWhileTypeErrorsPropagate(bool classParameter)
    {
        var function = new VmFunction((args, count) => [VmValue.FromObject(classParameter
            ? VmNativeParameters.RequireClass(args[0], typeof(CastBase), "target")
            : VmNativeParameters.RequireObject<CastBase>(args[0], "target"))], "CheckTarget");
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 0, 1, 0), VmCode.Rrr(VmOp.Param, 3, 0, 0), VmCode.Rrr(VmOp.CallK, 0, 1, 1),
            VmCode.Rrr(VmOp.Result, 0, 3, 0), VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 3, 0) };
        var abort = VmExec.Run(code, [], [], [], objectConstants: [function, null]);
        Assert.Equal((ushort)1, abort.AbortCode); Assert.Equal("In function parameter target", abort.AbortContext);
        Assert.Equal("Called from CheckTarget\n", abort.AbortStackTrace);
        object valid = classParameter ? typeof(CastDerived) : new CastDerived();
        Assert.Same(valid, VmExec.Run(code, [], [], [], objectConstants: [function, valid]).ObjectValue);
        Assert.Throws<ArgumentException>(() => VmExec.Run(code, [], [], [], objectConstants: [function, new CastUnrelated()]));
    }

    [Fact]
    public void NativeRequiredPointerPreservesIdentityAndReportsNamedNullAbort()
    {
        var pointer = new object();
        Assert.Same(pointer, VmNativeParameters.RequirePointer(VmValue.FromObject(pointer), "self"));
        var descriptor = typeof(CastBase);
        Assert.Same(descriptor, VmNativeParameters.RequirePointer(VmValue.FromObject(descriptor), "classType"));
        var exception = Assert.Throws<VmAbortException>(() => VmNativeParameters.RequirePointer(VmValue.FromObject(null), "self"));
        Assert.Equal((ushort)1, exception.Reason);
        Assert.Equal("In function parameter self", exception.Context);
        Assert.Equal("VM execution aborted: tried to read from address zero. In function parameter self", exception.Message);
        Assert.Throws<ArgumentNullException>(() => VmNativeParameters.RequirePointer(null!, "self"));
        Assert.Throws<ArgumentNullException>(() => VmNativeParameters.RequirePointer(VmValue.FromObject(pointer), null!));
        Assert.Throws<ArgumentException>(() => VmNativeParameters.RequirePointer(VmValue.FromInteger(0), "self"));
    }

    [Fact]
    public void NativeRequiredPointerAbortFlowsThroughBytecodeAndDirectEntry()
    {
        var callback = new VmFunction((args, count) => [VmValue.FromObject(VmNativeParameters.RequirePointer(args[0], "target"))],
            "Native.CheckTarget", prototype: new VmNativePrototype([VmValueType.Object]));
        var function = new VmScriptFunction([VmCode.Rrr(VmOp.Param, 3, 0, 0), VmCode.Rrr(VmOp.CallK, 0, 1, 1),
            VmCode.Rrr(VmOp.Result, 0, 3, 0), VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 3, 0)], [], [VmValueType.Object],
            objectConstants: [callback], printableName: "Script.Check", sourceFileName: "check.zs", lineInfo: [new(0, 17)]);
        var aborted = VmExec.RunFunction(function, [VmValue.FromObject(null)]);
        Assert.False(aborted.Returned); Assert.Empty(aborted.Values);
        Assert.Equal((ushort)1, aborted.AbortCode);
        Assert.Equal("In function parameter target", aborted.AbortContext);
        Assert.Equal("Called from Native.CheckTarget\nCalled from Script.Check at check.zs, line 17\n", aborted.AbortStackTrace);
        var direct = Assert.Throws<VmAbortException>(() => VmExec.RunNativeFunction(callback, [VmValue.FromObject(null)]));
        Assert.Equal(aborted.AbortMessage, direct.Message);
        var pointer = new object();
        Assert.Same(pointer, VmExec.RunFunction(function, [VmValue.FromObject(pointer)]).Values[0].ObjectValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NativeOutPrototypesAcceptMatchingRegisterReferences(int type)
    {
        var flags = new[] { true };
        var prototype = new VmNativePrototype([(VmValueType)type], flags);
        flags[0] = false;
        var function = new VmFunction((args, count) =>
        {
            var reference = Assert.IsType<VmRegisterReference>(args[0].Object);
            reference.Value = type == 0 ? VmValue.FromInteger(42) : type == 1 ? VmValue.FromFloat(2.5) : VmValue.FromString("changed");
            return [];
        }, prototype: prototype);
        var result = VmExec.Run([VmCode.Rrr(VmOp.Param, 32 + type, 255, 0), VmCode.Rrr(VmOp.CallK, 0, 1, 0),
            VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, type, 255)], [], [], [], objectConstants: [function]);
        Assert.True(result.Returned);
        if (type == 0) Assert.Equal(42, result.Value);
        else if (type == 1) Assert.Equal(2.5, result.FloatValue);
        else Assert.Equal("changed", result.StringValue);
        Assert.Equal(VmFault.InvalidCall, VmExec.Run([VmCode.Rrr(VmOp.Param, 32 + (type + 1) % 3, 0, 0),
            VmCode.Rrr(VmOp.CallK, 0, 1, 0)], [], [], [], objectConstants: [function]).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.RunNativeFunction(function, [VmValue.FromObject(null)], 0).Fault);
    }

    [Fact]
    public void NativeOutPrototypeFlagsPreservePointerArgumentsAndRejectUnsupportedDefinitions()
    {
        var prototype = new VmNativePrototype([VmValueType.Object], [true]);
        var function = new VmFunction((args, count) => [args[0]], prototype: prototype);
        Assert.True(VmExec.RunNativeFunction(function, [VmValue.FromObject(null)]).Values[0].HasObjectValue);
        Assert.Throws<ArgumentException>(() => new VmNativePrototype([], [true]));
        Assert.Throws<ArgumentException>(() => new VmNativePrototype([VmValueType.Vector3], [true]));
        Assert.Throws<ArgumentException>(() => new VmFunction((args, count) => [],
            prototype: new VmNativePrototype([VmValueType.Integer], [true]), defaultArguments: [VmValue.FromObject(new object())]));
    }

    [Fact]
    public void NativePrototypesValidateExpandedArgumentsAtEveryEntry()
    {
        var types = new[] { VmValueType.Integer, VmValueType.Vector2, VmValueType.String, VmValueType.Object };
        var prototype = new VmNativePrototype(types);
        types[0] = VmValueType.Object;
        var calls = 0;
        var function = new VmFunction((args, count) => { calls++; return [VmValue.FromInteger(args[0].Integer)]; },
            prototype: prototype, defaultArguments: [VmValue.FromInteger(7), VmValue.FromVector2(1, 2), VmValue.FromString("text"), VmValue.FromObject(null)]);
        Assert.Equal(42, VmExec.RunNativeFunctionWithDefaults(function, [VmValue.FromInteger(42)]).Values[0].Value);
        Assert.Equal(7, VmExec.RunNativeFunction(function, [VmValue.FromInteger(7), VmValue.FromVector2(1, 2), VmValue.FromString("text"), VmValue.FromObject(null)]).Values[0].Value);
        Assert.Equal(VmFault.ParameterRange, VmExec.RunNativeFunction(function, []).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.RunNativeFunctionWithDefaults(function, [VmValue.FromString("bad")]).Fault);
        Assert.Equal(VmFault.ParameterRange, VmExec.Run([VmCode.Rrr(VmOp.CallK, 0, 0, 0)], [], [], [], objectConstants: [function]).Fault);
        Assert.Equal(2, calls);
    }

    [Fact]
    public void BytecodeNativePrototypeRejectsWrongBankBeforeCalling()
    {
        var called = false;
        var function = new VmFunction((args, count) => { called = true; return [args[0]]; }, prototype: new VmNativePrototype([VmValueType.Integer]));
        Assert.Equal(VmFault.InvalidCall, VmExec.Run([VmCode.Rrr(VmOp.Param, 1, 0, 0), VmCode.Rrr(VmOp.CallK, 0, 1, 1),
            VmCode.Rrr(VmOp.Result, 0, 0, 0)], [], [], [], objectConstants: [function]).Fault);
        Assert.False(called);
        Assert.Equal(42, VmExec.Run([VmCode.Rrr(VmOp.ParamImmediate, 42, 0, 0), VmCode.Rrr(VmOp.CallK, 0, 1, 1),
            VmCode.Rrr(VmOp.Result, 0, 0, 0), VmCode.RetInt(0)], [], [], [], objectConstants: [function]).Value);
    }

    [Fact]
    public void NativePrototypeDefinitionsRejectInvalidTypesCapacityAndDefaults()
    {
        Assert.Throws<ArgumentException>(() => new VmNativePrototype([VmValueType.Nil]));
        Assert.Throws<ArgumentException>(() => new VmNativePrototype([(VmValueType)32]));
        Assert.Throws<ArgumentException>(() => new VmNativePrototype(Enumerable.Repeat(VmValueType.Vector4, 65).ToArray()));
        Assert.Throws<ArgumentException>(() => new VmFunction((args, count) => [], prototype: new VmNativePrototype([VmValueType.Integer]),
            defaultArguments: [VmValue.FromString("bad")]));
        Assert.Throws<ArgumentException>(() => new VmFunction((args, count) => [], prototype: new VmNativePrototype([]), defaultArguments: [VmValue.FromInteger(1)]));
    }

    [Fact]
    public void NativeDefaultsAppendOnlyMissingFlatPositions()
    {
        var defaults = new[] { VmValue.FromInteger(7), VmValue.FromVector2(1, 2), VmValue.FromString("default"), VmValue.FromObject(null) };
        var function = new VmFunction((args, count) => [VmValue.FromInteger(args.Count), args[^2]], defaultArguments: defaults);
        defaults[3] = VmValue.FromString("changed");
        var supplied = new[] { VmValue.FromInteger(42), VmValue.FromVector2(3, 4) };
        var result = VmExec.RunNativeFunctionWithDefaults(function, supplied, 2);
        Assert.Equal(5, result.Values[0].Value); Assert.Equal("default", result.Values[1].StringValue);
        Assert.Equal(2, supplied.Length);
        Assert.Equal(5, VmExec.RunNativeFunctionWithDefaults(function, [], 2).Values[0].Value);
        Assert.Equal("explicit", VmExec.RunNativeFunctionWithDefaults(function,
            [VmValue.FromInteger(1), VmValue.FromVector2(5, 6), VmValue.FromString("explicit"), VmValue.FromObject(null)], 2).Values[1].StringValue);
    }

    [Fact]
    public void NativeIntegerDefaultsDoNotAffectOrdinaryOrBytecodeCalls()
    {
        var defaults = new[] { 7, 8, 9 };
        var function = new VmIntegerFunction((args, count) => [args.Sum()], defaultArguments: defaults);
        defaults[1] = 99;
        Assert.Equal(59, VmExec.RunNativeFunctionWithDefaults(function, [42]).Values[0].Value);
        Assert.Equal(24, VmExec.RunNativeFunctionWithDefaults(function, []).Values[0].Value);
        Assert.Equal(10, VmExec.RunNativeFunctionWithDefaults(function, [1, 2, 3, 4]).Values[0].Value);
        Assert.Equal(42, VmExec.RunNativeFunction(function, [42]).Values[0].Value);
        Assert.Equal(42, VmExec.Run([VmCode.Rrr(VmOp.ParamImmediate, 42, 0, 0), VmCode.Rrr(VmOp.CallK, 0, 1, 1),
            VmCode.Rrr(VmOp.Result, 0, 0, 0), VmCode.RetInt(0)], [], [], [], objectConstants: [function]).Value);
    }

    [Fact]
    public void NativeDefaultsValidateCapacityAndEntriesBeforeInvocation()
    {
        Assert.Throws<ArgumentException>(() => new VmFunction((args, count) => [], defaultArguments: [null!]));
        Assert.Throws<ArgumentException>(() => new VmFunction((args, count) => [],
            defaultArguments: Enumerable.Repeat(VmValue.FromVector4(1, 2, 3, 4), 65).ToArray()));
        Assert.Throws<ArgumentException>(() => new VmIntegerFunction((args, count) => [], defaultArguments: new int[257]));
        var function = new VmFunction((args, count) => throw new InvalidOperationException("Must not invoke."), defaultArguments: [VmValue.FromInteger(1)]);
        Assert.Equal(VmFault.InvalidCall, VmExec.RunNativeFunctionWithDefaults(function, [null!]).Fault);
        Assert.Equal(VmFault.ParameterRange, VmExec.RunNativeFunctionWithDefaults(function, Enumerable.Repeat(VmValue.FromInteger(0), 257).ToArray()).Fault);
    }

    [Fact]
    public void DirectNativeEntryExpandsVectorsAndPreservesTypedResults()
    {
        var reference = new object();
        var callback = new VmFunction((args, count) =>
        {
            Assert.Equal(7, count);
            Assert.Equal(new[] { VmValueType.Integer, VmValueType.Float, VmValueType.Float, VmValueType.String, VmValueType.Object, VmValueType.Nil }, args.Select(value => value.Type));
            Assert.Equal(-0.0, args[1].Float); Assert.Equal(2, args[2].Float);
            return [VmValue.FromInteger(42), VmValue.FromFloat(-0.0), VmValue.FromString("text"), VmValue.FromObject(reference),
                VmValue.FromVector2(1, 2), VmValue.FromVector3(1, 2, 3), VmValue.FromVector4(1, 2, 3, 4)];
        });
        var result = VmExec.RunNativeFunction(callback, [VmValue.FromInteger(7), VmValue.FromVector2(-0.0, 2),
            VmValue.FromString("input"), VmValue.FromObject(null), VmValue.Nil()], 7);
        Assert.True(result.Returned); Assert.Equal(42, result.Values[0].Value);
        Assert.Equal(long.MinValue, BitConverter.DoubleToInt64Bits(result.Values[1].FloatValue!.Value));
        Assert.Equal("text", result.Values[2].StringValue); Assert.Same(reference, result.Values[3].ObjectValue);
        Assert.Equal(2, result.Values[4].VectorValue!.Value.Components);
        Assert.Equal(3, result.Values[5].VectorValue!.Value.Components);
        Assert.Equal(4, result.Values[6].VectorValue!.Value.Components);
    }

    [Fact]
    public void DirectNativeEntryAllowsFewerResultsAndValidatesCallbackOutput()
    {
        Assert.Empty(VmExec.RunNativeFunction(new VmFunction((args, count) => []), [], 3).Values);
        Assert.True(VmExec.RunNativeFunction(new VmIntegerFunction((args, count) => [args[0] + 1]), [41], 3).Returned);
        Assert.Equal(VmFault.InvalidCall, VmExec.RunNativeFunction(new VmFunction((args, count) => [VmValue.Nil()]), []).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.RunNativeFunction(new VmFunction((args, count) => null!), []).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.RunNativeFunction(new VmIntegerFunction((args, count) => [1]), [], 0).Fault);
        var invoked = false;
        var function = new VmFunction((args, count) => { invoked = true; return []; });
        Assert.Equal(VmFault.ParameterRange, VmExec.RunNativeFunction(function, Enumerable.Repeat(VmValue.FromVector4(1, 2, 3, 4), 65).ToArray()).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.RunNativeFunction(function, [null!]).Fault);
        Assert.False(invoked);
        Assert.Throws<ArgumentOutOfRangeException>(() => VmExec.RunNativeFunction(function, [], 129));
    }

    [Fact]
    public void DirectNativeEntryPropagatesAbortAndClrExceptions()
    {
        var abort = new VmAbortException(0, "direct abort");
        Assert.Same(abort, Assert.Throws<VmAbortException>(() => VmExec.RunNativeFunction(new VmFunction((args, count) => throw abort), [])));
        Assert.Same(abort, Assert.Throws<VmAbortException>(() => VmExec.RunNativeFunction(new VmIntegerFunction((args, count) => throw abort), [])));
        var error = new InvalidOperationException("bug");
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => VmExec.RunNativeFunction(new VmFunction((args, count) => throw error), [])));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CallbackAbortsKeepNativeAndScriptFrames(bool integer)
    {
        var exception = new VmAbortException(6, "Actor.Tick");
        object callback = integer
            ? new VmIntegerFunction((args, count) => throw exception, "Native.Tick")
            : new VmFunction((args, count) => throw exception, "Native.Tick");
        var script = new VmScriptFunction([VmCode.Rrr(VmOp.CallK, 0, 0, 1), VmCode.Rrr(VmOp.Result, 0, 0, 0), VmCode.Reti(7)], [], [],
            objectConstants: [callback], printableName: "Script.Tick", sourceFileName: "actor.zs", lineInfo: [new(0, 12)]);
        var result = VmExec.RunFunction(script, []);
        Assert.False(result.Returned); Assert.Empty(result.Values);
        Assert.Equal(VmFault.ScriptAbort, result.Fault);
        Assert.Equal((ushort)6, result.AbortCode);
        Assert.Equal("Actor.Tick", result.AbortContext);
        Assert.Equal(exception.Message, result.AbortMessage);
        Assert.Equal(new[] { new VmAbortFrame("Native.Tick", "", -1, -1) { IsNative = true },
            new VmAbortFrame("Script.Tick", "actor.zs", 0, 12) }, result.AbortFrames);
        Assert.Equal("Called from Native.Tick\nCalled from Script.Tick at actor.zs, line 12\n", result.AbortStackTrace);
        var raw = VmExec.Run([VmCode.Rrr(VmOp.CallK, 0, 0, 0)], [], [], [], objectConstants: [callback]);
        Assert.True(Assert.Single(raw.AbortFrames!).IsNative);
        Assert.Equal("Called from Native.Tick\n", raw.AbortStackTrace);
        Assert.Equal(result.AbortFrames, VmExec.RunFunction(script, []).AbortFrames);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CallbackClrErrorsContinueToPropagate(bool integer)
    {
        var exception = new InvalidOperationException("callback bug");
        object callback = integer ? new VmIntegerFunction((args, count) => throw exception)
            : new VmFunction((args, count) => throw exception);
        Assert.Same(exception, Assert.Throws<InvalidOperationException>(() =>
            VmExec.Run([VmCode.Rrr(VmOp.CallK, 0, 0, 0)], [], [], [], objectConstants: [callback])));
    }

    [Theory]
    [InlineData(0, null, "VM execution aborted: ")]
    [InlineData(0, "detail", "VM execution aborted: detail")]
    [InlineData(1, "parameter value", "VM execution aborted: tried to read from address zero. parameter value")]
    [InlineData(1, "", "VM execution aborted: tried to read from address zero. ")]
    [InlineData(9, "detail", "VM execution aborted: Unknown reason 9 detail")]
    public void AbortContextUsesNativeSpacing(int reason, string? context, string expected)
        => Assert.Equal(expected, VmAbortDiagnostics.FormatMessage((ushort)reason, context));

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ScopeAbortContextNamesFunctionsAndSurvivesNestedCalls(int kind)
    {
        object target = kind switch
        {
            0 => new VmScriptFunction([VmCode.Reti(7)], [], [], printableName: "Actor.Tick"),
            1 => new VmFunction((args, count) => throw new InvalidOperationException("Scope must reject before invocation."), "Actor.Tick"),
            _ => new VmIntegerFunction((args, count) => throw new InvalidOperationException("Scope must reject before invocation."), "Actor.Tick"),
        };
        var metadata = new VmClassMetadata(new Dictionary<Type, object?>(), scopes:
            new Dictionary<Type, VmClassScope> { [typeof(CastBase)] = VmClassScope.Play });
        var guarded = new VmScriptFunction([VmCode.Rrr(VmOp.Lkp, 0, 0, 0), VmCode.Rrr(VmOp.Scope, 0, 2, 1)], [], [],
            objectConstants: [new CastBase(), target], printableName: "Guard", lineInfo: [new(0, 42)]);
        const string context = "Cannot call play function Actor.Tick from ui context";
        var direct = VmExec.RunFunction(guarded, [], classMetadata: metadata);
        Assert.Equal(context, direct.AbortContext);
        Assert.Equal("VM execution aborted: " + context, direct.AbortMessage);
        Assert.Single(direct.AbortFrames!);
        var nested = VmExec.RunWithReturns([VmCode.Rrr(VmOp.CallK, 0, 0, 0)], [], [], [], 0,
            objectConstants: [guarded], classMetadata: metadata);
        Assert.Equal(context, nested.AbortContext);
        Assert.Equal(direct.AbortMessage, nested.AbortMessage);
        Assert.Throws<ArgumentNullException>(() => new VmFunction((args, count) => [], null!));
        Assert.Throws<ArgumentNullException>(() => new VmIntegerFunction((args, count) => [], null!));
    }

    [Theory]
    [InlineData(0, "")]
    [InlineData(1, "tried to read from address zero.")]
    [InlineData(2, "tried to write to address zero.")]
    [InlineData(3, "too many try-catch blocks.")]
    [InlineData(4, "array access out of bounds.")]
    [InlineData(5, "division by zero.")]
    [InlineData(6, "invalid self pointer.")]
    [InlineData(7, "string format failed.")]
    [InlineData(65535, "Unknown reason 65535")]
    public void AbortMessagesMatchNativeReasonCodes(int reason, string suffix)
    {
        var result = VmExec.Run([VmCode.Rrr(VmOp.Throw, 0, reason & 255, reason >> 8)], []);
        Assert.Equal("VM execution aborted: " + suffix, result.AbortMessage);
        var multi = VmExec.RunWithReturns([VmCode.Rrr(VmOp.Throw, 0, reason & 255, reason >> 8)], [], [], [], 0);
        Assert.Equal(result.AbortMessage, multi.AbortMessage);
    }

    [Fact]
    public void AbortStackTextMatchesNativeOrderAndLineFormatting()
    {
        var frames = new[] { new VmAbortFrame("Leaf", "leaf.zs", 1, 20), new VmAbortFrame("Caller", "caller.zs", 2, -1) };
        const string expected = "Called from Leaf at leaf.zs, line 20\nCalled from Caller at caller.zs, line -1\n";
        Assert.Equal(expected, VmAbortDiagnostics.FormatStackTrace(frames));
        Assert.Equal(expected, new VmResult(false, 0, VmFault.ScriptAbort) { AbortFrames = frames }.AbortStackTrace);
        Assert.Equal(expected, new VmMultiResult(false, VmFault.ScriptAbort, []) { AbortFrames = frames }.AbortStackTrace);
        Assert.Equal("", VmAbortDiagnostics.FormatStackTrace([]));
        Assert.Throws<ArgumentNullException>(() => VmAbortDiagnostics.FormatStackTrace(null!));
        Assert.Null(VmExec.Run([VmCode.Reti(42)], []).AbortMessage);
        Assert.Null(VmExec.Run([255], []).AbortMessage);
        Assert.Null(VmExec.Run([VmCode.Reti(42)], []).AbortStackTrace);
    }

    [Fact]
    public void ScriptAbortsCollectSourceFramesFromLeafToCaller()
    {
        var leaf = new VmScriptFunction([VmCode.Li(0, 1), VmCode.Rrr(VmOp.Throw, 0, 2, 0)], [], [],
            lineInfo: [new(0, 10), new(1, 20), new(2, 30)], printableName: "Leaf", sourceFileName: "leaf.zs");
        var caller = new VmScriptFunction([VmCode.Li(0, 7), VmCode.Rrr(VmOp.CallK, 0, 0, 0), VmCode.Reti(0)], [], [],
            objectConstants: [leaf], lineInfo: [new(0, 40), new(1, 50), new(3, 60)], printableName: "Caller", sourceFileName: "caller.zs");
        var result = VmExec.RunFunction(caller, []);
        Assert.False(result.Returned);
        Assert.Equal(VmFault.ScriptAbort, result.Fault);
        Assert.Equal((ushort)2, result.AbortCode);
        Assert.Empty(result.Values);
        Assert.Equal(new[] { new VmAbortFrame("Leaf", "leaf.zs", 1, 20), new VmAbortFrame("Caller", "caller.zs", 1, 50) }, result.AbortFrames);
        var raw = VmExec.RunWithReturns([VmCode.Rrr(VmOp.CallK, 0, 0, 0)], [], [], [], 0, objectConstants: [caller]);
        Assert.Equal(result.AbortFrames, raw.AbortFrames);
        var single = VmExec.Run([VmCode.Rrr(VmOp.CallK, 0, 0, 0)], [], [], [], objectConstants: [caller]);
        Assert.Equal(result.AbortFrames, single.AbortFrames);
        Assert.IsAssignableFrom<IList<VmAbortFrame>>(result.AbortFrames);
        Assert.Throws<NotSupportedException>(() => ((IList<VmAbortFrame>)result.AbortFrames!)[0] = default);
    }

    [Fact]
    public void ScriptAbortFramesHandleMissingMetadataAndDoNotLeakBetweenExecutions()
    {
        var function = new VmScriptFunction([VmCode.Rrr(VmOp.Throw, 0, 1, 0)], [], []);
        var first = VmExec.RunFunction(function, []);
        var second = VmExec.RunFunction(function, []);
        Assert.Single(first.AbortFrames!);
        Assert.Equal(new VmAbortFrame("", "", 0, -1), first.AbortFrames![0]);
        Assert.Equal(first.AbortFrames, second.AbortFrames);
        Assert.Null(VmExec.RunFunction(new VmScriptFunction([VmCode.Reti(42)], [], []), []).AbortFrames);
        Assert.Null(VmExec.RunFunction(new VmScriptFunction([255], [], []), []).AbortFrames);
        Assert.Throws<ArgumentNullException>(() => new VmScriptFunction([], [], [], printableName: null!));
        Assert.Throws<ArgumentNullException>(() => new VmScriptFunction([], [], [], sourceFileName: null!));
    }

    [Fact]
    public void ScriptSourceLinesMatchNativeBoundaryLookup()
    {
        var function = new VmScriptFunction([], [], [], lineInfo: [new(0, 10), new(3, 20), new(3, 30), new(8, 40)]);
        Assert.Equal(10, function.GetSourceLine(-1));
        Assert.Equal(10, function.GetSourceLine(2));
        Assert.Equal(30, function.GetSourceLine(3));
        Assert.Equal(30, function.GetSourceLine(7));
        Assert.Equal(-1, function.GetSourceLine(8));
        Assert.Equal(-1, function.GetSourceLine(100));
    }

    [Fact]
    public void ScriptSourceLinesCopyMetadataAndHandleEmptyAndSingleTables()
    {
        Assert.Equal(-1, new VmScriptFunction([], [], []).GetSourceLine(0));
        var lines = new[] { new VmStatementInfo(5, 42) };
        var function = new VmScriptFunction([], [], [], lineInfo: lines);
        lines[0] = new(0, 99);
        Assert.Equal(42, function.GetSourceLine(-1));
        Assert.Equal(42, function.GetSourceLine(100));
        Assert.Throws<ArgumentException>(() => new VmScriptFunction([], [], [], lineInfo: [new(-1, 10)]));
        Assert.Throws<ArgumentException>(() => new VmScriptFunction([], [], [], lineInfo: [new(3, 10), new(2, 20)]));
    }

    [Theory]
    [InlineData(VmValueType.Vector2, 2)]
    [InlineData(VmValueType.Vector3, 3)]
    [InlineData(VmValueType.Vector4, 4)]
    public void ScriptVectorSignaturesExpandArgumentsAndDefaults(VmValueType type, int components)
    {
        var vector = components == 2 ? VmValue.FromVector2(-0.0, 2)
            : components == 3 ? VmValue.FromVector3(-0.0, 2, 3) : VmValue.FromVector4(-0.0, 2, 3, 4);
        var function = new VmScriptFunction([VmCode.RetVector(0, components)], [], [type], trailingDefaults: [vector]);
        var direct = VmExec.RunFunction(function, [vector]);
        var defaulted = VmExec.RunFunction(function, []);
        Assert.True(direct.Returned); Assert.Equal(vector.Vector, direct.Values[0].VectorValue);
        Assert.Equal(direct.Values[0], defaulted.Values[0]);
        var code = new List<int>();
        var values = new[] { vector.Vector.X, vector.Vector.Y, vector.Vector.Z, vector.Vector.W };
        for (var i = 0; i < components; i++) code.Add(VmCode.Rrr(VmOp.Lkf, i, i, 0));
        code.Add(VmCode.Rrr(VmOp.Param, (int)type, 0, 0));
        code.Add(VmCode.Rrr(VmOp.CallK, 0, components, 1));
        code.Add(VmCode.Rrr(VmOp.Result, 0, (int)type, 0)); code.Add(VmCode.RetVector(0, components));
        Assert.Equal(direct.Values[0], VmExec.Run(code.ToArray(), [], values, [], objectConstants: [function]));
    }

    [Fact]
    public void ScriptVectorSignaturesValidateLogicalWidthsAndFlattenedCapacity()
    {
        var function = new VmScriptFunction([VmCode.RetVector(0, 3)], [], [VmValueType.Vector3]);
        Assert.Equal(VmFault.InvalidCall, VmExec.RunFunction(function, [VmValue.FromVector2(1, 2)]).Fault);
        Assert.Equal(VmFault.ParameterRange, VmExec.RunFunction(function, [VmValue.FromFloat(1), VmValue.FromFloat(2), VmValue.FromFloat(3)]).Fault);
        Assert.Throws<ArgumentException>(() => new VmScriptFunction([], [], Enumerable.Repeat(VmValueType.Vector4, 65).ToArray()));
        Assert.Throws<ArgumentException>(() => new VmScriptFunction([], [], [VmValueType.Vector3], trailingDefaults: [VmValue.FromVector2(1, 2)]));
    }

    [Fact]
    public void FunctionEntryFillsOnlyOmittedTrailingDefaults()
    {
        var defaults = new[] { VmValue.FromInteger(7), VmValue.FromString("default") };
        var function = new VmScriptFunction([VmCode.Rrr(VmOp.AddRr, 0, 0, 1), VmCode.Rrr(VmOp.Ret, 0, 0, 0),
            VmCode.Rrr(VmOp.Ret, VmCode.RetFinal | 1, 2, 0)], [], [VmValueType.Integer, VmValueType.Integer, VmValueType.String], trailingDefaults: defaults);
        defaults[0] = VmValue.FromInteger(99);
        var result = VmExec.RunFunction(function, [VmValue.FromInteger(35)], 2);
        Assert.True(result.Returned); Assert.Equal(42, result.Values[0].Value); Assert.Equal("default", result.Values[1].StringValue);
        result = VmExec.RunFunction(function, [VmValue.FromInteger(35), VmValue.FromInteger(8), VmValue.FromString("explicit")], 2);
        Assert.Equal(43, result.Values[0].Value); Assert.Equal("explicit", result.Values[1].StringValue);
    }

    [Fact]
    public void DefaultsDoNotChangeBytecodeCallArgumentRequirements()
    {
        var function = new VmScriptFunction([VmCode.RetInt(0)], [], [VmValueType.Integer], trailingDefaults: [VmValue.FromInteger(42)]);
        Assert.Equal(42, VmExec.RunFunction(function, []).Values[0].Value);
        Assert.Equal(VmFault.ParameterRange, VmExec.Run([VmCode.Rrr(VmOp.CallK, 0, 0, 1), VmCode.Rrr(VmOp.Result, 0, 0, 0)],
            [], [], [], objectConstants: [function]).Fault);
    }

    [Fact]
    public void FunctionEntryValidatesArgumentCountsTypesAndDefaultDefinitions()
    {
        var function = new VmScriptFunction([VmCode.RetInt(0)], [], [VmValueType.Integer]);
        Assert.Equal(VmFault.ParameterRange, VmExec.RunFunction(function, []).Fault);
        Assert.Equal(VmFault.ParameterRange, VmExec.RunFunction(function, [VmValue.FromInteger(1), VmValue.FromInteger(2)]).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.RunFunction(function, [VmValue.FromString("wrong")]).Fault);
        Assert.Throws<ArgumentException>(() => new VmScriptFunction([], [], [], trailingDefaults: [VmValue.FromInteger(1)]));
        Assert.Throws<ArgumentException>(() => new VmScriptFunction([], [], [VmValueType.Integer], trailingDefaults: [VmValue.FromString("wrong")]));
    }

    [Fact]
    public void VirtualMethodsCanDispatchToScriptBytecode()
    {
        var child = new VmScriptFunction([VmCode.Reti(42)], [], []);
        var metadata = new VmClassMetadata(new Dictionary<Type, object?>(),
            new Dictionary<Type, IReadOnlyList<object?>> { [typeof(CastBase)] = new object?[] { child } });
        Assert.Equal(new VmResult(true, 42, VmFault.None), VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0),
            VmCode.Rrr(VmOp.Virtual, 1, 1, 0), VmCode.Rrr(VmOp.Call, 1, 0, 1),
            VmCode.Rrr(VmOp.Result, 0, 0, 0), VmCode.RetInt(0)], [], [], [], objectConstants: [new CastBase()], classMetadata: metadata));
    }

    [Fact]
    public void ScriptCallsInitializeIndependentTypedRegisterBanks()
    {
        var reference = new object();
        var child = new VmScriptFunction([VmCode.Rrr(VmOp.AddRk, 0, 0, 0), VmCode.Rrr(VmOp.Ret, 0, 0, 0),
            VmCode.Rrr(VmOp.Ret, 1, 1, 0), VmCode.Rrr(VmOp.Ret, 2, 2, 0), VmCode.Rrr(VmOp.Ret, VmCode.RetFinal | 3, 3, 0)],
            [2], [VmValueType.Integer, VmValueType.Float, VmValueType.String, VmValueType.Object]);
        var code = new List<int> { VmCode.Li(0, 40), VmCode.Rrr(VmOp.Lkf, 0, 0, 0), VmCode.Rrr(VmOp.Lks, 0, 0, 0), VmCode.Rrr(VmOp.Lkp, 0, 1, 0) };
        for (var type = 0; type < 4; type++) code.Add(VmCode.Rrr(VmOp.Param, type, 0, 0));
        code.Add(VmCode.Rrr(VmOp.CallK, 0, 4, 4));
        for (var type = 0; type < 4; type++) code.Add(VmCode.Rrr(VmOp.Result, 0, type, 1));
        for (var type = 0; type < 4; type++) code.Add(VmCode.Rrr(VmOp.Ret, type | (type == 3 ? VmCode.RetFinal : 0), type, 1));
        var result = VmExec.RunWithReturns(code.ToArray(), [], [-0.0], ["script"], 4, objectConstants: [child, reference]);
        Assert.True(result.Returned); Assert.Equal(42, result.Values[0].Value);
        Assert.Equal("script", result.Values[2].StringValue); Assert.Same(reference, result.Values[3].ObjectValue);
    }

    [Fact]
    public void NestedScriptCallsUsePrivateFramesAndCopiedBytecode()
    {
        var code = new[] { VmCode.Rrr(VmOp.LoadFramePointer, 1, 0, 0), VmCode.Li(0, 42),
            VmCode.Rrr(VmOp.StoreWord, 1, 0, 0), VmCode.Rrr(VmOp.LoadWord, 0, 1, 0), VmCode.RetInt(0) };
        var leaf = new VmScriptFunction(code, [0], [], frameExtraSpace: 4);
        code[1] = VmCode.Li(0, 7);
        var middle = new VmScriptFunction([VmCode.Rrr(VmOp.CallK, 0, 0, 1), VmCode.Rrr(VmOp.Result, 0, 0, 0), VmCode.RetInt(0)],
            [], [], objectConstants: [leaf]);
        Assert.Equal(new VmResult(true, 42, VmFault.None), VmExec.Run([VmCode.Li(0, 99), VmCode.Rrr(VmOp.CallK, 0, 0, 1),
            VmCode.Rrr(VmOp.Result, 0, 0, 1), VmCode.RetInt(1)], [], [], [], objectConstants: [middle]));
    }

    [Fact]
    public void ScriptCallsPropagateAbortAndValidateArguments()
    {
        var child = new VmScriptFunction([VmCode.Rrr(VmOp.Throw, 0, 2, 0)], [], [VmValueType.Integer]);
        var code = new[] { VmCode.Rrr(VmOp.Param, 0, 0, 0), VmCode.Rrr(VmOp.CallK, 0, 1, 0), VmCode.Reti(7) };
        var aborted = VmExec.Run(code, [], [], [], objectConstants: [child]);
        Assert.Equal(VmFault.ScriptAbort, aborted.Fault);
        Assert.Equal((ushort)2, aborted.AbortCode);
        Assert.Equal(new VmAbortFrame("", "", 0, -1), Assert.Single(aborted.AbortFrames!));
        Assert.Equal(VmFault.ParameterRange, VmExec.Run([VmCode.Rrr(VmOp.CallK, 0, 0, 0)], [], [], [], objectConstants: [child]).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.Run([VmCode.Rrr(VmOp.Param, 1, 0, 0), VmCode.Rrr(VmOp.CallK, 0, 1, 0)], [], [], [], objectConstants: [child]).Fault);
    }

    [Fact]
    public void NestedScriptsShareExecutionBudget()
    {
        var leaf = new VmScriptFunction(Enumerable.Repeat(VmCode.Rrr(VmOp.Nop, 0, 0, 0), 140).Append(VmCode.Reti(1)).ToArray(), [], []);
        var code = new List<int>();
        for (var i = 0; i < 2; i++) { code.Add(VmCode.Rrr(VmOp.CallK, 0, 0, 1)); code.Add(VmCode.Rrr(VmOp.Result, 0, 0, 0)); }
        code.Add(VmCode.RetInt(0));
        Assert.Equal(VmFault.Budget, VmExec.Run(code.ToArray(), [], [], [], objectConstants: [leaf]).Fault);
    }

    [Fact]
    public void SparseVirtualOverridesReuseSlotsAndPropagateToDescendants()
    {
        var inherited = new VmIntegerFunction((args, count) => new[] { 7 });
        var replacement = new VmIntegerFunction((args, count) => new[] { 42 });
        var appended = new VmIntegerFunction((args, count) => new[] { 99 });
        var slots = new Dictionary<int, object?> { [2] = appended, [0] = replacement };
        var metadata = new VmClassMetadata(new Dictionary<Type, object?> { [typeof(CastGrandchild)] = null },
            new Dictionary<Type, IReadOnlyList<object?>> { [typeof(CastBase)] = new object?[] { inherited, inherited } },
            virtualOverrides: new Dictionary<Type, IReadOnlyDictionary<int, object?>> { [typeof(CastDerivedForInheritance)] = slots });
        slots[0] = null;
        for (var slot = 0; slot < 3; slot++)
        {
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Virtual, 1, 1, slot),
                VmCode.Rrr(VmOp.Call, 1, 0, 1), VmCode.Rrr(VmOp.Result, 0, 0, 0), VmCode.RetInt(0)],
                [], [], [], objectConstants: [new CastGrandchild()], classMetadata: metadata);
            Assert.Equal(new VmResult(true, new[] { 42, 7, 99 }[slot], VmFault.None), result);
        }
    }

    [Fact]
    public void SparseVirtualOverridesDoNotMutateParentTables()
    {
        var original = new VmIntegerFunction((args, count) => Array.Empty<int>());
        var metadata = new VmClassMetadata(new Dictionary<Type, object?>(),
            new Dictionary<Type, IReadOnlyList<object?>> { [typeof(CastBase)] = new object?[] { original } },
            virtualOverrides: new Dictionary<Type, IReadOnlyDictionary<int, object?>> { [typeof(CastDerived)] = new Dictionary<int, object?> { [0] = null } });
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Virtual, 1, 1, 0), VmCode.RetObject(1) };
        Assert.Same(original, VmExec.Run(code, [], [], [], objectConstants: [new CastBase()], classMetadata: metadata).ObjectValue);
        Assert.Null(VmExec.Run(code, [], [], [], objectConstants: [new CastDerived()], classMetadata: metadata).ObjectValue);
    }

    [Fact]
    public void SparseVirtualOverridesRejectGapsInvalidSlotsAndUnsupportedMethods()
    {
        foreach (var slot in new[] { -1, 1, 256 })
            Assert.Throws<ArgumentException>(() => new VmClassMetadata(new Dictionary<Type, object?>(), virtualOverrides:
                new Dictionary<Type, IReadOnlyDictionary<int, object?>> { [typeof(CastBase)] = new Dictionary<int, object?> { [slot] = null } }));
        Assert.Throws<ArgumentException>(() => new VmClassMetadata(new Dictionary<Type, object?>(), virtualOverrides:
            new Dictionary<Type, IReadOnlyDictionary<int, object?>> { [typeof(CastBase)] = new Dictionary<int, object?> { [0] = new object() } }));
    }

    private sealed class CastGrandchild : CastDerivedForInheritance { }
    private class CastDerivedForInheritance : CastBase { }

    [Fact]
    public void RegisteredDerivedClassesInheritNearestVirtualTableRegardlessOfRegistrationOrder()
    {
        var baseMethod = new VmIntegerFunction((args, count) => new[] { 7 });
        var overrideMethod = new VmIntegerFunction((args, count) => new[] { 42 });
        var methods = new object?[] { overrideMethod };
        var metadata = new VmClassMetadata(new Dictionary<Type, object?> { [typeof(CastGrandchild)] = null, [typeof(CastDerived)] = null },
            new Dictionary<Type, IReadOnlyList<object?>> { [typeof(CastBase)] = new object?[] { baseMethod }, [typeof(CastDerivedForInheritance)] = methods });
        methods[0] = baseMethod;
        foreach (var source in new object[] { new CastGrandchild(), new CastDerived() })
        {
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Virtual, 1, 1, 0),
                VmCode.Rrr(VmOp.Call, 1, 0, 1), VmCode.Rrr(VmOp.Result, 0, 0, 0), VmCode.RetInt(0)],
                [], [], [], objectConstants: [source], classMetadata: metadata);
            Assert.Equal(new VmResult(true, source is CastGrandchild ? 42 : 7, VmFault.None), result);
        }
    }

    [Fact]
    public void ScopeRegisteredClassesInheritTablesButExplicitEmptyTablesRemainEmpty()
    {
        var method = new VmIntegerFunction((args, count) => new[] { 7 });
        var metadata = new VmClassMetadata(new Dictionary<Type, object?>(),
            new Dictionary<Type, IReadOnlyList<object?>> { [typeof(CastBase)] = new object?[] { method }, [typeof(CastDerived)] = Array.Empty<object?>() },
            new Dictionary<Type, VmClassScope> { [typeof(CastDerivedForInheritance)] = VmClassScope.Play });
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Virtual, 1, 1, 0), VmCode.RetObject(1) };
        Assert.Same(method, VmExec.Run(code, [], [], [], objectConstants: [new CastDerivedForInheritance()], classMetadata: metadata).ObjectValue);
        Assert.Equal(VmFault.InvalidCall, VmExec.Run(code, [], [], [], objectConstants: [new CastDerived()], classMetadata: metadata).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.Run(code, [], [], [], objectConstants: [new CastGrandchild()], classMetadata: metadata).Fault);
    }

    [Fact]
    public void StringArgumentsFollowSourceRegisterChangesBeforeAndDuringCalls()
    {
        var function = new VmFunction((args, count) =>
        {
            Assert.Equal("new", args[0].String);
            Assert.Equal("constant", args[1].String);
            var reference = Assert.IsType<VmRegisterReference>(args[2].Object);
            reference.Value = VmValue.FromString("changed\0inside");
            Assert.Equal("changed\0inside", args[0].String);
            Assert.Equal("constant", args[1].String);
            return new[] { VmValue.FromString(args[0].String!) };
        });
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lks, 255, 0, 0), VmCode.Rrr(VmOp.Param, 2, 255, 0),
            VmCode.Rrr(VmOp.Param, 6, 2, 0), VmCode.Rrr(VmOp.Param, 34, 255, 0),
            VmCode.Rrr(VmOp.Lks, 255, 1, 0), VmCode.Rrr(VmOp.CallK, 0, 3, 1),
            VmCode.Rrr(VmOp.Result, 0, 2, 0), VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 0)],
            [], [], ["old", "new", "constant"], objectConstants: [function]);
        Assert.True(result.Returned); Assert.Equal("changed\0inside", result.StringValue);
    }

    [Fact]
    public void StringArgumentViewsKeepDistinctRegisterIndices()
    {
        var function = new VmFunction((args, count) =>
        {
            Assert.Equal("first", args[0].String); Assert.Equal("second", args[1].String);
            return Array.Empty<VmValue>();
        });
        Assert.Equal(new VmResult(true, 7, VmFault.None), VmExec.Run([VmCode.Rrr(VmOp.Param, 2, 0, 0),
            VmCode.Rrr(VmOp.Param, 2, 1, 0), VmCode.Rrr(VmOp.Lks, 0, 0, 0), VmCode.Rrr(VmOp.Lks, 1, 1, 0),
            VmCode.Rrr(VmOp.CallK, 0, 2, 0), VmCode.Reti(7)], [], [], ["first", "second"], objectConstants: [function]));
    }

    [Theory]
    [InlineData(VmClassScope.PlainData)]
    [InlineData(VmClassScope.UI)]
    [InlineData(VmClassScope.Play)]
    public void ScopeChecksUseNativeContextEncodingAndRuntimeClass(VmClassScope inner)
    {
        var function = new VmIntegerFunction((args, count) => new[] { 42 });
        var metadata = new VmClassMetadata(new Dictionary<Type, object?>(), scopes:
            new Dictionary<Type, VmClassScope> { [typeof(CastBase)] = VmClassScope.PlainData, [typeof(CastDerived)] = inner });
        foreach (var context in new[] { 0, 1, 2, 3, 255 })
        {
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 255, 0, 0), VmCode.Rrr(VmOp.Scope, 255, context, 1),
                VmCode.Rrr(VmOp.CallK, 1, 0, 1), VmCode.Rrr(VmOp.Result, 0, 0, 0), VmCode.RetInt(0)],
                [], [], [], objectConstants: [new CastDerived(), function], classMetadata: metadata);
            if (inner == VmClassScope.PlainData || context - 1 == (int)inner) Assert.Equal(new VmResult(true, 42, VmFault.None), result);
            else
            {
                Assert.Equal(VmFault.ScriptAbort, result.Fault);
                Assert.Equal((ushort)0, result.AbortCode);
                Assert.NotNull(result.AbortContext);
            }
        }
    }

    [Fact]
    public void ScopeChecksRejectMissingMetadataInvalidFunctionsAndNullReceivers()
    {
        var function = new VmIntegerFunction((args, count) => Array.Empty<int>());
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 0, 0, 0), VmCode.Rrr(VmOp.Scope, 0, 1, 1) };
        Assert.Equal(VmFault.InvalidClass, VmExec.Run(code, [], [], [], objectConstants: [new CastBase(), function]).Fault);
        Assert.Equal(new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 }, VmExec.Run(code, [], [], [], objectConstants: [null, function]));
        Assert.Equal(VmFault.InvalidCall, VmExec.Run(code, [], [], [], objectConstants: [new CastBase(), new object()]).Fault);
        Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(VmOp.Scope, 0, 0, 255)], []).Fault);
        Assert.Throws<ArgumentException>(() => new VmClassMetadata(new Dictionary<Type, object?>(), scopes:
            new Dictionary<Type, VmClassScope> { [typeof(int)] = VmClassScope.UI }));
        Assert.Throws<ArgumentException>(() => new VmClassMetadata(new Dictionary<Type, object?>(), scopes:
            new Dictionary<Type, VmClassScope> { [typeof(CastBase)] = (VmClassScope)3 }));
    }

    [Fact]
    public void VirtualLookupUsesRuntimeClassAndFeedsCallbackCall()
    {
        var source = new CastDerived();
        var derivedMethod = new VmFunction((args, count) => { Assert.Same(source, args[0].Object); return new[] { VmValue.FromInteger(42) }; });
        var methods = new object?[] { null, derivedMethod };
        var metadata = new VmClassMetadata(new Dictionary<Type, object?>(), new Dictionary<Type, IReadOnlyList<object?>>
        {
            [typeof(CastBase)] = new object?[] { null, new VmIntegerFunction((args, count) => new[] { 7 }) },
            [typeof(CastDerived)] = methods,
        });
        methods[1] = null;
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 255, 0, 0), VmCode.Rrr(VmOp.Param, 3, 255, 0),
            VmCode.Rrr(VmOp.Virtual, 255, 255, 1), VmCode.Rrr(VmOp.Call, 255, 1, 1),
            VmCode.Rrr(VmOp.Result, 0, 0, 255), VmCode.RetInt(255)], [], [], [], objectConstants: [source], classMetadata: metadata);
        Assert.Equal(new VmResult(true, 42, VmFault.None), result);
    }

    [Fact]
    public void VirtualLookupPreservesNullSlotsAndRejectsMissingTablesOrSlots()
    {
        var metadata = new VmClassMetadata(new Dictionary<Type, object?>(),
            new Dictionary<Type, IReadOnlyList<object?>> { [typeof(CastBase)] = new object?[] { null } });
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Virtual, 1, 1, 0), VmCode.RetObject(1) };
        var result = VmExec.Run(code, [], [], [], objectConstants: [new CastBase()], classMetadata: metadata);
        Assert.True(result.HasObjectValue); Assert.Null(result.ObjectValue);
        Assert.Equal(VmFault.InvalidCall, VmExec.Run(code, [], [], [], objectConstants: [new CastDerived()], classMetadata: metadata).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Virtual, 1, 1, 255)],
            [], [], [], objectConstants: [new CastBase()], classMetadata: metadata).Fault);
        Assert.Equal(new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 }, VmExec.Run(code, [], [], [], objectConstants: [null], classMetadata: metadata));
    }

    [Fact]
    public void VirtualTableRegistrationRejectsUnsupportedMethods()
    {
        Assert.Throws<ArgumentException>(() => new VmClassMetadata(new Dictionary<Type, object?>(),
            new Dictionary<Type, IReadOnlyList<object?>> { [typeof(int)] = Array.Empty<object?>() }));
        Assert.Throws<ArgumentException>(() => new VmClassMetadata(new Dictionary<Type, object?>(),
            new Dictionary<Type, IReadOnlyList<object?>> { [typeof(CastBase)] = new object?[] { new object() } }));
        Assert.Throws<ArgumentNullException>(() => new VmClassMetadata(new Dictionary<Type, object?>(),
            new Dictionary<Type, IReadOnlyList<object?>> { [typeof(CastBase)] = null! }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ScalarReferenceParametersReadLiveRegistersAndWriteBack(int type)
    {
        var referenceObject = new object();
        var replacement = type switch
        {
            0 => VmValue.FromInteger(42), 1 => VmValue.FromFloat(-0.0),
            2 => VmValue.FromString("new\0value"), _ => VmValue.FromObject(referenceObject),
        };
        var function = new VmFunction((args, count) =>
        {
            var first = Assert.IsType<VmRegisterReference>(args[0].Object);
            var second = Assert.IsType<VmRegisterReference>(args[1].Object);
            Assert.Equal((VmValueType)type, first.Type);
            if (type == 0) Assert.Equal(7, first.Value.Integer);
            first.Value = replacement;
            Assert.Equal(replacement.Type, second.Value.Type);
            if (type == 0) Assert.Equal(42, second.Value.Integer);
            return Array.Empty<VmValue>();
        });
        var result = VmExec.Run([VmCode.Rrr(VmOp.Param, 32 + type, 255, 0), VmCode.Rrr(VmOp.Param, 32 + type, 255, 0),
            VmCode.Li(255, 7), VmCode.Rrr(VmOp.CallK, 0, 2, 0), VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, type, 255)],
            [], [], [], objectConstants: [function]);
        Assert.True(result.Returned);
        switch (type)
        {
            case 0: Assert.Equal(42, result.Value); break;
            case 1: Assert.Equal(BitConverter.DoubleToInt64Bits(-0.0), BitConverter.DoubleToInt64Bits(result.FloatValue!.Value)); break;
            case 2: Assert.Equal("new\0value", result.StringValue); break;
            case 3: Assert.Same(referenceObject, result.ObjectValue); break;
        }
    }

    [Fact]
    public void ScalarReferenceParametersRejectWrongAssignmentsWithoutMutation()
    {
        var function = new VmFunction((args, count) =>
        {
            var reference = Assert.IsType<VmRegisterReference>(args[0].Object);
            Assert.Throws<ArgumentException>(() => reference.Value = VmValue.FromString("wrong"));
            Assert.Throws<ArgumentNullException>(() => reference.Value = null!);
            Assert.Equal(7, reference.Value.Integer);
            return Array.Empty<VmValue>();
        });
        Assert.Equal(new VmResult(true, 7, VmFault.None), VmExec.Run([VmCode.Li(0, 7), VmCode.Rrr(VmOp.Param, 32, 0, 0),
            VmCode.Rrr(VmOp.CallK, 0, 1, 0), VmCode.RetInt(0)], [], [], [], objectConstants: [function]));
        foreach (var type in new[] { 32, 33, 34, 35 })
            Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(VmOp.Param, type, 0, 1)], []).Fault);
    }

    [Theory]
    [InlineData(9, 2)]
    [InlineData(17, 3)]
    [InlineData(65, 4)]
    public void VectorCallbackResultsPopulateFullDestinationSpan(int selector, int components)
    {
        var vector = components == 2 ? VmValue.FromVector2(-0.0, 1.25)
            : components == 3 ? VmValue.FromVector3(-0.0, 1.25, double.PositiveInfinity)
            : VmValue.FromVector4(-0.0, 1.25, double.PositiveInfinity, double.NaN);
        var function = new VmFunction((args, count) => { Assert.Empty(args); Assert.Equal(2, count); return new[] { vector, VmValue.FromInteger(42) }; });
        var start = 256 - components;
        var result = VmExec.RunWithReturns([VmCode.Rrr(VmOp.CallK, 0, 0, 2), VmCode.Rrr(VmOp.Result, 0, selector, start),
            VmCode.Rrr(VmOp.Result, 0, 0, start), VmCode.Rrr(VmOp.Ret, 0, selector, start),
            VmCode.Rrr(VmOp.Ret, VmCode.RetFinal | 1, 0, start)], [], [], [], 2, objectConstants: [function]);
        Assert.True(result.Returned); Assert.Equal(vector.Vector, result.Values[0].VectorValue);
        Assert.Equal(42, result.Values[1].Value);
        Assert.Equal(BitConverter.DoubleToInt64Bits(-0.0), BitConverter.DoubleToInt64Bits(result.Values[0].VectorValue!.Value.X));
    }

    [Theory]
    [InlineData(9, 2)]
    [InlineData(17, 3)]
    [InlineData(65, 4)]
    public void VectorResultSpansAreValidatedBeforeCallbackInvocation(int selector, int components)
    {
        var called = false;
        var function = new VmFunction((args, count) => { called = true; return Array.Empty<VmValue>(); });
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(VmOp.CallK, 0, 0, 1),
            VmCode.Rrr(VmOp.Result, 0, selector, 257 - components)], [], [], [], objectConstants: [function]).Fault);
        Assert.False(called);
    }

    [Fact]
    public void VectorResultsRespectDescriptorOrderForOverlappingDestinations()
    {
        var function = new VmFunction((args, count) => new[] { VmValue.FromVector3(1, 2, 3), VmValue.FromVector2(7, 8) });
        var result = VmExec.Run([VmCode.Rrr(VmOp.CallK, 0, 0, 2), VmCode.Rrr(VmOp.Result, 0, 17, 0),
            VmCode.Rrr(VmOp.Result, 0, 9, 1), VmCode.RetVector(0, 3)], [], [], [], objectConstants: [function]);
        Assert.Equal(new VmVectorResult(3, 1, 7, 8, 0), result.VectorValue);
    }

    [Fact]
    public void VectorResultsRejectWrongWidthAndUnsupportedDescriptorFlags()
    {
        var function = new VmFunction((args, count) => new[] { VmValue.FromVector2(1, 2) });
        foreach (var selector in new[] { 17, 13, 41, 128 })
            Assert.Equal(VmFault.InvalidCall, VmExec.Run([VmCode.Rrr(VmOp.CallK, 0, 0, 1),
                VmCode.Rrr(VmOp.Result, 0, selector, 0)], [], [], [], objectConstants: [function]).Fault);
    }

    [Theory]
    [InlineData(9, 2)]
    [InlineData(17, 3)]
    [InlineData(65, 4)]
    public void VectorParametersExpandInOrderAndCaptureAtPushTime(int selector, int components)
    {
        var values = new[] { -0.0, 1.25, double.PositiveInfinity, double.NaN };
        var function = new VmFunction((args, count) =>
        {
            Assert.Equal(components, args.Count); Assert.Equal(1, count);
            for (var i = 0; i < components; i++)
            {
                Assert.Equal(VmValueType.Float, args[i].Type);
                Assert.Equal(BitConverter.DoubleToInt64Bits(values[i]), BitConverter.DoubleToInt64Bits(args[i].Float));
            }
            return new[] { VmValue.FromInteger(42) };
        });
        var start = 256 - components;
        var code = new List<int>();
        for (var i = 0; i < components; i++) code.Add(VmCode.Rrr(VmOp.Lkf, start + i, i, 0));
        code.Add(VmCode.Rrr(VmOp.Param, selector, start, 0));
        code.Add(VmCode.Rrr(VmOp.Lkf, start, 1, 0));
        code.Add(VmCode.Rrr(VmOp.CallK, 0, components, 1));
        code.Add(VmCode.Rrr(VmOp.Result, 0, 0, 0)); code.Add(VmCode.RetInt(0));
        Assert.Equal(new VmResult(true, 42, VmFault.None), VmExec.Run(code.ToArray(), [], values, [], objectConstants: [function]));
    }

    [Theory]
    [InlineData(9, 2)]
    [InlineData(17, 3)]
    [InlineData(65, 4)]
    public void VectorParametersRejectInvalidRegisterSpansAndUnsupportedForms(int selector, int components)
    {
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(VmOp.Param, selector, 257 - components, 0)], []).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.Run([VmCode.Rrr(VmOp.Param, selector | 4, 0, 0)], []).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.Run([VmCode.Rrr(VmOp.Param, selector | 32, 0, 0)], []).Fault);
    }

    [Fact]
    public void NilParameterIgnoresIndexAndRemainsDistinctFromNullObject()
    {
        var function = new VmFunction((args, count) =>
        {
            Assert.Equal(VmValueType.Nil, args[0].Type);
            Assert.Equal(VmValueType.Object, args[1].Type); Assert.Null(args[1].Object);
            return Array.Empty<VmValue>();
        });
        Assert.Equal(new VmResult(true, 7, VmFault.None), VmExec.Run([VmCode.Rrr(VmOp.Param, 128, 255, 255),
            VmCode.Rrr(VmOp.Param, 3, 0, 0), VmCode.Rrr(VmOp.CallK, 0, 2, 0), VmCode.Reti(7)], [], [], [], objectConstants: [function]));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TypedCallsPreserveScalarBanksAndObjectIdentity(bool constants)
    {
        var reference = new object();
        var function = new VmFunction((arguments, count) =>
        {
            Assert.Equal(4, count); Assert.Equal(4, arguments.Count);
            Assert.Equal(-7, arguments[0].Integer); Assert.Equal(-0.0, arguments[1].Float);
            Assert.Equal("a\0b", arguments[2].String); Assert.Same(reference, arguments[3].Object);
            return arguments;
        });
        var code = new List<int> { VmCode.Li(2, -7), VmCode.Rrr(VmOp.Lkf, 2, 0, 0),
            VmCode.Rrr(VmOp.Lks, 2, 0, 0), VmCode.Rrr(VmOp.Lkp, 2, 1, 0) };
        for (var type = 0; type < 4; type++) code.Add(VmCode.Rrr(VmOp.Param, type + (constants ? 4 : 0), constants ? (type == 3 ? 1 : 0) : 2, 0));
        code.Add(VmCode.Rrr(VmOp.CallK, 0, 4, 4));
        for (var type = 0; type < 4; type++) code.Add(VmCode.Rrr(VmOp.Result, 255, type, 255));
        for (var type = 0; type < 4; type++) code.Add(VmCode.Rrr(VmOp.Ret, type | (type == 3 ? VmCode.RetFinal : 0), type, 255));
        var result = VmExec.RunWithReturns(code.ToArray(), [-7], [-0.0], ["a\0b"], 4, objectConstants: [function, reference]);
        Assert.True(result.Returned); Assert.Equal(-7, result.Values[0].Value);
        Assert.Equal(BitConverter.DoubleToInt64Bits(-0.0), BitConverter.DoubleToInt64Bits(result.Values[1].FloatValue!.Value));
        Assert.Equal("a\0b", result.Values[2].StringValue); Assert.Same(reference, result.Values[3].ObjectValue);
    }

    [Fact]
    public void TypedCallsPreserveNullObjectsAndRejectResultTypeMismatch()
    {
        var code = new[] { VmCode.Rrr(VmOp.Param, 3, 0, 0), VmCode.Rrr(VmOp.CallK, 0, 1, 1),
            VmCode.Rrr(VmOp.Result, 0, 3, 1), VmCode.RetObject(1) };
        var result = VmExec.Run(code, [], [], [], objectConstants: [new VmFunction((args, count) => args)]);
        Assert.True(result.HasObjectValue); Assert.Null(result.ObjectValue);
        Assert.Equal(VmFault.InvalidCall, VmExec.Run(code, [], [], [], objectConstants:
            [new VmFunction((args, count) => new[] { VmValue.FromFloat(1) })]).Fault);
    }

    [Fact]
    public void TypedParameterValidationPreventsInvalidCallbacks()
    {
        foreach (var selector in new[] { 5, 6, 7 })
            Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(VmOp.Param, selector, 255, 0)], []).Fault);
        var called = false;
        var integerFunction = new VmIntegerFunction((args, count) => { called = true; return Array.Empty<int>(); });
        Assert.Equal(VmFault.InvalidCall, VmExec.Run([VmCode.Rrr(VmOp.Param, 1, 0, 0), VmCode.Rrr(VmOp.CallK, 0, 1, 0)],
            [], [], [], objectConstants: [integerFunction]).Fault);
        Assert.False(called);
        Assert.Throws<ArgumentNullException>(() => VmValue.FromString(null!));
        Assert.Throws<ArgumentNullException>(() => new VmFunction(null!));
    }

    [Theory]
    [InlineData(VmOp.Call)]
    [InlineData(VmOp.CallK)]
    public void IntegerCallsConsumeTrailingParametersAndStoreMultipleResults(VmOp op)
    {
        var calls = 0;
        var function = new VmIntegerFunction((arguments, count) =>
        {
            calls++;
            if (calls == 1) { Assert.Equal(new[] { 7, -3 }, arguments); Assert.Equal(2, count); return new[] { 4, 10 }; }
            Assert.Equal(new[] { 99 }, arguments); Assert.Equal(1, count); return new[] { 99 };
        });
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), ((int)VmOp.ParamImmediate | (99 << 8)),
            VmCode.Li(2, 7), VmCode.Rrr(VmOp.Param, 0, 2, 0), VmCode.Rrr(VmOp.Param, 4, 0, 0),
            VmCode.Rrr(op, op == VmOp.Call ? 1 : 0, 2, 2), VmCode.Rrr(VmOp.Result, 255, 0, 2), VmCode.Rrr(VmOp.Result, 0, 0, 3),
            VmCode.Rrr(op, op == VmOp.Call ? 1 : 0, 1, 1), VmCode.Rrr(VmOp.Result, 0, 0, 4),
            VmCode.Rrr(VmOp.AddRr, 0, 2, 3), VmCode.Rrr(VmOp.AddRr, 0, 0, 4), VmCode.RetInt(0)],
            [-3], [], [], objectConstants: [function]);
        Assert.Equal(new VmResult(true, 113, VmFault.None), result); Assert.Equal(2, calls);
    }

    [Fact]
    public void IntegerCallsSignExtendImmediateAndAllowZeroResults()
    {
        var called = false;
        var function = new VmIntegerFunction((arguments, count) => { called = true; Assert.Equal(-8388608, arguments[0]); Assert.Equal(0, count); return Array.Empty<int>(); });
        Assert.Equal(new VmResult(true, 7, VmFault.None), VmExec.Run([
            unchecked((int)0x80000000) | (int)VmOp.ParamImmediate, VmCode.Rrr(VmOp.CallK, 0, 1, 0), VmCode.Reti(7)],
            [], [], [], objectConstants: [function]));
        Assert.True(called);
    }

    [Fact]
    public void IntegerCallsValidateDescriptorsBeforeInvokingCallback()
    {
        var called = false;
        var function = new VmIntegerFunction((arguments, count) => { called = true; return new[] { 1 }; });
        foreach (var descriptor in new[] { VmCode.Reti(1), VmCode.Rrr(VmOp.Result, 0, 1, 0) })
            Assert.Equal(VmFault.InvalidCall, VmExec.Run([VmCode.Rrr(VmOp.CallK, 0, 0, 1), descriptor],
                [], [], [], objectConstants: [function]).Fault);
        Assert.False(called);
        Assert.Equal(VmFault.ParameterRange, VmExec.Run([VmCode.Rrr(VmOp.CallK, 0, 1, 0)], [], [], [], objectConstants: [function]).Fault);
        Assert.Equal(VmFault.InvalidCall, VmExec.Run([VmCode.Rrr(VmOp.Result, 0, 0, 0)], []).Fault);
        Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(VmOp.CallK, 255, 0, 0)], []).Fault);
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(VmOp.Param, 0, 0, 1)], []).Fault);
        Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(VmOp.Param, 4, 0, 0)], []).Fault);
    }

    [Fact]
    public void IntegerCallsRejectResultCountMismatchAndPropagateCallbackErrors()
    {
        var code = new[] { VmCode.Rrr(VmOp.CallK, 0, 0, 1), VmCode.Rrr(VmOp.Result, 0, 0, 0) };
        Assert.Equal(VmFault.InvalidCall, VmExec.Run(code, [], [], [], objectConstants: [new VmIntegerFunction((args, count) => Array.Empty<int>())]).Fault);
        Assert.Throws<InvalidOperationException>(() => VmExec.Run(code, [], [], [], objectConstants:
            [new VmIntegerFunction((args, count) => throw new InvalidOperationException("callback failed"))]));
    }

    [Fact]
    public void MetadataUsesActualDerivedClassAndFeedsMemoryAccess()
    {
        var memory = new byte[] { 0, 42, 0, 0, 0 };
        var allocations = new Dictionary<Type, object?> { [typeof(CastBase)] = new byte[5], [typeof(CastDerived)] = memory };
        var metadata = new VmClassMetadata(allocations);
        allocations[typeof(CastDerived)] = null;
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 255, 0, 0), VmCode.Rrr(VmOp.Meta, 255, 255, 255),
            VmCode.Rrr(VmOp.AddPointerK, 255, 255, 0), VmCode.Rrr(VmOp.LoadWord, 0, 255, 1), VmCode.RetInt(0)],
            [1, 0], [], [], objectConstants: [new CastDerived()], classMetadata: metadata);
        Assert.Equal(new VmResult(true, 42, VmFault.None), result);
    }

    [Fact]
    public void MetadataPreservesSharedAllocationIdentityAndExplicitNull()
    {
        var memory = new VmStringMemory(new Dictionary<int, string> { [7] = "metadata" });
        var metadata = new VmClassMetadata(new Dictionary<Type, object?> { [typeof(CastDerived)] = memory, [typeof(CastBase)] = null });
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Meta, 1, 1, 0), VmCode.RetObject(1) };
        Assert.Same(memory, VmExec.Run(code, [], [], [], objectConstants: [new CastDerived()], classMetadata: metadata).ObjectValue);
        var result = VmExec.RunWithReturns(code, [], [], [], 1, objectConstants: [new CastBase()], classMetadata: metadata);
        Assert.True(result.Values[0].HasObjectValue); Assert.Null(result.Values[0].ObjectValue);
    }

    [Fact]
    public void MetadataRejectsMissingDescriptorsAndInvalidSources()
    {
        var metadata = new VmClassMetadata(new Dictionary<Type, object?> { [typeof(CastBase)] = new byte[1] });
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 0, 0, 0), VmCode.Rrr(VmOp.Meta, 0, 0, 0) };
        foreach (var source in new object[] { new CastDerived(), typeof(CastBase), 1 })
            Assert.Equal(VmFault.InvalidClass, VmExec.Run(code, [], [], [], objectConstants: [source], classMetadata: metadata).Fault);
        Assert.Equal(VmFault.InvalidClass, VmExec.Run(code, [], [], [], objectConstants: [new CastBase()]).Fault);
        Assert.Equal(new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 },
            VmExec.Run(code, [], [], [], objectConstants: [null], classMetadata: metadata));
    }

    [Fact]
    public void MetadataCatalogValidatesDescriptorsAndAllocationTypes()
    {
        Assert.Throws<ArgumentNullException>(() => new VmClassMetadata(null!));
        Assert.Throws<ArgumentException>(() => new VmClassMetadata(new Dictionary<Type, object?> { [typeof(int)] = null }));
        Assert.Throws<ArgumentException>(() => new VmClassMetadata(new Dictionary<Type, object?> { [typeof(CastBase)] = new object() }));
    }

    [Fact]
    public void FramePointerSharesOneAllocationAndFeedsMemoryArithmetic()
    {
        var result = VmExec.Run([VmCode.Rrr(VmOp.LoadFramePointer, 1, 255, 255), VmCode.Rrr(VmOp.LoadFramePointer, 2, 0, 0),
            VmCode.Li(3, 4), VmCode.Rrr(VmOp.AddPointerR, 1, 1, 3), VmCode.Li(4, 123),
            VmCode.Rrr(VmOp.StoreWord, 1, 4, 0), VmCode.Rrr(VmOp.LoadWord, 4, 2, 1), VmCode.RetInt(4)],
            [0, 4], [], [], frameExtraSpace: 8);
        Assert.Equal(new VmResult(true, 123, VmFault.None), result);
    }

    [Fact]
    public void FramePointerAllocationsAreIsolatedAcrossExecutions()
    {
        var code = new[] { VmCode.Rrr(VmOp.LoadFramePointer, 255, 0, 0), VmCode.RetObject(255) };
        var first = VmExec.Run(code, [], [], [], frameExtraSpace: 4);
        var second = VmExec.RunWithReturns(code, [], [], [], 1, frameExtraSpace: 4);
        var firstMemory = Assert.IsType<byte[]>(first.ObjectValue);
        var secondMemory = Assert.IsType<byte[]>(second.Values[0].ObjectValue);
        firstMemory[0] = 99;
        Assert.NotSame(firstMemory, secondMemory); Assert.Equal((byte)0, secondMemory[0]);
    }

    [Fact]
    public void FramePointerRejectsAbsentSpaceAndEnforcesMemoryBounds()
    {
        Assert.Equal(VmFault.InvalidFrame, VmExec.Run([VmCode.Rrr(VmOp.LoadFramePointer, 0, 0, 0)], []).Fault);
        Assert.Throws<ArgumentOutOfRangeException>(() => VmExec.Run([], [], [], [], frameExtraSpace: -1));
        Assert.Equal(VmFault.MemoryRange, VmExec.Run([VmCode.Rrr(VmOp.LoadFramePointer, 1, 0, 0),
            VmCode.Rrr(VmOp.StoreWord, 1, 0, 0)], [1], [], [], frameExtraSpace: 4).Fault);
    }

    [Fact]
    public void ManagedAddressesDereferenceAllNumericMemoryForms()
    {
        var stores = IntegerMemoryStores().Select(row => (VmOp)row[0])
            .Concat(FloatingMemoryStores().Select(row => (VmOp)row[0]))
            .Concat(VectorMemoryStores().Select(row => (VmOp)row[0])).ToArray();
        var loads = IntegerMemoryLoads().Select(row => (VmOp)row[0])
            .Concat(FloatingMemoryLoads().Select(row => (VmOp)row[0]))
            .Concat(VectorMemoryLoads().Select(row => (VmOp)row[0])).ToArray();
        foreach (var op in stores.Concat(loads))
        {
            var store = stores.Contains(op);
            var register = store ? ((int)op & 1) != 0 : ((int)op & 1) == 0;
            var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Li(3, -2), VmCode.Li(2, 123),
                VmCode.Rrr(VmOp.Lkf, 2, 0, 0), VmCode.Rrr(op, store ? 1 : 2, store ? 2 : 1, register ? 3 : 0),
                store ? VmCode.Reti(7) : (int)op <= (int)VmOp.LoadUnsignedHalfR ? VmCode.RetInt(2)
                    : (int)op >= (int)VmOp.LoadVector2 ? VmCode.RetVector(2,
                        2 + ((int)op - ((int)op >= (int)VmOp.LoadFloatVector2 ? (int)VmOp.LoadFloatVector2 : (int)VmOp.LoadVector2)) / 2)
                    : VmCode.RetFloat(2) };
            var direct = Enumerable.Repeat((byte)0x21, 64).ToArray();
            var indirect = direct.ToArray();
            var directCode = code.ToArray();
            directCode[1] = VmCode.Li(3, 5);
            var expected = VmExec.Run(directCode, [5], [1.25], [], objectConstants: [direct]);
            var actual = VmExec.Run(code, [-2], [1.25], [], objectConstants: [new VmMemoryPointer(indirect, 7)]);
            Assert.Equal(expected, actual); Assert.Equal(direct, indirect);
        }
    }

    [Fact]
    public void PointerAdditionFeedsIntegerAndBitMemoryOperations()
    {
        var memory = new byte[] { 0xaa, 0, 0xbb };
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Li(2, 1),
            VmCode.Rrr(VmOp.AddPointerR, 1, 1, 2), VmCode.Rrr(VmOp.StoreBit, 1, 2, 0x80),
            VmCode.Rrr(VmOp.LoadBit, 3, 1, 0x80), VmCode.Rrr(VmOp.StoreByte, 1, 3, 0),
            VmCode.Rrr(VmOp.LoadUnsignedByte, 3, 1, 0), VmCode.RetInt(3)], [0], [], [], objectConstants: [memory]);
        Assert.Equal(new VmResult(true, 1, VmFault.None), result);
        Assert.Equal(new byte[] { 0xaa, 1, 0xbb }, memory);
    }

    [Fact]
    public void ManagedAddressesDereferenceStringAndReferenceFields()
    {
        var value = new object();
        var references = new VmObjectMemory(new Dictionary<int, object?> { [7] = value, [19] = "head\0tail" });
        var strings = new VmStringMemory(new Dictionary<int, string> { [7] = "old" });
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.LoadPointer, 2, 1, 0),
            VmCode.Rrr(VmOp.StoreObject, 1, 2, 0), VmCode.Rrr(VmOp.LoadCString, 3, 1, 1),
            VmCode.Rrr(VmOp.Lkp, 1, 1, 0), VmCode.Rrr(VmOp.StoreString, 1, 3, 0),
            VmCode.Rrr(VmOp.LoadString, 3, 1, 0), VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 3)],
            [-2, 10], [], [], objectConstants: [new VmMemoryPointer(references, 9), new VmMemoryPointer(strings, 9)]);
        Assert.True(result.Returned); Assert.Equal("head", result.StringValue);
        Assert.Equal("head", strings[7]); Assert.Same(value, references[7]);
    }

    [Theory]
    [InlineData(long.MaxValue, 1)]
    [InlineData(long.MinValue, -1)]
    [InlineData(-1L, 0)]
    [InlineData(2147483648L, 0)]
    public void ManagedAddressDereferencesRejectOverflowAndInvalidRanges(long position, int offset)
    {
        var memory = new byte[] { 0xaa };
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.StoreByte, 1, 0, 0)],
            [offset], [], [], objectConstants: [new VmMemoryPointer(memory, position)]);
        Assert.Equal(VmFault.MemoryRange, result.Fault); Assert.Equal((byte)0xaa, memory[0]);
    }

    [Theory]
    [InlineData(VmOp.AddPointerR)]
    [InlineData(VmOp.AddPointerK)]
    public void PointerAdditionPreservesAllocationOffsetsAliasingAndNull(VmOp op)
    {
        var memory = new byte[8];
        foreach (var source in new object?[] { memory, new VmMemoryPointer(memory, 7), null })
        foreach (var delta in new[] { -9, 0, 17 })
        {
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Lk(3, 0),
                VmCode.Rrr(op, 1, 1, op == VmOp.AddPointerR ? 3 : 0), VmCode.RetObject(1)],
                [delta], [], [], objectConstants: [source]);
            Assert.True(result.Returned);
            if (source is null) Assert.Null(result.ObjectValue);
            else
            {
                var pointer = Assert.IsType<VmMemoryPointer>(result.ObjectValue);
                Assert.Same(memory, pointer.Memory);
                Assert.Equal((source is VmMemoryPointer ? 7L : 0L) + delta, pointer.ByteOffset);
            }
        }
    }

    [Fact]
    public void PointerSubtractionTruncatesByteDifferenceToNativeWord()
    {
        var memory = new byte[1];
        foreach (var difference in new[] { -17L, 0, 17, 0x100000001L })
        {
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Lkp, 2, 1, 0),
                VmCode.Rrr(VmOp.SubPointer, 1, 1, 2), VmCode.RetInt(1)], [], [], [],
                objectConstants: [new VmMemoryPointer(memory, difference), memory]);
            Assert.Equal(new VmResult(true, unchecked((int)difference), VmFault.None), result);
        }
    }

    [Fact]
    public void PointerEqualityComparesAllocationAndByteOffset()
    {
        var memory = new byte[1];
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Lkp, 2, 1, 0),
            VmCode.Cmp(VmOp.EqaR, 1, 1, 2), VmCode.Jmp(1), VmCode.Reti(0), VmCode.Reti(1)], [], [], [],
            objectConstants: [memory, new VmMemoryPointer(memory)]);
        Assert.Equal(new VmResult(true, 1, VmFault.None), result);
    }

    [Fact]
    public void PointerArithmeticRejectsUnrelatedStorageAndOverflow()
    {
        Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(VmOp.AddPointerK, 0, 0, 255)], []).Fault);
        var add = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.AddPointerK, 1, 1, 0) };
        Assert.Equal(VmFault.InvalidMemory, VmExec.Run(add, [1], [], [], objectConstants: [new object()]).Fault);
        Assert.Equal(VmFault.Overflow, VmExec.Run(add, [1], [], [], objectConstants: [new VmMemoryPointer(new byte[1], long.MaxValue)]).Fault);
        Assert.Equal(VmFault.InvalidMemory, VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Lkp, 2, 1, 0),
            VmCode.Rrr(VmOp.SubPointer, 0, 1, 2)], [], [], [], objectConstants: [new byte[1], new byte[1]]).Fault);
    }

    [Theory]
    [InlineData(VmOp.LoadCString)]
    [InlineData(VmOp.LoadCStringR)]
    public void CStringLoadsCopyUntilFirstNullAndAcceptNullPointers(VmOp op)
    {
        foreach (var value in new string?[] { null, "", "hello", "\0tail", "head\0tail", "\u03a9" })
        {
            var memory = new VmObjectMemory(new Dictionary<int, object?> { [7] = value });
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 255, 0, 0), VmCode.Li(255, 7),
                VmCode.Rrr(op, 255, 255, op == VmOp.LoadCStringR ? 255 : 0),
                VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 255)], [7], [], [], objectConstants: [memory]);
            Assert.True(result.Returned); Assert.Equal(VmFault.None, result.Fault);
            Assert.Equal((value ?? "").Split('\0')[0], result.StringValue);
            Assert.Same(value, memory[7]);
        }
    }

    [Theory]
    [InlineData(VmOp.LoadCString)]
    [InlineData(VmOp.LoadCStringR)]
    public void CStringLoadsValidateBaseFieldsAndPointeeTypes(VmOp op)
    {
        var memory = new VmObjectMemory(new Dictionary<int, object?> { [7] = new object() });
        foreach (var offset in new[] { -1, 0, 8, int.MaxValue })
            Assert.Equal(VmFault.MemoryRange, VmExec.Run([VmCode.Rrr(VmOp.Lkp, 0, 0, 0), VmCode.Lk(3, 0),
                VmCode.Rrr(op, 0, 0, op == VmOp.LoadCStringR ? 3 : 0)], [offset], [], [], objectConstants: [memory]).Fault);
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 0, 0, 0), VmCode.Li(3, 7), VmCode.Rrr(op, 0, 0, op == VmOp.LoadCStringR ? 3 : 0) };
        Assert.Equal(VmFault.InvalidMemory, VmExec.Run(code, [7], [], [], objectConstants: [memory]).Fault);
        Assert.Equal(VmFault.InvalidMemory, VmExec.Run(code, [7], [], [], objectConstants: [new byte[32]]).Fault);
        Assert.Equal(new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 }, VmExec.Run(code, [7], [], [], objectConstants: [null]));
        if (op == VmOp.LoadCString) Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(op, 0, 0, 255)], []).Fault);
    }

    [Theory]
    [InlineData(VmOp.LoadObject)]
    [InlineData(VmOp.LoadObjectR)]
    [InlineData(VmOp.LoadPointer)]
    [InlineData(VmOp.LoadPointerR)]
    public void ReferenceLoadsPreserveIdentityNullAndBaseAliasing(VmOp op)
    {
        foreach (var value in new object?[] { new object(), null })
        {
            var memory = new VmObjectMemory(new Dictionary<int, object?> { [7] = value });
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 255, 0, 0), VmCode.Li(255, 7),
                VmCode.Rrr(op, 255, 255, ((int)op & 1) == 0 ? 255 : 0), VmCode.RetObject(255)],
                [7], [], [], objectConstants: [memory]);
            Assert.True(result.Returned); Assert.True(result.HasObjectValue); Assert.Same(value, result.ObjectValue);
            Assert.Same(value, memory[7]);
        }
    }

    [Theory]
    [InlineData(VmOp.StorePointer)]
    [InlineData(VmOp.StorePointerR)]
    [InlineData(VmOp.StoreObject)]
    [InlineData(VmOp.StoreObjectR)]
    public void ReferenceStoresPreserveNativeAssignmentAndBarrierOnlyBehavior(VmOp op)
    {
        foreach (var value in new object?[] { new object(), null })
        {
            var original = new object();
            var fields = new Dictionary<int, object?> { [7] = original, [19] = original };
            var memory = new VmObjectMemory(fields);
            fields[19] = null;
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Lkp, 255, 1, 0),
                VmCode.Li(255, 7), VmCode.Rrr(op, 1, 255, ((int)op & 1) != 0 ? 255 : 0),
                VmCode.Rrr(VmOp.LoadPointer, 1, 1, 0), VmCode.RetObject(1)], [7], [], [], objectConstants: [memory, value]);
            var expected = op == VmOp.StoreObjectR ? original : value;
            Assert.True(result.Returned); Assert.Same(expected, result.ObjectValue);
            Assert.Same(expected, memory[7]); Assert.Same(original, memory[19]);
        }
    }

    [Theory]
    [InlineData(VmOp.LoadObject)]
    [InlineData(VmOp.LoadObjectR)]
    [InlineData(VmOp.LoadPointer)]
    [InlineData(VmOp.LoadPointerR)]
    [InlineData(VmOp.StorePointer)]
    [InlineData(VmOp.StorePointerR)]
    [InlineData(VmOp.StoreObject)]
    [InlineData(VmOp.StoreObjectR)]
    public void ReferenceMemoryInstructionsValidateFieldsAndBases(VmOp op)
    {
        var store = (int)op >= (int)VmOp.StorePointer;
        var register = store ? ((int)op & 1) != 0 : ((int)op & 1) == 0;
        var original = new object();
        var memory = new VmObjectMemory(new Dictionary<int, object?> { [7] = original });
        foreach (var offset in new[] { -1, 0, 8, int.MaxValue })
        {
            Assert.Equal(VmFault.MemoryRange, VmExec.Run([VmCode.Rrr(VmOp.Lkp, 0, 0, 0), VmCode.Lk(3, 0),
                VmCode.Rrr(op, 0, 0, register ? 3 : 0)], [offset], [], [], objectConstants: [memory]).Fault);
            Assert.Same(original, memory[7]);
        }
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 0, 0, 0), VmCode.Rrr(op, 0, 0, 0) };
        Assert.Equal(new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = (ushort)(store ? 2 : 1) },
            VmExec.Run(code, [7], [], [], objectConstants: [null]));
        Assert.Equal(VmFault.InvalidMemory, VmExec.Run(code, [7], [], [], objectConstants: [new byte[32]]).Fault);
        if (!register) Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(op, 0, 0, 255)], []).Fault);
    }

    [Fact]
    public void ReferenceMemoryRejectsInvalidFieldMaps()
    {
        Assert.Throws<ArgumentNullException>(() => new VmObjectMemory(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => new VmObjectMemory(new Dictionary<int, object?> { [-1] = null }));
    }

    [Theory]
    [InlineData(VmOp.LoadString, VmOp.StoreString)]
    [InlineData(VmOp.LoadStringR, VmOp.StoreStringR)]
    public void StringMemoryInstructionsCopyInitializedFields(VmOp load, VmOp store)
    {
        var register = load == VmOp.LoadStringR;
        foreach (var value in new[] { "", "hello", "a\0b", "\u03a9" })
        {
            var fields = new Dictionary<int, string> { [7] = "old", [19] = "untouched" };
            var memory = new VmStringMemory(fields);
            fields[19] = "changed outside";
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 255, 0, 0), VmCode.Rrr(VmOp.Lks, 255, 0, 0),
                VmCode.Li(255, 7), VmCode.Rrr(store, 255, 255, register ? 255 : 0),
                VmCode.Rrr(load, 255, 255, register ? 255 : 0), VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 255)],
                [7], [], [value], objectConstants: [memory]);
            Assert.True(result.Returned); Assert.Equal(VmFault.None, result.Fault);
            Assert.Equal(value, result.StringValue); Assert.Equal(value, memory[7]);
            Assert.Equal("untouched", memory[19]);
        }
    }

    [Theory]
    [InlineData(VmOp.LoadString, false, false)]
    [InlineData(VmOp.LoadStringR, false, true)]
    [InlineData(VmOp.StoreString, true, false)]
    [InlineData(VmOp.StoreStringR, true, true)]
    public void StringMemoryInstructionsRejectMissingFieldsAndInvalidBases(VmOp op, bool store, bool register)
    {
        var memory = new VmStringMemory(new Dictionary<int, string> { [7] = "preserved" });
        foreach (var offset in new[] { -1, 0, 8, int.MaxValue })
        {
            Assert.Equal(VmFault.MemoryRange, VmExec.Run([VmCode.Rrr(VmOp.Lkp, 0, 0, 0), VmCode.Lk(3, 0),
                VmCode.Rrr(op, 0, 0, register ? 3 : 0)], [offset], [], [], objectConstants: [memory]).Fault);
            Assert.Equal("preserved", memory[7]);
        }
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 0, 0, 0), VmCode.Rrr(op, 0, 0, 0) };
        Assert.Equal(new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = (ushort)(store ? 2 : 1) },
            VmExec.Run(code, [7], [], [], objectConstants: [null]));
        Assert.Equal(VmFault.InvalidMemory, VmExec.Run(code, [7], [], [], objectConstants: [new byte[32]]).Fault);
        if (!register) Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(op, 0, 0, 255)], []).Fault);
    }

    [Fact]
    public void StringMemoryRequiresInitializedNonnegativeFields()
    {
        Assert.Throws<ArgumentNullException>(() => new VmStringMemory(null!));
        Assert.Throws<ArgumentNullException>(() => new VmStringMemory(new Dictionary<int, string> { [0] = null! }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new VmStringMemory(new Dictionary<int, string> { [-1] = "value" }));
    }

    [Fact]
    public void BitLoadsUseImmediateMaskAndReturnNormalizedBoolean()
    {
        foreach (var value in new[] { 0, 1, 0x80, 0xa5, 0xff })
        foreach (var mask in new[] { 0, 1, 0x80, 0x55, 0xff })
        {
            var memory = new byte[] { (byte)value, 0x5a };
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 255, 0, 0),
                VmCode.Rrr(VmOp.LoadBit, 255, 255, mask), VmCode.RetInt(255)], [], [], [], objectConstants: [memory]);
            Assert.Equal(new VmResult(true, (value & mask) != 0 ? 1 : 0, VmFault.None), result);
            Assert.Equal(new byte[] { (byte)value, 0x5a }, memory);
        }
    }

    [Fact]
    public void BitStoresSetOrClearOnlyMaskedBitsAndCanBeReadBack()
    {
        foreach (var original in new[] { 0, 0xa5, 0xff })
        foreach (var mask in new[] { 0, 1, 0x80, 0x55, 0xff })
        foreach (var value in new[] { 0, 1, -1, int.MinValue })
        {
            var memory = new byte[] { (byte)original, 0x5a };
            var expected = unchecked((byte)(value != 0 ? original | mask : original & ~mask));
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 255, 0, 0), VmCode.Lk(255, 0),
                VmCode.Rrr(VmOp.StoreBit, 255, 255, mask), VmCode.Rrr(VmOp.LoadBit, 0, 255, mask), VmCode.RetInt(0)],
                [value], [], [], objectConstants: [memory]);
            Assert.Equal(new VmResult(true, (expected & mask) != 0 ? 1 : 0, VmFault.None), result);
            Assert.Equal(new byte[] { expected, 0x5a }, memory);
        }
    }

    [Theory]
    [InlineData(VmOp.LoadBit, 1)]
    [InlineData(VmOp.StoreBit, 2)]
    public void BitMemoryInstructionsValidateEvenZeroMasks(VmOp op, ushort abortCode)
    {
        foreach (var mask in new[] { 0, 255 })
        {
            var code = new[] { VmCode.Rrr(VmOp.Lkp, 0, 0, 0), VmCode.Rrr(op, 0, 0, mask) };
            Assert.Equal(new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = abortCode },
                VmExec.Run(code, [], [], [], objectConstants: [null]));
            Assert.Equal(VmFault.InvalidMemory, VmExec.Run(code, [], [], [], objectConstants: [new object()]).Fault);
            Assert.Equal(VmFault.MemoryRange, VmExec.Run(code, [], [], [], objectConstants: [Array.Empty<byte>()]).Fault);
        }
    }

    public static IEnumerable<object[]> FloatingMemoryLoads()
    {
        yield return [VmOp.LoadFloat, 4]; yield return [VmOp.LoadFloatR, 4];
        yield return [VmOp.LoadDouble, 8]; yield return [VmOp.LoadDoubleR, 8];
    }

    [Theory]
    [MemberData(nameof(FloatingMemoryLoads))]
    public void FloatingLoadsPreservePrecisionSpecialValuesAndSourceBytes(VmOp op, int width)
    {
        var register = ((int)op & 1) == 0;
        foreach (var value in new[] { 1.0000000596046448, -0.0, double.MaxValue, double.Epsilon,
            (double)float.Epsilon, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            var memory = Enumerable.Repeat((byte)0xaa, width + 2).ToArray();
            if (width == 4) System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(memory.AsSpan(1, width), (float)value);
            else System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(memory.AsSpan(1, width), value);
            var original = memory.ToArray();
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Li(255, 1),
                VmCode.Rrr(op, 255, 1, register ? 255 : 0), VmCode.RetFloat(255)], [1], [], [], objectConstants: [memory]);
            Assert.True(result.Returned); Assert.Equal(VmFault.None, result.Fault);
            var actual = Assert.IsType<double>(result.FloatValue);
            var expected = width == 4 ? (double)(float)value : value;
            if (double.IsNaN(expected)) Assert.True(double.IsNaN(actual));
            else Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(actual));
            Assert.Equal(original, memory);
        }
    }

    [Theory]
    [MemberData(nameof(FloatingMemoryLoads))]
    public void FloatingLoadsRejectInvalidReads(VmOp op, int width)
    {
        var register = ((int)op & 1) == 0;
        foreach (var offset in new[] { -1, 3, int.MaxValue })
            Assert.Equal(VmFault.MemoryRange, VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Lk(3, 0),
                VmCode.Rrr(op, 0, 1, register ? 3 : 0)], [offset], [], [], objectConstants: [new byte[width + 2]]).Fault);
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(op, 0, 1, 0) };
        Assert.Equal(new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 }, VmExec.Run(code, [0], [], [], objectConstants: [null]));
        Assert.Equal(VmFault.InvalidMemory, VmExec.Run(code, [0], [], [], objectConstants: [new object()]).Fault);
        if (!register) Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(op, 0, 0, 255)], []).Fault);
    }

    public static IEnumerable<object[]> VectorMemoryLoads()
    {
        yield return [VmOp.LoadVector2, 2, 8]; yield return [VmOp.LoadVector2R, 2, 8];
        yield return [VmOp.LoadVector3, 3, 8]; yield return [VmOp.LoadVector3R, 3, 8];
        yield return [VmOp.LoadVector4, 4, 8]; yield return [VmOp.LoadVector4R, 4, 8];
        yield return [VmOp.LoadFloatVector2, 2, 4]; yield return [VmOp.LoadFloatVector2R, 2, 4];
        yield return [VmOp.LoadFloatVector3, 3, 4]; yield return [VmOp.LoadFloatVector3R, 3, 4];
        yield return [VmOp.LoadFloatVector4, 4, 4]; yield return [VmOp.LoadFloatVector4R, 4, 4];
    }

    [Theory]
    [MemberData(nameof(VectorMemoryLoads))]
    public void VectorLoadsPreserveComponentOrderPrecisionAndSourceBytes(VmOp op, int components, int width)
    {
        var register = ((int)op & 1) == 0;
        foreach (var values in new[] { new[] { 1.0000000596046448, -0.0, double.MaxValue, double.Epsilon },
            new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, -7.25 } })
        {
            var memory = Enumerable.Repeat((byte)0xaa, components * width + 2).ToArray();
            for (var i = 0; i < components; i++)
            {
                var bytes = memory.AsSpan(1 + i * width, width);
                if (width == 4) System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes, (float)values[i]);
                else System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(bytes, values[i]);
            }
            var original = memory.ToArray();
            var start = 256 - components;
            var result = VmExec.RunWithReturns([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Li(start, 1),
                VmCode.Rrr(op, start, 1, register ? start : 0), VmCode.RetVector(start, components)], [1], [], [], 1, objectConstants: [memory]);
            Assert.True(result.Returned); Assert.Equal(VmFault.None, result.Fault);
            var vector = Assert.IsType<VmVectorResult>(result.Values[0].VectorValue);
            var actual = new[] { vector.X, vector.Y, vector.Z, vector.W };
            for (var i = 0; i < components; i++)
            {
                var expected = width == 4 ? (double)(float)values[i] : values[i];
                if (double.IsNaN(expected)) Assert.True(double.IsNaN(actual[i]));
                else Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(actual[i]));
            }
            Assert.Equal(original, memory);
        }
    }

    [Theory]
    [MemberData(nameof(VectorMemoryLoads))]
    public void VectorLoadsRejectInvalidMemoryAndDestinationSpans(VmOp op, int components, int width)
    {
        var register = ((int)op & 1) == 0;
        foreach (var offset in new[] { -1, 3, int.MaxValue })
            Assert.Equal(VmFault.MemoryRange, VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Lk(3, 0),
                VmCode.Rrr(op, 0, 1, register ? 3 : 0)], [offset], [], [], objectConstants: [new byte[components * width + 2]]).Fault);
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(op, 257 - components, 0, 0)], [0]).Fault);
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(op, 0, 1, 0) };
        Assert.Equal(new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 }, VmExec.Run(code, [0], [], [], objectConstants: [null]));
        Assert.Equal(VmFault.InvalidMemory, VmExec.Run(code, [0], [], [], objectConstants: [new object()]).Fault);
        if (!register) Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(op, 0, 0, 255)], []).Fault);
    }

    public static IEnumerable<object[]> VectorMemoryStores()
    {
        yield return [VmOp.StoreVector2, 2, 8]; yield return [VmOp.StoreVector2R, 2, 8];
        yield return [VmOp.StoreVector3, 3, 8]; yield return [VmOp.StoreVector3R, 3, 8];
        yield return [VmOp.StoreVector4, 4, 8]; yield return [VmOp.StoreVector4R, 4, 8];
        yield return [VmOp.StoreFloatVector2, 2, 4]; yield return [VmOp.StoreFloatVector2R, 2, 4];
        yield return [VmOp.StoreFloatVector3, 3, 4]; yield return [VmOp.StoreFloatVector3R, 3, 4];
        yield return [VmOp.StoreFloatVector4, 4, 4]; yield return [VmOp.StoreFloatVector4R, 4, 4];
    }

    [Theory]
    [MemberData(nameof(VectorMemoryStores))]
    public void VectorStoresWriteOrderedComponentsAtUnalignedOffsets(VmOp op, int components, int width)
    {
        var register = ((int)op & 1) != 0;
        foreach (var values in new[] { new[] { 1.0000000596046448, -0.0, double.MaxValue, double.Epsilon },
            new[] { double.PositiveInfinity, double.NaN, double.NegativeInfinity, -7.25 } })
        {
            var memory = Enumerable.Repeat((byte)0xaa, components * width + 2).ToArray();
            var code = new List<int> { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Li(3, 1) };
            for (var i = 0; i < components; i++) code.Add(VmCode.Rrr(VmOp.Lkf, 256 - components + i, i, 0));
            code.Add(VmCode.Rrr(op, 1, 256 - components, register ? 3 : 0));
            code.Add(VmCode.Reti(7));
            Assert.Equal(new VmResult(true, 7, VmFault.None), VmExec.Run(code.ToArray(), [1], values, [], objectConstants: [memory]));
            Assert.Equal((byte)0xaa, memory[0]); Assert.Equal((byte)0xaa, memory[^1]);
            for (var i = 0; i < components; i++)
            {
                var bytes = memory.AsSpan(1 + i * width, width);
                if (width == 8) Assert.Equal(BitConverter.DoubleToInt64Bits(values[i]), System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(bytes));
                else
                {
                    var actual = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(bytes);
                    if (double.IsNaN(values[i])) Assert.True(float.IsNaN(actual));
                    else Assert.Equal(BitConverter.SingleToInt32Bits((float)values[i]), BitConverter.SingleToInt32Bits(actual));
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(VectorMemoryStores))]
    public void VectorStoresValidateEntireWriteBeforeMutation(VmOp op, int components, int width)
    {
        var register = ((int)op & 1) != 0;
        foreach (var offset in new[] { -1, 3, int.MaxValue })
        {
            var memory = Enumerable.Repeat((byte)0xaa, components * width + 2).ToArray();
            Assert.Equal(VmFault.MemoryRange, VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Lk(3, 0),
                VmCode.Rrr(op, 1, 0, register ? 3 : 0)], [offset], [], [], objectConstants: [memory]).Fault);
            Assert.All(memory, value => Assert.Equal((byte)0xaa, value));
        }
        var buffer = Enumerable.Repeat((byte)0xaa, components * width).ToArray();
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0),
            VmCode.Rrr(op, 1, 257 - components, 0)], [0], [], [], objectConstants: [buffer]).Fault);
        Assert.All(buffer, value => Assert.Equal((byte)0xaa, value));
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(op, 1, 0, 0) };
        Assert.Equal(new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 2 }, VmExec.Run(code, [0], [], [], objectConstants: [null]));
        Assert.Equal(VmFault.InvalidMemory, VmExec.Run(code, [0], [], [], objectConstants: [new object()]).Fault);
        if (!register) Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(op, 0, 0, 255)], []).Fault);
    }

    public static IEnumerable<object[]> FloatingMemoryStores()
    {
        yield return [VmOp.StoreFloat, 4]; yield return [VmOp.StoreFloatR, 4];
        yield return [VmOp.StoreDouble, 8]; yield return [VmOp.StoreDoubleR, 8];
    }

    [Theory]
    [MemberData(nameof(FloatingMemoryStores))]
    public void FloatingStoresPreservePrecisionAndSpecialValues(VmOp op, int width)
    {
        var register = ((int)op & 1) != 0;
        foreach (var value in new[] { 1.0000000596046448, double.MaxValue, double.Epsilon, -0.0, double.PositiveInfinity, double.NegativeInfinity, double.NaN })
        {
            var memory = Enumerable.Repeat((byte)0xaa, width + 2).ToArray();
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Lkf, 255, 0, 0),
                VmCode.Li(3, 1), VmCode.Rrr(op, 1, 255, register ? 3 : 0), VmCode.Reti(7)], [1], [value], [], objectConstants: [memory]);
            Assert.Equal(new VmResult(true, 7, VmFault.None), result);
            Assert.Equal((byte)0xaa, memory[0]); Assert.Equal((byte)0xaa, memory[^1]);
            if (width == 4)
            {
                var actual = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(memory.AsSpan(1, width));
                if (double.IsNaN(value)) Assert.True(float.IsNaN(actual));
                else Assert.Equal(BitConverter.SingleToInt32Bits((float)value), BitConverter.SingleToInt32Bits(actual));
            }
            else
                Assert.Equal(BitConverter.DoubleToInt64Bits(value), System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(memory.AsSpan(1, width)));
        }
    }

    [Theory]
    [MemberData(nameof(FloatingMemoryStores))]
    public void FloatingStoresRejectInvalidWritesBeforeMutation(VmOp op, int width)
    {
        var register = ((int)op & 1) != 0;
        foreach (var offset in new[] { -1, 3, int.MaxValue })
        {
            var memory = Enumerable.Repeat((byte)0xaa, width + 2).ToArray();
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Lk(3, 0),
                VmCode.Rrr(op, 1, 0, register ? 3 : 0)], [offset], [], [], objectConstants: [memory]);
            Assert.Equal(VmFault.MemoryRange, result.Fault);
            Assert.All(memory, value => Assert.Equal((byte)0xaa, value));
        }
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(op, 1, 0, 0) };
        Assert.Equal(new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 2 }, VmExec.Run(code, [0], [], [], objectConstants: [null]));
        Assert.Equal(VmFault.InvalidMemory, VmExec.Run(code, [0], [], [], objectConstants: [new object()]).Fault);
        if (!register) Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(op, 0, 0, 255)], []).Fault);
    }

    public static IEnumerable<object[]> IntegerMemoryStores()
    {
        yield return [VmOp.StoreByte, 1]; yield return [VmOp.StoreByteR, 1];
        yield return [VmOp.StoreHalf, 2]; yield return [VmOp.StoreHalfR, 2];
        yield return [VmOp.StoreWord, 4]; yield return [VmOp.StoreWordR, 4];
    }

    [Theory]
    [MemberData(nameof(IntegerMemoryStores))]
    public void IntegerStoresPreserveLowBitsAndOnlyModifySelectedBytes(VmOp op, int width)
    {
        var register = ((int)op & 1) != 0;
        foreach (var value in new[] { unchecked((int)0x87654321), -1 })
        {
            var memory = Enumerable.Repeat((byte)0xaa, 6).ToArray();
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Lk(2, 1), VmCode.Li(3, 1),
                VmCode.Rrr(op, 1, 2, register ? 3 : 0), VmCode.Reti(7)], [1, value], [], [], objectConstants: [memory]);
            Assert.Equal(7, result.Value);
            for (var index = 0; index < memory.Length; index++)
                Assert.Equal(index >= 1 && index <= width ? unchecked((byte)((uint)value >> ((index - 1) * 8))) : (byte)0xaa, memory[index]);
        }
    }

    [Theory]
    [MemberData(nameof(IntegerMemoryStores))]
    public void IntegerStoresRejectInvalidWritesBeforeMutatingBuffer(VmOp op, int width)
    {
        var register = ((int)op & 1) != 0;
        foreach (var offset in new[] { -1, 6 - width + 1, int.MaxValue })
        {
            var memory = Enumerable.Repeat((byte)0xaa, 6).ToArray();
            var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Lk(3, 0), VmCode.Rrr(op, 1, 0, register ? 3 : 0) };
            Assert.Equal(VmFault.MemoryRange, VmExec.Run(code, [offset], [], [], objectConstants: [memory]).Fault);
            Assert.All(memory, value => Assert.Equal((byte)0xaa, value));
        }
        var store = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(op, 1, 0, 0) };
        var nil = VmExec.Run(store, [0], [], [], objectConstants: [null]);
        Assert.Equal(VmFault.ScriptAbort, nil.Fault); Assert.Equal((ushort)2, nil.AbortCode);
        Assert.Equal(VmFault.InvalidMemory, VmExec.Run(store, [0], [], [], objectConstants: [new object()]).Fault);
        if (!register) Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(op, 0, 0, 255)], []).Fault);
    }

    public static IEnumerable<object[]> IntegerMemoryLoads()
    {
        yield return [VmOp.LoadByte, -128]; yield return [VmOp.LoadByteR, -128];
        yield return [VmOp.LoadHalf, -128]; yield return [VmOp.LoadHalfR, -128];
        yield return [VmOp.LoadWord, -128]; yield return [VmOp.LoadWordR, -128];
        yield return [VmOp.LoadUnsignedByte, 128]; yield return [VmOp.LoadUnsignedByteR, 128];
        yield return [VmOp.LoadUnsignedHalf, 65408]; yield return [VmOp.LoadUnsignedHalfR, 65408];
    }

    [Theory]
    [MemberData(nameof(IntegerMemoryLoads))]
    public void IntegerLoadsPreserveNativeWidthsSignednessAndUnalignedOffsets(VmOp op, int expected)
    {
        var register = ((int)op & 1) == 0;
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Li(2, 1),
            VmCode.Rrr(op, 2, 1, register ? 2 : 0), VmCode.RetInt(2)], [1], [], [],
            objectConstants: [new byte[] { 0, 128, 255, 255, 255 }]);
        Assert.Equal(new VmResult(true, expected, VmFault.None), result);
    }

    [Theory]
    [MemberData(nameof(IntegerMemoryLoads))]
    public void IntegerLoadsValidateMemoryAndOffsetInputs(VmOp op, int unused)
    {
        Assert.NotEqual(0, unused);
        var register = ((int)op & 1) == 0;
        foreach (var offset in new[] { -1, 5, int.MaxValue })
        {
            var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Lk(2, 0), VmCode.Rrr(op, 0, 1, register ? 2 : 0) };
            Assert.Equal(VmFault.MemoryRange, VmExec.Run(code, [offset], [], [], objectConstants: [new byte[5]]).Fault);
        }
        var load = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(op, 0, 1, 0) };
        var nil = VmExec.Run(load, [0], [], [], objectConstants: [null]);
        Assert.Equal(VmFault.ScriptAbort, nil.Fault); Assert.Equal((ushort)1, nil.AbortCode);
        Assert.Equal(VmFault.InvalidMemory, VmExec.Run(load, [0], [], [], objectConstants: [new object()]).Fault);
        if (!register) Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(op, 0, 0, 255)], []).Fault);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ClassLookupReturnsActualDerivedDescriptorAndSupportsAliasing(int destination)
    {
        CastBase source = new CastDerived();
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0),
            VmCode.Rrr(VmOp.Class, destination, 1, 0), VmCode.RetObject(destination)], [], [], [], objectConstants: [source]);
        Assert.True(result.HasObjectValue);
        Assert.Same(typeof(CastDerived), result.ObjectValue);
    }

    [Fact]
    public void ClassLookupFeedsNativeClassCastFlow()
    {
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Class, 1, 1, 0),
            VmCode.Rrr(VmOp.DynCastClassK, 1, 1, 1), VmCode.RetObject(1)], [], [], [],
            objectConstants: [new CastDerived(), typeof(CastBase)]);
        Assert.Same(typeof(CastDerived), result.ObjectValue);
    }

    [Fact]
    public void ClassLookupReportsReadNilAndRejectsNonObjectDescriptors()
    {
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Class, 1, 1, 0), VmCode.RetObject(1) };
        Assert.Equal(new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 },
            VmExec.Run(code, [], [], [], objectConstants: [null]));
        Assert.Equal(VmFault.InvalidClass, VmExec.Run(code, [], [], [], objectConstants: [typeof(CastDerived)]).Fault);
        Assert.Equal(VmFault.InvalidClass, VmExec.Run(code, [], [], [], objectConstants: [7]).Fault);
    }

    private class CastBase { }
    private sealed class CastDerived : CastBase { }
    private sealed class CastUnrelated { }

    [Theory]
    [InlineData(VmOp.DynCastR)]
    [InlineData(VmOp.DynCastK)]
    [InlineData(VmOp.DynCastClassR)]
    [InlineData(VmOp.DynCastClassK)]
    public void DynamicCastsPreserveClassInheritanceIdentityAndAliasing(VmOp op)
    {
        var classCast = op is VmOp.DynCastClassR or VmOp.DynCastClassK;
        var constant = op is VmOp.DynCastK or VmOp.DynCastClassK;
        object source = classCast ? typeof(CastDerived) : new CastDerived();
        foreach (var target in new[] { typeof(CastBase), typeof(CastDerived), typeof(CastUnrelated) })
        foreach (var destination in new[] { 1, 2 })
        {
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Lkp, 2, 1, 0),
                VmCode.Rrr(op, destination, 1, constant ? 1 : 2), VmCode.RetObject(destination)], [], [], [],
                objectConstants: [source, target]);
            Assert.True(result.HasObjectValue);
            Assert.Same(target == typeof(CastUnrelated) ? null : source, result.ObjectValue);
        }
    }

    [Theory]
    [InlineData(VmOp.DynCastR)]
    [InlineData(VmOp.DynCastK)]
    [InlineData(VmOp.DynCastClassR)]
    [InlineData(VmOp.DynCastClassK)]
    public void DynamicCastsHandleNullAndRejectInvalidDescriptors(VmOp op)
    {
        var constant = op is VmOp.DynCastK or VmOp.DynCastClassK;
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Lkp, 2, 1, 0),
            VmCode.Rrr(op, 1, 1, constant ? 1 : 2), VmCode.RetObject(1) };
        var nullResult = VmExec.Run(code, [], [], [], objectConstants: [null, new object()]);
        Assert.True(nullResult.HasObjectValue); Assert.Null(nullResult.ObjectValue);
        Assert.Equal(VmFault.InvalidClass, VmExec.Run(code, [], [], [], objectConstants: [new CastDerived(), new object()]).Fault);
        Assert.Equal(VmFault.InvalidClass, VmExec.Run(code, [], [], [], objectConstants: [new CastDerived(), typeof(IDisposable)]).Fault);
        if (constant)
            Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(op, 0, 0, 255)], []).Fault);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ObjectReturnsPreserveIdentityAndTypedNull(bool constant, bool present)
    {
        var value = present ? new object() : null;
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.RetObject(constant ? 0 : 1, constant)],
            [], [], [], objectConstants: [value]);
        Assert.True(result.Returned);
        Assert.True(result.HasObjectValue);
        Assert.Same(value, result.ObjectValue);
        Assert.Null(result.FloatValue); Assert.Null(result.StringValue); Assert.Null(result.VectorValue);
    }

    [Fact]
    public void NonfinalObjectReturnCapturesReferenceBeforeRegisterReplacement()
    {
        var first = new object(); var second = new object();
        var result = VmExec.RunWithReturns([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Ret, 0, 3, 1),
            VmCode.Rrr(VmOp.Lkp, 1, 1, 0), VmCode.Rrr(VmOp.Ret, VmCode.RetFinal | 1, 3, 1)], [], [], [], 2,
            objectConstants: [first, second]);
        Assert.Equal(2, result.Values.Count);
        Assert.Same(first, result.Values[0].ObjectValue);
        Assert.Same(second, result.Values[1].ObjectValue);
    }

    [Fact]
    public void ObjectReturnChecksConstantsAndKeepsUnrequestedSlotsUnaccessed()
    {
        Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.RetObject(255, true)], []).Fault);
        var ignored = VmExec.RunWithReturns([VmCode.RetObject(255, true)], [], [], [], 0);
        Assert.True(ignored.Returned); Assert.Empty(ignored.Values);
        var empty = VmExec.Run([VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 128, 0)], []);
        Assert.False(empty.HasObjectValue);
        Assert.True(VmExec.Run([VmCode.RetObject(255)], []).HasObjectValue);
    }

    [Theory]
    [InlineData(VmOp.EqaR, true)]
    [InlineData(VmOp.EqaR, false)]
    [InlineData(VmOp.EqaK, true)]
    [InlineData(VmOp.EqaK, false)]
    public void ObjectComparisonsUseIdentityRatherThanValueEquality(VmOp op, bool same)
    {
        object left = new string('a', 1);
        object right = same ? left : new string('a', 1);
        foreach (var expect in new[] { 0, 1 })
        {
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkp, 10, 0, 0), VmCode.Rrr(VmOp.Lkp, 20, 1, 0),
                VmCode.Rrr(VmOp.MoveA, 11, 10, 0), VmCode.Cmp(op, expect, 11, op == VmOp.EqaK ? 1 : 20),
                VmCode.Jmp(1), VmCode.Reti(10), VmCode.Reti(20)], [], [], [], objectConstants: [left, right]);
            Assert.Equal(same == (expect == 1) ? 20 : 10, result.Value);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ObjectBooleanAndNullCheckMatchNativePointerPresence(bool present)
    {
        object?[] constants = [present ? new object() : null];
        var code = new[] { VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.CastB, 1, 1, (int)VmCast.ObjectToBool), VmCode.RetInt(1) };
        Assert.Equal(present ? 1 : 0, VmExec.Run(code, [], [], [], objectConstants: constants).Value);
        code[1] = VmCode.Rrr(VmOp.NullCheck, 1, 0, 0);
        var result = VmExec.Run(code, [], [], [], objectConstants: constants);
        Assert.Equal(present ? VmFault.None : VmFault.ScriptAbort, result.Fault);
        if (!present) Assert.Equal((ushort)2, result.AbortCode);
    }

    [Fact]
    public void IndexedObjectLoadsValidateRangeAndFullWidthConstantIndexes()
    {
        var constants = new object?[257]; constants[256] = new object();
        var result = VmExec.Run([VmCode.Li(1, 255), VmCode.Rrr(VmOp.LkpR, 1, 1, 1),
            VmCode.Rrr(VmOp.NullCheck, 1, 0, 0), VmCode.Reti(7)], [], [], [], objectConstants: constants);
        Assert.Equal(7, result.Value);
        Assert.Equal(VmFault.None, VmExec.Run([VmCode.Rrr(VmOp.Lkp, 1, 0, 1),
            VmCode.Rrr(VmOp.NullCheck, 1, 0, 0), VmCode.Reti(0)], [], [], [], objectConstants: constants).Fault);
        foreach (var index in new[] { -1, 257, int.MaxValue })
            Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Lk(1, 0), VmCode.Rrr(VmOp.LkpR, 0, 1, 0)],
                [index], [], [], objectConstants: constants).Fault);
        Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Cmp(VmOp.EqaK, 1, 0, 0)], []).Fault);
        Assert.Equal(VmFault.BadJump, VmExec.Run([VmCode.Cmp(VmOp.EqaR, 1, 0, 0), VmCode.Reti(0)], []).Fault);
    }

    [Fact]
    public void ColorCatalogResolvesNamesBeforeNumericFallback()
    {
        var catalog = VmColorCatalog.Parse("! comment\n255 0 0 Red\n1 2 3 pale blue\n4 5 6 RED\n7 8 9 abcdef\n");
        foreach (var item in new[] { ("rEd", 0xff0000), ("pale blue", 0x010203), ("abcdef", 0x070809),
            ("Red\0tail", 0xff0000), ("#123", 0x112233) })
        {
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lks, 0, 0, 0),
                VmCode.Rrr(VmOp.Cast, 0, 0, (int)VmCast.StringToColor), VmCode.RetInt(0)], [], [], [item.Item1], catalog);
            Assert.Equal(item.Item2, result.Value);
            Assert.Equal(VmFault.None, result.Fault);
        }
    }

    [Fact]
    public void ColorCatalogWorksWithMultipleReturnsAndIsOptional()
    {
        var catalog = VmColorCatalog.Parse("255 0 0 red");
        var code = new[] { VmCode.Rrr(VmOp.Lks, 0, 0, 0), VmCode.Rrr(VmOp.Cast, 0, 0, (int)VmCast.StringToColor), VmCode.RetInt(0) };
        Assert.Equal(0xff0000, VmExec.RunWithReturns(code, [], [], ["red"], 1, catalog).Values[0].Value);
        Assert.Equal(VmFault.InvalidColor, VmExec.Run(code, [], [], ["red"]).Fault);
    }

    [Theory]
    [InlineData("256 0 0 bad")]
    [InlineData("-1 0 0 bad")]
    [InlineData("1 2 bad")]
    [InlineData("1 2 3")]
    public void ColorCatalogReportsMalformedResourceData(string text)
        => Assert.Throws<FormatException>(() => VmColorCatalog.Parse(text));

    [Theory]
    [InlineData("#123", 0x112233)]
    [InlineData("#AbC", 0xaabbcc)]
    [InlineData("#123456", 0x123456)]
    [InlineData("abcdef", 0xabcdef)]
    [InlineData("12 34 56", 0x123456)]
    [InlineData("1 2 f", 0x1122ff)]
    [InlineData("1234 5678 9abc", 0x12569a)]
    [InlineData("  12\t34\n56 extra", 0x123456)]
    [InlineData("12", 0x120000)]
    [InlineData("", 0)]
    [InlineData("#12", 0)]
    [InlineData("#123\0ignored", 0x112233)]
    public void NumericColorCastPreservesNativeFormatsAndComponents(string input, int expected)
        => Assert.Equal(new VmResult(true, expected, VmFault.None), VmExec.Run(
            [VmCode.Rrr(VmOp.Lks, 1, 0, 0), VmCode.Rrr(VmOp.Cast, 1, 1, (int)VmCast.StringToColor),
                VmCode.RetInt(1)], [], [], [input]));

    [Theory]
    [InlineData("#ggg")]
    [InlineData("red")]
    [InlineData("xy 00 00")]
    public void UnsupportedColorNamesOrInvalidDigitsFaultExplicitly(string input)
        => Assert.Equal(VmFault.InvalidColor, VmExec.Run(
            [VmCode.Rrr(VmOp.Lks, 1, 0, 0), VmCode.Rrr(VmOp.Cast, 1, 1, (int)VmCast.StringToColor)], [], [], [input]).Fault);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(255)]
    public void ThrowPreservesNativeAbortReasonAndIgnoresDisabledObjectSelector(int selector)
    {
        foreach (var reason in new[] { 0, 1, 2, 3, 4, 5, 6, 7, 256, 65535 })
        {
            var result = VmExec.Run([VmCode.Rrr(VmOp.Throw, selector, reason & 255, reason >> 8), VmCode.Reti(9)], []);
            Assert.Equal(new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = (ushort)reason }, result);
        }
    }

    [Fact]
    public void ThrowDiscardsCapturedMultipleReturnsAndPropagatesAbortCode()
    {
        var result = VmExec.RunWithReturns([VmCode.Rrr(VmOp.Reti, 0, 7, 0),
            VmCode.Rrr(VmOp.Throw, 0, 5, 1), VmCode.Reti(99)], [], [], [], 2);
        Assert.False(result.Returned);
        Assert.Equal(VmFault.ScriptAbort, result.Fault);
        Assert.Equal((ushort)261, result.AbortCode);
        Assert.Empty(result.Values);
        Assert.Null(VmExec.Run([VmCode.Reti(9)], []).AbortCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void MultipleReturnsRespectRequestedCountAndTypedSlots(int requested)
    {
        var code = new[] { VmCode.Rrr(VmOp.Reti, 0, 7, 0), VmCode.Rrr(VmOp.Ret, 1, 5, 0),
            VmCode.Rrr(VmOp.Ret, VmCode.RetFinal | 2, 6, 0) };
        var result = VmExec.RunWithReturns(code, [], [2.5], ["three"], requested);
        Assert.True(result.Returned);
        Assert.Equal(VmFault.None, result.Fault);
        Assert.Equal(Math.Min(requested, 3), result.Values.Count);
        if (requested > 0) Assert.Equal(7, result.Values[0].Value);
        if (requested > 1) Assert.Equal(2.5, result.Values[1].FloatValue);
        if (requested > 2) Assert.Equal("three", result.Values[2].StringValue);
    }

    [Fact]
    public void MultipleReturnsCaptureVectorsOverwriteSlotsAndDiscardFaultedResults()
    {
        var code = new[] { VmCode.Rrr(VmOp.Ret, 0, 13, 0), VmCode.Rrr(VmOp.Reti, 1, 1, 0),
            VmCode.Rrr(VmOp.Reti, VmCode.RetFinal | 1, 9, 0) };
        var result = VmExec.RunWithReturns(code, [], [1.5, 2.5], [], 2);
        Assert.Equal(new VmVectorResult(2, 1.5, 2.5, 0, 0), result.Values[0].VectorValue);
        Assert.Equal(9, result.Values[1].Value);
        code[2] = VmCode.Rrr(VmOp.Lk, 0, 255, 0);
        result = VmExec.RunWithReturns(code, [], [1.5, 2.5], [], 2);
        Assert.Equal(VmFault.ConstantRange, result.Fault);
        Assert.False(result.Returned);
        Assert.Empty(result.Values);
    }

    [Fact]
    public void MultipleReturnsValidateCountAndNilReturnsNoValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => VmExec.RunWithReturns([], [], [], [], -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => VmExec.RunWithReturns([], [], [], [], 129));
        var result = VmExec.RunWithReturns([VmCode.Rrr(VmOp.Reti, 0, 7, 0),
            VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 128, 0)], [], [], [], 2);
        Assert.True(result.Returned);
        Assert.Empty(result.Values);
    }

    [Theory]
    [InlineData(0, "00 00 00")]
    [InlineData(0x123456, "12 34 56")]
    [InlineData(0xabcdef, "ab cd ef")]
    [InlineData(0x01020f, "01 02 0f")]
    [InlineData(-1, "ff ff ff")]
    [InlineData(unchecked((int)0xff123456), "12 34 56")]
    public void ColorStringCastUsesNativeRgbByteOrderAndIgnoresAlpha(int input, string expected)
    {
        var code = new[] { VmCode.Lk(1, 0), VmCode.Rrr(VmOp.Cast, 1, 1, (int)VmCast.ColorToString),
            VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 1) };
        Assert.Equal(expected, VmExec.Run(code, [input]).StringValue);
        code[2] = VmCode.RetInt(1);
        Assert.Equal(input, VmExec.Run(code, [input]).Value);
    }

    [Theory]
    [InlineData("  -1.25tail", -1.25)]
    [InlineData(".5", 0.5)]
    [InlineData("1e2junk", 100)]
    [InlineData("1e+", 1)]
    [InlineData("0x1.8p+2tail", 6)]
    [InlineData("-0X1p-1", -0.5)]
    [InlineData("0x.8", 0.5)]
    [InlineData("0x", 0)]
    [InlineData("invalid", 0)]
    [InlineData("2\0tail", 2)]
    [InlineData("INF", double.PositiveInfinity)]
    [InlineData("-infinity", double.NegativeInfinity)]
    [InlineData("1e9999", double.PositiveInfinity)]
    [InlineData("0x1p99999", double.PositiveInfinity)]
    [InlineData("0x1p-1074", double.Epsilon)]
    [InlineData("0x1p-1075", 0)]
    [InlineData("0x1.00000000000008p0", 1)]
    public void StringFloatCastsUseNativePrefixAndHexadecimalSemantics(string value, double expected)
    {
        var code = new[] { VmCode.Rrr(VmOp.Lks, 1, 0, 0), VmCode.Rrr(VmOp.Cast, 1, 1, (int)VmCast.StringToFloat),
            VmCode.RetFloat(1) };
        Assert.Equal(expected, VmExec.Run(code, [], [], [value]).FloatValue);
        code[2] = VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 1);
        Assert.Equal(value, VmExec.Run(code, [], [], [value]).StringValue);
    }

    [Fact]
    public void StringFloatCastsPreserveNanAndNegativeZero()
    {
        foreach (var value in new[] { "nan", "-NaN(payload)" })
            Assert.True(double.IsNaN(VmExec.Run([VmCode.Rrr(VmOp.Lks, 0, 0, 0),
                VmCode.Rrr(VmOp.Cast, 0, 0, (int)VmCast.StringToFloat), VmCode.RetFloat(0)], [], [], [value]).FloatValue!.Value));
        foreach (var value in new[] { "-0", "-0x0", "-0x1p-99999" })
            Assert.Equal(long.MinValue, BitConverter.DoubleToInt64Bits(VmExec.Run([VmCode.Rrr(VmOp.Lks, 0, 0, 0),
                VmCode.Rrr(VmOp.Cast, 0, 0, (int)VmCast.StringToFloat), VmCode.RetFloat(0)], [], [], [value]).FloatValue!.Value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(9)]
    [InlineData(17)]
    [InlineData(65)]
    public void NonfinalReturnsCaptureSlotZeroBeforeLaterRegisterChanges(int type)
    {
        var code = new[] { VmCode.Li(0, 7), VmCode.Rrr(VmOp.Lkf, 0, 0, 0), VmCode.Rrr(VmOp.Lks, 0, 0, 0),
            VmCode.Rrr(VmOp.Ret, 0, type, 0), VmCode.Li(0, 99), VmCode.Rrr(VmOp.Lkf, 0, 1, 0),
            VmCode.Rrr(VmOp.Lks, 0, 1, 0), VmCode.Rrr(VmOp.Ret, VmCode.RetFinal | 1, 255, 255) };
        var result = VmExec.Run(code, [], [2.5, 99], ["saved", "changed"]);
        Assert.True(result.Returned);
        if (type == 0) Assert.Equal(7, result.Value);
        else if (type == 1) Assert.Equal(2.5, result.FloatValue);
        else if (type == 2) Assert.Equal("saved", result.StringValue);
        else Assert.Equal(2.5, result.VectorValue!.Value.X);
    }

    [Fact]
    public void ImmediateReturnsCaptureAndOverwriteOnlyRequestedSlot()
    {
        Assert.Equal(7, VmExec.Run([VmCode.Rrr(VmOp.Reti, 0, 7, 0),
            VmCode.Rrr(VmOp.Reti, 1, 99, 0), VmCode.Rrr(VmOp.Reti, VmCode.RetFinal | 2, 88, 0)], []).Value);
        Assert.Equal(9, VmExec.Run([VmCode.Rrr(VmOp.Reti, 0, 7, 0), VmCode.Reti(9)], []).Value);
        Assert.Equal(VmFault.RanOffEnd, VmExec.Run([VmCode.Rrr(VmOp.Reti, 0, 7, 0)], []).Fault);
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData(" \t-42tail", -42)]
    [InlineData("+17", 17)]
    [InlineData("077", 63)]
    [InlineData("08", 0)]
    [InlineData("0xff", 255)]
    [InlineData("-0X10", -16)]
    [InlineData("0x", 0)]
    [InlineData("0b11", 0)]
    [InlineData("", 0)]
    [InlineData("garbage", 0)]
    [InlineData("12\0ignore", 12)]
    [InlineData("4294967295", -1)]
    [InlineData("9223372036854775808", -1)]
    [InlineData("-9223372036854775809", 0)]
    [InlineData("99999999999999999999999999999", -1)]
    [InlineData("\u00a012", 0)]
    public void StringIntegerCastsMatchNativeBaseZeroPrefixAndSaturation(string value, int expected)
    {
        var code = new[] { VmCode.Rrr(VmOp.Lks, 1, 0, 0), VmCode.Rrr(VmOp.Cast, 1, 1, (int)VmCast.StringToInt),
            VmCode.RetInt(1) };
        Assert.Equal(new VmResult(true, expected, VmFault.None), VmExec.Run(code, [], [], [value]));
        code[2] = VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 1);
        Assert.Equal(value, VmExec.Run(code, [], [], [value]).StringValue);
    }

    [Theory]
    [InlineData(1.25, "1.25000")]
    [InlineData(-2.5, "-2.50000")]
    [InlineData(0.0, "0.00000")]
    [InlineData(0.000001, "0.00000")]
    [InlineData(-0.000001, "-0.00000")]
    [InlineData(0.015625, "0.01562")]
    [InlineData(0.046875, "0.04688")]
    [InlineData(double.PositiveInfinity, "inf")]
    [InlineData(double.NegativeInfinity, "-inf")]
    [InlineData(double.NaN, "nan")]
    [InlineData(1e20, "100000000000000000000.00000")]
    public void FloatingStringCastsUseFiveDigitsAndExactBinaryRounding(double value, string expected)
        => Assert.Equal(expected, VmExec.Run([VmCode.Rrr(VmOp.Lkf, 1, 0, 0),
            VmCode.Rrr(VmOp.Cast, 1, 1, (int)VmCast.FloatToString),
            VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 1)], [], [value]).StringValue);

    [Theory]
    [InlineData(VmCast.Vector2ToString, "(1.25000, -2.50000)")]
    [InlineData(VmCast.Vector3ToString, "(1.25000, -2.50000, 3.00000)")]
    public void VectorStringCastsUseNativePunctuation(VmCast cast, string expected)
    {
        Assert.Equal(expected, VmExec.Run([VmCode.Rrr(VmOp.Lkf, 10, 0, 0), VmCode.Rrr(VmOp.Lkf, 11, 1, 0),
            VmCode.Rrr(VmOp.Lkf, 12, 2, 0), VmCode.Rrr(VmOp.Cast, 10, 10, (int)cast),
            VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 10)], [], [1.25, -2.5, 3]).StringValue);
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(VmOp.Cast, 0, 255, (int)cast)], []).Fault);
    }

    [Fact]
    public void FloatingStringCastPreservesNegativeZeroAndSubnormalSign()
    {
        foreach (var value in new[] { BitConverter.Int64BitsToDouble(long.MinValue), -double.Epsilon })
            Assert.Equal("-0.00000", VmExec.Run([VmCode.Rrr(VmOp.Lkf, 0, 0, 0),
                VmCode.Rrr(VmOp.Cast, 0, 0, (int)VmCast.FloatToString),
                VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 0)], [], [value]).StringValue);
    }

    [Theory]
    [InlineData(VmCast.IntToString, 0, "0")]
    [InlineData(VmCast.IntToString, int.MinValue, "-2147483648")]
    [InlineData(VmCast.IntToString, int.MaxValue, "2147483647")]
    [InlineData(VmCast.UnsignedToString, -1, "4294967295")]
    [InlineData(VmCast.UnsignedToString, int.MinValue, "2147483648")]
    [InlineData(VmCast.UnsignedToString, 0, "0")]
    public void IntegerStringCastsUseNativeDecimalSignedness(VmCast cast, int input, string expected)
    {
        var code = new[] { VmCode.Lk(1, 0), VmCode.Rrr(VmOp.Cast, 1, 1, (int)cast),
            VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 1) };
        Assert.Equal(expected, VmExec.Run(code, [input]).StringValue);
        code[2] = VmCode.RetInt(1);
        Assert.Equal(input, VmExec.Run(code, [input]).Value);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("0", 1)]
    [InlineData("false", 1)]
    [InlineData("\0", 1)]
    [InlineData("\u00e9", 1)]
    public void StringBooleanCastTestsStoredLengthRatherThanTextOrNullTerminator(string input, int expected)
    {
        var code = new[] { VmCode.Rrr(VmOp.Lks, 1, 0, 0), VmCode.Rrr(VmOp.CastB, 1, 1, (int)VmCast.StringToBool),
            VmCode.RetInt(1) };
        Assert.Equal(expected, VmExec.Run(code, [], [], [input]).Value);
        code[2] = VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 1);
        Assert.Equal(input, VmExec.Run(code, [], [], [input]).StringValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(24)]
    public void StringComparisonsHonorOperandFlagsMethodsAndPolarity(int operandFlags)
    {
        var cases = new (string Left, string Right, int Exact, int Folded)[]
        {
            ("a", "b", -1, -1), ("same", "same", 0, 0), ("b", "a", 1, 1),
            ("A", "a", -1, 0), ("prefix", "prefixlong", -1, -1),
            ("a\0z", "a\0b", 0, 0), ("\u00e9", "z", 1, 1),
            ("\ue000", "\ud800\udc00", -1, -1), ("", "a", -1, -1),
        };
        foreach (var item in cases)
        foreach (var method in new[] { 0, 2, 4 })
        foreach (var ignoreCase in new[] { false, true })
        foreach (var expect in new[] { 0, 1 })
        {
            var order = ignoreCase ? item.Folded : item.Exact;
            var truth = method == 0 ? order == 0 : method == 2 ? order < 0 : order <= 0;
            var flags = operandFlags | method | expect | (ignoreCase ? VmCode.CompareApprox : 0);
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lks, 10, 0, 0), VmCode.Rrr(VmOp.Lks, 20, 1, 0),
                VmCode.Cmp(VmOp.CmpS, flags, (operandFlags & 8) != 0 ? 0 : 10, (operandFlags & 16) != 0 ? 1 : 20),
                VmCode.Jmp(1), VmCode.Reti(10), VmCode.Reti(20)], [], [], [item.Left, item.Right]);
            Assert.Equal(new VmResult(true, truth == (expect == 1) ? 20 : 10, VmFault.None), result);
        }
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(24)]
    public void StringComparisonsRejectMissingConstants(int flags)
        => Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Cmp(VmOp.CmpS, flags, 255, 255)], []).Fault);

    [Fact]
    public void StringComparisonsRejectInvalidMethodAndMalformedTakenBranch()
    {
        Assert.Equal(VmFault.InvalidComparison, VmExec.Run([VmCode.Cmp(VmOp.CmpS, 6, 0, 0)], []).Fault);
        Assert.Equal(VmFault.BadJump, VmExec.Run([VmCode.Cmp(VmOp.CmpS, 1, 0, 0), VmCode.Reti(10)], []).Fault);
    }

    [Fact]
    public void StringsLoadMoveConcatenateAndReturnWithoutChangingOtherBanks()
    {
        var code = new[] { VmCode.Li(1, 1), VmCode.Rrr(VmOp.LksR, 1, 1, 1),
            VmCode.Rrr(VmOp.MoveS, 2, 1, 0), VmCode.Rrr(VmOp.Concat, 2, 2, 1),
            VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 2) };
        Assert.Equal("abab", VmExec.Run(code, [], [], ["unused", "unused", "ab"]).StringValue);
        code[4] = VmCode.RetInt(1);
        Assert.Equal(1, VmExec.Run(code, [], [], ["unused", "unused", "ab"]).Value);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("abc", 3)]
    [InlineData("\u00e9", 2)]
    [InlineData("\ud83d\ude00", 4)]
    [InlineData("a\0b", 3)]
    public void StringLengthsUseUtf8Bytes(string value, int length)
        => Assert.Equal(length, VmExec.Run([VmCode.Rrr(VmOp.Lks, 1, 0, 0),
            VmCode.Rrr(VmOp.LenS, 1, 1, 0), VmCode.RetInt(1)], [], [], [value]).Value);

    [Fact]
    public void StringsValidateIndexesAndDefaultToEmpty()
    {
        Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(VmOp.Lks, 0, 0, 1)], [], [], []).Fault);
        Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Li(1, -1), VmCode.Rrr(VmOp.LksR, 0, 1, 0)], [], [], ["a"]).Fault);
        Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Lk(1, 0), VmCode.Rrr(VmOp.LksR, 0, 1, 255)], [int.MaxValue], [], ["a"]).Fault);
        Assert.Equal("", VmExec.Run([VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 0)], []).StringValue);
        var constants = Enumerable.Repeat("", 257).ToArray(); constants[256] = "wide";
        Assert.Equal("wide", VmExec.Run([VmCode.Rrr(VmOp.Lks, 0, 0, 1),
            VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 2, 0)], [], [], constants).StringValue);
    }

    [Fact]
    public void StringConstantsReturnWithNativeTypedFlag()
    {
        Assert.Equal("constant", VmExec.Run([VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 6, 0)], [], [], ["constant"]).StringValue);
        Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, 6, 0)], []).Fault);
    }

    [Theory]
    [InlineData(2, false)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(4, true)]
    public void VectorReturnsCopyNativeComponentCountFromRegistersOrConstants(int size, bool constant)
    {
        var code = new List<int>();
        for (var i = 0; i < size; i++) code.Add(VmCode.Rrr(VmOp.Lkf, 10 + i, i + 1, 0));
        code.Add(VmCode.RetVector(constant ? 1 : 10, size, constant));
        var result = VmExec.Run(code.ToArray(), [], [99, 1.5, -2.5, 3.5, -4.5]);
        Assert.Equal(new VmResult(true, 0, VmFault.None)
        {
            VectorValue = new VmVectorResult(size, 1.5, -2.5, size >= 3 ? 3.5 : 0, size == 4 ? -4.5 : 0),
        }, result);
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.RetVector(257 - size, size)], []).Fault);
        Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.RetVector(1, size, true)], [], new double[size]).Fault);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public void ScalarReturnsReadTypedConstants(int type)
    {
        var result = VmExec.Run([VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, type, 1)], [99, -7], [99, 2.5]);
        Assert.True(result.Returned);
        if (type == 4) Assert.Equal(-7, result.Value);
        else Assert.Equal(2.5, result.FloatValue);
        Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, type, 255)], []).Fault);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(33)]
    public void ReturnsRejectUnsupportedTypeFlags(int type)
        => Assert.Equal(VmFault.UnknownOpcode, VmExec.Run([VmCode.Rrr(VmOp.Ret, VmCode.RetFinal, type, 0)], []).Fault);

    [Fact]
    public void VectorReturnAcceptsLastCompleteRegisterRange()
    {
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkf, 255, 0, 0), VmCode.RetVector(252, 4)], [], [7]);
        Assert.Equal(new VmVectorResult(4, 0, 0, 0, 7), result.VectorValue);
        Assert.Throws<ArgumentOutOfRangeException>(() => VmCode.RetVector(0, 5));
    }

    [Theory]
    [InlineData(VmOp.MulQq, 10)]
    [InlineData(VmOp.MulQq, 11)]
    [InlineData(VmOp.MulQq, 20)]
    [InlineData(VmOp.MulQq, 21)]
    [InlineData(VmOp.MulQv3, 10)]
    [InlineData(VmOp.MulQv3, 11)]
    [InlineData(VmOp.MulQv3, 20)]
    [InlineData(VmOp.MulQv3, 21)]
    [InlineData(VmOp.ConjQ, 10)]
    [InlineData(VmOp.ConjQ, 11)]
    public void QuaternionOperationsPreserveLayoutAndOverlap(VmOp op, int destination)
    {
        var vector = op == VmOp.MulQv3;
        double[] constants = vector ? [0, 0, 1, 0, 1, 2, 3, 0] : [1, 2, 3, 4, 5, 6, 7, 8];
        double[] expected = vector ? [-1, -2, 3] : op == VmOp.ConjQ ? [-1, -2, -3, 4] : [24, 48, 48, -6];
        for (var component = 0; component < expected.Length; component++)
        {
            var code = new List<int>();
            for (var i = 0; i < 4; i++)
            {
                code.Add(VmCode.Rrr(VmOp.Lkf, 10 + i, i, 0));
                code.Add(VmCode.Rrr(VmOp.Lkf, 20 + i, i + 4, 0));
            }
            code.Add(VmCode.Rrr(op, destination, 10, 20));
            code.Add(VmCode.RetFloat(destination + component));
            Assert.Equal(expected[component], VmExec.Run(code.ToArray(), [], constants).FloatValue);
        }
    }

    [Theory]
    [InlineData(VmOp.MulQq)]
    [InlineData(VmOp.MulQv3)]
    [InlineData(VmOp.ConjQ)]
    public void QuaternionOperationsRejectInvalidRanges(VmOp op)
    {
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(op, 254, 0, 0)], []).Fault);
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(op, 0, 253, 0)], []).Fault);
        if (op != VmOp.ConjQ)
            Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(op, 0, 0, 254)], []).Fault);
    }

    [Fact]
    public void QuaternionVectorMultiplicationDoesNotNormalizeQuaternion()
    {
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkf, 13, 0, 0), VmCode.Rrr(VmOp.Lkf, 20, 1, 0),
            VmCode.Rrr(VmOp.MulQv3, 0, 10, 20), VmCode.RetFloat(0)], [], [2, 3]);
        Assert.Equal(12.0, result.FloatValue);
    }

    public static IEnumerable<object[]> VectorEqualityOperations()
    {
        yield return [VmOp.EqV2R, 2, false]; yield return [VmOp.EqV2K, 2, true];
        yield return [VmOp.EqV3R, 3, false]; yield return [VmOp.EqV3K, 3, true];
        yield return [VmOp.EqV4R, 4, false]; yield return [VmOp.EqV4K, 4, true];
    }

    [Theory]
    [MemberData(nameof(VectorEqualityOperations))]
    public void VectorEqualityChecksEveryComponentAndNativeEpsilon(VmOp op, int size, bool constant)
    {
        foreach (var component in Enumerable.Range(0, size))
        foreach (var difference in new[] { 0.0, 1.0 / 131072, 1.0 / 65536, double.NaN, double.PositiveInfinity })
        foreach (var approximate in new[] { false, true })
        foreach (var expect in new[] { 0, 1 })
        {
            var constants = new double[size * 2];
            constants[size + component] = difference;
            var code = new List<int>();
            for (var i = 0; i < size; i++)
            {
                code.Add(VmCode.Rrr(VmOp.Lkf, 10 + i, i, 0));
                code.Add(VmCode.Rrr(VmOp.Lkf, 20 + i, size + i, 0));
            }
            code.Add(VmCode.Cmp(op, expect | (approximate ? VmCode.CompareApprox : 0), 10, constant ? size : 20));
            code.Add(VmCode.Jmp(1)); code.Add(VmCode.Reti(10)); code.Add(VmCode.Reti(20));
            var equal = difference == 0 || approximate && difference == 1.0 / 131072;
            Assert.Equal(equal == (expect == 1) ? 20 : 10, VmExec.Run(code.ToArray(), [], constants).Value);
        }
    }

    [Theory]
    [MemberData(nameof(VectorEqualityOperations))]
    public void VectorEqualityValidatesRangesAndFollowingJump(VmOp op, int size, bool constant)
    {
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Cmp(op, 1, 257 - size, 0)], [], new double[size]).Fault);
        Assert.Equal(constant ? VmFault.ConstantRange : VmFault.RegisterRange,
            VmExec.Run([VmCode.Cmp(op, 1, 0, 257 - size)], [], []).Fault);
        Assert.Equal(VmFault.BadJump, VmExec.Run([VmCode.Cmp(op, 1, 0, 0), VmCode.Reti(10)], [], new double[size]).Fault);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(20)]
    [InlineData(21)]
    public void CrossProductCapturesAllComponentsBeforeOverlappingWrites(int destination)
    {
        for (var component = 0; component < 3; component++)
        {
            var code = new List<int>();
            for (var i = 0; i < 3; i++)
            {
                code.Add(VmCode.Rrr(VmOp.Lkf, 10 + i, i, 0));
                code.Add(VmCode.Rrr(VmOp.Lkf, 20 + i, i + 3, 0));
            }
            code.Add(VmCode.Rrr(VmOp.CrossV, destination, 10, 20));
            code.Add(VmCode.RetFloat(destination + component));
            double[] expected = [-3, 6, -3];
            Assert.Equal(expected[component], VmExec.Run(code.ToArray(), [], [1, 2, 3, 4, 5, 6]).FloatValue);
        }
    }

    [Fact]
    public void CrossProductRejectsAllInvalidVectorRanges()
    {
        foreach (var operands in new[] { (254, 0, 0), (0, 254, 0), (0, 0, 254) })
            Assert.Equal(VmFault.RegisterRange, VmExec.Run(
                [VmCode.Rrr(VmOp.CrossV, operands.Item1, operands.Item2, operands.Item3)], []).Fault);
    }

    public static IEnumerable<object[]> VectorScalingOperations()
    {
        yield return [VmOp.MulVf2Rr, 2, false, false]; yield return [VmOp.MulVf2Rk, 2, true, false];
        yield return [VmOp.DivVf2Rr, 2, false, true]; yield return [VmOp.DivVf2Rk, 2, true, true];
        yield return [VmOp.MulVf3Rr, 3, false, false]; yield return [VmOp.MulVf3Rk, 3, true, false];
        yield return [VmOp.DivVf3Rr, 3, false, true]; yield return [VmOp.DivVf3Rk, 3, true, true];
        yield return [VmOp.MulVf4Rr, 4, false, false]; yield return [VmOp.MulVf4Rk, 4, true, false];
        yield return [VmOp.DivVf4Rr, 4, false, true]; yield return [VmOp.DivVf4Rk, 4, true, true];
    }

    [Theory]
    [MemberData(nameof(VectorScalingOperations))]
    public void VectorScalingUsesTypedScalarAndNativeWriteOrder(VmOp op, int size, bool constant, bool divide)
    {
        for (var component = 0; component < size; component++)
        {
            var code = new List<int> { VmCode.Rrr(VmOp.Lkf, 1, 0, 0) };
            for (var i = 0; i < size; i++) code.Add(VmCode.Rrr(VmOp.Lkf, 10 + i, i + 1, 0));
            code.Add(VmCode.Rrr(op, 10, 10, constant ? 0 : 1));
            code.Add(VmCode.RetFloat(10 + component));
            var expected = divide ? (component + 1.5) / 2 : (component + 1.5) * 2;
            Assert.Equal(expected, VmExec.Run(code.ToArray(), [], [2, 1.5, 2.5, 3.5, 4.5]).FloatValue);
        }
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(op, 0, 257 - size, 0)], [], [2]).Fault);
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(op, 257 - size, 0, 0)], [], [2]).Fault);
        if (constant) Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(op, 0, 0, 255)], [], []).Fault);
    }

    [Theory]
    [InlineData(VmOp.DivVf2Rr, false)]
    [InlineData(VmOp.DivVf2Rk, true)]
    [InlineData(VmOp.DivVf3Rr, false)]
    [InlineData(VmOp.DivVf3Rk, true)]
    [InlineData(VmOp.DivVf4Rr, false)]
    [InlineData(VmOp.DivVf4Rk, true)]
    public void VectorDivisionByZeroPreservesNativeIeeeResults(VmOp op, bool constant)
    {
        foreach (var zero in new[] { 0.0, BitConverter.Int64BitsToDouble(long.MinValue) })
        foreach (var source in new[] { 1.0, 0.0 })
        {
            var result = VmExec.Run([VmCode.Rrr(VmOp.Lkf, 10, 0, 0), VmCode.Rrr(VmOp.Lkf, 5, 1, 0),
                VmCode.Rrr(op, 10, 10, constant ? 1 : 5), VmCode.RetFloat(10)], [], [source, zero]);
            Assert.Equal(VmFault.None, result.Fault);
            if (source == 0) Assert.True(double.IsNaN(result.FloatValue!.Value));
            else Assert.Equal(source / zero, result.FloatValue);
        }
    }

    [Theory]
    [InlineData(VmOp.MulVf2Rr)]
    [InlineData(VmOp.MulVf3Rr)]
    [InlineData(VmOp.MulVf4Rr)]
    public void VectorScalarIsCapturedBeforeOverlappingWrites(VmOp op)
        => Assert.Equal(12.0, VmExec.Run([VmCode.Rrr(VmOp.Lkf, 1, 0, 0), VmCode.Rrr(VmOp.Lkf, 2, 1, 0),
            VmCode.Rrr(op, 2, 1, 2), VmCode.RetFloat(3)], [], [3.0, 2.0]).FloatValue);

    [Theory]
    [InlineData(VmOp.LenV2, 2, 5.0)]
    [InlineData(VmOp.LenV3, 3, 13.0)]
    [InlineData(VmOp.LenV4, 4, 15.0)]
    public void VectorLengthsReadAllComponentsBeforeScalarWrite(VmOp op, int size, double expected)
    {
        var code = new List<int>();
        for (var i = 0; i < size; i++) code.Add(VmCode.Rrr(VmOp.Lkf, 10 + i, i, 0));
        code.Add(VmCode.Rrr(op, 10, 10, 0)); code.Add(VmCode.RetFloat(10));
        Assert.Equal(expected, VmExec.Run(code.ToArray(), [], [3, 4, 12, Math.Sqrt(56)]).FloatValue!.Value, 12);
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(op, 0, 257 - size, 0)], []).Fault);
        Assert.Equal(double.PositiveInfinity, VmExec.Run([VmCode.Rrr(VmOp.Lkf, 10, 0, 0),
            VmCode.Rrr(op, 0, 10, 0), VmCode.RetFloat(0)], [], [double.MaxValue]).FloatValue);
    }

    public static IEnumerable<object[]> VectorOperations()
    {
        yield return [VmOp.MoveV2, 2, 0]; yield return [VmOp.MoveV3, 3, 0]; yield return [VmOp.MoveV4, 4, 0];
        yield return [VmOp.NegV2, 2, 1]; yield return [VmOp.NegV3, 3, 1]; yield return [VmOp.NegV4, 4, 1];
        yield return [VmOp.AddV2, 2, 2]; yield return [VmOp.AddV3, 3, 2]; yield return [VmOp.AddV4, 4, 2];
        yield return [VmOp.SubV2, 2, 3]; yield return [VmOp.SubV3, 3, 3]; yield return [VmOp.SubV4, 4, 3];
        yield return [VmOp.DotV2, 2, 4]; yield return [VmOp.DotV3, 3, 4]; yield return [VmOp.DotV4, 4, 4];
    }

    [Theory]
    [MemberData(nameof(VectorOperations))]
    public void VectorOperationsUseConsecutiveDoubleRegisters(VmOp op, int size, int operation)
    {
        double[] left = [1.5, -2.0, 3.0, -4.0];
        double[] right = [2.0, 3.0, -4.0, 5.0];
        for (var resultComponent = 0; resultComponent < (operation == 4 ? 1 : size); resultComponent++)
        {
            var code = new List<int>();
            for (var component = 0; component < size; component++)
            {
                code.Add(VmCode.Rrr(VmOp.Lkf, 10 + component, component, 0));
                code.Add(VmCode.Rrr(VmOp.Lkf, 20 + component, 4 + component, 0));
            }
            code.Add(VmCode.Rrr(op, 10, 10, 20));
            code.Add(VmCode.RetFloat(10 + resultComponent));
            var expected = operation switch
            {
                0 => left[resultComponent], 1 => -left[resultComponent],
                2 => left[resultComponent] + right[resultComponent],
                3 => left[resultComponent] - right[resultComponent],
                _ => Enumerable.Range(0, size).Sum(i => left[i] * right[i]),
            };
            Assert.Equal(expected, VmExec.Run(code.ToArray(), [], left.Concat(right).ToArray()).FloatValue);
        }
    }

    [Theory]
    [MemberData(nameof(VectorOperations))]
    public void VectorOperationsRejectRegisterRangesPastBank(VmOp op, int size, int operation)
    {
        Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(op, 0, 257 - size, 0)], []).Fault);
        if (operation != 4)
            Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(op, 257 - size, 0, 0)], []).Fault);
        if (operation >= 2)
            Assert.Equal(VmFault.RegisterRange, VmExec.Run([VmCode.Rrr(op, 0, 0, 257 - size)], []).Fault);
    }

    [Theory]
    [InlineData(VmOp.MoveV2, 1.0)]
    [InlineData(VmOp.NegV2, 1.0)]
    [InlineData(VmOp.AddV2, 21.0)]
    public void OverlappingVectorWritesFollowNativeComponentOrder(VmOp op, double expected)
        => Assert.Equal(expected, VmExec.Run([VmCode.Rrr(VmOp.Lkf, 1, 0, 0), VmCode.Rrr(VmOp.Lkf, 2, 1, 0),
            VmCode.Rrr(VmOp.Lkf, 10, 2, 0), VmCode.Rrr(VmOp.Lkf, 11, 3, 0),
            VmCode.Rrr(op, 2, 1, 10), VmCode.RetFloat(3)], [], [1.0, 2.0, 10.0, 10.0]).FloatValue);

    [Theory]
    [InlineData(VmCast.IntToFloat, -1, -1.0)]
    [InlineData(VmCast.IntToFloat, int.MinValue, -2147483648.0)]
    [InlineData(VmCast.UnsignedToFloat, -1, 4294967295.0)]
    [InlineData(VmCast.UnsignedToFloat, int.MinValue, 2147483648.0)]
    public void IntegerToFloatCastsPreserveSignednessAndSeparateBanks(VmCast cast, int value, double expected)
    {
        var code = new[] { VmCode.Lk(1, 0), VmCode.Rrr(VmOp.Cast, 1, 1, (int)cast), VmCode.RetFloat(1) };
        Assert.Equal(expected, VmExec.Run(code, [value]).FloatValue);
        code[2] = VmCode.RetInt(1);
        Assert.Equal(value, VmExec.Run(code, [value]).Value);
    }

    [Theory]
    [InlineData(VmCast.FloatToInt, -3.9, -3)]
    [InlineData(VmCast.FloatToInt, 3.9, 3)]
    [InlineData(VmCast.FloatToInt, -2147483648.9, int.MinValue)]
    [InlineData(VmCast.FloatToInt, 2147483647.9, int.MaxValue)]
    [InlineData(VmCast.FloatToUnsigned, -0.9, 0)]
    [InlineData(VmCast.FloatToUnsigned, 2147483648.0, int.MinValue)]
    [InlineData(VmCast.FloatToUnsigned, 4294967295.9, -1)]
    public void FloatToIntegerCastsTruncateAndPreserveUnsignedBits(VmCast cast, double value, int expected)
        => Assert.Equal(new VmResult(true, expected, VmFault.None), VmExec.Run(
            [VmCode.Rrr(VmOp.Lkf, 1, 0, 0), VmCode.Rrr(VmOp.Cast, 1, 1, (int)cast), VmCode.RetInt(1)], [], [value]));

    [Theory]
    [InlineData(VmCast.FloatToInt, double.NaN)]
    [InlineData(VmCast.FloatToInt, double.PositiveInfinity)]
    [InlineData(VmCast.FloatToInt, double.NegativeInfinity)]
    [InlineData(VmCast.FloatToInt, 2147483648.0)]
    [InlineData(VmCast.FloatToInt, -2147483649.0)]
    [InlineData(VmCast.FloatToUnsigned, double.NaN)]
    [InlineData(VmCast.FloatToUnsigned, double.PositiveInfinity)]
    [InlineData(VmCast.FloatToUnsigned, -1.0)]
    [InlineData(VmCast.FloatToUnsigned, 4294967296.0)]
    public void NonPortableFloatToIntegerCastsReportOverflow(VmCast cast, double value)
        => Assert.Equal(VmFault.Overflow, VmExec.Run(
            [VmCode.Rrr(VmOp.Lkf, 1, 0, 0), VmCode.Rrr(VmOp.Cast, 1, 1, (int)cast)], [], [value]).Fault);

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 1)]
    [InlineData(int.MinValue, 1)]
    public void IntegerBooleanCastsNormalizeInPlace(int value, int expected)
        => Assert.Equal(expected, VmExec.Run([VmCode.Lk(1, 0),
            VmCode.Rrr(VmOp.CastB, 1, 1, (int)VmCast.IntToBool), VmCode.RetInt(1)], [value]).Value);

    [Theory]
    [InlineData(0.0, 0)]
    [InlineData(-0.5, 1)]
    [InlineData(double.NaN, 1)]
    [InlineData(double.PositiveInfinity, 1)]
    public void FloatingBooleanCastsUseNativeNonzeroTest(double value, int expected)
    {
        foreach (var input in value == 0.0 ? new[] { 0.0, BitConverter.Int64BitsToDouble(long.MinValue) } : new[] { value })
            Assert.Equal(expected, VmExec.Run([VmCode.Rrr(VmOp.Lkf, 1, 0, 0),
                VmCode.Rrr(VmOp.CastB, 1, 1, (int)VmCast.FloatToBool), VmCode.RetInt(1)], [], [input]).Value);
    }

    [Theory]
    [InlineData(VmOp.Cast, 21)]
    [InlineData(VmOp.Cast, 255)]
    [InlineData(VmOp.CastB, 0)]
    [InlineData(VmOp.CastB, 255)]
    public void CastInstructionsRejectUnsupportedSelectors(VmOp op, int selector)
        => Assert.Equal(VmFault.InvalidCast, VmExec.Run([VmCode.Rrr(op, 0, 0, selector)], []).Fault);

    [Theory]
    [InlineData(VmFloatFunction.Abs, -2.5, 2.5)]
    [InlineData(VmFloatFunction.Neg, -2.5, 2.5)]
    [InlineData(VmFloatFunction.Exp, 0, 1)]
    [InlineData(VmFloatFunction.Log, 1, 0)]
    [InlineData(VmFloatFunction.Log10, 100, 2)]
    [InlineData(VmFloatFunction.Sqrt, 4, 2)]
    [InlineData(VmFloatFunction.Ceil, -2.5, -2)]
    [InlineData(VmFloatFunction.Floor, -2.5, -3)]
    [InlineData(VmFloatFunction.Acos, 1, 0)]
    [InlineData(VmFloatFunction.Asin, 0, 0)]
    [InlineData(VmFloatFunction.Atan, 1, Math.PI / 4)]
    [InlineData(VmFloatFunction.Cos, Math.PI, -1)]
    [InlineData(VmFloatFunction.Sin, Math.PI / 2, 1)]
    [InlineData(VmFloatFunction.Tan, Math.PI / 4, 1)]
    [InlineData(VmFloatFunction.AcosDegrees, 0, 90)]
    [InlineData(VmFloatFunction.AsinDegrees, 1, 90)]
    [InlineData(VmFloatFunction.AtanDegrees, 1, 45)]
    [InlineData(VmFloatFunction.CosDegrees, 180, -1)]
    [InlineData(VmFloatFunction.SinDegrees, 90, 1)]
    [InlineData(VmFloatFunction.TanDegrees, 45, 1)]
    [InlineData(VmFloatFunction.Cosh, 0, 1)]
    [InlineData(VmFloatFunction.Sinh, 0, 0)]
    [InlineData(VmFloatFunction.Tanh, 0, 0)]
    [InlineData(VmFloatFunction.Round, 2.5, 3)]
    [InlineData(VmFloatFunction.Round, -2.5, -3)]
    public void FloatingFunctionsUseNativeSelectorsUnitsAndAliasing(VmFloatFunction function, double input, double expected)
    {
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkf, 1, 0, 0),
            VmCode.Rrr(VmOp.Flop, 1, 1, (int)function), VmCode.RetFloat(1)], [], [input]);
        Assert.True(result.Returned);
        Assert.Equal(expected, result.FloatValue!.Value, 12);
    }

    [Theory]
    [InlineData(VmFloatFunction.Sqrt, -1)]
    [InlineData(VmFloatFunction.Log, -1)]
    [InlineData(VmFloatFunction.Acos, 2)]
    public void FloatingFunctionDomainsPreserveNan(VmFloatFunction function, double input)
    {
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkf, 1, 0, 0),
            VmCode.Rrr(VmOp.Flop, 1, 1, (int)function), VmCode.RetFloat(1)], [], [input]);
        Assert.Equal(VmFault.None, result.Fault);
        Assert.True(double.IsNaN(result.FloatValue!.Value));
    }

    [Theory]
    [InlineData(24)]
    [InlineData(255)]
    public void FloatingFunctionsRejectInvalidSelectors(int selector)
        => Assert.Equal(VmFault.InvalidFloatFunction, VmExec.Run([VmCode.Rrr(VmOp.Flop, 0, 0, selector)], [], []).Fault);

    [Theory]
    [InlineData(1, 1, 45)]
    [InlineData(1, -1, 135)]
    [InlineData(-1, -1, -135)]
    [InlineData(-1, 1, -45)]
    public void Atan2ReturnsDegreesAndPreservesQuadrants(double y, double x, double expected)
    {
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkf, 1, 0, 0), VmCode.Rrr(VmOp.Lkf, 2, 1, 0),
            VmCode.Rrr(VmOp.Atan2, 1, 1, 2), VmCode.RetFloat(1)], [], [y, x]);
        Assert.Equal(expected, result.FloatValue!.Value, 12);
    }

    [Theory]
    [InlineData(VmOp.EqfR)]
    [InlineData(VmOp.EqfK)]
    [InlineData(VmOp.LtfRr)]
    [InlineData(VmOp.LtfRk)]
    [InlineData(VmOp.LtfKr)]
    [InlineData(VmOp.LefRr)]
    [InlineData(VmOp.LefRk)]
    [InlineData(VmOp.LefKr)]
    public void FloatingComparisonsHonorExactApproximateBoundariesAndBranchPolarity(VmOp op)
    {
        const double epsilon = 1.0 / 65536.0;
        var equality = op is VmOp.EqfR or VmOp.EqfK;
        var less = op is VmOp.LtfRr or VmOp.LtfRk or VmOp.LtfKr;
        var cases = new (double Left, double Right, bool Exact, bool Approximate)[]
        {
            (0, 0, !less, equality),
            (0, epsilon / 2, !equality, equality),
            (0, epsilon, !equality, !equality && !less),
            (0, epsilon * 2, !equality, !equality),
            (epsilon * 2, 0, false, false),
            (double.NaN, 0, false, false),
            (0, double.NaN, false, false),
            (double.PositiveInfinity, double.PositiveInfinity, !less, false),
            (double.NegativeInfinity, double.PositiveInfinity, !equality, !equality),
            (0, BitConverter.Int64BitsToDouble(long.MinValue), !less, equality),
        };
        foreach (var item in cases)
        foreach (var approximate in new[] { false, true })
        foreach (var expect in new[] { 0, 1 })
        {
            var constantLeft = op is VmOp.LtfKr or VmOp.LefKr;
            var constantRight = op is VmOp.EqfK or VmOp.LtfRk or VmOp.LefRk;
            var code = new[] { VmCode.Rrr(VmOp.Lkf, 1, 0, 0), VmCode.Rrr(VmOp.Lkf, 2, 1, 0),
                VmCode.Cmp(op, expect | (approximate ? VmCode.CompareApprox : 0), constantLeft ? 0 : 1, constantRight ? 1 : 2),
                VmCode.Jmp(1), VmCode.Reti(10), VmCode.Reti(20) };
            var truth = approximate ? item.Approximate : item.Exact;
            Assert.Equal(new VmResult(true, truth == (expect == 1) ? 20 : 10, VmFault.None),
                VmExec.Run(code, [], [item.Left, item.Right]));
        }
    }

    [Theory]
    [InlineData(VmOp.EqfK)]
    [InlineData(VmOp.LtfRk)]
    [InlineData(VmOp.LtfKr)]
    [InlineData(VmOp.LefRk)]
    [InlineData(VmOp.LefKr)]
    public void FloatingComparisonsRejectMissingTypedConstants(VmOp op)
        => Assert.Equal(VmFault.ConstantRange, VmExec.Run(
            [VmCode.Cmp(op, 1, 255, 255), VmCode.Jmp(0)], new int[256], []).Fault);

    [Theory]
    [InlineData(VmOp.EqfR, 1)]
    [InlineData(VmOp.EqfK, 1)]
    [InlineData(VmOp.LtfRr, 0)]
    [InlineData(VmOp.LtfRk, 0)]
    [InlineData(VmOp.LtfKr, 0)]
    [InlineData(VmOp.LefRr, 1)]
    [InlineData(VmOp.LefRk, 1)]
    [InlineData(VmOp.LefKr, 1)]
    public void FloatingComparisonTakenBranchesRequireJump(VmOp op, int expect)
        => Assert.Equal(VmFault.BadJump, VmExec.Run(
            [VmCode.Cmp(op, expect, 0, 0), VmCode.Reti(10)], [], [0.0]).Fault);

    [Theory]
    [InlineData(VmOp.ModfRr, -5.5, 2.0, 0.5)]
    [InlineData(VmOp.ModfRk, 5.5, -2.0, -0.5)]
    [InlineData(VmOp.ModfKr, -5.5, -2.0, -1.5)]
    [InlineData(VmOp.PowfRr, 2.0, 3.0, 8.0)]
    [InlineData(VmOp.PowfRk, 4.0, 0.5, 2.0)]
    [InlineData(VmOp.PowfKr, 2.0, -2.0, 0.25)]
    [InlineData(VmOp.MinfRr, -2.5, 3.0, -2.5)]
    [InlineData(VmOp.MinfRk, 3.0, -2.5, -2.5)]
    [InlineData(VmOp.MaxfRr, -2.5, 3.0, 3.0)]
    [InlineData(VmOp.MaxfRk, 3.0, -2.5, 3.0)]
    public void FloatingExtendedArithmeticPreservesNativeOperandSemantics(VmOp op, double left, double right, double expected)
        => Assert.Equal(expected, RunFloatingBinary(op, left, right).FloatValue);

    private static VmResult RunFloatingBinary(VmOp op, double left, double right)
    {
        var constantLeft = op is VmOp.ModfKr or VmOp.PowfKr;
        var constantRight = op is VmOp.ModfRk or VmOp.PowfRk or VmOp.MinfRk or VmOp.MaxfRk;
        return VmExec.Run([VmCode.Rrr(VmOp.Lkf, 1, 0, 0), VmCode.Rrr(VmOp.Lkf, 2, 1, 0),
            VmCode.Rrr(op, 1, constantLeft ? 0 : 1, constantRight ? 1 : 2), VmCode.RetFloat(1)], [], [left, right]);
    }

    [Theory]
    [InlineData(VmOp.ModfRr)]
    [InlineData(VmOp.ModfRk)]
    [InlineData(VmOp.ModfKr)]
    public void FloatingModuloRejectsBothZeroSigns(VmOp op)
    {
        foreach (var zero in new[] { 0.0, BitConverter.Int64BitsToDouble(long.MinValue) })
            Assert.Equal(VmFault.DivisionByZero, RunFloatingBinary(op, 5.5, zero).Fault);
    }

    [Theory]
    [InlineData(VmOp.ModfRk)]
    [InlineData(VmOp.ModfKr)]
    [InlineData(VmOp.PowfRk)]
    [InlineData(VmOp.PowfKr)]
    [InlineData(VmOp.MinfRk)]
    [InlineData(VmOp.MaxfRk)]
    public void ExtendedFloatingArithmeticChecksTypedConstants(VmOp op)
        => Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(op, 0, 255, 255)], new int[256], []).Fault);

    [Theory]
    [InlineData(VmOp.MinfRr)]
    [InlineData(VmOp.MinfRk)]
    [InlineData(VmOp.MaxfRr)]
    [InlineData(VmOp.MaxfRk)]
    public void FloatingMinMaxSelectRightOperandForNanAndSignedZeroTies(VmOp op)
    {
        Assert.Equal(3.0, RunFloatingBinary(op, double.NaN, 3.0).FloatValue);
        Assert.True(double.IsNaN(RunFloatingBinary(op, 3.0, double.NaN).FloatValue!.Value));
        var negativeZero = BitConverter.Int64BitsToDouble(long.MinValue);
        Assert.Equal(long.MinValue, BitConverter.DoubleToInt64Bits(RunFloatingBinary(op, 0.0, negativeZero).FloatValue!.Value));
        Assert.Equal(0L, BitConverter.DoubleToInt64Bits(RunFloatingBinary(op, negativeZero, 0.0).FloatValue!.Value));
    }

    [Fact]
    public void FloatingPowerAndModuloPreserveNonFiniteResults()
    {
        Assert.True(double.IsNaN(RunFloatingBinary(VmOp.PowfRr, -1.0, 0.5).FloatValue!.Value));
        Assert.Equal(double.PositiveInfinity, RunFloatingBinary(VmOp.PowfRk, 0.0, -1.0).FloatValue);
        Assert.True(double.IsNaN(RunFloatingBinary(VmOp.ModfKr, double.PositiveInfinity, 2.0).FloatValue!.Value));
        Assert.True(double.IsNaN(RunFloatingBinary(VmOp.ModfRr, 2.0, double.PositiveInfinity).FloatValue!.Value));
    }

    [Theory]
    [InlineData(VmOp.AddfRr, 5.5)]
    [InlineData(VmOp.AddfRk, 5.5)]
    [InlineData(VmOp.SubfRr, 1.5)]
    [InlineData(VmOp.SubfRk, 1.5)]
    [InlineData(VmOp.SubfKr, 1.5)]
    [InlineData(VmOp.MulfRr, 7.0)]
    [InlineData(VmOp.MulfRk, 7.0)]
    [InlineData(VmOp.DivfRr, 1.75)]
    [InlineData(VmOp.DivfRk, 1.75)]
    [InlineData(VmOp.DivfKr, 1.75)]
    public void FloatingArithmeticPreservesFractionsOperandOrderAndAliasing(VmOp op, double expected)
    {
        var constantLeft = op is VmOp.SubfKr or VmOp.DivfKr;
        var constantRight = op is VmOp.AddfRk or VmOp.SubfRk or VmOp.MulfRk or VmOp.DivfRk;
        var result = VmExec.Run([VmCode.Rrr(VmOp.Lkf, 1, 0, 0), VmCode.Rrr(VmOp.Lkf, 2, 1, 0),
            VmCode.Rrr(op, 1, constantLeft ? 0 : 1, constantRight ? 1 : 2), VmCode.RetFloat(1)], [], [3.5, 2.0]);
        Assert.Equal(new VmResult(true, 0, VmFault.None) { FloatValue = expected }, result);
    }

    [Theory]
    [InlineData(VmOp.DivfRr, false)]
    [InlineData(VmOp.DivfRk, false)]
    [InlineData(VmOp.DivfKr, false)]
    [InlineData(VmOp.DivfRr, true)]
    [InlineData(VmOp.DivfRk, true)]
    [InlineData(VmOp.DivfKr, true)]
    public void FloatingDivisionRejectsEitherZeroSign(VmOp op, bool negative)
        => Assert.Equal(VmFault.DivisionByZero, VmExec.Run(
            [VmCode.Rrr(VmOp.Lkf, 2, 1, 0), VmCode.Rrr(op, 1, 0, op == VmOp.DivfRk ? 1 : 2)], [], [1.0, negative ? BitConverter.Int64BitsToDouble(long.MinValue) : 0.0]).Fault);

    [Theory]
    [InlineData(VmOp.AddfRk)]
    [InlineData(VmOp.SubfRk)]
    [InlineData(VmOp.SubfKr)]
    [InlineData(VmOp.MulfRk)]
    [InlineData(VmOp.DivfRk)]
    [InlineData(VmOp.DivfKr)]
    public void FloatingArithmeticRejectsMissingTypedConstants(VmOp op)
        => Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(op, 0, 255, 255)], new int[256], []).Fault);

    [Fact]
    public void FloatingLoadsMovesAndReturnsUseSeparateRegisterBank()
    {
        var constants = new double[257];
        constants[256] = -3.25;
        var result = VmExec.Run([VmCode.Li(1, 256), VmCode.Rrr(VmOp.Lkf, 1, 0, 1),
            VmCode.Rrr(VmOp.LkfR, 2, 1, 0), VmCode.Rrr(VmOp.MoveF, 3, 2, 0), VmCode.RetFloat(3)], [], constants);
        Assert.Equal(-3.25, result.FloatValue);
        Assert.Equal(VmFault.ConstantRange, VmExec.Run([VmCode.Rrr(VmOp.Lkf, 0, 0, 1)], [], []).Fault);
        Assert.Equal(VmFault.ConstantRange, VmExec.Run(
            [VmCode.Li(1, -1), VmCode.Rrr(VmOp.LkfR, 0, 1, 0)], [], constants).Fault);
        Assert.Equal(VmFault.ConstantRange, VmExec.Run(
            [VmCode.Lk(1, 0), VmCode.Rrr(VmOp.LkfR, 0, 1, 255)], [int.MaxValue], constants).Fault);
    }

    [Fact]
    public void FloatingArithmeticRetainsIeeeNonFiniteResults()
    {
        var overflow = VmExec.Run([VmCode.Rrr(VmOp.Lkf, 1, 0, 0),
            VmCode.Rrr(VmOp.MulfRr, 1, 1, 1), VmCode.RetFloat(1)], [], [double.MaxValue]);
        Assert.Equal(double.PositiveInfinity, overflow.FloatValue);
        var nan = VmExec.Run([VmCode.Rrr(VmOp.Lkf, 1, 0, 0),
            VmCode.Rrr(VmOp.AddfRr, 1, 1, 1), VmCode.RetFloat(1)], [], [double.NaN]);
        Assert.True(double.IsNaN(nan.FloatValue!.Value));
        Assert.Equal(9, VmExec.Run([VmCode.Li(1, 9), VmCode.Rrr(VmOp.Lkf, 1, 0, 0), VmCode.RetInt(1)], [], [2.5]).Value);
    }

    [Theory]
    [InlineData(0, 1, 22)]
    [InlineData(1, 0, 22)]
    [InlineData(2, 0, 33)]
    public void IndexedConstantsReadBeforeDestinationAliasing(int index, int offset, int expected)
        => Assert.Equal(new VmResult(true, expected, VmFault.None), VmExec.Run(
            [VmCode.Li(1, (short)index), VmCode.Rrr(VmOp.LkR, 1, 1, offset), VmCode.RetInt(1)], [11, 22, 33]));

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(3, 0)]
    [InlineData(int.MaxValue, 255)]
    public void IndexedConstantsRejectInvalidIndexes(int index, int offset)
        => Assert.Equal(new VmResult(false, 0, VmFault.ConstantRange), VmExec.Run(
            [VmCode.Lk(1, 0), VmCode.Rrr(VmOp.LkR, 1, 1, offset)], [index]));

    [Theory]
    [InlineData(-65535, 65535, 10)]
    [InlineData(-1, 1, 10)]
    [InlineData(0, 0, 10)]
    [InlineData(1, 1, 20)]
    [InlineData(int.MinValue, 0, 20)]
    public void NegativeTestsCompareUnsignedImmediateMagnitude(int value, int magnitude, int expected)
        => Assert.Equal(new VmResult(true, expected, VmFault.None), VmExec.Run(
            [VmCode.Lk(1, 0), VmCode.Rrr(VmOp.TestN, 1, magnitude & 255, magnitude >> 8),
                VmCode.Reti(10), VmCode.Reti(20)], [value]));

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 20)]
    public void IndexedJumpUsesNativeRegisterOffsetAndIgnoresBc(int index, int expected)
        => Assert.Equal(new VmResult(true, expected, VmFault.None), VmExec.Run(
            [VmCode.Li(1, (short)index), VmCode.Rrr(VmOp.IJmp, 1, 255, 255),
                VmCode.Jmp(1), VmCode.Jmp(1), VmCode.Reti(10), VmCode.Reti(20)], []));

    [Theory]
    [InlineData(-3)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void IndexedJumpRejectsMissingOrNonJumpTableEntries(int index)
        => Assert.Equal(new VmResult(false, 0, VmFault.BadJump), VmExec.Run(
            [VmCode.Lk(1, 0), VmCode.Rrr(VmOp.IJmp, 1, 0, 0), VmCode.Jmp(0), VmCode.Reti(10)], [index]));

    [Theory]
    [InlineData(VmOp.Bound)]
    [InlineData(VmOp.BoundK)]
    [InlineData(VmOp.BoundR)]
    public void BoundsCheckAllFormsAtBothEdges(VmOp op)
    {
        foreach (var index in new[] { -1, 0, 2, 3 })
        {
            var check = op == VmOp.BoundR ? VmCode.Rrr(op, 1, 2, 0)
                : VmCode.Rrr(op, 1, op == VmOp.BoundK ? 0 : 3, 0);
            var result = VmExec.Run([VmCode.Li(1, (short)index), VmCode.Li(2, 3), check, VmCode.RetInt(1)], [3]);
            Assert.Equal(index is 0 or 2 ? new VmResult(true, index, VmFault.None)
                : new VmResult(false, 0, VmFault.ArrayBounds), result);
        }
    }

    [Fact]
    public void ConstantBoundUsesFullSixteenBitIndex()
    {
        var constants = new int[257];
        constants[256] = 1;
        Assert.Equal(new VmResult(true, 0, VmFault.None), VmExec.Run(
            [VmCode.Rrr(VmOp.BoundK, 0, 0, 1), VmCode.RetInt(0)], constants));
        Assert.Equal(new VmResult(false, 0, VmFault.ConstantRange), VmExec.Run(
            [VmCode.Rrr(VmOp.BoundK, 0, 0, 1)], []));
    }

    [Theory]
    [InlineData(VmOp.EqK, -1, -1, true)]
    [InlineData(VmOp.LtRk, -1, 1, true)]
    [InlineData(VmOp.LtKr, 1, -1, false)]
    [InlineData(VmOp.LeRk, -1, -1, true)]
    [InlineData(VmOp.LeKr, 1, -1, false)]
    [InlineData(VmOp.LtuRr, -1, 1, false)]
    [InlineData(VmOp.LtuRk, -1, 1, false)]
    [InlineData(VmOp.LtuKr, -1, 1, false)]
    [InlineData(VmOp.LeuRr, -1, -1, true)]
    [InlineData(VmOp.LeuRk, -1, -1, true)]
    [InlineData(VmOp.LeuKr, -1, -1, true)]
    public void ComparisonVariantsHonorOperandsSignednessAndExpectedPolarity(VmOp op, int left, int right, bool truth)
    {
        var constantLeft = op is VmOp.LtKr or VmOp.LeKr or VmOp.LtuKr or VmOp.LeuKr;
        foreach (var expect in new[] { 0, 1 })
        {
            var code = new[] { VmCode.Lk(1, 0), VmCode.Lk(2, 1),
                VmCode.Cmp(op, expect, constantLeft ? 0 : 1, 2), VmCode.Jmp(1),
                VmCode.Reti(10), VmCode.Reti(20) };
            Assert.Equal(new VmResult(true, truth == (expect == 1) ? 20 : 10, VmFault.None),
                VmExec.Run(code, new[] { left, right, right }));
        }
    }

    [Theory]
    [InlineData(VmOp.EqK)]
    [InlineData(VmOp.LtRk)]
    [InlineData(VmOp.LtKr)]
    [InlineData(VmOp.LeRk)]
    [InlineData(VmOp.LeKr)]
    [InlineData(VmOp.LtuRk)]
    [InlineData(VmOp.LtuKr)]
    [InlineData(VmOp.LeuRk)]
    [InlineData(VmOp.LeuKr)]
    public void ConstantComparisonsRejectMissingConstants(VmOp op)
        => Assert.Equal(new VmResult(false, 0, VmFault.ConstantRange),
            VmExec.Run(new[] { VmCode.Cmp(op, 1, 255, 255), VmCode.Jmp(0) }, []));

    [Theory]
    [InlineData(VmOp.EqK, 1)]
    [InlineData(VmOp.LtRk, 0)]
    [InlineData(VmOp.LtKr, 0)]
    [InlineData(VmOp.LeRk, 1)]
    [InlineData(VmOp.LeKr, 1)]
    [InlineData(VmOp.LtuRr, 0)]
    [InlineData(VmOp.LtuRk, 0)]
    [InlineData(VmOp.LtuKr, 0)]
    [InlineData(VmOp.LeuRr, 1)]
    [InlineData(VmOp.LeuRk, 1)]
    [InlineData(VmOp.LeuKr, 1)]
    public void ComparisonVariantsRequireJumpWhenBranchIsTaken(VmOp op, int expect)
        => Assert.Equal(new VmResult(false, 0, VmFault.BadJump),
            VmExec.Run(new[] { VmCode.Cmp(op, expect, 0, 0), VmCode.Reti(5) }, new[] { 0 }));

    [Theory]
    [InlineData(VmOp.SllRi, 1, -2)]
    [InlineData(VmOp.SllKr, 1, -2)]
    [InlineData(VmOp.SrlRr, 1, int.MaxValue)]
    [InlineData(VmOp.SrlKr, 1, int.MaxValue)]
    [InlineData(VmOp.SraRr, 1, -1)]
    [InlineData(VmOp.SraKr, 1, -1)]
    [InlineData(VmOp.SllRi, 31, int.MinValue)]
    [InlineData(VmOp.SllKr, 31, int.MinValue)]
    [InlineData(VmOp.SrlRr, 31, 1)]
    [InlineData(VmOp.SrlKr, 31, 1)]
    [InlineData(VmOp.SraRr, 31, -1)]
    [InlineData(VmOp.SraKr, 31, -1)]
    public void ShiftVariantsPreserveSignednessAndBoundaryCounts(VmOp op, int count, int expected)
    {
        var constantSource = op is VmOp.SllKr or VmOp.SrlKr or VmOp.SraKr;
        var code = new[] { VmCode.Lk(1, 0), VmCode.Li(2, (short)count),
            VmCode.Rrr(op, 1, constantSource ? 0 : 1, op == VmOp.SllRi ? count : 2), VmCode.RetInt(1) };
        Assert.Equal(new VmResult(true, expected, VmFault.None), VmExec.Run(code, new[] { -1 }));
    }

    [Theory]
    [InlineData(VmOp.SllRi)]
    [InlineData(VmOp.SllKr)]
    [InlineData(VmOp.SrlRr)]
    [InlineData(VmOp.SrlKr)]
    [InlineData(VmOp.SraRr)]
    [InlineData(VmOp.SraKr)]
    public void ShiftVariantsRejectOutOfRangeCounts(VmOp op)
    {
        var code = new[] { VmCode.Li(2, -1), VmCode.Rrr(op, 0, 0, op == VmOp.SllRi ? 32 : 2) };
        Assert.Equal(new VmResult(false, 0, VmFault.ShiftRange), VmExec.Run(code, new[] { 1 }));
    }

    [Theory]
    [InlineData(VmOp.SllKr)]
    [InlineData(VmOp.SrlKr)]
    [InlineData(VmOp.SraKr)]
    public void ConstantSourceShiftsRejectMissingConstants(VmOp op)
        => Assert.Equal(new VmResult(false, 0, VmFault.ConstantRange),
            VmExec.Run(new[] { VmCode.Rrr(op, 0, 255, 0) }, []));

    [Theory]
    [InlineData(VmOp.AndRk, -1, 15, 15)]
    [InlineData(VmOp.OrRk, int.MinValue, 15, -2147483633)]
    [InlineData(VmOp.XorRk, -1, 15, -16)]
    [InlineData(VmOp.MinRr, -1, 1, -1)]
    [InlineData(VmOp.MinRk, -1, 1, -1)]
    [InlineData(VmOp.MaxRr, -1, 1, 1)]
    [InlineData(VmOp.MaxRk, -1, 1, 1)]
    [InlineData(VmOp.MinuRr, -1, 1, 1)]
    [InlineData(VmOp.MinuRk, -1, 1, 1)]
    [InlineData(VmOp.MaxuRr, -1, 1, -1)]
    [InlineData(VmOp.MaxuRk, -1, 1, -1)]
    public void BitwiseAndMinMaxUseNativeBitPatternsAndSignedness(VmOp op, int left, int right, int expected)
    {
        var code = new[] { VmCode.Lk(1, 0), VmCode.Lk(2, 1), VmCode.Rrr(op, 1, 1, 2), VmCode.RetInt(1) };
        Assert.Equal(new VmResult(true, expected, VmFault.None), VmExec.Run(code, new[] { left, right, right }));
    }

    [Theory]
    [InlineData(VmOp.AndRk)]
    [InlineData(VmOp.OrRk)]
    [InlineData(VmOp.XorRk)]
    [InlineData(VmOp.MinRk)]
    [InlineData(VmOp.MaxRk)]
    [InlineData(VmOp.MinuRk)]
    [InlineData(VmOp.MaxuRk)]
    public void BitwiseAndMinMaxRejectInvalidConstants(VmOp op)
        => Assert.Equal(new VmResult(false, 0, VmFault.ConstantRange),
            VmExec.Run(new[] { VmCode.Rrr(op, 0, 1, 255) }, []));

    [Theory]
    [InlineData(VmOp.DivRk)]
    [InlineData(VmOp.DivKr)]
    public void SignedConstantDivisionReportsUnrepresentableQuotient(VmOp op)
    {
        var leftConstant = op == VmOp.DivKr;
        var code = new[] { VmCode.Lk(1, 0), VmCode.Lk(2, 1),
            VmCode.Rrr(op, 1, leftConstant ? 0 : 1, 2) };
        Assert.Equal(new VmResult(false, 0, VmFault.Overflow),
            VmExec.Run(code, new[] { int.MinValue, -1, -1 }));
    }

    [Theory]
    [InlineData(VmOp.DivRk, -7, 3, -2)]
    [InlineData(VmOp.DivKr, -7, 3, -2)]
    [InlineData(VmOp.ModRk, -7, 3, -1)]
    [InlineData(VmOp.ModKr, -7, 3, -1)]
    [InlineData(VmOp.DivuRr, -1, 2, int.MaxValue)]
    [InlineData(VmOp.DivuRk, -1, 2, int.MaxValue)]
    [InlineData(VmOp.DivuKr, -1, 2, int.MaxValue)]
    [InlineData(VmOp.ModuRr, -1, 2, 1)]
    [InlineData(VmOp.ModuRk, -1, 2, 1)]
    [InlineData(VmOp.ModuKr, -1, 2, 1)]
    public void DivisionVariantsUseNativeSignednessAndOperandOrder(VmOp op, int numerator, int denominator, int expected)
    {
        var leftConstant = op is VmOp.DivKr or VmOp.DivuKr or VmOp.ModKr or VmOp.ModuKr;
        var code = new[] { VmCode.Lk(1, 0), VmCode.Lk(2, 1),
            VmCode.Rrr(op, 1, leftConstant ? 0 : 1, 2), VmCode.RetInt(1) };
        Assert.Equal(new VmResult(true, expected, VmFault.None),
            VmExec.Run(code, new[] { numerator, denominator, denominator }));
    }

    [Theory]
    [InlineData(VmOp.DivRk)]
    [InlineData(VmOp.DivKr)]
    [InlineData(VmOp.DivuRr)]
    [InlineData(VmOp.DivuRk)]
    [InlineData(VmOp.DivuKr)]
    [InlineData(VmOp.ModRk)]
    [InlineData(VmOp.ModKr)]
    [InlineData(VmOp.ModuRr)]
    [InlineData(VmOp.ModuRk)]
    [InlineData(VmOp.ModuKr)]
    public void DivisionVariantsRejectZero(VmOp op)
    {
        var result = VmExec.Run(new[] { VmCode.Rrr(op, 0, 0, 0), VmCode.RetInt(0) }, new[] { 0 });
        Assert.Equal(new VmResult(false, 0, VmFault.DivisionByZero), result);
    }

    [Theory]
    [InlineData(VmOp.DivRk)]
    [InlineData(VmOp.DivKr)]
    [InlineData(VmOp.DivuRk)]
    [InlineData(VmOp.DivuKr)]
    [InlineData(VmOp.ModRk)]
    [InlineData(VmOp.ModKr)]
    [InlineData(VmOp.ModuRk)]
    [InlineData(VmOp.ModuKr)]
    public void DivisionVariantsValidateConstantIndexBeforeDivision(VmOp op)
        => Assert.Equal(new VmResult(false, 0, VmFault.ConstantRange),
            VmExec.Run(new[] { VmCode.Rrr(op, 0, 255, 255) }, []));

    [Theory]
    [InlineData(VmOp.SubRk, 6, 7, -1)]
    [InlineData(VmOp.SubKr, 6, 7, 1)]
    [InlineData(VmOp.MulRk, 6, 7, 42)]
    [InlineData(VmOp.SubRk, int.MinValue, 1, int.MaxValue)]
    [InlineData(VmOp.SubKr, 1, int.MinValue, int.MaxValue)]
    [InlineData(VmOp.MulRk, int.MaxValue, 2, -2)]
    public void ConstantArithmeticUsesNativeOperandOrderAndWraps(VmOp op, int register, int constant, int expected)
    {
        var operands = VmCode.Rrr(op, 1, 1, 1);
        var code = new[] { VmCode.Lk(1, 0), operands, VmCode.RetInt(1) };
        Assert.Equal(new VmResult(true, expected, VmFault.None), VmExec.Run(code, new[] { register, constant }));
    }

    [Theory]
    [InlineData(VmOp.SubRk)]
    [InlineData(VmOp.SubKr)]
    [InlineData(VmOp.MulRk)]
    public void ConstantArithmeticRejectsMissingConstant(VmOp op)
    {
        var code = new[] { VmCode.Rrr(op, 0, 255, 255), VmCode.RetInt(0) };
        Assert.Equal(new VmResult(false, 0, VmFault.ConstantRange), VmExec.Run(code, []));
    }

    [Fact]
    public void Multiply_Returns42()
    {
        var code = new[]
        {
            VmCode.Li(1, 6),
            VmCode.Li(2, 7),
            VmCode.Rrr(VmOp.MulRr, 0, 1, 2),
            VmCode.RetInt(0),
        };

        var result = VmExec.Run(code, []);

        Assert.Equal(new VmResult(true, 42, VmFault.None), result);
    }

    [Fact]
    public void DivideByZero_Stops()
    {
        var code = new[]
        {
            VmCode.Li(0, 1),
            VmCode.Li(1, 0),
            VmCode.Rrr(VmOp.DivRr, 2, 0, 1),
            VmCode.RetInt(2),
        };

        var result = VmExec.Run(code, []);

        Assert.Equal(VmFault.DivisionByZero, result.Fault);
        Assert.False(result.Returned);
    }

    [Fact]
    public void Equal_JumpsOverTheFallThrough()
    {
        var code = new[]
        {
            VmCode.Li(0, 3),
            VmCode.Li(1, 3),
            VmCode.Cmp(VmOp.EqR, 1, 0, 1),
            VmCode.Jmp(1),
            VmCode.Reti(0),
            VmCode.Reti(1),
        };

        Assert.Equal(new VmResult(true, 1, VmFault.None), VmExec.Run(code, []));
    }

    [Fact]
    public void LessThan_FallsThroughWhenEqual()
    {
        var code = new[]
        {
            VmCode.Li(0, 5),
            VmCode.Li(1, 5),
            VmCode.Cmp(VmOp.LtRr, 1, 0, 1),
            VmCode.Jmp(1),
            VmCode.Reti(0),
            VmCode.Reti(1),
        };

        Assert.Equal(new VmResult(true, 0, VmFault.None), VmExec.Run(code, []));
    }

    [Fact]
    public void AddConstant_AndImmediate()
    {
        var code = new[]
        {
            VmCode.Li(0, 2),
            VmCode.Rrr(VmOp.AddRk, 1, 0, 0),
            VmCode.Addi(2, 1, -2),
            VmCode.RetInt(2),
        };

        Assert.Equal(new VmResult(true, 40, VmFault.None), VmExec.Run(code, [40]));
    }

    [Fact]
    public void Loop_CountsToThree()
    {
        var code = new[]
        {
            VmCode.Li(0, 0),
            VmCode.Li(1, 1),
            VmCode.Rrr(VmOp.AddRr, 0, 0, 1),
            VmCode.Li(2, 3),
            VmCode.Cmp(VmOp.EqR, 1, 0, 2),
            VmCode.Jmp(1),
            VmCode.Jmp(-5),
            VmCode.RetInt(0),
        };

        Assert.Equal(new VmResult(true, 3, VmFault.None), VmExec.Run(code, []));
    }

    [Fact]
    public void Shifts_KeepTheSignOnlyForArithmetic()
    {
        var arithmetic = new[]
        {
            VmCode.Li(0, -8),
            VmCode.Rrr(VmOp.SraRi, 1, 0, 1),
            VmCode.RetInt(1),
        };
        var logical = new[]
        {
            VmCode.Li(0, -1),
            VmCode.Rrr(VmOp.SrlRi, 1, 0, 1),
            VmCode.RetInt(1),
        };

        Assert.Equal(new VmResult(true, -4, VmFault.None), VmExec.Run(arithmetic, []));
        Assert.Equal(new VmResult(true, int.MaxValue, VmFault.None), VmExec.Run(logical, []));
    }

    [Fact]
    public void UnknownOpcode_Stops()
    {
        var result = VmExec.Run([0xFF], []);

        Assert.Equal(VmFault.UnknownOpcode, result.Fault);
        Assert.False(result.Returned);
    }
}
