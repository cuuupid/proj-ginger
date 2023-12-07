using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class GeodeMenu : MenuWithInventory
	{
		public const int region_geodeSpot = 998;

		public ClickableComponent geodeSpot;

		public AnimatedSprite clint;

		public TemporaryAnimatedSprite geodeDestructionAnimation;

		public TemporaryAnimatedSprite sparkle;

		public int geodeAnimationTimer;

		public int yPositionOfGem;

		public int alertTimer;

		public float delayBeforeShowArtifactTimer;

		public Item geodeTreasure;

		public Item geodeTreasureOverride;

		public bool waitingForServerResponse;

		private List<TemporaryAnimatedSprite> fluffSprites = new List<TemporaryAnimatedSprite>();

		private int _selectedItemIndex = -1;

		private bool _showTooltip;

		private string fullText = Game1.content.LoadString("Strings\\UI:GeodeMenu_InventoryFull");

		private string noMoneyText = Game1.content.LoadString("Strings\\UI:GeodeMenu_Description_NotEnoughMoney");

		private string geodeText = Game1.content.LoadString("Strings\\UI:GeodeMenu_Description");

		private Rectangle infoBox;

		private Rectangle bottomInv;

		private float widthMod;

		private float heightMod;

		private new int width;

		private new int height;

		private int goldX;

		private int goldY;

		private int geodeX;

		private int geodeY;

		private int geodeHeight;

		private int geodeWidth;

		private int geodeCrop;

		public GeodeMenu()
			: base(null, okButton: true, trashCan: true, Game1.xEdge)
		{
			inventory.showOrganizeButton = false;
			xPositionOnScreen = Game1.xEdge;
			yPositionOnScreen = 0;
			width = Game1.uiViewport.Width - Game1.xEdge * 2;
			height = Game1.uiViewport.Height;
			widthMod = (float)width / 1280f;
			heightMod = (float)height / 720f;
			geodeX = xPositionOnScreen;
			geodeY = 0;
			geodeWidth = 592;
			if (height < 720)
			{
				geodeHeight = 32 + (int)(308f * (float)height / 720f);
				geodeCrop = (int)(308f - ((float)geodeHeight - 32f)) / 2;
			}
			else
			{
				geodeHeight = 340;
				geodeCrop = 0;
			}
			upperRightCloseButton.bounds.X = Game1.uiViewport.Width - 68 - Game1.xEdge;
			int num = Math.Min(height - geodeHeight, height / 2);
			if (num < height - geodeHeight)
			{
				geodeY = (Game1.uiViewport.Height - geodeHeight - num) / 2;
			}
			infoBox = new Rectangle(geodeWidth + geodeX, geodeY, width - geodeWidth, geodeHeight);
			bottomInv = new Rectangle(xPositionOnScreen, geodeY + geodeHeight, width, num);
			inventory.movePosition(0, (geodeY + geodeHeight - inventory.getInvY()) / 2);
			geodeSpot = new ClickableComponent(new Rectangle(geodeX + 16, geodeY + 16, geodeWidth - 32, geodeHeight - 32), "")
			{
				myID = 998,
				downNeighborID = 0
			};
			if (width < 1000)
			{
				goldX = geodeX + (geodeWidth - 288) / 2;
			}
			else
			{
				goldX = infoBox.X + (infoBox.Width - 288) / 2;
			}
			goldY = 12 + geodeY;
			Log.It("GeodeMenu setting highlightMethod to highlightGeodes");
			inventory.highlightMethod = highlightGeodes;
			clint = new AnimatedSprite("Characters\\Clint", 8, 32, 48);
			if (inventory.inventory != null && inventory.inventory.Count >= 12)
			{
				for (int i = 0; i < 12; i++)
				{
					if (inventory.inventory[i] != null)
					{
						inventory.inventory[i].upNeighborID = 998;
					}
				}
			}
			if (trashCan != null)
			{
				trashCan.myID = 106;
			}
			if (okButton != null)
			{
				okButton.leftNeighborID = 11;
			}
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public override bool readyToClose()
		{
			if (base.readyToClose() && geodeAnimationTimer <= 0 && heldItem == null)
			{
				return !waitingForServerResponse;
			}
			return false;
		}

		public bool highlightGeodes(Item i)
		{
			if (i == null)
			{
				return false;
			}
			if (Utility.IsGeode(i))
			{
				return true;
			}
			return false;
		}

		public virtual void startGeodeCrack()
		{
			geodeSpot.item = heldItem.getOne();
			heldItem.Stack--;
			if (heldItem.Stack <= 0)
			{
				heldItem = null;
				Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
				inventory.currentlySelectedItem = -1;
			}
			geodeAnimationTimer = 2700;
			Game1.player.Money -= 25;
			Game1.playSound("stoneStep");
			clint.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(8, 300),
				new FarmerSprite.AnimationFrame(9, 200),
				new FarmerSprite.AnimationFrame(10, 80),
				new FarmerSprite.AnimationFrame(11, 200),
				new FarmerSprite.AnimationFrame(12, 100),
				new FarmerSprite.AnimationFrame(8, 300)
			});
			clint.loop = false;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (waitingForServerResponse)
			{
				return;
			}
			if (geodeSpot.containsPoint(x, y))
			{
				if (heldItem == null && inventory.currentlySelectedItem != -1 && Utility.IsGeode(inventory.selectedItem))
				{
					heldItem = inventory.selectedItem;
				}
				OnPlaceGeodeOnAnvil();
			}
			else
			{
				base.receiveLeftClick(x, y, playSound);
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (inventory.dragItem != -1 && geodeSpot.containsPoint(x, y))
			{
				receiveLeftClick(x, y);
				inventory.currentlySelectedItem = -1;
				inventory.dragItem = -1;
			}
			base.releaseLeftClick(x, y);
			heldItem = null;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			base.receiveRightClick(x, y, playSound: true);
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			if (heldItem != null)
			{
				Game1.player.addItemToInventoryBool(heldItem);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			for (int num = fluffSprites.Count - 1; num >= 0; num--)
			{
				if (fluffSprites[num].update(time))
				{
					fluffSprites.RemoveAt(num);
				}
			}
			if (heldItem != null)
			{
				if (Game1.player.freeSpotsInInventory() < 1 && (Game1.player.freeSpotsInInventory() != 0 || heldItem.Stack != 1))
				{
					descriptionText = fullText;
				}
				else if (Game1.player.Money < 25)
				{
					descriptionText = noMoneyText;
				}
				else
				{
					descriptionText = geodeText;
				}
			}
			else
			{
				descriptionText = geodeText;
			}
			if (alertTimer > 0)
			{
				alertTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (geodeAnimationTimer <= 0)
			{
				return;
			}
			Game1.changeMusicTrack("none");
			geodeAnimationTimer -= time.ElapsedGameTime.Milliseconds;
			if (geodeAnimationTimer <= 0)
			{
				geodeDestructionAnimation = null;
				geodeSpot.item = null;
				if (geodeTreasure != null && Utility.IsNormalObjectAtParentSheetIndex(geodeTreasure, 73))
				{
					Game1.netWorldState.Value.GoldenCoconutCracked.Value = true;
				}
				Game1.player.addItemToInventoryBool(geodeTreasure);
				geodeTreasure = null;
				yPositionOfGem = 0;
				fluffSprites.Clear();
				delayBeforeShowArtifactTimer = 0f;
				return;
			}
			int currentFrame = clint.currentFrame;
			clint.animateOnce(time);
			if (clint.currentFrame == 11 && currentFrame != 11)
			{
				if (geodeSpot.item != null && (int)geodeSpot.item.parentSheetIndex == 275)
				{
					Game1.playSound("hammer");
					Game1.playSound("woodWhack");
				}
				else
				{
					Game1.playSound("hammer");
					Game1.playSound("stoneCrack");
				}
				Game1.stats.GeodesCracked++;
				int num2 = 448;
				if (geodeSpot.item != null)
				{
					switch ((int)(geodeSpot.item as Object).parentSheetIndex)
					{
					case 536:
						num2 += 64;
						break;
					case 537:
						num2 += 128;
						break;
					}
					geodeDestructionAnimation = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, num2, 64, 64), 100f, 8, 0, new Vector2(geodeSpot.bounds.X + 392 - 32, geodeSpot.bounds.Y + 192 - 32), flicker: false, flipped: false);
					if (geodeSpot.item != null && (int)geodeSpot.item.parentSheetIndex == 275)
					{
						geodeDestructionAnimation = new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites//temporary_sprites_1"),
							sourceRect = new Rectangle(388, 123, 18, 21),
							sourceRectStartingPos = new Vector2(388f, 123f),
							animationLength = 6,
							position = new Vector2(geodeSpot.bounds.X + 380 - 32, geodeSpot.bounds.Y + 192 - 32),
							holdLastFrame = true,
							interval = 100f,
							id = 777f,
							scale = 4f
						};
						for (int i = 0; i < 6; i++)
						{
							fluffSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(geodeSpot.bounds.X + 392 - 32 + Game1.random.Next(21), geodeSpot.bounds.Y + 192 - 16), flipped: false, 0.002f, new Color(255, 222, 198))
							{
								alphaFade = 0.02f,
								motion = new Vector2((float)Game1.random.Next(-20, 21) / 10f, (float)Game1.random.Next(5, 20) / 10f),
								interval = 99999f,
								layerDepth = 0.9f,
								scale = 3f,
								scaleChange = 0.01f,
								rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
								delayBeforeAnimationStart = i * 20
							});
							fluffSprites.Add(new TemporaryAnimatedSprite
							{
								texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites//temporary_sprites_1"),
								sourceRect = new Rectangle(499, 132, 5, 5),
								sourceRectStartingPos = new Vector2(499f, 132f),
								motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-7, -4)),
								acceleration = new Vector2(0f, 0.25f),
								totalNumberOfLoops = 1,
								interval = 1000f,
								alphaFade = 0.015f,
								animationLength = 1,
								layerDepth = 1f,
								scale = 4f,
								rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
								delayBeforeAnimationStart = i * 10,
								position = new Vector2(geodeSpot.bounds.X + 392 - 32 + Game1.random.Next(21), geodeSpot.bounds.Y + 192 - 16)
							});
							delayBeforeShowArtifactTimer = 500f;
						}
					}
					if (geodeTreasureOverride != null)
					{
						geodeTreasure = geodeTreasureOverride;
						geodeTreasureOverride = null;
					}
					else
					{
						geodeTreasure = Utility.getTreasureFromGeode(geodeSpot.item);
					}
					if ((int)geodeSpot.item.parentSheetIndex != 275 && (!(geodeTreasure is Object) || !(geodeTreasure as Object).Type.Contains("Mineral")) && geodeTreasure is Object && (geodeTreasure as Object).Type.Contains("Arch") && !Game1.player.hasOrWillReceiveMail("artifactFound"))
					{
						geodeTreasure = new Object(390, 5);
					}
				}
			}
			if (geodeDestructionAnimation != null && ((geodeDestructionAnimation.id != 777f && geodeDestructionAnimation.currentParentTileIndex < 7) || (geodeDestructionAnimation.id == 777f && geodeDestructionAnimation.currentParentTileIndex < 5)))
			{
				geodeDestructionAnimation.update(time);
				if (delayBeforeShowArtifactTimer > 0f)
				{
					delayBeforeShowArtifactTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
					if (delayBeforeShowArtifactTimer <= 0f)
					{
						fluffSprites.Add(geodeDestructionAnimation);
						fluffSprites.Reverse();
						geodeDestructionAnimation = new TemporaryAnimatedSprite
						{
							interval = 100f,
							animationLength = 6,
							alpha = 0.001f,
							id = 777f
						};
					}
				}
				else
				{
					if (geodeDestructionAnimation.currentParentTileIndex < 3)
					{
						yPositionOfGem--;
					}
					yPositionOfGem--;
					if ((geodeDestructionAnimation.currentParentTileIndex == 7 || (geodeDestructionAnimation.id == 777f && geodeDestructionAnimation.currentParentTileIndex == 5)) && (!(geodeTreasure is Object) || (int)(geodeTreasure as Object).price > 75))
					{
						sparkle = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 640, 64, 64), 100f, 8, 0, new Vector2(geodeSpot.bounds.X + 392 - 32, geodeSpot.bounds.Y + 192 + yPositionOfGem - 32), flicker: false, flipped: false);
						Game1.playSound("discoverMineral");
					}
					else if ((geodeDestructionAnimation.currentParentTileIndex == 7 || (geodeDestructionAnimation.id == 777f && geodeDestructionAnimation.currentParentTileIndex == 5)) && geodeTreasure is Object && (int)(geodeTreasure as Object).price <= 75)
					{
						Game1.playSound("newArtifact");
					}
				}
			}
			if (sparkle != null && sparkle.update(time))
			{
				sparkle = null;
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			geodeSpot = new ClickableComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8, 560, 308), "Anvil");
			int yPosition = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + 128 + 4;
			inventory = new InventoryMenu(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 12, yPosition, playerInventory: false, null, inventory.highlightMethod);
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			IClickableMenu.drawTextureBox(b, infoBox.X, infoBox.Y, infoBox.Width, infoBox.Height, Color.White);
			IClickableMenu.drawTextureBox(b, geodeX, geodeY, geodeWidth, geodeHeight, Color.White);
			IClickableMenu.drawTextureBox(b, bottomInv.X, bottomInv.Y, bottomInv.Width, bottomInv.Height, Color.White);
			base.draw(b);
			Game1.dayTimeMoneyBox.drawMoneyBox(b, goldX, goldY, oldGFX: true);
			b.Draw(Game1.mouseCursors, new Vector2(geodeSpot.bounds.X, geodeSpot.bounds.Y), new Rectangle(0, 512 + geodeCrop / 4, 140, 77 - geodeCrop * 2 / 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.087f);
			if (geodeSpot.item != null)
			{
				if (geodeDestructionAnimation == null)
				{
					geodeSpot.item.drawInMenu(b, new Vector2(geodeSpot.bounds.X + 360 + (((int)geodeSpot.item.parentSheetIndex == 275) ? (-8) : 0), geodeSpot.bounds.Y + 160 + (((int)geodeSpot.item.parentSheetIndex == 275) ? 8 : 0) - geodeCrop), 1f);
				}
				else
				{
					geodeDestructionAnimation.draw(b, localPosition: true);
				}
				foreach (TemporaryAnimatedSprite fluffSprite in fluffSprites)
				{
					fluffSprite.draw(b, localPosition: true);
				}
				if (geodeTreasure != null && delayBeforeShowArtifactTimer <= 0f)
				{
					geodeTreasure.drawInMenu(b, new Vector2(geodeSpot.bounds.X + 360, geodeSpot.bounds.Y + 160 + yPositionOfGem - geodeCrop), 1f);
				}
				if (sparkle != null)
				{
					sparkle.draw(b, localPosition: true);
				}
			}
			clint.draw(b, new Vector2(geodeSpot.bounds.X + 384, geodeSpot.bounds.Y + 64 - geodeCrop), 0.0877f);
			if (alertTimer <= 0)
			{
				if (descriptionText.Equals(""))
				{
					if (Game1.player.Money < 25)
					{
						descriptionText = Game1.content.LoadString("Strings\\UI:GeodeMenu_Description_NotEnoughMoney");
					}
					else
					{
						descriptionText = Game1.content.LoadString("Strings\\UI:GeodeMenu_Description");
					}
				}
				Utility.drawMultiLineTextWithShadow(b, descriptionText, Game1.smallFont, new Vector2(infoBox.X + 32, infoBox.Y + 32), infoBox.Width - 64, infoBox.Height - 64, Game1.textColor);
			}
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
			inventory.drawDragItem(b);
			inventory.drawInfoPanel(b, force: true);
			drawMouse(b);
		}

		public override void receiveGamePadButton(Buttons b)
		{
			switch (b)
			{
			case Buttons.DPadUp:
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
			case Buttons.LeftThumbstickUp:
			{
				bool flag2 = false;
				for (int num = Math.Max(0, _selectedItemIndex - 1); num >= 0; num--)
				{
					Item itemAt2 = inventory.GetItemAt(num);
					if (itemAt2 != null && inventory.highlightMethod(itemAt2))
					{
						_selectedItemIndex = num;
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					break;
				}
				for (int num2 = inventory.inventory.Count - 1; num2 > _selectedItemIndex; num2--)
				{
					Item itemAt2 = inventory.GetItemAt(num2);
					if (itemAt2 != null && inventory.highlightMethod(itemAt2))
					{
						_selectedItemIndex = num2;
						break;
					}
				}
				break;
			}
			case Buttons.DPadDown:
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickDown:
			case Buttons.LeftThumbstickRight:
			{
				bool flag = false;
				for (int i = Math.Max(0, _selectedItemIndex + 1); i < inventory.inventory.Count; i++)
				{
					Item itemAt = inventory.GetItemAt(i);
					if (itemAt != null && inventory.highlightMethod(itemAt))
					{
						_selectedItemIndex = i;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
				for (int j = 0; j < _selectedItemIndex; j++)
				{
					Item itemAt = inventory.GetItemAt(j);
					if (itemAt != null && inventory.highlightMethod(itemAt))
					{
						_selectedItemIndex = j;
						break;
					}
				}
				break;
			}
			case Buttons.A:
				_showTooltip = !_showTooltip;
				if (!_showTooltip)
				{
					inventory.GamePadHideInfoPanel();
				}
				break;
			case Buttons.X:
				if (_selectedItemIndex > -1)
				{
					heldItem = inventory.actualInventory[_selectedItemIndex];
					OnPlaceGeodeOnAnvil();
				}
				break;
			case Buttons.B:
				heldItem = null;
				Game1.playSound("smallSelect");
				OnTapCloseButton();
				Game1.player.forceCanMove();
				break;
			}
			if (_selectedItemIndex > -1)
			{
				inventory.currentlySelectedItem = _selectedItemIndex;
				if (_showTooltip)
				{
					inventory.GamePadShowInfoPanel();
				}
			}
		}

		private void OnPlaceGeodeOnAnvil()
		{
			if (heldItem != null && Utility.IsGeode(heldItem) && Game1.player.Money >= 25 && geodeAnimationTimer <= 0)
			{
				if (Game1.player.freeSpotsInInventory() >= 1 || (Game1.player.freeSpotsInInventory() == 0 && heldItem.Stack == 1))
				{
					if (heldItem.ParentSheetIndex == 791 && !Game1.netWorldState.Value.GoldenCoconutCracked.Value)
					{
						geodeTreasureOverride = new Object(73, 1);
						startGeodeCrack();
					}
					else
					{
						startGeodeCrack();
					}
				}
				else
				{
					descriptionText = Game1.content.LoadString("Strings\\UI:GeodeMenu_InventoryFull");
					wiggleWordsTimer = 500;
					alertTimer = 1500;
				}
			}
			else if (Game1.player.Money < 25)
			{
				wiggleWordsTimer = 500;
				Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
			}
		}
	}
}
