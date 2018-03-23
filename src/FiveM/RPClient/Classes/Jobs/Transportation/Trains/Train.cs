using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.Helpers;

namespace FamilyRP.Roleplay.Client.Classes.Jobs.Transportation.Trains
{
    class DoorState
    {
        public List<bool> State;
        public string Message;
        public DoorState(List<bool> State, string Message)
        {
            this.State = State;
            this.Message = Message;
        }
    }

    class Carriage
    {
        public CitizenFX.Core.Vehicle Handle;
        public int CarriageNumber;

        public Carriage(CitizenFX.Core.Vehicle Handle, int CarriageNumber)
        {
            this.Handle = Handle;
            this.CarriageNumber = CarriageNumber;
        }


        public void Derail() { }
    }

    public class CarriageList : IEnumerable<CitizenFX.Core.Vehicle>
    {
        public CitizenFX.Core.Vehicle Train;

        public IEnumerator<CitizenFX.Core.Vehicle> GetEnumerator()
        {
            int i = 0;
            int handle = Function.Call<int>(Hash.GET_TRAIN_CARRIAGE, Train.Handle, i);
            CitizenFX.Core.Vehicle Carriage;
            while (handle != 0)
            {
                Carriage = (CitizenFX.Core.Vehicle)Entity.FromHandle(handle);
                yield return Carriage;
                i++;
                handle = Function.Call<int>(Hash.GET_TRAIN_CARRIAGE, Train.Handle, i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class Train
    {
        public virtual bool IsMetroTrain { get; protected set; } = false;
        public bool IsPlayerDriver { get; protected set; } = true;
        public bool IsDerailed = false;
        public virtual List<int> SpawnableVersions { get; protected set; } = new List<int>() { 2, 3, 4 };
        public CitizenFX.Core.Vehicle TrainHandle;
        internal float speed;
        public bool Derailed;
        public float Speed { get { return speed; } set { speed = value; Function.Call(Hash.SET_TRAIN_SPEED, TrainHandle.Handle, value); Function.Call(Hash.SET_TRAIN_CRUISE_SPEED, TrainHandle.Handle, value); } }
        public int numCarriages { get { int i = 0; while (Entity.FromHandle(Function.Call<int>(Hash.GET_TRAIN_CARRIAGE, TrainHandle.Handle, i)).Exists()) { i++; } return i; } }

        public Carriage this[int carriageNumber] => new Carriage((CitizenFX.Core.Vehicle) Entity.FromHandle(Function.Call<int>(Hash.GET_TRAIN_CARRIAGE, TrainHandle.Handle, carriageNumber)), carriageNumber);

        public CarriageList Carriages { get { return new CarriageList { Train = this.TrainHandle }; } }

        public Train(Vector3 position, bool direction)
        {
            int spawnVersion = SpawnableVersions[new Random().Next(SpawnableVersions.Count)];
            TrainHandle = (CitizenFX.Core.Vehicle)Entity.FromHandle(Function.Call<int>(Hash.CREATE_MISSION_TRAIN, spawnVersion, position.X, position.Y, position.Z, direction));
            TrainHandle.IsPersistent = true;
            Speed = 0;
        }

        public Train(CitizenFX.Core.Vehicle handle)
        {
            TrainHandle = handle;
            Speed = 0;
        }

        protected int oldTime = -1;
        protected int currentTime = -1;
        protected float dt = -1;
        const float SlowStartThreshold = 6f;

        public void UpdateTime()
        {
            oldTime = currentTime;
            currentTime = Function.Call<int>(Hash.GET_GAME_TIMER);
            if (oldTime == -1) oldTime = currentTime;
            dt = (currentTime - oldTime) / 1000f;
        }

        public float AccelerationMultiplier()
        {
            return Math.Abs(Speed) < SlowStartThreshold ? Math.Abs(Speed) / (float) Math.Pow(SlowStartThreshold, 1.6) : 1f;
        }

        public void Accelerate()
        {
            if (Speed >= 0)
            {
                Speed = Speed + AccelerationMultiplier() * (15 * (float)Math.Exp(-Speed / 20) * dt) + 0.2f * dt;
            }
            else
            {
                Speed = Speed * (float)Math.Pow(0.8, dt) + 3f * dt;
            }
        }

        public void Deccelerate()
        {
            if (Speed <= 0)
            {
                Speed = Speed + AccelerationMultiplier() * (-15 * (float)Math.Exp(-Speed / 20) * dt) - 0.2f * dt;
            }
            else
            {
                Speed = Speed * (float)Math.Pow(0.8, dt) - 3f * dt;
            }
        }

        public void Brake()
        {
            if (Speed > 0)
            {
                Speed = (float)Math.Max(Speed * Math.Pow(0.8, dt) - 3f * dt, 0);
            }
            else if (Speed < 0)
            {
                Speed = (float)Math.Min(Speed * Math.Pow(0.8, dt) + 3f * dt, 0);
            }
        }

        public void ApplyFriction()
        {
            if(Speed > SlowStartThreshold)
                Speed *= (float)Math.Pow(0.96, dt);
        }

        public bool ShouldDerail()
        {
            if (Speed > 50f) return true;
            return false;
        }

        public void Delete() { }
        public virtual async Task Derail()
        {
            Carriages.ToList().ForEach(async c =>
            {
                try
                {
                    Dictionary<int, Ped> occupants = new Dictionary<int, Ped>();

                    for (int seat = -1, seats = c.PassengerCapacity; seat < seats; seat++)
                    {
                        if (!c.IsSeatFree((VehicleSeat)seat))
                        {
                            occupants[seat] = c.GetPedOnSeat((VehicleSeat)seat);
                        }
                    }

                    Vector3 Rotation = c.Rotation;
                    Vector3 Position = c.Position;
                    float Heading = c.Heading;
                    Vector3 Velocity = c.Velocity;
                    Model Model = c.Model;

                    c.Delete();
                    c = await World.CreateVehicle(Model, Position + new Vector3(0, 0, 1), Heading);
                    c.Rotation = Rotation;
                    c.Velocity = 1.1f * Velocity + new Vector3(0, 0, 2);
                    Function.Call(Hash.SET_ENTITY_DYNAMIC, c.Handle, true);
                    NativeWrappers.SetObjectPhysicsParams(c, 1000f, 1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, 6.28f, 1f);

                    occupants.ToList().ForEach(o => o.Value.Task.WarpIntoVehicle(c, (VehicleSeat)o.Key));

                    if (IsMetroTrain)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, c.Handle, i, true, true);
                            Function.Call(Hash.SET_VEHICLE_DOOR_BROKEN, c.Handle, i, false);
                        }
                    }

                    await BaseScript.Delay(50);
                }
                catch (Exception ex)
                {
                    Log.Error($"Train Carriage Derail Error: {ex.Message}");
                }
            });
            Derailed = true;
        }

        public void Enter(bool driver = true)
        {
            Function.Call(Hash.SET_MOBILE_RADIO_ENABLED_DURING_GAMEPLAY, true);
            Game.PlayerPed.Task.EnterVehicle(TrainHandle, VehicleSeat.Driver, -1, 1f, 16);
            IsPlayerDriver = driver;
        }

        public bool Exit()
        {
            if(Speed == 0)
            {
                Game.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);
                Function.Call(Hash.SET_MOBILE_RADIO_ENABLED_DURING_GAMEPLAY, false);
                return true;
            }
            else
            {
                Log.ToChat("You need to come to a complete stop before exiting the train.");
                return false;
            }
        }
    }

