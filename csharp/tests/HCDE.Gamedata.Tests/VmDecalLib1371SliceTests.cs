namespace HCDE.Gamedata.Tests;

public class VmDecalLib1371SliceTests
{
    [Fact]
    public void GroupKeywordsAndReplace()
    {
        Assert.Equal(19, VmDecalLib1371Slice.Keywords.Length);
        Assert.Null(VmDecalLib1371Slice.Keywords[^1]);
        Assert.Equal("opaqueblood", VmDecalLib1371Slice.Keywords[VmDecalLib1371Slice.OpaqueBlood]);
        Assert.Equal("translatable", VmDecalLib1371Slice.Keywords[VmDecalLib1371Slice.Translatable]);
        Assert.Equal(0, VmDecalLib1371Slice.XScale);
        Assert.Equal(17, VmDecalLib1371Slice.Translatable);
        Assert.Equal(
            "Deprecated - use translatable instead!",
            VmDecalLib1371Slice.OpaqueBloodNote);
        Assert.Equal(
            VmDecalLib1371Slice.Translatable,
            VmDecalLib1371Slice.Keywords.Length - 2);

        var slice = new VmDecalLib1371Slice();
        var group = slice.CreateGroup();
        Assert.Equal("DecalChoice", group.RngName);
        Assert.Equal("None", group.GetDecalName());
        var missed = Assert.Throws<InvalidOperationException>(() => group.GetDecal());
        Assert.Equal("GetDecal", missed.Message);

        var heavy = new VmDecalLib1371Slice.Base();
        var light = new VmDecalLib1371Slice.Base();
        group.AddDecal(heavy, 0);
        Assert.Empty(group.Snapshot());
        group.AddDecal(heavy, 2);
        group.AddDecal(light, 2);
        var tied = group.Snapshot();
        Assert.Equal(2, tied.Count);
        Assert.Same(light, tied[0].Value);
        Assert.Same(heavy, tied[1].Value);
        Assert.Equal((byte)127, tied[0].RandomVal);
        Assert.Equal((byte)255, tied[1].RandomVal);

        var ordered = slice.CreateGroup();
        var low = new VmDecalLib1371Slice.Base();
        var high = new VmDecalLib1371Slice.Base();
        ordered.AddDecal(high, 5);
        ordered.AddDecal(low, 1);
        var rising = ordered.Snapshot();
        Assert.Same(low, rising[0].Value);
        Assert.Same(high, rising[1].Value);
        Assert.Equal((byte)42, rising[0].RandomVal);
        Assert.Equal((byte)255, rising[1].RandomVal);

        var spread = slice.CreateGroup();
        var one = new VmDecalLib1371Slice.Base();
        var three = new VmDecalLib1371Slice.Base();
        spread.AddDecal(one, 1);
        spread.AddDecal(three, 3);
        var quarters = spread.Snapshot();
        Assert.Equal((byte)63, quarters[0].RandomVal);
        Assert.Equal((byte)255, quarters[1].RandomVal);
        var before = quarters.Select(row => row.RandomVal).ToArray();
        var replacement = new VmDecalLib1371Slice.Base();
        spread.ReplaceDecalRef(one, replacement);
        var after = spread.Snapshot();
        Assert.Same(replacement, after[0].Value);
        Assert.Same(three, after[1].Value);
        Assert.Equal(before, after.Select(row => row.RandomVal).ToArray());

        var shared = new VmDecalLib1371Slice.Base();
        var other = new VmDecalLib1371Slice.Base();
        var many = slice.CreateGroup();
        many.AddDecal(shared, 1);
        many.AddDecal(shared, 1);
        many.AddDecal(other, 1);
        var thirds = many.Snapshot();
        Assert.Same(other, thirds[0].Value);
        Assert.Equal(new byte[] { 85, 170, 255 }, thirds.Select(row => row.RandomVal).ToArray());
        many.ReplaceDecalRef(shared, other);
        var replaced = many.Snapshot();
        Assert.Same(other, replaced[0].Value);
        Assert.Same(other, replaced[1].Value);
        Assert.Same(other, replaced[2].Value);
        Assert.Equal(new byte[] { 85, 170, 255 }, replaced.Select(row => row.RandomVal).ToArray());
        many.AddDecal(null, 0);
        Assert.Equal(3, many.Snapshot().Count);

        var blank = slice.CreateGroup();
        var returned = blank.Assign(many);
        Assert.Same(blank, returned);
        Assert.Empty(blank.Snapshot());
        Assert.Equal(3, many.Snapshot().Count);

        var bare = new VmDecalLib1371Slice.Base();
        Assert.Null(bare.GetDecal());
        Assert.Equal("None", bare.GetDecalName());
        bare.Name = "blood";
        Assert.Equal("blood", bare.GetDecalName());

        var template = new VmDecalLib1371Slice.Template();
        var from = new VmDecalLib1371Slice.Base();
        var to = new VmDecalLib1371Slice.Base();
        template.ReplaceDecalRef(null, from);
        Assert.Same(from, template.LowerDecal);
        template.ReplaceDecalRef(to, null);
        Assert.Same(from, template.LowerDecal);
        template.ReplaceDecalRef(from, to);
        Assert.Same(to, template.LowerDecal);
    }

