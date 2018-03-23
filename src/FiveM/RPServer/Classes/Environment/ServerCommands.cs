using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using MySql.Data.MySqlClient;
using FamilyRP.Roleplay.SharedClasses;

namespace FamilyRP.Roleplay.Server
{
	class ServerCommands : CommandProcessor
	{
		public ServerCommands() {
            Server.GetInstance().RegisterEventHandler("Chat.CommandEntered", new Action<Player, string>(Handle));
            
            Register("/ad", Advert);
            Register("/tweet", Tweet);
            Register("/ooc", GlobalOOC);
        }

        public void Advert(Command command)
        {
            bool hasEnoughMoney = true;
            if(hasEnoughMoney)
            {
                //deductMoney(advertCost);
                BaseScript.TriggerClientEvent("Chat.Message", "AD", "#000099", $"{command.Args.ToString()}");
            }
            else
            {
                BaseScript.TriggerClientEvent(command.Source, "Chat.Message", "BANK", "#995533", "You have insufficient funds to post an advert.");
            }
        }

        public void Tweet(Command command)
        {
            BaseScript.TriggerClientEvent("Chat.Message", "TWEET", "#000099",  $"[^3{command.Source.Name}^0] {command.Args.ToString()}");
        }

        public void GlobalOOC(Command command)
        {
            BaseScript.TriggerClientEvent("Chat.Message", "OOC", "#FFFF00", $"[^3{command.Source.Name}^0] {command.Args.ToString()}");
        }
        
        protected override void OnCommandNotFound(Command command)
        {
            Log.Verbose($"ServerCommands CommandNotFound");
            // TODO: Discuss if we want this visible to all or devs only
            BaseScript.TriggerClientEvent(command.Source, "Chat.Message", "SERVER", "#9931EE", $"Command '{command.Name}' not found on the server");
        }
    }
}