using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Common;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Environment.UI
{
	/// <summary>
	/// List menu
	/// </summary>
	class InteractionListMenu
	{
		public static MenuObserver Observer;
		public static MenuModel InteractionMenu;
		public static List<Tuple<int, MenuItem, Func<bool>>> ItemsAll = new List<Tuple<int, MenuItem, Func<bool>>>();
		internal static List<MenuItem> ItemsFiltered = new List<MenuItem>();
		public static bool IsDirty = false;

		//TODO: Move to some better home
		
		static List<string> walkingStyles = new List<string>() { "Reset to default", "move_m@tough_guy@", "move_m@alien", "move_m@non_chalant", "move_m@hobo@a", "move_m@money", "move_m@swagger", "move_m@joy", "move_m@powerwalk", "move_m@posh@", "move_m@sad@a", "move_m@shadyped@a", "move_m@tired", "move_f@sexy", "move_m@drunk@slightlydrunk", "move_m@drunk@verydrunk", "move_f@arrogant@a", "move_f@arrogant@b", "move_f@arrogant@c", "move_f@heels@c", "move_f@heels@d", "move_f@sassy", "move_f@posh@", "move_f@maneater", "move_f@chichi", "move_m@swagger" };
		static int walkingStyleIndex = 0;
		static MenuItem WalkingStyleItem = new MenuItemHorNamedSelector
		{
			Title = $"Walking Style",
			state = walkingStyleIndex,
			Type = MenuItemHorizontalSelectorType.NumberAndBar,
			wrapAround = true,
			optionList = walkingStyles,
			overrideDetailWith = walkingStyles[walkingStyleIndex].Replace("move_m@", "").Replace("move_f@", "").Replace("@", " ").Replace("_", " ").ToTitleCase(),
			OnChange = new Action<int, string, MenuItemHorNamedSelector>(async (selectedAlternative, selName, menuItem) =>
			{
				walkingStyleIndex = selectedAlternative;
				if (walkingStyleIndex == 0)
				{
					Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, Game.PlayerPed.Handle);
				}
				else
				{
					Function.Call(Hash.REQUEST_CLIP_SET, walkingStyles[selectedAlternative]);
					Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, Game.PlayerPed.Handle, walkingStyles[selectedAlternative], 0.25f);
					await BaseScript.Delay(1000);
					if (walkingStyleIndex == selectedAlternative) Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, Game.PlayerPed.Handle, walkingStyles[selectedAlternative], 0.25f);
				}
			})
		};

		public static void Init()
		{
			Observer = new MenuObserver();
			InteractionMenu = new MenuModel { numVisibleItems = 10 };
			InteractionMenu.headerTitle = "Interaction Menu";
			InteractionMenu.statusTitle = "";
			InteractionMenu.menuItems = new List<MenuItem>() { new MenuItemStandard { Title = "Populating menu..." } }; // Currently we need at least one item in a menu; could make it work differently, but eh.
			Client.GetInstance().RegisterTickHandler(OnTick);

			RegisterInteractionMenuItem(WalkingStyleItem, () => true, 100);
		}
        
		public static void RegisterInteractionMenuItem(MenuItem item, Func<bool> check, int priority = 100)
		{
			ItemsAll.Add(new Tuple<int, MenuItem, Func<bool>>(priority, item, check));
		}

		private static async Task OnTick()
		{
			try
			{

				if (Observer.CurrentMenu == InteractionMenu)
				{
					(WalkingStyleItem as MenuItemHorNamedSelector).state = walkingStyleIndex;
					(WalkingStyleItem as MenuItemHorNamedSelector).overrideDetailWith = walkingStyles[walkingStyleIndex].Replace("move_m@", "").Replace("move_f@", "").Replace("@", " ").Replace("_", " ").ToTitleCase();

					ItemsFiltered = ItemsAll.Where(m => m.Item3.Invoke()).OrderBy(m => m.Item2.Title).OrderByDescending(m => m.Item1).Select(m => m.Item2).ToList();
					if(!ItemsFiltered.SequenceEqual(InteractionMenu.menuItems) || IsDirty)
					{
						if (ItemsFiltered.Contains(InteractionMenu.SelectedItem))
						{
							int newSelectedIndex = ItemsFiltered.IndexOf(InteractionMenu.SelectedItem);
							InteractionMenu.menuItems = ItemsFiltered;
							InteractionMenu.SelectedIndex = newSelectedIndex;
						}
						else if(InteractionMenu.SelectedIndex.IsBetween(0, ItemsFiltered.Count-1))
						{
							InteractionMenu.menuItems = ItemsFiltered;
							InteractionMenu.SelectedIndex = InteractionMenu.SelectedIndex; // Simply refreshes selection
						}
						else
						{
							InteractionMenu.menuItems = ItemsFiltered;
							InteractionMenu.SelectedIndex = 0;
						}

						IsDirty = false;
					}
				}

				Observer.Tick();

				if (ControlHelper.IsControlJustPressed(Control.InteractionMenu))
				{
					if (Observer.CurrentMenu == null)
					{
						Observer.OpenMenu(InteractionMenu);
					}
					else
					{
						Observer.CloseMenu(true);
					}
					MenuController.PlaySound("NAV_UP_DOWN");
				}
			}
			catch (Exception ex)
			{
				Log.Error($"[INTERACTIONLISTMENU] Exception in OnTick; {ex.Message}");
			}

			await Task.FromResult(0);
		}

		public void RefreshInteractionMenu()
		{

		}
	}
}
