namespace HCDE.Playsim;

/// <summary>
/// Continued ACS P-code opcode constants from the engine enum in <c>p_acs.cpp</c> (**151–300** / opcodes **71–220**).
/// Exclusive to chunk **969**.
/// </summary>
public static class VmPAcsPcodeEnumContinued969Slice
{
    public const int PcdOrlogical = 71;
    public const int PcdAndbitwise = 72;
    public const int PcdOrbitwise = 73;
    public const int PcdEorbitwise = 74;
    public const int PcdNegatelogical = 75;
    public const int PcdLshift = 76;
    public const int PcdRshift = 77;
    public const int PcdUnaryminus = 78;
    public const int PcdIfnotgoto = 79;
    public const int PcdLineside = 80;
    public const int PcdScriptwait = 81;
    public const int PcdScriptwaitdirect = 82;
    public const int PcdClearlinespecial = 83;
    public const int PcdCasegoto = 84;
    public const int PcdBeginprint = 85;
    public const int PcdEndprint = 86;
    public const int PcdPrintstring = 87;
    public const int PcdPrintnumber = 88;
    public const int PcdPrintcharacter = 89;
    public const int PcdPlayercount = 90;
    public const int PcdGametype = 91;
    public const int PcdGameskill = 92;
    public const int PcdTimer = 93;
    public const int PcdSectorsound = 94;
    public const int PcdAmbientsound = 95;
    public const int PcdSoundsequence = 96;
    public const int PcdSetlinetexture = 97;
    public const int PcdSetlineblocking = 98;
    public const int PcdSetlinespecial = 99;
    public const int PcdThingsound = 100;
    public const int PcdEndprintbold = 101;
    public const int PcdActivatorsound = 102;
    public const int PcdLocalambientsound = 103;
    public const int PcdSetlinemonsterblocking = 104;
    public const int PcdPlayerblueskull = 105;
    public const int PcdPlayerredskull = 106;
    public const int PcdPlayeryellowskull = 107;
    public const int PcdPlayermasterskull = 108;
    public const int PcdPlayerbluecard = 109;
    public const int PcdPlayerredcard = 110;
    public const int PcdPlayeryellowcard = 111;
    public const int PcdPlayermastercard = 112;
    public const int PcdPlayerblackskull = 113;
    public const int PcdPlayersilverskull = 114;
    public const int PcdPlayergoldskull = 115;
    public const int PcdPlayerblackcard = 116;
    public const int PcdPlayersilvercard = 117;
    public const int PcdIsnetworkgame = 118;
    public const int PcdPlayerteam = 119;
    public const int PcdPlayerhealth = 120;
    public const int PcdPlayerarmorpoints = 121;
    public const int PcdPlayerfrags = 122;
    public const int PcdPlayerexpert = 123;
    public const int PcdBlueteamcount = 124;
    public const int PcdRedteamcount = 125;
    public const int PcdBlueteamscore = 126;
    public const int PcdRedteamscore = 127;
    public const int PcdIsoneflagctf = 128;
    public const int PcdLspec6 = 129;
    public const int PcdLspec6direct = 130;
    public const int PcdPrintname = 131;
    public const int PcdMusicchange = 132;
    public const int PcdConsolecommanddirect = 133;
    public const int PcdConsolecommand = 134;
    public const int PcdSingleplayer = 135;
    public const int PcdFixedmul = 136;
    public const int PcdFixeddiv = 137;
    public const int PcdSetgravity = 138;
    public const int PcdSetgravitydirect = 139;
    public const int PcdSetaircontrol = 140;
    public const int PcdSetaircontroldirect = 141;
    public const int PcdClearinventory = 142;
    public const int PcdGiveinventory = 143;
    public const int PcdGiveinventorydirect = 144;
    public const int PcdTakeinventory = 145;
    public const int PcdTakeinventorydirect = 146;
    public const int PcdCheckinventory = 147;
    public const int PcdCheckinventorydirect = 148;
    public const int PcdSpawn = 149;
    public const int PcdSpawndirect = 150;
    public const int PcdSpawnspot = 151;
    public const int PcdSpawnspotdirect = 152;
    public const int PcdSetmusic = 153;
    public const int PcdSetmusicdirect = 154;
    public const int PcdLocalsetmusic = 155;
    public const int PcdLocalsetmusicdirect = 156;
    public const int PcdPrintfixed = 157;
    public const int PcdPrintlocalized = 158;
    public const int PcdMorehudmessage = 159;
    public const int PcdOpthudmessage = 160;
    public const int PcdEndhudmessage = 161;
    public const int PcdEndhudmessagebold = 162;
    public const int PcdSetstyle = 163;
    public const int PcdSetstyledirect = 164;
    public const int PcdSetfont = 165;
    public const int PcdSetfontdirect = 166;
    public const int PcdPushbyte = 167;
    public const int PcdLspec1directb = 168;
    public const int PcdLspec2directb = 169;
    public const int PcdLspec3directb = 170;
    public const int PcdLspec4directb = 171;
    public const int PcdLspec5directb = 172;
    public const int PcdDelaydirectb = 173;
    public const int PcdRandomdirectb = 174;
    public const int PcdPushbytes = 175;
    public const int PcdPush2bytes = 176;
    public const int PcdPush3bytes = 177;
    public const int PcdPush4bytes = 178;
    public const int PcdPush5bytes = 179;
    public const int PcdSetthingspecial = 180;
    public const int PcdAssignglobalvar = 181;
    public const int PcdPushglobalvar = 182;
    public const int PcdAddglobalvar = 183;
    public const int PcdSubglobalvar = 184;
    public const int PcdMulglobalvar = 185;
    public const int PcdDivglobalvar = 186;
    public const int PcdModglobalvar = 187;
    public const int PcdIncglobalvar = 188;
    public const int PcdDecglobalvar = 189;
    public const int PcdFadeto = 190;
    public const int PcdFaderange = 191;
    public const int PcdCancelfade = 192;
    public const int PcdPlaymovie = 193;
    public const int PcdSetfloortrigger = 194;
    public const int PcdSetceilingtrigger = 195;
    public const int PcdGetactorx = 196;
    public const int PcdGetactory = 197;
    public const int PcdGetactorz = 198;
    public const int PcdStarttranslation = 199;
    public const int PcdTranslationrange1 = 200;
    public const int PcdTranslationrange2 = 201;
    public const int PcdEndtranslation = 202;
    public const int PcdCall = 203;
    public const int PcdCalldiscard = 204;
    public const int PcdReturnvoid = 205;
    public const int PcdReturnval = 206;
    public const int PcdPushmaparray = 207;
    public const int PcdAssignmaparray = 208;
    public const int PcdAddmaparray = 209;
    public const int PcdSubmaparray = 210;
    public const int PcdMulmaparray = 211;
    public const int PcdDivmaparray = 212;
    public const int PcdModmaparray = 213;
    public const int PcdIncmaparray = 214;
    public const int PcdDecmaparray = 215;
    public const int PcdDup = 216;
    public const int PcdSwap = 217;
    public const int PcdWritetoini = 218;
    public const int PcdGetfromini = 219;
    public const int PcdSin = 220;

    public const int ContinuedOpcodeMin = PcdOrlogical;
    public const int ContinuedOpcodeMax = PcdSin;

    public static bool IsContinuedOpcode(int opcode) =>
        opcode >= ContinuedOpcodeMin && opcode <= ContinuedOpcodeMax;

    public static bool IsLogicalOpcode(int opcode) =>
        opcode >= PcdOrlogical && opcode <= PcdNegatelogical;

    public static bool IsPrintClusterOpcode(int opcode) =>
        opcode >= PcdBeginprint && opcode <= PcdPrintcharacter;

    public static bool IsInventoryOpcode(int opcode) =>
        opcode >= PcdClearinventory && opcode <= PcdCheckinventorydirect;

    public static bool IsStackManipOpcode(int opcode) =>
        opcode == PcdDup || opcode == PcdSwap;

    public static bool IsCallOpcode(int opcode) =>
        opcode == PcdCall || opcode == PcdCalldiscard;
}
