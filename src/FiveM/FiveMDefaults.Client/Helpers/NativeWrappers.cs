using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Client.Helpers.NativeWrappers
{
    public static class NativeWrappers
    {
        public static bool NetworkIsSessionStarted()
        {
            return Function.Call<bool>(Hash.NETWORK_IS_SESSION_STARTED, new InputArgument[0]);
        }

        static public void SendNuiMessage(string message)
        {
            Function.Call(Hash.SEND_NUI_MESSAGE, message);
        }

        public static bool NetworkIsPlayerActive(int playerId)
        {
            return Function.Call<bool>(Hash.NETWORK_IS_PLAYER_ACTIVE, playerId);
        }

        public static string GetPlayerFromServerId(int playerId)
        {
            return Function.Call<string>(Hash.GET_PLAYER_FROM_SERVER_ID, playerId);
        }

        public static string GetPlayerName(string netId)
        {
            return Function.Call<string>(Hash.GET_PLAYER_NAME, netId);
        }

        public static int GetGameTimer()
        {
            return Function.Call<int>(Hash.GET_GAME_TIMER);
        }

        public static int PlayerId()
        {
            return Function.Call<int>(Hash.PLAYER_ID);
        }

        public static bool IsPedFatallyInjured(int ped)
        {
            return Function.Call<bool>(Hash.IS_PED_FATALLY_INJURED, ped);
        }

        public static void ClearPedTasksImmediately(int ped)
        {
            Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, ped);
        }

        public static void RequestCollisionAtCoord(Vector3 location)
        {
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, location.X, location.Y, location.Z);
        }

        public static void LoadScene(Vector3 location)
        {
            Function.Call(Hash.LOAD_SCENE, location.X, location.Y, location.Z);
        }

        public static void NetworkResurrectLocalPlayer(Vector3 location, float heading)
        {
            Function.Call(Hash.NETWORK_RESURRECT_LOCAL_PLAYER, location.X, location.Y, location.Z, heading, true, true, false);
        }

        public static void SetEntityCoordsNoOffset(int handle, Vector3 location)
        { 
            Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, handle, location.X, location.Y, location.Z, false, false, false, true);
        }

        public static void ClearPlayerWantedLevel(int playerId)
        {
            Function.Call(Hash.CLEAR_PLAYER_WANTED_LEVEL, playerId);
        }

        public static void ShutdownLoadingScreen()
        {
            Function.Call(Hash.SHUTDOWN_LOADING_SCREEN, new InputArgument[0]);
        }

        public static bool HasCollisionLoadedAroundEntity(int ped)
        {
            return Function.Call<bool>(Hash.HAS_COLLISION_LOADED_AROUND_ENTITY, ped);
        }
    }
}
