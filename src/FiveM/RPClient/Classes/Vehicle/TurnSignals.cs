using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Enums.Controls;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Vehicle
{
    enum IndicatorStatus
    {
        Left = -1,
        None = 0,
        Right = 1,
        Hazards = 256
    }

    // TODO: Check how well this syncs and if we need manual sync
    class TurnSignals
    {
        static IndicatorStatus IndicatorStatus = IndicatorStatus.None;
        static bool HasTrailer;
        static CitizenFX.Core.Vehicle Trailer;

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static private Task OnTick()
        {
            if(Game.PlayerPed.IsInVehicle())
            { 
                if (ControlHelper.IsControlJustPressed(Control.NextWeapon))
                {
                    Toggle(IndicatorStatus.Left);
                }
                else if (ControlHelper.IsControlJustPressed(Control.PrevWeapon))
                {
                    Toggle(IndicatorStatus.Right);
                }
                else if (ControlHelper.IsControlJustPressed(Control.MpTextChatTeam, true, ControlModifier.Shift | ControlModifier.Ctrl))
                {
                    Toggle(IndicatorStatus.Hazards);
                }
            }

            return Task.FromResult(0);
        }

        static public async void Toggle(IndicatorStatus ToggleIndicator)
        {
            OutputArgument OutArg = new OutputArgument();
            HasTrailer = Function.Call<bool>(Hash.GET_VEHICLE_TRAILER_VEHICLE, Game.PlayerPed.CurrentVehicle.Handle, OutArg);
            Trailer = new CitizenFX.Core.Vehicle(OutArg.GetResult<int>());
            Log.ToChat($"{HasTrailer} {OutArg.GetResult<int>()}");

            if (ToggleIndicator == IndicatorStatus.Hazards)
            {
                if (IndicatorStatus == IndicatorStatus.Left || IndicatorStatus == IndicatorStatus.Right)
                {
                    IndicatorStatus = IndicatorStatus.Hazards;
                    SetIndicatorCombination(true, true);
                }
                else if(IndicatorStatus == IndicatorStatus.Hazards)
                {
                    SetIndicatorCombination(false, false);
                    IndicatorStatus = IndicatorStatus.None;
                }
                else
                {
                    IndicatorStatus = IndicatorStatus.Hazards;
                    SetIndicatorCombination(true, true);
                }
                return;
            }

            if (IndicatorStatus != IndicatorStatus.None)
            {
                SetIndicatorCombination(false, false);

                if (IndicatorStatus == ToggleIndicator)
                {
                    IndicatorStatus = IndicatorStatus.None;
                    return;
                }

                IndicatorStatus = IndicatorStatus.None;
            }

            if(ToggleIndicator != IndicatorStatus.None)
            { 
                if(ToggleIndicator == IndicatorStatus.Left)
                {
                    SetIndicatorCombination(true, false);
                }
                else if(ToggleIndicator == IndicatorStatus.Right)
                {
                    SetIndicatorCombination(false, true);
                }

                IndicatorStatus = ToggleIndicator;

                // We don't want to await this
                Task.Factory.StartNew(async () =>
                {
                    float heading = Game.PlayerPed.CurrentVehicle.Heading;
                    while (Math.Abs(heading - Game.PlayerPed.CurrentVehicle.Heading) < 55)
                        await BaseScript.Delay(250);
                    // If indicator status is unchanged disable signals now, otherwise do nothing
                    if(IndicatorStatus == ToggleIndicator)
                        IndicatorStatus = IndicatorStatus.None;
                });

                while (IndicatorStatus != IndicatorStatus.None)
                    await BaseScript.Delay(50);
                SetIndicatorCombination(false, false);
            }
        }

        static void SetIndicatorCombination(bool Left, bool Right)
        {
            Game.PlayerPed.CurrentVehicle.IsLeftIndicatorLightOn = Left;
            Game.PlayerPed.CurrentVehicle.IsRightIndicatorLightOn = Right;
            // For the hauler trailer I tested I could not get it to work
            // But keeping it as it may work for some
            if (HasTrailer)
            {
                Trailer.IsLeftIndicatorLightOn = Left;
                Trailer.IsRightIndicatorLightOn = Right;
            }
        }
    }
}
