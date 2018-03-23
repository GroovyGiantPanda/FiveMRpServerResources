using System;

namespace FamilyRP.Roleplay.Enums.Session
{
    [Flags]
    public enum Privilege
    {
		NONE		= 0,
        ADMIN		= 1 << 0,
        DEV			= 1 << 1
    }

    public enum VoipRange
    {
        None = 0,
        Whisper = 1,
        Normal = 2,
        Shout = 3
    }
}