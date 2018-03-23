using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Jobs
{
    static class Hunting
    {
        const float lootableDistance = 3f;

        // TODO: Shorten down significantly or something like that
        // Make sure to just comment out, don't remove
        static List<PedHash> HuntableModels = new List<PedHash>()
        {
            PedHash.Coyote,
            PedHash.Chimp,
            PedHash.ChickenHawk,
            PedHash.Hen,
            PedHash.Boar,
            PedHash.Chop,
            PedHash.Cormorant,
            PedHash.Cow,
            PedHash.Crow,
            PedHash.Deer,
            PedHash.Fish,
            PedHash.Husky,
            PedHash.MountainLion,
            PedHash.Pig,
            PedHash.Pigeon,
            PedHash.Rat,
            PedHash.Retriever,
            PedHash.Rhesus,
            PedHash.Rottweiler,
            PedHash.Seagull,
            PedHash.TigerShark,
            PedHash.Shepherd,
            PedHash.HammerShark,
            PedHash.Rabbit,
            PedHash.Cat,
            PedHash.KillerWhale
        };

        static PedList PedList = new PedList();
        static List<Ped> NearbyAnimals = new List<Ped>();

        static public void Init()
        {
            Function.Call(Hash.DECOR_REGISTER, "Animal.Looted");
            Client.GetInstance().RegisterTickHandler(OnTick);
            PeriodicRefresh();
        }

        static internal Task OnTick()
        {
            NearbyAnimals.Where(a => a.IsDead && !Function.Call<bool>(Hash.DECOR_EXIST_ON, a.Handle, "Animal.Looted")).ToList().ForEach(a =>
            {
                World.DrawMarker(MarkerType.ChevronUpx3, a.Position + new Vector3(0, 0, 1f), Vector3.Zero, new Vector3(180f, 0, 0), 1.0f * Vector3.One, Color.FromArgb(255, 255, 0, 0), true);
            });

            if (ControlHelper.IsControlJustPressed(Control.Context))
            {
                var LootableAnimals = NearbyAnimals.Where(a => a.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(lootableDistance, 2) && a.IsDead && !Function.Call<bool>(Hash.DECOR_EXIST_ON, a.Handle, "Animal.Looted"));
                if (LootableAnimals.Count() > 0)
                {
                    Log.ToChat($"Looted a {(PedHash)LootableAnimals.First().Model.Hash}.");
                    Function.Call(Hash.DECOR_SET_INT, LootableAnimals.First(), "Animal.Looted", 1);
                }
            }
            return Task.FromResult(0);
        }

        static internal async void PeriodicRefresh()
        {
            while (true)
            {
                NearbyAnimals = PedList.Select(p => new Ped(p)).Where(p => HuntableModels.Contains(p.Model)).ToList();
                await BaseScript.Delay(5000);
            }
        }
    }
}
