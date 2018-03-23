using System;

namespace FamilyRP.Roleplay.Enums.Controls
{
    public enum KeybindType
    {
        Pressed = 0,
        JustPressed = 1,
        JustReleased = 2,
        DisabledPressed = 3,
        DisabledJustPressed = 4,
        DisabledReleased = 5,
        DisabledJustReleased = 6
    }

    [Flags]
    public enum ControlModifier
    {
        Any = -1,
        None = 0,
        Ctrl = 1 << 0,
        Alt = 1 << 1,
        Shift = 1 << 2
    }
}