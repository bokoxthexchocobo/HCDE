/*
 wildmidi_lib.c

 Midi Wavetable Processing library

 Copyright (C) Chris Ison  2001-2014
 Copyright (C) Bret Curtis 2013-2014

 This file is part of WildMIDI.

 WildMIDI is free software: you can redistribute and/or modify the player
 under the terms of the GNU General Public License and you can redistribute
 and/or modify the library under the terms of the GNU Lesser General Public
 License as published by the Free Software Foundation, either version 3 of
 the licenses, or(at your option) any later version.

 WildMIDI is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License and
 the GNU Lesser General Public License for more details.

 You should have received a copy of the GNU General Public License and the
 GNU Lesser General Public License along with WildMIDI.  If not,  see
 <http://www.gnu.org/licenses/>.
 */

namespace HCDE.Client;

public readonly record struct VmMusicWildMidiMixVolume(int Left, int Right);

public static class VmMusicWildMidiNoteVolume
{
    public static int MasterVolume(byte volume) => LinearVolume[Math.Min(127, (int)volume)];

    public static void Adjust(IReadOnlyList<VmMusicWildMidiChannelState> channels, int channel,
        VmMusicWildMidiNoteReleaseState voice, int masterVolume, bool logarithmic)
    {
        ArgumentNullException.ThrowIfNull(channels);
        ArgumentNullException.ThrowIfNull(voice);
        if (channels.Count != 16) throw new ArgumentException("WildMIDI volume adjustment requires sixteen channels.", nameof(channels));
        if (channel is < 0 or > 255) throw new ArgumentOutOfRangeException(nameof(channel));
        voice.MixVolume = Compute(channels[Math.Min(channel, 15)], voice.Velocity, masterVolume, logarithmic);
    }

