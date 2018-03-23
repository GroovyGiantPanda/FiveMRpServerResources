using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.SharedClasses;

namespace FamilyRP.Roleplay.Client.Helpers.GameHelper
{
	public class Game
	{
		public static RaycastResult GetEntityInFrontOfPed( Ped ped, float maxDistance = 5.0f ) {
			Vector3 offset = Function.Call<Vector3>( Hash.GET_OFFSET_FROM_ENTITY_IN_WORLD_COORDS, ped, 0.0, 5.0, 0.0 );
			return Function.Call<RaycastResult>( Hash._START_SHAPE_TEST_RAY, ped.Position.X, ped.Position.Y, ped.Position.Z, offset.X, offset.Y, offset.Z, 10, ped, 0 );
		}

        public static CitizenFX.Core.Entity GetEntityInFrontOfPlayer(float distance = 5f)
        {
            try
            {
                RaycastResult raycast = CitizenFX.Core.World.Raycast(CitizenFX.Core.Game.PlayerPed.Position, CitizenFX.Core.Game.PlayerPed.GetOffsetPosition(new Vector3(0f, distance, 0f)), IntersectOptions.Everything);
                if (raycast.DitHitEntity)
                    return (CitizenFX.Core.Entity)raycast.HitEntity;
            }
            catch (Exception ex)
            {
                Log.Info($"{ex.Message}");
            }
            return null;
        }
	}
}