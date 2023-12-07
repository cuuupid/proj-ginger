using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.GameData.Crafting;
using StardewValley.Objects;

namespace StardewValley.Menus
{
	public class TailorRecipeListTool : IClickableMenu
	{
		public Rectangle scrollView;

		public List<ClickableTextureComponent> recipeComponents;

		public ClickableTextureComponent okButton;

		public float scrollY;

		public Dictionary<string, KeyValuePair<Item, Item>> _recipeLookup;

		public Item hoveredItem;

		public string metadata = "";

		public Dictionary<string, string> _recipeMetadata;

		public Dictionary<string, Color> _recipeColors;

		public TailorRecipeListTool()
			: base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 64)
		{
			TailoringMenu tailoringMenu = new TailoringMenu();
			Game1.player.faceDirection(2);
			Game1.player.FarmerSprite.StopAnimation();
			recipeComponents = new List<ClickableTextureComponent>();
			_recipeLookup = new Dictionary<string, KeyValuePair<Item, Item>>();
			_recipeMetadata = new Dictionary<string, string>();
			_recipeColors = new Dictionary<string, Color>();
			Item left_item = new Object(Vector2.Zero, 428, 1);
			foreach (int key in Game1.objectInformation.Keys)
			{
				Item item = new Object(Vector2.Zero, key, 1);
				if (item.Name.Contains("Seeds") || item.Name.Contains("Floor") || item.Name.Equals("Stone") || item.Name.Contains("Weeds") || item.Name.Equals("Lumber") || item.Name.Contains("Fence") || item.Name.Equals("Gate") || item.Name.Contains("Starter") || item.Name.Contains("Twig") || item.Name.Equals("Secret Note") || item.Name.Contains("Guide") || item.Name.Contains("Path") || item.Name.Contains("Ring") || (int)item.category == -22 || item.Name.Contains("Sapling"))
				{
					continue;
				}
				Item value = tailoringMenu.CraftItem(left_item, item);
				TailorItemRecipe recipeForItems = tailoringMenu.GetRecipeForItems(left_item, item);
				KeyValuePair<Item, Item> value2 = new KeyValuePair<Item, Item>(item, value);
				_recipeLookup[Utility.getStandardDescriptionFromItem(item, 1)] = value2;
				string text = "";
				Color? dyeColor = TailoringMenu.GetDyeColor(item);
				if (dyeColor.HasValue)
				{
					_recipeColors[Utility.getStandardDescriptionFromItem(item, 1)] = dyeColor.Value;
				}
				if (recipeForItems != null)
				{
					text = "clothes id: " + recipeForItems.CraftedItemID + " from ";
					foreach (string secondItemTag in recipeForItems.SecondItemTags)
					{
						text = text + secondItemTag + " ";
					}
					text.Trim();
				}
				_recipeMetadata[Utility.getStandardDescriptionFromItem(item, 1)] = text;
				ClickableTextureComponent item2 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), null, default(Rectangle), 1f)
				{
					myID = 0,
					name = Utility.getStandardDescriptionFromItem(item, 1),
					label = item.DisplayName
				};
				recipeComponents.Add(item2);
			}
			okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, yPositionOnScreen + height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			RepositionElements();
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
			RepositionElements();
		}

		private void RepositionElements()
		{
			scrollView = new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder, width - IClickableMenu.borderWidth, 500);
			if (scrollView.Left < Game1.graphics.GraphicsDevice.ScissorRectangle.Left)
			{
				int num = Game1.graphics.GraphicsDevice.ScissorRectangle.Left - scrollView.Left;
				scrollView.X += num;
				scrollView.Width -= num;
			}
			if (scrollView.Right > Game1.graphics.GraphicsDevice.ScissorRectangle.Right)
			{
				int num2 = scrollView.Right - Game1.graphics.GraphicsDevice.ScissorRectangle.Right;
				scrollView.X -= num2;
				scrollView.Width -= num2;
			}
			if (scrollView.Top < Game1.graphics.GraphicsDevice.ScissorRectangle.Top)
			{
				int num3 = Game1.graphics.GraphicsDevice.ScissorRectangle.Top - scrollView.Top;
				scrollView.Y += num3;
				scrollView.Width -= num3;
			}
			if (scrollView.Bottom > Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom)
			{
				int num4 = scrollView.Bottom - Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom;
				scrollView.Y -= num4;
				scrollView.Width -= num4;
			}
			RepositionScrollElements();
		}

		public void RepositionScrollElements()
		{
			int num = (int)scrollY;
			if (scrollY > 0f)
			{
				scrollY = 0f;
			}
			foreach (ClickableTextureComponent recipeComponent in recipeComponents)
			{
				recipeComponent.bounds.X = scrollView.X;
				recipeComponent.bounds.Y = scrollView.Y + num;
				num += recipeComponent.bounds.Height;
				if (scrollView.Intersects(recipeComponent.bounds))
				{
					recipeComponent.visible = true;
				}
				else
				{
					recipeComponent.visible = false;
				}
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			snapCursorToCurrentSnappedComponent();
		}

		public override void gamePadButtonHeld(Buttons b)
		{
			base.gamePadButtonHeld(b);
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			_ = currentlySnappedComponent;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableTextureComponent recipeComponent in recipeComponents)
			{
				if (!recipeComponent.bounds.Contains(x, y) || !scrollView.Contains(x, y))
				{
					continue;
				}
				try
				{
					int num = Convert.ToInt32(_recipeMetadata[recipeComponent.name].Split(' ')[2]);
					if (num >= 2000)
					{
						Game1.player.addItemToInventoryBool(new Hat(num - 2000));
						continue;
					}
					Clothing clothing = new Clothing(num);
					if (_recipeColors.ContainsKey(recipeComponent.name))
					{
						clothing.Dye(_recipeColors[recipeComponent.name], 1f);
					}
					Game1.player.addItemToInventoryBool(clothing);
				}
				catch (Exception)
				{
				}
			}
			if (okButton.containsPoint(x, y))
			{
				exitThisMenu();
			}
		}

		public override void leftClickHeld(int x, int y)
		{
		}

		public override void releaseLeftClick(int x, int y)
		{
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void receiveKeyPress(Keys key)
		{
		}

		public override void receiveScrollWheelAction(int direction)
		{
			scrollY += direction;
			RepositionScrollElements();
			base.receiveScrollWheelAction(direction);
		}

		public override void performHoverAction(int x, int y)
		{
			hoveredItem = null;
			metadata = "";
			foreach (ClickableTextureComponent recipeComponent in recipeComponents)
			{
				if (recipeComponent.containsPoint(x, y))
				{
					hoveredItem = _recipeLookup[recipeComponent.name].Value;
					metadata = _recipeMetadata[recipeComponent.name];
				}
			}
		}

		public bool canLeaveMenu()
		{
			return true;
		}

		public override void draw(SpriteBatch b)
		{
			bool ignoreTitleSafe = false;
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe);
			b.End();
			Rectangle scissorRectangle = b.GraphicsDevice.ScissorRectangle;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
			b.GraphicsDevice.ScissorRectangle = scrollView;
			foreach (ClickableTextureComponent recipeComponent in recipeComponents)
			{
				if (recipeComponent.visible)
				{
					drawHorizontalPartition(b, recipeComponent.bounds.Bottom - 32, small: true);
					KeyValuePair<Item, Item> keyValuePair = _recipeLookup[recipeComponent.name];
					recipeComponent.draw(b);
					keyValuePair.Key.drawInMenu(b, new Vector2(recipeComponent.bounds.X, recipeComponent.bounds.Y), 1f);
					if (_recipeColors.ContainsKey(recipeComponent.name))
					{
						int num = 24;
						b.Draw(Game1.staminaRect, new Rectangle(scrollView.Left + scrollView.Width / 2 - num / 2, recipeComponent.bounds.Center.Y - num / 2, num, num), _recipeColors[recipeComponent.name]);
					}
					if (keyValuePair.Value != null)
					{
						keyValuePair.Value.drawInMenu(b, new Vector2(scrollView.Left + scrollView.Width - 128, recipeComponent.bounds.Y), 1f);
					}
				}
			}
			b.End();
			b.GraphicsDevice.ScissorRectangle = scissorRectangle;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			okButton.draw(b);
			drawMouse(b);
			if (hoveredItem != null)
			{
				Utility.drawTextWithShadow(b, metadata, Game1.smallFont, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth, yPositionOnScreen + height - 64), Color.Black);
				if (!Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem);
				}
			}
		}

		public override void update(GameTime time)
		{
		}
	}
}
