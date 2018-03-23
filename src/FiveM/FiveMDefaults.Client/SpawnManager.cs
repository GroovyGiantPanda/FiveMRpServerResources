using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Client.Helpers.NativeWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveMDefaults.Client
{
    public class SpawnManager : BaseScript
    {
        // This won't matter for our purposes anyway since we handle /re/spawn ourselves
        Vector3 InitialSpawnCoords = new Vector3((float)-802.311, (float)175.056, (float)72.8446);
        bool hasSpawned = false;
        Model defaultModel = PedHash.Airhostess01SFY;

        public SpawnManager()
        {
            Tick += SpawnCheck;
            Exports.Add("hasSpawned", new Func<bool>(() => hasSpawned));
        }

        public void RegisterEventHandler(string trigger, Delegate callback)
        {
            EventHandlers[trigger] += callback;
        }

        internal async Task SpawnCheck()
        {
            bool playerPedExists = (Game.PlayerPed.Handle != 0);
            bool playerActive = NativeWrappers.NetworkIsPlayerActive(NativeWrappers.PlayerId());

            if (playerPedExists && playerActive && !hasSpawned)
            {
                SpawnPlayer(defaultModel, InitialSpawnCoords, (float)0.0);
                hasSpawned = true;
            }
            await Task.FromResult(0);
        }

        internal async void SpawnPlayer(Model model, Vector3 location, float heading)
        {
            // FiveM used this on C++ side, so this might be needed (GTA might fade out on load themselves)
            Screen.Fading.FadeIn(0);
            NativeWrappers.RequestCollisionAtCoord(location);
            await Game.Player.ChangeModel(model);
            Game.PlayerPed.Position = location;
            Game.PlayerPed.Heading = heading;
            NativeWrappers.ShutdownLoadingScreen();
            TriggerEvent("playerSpawned");
        }
    }
}
