using System;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Menus.CharacterEditor.MPOverlayMenu;
using FamilyRP.Roleplay.SharedModels;

namespace FamilyRP.Roleplay.Client.Menus.CharacterEditor.CustomizeMenu
{
	class MPOverlayMenu : MenuModel
	{
		private CharacterEditorMenu Root;

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

		public MPOverlayMenu( CharacterEditorMenu root ) {
			Root = root;

			headerTitle = "Overlays";
			statusTitle = "";

			overlays.ForEach( o =>
			{
				menuItems.Add( new MenuItemHorNamedSelector {
					Title = o.Name,
					state = o.OptionValues.IndexOf( Function.Call<int>( Hash._GET_PED_HEAD_OVERLAY_VALUE, Game.PlayerPed.Handle, o.ID ) ),
					Type = MenuItemHorizontalSelectorType.NumberAndBar,
					wrapAround = true,
					optionList = o.OptionNames,
					OnChange = new Action<int, string, MenuItemHorNamedSelector>( ( selectedAlternative, selName, menuItem ) => {
						Function.Call( Hash.SET_PED_HEAD_OVERLAY, Game.PlayerPed.Handle, o.ID, o.OptionValues[selectedAlternative], 1f );

						if( !Root.AdditionalSaveData.HeadOverlays.ContainsKey( (byte)o.ID ) )
							Root.AdditionalSaveData.HeadOverlays[(byte)o.ID] = new PedData.HeadOverlay();

						Root.AdditionalSaveData.HeadOverlays[(byte)o.ID].Variant = (byte)o.OptionValues[selectedAlternative];
						Root.AdditionalSaveData.HeadOverlays[(byte)o.ID].Opacity = 1.0f; // Setting default opacity of 1f
					} )
				} );
				if( o.colorType != 0 ) {
					if( !overlayColors.ContainsKey( o.ID ) ) overlayColors[o.ID] = 0;
					menuItems.Add( new MenuItemHorSelector<int> {
						Title = $"{o.Name}: Color",
						state = overlayColors[o.ID],
						Type = MenuItemHorizontalSelectorType.NumberAndBar,
						wrapAround = true,
						minState = 0,
						maxState = 63,
						OnChange = new Action<int, MenuItemHorSelector<int>>( ( selectedAlternative, menuItem ) => {
							overlayColors[o.ID] = selectedAlternative; Function.Call( Hash._SET_PED_HEAD_OVERLAY_COLOR, Game.PlayerPed.Handle, o.ID, o.colorType, selectedAlternative, selectedAlternative );

							if( !Root.AdditionalSaveData.HeadOverlays.ContainsKey( (byte)o.ID ) )
								Root.AdditionalSaveData.HeadOverlays[(byte)o.ID] = new PedData.HeadOverlay();

							Root.AdditionalSaveData.HeadOverlays[(byte)o.ID].PrimaryColor = (byte)selectedAlternative;
							Root.AdditionalSaveData.HeadOverlays[(byte)o.ID].SecondaryColor = (byte)selectedAlternative;
						} )
					} );
				}
			} );

			menuItems.Add( new MenuItemStandard { Title = "Back", OnActivate = CloseMenu } );
		}

		public override void OnTick( long frameCount, int frameTime, long gameTimer ) {
			base.OnTick( frameCount, frameTime, gameTimer );

			foreach( MenuItem item in menuItems ) {
				item.OnTick( frameCount, frameTime, gameTimer );
			}

			if( Root.Observer.CurrentMenu == this ) {
				if( Game.IsDisabledControlJustReleased( 0, Control.FrontendCancel ) ) {
					CloseMenu( null );
				}

				SelectedIndex = MathUtil.Clamp( SelectedIndex, 0, menuItems.Count - 1 );
			}
		}

		private void CloseMenu( MenuItemStandard m ) {
			if( Root.Observer.CurrentMenu == this ) {
				Root.Observer.CloseMenu();
			}
		}
	}
}
