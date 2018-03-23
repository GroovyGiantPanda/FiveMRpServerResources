using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.Helpers;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyRP.Roleplay.Server.Enums;

namespace FamilyRP.Roleplay.Client.Classes.Environment.DevTools
{
    static class ManipulateObject
    {
        static Entity CurrentObject = null;
        static Vector3 RaycastPickupOffset = new Vector3();
        static double ManipulationDistance = 0;
        static double ZoomSpeed = 0.1f;
        static int RotationAxis = 2;
        static int LastTickTime = 0;


        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static public Task OnTick()
        {
            int CurrentTime = Function.Call<int>(Hash.GET_GAME_TIMER);
            int TickTime = (CurrentTime - LastTickTime);
            LastTickTime = CurrentTime;
            if (CurrentObject != null)
            {
                if (!CurrentObject.Exists())
                {
                    Log.ToChat($"[OBJECTMANIP] Removing invalid object reference");
                    NativeWrappers.SetPedCanSwitchWeapon(Game.PlayerPed, true);
                    CurrentObject = null;
                    Task.Factory.StartNew(async () => { await BaseScript.Delay(2000); FamilyRP.Roleplay.Client.UI.disableEntityUI = false; });
                    return Task.FromResult(0);
                }
                else
                {
                    var raycast = WorldProbe.CrosshairRaycast(CurrentObject);
                    float RaycastDistance = raycast.HitPosition.DistanceToSquared(CitizenFX.Core.GameplayCamera.Position);
                    // Uncomment if you want objects to snap to ground
                    // Similar to GTAs own version but  with different logic (different snapping)
                    //CurrentObject.Position = raycast.HitPosition - RaycastPickupOffset;
                    //CurrentObject.Rotation = NormalVectorToRotation(raycast.SurfaceNormal);

                    // Replace Position with PositionNoOffset if you don't want objects to snap to surfaces
                    CurrentObject.Position = CitizenFX.Core.GameplayCamera.Position + (float)ManipulationDistance * WorldProbe.GameplayCamForwardVector();

                    if (NoClip.DevToolsBindEnabled && ControlHelper.IsControlJustPressed(Control.PhoneCameraSelfie, true, Enums.Controls.ControlModifier.Ctrl))
                    {
                        CurrentObject.Opacity = 255;
                        Function.Call(Hash.SET_ENTITY_DYNAMIC, CurrentObject.Handle, true);
                        CurrentObject = null;
                        NativeWrappers.SetPedCanSwitchWeapon(Game.PlayerPed, true);
                        Task.Factory.StartNew(async () => { await BaseScript.Delay(2000); FamilyRP.Roleplay.Client.UI.disableEntityUI = false; });
                    }
                    else if (ControlHelper.IsControlPressed(Control.CursorCancel, true, Enums.Controls.ControlModifier.Shift))
                    {
                        CurrentObject.Rotation = CurrentObject.Rotation + TickTime * GetRotationVector(RotationAxis);
                    }
                    else if (ControlHelper.IsControlPressed(Control.CursorCancel, true, Enums.Controls.ControlModifier.Ctrl))
                    {
                        CurrentObject.Rotation = CurrentObject.Rotation - TickTime * GetRotationVector(RotationAxis);
                    }
                    else if (ControlHelper.IsControlJustPressed(Control.CursorCancel, true, Enums.Controls.ControlModifier.Alt))
                    {
                        RotationAxis = (RotationAxis + 1) % 3;
                        Log.ToChat($"Rotating along axis {RotationAxis}");
                    }
                    else if (ControlHelper.IsControlJustPressed(Control.NextWeapon))
                    {
                        ManipulationDistance -= TickTime * ZoomSpeed;
                        Log.ToChat($"Manipulation distance: {ManipulationDistance:0.0}");
                    }
                    else if (ControlHelper.IsControlJustPressed(Control.PrevWeapon))
                    {
                        ManipulationDistance += TickTime * ZoomSpeed;
                        Log.ToChat($"Manipulation distance: {ManipulationDistance:0.0}");
                    }
                }
            }
            else
            {
                if (NoClip.DevToolsBindEnabled && ControlHelper.IsControlJustPressed(Control.PhoneCameraSelfie, true, Enums.Controls.ControlModifier.Ctrl))
                {
                    var raycast = WorldProbe.CrosshairRaycast();
                    if (!raycast.DitHitEntity || !Function.Call<bool>(Hash.DOES_ENTITY_EXIST, raycast.HitEntity.Handle))
                        return Task.FromResult(0);
                    CurrentObject = raycast.HitEntity;
                    Log.ToChat($"Entity handle {CurrentObject.Handle}");
                    RaycastPickupOffset = raycast.HitPosition - CurrentObject.Position;
                    ManipulationDistance = (float)Math.Sqrt(CitizenFX.Core.GameplayCamera.Position.DistanceToSquared(raycast.HitPosition));
                    NativeWrappers.SetPedCanSwitchWeapon(Game.PlayerPed, false);
                    FamilyRP.Roleplay.Client.UI.disableEntityUI = true;
                    CurrentObject.Opacity = 180;
                }
            }
            return Task.FromResult(0);
        }

