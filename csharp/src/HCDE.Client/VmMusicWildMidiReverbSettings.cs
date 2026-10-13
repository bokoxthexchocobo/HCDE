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

public sealed class VmMusicWildMidiReverbSettings
{
    public float RoomWidth { get; private set; } = 16.875f;
    public float RoomLength { get; private set; } = 22.5f;
    public float ListenerX { get; private set; } = 8.4375f;
    public float ListenerY { get; private set; } = 16.875f;

    public bool Apply(string line)
    {
        var tokens = VmMusicWildMidiConfigTokens.Tokenize(line);
        if (tokens.Count == 0) return false;
        var name = tokens[0].ToLowerInvariant();
        if (name is not ("reverb_room_width" or "reverb_room_length" or "reverb_listener_posx" or "reverb_listener_posy")) return false;
        if (tokens.Count < 2 || tokens[1].Length == 0 || tokens[1][0] is < '0' or > '9')
            throw new InvalidDataException("Reverb settings require a value beginning with an ASCII decimal digit.");
        var value = VmMusicWildMidiConfigReader.ParseFloatPrefix(tokens[1]);
        switch (name)
        {
            case "reverb_room_width": RoomWidth = Math.Clamp(value, 1f, 100f); break;
            case "reverb_room_length": RoomLength = Math.Clamp(value, 1f, 100f); break;
            case "reverb_listener_posx": ListenerX = value > RoomWidth || value < 0 ? RoomWidth / 2f : value; break;
            // Native code compares Y to width, then uses length for the fallback.
            case "reverb_listener_posy": ListenerY = value > RoomWidth || value < 0 ? RoomLength * 0.75f : value; break;
        }
        return true;
    }
}
