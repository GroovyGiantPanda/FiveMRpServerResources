using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.Client.Menus.CharacterEditor.MainMenu;
using FamilyRP.Roleplay.SharedModels;

namespace FamilyRP.Roleplay.Client
{
	class CharacterEditorMenu : MenuModel
	{
		public CharacterEditor Editor { get; private set; }
		public MenuObserver Observer { get; private set; }
		public PedData AdditionalSaveData { get; private set; } = new PedData();

		public CharacterEditorMenu() {
			headerTitle = "Character designer";
			statusTitle = "";

			//menuItems.Add( new GenderSelector( this ) );
			menuItems.Add( new ModelSelector( this ) );
			menuItems.Add( new CustomizeButton( this ) );
			menuItems.Add( new SaveButton( this ) );
			menuItems.Add( new ExitButton( this ) );

			Observer = new MenuObserver();
			Observer.OpenMenu( this );

			// Remove default ESC, Backspace & RMB behaviour
			Observer.Controller.controlBinds.Remove( Control.PhoneCancel );

			Client.ActiveInstance.RegisterTickHandler( Update );
		}

		~CharacterEditorMenu() {
			Observer.CloseMenu( true );

			Client.ActiveInstance.DeregisterTickHandler( Update );
		}

		public override void Refresh() {
			base.Refresh();

			foreach( MenuItem item in menuItems ) {
				item.Refresh();
			}
		}

		private async Task Update() {
			long frameCount = Function.Call<long>( Hash.GET_FRAME_COUNT );
			int frameTime = Function.Call<int>( Hash.GET_FRAME_TIME );
			long gameTimer = Function.Call<long>( Hash.GET_GAME_TIMER );

			foreach( MenuItem item in menuItems ) {
				item.OnTick( frameCount, frameTime, gameTimer );
			}

			Observer.Tick();

			await Task.FromResult( 0 );
		}
	}
}