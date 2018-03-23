using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.SharedClasses;

namespace FamilyRP.Roleplay.Client.Helpers
{
    static class WorldProbe
    {
        // Intended for:
        // Getting closest ped, closest vehicle, vehicle in front (raycast), raycast normal, raycast collision coordinates
        static _RaycastResult? CrosshairRaycastThisTick = null;

        static WorldProbe()
        {
            Client.GetInstance().RegisterTickHandler(new Func<Task>(() => { CrosshairRaycastThisTick = null; return Task.FromResult(0); }));
        }

        public static CitizenFX.Core.Vehicle GetVehicleInFrontOfPlayer(float distance = 5f)
        {
            try
            {
                Entity source = Game.PlayerPed.IsInVehicle() ? (Entity)Game.PlayerPed.CurrentVehicle : Game.PlayerPed;
                return GetVehicleInFrontOfPlayer(source, source, distance);
            }
            catch (Exception ex)
            {
                Log.Error($"WorldProbe GetVehicleInFrontOfPlayer Error: {ex.Message}");
            }
            return default(CitizenFX.Core.Vehicle);
        }

        public static CitizenFX.Core.Vehicle GetVehicleInFrontOfPlayer(Entity source, Entity ignore, float distance = 5f)
        {
            try
            {
                RaycastResult raycast = World.Raycast(source.Position + new Vector3(0f, 0f, -0.4f), source.GetOffsetPosition(new Vector3(0f, distance, 0f)) + new Vector3(0f, 0f, -0.4f), (IntersectOptions)71, ignore);
                if (raycast.DitHitEntity && raycast.HitEntity.Model.IsVehicle)
                {
                    return (CitizenFX.Core.Vehicle)raycast.HitEntity;
                }
            }
            catch (Exception ex)
            {
                Log.Info($"[WORLDPROBE] GetVehicleInFrontOfPlayer Error: {ex.Message}");
            }
            return default(CitizenFX.Core.Vehicle);
        }

        public static Vector3 CalculateClosestPointOnLine(Vector3 start, Vector3 end, Vector3 point)
        {
            try
            {
                float dotProduct = Vector3.Dot(start - end, point - start);
                float percent = dotProduct / (start - end).LengthSquared();
                if (percent < 0.0f) return start;
                else if (percent > 1.0f) return end;
                else return (start + (percent * (end - start)));
            }
            catch (Exception ex)
            {
                Log.Error($"WorldProbe CalculateClosestPointOnLine Error: {ex.Message}");
            }
            return default(Vector3);
        }

        public static Vector3 GameplayCamForwardVector()
        {
            try
            {
                Vector3 rotation = (float)(Math.PI / 180.0) * Function.Call<Vector3>(Hash._GET_GAMEPLAY_CAM_ROT, 2);
                return Vector3.Normalize(new Vector3((float)-Math.Sin(rotation.Z) * (float)Math.Abs(Math.Cos(rotation.X)), (float)Math.Cos(rotation.Z) * (float)Math.Abs(Math.Cos(rotation.X)), (float)Math.Sin(rotation.X)));
            }
            catch (Exception ex)
            {
                Log.Error($"WorldProbe GameplayCamForwardVector Error: {ex.Message}");
            }
            return default(Vector3);
        }


        public static RaycastResult _CrosshairRaycast()
        {
            try
            {
                return World.Raycast(CitizenFX.Core.Game.PlayerPed.Position, CitizenFX.Core.Game.PlayerPed.Position + 1000 * GameplayCamForwardVector(), IntersectOptions.Everything, CitizenFX.Core.Game.PlayerPed);
            }
            catch (Exception ex)
            {
                Log.Error($"WorldProbe _CrosshairRaycast Error: {ex.Message}");
            }
            return default(RaycastResult);
        }

        public struct _RaycastResult
        {
            public Entity HitEntity { get; }
            public Vector3 HitPosition { get; }
            public Vector3 SurfaceNormal { get; }
            public bool DitHit { get; }
            public bool DitHitEntity { get; }
            public int Result { get; }

