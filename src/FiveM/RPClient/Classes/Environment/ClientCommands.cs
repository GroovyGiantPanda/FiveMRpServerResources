using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.SharedModels;
using FamilyRP.Roleplay.Enums.Character;
using System.Diagnostics;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Roleplay.Client.Classes.Jobs.Police;
using SharedModels;

namespace FamilyRP.Roleplay.Client
{
    public class ClientCommands : CommandProcessor
    {
        PlayerList playerList = new PlayerList();

        public ClientCommands()
        {
            Client.GetInstance().RegisterEventHandler("chatCommandEntered", new Action<string>(Handle));

            Register("/me", SendSlashMe);
            Client.GetInstance().PointEventHandlers["Communication.SlashMe"] = new Func<PointEvent, Task>(ReceivedSlashMe);
            Register("/911", Outgoing911);
            Client.GetInstance().RegisterEventHandler("Communication.Incoming911", new Action<string>(Incoming911));
            Register("/311", Outgoing311);
            Client.GetInstance().RegisterEventHandler("Communication.Incoming311", new Action<string>(Incoming311));
            Register("/callerid", EmergencyCallerId);

            Register("/911r", Outgoing911Reply);
            Client.GetInstance().RegisterEventHandler("Communication.Incoming911Reply", new Action<string>(Incoming911Reply));
            Register("/311r", Outgoing311Reply);
            Client.GetInstance().RegisterEventHandler("Communication.Incoming311Reply", new Action<string>(Incoming311Reply));

            Register("/jail", JailCharacter);

            Register("/ticket", TicketCharacter);
            // refuse/accept potentially with a menu popping up later, or just some other prompt
            Register("/bill", BillCharacter);
			
            //Register("/vin", CheckVin);
            Register("/installsirens", InstallSirens);

            Register("/alertems", OutgoingAlertForEms);
            Register("/alertmedic", OutgoingAlertForEms);
            Client.GetInstance().RegisterEventHandler("Communication.IncomingAlert.Ems", new Action<string>(IncomingAlertForEms));

            Register("/alertcops", OutgoingAlertForPolice);
            Register("/alertpolice", OutgoingAlertForPolice);
            Client.GetInstance().RegisterEventHandler("Communication.IncomingAlert.Police", new Action<string>(IncomingAlertForPolice));
            //Register("/alerttaxi", OutgoingAlertForTaxi);
            Client.GetInstance().RegisterEventHandler("Communication.IncomingAlert.Taxi", new Action<string>(IncomingAlertForTaxi));
        }

