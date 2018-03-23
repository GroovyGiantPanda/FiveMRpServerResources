using CitizenFX.Core;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Environment
{
    static class AquaticSpawner
    {
        // TODO: Weighted spawn probabilities
        static List<PedHash> AquaticHashes = new List<PedHash>()
        {
            PedHash.TigerShark,
            PedHash.HammerShark,
            PedHash.KillerWhale
        };
        static Vector3 Center = new Vector3(-2900, -1970, 0);
        static float Radius = 100f;
        static int MinimumCount = 50;
        static float SpawnDepth = -3f;
        static float AggroDistanceOnSpawn = 200f;

        static PedList PedList = new PedList();
        static public async void Init()
        {
            // Because it seems to throw a fit if you do it at first load. Hm.
            await BaseScript.Delay(600000);
            PeriodicCheck();

            // Creates intriguing colored map area in the middle of the ocean.
            Blip blip = World.CreateBlip(Center, Radius);
            blip.Color = BlipColor.Red;
        }

        static private async void PeriodicCheck()
        {
            while (true)
            {
                if(Game.PlayerPed.Position.DistanceToSquared(Center) < Math.Pow(Radius, 2))
                { 
                    var AquaticLifeList = PedList.Select(p => new Ped(p)).Where(p => p.Exists() && AquaticHashes.Contains((PedHash)p.Model.Hash) && p.Position.DistanceToSquared(Center) < Math.Pow(Radius, 2));
                    //Log.ToChat(AquaticLifeList.Count().ToString());
                    if (AquaticLifeList.Count() < MinimumCount)
                    {
                        Vector2 SpawnLocation = GetRandomPointAroundPlayer();
                        Ped Ped = await World.CreatePed(AquaticHashes[new Random().Next(0, AquaticHashes.Count - 1)], new Vector3(SpawnLocation.X, SpawnLocation.Y, SpawnDepth));
                        //Ped.Task.FightAgainst(Game.PlayerPed);
                    }
                }
                await BaseScript.Delay(1000);
            }
        }

        static public Vector2 GetRandomPointAroundPlayer()
        {
            Random Random = new Random();
            float distance = (float)Random.NextDouble() * 10f;
            double angleInRadians = Random.Next(360) / (2 * Math.PI);

            float x = (float)(distance * Math.Cos(angleInRadians));
            float y = (float)(distance * Math.Sin(angleInRadians));
            return new Vector2(Game.PlayerPed.Position.X + x, Game.PlayerPed.Position.Y + y);
        }
    }
}
