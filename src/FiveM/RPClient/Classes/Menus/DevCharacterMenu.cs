using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Common;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client
{
    static class CharacterMenuTest
    {
        public static MenuObserver Observer;
        public static MenuModel PedMenu;

        // It's fine if this is kept between switches
        static int blendHeadA = 0;
        static int blendHeadB = 0;
        static float blendHeadAmount = 0.5f;
        static int blendSkinA = 0;
        static int blendSkinB = 0;
        static float blendSkinAmount = 0.5f;
        static Dictionary<string, Tuple<int, int>> componentSettings = new Dictionary<string, Tuple<int, int>>();
        static Dictionary<string, Tuple<int, int>> propSettings = new Dictionary<string, Tuple<int, int>>();
        static Dictionary<string, string> componentAndPropRenamings = new Dictionary<string, string>()
        {
            ["Torso"] = "Arms",
            ["Legs"] = "Pants",
            ["Hands"] = "Parachutes, Vests and Bags",
            ["Special1"] = "Neck",
            ["Special2"] = "Overshirt",
            ["Special3"] = "Tactical Vests",
            ["Textures"] = "Logos",
            ["Torso"] = "Arms",
            ["Torso2"] = "Jacket",
            ["EarPieces"] = "Ear Pieces"
        };

        static List<string> PedHashFilter = new List<string>()
        {
            "g_f_y_lost_01", "g_m_y_lost_01", "g_m_y_lost_02", "g_m_y_lost_03", "a_f_y_vinewood_01", "a_f_y_vinewood_04", "a_m_y_vinewood_01", "a_m_y_vinewood_03", "g_f_y_vagos_01", "g_m_y_salvagoon_01", "g_m_y_salvagoon_02", "g_m_y_salvagoon_03", "s_m_m_trucker_01", "a_f_y_topless_01", "s_f_y_sweatshop_01", "s_f_y_stripper_01", "s_f_y_stripper_02", "a_f_y_skater_01", "a_m_m_skater_01", "s_m_y_robber_01", "a_f_y_rurmeth_01", "a_m_m_rurmeth_01", "g_m_m_armgoon_01", "g_m_m_chigoon_01", "g_m_m_chigoon_02", "g_m_y_armgoon_02", "g_m_y_mexgoon_01", "g_m_y_mexgoon_02", "a_f_o_indian_01", "a_f_y_indian_01", "a_m_m_indian_01", "a_m_y_indian_01", "s_f_y_hooker_01", "s_f_y_hooker_02", "s_f_y_hooker_03", "u_f_y_hotposh_01", "a_f_y_hipster_01", "a_m_y_hipster_01", "a_f_y_hippie_01", "u_m_y_hippie_01", "a_f_m_bevhills_01", "a_f_y_bevhills_01", "a_f_y_bevhills_02", "a_f_y_bevhills_03", "a_f_y_bevhills_04", "a_m_y_bevhills_02", "g_f_y_ballas_01", "g_m_y_ballasout_01", "a_f_y_business_01", "a_f_y_business_02", "a_f_y_business_03", "a_f_y_business_04", "a_m_m_business_01", "a_m_y_business_01", "a_m_y_business_02", "a_m_y_business_03", "A_F_M_Beach_01", "A_F_M_BevHills_02", "A_F_M_BodyBuild_01", "A_F_M_Business_02", "A_F_M_Downtown_01", "A_F_M_EastSA_01", "A_F_M_EastSA_02", "A_F_M_FatBla_01", "A_F_M_FatWhite_01", "A_F_M_KTown_01", "A_F_M_KTown_02", "A_F_M_ProlHost_01", "A_F_M_Salton_01", "A_F_M_SkidRow_01", "A_F_M_SouCentMC_01", "A_F_M_SouCent_01", "A_F_M_SouCent_02", "A_F_M_Tourist_01", "A_F_M_TrampBeac_01", "A_F_M_Tramp_01", "A_F_O_GenStreet_01", "A_F_O_KTown_01", "A_F_O_Salton_01", "A_F_O_SouCent_01", "A_F_O_SouCent_02", "A_F_Y_Beach_01", "A_F_Y_EastSA_01", "A_F_Y_EastSA_02", "A_F_Y_EastSA_03", "A_F_Y_Epsilon_01", "A_F_Y_Fitness_01", "A_F_Y_Fitness_02", "A_F_Y_GenHot_01", "A_F_Y_Golfer_01", "A_F_Y_Hiker_01", "A_F_Y_Hipster_02", "A_F_Y_Hipster_03", "A_F_Y_Hipster_04", "A_F_Y_Juggalo_01", "A_F_Y_Runner_01", "A_F_Y_SCDressy_01", "A_F_Y_SouCent_01", "A_F_Y_SouCent_02", "A_F_Y_SouCent_03", "A_F_Y_Tennis_01", "A_F_Y_Tourist_01", "A_F_Y_Tourist_02", "A_F_Y_Vinewood_02", "A_F_Y_Vinewood_03", "A_F_Y_Yoga_01", "A_M_M_AfriAmer_01", "A_M_M_Beach_01", "A_M_M_Beach_02", "A_M_M_BevHills_01", "A_M_M_BevHills_02", "A_M_M_EastSA_01", "A_M_M_EastSA_02", "A_M_M_Farmer_01", "A_M_M_FatLatin_01", "A_M_M_GenFat_01", "A_M_M_GenFat_02", "A_M_M_Golfer_01", "A_M_M_HasJew_01", "A_M_M_Hillbilly_01", "A_M_M_Hillbilly_02", "A_M_M_KTown_01", "A_M_M_Malibu_01", "A_M_M_MexCntry_01", "A_M_M_MexLabor_01", "A_M_M_OG_Boss_01", "A_M_M_Paparazzi_01", "A_M_M_Polynesian_01", "A_M_M_ProlHost_01", "A_M_M_Salton_01", "A_M_M_Salton_02", "A_M_M_Salton_03", "A_M_M_Salton_04", "A_M_M_Skidrow_01", "A_M_M_SoCenLat_01", "A_M_M_SouCent_01", "A_M_M_SouCent_02", "A_M_M_SouCent_03", "A_M_M_SouCent_04", "A_M_M_StLat_02", "A_M_M_Tennis_01", "A_M_M_Tourist_01", "A_M_M_TrampBeac_01", "A_M_M_Tramp_01", "A_M_M_TranVest_01", "A_M_M_TranVest_02", "A_M_O_ACult_02", "A_M_O_Beach_01", "A_M_O_GenStreet_01", "A_M_O_KTown_01", "A_M_O_Salton_01", "A_M_O_SouCent_01", "A_M_O_SouCent_02", "A_M_O_SouCent_03", "A_M_O_Tramp_01", "A_M_Y_ACult_02", "A_M_Y_BeachVesp_01", "A_M_Y_BeachVesp_02", "A_M_Y_Beach_01", "A_M_Y_Beach_02", "A_M_Y_Beach_03", "A_M_Y_BevHills_01", "A_M_Y_BreakDance_01", "A_M_Y_BusiCas_01", "A_M_Y_Cyclist_01", "A_M_Y_DHill_01", "A_M_Y_Downtown_01", "A_M_Y_EastSA_01", "A_M_Y_EastSA_02", "A_M_Y_Epsilon_01", "A_M_Y_Epsilon_02", "A_M_Y_Gay_01", "A_M_Y_Gay_02", "A_M_Y_GenStreet_01", "A_M_Y_GenStreet_02", "A_M_Y_Golfer_01", "A_M_Y_HasJew_01", "A_M_Y_Hiker_01", "A_M_Y_Hippy_01", "A_M_Y_Hipster_02", "A_M_Y_Hipster_03", "A_M_Y_Jetski_01", "A_M_Y_Juggalo_01", "A_M_Y_KTown_01", "A_M_Y_KTown_02", "A_M_Y_Latino_01", "A_M_Y_MethHead_01", "A_M_Y_MexThug_01", "A_M_Y_MotoX_01", "A_M_Y_MotoX_02", "A_M_Y_MusclBeac_01", "A_M_Y_MusclBeac_02", "A_M_Y_Polynesian_01", "A_M_Y_RoadCyc_01", "A_M_Y_Runner_01", "A_M_Y_Runner_02", "A_M_Y_Salton_01", "A_M_Y_Skater_01", "A_M_Y_Skater_02", "A_M_Y_SouCent_01", "A_M_Y_SouCent_02", "A_M_Y_SouCent_03", "A_M_Y_SouCent_04", "A_M_Y_StBla_01", "A_M_Y_StBla_02", "A_M_Y_StLat_01", "A_M_Y_StWhi_01", "A_M_Y_StWhi_02", "A_M_Y_Sunbathe_01", "A_M_Y_Surfer_01", "A_M_Y_VinDouche_01", "A_M_Y_Vinewood_02", "A_M_Y_Vinewood_04", "A_M_Y_Yoga_01", "CSB_Car3guy1", "CSB_Car3guy2", "CSB_Chin_goon", "CSB_Denise_friend", "CSB_FOS_rep", "CSB_Grove_str_dlr", "CSB_Ramp_gang", "CSB_Ramp_hic", "CSB_Ramp_hipster", "CSB_Ramp_mex", "CSB_Screen_Writer", "CSB_Stripper_01", "CSB_Stripper_02", "CS_FBISuit_01", "CS_LifeInvad_01", "CS_MovPremF_01", "CS_MRS_Thornhill", "CS_Old_Man1A", "CS_Old_Man2", "G_F_Y_Families_01", "G_M_M_ArmBoss_01", "G_M_M_ArmLieut_01", "G_M_M_ChiBoss_01", "G_M_M_ChiCold_01", "G_M_M_KorBoss_01", "G_M_M_MexBoss_01", "G_M_M_MexBoss_02", "G_M_Y_Azteca_01", "G_M_Y_BallaEast_01", "G_M_Y_BallaOrig_01", "G_M_Y_FamCA_01", "G_M_Y_FamDNF_01", "G_M_Y_FamFor_01", "G_M_Y_Korean_01", "G_M_Y_Korean_02", "G_M_Y_KorLieut_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_02", "G_M_Y_MexGoon_03", "G_M_Y_PoloGoon_01", "G_M_Y_PoloGoon_02", "G_M_Y_SalvaBoss_01", "G_M_Y_StrPunk_01", "G_M_Y_StrPunk_02", "HC_Hacker", "IG_Abigail", "IG_AmandaTownley", "IG_Ashley", "IG_BallasOG", "IG_Bankman", "IG_Barry", "IG_BestMen", "IG_Brad", "IG_Bride", "IG_Car3guy1", "IG_Car3guy2", "IG_ChengSr", "IG_ChrisFormage", "IG_Clay", "IG_ClayPain", "IG_Cletus", "IG_Dale", "IG_DaveNorton", "IG_Denise", "IG_Devin", "IG_Dom", "IG_Dreyfuss", "IG_DrFriedlander", "IG_Floyd", "IG_Groom", "IG_Hao", "IG_Janet", "ig_JAY_Norris", "IG_JewelAss", "IG_JimmyBoston", "IG_JimmyDiSanto", "IG_JoeMinuteMan", "IG_Josef", "IG_Josh", "IG_KerryMcIntosh", "IG_LamarDavis", "IG_Lazlow", "IG_LesterCrest", "IG_LifeInvad_01", "IG_LifeInvad_02", "IG_Magenta", "IG_Manuel", "IG_Marnie", "IG_MaryAnn", "IG_Maude", "IG_Milton", "IG_Molly", "IG_MRK", "IG_MrsPhillips", "IG_MRS_Thornhill", "IG_Natalia", "IG_NervousRon", "IG_Nigel", "IG_Old_Man1A", "IG_Old_Man2", "IG_Omega", "IG_ONeil", "IG_Ortega", "IG_Paper", "IG_Patricia", "IG_Priest", "IG_Ramp_Gang", "IG_Ramp_Hic", "IG_Ramp_Hipster", "IG_Ramp_Mex", "IG_RoccoPelosi", "IG_RussianDrunk", "IG_Screen_Writer", "IG_SiemonYetarian", "IG_Solomon", "IG_SteveHains", "IG_Stretch", "IG_Talina", "IG_Tanisha", "IG_TaoCheng", "IG_TaosTranslator", "ig_TennisCoach", "IG_Terry", "IG_TomEpsilon", "IG_Tonya", "IG_TracyDiSanto", "IG_TrafficWarden", "IG_TylerDix", "IG_Wade", "IG_Zimbor", "MP_F_StripperLite", "MP_G_M_Pros_01", "MP_M_ExArmy_01", "MP_M_FamDD_01", "MP_M_ShopKeep_01", "Player_One", "Player_Two", "Player_Zero", "S_F_M_Fembarber", "S_F_M_Maid_01", "S_F_M_Shop_HIGH", "S_F_M_SweatShop_01", "S_F_Y_AirHostess_01", "S_F_Y_Bartender_01", "S_F_Y_Baywatch_01", "S_F_Y_MovPrem_01", "S_F_Y_Shop_LOW", "S_F_Y_Shop_MID", "S_F_Y_StripperLite", "S_M_M_AmmuCountry", "S_M_M_AutoShop_01", "S_M_M_AutoShop_02", "S_M_M_Bouncer_01", "S_M_M_CntryBar_01", "S_M_M_DockWork_01", "S_M_M_Gaffer_01", "S_M_M_Gardener_01", "S_M_M_GenTransport", "S_M_M_HairDress_01", "S_M_M_HighSec_01", "S_M_M_HighSec_02", "S_M_M_Janitor", "S_M_M_LatHandy_01", "S_M_M_LifeInvad_01", "S_M_M_Linecook", "S_M_M_LSMetro_01", "S_M_M_Mariachi_01", "S_M_M_Migrant_01", "S_M_M_MovAlien_01", "S_M_M_MovPrem_01", "S_M_M_Postal_01", "S_M_M_Postal_02", "S_M_M_StrPreach_01", "S_M_M_StrVend_01", "S_M_M_UPS_01", "S_M_M_UPS_02", "S_M_O_Busker_01", "S_M_Y_AirWorker", "S_M_Y_AmmuCity_01", "S_M_Y_Barman_01", "S_M_Y_BayWatch_01", "S_M_Y_BusBoy_01", "S_M_Y_Chef_01", "S_M_Y_Construct_01", "S_M_Y_Construct_02", "S_M_Y_Dealer_01", "S_M_Y_DevinSec_01", "S_M_Y_DockWork_01", "S_M_Y_Doorman_01", "S_M_Y_Grip_01", "S_M_Y_Shop_MASK", "S_M_Y_StrVend_01", "S_M_Y_Valet_01", "S_M_Y_Waiter_01", "S_M_Y_WinClean_01", "S_M_Y_XMech_01", "S_M_Y_XMech_02", "U_F_M_Miranda", "U_F_M_ProMourn_01", "U_F_O_MovieStar", "U_F_O_ProlHost_01", "U_F_Y_BikerChic", "U_F_Y_COMJane", "U_F_Y_JewelAss_01", "U_F_Y_Mistress", "U_F_Y_PoppyMich", "U_F_Y_Princess", "U_F_Y_SpyActress", "U_M_M_Aldinapoli", "U_M_M_BankMan", "U_M_M_BikeHire_01", "U_M_M_FIBArchitect", "U_M_M_FilmDirector", "U_M_M_GlenStank_01", "U_M_M_Griff_01", "U_M_M_Jesus_01", "U_M_M_JewelSec_01", "U_M_M_JewelThief", "U_M_M_MarkFost", "U_M_M_PartyTarget", "U_M_M_ProMourn_01", "U_M_M_RivalPap", "U_M_M_SpyActor", "U_M_M_WillyFist", "U_M_O_FinGuru_01", "U_M_O_TapHillBilly", "U_M_O_Tramp_01", "U_M_Y_Abner", "U_M_Y_AntonB", "U_M_Y_BabyD", "U_M_Y_Baygor", "U_M_Y_BurgerDrug_01", "U_M_Y_Chip", "U_M_Y_Cyclist_01", "U_M_Y_FIBMugger_01", "U_M_Y_Guido_01", "U_M_Y_GunVend_01", "U_M_Y_Mani", "U_M_Y_MilitaryBum", "U_M_Y_Paparazzi", "U_M_Y_Party_01", "U_M_Y_ProlDriver_01", "U_M_Y_SBike", "U_M_Y_StagGrm_01", "U_M_Y_Tattoo_01"
        };
        static List<PedHash> PedHashFilterConverted = PedHashFilter.Select(p => (PedHash)Game.GenerateHash(p)).ToList();
        static List<PedHash> PedHashValues = PedHashFilterConverted;/*Enum.GetValues(typeof(PedHash)).OfType<PedHash>().Where(p => PedHashFilterConverted.Contains(p)).ToList();*/
        static List<string> PedHashNames = PedHashFilterConverted.Select(c => c.ToString().AddSpacesToCamelCase()).ToList();

        static List<PedProps> PedPropValues = Enum.GetValues(typeof(PedProps)).OfType<PedProps>().ToList();
        static List<string> PedPropNames = Enum.GetNames(typeof(PedProps)).Select(c => c.AddSpacesToCamelCase()).ToList();


        class PedHeadOverlay
        {
            public int ID;
            public string Name;
            public int maxOption;
            public int colorType = 0;
            public List<int> OptionValues { get { var list = Enumerable.Range(0, maxOption+1).ToList(); list.Add(255); return list; } }
            public List<string> OptionNames { get { var list = Enumerable.Range(0, maxOption+1).Select(i => i.ToString()).ToList(); list.Add("None"); return list; } }
        }

        static List<PedHeadOverlay> overlays = new List<PedHeadOverlay>()
        {
            new PedHeadOverlay { ID = 0, Name = "Blemishes", maxOption = 23 },
            new PedHeadOverlay { ID = 1, Name = "Facial Hair", maxOption = 28, colorType = 1 },
            new PedHeadOverlay { ID = 2, Name = "Eyebrows", maxOption = 33, colorType = 1 },
            new PedHeadOverlay { ID = 3, Name = "Ageing", maxOption = 14 },
            new PedHeadOverlay { ID = 4, Name = "Makeup", maxOption = 74 },
            new PedHeadOverlay { ID = 5, Name = "Blush", maxOption = 6, colorType = 2 },
            new PedHeadOverlay { ID = 6, Name = "Complexion", maxOption = 11 },
            new PedHeadOverlay { ID = 7, Name = "Sun Damage", maxOption = 10 },
            new PedHeadOverlay { ID = 8, Name = "Lipstick", maxOption = 9, colorType = 2 },
            new PedHeadOverlay { ID = 9, Name = "Moles/Freckles", maxOption = 17 },
            new PedHeadOverlay { ID = 10, Name = "Chest Hair", maxOption = 16, colorType = 1 },
            new PedHeadOverlay { ID = 11, Name = "Body Blemishes", maxOption = 11 },
            new PedHeadOverlay { ID = 12, Name = "Birth Marks", maxOption = 1 }
        };
        static Dictionary<int, int> overlayColors = new Dictionary<int, int>();

        class PedMenuModel : MenuModel
        {
            private int EyeColor = 0;
            private int HairColorHighlights = 0;
            private int HairColorPrimary = 0;
            Dictionary<int, float> MorphStatus = new Dictionary<int, float>();
            int MorphIndex = 0;
            private List<string> walkingStyles = new List<string>() { "Reset to default", "move_m@tough_guy@", "move_m@alien", "move_m@non_chalant", "move_m@hobo@a", "move_m@money", "move_m@swagger", "move_m@joy", "move_m@powerwalk", "move_m@posh@", "move_m@sad@a", "move_m@shadyped@a", "move_m@tired", "move_f@sexy", "move_m@drunk@slightlydrunk", "move_m@drunk@verydrunk", "move_f@arrogant@a", "move_f@arrogant@b", "move_f@arrogant@c", "move_f@heels@c", "move_f@heels@d", "move_f@sassy", "move_f@posh@", "move_f@maneater", "move_f@chichi", "move_m@swagger" };
            private int walkingStyleIndex = 0;

            public static async Task ReplaceCurrentPedModelByIndex(int index)
            {
                await new Model(PedHashValues[index]).Request(10000);
                await Game.Player.ChangeModel(new Model(PedHashValues[index]));
                await BaseScript.Delay(100);
                if (PedHashValues[index] == PedHash.FreemodeMale01 || PedHashValues[index] == PedHash.FreemodeFemale01)
                { 
                    Function.Call(Hash.SET_PED_HEAD_BLEND_DATA, Game.PlayerPed.Handle, 0, 0, 0, 0, 0, 0, 0, 0, 0, false);
                }
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.PlayerPed.Handle, 1, 0, 0, 0);
            }

            public override void Refresh()
            {
                var _menuItems = new List<MenuItem>();
                var ped = Game.PlayerPed;
                int selectedPedHashIndex = PedHashValues.Contains((PedHash)ped.Model.Hash) ? PedHashValues.IndexOf((PedHash)ped.Model.Hash) : PedHashValues.IndexOf(PedHash.FreemodeMale01);
                if (Game.PlayerPed.Exists())
                {
                    _menuItems.Add(new MenuItemHorNamedSelector
                    {
                        Title = $"Skin",
                        Description = "Activate to replace your current skin.",
                        state = menuItems[0] is MenuItemHorNamedSelector ? (menuItems[0] as MenuItemHorNamedSelector).state : selectedPedHashIndex,
                        Type = MenuItemHorizontalSelectorType.NumberAndBar,
                        wrapAround = true,
                        optionList = PedHashNames,
                        OnActivate = new Action<int, string, MenuItemHorNamedSelector>(async (selectedAlternative, selName, menuItem) => { await ReplaceCurrentPedModelByIndex(selectedAlternative); componentSettings.Clear(); propSettings.Clear(); 
                        })
                    });
                    if ((PedHash)ped.Model.Hash == PedHash.FreemodeMale01 || (PedHash)ped.Model.Hash == PedHash.FreemodeFemale01)
                    {
                        _menuItems.Add(new MenuItemHorSelector<int>
                        {
                            Title = $@"Face: Father's appearance",
                            state = (blendHeadA >= 0 && blendHeadA <= 45) ? blendHeadA : 0,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            minState = 0,
                            maxState = 45,
                            OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { blendHeadA = selectedAlternative; Function.Call(Hash.SET_PED_HEAD_BLEND_DATA, ped.Handle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false); })
                        });
                        _menuItems.Add(new MenuItemHorSelector<int>
                        {
                            Title = $@"Face: Mother's appearance",
                            state = (blendHeadB >= 0 && blendHeadB <= 45) ? blendHeadB : 0,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            minState = 0,
                            maxState = 45,
                            OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { blendHeadB = selectedAlternative; Function.Call(Hash.SET_PED_HEAD_BLEND_DATA, ped.Handle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false); })
                        });
                        _menuItems.Add(new MenuItemHorSelector<float>
                        {
                            Title = $@"Face: Father/Mother Gene Weight",
                            state = blendHeadAmount,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            minState = 0f,
                            maxState = 1f,
                            stepSize = 0.05f,
                            OnChange = new Action<float, MenuItemHorSelector<float>>((selectedAlternative, menuItem) => { blendHeadAmount = selectedAlternative; Function.Call(Hash.SET_PED_HEAD_BLEND_DATA, ped.Handle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false); })
                        });
                        _menuItems.Add(new MenuItemHorSelector<int>
                        {
                            Title = $@"Skin color: Father's appearance",
                            state = (blendSkinA >= 0 && blendSkinA <= 45) ? blendSkinA : 0,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            minState = 0,
                            maxState = 45,
                            OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { blendSkinA = selectedAlternative; Function.Call(Hash.SET_PED_HEAD_BLEND_DATA, ped.Handle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false); })
                        });
                        _menuItems.Add(new MenuItemHorSelector<int>
                        {
                            Title = $@"Skin color: Mother's appearance",
                            state = (blendSkinB >= 0 && blendSkinB <= 45) ? blendSkinB : 0,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            minState = 0,
                            maxState = 45,
                            OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { blendSkinB = selectedAlternative; Function.Call(Hash.SET_PED_HEAD_BLEND_DATA, ped.Handle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false); })
                        });
                        _menuItems.Add(new MenuItemHorSelector<float>
                        {
                            Title = $@"Skin color: Father/Mother Gene Weight",
                            state = blendSkinAmount,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            minState = 0f,
                            maxState = 1f,
                            stepSize = 0.05f,
                            OnChange = new Action<float, MenuItemHorSelector<float>>((selectedAlternative, menuItem) => { blendSkinAmount = selectedAlternative; Function.Call(Hash.SET_PED_HEAD_BLEND_DATA, ped.Handle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false); })
                        });
                        overlays.ForEach(o =>
                        {
                            _menuItems.Add(new MenuItemHorNamedSelector
                            {
                                Title = o.Name,
                                state = o.OptionValues.IndexOf(Function.Call<int>(Hash._GET_PED_HEAD_OVERLAY_VALUE, ped.Handle, o.ID)),
                                Type = MenuItemHorizontalSelectorType.NumberAndBar,
                                wrapAround = true,
                                optionList = o.OptionNames,
                                OnChange = new Action<int, string, MenuItemHorNamedSelector>((selectedAlternative, selName, menuItem) => { Function.Call(Hash.SET_PED_HEAD_OVERLAY, ped.Handle, o.ID, o.OptionValues[selectedAlternative], 1f); }) // Setting default opacity of 1f
                            });
                            if(o.colorType != 0)
                            {
                                if (!overlayColors.ContainsKey(o.ID)) overlayColors[o.ID] = 0;
                                _menuItems.Add(new MenuItemHorSelector<int>
                                {
                                    Title = $"{o.Name}: Color",
                                    state = overlayColors[o.ID],
                                    Type = MenuItemHorizontalSelectorType.NumberAndBar,
                                    wrapAround = true,
                                    minState = 0,
                                    maxState = 63,
                                    OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { overlayColors[o.ID] = selectedAlternative; Function.Call(Hash._SET_PED_HEAD_OVERLAY_COLOR, ped.Handle, o.ID, o.colorType, selectedAlternative, selectedAlternative); })
                                });
                            }
                        });
                        _menuItems.Add(new MenuItemHorSelector<int>
                        {
                            Title = "Eye Color",
                            state = EyeColor,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            minState = 0,
                            maxState = 63,
                            OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { EyeColor = selectedAlternative; Function.Call(Hash._SET_PED_EYE_COLOR, ped.Handle, selectedAlternative); })
                        });
                        _menuItems.Add(new MenuItemHorSelector<int>
                        {
                            Title = "Primary Hair Color",
                            state = HairColorPrimary,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            minState = 0,
                            maxState = 63,
                            OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { HairColorPrimary = selectedAlternative; Function.Call(Hash._SET_PED_HAIR_COLOR, ped.Handle, selectedAlternative, HairColorHighlights); })
                        });
                        _menuItems.Add(new MenuItemHorSelector<int>
                        {
                            Title = "Highlight Hair Color",
                            state = HairColorHighlights,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            minState = 0,
                            maxState = 63,
                            OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { HairColorHighlights = selectedAlternative; Function.Call(Hash._SET_PED_HAIR_COLOR, ped.Handle, HairColorPrimary, selectedAlternative); })
                        });
                        _menuItems.Add(new MenuItemHorSelector<int>
                        {
                            Title = "Facial Morph: Feature Index",
                            state = MorphIndex,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            minState = 0,
                            maxState = 20,
                            OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { MorphIndex = selectedAlternative; })
                        });
                        if (!MorphStatus.ContainsKey(MorphIndex)) MorphStatus[MorphIndex] = 0f;
                        _menuItems.Add(new MenuItemHorSelector<float>
                        {
                            Title = "Facial Morph: Morph Stage",
                            state = MorphStatus[MorphIndex],
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            minState = -2f,
                            maxState = 2f,
                            stepSize = 0.5f,
                            OnChange = new Action<float, MenuItemHorSelector<float>>((selectedAlternative, menuItem) => { MorphStatus[MorphIndex] = selectedAlternative; Function.Call(Hash._SET_PED_FACE_FEATURE, ped.Handle, MorphIndex, selectedAlternative); })
                        });
                    }


                    PedComponent[] components = Game.PlayerPed.Style.GetAllComponents();
                    PedProp[] props = Game.PlayerPed.Style.GetAllProps();

                    components.ToList().ForEach(c =>
                    {
                        try
                        {
                            if(!(c.ToString() == "Face") || (!((PedHash)ped.Model.Hash == PedHash.FreemodeMale01) && !((PedHash)ped.Model.Hash == PedHash.FreemodeFemale01))) // Since Face doesn't work on freemode characters (if you use the blending/morphing option anyway, which everybody should be)
                            {
                                if (!componentSettings.ContainsKey(c.ToString())) componentSettings.Add(c.ToString(), new Tuple<int, int>(0, 0));
                                if (componentSettings[c.ToString()].Item1 < 0 || componentSettings[c.ToString()].Item1 > c.Count - 1) componentSettings[c.ToString()] = new Tuple<int, int>(0, componentSettings[c.ToString()].Item2);
                                if (componentSettings[c.ToString()].Item2 < 0 || componentSettings[c.ToString()].Item2 > c.TextureCount - 1) componentSettings[c.ToString()] = new Tuple<int, int>(componentSettings[c.ToString()].Item1, 0);
                                if(c.Count > 1)
                                { 
                                    _menuItems.Add(new MenuItemHorSelector<int>
                                    {
                                        Title = $@"{(componentAndPropRenamings.ContainsKey(c.ToString()) ? componentAndPropRenamings[c.ToString()] : c.ToString())}",
                                        state = componentSettings[c.ToString()].Item1,
                                        Type = MenuItemHorizontalSelectorType.NumberAndBar,
                                        wrapAround = true,
                                        minState = 0,
                                        maxState = c.Count - 1,
                                        overrideDetailWith = $"{componentSettings[c.ToString()].Item1+1}/{c.Count}",
                                        OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) =>
                                        {
                                            componentSettings[c.ToString()] = new Tuple<int, int>(selectedAlternative, 0);
                                            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, ped.Handle, Enum.GetNames(typeof(PedComponents)).ToList().IndexOf(c.ToString()), selectedAlternative, 0, 0);
                                        })
                                    });
                                }
                                if (c.TextureCount > 1)
                                {
                                    _menuItems.Add(new MenuItemHorSelector<int>
                                    {
                                        Title = $@"{(componentAndPropRenamings.ContainsKey(c.ToString()) ? componentAndPropRenamings[c.ToString()] : c.ToString())}: Variants",
                                        state = componentSettings[c.ToString()].Item2,
                                        Type = MenuItemHorizontalSelectorType.NumberAndBar,
                                        wrapAround = true,
                                        minState = 0,
                                        maxState = c.TextureCount - 1,
                                        overrideDetailWith = $"{componentSettings[c.ToString()].Item2+1}/{c.TextureCount}",
                                        OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { componentSettings[c.ToString()] = new Tuple<int, int>(componentSettings[c.ToString()].Item1, selectedAlternative);
                                            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, ped.Handle, Enum.GetNames(typeof(PedComponents)).ToList().IndexOf(c.ToString()), componentSettings[c.ToString()].Item1, selectedAlternative, 0);
                                        })
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"[PEDTESTMENU] Exception in ped components code; {ex.Message}");
                        }
                    });

                    props.ToList().ForEach(p =>
                    {
                        try
                        {
                            if (!propSettings.ContainsKey(p.ToString())) propSettings.Add(p.ToString(), new Tuple<int, int>(0, 0));
                            if (propSettings[p.ToString()].Item1 < -1 || propSettings[p.ToString()].Item1 > p.Count - 1) componentSettings[p.ToString()] = new Tuple<int, int>(0, propSettings[p.ToString()].Item2);
                            if (propSettings[p.ToString()].Item2 < 0 || propSettings[p.ToString()].Item2 > p.TextureCount - 1) propSettings[p.ToString()] = new Tuple<int, int>(propSettings[p.ToString()].Item1, 0);
                            if(p.Count > 1)
                            {
                                _menuItems.Add(new MenuItemHorSelector<int>
                                {
                                    Title = $@"{(componentAndPropRenamings.ContainsKey(p.ToString()) ? componentAndPropRenamings[p.ToString()] : p.ToString())}",
                                    state = propSettings[p.ToString()].Item1,
                                    Type = MenuItemHorizontalSelectorType.NumberAndBar,
                                    wrapAround = true,
                                    minState = -1,
                                    maxState = p.Count-1,
                                    overrideDetailWith = $"{propSettings[p.ToString()].Item1 + 2}/{p.Count + 1}",
                                    OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { propSettings[p.ToString()] = new Tuple<int, int>(selectedAlternative, 0);
                                        if (selectedAlternative == -1)
                                            Function.Call(Hash.CLEAR_PED_PROP, ped.Handle, Enum.GetNames(typeof(PedProps)).ToList().IndexOf(p.ToString()));
                                        else
                                            Function.Call(Hash.SET_PED_PROP_INDEX, ped.Handle, Enum.GetNames(typeof(PedProps)).ToList().IndexOf(p.ToString()), selectedAlternative, 0, false);
                                    })
                                });
                            }
                            if (p.TextureCount > 1)
                            {
                                _menuItems.Add(new MenuItemHorSelector<int>
                                {
                                    Title = $"{(componentAndPropRenamings.ContainsKey(p.ToString()) ? componentAndPropRenamings[p.ToString()] : p.ToString())}: Variants",
                                    state = propSettings[p.ToString()].Item2,
                                    Type = MenuItemHorizontalSelectorType.NumberAndBar,
                                    wrapAround = true,
                                    minState = 0,
                                    maxState = p.TextureCount - 1,
                                    overrideDetailWith = $"{propSettings[p.ToString()].Item2 + 1}/{p.TextureCount}",
                                    OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { propSettings[p.ToString()] = new Tuple<int, int>(propSettings[p.ToString()].Item1, selectedAlternative);
                                        Function.Call(Hash.SET_PED_PROP_INDEX, ped.Handle, Enum.GetNames(typeof(PedProps)).ToList().IndexOf(p.ToString()), propSettings[p.ToString()].Item1, selectedAlternative, false);
                                    })
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"[PEDTESTMENU] Exception in ped props code; {ex.Message}");
                        }
                    });
                }

                _menuItems.Add(new MenuItemHorNamedSelector
                {
                    Title = $"Walking Style",
                    state = walkingStyleIndex,
                    Type = MenuItemHorizontalSelectorType.NumberAndBar,
                    wrapAround = true,
                    optionList = walkingStyles,
                    overrideDetailWith = walkingStyles[walkingStyleIndex].Replace("move_m@", "").Replace("move_f@", "").Replace("@", " ").AddSpacesToCamelCase().Replace("_", " ").ToTitleCase(),
                    OnChange = new Action<int, string, MenuItemHorNamedSelector>(async (selectedAlternative, selName, menuItem) => {
                        walkingStyleIndex = selectedAlternative;
                        if(walkingStyleIndex == 0)
                        {
                            Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, Game.PlayerPed.Handle);
                        }
                        else
                        {
                            Function.Call(Hash.REQUEST_CLIP_SET, walkingStyles[selectedAlternative]);
                            Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, ped.Handle, walkingStyles[selectedAlternative], 0.25f);
                            await BaseScript.Delay(1000);
                            if (walkingStyleIndex == selectedAlternative) Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, ped.Handle, walkingStyles[selectedAlternative], 0.25f);
                        }
                    })
                });

                visibleItems = _menuItems.Slice(visibleItems.IndexOf(menuItems.First()), visibleItems.IndexOf(menuItems.Last()));
                menuItems = _menuItems;
                SelectedIndex = SelectedIndex; // refreshes state
            }
        }

        public static void Init()
        {
            MenuOptions PedMenuOptions = new MenuOptions { Origin = new PointF(700, 200) };
            PedMenu = new PedMenuModel { numVisibleItems = 7 };
            PedMenu.headerTitle = "Character Customization";
            PedMenu.statusTitle = "";
            PedMenu.menuItems = new List<MenuItem>() { new MenuItemStandard { Title = "Populating menu..." } }; // Currently we need at least one item in a menu; could make it work differently, but eh.
            //MenuManager.TestMenu.menuItems.Insert(0, new MenuItemSubMenu { Title = $"Character Customization", SubMenu = PedMenu });
            //MenuManager.TestMenu.SelectedIndex = 0;
            
            InteractionListMenu.RegisterInteractionMenuItem(new MenuItemSubMenu
            {
                Title = $"[DEV] Customize Character",
                SubMenu = PedMenu
            }, () => true, 1000);
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        private static async Task OnTick()
        {
            try
            {
                if (InteractionListMenu.Observer.CurrentMenu == PedMenu)
                {
                    PedMenu.Refresh();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[VEHICLETESTMENU] Exception in OnTick; {ex.Message}");
            }

            await Task.FromResult(0);
        }
    }
}
