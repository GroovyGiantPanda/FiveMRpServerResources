using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;

namespace FamilyRP.Roleplay.Client
{
	public static class MenuGlobals
	{
		public static bool MuteSounds = false;
		static MenuItemCheckbox menuItem = new MenuItemCheckbox { Title = "Mute Menu Sounds", OnActivate = (state, item) => { MuteSounds = state; } };

		public static void Init()
		{
			InteractionListMenu.RegisterInteractionMenuItem(menuItem, () => true);
		}
	}

	class MenuOptions
	{
		// Defaults
		public float HeaderHeight = 45;
		public float Width = 250;
		public float HeaderBottomMargin = 0;
		public float ItemBottomMargin = 0;
		public float ItemHeight = 20;
		public float ItemTextSize = 0.25f;
		public float HeaderTextSize = 0.6f;
		public Font HeaderFont = Font.HouseScript;
		public Font ItemFont = Font.ChaletLondon;
		public PointF ItemPadding = new PointF(3, 3);
		public PointF HeaderPadding = new PointF(10, 10);
		public PointF Origin = new PointF(200, 200);
		public Color HeaderBackgroundColor = Color.FromArgb(220, 160, 1, 33);
		public Color ItemBackgroundColor = Color.FromArgb(180, 0, 0, 0);
		public Color ItemSelectedBackgroundColor = Color.FromArgb(200, 220, 220, 220);
		public Color ItemTextColor = Color.FromArgb(255, 255, 255, 255);
		public Color ItemSelectedTextColor = Color.FromArgb(255, 0, 0, 0);
		public Color HeaderTextColor = Color.FromArgb(255, 255, 255, 255);
		public float DescriptionMarginTop = 5f;
		public float ItemWithDetailHeight = 30;
		public float ItemHorSelWithBarHeight = 35;
		public float ItemSubDetailTextSize = 0.18f;
		internal PointF MenuItemHorSelOuterBarOffset = new PointF(5, 20);
		internal PointF MenuItemHorSelInnerBarOffset = new PointF(2, 2);
		internal float MenuItemHorSelInnerBarHeight = 6;
		internal float MenuItemHorSelOuterBarHeight = 10;
		internal Color ItemHorSelInnerBgColor = Color.FromArgb(200, 0, 0, 0);
		internal Color ItemHorSelOuterBgColor = Color.FromArgb(200, 210, 210, 210);
		internal Color ItemHorSelSelectedInnerBgColor = Color.FromArgb(200, 160, 1, 33);
		internal Color ItemHorSelSelectedOuterBgColor = Color.FromArgb(200, 0, 0, 0);
		internal float StatusBarHeight = 20;
		internal float StatusBarBottomMargin = 0;
		internal float HeaderTextVerticalOffset = -10;
		internal PointF MenuItemCheckboxOffset = new PointF(-26, -8);
		internal SizeF MenuItemCheckboxSize = new SizeF(30, 30);
		internal Color StatusBarBgColor = Color.FromArgb(255, 0, 0, 0);
		internal Color StatusBarTextColor = Color.FromArgb(255, 255, 255, 255);
	}

	static class UIHelpers
	{

		public static void DrawSprite(string _pinnedDict, string _pinnedName, PointF Position, SizeF Size, SizeF offset, Color Color, bool Centered)
		{
			float screenWidth = Screen.Width;
			float screenHeight = Screen.Height;
			float scaleX = Size.Width / screenWidth;
			float scaleY = Size.Height / screenHeight;
			float positionX = (Position.X + offset.Width) / screenWidth;
			float positionY = (Position.Y + offset.Height) / screenHeight;

			if (!Centered)
			{
				positionX += scaleX * 0.5f;
				positionY += scaleY * 0.5f;
			}
			if (!Function.Call<bool>(Hash.HAS_STREAMED_TEXTURE_DICT_LOADED, _pinnedDict))
				Function.Call(Hash.REQUEST_STREAMED_TEXTURE_DICT, _pinnedDict, true);

			Function.Call(Hash.DRAW_SPRITE, _pinnedDict, _pinnedName, positionX, positionY, scaleX, scaleY, 0, Color.R, Color.G, Color.B, Color.A);
		}

