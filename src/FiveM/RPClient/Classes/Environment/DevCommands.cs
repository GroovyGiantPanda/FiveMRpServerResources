using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.Client.Classes.Environment.Controls;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.Server.Enums;
using FamilyRP.Roleplay.Client.Classes.Environment.DevTools;
using FamilyRP.Roleplay.Client.Classes.Vehicle;
using FamilyRP.Roleplay.Client.Classes.Player;
using FamilyRP.Roleplay.SharedModels;
using FamilyRP.Roleplay.Server.Classes;
using FamilyRP.Roleplay.Enums.Character;
using FamilyRP.Roleplay.Helpers;

namespace FamilyRP.Roleplay.Client
{
    public class DevCommands : CommandProcessor
    {
        public static bool IsDevUIEnabled = false;
        public static bool IsDevEntityUIEnabled = false;

		public DevCommands() {
			try {
				Client.GetInstance().ClientCommands.Register( "/dev", Handle );
				Register( "ui", ToggleDevUI );
				Register( "eui", ToggleEntityUI );
				Register( "load_ipl", LoadIPL );
				Register( "unload_ipl", UnloadIPL );
				Register( "ainterioramp", ActivateInteriorAtMyPosition );
				Register( "ainteriorprops", ActivateInteriorProps );
				Register( "dainteriorprops", DeactivateInteriorProps );
				Register( "tp", Teleport );
				Register( "tpp", TeleportToPlayer );
				Register( "iddqd", Godmode );
				Register( "veh", SpawnVehicle );
				Register( "delveh", RemoveVehicle );
				Register( "lock", ToggleVehicleLock );
				Register( "fix", FixVehicle );
				Register( "ochood", OpenCloseHood );
				Register( "tcmtm", TestChatMessageToMyself );
				Register( "setprop", SetPlayerProp );
				Register( "setcomp", SetPlayerComponent );
				Register( "setpmodel", SetPlayerModel );
				Register( "toggledevtools", ToggleDevToolsBind );
				Register( "suicide", Suicide );
				Register( "replacepilot", ReplacePilot );
				Register( "setfloat", SetFloat );
				Register( "setmissionentity", SetMissionEntity );
				Register( "setnetmigration", SetNetMigration );
				Register( "nettoent", NetToEnt );
				Function.Call( Hash.DECOR_REGISTER, "TEST", 3 );
				Register( "settestdecor", SetTestDecor );
				Register( "addentityblip", AddEntityBlip );
				Register( "crobj", CreateObject );
				Register( "cfp", CreateFollowingPed );
				Register( "findtestdecor", FindTestDecor );
				Register( "listtogglemods", ListToggleMods );
				Register( "tpm", TeleportToMarker );
				Register( "bring", Bring );
				Client.GetInstance().RegisterEventHandler( "Dev.Bring", new Func<int, Task>( HandleBring ) );
				Register( "setint", SetInt );
				Register( "skipwall", SkipWall );
				Register( "adjentitycoords", AdjustEntityCoords );
				Register( "adjentityrot", AdjustEntityRot );
				Register( "keycodetester", ToggleKeyCodeTester );
				Register( "createblip", CreateBlip );
				Register( "reviveme", ReviveMe );

				Register( "spawnprop", SpawnProp );

				Register( "blveh", BlipAndListVehicles );
				Register( "tlg", TrafficLights );
				Register( "lip", ListInjuredPeds );
				Register( "gaw", GiveAllWeapons );
				//Register("effect", Effect);
				Register( "do", DeleteObjects );
				Register( "crp", CreateRandPed );
				Register( "setmapzoom", SetMapZoom );
				Register( "create3dthing", Create3DThing );
				Register( "spawntrain", SpawnTrain );

				//Client.GetInstance().RegisterEventHandler("TempBlipModule.receiveInitialBlips", new Action<string>(InitialBlipCreatorExecute)); // Temporary
				//Client.GetInstance().RegisterEventHandler("playerSpawned", new Action(InitialBlipCreatorRequest)); // Temporary
				//Client.GetInstance().RegisterEventHandler("TempBlipModule.addBlip", new Action<string>(PostSpawnBlipReceiver)); // Temporary
				//Client.GetInstance().RegisterEventHandler("Dev.Print", new Action<string>(DevPrint)); // Temporary
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		private void SpawnProp( Command cmd ) {
			Task.Factory.StartNew( async () => {
				Model model = new Model( Function.Call<int>( Hash.GET_HASH_KEY, cmd.Args.Get( 0 ) ) );

				Debug.WriteLine( "Requesting model hash: {0}, cdimage: {1}, collision: {2}, valid: {3}, prop: {4}", model.Hash, model.IsInCdImage, model.IsCollisionLoaded, model.IsValid, model.IsProp );

				if( !await model.Request( 3000 ) ) {
					Debug.WriteLine( "Failed to load model hash: {0}", model.Hash );
					return;
				}

				await World.CreatePropNoOffset( model, Game.PlayerPed.Position, false );
				model.MarkAsNoLongerNeeded();
			} );
		}

        private void ReviveMe(Command command)
        {
            try
            {
                Function.Call(Hash.NETWORK_RESURRECT_LOCAL_PLAYER, Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, Game.PlayerPed.Heading, false, false);
            }
            catch(Exception ex)
            {
                Log.Error($"ReviveMe Error: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private void GiveAllWeapons(Command command)
        {
            Enum.GetValues(typeof(WeaponHash)).OfType<WeaponHash>().ToList().ForEach(w =>
            {
                Game.PlayerPed.Weapons.Give(w, 999, false, true);
                //Game.PlayerPed.Weapons[w].InfiniteAmmo = true;
                //Game.PlayerPed.Weapons[w].InfiniteAmmoClip = true;
            });
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private async void CreateRandPed(Command command)
        {
            try
            {
                Model model = new Model(PedHash.Eastsa01AFM);
                await model.Request(3000);

                int ped = Function.Call<int>(Hash.CREATE_PED, 26, model.Hash, Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 0f, true, false);
                Log.ToChat($"Ped ID: {ped}");
            }
            catch (Exception ex)
            {
                Log.Error($"ClientCommands CreateRandPed Error: {ex.Message}");
            }
        }

        static T RandomEnumValue<T>()
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(new Random().Next(v.Length));
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private async void BlipAndListVehicles(Command command)
        {
            try
            {
                int i = 1;
                Dictionary<string, BlipColor> blipColors = new Dictionary<string, BlipColor>();
                OutputArgument OutArgEntity = new OutputArgument();
                int pedHandle = Function.Call<int>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_FIRST_VEHICLE")), OutArgEntity);
                Log.ToChat($"Ped ID: {OutArgEntity.GetResult<int>()}");
                while (Function.Call<bool>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_NEXT_VEHICLE")), pedHandle, OutArgEntity))
                {
                    i++;
                    uint modelHash = (uint)new Ped(OutArgEntity.GetResult<int>()).Model.Hash;
                    string modelName = Enum.GetName(typeof(VehicleHash), modelHash);
                    Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, OutArgEntity.GetResult<int>(), true, true);
                    int blipHandle = Function.Call<int>(Hash.ADD_BLIP_FOR_ENTITY, OutArgEntity.GetResult<int>());
                    await BaseScript.Delay(30);
                    if (blipColors.Keys.Contains(modelName))
                    {
                        Function.Call(Hash.SET_BLIP_COLOUR, blipHandle, (int)blipColors[modelName]);
                    }
                    else
                    {
                        blipColors.Add(modelName, RandomEnumValue<BlipColor>());
                        Function.Call(Hash.SET_BLIP_COLOUR, blipHandle, (int)blipColors[modelName]);
                    }
                    Log.ToChat($"Next Vehicle Model Name: {modelName}, Index: {i}");
                    await BaseScript.Delay(30);
                }
                Function.Call((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("END_FIND_VEHICLE")), pedHandle);
            }
            catch (Exception ex)
            {
                Log.Error($"ClientCommands BlipAndListVehicles Error: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private void ListInjuredPeds(Command command)
        {
            try
            {
                int i = 1;

                Dictionary<string, BlipColor> blipColors = new Dictionary<string, BlipColor>();
                OutputArgument OutArgEntity = new OutputArgument();
                int pedHandle = Function.Call<int>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_FIRST_PED")), OutArgEntity);
                Log.ToChat($"Ped ID: {OutArgEntity.GetResult<int>()}");
                while (Function.Call<bool>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_NEXT_PED")), pedHandle, OutArgEntity))
                {
                    i++;
                    int entityId = OutArgEntity.GetResult<int>();
                    uint modelHash = (uint)new Ped(entityId).Model.Hash;
                    string modelName = Enum.GetName(typeof(PedHash), modelHash);
                    Function.Call(Hash.SET_ENTITY_HEALTH, entityId, 0);
                    Log.ToChat($"Next Ped Model Name: {modelName}, Index: {i}");
                }
                Function.Call((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("END_FIND_PED")), pedHandle);
            }
            catch (Exception ex)
            {
                Log.Error($"ClientCommands ListInjuredPeds Error: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private async void TrafficLights(Command command)
        {
            try
            {
                int i = 1;
                List<string> trafficLightObjects = new List<string>()
                {
                    "prop_traffic_01a",
                    "prop_traffic_01b",
                    "prop_traffic_01d",
                    "prop_traffic_02a",
                    "prop_traffic_02b",
                    "prop_traffic_03a",
                    "prop_traffic_03b"
                };
                List<uint> trafficLightHashes = new List<uint>();
                trafficLightHashes = trafficLightObjects.Select(o => (uint)CitizenFX.Core.Game.GenerateHash(o)).ToList();
                OutputArgument OutArgEntity = new OutputArgument();
                int pedHandle = Function.Call<int>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_FIRST_OBJECT")), OutArgEntity);
                Log.ToChat($"Object ID: {OutArgEntity.GetResult<int>()}");
                while (Function.Call<bool>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_NEXT_OBJECT")), pedHandle, OutArgEntity))
                {
                    i++;
                    uint modelHash = (uint)new Prop(OutArgEntity.GetResult<int>()).Model.Hash;
                    if (trafficLightHashes.Contains(modelHash))
                    {
                        Log.ToChat($"Next Ped Model Hash: {modelHash}, Index: {i}");
                        Function.Call(Hash.SET_ENTITY_TRAFFICLIGHT_OVERRIDE, OutArgEntity.GetResult<int>(), 0);
                        Function.Call(Hash.SET_ENTITY_ALPHA, OutArgEntity.GetResult<int>(), 0, false);
                        Function.Call(Hash.SET_ENTITY_COLLISION, OutArgEntity.GetResult<int>(), false, false);
                    }
                }
                Function.Call((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("END_FIND_OBJECT")), pedHandle);
            }
            catch (Exception ex)
            {
                Log.Error($"ClientCommands TrafficLights Error: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private void DeleteObjects(Command command)
        {
            try
            {
                int i = 1;
                int deleteHash = (int)CitizenFX.Core.Game.GenerateHash(command.Args.Get(0));
                OutputArgument OutArgEntity = new OutputArgument();
                int objHandle = Function.Call<int>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_FIRST_OBJECT")), OutArgEntity);
                int entityId = OutArgEntity.GetResult<int>();
                while (Function.Call<bool>((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("FIND_NEXT_OBJECT")), objHandle, OutArgEntity))
                {
                    // Vault doors
                    entityId = OutArgEntity.GetResult<int>();
                    if ((Function.Call<int>(Hash.GET_ENTITY_MODEL, entityId) == deleteHash) || (Function.Call<int>(Hash.GET_ENTITY_MODEL, entityId) == 2121050683) || (Function.Call<int>(Hash.GET_ENTITY_MODEL, entityId) == -63539571))
                    {
                        Log.ToChat($"´Deleting Index: {i}");
                        Function.Call(Hash.DELETE_OBJECT, entityId);
                    }
                    i++;
                }
                Function.Call((CitizenFX.Core.Native.Hash)((uint)CitizenFX.Core.Game.GenerateHash("END_FIND_OBJECT")), objHandle);
            }
            catch (Exception ex)
            {
                Log.Error($"ClientCommands DeleteObjects Error: {ex.Message}");
            }
        }

		[RequiresPermissionFlags( Permissions.Dev )]
		private async void SpawnTrain( Command cmd ) {
			Vector3 position = Game.PlayerPed.Position;
			bool direction = true;

			if( cmd.Args.Count == 4 ) {
				position = new Vector3( cmd.Args.GetFloat( 0 ), cmd.Args.GetFloat( 1 ), cmd.Args.GetFloat( 2 ) );
				direction = cmd.Args.GetBool( 3 );
			}
			else if( cmd.Args.Count == 1 ) {
				direction = cmd.Args.GetBool( 0 );
			}

			await Classes.Jobs.Transportation.Trains.TrainManager.SpawnTrain( position, direction );
		}

        //[RequiresPermissionFlags(Permissions.Dev)]
        private async void SetMapZoom(Command command)
        {
            int zoomLevel = 100;

            try
            {
                int setZoomLevel = command.Args.GetInt32(0);
                Log.ToChat($"Setting zoom level to {setZoomLevel}");
                zoomLevel = setZoomLevel;

                while (zoomLevel == setZoomLevel)
                {
                    Function.Call(Hash.SET_RADAR_ZOOM, setZoomLevel);
                    await BaseScript.Delay(0);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private async void Create3DThing(Command command)
        {
            try
            {
                while (true)
                {
                    await BaseScript.Delay(0);
                    if (ControlHelper.IsControlJustPressed(Control.Cover))
                    {
                        var mysf = new Scaleform("PlayerList");
                        if (mysf.IsValid)
                        {
                            Log.ToChat("Scaleform Valid");
                        }
                        else
                        {
                            Log.ToChat("Scaleform Invalid");
                        }
                        
                        if (mysf.IsLoaded)
                        {
                            Log.ToChat("Scaleform Loaded");
                            while (true)
                            {
                                //mysf.Render3D(Game.PlayerPed.GetOffsetPosition(new Vector3(-0.1f, 0, -0.4f)), -Game.PlayerPed.Rotation, 4f * Vector3.One);
                                mysf.Render3DAdditive(new Vector3(23.424f, -896.012f, 30.487f), new Vector3(0, 0, -160), 20f * Vector3.One);
                                await BaseScript.Delay(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private void CreateBlip(Command command)
        {
            try
            {
                BlipModel blip = new BlipModel();
                blip.coords = Game.PlayerPed.Position.ToArray();

                Log.ToChat($"Number X: {command.Args.Get(0)}");
                Log.ToChat($"Number Y: {command.Args.Get(1)}");
                int result;
                if (Int32.TryParse(command.Args.Get(0), out result))
                {
                    Log.ToChat($"Number A: {result}");
                    blip.sprite = result;
                }
                else
                {
                    Log.ToChat($"Number Ax: {result}");
                    BlipSprite spriteResult;
                    Enum.TryParse<BlipSprite>(command.Args.Get(0), true, out spriteResult);
                    blip.sprite = (int)spriteResult;
                }

                if (Int32.TryParse(command.Args.Get(1), out result))
                {
                    Log.ToChat($"Number B: {result}");
                    blip.color = result;
                }
                else
                {
                    Log.ToChat($"Number Bx: {result}");
                    BlipColor colorResult;
                    Enum.TryParse<BlipColor>(command.Args.Get(1), true, out colorResult);
                    blip.color = (int)colorResult;
                }

                blip.name = command.Args.ToString().Substring(command.Args.Get(0).Length + command.Args.Get(1).Length + 2);
                Log.ToChat($"Passing blip at ({blip.coords[0]}, {blip.coords[1]}, {blip.coords[2]}) with color '{blip.color}', name '{blip.name}' and icon '{blip.sprite}' to the server.");
                BaseScript.TriggerServerEvent("TempBlipModule.addBlip", Helpers.MsgPack.Serialize(blip));
            }
            catch (Exception ex)
            {
                Log.Error($"CreateBlip error: {ex.Message}");
            }
        }

        bool haveBlipsBeenInitialized = false;
        private void InitialBlipCreatorRequest()
        {
            try
            {
                if (haveBlipsBeenInitialized) return;
                haveBlipsBeenInitialized = true;
                Log.Debug($"Requesting serializedBlips");
                BaseScript.TriggerServerEvent("TempBlipModule.requestBlips");
            }
            catch (Exception ex)
            {
                Log.Error($"InitialBlipCreatorAdd error: {ex.Message}");
            }
        }

        private void InitialBlipCreatorExecute(string serializedBlipArray)
        {
            try
            {
                int i = 0;
                string[] SerializedBlips = Helpers.MsgPack.Deserialize<string[]>(serializedBlipArray);
                SerializedBlips.ToList().ForEach(b =>
                {
                    Log.Debug($"Creating blip {++i}");
                    BlipModel blipData = Helpers.MsgPack.Deserialize<BlipModel>(b);
                    Log.Debug($"Deserialized blip {++i}");
                    Blip blip = World.CreateBlip(blipData.coords.ToVector3());
                    blip.Sprite = (BlipSprite)blipData.sprite;
                    blip.Color = (BlipColor)blipData.color;
                    blip.Name = blipData.name;
                    Log.Debug($"Creating blip received from server at ({blipData.coords[0]}, {blipData.coords[1]}, {blipData.coords[2]}) with color '{(BlipColor)blipData.color}', name '{blipData.name}' and icon '{(BlipSprite)blipData.sprite}'.");
                });
            }
            catch (Exception ex)
            {
                Log.Error($"InitialBlipCreatorExecute error: {ex.Message}");
            }
        }

        private void PostSpawnBlipReceiver(string serializedBlip)
        {
            try
            {
                BlipModel blipData = Helpers.MsgPack.Deserialize<BlipModel>(serializedBlip);
                Blip blip = World.CreateBlip(blipData.coords.ToVector3());
                blip.Sprite = (BlipSprite)blipData.sprite;
                blip.Color = (BlipColor)blipData.color;
                blip.Name = blipData.name;
                Log.Debug($"Creating blip received from server at ({blipData.coords[0]}, {blipData.coords[1]}, {blipData.coords[2]}) with color '{(BlipColor)blipData.color}', name '{blipData.name}' and icon '{(BlipSprite)blipData.sprite}'.");
            }
            catch (Exception ex)
            {
                Log.Error($"PostSpawnBlipReceiver error: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private void TestChatMessageToMyself(Command command)
        {
            try
            {
                TriggerEventForPlayersModel data = new TriggerEventForPlayersModel(Game.Player.ServerId, "Dev.Print", "does this work?");
                var eventData = Helpers.MsgPack.Serialize(data);
                BaseScript.TriggerServerEvent("TriggerEventForPlayers", eventData);
            }
            catch (Exception ex)
            {
                Log.Error($"TestChatMessageToMyself error: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private void SetPlayerProp(Command command)
        {
            try
            {
                Function.Call(Hash.SET_PED_PROP_INDEX, Game.PlayerPed.Handle, command.Args.GetInt32(0), command.Args.GetInt32(1), command.Args.GetInt32(2), command.Args.GetBool(3));
            }
            catch (Exception ex)
            {
                Log.Error($"SetPlayerProp error: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private void SetPlayerComponent(Command command)
        {
            try
            {
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.PlayerPed.Handle, command.Args.GetInt32(0), command.Args.GetInt32(1), command.Args.GetInt32(2), command.Args.GetInt32(3));
            }
            catch (Exception ex)
            {
                Log.Error($"SetPlayerComponent error: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private async void SetPlayerModel(Command command)
        {
            try
            {
                Model model = new Model(command.Args.Get(0));
                if (!model.IsInCdImage)
                {
                    string inputName = command.Args.Get(0);
                    int skip;
                    if (command.Args.Count < 2)
                    {
                        skip = 0;
                    }
                    else
                    {
                        skip = command.Args.GetInt32(1);
                    }
                    var pedMatches = Enum.GetNames(typeof(PedHash)).Where(n => n.ToUpper().Contains(inputName.ToUpper()));
                    if (pedMatches.Count() == 0)
                    {
                        Log.ToChat("[DEVTOOLS] No model matches that name.");
                    }
                    int i = 0;
                    Log.ToChat($"[DEVTOOLS] '{inputName}' matches: {String.Join(", ", pedMatches.Select(v => $"({i++}) {v}"))}.");
                    string pedName = pedMatches.Skip(skip).Take(1).First();
                    PedHash pedHash = (PedHash)Enum.Parse(typeof(PedHash), pedName, true);
                    model = new Model(pedHash);
                }
                await model.Request(2000);
                await Game.Player.ChangeModel(model);
                await BaseScript.Delay(100);
                if ((PedHash)model.Hash == PedHash.FreemodeMale01 || (PedHash)model.Hash == PedHash.FreemodeFemale01)
                {
                    Function.Call(Hash.SET_PED_HEAD_BLEND_DATA, Game.PlayerPed.Handle, 0, 0, 0, 0, 0, 0, 0, 0, 0, false);
                }
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.PlayerPed.Handle, 1, 0, 0, 0);
            }
            catch (Exception ex)
            {
                Log.Error($"SetPlayerModel error: {ex.Message}");
            }
        }

        private void DevPrint(string msg)
        {
            try
            {
                Log.ToChat(msg);
            }
            catch (Exception ex)
            {
                Log.Error($"DevPrint error: {ex.Message}");
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="any"></param>
        public override void Handle(object any)
        {
            // Try to remove the "/dev" from the passed on command
            try
            {
                base.Handle(((Command)any).Args.ToString());
                return;
            }
            catch (Exception) { }

            // Fall back to default behaviour
            // ISSUE: My console is being spammed and I don't understand myself the purpose of the fallback?
            // But I don't doubt there's a purpose
            //base.Handle(any);
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void Effect(Command command)
        {
            try
            {
                if (command.Args.GetInt32(0) == -1)
                {
                    UI.StopScreenEffect();
                }
                else
                {
                    UI.StopScreenEffect();
                    UI.PlayScreenEffect(command.Args.GetInt32(0), true, 0, true);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.Effect exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void ToggleDevUI(Command command)
        {
            try
            {
                if (!command.Args.HasAny)
                {
                    IsDevUIEnabled = !IsDevUIEnabled;
                    return;
                }

                IsDevUIEnabled = command.Args.GetBool(0);
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.ToggleDevUI exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void AdjustEntityCoords(Command command)
        {
            try
            {
                Entity entity;
                if (command.Args.Get(0) != "t")
                {
                    entity = Entity.FromHandle(command.Args.GetInt32(0));
                }
                else
                {
                    entity = WorldProbe.CrosshairRaycast().HitEntity;
                }
                entity.Position = entity.Position + new Vector3(command.Args.GetFloat(1), command.Args.GetFloat(2), command.Args.GetFloat(3));
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.SetEntityCoords exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void AdjustEntityRot(Command command)
        {
            try
            {
                Entity entity;
                if (command.Args.Get(0) != "t")
                {
                    entity = Entity.FromHandle(command.Args.GetInt32(0));
                }
                else
                {
                    entity = WorldProbe.CrosshairRaycast().HitEntity;
                }
                entity.Rotation = entity.Rotation + new Vector3(command.Args.GetFloat(1), command.Args.GetFloat(2), command.Args.GetFloat(3));
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.SetEntityRot exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void ToggleEntityUI(Command command)
        {
            try
            {
                if (!command.Args.HasAny)
                {
                    IsDevEntityUIEnabled = !IsDevEntityUIEnabled;
                    return;
                }

                IsDevEntityUIEnabled = command.Args.GetBool(0);
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.ToggleEntityUI exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void ToggleKeyCodeTester(Command command)
        {
            try
            {
                if (!command.Args.HasAny)
                {
                    ControlCodeTester.Enabled = !ControlCodeTester.Enabled;
                    return;
                }

                ControlCodeTester.Enabled = command.Args.GetBool(0);
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.ToggleKeyCodeTester exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void LoadIPL(Command command)
        {
            try
            {
                //Log.ToChat($"{Function.Call<bool>(Hash.IS_VALID_INTERIOR, )}");
                Log.ToChat("Loading IPL...");
                Function.Call(Hash.REQUEST_IPL, command.Args.Get(0));
                Log.ToChat("Might have loaded IPL...");
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.LoadIPL exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void ActivateInteriorAtMyPosition(Command command)
        {
                try
                {
                Vector3 pos = Game.PlayerPed.Position;
                ////int interior = Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS_WITH_TYPE, pos.X, pos.Y, pos.Z, command.Args.Get(0));
                int interior = Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, pos.X, pos.Y, pos.Z);
                Log.ToChat($"{(interior > 0 ? $"You are inside the bounds of an interior with ID {interior}" : $"You are not inside an interior")}");
                //while (!Function.Call<bool>(Hash.IS_VALID_INTERIOR, interior))
                //{
                //    pos = Game.PlayerPed.Position;
                //    interior = Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, pos.X, pos.Y, pos.Z);
                //    Log.ToChat($"{interior} not yet 'valid', still checking");
                //    await BaseScript.Delay(0);
                //}

                //while (!Function.Call<bool>(Hash.IS_INTERIOR_READY, interior))
                //{
                //    Log.ToChat($"{interior} not yet ready, still loading");
                //    await BaseScript.Delay(0);
                //}
                Function.Call<bool>(Hash.SET_INTERIOR_ACTIVE, interior, true);
                Function.Call<bool>(Hash.UNPIN_INTERIOR, interior, true);
                //Function.Call(Hash.FORCE_ROOM_FOR_ENTITY, Game.PlayerPed.Handle, interior, Game.GenerateHash("mainRoom001"));
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.ActivateInteriorAtMyPosition exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void ActivateInteriorProps(Command command)
        {
            try
            {
                Vector3 pos = Game.PlayerPed.Position;
                int interior = Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, pos.X, pos.Y, pos.Z);
                Function.Call(Hash._ENABLE_INTERIOR_PROP, interior, command.Args.Get(0));
                Log.ToChat($"{command.Args.Get(0)} enabled? {Function.Call<bool>(Hash._IS_INTERIOR_PROP_ENABLED, command.Args.Get(0))}");

                Function.Call(Hash.REFRESH_INTERIOR, interior);
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.LoadIPL exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void DeactivateInteriorProps(Command command)
        {
            try
            {
                Vector3 pos = Game.PlayerPed.Position;
                int interior = Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, pos.X, pos.Y, pos.Z);
                Function.Call(Hash._DISABLE_INTERIOR_PROP, interior, command.Args.Get(0));
                Log.ToChat($"{command.Args.Get(0)} enabled? {Function.Call<bool>(Hash._IS_INTERIOR_PROP_ENABLED, command.Args.Get(0))}");
                Function.Call(Hash.REFRESH_INTERIOR, interior);
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.LoadIPL exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void UnloadIPL(Command command)
        {
            try
            {
                Function.Call(Hash.REMOVE_IPL, command.Args.Get(0));
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.UnloadIPL exception: {ex.Message}");
            }
        }

        [RequiresPermissionFlags(Permissions.Dev | Permissions.Admin)]
        private static void Teleport(Command command)
        {
            try
            {
                Log.ToChat($"{command.Args.GetFloat(0)} {command.Args.GetFloat(1)} {command.Args.GetFloat(2)}");
                if (Game.PlayerPed.IsInVehicle())
                {
                    Function.Call(Hash.SET_ENTITY_COORDS, Game.PlayerPed.CurrentVehicle, command.Args.GetFloat(0), command.Args.GetFloat(1), command.Args.GetFloat(2));
                }
                else
                {
                    Function.Call(Hash.SET_ENTITY_COORDS, Game.PlayerPed, command.Args.GetFloat(0), command.Args.GetFloat(1), command.Args.GetFloat(2));
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.Teleport exception: {ex.Message}");
            }
        }

        [RequiresPermissionFlags(Permissions.Dev | Permissions.Admin)]
        private static void TeleportToPlayer(Command command)
        {
            try
            {
                int playerId;
                bool isNumeric = int.TryParse(command.Args.Get(0), out playerId);
                CitizenFX.Core.Player targetPlayer;
                if (isNumeric)
                {
                    targetPlayer = new PlayerList().Where(p => p.ServerId == playerId).First();
                }
                else
                {
                    targetPlayer = new PlayerList().Where(p => p.Name.Contains(command.Args.Get(0))).First();
                }
                Log.ToChat($"Trying to teleport to {targetPlayer.Name} (#{targetPlayer.Handle}, server # {targetPlayer.ServerId})");
                Entity entity = Game.PlayerPed.IsInVehicle() ? (Entity)Game.PlayerPed.CurrentVehicle : (Entity)Game.PlayerPed;
                entity.Position = targetPlayer.Character.Position;

            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.Teleport exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void Godmode(Command command)
        {
            try
            {
                if (!command.Args.HasAny)
                {
                    Game.Player.IsInvincible = !Game.Player.IsInvincible;
                    return;
                }

                Game.Player.IsInvincible = command.Args.GetBool(0);
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.Godmode exception: {ex.Message}");
            }
        }

        // Basically a NoClip jump
        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void SkipWall(Command command)
        {
            try
            {
                // If you ever need to get below ground to get to e.g. 
                // The Comedy Club (NoClip currently can't get you
                // through all walls)
                if (command.Args.Count > 0 && command.Args.Get(0) == "down")
                {
                    Game.PlayerPed.PositionNoOffset = Game.PlayerPed.Position + new Vector3(0, 0, -5);
                }
                else
                {
                    Game.PlayerPed.PositionNoOffset = Game.PlayerPed.Position + NoClip.GetCurrentForwardVector() * 2f;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.SkipWall exception: {ex.Message}");
            }
        }

        // List all vehicle toggle mod names
        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void ListToggleMods(Command command)
        {
            try
            {
                string s = "";
                Enumerable.Range(0, 50).ToList().ForEach(i => { try { s += "," + Function.Call<object>(Hash.GET_MOD_SLOT_NAME, Game.PlayerPed.CurrentVehicle.Handle, i); } catch (Exception) { } });
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.SkipWall exception: {ex.Message}");
            }
        }

        [RequiresPermissionFlags(Permissions.Dev | Permissions.Admin)]
        private static async void SpawnVehicle(Command command)
        {
            try
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    var previousVehicle = Game.PlayerPed.CurrentVehicle;
                    Game.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);
                    await BaseScript.Delay(0);
                    previousVehicle.Delete();
                }

                Model model = new Model(command.Args.Get(0).ToUpper());
                if (!model.IsInCdImage)
                {
                    string inputName = command.Args.Get(0);
                    int skip;
                    if (command.Args.Count < 2)
                    {
                        skip = 0;
                    }
                    else
                    {
                        skip = command.Args.GetInt32(1);
                    }
                    var vehicleMatches = Enum.GetNames(typeof(VehicleHash)).Where(n => n.ToUpper().Contains(inputName.ToUpper()));
                    if (vehicleMatches.Count() == 0)
                    {
                        Log.ToChat("[DEVTOOLS] No vehicle matches that name.");
                    }
                    int i = 0;
                    Log.ToChat($"[DEVTOOLS] '{inputName}' matches: {String.Join(", ", vehicleMatches.Select(v => $"({i++}) {v}"))}.");
                    string vehicleName = vehicleMatches.Skip(skip).Take(1).First();
                    VehicleHash vehicleHash = (VehicleHash)Enum.Parse(typeof(VehicleHash), vehicleName, true);
                    model = new Model(vehicleHash);
                }
                await model.Request(4000); // On my slowest testing PC with low RAM it sometimes takes a bit longer to load a model (needs to swap or something)
                // It especially takes longer if you spawn a custom vehicle you have to download from the server
                CitizenFX.Core.Vehicle vehicle = await CitizenFX.Core.World.CreateVehicle(model, Game.PlayerPed.Position, Game.PlayerPed.Heading);
                Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                Function.Call(Hash.DECOR_SET_BOOL, vehicle.Handle, "Vehicle.PlayerTouched", true);
                Function.Call(Hash.DECOR_SET_BOOL, vehicle.Handle, "Vehicle.PlayerOwned", true);
                Function.Call(Hash.DECOR_SET_BOOL, vehicle.Handle, "Vehicle.Locked", false);
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.SpawnVehicle exception: {ex.Message}");
            }
        }

        [RequiresPermissionFlags(Permissions.Dev | Permissions.Admin | Permissions.EMS | Permissions.Police)]
        private static void RemoveVehicle(Command command)
        {
            try
            {
                RaycastResult detected = FamilyRP.Roleplay.Client.Helpers.GameHelper.Game.GetEntityInFrontOfPed(Game.PlayerPed);

                if (detected.DitHitEntity && Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, detected.HitEntity))
                {
                    Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, detected.HitEntity);
                    detected.HitEntity.MarkAsNoLongerNeeded();
                    Function.Call(Hash.DELETE_VEHICLE, detected.HitEntity);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.RemoveVehicle exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void Suicide(Command command)
        {
            try
            {
                Game.PlayerPed.Health = -1;
                Log.ToChat("Suicided");
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.Suicide exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void SetTestDecor(Command command)
        {
            try
            {
                var raycast = WorldProbe.CrosshairRaycast();
                if (raycast.DitHitEntity)
                {
                    if (command.Args.Count > 0)
                    {
                        Log.ToChat("[DEVTOOLS] Setting decor TEST");
                        Function.Call(Hash.DECOR_SET_INT, raycast.HitEntity.Handle, "TEST", command.Args.GetInt32(0));
                    }
                    else
                    {
                        Log.ToChat("[DEVTOOLS] Removing decor TEST");
                        Function.Call(Hash.DECOR_REMOVE, raycast.HitEntity.Handle, "TEST");
                    }
                }
                else
                {
                    Log.ToChat("[DEVTOOLS] No crosshair target");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.SetTestDecor exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void AddEntityBlip(Command command)
        {
            try
            {
                var raycast = WorldProbe.CrosshairRaycast();
                if (raycast.DitHitEntity)
                {
                    Log.ToChat("[DEVTOOLS] Adding blip for target");
                    Function.Call(Hash.ADD_BLIP_FOR_ENTITY, raycast.HitEntity.Handle);
                }
                else
                {
                    Log.ToChat("[DEVTOOLS] No crosshair target");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.SetTestDecor exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void CreateObject(Command command)
        {
            try
            {
                string inputName = command.Args.Get(0);
                int skip;
                if (command.Args.Count < 2)
                {
                    skip = 0;
                }
                else
                {
                    skip = command.Args.GetInt32(1);
                }
                var objectMatches = Enum.GetNames(typeof(ObjectHash)).Where(n => n.Contains(inputName));
                if (objectMatches.Count() == 0)
                {
                    Log.ToChat("[DEVTOOLS] No object matches that name.");
                }
                int i = 0;
                Log.ToChat($"[DEVTOOLS] '{inputName}' matches: {String.Join(", ", objectMatches.Take(20).Select(v => $"({i++}) {v}"))}.");
                Log.ToChat($"{(objectMatches.Count() > 20 ? $"In total {objectMatches.Count()} objects, only printed 20." : "")}");
                string objectName = objectMatches.Skip(skip).Take(1).First();
                ObjectHash objectHash = (ObjectHash)Enum.Parse(typeof(ObjectHash), objectName, true);
                Log.ToChat($"[DEVTOOLS] Creating object {objectName} (Hash {(int)objectHash} / 0x{objectHash:X})");
                ManipulateObject.CreateObject(objectHash);
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.SpawnObject exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void NetToEnt(Command command)
        {
            try
            {
                Log.ToChat($"[DEVTOOLS] {Function.Call<int>(Hash.NETWORK_GET_ENTITY_FROM_NETWORK_ID, command.Args.GetInt32(0))}");
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.NetToEnt exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void SetMissionEntity(Command command)
        {
            try
            {
                if (command.Args.Count < 1)
                {
                    Log.ToChat("[DEVTOOLS] Specify true or false");
                    return;
                }
                var raycast = WorldProbe.CrosshairRaycast();
                if (raycast.DitHitEntity)
                {
                    if (command.Args.GetBool(0))
                        Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, raycast.HitEntity, true, true);
                    else
                        Function.Call(Hash.SET_ENTITY_AS_NO_LONGER_NEEDED, raycast.HitEntity);
                }
                else
                {
                    Log.ToChat("[DEVTOOLS] No crosshair target");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.SetTestDecor exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void SetNetMigration(Command command)
        {
            try
            {
                var raycast = WorldProbe.CrosshairRaycast();
                if (raycast.DitHitEntity)
                {
                    //Function.Call(Hash.SET_NETWORK_ID_EXISTS_ON_ALL_MACHINES, raycast.HitEntity, true);
                    Function.Call(Hash.NETWORK_REQUEST_CONTROL_OF_NETWORK_ID, raycast.HitEntity);
                    //Function.Call(Hash.SET_NETWORK_ID_CAN_MIGRATE, raycast.HitEntity, true);
                }
                else
                {
                    Log.ToChat("[DEVTOOLS] No crosshair target");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.SetTestDecor exception: {ex.Message}");
            }
        }

        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void FindTestDecor(Command command)
        {
            try
            {
                var vehicles = new VehicleList().Where(e => Function.Call<bool>(Hash.DECOR_EXIST_ON, e, "TEST"));
                var objects = new ObjectList().Where(e => Function.Call<bool>(Hash.DECOR_EXIST_ON, e, "TEST"));
                var peds = new ObjectList().Where(e => Function.Call<bool>(Hash.DECOR_EXIST_ON, e, "TEST"));
                Log.ToChat($"[DEVTOOLS] Found {vehicles.Count()} vehicles, {objects.Count()} objects and {peds.Count()} peds with TEST decors [{String.Join(", ", vehicles.Concat(objects).Concat(peds).Select(e => Function.Call<int>(Hash.DECOR_GET_INT, e, "TEST")).ToList())}]");
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.FindTestDecor exception: {ex.Message}");
            }
        }

        [RequiresPermissionFlags(Permissions.Dev | Permissions.EMS | Permissions.Admin | Permissions.Police)]
        private static void Revive(Command command)
        {
            try
            {
                Game.PlayerPed.Health = Game.PlayerPed.MaxHealth;
                Function.Call(Hash.NETWORK_RESURRECT_LOCAL_PLAYER, Game.PlayerPed.Position, true, true, false);
                //Log.ToChat("Revived");
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.Revive exception: {ex.Message}");
            }
        }

        [RequiresPermissionFlags(Permissions.Dev | Permissions.Admin)]
        private static void Bring(Command command)
        {
            try
            {
                int playerId;
                bool isNumeric = int.TryParse(command.Args.Get(0), out playerId);
                CitizenFX.Core.Player targetPlayer;
                if (isNumeric)
                {
                    Log.ToChat($"Bringing by ID");
                    targetPlayer = new PlayerList().Where(p => p.ServerId == playerId).First();
                }
                else
                {
                    Log.ToChat($"Bringing by Name");
                    targetPlayer = new PlayerList().Where(p => p.Name.Contains(command.Args.Get(0))).First();
                }
                Log.ToChat($"Trying to bring {targetPlayer.Name} (#{targetPlayer.Handle}, server # {targetPlayer.ServerId})");
                BaseScript.TriggerServerEvent("Dev.Bring", targetPlayer.ServerId);
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.Bring exception: {ex.Message}");
            }
        }

        public static Task HandleBring(int destinationPlayerHandle)
        {
            try
            {
                CitizenFX.Core.Player destinationPlayer;
                destinationPlayer = new PlayerList().Where(p => p.ServerId == destinationPlayerHandle).First();
                Entity entity = Game.PlayerPed.IsInVehicle() ? (Entity)Game.PlayerPed.CurrentVehicle : (Entity)Game.PlayerPed;
                entity.PositionNoOffset = destinationPlayer.Character.Position;
                Log.ToChat($"Trying to go to bring requester {destinationPlayer.Name} (#{destinationPlayer.Handle}, server # {destinationPlayer.ServerId})");
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.HandleBring exception: {ex.Message}");
            }
            return Task.FromResult(0);
        }

        [RequiresPermissionFlags(Permissions.Dev | Permissions.Admin)]
        private static void TeleportToMarker(Command command)
        {
            try
            {
                float[] groundCheckHeight = new float[] { 100.0f, 150.0f, 50.0f, 0.0f, 200.0f, 250.0f, 300.0f, 350.0f, 400.0f, 450.0f, 500.0f, 550.0f, 600.0f, 650.0f, 700.0f, 750.0f, 800.0f };

                Vector3 WaypointPosition = World.WaypointPosition;
                Entity entity = Game.PlayerPed.IsInVehicle() ? (Entity)Game.PlayerPed.CurrentVehicle : (Entity)Game.PlayerPed;

                bool groundFound = false;

                // Since this function cannot Delay by itself:
                Task.Factory.StartNew(async () =>
                {
                    // We need this to not fall through ground for like 10 secs after TP
                    foreach (float h in groundCheckHeight)
                    {
                        // One check per tick:
                        await BaseScript.Delay(100);
                        entity.PositionNoOffset = new Vector3(WaypointPosition.X, WaypointPosition.Y, (float)h);
                        OutputArgument z = new OutputArgument();
                        if (Function.Call<bool>(Hash.GET_GROUND_Z_FOR_3D_COORD, WaypointPosition.X, WaypointPosition.Y, (float)h, z, false))
                        {
                            entity.PositionNoOffset = new Vector3(WaypointPosition.X, WaypointPosition.Y, z.GetResult<float>());
                            groundFound = true;
                            break;
                        }
                    }
                    if (groundFound == false)
                        await Task.Factory.StartNew(async () => { await BaseScript.Delay(1000); CitizenFX.Core.Game.PlayerPed.Weapons.Give(WeaponHash.Parachute, 1, true, true); });
                });
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.TeleportToMarker exception: {ex.Message}");
            }
        }

        // Just so I could test rappel by myself I implemented the below...
        // Replaces the pilot (you) with an AI pilot
        // Otherwise the helicopter crashes before you get the chance to try
        [RequiresPermissionFlags(Permissions.Dev | Permissions.Admin | Permissions.Police)]
        private static async void ReplacePilot(Command command)
        {
            try
            {
                Function.Call(Hash.TASK_WARP_PED_INTO_VEHICLE, Game.PlayerPed.Handle, Game.PlayerPed.CurrentVehicle.Handle, 1);
                Model model = new Model(PedHash.Pilot01SMM);
                await model.Request(9999);
                await BaseScript.Delay(500);
                int ped = Function.Call<int>(Hash.CREATE_PED_INSIDE_VEHICLE, Game.PlayerPed.CurrentVehicle.Handle, 4, model.Hash, -1, true, true);
                ped = Function.Call<int>(Hash.GET_PED_IN_VEHICLE_SEAT, Game.PlayerPed.CurrentVehicle.Handle, -1);
                await BaseScript.Delay(500);
                Function.Call(Hash.TASK_HELI_MISSION, ped, Game.PlayerPed.CurrentVehicle.Handle, 0, 0, Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, 25f, 4, 5f, -1f, -1f, 10, 10, 5f, 0);

            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.Suicide exception: {ex.Message}");
            }
        }

        [RequiresPermissionFlags(Permissions.Dev | Permissions.Admin)]
        private static void ToggleVehicleLock(Command command)
        {
            try
            {
                bool state = command.Args.HasAny ? command.Args.GetBool(0) : false;

                if (Game.PlayerPed.IsInVehicle())
                {
                    Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED_FOR_ALL_PLAYERS, Game.PlayerPed.CurrentVehicle, state);
                }
                else
                {
                    RaycastResult detected = Helpers.GameHelper.Game.GetEntityInFrontOfPed(Game.PlayerPed);

                    if (detected.DitHitEntity && Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, detected.HitEntity))
                    {
                        Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED_FOR_ALL_PLAYERS, detected.HitEntity, state);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.ToggleVehicleLock exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Sample function that allows for quickly setting value of some floats
        /// </summary>
        /// <param name="args"></param>
        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void SetFloat(Command command)
        {
            try
            {
                Log.ToChat($"Arg count: {command.Args.Count}");
                if (command.Args.Count < 2) throw new ArgumentException();
                string valueName = command.Args.Get(0);
                float newValue = command.Args.GetFloat(1);
                Log.ToChat($"Float value: {newValue}");
                switch (valueName)
                {
                    case "fuel":
                        FuelManager.vehicleFuel = newValue;
                        Function.Call(Hash._DECOR_SET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel", newValue);
                        break;
                    case "sprintmult":
                        Function.Call(Hash.SET_RUN_SPRINT_MULTIPLIER_FOR_PLAYER, Game.PlayerPed.Handle, newValue);
                        Log.ToChat($"Set sprint multiplier to {newValue}");
                        break;
                    case "setalldoorangles":
                        Array.ForEach((int[])Enum.GetValues(typeof(VehicleDoorIndex)), d =>
                        {
                            Function.Call(Hash.SET_VEHICLE_DOOR_CONTROL, Game.PlayerPed.CurrentVehicle.Handle, d, 3, 1.0);
                        });
                        Log.ToChat($"Set all door angles to {newValue}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.SetFloat exception: {ex.GetType().ToString()}");
            }
        }

        /// <summary>
        /// Sample function that allows for quickly setting value of some floats
        /// </summary>
        /// <param name="args"></param>
        //[RequiresPermissionFlags(Permissions.Dev)]
        private static void SetInt(Command command)
        {
            try
            {
                Log.ToChat($"Arg count: {command.Args.Count}");
                if (command.Args.Count < 2) throw new ArgumentException();
                string valueName = command.Args.Get(0);
                int newValue = command.Args.GetInt32(1);
                Log.ToChat($"Int value: {newValue}");
                switch (valueName)
                {
                    case "seat":
                        Function.Call(Hash.TASK_WARP_PED_INTO_VEHICLE, Game.PlayerPed.Handle, Game.PlayerPed.CurrentVehicle.Handle, newValue);
                        Log.ToChat($"Set you into seat ${Enum.GetName(typeof(VehicleSeat), newValue)} (#{newValue})");
                        break;
                    case "health":
                        Function.Call(Hash.SET_ENTITY_HEALTH, Game.PlayerPed.Handle, newValue);
                        Log.ToChat($"Set your player health to {newValue}");
                        break;
                    case "maxhealth":
                        Function.Call(Hash.SET_ENTITY_MAX_HEALTH, Game.PlayerPed.Handle, newValue);
                        Log.ToChat($"Set your player max health to {newValue}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.SetFloat exception: {ex.GetType().ToString()}");
            }
        }

        [RequiresPermissionFlags(Permissions.Dev | Permissions.Admin)]
        private async static void CreateFollowingPed(Command command)
        {
            try
            {
                Model model = new Model(command.Args.Get(0));
                if (!model.IsInCdImage)
                {
                    string inputName = command.Args.Get(0);
                    int skip;
                    if (command.Args.Count < 2)
                    {
                        skip = 0;
                    }
                    else
                    {
                        skip = command.Args.GetInt32(1);
                    }
                    var pedMatches = Enum.GetNames(typeof(PedHash)).Where(n => n.ToUpper().Contains(inputName.ToUpper()));
                    if (pedMatches.Count() == 0)
                    {
                        Log.ToChat("[DEVTOOLS] No ped matches that name.");
                    }
                    int i = 0;
                    Log.ToChat($"[DEVTOOLS] '{inputName}' matches: {String.Join(", ", pedMatches.Select(v => $"({i++}) {v}"))}.");
                    string pedName = pedMatches.Skip(skip).Take(1).First();
                    PedHash pedHash = (PedHash)Enum.Parse(typeof(PedHash), pedName, true);
                    model = new Model(pedHash);
                }
                await model.Request(2000); // On my slowest testing PC with low RAM it sometimes takes a bit longer to load a model (needs to swap or something)
                                           // Uncomment for madness
                                           //int j = 300;
                                           //while(j > 0)
                                           //{ 
                CitizenFX.Core.Ped ped = await CitizenFX.Core.World.CreatePed(model, Game.PlayerPed.Position + new Vector3(0, 3f, 0));
                ped.Task.ClearAllImmediately();
                //await BaseScript.Delay(100);
                //Function.Call(Hash.TASK_FOLLOW_TO_OFFSET_OF_ENTITY, ped.Handle, Game.PlayerPed.Handle, 1f, 1f, 0f, 1f, -1, 5f, true);
                await BaseScript.Delay(500); Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, 46, true);

                ped.Task.FightAgainst(Game.PlayerPed, 10000);
                //  j--;
                //}
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.SetFloat exception: {ex.GetType().ToString()}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        [RequiresPermissionFlags(Permissions.Dev | Permissions.Admin | Permissions.EMS | Permissions.Police | Permissions.FireDept)]
        private static void FixVehicle(Command command)
        {
            try
            {
                VehicleDamage.Fix();
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.FixVehicle exception: {ex.Message}");
            }
        }


        /// <summary>
        /// To be changed to a general function for all doors and redone completely
        /// Just testing repair mechanic
        /// </summary>
        /// <param name="args"></param>
        private static float openCloseHoodRange = 4.0f;
        private static void OpenCloseHood(Command command)
        {
            try
            {
                CitizenFX.Core.Vehicle vehicle;
                if ((Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed))
                {
                    vehicle = Game.PlayerPed.CurrentVehicle;
                }
                else
                {
                    vehicle = WorldProbe.GetVehicleInFrontOfPlayer(3f);
                    IEnumerable<CitizenFX.Core.Vehicle> vehicles;
                    if (vehicle == null || !vehicle.Exists())
                    {
                        vehicles = new VehicleList().Select(v => new CitizenFX.Core.Vehicle(v)).Where(v => v.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(openCloseHoodRange * 2, 2));
                        if (vehicles.Count() > 0)
                        {
                            vehicle = vehicles.First();
                        }
                        else
                        {
                            Log.ToChat("Too far away from the hood to open it.");
                            return;
                        }
                    }
                }

                if (vehicle.Doors[VehicleDoorIndex.Hood].IsOpen) vehicle.Doors[VehicleDoorIndex.Hood].Close(); else vehicle.Doors[VehicleDoorIndex.Hood].Open();
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.OpenCloseHood exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Just a simple toggle
        /// No bool right now
        /// </summary>
        /// <param name="args"></param>
        [RequiresPermissionFlags(Permissions.Dev | Permissions.Admin)]
        private static void ToggleDevToolsBind(Command command)
        {
            try
            {
                NoClip.DevToolsBindEnabled = !NoClip.DevToolsBindEnabled;
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.ToggleNoClip exception: {ex.Message}");
            }
        }

        override protected void OnCommandNotFound(Command command)
        {
            BaseScript.TriggerServerEvent("Chat.DevCommandEntered", command.CommandString);
            Log.Info($"Command '{command.CommandString}' not found on client, passing to server");
        }
    }
}