using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.Enums.Controls;
using FamilyRP.Roleplay.Server.Enums;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Jobs
{
    static class Fishing
    {
        static private bool IsFishing = false;
        private static int StartTime = 0;
        static int FishingDuration = new Random().Next(6000, 15000);

        static public void Init()
        {
            Client.GetInstance().ClientCommands.Register("/fishing", HandleFishing);
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static private async Task OnTick()
        {
            if (ControlHelper.IsControlJustPressed(Control.MoveUpOnly, false, ControlModifier.Any) || ControlHelper.IsControlJustPressed(Control.MoveDown, false, ControlModifier.Any) || ControlHelper.IsControlJustPressed(Control.MoveLeft, false, ControlModifier.Any) || ControlHelper.IsControlJustPressed(Control.MoveRight, false, ControlModifier.Any))
            {
                IsFishing = false;
                Game.PlayerPed.Task.ClearAll();
            }
            await Task.FromResult(0);
        }

        static public async void HandleFishing(Command command)
        {
            try
            {
                if (!IsFishing)
                {
                    if (Function.Call<int>(Hash.GET_GAME_TIMER) - StartTime > FishingDuration)
                    {
                        IsFishing = true;
                        if (await Game.PlayerPed.IsInFrontOfWater())
                        {
                            float heading = Game.PlayerPed.Heading;
                            Game.PlayerPed.Task.StartScenario("WORLD_HUMAN_STAND_FISHING", Game.PlayerPed.Position);
                            await BaseScript.Delay(40);
                            Game.PlayerPed.Heading = heading;
                            IsFishing = true;
                            StartTime = Function.Call<int>(Hash.GET_GAME_TIMER);
                            while (Function.Call<int>(Hash.GET_GAME_TIMER) - StartTime < FishingDuration)
                            {
                                await BaseScript.Delay(0);
                            }
                            if (IsFishing)
                            {
                                Game.PlayerPed.Task.ClearAll();
                                BaseScript.TriggerEvent("Chat.Message", "", "#AA77DD", new Random().NextBool(50) ? "You caught a fish!" : "You didn't catch anything :(");
                                IsFishing = false;
                            }
                            FishingDuration = new Random().Next(6000, 15000);
                        }
                        else
                        {
                            Log.ToChat("You can't do that here.");
                        }
                    }
                    else
                    {
                        Log.ToChat("You can't do that yet.");
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public static async Task<bool> IsInFrontOfWater(this CitizenFX.Core.Ped PlayerPed)
        {
            Vector3 ProbeLocation = PlayerPed.GetOffsetPosition(new Vector3(0, 8, 0));

            OutputArgument z = new OutputArgument();
            bool groundFound = Function.Call<bool>(Hash.GET_GROUND_Z_FOR_3D_COORD, ProbeLocation.X, ProbeLocation.Y, ProbeLocation.Z, z, false);
            float groundZ = z.GetResult<float>();
            ProbeLocation.Z = (float)groundZ - 0.1f;
            Log.ToChat(ProbeLocation.ToString());
            Ped ProbePed = await World.CreatePed(new Model(PedHash.Rat), ProbeLocation);
            ProbePed.PositionNoOffset = ProbeLocation;
            ProbePed.Opacity = 0;
            await BaseScript.Delay(50);
            bool isProbeInWater = Function.Call<bool>(Hash.IS_ENTITY_IN_WATER, ProbePed.Handle);
            ProbePed.Delete();
            bool isPlayerInWater = Function.Call<bool>(Hash.IS_ENTITY_IN_WATER, PlayerPed.Handle);
            return isPlayerInWater || isProbeInWater;
        }
    }
}
