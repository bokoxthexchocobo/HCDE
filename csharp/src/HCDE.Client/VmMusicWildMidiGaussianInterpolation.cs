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

public static class VmMusicWildMidiGaussianInterpolation
{
    private const int Order = 34;
    private static readonly Lazy<(double[,] Newton, double[] Gaussian)> Tables = new(CreateTables);

    public static VmMusicWildMidiMixVolume Interpolate(VmMusicWildMidiSampleData sample,
        VmMusicWildMidiNoteReleaseState voice)
    {
        ArgumentNullException.ThrowIfNull(voice);
        var data = sample.Data.Span;
        var position = (int)(voice.SamplePosition >> 10);
        var right = (int)(sample.Bounds.DataLength >> 10) - position - 1;
        var order = Math.Min(Math.Max((right << 1) - 1, 1), (position << 1) + 1);
        var windowOrder = Math.Min(order, Order);
        var start = position - (windowOrder >> 1);
        if (start < 0 || start > data.Length - windowOrder - 1)
            throw new ArgumentException("Enhanced interpolation requires the full sample window and boundary guard samples.", nameof(sample));
        var tables = Tables.Value;
        double value = 0;
        var fraction = (int)(voice.SamplePosition & 1023);
        if (order < Order)
        {
            var x = fraction / 1024.0 + (order >> 1);
            for (var i = order; i > 0;)
            {
                for (var j = 0; j <= i; j++) value += data[start + j] * tables.Newton[i, j];
                value *= x - --i;
            }
            value += data[start];
        }
        else
        {
            var offset = fraction * (Order + 1);
            for (var i = 0; i <= Order; i++) value += data[start + i] * tables.Gaussian[offset + i];
        }
        var premix = (int)(value * (voice.EnvelopeLevel >> 12) / 1024);
        return new(unchecked(premix * voice.MixVolume.Left) / 1024,
            unchecked(premix * voice.MixVolume.Right) / 1024);
    }

    private static (double[,] Newton, double[] Gaussian) CreateTables()
    {
        var newton = new double[Order + 1, Order + 1];
        var z = new double[Order + 1];
        for (var i = 0; i <= Order; i++)
        {
            newton[i, 0] = newton[i, i] = i > 1 ? newton[i - 1, 0] / i : 1;
            for (var j = 1; j < i; j++)
                newton[i, j] = (newton[i - 1, j - 1] + newton[i - 1, j]) / i;
            z[i] = i / (4 * Math.PI);
        }
        for (var i = 0; i <= Order; i++)
        {
            var sign = (i & 1) == 0 ? 1 : -1;
            for (var j = 0; j <= i; j++, sign = -sign) newton[i, j] *= sign;
        }
        var gaussian = new double[1024 * (Order + 1)];
        for (var fraction = 0; fraction < 1024; fraction++)
        {
            var x = (fraction / 1024.0 + (Order >> 1)) / (4 * Math.PI);
            for (var k = 0; k <= Order; k++)
            {
                double coefficient = 1;
                for (var i = 0; i <= Order; i++)
                    if (i != k) coefficient *= Math.Sin(x - z[i]) / Math.Sin(z[k] - z[i]);
                gaussian[fraction * (Order + 1) + k] = coefficient;
            }
        }
        return (newton, gaussian);
    }
}
