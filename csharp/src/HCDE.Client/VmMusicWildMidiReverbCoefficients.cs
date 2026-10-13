/*
 reverb.c

 Midi Wavetable Processing library

 Copyright (C) Chris Ison  2001-2011
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

public static class VmMusicWildMidiReverbCoefficients
{
    public static int[,,] Create(int sampleRate, ReadOnlySpan<double> reflectionDistances)
    {
        if (sampleRate is < 11025 or > 65535) throw new ArgumentOutOfRangeException(nameof(sampleRate));
        if (reflectionDistances.Length != 8) throw new ArgumentException("Reverb requires eight round-trip reflection distances.", nameof(reflectionDistances));
        foreach (var distance in reflectionDistances)
            if (!double.IsFinite(distance) || distance < 0) throw new ArgumentOutOfRangeException(nameof(reflectionDistances));
        double[] frequencies = [125, 250, 500, 1000, 2000, 4000];
        double[] airAbsorption = [-0.00044, -0.00131, -0.002728, -0.004665, -0.009887, -0.029665];
        double[] surfaceA = [-1.839, -6.205, -8.891, -12.059, -15.935, -20.942];
        double[] surfaceB = [-0.131, -6.205, -12.059, -20.933, -20.933, -15.944];
        var result = new int[8, 6, 5];
        for (var reflection = 0; reflection < 8; reflection++)
        {
            var surface = reflection is 0 or 3 or 4 or 7 ? surfaceA : surfaceB;
            for (var band = 0; band < 6; band++)
            {
                var omega = 2.0 * Math.PI * frequencies[band] / sampleRate;
                var sine = Math.Sin(omega);
                var cosine = Math.Cos(omega);
                var alpha = sine * Math.Sinh(Math.Log(2.0) / 2 * 2.0 * omega / sine);
                var amplitude = Math.Pow(10.0, (surface[band] + airAbsorption[band] * reflectionDistances[reflection]) / 40.0);
                var denominator = 1 + alpha / amplitude;
                result[reflection, band, 0] = Quantize((1 + alpha * amplitude) / denominator);
                result[reflection, band, 1] = Quantize(-2 * cosine / denominator);
                result[reflection, band, 2] = Quantize((1 - alpha * amplitude) / denominator);
                result[reflection, band, 3] = result[reflection, band, 1];
                result[reflection, band, 4] = Quantize((1 - alpha / amplitude) / denominator);
            }
        }
        return result;
    }

    private static int Quantize(double coefficient)
    {
        var scaled = coefficient * 1024.0;
        if (!double.IsFinite(scaled) || scaled < int.MinValue || scaled > int.MaxValue)
            throw new InvalidDataException("The reverb coefficient cannot be represented as a native integer.");
        return (int)scaled;
    }
}
