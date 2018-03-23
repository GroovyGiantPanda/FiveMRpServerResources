using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedModels;

namespace FamilyRP.Roleplay.Client.Classes.Player
{
    // TODO: Only let other people know about these events if they can see the player
    // Preferrably requiring the correct sign of the dot product between the other player's forward vector and the position difference between us so as to make sure the other player actually can see you
    // TODO: Transmit these events to other players (or just check locally when other player peds do these things)
    static class WeaponUnholsterHandler
    {
        static bool CurrentlyUnholstering = false;
        static bool HasRaisedGun = false;
        static WeaponHash PreviousWeapon = WeaponHash.Unarmed;
        static private bool isPolice = false;

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
            Client.GetInstance().PointEventHandlers["Weapons.ManipulationEvent"] = ReceivedWeaponEvent;
        }


        static private async Task OnTick()
        {
            if (!CurrentlyUnholstering)
            {
                WeaponHash CurrentWeapon = Game.PlayerPed.Weapons.Current.Hash;
                if (!isPolice && CurrentWeapon != PreviousWeapon && CurrentWeapon != WeaponHash.Unarmed && CurrentWeapon != (WeaponHash) 966099553 /* WeaponHash for WEAPON_OBJECT; not in enum */)
                {
                    CurrentlyUnholstering = true;
                    SendWeaponEvent("manipulate");
                    //BaseScript.TriggerEvent("Chat.Message", "", "#FF0000", "Somebody nearby is manipulating a weapon!");
                    Function.Call(Hash.SET_CURRENT_PED_WEAPON, Game.PlayerPed.Handle,  WeaponHash.Unarmed, true);
                    await BaseScript.Delay(1000);
                    Function.Call(Hash.SET_CURRENT_PED_WEAPON, Game.PlayerPed.Handle, CurrentWeapon, true);
                    CurrentlyUnholstering = false;
                }
                PreviousWeapon = CurrentWeapon;

                if (!HasRaisedGun && Game.Player.IsAiming)
                {
                    HasRaisedGun = true;
                    //BaseScript.TriggerEvent("Chat.Message", "", "#AA4444", "Somebody nearby raised a weapon!");
                    SendWeaponEvent("raise");
                }
                else if(HasRaisedGun && !Game.Player.IsAiming)
                {
                    await BaseScript.Delay(100);
                    if (Game.Player.IsAiming) return;
                    HasRaisedGun = false;
                    //BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", "The weapon was lowered.");
                    SendWeaponEvent("lower");
                }
            }
        }

        static void SendWeaponEvent(string weaponEvent)
        {
            const float weaponEventAoe = 25f;
            PointEvent pointEvent = new PointEvent("Weapons.ManipulationEvent", CitizenFX.Core.Game.PlayerPed.Position.ToArray(), weaponEventAoe, weaponEvent, Game.Player.ServerId, false);
            BaseScript.TriggerServerEvent("TriggerEventNearPoint", Helpers.MsgPack.Serialize(pointEvent));
        }

        static PlayerList playerList = new PlayerList();

        static Task ReceivedWeaponEvent(PointEvent pointEvent)
        {
            try
            { 
                if(Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY_IN_FRONT, Game.PlayerPed.Handle, playerList[pointEvent.SourceServerId].Character.Handle))
                { 
                    if(pointEvent.SerializedArguments == "lower") // May serialize these later from an enum or something just to make everything look nicer, but this is not that bad
                    {
                        //BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", "The weapon was lowered.");
                    }
                    else if (pointEvent.SerializedArguments == "raise")
                    {
                        //BaseScript.TriggerEvent("Chat.Message", "", "#AA4444", "Somebody nearby raised a weapon!");
                    }
                    else if (pointEvent.SerializedArguments == "manipulate")
                    {
                        //BaseScript.TriggerEvent("Chat.Message", "", "#EE5555", "Somebody nearby is manipulating a weapon!");
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error($"ReceivedWeaponEvent: {ex.Message}");
            }
            return Task.FromResult(0);
        }
    }
}