        //// Everybody can check their permissions
        //private void ListPermissions(Command command)
        //{
        //    try
        //    {
        //        List<string> flags = new List<string>(CurrentPlayer.Character.Data.Permissions.ToString().Split(','));
        //        BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", $"Permissions: {String.Join(", ", flags)}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error($"ListPermissions error: {ex.Message}");
        //    }
        //}

        
        private void JailCharacter(Command command)
        {
            try
            {
                if(command.Args.Count >= 2)
                {
                    int targetServerId = command.Args.GetInt32(0);
                    if(!playerList.Any(p => p.ServerId == targetServerId))
                    {
                        BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", "That player ID could not be found!");
                        return;
                    }
                    int jailTime = command.Args.GetInt32(1);
                    string description = command.Args.Count >= 3 ? command.Args.ToString().Substring(command.Args.Get(0).Length + 1 + command.Args.Get(1).Length + 1) : "";
                    BaseScript.TriggerServerEvent("Police.ImprisonCharacter", targetServerId, jailTime, description);
                }
                else
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", "Invalid parameters provided; you need to provide at least two numeric arguments.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"JailCharacter error: {ex.Message}");
            }
        }

        
        private void TicketCharacter(Command command)
        {
            try
            {
                if (command.Args.Count >= 2)
                {
                    int targetServerId = command.Args.GetInt32(0);
                    if (!playerList.Any(p => p.ServerId == targetServerId))
                    {
                        BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", "That player ID could not be found!");
                    }
                    int ticketAmount = command.Args.GetInt32(1);
                    string description = command.Args.Count >= 3 ? command.Args.ToString().Substring(command.Args.Get(0).Length + 1 + command.Args.Get(1).Length + 1) : "";
                    BaseScript.TriggerServerEvent("Police.TicketCharacter", targetServerId, ticketAmount, description);
                }
                else
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", "Invalid parameters provided; you need to provide at least two numeric arguments.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"TicketCharacter error: {ex.Message}");
            }
        }

        
        private void BillCharacter(Command command)
        {
            try
            {
                if (command.Args.Count >= 2)
                {
                    int targetServerId = command.Args.GetInt32(0);
                    if (!playerList.Any(p => p.ServerId == targetServerId))
                    {
                        BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", "That player ID could not be found!");
                    }
                    int billAmount = command.Args.GetInt32(1);
                    string description = command.Args.Count >= 3 ? command.Args.ToString().Substring(command.Args.Get(0).Length + 1 + command.Args.Get(1).Length + 1) : "";
                    BaseScript.TriggerServerEvent("Police.BillCharacter", targetServerId, billAmount, description);
                }
                else
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", "Invalid parameters provided; you need to provide at least two numeric arguments.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"BillCharacter error: {ex.Message}");
            }
        }

        
        private void InstallSirens(Command command)
        {
            try
            {
                if(!Game.PlayerPed.IsInVehicle())
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", "You have to be in the vehicle to do that!");
                    return;
                }

                bool AreSirensInstalled = API.DecorExistOn(Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.SirensInstalled") ? API.DecorGetBool(Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.SirensInstalled") : Game.PlayerPed.IsInPoliceVehicle;
                bool targetState;
                if (command.Args.Count < 1)
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", $"Sirens {(AreSirensInstalled ? "removed" : "installed")}.");
                    API.DecorSetBool(Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.SirensInstalled", !AreSirensInstalled);
                    return;
                }
                else if(bool.TryParse(command.Args.Get(0), out targetState))
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", $"Sirens {(targetState ? "installed" : "removed")}.");
                    API.DecorSetBool(Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.SirensInstalled", targetState);
                }
                else
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", "Invalid arguments given.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"InstallSirens error: {ex.Message}");
            }
        }
        
        private void Incoming311Reply(string message)
        {
            try
            {
                BaseScript.TriggerEvent("Chat.Message", "311", "#66DD66", message);
            }
            catch (Exception ex)
            {
                Log.Error($"Incoming311Reply error: {ex.Message}");
            }
        }

        
        private void Outgoing311Reply(Command command)
        {
            try
            {
                int result;
                bool IsCharacterIdSpecified = Int32.TryParse(command.Args.Get(0), out result);
                if(!IsCharacterIdSpecified && lastCallCharId == -1)
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#DD6666", "No recent emergency calls.");
                    return;
                }
                string playerEvent = Helpers.MsgPack.Serialize(new TriggerEventForPlayersModel(IsCharacterIdSpecified ? result : lastCallCharId, "Communication.Incoming311Reply", command.Args.ToString().Substring(command.Args.Get(0).Length + 1)));
                BaseScript.TriggerServerEvent("TriggerEventForPlayers", playerEvent);
            }
            catch (Exception ex)
            {
                Log.Error($"Outgoing311Reply error: {ex.Message}");
            }
        }

        private void Incoming911Reply(string message)
        {
            try
            {
                BaseScript.TriggerEvent("Chat.Message", "911", "#6666DD", message);
            }
            catch (Exception ex)
            {
                Log.Error($"Incoming911Reply error: {ex.Message}");
            }
        }

        
        private void Outgoing911Reply(Command command)
        {
            try
            {
                int result;
                bool IsCharacterIdSpecified = Int32.TryParse(command.Args.Get(0), out result);
                if (!IsCharacterIdSpecified && lastCallCharId == -1)
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#DD6666", "No recent emergency calls.");
                    return;
                }
                string playerEvent = Helpers.MsgPack.Serialize(new TriggerEventForPlayersModel(IsCharacterIdSpecified ? result : lastCallCharId, "Communication.Incoming911Reply", command.Args.ToString().Substring(command.Args.Get(0).Length + 1)));
                BaseScript.TriggerServerEvent("TriggerEventForPlayers", playerEvent);
            }
            catch (Exception ex)
            {
                Log.Error($"Outgoing911Reply error: {ex.Message}");
            }
        }

        
        private void OutgoingCode13(Command command)
        {
            string currentStreetName = CitizenFX.Core.World.GetStreetName(Game.PlayerPed.Position);
            string currentCrossingName = Location.GetCrossingName(Game.PlayerPed.Position);
            string localizedZone = World.GetZoneLocalizedName(Game.PlayerPed.Position);
            string alert = Helpers.MsgPack.Serialize(new AlertModel { Position = Game.PlayerPed.Position.ToArray(), Street = currentStreetName, CrossingStreet = currentCrossingName, Zone = localizedZone/*, SourceCharId = CurrentPlayer.CharacterData.CharID */ });
            string jobEvent = Helpers.MsgPack.Serialize(new TriggerEventForDutyModel(Duty.Police, "Communication.IncomingAlert.Code13", alert));
            BaseScript.TriggerServerEvent("TriggerEventForDuty", jobEvent);
            jobEvent = Helpers.MsgPack.Serialize(new TriggerEventForDutyModel(Duty.EMS, "Communication.IncomingAlert.Code13", alert));
            BaseScript.TriggerServerEvent("TriggerEventForDuty", jobEvent);
        }

        static public int lastCallCharId = -1;
        // The below will store the last emergency call for each character ID
        static public Dictionary<int, EmergencyCallModel> emergencyCalls = new Dictionary<int, EmergencyCallModel>();

        public class AlertModel
        {
            public int SourceCharId { get; set; }
            public float[] Position { get; set; }
            public string Zone { get; set; } = "";
            public string Street { get; set; } = "";
            public string CrossingStreet { get; set; } = "";
            public AlertModel() { }
        }

        public class EmergencyCallModel
        {
            public int SourceCharId { get; set; }
            public float[] Position { get; set; }
            public string Zone { get; set; }
            public string Street { get; set; }
            public string CrossingStreet { get; set; }
            public string Message { get; set; }
            public EmergencyCallModel() { }
        }

        private async void IncomingAlertForTaxi(string serializedAlert)
        {
            try
            {
                AlertModel alert = Helpers.MsgPack.Deserialize<AlertModel>(serializedAlert);
                Log.ToChat($"Received taxi blip with coords {alert.Position}");
                Blip taxiAlertBlip = World.CreateBlip(alert.Position.ToVector3());
                taxiAlertBlip.IsFlashing = true;
                await BaseScript.Delay(60 * 1000);
                taxiAlertBlip.Delete();
            }
            catch (Exception ex)
            {
                Log.Error($"IncomingAlertForTaxi error: {ex.Message}");
            }
        }

        //private void OutgoingAlertForTaxi(Command command)
        //{
        //    try
        //    {
        //        string alert = Helpers.MsgPack.Serialize(new AlertModel { Position = Game.PlayerPed.Position.ToArray(), /*SourceCharId = CurrentPlayer.CharacterData.CharID*/ });
        //        string jobEvent = Helpers.MsgPack.Serialize(new TriggerEventForJobModel(Job.Taxi, "Communication.IncomingAlert.Taxi", alert));
        //        BaseScript.TriggerServerEvent("TriggerEventForJob", jobEvent);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error($"OutgoingAlertForTaxi error: {ex.Message}");
        //    }
        //}

        private async void IncomingAlertForPolice(string serializedAlert)
        {
            try
            {
                AlertModel alert = Helpers.MsgPack.Deserialize<AlertModel>(serializedAlert);
                Log.ToChat($"Received police blip with coords {alert.Position.ToVector3()}; {alert.Zone}; {alert.Street}; {alert.CrossingStreet}");
                Blip policeAlertBlip = World.CreateBlip(alert.Position.ToVector3());
                policeAlertBlip.IsFlashing = true;
                await BaseScript.Delay(60 * 1000);
                policeAlertBlip.Delete();
            }
            catch (Exception ex)
            {
                Log.Error($"IncomingAlertForPolice error: {ex.Message}");
            }
        }

        private void OutgoingAlertForPolice(Command command)
        {
            try
            {
                // Think some of these may require the player to be at the location, hence why they are fetched beforehand and packed in model
                string currentStreetName = CitizenFX.Core.World.GetStreetName(Game.PlayerPed.Position);
                string currentCrossingName = Location.GetCrossingName(Game.PlayerPed.Position);
                string localizedZone = World.GetZoneLocalizedName(Game.PlayerPed.Position);
                string alert = Helpers.MsgPack.Serialize(new AlertModel { Position = Game.PlayerPed.Position.ToArray(), Street = currentStreetName, CrossingStreet = currentCrossingName, Zone = localizedZone, /*SourceCharId = CurrentPlayer.CharacterData.CharID*/ });
                string dutyEvent = Helpers.MsgPack.Serialize(new TriggerEventForDutyModel(Duty.Police, "Communication.IncomingAlert.Police", alert));
                BaseScript.TriggerServerEvent("TriggerEventForDuty", dutyEvent);
            }
            catch (Exception ex)
            {
                Log.Error($"OutgoingAlertForPolice error: {ex.Message}");
            }
        }

        private async void IncomingAlertForEms(string serializedAlert)
        {
            try
            {
                AlertModel alert = Helpers.MsgPack.Deserialize<AlertModel>(serializedAlert);
                Log.ToChat($"Received EMS blip with coords {alert.Position}; {alert.Zone}; {alert.Street}; {alert.CrossingStreet}");
                Blip emsAlertBlip = World.CreateBlip(alert.Position.ToVector3());
                emsAlertBlip.IsFlashing = true;
                await BaseScript.Delay(60 * 1000);
                emsAlertBlip.Delete();
            }
            catch (Exception ex)
            {
                Log.Error($"IncomingAlertForEms error: {ex.Message}");
            }
        }

        private void OutgoingAlertForEms(Command command)
        {
            try
            {
                // Think some of these may require the player to be at the location, hence why they are fetched beforehand and packed in model
                string currentStreetName = CitizenFX.Core.World.GetStreetName(Game.PlayerPed.Position);
                string currentCrossingName = Location.GetCrossingName(Game.PlayerPed.Position);
                string localizedZone = World.GetZoneLocalizedName(Game.PlayerPed.Position);
                string alert = Helpers.MsgPack.Serialize(new AlertModel { Position = Game.PlayerPed.Position.ToArray(), Street = currentStreetName, CrossingStreet = currentCrossingName, Zone = localizedZone, /*SourceCharId = CurrentPlayer.CharacterData.CharID*/ });
                string dutyEvent = Helpers.MsgPack.Serialize(new TriggerEventForDutyModel(Duty.EMS, "Communication.IncomingAlert.EMS", alert));
                BaseScript.TriggerServerEvent("TriggerEventForDuty", dutyEvent);
            }
            catch (Exception ex)
            {
                Log.Error($"OutgoingAlertForEms error: {ex.Message}");
            }
        }

        
        private void EmergencyCallerId(Command command)
        {
            try
            {
                int charId = lastCallCharId;
                if (command.Args.Count > 0)
                {
                    charId = command.Args.GetInt32(0);
                }

                BaseScript.TriggerEvent("Chat.Message", "Caller ID", "#77DD99", $"Caller's location appears to be at {emergencyCalls[charId].Zone} on {emergencyCalls[charId].Street}{(String.IsNullOrWhiteSpace(emergencyCalls[charId].CrossingStreet) ? "" : $" and {emergencyCalls[charId].CrossingStreet}")}.");
            }
            catch (Exception ex)
            {
                Log.Error($"EmergencyCallerId error: {ex.Message}");
            }
        }

        private void Incoming311(string serializedCall)
        {
            try
            {
                EmergencyCallModel call = Helpers.MsgPack.Deserialize<EmergencyCallModel>(serializedCall);
                BaseScript.TriggerEvent("Chat.Message", "311", "#6655EE", $"(( Call #{call.SourceCharId} )) {call.Message}");
                if (!emergencyCalls.ContainsKey(call.SourceCharId))
                    EmergencyCallerId(new Command($"/callerid {call.SourceCharId}")); // TODO: refactor so this ugly hack won't be used
                emergencyCalls[call.SourceCharId] = call;
                lastCallCharId = call.SourceCharId;
            }
            catch (Exception ex)
            {
                Log.Error($"Incoming311 error: {ex.Message}");
            }
        }

        private void Outgoing311(Command command)
        {
            try
            {
                // Makes just /311 give cops an alert
                if (command.Args.Count < 1)
                {
                    // TODO: Refactor to not need this ugly hack
                    OutgoingAlertForPolice(new Command(""));
                    OutgoingAlertForEms(new Command(""));
                    BaseScript.TriggerEvent("Chat.Message", "311", "#6655EE", $"You sent an emergency services alert.");
                    return;
                }

                // Think some of these may require the player to be at the location, hence why they are fetched beforehand and packed in model
                string currentStreetName = CitizenFX.Core.World.GetStreetName(Game.PlayerPed.Position);
                string currentCrossingName = Location.GetCrossingName(Game.PlayerPed.Position);
                string localizedZone = World.GetZoneLocalizedName(Game.PlayerPed.Position);
                string call = Helpers.MsgPack.Serialize(new EmergencyCallModel { Position = Game.PlayerPed.Position.ToArray(), Street = currentStreetName, CrossingStreet = currentCrossingName, Zone = localizedZone, Message = command.Args.ToString(), /*SourceCharId = CurrentPlayer.CharacterData.CharID*/ });
                string dutyEvent = Helpers.MsgPack.Serialize(new TriggerEventForDutyModel(Duty.EMS, "Communication.Incoming311", call));
                BaseScript.TriggerServerEvent("TriggerEventForDuty", dutyEvent);
                dutyEvent = Helpers.MsgPack.Serialize(new TriggerEventForDutyModel(Duty.Police, "Communication.Incoming311", call));
                BaseScript.TriggerServerEvent("TriggerEventForDuty", dutyEvent);
                BaseScript.TriggerEvent("Chat.Message", "311", "#6655EE", $"{/*CurrentPlayer.CharacterData.FirstName*/""} {/*CurrentPlayer.CharacterData.LastName*/""}: {command.Args.ToString()}");
            }
            catch (Exception ex)
            {
                Log.Error($"Outgoing311 error: {ex.Message}");
            }
        }

        private void Incoming911(string serializedCall)
        {
            try
            {
                EmergencyCallModel call = Helpers.MsgPack.Deserialize<EmergencyCallModel>(serializedCall);
                BaseScript.TriggerEvent("Chat.Message", "911", "#6655EE", $"(( Call #{call.SourceCharId} )) {call.Message}");
                if (!emergencyCalls.ContainsKey(call.SourceCharId))
                    EmergencyCallerId(new Command($"/callerid {call.SourceCharId}")); // TODO: refactor so this ugly hack won't be used
                emergencyCalls[call.SourceCharId] = call;
                lastCallCharId = call.SourceCharId;
            }
            catch (Exception ex)
            {
                Log.Error($"Incoming311 error: {ex.Message}");
            }
        }

        private void Outgoing911(Command command)
        {
            try
            {
                Log.ToChat($"{Function.Call<int>(Hash.PLAYER_ID)} {Function.Call<int>(Hash.GET_PLAYER_SERVER_ID)} {CitizenFX.Core.Game.Player.Handle} {CitizenFX.Core.Game.Player.ServerId}");
                // Makes just /911 give cops an alert
                if (command.Args.Count < 1)
                {
                    // TODO: Refactor to not need this ugly hack
                    OutgoingAlertForPolice(new Command(""));
                    OutgoingAlertForEms(new Command(""));
                    BaseScript.TriggerEvent("Chat.Message", "911", "#6655EE", $"You sent an emergency services alert.");
                    return;
                }

                // Think some of these may require the player to be at the location, hence why they are fetched beforehand and packed in model
                string currentStreetName = CitizenFX.Core.World.GetStreetName(Game.PlayerPed.Position);
                string currentCrossingName = Location.GetCrossingName(Game.PlayerPed.Position);
                string localizedZone = World.GetZoneLocalizedName(Game.PlayerPed.Position);
                string call = Helpers.MsgPack.Serialize(new EmergencyCallModel { Position = Game.PlayerPed.Position.ToArray(), Street = currentStreetName, CrossingStreet = currentCrossingName, Zone = localizedZone, Message = command.Args.ToString(), /*SourceCharId = CurrentPlayer.CharacterData.CharID*/ });
                string dutyEvent = Helpers.MsgPack.Serialize(new TriggerEventForDutyModel(Duty.EMS, "Communication.Incoming911", call));
                BaseScript.TriggerServerEvent("TriggerEventForDuty", dutyEvent);
                dutyEvent = Helpers.MsgPack.Serialize(new TriggerEventForDutyModel(Duty.Police, "Communication.Incoming911", call));
                BaseScript.TriggerServerEvent("TriggerEventForDuty", dutyEvent);
                BaseScript.TriggerEvent("Chat.Message", "911", "#6655EE", $"{/*CurrentPlayer.CharacterData.FirstName*/""} {/*CurrentPlayer.CharacterData.LastName*/""}: {command.Args.ToString()}");
            }
            catch (Exception ex)
            {
                Log.Error($"Outgoing911 error: {ex.Message}");
            }
        }

        private void PrivateMessage(Command command)
        {
            try
            {
                TriggerEventForPlayersModel data = new TriggerEventForPlayersModel(command.Args.GetInt32(0), "Communication.PrivateMessage", command.Args.ToString().Substring(command.Args.Get(0).Length), true);
                Log.ToChat(Game.Player.ServerId.ToString());
                var eventData = Helpers.MsgPack.Serialize(data);
                BaseScript.TriggerServerEvent("TriggerEventForPlayers", eventData);
            }
            catch (Exception ex)
            {
                Log.Error($"PrivateMessage error: {ex.Message}");
            }
        }


        // Placeholder
        
        private void SendFromCopsToEms(Command command)
        {
            try
            {
                TriggerEventForDutyModel data = new TriggerEventForDutyModel(Duty.EMS, "Communication.CopsToEms", $"{command.Args.ToString()}", true);
                var eventData = Helpers.MsgPack.Serialize(data);
                BaseScript.TriggerServerEvent("TriggerEventForDuty", eventData);
            }
            catch (Exception ex)
            {
                Log.Error($"SendFromCopsToEms error: {ex.Message}");
            }
        }

        // Placeholder
        private void ReceiveFromCopsToEms(string message)
        {
            try
            {
                BaseScript.TriggerEvent("Chat.Message", "COPS to EMS", "#7667AA", message);
            }
            catch (Exception ex)
            {
                Log.Error($"ReceiveFromCopsToEms error: {ex.Message}");
            }
        }

        // Placeholder
        
        private void SendFromEmsToCops(Command command)
        {
            try
            {
                TriggerEventForDutyModel data = new TriggerEventForDutyModel(Duty.EMS, "Communication.EmsToCops", $"{command.Args.ToString()}", true);
                var eventData = Helpers.MsgPack.Serialize(data);
                BaseScript.TriggerServerEvent("TriggerEventForDuty", eventData);
            }
            catch (Exception ex)
            {
                Log.Error($"SendFromEmsToCops error: {ex.Message}");
            }
        }

        // Placeholder
        
        private void ReceiveFromEmsToCops(string message)
        {
            try
            {
                BaseScript.TriggerEvent("Chat.Message", "COPS to EMS", "#7667AA", message);
            }
            catch (Exception ex)
            {
                Log.Error($"ReceiveFromEmsToCops error: {ex.Message}");
            }
        }

        private void SendSlashMe(Command command)
        {
			try {
				const float slashMeAoe = 25f;
				string message = command.Args.ToString();
				if( message[message.Length - 1] != '.' ) message += ".";
				PointEvent pointEvent = new PointEvent( "Communication.SlashMe", CitizenFX.Core.Game.PlayerPed.Position.ToArray(), slashMeAoe, message, Game.Player.ServerId, false );
				BaseScript.TriggerServerEvent( "TriggerEventNearPoint", Helpers.MsgPack.Serialize( pointEvent ) );
			}
			catch( Exception ex ) {
				Log.Error( $"ClientCommands SendSlashMe Error: {ex.Message}" );
			}
		}

        private Task ReceivedSlashMe(PointEvent pointEvent)
        {
            Log.Verbose($"SlashMe Triggered");
            BaseScript.TriggerEvent("Chat.Message", "", "#FFD23F", $"{new PlayerList()[pointEvent.SourceServerId].Name /* TODO: Replace with character name */} {pointEvent.SerializedArguments}");
            return Task.FromResult(0);
        }

        override protected void OnCommandNotFound(Command command)
        {
            try
            {
                Log.Info($"[CHAT] Command '{command.CommandString}' not found on client, passing to server");
                BaseScript.TriggerServerEvent("Chat.CommandEntered", command.CommandString);
            }
            catch (Exception ex)
            {
                Log.Error($"ClientCommands OnCommandNotFound Error: {ex.Message}");
            }
        }
    }
}