    class MetroTrain : Train
    {
        public override bool IsMetroTrain { get; protected set; } = true;
        public override List<int> SpawnableVersions { get; protected set; } = new List<int>() { 24 };
        public string CurrentDoorState = "both";

        public MetroTrain(Vector3 position, bool direction) : base(position, direction) { }
        public MetroTrain(CitizenFX.Core.Vehicle handle) : base(handle) { }

        public Dictionary<string, DoorState> DoorStates = new Dictionary<string, DoorState>()
        {
            ["left"] = new DoorState(new List<bool> { true, false, true, false }, "Left side doors open."),
            ["right"] = new DoorState(new List<bool> { false, true, false, true }, "Right side doors open."),
            ["both"] = new DoorState(new List<bool> { true, true, true, true }, "All doors open."),
            ["none"] = new DoorState(new List<bool> { false, false, false, false }, "All doors closed.")
        };

        public void ToggleDoors()
        {
            CurrentDoorState = DoorStates.Keys.ElementAt((DoorStates.Keys.ToList().IndexOf(CurrentDoorState) + 1) % DoorStates.Count);
            Log.ToChat(DoorStates[CurrentDoorState].Message);
        }

        public void RefreshDoors()
        {
            int carriage = 0;
            Carriages.ToList().ForEach(c =>
            {
                c.Repair();
                bool oddCart = carriage % 2 == 1;
                int i = 0;
                DoorStates[CurrentDoorState].State.ForEach(s =>
                {
                    if (s == true)
                    {
                        Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, c.Handle, oddCart ? 3 - i : i, true, true);
                        Function.Call(Hash.SET_VEHICLE_DOOR_BROKEN, c.Handle, oddCart ? 3 - i : i, true);
                    }
                    else
                    {
                        Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, c.Handle, oddCart ? 3-i : i, false);
                    }
                    i++;
                });
                carriage++;
            });
        }

        public async void Derail()
        {
            base.Derail();
        }
    }

    static class TrainManager
    {
        public static List<VehicleHash> TrainModels = new List<VehicleHash>()
        {
            VehicleHash.Freight,
            VehicleHash.FreightCar,
            VehicleHash.FreightCont1,
            VehicleHash.FreightCont2,
            VehicleHash.FreightGrain,
            VehicleHash.FreightTrailer,
            VehicleHash.TankerCar,
            (VehicleHash) Game.GenerateHash("metrotrain")
        };

        public static List<VehicleHash> TrainLocomotiveModels = new List<VehicleHash>()
        {
            VehicleHash.Freight,
            (VehicleHash) Game.GenerateHash("metrotrain")
        };

        public static Train train = null;
        private static bool hasTrainJustSpawned;

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

		public static async Task SpawnTrain( bool direction = true ) {
			await SpawnTrain( Game.PlayerPed.Position, direction );
		}

		public static async Task SpawnTrain( Vector3 position, bool direction ) {
			if( Game.PlayerPed.IsInVehicle() && TrainLocomotiveModels.Contains( (VehicleHash)Game.PlayerPed.CurrentVehicle.Model.Hash ) ) {
				train = (VehicleHash)Game.PlayerPed.CurrentVehicle.Model.Hash == VehicleHash.Freight ? new Train( Game.PlayerPed.CurrentVehicle ) : new MetroTrain( Game.PlayerPed.CurrentVehicle );
				return;
			}

			await TrainModels.ForEachAsync( async m => {
				await new Model( m ).Request( 3000 );
				Log.ToChat( $"Loaded {m}." );
			} );

			hasTrainJustSpawned = true;
			train = new MetroTrain( position, direction );

			Function.Call( Hash.DECOR_SET_BOOL, train.TrainHandle.Handle, "Vehicle.Locked", false );
			Function.Call( Hash.DECOR_SET_BOOL, train.TrainHandle.Handle, "Vehicle.PlayerTouched", true );
			Function.Call( Hash.DECOR_SET_BOOL, train.TrainHandle.Handle, "Vehicle.PlayerOwned", true );

			train.Enter();

			// Hack until I can figure out why setting the handle is not working without it
			train = (VehicleHash)Game.PlayerPed.CurrentVehicle.Model.Hash == VehicleHash.Freight ? new Train( Game.PlayerPed.CurrentVehicle ) : new MetroTrain( Game.PlayerPed.CurrentVehicle );
			hasTrainJustSpawned = false;
		}

        static private async Task OnTick()
        {
            try
            {
                CitizenFX.Core.Vehicle vehicle;
                if (ControlHelper.IsControlJustPressed(Control.Enter) && !Game.PlayerPed.IsInVehicle() && (vehicle = WorldProbe.GetVehicleInFrontOfPlayer()).Exists() && TrainLocomotiveModels.Contains((VehicleHash)vehicle.Model.Hash))
                {
                    train = (VehicleHash)vehicle.Model.Hash == VehicleHash.Freight ? new Train(Game.PlayerPed.CurrentVehicle) : new MetroTrain(Game.PlayerPed.CurrentVehicle);
                    train.Enter(!train.TrainHandle.Driver.Exists());
                    return;
                }

                if (train != null)
                {
                    if(!Game.PlayerPed.IsInVehicle() && !hasTrainJustSpawned || Game.PlayerPed.IsInVehicle() && !TrainLocomotiveModels.Contains(Game.PlayerPed.CurrentVehicle.Model))
                    {
                        train = null;
                        return;
                    }
                    train.UpdateTime();
                    if (train.IsMetroTrain) ((MetroTrain)train).RefreshDoors();
                    if (ControlHelper.IsControlJustPressed(Control.Enter))
                    {
                        if(train.Exit()) train = null;
                    }
                    else if (train.IsPlayerDriver)
                    {
                        if (ControlHelper.IsControlPressed(Control.VehicleAccelerate))
                        {
                            train.Accelerate();
                        }
                        else if (ControlHelper.IsControlPressed(Control.VehicleBrake))
                        {
                            train.Deccelerate();
                        }
                        else if (ControlHelper.IsControlPressed(Control.Context, true, Enums.Controls.ControlModifier.Alt))
                        {
                            await train.Derail();
                        }
                        else if (ControlHelper.IsControlPressed(Control.VehicleHandbrake))
                        {
                            train.Brake();
                        }
                        else if (train.IsMetroTrain && ControlHelper.IsControlJustPressed(Control.CinematicSlowMo))
                        {
                            ((MetroTrain)train).ToggleDoors();
                        }
                        train.ApplyFriction();
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error($"Train OnTick Error: {ex.Message}");
            }
            return;
        }
    }
}