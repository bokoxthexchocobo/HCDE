using S = HCDE.Playsim.VmLevelSerialize1357Slice;

namespace HCDE.Playsim.Tests;

public class VmLevelSerialize1357SliceTests
{
    [Fact]
    public void DrawnSubsectors()
    {
        Assert.Equal(0x100, S.Mapped);
        Assert.Equal(2, S.Drawn);

        var keep = Sub(S.Drawn | 4);
        var mapped = Sub(1, S.Mapped | 8);
        var unmapped = Sub(0, 0x200);
        var hole = Sub(0, null, S.Mapped);
        var shortLines = Sub(0, 0, S.Mapped);
        shortLines.NumLines = 1;
        var level = new S.Level();
        level.Subsectors.Add(keep);
        level.Subsectors.Add(mapped);
        level.Subsectors.Add(unmapped);
        level.Subsectors.Add(hole);
        level.Subsectors.Add(shortLines);
        S.Recalculate(level);
        Assert.Equal(S.Drawn | 4, keep.Flags);
        Assert.Equal(1 | S.Drawn, mapped.Flags);
        Assert.Equal(0, unmapped.Flags);
        Assert.Equal(S.Drawn, hole.Flags);
        Assert.Equal(0, shortLines.Flags);

        var bare = new S.Level();
        bare.Subsectors.Add(new S.Subsector { Flags = 0, NumLines = 1, Lines = null });
        var missing = Assert.Throws<NullReferenceException>(() => S.Recalculate(bare));
        Assert.Equal("Lines", missing.Message);
        var oob = new S.Level();
        oob.Subsectors.Add(new S.Subsector { NumLines = 1, Lines = Array.Empty<S.Seg>() });
        Assert.Throws<IndexOutOfRangeException>(() => S.Recalculate(oob));

        for (int by = 0; by < 64; by++)
        {
            var packed = new S.Level { VertexCount = 3 };
            for (int j = 0; j < 6; j++)
            {
                packed.Subsectors.Add(Sub((by & (1 << j)) != 0 ? S.Drawn : 0));
            }

            var arc = new S.Arc { Writing = true };
            S.SerializeSubsectors(packed, arc, "subs");
            Assert.Equal(
                new[] { "begin-array", "i32", "i32", "str", "end-array" },
                arc.Log.Select(s => s.Kind).ToArray());
            Assert.Equal("subs", arc.Log[0].Key);
            Assert.Null(arc.Log[1].Key);
            Assert.Equal(3, (int)arc.Log[1].Value!);
            Assert.Equal(6, (int)arc.Log[2].Value!);
            Assert.Equal(Expect(by).ToString(), (string?)arc.Log[3].Value);
        }

        var seven = Bits(1, 1, 1, 1, 1, 1, 1);
        seven.VertexCount = 9;
        Assert.Equal("+1", Packed(seven));
        Assert.Equal("00", Packed(Bits(0, 0, 0, 0, 0, 0, 0)));
        var none = new S.Level { VertexCount = 4 };
        var noneArc = new S.Arc { Writing = true };
        S.SerializeSubsectors(none, noneArc, "subs");
        Assert.Equal(4, (int)noneArc.Log[1].Value!);
        Assert.Equal(0, (int)noneArc.Log[2].Value!);
        Assert.Equal("", (string?)noneArc.Log[3].Value);

        var closed = Bits(1);
        var shut = new S.Arc { Writing = true, ArrayOpens = false };
        S.SerializeSubsectors(closed, shut, "subs");
        Assert.Equal(new[] { "begin-array" }, shut.Log.Select(s => s.Kind).ToArray());
        Assert.Equal(S.Drawn, closed.Subsectors[0].Flags);

        var quiet = new S.Level { VertexCount = 8 };
        quiet.Subsectors.Add(Sub(0, S.Mapped));
        Read(quiet, 8, 1, "0");
        Assert.Equal(0, quiet.Subsectors[0].Flags);

        var or = new S.Level { VertexCount = 1 };
        or.Subsectors.Add(Sub(4));
        Read(or, 1, 1, "1");
        Assert.Equal(4 | S.Drawn, or.Subsectors[0].Flags);

        var kept = new S.Level { VertexCount = 1 };
        kept.Subsectors.Add(Sub(S.Drawn | 4));
        Read(kept, 1, 1, "0");
        Assert.Equal(S.Drawn | 4, kept.Subsectors[0].Flags);

        foreach (var sub in seven.Subsectors)
        {
            sub.Flags = 0;
        }

        seven.Subsectors[6].Lines = new[] { new S.Seg { Linedef = new S.Line { Flags = S.Mapped } } };
        seven.Subsectors[6].NumLines = 1;
        Read(seven, 9, 7, "0");
        Assert.Equal(0, seven.Subsectors[0].Flags);
        Assert.Equal(0, seven.Subsectors[6].Flags);

        var bad = new S.Level { VertexCount = 5 };
        for (int i = 0; i < 8; i++)
        {
            bad.Subsectors.Add(Sub(i == 1 ? S.Drawn | 8 : 0));
        }

        bad.Subsectors[6].Lines = new[] { new S.Seg { Linedef = new S.Line { Flags = S.Mapped } } };
        bad.Subsectors[6].NumLines = 1;
        var badArc = Read(bad, 5, 8, "1!");
        Assert.Equal(S.Drawn, bad.Subsectors[0].Flags);
        Assert.Equal(S.Drawn | 8, bad.Subsectors[1].Flags);
        Assert.Equal(0, bad.Subsectors[2].Flags);
        Assert.Equal(S.Drawn, bad.Subsectors[6].Flags);
        Assert.Contains(badArc.Log, s => s.Kind == "end-array");

        var mismatch = new S.Level { VertexCount = 4 };
        mismatch.Subsectors.Add(Sub(0));
        mismatch.Subsectors.Add(Sub(0, S.Mapped));
        var mis = Read(mismatch, 5, 2, "1");
        Assert.Equal(0, mismatch.Subsectors[0].Flags);
        Assert.Equal(S.Drawn, mismatch.Subsectors[1].Flags);
        Assert.Equal("1", (string?)mis.Log.Single(s => s.Kind == "str").Value);

        var closedRead = new S.Level { VertexCount = 1 };
        closedRead.Subsectors.Add(Sub(0, S.Mapped));
        var cr = new S.Arc { Writing = false, ArrayOpens = false };
        cr.Reads.Enqueue(1);
        S.SerializeSubsectors(closedRead, cr, "subs");
        Assert.Equal(0, closedRead.Subsectors[0].Flags);
        Assert.Equal(1, Assert.Single(cr.Reads));
        Assert.Equal("begin-array", cr.Log.Single().Kind);

        var nul = new S.Level { VertexCount = 2 };
        nul.Subsectors.Add(Sub(0, S.Mapped));
        nul.Subsectors.Add(Sub(0, S.Mapped));
        Read(nul, 2, 2, "\0+");
        Assert.Equal(0, nul.Subsectors[0].Flags);
        Assert.Equal(0, nul.Subsectors[1].Flags);

        var nulMid = new S.Level { VertexCount = 2 };
        for (int i = 0; i < 8; i++)
        {
            nulMid.Subsectors.Add(Sub(0, i == 6 ? S.Mapped : 0));
        }

        Read(nulMid, 2, 8, "1\0!");
        Assert.Equal(S.Drawn, nulMid.Subsectors[0].Flags);
        Assert.Equal(0, nulMid.Subsectors[6].Flags);

        var crash = new S.Level { VertexCount = 1 };
        crash.Subsectors.Add(Sub(0, S.Mapped));
        var crashArc = new S.Arc { Writing = false };
        crashArc.Reads.Enqueue(1);
        crashArc.Reads.Enqueue(1);
        crashArc.Reads.Enqueue(null);
        var nre = Assert.Throws<NullReferenceException>(() => S.SerializeSubsectors(crash, crashArc, "subs"));
        Assert.Equal("str", nre.Message);
        Assert.Equal(0, crash.Subsectors[0].Flags);
        Assert.Contains(crashArc.Log, s => s.Kind == "end-array");

        var empty = new S.Level { VertexCount = 4 };
        Read(empty, 4, 0, "");
        Assert.Empty(empty.Subsectors);
    }

