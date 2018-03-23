using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Environment.UI
{
    static class HideReticle
    {
        static public void Init()
        {
            // TODO: Uncomment (just annoying LOL)
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static public async Task OnTick()
        {
            bool isAimCamActive = Function.Call<bool>(Hash.IS_AIM_CAM_ACTIVE);
            bool isFirstPersonAimCamActive = Function.Call<bool>(Hash.IS_FIRST_PERSON_AIM_CAM_ACTIVE);
            if (!isAimCamActive || !isFirstPersonAimCamActive)
            {
                CitizenFX.Core.UI.Screen.Hud.HideComponentThisFrame(CitizenFX.Core.UI.HudComponent.Reticle);
            }
            await Task.FromResult(0);
        }
    }
}
