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

public sealed class VmMusicWildMidiReverbFilter
{
    private readonly int[,,] coefficients;
    private readonly int[,,,] inputs = new int[2, 8, 6, 2];
    private readonly int[,,,] outputs = new int[2, 8, 6, 2];

    public VmMusicWildMidiReverbFilter(int[,,] coefficients)
    {
        ArgumentNullException.ThrowIfNull(coefficients);
        if (coefficients.GetLength(0) != 8 || coefficients.GetLength(1) != 6 || coefficients.GetLength(2) != 5)
            throw new ArgumentException("Reverb filters require eight reflections, six bands, and five coefficients.", nameof(coefficients));
        this.coefficients = (int[,,])coefficients.Clone();
    }

    public void Reset()
    {
        Array.Clear(inputs);
        Array.Clear(outputs);
    }

    public VmMusicWildMidiMixVolume Process(int leftReflection, int rightReflection)
    {
        var left = 0;
        var right = 0;
        for (var reflection = 0; reflection < 8; reflection++)
            for (var band = 0; band < 6; band++)
            {
                left = unchecked(left + ProcessBand(0, reflection, band, leftReflection) / 8);
                right = unchecked(right + ProcessBand(1, reflection, band, rightReflection) / 8);
            }
        return new(left, right);
    }

    private int ProcessBand(int channel, int reflection, int band, int value)
    {
        var result = unchecked(value * coefficients[reflection, band, 0]
            + inputs[channel, reflection, band, 0] * coefficients[reflection, band, 1]
            + inputs[channel, reflection, band, 1] * coefficients[reflection, band, 2]
            - outputs[channel, reflection, band, 0] * coefficients[reflection, band, 3]
            - outputs[channel, reflection, band, 1] * coefficients[reflection, band, 4]) / 1024;
        inputs[channel, reflection, band, 1] = inputs[channel, reflection, band, 0];
        inputs[channel, reflection, band, 0] = value;
        outputs[channel, reflection, band, 1] = outputs[channel, reflection, band, 0];
        outputs[channel, reflection, band, 0] = result;
        return result;
    }
}
