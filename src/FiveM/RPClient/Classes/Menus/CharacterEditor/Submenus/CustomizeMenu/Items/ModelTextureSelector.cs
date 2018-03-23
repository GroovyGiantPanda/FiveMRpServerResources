using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace FamilyRP.Roleplay.Client.Menus.CharacterEditor.CustomizeMenu
{
	class ModelTextureSelector : MenuItemStandard
	{
		private CharacterEditorMenu Root;
		private IPedVariation Component;
		private int ComponentID;
		private bool IsProp = false;
		private int AppliedModel = 0;
		private int AppliedTexture = 0;

		private MenuModel Menu;
		private MenuItemHorSelector<int> ModelSelector = null;
		private MenuItemHorSelector<int> TextureSelector = null;

		private int ComponentIndex { get { return ModelSelector != null ? ModelSelector.state : 0; } }
		private int TextureIndex { get { return TextureSelector != null ? TextureSelector.state : 0; } }

		private readonly static IDictionary<string, string> ComponentNames = new Dictionary<string, string>() {
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

		public ModelTextureSelector( CharacterEditorMenu root, IPedVariation component ) {
			Root = root;
			Component = component;

			Title = ComponentNames.ContainsKey( component.ToString() ) ? ComponentNames[component.ToString()] : component.ToString();
			OnActivate = OpenComponentMenu;
			OnSelect = new Action<MenuItem>( ( menuItem ) => { Roleplay.Client.CharacterEditor.LookAtBody(); } );

			// Is this variation a prop or component?
			PedProps propID;
			PedComponents compID;
			if( Enum.TryParse( Component.ToString(), out propID ) ) {
				ComponentID = (int)propID;
				IsProp = true;
				AppliedModel = -1;
			}
			else if( Enum.TryParse( Component.ToString(), out compID ) ) {
				ComponentID = (int)compID;
			}

			Menu = new MenuModel {
				headerTitle = ComponentNames.ContainsKey( component.ToString() ) ? ComponentNames[component.ToString()] : component.ToString(),
				statusTitle = ComponentNames.ContainsKey( component.ToString() ) ? ComponentNames[component.ToString()] : component.ToString()
			};

			// Only add the Change Model button if there is more than one model
			if( component.HasVariations ) {
				ModelSelector = new MenuItemHorSelector<int> {
					Title = "Model",
					Type = MenuItemHorizontalSelectorType.Number,
					state = 0,
					minState = 0,
					maxState = component.Count - 1,
					wrapAround = true,
					overrideDetailWith = string.Format( "{0}/{1}", component.Index + 1, component.Count ),
					OnChange = SetNewModel
				};

				Menu.menuItems.Add( ModelSelector );
			}

			// Only add the Change Texture button if there is more than one texture for this model
			if( component.HasTextureVariations ) {
				TextureSelector = new MenuItemHorSelector<int> {
					Title = "Texture",
					Type = MenuItemHorizontalSelectorType.Number,
					state = 0,
					minState = 0,
					maxState = component.TextureCount - 1,
					wrapAround = true,
					overrideDetailWith = string.Format( "{0}/{1}", component.TextureIndex + 1, component.TextureCount ),
					OnChange = SetNewTexture
				};

				Menu.menuItems.Add( TextureSelector );
			}

			Menu.menuItems.Add( new MenuItemStandard { Title = "Back", OnActivate = CloseComponentMenu } );
		}

		public override void Refresh() {
			base.Refresh();

			if( ModelSelector != null ) {
				ModelSelector.overrideDetailWith = string.Format( "{0}/{1}", ComponentIndex + 1, Component.Count );
				ModelSelector.maxState = Component.Count;
				ModelSelector.state = 0;
			}

			if( TextureSelector != null ) {
				TextureSelector.overrideDetailWith = string.Format( "{0}/{1}", TextureIndex + 1, Component.TextureCount );
				TextureSelector.maxState = Component.TextureCount;
				TextureSelector.state = 0;
			}
		}

		public override void OnTick( long frameCount, int frameTime, long gameTimer ) {
			base.OnTick( frameCount, frameTime, gameTimer );

			if( Root.Observer.CurrentMenu == Menu ) {
				if( Game.IsDisabledControlJustReleased( 0, Control.FrontendCancel ) ) {
					CloseComponentMenu( this );
				}
			}
		}

		private void OpenComponentMenu( MenuItemStandard m ) {
			Root.Observer.OpenMenu( Menu );
		}

		private void CloseComponentMenu( MenuItemStandard m ) {
			if( Root.Observer.CurrentMenu == Menu ) {
				Root.Observer.CloseMenu();
			}
		}

		private void ApplyChange() {
			// Component.Index and Component.TextureIndex don't apply props if the model is NPC and therefore we cannot use this cleaner method (FiveM/RAGE bug)
			AppliedModel = ComponentIndex;
			AppliedTexture = TextureIndex;

			if( IsProp ) {
				// Unlike variations, to hide props the index needs to be '-1'. The Component.Count also includes this '-1' so no data is lost.
				if( AppliedModel > 0 ) {
					API.SetPedPropIndex( Game.PlayerPed.Handle, ComponentID, AppliedModel - 1, AppliedTexture, false );
				}
				else {
					API.ClearPedProp( Game.PlayerPed.Handle, ComponentID );
				}
			}
			else {
				API.SetPedComponentVariation( Game.PlayerPed.Handle, ComponentID, AppliedModel, AppliedTexture, 0 );
			}
		}

		private void SetNewModel( int selected, MenuItemHorSelector<int> m ) {
			ApplyChange();
			ModelSelector.overrideDetailWith = string.Format( "{0}/{1}", ComponentIndex + 1, Component.Count );
		}

		private void SetNewTexture( int selected, MenuItemHorSelector<int> m ) {
			ApplyChange();
			TextureSelector.overrideDetailWith = string.Format( "{0}/{1}", TextureIndex + 1, Component.TextureCount );
		}
	}
}