namespace HCDE.Gamedata;

/// <summary><c>F_StartFinale</c> descriptor construction from <c>intermission_parse.cpp</c>.</summary>
public static class VmFinaleIntermissionBuilder
{
    public const string InterChess = "Inter_Chess";

    public static bool TryBuild(VmFinaleBuildRequest request, IIntermissionTextLumpResolver? lumpResolver,
        VmIntermissionRegistry registry, out VmIntermissionDescriptor? descriptor, out bool useRegistryLookup)
    {
        descriptor = null;
        useRegistryLookup = false;
        if (request.SkipForChessEnding && request.EndSequence.Equals(InterChess, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrEmpty(request.Text))
        {
            var textscreen = new VmIntermissionActionTextscreen();
            if (request.TextInLump)
            {
                if (lumpResolver is not null && lumpResolver.TryResolve(request.Text, out var lumpText, out var hexenKey))
                    textscreen.Text = hexenKey is not null ? "$" + hexenKey : lumpText;
                else
                    textscreen.Text = $"Unknown text lump '{request.Text}'";
            }
            else if (!request.LookupText)
                textscreen.Text = request.Text;
            else
                textscreen.Text = "$" + request.Text;

            textscreen.TextDelayTics = 10;
            textscreen.Background = string.IsNullOrEmpty(request.Flat) ? "-" : request.Flat!;
            textscreen.FlatFill = !request.FinalePic;
            if (!string.IsNullOrEmpty(request.Music))
            {
                textscreen.Music = request.Music!;
                textscreen.MusicOrder = request.MusicOrder;
            }

            if (request.CdTrack > 0)
            {
                textscreen.CdTrack = request.CdTrack;
                textscreen.CdId = (int)request.CdId;
            }

            descriptor = new VmIntermissionDescriptor();
            descriptor.Actions.Add(textscreen);
            if (request.Ending)
            {
                descriptor.Link = request.EndSequence;
                descriptor.Actions.Add(new VmIntermissionActionWiper());
            }

            return true;
        }

        if (request.Ending && registry.TryGet(request.EndSequence, out var existing))
        {
            descriptor = existing;
            useRegistryLookup = true;
            return true;
        }

        return false;
    }
}