    [Fact]
    public void LibraryAnimatorsAndNames()
    {
        VmDecalLib1371Slice.Library? seen = null;
        var slice = new VmDecalLib1371Slice(lib => seen = lib);
        Assert.Null(slice.DecalLibrary.Root);
        Assert.Null(slice.DecalLibrary.Translations);
        Assert.Empty(slice.DecalTranslations);
        Assert.Empty(slice.Animators);
        Assert.Empty(slice.CombinerAnimatorList);
        Assert.NotSame(slice.Animators, slice.CombinerAnimatorList);
        Assert.Equal("DecalChoice", VmDecalLib1371Slice.ChoiceRngName);
        Assert.Equal("Decal", VmDecalLib1371Slice.DecalRngName);
        Assert.True(VmDecalLib1371Slice.ChoiceRngClient);
        Assert.True(VmDecalLib1371Slice.DecalRngClient);

        var root = new VmDecalLib1371Slice.Base();
        var translation = new VmDecalLib1371Slice.Translation
        {
            StartColor = 1,
            EndColor = 2,
            Index = 4,
        };
        translation.Next = new VmDecalLib1371Slice.Translation();
        slice.DecalLibrary.Root = root;
        slice.DecalLibrary.Translations = translation;
        slice.DecalLibrary.Destroy();
        Assert.Same(slice.DecalLibrary, seen);
        Assert.Same(root, slice.DecalLibrary.Root);
        Assert.Equal(1u, slice.DecalLibrary.Translations!.StartColor);
        Assert.Equal(2u, slice.DecalLibrary.Translations.EndColor);
        Assert.Equal(4, slice.DecalLibrary.Translations.Index);
        Assert.NotNull(slice.DecalLibrary.Translations.Next);

        var bare = slice.CreateLibrary(null);
        bare.Root = root;
        var missing = Assert.Throws<InvalidOperationException>(() => bare.Destroy());
        Assert.Equal("Clear", missing.Message);
        Assert.Same(root, bare.Root);

        Assert.Equal("None", slice.InternName(null));
        Assert.Equal("None", slice.InternName("none"));
        Assert.Equal("Fade", slice.InternName("Fade"));
        Assert.Equal("Fade", slice.InternName("FADE"));
        Assert.Equal("", slice.InternName(""));
        Assert.NotEqual("None", slice.InternName(""));

        var fader = slice.CreateFader(null);
        Assert.Equal("None", fader.Name);
        Assert.Equal(0, fader.DecayStart);
        Assert.Equal(0, fader.DecayTime);
        var named = slice.CreateFader("Slide");
        var colorer = slice.CreateColorer("slide");
        Assert.Equal("Slide", named.Name);
        Assert.Equal("Slide", colorer.Name);
        Assert.Equal(0u, colorer.GoalColor);
        var stretcher = slice.CreateStretcher("");
        Assert.Equal("", stretcher.Name);
        Assert.Equal(0, stretcher.StretchStart);
        Assert.Equal(0.0, stretcher.GoalX);
        Assert.Equal(0.0, stretcher.GoalY);
        var slider = slice.CreateSlider("move");
        Assert.Equal(0, slider.SlideStart);
        Assert.Equal(0, slider.SlideTime);
        Assert.Equal(0.0, slider.DistY);
        Assert.Null(typeof(VmDecalLib1371Slice.Slider).GetField("DistX"));
        Assert.Null(typeof(VmDecalLib1371Slice.Slider).GetProperty("DistX"));
        var combiner = slice.CreateCombiner("mix");
        Assert.Equal("mix", combiner.Name);
        Assert.Equal(0, combiner.FirstAnimator);
        Assert.Equal(0, combiner.NumAnimators);
        Assert.NotSame(fader, named);
    }
}
