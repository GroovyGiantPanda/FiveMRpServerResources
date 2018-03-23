using FamilyRP.Roleplay.Client.Classes;
using FamilyRP.Roleplay.Client.Classes.Actions;
using FamilyRP.Roleplay.Client.Classes.Actions.Emotes;
using FamilyRP.Roleplay.Client.Classes.Actions.EMS;
using FamilyRP.Roleplay.Client.Classes.Actions.Jobs.Police;
using FamilyRP.Roleplay.Client.Classes.Environment;
using FamilyRP.Roleplay.Client.Classes.Environment.Controls;
using FamilyRP.Roleplay.Client.Classes.Environment.DevTools;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Roleplay.Client.Classes.Environment.UI.Police;
using FamilyRP.Roleplay.Client.Classes.Jobs;
using FamilyRP.Roleplay.Client.Classes.Jobs.Police;
using FamilyRP.Roleplay.Client.Classes.Jobs.Police.Helicopter;
using FamilyRP.Roleplay.Client.Classes.Jobs.Police.Vehicle;
using FamilyRP.Roleplay.Client.Classes.Jobs.Transportation.Trains;
using FamilyRP.Roleplay.Client.Classes.Player;
using FamilyRP.Roleplay.Client.Classes.Vehicle;
using FamilyRP.Roleplay.SharedClasses;

namespace FamilyRP.Roleplay.Client
{
    /// <summary>
    /// For initialization of all these static classes
    /// May want to split this into multiple more specific files later
    /// </summary>
    static class ClassLoader
    {
        public static void Init()
        {
            //Log.Verbose("Entering ClassLoader Init");

            // Emotes
            //EmotesManager.Init();
            //HandsUp.Init();
            //Pointing.Init();

            // UI
            //ControlCodeTester.Init();
            //Radar.Init();
            //ButtonInstructions.Init(); // To be tested
            //CinematicMode.Init();
            //HideReticle.Init();
            //Location.Init();
            //PlayerOverheadMarkers.Init();
            //PlayerOverheadText.Init(); // Not needed an longer; may be removed in later patch by me
            //Speedometer.Init();
            //VehicleLoadoutPresets.Init();
            //MenuGlobals.Init();
            //VehicleMenuTest.Init();
            //CharacterMenuTest.Init();
            //InteractionListMenu.Init();

            // Environment
            //Poi.Init();
            //AfkKick.Init();
            //ManipulateObject.Init();
            //NoClip.Init();
            //InstancingChecker.Init(); // Hard to test
            //MarkerHandler.Init();
            //Pvp.Init();
            //WarpPoints.Init();
            //Voip.Init();
            //AquaticSpawner.Init();
            //EmergencyServices.Init();

            // EMS
            //EMS.Init();

            // Police
   //         Sirens.Init();
   //         WeaponStash.Init();
   //         Arrest.Init(); 
   //         CivilianCarSirenLights.Init();
   //         CustomizationCommands.Init();
   //         Helicopter.Init();
   //         SkinLoadoutPresets.Init();
			//Slimjim.Init();
   //         SpikeStrip.Init();
            //CellDoors.Init();
            //Tackle.Init();
            //PoliceCharacterMenu.Init();
            //PoliceVehicleMenu.Init();
            //DutyManager.Init();

            // Jobs
            //TrainManager.Init();
            //Fishing.Init();
            //Hunting.Init();
   //         Garages.Init();

			// Player
   //         GunShotResidueManager.Init();
   //         PedDamage.Init();
   //         PrisonSentence.Init();
   //         DeathHandler.Init();
   //         WeaponUnholsterHandler.Init();

            // Vehicles
            //Vehicles.Init();
            //CarHud.Init();
            //BannedMilitaryVehicles.Init();
            //BrakeSignals.Init();
            //CruiseControl.Init();
            //VehicleDamage.Init();
            //EngineManager.Init();
            //VehicleWindowManager.Init();
            //DoorManager.Init();
            //FuelManager.Init();
            //LockManager.Init();
            //Lockpicking.Init();
            //RandomCarLocks.Init();
            //EmotesManager.Init();
            //Stay.Init();
            //DisableAirControls.Init();

            //Log.Verbose("Leaving ClassLoader Init");
        }
    }
}
