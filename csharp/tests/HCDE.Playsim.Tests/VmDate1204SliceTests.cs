namespace HCDE.Playsim.Tests;

public class VmDate1204SliceTests
{
    const long Now = 1761750245;

    public VmDate1204SliceTests()
    {
        VmDate1204Slice.Reset();
        VmDate1204Slice.Zone = TimeZoneInfo.Utc;
        VmDate1204Slice.ReadClock = () => Now;
    }

    [Fact]
    public void SetDate_MovesTheEpochAndRejectsBadFields()
    {
        Assert.True(VmDate1204Slice.IsInt("0"));
        Assert.True(VmDate1204Slice.IsInt("00"));
        Assert.True(VmDate1204Slice.IsInt("0x10"));
        Assert.False(VmDate1204Slice.IsInt("08"));
        Assert.False(VmDate1204Slice.IsInt("+0"));
        Assert.False(VmDate1204Slice.IsInt(" 0"));
        Assert.Equal(8, VmDate1204Slice.ToLong("010"));
        Assert.Equal(16, VmDate1204Slice.ToLong("0x10"));

        VmDate1204Slice.SetDate("setdate");
        Assert.Equal("setdate HH:MM:SS DD-MM-YYYY: Set the current date\n", VmDate1204Slice.Messages[0]);
        Assert.Equal(0, VmDate1204Slice.EpochOffset);

        VmDate1204Slice.Messages.Clear();
        VmDate1204Slice.SetDate("setdate", "08:00:00", "01-01-1970");
        Assert.Equal("Invalid hour\n", VmDate1204Slice.Messages[0]);
        Assert.Equal(0, VmDate1204Slice.EpochOffset);

        VmDate1204Slice.Messages.Clear();
        VmDate1204Slice.SetDate("setdate", "10:08:00", "01-01-1970");
        Assert.Equal("Invalid minutes\n", VmDate1204Slice.Messages[0]);

        VmDate1204Slice.Messages.Clear();
        VmDate1204Slice.SetDate("setdate", "15:04:05", "29-10-2026");
        Assert.Empty(VmDate1204Slice.Messages);
        Assert.Equal(31536000, VmDate1204Slice.EpochOffset);
        Assert.Equal(1793286245, VmDate1204Slice.GetEpochTime());
        VmDate1204Slice.GetDate();
        Assert.Equal("15:04:05 29-10-2026\n\n", VmDate1204Slice.Messages[0]);

        VmDate1204Slice.Messages.Clear();
        VmDate1204Slice.EpochOffset = 0;
        VmDate1204Slice.SetDate("setdate", "010:00:00", "01-01-1970");
        VmDate1204Slice.GetDate();
        Assert.Equal("08:00:00 01-01-1970\n\n", VmDate1204Slice.Messages[0]);

        VmDate1204Slice.Messages.Clear();
        VmDate1204Slice.EpochOffset = 0;
        VmDate1204Slice.SetDate("setdate", "25:00:00", "01-01-1970");
        VmDate1204Slice.GetDate();
        Assert.Equal("01:00:00 02-01-1970\n\n", VmDate1204Slice.Messages[0]);

        VmDate1204Slice.Messages.Clear();
        VmDate1204Slice.EpochOffset = 0;
        VmDate1204Slice.SetDate("setdate", "10:00:00", "32-01-1970");
        VmDate1204Slice.GetDate();
        Assert.Equal("10:00:00 01-02-1970\n\n", VmDate1204Slice.Messages[0]);

        Assert.Equal(3610, VmDate1204Slice.ApplyHourNudge(10, 14, 15));
        Assert.Equal(3610, VmDate1204Slice.ApplyHourNudge(10, 23, 0));
        Assert.Equal(-3590, VmDate1204Slice.ApplyHourNudge(10, 16, 15));
        Assert.Equal(-3590, VmDate1204Slice.ApplyHourNudge(10, 0, 23));

        VmDate1204Slice.ReadClock = () => null;
        VmDate1204Slice.EpochOffset = 0;
        VmDate1204Slice.Messages.Clear();
        VmDate1204Slice.SetDate("setdate", "15:04:05", "29-10-2026");
        Assert.Equal("Unable to set the date\n", VmDate1204Slice.Messages[0]);
        Assert.Equal(-1, VmDate1204Slice.GetEpochTime());
    }

