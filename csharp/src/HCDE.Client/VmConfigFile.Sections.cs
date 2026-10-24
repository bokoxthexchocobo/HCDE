namespace HCDE.Client;

public partial class VmConfigFile
{
    public bool SectionIsEmpty() => currentSection is null || currentSection.Entries.Count == 0;

    public void RenameSection(string oldName, string newName)
    {
        var section = sections.FirstOrDefault(s => string.Equals(s.Name, oldName, StringComparison.OrdinalIgnoreCase));
        if (section is not null) section.Name = newName;
    }

    public void CreateSectionAtStart(string name)
    {
        var existing = sections.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            existing = new VmConfigSection(name);
            sections.Insert(0, existing);
        }
        else
        {
            sections.Remove(existing);
            sections.Insert(0, existing);
        }

        currentSection = existing;
        currentEntryIndex = 0;
    }

    public void MoveSectionToStart(string name)
    {
        var section = sections.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        if (section is null) return;
        sections.Remove(section);
        sections.Insert(0, section);
    }

    public void SetSectionNote(string sectionName, string note)
    {
        var section = sections.FirstOrDefault(s => string.Equals(s.Name, sectionName, StringComparison.OrdinalIgnoreCase));
        if (section is not null) section.Note = note;
    }

    public bool DeleteCurrentSection()
    {
        if (currentSection is null) return false;
        var index = sections.IndexOf(currentSection);
        sections.RemoveAt(index);
        currentSection = index < sections.Count ? sections[index] : null;
        currentEntryIndex = 0;
        return currentSection is not null;
    }

    public void ClearCurrentSection()
    {
        currentSection?.Entries.Clear();
        currentEntryIndex = 0;
    }
}
