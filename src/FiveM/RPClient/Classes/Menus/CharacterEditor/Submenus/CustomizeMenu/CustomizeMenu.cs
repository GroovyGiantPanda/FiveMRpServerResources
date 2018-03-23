using System.Collections.Generic;
using CitizenFX.Core;
using FamilyRP.Roleplay.Client.Menus.CharacterEditor.CustomizeMenu;

namespace FamilyRP.Roleplay.Client.Menus.CharacterEditor.MainMenu
{
	class CustomizeMenu : MenuModel
	{
		private CharacterEditorMenu Root;

		public CustomizeMenu( CharacterEditorMenu root ) {
			Root = root;

			headerTitle = "Customizable";
			statusTitle = "Customizable your model";

			PopulateMenu();
		}

		public override void Refresh() {
			base.Refresh();
			PopulateMenu();
		}

		public override void OnTick( long frameCount, int frameTime, long gameTimer ) {
			base.OnTick( frameCount, frameTime, gameTimer );

			foreach( MenuItem item in menuItems ) {
				item.OnTick( frameCount, frameTime, gameTimer );
			}

			if( Root.Observer.CurrentMenu == this ) {
				if( Game.IsDisabledControlJustReleased( 0, Control.FrontendCancel ) ) {
					CloseCustomizeMenu( null );
				}

				if( SelectedIndex > menuItems.Count - 1 )
					SelectedIndex = menuItems.Count - 1;
			}
		}

		private void PopulateMenu() {
			List<MenuItem> menuItemsBuffer = new List<MenuItem>();

			if( (PedHash)Game.PlayerPed.Model.Hash == PedHash.FreemodeMale01 || (PedHash)Game.PlayerPed.Model.Hash == PedHash.FreemodeFemale01 ) {
				menuItemsBuffer.Add( new MPBodyButton( Root ) );
				menuItemsBuffer.Add( new MPHairButton( Root ) );
				menuItemsBuffer.Add( new MPEyeSelector( Root ) );
				menuItemsBuffer.Add( new MPOverlayButton( Root ) );

				IPedVariation[] variations = Game.PlayerPed.Style.GetAllVariations();

				foreach( IPedVariation variation in variations ) {
					if( variation.ToString() == "Face" || variation.ToString() == "Hair" )
						continue;

					if( variation.HasAnyVariations ) {
						menuItemsBuffer.Add( new ModelTextureSelector( Root, variation ) );
					}
				}
			}
			else {
				IPedVariation[] variations = Game.PlayerPed.Style.GetAllVariations();

				foreach( IPedVariation variation in variations ) {
					if( variation.HasAnyVariations ) {
						menuItemsBuffer.Add( new ModelTextureSelector( Root, variation ) );
					}
				}
			}

			menuItemsBuffer.Add( new MenuItemStandard { Title = "Back", OnActivate = CloseCustomizeMenu } );

			menuItems = menuItemsBuffer;
			SelectedIndex = 0;
		}

		private void CloseCustomizeMenu( MenuItemStandard m ) {
			if( Root.Observer.CurrentMenu == this ) {
				Root.Observer.CloseMenu();
			}
		}
	}
}
