/*
** music_hmi_midiout.cpp
** Code to let ZDoom play HMI MIDI music through the MIDI streaming API.
**
**---------------------------------------------------------------------------
** Copyright 2010 Marisa Heit
** All rights reserved.
**
** Redistribution and use in source and binary forms, with or without
** modification, are permitted provided that the following conditions
** are met:
**
** 1. Redistributions of source code must retain the above copyright
**    notice, this list of conditions and the following disclaimer.
** 2. Redistributions in binary form must reproduce the above copyright
**    notice, this list of conditions and the following disclaimer in the
**    documentation and/or other materials provided with the distribution.
** 3. The name of the author may not be used to endorse or promote products
**    derived from this software without specific prior written permission.
**
** THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
** IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
** OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
** IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
** INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
** NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
** DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
** THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
** (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
** THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
**---------------------------------------------------------------------------
**
*/

namespace HCDE.Client;

public static class VmMusicHmiDevices
{
    public static bool IsEnabled(IReadOnlyList<ushort> designations, int technology)
    {
        ArgumentNullException.ThrowIfNull(designations);
        var device = technology switch { 4 => 0xa009, 1 => 0xa001, _ => 0xa008 };
        for (var i = 0; i < Math.Min(8, designations.Count) && designations[i] != 0; i++)
        {
            var designation = designations[i];
            if (designation == device
                || (designation == 0xa000 && device is 0xa001 or 0xa008)
                || (designation == 0xa002 && device == 0xa009)) return true;
        }
        return false;
    }

    public static IReadOnlyList<VmMusicHmiTrack> SelectTracks(VmMusicHmiDocument document, int technology)
    {
        ArgumentNullException.ThrowIfNull(document);
        return Array.AsReadOnly(document.Tracks.Where(track => IsEnabled(track.Designations, technology)).ToArray());
    }
}
