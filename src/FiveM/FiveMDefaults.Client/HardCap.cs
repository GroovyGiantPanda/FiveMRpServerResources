using CitizenFX.Core;
using Client.Helpers.NativeWrappers;
using System;
using System.Threading.Tasks;

namespace TheFamily.FiveMCSharp.Client
{
    public class HardCap : BaseScript
    {
        public HardCap()
        {
            BaseScript.Delay(1000);
            Debug.WriteLine("HARDCAP Resource constructor called");
            Tick += PlayerActivatedCheck;
        }

        public void RegisterEventHandler(string trigger, Delegate callback)
        {
            EventHandlers[trigger] += callback;
        }

        internal async Task PlayerActivatedCheck()
        {
            if (NativeWrappers.NetworkIsSessionStarted())
            {
                TriggerServerEvent("HardCap.PlayerActivated");
                Tick -= PlayerActivatedCheck;
            }
            await Task.FromResult(0);
        }
    }
}
