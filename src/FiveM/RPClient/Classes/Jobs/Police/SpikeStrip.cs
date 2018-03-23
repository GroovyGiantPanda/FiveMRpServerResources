using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Jobs.Police
{
    // TODO: Utilize dot product for accuracy (i.e. check actual line and not just a circle around center of spikes)
    // Function that'll be used already written in WorldProbe
    static class SpikeStrip
    {
        const float cullingDistance = 20f;
        const float spikeRange = 3f;
        const float pickupDistance = 15f;
        static Dictionary<int, string> wheelBoneNames = new Dictionary<int, string>()
        {
            [0] = "wheel_lf",
            [1] = "wheel_rf",
            [2] = "wheel_lm1",
            [3] = "wheel_rm1",
            [4] = "wheel_lr",
            [5] = "wheel_rr",
        };

        static HashSet<Prop> propHandles = new HashSet<Prop>();
        static Model stingerModel = new Model("p_stinger_04");
        static PlayerList playerList = new PlayerList();
        static MenuItemCheckbox menuItem = new MenuItemCheckbox
        {
            Title = "Deploy Spike Strip",
            OnActivate = (state, item) =>
            {
                if (state) DeploySpikes(); else RetractSpikes();
            }
        };

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
            InteractionListMenu.RegisterInteractionMenuItem(menuItem, () => !Game.PlayerPed.IsInVehicle() /*CurrentPlayer.CharacterData.Duty.HasFlag(Enums.Character.Duty.Police)*/, 1010);
        }

        static public Task OnTick()
        {
            if (ControlHelper.IsControlJustPressed(Settings.Controls["SpikeStrip.Toggle"]) && !CitizenFX.Core.Game.PlayerPed.IsInVehicle())
            {
                if (propHandles.Count() == 0)
                {
                    DeploySpikes();
                    (menuItem as MenuItemCheckbox).state = true;
                }
                else
                {
                    RetractSpikes();
                    (menuItem as MenuItemCheckbox).state = false;
                }
            }
            if(propHandles.Count > 0)
            {
                new VehicleList().ToList().Select(id => new CitizenFX.Core.Vehicle(id)).ToList().ForEach(v =>
                {
                    if (v.Position.DistanceToSquared(propHandles.First().Position) < Math.Pow(cullingDistance, 2))
                    {
                        propHandles.ToList().ForEach(s =>
                        {
                            wheelBoneNames.Keys.ToList().ForEach(i =>
                            {
                            if (v.Bones[wheelBoneNames[i]].Position.DistanceToSquared(s.Position) < Math.Pow(spikeRange, 2) && !Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, v.Handle, i, false))
                                {
                                    v.Wheels[i].Burst();
                                }
                            });
                        });
                    }
                });
            }
            return Task.FromResult(0);
        }

        static public void DeploySpikes()
        {
            Enumerable.Range(0, 3).ToList().ForEach(async n =>
            {
                Prop s = await World.CreateProp(stingerModel, CitizenFX.Core.Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 3.7f + 3f * n, 0f)), true, true);
                s.Heading = CitizenFX.Core.Game.PlayerPed.Heading;
                propHandles.Add(s);
            });
        }

        static public void RetractSpikes()
        {
            if (CitizenFX.Core.Game.PlayerPed.Position.DistanceToSquared(propHandles.First().Position) > Math.Pow(pickupDistance, 2))
            {
                Log.ToChat("Too far away to retract spikestrip.");
                return;
            }
            propHandles.ToList().ForEach(p => { p.Delete(); });
            propHandles.Clear();
        }
    }
}