		public static PointF MousePosition()
		{
			int mouseX = (int)Math.Round(Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, (int)Control.CursorX) * CitizenFX.Core.UI.Screen.Width);
			int mouseY = (int)Math.Round(Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, (int)Control.CursorY) * CitizenFX.Core.UI.Screen.Height);
			return new PointF(mouseX, mouseY);
		}
	}

	enum MenuAction
	{
		Down,
		Up,
		Select,
		Cancel,
		Left,
		Right
	}
	enum MenuItemHorizontalSelectorType
	{
		Number,
		Bar,
		NumberAndBar
	}

	class MenuView
	{
		class MenuHeader
		{
			private MenuView menu;
			public SizeF Size;
			bool Initialized = false;

			public MenuHeader(MenuView menu)
			{
				this.menu = menu;
			}

			public void Init()
			{
				Size = new SizeF(menu.current.options.Width, menu.current.options.HeaderHeight);
				Initialized = true;
			}

			public void Draw()
			{
				if (!Initialized)
					Init();
				UIHelpers.DrawSprite("commonmenu", "gradient_nav", menu.current.options.Origin, Size, new SizeF(0f, 0f), menu.current.options.HeaderBackgroundColor, false);
				new Text(menu.current.headerTitle, menu.current.options.Origin.Add(new PointF(menu.current.options.Width / 2, menu.current.options.HeaderHeight / 2 + menu.current.options.HeaderTextVerticalOffset)), menu.current.options.HeaderTextSize, menu.current.options.HeaderTextColor, menu.current.options.HeaderFont, Alignment.Center).Draw();
			}
		}

		class MenuStatusBar
		{
			private MenuView menu;
			MenuOptions o;
			public PointF Position;
			public SizeF Size;
			bool Initialized = false;

			public MenuStatusBar(MenuView menu)
			{
				this.menu = menu;
			}

			public void Init()
			{
				o = menu.current.options;
				Size = new SizeF(o.Width, o.StatusBarHeight);
				Position = o.Origin.Add(new PointF(0, o.HeaderHeight + o.HeaderBottomMargin));
				Initialized = true;
			}

			public void Draw()
			{
				if (!Initialized)
					Init();
				new Rectangle(Position, new SizeF(o.Width, o.StatusBarHeight), o.StatusBarBgColor).Draw();
				new Text(menu.current.statusTitle, Position.Add(o.ItemPadding), o.ItemTextSize, o.StatusBarTextColor).Draw();
				new Text($"{menu.current.SelectedIndex+1}/{menu.current.menuItems.Count}", new PointF(Position.X + o.Width - o.ItemPadding.X, Position.Y + o.ItemPadding.Y), o.ItemTextSize, o.StatusBarTextColor, o.ItemFont, Alignment.Right).Draw();
			}
		}

		class MenuItems
		{
			private MenuView menu;
			static MenuOptions o;
			public PointF Position;
			bool Initialized = false;

			public float TotalHeight { get { return menu.current.visibleItems.Select(i => GetItemHeight(o, i)).Sum(); } }

			public MenuItems(MenuView menu)
			{
				this.menu = menu;
			}

			public static float GetItemHeight(MenuOptions o, object item)
			{
				if (item is MenuItemStandard && ((MenuItemStandard)item).SubDetail != null)
				{
					return o.ItemWithDetailHeight + o.ItemBottomMargin;
				}
				else if ((item is MenuItemHorSelector<int> && (((MenuItemHorSelector<int>)item).Type == MenuItemHorizontalSelectorType.Bar || ((MenuItemHorSelector<int>)item).Type == MenuItemHorizontalSelectorType.NumberAndBar)) || (item is MenuItemHorSelector<float> && (((MenuItemHorSelector<float>)item).Type == MenuItemHorizontalSelectorType.Bar || ((MenuItemHorSelector<float>)item).Type == MenuItemHorizontalSelectorType.NumberAndBar)) || (item is MenuItemHorNamedSelector && (((MenuItemHorNamedSelector)item).Type == MenuItemHorizontalSelectorType.Bar || ((MenuItemHorNamedSelector)item).Type == MenuItemHorizontalSelectorType.NumberAndBar)))
				{
					return o.ItemHorSelWithBarHeight + o.ItemBottomMargin;
				}
				else
				{
					return o.ItemHeight + o.ItemBottomMargin;
				}
			}

			public void Init()
			{
				o = menu.current.options;
				Position = o.Origin.Add(new PointF(0, o.HeaderHeight + o.HeaderBottomMargin + o.StatusBarHeight + o.StatusBarBottomMargin));
				Initialized = true;
			}

			public void Draw()
			{
				try
				{
					if (!Initialized)
						Init();

					MenuOptions o = menu.current.options; // should save a lot of characters below
					PointF itemPosition = Position;
					menu.current.visibleItems.Select((item, index) => new { item, index }).ToList().ForEach(n =>
					{
						var i = n.item;
						bool isSelected = menu.current.SelectedIndex == (menu.current.topMostVisibleItem + n.index);
						if (i is MenuItemStandard)
						{
							if (((MenuItemStandard)i).SubDetail != null)
							{
								new Rectangle(itemPosition, new SizeF(o.Width, o.ItemWithDetailHeight), isSelected ? o.ItemSelectedBackgroundColor : o.ItemBackgroundColor).Draw();
								new Text(i.Title, itemPosition.Add(o.ItemPadding), o.ItemTextSize, isSelected ? o.ItemSelectedTextColor : o.ItemTextColor).Draw();
								if (((MenuItemStandard)i).Detail != null)
								{
									new Text(((MenuItemStandard)i).Detail, new PointF(itemPosition.X + o.Width - o.ItemPadding.X, itemPosition.Y + o.ItemPadding.Y), o.ItemTextSize, isSelected ? o.ItemSelectedTextColor : o.ItemTextColor, o.ItemFont, Alignment.Right).Draw();
								}
								new Text(((MenuItemStandard)i).SubDetail, new PointF(itemPosition.X + o.Width - o.ItemPadding.X, itemPosition.Y + o.ItemPadding.Y + 14f), o.ItemSubDetailTextSize, isSelected ? o.ItemSelectedTextColor : o.ItemTextColor, o.ItemFont, Alignment.Right).Draw();
								itemPosition = itemPosition.Add(new PointF(0, o.ItemWithDetailHeight + o.ItemBottomMargin));
							}
							else
							{
								new Rectangle(itemPosition, new SizeF(o.Width, o.ItemHeight), isSelected ? o.ItemSelectedBackgroundColor : o.ItemBackgroundColor).Draw();
								if (((MenuItemStandard)i).Detail != null)
								{
									new Text(((MenuItemStandard)i).Detail, new PointF(itemPosition.X + o.Width - o.ItemPadding.X, itemPosition.Y + o.ItemPadding.Y), o.ItemTextSize, isSelected ? o.ItemSelectedTextColor : o.ItemTextColor, o.ItemFont, Alignment.Right).Draw();
								}
								new Text(i.Title, itemPosition.Add(o.ItemPadding), o.ItemTextSize, isSelected ? o.ItemSelectedTextColor : o.ItemTextColor).Draw();
								itemPosition = itemPosition.Add(new PointF(0, o.ItemHeight + o.ItemBottomMargin));
							}
						}
						else if (i is MenuItemCheckbox)
						{
							new Rectangle(itemPosition, new SizeF(o.Width, o.ItemHeight), isSelected ? o.ItemSelectedBackgroundColor : o.ItemBackgroundColor).Draw();
							bool isChecked = ((MenuItemCheckbox)i).State;
							string checkboxSprite = isSelected ? (isChecked ? "shop_box_tickb" : "shop_box_blankb") : (isChecked ? "shop_box_tick" : "shop_box_blank");
							UIHelpers.DrawSprite("commonmenu", checkboxSprite, new PointF(itemPosition.X + o.Width - o.ItemPadding.X, itemPosition.Y + o.ItemPadding.Y).Add(o.MenuItemCheckboxOffset), new SizeF(o.MenuItemCheckboxSize), new Size(0, 0), Color.FromArgb(80, 255, 255, 255), false);
							new Text(i.Title, itemPosition.Add(o.ItemPadding), o.ItemTextSize, isSelected ? o.ItemSelectedTextColor : o.ItemTextColor).Draw();
							itemPosition = itemPosition.Add(new PointF(0, o.ItemHeight + o.ItemBottomMargin));
						}
						else if (i is MenuItemHorSelector<int>) // I gave up on trying to combine these two into one with Reflection methods
						{
							MenuItemHorSelector<int> item = i as MenuItemHorSelector<int>;
							MenuItemHorizontalSelectorType type = item.Type;
							int state = item.state;
							int maxState = item.maxState;
							int minState = item.minState;
							bool hasBar = type == MenuItemHorizontalSelectorType.Bar || type == MenuItemHorizontalSelectorType.NumberAndBar;
							bool hasNumber = type == MenuItemHorizontalSelectorType.Number || type == MenuItemHorizontalSelectorType.NumberAndBar;
							new Rectangle(itemPosition, new SizeF(o.Width, hasBar ? o.ItemHorSelWithBarHeight : o.ItemHeight), isSelected ? o.ItemSelectedBackgroundColor : o.ItemBackgroundColor).Draw();
							new Text(i.Title, itemPosition.Add(o.ItemPadding), o.ItemTextSize, isSelected ? o.ItemSelectedTextColor : o.ItemTextColor).Draw();

							if (hasNumber)
								new Text($"{(item.overrideDetailWith != null ? item.overrideDetailWith : $"{state}/{maxState}")}", new PointF(itemPosition.X + o.Width - o.ItemPadding.X, itemPosition.Y + o.ItemPadding.Y), o.ItemTextSize, isSelected ? o.ItemSelectedTextColor : o.ItemTextColor, o.ItemFont, Alignment.Right).Draw();
							if (hasBar)
							{
								float progressFraction = (state - minState) / (float)((maxState - minState));
								new Rectangle(itemPosition.Add(o.MenuItemHorSelOuterBarOffset), new SizeF(o.Width - 2 * o.MenuItemHorSelOuterBarOffset.X, o.MenuItemHorSelOuterBarHeight), isSelected ? o.ItemHorSelSelectedOuterBgColor : o.ItemHorSelOuterBgColor).Draw();
								new Rectangle(itemPosition.Add(o.MenuItemHorSelOuterBarOffset).Add(o.MenuItemHorSelInnerBarOffset), new SizeF(progressFraction * (o.Width - 2 * (o.MenuItemHorSelOuterBarOffset.X + o.MenuItemHorSelInnerBarOffset.X)), o.MenuItemHorSelInnerBarHeight), isSelected ? o.ItemHorSelSelectedInnerBgColor : o.ItemHorSelInnerBgColor).Draw();
							}

							itemPosition = itemPosition.Add(new PointF(0, (hasBar ? o.ItemHorSelWithBarHeight : o.ItemHeight) + o.ItemBottomMargin));
						}
						else if (i is MenuItemHorSelector<float>) // I gave up on trying to combine these two into one with Reflection methods, dynamic types etc. Feel free
						{
							MenuItemHorSelector<float> item = i as MenuItemHorSelector<float>;
							MenuItemHorizontalSelectorType type = item.Type;
							float state = item.state;
							float maxState = item.maxState;
							float minState = item.minState;
							bool hasBar = type == MenuItemHorizontalSelectorType.Bar || type == MenuItemHorizontalSelectorType.NumberAndBar;
							bool hasNumber = type == MenuItemHorizontalSelectorType.Number || type == MenuItemHorizontalSelectorType.NumberAndBar;
							new Rectangle(itemPosition, new SizeF(o.Width, hasBar ? o.ItemHorSelWithBarHeight : o.ItemHeight), isSelected ? o.ItemSelectedBackgroundColor : o.ItemBackgroundColor).Draw();
							new Text(i.Title, itemPosition.Add(o.ItemPadding), o.ItemTextSize, isSelected ? o.ItemSelectedTextColor : o.ItemTextColor).Draw();

							if (hasNumber)
								new Text($"{(item.overrideDetailWith != null ? item.overrideDetailWith : $"{state:0.00}/{maxState:0.00}")}", new PointF(itemPosition.X + o.Width - o.ItemPadding.X, itemPosition.Y + o.ItemPadding.Y), o.ItemTextSize, isSelected ? o.ItemSelectedTextColor : o.ItemTextColor, o.ItemFont, Alignment.Right).Draw();
							if (hasBar)
							{
								float progressFraction = (state - minState) / (float)((maxState - minState));
								new Rectangle(itemPosition.Add(o.MenuItemHorSelOuterBarOffset), new SizeF(o.Width - 2 * o.MenuItemHorSelOuterBarOffset.X, o.MenuItemHorSelOuterBarHeight), isSelected ? o.ItemHorSelSelectedOuterBgColor : o.ItemHorSelOuterBgColor).Draw();
								new Rectangle(itemPosition.Add(o.MenuItemHorSelOuterBarOffset).Add(o.MenuItemHorSelInnerBarOffset), new SizeF(progressFraction * (o.Width - 2 * (o.MenuItemHorSelOuterBarOffset.X + o.MenuItemHorSelInnerBarOffset.X)), o.MenuItemHorSelInnerBarHeight), isSelected ? o.ItemHorSelSelectedInnerBgColor : o.ItemHorSelInnerBgColor).Draw();
							}

							itemPosition = itemPosition.Add(new PointF(0, (hasBar ? o.ItemHorSelWithBarHeight : o.ItemHeight) + o.ItemBottomMargin));
						}
						else if (i is MenuItemHorNamedSelector) // I gave up on trying to combine these into one with Reflection methods
						{
							MenuItemHorNamedSelector item = i as MenuItemHorNamedSelector;
							MenuItemHorizontalSelectorType type = item.Type;
							int maxState = item.optionList.Count()-1;
							int minState = 0;
							int state = (item.state < minState) ? minState : (item.state > maxState) ? maxState : item.state;
							string stateName = item.optionList[state];
							bool hasBar = type == MenuItemHorizontalSelectorType.Bar || type == MenuItemHorizontalSelectorType.NumberAndBar;
							bool hasNumber = type == MenuItemHorizontalSelectorType.Number || type == MenuItemHorizontalSelectorType.NumberAndBar;
							new Rectangle(itemPosition, new SizeF(o.Width, hasBar ? o.ItemHorSelWithBarHeight : o.ItemHeight), isSelected ? o.ItemSelectedBackgroundColor : o.ItemBackgroundColor).Draw();
							new Text(i.Title, itemPosition.Add(o.ItemPadding), o.ItemTextSize, isSelected ? o.ItemSelectedTextColor : o.ItemTextColor).Draw();

							if (hasNumber)
								new Text($"{(item.overrideDetailWith != null ? item.overrideDetailWith : stateName)}", new PointF(itemPosition.X + o.Width - o.ItemPadding.X, itemPosition.Y + o.ItemPadding.Y), o.ItemTextSize, isSelected ? o.ItemSelectedTextColor : o.ItemTextColor, o.ItemFont, Alignment.Right).Draw();
							if (hasBar)
							{
								float progressFraction = (state - minState) / (float)((maxState - minState));
								new Rectangle(itemPosition.Add(o.MenuItemHorSelOuterBarOffset), new SizeF(o.Width - 2 * o.MenuItemHorSelOuterBarOffset.X, o.MenuItemHorSelOuterBarHeight), isSelected ? o.ItemHorSelSelectedOuterBgColor : o.ItemHorSelOuterBgColor).Draw();
								new Rectangle(itemPosition.Add(o.MenuItemHorSelOuterBarOffset).Add(o.MenuItemHorSelInnerBarOffset), new SizeF(progressFraction * (o.Width - 2 * (o.MenuItemHorSelOuterBarOffset.X + o.MenuItemHorSelInnerBarOffset.X)), o.MenuItemHorSelInnerBarHeight), isSelected ? o.ItemHorSelSelectedInnerBgColor : o.ItemHorSelInnerBgColor).Draw();
							}

							itemPosition = itemPosition.Add(new PointF(0, (hasBar ? o.ItemHorSelWithBarHeight : o.ItemHeight) + o.ItemBottomMargin));
						}
						else
						{
							new Rectangle(itemPosition, new SizeF(o.Width, o.ItemHeight), isSelected ? o.ItemSelectedBackgroundColor : o.ItemBackgroundColor).Draw();
							new Text(i.Title, itemPosition.Add(o.ItemPadding), o.ItemTextSize, isSelected ? o.ItemSelectedTextColor : o.ItemTextColor).Draw();
							itemPosition = itemPosition.Add(new PointF(0, o.ItemHeight + o.ItemBottomMargin));
						}
					});
				}
				catch (Exception ex)
				{
					Log.Error($"[MENU] {ex.Message} thrown");
				}
			}
		}

		class MenuDescription
		{
			MenuOptions o;
			private MenuView menu;
			public PointF Position;

			public MenuDescription(MenuView menu)
			{
				this.menu = menu;
			}

			public void Draw()
			{
				o = menu.current.options;
				Position = menu.menuItems.Position.Add(new PointF(0, menu.menuItems.TotalHeight + o.DescriptionMarginTop));
				if (menu.current.SelectedItem.Description != null)
				{
					new Rectangle(Position, new SizeF(o.Width, o.ItemHeight /* TODO: Measure */), o.ItemBackgroundColor, false).Draw();
					new Text(menu.current.SelectedItem.Description, Position.Add(o.ItemPadding), o.ItemTextSize, o.ItemTextColor, o.ItemFont, Alignment.Left).Draw(); // TODO: word wrap
				}
			}
		}

		MenuHeader menuHeader;
		MenuStatusBar menuStatusBar;
		MenuItems menuItems;
		MenuDescription menuDescription;

		public MenuModel current;

		public MenuView()
		{
			menuHeader = new MenuHeader(this);
			menuStatusBar = new MenuStatusBar(this);
			menuItems = new MenuItems(this);
			menuDescription = new MenuDescription(this);
		}

		public void Draw(MenuModel current)
		{
			this.current = current;
			if (this.current == null) return;
			if (MouseEnabled)
			{
				Function.Call(Hash._SHOW_CURSOR_THIS_FRAME);
				CursorSelectCheck();
			}
			menuHeader.Draw();
			menuStatusBar.Draw();
			menuItems.Draw();
			menuDescription.Draw();
		}

		public bool MouseEnabled = true;
		void CursorSelectCheck()
		{
			PointF point = UIHelpers.MousePosition();
			PointF Position = current.options.Origin.Add(new PointF(0, current.options.HeaderHeight + current.options.HeaderBottomMargin + current.options.StatusBarHeight + current.options.StatusBarBottomMargin));
			float itemTopY;
			float itemBottomY = Position.Y;
			float itemLeftX = Position.X;
			float itemRightX = Position.X + current.options.Width;
			var selected = current.menuItems.Select((item, index) => new { item, index }).Where(i => current.visibleItems.Contains(i.item)).Where(i =>
			{
				itemTopY = itemBottomY;
				itemBottomY = itemTopY + MenuItems.GetItemHeight(current.options, i.item);
				if (point.X.IsBetween(itemLeftX, itemRightX) && point.Y> itemTopY && point.Y < itemBottomY)
				{
					return true;
				}
				else
				{
					return false;
				}
			}).ToList();
			if(selected.Count() > 0)
			{
				if (current.SelectedIndex != selected.First().index) MenuController.PlaySound("NAV_UP_DOWN");
				current.SelectedIndex = selected.First().index;
			}
		}
	}

	class MenuController
	{
		public MenuObserver Observer = null;
		public Dictionary<Control, Delegate> controlBinds;

		public MenuController()
		{
			controlBinds = new Dictionary<Control, Delegate>()
			{
				[Control.PhoneUp] = new Action(() => { Observer.Dispatch(MenuAction.Up); PlaySound("NAV_UP_DOWN"); }),
				[Control.PhoneDown] = new Action(() => { Observer.Dispatch(MenuAction.Down); PlaySound("NAV_UP_DOWN"); }),
				[Control.PhoneSelect] = new Action(() => { Observer.Dispatch(MenuAction.Select); PlaySound("SELECT"); }),
				[Control.PhoneCancel] = new Action(() => { Observer.CloseMenu(); PlaySound("BACK"); }),
				[Control.PhoneLeft] = new Action(() => { Observer.Dispatch(MenuAction.Left); PlaySound("NAV_UP_DOWN"); }),
				[Control.PhoneRight] = new Action(() => { Observer.Dispatch(MenuAction.Right); PlaySound("NAV_UP_DOWN"); })
			};
		}

		public static void PlaySound(string sound, MenuObserver observer = null)
		{
			if(!MenuGlobals.MuteSounds)
			Audio.PlaySoundFrontend(sound, "HUD_FRONTEND_DEFAULT_SOUNDSET");
		}

		internal void Check()
		{
			if(Observer.CurrentMenu != null)
			{ 
				controlBinds.ToList().ForEach(c =>
				{
					if(ControlHelper.IsControlJustReleased(c.Key) || ControlHelper.IsDisabledControlJustReleased(c.Key))
					{
						c.Value.DynamicInvoke();
					}
				});
			}
		}
	}

	class MenuObserver
	{
		public Stack<MenuModel> ParentMenuStack = new Stack<MenuModel>();
		public MenuModel CurrentMenu = null;
		MenuView View = new MenuView();
		public MenuController Controller = new MenuController();

		public MenuObserver()
		{
			Controller.Observer = this;
		}

		public void OpenMenu(MenuModel menu, bool resetMenu = false)
		{
			var PreviousMenu = CurrentMenu;
			CurrentMenu = menu;
			CurrentMenu.Refresh();
			if (!menu.Initialized)
				menu.Init();
			if (!resetMenu && PreviousMenu != null)
			{ 
				ParentMenuStack.Push(PreviousMenu);
			}
			else
			{
				ParentMenuStack.Clear();
			}
		}

		public void CloseMenu(bool closeAll = false)
		{
			if (closeAll == false && ParentMenuStack.Count > 0)
			{
				CurrentMenu = ParentMenuStack.Pop();
				CurrentMenu.Refresh();
			}
			else
			{
				CurrentMenu = null;
			}
		}

		public void Dispatch(MenuAction action)
		{
			// Handled at the observer level mainly so that OpenMenu can be 
			if (action == MenuAction.Select)
				if (CurrentMenu.SelectedItem is MenuItemBack)
					CloseMenu();
				else if (CurrentMenu.SelectedItem is MenuItemSubMenu)
					OpenMenu(((MenuItemSubMenu)CurrentMenu.SelectedItem).SubMenu);
				else if (CurrentMenu.SelectedItem is MenuItemStandard)
					((MenuItemStandard)CurrentMenu.SelectedItem).OnActivate?.Invoke((MenuItemStandard)CurrentMenu.SelectedItem);
				else if (CurrentMenu.SelectedItem is MenuItemCheckbox)
					((MenuItemCheckbox)CurrentMenu.SelectedItem).State = !((MenuItemCheckbox)CurrentMenu.SelectedItem).State;
				else
					CurrentMenu.Dispatch(MenuAction.Select);
			else if ((action == MenuAction.Left || action == MenuAction.Right) && (CurrentMenu.SelectedItem is MenuItemHorSelector<int>))
				((MenuItemHorSelector<int>)CurrentMenu.SelectedItem).Dispatch(action);
			else if ((action == MenuAction.Left || action == MenuAction.Right) && (CurrentMenu.SelectedItem is MenuItemHorSelector<float>)) // I couldn't manage to do this any other way without constant errors; feel free if you know
				((MenuItemHorSelector<float>)CurrentMenu.SelectedItem).Dispatch(action);
			else if ((action == MenuAction.Left || action == MenuAction.Right) && (CurrentMenu.SelectedItem is MenuItemHorNamedSelector))
				((MenuItemHorNamedSelector)CurrentMenu.SelectedItem).Dispatch(action);
			else
				CurrentMenu.Dispatch(action);
		}

		public void Tick()
		{
			Controller.Check();
			View.Draw(CurrentMenu);
		}
	}

	class MenuModel
	{
		public string headerTitle = "";
		public string statusTitle = "";
		public List<MenuItem> menuItems = new List<MenuItem>();
		public List<MenuItem> visibleItems = new List<MenuItem>();
		public int topMostVisibleItem = 0;
		public int numVisibleItems = 10;
		public int _selectedIndex = 0;
		public bool Initialized = false;

		/// <summary>
		/// Intended for refreshing the menu in case something has changed
		/// </summary>
		public virtual void Refresh() {
		}

		/// <summary>
		/// Intended for updating the menu with live information (call every tick)
		/// </summary>
		/// <param name="frameCount">The frame number of this current tick, which increments from 0 since the start of the game</param>
		/// <param name="frameTime">The frame time delta, the time since last frame in milliseconds</param>
		/// <param name="gameTimer">How long the current game session has been running for in milliseconds</param>
		public virtual void OnTick( long frameCount, int frameTime, long gameTimer ) {
		}

		public int SelectedIndex
		{
			get
			{
				//return menuItems.IndexOf(SelectedItem);
				return _selectedIndex;
			}
			set
			{
				if (value >= 0 && value <= menuItems.Count() - 1)
				{
					SelectedItem = menuItems[value];
					_selectedIndex = value;
					if (menuItems.Count <= numVisibleItems) { visibleItems = menuItems; return; }
					if (value > topMostVisibleItem + numVisibleItems - 1)
					{
						visibleItems = menuItems.Slice(value - numVisibleItems + 1, value);
						topMostVisibleItem = value - numVisibleItems + 1;
					}
					else if(value < topMostVisibleItem)
					{
						visibleItems = menuItems.Slice(value, value + numVisibleItems - 1);
						topMostVisibleItem = value;
					}
					else
					{
						visibleItems = menuItems.Slice(topMostVisibleItem, topMostVisibleItem + numVisibleItems - 1);
					}
				}
				else
				{
					if (value < 0)
					{
						_selectedIndex = menuItems.Count() - 1;
					}
					else if (value > menuItems.Count() - 1)
					{
						_selectedIndex = 0;
					}
					SelectedItem = menuItems[_selectedIndex];
					if (menuItems.Count <= numVisibleItems) { visibleItems = menuItems; return; }
					if (!visibleItems.Contains(SelectedItem))
					{
						if (_selectedIndex == 0)
						{
							visibleItems = menuItems.Slice(0, Math.Min(numVisibleItems - 1, menuItems.Count - 1));
							topMostVisibleItem = 0;
						}
						else if (_selectedIndex == menuItems.Count - 1)
						{
							visibleItems = menuItems.Slice(Math.Max(0, menuItems.Count - numVisibleItems), menuItems.Count - 1);
							topMostVisibleItem = Math.Max(0, menuItems.Count - numVisibleItems);
						}
					}
				}
			}
		}

		public MenuItem SelectedItem { get; protected set; }
		public MenuOptions options = null;

		public void Init()
		{
			if (menuItems.Count <= numVisibleItems)
			{
				visibleItems = menuItems;
			}
			else
			{
				visibleItems = menuItems.Slice(0, numVisibleItems - 1);
			}
			SelectedIndex = 0;

			Initialized = true;

			Refresh();
		}

		/// <summary>
		/// To provide custom settings for a menu (all settings can be seen in MenuOptions) create a new MenuOptions object with the settings changed that you want to change.  Recommended syntax would be 
		/// MenuOptions options = new MenuOptions { ItemFont = Font.HouseScript };
		/// These options will need to be specified for any sub-menus as well.
		/// </summary>
		/// <param name="Options"></param>
		public MenuModel(MenuOptions Options = null)
		{
			if(Options == null) options = new MenuOptions();
			else options = Options;
		}

		public void Dispatch(MenuAction action)
		{
			if (action == MenuAction.Up)
			{ 
				SelectedIndex--;
				SelectedItem.OnSelect?.Invoke(SelectedItem);
			}
			else if (action == MenuAction.Down)
			{ 
				SelectedIndex++;
				SelectedItem.OnSelect?.Invoke(SelectedItem);
			}
			else
			{
				menuItems[SelectedIndex].Dispatch(action);
			}
		}
	}

	abstract class MenuItem
	{
		public Action<MenuItem> OnSelect = null;
		public string Title = null;
		public string Description = null;
		public virtual void Dispatch(MenuAction action) { }

		/// <summary>
		/// Intended for refreshing the item in case something has changed
		/// </summary>
		public virtual void Refresh() {
		}

		/// <summary>
		/// Intended for updating the menu with live information (call every tick)
		/// </summary>
		/// <param name="frameCount">The frame number of this current tick, which increments from 0 since the start of the game (<see cref="Hash.GET_FRAME_COUNT">)</see>/></param>
		/// <param name="frameTime">The frame time delta, the time since last frame in milliseconds (<see cref="Hash.GET_FRAME_TIME">)</param>
		/// <param name="gameTimer">How long the current game session has been running for in milliseconds (<see cref="Hash.GET_GAME_TIMER">)</param>
		public virtual void OnTick( long frameCount, int frameTime, long gameTimer ) {
		}
	}

	class MenuItemSubMenu : MenuItem
	{
		public MenuModel SubMenu = null;
	}

	class MenuItemBack : MenuItem
	{
		public MenuItemBack()
		{
			Title = "Back";
		}
	}

	class MenuItemStandard : MenuItem
	{
		public Action<MenuItemStandard> OnActivate = null;
		public string Detail = null;
		public string SubDetail = null;
		public string MetaData = null;
	}

	class MenuItemCheckbox : MenuItem
	{
		public Action<bool, MenuItemCheckbox> OnActivate = null;
		internal bool state = false;
		public bool State { get { return state; } set { state = value; OnActivate.Invoke(state, this); } }
	}

	/// <summary>
	/// Variables that need to be set: minState, maxState, potentially state (initial state), potentially stepSize
	/// (I have yet to test if the last two get their defaults set properly)
	/// </summary>
	/// <typeparam name="T">Don't you dare put anything but a number type here</typeparam>
	class MenuItemHorSelector<T> : MenuItem
	{
		public Action<T, MenuItemHorSelector<T>> OnChange = null;
		public Action<MenuItemHorSelector<T>> OnActivate = new Action<MenuItemHorSelector<T>>((item) => { });
		internal T state = (dynamic) 0;
		public T State { get { return state; } set { state = value; OnChange.Invoke(state, this); } }
		public T minState = default(T);
		public T maxState = default(T);
		public MenuItemHorizontalSelectorType Type = MenuItemHorizontalSelectorType.Bar;
		public T stepSize = (dynamic) 1;
		public string overrideDetailWith = null;
		public bool wrapAround = true;
		public override void Dispatch(MenuAction action)
		{
			try
			{ 
				T _state = state;
				switch (action)
				{
					case MenuAction.Left:
						_state = (dynamic)_state - stepSize; break;
					case MenuAction.Right:
						_state = (dynamic)_state + stepSize; break;
				}
				if (wrapAround)
				{
					if ((dynamic)_state > maxState) _state = minState;
					else if ((dynamic)_state < minState) _state = maxState;
				}
				else
				{
					if ((dynamic)_state > maxState) _state = maxState;
					else if ((dynamic)_state < minState) _state = minState;
				}
				State = _state;
			}
			catch(Exception ex)
			{
				Log.Error($"[MENU] In Dispatch (menu level): {ex.Message}");
			}
		}
	}


	class MenuItemHorNamedSelector : MenuItem
	{
		public Action<int, string, MenuItemHorNamedSelector> OnChange = null;
		public Action<int, string, MenuItemHorNamedSelector> OnActivate = new Action<int, string, MenuItemHorNamedSelector>((selectedAlternative, selName, menuItem) => {  });
		internal int state = 0;
		public int State { get { return state; } set { state = value; if(OnChange != null) OnChange.Invoke(state, optionList[state], this); } }
		public MenuItemHorizontalSelectorType Type = MenuItemHorizontalSelectorType.Bar;
		public List<string> optionList = new List<string>();
		public string overrideDetailWith = null;
		public bool wrapAround = true;
		public override void Dispatch(MenuAction action)
		{
			try
			{
				switch (action)
				{
					case MenuAction.Left:
						state--; break;
					case MenuAction.Right:
						state++; break;
					case MenuAction.Select:
						OnActivate(state, optionList[state], this); break;
				}
				if (wrapAround)
				{
					if (state > optionList.Count-1) state = 0;
					else if (state < 0) state = optionList.Count-1;
				}
				else
				{
					if (state > optionList.Count-1) state = optionList.Count-1;
					else if (state < 0) state = 0;
				}
			}
			catch (Exception ex)
			{
				Log.Error($"[MENU] In Dispatch (selector level): {ex.Message}");
			}

			try
			{
				State = state;
			}
			catch (Exception ex)
			{
				Log.Error($"[MENU] In Dispatch (updating State property): {ex.Message}");
			}
		}
	}
}