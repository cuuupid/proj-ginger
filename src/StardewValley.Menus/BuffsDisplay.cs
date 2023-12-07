using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class BuffsDisplay : IClickableMenu
	{
		public const int fullnessLength = 180000;

		public const int quenchedLength = 60000;

		private Dictionary<ClickableTextureComponent, Buff> buffs = new Dictionary<ClickableTextureComponent, Buff>();

		public Buff food;

		public Buff drink;

		public List<Buff> otherBuffs = new List<Buff>();

		public int fullnessLeft;

		public int quenchedLeft;

		public string hoverText = "";

		private bool _hovering;

		private Buff _selectedBuff;

		public BuffsDisplay()
		{
			updatePosition();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			x = (int)((float)x / Game1.DateTimeScale);
			y = (int)((float)y / Game1.DateTimeScale);
			foreach (KeyValuePair<ClickableTextureComponent, Buff> buff in buffs)
			{
				ClickableTextureComponent key = buff.Key;
				if (key.bounds.Contains(x, y))
				{
					_hovering = true;
					break;
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		private void updatePosition()
		{
			int num = (int)((float)Game1.uiViewport.Width / Game1.DateTimeScale);
			int num2 = 288;
			int num3 = 64;
			int num4 = num - Math.Max(12, Game1.xEdge) - (int)(392f * Game1.DateTimeScale) - 32 - width;
			int num5 = 16;
			xPositionOnScreen = num4;
			yPositionOnScreen = num5;
			width = num2;
			height = num3;
			if (!Game1.options.verticalToolbar && Game1.toolbar.alignTop)
			{
				yPositionOnScreen += (int)((float)Game1.toolbar.itemSlotSize / Game1.DateTimeScale) + 12;
			}
			syncIcons();
		}

		public override void performHoverAction(int x, int y)
		{
			hoverText = "";
			foreach (KeyValuePair<ClickableTextureComponent, Buff> buff in buffs)
			{
				if (buff.Key.containsPoint(x, y))
				{
					_hovering = true;
					_selectedBuff = buff.Value;
					Toolbar.toolbarPressed = true;
					hoverText = buff.Key.hoverText;
					buff.Key.scale = Math.Min(buff.Key.baseScale + 0.1f, buff.Key.scale + 0.02f);
					break;
				}
			}
		}

		public void mobileArrangeComponents()
		{
			int num = 64;
			int num2 = 16;
			width = buffs.Count * (num + num2);
			if (buffs.Count > 0)
			{
				width -= num2;
			}
			int num3 = (int)((float)Game1.uiViewport.Width / Game1.DateTimeScale);
			xPositionOnScreen = num3 - width - Math.Max(12, Game1.xEdge) - 392;
			int num4 = 0;
			foreach (KeyValuePair<ClickableTextureComponent, Buff> buff in buffs)
			{
				buff.Key.bounds = new Rectangle(xPositionOnScreen + num4 * (num + num2), yPositionOnScreen, num, num);
				num4++;
			}
		}

		public void arrangeTheseComponentsInThisRectangle(int rectangleX, int rectangleY, int rectangleWidthInComponentWidthUnits, int componentWidth, int componentHeight, int buffer, bool rightToLeft)
		{
			int num = 0;
			int num2 = 0;
			foreach (KeyValuePair<ClickableTextureComponent, Buff> buff in buffs)
			{
				ClickableTextureComponent key = buff.Key;
				if (rightToLeft)
				{
					key.bounds = new Rectangle(rectangleX + rectangleWidthInComponentWidthUnits * componentWidth - (num + 1) * (componentWidth + buffer), rectangleY + num2 * (componentHeight + buffer), componentWidth, componentHeight);
				}
				else
				{
					key.bounds = new Rectangle(rectangleX + num * (componentWidth + buffer), rectangleY + num2 * (componentHeight + buffer), componentWidth, componentHeight);
				}
				num++;
				if (num > rectangleWidthInComponentWidthUnits)
				{
					num2++;
					num %= rectangleWidthInComponentWidthUnits;
				}
			}
		}

		public void syncIcons()
		{
			buffs.Clear();
			if (food != null)
			{
				foreach (ClickableTextureComponent clickableComponent in food.getClickableComponents())
				{
					buffs.Add(clickableComponent, food);
				}
			}
			if (drink != null)
			{
				foreach (ClickableTextureComponent clickableComponent2 in drink.getClickableComponents())
				{
					buffs.Add(clickableComponent2, drink);
				}
			}
			foreach (Buff otherBuff in otherBuffs)
			{
				foreach (ClickableTextureComponent clickableComponent3 in otherBuff.getClickableComponents())
				{
					buffs.Add(clickableComponent3, otherBuff);
				}
			}
			mobileArrangeComponents();
		}

		public bool hasBuff(int which)
		{
			return Game1.player.hasBuff(which);
		}

		public bool tryToAddFoodBuff(Buff b, int duration)
		{
			if (b.source.Equals("Squid Ink Ravioli"))
			{
				addOtherBuff(new Buff(28));
			}
			if (b.total > 0 && fullnessLeft <= 0)
			{
				if (food != null)
				{
					food.removeBuff();
				}
				food = b;
				food.addBuff();
				syncIcons();
				return true;
			}
			return false;
		}

		public bool tryToAddDrinkBuff(Buff b)
		{
			if (b.source.Contains("Beer") || b.source.Contains("Wine") || b.source.Contains("Mead") || b.source.Contains("Pale Ale"))
			{
				addOtherBuff(new Buff(17));
			}
			else if (b.source.Equals("Oil of Garlic"))
			{
				addOtherBuff(new Buff(23));
			}
			else if (b.source.Equals("Life Elixir"))
			{
				Game1.player.health = Game1.player.maxHealth;
			}
			else if (b.source.Equals("Muscle Remedy"))
			{
				Game1.player.exhausted.Value = false;
			}
			if (b.total > 0 && quenchedLeft <= 0)
			{
				if (drink != null)
				{
					drink.removeBuff();
				}
				drink = b;
				drink.addBuff();
				syncIcons();
				return true;
			}
			return false;
		}

		public bool removeOtherBuff(int which)
		{
			bool flag = false;
			for (int i = 0; i < otherBuffs.Count; i++)
			{
				Buff buff = otherBuffs[i];
				if (which == buff.which)
				{
					buff.removeBuff();
					otherBuffs.RemoveAt(i);
					flag = true;
				}
			}
			if (flag)
			{
				syncIcons();
			}
			return flag;
		}

		public bool addOtherBuff(Buff buff)
		{
			if (buff.which != -1)
			{
				foreach (KeyValuePair<ClickableTextureComponent, Buff> buff2 in buffs)
				{
					if (buff.which == buff2.Value.which)
					{
						buff2.Value.millisecondsDuration = buff.millisecondsDuration;
						buff2.Key.scale = buff2.Key.baseScale + 0.2f;
						return false;
					}
				}
			}
			otherBuffs.Add(buff);
			buff.addBuff();
			syncIcons();
			return true;
		}

		public new void update(GameTime time)
		{
			if (!Game1.wasMouseVisibleThisFrame)
			{
				hoverText = "";
			}
			if (food != null && food.update(time))
			{
				food.removeBuff();
				food = null;
				syncIcons();
			}
			if (drink != null && drink.update(time))
			{
				drink.removeBuff();
				drink = null;
				syncIcons();
			}
			for (int num = otherBuffs.Count - 1; num >= 0; num--)
			{
				if (otherBuffs[num].update(time))
				{
					otherBuffs[num].removeBuff();
					otherBuffs.RemoveAt(num);
					syncIcons();
				}
			}
			foreach (KeyValuePair<ClickableTextureComponent, Buff> buff in buffs)
			{
				ClickableTextureComponent key = buff.Key;
				key.scale = Math.Max(key.baseScale, key.scale - 0.01f);
				if (!buff.Value.alreadyUpdatedIconAlpha && (float)buff.Value.millisecondsDuration < Math.Min(10000f, (float)buff.Value.totalMillisecondsDuration / 10f))
				{
					buff.Value.displayAlphaTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / (((float)buff.Value.millisecondsDuration < Math.Min(2000f, (float)buff.Value.totalMillisecondsDuration / 20f)) ? 1f : 2f);
					buff.Value.alreadyUpdatedIconAlpha = true;
				}
			}
		}

		public void clearAllBuffs()
		{
			otherBuffs.Clear();
			if (food != null)
			{
				food.removeBuff();
				food = null;
			}
			if (drink != null)
			{
				drink.removeBuff();
				drink = null;
			}
			buffs.Clear();
		}

		public override void draw(SpriteBatch b)
		{
			if (Game1.activeClickableMenu != null)
			{
				return;
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(Game1.DateTimeScale));
			updatePosition();
			foreach (KeyValuePair<ClickableTextureComponent, Buff> buff in buffs)
			{
				buff.Key.draw(b, Color.White * ((buff.Value.displayAlphaTimer > 0f) ? ((float)(Math.Cos(buff.Value.displayAlphaTimer / 100f) + 3.0) / 4f) : 1f), 0.001f);
				buff.Value.alreadyUpdatedIconAlpha = false;
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (_hovering && _selectedBuff != null && hoverText != null)
			{
				IClickableMenu.drawHoverText(b, hoverText + "\n" + _selectedBuff.getTimeLeft(), Game1.dialogueFont, 0, 64);
				_hovering = false;
			}
		}
	}
}
