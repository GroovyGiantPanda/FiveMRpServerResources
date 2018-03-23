using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Menus.CharacterEditor.MPBodyMenu;

namespace FamilyRP.Roleplay.Client.Menus.CharacterEditor.CustomizeMenu
{
	class MPBodyMenu : MenuModel
	{
		private CharacterEditorMenu Root;

		ItemSelector FatherHead;
		ItemSelector MotherHead;
		ItemSlider HeadWeight;
		ItemSelector FacialFeatures;
		ItemSlider FacialMorphStrength;
		ItemSelector FatherSkin;
		ItemSelector MotherSkin;
		ItemSlider SkinWeight;

		public MPBodyMenu( CharacterEditorMenu root ) {
			Root = root;

			headerTitle = "Body";
			statusTitle = "";

			FatherHead = new ItemSelector( this, "Father's face", 45 );
			MotherHead = new ItemSelector( this, "Mother's face", 45 );
			HeadWeight = new ItemSlider( this, "Father/Mother face blend" );
			FacialFeatures = new ItemSelector( this, "Facial features", 20 );
			FacialMorphStrength = new ItemSlider( this, "Facial features blend", -2.0f, 2.0f, 0.0f, 0.5f );
			FatherSkin = new ItemSelector( this, "Father's skin color", 45 );
			MotherSkin = new ItemSelector( this, "Mother's skin color", 45 );
			SkinWeight = new ItemSlider( this, "Father/Mother skin blend" );

			menuItems.Add( FatherHead );
			menuItems.Add( MotherHead );
			menuItems.Add( HeadWeight );
			menuItems.Add( FacialFeatures );
			menuItems.Add( FacialMorphStrength );
			menuItems.Add( FatherSkin );
			menuItems.Add( MotherSkin );
			menuItems.Add( SkinWeight );
			menuItems.Add( new MenuItemStandard { Title = "Back", OnActivate = CloseMenu } );
		}

		public void SetNewAppearance() {
			API.SetPedHeadBlendData( Game.PlayerPed.Handle, FatherHead.Value, MotherHead.Value, 0, FatherSkin.Value, MotherSkin.Value, 0, HeadWeight.Value, SkinWeight.Value, 0.0f, false );
			API.SetPedFaceFeature( Game.PlayerPed.Handle, FacialFeatures.Value, FacialMorphStrength.Value );

			Root.AdditionalSaveData.FatherHead = (byte)FatherHead.Value;
			Root.AdditionalSaveData.MotherHead = (byte)MotherHead.Value;
			Root.AdditionalSaveData.FatherSkin = (byte)FatherSkin.Value;
			Root.AdditionalSaveData.MotherSkin = (byte)MotherSkin.Value;
			Root.AdditionalSaveData.HeadWeight = HeadWeight.Value;
			Root.AdditionalSaveData.SkinWeight = SkinWeight.Value;
			Root.AdditionalSaveData.FacialFeatures[FacialFeatures.Value] = FacialMorphStrength.Value;
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
