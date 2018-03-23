using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Player;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyRP.Roleplay.Enums.Character;

namespace FamilyRP.Roleplay.Client.Classes.Actions.EMS
{
	static class EMS
	{
		static float HealRange = 3f;

		static PlayerList PlayerList = new PlayerList();
		static PedList PedList = new PedList();

		static public void Init() {
			Client.GetInstance().ClientCommands.Register( "/heal", Heal );
			Client.GetInstance().ClientCommands.Register( "/revive", Revive );
			Client.GetInstance().RegisterEventHandler( "Player.Revive", new Action<string>( ReceiveRevive ) );
			Client.GetInstance().RegisterEventHandler( "NPC.Revive", new Action<string>( ReceiveNPCRevive ) );
			Function.Call<bool>( Hash.DECOR_REGISTER, "Ped.IsIncapacitated", 2 );
			Function.Call<bool>( Hash.DECOR_REGISTER, "Ped.BeingRevived", 2 );
		}

		// TODO: Test if this syncs or if it has to be passed via trigger
		[RequiresPermissionFlags( Permissions.EMS )]
		static public void Heal( Command command ) {
			var healList = PlayerList.Where( p => p.Character.Position.DistanceToSquared( Game.PlayerPed.Position ) < Math.Pow( HealRange, 2 ) && p.Character.Health < p.Character.MaxHealth );
			if( healList.Count() > 0 ) {
				var healTarget = healList.First();
				healTarget.Character.Health = healTarget.Character.MaxHealth;
			}
		}

		static public void ReceiveRevive( string serializedEventModel ) {
			try {
				TriggerEventForPlayersModel eventModel = Helpers.MsgPack.Deserialize<TriggerEventForPlayersModel>( serializedEventModel );
				DeathHandler.RevivePlayer();
				DamageBitFlags.ClearAllDamageFlags( Game.PlayerPed );
				Function.Call( Hash.DECOR_SET_BOOL, Game.PlayerPed.Handle, "Ped.IsIncapacitated", false );
			}
			catch( Exception ex ) {
				Log.Error( $"ReceiveRevive ERROR: {ex.Message}" );
			}
		}

		static public void ReceiveNPCRevive( string serializedLocation ) {
			try {
				var eventData = Helpers.MsgPack.Deserialize<float[]>( serializedLocation );
				var reviveListAI = PedList.Select( p => new Ped( p ) ).Where( p => p.Position.DistanceToSquared( eventData.ToVector3() ) < Math.Pow( HealRange, 2 ) && Function.Call<bool>( Hash.DECOR_GET_BOOL, p.Handle, "Ped.BeingRevived" ) );

				if( reviveListAI.Count() > 0 ) {
					var reviveTarget = reviveListAI.First();

					Function.Call( Hash.RESURRECT_PED, reviveTarget.Handle );
					Function.Call( Hash.REVIVE_INJURED_PED, reviveTarget.Handle );
					Function.Call( Hash.CLEAR_PED_TASKS_IMMEDIATELY, reviveTarget.Handle );
					Function.Call( Hash.SET_ENTITY_COLLISION, reviveTarget.Handle, true, true );
					Function.Call( Hash._PLAY_AMBIENT_SPEECH1, reviveTarget.Handle, "GENERIC_THANKS", "SPEECH_PARAMS_FORCE" );
				}
			}
			catch( Exception ex ) {
				Log.Error( $"EMS/Cop NPC Revive error: {ex.Message}" );
			}
		}

		[RequiresPermissionFlags( Permissions.EMS, Permissions.Police )]
		static public async void Revive( Command command ) {
			try {
				var reviveList = PlayerList.Where( p => p != Game.Player && p.Character.Position.DistanceToSquared( Game.PlayerPed.Position ) < Math.Pow( HealRange, 2 ) && (Function.Call<bool>( Hash.IS_ENTITY_DEAD, p.Handle ) || Function.Call<bool>( Hash.IS_PED_FATALLY_INJURED, p.Handle ) || (Function.Call<bool>( Hash.DECOR_EXIST_ON, p.Character.Handle, "Ped.IsIncapacitated" ) && Function.Call<bool>( Hash.DECOR_GET_BOOL, p.Character.Handle, "Ped.IsIncapacitated" ))) );
				var reviveListAi = PedList.Select( p => new Ped( p ) ).Where( p => p.Position.DistanceToSquared( Game.PlayerPed.Position ) < Math.Pow( HealRange, 2 ) && (Function.Call<bool>( Hash.IS_ENTITY_DEAD, p.Handle ) || Function.Call<bool>( Hash.IS_PED_FATALLY_INJURED, p.Handle )) );

				if( reviveList.Any() ) {
					var reviveTarget = reviveList.First();
					BaseScript.TriggerServerEvent( "TriggerEventForPlayers", Helpers.MsgPack.Serialize( new TriggerEventForPlayersModel( reviveTarget.ServerId, "Player.Revive", "", true ) ) );
				}
				else if( reviveListAi.Any() ) {
					var reviveTarget = reviveListAi.First();

					Function.Call<bool>( Hash.DECOR_SET_BOOL, reviveTarget.Handle, "Ped.BeingRevived", true );

					BaseScript.TriggerServerEvent( "TriggerEventForAll", Helpers.MsgPack.Serialize( new TriggerEventForAllModel( "NPC.Revive", Helpers.MsgPack.Serialize( reviveTarget.Position.ToArray() ), false ) ) );

                    DamageBitFlags.ClearAllDamageFlags(reviveTarget);
                    Function.Call(Hash.RESURRECT_PED, reviveTarget.Handle);
                    Function.Call(Hash.REVIVE_INJURED_PED, reviveTarget.Handle);
                    Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, reviveTarget.Handle);
                    Function.Call(Hash.SET_ENTITY_COLLISION, reviveTarget.Handle, true, true);
                    reviveTarget.Position = reviveTarget.Position + new Vector3(0, 0, 1f);
                    Function.Call(Hash._PLAY_AMBIENT_SPEECH1, reviveTarget.Handle, "GENERIC_THANKS", "SPEECH_PARAMS_FORCE");

                    await BaseScript.Delay( 3000 );

					Function.Call<bool>( Hash.DECOR_SET_BOOL, reviveTarget.Handle, "Ped.BeingRevived", false );
				}
			}
			catch( Exception ex ) {
				Log.Error( $"EMS/Cop Revive error: {ex.Message}" );
			}
		}
	}
}
