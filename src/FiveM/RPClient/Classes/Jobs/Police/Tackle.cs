using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Linq;
using System.Threading.Tasks;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.SharedModels.PoliceEventModels;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.Client.Classes.Actions.Jobs.Police;

namespace FamilyRP.Roleplay.Client.Classes.Actions
{
    static class Tackle
    {
        static bool isPolice = true;
        static bool isPoliceDog = false;
        static int ragdollTimeSource = 1500;
        static int ragdollTimeTarget = 3000;
        static int ragdollTimeTargetAI = 4000;
        static float AoeRadius = 2.125f;
        static float lagCompensationFactor = 4;
        static PlayerList playerList = new PlayerList();

        static public void Init()
        {
            Client.GetInstance().RegisterEventHandler("Police.Tackle", new Func<string, Task>(TackleHandler));
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static public Task TackleHandler(string serializedArguments)
        {
            TackleEvent arguments = Helpers.MsgPack.Deserialize<TackleEvent>(serializedArguments);
            //Log.ToChat($"In tackle event handler {playerList[arguments.SourceId].Character.Position.DistanceToSquared(CitizenFX.Core.Game.PlayerPed.Position).ToString()}");
            if(playerList[arguments.SourceId].Character.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(lagCompensationFactor*AoeRadius, 2))
            { 
                Function.Call(Hash.SET_PED_TO_RAGDOLL, Game.PlayerPed.Handle, ragdollTimeTarget, 2*ragdollTimeTarget, 0, true, true, true);

                if(arguments.IsSourcePoliceDog)
                {
                    BaseScript.Delay(1000);
                    Game.PlayerPed.Kill();
                }
            }
            return Task.FromResult(0);
        }

        static public Task OnTick()
        {
            if(ControlHelper.IsControlJustPressed(Control.Context, true, Enums.Controls.ControlModifier.Any)
                && isPolice
                && !Game.PlayerPed.IsInVehicle()
                && Function.Call<float>(Hash.GET_ENTITY_SPEED, Game.PlayerPed.Handle) > 4.7
                /*&& Arrest.attachedPlayer == null*/)
            {
                var playerTargets = playerList.Where(p => p.Handle != Game.Player.Handle && p.Character.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(AoeRadius, 2)).ToList();

                var pedTargets = new PedList()
                    .Select(i => new Ped(i))
                    .Where(p => 
                        Game.PlayerPed.Handle != p.Handle && !p.IsDead && p.IsOnFoot
                        && !p.IsProne && !p.IsSwimming && p.IsUpright && !p.IsInVehicle() && !p.IsRagdoll 
                        && p.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(AoeRadius, 2)).ToList();

                if (playerTargets.Count > 0 || pedTargets.Count > 0)
                {
                    // No fun allowed, players and peds both ragdoll
                    Function.Call(Hash.SET_PED_TO_RAGDOLL, Game.PlayerPed.Handle, ragdollTimeSource, 2 * ragdollTimeSource, 0, true, true, true);

                    if(playerTargets.Count > 0)
                    {
                        BaseScript.TriggerServerEvent("Police.Tackle", Helpers.MsgPack.Serialize(new TackleEvent(playerTargets.Select(p => p.ServerId).ToList(), isPoliceDog)));
                    }
                    else
                    {
                        pedTargets.ToList().ForEachAsync(async p => { Function.Call(Hash.SET_PED_TO_RAGDOLL, p.Handle, ragdollTimeTargetAI, 2 * ragdollTimeTargetAI, 0, true, true, true); await BaseScript.Delay(2 * ragdollTimeTargetAI); p.Task.ClearAllImmediately(); p.Task.ReactAndFlee(Game.PlayerPed); });
                    }
                }
            }
            return Task.FromResult(0);
        }
    }
}
