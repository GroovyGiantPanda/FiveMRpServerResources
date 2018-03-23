using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CitizenFX.Core;
using FamilyRP.Roleplay.SharedClasses;

namespace FamilyRP.Roleplay.Server
{
    public class Server : BaseScript
    {
        private static Server _server;

        public static Server GetInstance()
        {
            return _server;
        }

		private HashSet<CommandProcessor> CommandHandlers;

        public Server()
        {
            Debug.WriteLine("Entering Server ctor"); // Log not yet ready here

            _server = this;
            
			Config.Init();
            Log.Init();

            // Initialize World Details
            ClassLoader.Init();
            Log.Verbose("Leaving Server ctor");
        }

        public void RegisterEventHandler(string name, Delegate action)
        {
            EventHandlers[name] += action;
        }
    }
}