            public _RaycastResult(bool DitHit, Vector3 HitPosition, Vector3 SurfaceNormal, int entityHandle, int Result)
            {
                this.DitHit = DitHit;
                this.HitPosition = HitPosition;
                this.SurfaceNormal = SurfaceNormal;
                if (entityHandle == 0)
                {
                    this.HitEntity = null;
                    this.DitHitEntity = false;
                }
                else
                {
                    this.HitEntity = Entity.FromHandle(entityHandle);
                    this.DitHitEntity = true;
                }
                this.Result = Result;
            }
        }


        public static _RaycastResult CrosshairRaycast(float distance = 1000f)
        {
            return CrosshairRaycast(CitizenFX.Core.Game.PlayerPed);
        }

        /// <summary>
        /// Because HitPosition and SurfaceNormal are currently broken in platform function
        /// </summary>
        /// <returns></returns>
        public static _RaycastResult CrosshairRaycast(Entity ignore, float distance = 1000f)
        {
            try
            {
                // Uncomment these to potentially save on raycasts (don't think they're ridiculously costly, but there's a # limit per tick)
                //if(CrosshairRaycastThisTick != null && distance == 1000f) return (_RaycastResult) CrosshairRaycastThisTick;

                Vector3 start = CitizenFX.Core.GameplayCamera.Position;
                Vector3 end = CitizenFX.Core.GameplayCamera.Position + distance * WorldProbe.GameplayCamForwardVector();
                int raycastHandle = Function.Call<int>(Hash._START_SHAPE_TEST_RAY, start.X, start.Y, start.Z, end.X, end.Y, end.Z, IntersectOptions.Everything, ignore.Handle, 0);
                OutputArgument DitHit = new OutputArgument();
                OutputArgument HitPosition = new OutputArgument();
                OutputArgument SurfaceNormal = new OutputArgument();
                OutputArgument HitEntity = new OutputArgument();
                Function.Call<int>(Hash.GET_SHAPE_TEST_RESULT, raycastHandle, DitHit, HitPosition, SurfaceNormal, HitEntity);

                var result = new _RaycastResult(DitHit.GetResult<bool>(), HitPosition.GetResult<Vector3>(), SurfaceNormal.GetResult<Vector3>(), HitEntity.GetResult<int>(), raycastHandle);

                //if(distance == 1000f) CrosshairRaycastThisTick = result;
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"WorldProbe CrosshairRaycast Error: {ex.Message}");
            }
            return default(_RaycastResult);
        }

        // Apparently there's a built-in native for this...
        // TODO: Replace with that
        public static string GetEntityType(int entityHandle)
        {
            try
            {
                if (Function.Call<bool>(Hash.IS_ENTITY_A_PED, entityHandle))
                    return "PED";
                else if (Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, entityHandle))
                    return "VEH";
                else if (Function.Call<bool>(Hash.IS_ENTITY_AN_OBJECT, entityHandle))
                    return "OBJ";
            }
            catch (Exception ex)
            {
                Log.Error($"WorldProbe GetEntityType Error: {ex.Message}");
            }
            return "UNK";
        }


        public static async Task<float?> FindGroundZ(Vector2 position)
        {
            float? result = null;
            try
            {
                float[] groundCheckHeight = new float[] { 100.0f, 150.0f, 50.0f, 0.0f, 200.0f, 250.0f, 300.0f, 350.0f, 400.0f, 450.0f, 500.0f, 550.0f, 600.0f, 650.0f, 700.0f, 750.0f, 800.0f };
                
                foreach (float h in groundCheckHeight)
                {
                    //await BaseScript.Delay(0);
                    OutputArgument z = new OutputArgument();
                    if (Function.Call<bool>(Hash.GET_GROUND_Z_FOR_3D_COORD, position.X, position.Y, (float)h, z, false))
                    {
                        result = z.GetResult<float>();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"WorldProbe FindGroundZ Error: {ex.Message}");
            }
            return result;
        }
    }
}
