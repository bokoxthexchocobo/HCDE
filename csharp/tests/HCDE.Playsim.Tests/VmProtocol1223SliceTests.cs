using System.Globalization;
using System.Text;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmProtocol1223SliceTests
{
    [Fact]
    public void Pack_ResendsHeldAxesAndDropsButtonsAboveBit28()
    {
        string dump = Build();
        int cut = dump.IndexOf("skip-yaw", StringComparison.Ordinal);
        string pack = dump[..cut];
        Assert.Contains("pack-b29  b=0 p=0 y=0 r=0 f=0 s=0 u=0\n", pack, StringComparison.Ordinal);
        Assert.Contains("pack-steady 3e00010002000400050006\n", pack, StringComparison.Ordinal);
        Assert.Contains("msg-roll 02\n", pack, StringComparison.Ordinal);
        Assert.Contains("msg-yaw 0104ffe0\n", pack, StringComparison.Ordinal);
        Assert.Contains("alias0  b=5 p=0 y=9 r=0 f=0 s=0 u=0\n", pack, StringComparison.Ordinal);
        Assert.Equal(PackGolden, pack);
    }

    [Fact]
    public void Read_StopsWhenSkipStallsAndPadsAnOddChunk()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("skip-yaw", StringComparison.Ordinal)..];
        Assert.Contains("read-stuck rem=1 data=0b\n", tail, StringComparison.Ordinal);
        Assert.Contains("read-nest b=0 p=0 y=-32 r=0 f=0 s=0 u=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("run-gate\nrun-go\ndo 4 p=1 rem=3\n", tail, StringComparison.Ordinal);
        Assert.Contains("iff 010203040000000301020300\n", tail, StringComparison.Ordinal);
        Assert.Contains("skip-1 1122\n", tail, StringComparison.Ordinal);
        Assert.Contains("i32 -1\n", tail, StringComparison.Ordinal);
        Assert.Equal(ReadGolden, tail);
    }

    static string Build()
    {
        var sb = new StringBuilder();
        var p = new VmProtocol1223();
        p.SkipCommand = (cmd, stream) =>
        {
            if (cmd == 10)
            {
                int n = p.ReadInt8(stream);
                p.Advance(stream, n);
            }
        };
        p.DoCommand = (cmd, stream, player) =>
        {
            sb.Append("do ");
            sb.Append(cmd.ToString(CultureInfo.InvariantCulture));
            sb.Append(" p=");
            sb.Append(player.ToString(CultureInfo.InvariantCulture));
            sb.Append(" rem=");
            sb.Append(stream.Size.ToString(CultureInfo.InvariantCulture));
            sb.Append('\n');
            if (cmd == 4)
                p.ReadInt8(stream);
        };

        var cmd = new VmUserCmd1223
        {
            Buttons = 0x01020304u,
            Pitch = -2,
            Yaw = 3,
            Roll = 4,
            Forward = 5,
            Side = 6,
            Up = 7,
        };
        var closed = new VmProtoArc1223 { Open = false };
        sb.Append("ser-closed\n");
        p.Serialize(closed, "closed", ref cmd);
        sb.Append(closed.Log);
        var open = new VmProtoArc1223();
        sb.Append("ser-open\n");
        p.Serialize(open, "cmd", ref cmd);
        sb.Append(open.Log);

        void PackBoth(string tag, VmUserCmd1223 c, VmUserCmd1223? basis)
        {
            var buf = new byte[64];
            Array.Fill(buf, (byte)0x5a);
            var st = new VmProtoView1223 { Buf = buf, Pos = 0, End = 64 };
            p.Pack(c, basis, st);
            int n = 64 - st.Size;
            Hex(tag, buf, n);
            var rd = new VmProtoView1223 { Buf = buf, Pos = 0, End = n };
            VmUserCmd1223 outCmd = default;
            p.Unpack(ref outCmd, basis, false, rd);
            Show(tag, outCmd);
            sb.Append(tag);
            sb.Append(" rem=");
            sb.Append(rd.Size.ToString(CultureInfo.InvariantCulture));
            sb.Append('\n');
        }

        void Show(string tag, VmUserCmd1223 c)
        {
            sb.Append(tag);
            sb.Append(" b=");
            sb.Append(c.Buttons.ToString(CultureInfo.InvariantCulture));
            sb.Append(" p=");
            sb.Append(c.Pitch.ToString(CultureInfo.InvariantCulture));
            sb.Append(" y=");
            sb.Append(c.Yaw.ToString(CultureInfo.InvariantCulture));
            sb.Append(" r=");
            sb.Append(c.Roll.ToString(CultureInfo.InvariantCulture));
            sb.Append(" f=");
            sb.Append(c.Forward.ToString(CultureInfo.InvariantCulture));
            sb.Append(" s=");
            sb.Append(c.Side.ToString(CultureInfo.InvariantCulture));
            sb.Append(" u=");
            sb.Append(c.Up.ToString(CultureInfo.InvariantCulture));
            sb.Append('\n');
        }

        void Hex(string tag, byte[] buf, int n)
        {
            sb.Append(tag);
            for (int i = 0; i < n; i++)
                sb.Append(buf[i].ToString("x2"));
            sb.Append('\n');
        }

        void WriteMsg(string tag, VmUserCmd1223 c, VmUserCmd1223? basis)
        {
            var buf = new byte[64];
            Array.Fill(buf, (byte)0x5a);
            var st = new VmProtoView1223 { Buf = buf, Pos = 0, End = 64 };
            p.WriteMessage(c, basis, st);
            Hex(tag, buf, 64 - st.Size);
        }

        void SkipShow(string tag, byte[] b)
        {
            var st = new VmProtoView1223 { Buf = b, Pos = 0, End = b.Length };
            p.SkipMessage(st);
            sb.Append(tag);
            sb.Append(" rem=");
            sb.Append(st.Size.ToString(CultureInfo.InvariantCulture));
            sb.Append(' ');
            if (st.Size > 0)
                Hex("next=", st.Buf.AsSpan(st.Pos, st.Size).ToArray(), st.Size);
            else
                sb.Append("next=\n");
        }

        void ReadShow(string tag, byte[] b, int count, int player, int tic)
        {
            var st = new VmProtoView1223 { Buf = b, Pos = 0, End = count };
            p.ReadMessage(st, player, tic);
            sb.Append(tag);
            sb.Append(" rem=");
            sb.Append(st.Size.ToString(CultureInfo.InvariantCulture));
            sb.Append(" data=");
            foreach (byte by in p.PayloadAt(player, tic))
                sb.Append(by.ToString("x2"));
            sb.Append('\n');
            Show(tag, p.CommandAt(player, tic));
        }

        var z = new VmUserCmd1223();
        PackBoth("pack-zero ", z, null);
        z.Yaw = -32;
        PackBoth("pack-yaw ", z, null);
        z = default;
        z.Buttons = 1;
        PackBoth("pack-b1 ", z, null);
        z.Buttons = 0x80;
        PackBoth("pack-b7 ", z, null);
        z.Buttons = 0x4000;
        PackBoth("pack-b14 ", z, null);
        z.Buttons = 0x200000;
        PackBoth("pack-b21 ", z, null);
        z.Buttons = 0xE0000000u;
        PackBoth("pack-b29 ", z, null);
        z.Buttons = 0x1FE00000u;
        PackBoth("pack-b21to28 ", z, null);

        var basis = new VmUserCmd1223 { Buttons = 0xE0000000u, Yaw = 9 };
        cmd = basis;
        cmd.Buttons = 0xE0000001u;
        PackBoth("pack-keep29 ", cmd, basis);
        basis = new VmUserCmd1223 { Pitch = 1, Yaw = 2, Roll = 3, Forward = 4, Side = 5, Up = 6 };
        PackBoth("pack-steady ", basis, basis);
        basis = new VmUserCmd1223 { Roll = 7, Yaw = 9 };
        PackBoth("pack-roll ", basis, basis);
        basis = new VmUserCmd1223 { Pitch = 5, Yaw = 9 };
        cmd = basis;
        cmd.Pitch = 0;
        PackBoth("pack-pitch0 ", cmd, basis);

        cmd = new VmUserCmd1223 { Yaw = 9, Buttons = 5 };
        var flags0 = new byte[] { 0 };
        var alias = new VmProtoView1223 { Buf = flags0, Pos = 0, End = 1 };
        p.Unpack(ref cmd, cmd, true, alias);
        Show("alias0 ", cmd);
        cmd = new VmUserCmd1223 { Yaw = 9, Buttons = 5 };
        var pitchSet = new byte[] { 0x02, 0x00, 0x05 };
        var alias2 = new VmProtoView1223 { Buf = pitchSet, Pos = 0, End = 3 };
        p.Unpack(ref cmd, cmd, true, alias2);
        Show("aliasp ", cmd);

        z = default;
        WriteMsg("msg-zero ", z, null);
        z.Roll = 3;
        WriteMsg("msg-roll0 ", z, null);
        basis = new VmUserCmd1223 { Roll = 3 };
        WriteMsg("msg-roll ", basis, basis);
        basis = new VmUserCmd1223 { Yaw = -32 };
        WriteMsg("msg-yaw ", basis, basis);
        basis = new VmUserCmd1223 { Pitch = 1, Yaw = 2, Roll = 3, Forward = 4, Side = 5, Up = 6 };
        WriteMsg("msg-steady ", basis, basis);
        basis = new VmUserCmd1223 { Pitch = 5, Yaw = 9 };
        cmd = basis;
        cmd.Pitch = 0;
        WriteMsg("msg-pitch0 ", cmd, basis);
        z = default;
        z.Buttons = 0x80;
        WriteMsg("msg-b7 ", z, null);

        SkipShow("skip-yaw", new byte[] { 1, 0x04, 0xff, 0xe0, 0xee });
        SkipShow("skip-empty", new byte[] { 2, 0xee });
        SkipShow("skip-nest", new byte[] { 10, 2, 0xaa, 0xbb, 2, 0xee });
        SkipShow("skip-stuck", new byte[] { 11, 0xee });

        p.SetCommand(0, 0, new VmUserCmd1223 { Yaw = 99 });
        ReadShow("read-empty1", new byte[] { 0 }, 0, 0, 1);
        p.SetCommand(0, 0, new VmUserCmd1223 { Yaw = 77 });
        ReadShow("read-empty0", new byte[] { 2 }, 1, 0, 0);
        p.SetCommand(0, 4, new VmUserCmd1223 { Yaw = 8 });
        ReadShow("read-empty5", new byte[] { 2 }, 1, 0, 5);
        ReadShow("read-yaw0", new byte[] { 0xab, 0xcd, 1, 0x04, 0xff, 0xe0 }, 6, 0, 0);
        p.SetCommand(0, 174, new VmUserCmd1223 { Buttons = 0x11, Yaw = 4 });
        ReadShow("read-175", new byte[] { 1, 0x00 }, 2, 0, 175);
        p.SetCommand(0, 0, new VmUserCmd1223 { Yaw = 3 });
        ReadShow("read-stuck", new byte[] { 11, 0xee }, 2, 0, 0);
        ReadShow("read-nest", new byte[] { 10, 2, 0xaa, 0xbb, 1, 0x04, 0xff, 0xe0 }, 8, 0, 0);

        p.Gametic = 2;
        p.TicDup = 1;
        sb.Append("run-empty\n");
        p.Run(1, 0);
        p.SetPayload(1, 0, new byte[] { 4, 9, 1, 2 });
        p.Gametic = 1;
        p.TicDup = 2;
        sb.Append("run-gate\n");
        p.Run(1, 0);
        p.Gametic = 2;
        sb.Append("run-go\n");
        p.Run(1, 0);
        sb.Append("run-again\n");
        p.Run(1, 0);

        var chunk = new byte[32];
        Array.Fill(chunk, (byte)0x5a);
        var cst = new VmProtoView1223 { Buf = chunk, Pos = 0, End = 32 };
        p.StartChunk(0x01020304, cst);
        p.WriteInt8(1, cst);
        p.WriteInt8(2, cst);
        p.WriteInt8(3, cst);
        p.FinishChunk(cst);
        int used = 32 - cst.Size;
        Hex("iff ", chunk, used);
        int remBefore = cst.Size;
        p.FinishChunk(cst);
        sb.Append("iff-second rem=");
        sb.Append(cst.Size == remBefore ? "1" : "0");
        sb.Append('\n');

        var odd = new byte[] { 0, 0, 0, 3, 0xaa, 0xbb, 0xcc, 0x00, 0xee };
        var sk = new VmProtoView1223 { Buf = odd, Pos = 0, End = odd.Length };
        p.SkipChunk(sk);
        Hex("skip3 ", odd.AsSpan(sk.Pos).ToArray(), sk.Size);
        var neg = new byte[] { 0xff, 0xff, 0xff, 0xff, 0x11, 0x22 };
        var nk = new VmProtoView1223 { Buf = neg, Pos = 0, End = neg.Length };
        p.SkipChunk(nk);
        Hex("skip-1 ", neg.AsSpan(nk.Pos).ToArray(), nk.Size);
        var i16b = new byte[] { 0x80, 0x00 };
        var iv = new VmProtoView1223 { Buf = i16b, Pos = 0, End = 2 };
        sb.Append("i16 ");
        sb.Append(p.ReadInt16(iv).ToString(CultureInfo.InvariantCulture));
        sb.Append('\n');
        var i32b = new byte[] { 0xff, 0xff, 0xff, 0xff };
        var iw = new VmProtoView1223 { Buf = i32b, Pos = 0, End = 4 };
        sb.Append("i32 ");
        sb.Append(p.ReadInt32(iw).ToString(CultureInfo.InvariantCulture));
        sb.Append('\n');

        return sb.ToString();
    }

    const string PackGolden =
        """
        ser-closed
        B closed
        ser-open
        B cmd
        U buttons 16909060
        S pitch -2
        S yaw 3
        S roll 4
        S forwardmove 5
        S sidemove 6
        S upmove 7
        E
        pack-zero 00
        pack-zero  b=0 p=0 y=0 r=0 f=0 s=0 u=0
        pack-zero  rem=0
        pack-yaw 04ffe0
        pack-yaw  b=0 p=0 y=-32 r=0 f=0 s=0 u=0
        pack-yaw  rem=0
        pack-b1 0101
        pack-b1  b=1 p=0 y=0 r=0 f=0 s=0 u=0
        pack-b1  rem=0
        pack-b7 018001
        pack-b7  b=128 p=0 y=0 r=0 f=0 s=0 u=0
        pack-b7  rem=0
        pack-b14 01808001
        pack-b14  b=16384 p=0 y=0 r=0 f=0 s=0 u=0
        pack-b14  rem=0
        pack-b21 0180808001
        pack-b21  b=2097152 p=0 y=0 r=0 f=0 s=0 u=0
        pack-b21  rem=0
        pack-b29 0180808000
        pack-b29  b=0 p=0 y=0 r=0 f=0 s=0 u=0
        pack-b29  rem=0
        pack-b21to28 01808080ff
        pack-b21to28  b=534773760 p=0 y=0 r=0 f=0 s=0 u=0
        pack-b21to28  rem=0
        pack-keep29 05010009
        pack-keep29  b=3758096385 p=0 y=9 r=0 f=0 s=0 u=0
        pack-keep29  rem=0
        pack-steady 3e00010002000400050006
        pack-steady  b=0 p=1 y=2 r=3 f=4 s=5 u=6
        pack-steady  rem=0
        pack-roll 040009
        pack-roll  b=0 p=0 y=9 r=7 f=0 s=0 u=0
        pack-roll  rem=0
        pack-pitch0 0600000009
        pack-pitch0  b=0 p=0 y=9 r=0 f=0 s=0 u=0
        pack-pitch0  rem=0
        alias0  b=5 p=0 y=9 r=0 f=0 s=0 u=0
        aliasp  b=5 p=5 y=9 r=0 f=0 s=0 u=0
        msg-zero 02
        msg-roll0 01400003
        msg-roll 02
        msg-yaw 0104ffe0
        msg-steady 013e00010002000400050006
        msg-pitch0 010600000009
        msg-b7 01018001

        """;

    const string ReadGolden =
        """
        skip-yaw rem=1 next=ee
        skip-empty rem=1 next=ee
        skip-nest rem=1 next=ee
        skip-stuck rem=1 next=ee
        read-empty1 rem=0 data=
        read-empty1 b=0 p=0 y=99 r=0 f=0 s=0 u=0
        read-empty0 rem=0 data=
        read-empty0 b=0 p=0 y=0 r=0 f=0 s=0 u=0
        read-empty5 rem=0 data=
        read-empty5 b=0 p=0 y=8 r=0 f=0 s=0 u=0
        read-yaw0 rem=5 data=ab
        read-yaw0 b=0 p=0 y=0 r=0 f=0 s=0 u=0
        read-175 rem=0 data=
        read-175 b=17 p=0 y=4 r=0 f=0 s=0 u=0
        read-stuck rem=1 data=0b
        read-stuck b=0 p=0 y=0 r=0 f=0 s=0 u=0
        read-nest rem=0 data=0a02aabb
        read-nest b=0 p=0 y=-32 r=0 f=0 s=0 u=0
        run-empty
        run-gate
        run-go
        do 4 p=1 rem=3
        do 1 p=1 rem=1
        do 2 p=1 rem=0
        run-again
        do 4 p=1 rem=3
        do 1 p=1 rem=1
        do 2 p=1 rem=0
        iff 010203040000000301020300
        iff-second rem=1
        skip3 ee
        skip-1 1122
        i16 -32768
        i32 -1

        """;
}
