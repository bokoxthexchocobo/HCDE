namespace HCDE.Playsim;

/// <summary><c>P_TouchSpecialThing</c>, <c>PronounMessage</c>, and <c>ClientObituary</c> from <c>p_interaction.cpp</c> (85–321).</summary>
public static class VmPInteractionTouchObituary
{
    private static readonly string[][] Pronouns =
    {
        ["he", "him", "his", "his", "he's"],
        ["she", "her", "her", "hers", "she's"],
        ["they", "them", "their", "theirs", "they're"],
        ["it", "it", "its", "its'", "it's"],
    };

    private static readonly int[][] PronounShift =
    {
        [2, 3, 3, 3, 4],
        [3, 3, 3, 4, 5],
        [4, 4, 5, 6, 7],
        [2, 2, 3, 4, 4],
    };

    public sealed class TouchSpecialThingState
    {
        public double SpecialZ { get; set; }
        public double ToucherZ { get; set; }
        public double ToucherHeight { get; set; }
        public double SpecialHeight { get; set; }
        public int ToucherHealth { get; set; }
        public bool HasPlayer { get; set; }
        public bool HasBot { get; set; }
        public bool DestIsSpecial { get; set; }
        public object? BotPrev { get; set; }
        public object? BotDest { get; set; }
        public Action? CallTouch { get; init; }
    }

    public static void TouchSpecialThing(TouchSpecialThingState s)
    {
        var delta = s.SpecialZ - s.ToucherZ;
        if (delta > s.ToucherHeight || delta < Math.Min(-32.0, -s.SpecialHeight))
            return;

        if (s.ToucherHealth <= 0)
            return;

        if (s.HasPlayer && s.HasBot && s.DestIsSpecial)
        {
            s.BotPrev = s.BotDest;
            s.BotDest = null;
        }

        s.CallTouch?.Invoke();
    }

    public static void PronounMessage(string from, Span<char> to, int pronoun, string? victim, string? killer)
    {
        var outIdx = 0;
        var fromIdx = 0;
        while (fromIdx < from.Length)
        {
            if (from[fromIdx] != '%')
            {
                to[outIdx++] = from[fromIdx++];
                continue;
            }

            var grammarCase = -1;
            string? substitute = null;
            if (fromIdx + 1 < from.Length)
            {
                switch (from[fromIdx + 1])
                {
                    case 'g': grammarCase = 0; break;
                    case 'h': grammarCase = 1; break;
                    case 'p': grammarCase = 2; break;
                    case 's': grammarCase = 3; break;
                    case 'r': grammarCase = 4; break;
                    case 'o': substitute = victim; break;
                    case 'k': substitute = killer; break;
                }
            }

            if (substitute != null)
            {
                foreach (var ch in substitute)
                    to[outIdx++] = ch;
                fromIdx += 2;
                continue;
            }

            if (grammarCase < 0)
            {
                to[outIdx++] = '%';
                fromIdx++;
                continue;
            }

            if (pronoun >= 0 && pronoun < Pronouns.Length && grammarCase >= 0 && grammarCase < Pronouns[pronoun].Length)
            {
                var word = Pronouns[pronoun][grammarCase];
                foreach (var ch in word)
                    to[outIdx++] = ch;
            }

            fromIdx += 2;
        }

        if (outIdx < to.Length)
            to[outIdx] = '\0';
    }

    public sealed class ClientObituaryState
    {
        public bool HasPlayer { get; set; }
        public bool MoIsSelf { get; set; }
        public bool ShowObituaries { get; set; }
        public bool InflictorPlayer { get; set; }
        public bool InflictorMoIsSelf { get; set; }
        public bool AttackerNull { get; set; }
        public bool AttackerIsPlayer { get; set; }
        public bool AttackerIsSelf { get; set; }
        public int ModIndex { get; set; }
        public int NameTelefrag { get; set; }
        public int NameSuicide { get; set; }
        public int NameFalling { get; set; }
        public int NameCrush { get; set; }
        public int NameExit { get; set; }
        public int NameDrowning { get; set; }
        public int NameSlime { get; set; }
        public int NameFire { get; set; }
        public int NameMelee { get; set; }
        public int NameNone { get; set; }
        public bool DmgPlayerAttack { get; set; }
        public int Gender { get; set; }
        public string? VictimName { get; set; }
        public string? AttackerName { get; set; }
        public string? AttackerClassName { get; set; }
        public string? ModName { get; set; }
        public string? DamageTypeObit { get; set; }
        public string? Message { get; set; }
        public string? MessageName { get; set; }
        public string? PrintedMessage { get; set; }
        public bool KillfeedEnabled { get; set; }
        public int LevelTime { get; set; }
        public Func<bool>? AnnounceTelefrag { get; init; }
        public Func<bool>? AnnounceKill { get; init; }
        public Func<string?, bool>? CheckString { get; init; }
        public Func<string, int, string?>? ResolveString { get; init; }
        public Func<string?>? GetSelfObituaryVirtual { get; init; }
        public Func<string?>? GetObituaryVirtual { get; init; }
        public Action<string>? Printf { get; init; }
        public Action<string, int>? KillfeedPush { get; init; }
    }

