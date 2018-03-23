using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.SharedClasses;
using static FamilyRP.Roleplay.Client.Helpers.WorldProbe;
using FamilyRP.Roleplay.Server.Enums;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;

namespace FamilyRP.Roleplay.Client {
	public static class UI
    {
        private static bool overlayActive = false;
        // To temporarily disable raycasts as it does not like raycasts while entity position is being changed
        // With e.g. devtools
        public static bool disableEntityUI = false; 
        private static string[] ScreenEffects = { "SwitchHUDIn", "SwitchHUDOut", "FocusIn", "FocusOut", "MinigameEndNeutral", "MinigameEndTrevor", "MinigameEndFranklin", "MinigameEndMichael", "MinigameTransitionOut", "MinigameTransitionIn", "SwitchShortNeutralIn", "SwitchShortFranklinIn", "SwitchShortTrevorIn", "SwitchShortMichaelIn", "SwitchOpenMichaelIn", "SwitchOpenFranklinIn", "SwitchOpenTrevorIn", "SwitchHUDMichaelOut", "SwitchHUDFranklinOut", "SwitchHUDTrevorOut", "SwitchShortFranklinMid", "SwitchShortMichaelMid", "SwitchShortTrevorMid", "DeathFailOut", "CamPushInNeutral", "CamPushInFranklin", "CamPushInMichael", "CamPushInTrevor", "SwitchOpenMichaelIn", "SwitchSceneFranklin", "SwitchSceneTrevor", "SwitchSceneMichael", "SwitchSceneNeutral", "MP_Celeb_Win", "MP_Celeb_Win_Out", "MP_Celeb_Lose", "MP_Celeb_Lose_Out", "DeathFailNeutralIn", "DeathFailMPDark", "DeathFailMPIn", "MP_Celeb_Preload_Fade", "PeyoteEndOut", "PeyoteEndIn", "PeyoteIn", "PeyoteOut", "MP_race_crash", "SuccessFranklin", "SuccessTrevor", "SuccessMichael", "DrugsMichaelAliensFightIn", "DrugsMichaelAliensFight", "DrugsMichaelAliensFightOut", "DrugsTrevorClownsFightIn", "DrugsTrevorClownsFight", "DrugsTrevorClownsFightOut", "HeistCelebPass", "HeistCelebPassBW", "HeistCelebEnd", "HeistCelebToast", "MenuMGHeistIn", "MenuMGTournamentIn", "MenuMGSelectionIn", "ChopVision", "DMT_flight_intro", "DMT_flight", "DrugsDrivingIn", "DrugsDrivingOut", "SwitchOpenNeutralFIB5", "HeistLocate", "MP_job_load", "RaceTurbo", "MP_intro_logo", "HeistTripSkipFade", "MenuMGHeistOut", "MP_corona_switch", "MenuMGSelectionTint", "SuccessNeutral", "ExplosionJosh3", "SniperOverlay", "RampageOut", "Rampage", "Dont_tazeme_bro", "DeathFailOut" };
		private static int CurrentScreenEffect = 0;
        
