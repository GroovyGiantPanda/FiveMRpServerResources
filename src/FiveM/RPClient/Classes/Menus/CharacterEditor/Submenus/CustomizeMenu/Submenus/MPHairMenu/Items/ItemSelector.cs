using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using FamilyRP.Common;

namespace FamilyRP.Roleplay.Client.Menus.CharacterEditor.MPHairMenu
{
	class ItemSelector : MenuItemHorSelector<int>
	{
		private CustomizeMenu.MPHairMenu Parent;

		private int _value;
		public int Value
		{
			get { return _value; }
			private set { _value = MathUtil.Clamp( value, minState, maxState ); }
		}

		public ItemSelector( CustomizeMenu.MPHairMenu parent, string title, int max ) {
			Parent = parent;
			Title = title;
			Type = MenuItemHorizontalSelectorType.Number;
			wrapAround = true;
			minState = 0;
			maxState = max;
			state = 0;
			Value = state;
			OnChange = SetNewAppearance;
		}

		private void SetNewAppearance( int value, MenuItemHorSelector<int> item ) {
			Value = value;
			Parent.SetNewAppearance();
		}
	}
}
