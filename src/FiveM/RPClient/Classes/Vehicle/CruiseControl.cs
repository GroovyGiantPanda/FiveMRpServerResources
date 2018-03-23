using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Enums.Controls;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client
{
    static class CruiseControl
    {
        static public bool CruiseControlActive = false;
        static public bool CruiseControlDisabled = false;
        static float CruiseControlSpeed = -1f;

        // The maximum incline we want players to be able to climb with cruise control
        // (Yes, some genuine roads are _really_ steep)
        // Most unrealistic hill climbs are 40+ degrees
        static float InclineLimit = 32f;

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static private Task OnTick()
        {
            try
            {
                if (!CruiseControlDisabled && Game.PlayerPed.IsInVehicle() && (Game.PlayerPed.CurrentVehicle.Model.IsCar || Game.PlayerPed.CurrentVehicle.Model.IsBike))
            { 
                if(Game.PlayerPed.CurrentVehicle.IsEngineRunning == false)
                {
                    CruiseControlActive = false;
                    return Task.FromResult(0);
                }
                if (!Game.PlayerPed.CurrentVehicle.IsOnAllWheels)
                {
                    //// Allow car to leave ground for short periods
                    //Task.Factory.StartNew(async () =>
                    //{
                    //    int i = 60; // Milliseconds to allow vehicle wheels off ground
                    //    while(i > 0)
                    //    { 
                    //        await BaseScript.Delay(20);
                    //        i -= 20;
                    //        if (Game.PlayerPed.CurrentVehicle.IsOnAllWheels)
                    //        {
                    //            return;
                    //        }
                    //    }
                    //});
                            CruiseControlActive = false;
                    }

                    if (ControlHelper.IsControlJustPressed(Control.MpTextChatTeam) && Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, Game.PlayerPed.CurrentVehicle.Handle).Y > 2)
                    {
                        if (Game.PlayerPed.CurrentVehicle.Speed > 70.0f)
                        {
                            BaseScript.TriggerEvent("Chat.Message", "[CruiseControl]", "#ffffff", "You are going too fast to start cruise control!");
                            return Task.FromResult(0);
                        }
                        bool NewState = !CruiseControlActive;
                        if (NewState == true)
                        {
                            CruiseControlSpeed = Game.PlayerPed.CurrentVehicle.Speed;
                        }
                        else
                        {
                            CruiseControlSpeed = -1;
                        }
                        CruiseControlActive = NewState;
                    }
                    else if (CruiseControlActive && ControlHelper.IsControlPressed(Control.VehicleAccelerate, false, ControlModifier.Any) && Game.PlayerPed.CurrentVehicle.Model.IsBike)
                    {
                        // Because bikes start flying when accelerating...
                        CruiseControlActive = false;
                    }
                    else if (CruiseControlActive && ControlHelper.IsControlPressed(Control.VehicleAccelerate, false, ControlModifier.Any))
                    {
                        CruiseControlSpeed = Game.PlayerPed.CurrentVehicle.Speed;
                    }
                    else if (CruiseControlActive && (ControlHelper.IsControlPressed(Control.VehicleBrake, false, ControlModifier.Any) || ControlHelper.IsControlPressed(Control.VehicleHandbrake, false, ControlModifier.Any)))
                    {
                        CruiseControlActive = false;
                        Log.ToChat("Cruise control disabled.");
                    }

                    if (CruiseControlActive)
                    {
                        if (Game.PlayerPed.CurrentVehicle.Rotation.X > InclineLimit)
                        {
                            CruiseControlActive = false;
                            Log.ToChat("Cruise control disabled.");
                        }
                        else
                        {
                            Game.PlayerPed.CurrentVehicle.Speed = CruiseControlSpeed;
                        }
                    }
                }
                else
                {
                    CruiseControlActive = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"CruiseControl OnTick Error: {ex.Message}");
            }
            return Task.FromResult(0);
        }
    }
}
