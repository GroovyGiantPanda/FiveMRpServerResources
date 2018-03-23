using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Helpers;
using FamilyRP.Roleplay.SharedClasses;
using CitizenFX.Core.UI;
using System.Drawing;

namespace FamilyRP.Roleplay.Client.Classes.Environment.UI
{
    // NETWORK_IS_PLAYER_TALKING looks like it may be enough to detect players talking
    // The server that shall not be named used to send N keypresses and key releases to absolutely every player constantly
    // No matter the distance
    static class PlayerOverheadMarkers
    {
        static internal IEnumerable<CitizenFX.Core.Player> MarkerPlayers;
        static internal float MarkerDistance = 25;
        static internal float MarkerVehicleDistance = 50;
        static internal Vector3 MarkerOffset = new Vector3(0, 0, -0.96f);  // For circle
        //static internal Vector3 MarkerOffset = new Vector3(0, 0, 1.1f);
        static internal Vector3 MarkerVehicleOffset = new Vector3(0, 0, 2.0f);
        static internal Vector3 OverheadIdTextOffset = new Vector3(0, 0, 0.7f);
        static System.Drawing.Color MarkerColor;
        //internal System.Drawing.Color MarkerColorNotTalking = System.Drawing.Color.FromArgb(80, 12, 202, 74); // Lily (40, 95, 255, 71);
        static internal System.Drawing.Color MarkerColorNotTalking = System.Drawing.Color.FromArgb(100, 255, 255, 0); // Lily (40, 95, 255, 71);
        static internal System.Drawing.Color MarkerColorTalking = System.Drawing.Color.FromArgb(255, 0, 167, 225); // Lily (40, 71, 108, 255);
        static Vector3 MarkerDirection = new Vector3(0, 0, 0);
        static Vector3 MarkerRotation = new Vector3(180f, 0f, 0f);
        static Vector3 MarkerScale = 0.8f * Vector3.One; // For circle
        //static Vector3 MarkerScale =  0.3f * Vector3.One;
        static Vector3 MarkerVehicleScale = 0.4f * Vector3.One;
        static Dictionary<int, Scaleform> scaleformHandles = new Dictionary<int, Scaleform>();

        static MarkerType MarkerType = MarkerType.HorizontalCircleSkinny;
        //static MarkerType MarkerType = MarkerType.ThickChevronUp;
        static MarkerType MarkerVehicleType = MarkerType.ThickChevronUp;

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(DrawMarkers);
            Client.GetInstance().RegisterTickHandler(DrawCurrentlyTalking);
        }

        static internal bool ShouldShowMarker(CitizenFX.Core.Player player)
        {
            bool isCloseEnough;
            // TODO: Take into account VOIP range
            // TODO: increase visibility distance if other ped is in vehicle
            if (player.Character.IsInVehicle())
            {
                isCloseEnough = Math.Sqrt(player.Character.Position.DistanceToSquared(Game.PlayerPed.Position)) < MarkerDistance;
            }
            else
            {
                isCloseEnough = Math.Sqrt(player.Character.Position.DistanceToSquared(Game.PlayerPed.Position)) < MarkerVehicleDistance;
            }
            //Debug.WriteLine($"[MarkerS] Distance to player '{player.Name}': {Math.Sqrt(player.Character.Position.DistanceToSquared(Game.PlayerPed.Position))}");
            bool isSneaking = player.Character.IsInStealthMode || player.Character.IsInCover() || Function.Call<bool>(Hash.IS_PED_USING_SCENARIO, player.Character.Handle, "WORLD_HUMAN_SMOKING") /*|| (player.Character.IsInVehicle() && player.Character.CurrentVehicle.Speed < 3.0)*/;
            bool isCurrentPlayer = (Game.Player == player);
            if (isCloseEnough && !isSneaking && !isCurrentPlayer)
                return true;
            return false;
        }

