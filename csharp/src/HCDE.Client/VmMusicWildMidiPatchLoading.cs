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

public enum VmMusicWildMidiPatchLoadResult { Added, AlreadyPresent, Missing, SampleLoadFailed, NoSamples }

public interface IVmMusicWildMidiPatch
{
    ushort Id { get; }
    bool Loaded { get; set; }
    bool HasSamples { get; }
    int UseCount { get; set; }
}

public static class VmMusicWildMidiPatchLoading
{
    public static void Release<TPatch>(IList<TPatch> devicePatches, Action<TPatch> freeSamples)
        where TPatch : class, IVmMusicWildMidiPatch
    {
        ArgumentNullException.ThrowIfNull(devicePatches);
        ArgumentNullException.ThrowIfNull(freeSamples);
        while (devicePatches.Count != 0)
        {
            var patch = devicePatches[0];
            patch.UseCount = unchecked(patch.UseCount - 1);
            devicePatches.RemoveAt(0);
            if (patch.UseCount != 0) continue;
            freeSamples(patch);
            patch.Loaded = false;
        }
    }

    public static VmMusicWildMidiPatchLoadResult Load<TPatch>(ushort requestedId, IList<TPatch> devicePatches,
        Func<ushort, TPatch?> findExact, Func<TPatch, int> loadSamples) where TPatch : class, IVmMusicWildMidiPatch
    {
        ArgumentNullException.ThrowIfNull(devicePatches);
        ArgumentNullException.ThrowIfNull(findExact);
        ArgumentNullException.ThrowIfNull(loadSamples);
        foreach (var existing in devicePatches)
            if (existing.Id == requestedId) return VmMusicWildMidiPatchLoadResult.AlreadyPresent;
        var patch = VmMusicWildMidiPatchLookup.Find(requestedId, findExact);
        if (patch is null) return VmMusicWildMidiPatchLoadResult.Missing;
        if (!patch.Loaded && loadSamples(patch) == -1) return VmMusicWildMidiPatchLoadResult.SampleLoadFailed;
        if (!patch.HasSamples) return VmMusicWildMidiPatchLoadResult.NoSamples;
        devicePatches.Add(patch);
        patch.UseCount = unchecked(patch.UseCount + 1);
        return VmMusicWildMidiPatchLoadResult.Added;
    }
}
