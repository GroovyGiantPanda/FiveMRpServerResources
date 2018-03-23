using System.Collections.Generic;
using FamilyRP.Roleplay.SharedClasses;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;
using System;

namespace FamilyRP.Roleplay.Client.Menus.CharacterEditor.MainMenu
{
	class ModelSelector : MenuItemHorNamedSelector
	{
		private CharacterEditorMenu Root;

        private static readonly List<string> Options = new List<string>()
        {
			"mp_m_freemode_01", "mp_f_freemode_01",
			"g_f_y_lost_01", "g_m_y_lost_01", "g_m_y_lost_02", "g_m_y_lost_03", "a_f_y_vinewood_01", "a_f_y_vinewood_04", "a_m_y_vinewood_01", "a_m_y_vinewood_03", "g_f_y_vagos_01", "g_m_y_salvagoon_01", "g_m_y_salvagoon_02", "g_m_y_salvagoon_03", "s_m_m_trucker_01", "a_f_y_topless_01", "s_f_y_sweatshop_01", "s_f_y_stripper_01", "s_f_y_stripper_02", "a_f_y_skater_01", "a_m_m_skater_01", "s_m_y_robber_01", "a_f_y_rurmeth_01", "a_m_m_rurmeth_01", "g_m_m_armgoon_01", "g_m_m_chigoon_01", "g_m_m_chigoon_02", "g_m_y_armgoon_02", "g_m_y_mexgoon_01", "g_m_y_mexgoon_02", "a_f_o_indian_01", "a_f_y_indian_01", "a_m_m_indian_01", "a_m_y_indian_01", "s_f_y_hooker_01", "s_f_y_hooker_02", "s_f_y_hooker_03", "u_f_y_hotposh_01", "a_f_y_hipster_01", "a_m_y_hipster_01", "a_f_y_hippie_01", "u_m_y_hippie_01", "a_f_m_bevhills_01", "a_f_y_bevhills_01", "a_f_y_bevhills_02", "a_f_y_bevhills_03", "a_f_y_bevhills_04", "a_m_y_bevhills_02", "g_f_y_ballas_01", "g_m_y_ballasout_01", "a_f_y_business_01", "a_f_y_business_02", "a_f_y_business_03", "a_f_y_business_04", "a_m_m_business_01", "a_m_y_business_01", "a_m_y_business_02", "a_m_y_business_03", "A_F_M_Beach_01", "A_F_M_BevHills_02", "A_F_M_BodyBuild_01", "A_F_M_Business_02", "A_F_M_Downtown_01", "A_F_M_EastSA_01", "A_F_M_EastSA_02", "A_F_M_FatBla_01", "A_F_M_FatWhite_01", "A_F_M_KTown_01", "A_F_M_KTown_02", "A_F_M_ProlHost_01", "A_F_M_Salton_01", "A_F_M_SkidRow_01", "A_F_M_SouCentMC_01", "A_F_M_SouCent_01", "A_F_M_SouCent_02", "A_F_M_Tourist_01", "A_F_M_TrampBeac_01", "A_F_M_Tramp_01", "A_F_O_GenStreet_01", "A_F_O_KTown_01", "A_F_O_Salton_01", "A_F_O_SouCent_01", "A_F_O_SouCent_02", "A_F_Y_Beach_01", "A_F_Y_EastSA_01", "A_F_Y_EastSA_02", "A_F_Y_EastSA_03", "A_F_Y_Epsilon_01", "A_F_Y_Fitness_01", "A_F_Y_Fitness_02", "A_F_Y_GenHot_01", "A_F_Y_Golfer_01", "A_F_Y_Hiker_01", "A_F_Y_Hipster_02", "A_F_Y_Hipster_03", "A_F_Y_Hipster_04", "A_F_Y_Juggalo_01", "A_F_Y_Runner_01", "A_F_Y_SCDressy_01", "A_F_Y_SouCent_01", "A_F_Y_SouCent_02", "A_F_Y_SouCent_03", "A_F_Y_Tennis_01", "A_F_Y_Tourist_01", "A_F_Y_Tourist_02", "A_F_Y_Vinewood_02", "A_F_Y_Vinewood_03", "A_F_Y_Yoga_01", "A_M_M_AfriAmer_01", "A_M_M_Beach_01", "A_M_M_Beach_02", "A_M_M_BevHills_01", "A_M_M_BevHills_02", "A_M_M_EastSA_01", "A_M_M_EastSA_02", "A_M_M_Farmer_01", "A_M_M_FatLatin_01", "A_M_M_GenFat_01", "A_M_M_GenFat_02", "A_M_M_Golfer_01", "A_M_M_HasJew_01", "A_M_M_Hillbilly_01", "A_M_M_Hillbilly_02", "A_M_M_KTown_01", "A_M_M_Malibu_01", "A_M_M_MexCntry_01", "A_M_M_MexLabor_01", "A_M_M_OG_Boss_01", "A_M_M_Paparazzi_01", "A_M_M_Polynesian_01", "A_M_M_ProlHost_01", "A_M_M_Salton_01", "A_M_M_Salton_02", "A_M_M_Salton_03", "A_M_M_Salton_04", "A_M_M_Skidrow_01", "A_M_M_SoCenLat_01", "A_M_M_SouCent_01", "A_M_M_SouCent_02", "A_M_M_SouCent_03", "A_M_M_SouCent_04", "A_M_M_StLat_02", "A_M_M_Tennis_01", "A_M_M_Tourist_01", "A_M_M_TrampBeac_01", "A_M_M_Tramp_01", "A_M_M_TranVest_01", "A_M_M_TranVest_02", "A_M_O_ACult_02", "A_M_O_Beach_01", "A_M_O_GenStreet_01", "A_M_O_KTown_01", "A_M_O_Salton_01", "A_M_O_SouCent_01", "A_M_O_SouCent_02", "A_M_O_SouCent_03", "A_M_O_Tramp_01", "A_M_Y_ACult_02", "A_M_Y_BeachVesp_01", "A_M_Y_BeachVesp_02", "A_M_Y_Beach_01", "A_M_Y_Beach_02", "A_M_Y_Beach_03", "A_M_Y_BevHills_01", "A_M_Y_BreakDance_01", "A_M_Y_BusiCas_01", "A_M_Y_Cyclist_01", "A_M_Y_DHill_01", "A_M_Y_Downtown_01", "A_M_Y_EastSA_01", "A_M_Y_EastSA_02", "A_M_Y_Epsilon_01", "A_M_Y_Epsilon_02", "A_M_Y_Gay_01", "A_M_Y_Gay_02", "A_M_Y_GenStreet_01", "A_M_Y_GenStreet_02", "A_M_Y_Golfer_01", "A_M_Y_HasJew_01", "A_M_Y_Hiker_01", "A_M_Y_Hippy_01", "A_M_Y_Hipster_02", "A_M_Y_Hipster_03", "A_M_Y_Jetski_01", "A_M_Y_Juggalo_01", "A_M_Y_KTown_01", "A_M_Y_KTown_02", "A_M_Y_Latino_01", "A_M_Y_MethHead_01", "A_M_Y_MexThug_01", "A_M_Y_MotoX_01", "A_M_Y_MotoX_02", "A_M_Y_MusclBeac_01", "A_M_Y_MusclBeac_02", "A_M_Y_Polynesian_01", "A_M_Y_RoadCyc_01", "A_M_Y_Runner_01", "A_M_Y_Runner_02", "A_M_Y_Salton_01", "A_M_Y_Skater_01", "A_M_Y_Skater_02", "A_M_Y_SouCent_01", "A_M_Y_SouCent_02", "A_M_Y_SouCent_03", "A_M_Y_SouCent_04", "A_M_Y_StBla_01", "A_M_Y_StBla_02", "A_M_Y_StLat_01", "A_M_Y_StWhi_01", "A_M_Y_StWhi_02", "A_M_Y_Sunbathe_01", "A_M_Y_Surfer_01", "A_M_Y_VinDouche_01", "A_M_Y_Vinewood_02", "A_M_Y_Vinewood_04", "A_M_Y_Yoga_01", "CSB_Car3guy1", "CSB_Car3guy2", "CSB_Chin_goon", "CSB_Denise_friend", "CSB_FOS_rep", "CSB_Grove_str_dlr", "CSB_Ramp_gang", "CSB_Ramp_hic", "CSB_Ramp_hipster", "CSB_Ramp_mex", "CSB_Screen_Writer", "CSB_Stripper_01", "CSB_Stripper_02", "CS_FBISuit_01", "CS_LifeInvad_01", "CS_MovPremF_01", "CS_MRS_Thornhill", "CS_Old_Man1A", "CS_Old_Man2", "G_F_Y_Families_01", "G_M_M_ArmBoss_01", "G_M_M_ArmLieut_01", "G_M_M_ChiBoss_01", "G_M_M_ChiCold_01", "G_M_M_KorBoss_01", "G_M_M_MexBoss_01", "G_M_M_MexBoss_02", "G_M_Y_Azteca_01", "G_M_Y_BallaEast_01", "G_M_Y_BallaOrig_01", "G_M_Y_FamCA_01", "G_M_Y_FamDNF_01", "G_M_Y_FamFor_01", "G_M_Y_Korean_01", "G_M_Y_Korean_02", "G_M_Y_KorLieut_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_02", "G_M_Y_MexGoon_03", "G_M_Y_PoloGoon_01", "G_M_Y_PoloGoon_02", "G_M_Y_SalvaBoss_01", "G_M_Y_StrPunk_01", "G_M_Y_StrPunk_02", "HC_Hacker", "IG_Abigail", "IG_AmandaTownley", "IG_Ashley", "IG_BallasOG", "IG_Bankman", "IG_Barry", "IG_BestMen", "IG_Brad", "IG_Bride", "IG_Car3guy1", "IG_Car3guy2", "IG_ChengSr", "IG_ChrisFormage", "IG_Clay", "IG_ClayPain", "IG_Cletus", "IG_Dale", "IG_DaveNorton", "IG_Denise", "IG_Devin", "IG_Dom", "IG_Dreyfuss", "IG_DrFriedlander", "IG_Floyd", "IG_Groom", "IG_Hao", "IG_Janet", "ig_JAY_Norris", "IG_JewelAss", "IG_JimmyBoston", "IG_JimmyDiSanto", "IG_JoeMinuteMan", "IG_Josef", "IG_Josh", "IG_KerryMcIntosh", "IG_LamarDavis", "IG_Lazlow", "IG_LesterCrest", "IG_LifeInvad_01", "IG_LifeInvad_02", "IG_Magenta", "IG_Manuel", "IG_Marnie", "IG_MaryAnn", "IG_Maude", "IG_Milton", "IG_Molly", "IG_MRK", "IG_MrsPhillips", "IG_MRS_Thornhill", "IG_Natalia", "IG_NervousRon", "IG_Nigel", "IG_Old_Man1A", "IG_Old_Man2", "IG_Omega", "IG_ONeil", "IG_Ortega", "IG_Paper", "IG_Patricia", "IG_Priest", "IG_Ramp_Gang", "IG_Ramp_Hic", "IG_Ramp_Hipster", "IG_Ramp_Mex", "IG_RoccoPelosi", "IG_RussianDrunk", "IG_Screen_Writer", "IG_SiemonYetarian", "IG_Solomon", "IG_SteveHains", "IG_Stretch", "IG_Talina", "IG_Tanisha", "IG_TaoCheng", "IG_TaosTranslator", "ig_TennisCoach", "IG_Terry", "IG_TomEpsilon", "IG_Tonya", "IG_TracyDiSanto", "IG_TrafficWarden", "IG_TylerDix", "IG_Wade", "IG_Zimbor", "MP_F_StripperLite", "MP_G_M_Pros_01", "MP_M_ExArmy_01", "MP_M_FamDD_01", "MP_M_ShopKeep_01", "Player_One", "Player_Two", "Player_Zero", "S_F_M_Fembarber", "S_F_M_Maid_01", "S_F_M_Shop_HIGH", "S_F_M_SweatShop_01", "S_F_Y_AirHostess_01", "S_F_Y_Bartender_01", "S_F_Y_Baywatch_01", "S_F_Y_MovPrem_01", "S_F_Y_Shop_LOW", "S_F_Y_Shop_MID", "S_F_Y_StripperLite", "S_M_M_AmmuCountry", "S_M_M_AutoShop_01", "S_M_M_AutoShop_02", "S_M_M_Bouncer_01", "S_M_M_CntryBar_01", "S_M_M_DockWork_01", "S_M_M_Gaffer_01", "S_M_M_Gardener_01", "S_M_M_GenTransport", "S_M_M_HairDress_01", "S_M_M_HighSec_01", "S_M_M_HighSec_02", "S_M_M_Janitor", "S_M_M_LatHandy_01", "S_M_M_LifeInvad_01", "S_M_M_Linecook", "S_M_M_LSMetro_01", "S_M_M_Mariachi_01", "S_M_M_Migrant_01", "S_M_M_MovAlien_01", "S_M_M_MovPrem_01", "S_M_M_Postal_01", "S_M_M_Postal_02", "S_M_M_StrPreach_01", "S_M_M_StrVend_01", "S_M_M_UPS_01", "S_M_M_UPS_02", "S_M_O_Busker_01", "S_M_Y_AirWorker", "S_M_Y_AmmuCity_01", "S_M_Y_Barman_01", "S_M_Y_BayWatch_01", "S_M_Y_BusBoy_01", "S_M_Y_Chef_01", "S_M_Y_Construct_01", "S_M_Y_Construct_02", "S_M_Y_Dealer_01", "S_M_Y_DevinSec_01", "S_M_Y_DockWork_01", "S_M_Y_Doorman_01", "S_M_Y_Grip_01", "S_M_Y_Shop_MASK", "S_M_Y_StrVend_01", "S_M_Y_Valet_01", "S_M_Y_Waiter_01", "S_M_Y_WinClean_01", "S_M_Y_XMech_01", "S_M_Y_XMech_02", "U_F_M_Miranda", "U_F_M_ProMourn_01", "U_F_O_MovieStar", "U_F_O_ProlHost_01", "U_F_Y_BikerChic", "U_F_Y_COMJane", "U_F_Y_JewelAss_01", "U_F_Y_Mistress", "U_F_Y_PoppyMich", "U_F_Y_Princess", "U_F_Y_SpyActress", "U_M_M_Aldinapoli", "U_M_M_BankMan", "U_M_M_BikeHire_01", "U_M_M_FIBArchitect", "U_M_M_FilmDirector", "U_M_M_GlenStank_01", "U_M_M_Griff_01", "U_M_M_Jesus_01", "U_M_M_JewelSec_01", "U_M_M_JewelThief", "U_M_M_MarkFost", "U_M_M_PartyTarget", "U_M_M_ProMourn_01", "U_M_M_RivalPap", "U_M_M_SpyActor", "U_M_M_WillyFist", "U_M_O_FinGuru_01", "U_M_O_TapHillBilly", "U_M_O_Tramp_01", "U_M_Y_Abner", "U_M_Y_AntonB", "U_M_Y_BabyD", "U_M_Y_Baygor", "U_M_Y_BurgerDrug_01", "U_M_Y_Chip", "U_M_Y_Cyclist_01", "U_M_Y_FIBMugger_01", "U_M_Y_Guido_01", "U_M_Y_GunVend_01", "U_M_Y_Mani", "U_M_Y_MilitaryBum", "U_M_Y_Paparazzi", "U_M_Y_Party_01", "U_M_Y_ProlDriver_01", "U_M_Y_SBike", "U_M_Y_StagGrm_01", "U_M_Y_Tattoo_01"
        };

		public ModelSelector( CharacterEditorMenu root ) {
			Root = root;

			Title = "Model";
			Type = MenuItemHorizontalSelectorType.Number;
			state = 0;
			optionList = Options;
			wrapAround = true;
			OnChange = SetNewModel;
		}

		private void SetNewModel( int index, string label, MenuItemHorNamedSelector m ) {
			Task.Factory.StartNew( async () => {
				try {
					PedHash pedHash = (PedHash)Game.GenerateHash( label );

					if( await Game.Player.ChangeModel( new Model( pedHash ) ) ) {
						API.SetPedDefaultComponentVariation( Game.PlayerPed.Handle );

						if( pedHash == PedHash.FreemodeMale01 || pedHash == PedHash.FreemodeFemale01 ) {
							API.SetPedHeadBlendData( Game.PlayerPed.Handle, 0, 0, 0, 0, 0, 0, 0, 0, 0, false );
						}
					}
				}
				catch( Exception ex ) {
					Log.Error( ex );
				}

				Root.Refresh();
			} );
		}
	}
}