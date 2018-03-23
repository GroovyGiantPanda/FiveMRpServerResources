using CitizenFX.Core;
using FamilyRP.Roleplay.Client.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Player
{
    // Tested and working
    // Effectively invisible walls instead of just a teleport back to the center of the courtyard
    // TODO: Fine-tune prison polygon more later, potentially even add an inner and outer layer
    // e.g. Allow prisoner to flee to outer layer but have guards fire at you
    // Does not bound vertically but should not be a huge issue
    static class PrisonSentence
    {
        static int previousMinute = -1;
        static public bool IsInPrison = false;
        static public int PrisonTimeRemaining = 0;
        static float[] PreviousPosition;
        static private Vector3 PreviousPositionFull;

        static float[][] PrisonPolygon =
        {
            new float[] {1767.076f, 2557.332f},
            new float[] {1781.502f, 2563.654f},
            new float[] {1777.738f, 2536.071f},
            new float[] {1758.665f, 2501.655f},
            new float[] {1714.147f, 2479.080f},
            new float[] {1689.400f, 2460.822f},
            new float[] {1645.425f, 2479.439f},
            new float[] {1607.681f, 2492.532f},
            new float[] {1593.306f, 2546.877f},
            new float[] {1606.972f, 2576.762f},
            new float[] {1779.412f, 2570.443f}
        };

        static Vector3 EnterPrisonLocation = new Vector3(1676.277f, 2536.605f, 45.565f);
        static Vector3 ExitPrisonLocation = new Vector3(1846f, 2586f, 46f);
        static float ExitPrisonHeading = 265f;
        static float EnterPrisonHeading = 120f;

        static public void Init()
        {
            Client.GetInstance().RegisterEventHandler("Arrest.UpdatePrisonTimer", new Action<int>((time) =>
            {
                PrisonTimeRemaining = time;
                IsInPrison = PrisonTimeRemaining > 0;
                BaseScript.TriggerEvent("Chat.Message", "", "#555555", $"You have {PrisonTimeRemaining} months remaining on your sentence.");
                if (time == 0)
                {
                    Game.PlayerPed.Position = ExitPrisonLocation;
                    Game.PlayerPed.Heading = ExitPrisonHeading;
                    IsInPrison = false;
                }
            }));
            PreviousPosition = new float[] { Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y };
            PreviousPositionFull = Game.PlayerPed.Position;
            PeriodicCheck();
        }

        static private async void PeriodicCheck()
        {
            while (true)
            {
                float[] CurrentPosition = { Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y };
                if (IsInPrison && !PolygonCollision.Contains(PrisonPolygon, CurrentPosition))
                {
                    if (previousMinute != DateTime.UtcNow.Minute)
                    {
                        previousMinute = DateTime.UtcNow.Minute;
                        PrisonTimeRemaining -= 1;
                        if (PrisonTimeRemaining > 0)
                        {
                            BaseScript.TriggerEvent("Chat.Message", "", "#555555", $"You have {PrisonTimeRemaining} months remaining on your sentence.");
                        }
                    }
                    if (PolygonCollision.Contains(PrisonPolygon, PreviousPosition))
                    {
                        Game.PlayerPed.PositionNoOffset = PreviousPositionFull;
                        await BaseScript.Delay(50);
                        continue;
                    }
                    else
                    {
                        Game.PlayerPed.PositionNoOffset = EnterPrisonLocation;
                        Game.PlayerPed.Heading = EnterPrisonHeading;
                    }
                }
                if (IsInPrison)
                {
                    Game.PlayerPed.Weapons.RemoveAll();
                }
                PreviousPosition = CurrentPosition;
                PreviousPositionFull = Game.PlayerPed.Position;
                await BaseScript.Delay(50);
            }
        }
    }
}
