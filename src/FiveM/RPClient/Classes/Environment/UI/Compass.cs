using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Environment.UI
{
    static class Compass
    {
        public static string GetCardinalDirection()
        {
            float h = Game.PlayerPed.Heading;
            if (h >= 315f || h < 45f) return "N";
            else if (h >= 45f && h < 135f) return "W";
            else if (h >= 135f && h < 225f) return "S";
            else if (h >= 225f && h < 315f) return "E";
            else return "N";
        }
    }
}
