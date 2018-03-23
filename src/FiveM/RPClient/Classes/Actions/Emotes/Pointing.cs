using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Actions.Jobs.Police;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Actions.Emotes
{
    static class Pointing
    {
        static private bool isPointing = false;

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static private async Task OnTick()
        {
            float pitch = GameplayCamera.RelativePitch;
            pitch = (Extensions.Clamp(pitch, -70f, 42f) + 70f) / 112f;

            float heading = GameplayCamera.RelativeHeading;
            heading = (Extensions.Clamp(heading, -180f, 180f) + 180f) / 360f;

            Function.Call((Hash)15400973881305242190, Game.PlayerPed.Handle, "Pitch", pitch);
            Function.Call((Hash)15400973881305242190, Game.PlayerPed.Handle, "Heading", -1*heading+1f);
            Function.Call((Hash)15400973881305242190, Game.PlayerPed.Handle, "Speed", 0);
            Function.Call((Hash)12729089900991484040, Game.PlayerPed.Handle, "isBlocked", false);

            // If we want obstacles to activate a different type of pointing
            // (Close to body)
            // I don't recommend it as some RP depends on being able to stick a finger in people's faces
            //float cosHeading = (float)Math.Cos(heading * (float)Math.PI / 180f);
            //float sinHeading = (float)Math.Sin(heading * (float)Math.PI / 180f);
            //Vector3 offsetCoords = Game.PlayerPed.GetOffsetPosition(new Vector3((cosHeading * -0.2f) - (sinHeading * (0.4f * heading + 0.3f)), (sinHeading * -0.2f) + (cosHeading * (0.4f * heading + 0.3f)), 0.6f));
            //RaycastResult raycast = World.RaycastCapsule(new Vector3(offsetCoords.X, offsetCoords.Y, offsetCoords.Z - 0.2f), new Vector3(offsetCoords.X, offsetCoords.Y, offsetCoords.Z + 0.2f), 0.4f, (IntersectOptions)95, Game.PlayerPed);
            //Function.Call((Hash)(ulong)0xB0A6CFD2C69C1088, Game.PlayerPed.Handle, "isBlocked", raycast.DitHit);

            Function.Call((Hash)(ulong)0xB0A6CFD2C69C1088, Game.PlayerPed.Handle, "isFirstPerson", Function.Call<int>((Hash)(ulong)0xEE778F8C7E1142E2, Function.Call<int>((Hash)(ulong)0x19CAFA3C87F7C2FF)) == 4);

            if (ControlHelper.IsControlPressed(Control.SpecialAbilitySecondary) && Arrest.playerCuffState == Enums.Police.CuffState.None)
            {
                if(!isPointing)
                { 
                    Function.Call(Hash.REQUEST_ANIM_DICT, "anim@mp_point");
                    while(!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, "anim@mp_point"))
                    {
                        await BaseScript.Delay(1);
                    }

                    Function.Call(Hash.SET_PED_CONFIG_FLAG, Game.PlayerPed.Handle, 36, true);
                    Function.Call(Hash._TASK_MOVE_NETWORK, Game.PlayerPed.Handle, "task_mp_pointing", 0.5f, 0, "anim@mp_point", 24);
                    Function.Call(Hash.REMOVE_ANIM_DICT, "anim@mp_point");
                    isPointing = true;
                    
                }
            }
            else if (isPointing)
            {
                Function.Call((Hash)0xD01015C7316AE176, Game.PlayerPed.Handle, "Stop");
                Game.PlayerPed.Task.ClearSecondary();
                isPointing = false;
            }
        }
    }
}