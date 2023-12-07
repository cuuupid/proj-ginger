using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Minigames
{
	public class Slots : IMinigame
	{
		public const float slotTurnRate = 0.008f;

		public const int numberOfIcons = 8;

		public const int defaultBet = 10;

		private string coinBuffer;

		private List<float> slots;

		private List<float> slotResults;

		private ClickableComponent spinButton10;

		private ClickableComponent spinButton100;

		private ClickableComponent doneButton;

		private bool spinning;

		private bool showResult;

		private float payoutModifier;

		private int currentBet;

		private int spinsCount;

		private int slotsFinished;

		private int endTimer;

		public ClickableComponent currentlySnappedComponent;

		public Slots(int toBet = -1, bool highStakes = false)
		{
			coinBuffer = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? "     " : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? "\u3000\u3000" : "  "));
			currentBet = toBet;
			if (currentBet == -1)
			{
				currentBet = 10;
			}
			slots = new List<float>();
			slots.Add(0f);
			slots.Add(0f);
			slots.Add(0f);
			slotResults = new List<float>();
			slotResults.Add(0f);
			slotResults.Add(0f);
			slotResults.Add(0f);
			Game1.playSound("newArtifact");
			setSlotResults(slots);
			RepositionButtons();
			if (Game1.isAnyGamePadButtonBeingPressed())
			{
				Game1.setMousePosition(spinButton10.bounds.Center);
				if (Game1.options.SnappyMenus)
				{
					currentlySnappedComponent = spinButton10;
				}
			}
		}

		public virtual void RepositionButtons()
		{
			Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 104, 52, -16, 32);
			spinButton10 = new ClickableComponent(new Rectangle((int)topLeftPositionForCenteringOnScreen.X, (int)topLeftPositionForCenteringOnScreen.Y, 104, 52), Game1.content.LoadString("Strings\\StringsFromCSFiles:Slots.cs.12117"));
			topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 124, 52, -16, 96);
			spinButton100 = new ClickableComponent(new Rectangle((int)topLeftPositionForCenteringOnScreen.X, (int)topLeftPositionForCenteringOnScreen.Y, 124, 52), Game1.content.LoadString("Strings\\StringsFromCSFiles:Slots.cs.12118"));
			topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 96, 52, -16, 160);
			doneButton = new ClickableComponent(new Rectangle((int)topLeftPositionForCenteringOnScreen.X, (int)topLeftPositionForCenteringOnScreen.Y, 96, 52), Game1.content.LoadString("Strings\\StringsFromCSFiles:NameSelect.cs.3864"));
		}

		public void setSlotResults(List<float> toSet)
		{
			double num = Game1.random.NextDouble();
			double num2 = 1.0 + Game1.player.DailyLuck * 2.0 + (double)Game1.player.LuckLevel * 0.08;
			if (num < 0.001 * num2)
			{
				set(toSet, 5);
				payoutModifier = 2500f;
				return;
			}
			if (num < 0.0016 * num2)
			{
				set(toSet, 6);
				payoutModifier = 1000f;
				return;
			}
			if (num < 0.0025 * num2)
			{
				set(toSet, 7);
				payoutModifier = 500f;
				return;
			}
			if (num < 0.005 * num2)
			{
				set(toSet, 4);
				payoutModifier = 200f;
				return;
			}
			if (num < 0.007 * num2)
			{
				set(toSet, 3);
				payoutModifier = 120f;
				return;
			}
			if (num < 0.01 * num2)
			{
				set(toSet, 2);
				payoutModifier = 80f;
				return;
			}
			if (num < 0.02 * num2)
			{
				set(toSet, 1);
				payoutModifier = 30f;
				return;
			}
			if (num < 0.12 * num2)
			{
				int num3 = Game1.random.Next(3);
				for (int i = 0; i < 3; i++)
				{
					toSet[i] = ((i == num3) ? Game1.random.Next(7) : 7);
				}
				payoutModifier = 3f;
				return;
			}
			if (num < 0.2 * num2)
			{
				set(toSet, 0);
				payoutModifier = 5f;
				return;
			}
			if (num < 0.4 * num2)
			{
				int num4 = Game1.random.Next(3);
				for (int j = 0; j < 3; j++)
				{
					toSet[j] = ((j == num4) ? 7 : Game1.random.Next(7));
				}
				payoutModifier = 2f;
				return;
			}
			payoutModifier = 0f;
			int[] array = new int[8];
			for (int k = 0; k < 3; k++)
			{
				int num5 = Game1.random.Next(6);
				while (array[num5] > 1)
				{
					num5 = Game1.random.Next(6);
				}
				toSet[k] = num5;
				array[num5]++;
			}
		}

		private void set(List<float> toSet, int number)
		{
			toSet[0] = number;
			toSet[1] = number;
			toSet[2] = number;
		}

		public bool tick(GameTime time)
		{
			if (spinning && endTimer <= 0)
			{
				for (int i = slotsFinished; i < slots.Count; i++)
				{
					float num = slots[i];
					slots[i] += (float)time.ElapsedGameTime.Milliseconds * 0.008f * (1f - (float)i * 0.05f);
					slots[i] %= 8f;
					if (i == 2)
					{
						if (num % (0.25f + (float)slotsFinished * 0.5f) > slots[i] % (0.25f + (float)slotsFinished * 0.5f))
						{
							Game1.playSound("shiny4");
						}
						if (num > slots[i])
						{
							spinsCount++;
						}
					}
					if (spinsCount > 0 && i == slotsFinished && Math.Abs(slots[i] - slotResults[i]) <= (float)time.ElapsedGameTime.Milliseconds * 0.008f)
					{
						slots[i] = slotResults[i];
						slotsFinished++;
						spinsCount--;
						Game1.playSound("Cowboy_gunshot");
					}
				}
				if (slotsFinished >= 3)
				{
					endTimer = ((payoutModifier == 0f) ? 600 : 1000);
				}
			}
			if (endTimer > 0)
			{
				endTimer -= time.ElapsedGameTime.Milliseconds;
				if (endTimer <= 0)
				{
					spinning = false;
					spinsCount = 0;
					slotsFinished = 0;
					if (payoutModifier > 0f)
					{
						showResult = true;
						Game1.playSound((!(payoutModifier >= 5f)) ? "newArtifact" : ((payoutModifier >= 10f) ? "reward" : "money"));
					}
					else
					{
						Game1.playSound("breathout");
					}
					Game1.player.clubCoins += (int)((float)currentBet * payoutModifier);
					if (payoutModifier == 2500f)
					{
						Game1.multiplayer.globalChatInfoMessage("Jackpot", Game1.player.Name);
					}
				}
			}
			spinButton10.scale = ((!spinning && spinButton10.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY())) ? 1.05f : 1f);
			spinButton100.scale = ((!spinning && spinButton100.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY())) ? 1.05f : 1f);
			doneButton.scale = ((!spinning && doneButton.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY())) ? 1.05f : 1f);
			return false;
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!spinning && Game1.player.clubCoins >= 10 && spinButton10.bounds.Contains(x, y))
			{
				Club.timesPlayedSlots++;
				setSlotResults(slotResults);
				spinning = true;
				Game1.playSound("bigSelect");
				currentBet = 10;
				slotsFinished = 0;
				spinsCount = 0;
				showResult = false;
				Game1.player.clubCoins -= 10;
			}
			if (!spinning && Game1.player.clubCoins >= 100 && spinButton100.bounds.Contains(x, y))
			{
				Club.timesPlayedSlots++;
				setSlotResults(slotResults);
				Game1.playSound("bigSelect");
				spinning = true;
				slotsFinished = 0;
				spinsCount = 0;
				showResult = false;
				currentBet = 100;
				Game1.player.clubCoins -= 100;
			}
			if (!spinning && doneButton.bounds.Contains(x, y))
			{
				Game1.playSound("bigDeSelect");
				Game1.currentMinigame = null;
			}
		}

		public void leftClickHeld(int x, int y)
		{
		}

		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void releaseLeftClick(int x, int y)
		{
		}

		public void releaseRightClick(int x, int y)
		{
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public void receiveKeyPress(Keys k)
		{
			if (!spinning && (k.Equals(Keys.Escape) || Game1.options.doesInputListContain(Game1.options.menuButton, k)))
			{
				unload();
				Game1.playSound("bigDeSelect");
				Game1.currentMinigame = null;
			}
			else
			{
				if (spinning || currentlySnappedComponent == null)
				{
					return;
				}
				if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
				{
					if (currentlySnappedComponent.Equals(spinButton10))
					{
						currentlySnappedComponent = spinButton100;
						Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
					}
					else if (currentlySnappedComponent.Equals(spinButton100))
					{
						currentlySnappedComponent = doneButton;
						Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
					}
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
				{
					if (currentlySnappedComponent.Equals(doneButton))
					{
						currentlySnappedComponent = spinButton100;
						Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
					}
					else if (currentlySnappedComponent.Equals(spinButton100))
					{
						currentlySnappedComponent = spinButton10;
						Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
					}
				}
			}
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public int getIconIndex(int index)
		{
			return index switch
			{
				0 => 24, 
				1 => 186, 
				2 => 138, 
				3 => 392, 
				4 => 254, 
				5 => 434, 
				6 => 72, 
				7 => 638, 
				_ => 24, 
			};
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), new Color(38, 0, 7));
			b.Draw(Game1.mouseCursors, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 228, 52, 0, -256), new Rectangle(441, 424, 57, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			for (int i = 0; i < 3; i++)
			{
				b.Draw(Game1.mouseCursors, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 112 + i * 26 * 4, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 128), new Rectangle(306, 320, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				float num = (slots[i] + 1f) % 8f;
				int iconIndex = getIconIndex(((int)num + 8 - 1) % 8);
				int iconIndex2 = getIconIndex((iconIndex + 1) % 8);
				b.Draw(Game1.objectSpriteSheet, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 112 + i * 26 * 4, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 128) - new Vector2(0f, -64f * (num % 1f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, iconIndex, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				b.Draw(Game1.objectSpriteSheet, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 112 + i * 26 * 4, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 128) - new Vector2(0f, 64f - 64f * (num % 1f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, iconIndex2, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				b.Draw(Game1.mouseCursors, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 132 + i * 26 * 4, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 192), new Rectangle(415, 385, 26, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			}
			if (showResult)
			{
				SpriteText.drawString(b, "+" + payoutModifier * (float)currentBet, Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 372, spinButton10.bounds.Y - 64 + 8, 9999, -1, 9999, 1f, 1f, junimoText: false, -1, "", 4);
			}
			b.Draw(Game1.mouseCursors, new Vector2(spinButton10.bounds.X, spinButton10.bounds.Y), new Rectangle(441, 385, 26, 13), Color.White * ((!spinning && Game1.player.clubCoins >= 10) ? 1f : 0.5f), 0f, Vector2.Zero, 4f * spinButton10.scale, SpriteEffects.None, 0.99f);
			b.Draw(Game1.mouseCursors, new Vector2(spinButton100.bounds.X, spinButton100.bounds.Y), new Rectangle(441, 398, 31, 13), Color.White * ((!spinning && Game1.player.clubCoins >= 100) ? 1f : 0.5f), 0f, Vector2.Zero, 4f * spinButton100.scale, SpriteEffects.None, 0.99f);
			b.Draw(Game1.mouseCursors, new Vector2(doneButton.bounds.X, doneButton.bounds.Y), new Rectangle(441, 411, 24, 13), Color.White * ((!spinning) ? 1f : 0.5f), 0f, Vector2.Zero, 4f * doneButton.scale, SpriteEffects.None, 0.99f);
			SpriteText.drawStringWithScrollBackground(b, coinBuffer + Game1.player.clubCoins, Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 376, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 120);
			Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 376 + 4, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 120 + 4), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Vector2 vector = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 + 200, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 352);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), (int)vector.X, (int)vector.Y, 384, 704, Color.White, 4f);
			b.Draw(Game1.objectSpriteSheet, vector + new Vector2(8f, 8f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIconIndex(7), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			SpriteText.drawString(b, "x2", (int)vector.X + 192 + 16, (int)vector.Y + 24, 9999, -1, 99999, 1f, 0.88f, junimoText: false, -1, "", 4);
			b.Draw(Game1.objectSpriteSheet, vector + new Vector2(8f, 76f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIconIndex(7), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			b.Draw(Game1.objectSpriteSheet, vector + new Vector2(76f, 76f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIconIndex(7), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			SpriteText.drawString(b, "x3", (int)vector.X + 192 + 16, (int)vector.Y + 68 + 24, 9999, -1, 99999, 1f, 0.88f, junimoText: false, -1, "", 4);
			for (int j = 0; j < 8; j++)
			{
				int index = j;
				switch (j)
				{
				case 5:
					index = 7;
					break;
				case 7:
					index = 5;
					break;
				}
				b.Draw(Game1.objectSpriteSheet, vector + new Vector2(8f, 8 + (j + 2) * 68), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIconIndex(index), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				b.Draw(Game1.objectSpriteSheet, vector + new Vector2(76f, 8 + (j + 2) * 68), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIconIndex(index), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				b.Draw(Game1.objectSpriteSheet, vector + new Vector2(144f, 8 + (j + 2) * 68), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIconIndex(index), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				int num2 = 0;
				switch (j)
				{
				case 0:
					num2 = 5;
					break;
				case 1:
					num2 = 30;
					break;
				case 2:
					num2 = 80;
					break;
				case 3:
					num2 = 120;
					break;
				case 4:
					num2 = 200;
					break;
				case 5:
					num2 = 500;
					break;
				case 6:
					num2 = 1000;
					break;
				case 7:
					num2 = 2500;
					break;
				}
				SpriteText.drawString(b, "x" + num2, (int)vector.X + 192 + 16, (int)vector.Y + (j + 2) * 68 + 24, 9999, -1, 99999, 1f, 0.88f, junimoText: false, -1, "", 4);
			}
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), (int)vector.X - 640, (int)vector.Y, 1024, 704, Color.Red, 4f, drawShadow: false);
			for (int k = 1; k < 8; k++)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), (int)vector.X - 640 - 4 * k, (int)vector.Y - 4 * k, 1024 + 8 * k, 704 + 8 * k, Color.Red * (1f - (float)k * 0.15f), 4f, drawShadow: false);
			}
			for (int l = 0; l < 17; l++)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(147, 472, 3, 3), (int)vector.X - 640 + 8, (int)vector.Y + l * 4 * 3 + 12, (int)(608f - (float)(l * 64) * 1.2f + (float)(l * l * 4) * 0.7f), 4, new Color(l * 25, (l > 8) ? (l * 10) : 0, 255 - l * 25), 4f, drawShadow: false);
			}
			if (!Game1.options.hardwareCursor)
			{
				b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
			b.End();
		}

		public void changeScreenSize()
		{
			RepositionButtons();
		}

		public void unload()
		{
		}

		public void receiveEventPoke(int data)
		{
		}

		public string minigameId()
		{
			return "Slots";
		}

		public bool doMainGameUpdates()
		{
			return false;
		}

		public bool forceQuit()
		{
			if (spinning)
			{
				Game1.player.clubCoins += currentBet;
			}
			unload();
			return true;
		}

		public float GetForcedScaleFactor()
		{
			float num = 1.7777778f;
			float num2 = (float)Game1.uiViewport.Width / (float)Game1.uiViewport.Height;
			if (Game1.uiViewport.Width == 1920 && Game1.uiViewport.Height == 1080)
			{
				return 1.48f;
			}
			if (Game1.uiViewport.Width == 2436 && Game1.uiViewport.Height == 1125)
			{
				return 1.56f;
			}
			if (num2 >= num)
			{
				return (float)Game1.uiViewport.Height / 720f;
			}
			return (float)Game1.uiViewport.Width / 1280f;
		}
	}
}
