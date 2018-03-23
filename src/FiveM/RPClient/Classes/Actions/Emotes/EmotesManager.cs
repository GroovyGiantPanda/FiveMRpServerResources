using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyRP.Roleplay.SharedClasses;
using CitizenFX.Core;
using FamilyRP.Roleplay.Client.Classes.Actions.Jobs.Police;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Common;
using FamilyRP.Roleplay.Enums.Controls;
using FamilyRP.Roleplay.Enums.Police;

namespace FamilyRP.Roleplay.Client
{
    static class EmotesManager
    {
        static Dictionary<string, string> scenarios = new Dictionary<string, string>()
        {
            ["cheer"] = "WORLD_HUMAN_CHEERING",
            ["sit"] = "WORLD_HUMAN_PICNIC",
            ["sitchair"] = "PROP_HUMAN_SEAT_CHAIR_MP_PLAYER",
            ["lean"] = "WORLD_HUMAN_LEANING",
            ["hangout"] = "WORLD_HUMAN_HANG_OUT_STREET",
            ["cop"] = "WORLD_HUMAN_COP_IDLES",
            ["bum"] = "WORLD_HUMAN_BUM_STANDING",
            ["kneel"] = "CODE_HUMAN_MEDIC_KNEEL",
            ["medic"] = "CODE_HUMAN_MEDIC_TEND_TO_DEAD",
			["musician"] = "WORLD_HUMAN_MUSICIAN",
            ["film"] = "WORLD_HUMAN_MOBILE_FILM_SHOCKING",
            ["guard"] = "WORLD_HUMAN_GUARD_STAND",
            ["phone"] = "WORLD_HUMAN_STAND_MOBILE",
            ["traffic"] = "WORLD_HUMAN_CAR_PARK_ATTENDANT",
            ["bumsleep"] = "WORLD_HUMAN_BUM_SLUMPED",
			["smoke"] = "WORLD_HUMAN_SMOKING",
			["drink"] = "WORLD_HUMAN_DRINKING",
            ["dealer"] = "WORLD_HUMAN_DRUG_DEALER",
            ["dealerhard"] = "WORLD_HUMAN_DRUG_DEALER_HARD",
            ["patrol"] = "WORLD_HUMAN_GUARD_PATROL",
            ["hangout"] = "WORLD_HUMAN_HANG_OUT_STREET",
            ["hikingstand"] = "WORLD_HUMAN_HIKER_STANDING",
            ["statue"] = "WORLD_HUMAN_HUMAN_STATUE",
            ["jog"] = "WORLD_HUMAN_JOG_STANDING",
            ["maid"] = "WORLD_HUMAN_MAID_CLEAN",
            ["flex"] = "WORLD_HUMAN_MUSCLE_FLEX",
            ["weights"] = "WORLD_HUMAN_MUSCLE_FLEX",
            ["party"] = "WORLD_HUMAN_PARTYING",
            ["prosthigh"] = "WORLD_HUMAN_PROSTITUTE_HIGH_CLASS",
            ["prostlow"] = "WORLD_HUMAN_PROSTITUTE_LOW_CLASS",
            ["pushup"] = "WORLD_HUMAN_PUSH_UPS",
            ["sitsteps"] = "WORLD_HUMAN_SEAT_STEPS",
            ["sitwall"] = "WORLD_HUMAN_SEAT_WALL",
            ["situp"] = "WORLD_HUMAN_SIT_UPS",
            ["fire"] = "WORLD_HUMAN_STAND_FIRE",
            ["impatient"] = "WORLD_HUMAN_STAND_IMPATIENT",
            ["impatientup"] = "WORLD_HUMAN_STAND_IMPATIENT_UPRIGHT",
            ["mobileup"] = "WORLD_HUMAN_STAND_MOBILE_UPRIGHT",
            ["stripwatch"] = "WORLD_HUMAN_STRIP_WATCH_STAND",
            ["stupor"] = "WORLD_HUMAN_STUPOR",
            ["sunbathe"] = "WORLD_HUMAN_SUNBATHE",
            ["sunbatheback"] = "WORLD_HUMAN_SUNBATHE_BACK",
            ["map"] = "WORLD_HUMAN_TOURIST_MAP",
            ["tourist"] = "WORLD_HUMAN_TOURIST_MOBILE",
            ["mechanic"] = "WORLD_HUMAN_VEHICLE_MECHANIC",
            ["windowshop"] = "WORLD_HUMAN_WINDOW_SHOP_BROWSE",
            ["yoga"] = "WORLD_HUMAN_YOGA",
            ["atm"] = "PROP_HUMAN_ATM",
            ["bumbin"] = "PROP_HUMAN_BUM_BIN",
            ["cart"] = "PROP_HUMAN_BUM_SHOPPING_CART",
            ["chinup"] = "PROP_HUMAN_MUSCLE_CHIN_UPS",
            ["chinuparmy"] = "PROP_HUMAN_MUSCLE_CHIN_UPS_ARMY",
            ["chinupprison"] = "PROP_HUMAN_MUSCLE_CHIN_UPS_PRISON",
            ["parkingmeter"] = "PROP_HUMAN_PARKING_METER",
            ["armchair"] = "PROP_HUMAN_SEAT_ARMCHAIR",
            ["crossroad"] = "CODE_HUMAN_CROSS_ROAD_WAIT",
            ["crowdcontrol"] = "CODE_HUMAN_POLICE_CROWD_CONTROL",
            ["investigate"] = "CODE_HUMAN_POLICE_INVESTIGATE"
        };

