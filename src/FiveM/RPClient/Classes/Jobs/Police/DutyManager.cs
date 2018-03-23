using CitizenFX.Core;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Roleplay.Enums.Character;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Jobs.Police
{
    static class DutyManager
    {
        static int cleanupInterval = 60000;

        static float DutyToggleRange = 3f;

        static Dictionary<Duty, Tuple<BlipSprite, BlipColor>> DutyConstants = new Dictionary<Duty, Tuple<BlipSprite, BlipColor>>()
        {
            [Duty.EMS] = new Tuple<BlipSprite, BlipColor>((BlipSprite)77, BlipColor.Red),
            [Duty.Police] = new Tuple<BlipSprite, BlipColor>((BlipSprite)78, BlipColor.Red),
            [Duty.FireDept] = new Tuple<BlipSprite, BlipColor>((BlipSprite)86, BlipColor.MichaelBlue)
        };

        /// <summary>
        /// int = CharID
        /// Keeps track of duty vehicle blips
        /// </summary>
        public static Dictionary<int, Tuple<CitizenFX.Core.Player, Duty, CitizenFX.Core.Vehicle, Blip>> duty = new Dictionary<int, Tuple<CitizenFX.Core.Player, Duty, CitizenFX.Core.Vehicle, Blip>>();
        static PlayerList playerList = new PlayerList();
        static OtherPlayerList otherPlayerList = new OtherPlayerList();
        static CitizenFX.Core.Vehicle vehicle = null;
        private static bool isAtValidDutyLocation = false;
        private static Duty AtDutyLocationType;

        static public void Init()
        {
            Client.ActiveInstance.RegisterEventHandler("Duty.UpdateDuty", new Action<int, int, bool>(HandleUpdateDuty));
            Client.ActiveInstance.RegisterEventHandler("Duty.InitialDuty", new Action<string>(HandleInitialDuty));
            Client.ActiveInstance.RegisterEventHandler("playerSpawned", new Action(HandlePlayerSpawned));
            Client.ActiveInstance.RegisterEventHandler("Duty.UpdateVehicleBlip", new Action<int>(HandleUpdateBlip));
            
            var menuItem = new MenuItemStandard { Title = "Toggle Duty", OnActivate = (item) => { AttemptDutyToggle(); } };
            InteractionListMenu.RegisterInteractionMenuItem(menuItem, () => isAtValidDutyLocation, 1050);
            Client.ActiveInstance.RegisterTickHandler(OnTick);
            RegularBlipCleanup();
        }

        private static void HandleUpdateDuty(int charId, int toggleDuty, bool toggle)
        {
            try
            {
                var player = otherPlayerList.Where(p => p.CharID == charId).First().Player;
                //var player = Game.Player;

                duty[charId] = new Tuple<CitizenFX.Core.Player, Duty, CitizenFX.Core.Vehicle, Blip>(player, (Duty) toggleDuty, null, null);
                if(player == Game.Player)
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#5555CC", $"You are now {(toggle ? "on" : "off")} duty.");
                }

                Log.Debug($"Received duty toggle for charId {charId} ({(toggle ? "on" : "off")} duty)");
            }
            catch (Exception ex)
            {
                Log.Error($"DutyManager HandleUpdateDuty Error: {ex.Message}");
            }
        }

        private static void HandleInitialDuty(string serializedDutyDictionary)
        {
            try
            {
                Dictionary<int, Duty> initialDuty = Helpers.MsgPack.Deserialize<Dictionary<int, Duty>>(serializedDutyDictionary);
                initialDuty.ToList().ForEach(d =>
                {
                    try
                    {
                        var player = otherPlayerList.Where(p => p.CharID == d.Key).First().Player;
                        CitizenFX.Core.Vehicle dutyVehicle = null;
                        Blip blip = null;

                        if (player.Character.IsInVehicle())
                        {
                            dutyVehicle = player.Character.CurrentVehicle;
                            blip = dutyVehicle.AttachBlip();
                            blip.IsShortRange = true;
                            blip.Sprite = DutyConstants[d.Value].Item1;
                            blip.Color = DutyConstants[d.Value].Item2;
                        }
                        duty.Add(d.Key, new Tuple<CitizenFX.Core.Player, Duty, CitizenFX.Core.Vehicle, Blip>(player, d.Value, dutyVehicle, blip));
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"HandleInitialDuty (inner) Error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error($"DutyManager HandleInitialDuty Error: {ex.Message}");
            }
        }

        private static void HandlePlayerSpawned()
        {
            try
            {
                BaseScript.TriggerServerEvent("Duty.InitialRequest");
            }
            catch (Exception ex)
            {
                Log.Error($"DutyManager HandlePlayerSpawned Error: {ex.Message}");
            }
        }

        private static void HandleUpdateBlip(int charId)
        {
            try
            {
                if (duty[charId].Item1.Character.IsInVehicle())
                {
                    var dutyVehicle = duty[charId].Item1.Character.CurrentVehicle;
                    var blip = dutyVehicle.AttachBlip();
                    blip.IsShortRange = true;
                    blip.Sprite = DutyConstants[duty[charId].Item2].Item1;
                    blip.Color = DutyConstants[duty[charId].Item2].Item2;
                    if (duty[charId].Item4 != null) duty[charId].Item4.Delete();
                    duty[charId] = new Tuple<CitizenFX.Core.Player, Duty, CitizenFX.Core.Vehicle, Blip>(duty[charId].Item1, duty[charId].Item2, dutyVehicle, blip);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DutyManager HandleUpdateBlip Error: {ex.Message}");
            }
        }

        private static Task OnTick()
        {
            try
            {
                if(isAtValidDutyLocation && ControlHelper.IsControlJustPressed(Control.Context))
                {
                    AttemptDutyToggle();
                }
                if (false && ((vehicle == null && Game.PlayerPed.IsInVehicle()) || vehicle != Game.PlayerPed.CurrentVehicle))
                {
                    vehicle = Game.PlayerPed.CurrentVehicle;
                    BaseScript.TriggerServerEvent("Duty.UpdateVehicleBlip");
                }

                if (!Game.PlayerPed.IsInVehicle()) vehicle = null;

                isAtValidDutyLocation = false;
                AtDutyLocationType = Duty.Civ;
                EmergencyServices.VehSpawnLocationDictionary.ToList().ForEach(o => {
                    if (CurrentPlayer.Character == null || !((CurrentPlayer.CharacterData.Duty &= o.Key) > 0)) return;
                    if (o.Value.Any((loc) => Game.PlayerPed.Position.DistanceToSquared(loc) < 25f))
                    {
                        isAtValidDutyLocation = true;
                        AtDutyLocationType = o.Key;
                    }
                    Log.Debug("D");
                    // For debug
                    //if(o.Value.Count > 0) Log.Debug($"{String.Join(", ", o.Value.Select(loc => Game.PlayerPed.Position.DistanceToSquared(loc).ToString()))}");
                });
            }
            catch(Exception ex)
            {
                Log.Error($"DutyManager OnTick Error: {ex.Message}");
            }
            return Task.FromResult(0);
        }

        private static void AttemptDutyToggle()
        {
            try
            {
                if ((CurrentPlayer.CharacterData.Duty &= AtDutyLocationType) > 0 && isAtValidDutyLocation)
                {
                    if (/*false*/duty.ContainsKey(CurrentPlayer.CharacterData.CharID))
                    {
                        BaseScript.TriggerServerEvent("Duty.OutgoingToggle", (int)AtDutyLocationType, false);
                        Log.Debug("Sent off-duty");
                    }
                    else
                    {
                        BaseScript.TriggerServerEvent("Duty.OutgoingToggle", (int)AtDutyLocationType, true);
                        Log.Debug("Sent on-duty");
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error($"AttemptDutyToggle error: {ex.Message}");
            }
        }

        static public void ReceiveDutyToggle(int charId, Duty toggleDuty, bool toggle)
        {
            try
            {
                if (toggle)
                {
                    var playerQuery = otherPlayerList.Where(p => p.CharID == charId);
                    if (playerQuery.Count() > 0)
                    {
                        var targetPlayer = playerQuery.First();
                        //var targetPlayer = new { Ped = Game.PlayerPed, Player = Game.Player };
                        if (targetPlayer.Ped.IsInVehicle())
                        {
                            Blip blip = targetPlayer.Ped.CurrentVehicle.AttachBlip();
                            blip.IsShortRange = true;
                            blip.Sprite = DutyConstants[toggleDuty].Item1;
                            blip.Color = DutyConstants[toggleDuty].Item2;
                            duty[charId] = new Tuple<CitizenFX.Core.Player, Duty, CitizenFX.Core.Vehicle, Blip>(targetPlayer.Player, toggleDuty, targetPlayer.Ped.CurrentVehicle, blip);
                        }
                        else
                        {
                            duty[charId] = new Tuple<CitizenFX.Core.Player, Duty, CitizenFX.Core.Vehicle, Blip>(targetPlayer.Player, toggleDuty, null, null);
                        }
                    }
                }
                else if (duty.ContainsKey(charId))
                { 
                    if (duty[charId].Item3 != null && duty[charId].Item3.Exists()) duty[charId].Item3.Delete();
                    if (duty[charId].Item4 != null && duty[charId].Item4.Exists()) duty[charId].Item4.Delete();
                    duty.Remove(charId);
                }
                Log.Debug($"Received duty toggle for charId {charId} ({(toggle ? "on" : "off")} duty)");
            }
            catch (Exception ex)
            {
                Log.Error($"ReceiveDutyToggle error: {ex.Message}");
            }
        }

        /// <summary>
        /// This is just an extra measure to be safe in the sense that we don't have orphaned blips/emergency vehicles around the map
        /// </summary>
        private static async void RegularBlipCleanup()
        {
            while(true)
            {
                duty.ToList().ForEach(d =>
                {
                    if (!otherPlayerList.ContainsCharID(d.Key))
                    {
                        if (d.Value.Item3 != null && d.Value.Item3.Exists()) d.Value.Item3.Delete();
                        if (d.Value.Item4 != null && d.Value.Item4.Exists()) d.Value.Item4.Delete();
                        duty.Remove(d.Key);
                    }
                });
                await BaseScript.Delay(cleanupInterval);
            }
        }
    }
}