        public static void DrawText(string text, int font, bool center, float x, float y, float scale, byte r, byte g, byte b, byte a)
        {
            Function.Call(Hash.SET_TEXT_FONT, font);
            Function.Call(Hash.SET_TEXT_PROPORTIONAL, 0);
            Function.Call(Hash.SET_TEXT_SCALE, scale, scale);
            Function.Call(Hash.SET_TEXT_COLOUR, r, g, b, a);
            Function.Call(Hash.SET_TEXT_DROP_SHADOW, 0, 0, 0, 255);
            Function.Call(Hash.SET_TEXT_EDGE, 1, 0, 0, 255);
            Function.Call(Hash.SET_TEXT_OUTLINE);
            Function.Call(Hash.SET_TEXT_CENTRE, center);
            Function.Call(Hash._SET_TEXT_ENTRY, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
            Function.Call(Hash._DRAW_TEXT, x, y);
        }

        public static void DrawText(string text, Vector2 position, System.Drawing.Color color, float scale, Font font, Alignment alignment)
        {
            Function.Call(Hash.SET_TEXT_FONT, font);
            Function.Call(Hash.SET_TEXT_PROPORTIONAL, 0);
            Function.Call(Hash.SET_TEXT_SCALE, scale, scale);
            Function.Call(Hash.SET_TEXT_COLOUR, color.R, color.G, color.B, color.A);
            Function.Call(Hash.SET_TEXT_DROP_SHADOW, 0, 0, 0, 255);
            Function.Call(Hash.SET_TEXT_EDGE, 1, 0, 0, 255);
            Function.Call(Hash.SET_TEXT_OUTLINE);
            //Function.Call(Hash.SET_TEXT_CENTRE, center);
            Function.Call(Hash.SET_TEXT_JUSTIFICATION, alignment);
            Function.Call(Hash._SET_TEXT_ENTRY, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
            Function.Call(Hash._DRAW_TEXT, position.X, position.Y);
        }

        public static void DrawText(string text, Vector2 position, System.Drawing.Color color, float scale = 0.25f, Font font = Font.ChaletLondon, bool center = false)
        {
            DrawText(text, position, color, scale, font, center ? Alignment.Center : Alignment.Left);
        }
        
        public static void Render()
        {
            if (CinematicMode.DoHideHud) return;

            float row = 0.027f;

            if (DevCommands.IsDevEntityUIEnabled && !disableEntityUI)
            {
                row += 0.04f;

                _RaycastResult raycast = WorldProbe.CrosshairRaycast();
                if (!ReferenceEquals(raycast, null) && !ReferenceEquals(raycast.DitHit, null) && raycast.DitHit)
                {
                    World.DrawLine(CitizenFX.Core.GameplayCamera.Position, raycast.HitPosition, System.Drawing.Color.FromArgb(255, 0, 0));
                    World.DrawLine(raycast.HitPosition, raycast.HitPosition + 1000 * raycast.SurfaceNormal, System.Drawing.Color.FromArgb(0, 255, 0));
                    DrawText($"Pos: {String.Join(" ", raycast.HitPosition.ToArray().Select(f => $"{f,6:0.000}"))}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    row += 0.02f;
                    DrawText($"Normal: {String.Join(" ", raycast.SurfaceNormal.ToArray().Select(f => $"{f,6:0.000}")).ToString()}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                }
                else
                {
                    DrawText($"Hit: false", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    World.DrawLine(CitizenFX.Core.GameplayCamera.Position, CitizenFX.Core.GameplayCamera.Position + 1000 * WorldProbe.GameplayCamForwardVector(), System.Drawing.Color.FromArgb(255, 0, 0));
                }
                row += 0.02f;
                if (!ReferenceEquals(raycast, null) && !ReferenceEquals(raycast.DitHitEntity, null) && !ReferenceEquals(raycast.HitEntity, null) && !ReferenceEquals(raycast.HitEntity.Handle, null) && raycast.DitHitEntity)
                {
                    DrawText($"Ent ID: {raycast.HitEntity.Handle}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    string type = WorldProbe.GetEntityType(raycast.HitEntity.Handle);
                    DrawText($"Type: {type}", 0, false, 0.84f, row, 0.25f, 255, 255, 255, 255);
                    DrawText($"MISSION: {Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, raycast.HitEntity.Handle)}", 0, false, 0.89f, row, 0.25f, 255, 255, 255, 255);
                    DrawText($"isNet: {Function.Call<bool>(Hash.NETWORK_GET_ENTITY_IS_NETWORKED, raycast.HitEntity.Handle)}", 0, false, 0.95f, row, 0.25f, 255, 255, 255, 255);
                    row += 0.02f;

                    DrawText($"hasNETID: {Function.Call<bool>(Hash.NETWORK_GET_NETWORK_ID_FROM_ENTITY, raycast.HitEntity.Handle)}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    DrawText($"NETID: {Function.Call<int>(Hash.NETWORK_GET_NETWORK_ID_FROM_ENTITY, raycast.HitEntity.Handle)}", 0, false, 0.84f, row, 0.25f, 255, 255, 255, 255);
                    DrawText($"EntCtr: {Function.Call<bool>(Hash.NETWORK_HAS_CONTROL_OF_ENTITY, raycast.HitEntity.Handle)}", 0, false, 0.90f, row, 0.25f, 255, 255, 255, 255);
                    DrawText($"isLoc: {Function.Call<bool>(Hash.NETWORK_GET_ENTITY_IS_LOCAL, raycast.HitEntity.Handle)}", 0, false, 0.95f, row, 0.25f, 255, 255, 255, 255);
                    row += 0.02f;
                    DrawText($"NETID to ENTID: {Function.Call<int>(Hash.NETWORK_GET_ENTITY_FROM_NETWORK_ID, Function.Call<int>(Hash.NETWORK_GET_NETWORK_ID_FROM_ENTITY, raycast.HitEntity.Handle))} (Broken)", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    DrawText($"NETIDCtr: {Function.Call<bool>(Hash.NETWORK_HAS_CONTROL_OF_NETWORK_ID, Function.Call<int>(Hash.NETWORK_GET_NETWORK_ID_FROM_ENTITY, raycast.HitEntity.Handle))}", 0, false, 0.88f, row, 0.25f, 255, 255, 255, 255);
                    row += 0.02f;

                    bool decorTestExists = Function.Call<bool>(Hash.DECOR_EXIST_ON, raycast.HitEntity.Handle, "TEST");
                    DrawText($"Decor TEST exists: {decorTestExists}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    if(decorTestExists)
                        DrawText($"TEST: {Function.Call<int>(Hash.DECOR_GET_INT, raycast.HitEntity.Handle, "TEST")}", 0, false, 0.90f, row, 0.25f, 255, 255, 255, 255);
                    row += 0.02f;

                    Vector3 coords = Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, raycast.HitEntity.Handle);
                    DrawText($"x: {coords.X:0.000}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    DrawText($"y: {coords.Y:0.000}", 0, false, 0.85f, row, 0.25f, 255, 255, 255, 255);
                    DrawText($"z: {coords.Z:0.000}", 0, false, 0.90f, row, 0.25f, 255, 255, 255, 255);
                    DrawText($"h: {Function.Call<float>(Hash.GET_ENTITY_HEADING, raycast.HitEntity.Handle):0.000}", 0, false, 0.95f, row, 0.25f, 255, 255, 255, 255);
                    row += 0.02f;

                    Vector3 rot = Function.Call<Vector3>(Hash.GET_ENTITY_ROTATION, raycast.HitEntity.Handle, 2);
                    DrawText($"Rot x: {rot.X:0.000}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    DrawText($"y: {rot.Y:0.000}", 0, false, 0.85f, row, 0.25f, 255, 255, 255, 255);
                    DrawText($"z: {rot.Z:0.000}", 0, false, 0.90f, row, 0.25f, 255, 255, 255, 255);
                    row += 0.02f;

                    OutputArgument max = new OutputArgument();
                    OutputArgument min = new OutputArgument();
                    int modelHash = Function.Call<int>(Hash.GET_ENTITY_MODEL, raycast.HitEntity.Handle);
                    Function.Call(Hash.GET_MODEL_DIMENSIONS, modelHash, max, min);
                    Vector3 dim = max.GetResult<Vector3>() - min.GetResult<Vector3>();
                    DrawText($"x: {dim.X:0.000}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    DrawText($"y: {dim.Y:0.000}", 0, false, 0.85f, row, 0.25f, 255, 255, 255, 255);
                    DrawText($"z: {dim.Z:0.000}", 0, false, 0.90f, row, 0.25f, 255, 255, 255, 255);
                    row += 0.02f;

                    DrawText($"Model hash (int): {Function.Call<int>(Hash.GET_ENTITY_MODEL, raycast.HitEntity.Handle)}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    row += 0.02f;
                    DrawText($"Model hash (uint): {Function.Call<uint>(Hash.GET_ENTITY_MODEL, raycast.HitEntity.Handle)}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    row += 0.02f;
                    DrawText($"Model hash (hex): {Function.Call<uint>(Hash.GET_ENTITY_MODEL, raycast.HitEntity.Handle):X}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    row += 0.02f;
                    if (type == "VEH")
                    {
                        // This commented version is not going to work for custom vehicles
                        //DrawText($"Model name: {Enum.GetName(typeof(VehicleHash), Function.Call<uint>(Hash.GET_ENTITY_MODEL, raycast.HitEntity.Handle))}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                        //row += 0.02f;
                        //DrawText($"Class name: {Enum.GetName(typeof(VehicleClass), Function.Call<uint>(Hash.GET_VEHICLE_CLASS, raycast.HitEntity.Handle))}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255); 
                        DrawText($"Model name: {Game.GetGXTEntry(Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, raycast.HitEntity.Model))}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                        row += 0.02f;
                        DrawText($"Class name: {Game.GetGXTEntry(CitizenFX.Core.Vehicle.GetClassDisplayName((VehicleClass)Function.Call<int>(Hash.GET_VEHICLE_CLASS, raycast.HitEntity.Handle)))}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                        row += 0.02f; 
                         DrawText($"Plate: {Function.Call<string>(Hash.GET_VEHICLE_NUMBER_PLATE_TEXT, raycast.HitEntity.Handle)}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    }
                    else if (type == "PED")
                        DrawText($"Model name: {Enum.GetName(typeof(PedHash), Function.Call<uint>(Hash.GET_ENTITY_MODEL, raycast.HitEntity.Handle))}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    else if (type == "OBJ")
                        DrawText($"Model name: {Enum.GetName(typeof(ObjectHash), Function.Call<int>(Hash.GET_ENTITY_MODEL, raycast.HitEntity.Handle))}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                    row += 0.02f;

                    if (Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, raycast.HitEntity.Handle))
                    {
                        DrawText($"LOCKS: {Function.Call<int>(Hash.GET_VEHICLE_DOOR_LOCK_STATUS, raycast.HitEntity.Handle)}", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
                        DrawText($"LK4ME: {Function.Call<bool>(Hash.GET_VEHICLE_DOORS_LOCKED_FOR_PLAYER, raycast.HitEntity.Handle, CitizenFX.Core.Game.Player.Handle)}", 0, false, 0.85f, row, 0.25f, 255, 255, 255, 255);
                        DrawText($"SPED: {Function.Call<float>(Hash._GET_VEHICLE_MODEL_MAX_SPEED, Function.Call<uint>(Hash.GET_ENTITY_MODEL, raycast.HitEntity.Handle)):0.00}", 0, false, 0.90f, row, 0.25f, 255, 255, 255, 255);
                        DrawText($"ACCL: {Function.Call<float>(Hash.GET_VEHICLE_MODEL_ACCELERATION, Function.Call<uint>(Hash.GET_ENTITY_MODEL, raycast.HitEntity.Handle)):0.00}", 0, false, 0.95f, row, 0.25f, 255, 255, 255, 255);
                        row += 0.02f;
                    }
                }
            }
            else if (disableEntityUI)
            {
                DrawText($"Entity UI temporarily disabled while manipulating objects (native errors occur)", 0, false, 0.78f, row, 0.25f, 255, 255, 255, 255);
            }
            // TODO: Draw debug information for the developer when desired
        }
	}
}