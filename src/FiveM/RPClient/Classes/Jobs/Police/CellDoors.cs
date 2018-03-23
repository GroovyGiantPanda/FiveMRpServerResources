using CitizenFX.Core;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.Server.Enums;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Jobs.Police
{
    static class CellDoors
    {
        static Vector3 CellDoorArea = new Vector3(461.8f, -997.7f, 25.1f);
        static Entity NearbyCellDoor = null;
        static float SearchRadius = 30f;
        static float LockRadius = 2f;
        static List<Vector3> CellDoorPositions = new List<Vector3>()
        {
            new Vector3(461.8f, -1001.3f, 25.1f),
            new Vector3(461.8f, -997.7f, 25.1f),
            new Vector3(461.8f, -0.1f, -2.3f)
        };
        static int SlowRefreshRate = 10000;
        static ObjectHash CellDoorHash = ObjectHash.v_ilev_ph_cellgate;

        static ObjectList ObjectList = new ObjectList();
        static public bool OnTickActive = false;
        static MenuItemCheckbox menuItem = new MenuItemCheckbox
        {
            Title = "Lock Cell Door",
            OnActivate = (state, item) =>
            {
                if(NearbyCellDoor != null) NearbyCellDoor.IsPositionFrozen = state;
            }
        };
        private static int QuickRefreshRate = 50;

        static public void Init()
        {
            PeriodicRefresh();
            QuickerPeriodicRefresh();
            InteractionListMenu.RegisterInteractionMenuItem(menuItem, () => NearbyCellDoor != null, 1010);
        }

        static private async void PeriodicRefresh()
        {
            while (true)
            {
                if (false/*Client._Exports["FiveMDefaults"].hasSpawned()*/)
                {
                    if (!OnTickActive && Game.PlayerPed.Position.DistanceToSquared(CellDoorArea) < Math.Pow(SearchRadius, 2))
                    {
                        Client.GetInstance().RegisterTickHandler(OnTick);
                        OnTickActive = true;
                    }
                    else if (OnTickActive && Game.PlayerPed.Position.DistanceToSquared(CellDoorArea) >= Math.Pow(SearchRadius, 2))
                    {
                        Client.GetInstance().DeregisterTickHandler(OnTick);
                        OnTickActive = false;
                    }
                }
                await BaseScript.Delay(SlowRefreshRate);
            }
        }

        static private async void QuickerPeriodicRefresh()
        {
            while (true)
            {
                if (false/*Client._Exports["FiveMDefaults"].hasSpawned()*/)
                {
                    var CellDoors = ObjectList.Select(o => new Prop(o)).Where(o => o.Model.Hash == (int)CellDoorHash).Where(p => p.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(LockRadius, 2)).OrderBy(p => p.Position.DistanceToSquared(Game.PlayerPed.Position));
                    if (CellDoors.Count() > 0)
                    {
                        NearbyCellDoor = CellDoors.First();
                        (menuItem as MenuItemCheckbox).state = NearbyCellDoor.IsPositionFrozen;
                    }
                    else
                    {
                        NearbyCellDoor = null;
                    }
                }
                await BaseScript.Delay(QuickRefreshRate);
            }
        }

        static private async Task OnTick()
        {
            if (ControlHelper.IsControlJustPressed(Control.Context) && NearbyCellDoor != null)
            {
                NearbyCellDoor.IsPositionFrozen = !NearbyCellDoor.IsPositionFrozen;
                BaseScript.TriggerEvent("Chat.Message", "", "#AAAAAA", $"You {(NearbyCellDoor.IsPositionFrozen ? "locked" : "unlocked")} the cell door.");
            }
            await Task.FromResult(0);
        }
    }
}