    [Fact]
    public void PortalsPolyReverbAndSounds()
    {
        var origin = new object();
        var dest = new object();
        var line = new S.LinePortal
        {
            Origin = origin,
            Destination = dest,
            X = 1.5,
            Y = -2,
            Type = 3,
            Flags = 4,
            DefFlags = 5,
            Align = 6,
        };
        var warc = new S.Arc { Writing = true };
        S.SerializeLine(warc, "line", line, new S.LinePortal { Type = 9 });
        Assert.Equal(
            new[]
            {
                "begin-object line", "obj origin", "obj destination", "vec displacement",
                "u8 type", "u8 flags", "u8 defflags", "u8 align", "end-object -",
            },
            Tags(warc));
        Assert.Same(origin, warc.Log[1].Value);
        Assert.Equal((1.5, -2d), Assert.IsType<(double, double)>(warc.Log[3].Value));
        Assert.Equal((byte)3, warc.Log.Single(s => s.Key == "type").Value);

        var shut = new S.Arc { ObjectOpens = false };
        S.SerializeLine(shut, "line", line, null);
        Assert.Equal("begin-object", shut.Log.Single().Kind);
        Assert.Equal((byte)3, line.Type);

        var rarc = new S.Arc { Writing = false };
        rarc.Reads.Enqueue("o2");
        rarc.Reads.Enqueue("d2");
        rarc.Reads.Enqueue((8d, 9d));
        rarc.Reads.Enqueue(261);
        rarc.Reads.Enqueue(300);
        rarc.Reads.Enqueue(1);
        rarc.Reads.Enqueue(2);
        S.SerializeLine(rarc, "line", line, null);
        Assert.Equal("o2", line.Origin);
        Assert.Equal("d2", line.Destination);
        Assert.Equal(8d, line.X);
        Assert.Equal(9d, line.Y);
        Assert.Equal((byte)5, line.Type);
        Assert.Equal((byte)44, line.Flags);
        Assert.Equal((byte)1, line.DefFlags);
        Assert.Equal((byte)2, line.Align);

        var sector = new S.SectorPortal
        {
            Type = 1,
            Flags = 2,
            Partner = 3,
            Plane = 4,
            Origin = "so",
            Destination = "sd",
            X = 5,
            Y = 6,
            PlaneZ = 7.25,
            Skybox = "sky",
        };
        var sarc = new S.Arc { Writing = true };
        S.SerializeSector(sarc, "sec", sector, new S.SectorPortal { Type = 99 });
        Assert.Equal(
            new[]
            {
                "begin-object sec", "i32 type", "i32 flags", "u32 partner", "i32 plane",
                "obj origin", "obj destination", "vec displacement", "f64 planez", "obj skybox",
                "end-object -",
            },
            Tags(sarc));
        Assert.Equal(1, sector.Type);
        Assert.Equal(3u, sarc.Log.Single(s => s.Key == "partner").Value);
        Assert.Equal(7.25, Assert.IsType<double>(sarc.Log.Single(s => s.Key == "planez").Value));

        var sread = new S.Arc { Writing = false };
        sread.Reads.Enqueue(9);
        sread.Reads.Enqueue(8);
        sread.Reads.Enqueue(7u);
        sread.Reads.Enqueue(6);
        sread.Reads.Enqueue("O");
        sread.Reads.Enqueue("D");
        sread.Reads.Enqueue((1.25, 2.5));
        sread.Reads.Enqueue(3.5);
        sread.Reads.Enqueue("box");
        S.SerializeSector(sread, "sec", sector, null);
        Assert.Equal(9, sector.Type);
        Assert.Equal(8, sector.Flags);
        Assert.Equal(7u, sector.Partner);
        Assert.Equal(6, sector.Plane);
        Assert.Equal("O", sector.Origin);
        Assert.Equal("D", sector.Destination);
        Assert.Equal(1.25, sector.X);
        Assert.Equal(2.5, sector.Y);
        Assert.Equal(3.5, sector.PlaneZ);
        Assert.Equal("box", sector.Skybox);

        var moved = false;
        var poly = new S.Poly
        {
            Angle = 7,
            X = 5,
            Y = 6,
            OriginalCount = 3,
            Interpolation = "old",
            Blocked = false,
            HasPortals = 1,
            Special = "sp",
            Level = "lv",
            Rotate = (_, _) =>
            {
                moved = true;
                return false;
            },
            Move = (_, _, _) =>
            {
                moved = true;
                return false;
            },
        };
        var parc = new S.Arc { Writing = true };
        parc.OnField = key =>
        {
            if (key == "angle")
            {
                poly.X = 99;
                poly.Y = 98;
                poly.Interpolation = "live";
            }
        };
        S.SerializePoly(parc, "poly", poly, new S.Poly { Angle = 123 });
        Assert.False(moved);
        Assert.Equal(7d, Assert.IsType<double>(parc.Log.Single(s => s.Key == "angle").Value));
        Assert.Equal((5d, 6d), Assert.IsType<(double, double)>(parc.Log.Single(s => s.Key == "pos").Value));
        Assert.Equal("live", parc.Log.Single(s => s.Key == "interpolation").Value);
        Assert.Equal(7d, poly.Angle);

        double gotAngle = -1;
        var gotForce = false;
        double mdx = 0;
        double mdy = 0;
        var moveForce = false;
        var moveCalled = false;
        poly.Angle = 7;
        poly.X = 8;
        poly.Y = 9;
        poly.OriginalCount = 2;
        poly.Rotate = (angle, force) =>
        {
            gotAngle = angle;
            gotForce = force;
            poly.X = 2;
            poly.Y = 3;
            return false;
        };
        poly.Move = (x, y, force) =>
        {
            moveCalled = true;
            mdx = x;
            mdy = y;
            moveForce = force;
            return false;
        };
        var pr = new S.Arc { Writing = false };
        pr.Reads.Enqueue(40d);
        pr.Reads.Enqueue((10d, 4d));
        pr.Reads.Enqueue("lerp");
        pr.Reads.Enqueue(true);
        pr.Reads.Enqueue(2);
        pr.Reads.Enqueue("spec");
        pr.Reads.Enqueue("lvl");
        S.SerializePoly(pr, "poly", poly, null);
        Assert.Equal(40d, gotAngle);
        Assert.True(gotForce);
        Assert.True(moveCalled);
        Assert.Equal(8d, mdx);
        Assert.Equal(1d, mdy);
        Assert.True(moveForce);
        Assert.Equal(7d, poly.Angle);
        Assert.Equal("lerp", poly.Interpolation);
        Assert.True(poly.Blocked);
        Assert.Equal((byte)2, poly.HasPortals);
        Assert.Equal("spec", poly.Special);
        Assert.Equal("lvl", poly.Level);
        Assert.Equal(2d, poly.X);
        Assert.Equal(3d, poly.Y);

        var spun = false;
        poly.OriginalCount = 0;
        poly.Rotate = (_, _) =>
        {
            spun = true;
            return true;
        };
        var skip = new S.Arc { Writing = false };
        Feed(skip, 1d, (0d, 0d));
        S.SerializePoly(skip, "poly", poly, null);
        Assert.False(spun);

        poly.OriginalCount = 4;
        var closedPoly = new S.Arc { Writing = false, ObjectOpens = false };
        S.SerializePoly(closedPoly, "poly", poly, null);
        Assert.False(spun);

        var rotated = false;
        poly.OriginalCount = 1;
        poly.Rotate = (_, _) =>
        {
            rotated = true;
            poly.X = 1;
            poly.Y = 1;
            return true;
        };
        poly.Move = null;
        var nomove = new S.Arc { Writing = false };
        Feed(nomove, 3d, (4d, 5d));
        var mex = Assert.Throws<InvalidOperationException>(() => S.SerializePoly(nomove, "poly", poly, null));
        Assert.Equal("MovePolyobj", mex.Message);
        Assert.True(rotated);

        poly.Rotate = null;
        var norot = new S.Arc { Writing = false };
        Feed(norot, 1d, (0d, 0d));
        var rex = Assert.Throws<InvalidOperationException>(() => S.SerializePoly(norot, "poly", poly, null));
        Assert.Equal("RotatePolyobj", rex.Message);

        var alive = new S.Reverb { Id = 4 };
        var ignored = new S.Reverb { Id = 99 };
        S.Reverb? slot = alive;
        var rev = new S.Arc { Writing = true };
        S.SerializeReverb(rev, "env", ref slot, ignored);
        Assert.Equal(4, rev.Log.Single().Value);
        Assert.Equal("env", rev.Log.Single().Key);
        Assert.Same(alive, slot);

        slot = null;
        var rev0 = new S.Arc { Writing = true };
        S.SerializeReverb(rev0, "env", ref slot, ignored);
        Assert.Equal(0, rev0.Log.Single().Value);

        var found = new S.Reverb { Id = 12 };
        slot = alive;
        var revr = new S.Arc { Writing = false };
        revr.Reads.Enqueue(12);
        revr.FindEnvironment = id =>
        {
            Assert.Equal(12, id);
            return found;
        };
        S.SerializeReverb(revr, "env", ref slot, ignored);
        Assert.Same(found, slot);
        Assert.Equal(99, ignored.Id);

        slot = alive;
        var missingFind = new S.Arc { Writing = false };
        missingFind.Reads.Enqueue(3);
        var fex = Assert.Throws<InvalidOperationException>(() => S.SerializeReverb(missingFind, "env", ref slot, null));
        Assert.Equal("S_FindEnvironment", fex.Message);
        Assert.Same(alive, slot);

        slot = alive;
        var revNull = new S.Arc { Writing = false, FindEnvironment = _ => null };
        revNull.Reads.Enqueue(8);
        S.SerializeReverb(revNull, "env", ref slot, null);
        Assert.Null(slot);

        var emptySer = new S.Arc { Writing = false };
        var iex = Assert.Throws<InvalidOperationException>(() => S.SerializeReverb(emptySer, "env", ref slot, null));
        Assert.Equal("Serialize", iex.Message);
        Assert.Null(slot);

        var zone = new S.Zone { Environment = new S.Reverb { Id = 4 } };
        var zoneDef = new S.Zone { Environment = new S.Reverb { Id = 99 } };
        var zarc = new S.Arc { Writing = true };
        S.SerializeZone(zarc, "zone", zone, zoneDef);
        Assert.Equal(4, zarc.Log.Single().Value);
        Assert.Equal(99, zoneDef.Environment!.Id);

        var zread = new S.Arc { Writing = false, FindEnvironment = id => new S.Reverb { Id = id } };
        zread.Reads.Enqueue(15);
        S.SerializeZone(zread, "zone", zone, zoneDef);
        Assert.Equal(15, zone.Environment!.Id);
        Assert.Equal(99, zoneDef.Environment!.Id);

        var calls = new List<string>();
        var level = new S.Level { Primary = true, MusicVolume = 0.5f };
        level.SerializeSounds = _ =>
        {
            calls.Add("ser");
            level.MusicVolume = 0.25f;
        };
        level.GetMusic = (ref string? name) =>
        {
            calls.Add("get");
            name = "d_run\0nin";
            level.MusicVolume = 9f;
            return 300;
        };
        var music = new S.Arc { Writing = true };
        S.SerializeSounds(level, music);
        Assert.Equal(new[] { "ser", "get" }, calls);
        Assert.Equal(
            new[] { "str musicname", "u8 musicorder", "f32 musicvolume" },
            Tags(music));
        Assert.Equal("d_run", music.Log.Single(s => s.Key == "musicname").Value);
        Assert.Equal((byte)44, music.Log.Single(s => s.Key == "musicorder").Value);
        Assert.Equal(0.25f, music.Log.Single(s => s.Key == "musicvolume").Value);
        Assert.Equal(9f, level.MusicVolume);

        level.GetMusic = (ref string? name) =>
        {
            name = null;
            return -1;
        };
        calls.Clear();
        var neg = new S.Arc { Writing = true };
        S.SerializeSounds(level, neg);
        Assert.Null(neg.Log.Single(s => s.Key == "musicname").Value);
        Assert.Equal((byte)255, neg.Log.Single(s => s.Key == "musicorder").Value);

        level.GetMusic = (ref string? name) => throw new InvalidOperationException("get-should-not " + name);
        level.SerializeSounds = _ => calls.Add("ser");
        calls.Clear();
        level.MusicVolume = 0.5f;
        string? gotName = "x";
        var gotOrder = -1;
        float gotVol = -1;
        level.ChangeMusic = (name, order) =>
        {
            calls.Add("change");
            gotName = name;
            gotOrder = order;
            return false;
        };
        level.SetMusic = () => calls.Add("set");
        level.SetMusicVolume = v =>
        {
            calls.Add("vol");
            gotVol = v;
        };
        var rd = new S.Arc { Writing = false };
        rd.Reads.Enqueue(null);
        rd.Reads.Enqueue(300);
        rd.Reads.Enqueue(0.8f);
        S.SerializeSounds(level, rd);
        Assert.Equal(new[] { "ser", "change", "set", "vol" }, calls);
        Assert.Null(gotName);
        Assert.Equal(44, gotOrder);
        Assert.Equal(0.8f, gotVol);
        Assert.Equal(0.5f, level.MusicVolume);

        calls.Clear();
        level.ChangeMusic = (_, _) =>
        {
            calls.Add("change");
            return true;
        };
        level.SetMusic = () => throw new InvalidOperationException("set-should-not");
        var ok = new S.Arc { Writing = false };
        ok.Reads.Enqueue("d_e1m1");
        ok.Reads.Enqueue(3);
        ok.Reads.Enqueue(1f);
        S.SerializeSounds(level, ok);
        Assert.Equal(new[] { "ser", "change", "vol" }, calls);

        level.Primary = false;
        level.SerializeSounds = _ => throw new InvalidOperationException("ser-should-not");
        S.SerializeSounds(level, new S.Arc { Writing = true });

        level.Primary = true;
        level.SerializeSounds = null;
        var sex = Assert.Throws<InvalidOperationException>(() => S.SerializeSounds(level, new S.Arc { Writing = true }));
        Assert.Equal("S_SerializeSounds", sex.Message);

        level.SerializeSounds = _ => calls.Add("ser");
        level.GetMusic = null;
        calls.Clear();
        var gex = Assert.Throws<InvalidOperationException>(() => S.SerializeSounds(level, new S.Arc { Writing = true }));
        Assert.Equal("S_GetMusic", gex.Message);
        Assert.Equal(new[] { "ser" }, calls);

        level.ChangeMusic = null;
        calls.Clear();
        var nochange = new S.Arc { Writing = false };
        nochange.Reads.Enqueue("a");
        nochange.Reads.Enqueue(1);
        nochange.Reads.Enqueue(0.1f);
        var cex = Assert.Throws<InvalidOperationException>(() => S.SerializeSounds(level, nochange));
        Assert.Equal("S_ChangeMusic", cex.Message);
        Assert.Equal(new[] { "ser" }, calls);

        level.ChangeMusic = (_, _) => false;
        level.SetMusic = null;
        var noset = new S.Arc { Writing = false };
        noset.Reads.Enqueue("a");
        noset.Reads.Enqueue(1);
        noset.Reads.Enqueue(0.1f);
        var setex = Assert.Throws<InvalidOperationException>(() => S.SerializeSounds(level, noset));
        Assert.Equal("SetMusic", setex.Message);

        level.SetMusic = () => { };
        level.SetMusicVolume = null;
        var novol = new S.Arc { Writing = false };
        novol.Reads.Enqueue("a");
        novol.Reads.Enqueue(1);
        novol.Reads.Enqueue(0.1f);
        var vex = Assert.Throws<InvalidOperationException>(() => S.SerializeSounds(level, novol));
        Assert.Equal("SetMusicVolume", vex.Message);
    }