        static internal async Task DrawCurrentlyTalking()
        {
            try
            {
                if (CinematicMode.DoHideHud) return;
                var TalkingPlayers = new PlayerList().Where(p => (Function.Call<bool>(Hash.NETWORK_IS_PLAYER_TALKING, p.Handle) || (p == Game.Player && ControlHelper.IsControlPressed(Control.PushToTalk, true, FamilyRP.Roleplay.Enums.Controls.ControlModifier.Any))) && p.Character.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(Voip.currentRange.Key, 2)).ToList();
                string currentlyTalking = $"{(ControlHelper.IsControlPressed(Control.PushToTalk, true, FamilyRP.Roleplay.Enums.Controls.ControlModifier.Any) ? $"{Game.Player.Name}\n" : "")}{String.Join("\n", TalkingPlayers.Select(p => $"{p.Name}"))}";

                if (currentlyTalking.Length > 0)
                {
                    FamilyRP.Roleplay.Client.UI.DrawText($"Currently Talking", new Vector2(0.5f, 0.02f), Color.FromArgb(255, 106, 76, 147), 0.3f, 0, Alignment.Center);
                    FamilyRP.Roleplay.Client.UI.DrawText(currentlyTalking, new Vector2(0.5f, 0.043f), Color.FromArgb(255, 255, 255, 255), 0.26f, 0, Alignment.Center);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR DrawMarkers: {ex.Message}");
            }
            await Task.FromResult(0);
        }

        static internal async Task DrawMarkers()
        {
            try
            {
                if (CinematicMode.DoHideHud) return;
                MarkerPlayers = new PlayerList().Where(ShouldShowMarker);
                List<CitizenFX.Core.Player> playerList = MarkerPlayers.ToList();
                //Debug.WriteLine($"[MarkerS] Number of players to draw: {playerList.Count()}");
                playerList.OrderBy(p => p.Character.Position.DistanceToSquared(Game.PlayerPed.Position)).Select((player, rank) => new { player, rank }).ToList().ForEach(async p => await DrawMarker(p.player, p.rank));
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR DrawMarkers: {ex.Message}");
            }
            await Task.FromResult(0);
        }

        static async Task DrawMarker(CitizenFX.Core.Player player, int distanceRank)
        {
            Vector3 CurrentMarkerPosition;
            MarkerType CurrentMarkerType;
            Vector3 CurrentMarkerScale;
            if ((Function.Call<bool>(Hash.NETWORK_IS_PLAYER_TALKING, player) || (player == Game.Player && ControlHelper.IsControlPressed(Control.PushToTalk, true, FamilyRP.Roleplay.Enums.Controls.ControlModifier.Any))))
                MarkerColor = MarkerColorTalking;
            else
                MarkerColor = MarkerColorNotTalking;

            if(player.Character.IsInVehicle())
            { 
                if(Function.Call<int>(Hash.GET_VEHICLE_NUMBER_OF_PASSENGERS, player.Character.CurrentVehicle.Handle) + (Function.Call<bool>(Hash.IS_VEHICLE_SEAT_FREE, player.Character.CurrentVehicle.Handle, -1) ? 0 : 1) > 1)
                { 
                    var talkingPlayersInVehicle = new PlayerList().Where(p => Function.Call<bool>(Hash.NETWORK_IS_PLAYER_TALKING, player) && p.Character.IsInVehicle() && p.Character.CurrentVehicle.Handle == player.Character.CurrentVehicle.Handle);
                    if (talkingPlayersInVehicle.Count() > 0)
                    { 
                        MarkerColor = MarkerColorTalking;
                    }
                    else
                    {
                        MarkerColor = MarkerColorNotTalking;
                    }
                }

                // Base marker altitude on vehicle height
                OutputArgument max = new OutputArgument();
                OutputArgument min = new OutputArgument();
                int modelHash = Function.Call<int>(Hash.GET_ENTITY_MODEL, player.Character.CurrentVehicle.Handle);
                Function.Call(Hash.GET_MODEL_DIMENSIONS, modelHash, max, min);
                Vector3 dim = max.GetResult<Vector3>() - min.GetResult<Vector3>();
                CurrentMarkerPosition = player.Character.Position + MarkerVehicleOffset + new Vector3(0f, 0f, Math.Abs(dim.Z)-1.8f);
                CurrentMarkerType = MarkerVehicleType;
                CurrentMarkerScale = MarkerVehicleScale;
            }
            else
            {
                CurrentMarkerPosition = player.Character.Position + MarkerOffset;
                CurrentMarkerType = MarkerType;
                CurrentMarkerScale = MarkerScale;
            }
            if(ControlHelper.IsControlPressed(Control.CharacterWheel, true, Enums.Controls.ControlModifier.Any) && distanceRank.IsBetween(0, 10))
            {
                if (!scaleformHandles.ContainsKey(player.Handle)) scaleformHandles[player.Handle] = new Scaleform($"mp_freemode_checkpoint_{(player.ServerId.IsBetween(0, 10) ? player.ServerId : 0):00}"); // server ID for players on the server should never be outside this span, but...
                if (scaleformHandles[player.Handle].IsLoaded)
                {
                    scaleformHandles[player.Handle].CallFunction("SET_CHECKPOINT_TEXT", player.ServerId.ToString());
                    scaleformHandles[player.Handle].Render3D(player.Character.Position + OverheadIdTextOffset, Function.Call<Vector3>(Hash._GET_GAMEPLAY_CAM_ROT, 2), 1f * Vector3.One);
                }
            }
            else
            {
                World.DrawMarker(CurrentMarkerType, CurrentMarkerPosition, MarkerDirection, MarkerRotation, CurrentMarkerScale, MarkerColor, false, true);
            }
            await Task.FromResult(0);
        }
    }
}
