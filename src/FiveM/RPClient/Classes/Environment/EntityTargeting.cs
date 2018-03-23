//using CitizenFX.Core;
//using FamilyRP.Roleplay.Client.Helpers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FamilyRP.Roleplay.Client.Classes.Environment
//{
//    // I had a cool idea for targeting entities for keyboard-bound activities
//    // where you could use alt to see the player (or vehicle player is in, or e.g your own police vehicle) closest to any point of your crosshair raycast line
//    // and that target would be used for e.g. your cuff keybind
//    // This way your aim would not have to be exact, just the closest
//    class EntityTargeting
//    {
//        const float filterDistance = 60f;
//        const float maxDistanceFromRay = 5f;
//        const float rayRange = 5f;

//        internal Vector3 ChevronOffset = new Vector3(0, 0, 0.2f);
//        System.Drawing.Color ChevronColor = System.Drawing.Color.FromArgb(255, 0, 0, 225);
//        Vector3 ChevronDirection = new Vector3(0, 0, 0);
//        Vector3 ChevronRotation = new Vector3(180f, 0f, 0f);
//        Vector3 ChevronScale = 0.7f * new Vector3(1.0f, 1.0f, 1.0f);

//        PlayerList playerList = new PlayerList();

//        public EntityTargeting()
//        {
//            //Tick += OnTick;
//        }

//        public Task OnTick()
//        {
//            if(ControlHelper.IsControlPressed(Control.CharacterWheel, true, Enums.Controls.ControlModifier.Alt))
//            {
//                DrawSelector();
//            }
//            return Task.FromResult(0);
//        }

//        // TODO: Add support for vehicles
//        public Task DrawSelector()
//        {
//            CitizenFX.Core.Player closest;
//            if((closest = ClosestPlayerToCrosshair()) != null)
//            {
//                World.DrawMarker(MarkerType.ChevronUpx1, closest.Character.Position + ChevronOffset, ChevronDirection, ChevronRotation, ChevronScale, ChevronColor, false, true);
//                // DrawText
//            }
//            return Task.FromResult(0);
//        }

//        public CitizenFX.Core.Player ClosestPlayerToCrosshair()
//        {
//            Vector3 start = GameplayCamera.Position;
//            Vector3 end = GameplayCamera.GetOffsetPosition(new Vector3(0f, rayRange, 0f));
//            Dictionary<int, Vector3> closest = new Dictionary<int, Vector3>();
//            var orderedPlayers = playerList
//                .Where(p => p.Character.Position.DistanceToSquared(CitizenFX.Core.Game.PlayerPed.Position) < Math.Pow(filterDistance, 2))
//                .OrderBy(p => { closest.Add(p.Character.Handle, WorldProbe.CalculateClosestPointOnLine(start, end, p.Character.Position)); return closest[p.Character.Handle]; });

//            if(orderedPlayers.Count() > 0)
//            {
//                var filteredPlayers = orderedPlayers.Where(p => closest[p.Character.Handle].DistanceToSquared(p.Character.Position) < Math.Pow(maxDistanceFromRay, 2));
//                if(filteredPlayers.Count() > 0)
//                { 
//                    return filteredPlayers.First();
//                }
//            }

//            return null;
//        }
//    }
//}
