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

public sealed class VmMusicWildMidiReverbDelay
{
    private readonly int[] left, right, leftSpeaker, rightSpeaker, leftFeedback, rightFeedback;
    private readonly VmMusicWildMidiReverbFilter filter;
    private int leftOutput, rightOutput;

    public VmMusicWildMidiReverbDelay(int leftSize, int rightSize, int[] leftSpeaker, int[] rightSpeaker,
        int[] leftFeedback, int[] rightFeedback, int[,,] coefficients)
    {
        if (leftSize <= 0) throw new ArgumentOutOfRangeException(nameof(leftSize));
        if (rightSize <= 0) throw new ArgumentOutOfRangeException(nameof(rightSize));
        this.leftSpeaker = Validate(leftSpeaker, 8, leftSize, rightSize);
        this.rightSpeaker = Validate(rightSpeaker, 8, leftSize, rightSize);
        this.leftFeedback = Validate(leftFeedback, 4, leftSize, leftSize);
        this.rightFeedback = Validate(rightFeedback, 4, rightSize, rightSize);
        filter = new(coefficients);
        left = new int[leftSize]; right = new int[rightSize];
    }

    public void Reset()
    {
        Array.Clear(left); Array.Clear(right);
        filter.Reset();
    }

    public void Process(Span<int> stereo)
    {
        if ((stereo.Length & 1) != 0) throw new ArgumentException("Reverb requires complete stereo frames.", nameof(stereo));
        for (var frame = 0; frame < stereo.Length; frame += 2)
        {
            for (var point = 0; point < 8; point++)
            {
                var buffer = point < 4 ? left : right;
                Add(buffer, leftSpeaker, point, stereo[frame] / 64);
                Add(buffer, rightSpeaker, point, stereo[frame + 1] / 64);
            }
            var wet = filter.Process(left[leftOutput], right[rightOutput]);
            left[leftOutput] = 0; right[rightOutput] = 0;
            leftOutput = (leftOutput + 1) % left.Length;
            rightOutput = (rightOutput + 1) % right.Length;
            stereo[frame] = unchecked(stereo[frame] + wet.Left);
            stereo[frame + 1] = unchecked(stereo[frame + 1] + wet.Right);
            for (var point = 0; point < 4; point++)
            {
                Add(left, leftFeedback, point, stereo[frame + 1] / 64);
                Add(right, rightFeedback, point, stereo[frame] / 64);
            }
        }
    }

    private static void Add(int[] buffer, int[] positions, int point, int value)
    {
        buffer[positions[point]] = unchecked(buffer[positions[point]] + value);
        positions[point] = (positions[point] + 1) % buffer.Length;
    }

    private static int[] Validate(int[] positions, int count, int leftSize, int rightSize)
    {
        ArgumentNullException.ThrowIfNull(positions);
        if (positions.Length != count) throw new ArgumentException("Invalid reverb position count.", nameof(positions));
        for (var i = 0; i < count; i++)
            if (positions[i] < 0 || positions[i] >= (i < 4 ? leftSize : rightSize))
                throw new ArgumentOutOfRangeException(nameof(positions));
        return (int[])positions.Clone();
    }
}
