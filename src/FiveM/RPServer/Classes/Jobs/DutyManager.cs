using CitizenFX.Core;
using FamilyRP.Roleplay.Enums.Character;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Server.Classes.Jobs
{
    static class DutyManager
    {
        static int CleanupInterval = 60000;

        /// <summary>
        /// Contains those currently on duty, by character ID
        /// </summary>
        public static Dictionary<int, Duty> duty = new Dictionary<int, Duty>();

        static public void Init()
        {
            Log.Debug("Enter DutyManager Init");
            Server.ActiveInstance.RegisterEventHandler("Duty.OutgoingToggle", new Action<Player, int, bool>(ToggleDutyHandler));
            Server.ActiveInstance.RegisterEventHandler("Duty.UpdateVehicleBlip", new Action<Player>(HandleBlipUpdate));
            Server.ActiveInstance.RegisterEventHandler("Duty.InitialRequest", new Action<Player>(InitialRequestHandler));
            RegularDutyCleanup();
            Log.Debug("Leave DutyManager Init");
        }

        private static void HandleBlipUpdate([FromSource] Player source)
        {
            try
            {
                BaseScript.TriggerClientEvent("Duty.UpdateVehicleBlip", SessionManager.SessionList[source.Handle].Character.CharID);

                //BaseScript.TriggerClientEvent("Duty.UpdateVehicleBlip", 1);
            }
            catch (Exception ex)
            {
                Log.Error($"DutyManager HandleBlipUpdate Error: {ex.Message}");
            }
        }

        private static async void RegularDutyCleanup()
        {
            while(true)
            { 
                try
                {
                    duty.ToList().ForEach(d =>
                    {
                        if (/*false*/!SessionManager.PlayerList.ContainsKey(d.Key))
                        {
                            BaseScript.TriggerClientEvent("Duty.UpdateDuty", (int) d.Key, (int) d.Value, false);
                            duty.Remove(d.Key);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Log.Error($"DutyManager RegularDutyCleanup Error: {ex.Message}");
                }
                await BaseScript.Delay(CleanupInterval);
            }
        }

        private static void InitialRequestHandler([FromSource] Player source)
        {
            try
            {
                BaseScript.TriggerClientEvent(source.Handle, "Duty.InitialDuty", Helpers.MsgPack.Serialize(duty));
            }
            catch (Exception ex)
            {
                Log.Error($"DutyManager InitialRequestHandler Error: {ex.Message}");
            }
        }

        static private void ToggleDutyHandler([FromSource] Player source, int dutyToggle, bool toggle)
        {
            try
            {
                Debug.WriteLine("Incoming duty toggle");
                if(/*true*/SessionManager.SessionList.ContainsKey(source.Handle) && (SessionManager.SessionList[source.Handle].Character.Duty |= (Duty)dutyToggle) > 0)
                {
                    int charId = SessionManager.SessionList[source.Handle].Character.CharID;
                    //int charId = 1;

                    if (toggle)
                    {
                        duty[charId] = (Duty)dutyToggle;
                    }
                    else
                    {
                        duty.Remove(charId);
                    }
                    Log.Debug($"Sending duty update for char {charId}");
                    BaseScript.TriggerClientEvent("Duty.UpdateDuty", charId, dutyToggle, toggle);
                }
            }
            catch(Exception ex)
            {
                Log.Error($"DutyManager ToggleDutyHandler Error: {ex.Message}");
            }
        }
    }
}
