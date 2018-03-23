using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Enums.Controls;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Jobs.Police
{
    enum CivilianCarSirenMode
    {
        None = 0,
        Slow = 600,
        Fast = 150
    }

    // TODO: Make sure cops don't alter players' vehicle underglow
    static class CivilianCarSirenLights
    {
        static CitizenFX.Core.Vehicle vehicle = null;
        static CivilianCarSirenMode CivilianLights = CivilianCarSirenMode.None;
        static List<CivilianCarSirenMode> CivilianCarSirenModeValues = Enum.GetValues(typeof(CivilianCarSirenMode)).OfType<CivilianCarSirenMode>().ToList();

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
            RegularUpdates();
        }

        static private Task OnTick()
        {
            try
            {
                if (ControlHelper.IsControlJustPressed(Control.ReplayShowhotkey, true, ControlModifier.Alt) && Game.PlayerPed.IsInVehicle())
                {
                    vehicle = Game.PlayerPed.CurrentVehicle;
                    vehicle.Mods.InstallModKit();
                    CivilianLights = CivilianCarSirenModeValues[(CivilianCarSirenModeValues.IndexOf(CivilianLights) + 1) % CivilianCarSirenModeValues.Count];
                    BaseScript.TriggerEvent("Chat.Message", "", "#9999EE", $"Set civilian car lights mode to {CivilianLights.ToString().ToLower()}.");
                    if (CivilianLights == CivilianCarSirenMode.None)
                    {
                        vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, false);
                        vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, false);
                        vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, false);
                        vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"CivilianCarSirenLights OnTick error: {ex.Message}");
            }
            return Task.FromResult(0);
        }

        static async Task RegularUpdates()
        {
            bool currentRed = true;
            Color color;
            while (true)
            {
                try
                {
                    if (vehicle != null)
                    {
                        if (!Game.PlayerPed.IsInVehicle())
                        {
                            CivilianLights = CivilianCarSirenMode.None;
                            vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, false);
                            vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, false);
                            vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, false);
                            vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, false);
                            vehicle = null;
                            continue;
                        }

                        if (CivilianLights != CivilianCarSirenMode.None)
                        {
                            if (vehicle != Game.PlayerPed.CurrentVehicle)
                            {
                                CivilianLights = CivilianCarSirenMode.None;
                                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, false);
                                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, false);
                                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, false);
                                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, false);
                                vehicle = Game.PlayerPed.CurrentVehicle;
                                vehicle.Mods.InstallModKit();
                            }
                            if (currentRed)
                            {
                                color = Color.FromArgb(255, 255, 0, 0);
                                currentRed = false;
                            }
                            else
                            {
                                color = Color.FromArgb(255, 0, 0, 255);
                                currentRed = true;
                            }
                            vehicle.Mods.NeonLightsColor = color;
                            vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, true);
                            vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, true);
                            vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, true);
                            vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, true);
                        }
                    }
                    await BaseScript.Delay((int)CivilianLights);
                }
                catch(Exception ex)
                {
                    Log.Error($"CivilianCarSirenLights error: {ex.Message}");
                }
            }
        }
    }
}
