using System;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Jobs.Transportation.Trains;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.SharedClasses;

namespace FamilyRP.Roleplay.Client.Classes.Vehicle
{
	// TODO: Integrate with key system
	internal static class LockManager
	{
		private const double UnlockDistance = 4f;

		public static void Init() {
			Client.GetInstance().RegisterTickHandler( OnTick );
		}

		public static void SetVehicleLocks( CitizenFX.Core.Vehicle vehicle, LockState lockState ) {
			try {
				switch( lockState ) {
				case LockState.Locked:
					Function.Call( Hash.SET_VEHICLE_DOORS_LOCKED_FOR_ALL_PLAYERS, vehicle.Handle, true );
					vehicle.LockStatus = VehicleLockStatus.Locked;
					Function.Call( Hash.DECOR_SET_BOOL, vehicle.Handle, "Vehicle.Locked", true );
					return;
				case LockState.Unlocked:
					Function.Call( Hash.SET_VEHICLE_DOORS_LOCKED_FOR_ALL_PLAYERS, vehicle.Handle, false );
					vehicle.LockStatus = VehicleLockStatus.Unlocked;
					Function.Call( Hash.DECOR_SET_BOOL, vehicle.Handle, "Vehicle.Locked", false );
					return;
				default:
					return;
				}
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		public static Task OnTick() {
			if( !ControlHelper.IsControlJustPressed( Control.CinematicSlowMo ) ) return Task.FromResult( 0 );

			if( Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed &&
				!TrainManager.TrainLocomotiveModels.Contains( Game.PlayerPed.CurrentVehicle.Model ) ) {
				var vehicle = Game.PlayerPed.CurrentVehicle;
				var isLocked = Function.Call<bool>( Hash.DECOR_EXIST_ON, vehicle.Handle, "Vehicle.Locked" ) &&
							   Function.Call<bool>( Hash.DECOR_GET_BOOL, vehicle.Handle, "Vehicle.Locked" );
				if( isLocked ) {
					SetVehicleLocks( vehicle, LockState.Unlocked );
					Log.ToChat( "Vehicle unlocked!" );
				}
				else {
					SetVehicleLocks( vehicle, LockState.Locked );
					Log.ToChat( "Vehicle locked!" );
				}
			}
			else {
				var vehicle = WorldProbe.GetVehicleInFrontOfPlayer( 3f );
				if( vehicle == null ) {
					var closeVehicles = new VehicleList().Select( v => new CitizenFX.Core.Vehicle( v ) )
						.Where( v => v.Position.DistanceToSquared( Game.PlayerPed.Position ) < Math.Pow( UnlockDistance, 2 ) )
						.OrderBy( v => v.Position.DistanceToSquared( Game.PlayerPed.Position ) );
					if( closeVehicles.Any() )
						vehicle = closeVehicles.First();
				}

				if( vehicle == null || TrainManager.TrainLocomotiveModels.Contains( vehicle.Model ) ) return Task.FromResult( 0 );

				var isLocked = Function.Call<bool>( Hash.DECOR_EXIST_ON, vehicle.Handle, "Vehicle.Locked" ) &&
							   Function.Call<bool>( Hash.DECOR_GET_BOOL, vehicle.Handle, "Vehicle.Locked" );
				if( isLocked ) {
					SetVehicleLocks( vehicle, LockState.Unlocked );
					Log.ToChat( "Vehicle unlocked!" );
					// TODO: Make all other players in the vehicle do this same thing to be extra-sure it unlocks properly
					Game.PlayerPed.Task.ClearAll();
				}
				else {
					SetVehicleLocks( vehicle, LockState.Locked );
					Log.ToChat( "Vehicle locked!" );
				}
			}
			return Task.FromResult( 0 );
		}
	}

	public enum LockState
	{
		Locked,
		Unlocked
	}
}