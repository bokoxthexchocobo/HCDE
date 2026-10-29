using System.Text;
namespace HCDE.Playsim;
/// <summary>
/// setdate, getdate, and SystemTime format from <c>date.cpp</c> (<b>1–234</b>).
/// Chunk <b>1204</b>. The clock and zone stand in for <c>time</c> and <c>localtime</c>.
/// </summary>
public static class VmDate1204Slice
{
    public static long EpochOffset;
    public static readonly List<string> Messages = new();
    public static TimeZoneInfo Zone = TimeZoneInfo.Local;
    public static Func<long?> ReadClock = DefaultClock;
    static readonly string[] ShortDays = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
    static readonly string[] LongDays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
    static readonly string[] ShortMonths = { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
    static readonly string[] LongMonths = { "", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
    public sealed class FormatError : Exception
    {
        public FormatError(string message) : base(message) { Detail = message; }
        public string Detail;
    }
    public static void Reset()
    {
        EpochOffset = 0;
        Messages.Clear();
        Zone = TimeZoneInfo.Local;
        ReadClock = DefaultClock;
    }
    static long? DefaultClock()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
    public static void SetDate(params string[] argv)
    {
        if (argv.Length != 3)
        {
            Messages.Add("setdate HH:MM:SS DD-MM-YYYY: Set the current date\n");
            return;
        }
        long? now = ReadClock();
        if (now == null || !TryLocal(now.Value, out var today))
        {
            Messages.Add("Unable to set the date\n");
            return;
        }
        string[] clock = argv[1].Split(':');
        string[] date = argv[2].Split('-');
        if (clock.Length != 3 || date.Length != 3)
        {
            Messages.Add("setdate HH:MM:SS DD-MM-YYYY: Set the current date\n");
            return;
        }
        string[] fields = { clock[0], clock[1], clock[2], date[0], date[1], date[2] };
        string[] names = { "hour", "minutes", "seconds", "day", "month", "year" };
        for (int i = 0; i < fields.Length; i++)
        {
            if (!IsInt(fields[i]))
            {
                Messages.Add($"Invalid {names[i]}\n");
                return;
            }
        }
        bool made = TryUnix(unchecked((int)ToLong(date[2])), unchecked((int)ToLong(date[1])) - 1, unchecked((int)ToLong(date[0])), unchecked((int)ToLong(clock[0])), unchecked((int)ToLong(clock[1])), unchecked((int)ToLong(clock[2])), out long newTime);
        bool kept = TryUnix(today.Year, today.Month - 1, today.Day, today.Hour, today.Minute, today.Second, out long oldTime);
        if (!made || !kept)
        {
            Messages.Add("Unable to set the date\n");
            return;
        }
        EpochOffset = newTime - oldTime;
        if (TryLocal(now.Value + EpochOffset, out var shown))
        {
            string hourText = Strf("%H", shown);
            if (hourText.Length != 0)
                EpochOffset = ApplyHourNudge(EpochOffset, ToLong(hourText), ToLong(clock[0]));
        }
    }
    public static void GetDate()
    {
        long? now = ReadClock();
        if (now == null || !TryLocal(now.Value + EpochOffset, out var info))
        {
            Messages.Add("Error Retrieving Current Date\n");
            return;
        }
        string text = Strf("%H:%M:%S %d-%m-%Y%n", info);
        Messages.Add(text.Length == 0 ? "Error Retrieving Current Date\n" : text + "\n");
    }
    public static int GetEpochTime()
    {
        long? now = ReadClock();
        if (now == null || now.Value == -1)
            return -1;
        return unchecked((int)(now.Value + EpochOffset));
    }
    public static string Format(string timeForm, int timeVal)
    {
        string error = CheckStrf(timeForm);
        if (error.Length != 0)
            throw new FormatError("VM execution aborted: string format failed. '" + error + "' is not a valid format specifier of SystemTime.Format()");
        if (!TryLocal(timeVal, out var info))
            return "";
        return Strf(timeForm, info);
    }
    public static long ApplyHourNudge(long offset, long shownHour, long requestedHour)
    {
        long diff = shownHour - requestedHour;
        if (diff == -1 || diff == 23)
            return offset + 3600;
        if (diff == 1 || diff == -23)
            return offset - 3600;
        return offset;
    }
    public static string CheckStrf(string timeForm)
    {
        const string valid = "aAbBcCdDeFgGhHIjmMnpRrSTtTuUVwWxXyYzZ";
        foreach (string token in ("a" + timeForm.Replace("%%", "%a")).Split('%'))
        {
            if (token.Length == 0)
                return "%";
            if (valid.IndexOf(token[0]) < 0)
                return "%" + token[0];
        }
        return "";
    }
    public static bool IsInt(string text)
    {
        if (text.Length == 1 && text[0] == '0')
            return true;
        int i = 0;
        while (i < text.Length && IsSpace(text[i]))
            i++;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
            i++;
        if (i >= text.Length)
            return false;
        if (text[i] == '0')
        {
            i++;
            if (i < text.Length && text[i] is >= '0' and <= '7')
            {
                do i++;
                while (i < text.Length && text[i] is >= '0' and <= '7');
            }
            else if (i < text.Length && text[i] is 'x' or 'X')
            {
                i++;
                int start = i;
                while (i < text.Length && Hex(text[i]) >= 0)
                    i++;
                if (i == start)
                    return false;
            }
            else
                return false;
        }
        else if (text[i] is >= '1' and <= '9')
        {
            do i++;
            while (i < text.Length && text[i] is >= '0' and <= '9');
        }
        else
            return false;
        while (i < text.Length && IsSpace(text[i]))
            i++;
        return i == text.Length;
    }
    public static long ToLong(string text)
    {
        int i = 0;
        while (i < text.Length && IsSpace(text[i]))
            i++;
        int sign = 1;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
        {
            if (text[i] == '-')
                sign = -1;
            i++;
        }
        int radix = 10;
        if (i < text.Length && text[i] == '0')
        {
            if (i + 1 < text.Length && text[i + 1] is 'x' or 'X')
            {
                radix = 16;
                i += 2;
            }
            else
                radix = 8;
        }
        long value = 0;
        bool any = false;
        while (i < text.Length)
        {
            int digit = Hex(text[i]);
            if (digit < 0 || digit >= radix)
                break;
            any = true;
            if (value > (long.MaxValue - digit) / radix)
                return sign < 0 ? long.MinValue : long.MaxValue;
            value = value * radix + digit;
            i++;
        }
        return any ? (sign < 0 ? -value : value) : 0;
    }
    static string Strf(string format, DateTime dt)
    {
        var built = new StringBuilder();
        for (int i = 0; i < format.Length; i++)
        {
            if (format[i] != '%')
                built.Append(format[i]);
            else if (i + 1 < format.Length)
                built.Append(Piece(format[++i], dt));
            else
                built.Append('%');
            if (built.Length > 1023)
                return "";
        }
        return built.ToString();
    }
    static string Piece(char spec, DateTime dt)
    {
        int hour12 = dt.Hour % 12 == 0 ? 12 : dt.Hour % 12;
        string ampm = dt.Hour < 12 ? "AM" : "PM";
        return spec switch
        {
            'a' => ShortDays[(int)dt.DayOfWeek],
            'A' => LongDays[(int)dt.DayOfWeek],
            'b' or 'h' => ShortMonths[dt.Month],
            'B' => LongMonths[dt.Month],
            'c' => $"{ShortDays[(int)dt.DayOfWeek]} {ShortMonths[dt.Month]} {dt.Day,2} {dt:HH:mm:ss} {dt.Year:0000}",
            'C' => (dt.Year / 100).ToString("00"),
            'd' => dt.Day.ToString("00"),
            'D' or 'x' => $"{dt:MM}/{dt:dd}/{dt.Year % 100:00}",
            'e' => dt.Day.ToString().PadLeft(2),
            'F' => $"{dt:yyyy-MM-dd}",
            'g' => (IsoYear(dt) % 100).ToString("00"),
            'G' => IsoYear(dt).ToString("0000"),
            'H' => dt.Hour.ToString("00"),
            'I' => hour12.ToString("00"),
            'j' => dt.DayOfYear.ToString("000"),
            'm' => dt.Month.ToString("00"),
            'M' => dt.Minute.ToString("00"),
            'n' => "\n",
            'p' => ampm,
            'r' => $"{hour12:00}:{dt.Minute:00}:{dt.Second:00} {ampm}",
            'R' => $"{dt:HH:mm}",
            'S' => dt.Second.ToString("00"),
            't' => "\t",
            'T' or 'X' => $"{dt:HH:mm:ss}",
            'u' => (dt.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dt.DayOfWeek).ToString(),
            'U' => SunWeek(dt).ToString("00"),
            'V' => IsoWeek(dt).ToString("00"),
            'w' => ((int)dt.DayOfWeek).ToString(),
            'W' => MonWeek(dt).ToString("00"),
            'y' => (dt.Year % 100).ToString("00"),
            '%' => "%",
            'Y' => dt.Year.ToString("0000"),
            'z' => OffsetText(dt),
            'Z' => Zone.Id == "UTC" ? "UTC" : (Zone.IsDaylightSavingTime(dt) ? Zone.DaylightName : Zone.StandardName),
            _ => "",
        };
    }
    static bool TryLocal(long unix, out DateTime local)
    {
        local = default;
        try
        {
            local = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(unix), Zone).DateTime;
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
    static bool TryUnix(int year, int month, int day, int hour, int minute, int second, out long unix)
    {
        unix = 0;
        try
        {
            var wall = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
            wall = wall.AddMonths(month).AddDays(day - 1).AddHours(hour).AddMinutes(minute).AddSeconds(second);
            unix = new DateTimeOffset(wall, Zone.GetUtcOffset(wall)).ToUnixTimeSeconds();
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
    static string OffsetText(DateTime dt)
    {
        int minutes = (int)Zone.GetUtcOffset(dt).TotalMinutes;
        int abs = Math.Abs(minutes);
        return $"{(minutes < 0 ? '-' : '+')}{abs / 60:00}{abs % 60:00}";
    }
    static int IsoYear(DateTime dt) => dt.Date.AddDays(4 - IsoDow(dt)).Year;
    static int IsoWeek(DateTime dt)
    {
        var thursday = dt.Date.AddDays(4 - IsoDow(dt));
        var jan4 = new DateTime(thursday.Year, 1, 4);
        return (thursday - jan4.AddDays(1 - IsoDow(jan4))).Days / 7 + 1;
    }
    static int IsoDow(DateTime dt) => dt.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dt.DayOfWeek;
    static int SunWeek(DateTime dt) => (dt.DayOfYear - 1 - (int)dt.DayOfWeek + 7) / 7;
    static int MonWeek(DateTime dt) => (dt.DayOfYear - 1 - (((int)dt.DayOfWeek + 6) % 7) + 7) / 7;
    static int Hex(char c)
    {
        if (c is >= '0' and <= '9')
            return c - '0';
        if (c is >= 'a' and <= 'f')
            return c - 'a' + 10;
        if (c is >= 'A' and <= 'F')
            return c - 'A' + 10;
        return -1;
    }
    static bool IsSpace(char c) => c is ' ' or '\t' or '\n' or '\v' or '\f' or '\r';
}
