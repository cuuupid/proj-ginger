using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Mobile;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class RenovateMenu : IClickableMenu
	{
		public const int region_okButton = 101;

		public const int region_randomButton = 103;

		public const int region_namingBox = 104;

		public static int menuHeight = 320;

		public static int menuWidth = 448;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent hovered;

		private bool freeze;

		protected HouseRenovation _renovation;

		protected string _oldLocation;

		protected Point _oldPosition;

		protected int _selectedIndex = -1;

		protected int _animatingIndex = -1;

		protected int _buildAnimationTimer;

		protected int _buildAnimationCount;

		private int _lastTapX = -1;

		private int _lastTapY = -1;

		public RenovateMenu(HouseRenovation renovation)
			: base(Game1.uiViewport.Width / 2 - menuWidth / 2 - IClickableMenu.borderWidth * 2, (Game1.uiViewport.Height - menuHeight - IClickableMenu.borderWidth * 2) / 4, menuWidth + IClickableMenu.borderWidth * 2, menuHeight + IClickableMenu.borderWidth)
		{
			height += 64;
			okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
			{
				myID = 101,
				upNeighborID = 103,
				leftNeighborID = 103
			};
			_renovation = renovation;
			menuHeight = 320;
			menuWidth = 448;
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
			SetupForRenovationPlacement();
		}

		public override bool shouldClampGamePadCursor()
		{
			return true;
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public void SetupForReturn()
		{
			freeze = true;
			LocationRequest locationRequest = Game1.getLocationRequest(_oldLocation);
			locationRequest.OnWarp += delegate
			{
				Console.WriteLine("Display farmer true");
				Game1.player.viewingLocation.Value = null;
				Game1.displayHUD = true;
				Game1.displayFarmer = true;
				freeze = false;
				Game1.viewportFreeze = false;
				FinalizeReturn();
			};
			Game1.warpFarmer(locationRequest, _oldPosition.X, _oldPosition.Y, Game1.player.facingDirection);
		}

		public void FinalizeReturn()
		{
			exitThisMenu(playSound: false);
			Game1.player.forceCanMove();
			freeze = false;
		}

		public void SetupForRenovationPlacement()
		{
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.displayFarmer = false;
			_oldLocation = Game1.currentLocation.NameOrUniqueName;
			_oldPosition = Game1.player.getTileLocationPoint();
			Game1.currentLocation = _renovation.location;
			Game1.player.viewingLocation.Value = _renovation.location.NameOrUniqueName;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear();
			freeze = false;
			okButton.bounds.X = Game1.uiViewport.Width - 128;
			okButton.bounds.Y = Game1.uiViewport.Height - 128;
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Vector2 vector = default(Vector2);
			int num = 0;
			foreach (List<Microsoft.Xna.Framework.Rectangle> renovationBound in _renovation.renovationBounds)
			{
				foreach (Microsoft.Xna.Framework.Rectangle item in renovationBound)
				{
					vector.X += item.Center.X;
					vector.Y += item.Center.Y;
					num++;
				}
			}
			if (num > 0)
			{
				vector.X = (int)Math.Round(vector.X / (float)num);
				vector.Y = (int)Math.Round(vector.Y / (float)num);
			}
			Game1.viewport.Location = new Location((int)((vector.X + 0.5f) * 64f) - Game1.viewport.Width / 2, (int)((vector.Y + 0.5f) * 64f) - Game1.viewport.Height / 2);
			Game1.panScreen(0, 0);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			_lastTapX = -1;
			_lastTapY = -1;
			if (Game1.globalFade || freeze)
			{
				return;
			}
			if (okButton != null && okButton.containsPoint(x, y) && readyToClose())
			{
				SetupForReturn();
				Game1.playSound("smallSelect");
				return;
			}
			Vector2 vector = new Vector2((Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64f, (Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64f);
			for (int i = 0; i < _renovation.renovationBounds.Count; i++)
			{
				List<Microsoft.Xna.Framework.Rectangle> list = _renovation.renovationBounds[i];
				foreach (Microsoft.Xna.Framework.Rectangle item in list)
				{
					if (item.Contains((int)vector.X, (int)vector.Y))
					{
						CompleteRenovation(i);
						return;
					}
				}
			}
		}

		public virtual void AnimateRenovation()
		{
			if (_buildAnimationTimer == 0)
			{
				return;
			}
			_buildAnimationTimer -= (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			if (_buildAnimationTimer > 0)
			{
				return;
			}
			if (_buildAnimationCount > 0)
			{
				_buildAnimationCount--;
				if (_renovation.animationType == HouseRenovation.AnimationType.Destroy)
				{
					_buildAnimationTimer = 50;
					for (int i = 0; i < 5; i++)
					{
						Microsoft.Xna.Framework.Rectangle random = Utility.GetRandom(_renovation.renovationBounds[_animatingIndex]);
						int num = (int)Utility.RandomFloat((random.Left - 1) * 64, 64 * random.Right);
						int num2 = (int)Utility.RandomFloat((random.Top - 1) * 64, 64 * random.Bottom);
						_renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(num, num2), flicker: false, Game1.random.NextDouble() < 0.5));
						_renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(num, num2), flicker: false, Game1.random.NextDouble() < 0.5));
						_renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(num, num2), flipped: false, 0f, Color.White)
						{
							interval = 30f,
							totalNumberOfLoops = 99999,
							animationLength = 4,
							scale = 4f,
							alphaFade = 0.01f
						});
					}
				}
				else
				{
					_buildAnimationTimer = 500;
					Game1.playSound("axe");
					for (int j = 0; j < 20; j++)
					{
						Microsoft.Xna.Framework.Rectangle random2 = Utility.GetRandom(_renovation.renovationBounds[_animatingIndex]);
						int num3 = (int)Utility.RandomFloat((random2.Left - 1) * 64, 64 * random2.Right);
						int num4 = (int)Utility.RandomFloat((random2.Top - 1) * 64, 64 * random2.Bottom);
						_renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90) - 64, 6, 1, new Vector2(num3, num4), flicker: false, Game1.random.NextDouble() < 0.5));
						_renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90) - 64, 6, 1, new Vector2(num3, num4), flicker: false, Game1.random.NextDouble() < 0.5));
					}
				}
			}
			else
			{
				_buildAnimationTimer = 0;
				SetupForReturn();
			}
		}

		public virtual void CompleteRenovation(int selected_index)
		{
			if (_renovation.validate == null || _renovation.validate(_renovation, selected_index))
			{
				freeze = true;
				if (_renovation.animationType == HouseRenovation.AnimationType.Destroy)
				{
					Game1.playSound("explosion");
					_buildAnimationCount = 10;
				}
				else
				{
					_buildAnimationCount = 3;
				}
				_buildAnimationTimer = -1;
				_animatingIndex = _selectedIndex;
				if (_renovation.onRenovation != null)
				{
					_renovation.onRenovation(_renovation, selected_index);
					Game1.player.renovateEvent.Fire(_renovation.location.NameOrUniqueName);
				}
				AnimateRenovation();
			}
		}

		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return true;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (b == Buttons.B && !Game1.globalFade)
			{
				SetupForReturn();
				Game1.playSound("smallSelect");
			}
		}

		public override bool readyToClose()
		{
			if (freeze)
			{
				return false;
			}
			return base.readyToClose();
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.globalFade || freeze)
			{
				return;
			}
			if (!Game1.globalFade)
			{
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
				{
					SetupForReturn();
				}
				else if (!Game1.options.SnappyMenus && !freeze)
				{
					if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
					{
						Game1.panScreen(0, 4);
					}
					else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
					{
						Game1.panScreen(4, 0);
					}
					else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
					{
						Game1.panScreen(0, -4);
					}
					else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
					{
						Game1.panScreen(-4, 0);
					}
				}
			}
			else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.globalFade)
			{
				if (readyToClose())
				{
					Game1.player.forceCanMove();
					Game1.exitActiveMenu();
					Game1.playSound("bigDeSelect");
				}
			}
			else if (Game1.options.SnappyMenus)
			{
				base.receiveKeyPress(key);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			AnimateRenovation();
			if (!PinchZoom.Instance.CheckForPinchZoom())
			{
				TestToPan(Game1.input.GetMouseState().X, Game1.input.GetMouseState().Y);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			hovered = null;
			if (Game1.globalFade || freeze)
			{
				return;
			}
			if (okButton != null)
			{
				if (okButton.containsPoint(x, y))
				{
					okButton.scale = Math.Min(1.1f, okButton.scale + 0.05f);
				}
				else
				{
					okButton.scale = Math.Max(1f, okButton.scale - 0.05f);
				}
			}
			Vector2 vector = new Vector2((Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64f, (Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64f);
			_selectedIndex = -1;
			for (int i = 0; i < _renovation.renovationBounds.Count; i++)
			{
				List<Microsoft.Xna.Framework.Rectangle> list = _renovation.renovationBounds[i];
				foreach (Microsoft.Xna.Framework.Rectangle item in list)
				{
					if (item.Contains((int)vector.X, (int)vector.Y))
					{
						_selectedIndex = i;
						break;
					}
				}
			}
		}

		public static string getAnimalTitle(string name)
		{
			if (name != null)
			{
				switch (name.Length)
				{
				case 4:
					switch (name[0])
					{
					case 'D':
						if (!(name == "Duck"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5937");
					case 'G':
						if (!(name == "Goat"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5933");
					}
					break;
				case 7:
					if (!(name == "Chicken"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5922");
				case 6:
					if (!(name == "Rabbit"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5945");
				case 9:
					if (!(name == "Dairy Cow"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5927");
				case 3:
					if (!(name == "Pig"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5948");
				case 5:
					if (!(name == "Sheep"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5942");
				}
			}
			return "";
		}

		public static string getAnimalDescription(string name)
		{
			if (name != null)
			{
				switch (name.Length)
				{
				case 4:
					switch (name[0])
					{
					case 'D':
						if (!(name == "Duck"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11337") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
					case 'G':
						if (!(name == "Goat"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11349") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
					}
					break;
				case 7:
					if (!(name == "Chicken"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11334") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
				case 6:
					if (!(name == "Rabbit"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11340") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
				case 9:
					if (!(name == "Dairy Cow"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11343") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
				case 3:
					if (!(name == "Pig"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11346") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
				case 5:
					if (!(name == "Sheep"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11352") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
				}
			}
			return "";
		}

		public override void draw(SpriteBatch b)
		{
			if (!Game1.globalFade && !freeze)
			{
				Game1.StartWorldDrawInUI(b);
				for (int i = 0; i < _renovation.renovationBounds.Count; i++)
				{
					List<Microsoft.Xna.Framework.Rectangle> list = _renovation.renovationBounds[i];
					foreach (Microsoft.Xna.Framework.Rectangle item in list)
					{
						for (int j = item.Left; j < item.Right; j++)
						{
							for (int k = item.Top; k < item.Bottom; k++)
							{
								int num = 0;
								if (i == _selectedIndex)
								{
									num = 1;
								}
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(j, k) * 64f), new Microsoft.Xna.Framework.Rectangle(194 + num * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
							}
						}
					}
				}
				Game1.EndWorldDrawInUI(b);
			}
			if (!Game1.globalFade && !freeze)
			{
				string placementText = _renovation.placementText;
				SpriteText.drawStringWithScrollBackground(b, placementText, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(placementText) / 2, 16);
			}
			if (!Game1.globalFade && !freeze && okButton != null)
			{
				okButton.draw(b);
			}
			Game1.mouseCursorTransparency = 1f;
			drawMouse(b);
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			_lastTapX = -1;
			_lastTapY = -1;
		}

		private void TestToPan(int x, int y)
		{
			if ((upperRightCloseButton == null || !upperRightCloseButton.containsPoint(x, y)) && Game1.input.GetMouseState().LeftButton == ButtonState.Pressed)
			{
				if (_lastTapX != -1 && _lastTapY != -1)
				{
					int num = (int)((float)(_lastTapX - x) / Game1.options.zoomLevel);
					int num2 = (int)((float)(_lastTapY - y) / Game1.options.zoomLevel);
					Console.WriteLine($"{num} {num2}");
					Game1.panScreen(num, num2);
				}
				_lastTapX = x;
				_lastTapY = y;
			}
		}
	}
}
