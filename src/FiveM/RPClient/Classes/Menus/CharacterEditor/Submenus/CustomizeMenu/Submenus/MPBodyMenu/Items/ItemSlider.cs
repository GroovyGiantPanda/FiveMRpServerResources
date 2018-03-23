using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using FamilyRP.Common;

namespace FamilyRP.Roleplay.Client.Menus.CharacterEditor.MPBodyMenu
{
	class ItemSlider : MenuItemHorSelector<float>
	{
		private CustomizeMenu.MPBodyMenu Parent;

		private float _value;
		public float Value
		{
			get { return _value; }
			private set { _value = MathUtil.Clamp( value, minState, maxState ); }
		}

		public ItemSlider( CustomizeMenu.MPBodyMenu parent, string title ) {
			Parent = parent;
			Title = title;
			Type = MenuItemHorizontalSelectorType.NumberAndBar;
			wrapAround = false;
			minState = 0.0f;
			maxState = 1.0f;
			stepSize = 0.05f;
			state = 0.5f;
			Value = state;
			OnChange = SetNewAppearance;
		}

		public ItemSlider( CustomizeMenu.MPBodyMenu parent, string title, float min, float max, float defaultValue, float increment ) {
			if( min >= max ) throw new Exception( "Min value cannot be bigger or equal to max value" );
			if( ((max - min) / increment) % 1.0f != 0.0f ) throw new Exception( "Min-max range must be wholly divisible by given increment value" );

			Parent = parent;
			Title = title;
			Type = MenuItemHorizontalSelectorType.NumberAndBar;
			wrapAround = true;
			minState = min;
			maxState = max;
			stepSize = increment;
			state = MathUtil.Clamp( defaultValue, min, max );
			Value = state;
			OnChange = SetNewAppearance;
		}

		private void SetNewAppearance( float value, MenuItemHorSelector<float> item ) {
			Value = value;
			Parent.SetNewAppearance();
		}
	}
}