        static private Vector3 GetRotationVector(int rotationAxis)
        {
            switch (rotationAxis)
            {
                case 0:
                    return new Vector3(0.1f, 0f, 0f);
                case 1:
                    return new Vector3(0f, 0.1f, 0f);
                case 2:
                    return new Vector3(0f, 0f, 0.1f);
            }
            return new Vector3();
        }

        static internal async void CreateObject(ObjectHash objectHash)
        {
            var raycast = WorldProbe.CrosshairRaycast();
            CurrentObject = await World.CreatePropNoOffset(new Model((int)objectHash), raycast.HitPosition, NormalVectorToRotation(raycast.SurfaceNormal), true);
            RaycastPickupOffset = raycast.HitPosition - CurrentObject.Position;
            ManipulationDistance = (float)Math.Sqrt(CitizenFX.Core.GameplayCamera.Position.DistanceToSquared(raycast.HitPosition));
            NativeWrappers.SetPedCanSwitchWeapon(Game.PlayerPed, false);
            CurrentObject.Opacity = 180;
        }

	    static internal async void CreateObjectAtPosition( ObjectHash objectHash, Vector3 position, Vector3 rotation ) {
		    CurrentObject = await World.CreatePropNoOffset( new Model( (int)objectHash ), position, rotation, false );
		    Function.Call( Hash.SET_ENTITY_DYNAMIC, CurrentObject.Handle, false );
		    CurrentObject = null;
		    Task.Factory.StartNew( async () => { await BaseScript.Delay( 2000 ); FamilyRP.Roleplay.Client.UI.disableEntityUI = false; } );
	    }

		// TODO: Move to WorldProbe
		public static Vector3 NormalVectorToRotation(Vector3 normal)
        {
            double pitch = Math.Asin(normal.Z) * 180 / (float)Math.PI - 90;
            Vector2 yaw = new Vector2(normal.X, normal.Y);
            yaw.Normalize();
            double multiplier = 1;
            if (normal.X > 0 && normal.Y > 0)
                multiplier = -1;
            Vector3 result = new Vector3((float)pitch, 0f, (float)multiplier * (float)Math.Acos(normal.Y) * 180 / (float)Math.PI);
            return result;
        }
    }


    static public class NoClip
    {
        static float rotationSpeed = 2.5f;
        static int currentTravelSpeed = 0;
        static float currentHeading;
        static public bool DevToolsBindEnabled = false;
        static bool noClipEnabled = false;
        static Entity entity = null;

        public static float GetTravelSpeed()
        {
            switch (currentTravelSpeed)
            {
                case 0:
                    return 0.2f;
                case 1:
                    return 0.4f;
                case 2:
                    return 0.8f;
                case 3:
                    return 1.6f;
                case 4:
                    return 3.2f;
                case 5:
                    return 6.4f;
                case 6:
                    return 0.1f;
                case 7:
                    return 0.02f;
            }
            return 0.8f;
        }

