using System.Collections.Generic;

namespace FamilyRP.Roleplay.Client.Menus.CharacterEditor.MainMenu
{
	class CustomizeButton : MenuItemSubMenu
	{
		public CustomizeButton( CharacterEditorMenu root ) {
			Title = "Customize";
			SubMenu = new CustomizeMenu( root );
		}

		public override void Refresh() {
			base.Refresh();
			SubMenu.Refresh();
		}

		public override void OnTick( long frameCount, int frameTime, long gameTimer ) {
			base.OnTick( frameCount, frameTime, gameTimer );
			SubMenu.OnTick( frameCount, frameTime, gameTimer );
		}
	}
}
