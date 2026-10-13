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

public static class VmMusicWildMidiReverbRoom
{
    public static VmMusicWildMidiReverbDelay Create(int sampleRate, VmMusicWildMidiReverbSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (sampleRate is < 11025 or > 65535) throw new ArgumentOutOfRangeException(nameof(sampleRate));
        double width = settings.RoomWidth, length = settings.RoomLength;
        var listener = (X: (double)settings.ListenerX, Y: (double)settings.ListenerY);
        var left = (X: width / 4.0, Y: length / 10.0);
        var right = (X: width / 4.0 * 3.0, Y: length / 10.0);
        (double X, double Y)[] points = [(width / 3.0, 0), (0, length / 3.0),
            (0, length / 3.0 * 2.0), (width / 3.0, length), (width / 3.0 * 2.0, length),
            (width, length / 3.0 * 2.0), (width, length / 3.0), (width / 3.0 * 2.0, 0)];
        var leftPositions = new int[8];
        var rightPositions = new int[8];
        var roundTrips = new double[8];
        var feedback = new int[8];
        var leftMaximum = Delay(Distance(left, listener), sampleRate);
        var rightMaximum = Delay(Distance(right, listener), sampleRate);
        for (var point = 0; point < 8; point++)
        {
            var distance = Distance(points[point], listener);
            roundTrips[point] = distance * 2.0;
            // Correct the native reused-index bug: all distances belong to this reflection.
            leftPositions[point] = Delay(Math.Max(0, Distance(left, points[point]) + distance - Distance(left, listener)), sampleRate);
            rightPositions[point] = Delay(Math.Max(0, Distance(right, points[point]) + distance - Distance(right, listener)), sampleRate);
            feedback[point] = Delay(roundTrips[point], sampleRate);
            var maximum = Math.Max(feedback[point], Math.Max(leftPositions[point], rightPositions[point]));
            if (point < 4) leftMaximum = Math.Max(leftMaximum, maximum);
            else rightMaximum = Math.Max(rightMaximum, maximum);
        }
        // Include the longest delay slot in the ring, rather than allocating an unreachable native guard slot.
        return new(leftMaximum + 1, rightMaximum + 1, leftPositions, rightPositions,
            feedback[..4], feedback[4..], VmMusicWildMidiReverbCoefficients.Create(sampleRate, roundTrips));
    }

    private static double Distance((double X, double Y) a, (double X, double Y) b)
    {
        var x = a.X - b.X;
        var y = a.Y - b.Y;
        return Math.Sqrt(x * x + y * y);
    }

    private static int Delay(double distance, int sampleRate) => (int)((float)sampleRate * (distance / 340.29));
}
