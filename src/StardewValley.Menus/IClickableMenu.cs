using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Mobile;
using StardewValley.Objects;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	[InstanceStatics]
	public abstract class IClickableMenu
	{
		public delegate void onExit();

		protected IClickableMenu _childMenu;

		protected IClickableMenu _parentMenu;

		public const int currency_g = 0;

		public const int currency_starTokens = 1;

		public const int currency_qiCoins = 2;

		public const int currency_qiGems = 4;

		public const int greyedOutSpotIndex = 57;

		public const int outerBorderWithUpArrow = 61;

		public const int lvlMarkerRedIndex = 54;

		public const int lvlMarkerGreyIndex = 55;

		public const int borderWithDownArrowIndex = 46;

		public const int borderWithUpArrowIndex = 47;

		public const int littleHeartIndex = 49;

		public const int uncheckedBoxIndex = 50;

		public const int checkedBoxIndex = 51;

		public const int presentIconIndex = 58;

		public const int itemSpotIndex = 10;

		public static int borderWidth = 40;

		public static int tabYPositionRelativeToMenuY = -48;

		public static int spaceToClearTopBorder = 116;

		public static int spaceToClearSideBorder = 16;

		public const int spaceBetweenTabs = 4;

		public int width;

		public int height;

		public int xPositionOnScreen;

		public int yPositionOnScreen;

		public int currentRegion;

		public Action<IClickableMenu> behaviorBeforeCleanup;

		public onExit exitFunction;

		public ClickableTextureComponent upperRightCloseButton;

		public bool destroy;

		public bool gamePadControlsImplemented;

		protected int _dependencies;

		public List<ClickableComponent> allClickableComponents;

		public ClickableComponent currentlySnappedComponent;

		[NonInstancedStatic]
		public static Microsoft.Xna.Framework.Rectangle lastTextureBoxRect;

		public static xTile.Dimensions.Rectangle viewport => Game1.uiViewport;

		public IClickableMenu()
		{
		}

		public IClickableMenu(int x, int y, int width, int height, bool showUpperRightCloseButton = false)
		{
			initialize(x, y, width, height, showUpperRightCloseButton);
			if (Game1.gameMode == 3 && Game1.player != null && !Game1.eventUp)
			{
				Game1.player.Halt();
			}
		}

		public virtual Type getMenuType()
		{
			return GetType();
		}

		public void initialize(int x, int y, int width, int height, bool showUpperRightCloseButton = false)
		{
			if (Game1.player != null && !Game1.player.UsingTool && !Game1.eventUp)
			{
				Game1.player.forceCanMove();
			}
			xPositionOnScreen = x;
			yPositionOnScreen = y;
			this.width = width;
			this.height = height;
			if (showUpperRightCloseButton)
			{
				initializeUpperRightCloseButton();
			}
			for (int i = 0; i < 4; i++)
			{
				Game1.directionKeyPolling[i] = 250;
			}
		}

		public virtual bool HasFocus()
		{
			return Game1.activeClickableMenu == this;
		}

		public IClickableMenu GetChildMenu()
		{
			return _childMenu;
		}

		public IClickableMenu GetParentMenu()
		{
			return _parentMenu;
		}

		public void SetChildMenu(IClickableMenu menu)
		{
			_childMenu = menu;
			if (_childMenu != null)
			{
				_childMenu._parentMenu = this;
			}
		}

		public void AddDependency()
		{
			_dependencies++;
		}

		public void RemoveDependency()
		{
			_dependencies--;
			if (_dependencies <= 0 && Game1.activeClickableMenu != this && TitleMenu.subMenu != this && this is IDisposable)
			{
				(this as IDisposable).Dispose();
			}
		}

		public bool HasDependencies()
		{
			return _dependencies > 0;
		}

		public virtual bool areGamePadControlsImplemented()
		{
			return false;
		}

		public ClickableComponent getLastClickableComponentInThisListThatContainsThisXCoord(List<ClickableComponent> ccList, int xCoord)
		{
			for (int num = ccList.Count - 1; num >= 0; num--)
			{
				if (ccList[num].bounds.Contains(xCoord, ccList[num].bounds.Center.Y))
				{
					return ccList[num];
				}
			}
			return null;
		}

		public ClickableComponent getFirstClickableComponentInThisListThatContainsThisXCoord(List<ClickableComponent> ccList, int xCoord)
		{
			for (int i = 0; i < ccList.Count; i++)
			{
				if (ccList[i].bounds.Contains(xCoord, ccList[i].bounds.Center.Y))
				{
					return ccList[i];
				}
			}
			return null;
		}

		public ClickableComponent getLastClickableComponentInThisListThatContainsThisYCoord(List<ClickableComponent> ccList, int yCoord)
		{
			for (int num = ccList.Count - 1; num >= 0; num--)
			{
				if (ccList[num].bounds.Contains(ccList[num].bounds.Center.X, yCoord))
				{
					return ccList[num];
				}
			}
			return null;
		}

		public ClickableComponent getFirstClickableComponentInThisListThatContainsThisYCoord(List<ClickableComponent> ccList, int yCoord)
		{
			for (int i = 0; i < ccList.Count; i++)
			{
				if (ccList[i].bounds.Contains(ccList[i].bounds.Center.X, yCoord))
				{
					return ccList[i];
				}
			}
			return null;
		}

		public virtual void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.B && upperRightCloseButton != null && readyToClose())
			{
				OnTapCloseButton();
			}
		}

		public void drawMouse(SpriteBatch b, bool ignore_transparency = false, int cursor = -1)
		{
			if (Game1.options.gamepadControls && Game1.virtualJoypad.mostRecentlyUsedControlType == ControlType.GAMEPAD && !Game1.options.hardwareCursor)
			{
				float num = Game1.mouseCursorTransparency;
				if (ignore_transparency)
				{
					num = 1f;
				}
				if (cursor < 0)
				{
					cursor = ((Game1.options.snappyMenus && Game1.options.gamepadControls) ? 44 : 0);
				}
				b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, cursor, 16, 16), Color.White * num, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
		}

		public void populateClickableComponentList()
		{
			allClickableComponents = new List<ClickableComponent>();
			FieldInfo[] fields = GetType().GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.GetCustomAttributes(typeof(SkipForClickableAggregation), inherit: true).Length != 0 || fieldInfo.DeclaringType == typeof(IClickableMenu))
				{
					continue;
				}
				if (fieldInfo.FieldType.IsSubclassOf(typeof(ClickableComponent)) || fieldInfo.FieldType == typeof(ClickableComponent))
				{
					if (fieldInfo.GetValue(this) != null)
					{
						allClickableComponents.Add((ClickableComponent)fieldInfo.GetValue(this));
					}
				}
				else if (fieldInfo.FieldType == typeof(List<ClickableComponent>))
				{
					List<ClickableComponent> list = (List<ClickableComponent>)fieldInfo.GetValue(this);
					if (list == null)
					{
						continue;
					}
					for (int num = list.Count - 1; num >= 0; num--)
					{
						if (list[num] != null)
						{
							allClickableComponents.Add(list[num]);
						}
					}
				}
				else if (fieldInfo.FieldType == typeof(List<ClickableTextureComponent>))
				{
					List<ClickableTextureComponent> list2 = (List<ClickableTextureComponent>)fieldInfo.GetValue(this);
					if (list2 == null)
					{
						continue;
					}
					for (int num2 = list2.Count - 1; num2 >= 0; num2--)
					{
						if (list2[num2] != null)
						{
							allClickableComponents.Add(list2[num2]);
						}
					}
				}
				else if (fieldInfo.FieldType == typeof(List<ClickableAnimatedComponent>))
				{
					List<ClickableAnimatedComponent> list3 = (List<ClickableAnimatedComponent>)fieldInfo.GetValue(this);
					for (int num3 = list3.Count - 1; num3 >= 0; num3--)
					{
						if (list3[num3] != null)
						{
							allClickableComponents.Add(list3[num3]);
						}
					}
				}
				else if (fieldInfo.FieldType == typeof(List<Bundle>))
				{
					List<Bundle> list4 = (List<Bundle>)fieldInfo.GetValue(this);
					for (int num4 = list4.Count - 1; num4 >= 0; num4--)
					{
						if (list4[num4] != null)
						{
							allClickableComponents.Add(list4[num4]);
						}
					}
				}
				else if (fieldInfo.FieldType == typeof(InventoryMenu))
				{
					allClickableComponents.AddRange(((InventoryMenu)fieldInfo.GetValue(this)).inventory);
					allClickableComponents.Add(((InventoryMenu)fieldInfo.GetValue(this)).dropItemInvisibleButton);
				}
				else if (fieldInfo.FieldType == typeof(List<Dictionary<ClickableTextureComponent, CraftingRecipe>>))
				{
					foreach (Dictionary<ClickableTextureComponent, CraftingRecipe> item in (List<Dictionary<ClickableTextureComponent, CraftingRecipe>>)fieldInfo.GetValue(this))
					{
						allClickableComponents.AddRange(item.Keys);
					}
				}
				else if (fieldInfo.FieldType == typeof(Dictionary<int, List<List<ClickableTextureComponent>>>))
				{
					foreach (List<List<ClickableTextureComponent>> value in ((Dictionary<int, List<List<ClickableTextureComponent>>>)fieldInfo.GetValue(this)).Values)
					{
						foreach (List<ClickableTextureComponent> item2 in value)
						{
							allClickableComponents.AddRange(item2);
						}
					}
				}
				else
				{
					if (!(fieldInfo.FieldType == typeof(Dictionary<int, ClickableTextureComponent>)))
					{
						continue;
					}
					foreach (ClickableTextureComponent value2 in ((Dictionary<int, ClickableTextureComponent>)fieldInfo.GetValue(this)).Values)
					{
						allClickableComponents.Add(value2);
					}
				}
			}
			if (Game1.activeClickableMenu is GameMenu gameMenu && this == gameMenu.GetCurrentPage())
			{
				gameMenu.AddTabsToClickableComponents(this);
			}
			if (upperRightCloseButton != null)
			{
				allClickableComponents.Add(upperRightCloseButton);
			}
		}

		public virtual void applyMovementKey(int direction)
		{
			if (allClickableComponents == null)
			{
				populateClickableComponentList();
			}
			moveCursorInDirection(direction);
		}

		public virtual void snapToDefaultClickableComponent()
		{
		}

		public void applyMovementKey(Keys key)
		{
			if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
			{
				applyMovementKey(0);
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
			{
				applyMovementKey(1);
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
			{
				applyMovementKey(2);
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
			{
				applyMovementKey(3);
			}
		}

		public virtual void setCurrentlySnappedComponentTo(int id)
		{
			currentlySnappedComponent = getComponentWithID(id);
		}

		public void moveCursorInDirection(int direction)
		{
			if (currentlySnappedComponent == null && allClickableComponents != null && allClickableComponents.Count() > 0)
			{
				snapToDefaultClickableComponent();
				if (currentlySnappedComponent == null)
				{
					currentlySnappedComponent = allClickableComponents.First();
				}
			}
			if (currentlySnappedComponent == null)
			{
				return;
			}
			ClickableComponent clickableComponent = currentlySnappedComponent;
			switch (direction)
			{
			case 0:
				if (currentlySnappedComponent.upNeighborID == -99999)
				{
					snapToDefaultClickableComponent();
				}
				else if (currentlySnappedComponent.upNeighborID == -99998)
				{
					automaticSnapBehavior(0, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else if (currentlySnappedComponent.upNeighborID == -7777)
				{
					customSnapBehavior(0, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else
				{
					currentlySnappedComponent = getComponentWithID(currentlySnappedComponent.upNeighborID);
				}
				if (currentlySnappedComponent != null && (clickableComponent == null || (clickableComponent.upNeighborID != -7777 && clickableComponent.upNeighborID != -99998)) && !currentlySnappedComponent.downNeighborImmutable && !currentlySnappedComponent.fullyImmutable)
				{
					currentlySnappedComponent.downNeighborID = clickableComponent.myID;
				}
				if (currentlySnappedComponent == null)
				{
					noSnappedComponentFound(0, clickableComponent.region, clickableComponent.myID);
				}
				break;
			case 1:
				if (currentlySnappedComponent.rightNeighborID == -99999)
				{
					snapToDefaultClickableComponent();
				}
				else if (currentlySnappedComponent.rightNeighborID == -99998)
				{
					automaticSnapBehavior(1, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else if (currentlySnappedComponent.rightNeighborID == -7777)
				{
					customSnapBehavior(1, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else
				{
					currentlySnappedComponent = getComponentWithID(currentlySnappedComponent.rightNeighborID);
				}
				if (currentlySnappedComponent != null && (clickableComponent == null || (clickableComponent.rightNeighborID != -7777 && clickableComponent.rightNeighborID != -99998)) && !currentlySnappedComponent.leftNeighborImmutable && !currentlySnappedComponent.fullyImmutable)
				{
					currentlySnappedComponent.leftNeighborID = clickableComponent.myID;
				}
				if (currentlySnappedComponent == null && clickableComponent.tryDefaultIfNoRightNeighborExists)
				{
					snapToDefaultClickableComponent();
				}
				else if (currentlySnappedComponent == null)
				{
					noSnappedComponentFound(1, clickableComponent.region, clickableComponent.myID);
				}
				break;
			case 2:
				if (currentlySnappedComponent.downNeighborID == -99999)
				{
					snapToDefaultClickableComponent();
				}
				else if (currentlySnappedComponent.downNeighborID == -99998)
				{
					automaticSnapBehavior(2, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else if (currentlySnappedComponent.downNeighborID == -7777)
				{
					customSnapBehavior(2, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else
				{
					currentlySnappedComponent = getComponentWithID(currentlySnappedComponent.downNeighborID);
				}
				if (currentlySnappedComponent != null && (clickableComponent == null || (clickableComponent.downNeighborID != -7777 && clickableComponent.downNeighborID != -99998)) && !currentlySnappedComponent.upNeighborImmutable && !currentlySnappedComponent.fullyImmutable)
				{
					currentlySnappedComponent.upNeighborID = clickableComponent.myID;
				}
				if (currentlySnappedComponent == null && clickableComponent.tryDefaultIfNoDownNeighborExists)
				{
					snapToDefaultClickableComponent();
				}
				else if (currentlySnappedComponent == null)
				{
					noSnappedComponentFound(2, clickableComponent.region, clickableComponent.myID);
				}
				break;
			case 3:
				if (currentlySnappedComponent.leftNeighborID == -99999)
				{
					snapToDefaultClickableComponent();
				}
				else if (currentlySnappedComponent.leftNeighborID == -99998)
				{
					automaticSnapBehavior(3, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else if (currentlySnappedComponent.leftNeighborID == -7777)
				{
					customSnapBehavior(3, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else
				{
					currentlySnappedComponent = getComponentWithID(currentlySnappedComponent.leftNeighborID);
				}
				if (currentlySnappedComponent != null && (clickableComponent == null || (clickableComponent.leftNeighborID != -7777 && clickableComponent.leftNeighborID != -99998)) && !currentlySnappedComponent.rightNeighborImmutable && !currentlySnappedComponent.fullyImmutable)
				{
					currentlySnappedComponent.rightNeighborID = clickableComponent.myID;
				}
				if (currentlySnappedComponent == null)
				{
					noSnappedComponentFound(3, clickableComponent.region, clickableComponent.myID);
				}
				break;
			}
			if (currentlySnappedComponent != null && clickableComponent != null && currentlySnappedComponent.region != clickableComponent.region)
			{
				actionOnRegionChange(clickableComponent.region, currentlySnappedComponent.region);
			}
			if (currentlySnappedComponent == null)
			{
				currentlySnappedComponent = clickableComponent;
			}
			snapCursorToCurrentSnappedComponent();
			if (currentlySnappedComponent != clickableComponent)
			{
				Game1.playSound("shiny4");
			}
		}

		public virtual void snapCursorToCurrentSnappedComponent()
		{
			if (currentlySnappedComponent != null)
			{
				Game1.setMousePosition(currentlySnappedComponent.bounds.Right - currentlySnappedComponent.bounds.Width / 4, currentlySnappedComponent.bounds.Bottom - currentlySnappedComponent.bounds.Height / 4, ui_scale: true);
			}
		}

		protected virtual void noSnappedComponentFound(int direction, int oldRegion, int oldID)
		{
		}

		protected virtual void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
		}

		public virtual bool IsActive()
		{
			if (_parentMenu == null)
			{
				return this == Game1.activeClickableMenu;
			}
			IClickableMenu parentMenu = _parentMenu;
			while (parentMenu != null && parentMenu._parentMenu != null)
			{
				parentMenu = parentMenu._parentMenu;
			}
			return parentMenu == Game1.activeClickableMenu;
		}

		public virtual void automaticSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (currentlySnappedComponent == null)
			{
				snapToDefaultClickableComponent();
				return;
			}
			Vector2 zero = Vector2.Zero;
			switch (direction)
			{
			case 3:
				zero.X = -1f;
				zero.Y = 0f;
				break;
			case 1:
				zero.X = 1f;
				zero.Y = 0f;
				break;
			case 0:
				zero.X = 0f;
				zero.Y = -1f;
				break;
			case 2:
				zero.X = 0f;
				zero.Y = 1f;
				break;
			}
			float num = -1f;
			ClickableComponent clickableComponent = null;
			for (int i = 0; i < allClickableComponents.Count; i++)
			{
				ClickableComponent clickableComponent2 = allClickableComponents[i];
				if ((clickableComponent2.leftNeighborID == -1 && clickableComponent2.rightNeighborID == -1 && clickableComponent2.upNeighborID == -1 && clickableComponent2.downNeighborID == -1) || clickableComponent2.myID == -500 || !IsAutomaticSnapValid(direction, currentlySnappedComponent, clickableComponent2) || !clickableComponent2.visible || clickableComponent2 == upperRightCloseButton || clickableComponent2 == currentlySnappedComponent)
				{
					continue;
				}
				Vector2 value = new Vector2(clickableComponent2.bounds.Center.X - currentlySnappedComponent.bounds.Center.X, clickableComponent2.bounds.Center.Y - currentlySnappedComponent.bounds.Center.Y);
				Vector2 value2 = new Vector2(value.X, value.Y);
				value2.Normalize();
				float num2 = Vector2.Dot(zero, value2);
				if (!(num2 > 0.01f))
				{
					continue;
				}
				float num3 = Vector2.DistanceSquared(Vector2.Zero, value);
				bool flag = false;
				switch (direction)
				{
				case 0:
				case 2:
					if (Math.Abs(value.X) < 32f)
					{
						flag = true;
					}
					break;
				case 1:
				case 3:
					if (Math.Abs(value.Y) < 32f)
					{
						flag = true;
					}
					break;
				}
				if (_ShouldAutoSnapPrioritizeAlignedElements() && (num2 > 0.99999f || flag))
				{
					num3 *= 0.01f;
				}
				if (num == -1f || num3 < num)
				{
					num = num3;
					clickableComponent = clickableComponent2;
				}
			}
			if (clickableComponent != null)
			{
				currentlySnappedComponent = clickableComponent;
			}
		}

		protected virtual bool _ShouldAutoSnapPrioritizeAlignedElements()
		{
			return true;
		}

		public virtual bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			return true;
		}

		protected virtual void actionOnRegionChange(int oldRegion, int newRegion)
		{
		}

		public ClickableComponent getComponentWithID(int id)
		{
			if (id == -500)
			{
				return null;
			}
			if (allClickableComponents != null)
			{
				for (int i = 0; i < allClickableComponents.Count; i++)
				{
					if (allClickableComponents[i] != null && allClickableComponents[i].myID == id && allClickableComponents[i].visible)
					{
						return allClickableComponents[i];
					}
				}
				for (int j = 0; j < allClickableComponents.Count; j++)
				{
					if (allClickableComponents[j] != null && allClickableComponents[j].myAlternateID == id && allClickableComponents[j].visible)
					{
						return allClickableComponents[j];
					}
				}
			}
			return null;
		}

		public void initializeUpperRightCloseButton()
		{
			upperRightCloseButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(viewport.Width - 68 - Game1.xEdge, 0, 68 + Game1.xEdge, 80), Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(62, 0, 17, 17), 4f, drawShadow: true);
		}

		public virtual void drawBackground(SpriteBatch b)
		{
			if (this is ShopMenu)
			{
				for (int i = 0; i < viewport.Width; i += 400)
				{
					for (int j = 0; j < viewport.Height; j += 384)
					{
						b.Draw(Game1.mouseCursors, new Vector2(i, j), new Microsoft.Xna.Framework.Rectangle(527, 0, 100, 96), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
					}
				}
				return;
			}
			if (Game1.isDarkOut())
			{
				b.Draw(Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(0, 0, viewport.Width, viewport.Height), new Microsoft.Xna.Framework.Rectangle(228, 0, 4, 184), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
			}
			else if (Game1.IsRainingHere())
			{
				b.Draw(Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(0, 0, viewport.Width, viewport.Height), new Microsoft.Xna.Framework.Rectangle(232, 0, 4, 184), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
			}
			else
			{
				b.Draw(Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(0, 0, viewport.Width, viewport.Height), new Microsoft.Xna.Framework.Rectangle(240 + Utility.getSeasonNumber(Game1.currentSeason) * 4, 0, 4, 400), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
			}
			b.Draw(Game1.mouseCursors, new Vector2(-120f, viewport.Height - 592), new Microsoft.Xna.Framework.Rectangle(0, Game1.currentSeason.Equals("winter") ? 1035 : ((Game1.isRaining || Game1.isDarkOut()) ? 886 : 737), 639, 148), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
			b.Draw(Game1.mouseCursors, new Vector2(2436f, viewport.Height - 592), new Microsoft.Xna.Framework.Rectangle(0, Game1.currentSeason.Equals("winter") ? 1035 : ((Game1.isRaining || Game1.isDarkOut()) ? 886 : 737), 639, 148), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
			if (Game1.isRaining)
			{
				b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(0, 0, viewport.Width, viewport.Height), Color.Blue * 0.2f);
			}
		}

		public virtual bool showWithoutTransparencyIfOptionIsSet()
		{
			if (this is GameMenu || this is ShopMenu || this is WheelSpinGame || this is ItemGrabMenu)
			{
				return true;
			}
			return false;
		}

		public virtual void clickAway()
		{
		}

		public virtual void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
		{
			xPositionOnScreen = (int)((float)(newBounds.Width - width) * ((float)xPositionOnScreen / (float)(oldBounds.Width - width)));
			yPositionOnScreen = (int)((float)(newBounds.Height - height) * ((float)yPositionOnScreen / (float)(oldBounds.Height - height)));
		}

		public virtual void setUpForGamePadMode()
		{
		}

		public virtual bool shouldClampGamePadCursor()
		{
			return false;
		}

		public virtual void releaseLeftClick(int x, int y)
		{
		}

		public virtual void leftClickHeld(int x, int y)
		{
		}

		public virtual void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (upperRightCloseButton != null && readyToClose() && upperRightCloseButton.containsPoint(x, y))
			{
				OnTapCloseButton();
			}
		}

		protected void OnTapCloseButton()
		{
			if (TutorialManager.Instance == null || !TutorialManager.Instance.dontAllowExit())
			{
				if (GetType() == typeof(QuestLog) && !TutorialManager.Instance.hasClosedMenu)
				{
					TutorialManager.Instance.hasClosedMenu = true;
					TutorialManager.Instance.completeTutorial(tutorialType.CLOSE_JOURNAL);
				}
				Game1.playSound("bigDeSelect");
				exitThisMenu();
			}
		}

		public virtual bool overrideSnappyMenuCursorMovementBan()
		{
			return false;
		}

		public virtual void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public virtual void receiveKeyPress(Keys key)
		{
			if (key != 0)
			{
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
				{
					exitThisMenu();
				}
				else if (Game1.options.snappyMenus && Game1.options.gamepadControls && !overrideSnappyMenuCursorMovementBan())
				{
					applyMovementKey(key);
				}
			}
		}

		public virtual void gamePadButtonHeld(Buttons b)
		{
		}

		public virtual ClickableComponent getCurrentlySnappedComponent()
		{
			return currentlySnappedComponent;
		}

		public virtual void receiveScrollWheelAction(int direction)
		{
		}

		public virtual void performHoverAction(int x, int y)
		{
		}

		public virtual void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
		{
			if (upperRightCloseButton != null && shouldDrawCloseButton())
			{
				upperRightCloseButton.draw(b);
			}
		}

		public virtual void draw(SpriteBatch b)
		{
			if (upperRightCloseButton != null && shouldDrawCloseButton())
			{
				upperRightCloseButton.draw(b);
			}
		}

		public virtual bool isWithinBounds(int x, int y)
		{
			if (x - xPositionOnScreen < width && x - xPositionOnScreen >= 0 && y - yPositionOnScreen < height)
			{
				return y - yPositionOnScreen >= 0;
			}
			return false;
		}

		public virtual void update(GameTime time)
		{
		}

		protected virtual void cleanupBeforeExit()
		{
		}

		public virtual bool shouldDrawCloseButton()
		{
			return true;
		}

		public void exitThisMenuNoSound()
		{
			exitThisMenu(playSound: false);
		}

		public void exitThisMenu(bool playSound = true)
		{
			if (behaviorBeforeCleanup != null)
			{
				behaviorBeforeCleanup(this);
			}
			cleanupBeforeExit();
			if (playSound)
			{
				Game1.playSound("bigDeSelect");
			}
			if (this == Game1.activeClickableMenu)
			{
				Game1.exitActiveMenu();
			}
			else if (Game1.activeClickableMenu is GameMenu && (Game1.activeClickableMenu as GameMenu).GetCurrentPage() == this)
			{
				Game1.exitActiveMenu();
			}
			if (_parentMenu != null)
			{
				IClickableMenu parentMenu = _parentMenu;
				_parentMenu = null;
				parentMenu.SetChildMenu(null);
			}
			if (exitFunction != null)
			{
				onExit onExit = exitFunction;
				exitFunction = null;
				onExit();
			}
		}

		public virtual bool autoCenterMouseCursorForGamepad()
		{
			return true;
		}

		public virtual void emergencyShutDown()
		{
		}

		public virtual bool readyToClose()
		{
			return true;
		}

		protected void drawMobileHorizontalPartition(SpriteBatch b, int xPosition, int yPosition, int partitionWidth, bool small = false)
		{
			if (small)
			{
				b.Draw(Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(xPosition, yPosition, partitionWidth, 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 25), Color.White);
				return;
			}
			b.Draw(Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(xPosition + 40, yPosition, partitionWidth - 64, 24), new Microsoft.Xna.Framework.Rectangle(142, 84, 52, 24), Color.White);
			b.Draw(Game1.menuTexture, new Vector2(xPosition - 8, yPosition), new Microsoft.Xna.Framework.Rectangle(12, 84, 52, 24), Color.White);
			b.Draw(Game1.menuTexture, new Vector2(xPosition + partitionWidth - 48, yPosition), new Microsoft.Xna.Framework.Rectangle(188, 84, 52, 24), Color.White);
		}

		protected void drawMobileVerticalPartition(SpriteBatch b, int xPosition, int yPosition, int partitionHeight, bool small = true)
		{
			if (small)
			{
				Microsoft.Xna.Framework.Rectangle sourceRectForStandardTileSheet = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 26);
				sourceRectForStandardTileSheet.Y += 2;
				sourceRectForStandardTileSheet.Height -= 6;
				b.Draw(Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(xPosition, yPosition, 64, partitionHeight), sourceRectForStandardTileSheet, Color.White);
			}
			else
			{
				Microsoft.Xna.Framework.Rectangle sourceRectForStandardTileSheet2 = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 5);
				b.Draw(Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(xPosition, yPosition + 16, 64, partitionHeight - 32), sourceRectForStandardTileSheet2, Color.White);
			}
		}

		protected void drawMobileVerticalIntersectingPartition(SpriteBatch b, int xPosition, int yPosition, int yOffset)
		{
			b.Draw(Game1.menuTexture, new Vector2(xPosition, yPosition), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 59), Color.White);
			b.Draw(Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(xPosition, yPosition + 64, 64, yPositionOnScreen + height - 64 - yPosition - 64 - yOffset), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 63), Color.White);
			b.Draw(Game1.menuTexture, new Vector2(xPosition, yPositionOnScreen + height - 64 - yOffset), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 62), Color.White);
		}

		protected void drawHorizontalPartition(SpriteBatch b, int yPosition, bool small = false, int red = -1, int green = -1, int blue = -1)
		{
			Color color = ((red == -1) ? Color.White : new Color(red, green, blue));
			Texture2D texture = ((red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture);
			if (small)
			{
				b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 32, yPosition, width - 64, 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 25), color);
				return;
			}
			b.Draw(texture, new Vector2(xPositionOnScreen, yPosition), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 4), color);
			b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64, yPosition, width - 128, 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 6), color);
			b.Draw(texture, new Vector2(xPositionOnScreen + width - 64, yPosition), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 7), color);
		}

		protected void drawVerticalPartition(SpriteBatch b, int xPosition, bool small = false, int red = -1, int green = -1, int blue = -1)
		{
			Color color = ((red == -1) ? Color.White : new Color(red, green, blue));
			Texture2D texture = ((red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture);
			if (small)
			{
				b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(xPosition, yPositionOnScreen + 64 + 32, 64, height - 128), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 26), color);
				return;
			}
			b.Draw(texture, new Vector2(xPosition, yPositionOnScreen + 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 1), color);
			b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(xPosition, yPositionOnScreen + 128, 64, height - 192), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 5), color);
			b.Draw(texture, new Vector2(xPosition, yPositionOnScreen + height - 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 13), color);
		}

		protected void drawVerticalIntersectingPartition(SpriteBatch b, int xPosition, int yPosition, int red = -1, int green = -1, int blue = -1)
		{
			Color color = ((red == -1) ? Color.White : new Color(red, green, blue));
			Texture2D texture = ((red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture);
			b.Draw(texture, new Vector2(xPosition, yPosition), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 59), color);
			b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(xPosition, yPosition + 64, 64, yPositionOnScreen + height - 64 - yPosition - 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 63), color);
			b.Draw(texture, new Vector2(xPosition, yPositionOnScreen + height - 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 62), color);
		}

		protected void drawVerticalUpperIntersectingPartition(SpriteBatch b, int xPosition, int partitionHeight, int red = -1, int green = -1, int blue = -1)
		{
			Color color = ((red == -1) ? Color.White : new Color(red, green, blue));
			Texture2D texture = ((red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture);
			b.Draw(texture, new Vector2(xPosition, yPositionOnScreen + 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 44), color);
			b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(xPosition, yPositionOnScreen + 128, 64, partitionHeight - 32), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 63), color);
			b.Draw(texture, new Vector2(xPosition, yPositionOnScreen + partitionHeight + 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 39), color);
		}

		public static void drawTextureBox(SpriteBatch b, int x, int y, int width, int height, Color color)
		{
			drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(107, 80, 15, 15), x, y, width, height, color, 4f);
		}

		public static void drawTextureBoxWithIcon(SpriteBatch b, Texture2D texture, Microsoft.Xna.Framework.Rectangle sourceRect, Texture2D iconTexture, Microsoft.Xna.Framework.Rectangle iconSourceRect, int x, int y, int width, int height, Color color, float scale = 1f, bool drawShadow = true)
		{
			int num = (height - iconSourceRect.Height) / 2;
			int num2 = 48 - iconSourceRect.Width / 2;
			drawTextureBox(b, texture, sourceRect, x, y, width, height, color, scale, drawShadow);
			b.Draw(iconTexture, new Vector2(x, y + num), iconSourceRect, color);
		}

		public static void drawButtonWithText(SpriteBatch b, SpriteFont font, string text, int x, int y, int width, int height, Color color, bool isClickable = true, bool heldDown = false)
		{
			drawTextureBoxWithIconAndText(b, font, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(256, 256, 10, 10), null, new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1), text, x, y, width, height, color, 4f, drawShadow: true, iconLeft: false, isClickable, heldDown, drawIcon: false);
		}

		public static void drawTextureBox(SpriteBatch b, Texture2D texture, Microsoft.Xna.Framework.Rectangle sourceRect, int x, int y, int width, int height, Color color, float scale = 1f, bool drawShadow = true, float draw_layer = -1f, bool ignoreBorder = false)
		{
			lastTextureBoxRect = new Microsoft.Xna.Framework.Rectangle(x, y, width, height);
			int num = sourceRect.Width / 3;
			float layerDepth = draw_layer - 0.03f;
			if (draw_layer < 0f)
			{
				draw_layer = 0.8f - (float)y * 1E-06f;
				layerDepth = 0.77f;
			}
			if (drawShadow && !ignoreBorder)
			{
				b.Draw(texture, new Vector2(x - 8, y + 8), new Microsoft.Xna.Framework.Rectangle(sourceRect.X, sourceRect.Y, num, num), Color.Black * 0.4f, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				b.Draw(texture, new Vector2(x + width - (int)((float)num * scale) - 8, y + 8), new Microsoft.Xna.Framework.Rectangle(sourceRect.X + sourceRect.Width - num, sourceRect.Y, num, num), Color.Black * 0.4f, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				b.Draw(texture, new Vector2(x - 8, y + height - (int)((float)num * scale) + 8), new Microsoft.Xna.Framework.Rectangle(sourceRect.X, sourceRect.Y + sourceRect.Height - num, num, num), Color.Black * 0.4f, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				b.Draw(texture, new Vector2(x + width - (int)((float)num * scale) - 8, y + height - (int)((float)num * scale) + 8), new Microsoft.Xna.Framework.Rectangle(sourceRect.X + sourceRect.Width - num, sourceRect.Y + sourceRect.Height - num, num, num), Color.Black * 0.4f, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + (int)((float)num * scale) - 8, y + 8, width - (int)((float)num * scale) * 2, (int)((float)num * scale)), new Microsoft.Xna.Framework.Rectangle(sourceRect.X + num, sourceRect.Y, num, num), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
				b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + (int)((float)num * scale) - 8, y + height - (int)((float)num * scale) + 8, width - (int)((float)num * scale) * 2, (int)((float)num * scale)), new Microsoft.Xna.Framework.Rectangle(sourceRect.X + num, sourceRect.Y + sourceRect.Height - num, num, num), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
				b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x - 8, y + (int)((float)num * scale) + 8, (int)((float)num * scale), height - (int)((float)num * scale) * 2), new Microsoft.Xna.Framework.Rectangle(sourceRect.X, num + sourceRect.Y, num, num), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
				b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + width - (int)((float)num * scale) - 8, y + (int)((float)num * scale) + 8, (int)((float)num * scale), height - (int)((float)num * scale) * 2), new Microsoft.Xna.Framework.Rectangle(sourceRect.X + sourceRect.Width - num, num + sourceRect.Y, num, num), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
				b.Draw(texture, new Microsoft.Xna.Framework.Rectangle((int)((float)num * scale / 2f) + x - 8, (int)((float)num * scale / 2f) + y + 8, width - (int)((float)num * scale), height - (int)((float)num * scale)), new Microsoft.Xna.Framework.Rectangle(num + sourceRect.X, num + sourceRect.Y, num, num), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
			}
			b.Draw(texture, new Microsoft.Xna.Framework.Rectangle((int)((float)num * scale) + x, (int)((float)num * scale) + y, width - (int)((float)num * scale * 2f), height - (int)((float)num * scale * 2f)), new Microsoft.Xna.Framework.Rectangle(num + sourceRect.X, num + sourceRect.Y, num, num), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
			if (!ignoreBorder)
			{
				b.Draw(texture, new Vector2(x, y), new Microsoft.Xna.Framework.Rectangle(sourceRect.X, sourceRect.Y, num, num), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
				b.Draw(texture, new Vector2(x + width - (int)((float)num * scale), y), new Microsoft.Xna.Framework.Rectangle(sourceRect.X + sourceRect.Width - num, sourceRect.Y, num, num), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
				b.Draw(texture, new Vector2(x, y + height - (int)((float)num * scale)), new Microsoft.Xna.Framework.Rectangle(sourceRect.X, sourceRect.Y + sourceRect.Height - num, num, num), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
				b.Draw(texture, new Vector2(x + width - (int)((float)num * scale), y + height - (int)((float)num * scale)), new Microsoft.Xna.Framework.Rectangle(sourceRect.X + sourceRect.Width - num, sourceRect.Y + sourceRect.Height - num, num, num), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
				b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + (int)((float)num * scale), y, width - (int)((float)num * scale) * 2, (int)((float)num * scale)), new Microsoft.Xna.Framework.Rectangle(sourceRect.X + num, sourceRect.Y, num, num), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
				b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + (int)((float)num * scale), y + height - (int)((float)num * scale), width - (int)((float)num * scale) * 2, (int)((float)num * scale)), new Microsoft.Xna.Framework.Rectangle(sourceRect.X + num, sourceRect.Y + sourceRect.Height - num, num, num), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
				b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x, y + (int)((float)num * scale), (int)((float)num * scale), height - (int)((float)num * scale) * 2), new Microsoft.Xna.Framework.Rectangle(sourceRect.X, num + sourceRect.Y, num, num), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
				b.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + width - (int)((float)num * scale), y + (int)((float)num * scale), (int)((float)num * scale), height - (int)((float)num * scale) * 2), new Microsoft.Xna.Framework.Rectangle(sourceRect.X + sourceRect.Width - num, num + sourceRect.Y, num, num), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
			}
		}

		public static void drawTextureBoxWithIconAndText(SpriteBatch b, SpriteFont font, Texture2D texture, Microsoft.Xna.Framework.Rectangle sourceRect, Texture2D iconTexture, Microsoft.Xna.Framework.Rectangle iconSourceRect, string text, int x, int y, int width, int height, Color color, float scale = 1f, bool drawShadow = true, bool iconLeft = true, bool isClickable = true, bool heldDown = false, bool drawIcon = true, bool reverseColors = false, bool bold = true)
		{
			int num = (int)font.MeasureString(text).X;
			int num2 = (int)font.MeasureString(text).Y;
			int num3 = 0;
			int num4 = 0;
			float num5 = 1f;
			float num6 = Math.Min(3f, height * 3 / 4 / iconSourceRect.Height);
			Vector2 vector = new Vector2(num6, num6);
			int num7 = 16;
			int num8 = num + num7 + (int)((float)iconSourceRect.Width * vector.X);
			if (heldDown)
			{
				num3 = -3;
				num4 = 3;
			}
			drawTextureBox(b, texture, sourceRect, x + num3, y + num4, width, height, (reverseColors ? (heldDown ? Color.White : Color.Wheat) : (heldDown ? Color.Wheat : color)) * (isClickable ? 1f : 0.5f), scale, !heldDown && drawShadow);
			int num10;
			if (drawIcon)
			{
				if (num8 >= width - num7 * 2)
				{
					drawIcon = false;
					num8 = num;
				}
				if (num8 >= width - num7 * 2)
				{
					num = (int)((float)num * num5);
					num7 = (int)((float)num7 * num5);
					num8 = num + num7;
					num2 = (int)((float)num2 * num5);
				}
				int num9;
				if (iconLeft)
				{
					num9 = num7;
					num10 = Math.Max((width - num) / 2, num9 + (int)((float)iconSourceRect.Width * scale));
				}
				else
				{
					num8 -= (int)(0.75 * (double)num7);
					num10 = (width - num8) / 2;
					num9 = num10 + num + num7 / 4;
				}
				if (drawIcon)
				{
					int num11 = (int)(((float)height - (float)iconSourceRect.Height * vector.Y) / 2f);
					b.Draw(iconTexture, new Vector2(x + num9 + num3, y + num11 + num4), iconSourceRect, Color.White, 0f, Vector2.Zero, vector * num5, SpriteEffects.None, 0.08f);
				}
				else
				{
					num10 = (width - num) / 2;
				}
			}
			else
			{
				num10 = (width - num) / 2;
			}
			int num12 = (height - num2) / 2 + 3;
			if (!text.Contains('\n'))
			{
				if (bold)
				{
					Utility.drawBoldText(b, text, font, Utility.To4(new Vector2(x + num10 + num3, y + num12 + num4)), isClickable ? Game1.textColor : Color.Red, num5);
				}
				else
				{
					b.DrawString(font, text, Utility.To4(new Vector2(x + num10 + num3, y + num12 + num4)), isClickable ? Game1.textColor : Color.Red, 0f, Vector2.Zero, num5, SpriteEffects.None, 0.08f);
				}
			}
			else
			{
				Utility.drawMultiLineTextWithShadow(b, text, font, new Vector2(x + num3, y + 20 + num4), width, height - 16 - num4 * 2, isClickable ? Game1.textColor : Color.Red, centreY: true, actuallyDrawIt: true, drawShadows: false, centerX: true, bold, close: true);
			}
		}

		public static void DrawRedBox(SpriteBatch b, int x, int y, int width, int height, int thickness = 4, float layerDepth = 0.08f)
		{
			if (thickness < 1 || height < thickness || width < thickness)
			{
				if (width > 0 && height > 0)
				{
					b.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(x, y, width, height), new Microsoft.Xna.Framework.Rectangle(80, 193, 2, 2), Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
				}
			}
			else
			{
				b.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(x, y, width, thickness), new Microsoft.Xna.Framework.Rectangle(80, 193, 2, 2), Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
				b.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(x, y + height - thickness, width, thickness), new Microsoft.Xna.Framework.Rectangle(80, 193, 2, 2), Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
				b.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(x, y, thickness, height), new Microsoft.Xna.Framework.Rectangle(80, 193, 2, 2), Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
				b.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(x + width - thickness, y, thickness, height), new Microsoft.Xna.Framework.Rectangle(80, 193, 2, 2), Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
			}
		}

		public void drawBorderLabel(SpriteBatch b, string text, SpriteFont font, int x, int y)
		{
			int num = (int)font.MeasureString(text).X;
			y += 52;
			b.Draw(Game1.mouseCursors, new Vector2(x, y), new Microsoft.Xna.Framework.Rectangle(256, 267, 6, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2(x + 24, y), new Microsoft.Xna.Framework.Rectangle(262, 267, 1, 16), Color.White, 0f, Vector2.Zero, new Vector2(num, 4f), SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2(x + 24 + num, y), new Microsoft.Xna.Framework.Rectangle(263, 267, 6, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			Utility.drawTextWithShadow(b, text, font, new Vector2(x + 24, y + 20), Game1.textColor);
		}

		public static void drawToolTipOverridePosition(SpriteBatch b, string hoverText, string hoverTitle, Item hoveredItem, int overrideX = -1, int overrideY = -1)
		{
			bool flag = hoveredItem != null && hoveredItem is Object && (int)(hoveredItem as Object).edibility != -300;
			drawHoverText(b, hoverText, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja) ? Game1.dialogueFont : Game1.smallFont, 0, 0, -1, hoverTitle, flag ? ((int)(hoveredItem as Object).edibility) : (-1), (flag && Game1.objectInformation[(hoveredItem as Object).parentSheetIndex].Split('/').Length > 7) ? Game1.objectInformation[(hoveredItem as Object).parentSheetIndex].Split('/')[7].Split(' ') : null, hoveredItem, 0, -1, -1, overrideX, overrideY);
		}

		public static void drawToolTip(SpriteBatch b, string hoverText, string hoverTitle, Item hoveredItem, bool heldItem = false, int healAmountToDisplay = -1, int currencySymbol = 0, int extraItemToShowIndex = -1, int extraItemToShowAmount = -1, CraftingRecipe craftingIngredients = null, int moneyAmountToShowAtBottom = -1)
		{
			bool flag = hoveredItem != null && hoveredItem is Object && (int)(hoveredItem as Object).edibility != -300;
			int num = Math.Max((healAmountToDisplay != -1) ? ((int)Game1.smallFont.MeasureString(healAmountToDisplay + "+ Energy").X + 32) : 0, Math.Max((int)Game1.smallFont.MeasureString(hoverText).X, (hoverTitle != null) ? ((int)Game1.dialogueFont.MeasureString(hoverTitle).X) : 0)) + 32;
			if (flag)
			{
				int num2 = 9999;
				int num3 = 92;
				num = (int)Math.Max(num, Math.Max(Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Energy", num2)).X + (float)num3, Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Health", num2)).X + (float)num3));
			}
			drawHoverText(b, hoverText, Game1.smallFont, heldItem ? 40 : 0, heldItem ? 40 : 0, moneyAmountToShowAtBottom, hoverTitle, flag ? ((int)(hoveredItem as Object).edibility) : (-1), (flag && Game1.objectInformation[(hoveredItem as Object).parentSheetIndex].Split('/').Length > 7) ? hoveredItem.ModifyItemBuffs(Game1.objectInformation[(hoveredItem as Object).parentSheetIndex].Split('/')[7].Split(' ')) : null, hoveredItem, currencySymbol, extraItemToShowIndex, extraItemToShowAmount, -1, -1, 1f, craftingIngredients);
		}

		public static void drawHoverText(SpriteBatch b, string text, SpriteFont font, int xOffset = 0, int yOffset = 0, int moneyAmountToDisplayAtBottom = -1, string boldTitleText = null, int healAmountToDisplay = -1, string[] buffIconsToDisplay = null, Item hoveredItem = null, int currencySymbol = 0, int extraItemToShowIndex = -1, int extraItemToShowAmount = -1, int overrideX = -1, int overrideY = -1, float alpha = 1f, CraftingRecipe craftingIngredients = null, IList<Item> additional_craft_materials = null, int inventoryPosition = -1, int squareSide = 80, int stackNumber = -1)
		{
			StringBuilder text2 = null;
			if (text != null)
			{
				text2 = new StringBuilder(text);
			}
			drawHoverText(b, text2, font, xOffset, yOffset, moneyAmountToDisplayAtBottom, boldTitleText, healAmountToDisplay, buffIconsToDisplay, hoveredItem, currencySymbol, extraItemToShowIndex, extraItemToShowAmount, overrideX, overrideY, alpha, craftingIngredients, additional_craft_materials, inventoryPosition, squareSide, stackNumber);
		}

		public static void drawHoverText(SpriteBatch b, StringBuilder text, SpriteFont font, int xOffset = 0, int yOffset = 0, int moneyAmountToDisplayAtBottom = -1, string boldTitleText = null, int healAmountToDisplay = -1, string[] buffIconsToDisplay = null, Item hoveredItem = null, int currencySymbol = 0, int extraItemToShowIndex = -1, int extraItemToShowAmount = -1, int overrideX = -1, int overrideY = -1, float alpha = 1f, CraftingRecipe craftingIngredients = null, IList<Item> additional_craft_materials = null, int inventoryPosition = -1, int squareSide = 80, int stackNumber = -1)
		{
			if (text == null || text.Length == 0)
			{
				return;
			}
			string text2 = null;
			if (boldTitleText != null && boldTitleText.Length == 0)
			{
				boldTitleText = null;
			}
			int num = 20;
			int num2 = Math.Max((healAmountToDisplay != -1) ? ((int)font.MeasureString(healAmountToDisplay + "+ Energy" + 32).X) : 0, Math.Max((int)font.MeasureString(text).X, (boldTitleText != null) ? ((int)Game1.dialogueFont.MeasureString(boldTitleText).X) : 0)) + 32;
			int num3 = Math.Max(num * 3, (int)font.MeasureString(text).Y + 32 + (int)((moneyAmountToDisplayAtBottom > -1) ? (font.MeasureString(moneyAmountToDisplayAtBottom.ToString() ?? "").Y + 4f) : 8f) + (int)((boldTitleText != null) ? (Game1.dialogueFont.MeasureString(boldTitleText).Y + 16f) : 0f));
			if (extraItemToShowIndex != -1)
			{
				string[] array = Game1.objectInformation[extraItemToShowIndex].Split('/');
				string text3 = array[0];
				if (LocalizedContentManager.CurrentLanguageCode != 0)
				{
					text3 = array[4];
				}
				string text4 = Game1.content.LoadString("Strings\\UI:ItemHover_Requirements", extraItemToShowAmount, (extraItemToShowAmount > 1) ? Lexicon.makePlural(text3) : text3);
				int num4 = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, extraItemToShowIndex, 16, 16).Width * 2 * 4;
				num2 = Math.Max(num2, num4 + (int)font.MeasureString(text4).X);
			}
			if (buffIconsToDisplay != null)
			{
				foreach (string text5 in buffIconsToDisplay)
				{
					if (!text5.Equals("0"))
					{
						num3 += 34;
					}
				}
				num3 += 4;
			}
			if (craftingIngredients != null && Game1.options.showAdvancedCraftingInformation && craftingIngredients.getCraftCountText() != null)
			{
				num3 += (int)font.MeasureString("T").Y;
			}
			string text6 = null;
			if (hoveredItem != null)
			{
				num3 += 68 * hoveredItem.attachmentSlots();
				text6 = hoveredItem.getCategoryName();
				if (text6.Length > 0)
				{
					num2 = Math.Max(num2, (int)font.MeasureString(text6).X + 32);
					num3 += (int)font.MeasureString("T").Y;
				}
				int num5 = 9999;
				int num6 = 92;
				Point extraSpaceNeededForTooltipSpecialIcons = hoveredItem.getExtraSpaceNeededForTooltipSpecialIcons(font, num2, num6, num3, text, boldTitleText, moneyAmountToDisplayAtBottom);
				num2 = ((extraSpaceNeededForTooltipSpecialIcons.X != 0) ? extraSpaceNeededForTooltipSpecialIcons.X : num2);
				num3 = ((extraSpaceNeededForTooltipSpecialIcons.Y != 0) ? extraSpaceNeededForTooltipSpecialIcons.Y : num3);
				if (hoveredItem is MeleeWeapon && (hoveredItem as MeleeWeapon).GetTotalForgeLevels() > 0)
				{
					num3 += (int)font.MeasureString("T").Y;
				}
				if (hoveredItem is MeleeWeapon && (hoveredItem as MeleeWeapon).GetEnchantmentLevel<GalaxySoulEnchantment>() > 0)
				{
					num3 += (int)font.MeasureString("T").Y;
				}
				if (hoveredItem is Object && (int)(hoveredItem as Object).edibility != -300)
				{
					num3 = ((healAmountToDisplay == -1) ? (num3 + 40) : (num3 + 40 * ((healAmountToDisplay <= 0) ? 1 : 2)));
					healAmountToDisplay = (hoveredItem as Object).staminaRecoveredOnConsumption();
					num2 = (int)Math.Max(num2, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Energy", num5)).X + (float)num6, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Health", num5)).X + (float)num6));
				}
				if (buffIconsToDisplay != null)
				{
					for (int j = 0; j < buffIconsToDisplay.Length; j++)
					{
						if (!buffIconsToDisplay[j].Equals("0") && j <= 11)
						{
							num2 = (int)Math.Max(num2, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Buff" + j, num5)).X + (float)num6);
						}
					}
				}
				if (hoveredItem is MeleeWeapon)
				{
					MeleeWeapon meleeWeapon = hoveredItem as MeleeWeapon;
					num3 = Math.Max(num * 3, (int)((boldTitleText != null) ? (Game1.dialogueFont.MeasureString(boldTitleText).Y + 16f) : 0f) + 32) + (int)font.MeasureString("T").Y + (int)((moneyAmountToDisplayAtBottom > -1) ? (font.MeasureString(moneyAmountToDisplayAtBottom.ToString() ?? "").Y + 4f) : 0f);
					num3 += ((!(hoveredItem.Name == "Scythe")) ? ((hoveredItem as MeleeWeapon).getNumberOfDescriptionCategories() * 4 * 12) : 0);
					num3 += (int)font.MeasureString(Game1.parseText((hoveredItem as MeleeWeapon).description, Game1.smallFont, 272)).Y;
					num2 = (int)Math.Max(num2, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Damage", num5, num5)).X + (float)num6, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Speed", num5)).X + (float)num6, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", num5)).X + (float)num6, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_CritChanceBonus", num5)).X + (float)num6, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_CritPowerBonus", num5)).X + (float)num6, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Weight", num5)).X + (float)num6))))));
					if ((hoveredItem as MeleeWeapon).GetTotalForgeLevels() > 0)
					{
						num3 += (int)font.MeasureString("T").Y;
					}
					foreach (BaseEnchantment enchantment in (hoveredItem as MeleeWeapon).enchantments)
					{
						if (!enchantment.IsForge() && !enchantment.IsSecondaryEnchantment())
						{
							num3 += (int)font.MeasureString("T").Y + 12;
						}
					}
					if ((hoveredItem as MeleeWeapon).GetEnchantmentLevel<GalaxySoulEnchantment>() > 0)
					{
						num3 += (int)font.MeasureString("T").Y;
					}
				}
				else if (hoveredItem is Boots)
				{
					num3 -= (int)font.MeasureString(text).Y;
					num3 += (int)((float)((hoveredItem as Boots).getNumberOfDescriptionCategories() * 4 * 12) + font.MeasureString(Game1.parseText((hoveredItem as Boots).description, Game1.smallFont, 272)).Y);
					num2 = (int)Math.Max(num2, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", num5)).X + (float)num6, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", num5)).X + (float)num6));
				}
				if (stackNumber > 0)
				{
					int num7 = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, extraItemToShowIndex, 16, 16).Width * 2 * 4;
					num2 = Math.Max(num2, num7 + (int)Game1.dialogueFont.MeasureString(boldTitleText).X);
					if (string.IsNullOrEmpty(text6))
					{
						num3 += (int)font.MeasureString("T").Y;
					}
				}
			}
			Vector2 vector = Vector2.Zero;
			if (craftingIngredients != null)
			{
				if (Game1.options.showAdvancedCraftingInformation)
				{
					int craftableCount = craftingIngredients.getCraftableCount(additional_craft_materials);
					if (craftableCount > 1)
					{
						text2 = " (" + craftableCount + ")";
						vector = Game1.smallFont.MeasureString(text2);
					}
				}
				num2 = (int)Math.Max(Game1.dialogueFont.MeasureString(boldTitleText).X + vector.X + 12f, 384f);
				num3 += craftingIngredients.getDescriptionHeight(num2 - 8) + ((healAmountToDisplay == -1) ? (-32) : 0);
			}
			else if (text2 != null && boldTitleText != null)
			{
				vector = Game1.smallFont.MeasureString(text2);
				num2 = (int)Math.Max(num2, Game1.dialogueFont.MeasureString(boldTitleText).X + vector.X + 12f);
			}
			int x = Game1.input.GetMouseState().X + 32 + xOffset;
			int num8 = Game1.input.GetMouseState().Y + 32 + yOffset;
			if (Game1.IsActiveClickableMenuNativeScaled)
			{
				x = (int)((float)Game1.input.GetMouseState().X / Game1.NativeZoomLevel) + 32 + xOffset;
				num8 = (int)((float)Game1.input.GetMouseState().Y / Game1.NativeZoomLevel) + 32 + yOffset;
			}
			if (overrideX != -1)
			{
				x = overrideX;
			}
			if (overrideY != -1)
			{
				num8 = overrideY;
			}
			if (x + num2 > Utility.getSafeArea().Right)
			{
				x = Utility.getSafeArea().Right - num2;
				num8 += 16;
			}
			if (num8 + num3 > Utility.getSafeArea().Bottom - (MobileDisplay.IsiPhoneX ? 64 : 0))
			{
				x += 16;
				if (x + num2 > Utility.getSafeArea().Right)
				{
					x = Utility.getSafeArea().Right - num2;
				}
				num8 = Utility.getSafeArea().Bottom - num3 - (MobileDisplay.IsiPhoneX ? 64 : 0);
			}
			drawTextureBox(b, Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(0, 256, 60, 60), x, num8, num2 + ((craftingIngredients != null) ? 21 : 0), num3, Color.White * alpha);
			bool flag = !string.IsNullOrEmpty(text6) || stackNumber > 0;
			if (boldTitleText != null)
			{
				Vector2 vector2 = Game1.dialogueFont.MeasureString(boldTitleText);
				drawTextureBox(b, Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(0, 256, 60, 60), x, num8, num2 + ((craftingIngredients != null) ? 21 : 0), (int)Game1.dialogueFont.MeasureString(boldTitleText).Y + 32 + (int)((hoveredItem != null && flag) ? font.MeasureString("asd").Y : 0f) - 4, Color.White * alpha, 1f, drawShadow: false);
				b.Draw(Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(x + 12, num8 + (int)Game1.dialogueFont.MeasureString(boldTitleText).Y + 32 + (int)((hoveredItem != null && flag) ? font.MeasureString("asd").Y : 0f) - 4, num2 - 4 * ((craftingIngredients != null) ? 1 : 6), 4), new Microsoft.Xna.Framework.Rectangle(44, 300, 4, 4), Color.White);
				b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2(x + 16, num8 + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
				b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2(x + 16, num8 + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
				b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2(x + 16, num8 + 16 + 4), Game1.textColor);
				if (text2 != null)
				{
					Utility.drawTextWithShadow(b, text2, Game1.smallFont, new Vector2((float)(x + 16) + vector2.X, (int)((float)(num8 + 16 + 4) + vector2.Y / 2f - vector.Y / 2f)), Game1.textColor);
				}
				if (stackNumber != -1 && boldTitleText != null)
				{
					hoveredItem.drawInMenuWithStackNumber(b, new Vector2(x + num2 + ((craftingIngredients != null) ? 21 : 0) - 16 - 64, num8 + 16), 1f, stackNumber);
				}
				num8 += (int)Game1.dialogueFont.MeasureString(boldTitleText).Y;
			}
			if (hoveredItem != null && text6.Length > 0)
			{
				num8 -= 4;
				Utility.drawTextWithShadow(b, text6, font, new Vector2(x + 16, num8 + 16 + 4), hoveredItem.getCategoryColor(), 1f, -1f, 2, 2);
				num8 += (int)font.MeasureString("T").Y + ((boldTitleText != null) ? 16 : 0) + 4;
				if (hoveredItem is Tool && (hoveredItem as Tool).GetTotalForgeLevels() > 0)
				{
					string text7 = Game1.content.LoadString("Strings\\UI:Item_Tooltip_Forged");
					Utility.drawTextWithShadow(b, text7, font, new Vector2(x + 16, num8 + 16 + 4), Color.DarkRed, 1f, -1f, 2, 2);
					int totalForgeLevels = (hoveredItem as Tool).GetTotalForgeLevels();
					if (totalForgeLevels < (hoveredItem as Tool).GetMaxForges() && !(hoveredItem as Tool).hasEnchantmentOfType<DiamondEnchantment>())
					{
						Utility.drawTextWithShadow(b, " (" + totalForgeLevels + "/" + (hoveredItem as Tool).GetMaxForges() + ")", font, new Vector2((float)(x + 16) + font.MeasureString(text7).X, num8 + 16 + 4), Color.DimGray, 1f, -1f, 2, 2);
					}
					num8 += (int)font.MeasureString("T").Y;
				}
				if (hoveredItem is MeleeWeapon && (hoveredItem as MeleeWeapon).GetEnchantmentLevel<GalaxySoulEnchantment>() > 0)
				{
					GalaxySoulEnchantment enchantmentOfType = (hoveredItem as MeleeWeapon).GetEnchantmentOfType<GalaxySoulEnchantment>();
					string text8 = Game1.content.LoadString("Strings\\UI:Item_Tooltip_GalaxyForged");
					Utility.drawTextWithShadow(b, text8, font, new Vector2(x + 16, num8 + 16 + 4), Color.DarkRed, 1f, -1f, 2, 2);
					int level = enchantmentOfType.GetLevel();
					if (level < enchantmentOfType.GetMaximumLevel())
					{
						Utility.drawTextWithShadow(b, " (" + level + "/" + enchantmentOfType.GetMaximumLevel() + ")", font, new Vector2((float)(x + 16) + font.MeasureString(text8).X, num8 + 16 + 4), Color.DimGray, 1f, -1f, 2, 2);
					}
					num8 += (int)font.MeasureString("T").Y;
				}
			}
			else
			{
				if (flag)
				{
					num8 += (int)font.MeasureString("T").Y;
				}
				num8 += ((boldTitleText != null) ? 16 : 0);
			}
			if (hoveredItem != null && craftingIngredients == null)
			{
				hoveredItem.drawTooltip(b, ref x, ref num8, font, alpha, text);
			}
			else if (text != null && text.Length != 0 && (text.Length != 1 || text[0] != ' '))
			{
				b.DrawString(font, text, new Vector2(x + 16, num8 + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor * alpha);
				b.DrawString(font, text, new Vector2(x + 16, num8 + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor * alpha);
				b.DrawString(font, text, new Vector2(x + 16, num8 + 16 + 4) + new Vector2(2f, 0f), Game1.textShadowColor * alpha);
				b.DrawString(font, text, new Vector2(x + 16, num8 + 16 + 4), Game1.textColor * 0.9f * alpha);
				num8 += (int)font.MeasureString(text).Y + 4;
			}
			if (craftingIngredients != null)
			{
				craftingIngredients.drawRecipeDescription(b, new Vector2(x + 16, num8 - 8), num2, additional_craft_materials);
				num8 += craftingIngredients.getDescriptionHeight(num2 - 8);
			}
			if (healAmountToDisplay != -1)
			{
				int num9 = (hoveredItem as Object).staminaRecoveredOnConsumption();
				if (num9 >= 0)
				{
					int num10 = (hoveredItem as Object).healthRecoveredOnConsumption();
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(x + 16 + 4, num8 + 16), new Microsoft.Xna.Framework.Rectangle((num9 < 0) ? 140 : 0, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.95f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Energy", ((num9 > 0) ? "+" : "") + num9), font, new Vector2(x + 16 + 34 + 4, num8 + 16), Game1.textColor);
					num8 += 34;
					if (Game1.options.bigFonts)
					{
						num8 += 4;
					}
					if (num10 > 0)
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(x + 16 + 4, num8 + 16), new Microsoft.Xna.Framework.Rectangle(0, 438, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.95f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Health", ((num10 > 0) ? "+" : "") + num10), font, new Vector2(x + 16 + 34 + 4, num8 + 16), Game1.textColor);
						num8 += 34;
						if (Game1.options.bigFonts)
						{
							num8 += 4;
						}
					}
				}
				else if (num9 != -300)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(x + 16 + 4, num8 + 16), new Microsoft.Xna.Framework.Rectangle(140, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.95f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Energy", num9.ToString() ?? ""), font, new Vector2(x + 16 + 34 + 4, num8 + 16), Game1.textColor);
					num8 += 34;
					if (Game1.options.bigFonts)
					{
						num8 += 4;
					}
				}
			}
			if (buffIconsToDisplay != null)
			{
				for (int k = 0; k < buffIconsToDisplay.Length; k++)
				{
					if (!buffIconsToDisplay[k].Equals("0"))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(x + 16 + 4, num8 + 16), new Microsoft.Xna.Framework.Rectangle(10 + k * 10, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.95f);
						string text9 = ((Convert.ToInt32(buffIconsToDisplay[k]) > 0) ? "+" : "") + buffIconsToDisplay[k] + " ";
						if (k <= 11)
						{
							text9 = Game1.content.LoadString("Strings\\UI:ItemHover_Buff" + k, text9);
						}
						Utility.drawTextWithShadow(b, text9, font, new Vector2(x + 16 + 34 + 4, num8 + 16), Game1.textColor);
						num8 += 34;
						if (Game1.options.bigFonts)
						{
							num8 += 4;
						}
					}
				}
			}
			if (hoveredItem != null && hoveredItem.attachmentSlots() > 0)
			{
				hoveredItem.drawAttachments(b, x + 16, num8 + 16);
				if (moneyAmountToDisplayAtBottom > -1)
				{
					num8 += 68 * hoveredItem.attachmentSlots();
				}
			}
			if (moneyAmountToDisplayAtBottom > -1)
			{
				b.DrawString(font, moneyAmountToDisplayAtBottom.ToString() ?? "", new Vector2(x + 16, num8 + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
				b.DrawString(font, moneyAmountToDisplayAtBottom.ToString() ?? "", new Vector2(x + 16, num8 + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
				b.DrawString(font, moneyAmountToDisplayAtBottom.ToString() ?? "", new Vector2(x + 16, num8 + 16 + 4) + new Vector2(2f, 0f), Game1.textShadowColor);
				b.DrawString(font, moneyAmountToDisplayAtBottom.ToString() ?? "", new Vector2(x + 16, num8 + 16 + 4), Game1.textColor);
				switch (currencySymbol)
				{
				case 0:
					b.Draw(Game1.debrisSpriteSheet, new Vector2((float)(x + 16) + font.MeasureString(moneyAmountToDisplayAtBottom.ToString() ?? "").X + 20f, num8 + 16 + 16), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 0.95f);
					break;
				case 1:
					b.Draw(Game1.mouseCursors, new Vector2((float)(x + 8) + font.MeasureString(moneyAmountToDisplayAtBottom.ToString() ?? "").X + 20f, num8 + 16 - 5), new Microsoft.Xna.Framework.Rectangle(338, 400, 8, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					break;
				case 2:
					b.Draw(Game1.mouseCursors, new Vector2((float)(x + 8) + font.MeasureString(moneyAmountToDisplayAtBottom.ToString() ?? "").X + 20f, num8 + 16 - 7), new Microsoft.Xna.Framework.Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					break;
				case 4:
					b.Draw(Game1.objectSpriteSheet, new Vector2((float)(x + 8) + font.MeasureString(moneyAmountToDisplayAtBottom.ToString() ?? "").X + 20f, num8 + 16 - 7), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 858, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					break;
				}
				num8 += 48;
			}
			if (extraItemToShowIndex != -1)
			{
				if (moneyAmountToDisplayAtBottom == -1)
				{
					num8 += 8;
				}
				string[] array2 = Game1.objectInformation[extraItemToShowIndex].Split('/');
				string sub = array2[4];
				string text10 = Game1.content.LoadString("Strings\\UI:ItemHover_Requirements", extraItemToShowAmount, sub);
				float num11 = Math.Max(font.MeasureString(text10).Y + 21f, 96f);
				drawTextureBox(b, Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(0, 256, 60, 60), x, num8 + 4, num2 + ((craftingIngredients != null) ? 21 : 0), (int)num11, Color.White);
				num8 += 20;
				b.DrawString(font, text10, new Vector2(x + 16, num8 + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
				b.DrawString(font, text10, new Vector2(x + 16, num8 + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
				b.DrawString(font, text10, new Vector2(x + 16, num8 + 4) + new Vector2(2f, 0f), Game1.textShadowColor);
				b.DrawString(Game1.smallFont, text10, new Vector2(x + 16, num8 + 4), Game1.textColor);
				b.Draw(Game1.objectSpriteSheet, new Vector2(x + 16 + (int)font.MeasureString(text10).X + 21, num8), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, extraItemToShowIndex, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (craftingIngredients != null && Game1.options.showAdvancedCraftingInformation)
			{
				Utility.drawTextWithShadow(b, craftingIngredients.getCraftCountText(), font, new Vector2(x + 16, num8 + 16 + 4), Game1.textColor, 1f, -1f, 2, 2);
				num8 += (int)font.MeasureString("T").Y + 4;
			}
		}

		public void drawMobileFloatingToolTip(SpriteBatch b, int x, int y, int inventoryPosition, int squareSide, string hoverText, string hoverTitle, Item hoveredItem, bool heldItem = false, int healAmountToDisplay = -1, int currencySymbol = 0, int extraItemToShowIndex = -1, int extraItemToShowAmount = -1, CraftingRecipe craftingIngredients = null, int moneyAmountToShowAtBottom = -1, int stackNumber = -1)
		{
			bool flag = hoveredItem != null && hoveredItem is Object && (int)(hoveredItem as Object).edibility != -300;
			drawHoverText(b, hoverText, Game1.smallFont, heldItem ? 40 : 0, heldItem ? 40 : 0, moneyAmountToShowAtBottom, hoverTitle, flag ? ((int)(hoveredItem as Object).edibility) : (-1), (flag && Game1.objectInformation[(hoveredItem as Object).parentSheetIndex].Split('/').Length > 7) ? hoveredItem.ModifyItemBuffs(Game1.objectInformation[(hoveredItem as Object).parentSheetIndex].Split('/')[7].Split(' ')) : null, hoveredItem, currencySymbol, extraItemToShowIndex, extraItemToShowAmount, x, y, 1f, craftingIngredients, null, inventoryPosition, squareSide, stackNumber);
		}

		public static void drawMobileToolTip(SpriteBatch b, int x, int y, int width, int height, int paragraphGap, string hoverText, string hoverTitle, Item hoveredItem, bool heldItem = false, int healAmountToDisplay = -1, int currencySymbol = 0, int extraItemToShowIndex = -1, int extraItemToShowAmount = -1, CraftingRecipe craftingIngredients = null, int moneyAmountToShowAtBottom = -1, int currency = 0, bool inStockAndBuyable = true, bool drawSmall = false)
		{
			bool flag = hoveredItem != null && hoveredItem is Object && (int)(hoveredItem as Object).edibility != -300;
			drawMobileTextPanel(b, hoverText, Game1.smallFont, x, y, width, height, paragraphGap, moneyAmountToShowAtBottom, hoverTitle, flag ? ((int)(hoveredItem as Object).edibility) : (-1), (flag && Game1.objectInformation[(hoveredItem as Object).parentSheetIndex].Split('/').Length > 7) ? hoveredItem.ModifyItemBuffs(Game1.objectInformation[(hoveredItem as Object).parentSheetIndex].Split('/')[7].Split(' ')) : null, hoveredItem, currencySymbol, extraItemToShowIndex, extraItemToShowAmount, -1, -1, 1f, craftingIngredients, currency, inStockAndBuyable, drawBackgroundBox: false, avoidOffscreenCull: false, drawSmall);
		}

		public static int drawMobileTextPanel(SpriteBatch b, string text, SpriteFont font, int x, int y, int width, int height, int paragraphGap = 34, int moneyAmountToDisplayAtBottom = -1, string boldTitleText = null, int healAmountToDisplay = -1, string[] buffIconsToDisplay = null, Item hoveredItem = null, int currencySymbol = 0, int extraItemToShowIndex = -1, int extraItemToShowAmount = -1, int overrideX = -1, int overrideY = -1, float alpha = 1f, CraftingRecipe craftingIngredients = null, int currency = 0, bool inStockAndBuyable = true, bool drawBackgroundBox = false, bool avoidOffscreenCull = false, bool drawSmall = false, IList<Item> additional_craft_materials = null)
		{
			SpriteFont spriteFont = (drawSmall ? Game1.smallFont : Game1.dialogueFont);
			if (text == null || text.Length == 0)
			{
				return 0;
			}
			if (boldTitleText != null && boldTitleText.Length == 0)
			{
				boldTitleText = null;
			}
			int num = y;
			int num2 = 20;
			int num3 = x + width / 16;
			int num4 = width * 7 / 8;
			string text2 = null;
			if (hoveredItem != null)
			{
				text2 = hoveredItem.getCategoryName();
				int num5 = 9999;
				int num6 = 92;
				if (buffIconsToDisplay != null)
				{
					for (int i = 0; i < buffIconsToDisplay.Length; i++)
					{
						if (!buffIconsToDisplay[i].Equals("0") && i <= 11)
						{
							width = (int)Math.Max(width, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Buff" + i, num5)).X + (float)num6);
						}
					}
				}
			}
			if (craftingIngredients != null && Game1.options.showAdvancedCraftingInformation)
			{
				int craftableCount = craftingIngredients.getCraftableCount(additional_craft_materials);
				if (craftableCount > 1)
				{
					boldTitleText = boldTitleText + " (" + craftableCount + ")";
				}
			}
			if (drawBackgroundBox)
			{
				drawTextureBox(b, Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(0, 256, 60, 60), x, y, width, height, Color.White * alpha);
			}
			if (boldTitleText != null)
			{
				boldTitleText = Game1.parseText(boldTitleText, spriteFont, num4);
				if (!drawSmall)
				{
					b.Draw(Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(num3, y + (int)spriteFont.MeasureString(boldTitleText).Y + 32 + (int)((hoveredItem != null && text2.Length > 0) ? font.MeasureString("asd").Y : 0f) - 4, num4, 4), new Microsoft.Xna.Framework.Rectangle(44, 300, 4, 4), Color.White);
					b.DrawString(spriteFont, boldTitleText, new Vector2(num3, y + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
					b.DrawString(spriteFont, boldTitleText, new Vector2(num3, y + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
					b.DrawString(spriteFont, boldTitleText, new Vector2(num3, y + 16 + 4), Game1.textColor);
					y += (int)spriteFont.MeasureString(boldTitleText).Y;
				}
				else
				{
					b.DrawString(spriteFont, boldTitleText, new Vector2(num3, y + 8) + new Vector2(2f, 2f), Game1.textShadowColor);
					b.DrawString(spriteFont, boldTitleText, new Vector2(num3, y + 8) + new Vector2(0f, 2f), Game1.textShadowColor);
					b.DrawString(spriteFont, boldTitleText, new Vector2(num3, y + 8), Game1.textColor);
					y += (int)spriteFont.MeasureString(boldTitleText).Y - 16;
				}
			}
			if (!drawSmall)
			{
				if (hoveredItem != null && text2.Length > 0)
				{
					y -= 4;
					Utility.drawTextWithShadow(b, text2, font, new Vector2(num3, y + 16 + 4), hoveredItem.getCategoryColor(), 1f, -1f, 2, 2);
					y += (int)font.MeasureString("T").Y + ((boldTitleText != null) ? 16 : 0) + 4;
				}
				else
				{
					y += paragraphGap;
				}
			}
			if (hoveredItem != null && hoveredItem is Boots)
			{
				Boots boots = hoveredItem as Boots;
				Utility.drawTextWithShadow(b, Game1.parseText(boots.description, Game1.smallFont, 272), font, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
				y += (int)font.MeasureString(Game1.parseText(boots.description, Game1.smallFont, 272)).Y;
				if ((int)boots.defenseBonus > 0)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num3, y + 16 + 4), new Microsoft.Xna.Framework.Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.089f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", boots.defenseBonus), font, new Vector2(num3 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
				if ((int)boots.immunityBonus > 0)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num3, y + 16 + 4), new Microsoft.Xna.Framework.Rectangle(150, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.089f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", boots.immunityBonus), font, new Vector2(num3 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
			}
			else if (hoveredItem != null && hoveredItem is MeleeWeapon)
			{
				MeleeWeapon meleeWeapon = hoveredItem as MeleeWeapon;
				Utility.drawTextWithShadow(b, Game1.parseText(meleeWeapon.description, Game1.smallFont, num4), font, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
				y += (int)font.MeasureString(Game1.parseText(meleeWeapon.description, Game1.smallFont, 272)).Y;
				if ((int)meleeWeapon.indexOfMenuItemView != 47)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num3, y + 16 + 4), new Microsoft.Xna.Framework.Rectangle(120, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.089f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Damage", meleeWeapon.minDamage, meleeWeapon.maxDamage), font, new Vector2(num3 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
					if ((int)meleeWeapon.speed != (((int)meleeWeapon.type == 2) ? (-8) : 0))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num3, y + 16 + 4), new Microsoft.Xna.Framework.Rectangle(130, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.089f);
						bool flag = ((int)meleeWeapon.type == 2 && (int)meleeWeapon.speed < -8) || ((int)meleeWeapon.type != 2 && (int)meleeWeapon.speed < 0);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Speed", (((((int)meleeWeapon.type == 2) ? ((int)meleeWeapon.speed - -8) : ((int)meleeWeapon.speed)) > 0) ? "+" : "") + (((int)meleeWeapon.type == 2) ? ((int)meleeWeapon.speed - -8) : ((int)meleeWeapon.speed)) / 2), font, new Vector2(num3 + 52, y + 16 + 12), flag ? Color.DarkRed : (Game1.textColor * 0.9f * alpha));
						y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
					}
					if ((int)meleeWeapon.addedDefense > 0)
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num3, y + 16 + 4), new Microsoft.Xna.Framework.Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.089f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", meleeWeapon.addedDefense), font, new Vector2(num3 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
						y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
					}
					if ((double)(float)meleeWeapon.critChance / 0.02 >= 2.0)
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num3, y + 16 + 4), new Microsoft.Xna.Framework.Rectangle(40, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.089f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_CritChanceBonus", (int)((double)(float)meleeWeapon.critChance / 0.02)), font, new Vector2(num3 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
						y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
					}
					if ((double)((float)meleeWeapon.critMultiplier - 3f) / 0.02 >= 1.0)
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num3, y + 16 + 4), new Microsoft.Xna.Framework.Rectangle(160, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.089f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_CritPowerBonus", (int)((double)((float)meleeWeapon.critMultiplier - 3f) / 0.02)), font, new Vector2(num3 + 44, y + 16 + 12), Game1.textColor * 0.9f * alpha);
						y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
					}
					if ((float)meleeWeapon.knockback != meleeWeapon.defaultKnockBackForThisType(meleeWeapon.type))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num3, y + 16 + 4), new Microsoft.Xna.Framework.Rectangle(70, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.089f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Weight", (((float)(int)Math.Ceiling(Math.Abs((float)meleeWeapon.knockback - meleeWeapon.defaultKnockBackForThisType(meleeWeapon.type)) * 10f) > meleeWeapon.defaultKnockBackForThisType(meleeWeapon.type)) ? "+" : "") + (int)Math.Ceiling(Math.Abs((float)meleeWeapon.knockback - meleeWeapon.defaultKnockBackForThisType(meleeWeapon.type)) * 10f)), font, new Vector2(num3 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
						y += paragraphGap;
					}
				}
			}
			else if (!string.IsNullOrEmpty(text) && text != " ")
			{
				if (avoidOffscreenCull)
				{
					y += Utility.drawMultiLineTextWithShadow(b, text, font, new Vector2(num3, y + 16 + 4), num4, height - y, Game1.textColor, centreY: false, actuallyDrawIt: true, drawShadows: true, centerX: false, bold: false, drawSmall, (drawSmall && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 0.75f : 1f);
				}
				else
				{
					string text3 = Game1.parseText(text, Game1.smallFont, num4, (drawSmall && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 0.75f : 1f);
					Utility.drawTextWithShadow(b, text3, Game1.smallFont, new Vector2(num3, y + 16 + 4), Game1.textColor, (drawSmall && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 0.75f : 1f);
					y += (int)Game1.smallFont.MeasureString(text3).Y + 4;
				}
			}
			if (craftingIngredients != null)
			{
				craftingIngredients.drawRecipeDescription(b, new Vector2(num3, y - 8), num4, additional_craft_materials, drawSmall);
				y += craftingIngredients.getDescriptionHeight(width) + paragraphGap;
			}
			if (hoveredItem is Object && (int)(hoveredItem as Object).edibility != -300)
			{
				healAmountToDisplay = (int)Math.Ceiling((double)(hoveredItem as Object).Edibility * 2.5) + (int)(hoveredItem as Object).quality * (hoveredItem as Object).Edibility;
			}
			if (healAmountToDisplay != -1)
			{
				if (healAmountToDisplay > 0)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num3, y + 16), new Microsoft.Xna.Framework.Rectangle((healAmountToDisplay < 0) ? 140 : 0, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.089f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Energy", ((healAmountToDisplay > 0) ? "+" : "") + healAmountToDisplay), font, new Vector2(num3 + 34 + 4, y + 16 + 8 - (Game1.options.bigFonts ? 12 : 0)), Game1.textColor);
					y += paragraphGap;
					if (Game1.options.bigFonts)
					{
						y += 12;
					}
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num3, y + 16), new Microsoft.Xna.Framework.Rectangle(0, 438, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.089f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Health", ((healAmountToDisplay > 0) ? "+" : "") + (int)((float)healAmountToDisplay * 0.4f)), font, new Vector2(num3 + 34 + 4, y + 16 + 8 - (Game1.options.bigFonts ? 12 : 0)), Game1.textColor);
					y += paragraphGap;
					if (Game1.options.bigFonts)
					{
						y += 12;
					}
				}
				else if (healAmountToDisplay != -300)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num3, y + 16), new Microsoft.Xna.Framework.Rectangle(140, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.089f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Energy", healAmountToDisplay.ToString() ?? ""), font, new Vector2(num3 + 34 + 4, y + 16 + 8 - (Game1.options.bigFonts ? 12 : 0)), Game1.textColor);
					y += paragraphGap;
					if (Game1.options.bigFonts)
					{
						y += 12;
					}
				}
			}
			if (buffIconsToDisplay != null)
			{
				for (int j = 0; j < buffIconsToDisplay.Length; j++)
				{
					if (!buffIconsToDisplay[j].Equals("0"))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num3, y + 16), new Microsoft.Xna.Framework.Rectangle(10 + j * 10, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.089f);
						string text4 = ((Convert.ToInt32(buffIconsToDisplay[j]) > 0) ? "+" : "") + buffIconsToDisplay[j] + " ";
						if (j <= 11)
						{
							text4 = Game1.content.LoadString("Strings\\UI:ItemHover_Buff" + j, text4);
						}
						Utility.drawTextWithShadow(b, text4, font, new Vector2(num3 + 34 + 4, y + 16 + 8 - (Game1.options.bigFonts ? 12 : 0)), Game1.textColor);
						y += paragraphGap;
						if (Game1.options.bigFonts)
						{
							y += 12;
						}
					}
				}
			}
			if (hoveredItem != null && hoveredItem.attachmentSlots() > 0)
			{
				y += paragraphGap;
				hoveredItem.drawAttachments(b, num3, y);
				if (moneyAmountToDisplayAtBottom > -1)
				{
					y += 64 * hoveredItem.attachmentSlots();
				}
			}
			if (moneyAmountToDisplayAtBottom > 0)
			{
				int num7 = (int)((double)num4 * 0.75);
				int num8 = num3 + (num4 - num7) / 2;
				int num9 = y + 16 + 4;
				int num10 = 60;
				b.DrawString(font, moneyAmountToDisplayAtBottom.ToString() ?? "", new Vector2(num3, y + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
				b.DrawString(font, moneyAmountToDisplayAtBottom.ToString() ?? "", new Vector2(num3, y + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
				b.DrawString(font, moneyAmountToDisplayAtBottom.ToString() ?? "", new Vector2(num3, y + 16 + 4) + new Vector2(2f, 0f), Game1.textShadowColor);
				b.DrawString(font, moneyAmountToDisplayAtBottom.ToString() ?? "", new Vector2(num3, y + 16 + 4), Game1.textColor);
				switch (currencySymbol)
				{
				case 0:
					b.Draw(Game1.debrisSpriteSheet, new Vector2((float)num3 + font.MeasureString(moneyAmountToDisplayAtBottom.ToString() ?? "").X + 20f, y + 16 + 16), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 0.089f);
					break;
				case 1:
					b.Draw(Game1.mouseCursors, new Vector2((float)num3 + font.MeasureString(moneyAmountToDisplayAtBottom.ToString() ?? "").X + 20f, y + 16 + 4), new Microsoft.Xna.Framework.Rectangle(338, 400, 8, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.089f);
					break;
				case 2:
					b.Draw(Game1.mouseCursors, new Vector2((float)num3 + font.MeasureString(moneyAmountToDisplayAtBottom.ToString() ?? "").X + 20f, y + 16), new Microsoft.Xna.Framework.Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.089f);
					break;
				}
				y += paragraphGap;
			}
			if (extraItemToShowIndex != -1)
			{
				y += paragraphGap;
				string[] array = Game1.objectInformation[extraItemToShowIndex].Split('/');
				string sub = array[4];
				string text5 = Game1.content.LoadString("Strings\\UI:ItemHover_Requirements", extraItemToShowAmount, sub);
				b.DrawString(font, text5, new Vector2(num3, y + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
				b.DrawString(font, text5, new Vector2(num3, y + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
				b.DrawString(font, text5, new Vector2(num3, y + 4) + new Vector2(2f, 0f), Game1.textShadowColor);
				Color color = Game1.textColor;
				if (!Game1.player.hasItemInInventory(extraItemToShowIndex, extraItemToShowAmount))
				{
					color = Color.Red;
				}
				b.DrawString(font, text5, new Vector2(num3, y + 4), color);
				int num11 = num3 + (int)font.MeasureString(text5).X + 21;
				b.Draw(Game1.objectSpriteSheet, new Vector2(num11, y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, extraItemToShowIndex, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.089f);
			}
			if (craftingIngredients != null && Game1.options.showAdvancedCraftingInformation)
			{
				Utility.drawTextWithShadow(b, craftingIngredients.getCraftCountText(), font, new Vector2(num3, y + 16 + 4), Game1.textColor, 1f, -1f, 2, 2);
				y += (int)font.MeasureString("T").Y + 4;
			}
			return y + paragraphGap;
		}
	}
}
