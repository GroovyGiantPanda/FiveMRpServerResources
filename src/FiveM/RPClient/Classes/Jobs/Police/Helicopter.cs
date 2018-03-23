using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Enums.Police.Helicopter;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Jobs.Police.Helicopter
{
    // TODO: Potentially make zoom more fluid
    // TODO: Add some sound effects
    // TODO: Potentially allow user to look around in more ways when in cam mode (this is how it worked on most other servers though)
    // TODO: Check if searchlight syncs
    static class Helicopter
    {
        static Camera camera = null;
        static HashSet<Model> modelHashes = new HashSet<Model>() { Game.GenerateHash("maverick"), Game.GenerateHash("polmav") };
        static Vector3 cameraOffset = new Vector3(0f, 2.5f, -1f);
        static CitizenFX.Core.Player currentLock = null;
        static Dictionary<CameraMode, bool[]> cameraModeMap = new Dictionary<CameraMode, bool[]>()
        {
            [CameraMode.FLIR] = new bool[] { true, false },
            [CameraMode.Nightvision] = new bool[] { false, true },
            [CameraMode.None] = new bool[] { false, false }
        };
        static Array cameraModeEnumValues = Enum.GetValues(typeof(CameraMode));
        static List<float> cameraZoom = new List<float>(){75f, 40f, 20f, 5f};
        static CameraMode currentCameraMode = CameraMode.FLIR;
        static int currentCameraZoomIndex = 0;
        static PlayerList playerList = new PlayerList();
        static Scaleform heliScaleform;

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static public async Task OnTick()
        {
            if(camera != null && World.RenderingCamera == camera)
            {
                if (!Game.PlayerPed.IsInVehicle())
                {
                    ResetCamera();
                }
                else if (modelHashes.Contains(Game.PlayerPed.CurrentVehicle.Model.Hash))
                {
                    if (heliScaleform.IsLoaded)
                    {
                        heliScaleform.CallFunction("SET_ALT_FOV_HEADING", Game.PlayerPed.CurrentVehicle.Position.Z, camera.FieldOfView, camera.Rotation.Z);
                        heliScaleform.Render2D();
                    }
                    camera.Rotation = new Vector3(0.0f, 0.0f, CitizenFX.Core.Game.PlayerPed.CurrentVehicle.Rotation.Z);
                    // Toggle NV/Infrared
                    if (ControlHelper.IsControlJustPressed(Settings.Controls["Helicopter.ToggleVisionMode"]))
                    {
                        switch (currentCameraMode)
                        {
                            case CameraMode.None:
                                currentCameraMode = CameraMode.FLIR;
                                break;
                            case CameraMode.FLIR:
                                currentCameraMode = CameraMode.Nightvision;
                                break;
                            case CameraMode.Nightvision:
                                currentCameraMode = CameraMode.None;
                                break;
                        }
                        Game.ThermalVision = cameraModeMap[currentCameraMode][0];
                        Game.Nightvision = cameraModeMap[currentCameraMode][1];
                    }
                    // Change locked target
                    // TODO: Only part left to test
                    else if (ControlHelper.IsControlJustPressed(Settings.Controls["Helicopter.SwitchTarget"]))
                    {
                        try
                        {
                            if (currentLock == null || currentLock == playerList.Last())
                            {
                                currentLock = playerList.First();
                            }
                            else
                            {
                                currentLock = playerList.SkipWhile(p => p != currentLock).Skip(1).First();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.ToChat($"[HELICAM] {ex.GetType().ToString()} thrown");
                        }
                    }
                    // Change zoom
                    else if (ControlHelper.IsControlJustPressed(Settings.Controls["Helicopter.ToggleZoom"]))
                    {
                        camera.FieldOfView = cameraZoom.ElementAt(currentCameraZoomIndex);
                        currentCameraZoomIndex = (currentCameraZoomIndex + 1) % (cameraZoom.Count() - 1);
                    }
                    if (ControlHelper.IsControlJustPressed(Settings.Controls["Helicopter.ToggleCamera"]))
                    {
                        ResetCamera();
                    }
                }
            }
            else if (Game.PlayerPed.IsInVehicle() && modelHashes.Contains(Game.PlayerPed.CurrentVehicle.Model.Hash))
            {
                if (ControlHelper.IsControlJustPressed(Settings.Controls["Helicopter.ToggleCamera"]))
                {
                    camera = World.CreateCamera(Game.PlayerPed.CurrentVehicle.Position, Game.PlayerPed.CurrentVehicle.Rotation, 75f);
                    camera.AttachTo(Game.PlayerPed.CurrentVehicle, cameraOffset);
                    World.RenderingCamera = camera;
                    Function.Call(Hash.SET_CAM_INHERIT_ROLL_VEHICLE, camera, true);
                    heliScaleform = new Scaleform("HELI_CAM");
                    while(!heliScaleform.IsLoaded)
                    {
                        await BaseScript.Delay(0);
                    }
                    Function.Call(Hash.SET_TIMECYCLE_MODIFIER, "heliGunCam");
                    Function.Call(Hash.SET_TIMECYCLE_MODIFIER_STRENGTH, 0.3);
                    heliScaleform.CallFunction("SET_CAM_LOGO", 1);
                }
                else if (ControlHelper.IsControlJustPressed(Settings.Controls["Helicopter.Rappel"]))
                {
                    Log.ToChat("Trying to rappel");
                    Function.Call(Hash.TASK_RAPPEL_FROM_HELI, Game.PlayerPed.Handle, 0x41200000);
                }
                else if (ControlHelper.IsControlJustPressed(Settings.Controls["Helicopter.ToggleSearchLight"]))
                {
                    Game.PlayerPed.CurrentVehicle.IsSearchLightOn = !Game.PlayerPed.CurrentVehicle.IsSearchLightOn;
                }
            }
        }

        static void ResetCamera()
        {
            World.RenderingCamera = null;
            camera.Delete();
            camera = null;
            Game.ThermalVision = false;
            Game.Nightvision = false;
            heliScaleform.Dispose();
        }
    }
}
