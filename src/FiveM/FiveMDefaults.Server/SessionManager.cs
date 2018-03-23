using CitizenFX.Core;
using CitizenFX.Core.Native;
using Server.Helpers.NativeWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveM.Server
{
    public class SessionManager : BaseScript
    {
        string currentlyHosting = null;
        List<Action> hostReleaseCallbacks = new List<Action>();

        public SessionManager()
        {
            RegisterEventHandler("hostingSession", new Action<Player>(HostingSession));
            RegisterEventHandler("hostedSession", new Action<Player>(HostedSession));

            NativeWrappers.EnableEnhancedHostSupport(true);
            Debug.WriteLine("SessionManager initialized");
        }

        public void HostingSession([FromSource] Player source)
        {
            if (!String.IsNullOrEmpty(currentlyHosting))
            {
                TriggerClientEvent(source, "sessionHostResult", "wait");
                hostReleaseCallbacks.Add(() => TriggerClientEvent(source, "sessionHostResult", "free"));
            }

            string hostId;
            // (If no host exists yet null exception is thrown)
            try
            {
                hostId = NativeWrappers.GetHostId();
            }
            catch (NullReferenceException)
            {
                hostId = null;
            }

            if (!String.IsNullOrEmpty(hostId) && NativeWrappers.GetPlayerLastMsg(hostId) < 1000)
            {
                TriggerClientEvent(source, "sessionHostResult", "conflict");
                return;
            }

            hostReleaseCallbacks.Clear();
            currentlyHosting = source.Handle;
            Debug.WriteLine($"[SESSIONMANAGER] current game host is '{currentlyHosting}'");

            TriggerClientEvent(source, "sessionHostResult", "go");

            BaseScript.Delay(5000);
            currentlyHosting = null;
            hostReleaseCallbacks.ForEach(f => f());
        }

        public void HostedSession([FromSource] Player source)
        {
            if (currentlyHosting != source.Handle && !String.IsNullOrEmpty(currentlyHosting))
            {
                Debug.WriteLine($@"Player client ""lying"" about being the host: current host '#{currentlyHosting}' != client '#{source.Handle}'");
                return;
            }

            hostReleaseCallbacks.ForEach(f => f());

            currentlyHosting = null;

            return;
        }

        public void RegisterEventHandler(string name, Delegate trigger)
        {
            EventHandlers[name] += trigger;
        }
    }
}