        public static Vector3 GetCurrentForwardVector()
        {
            if (Game.PlayerPed.IsInVehicle())
            {
                return Vector3.Normalize(new Vector3(-(float)Math.Sin(Game.PlayerPed.Rotation.Z / 180 * Math.PI), (float)Math.Cos(Game.PlayerPed.Rotation.Z / 180 * Math.PI), 0f));
            }
            else
            {
                // Don't ask why
                return Vector3.Normalize(new Vector3(-(float)Math.Sin(Game.PlayerPed.Rotation.Z / 180 * Math.PI), (float)Math.Cos(Game.PlayerPed.Rotation.Z / 180 * Math.PI), 0f));
            }
        }

        internal static Vector3 GetCurrentMovementVector()
        {
            return GetTravelSpeed() * GetCurrentForwardVector();
        }

        static public Task OnTick()
        {
            if (DevToolsBindEnabled && ControlHelper.IsControlJustPressed(Control.PhoneCameraSelfie, true, Enums.Controls.ControlModifier.Shift))
            {
                if (noClipEnabled)
                {
                    Log.ToChat("Disabling NoClip");
                    entity.IsInvincible = false;
                    Game.Player.IsInvincible = false;
                    Function.Call(Hash.SET_ENTITY_DYNAMIC, entity.Handle, true);
                    Function.Call(Hash.SET_ENTITY_COLLISION, entity.Handle, true, true);
                    entity.Opacity = 255;
                    Function.Call(Hash.SET_USER_RADIO_CONTROL_ENABLED, true);
                    entity = null;
                    noClipEnabled = false;
                }
                else
                {
                    Log.ToChat("Enabling NoClip");
                    entity = Game.PlayerPed.IsInVehicle() ? (Entity)Game.PlayerPed.CurrentVehicle : (Entity)Game.PlayerPed;
                    currentHeading = entity.Heading;
                    entity.Velocity = new Vector3(0, 0, 0);
                    entity.IsInvincible = true;
                    Game.Player.IsInvincible = true;
                    Function.Call(Hash.SET_ENTITY_DYNAMIC, entity.Handle, false);
                    Function.Call(Hash.SET_ENTITY_COLLISION, entity.Handle, false, false);
                    entity.Opacity = 180;
                    Function.Call(Hash.SET_USER_RADIO_CONTROL_ENABLED, false);
                    noClipEnabled = true;
                }
            }
            if (noClipEnabled)
            {
                entity.PositionNoOffset = entity.Position;
                entity.Velocity = Vector3.Zero;
                entity.Heading = currentHeading;
                if (ControlHelper.IsControlPressed(Control.ContextSecondary)) // Q
                {
                    entity.PositionNoOffset = new Vector3(entity.Position.X, entity.Position.Y, entity.Position.Z + GetTravelSpeed() / 2);
                }
                if (ControlHelper.IsControlPressed(Control.HUDSpecial)) // Z
                {
                    entity.PositionNoOffset = new Vector3(entity.Position.X, entity.Position.Y, entity.Position.Z - GetTravelSpeed() / 2);
                }
                if (ControlHelper.IsControlPressed(Control.VehicleAccelerate)) // W
                {
                    entity.PositionNoOffset = entity.Position + GetCurrentMovementVector();
                }
                if (ControlHelper.IsControlPressed(Control.ReplayRecord)) // S
                {
                    entity.PositionNoOffset = entity.Position - GetCurrentMovementVector();
                }
                if (ControlHelper.IsControlPressed(Control.VehicleMoveLeftOnly)) // A
                {
                    currentHeading += rotationSpeed;
                }
                if (ControlHelper.IsControlPressed(Control.VehicleMoveRightOnly)) // D
                {
                    currentHeading -= rotationSpeed;
                }
                if (ControlHelper.IsControlJustPressed(Control.Sprint, true, Enums.Controls.ControlModifier.Shift)) // D
                {
                    currentTravelSpeed = (currentTravelSpeed + 1) % 8;
                    Log.ToChat($"Set NoClip speed to {GetTravelSpeed():0.0}");
                }
            }
            return Task.FromResult(0);
        }

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }
    }
}
