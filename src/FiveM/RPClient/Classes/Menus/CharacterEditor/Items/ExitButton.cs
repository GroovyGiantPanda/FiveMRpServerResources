using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.SharedClasses;

namespace FamilyRP.Roleplay.Client.Menus.CharacterEditor.MainMenu
{
	class ExitButton : MenuItemStandard
	{
		private CharacterEditorMenu Root;
		private MenuModel Prompt;

		public ExitButton( CharacterEditorMenu root ) {
			Root = root;

			Title = "Return to character select";
			OnActivate = OpenExitPrompt;

			Prompt = new MenuModel { headerTitle = "Return to character select?", statusTitle = "All changes will be lost." };
			Prompt.menuItems.Add( new MenuItemStandard { Title = "Yes", OnActivate = ExitWithoutSaving } );
			Prompt.menuItems.Add( new MenuItemStandard { Title = "No", OnActivate = AbortExit } );
		}

		public override void OnTick( long frameCount, int frameTime, long gameTimer ) {
			base.OnTick( frameCount, frameTime, gameTimer );

			if( Root.Observer.CurrentMenu == Prompt ) {
				if( Game.IsDisabledControlJustReleased( 0, Control.FrontendCancel ) ) {
					AbortExit( this );
				}
			}
		}

		private void OpenExitPrompt( MenuItemStandard m ) {
			Root.Observer.OpenMenu( Prompt );
		}

		private void ExitWithoutSaving( MenuItemStandard m ) {
			//TODO: Proper support for this
			Roleplay.Client.CharacterEditor.Exit( false );
			Log.ToChat( "CharacterEditorMenu ExitWithoutSaving" );
		}

		private void AbortExit( MenuItemStandard m ) {
			Root.Observer.CloseMenu();
		}
	}
}
