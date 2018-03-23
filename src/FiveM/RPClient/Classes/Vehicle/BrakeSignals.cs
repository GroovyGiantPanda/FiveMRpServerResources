using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Vehicle
{
    static class BrakeSignals
    {
        static PlayerList PlayerList = new PlayerList();
        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static private Task OnTick()
        {
            PlayerList.Where(p => p.Character.IsInVehicle() && p.Character.CurrentVehicle.Driver == p.Character && p.Character.CurrentVehicle.IsEngineRunning && p.Character.CurrentVehicle.Speed < 4f && (p != Game.Player || (p == Game.Player && !ControlHelper.IsControlPressed(Control.VehicleAccelerate, false, Enums.Controls.ControlModifier.Any)))).ToList().ForEach(p => p.Character.CurrentVehicle.AreBrakeLightsOn = true);
            return Task.FromResult(0);
        }
    }
}
