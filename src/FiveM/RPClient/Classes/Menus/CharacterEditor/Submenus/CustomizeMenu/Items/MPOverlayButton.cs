using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace FamilyRP.Roleplay.Client.Menus.CharacterEditor.CustomizeMenu
{
	class MPOverlayButton : MenuItemSubMenu
	{
		public MPOverlayButton( CharacterEditorMenu root ) {
			Title = "Overlays";
			SubMenu = new MPOverlayMenu( root );
			OnSelect = new Action<MenuItem>( ( menuItem ) => { Roleplay.Client.CharacterEditor.LookAtFace(); } );
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