        static public bool isPlayingEmote = false;

        static public void Init()
        {
            Client.GetInstance().ClientCommands.Register("/emote", HandleEmote);
            Client.GetInstance().RegisterTickHandler(OnTick);

            var CancelMenuItem = new MenuItemStandard
            {
                Title = "Cancel Emote",
                OnActivate = (item) =>
                {
                    //if (!(Arrest.playerCuffState == CuffState.None)) return;
                    Game.PlayerPed.Task.ClearAll();
                    isPlayingEmote = false;
                    InteractionListMenu.Observer.CloseMenu();
                }
            };

            var CancelImmediatelyMenuItem = new MenuItemStandard
            {
                Title = "Cancel Emote (Immediately)",
                OnActivate = (item) =>
                {
                    //if (!(Arrest.playerCuffState == CuffState.None)) return;
                    Game.PlayerPed.Task.ClearAllImmediately();
                    isPlayingEmote = false;
                    InteractionListMenu.Observer.CloseMenu();
                }
            };

            InteractionListMenu.RegisterInteractionMenuItem(CancelMenuItem, () => { return isPlayingEmote; }, 1150);
            InteractionListMenu.RegisterInteractionMenuItem(CancelImmediatelyMenuItem, () => { return isPlayingEmote; }, 1149);

            var CancelEmoteMenu = new MenuModel { headerTitle = "Emotes", menuItems = new List<MenuItem>() { CancelMenuItem, CancelImmediatelyMenuItem } };

            List<MenuItem> EmotesMenuItems = new List<MenuItem>();
            scenarios.OrderBy(x => x.Key).ToList().ForEach(s =>
            {
                EmotesMenuItems.Add(new MenuItemStandard
                {
                    Title = $"{s.Key.ToTitleCase()}",
                    OnActivate = (item) =>
                    {
                        PlayEmote(item.Title.ToLower());
                        InteractionListMenu.Observer.OpenMenu(CancelEmoteMenu);
                    }
                });
            });
            var EmotesMenu = new MenuModel { headerTitle = "Emotes", menuItems = EmotesMenuItems };
            var ToEmotesMenuItem = new MenuItemSubMenu { Title = "Emotes", SubMenu = EmotesMenu };
            InteractionListMenu.RegisterInteractionMenuItem(ToEmotesMenuItem, () => { return !isPlayingEmote && !Game.PlayerPed.IsInVehicle(); }, 1150);
        }
        private static Task OnTick()
        {
            try
            {
                if (/* Arrest.playerCuffState == Enums.Police.CuffState.None && */ isPlayingEmote && (ControlHelper.IsControlJustPressed(Control.MoveUpOnly, false, ControlModifier.Any) || ControlHelper.IsControlJustPressed(Control.MoveDown, false, ControlModifier.Any) || ControlHelper.IsControlJustPressed(Control.MoveLeft, false, ControlModifier.Any) || ControlHelper.IsControlJustPressed(Control.MoveRight, false, ControlModifier.Any)) && !Game.PlayerPed.IsInVehicle() && Game.PlayerPed.VehicleTryingToEnter == null)
                {
                    isPlayingEmote = false;
                    Game.PlayerPed.Task.ClearAll();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Emotes OnTick error: {ex.Message}");
            }
            return Task.FromResult(0);
        }

        static private void HandleEmote(Command command)
        {
            try
            {
                if (command.Args.Count == 0)
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", $"Valid emotes: {String.Join(", ", scenarios.Select(e => e.Key.ToTitleCase()))}");
                }
                else if (command.Args.Count == 1 && scenarios.ContainsKey(command.Args.Get(0).ToLower()))
                {
                    PlayEmote(command.Args.Get(0).ToLower());
                }
                else
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", $"Invalid emote specified");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"HandleEmote error: {ex.Message}");
            }
        }

        private static void PlayEmote(string emoteName)
        {
            try
            {
                //if (Arrest.playerCuffState != Enums.Police.CuffState.None) return;
                Function.Call(Hash.SET_SCENARIO_TYPE_ENABLED, scenarios[emoteName]);
                Function.Call(Hash.RESET_SCENARIO_TYPES_ENABLED);
                if (!Game.PlayerPed.IsInVehicle())
                {
                    Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, Game.PlayerPed.Handle, scenarios[emoteName], 0, true);
                    isPlayingEmote = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PlayEmote error: {ex.Message}");
            }
        }
    }
}
