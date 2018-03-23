using System.Collections.Generic;
using FamilyRP.Roleplay.SharedClasses;

namespace FamilyRP.Roleplay.Client.Menus.CharacterEditor.MainMenu
{
	class SaveButton : MenuItemStandard
	{
		private CharacterEditorMenu Root;

		public SaveButton( CharacterEditorMenu root ) {
			Root = root;
			Title = "Save character & Play!";
			OnActivate = SaveAndExit;
		}

		private void SaveAndExit( MenuItemStandard m ) {
			Log.ToChat( "CharacterEditorMenu SaveAndExit" );

			// TODO: Proper support for this
			Roleplay.Client.CharacterEditor.Save();
			Roleplay.Client.CharacterEditor.Exit();
		}
	}
}