    public static VmMusicWildMidiMixVolume Compute(VmMusicWildMidiChannelState channel, byte velocity,
        int masterVolume, bool logarithmic)
    {
        ArgumentNullException.ThrowIfNull(channel);
        var volumeIndex = velocity * (channel.Expression * channel.Volume / 127) / 127;
        if (volumeIndex >= LinearVolume.Length)
            throw new ArgumentOutOfRangeException(nameof(velocity), "Combined MIDI volume exceeds the native table.");
        var pan = Math.Max(0, Math.Min(127, channel.Balance + channel.Pan - 64) - 1);
        double leftDb = PanVolume[126 - pan], rightDb = PanVolume[pan];
        double scale = ((float)masterVolume / 1024.0) / 4.0;
        double left, right;
        if (logarithmic)
        {
            left = Math.Pow(10.0, (leftDb + LogVolume[volumeIndex]) / 20.0) * scale;
            right = Math.Pow(10.0, (rightDb + LogVolume[volumeIndex]) / 20.0) * scale;
        }
        else
        {
            double linear = (float)LinearVolume[volumeIndex] / 1024.0;
            left = linear * Math.Pow(10.0, leftDb / 20.0) * scale;
            right = linear * Math.Pow(10.0, rightDb / 20.0) * scale;
        }
        return new((int)(left * 1024.0), (int)(right * 1024.0));
    }
    private static readonly short[] LinearVolume = [ 0, 8, 16, 24, 32, 40, 48, 56, 64, 72,
		80, 88, 96, 104, 112, 120, 129, 137, 145, 153, 161, 169, 177, 185, 193,
		201, 209, 217, 225, 233, 241, 249, 258, 266, 274, 282, 290, 298, 306,
		314, 322, 330, 338, 346, 354, 362, 370, 378, 387, 395, 403, 411, 419,
		427, 435, 443, 451, 459, 467, 475, 483, 491, 499, 507, 516, 524, 532,
		540, 548, 556, 564, 572, 580, 588, 596, 604, 612, 620, 628, 636, 645,
		653, 661, 669, 677, 685, 693, 701, 709, 717, 725, 733, 741, 749, 757,
		765, 774, 782, 790, 798, 806, 814, 822, 830, 838, 846, 854, 862, 870,
		878, 886, 894, 903, 911, 919, 927, 935, 943, 951, 959, 967, 975, 983,
		991, 999, 1007, 1015, 1024 ];
    private static readonly float[] LogVolume = [ -999999.999999f, -84.15214884f, -72.11094901f,
	-65.06729865f, -60.06974919f, -56.19334866f, -53.02609882f, -50.34822724f,
	-48.02854936f, -45.98244846f, -44.15214884f, -42.49644143f, -40.984899f,
	-39.59441475f, -38.30702741f, -37.10849848f, -35.98734953f, -34.93419198f,
	-33.94124863f, -33.0020048f, -32.11094901f, -31.26337705f, -30.45524161f,
	-29.6830354f, -28.94369917f, -28.23454849f, -27.55321492f, -26.89759827f,
	-26.26582758f, -25.65622892f, -25.06729865f, -24.49768108f, -23.94614971f,
	-23.41159124f, -22.89299216f, -22.38942706f, -21.90004881f, -21.42407988f,
	-20.96080497f, -20.50956456f, -20.06974919f, -19.64079457f, -19.22217722f,
	-18.81341062f, -18.41404178f, -18.02364829f, -17.64183557f, -17.26823452f,
	-16.90249934f, -16.54430564f, -16.19334866f, -15.84934179f, -15.51201509f,
	-15.18111405f, -14.85639845f, -14.53764126f, -14.22462776f, -13.91715461f,
	-13.6150291f, -13.31806837f, -13.02609882f, -12.73895544f, -12.45648126f,
	-12.17852686f, -11.90494988f, -11.63561457f, -11.37039142f, -11.10915673f,
	-10.85179233f, -10.59818521f, -10.34822724f, -10.10181489f, -9.858848981f,
	-9.619234433f, -9.382880049f, -9.149698303f, -8.919605147f, -8.692519831f,
	-8.468364731f, -8.247065187f, -8.028549359f, -7.812748083f, -7.599594743f,
	-7.389025143f, -7.180977396f, -6.97539181f, -6.772210788f, -6.571378733f,
	-6.372841952f, -6.176548572f, -5.982448461f, -5.790493145f, -5.600635744f,
	-5.412830896f, -5.227034694f, -5.043204627f, -4.861299517f, -4.681279468f,
	-4.503105811f, -4.326741054f, -4.152148838f, -3.979293887f, -3.808141968f,
	-3.63865985f, -3.470815266f, -3.304576875f, -3.139914228f, -2.976797731f,
	-2.815198619f, -2.655088921f, -2.496441432f, -2.339229687f, -2.183427931f,
	-2.029011099f, -1.875954785f, -1.724235224f, -1.573829269f, -1.424714368f,
	-1.276868546f, -1.130270383f, -0.9848989963f, -0.8407340256f, -0.6977556112f,
	-0.5559443807f, -0.4152814317f, -0.2757483179f, -0.1373270335f, 0 ];
    private static readonly float[] PanVolume = [
	-999999.999999f, -87.6945020928f, -73.8331126923f, -65.7264009888f,
	-59.9763864074f, -55.5181788833f, -51.8774481743f, -48.8011722841f,
	-46.1383198371f, -43.7914727130f, -41.6941147218f, -39.7988027954f,
	-38.0705069530f, -36.4826252703f, -35.0144798827f, -33.6496789707f,
	-32.3750084716f, -31.1796603753f, -30.0546819321f, -28.9925739783f,
	-27.9869924122f, -27.0325225804f, -26.1245061976f, -25.2589067713f,
	-24.4322036893f, -23.6413079424f, -22.8834943857f, -22.1563467917f,
	-21.4577129008f, -20.7856673630f, -20.1384809653f, -19.5145949062f,
	-18.9125991563f, -18.3312141503f, -17.7692752119f, -17.2257192381f,
	-16.6995732597f, -16.1899445690f, -15.6960121652f, -15.2170193110f,
	-14.7522670314f, -14.3011084168f, -13.8629436112f, -13.4372153915f,
	-13.0234052546f, -12.6210299451f, -12.2296383638f, -11.8488088095f,
	-11.4781465116f, -11.1172814164f, -10.7658661983f, -10.4235744668f,
	-10.0900991491f, -9.7651510261f, -9.4484574055f, -9.1397609172f,
	-8.8388184168f, -8.5453999868f, -8.2592880250f, -7.9802764101f,
	-7.7081697387f, -7.4427826248f, -7.1839390567f, -6.9314718056f,
	-6.6852218807f, -6.4450380272f, -6.2107762624f, -5.9822994468f,
	-5.7594768878f, -5.5421839719f, -5.3303018237f, -5.1237169899f,
	-4.9223211445f, -4.7260108155f, -4.5346871303f, -4.3482555779f,
	-4.1666257875f, -3.9897113219f, -3.8174294843f, -3.6497011373f,
	-3.4864505345f, -3.3276051620f, -3.1730955900f, -3.0228553340f,
	-2.8768207245f, -2.7349307844f, -2.5971271143f, -2.4633537845f,
	-2.3335572335f, -2.2076861725f, -2.0856914960f, -1.9675261968f,
	-1.8531452871f, -1.7425057233f, -1.6355663356f, -1.5322877618f,
	-1.4326323846f, -1.3365642732f, -1.2440491272f, -1.1550542250f,
	-1.0695483746f, -0.9875018671f, -0.9088864335f, -0.8336752037f,
	-0.7618426682f, -0.6933646420f, -0.6282182304f, -0.5663817981f,
	-0.5078349388f, -0.4525584478f, -0.4005342959f, -0.3517456058f,
	-0.3061766293f, -0.2638127266f, -0.2246403475f, -0.1886470134f,
	-0.1558213016f, -0.1261528303f, -0.0996322457f, -0.0762512098f,
	-0.0560023899f, -0.0388794497f, -0.0248770409f, -0.0139907967f,
	-0.0062173263f, -0.0015542108f, 0.0000000000f ];
}
