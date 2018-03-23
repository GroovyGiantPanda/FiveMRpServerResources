using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Environment
{
    // A warppoint specifies the label the player sees when he is close enough to the point to press E
    class WarpPoint
    {
        public string TargetLabel { get; private set; }
        public Vector3 TargetCoords { get; private set; }
        public float TargetHeading { get; private set; }

        public WarpPoint(string TargetLabel, Vector3 TargetCoords, float TargetHeading)
        {
            this.TargetLabel = TargetLabel;
            this.TargetCoords = TargetCoords;
            this.TargetHeading = TargetHeading;
        }
    }

    class WarpPointPair
    {
        public WarpPoint A { get; set; }
        public WarpPoint B { get; set; }
        public Dictionary<string, bool> PropListsAtoB { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, bool> IPLsAtoB { get; set; } = new Dictionary<string, bool>();
        public Func<WarpPointPair, bool> PermissionCheck { get; set; }

        public WarpPointPair(WarpPoint A, WarpPoint B) { }
        public WarpPointPair() { }
    }

    // All warps are always bi-directional.
    static class WarpPoints
    {
        static float NearbyThreshold = 200f;
        static float contextAoe = 5f; // How close you need to be to see instruction

        static System.Drawing.Color MarkerColor = System.Drawing.Color.FromArgb(200, 190, 130, 75);
        static Vector3 MarkerDirection = new Vector3(0, 0, 0);
        static Vector3 MarkerRotation = new Vector3(0, 0f, 0f);
        static Vector3 MarkerScale = 1f * new Vector3(1.0f, 1.0f, 1.0f);
        static float MarkerZAdjustment = -0.9f;

        static public List<WarpPointPair> Warps = new List<WarpPointPair>()
        {
            //FIB Building Lobby -> Floor 49
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(136.404f, -761.284f, 45.752f + MarkerZAdjustment), 170.862f), B = new WarpPoint("Go Outside", new Vector3(136.090f, -761.715f, 242.152f + MarkerZAdjustment), 166.890f), },
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(141.951f, -769.271f, 242.152f + MarkerZAdjustment), 70.022f), B = new WarpPoint("Go Outside", new Vector3(143.132f, -769.822f, 242.152f + MarkerZAdjustment), 249.561f), },
            //Grove St. Garage
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(-71.883f, -1821.445f, 26.942f + MarkerZAdjustment), 226.341f), B = new WarpPoint("Go Outside", new Vector3(1027.370f, -3101.449f, -39.000f + MarkerZAdjustment), 94.845f),
                                IPLsAtoB = new Dictionary<string, bool>(){ ["ex_exec_warehouse_placement_interior_2_int_warehouse_l_dlc_milo"] = true } },
            //La Fuente Blanca
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(1399.189f, 1127.582f, 114.334f + MarkerZAdjustment), 189.848f), B = new WarpPoint("Go Outside", new Vector3(1399.462f, 1128.994f, 114.334f + MarkerZAdjustment), 357.484f) },
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(1390.025f, 1132.200f, 114.434f + MarkerZAdjustment), 88.331f), B = new WarpPoint("Go Outside", new Vector3(1391.408f, 1132.597f, 114.334f + MarkerZAdjustment), 268.4482f) },
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(1409.762f, 1147.303f, 114.334f + MarkerZAdjustment), 270.610f), B = new WarpPoint("Go Outside", new Vector3(1408.315f, 1147.484f, 114.334f + MarkerZAdjustment), 93.512f) },
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(1395.033f, 1141.936f, 114.628f + MarkerZAdjustment), 86.494f), B = new WarpPoint("Go Outside", new Vector3(1396.902f, 1141.769f, 114.334f + MarkerZAdjustment), 259.203f) },
            //Psychologist House
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(-1911.674f, -576.088f, 19.097f + MarkerZAdjustment), 141.074f), B = new WarpPoint("Go Outside", new Vector3(-1910.834f, -575.006f, 19.097f + MarkerZAdjustment), 328.860f) },
            //Solomons Office
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(-1007.416f, -486.735f, 39.970f + MarkerZAdjustment), 122.347f), B = new WarpPoint("Go Outside", new Vector3(-1003.031f, -477.655f, 50.027f + MarkerZAdjustment), 114.669f) },
            //Vanilla Unicorn
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(126.428f, -1282.021f, 29.276f + MarkerZAdjustment), 127.886f), B = new WarpPoint("Go Outside", new Vector3(127.960f, -1280.941f, 29.270f + MarkerZAdjustment), 307.661f) },
            //Cocaine Lab (South Los Santos)
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(182.665f, -1836.874f, 28.103f + MarkerZAdjustment), 138.570f), B = new WarpPoint("Go Outside", new Vector3(1088.575f, -3187.979f, -38.993f + MarkerZAdjustment), 170.993f),
                                PropListsAtoB = new Dictionary<string, bool>(){ ["security_high"] = true, ["equipment_basic"] = true, ["coke_cut_02"] = true, ["coke_cut_03"] = true, ["table_equipment"] = true, ["coke_press_basic"] = true, ["set_up"] = true },
                                IPLsAtoB = new Dictionary<string, bool>(){ ["bkr_biker_interior_placement_interior_4_biker_dlc_int_ware03_milo"] = true } },
            //Counterfeit Cash (Bristol Coke Storage)
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(671.269f, -2667.656f, 6.081f + MarkerZAdjustment), 86.378f), B = new WarpPoint("Go Outside", new Vector3(1138.090f, -3198.728f, -39.666f + MarkerZAdjustment), 356.246f),
                                PropListsAtoB = new Dictionary<string, bool>(){ ["counterfeit_security"] = true, ["counterfeit_standard_equip_no_prod"] = true, ["counterfeit_cashpile10a"] = true, ["counterfeit_cashpile100b"] = true, ["counterfeit_setup"] = true,
                                ["dryera_off"] = true, ["dryerb_off"] = true, ["dryerc_open"] = true, ["dryerd_off"] = true, ["money_cutter"] = true, ["special_chairs"] = true},
                                IPLsAtoB = new Dictionary<string, bool>(){ ["bkr_biker_interior_placement_interior_4_biker_dlc_int_ware04_milo"] = true } },
            //Document Forgery (Little Seoul)
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(-594.489f, -748.777f, 29.487f + MarkerZAdjustment), 183.966f), B = new WarpPoint("Go Outside", new Vector3(1173.498f, -3196.577f, -39.008f + MarkerZAdjustment), 94.503f),
                                PropListsAtoB = new Dictionary<string, bool>(){ ["security_high"] = true, ["equipment_upgrade"] = true, ["production"] = true, ["set_up"] = true, ["clutter"] = true, ["interior_upgrade"] = true, ["set_up"] = true, ["chair01"] = true,
                                ["chair02"] = true, ["chair03"] = true, ["chair04"] = true, ["chair05"] = true, ["chair06"] = true, ["chair07"] = true  },
                                IPLsAtoB = new Dictionary<string, bool>(){ ["bkr_biker_interior_placement_interior_4_biker_dlc_int_ware05_milo"] = true } },
            //Indoor Grow (Rancho & MacDonald St)
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(449.594f, -1786.979f, 28.595f + MarkerZAdjustment), 69.950f), B = new WarpPoint("Go Outside", new Vector3(1038.967f, -3195.729f, -38.170f + MarkerZAdjustment), 254.853f),
                                PropListsAtoB = new Dictionary<string, bool>(){ ["weed_security_upgrade"] = true, ["weed_standard_equip"] = true, ["weed_production"] = true, ["weed_set_up"] = true, ["weed_growthi_stage3"] = true, ["weed_growthe_stage2"] = true, ["weed_growthd_stage1"] = true },
                                IPLsAtoB = new Dictionary<string, bool>(){ ["bkr_biker_interior_placement_interior_4_biker_dlc_int_ware02_milo"] = true } },
            //Meth Lab
            new WarpPointPair { A = new WarpPoint("Go Inside", new Vector3(1240.946f, 1866.951f, 78.951f + MarkerZAdjustment), 219.684f), B = new WarpPoint("Go Outside", new Vector3(1011.925f, -3202.161f, -38.993f + MarkerZAdjustment), 356.264f),
                                PropListsAtoB = new Dictionary<string, bool>(){ ["meth_lab_security_high"] = true, ["meth_lab_basic"] = true, ["meth_lab_production"] = true, ["meth_lab_setup"] = true },
                                IPLsAtoB = new Dictionary<string, bool>(){ ["bkr_biker_interior_placement_interior_4_biker_dlc_int_ware01_milo"] = true } }        
        };
        static internal List<WarpPointPair> NearbyWarps = new List<WarpPointPair>();

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
            PeriodicUpdate();
            //ButtonInstructions.StatusCheckCallbacks.Add(new Func<KeyValuePair<Control, string>?>(DisplayButtonInstructionCheck));
        }

        static public KeyValuePair<Control, string>? DisplayButtonInstructionCheck()
        {
            WarpPoint warp = GetActiveWarpPoint().Item1;
            if (warp != null)
            {
                return new KeyValuePair<Control, string>(Control.Context, warp.TargetLabel);
            }
            else
            {
                return null;
            }
        }

        static public Tuple<WarpPoint, Dictionary<string, bool>, Dictionary<string, bool>> GetActiveWarpPoint()
        {
            List<WarpPointPair> ContextWarps = NearbyWarps.Where(w => Math.Min(w.A.TargetCoords.DistanceToSquared(Game.PlayerPed.Position), w.B.TargetCoords.DistanceToSquared(Game.PlayerPed.Position)) < contextAoe).ToList();
            if (ContextWarps.Count > 0)
            {
                WarpPointPair closestPair = ContextWarps.OrderBy(w => Math.Min(w.A.TargetCoords.DistanceToSquared(Game.PlayerPed.Position), w.B.TargetCoords.DistanceToSquared(Game.PlayerPed.Position))).First();
                float distanceA = closestPair.A.TargetCoords.DistanceToSquared(Game.PlayerPed.Position);
                float distanceB = closestPair.B.TargetCoords.DistanceToSquared(Game.PlayerPed.Position);
                if (distanceA < distanceB)
                {
                    return new Tuple<WarpPoint, Dictionary<string, bool>, Dictionary<string, bool>>(closestPair.B, closestPair.PropListsAtoB, closestPair.IPLsAtoB);
                }
                else
                {
                    return new Tuple<WarpPoint, Dictionary<string, bool>, Dictionary<string, bool>>(closestPair.A, closestPair.PropListsAtoB.ToDictionary(l => l.Key, l => !l.Value), closestPair.IPLsAtoB.ToDictionary(l => l.Key, l => !l.Value));
                }
            }
            return null;
        }

        static public async Task OnTick()
        {
            try
            {
                NearbyWarps.ForEach(w =>
                {
                    World.DrawMarker(MarkerType.HorizontalCircleFat, w.A.TargetCoords, MarkerDirection, MarkerRotation, MarkerScale, MarkerColor);
                    World.DrawMarker(MarkerType.HorizontalCircleFat, w.B.TargetCoords, MarkerDirection, MarkerRotation, MarkerScale, MarkerColor);
                });
                if (ControlHelper.IsControlJustPressed(Control.Context))
                {
                    Tuple<WarpPoint, Dictionary<string, bool>, Dictionary<string, bool>> detailedWarp = GetActiveWarpPoint();
                    //TODO: Add the ability to gate warp points.  E.g. Berr's G.G. Deleware character is the only one that can unlock
                    //      (and lock) Solomon's office to allow (disallow) warps to the location.  
                    if (detailedWarp != null)
                    {
                        WarpPoint warp = detailedWarp.Item1;
                        Dictionary<string, bool> propList = detailedWarp.Item2;
                        Dictionary<string, bool> IPLsList = detailedWarp.Item3;

                        CitizenFX.Core.UI.Screen.Fading.FadeOut(500);

                        while (!CitizenFX.Core.UI.Screen.Fading.IsFadedOut)
                        {
                            await BaseScript.Delay(10);
                        }

                        Game.PlayerPed.Position = warp.TargetCoords;
                        Game.PlayerPed.Heading = warp.TargetHeading;
                        // Pretty sure the below is not required, but does no harm
                        Function.Call(Hash.REQUEST_COLLISION_AT_COORD, warp.TargetCoords.X, warp.TargetCoords.Y, warp.TargetCoords.Z);

                        if (IPLsList.Count > 0)
                        {
                            IPLsList.ToList().ForEach(l =>
                            {
                                if (l.Value)
                                    Function.Call(Hash.REQUEST_IPL, l.Key);
                                else
                                    Function.Call(Hash.REMOVE_IPL, l.Key);
                            });
                        }

                        if (propList.Count > 0)
                        {
                            int interior = Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, warp.TargetCoords.X, warp.TargetCoords.Y, warp.TargetCoords.Z);
                            propList.ToList().ForEach(l =>
                            {
                                if (l.Value)
                                    Function.Call(Hash._ENABLE_INTERIOR_PROP, interior, l.Key);
                                else
                                    Function.Call(Hash._DISABLE_INTERIOR_PROP, interior, l.Key);
                            });
                            Function.Call(Hash.REFRESH_INTERIOR, interior);
                        }
                        
                        RecalculateNearby();
                        //Necessary to allow props to load in
                        await BaseScript.Delay(1000);
                        CitizenFX.Core.UI.Screen.Fading.FadeIn(500);
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error($"[WARPPOINTS] OnTick error: {ex.Message}");
            }
            await Task.FromResult(0);
        }

        static public async void PeriodicUpdate()
        {
            while (true)
            {
                RecalculateNearby();
                await BaseScript.Delay(5000);
            }
        }

        // Separate function so it can get called by other functions on teleports
        // TODO: Call from e.g. /dev tp
        static public void RecalculateNearby()
        {
            NearbyWarps = Warps.Where(w =>
            w.A.TargetCoords.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(NearbyThreshold, 2) ||
            w.B.TargetCoords.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(NearbyThreshold, 2)).ToList();
        }
    }
}
