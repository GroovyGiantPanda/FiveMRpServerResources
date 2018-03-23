using System;

namespace FamilyRP.Roleplay.Models
{
    class Keybind
    {
        public Action<CitizenFX.Core.Control, bool> callback { get; set; }
        public bool keyboardOnly { get; set; }

        public Keybind(Action<CitizenFX.Core.Control, bool> callback, bool keyboardOnly)
        {
            this.callback = callback;
            this.keyboardOnly = keyboardOnly;
        }
    }
}