    public static void ClientObituary(ClientObituaryState s)
    {
        if (!s.HasPlayer || !s.MoIsSelf || !s.ShowObituaries)
            return;

        var modIndex = s.ModIndex;
        if (s.InflictorPlayer && !s.InflictorMoIsSelf)
            modIndex = s.NameNone;

        string? message = s.Message;
        string? messagename = s.MessageName;

        if (s.AttackerNull || s.AttackerIsPlayer)
        {
            if (modIndex == s.NameTelefrag)
            {
                if (s.AnnounceTelefrag?.Invoke() == true)
                    return;
            }
            else if (s.AnnounceKill?.Invoke() == true)
                return;
        }

        if (s.AttackerNull && !string.IsNullOrEmpty(s.DamageTypeObit))
            messagename = s.DamageTypeObit;
        else
        {
            switch (modIndex)
            {
                case var i when i == s.NameSuicide: messagename = "$OB_SUICIDE"; break;
                case var i when i == s.NameFalling: messagename = "$OB_FALLING"; break;
                case var i when i == s.NameCrush: messagename = "$OB_CRUSH"; break;
                case var i when i == s.NameExit: messagename = "$OB_EXIT"; break;
                case var i when i == s.NameDrowning: messagename = "$OB_WATER"; break;
                case var i when i == s.NameSlime: messagename = "$OB_SLIME"; break;
                case var i when i == s.NameFire: messagename = "$OB_LAVA"; break;
            }
        }

        if (s.InflictorPlayer && !s.InflictorMoIsSelf)
            messagename = "$OB_VOODOO";

        if (!s.AttackerNull && message == null)
        {
            if (s.AttackerIsSelf)
            {
                messagename = "$OB_KILLEDSELF";
                var selfMsg = s.GetSelfObituaryVirtual?.Invoke();
                if (!string.IsNullOrEmpty(selfMsg))
                    message = selfMsg;
            }
            else
            {
                var classMod = $"$Obituary_{s.AttackerClassName}_{s.ModName}";
                if (s.CheckString?.Invoke(classMod) == true)
                    message = classMod;
                else
                {
                    var classOnly = $"$Obituary_{s.AttackerClassName}";
                    if (s.CheckString?.Invoke(classOnly) == true)
                        message = classOnly;
                    else
                    {
                        var virt = s.GetObituaryVirtual?.Invoke();
                        if (!string.IsNullOrEmpty(virt))
                            message = virt;
                    }
                }
            }
        }

        if (message == null)
            message = messagename;

        var attackerNameForMessage = s.AttackerName;
        if (s.AttackerNull)
            attackerNameForMessage = s.VictimName;
        else if (!s.AttackerIsPlayer)
            attackerNameForMessage = s.VictimName;

        if (message != null && message.Length > 0 && message[0] == '$')
            message = s.ResolveString?.Invoke(message[1..], s.Gender);

        if (message == null)
        {
            if (modIndex == s.NameMelee)
            {
                var melee = $"DEFHITOB_{s.AttackerClassName}";
                message = s.ResolveString?.Invoke(melee, s.Gender);
            }

            if (message == null)
            {
                var def = $"DEFOB_{s.AttackerClassName}";
                message = s.ResolveString?.Invoke(def, s.Gender);
            }

            if (message == null)
                message = s.ResolveString?.Invoke("OB_DEFAULT", s.Gender);
        }

        if (message == null || message.Length == 0)
            return;

        Span<char> genderBuf = stackalloc char[1024];
        PronounMessage(message, genderBuf, s.Gender, s.VictimName, attackerNameForMessage);
        var printed = genderBuf.Slice(0, genderBuf.IndexOf('\0'));
        s.PrintedMessage = printed.ToString();
        s.Printf?.Invoke(s.PrintedMessage);

        if (s.KillfeedEnabled)
            s.KillfeedPush?.Invoke(s.PrintedMessage, s.LevelTime);
    }
}
