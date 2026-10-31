namespace HCDE.Playsim;

/// <summary>
/// FraggleScript section table from <c>t_prepro.cpp</c> (<b>1–212</b>). Chunk <b>1183</b>.
/// <c>ProcessFindChar</c> starts at line 213 and stays in the next band.
/// </summary>
public static class VmTPreproSections1183Slice
{
    public const int SectionSlots = 17;
    public const string ClassName = "DFsSection";
    public const string ParentClassName = "DObject";
    public const bool IsAbstract = false;
    public const bool HasPointers = true;

    public static readonly string[] PointerFields = { "next" };

    public sealed class Section
    {
        public int Type;
        public int StartIndex;
        public int EndIndex;
        public int LoopIndex;
        public Section? Next;
        public bool Destroyed;

        public void Destroy()
        {
            Destroyed = true;
            Next = null;
        }
    }

    public sealed class Script
    {
        public Section?[] Sections = new Section?[SectionSlots];
        public int WriteBarrierCount;
    }

    public readonly record struct Archive(
        int Type,
        int StartIndex,
        int EndIndex,
        int LoopIndex,
        Section? Next);

    public static Archive Serialize(Section section, Action? superSerialize)
    {
        if (superSerialize != null)
            superSerialize();
        return new Archive(section.Type, section.StartIndex, section.EndIndex, section.LoopIndex, section.Next);
    }

    public static int SectionStart(Section section)
    {
        return section.StartIndex;
    }

    public static int SectionEnd(Section section)
    {
        return section.EndIndex;
    }

    public static int SectionLoop(Section section)
    {
        return section.LoopIndex;
    }

    public static void ClearSections(Script script)
    {
        for (var i = 0; i < SectionSlots; i++)
        {
            var current = script.Sections[i];
            while (current != null)
            {
                var next = current.Next;
                current.Destroy();
                current = next;
            }

            script.Sections[i] = null;
        }
    }

    public static Section NewSection(Script script, int braceIndex)
    {
        var slot = SectionHash(braceIndex);
        var created = new Section();
        created.StartIndex = braceIndex;
        created.Next = script.Sections[slot];
        script.Sections[slot] = created;
        script.WriteBarrierCount++;
        return created;
    }

    public static Section? FindSectionStart(Script script, int braceIndex)
    {
        var current = script.Sections[SectionHash(braceIndex)];
        while (current != null)
        {
            if (SectionStart(current) == braceIndex)
                return current;
            current = current.Next;
        }

        return null;
    }

    public static Section? FindSectionEnd(Script script, int braceIndex)
    {
        for (var slot = 0; slot < SectionSlots; slot++)
        {
            var current = script.Sections[slot];
            while (current != null)
            {
                if (SectionEnd(current) == braceIndex)
                    return current;
                current = current.Next;
            }
        }

        return null;
    }

    public static int SectionHash(int braceIndex)
    {
        return braceIndex % SectionSlots;
    }
}
