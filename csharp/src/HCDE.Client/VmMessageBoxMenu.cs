namespace HCDE.Client;

/// <summary>Native <c>messagebox.cpp</c> message box menu factory.</summary>
public static class VmMessageBoxMenu
{
    public const string DefaultMenuClass = "MessageBoxMenu";

    public static VmMessageBoxMenuInstance CreateMessageBoxMenu(
        VmMessageBoxMenuInstance? parent,
        string message,
        int messageMode,
        bool playSound,
        string action = "",
        Action? handler = null,
        Func<string, string>? resolveClass = null)
    {
        resolveClass ??= name => name;
        var className = resolveClass(DefaultMenuClass);
        if (!className.Equals(DefaultMenuClass, StringComparison.OrdinalIgnoreCase))
            className = DefaultMenuClass;
        return new VmMessageBoxMenuInstance(className, parent, message, messageMode, playSound, action, handler);
    }

    public static VmMessageBoxMenuInstance StartMessage(
        VmMessageBoxMenuInstance? currentMenu,
        bool menuWasOff,
        string message,
        int messageMode,
        string action,
        Action<VmMessageBoxMenuInstance>? activateMenu,
        Action<bool>? startControlPanel = null)
    {
        if (currentMenu is null)
            startControlPanel?.Invoke(menuWasOff);
        var menu = CreateMessageBoxMenu(currentMenu, message, messageMode, playSound: false, action);
        menu.ParentMenu = currentMenu;
        activateMenu?.Invoke(menu);
        return menu;
    }
}

public sealed class VmMessageBoxMenuInstance
{
    public VmMessageBoxMenuInstance(
        string className,
        VmMessageBoxMenuInstance? parent,
        string message,
        int messageMode,
        bool playSound,
        string action,
        Action? handler)
    {
        ClassName = className;
        ParentMenu = parent;
        Message = message;
        MessageMode = messageMode;
        PlaySound = playSound;
        Action = action;
        Handler = handler;
    }

    public string ClassName { get; }
    public VmMessageBoxMenuInstance? ParentMenu { get; set; }
    public string Message { get; }
    public int MessageMode { get; }
    public bool PlaySound { get; }
    public string Action { get; }
    public Action? Handler { get; }
}