    static S.Subsector Sub(int flags, params int?[] lineFlags)
    {
        var segs = new S.Seg[lineFlags.Length];
        for (int i = 0; i < lineFlags.Length; i++)
        {
            segs[i] = new S.Seg();
            if (lineFlags[i] is int bits)
            {
                segs[i].Linedef = new S.Line { Flags = bits };
            }
        }

        return new S.Subsector { Flags = flags, NumLines = segs.Length, Lines = segs };
    }

    static S.Level Bits(params int[] drawn)
    {
        var level = new S.Level();
        foreach (int bit in drawn)
        {
            level.Subsectors.Add(Sub(bit != 0 ? S.Drawn : 0));
        }

        return level;
    }

    static string Packed(S.Level level)
    {
        var arc = new S.Arc { Writing = true };
        S.SerializeSubsectors(level, arc, "subs");
        return (string)arc.Log.Single(s => s.Kind == "str").Value!;
    }

    static S.Arc Read(S.Level level, int verts, int subs, string? text)
    {
        var arc = new S.Arc { Writing = false };
        arc.Reads.Enqueue(verts);
        arc.Reads.Enqueue(subs);
        arc.Reads.Enqueue(text);
        S.SerializeSubsectors(level, arc, "subs");
        return arc;
    }

    static void Feed(S.Arc arc, double angle, (double, double) pos)
    {
        arc.Reads.Enqueue(angle);
        arc.Reads.Enqueue(pos);
        arc.Reads.Enqueue(null);
        arc.Reads.Enqueue(false);
        arc.Reads.Enqueue(0);
        arc.Reads.Enqueue(null);
        arc.Reads.Enqueue(null);
    }

    static string[] Tags(S.Arc arc) =>
        arc.Log.Select(s => s.Kind + " " + (s.Key ?? "-")).ToArray();

    static char Expect(int by)
    {
        if (by < 10) return (char)('0' + by);
        if (by < 36) return (char)('A' - 10 + by);
        if (by < 62) return (char)('a' - 36 + by);
        if (by == 62) return '-';
        return '+';
    }
}