    [Fact]
    public void Format_MatchesCLocaleAndRejectsBadSpecifiers()
    {
        Assert.Equal("", VmDate1204Slice.CheckStrf("%H:%M"));
        Assert.Equal("", VmDate1204Slice.CheckStrf("%%"));
        Assert.Equal("", VmDate1204Slice.CheckStrf("%%%%"));
        Assert.Equal("%", VmDate1204Slice.CheckStrf("%"));
        Assert.Equal("%", VmDate1204Slice.CheckStrf("%%%"));
        Assert.Equal("%Q", VmDate1204Slice.CheckStrf("%H%Q"));

        var epoch = new Dictionary<char, string>
        {
            ['a'] = "Thu", ['A'] = "Thursday", ['b'] = "Jan", ['B'] = "January", ['c'] = "Thu Jan  1 00:00:00 1970",
            ['C'] = "19", ['d'] = "01", ['D'] = "01/01/70", ['e'] = " 1", ['F'] = "1970-01-01", ['g'] = "70", ['G'] = "1970",
            ['h'] = "Jan", ['H'] = "00", ['I'] = "12", ['j'] = "001", ['m'] = "01", ['M'] = "00", ['n'] = "\n", ['p'] = "AM",
            ['R'] = "00:00", ['r'] = "12:00:00 AM", ['S'] = "00", ['T'] = "00:00:00", ['t'] = "\t", ['u'] = "4", ['U'] = "00",
            ['V'] = "01", ['w'] = "4", ['W'] = "00", ['x'] = "01/01/70", ['X'] = "00:00:00", ['y'] = "70", ['Y'] = "1970",
            ['z'] = "+0000", ['Z'] = "UTC",
        };
        var later = new Dictionary<char, string>
        {
            ['a'] = "Wed", ['A'] = "Wednesday", ['b'] = "Oct", ['B'] = "October", ['c'] = "Wed Oct 29 15:04:05 2025",
            ['C'] = "20", ['d'] = "29", ['D'] = "10/29/25", ['e'] = "29", ['F'] = "2025-10-29", ['g'] = "25", ['G'] = "2025",
            ['h'] = "Oct", ['H'] = "15", ['I'] = "03", ['j'] = "302", ['m'] = "10", ['M'] = "04", ['n'] = "\n", ['p'] = "PM",
            ['R'] = "15:04", ['r'] = "03:04:05 PM", ['S'] = "05", ['T'] = "15:04:05", ['t'] = "\t", ['u'] = "3", ['U'] = "43",
            ['V'] = "44", ['w'] = "3", ['W'] = "43", ['x'] = "10/29/25", ['X'] = "15:04:05", ['y'] = "25", ['Y'] = "2025",
            ['z'] = "+0000", ['Z'] = "UTC",
        };
        foreach (var (spec, expected) in epoch)
            Assert.Equal(expected, VmDate1204Slice.Format("%" + spec, 0));
        foreach (var (spec, expected) in later)
            Assert.Equal(expected, VmDate1204Slice.Format("%" + spec, (int)Now));
        Assert.Equal("%Y", VmDate1204Slice.Format("%%Y", 0));
        Assert.Equal("", VmDate1204Slice.Format("", 0));
        var error = Assert.Throws<VmDate1204Slice.FormatError>(() => VmDate1204Slice.Format("%Q", 0));
        Assert.Equal("VM execution aborted: string format failed. '%Q' is not a valid format specifier of SystemTime.Format()", error.Detail);
    }
}
