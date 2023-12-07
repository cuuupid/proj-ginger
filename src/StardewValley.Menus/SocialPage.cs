using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Characters;

namespace StardewValley.Menus
{
	public class SocialPage : IClickableMenu
	{
		public static int slotsOnPage = 5;

		public static readonly string[] defaultFriendships = new string[2] { "Robin", "Lewis" };

		private string descriptionText = "";

		private string hoverText = "";

		private ClickableTextureComponent upButton;

		private ClickableTextureComponent downButton;

		private ClickableTextureComponent scrollBar;

		private Rectangle scrollBarRunner;

		public List<object> names;

		private List<ClickableTextureComponent> sprites;

		private int slotPosition;

		private int numFarmers;

		private List<string> kidsNames = new List<string>();

		private Dictionary<string, string> npcNames;

		public List<ClickableTextureComponent> characterSlots;

		private bool scrolling;

		private int clickedEntry;

		private bool wholePanelScrolling;

		private float widthMod;

		private float heightMod;

		private float scrollSpeed;

		private Rectangle mainBox;

		private string headerText;

		private const int offset = 16;

		private int slotHeight = 112;

		private int portraitX;

		private int nameX;

		private int divider1X;

		private int heartsX;

		private int divider2X;

		private int giftsX;

		private int talkX;

		private int divider0X;

		private int divider3X;

		private int divider4X;

		private int slotsYStart = 80;

		private MobileScrollbar newScrollbar;

		private MobileScrollbox scrollArea;

		public Friendship emptyFriendship = new Friendship();

		public SocialPage(int x, int y, int width, int height)
			: base(x, y, width, height)
		{
			mainBox.X = x;
			mainBox.Y = y;
			mainBox.Width = width;
			mainBox.Height = height;
			widthMod = (float)width / 1280f;
			heightMod = (float)height / (720f - (float)y);
			headerText = Game1.content.LoadString("Strings\\UI:GameMenu_Social");
			portraitX = (int)((float)mainBox.X + 48f * widthMod);
			nameX = portraitX + (int)(88f * widthMod);
			divider0X = (int)(32f * widthMod);
			divider1X = nameX + (int)(250f * widthMod);
			heartsX = divider1X + (int)(100f * widthMod);
			divider2X = divider1X + (int)(450f * widthMod);
			divider4X = (int)((float)mainBox.Width - 32f * widthMod - 28f);
			divider3X = divider2X + (divider4X - 32 - divider2X) / 2;
			int num = (divider4X - 32 - divider2X) / 4;
			giftsX = divider2X + 4 + num;
			talkX = divider2X + 4 + num * 3;
			int num2 = mainBox.Height - slotsYStart;
			slotsOnPage = Math.Max(1, num2 / slotHeight);
			slotHeight = num2 / slotsOnPage;
			slotsYStart = height - slotsOnPage * slotHeight - 32;
			scrolling = (wholePanelScrolling = false);
			clickedEntry = -1;
			string[] array = defaultFriendships;
			foreach (string key in array)
			{
				if (!Game1.player.friendshipData.ContainsKey(key))
				{
					Game1.player.friendshipData.Add(key, new Friendship());
				}
			}
			characterSlots = new List<ClickableTextureComponent>();
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			npcNames = new Dictionary<string, string>();
			foreach (string key2 in Game1.player.friendshipData.Keys)
			{
				string value = key2;
				if (dictionary.ContainsKey(key2) && dictionary[key2].Split('/').Length > 11)
				{
					value = dictionary[key2].Split('/')[11];
				}
				if (!npcNames.ContainsKey(key2))
				{
					npcNames.Add(key2, value);
				}
			}
			foreach (NPC allCharacter in Utility.getAllCharacters())
			{
				if (allCharacter.CanSocialize)
				{
					if (Game1.player.friendshipData.ContainsKey(allCharacter.Name) && allCharacter is Child)
					{
						kidsNames.Add(allCharacter.Name);
						npcNames[allCharacter.Name] = allCharacter.Name.Trim();
					}
					else if (Game1.player.friendshipData.ContainsKey(allCharacter.Name))
					{
						npcNames[allCharacter.Name] = allCharacter.displayName;
					}
					else
					{
						npcNames[allCharacter.Name] = "???";
					}
				}
			}
			names = new List<object>();
			sprites = new List<ClickableTextureComponent>();
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (((bool)allFarmer.isCustomized || allFarmer == Game1.MasterPlayer) && allFarmer != Game1.player)
				{
					names.Add(allFarmer.UniqueMultiplayerID);
					sprites.Add(new ClickableTextureComponent("", new Rectangle(portraitX + mainBox.X, 0, width, 64), null, "", allFarmer.Sprite.Texture, Rectangle.Empty, 4f));
					numFarmers++;
				}
			}
			foreach (KeyValuePair<string, string> item in npcNames.OrderBy((KeyValuePair<string, string> p) => -Game1.player.getFriendshipLevelForNPC(p.Key)))
			{
				NPC nPC = null;
				if (kidsNames.Contains(item.Key))
				{
					nPC = Game1.getCharacterFromName<Child>(item.Key, mustBeVillager: false);
				}
				else if (dictionary.ContainsKey(item.Key))
				{
					string[] array2 = dictionary[item.Key].Split('/');
					string[] array3 = array2[10].Split(' ');
					string textureNameForCharacter = NPC.getTextureNameForCharacter(item.Key);
					if (array3.Length > 2)
					{
						nPC = new NPC(new AnimatedSprite("Characters\\" + textureNameForCharacter, 0, 16, 32), new Vector2(Convert.ToInt32(array3[1]) * 64, Convert.ToInt32(array3[2]) * 64), array3[0], 0, item.Key, null, Game1.content.Load<Texture2D>("Portraits\\" + textureNameForCharacter), eventActor: false);
					}
				}
				if (nPC != null)
				{
					names.Add(nPC.Name);
					sprites.Add(new ClickableTextureComponent("", new Rectangle(portraitX, 0, width, 64), null, "", nPC.Sprite.Texture, nPC.getMugShotSourceRect(), 4f));
				}
			}
			int num3 = 33;
			newScrollbar = new MobileScrollbar(mainBox.X + mainBox.Width - 60, mainBox.Y + 16, 96, mainBox.Height - 32);
			scrollArea = new MobileScrollbox(mainBox.X, mainBox.Y + 16, mainBox.Width - 64, mainBox.Height, (sprites.Count - slotsOnPage) * slotHeight, new Rectangle(mainBox.X + 16, mainBox.Y + 16, mainBox.Width - 32, mainBox.Height - 28), newScrollbar);
			updateSlots();
		}

		public static bool isRoommateOfAnyone(string name)
		{
			return Game1.getCharacterFromName(name)?.isRoommate() ?? false;
		}

		public static bool isDatable(string name)
		{
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			if (!dictionary.ContainsKey(name))
			{
				return false;
			}
			string[] array = dictionary[name].Split('/');
			return array[5] == "datable";
		}

		public Friendship getFriendship(string name)
		{
			if (Game1.player.friendshipData.ContainsKey(name))
			{
				return Game1.player.friendshipData[name];
			}
			return emptyFriendship;
		}

		public override void snapToDefaultClickableComponent()
		{
			if (slotPosition < characterSlots.Count)
			{
				currentlySnappedComponent = characterSlots[slotPosition];
			}
			snapCursorToCurrentSnappedComponent();
		}

		public int getGender(string name)
		{
			if (kidsNames.Contains(name))
			{
				return Game1.getCharacterFromName<Child>(name, mustBeVillager: false).Gender;
			}
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			if (!dictionary.ContainsKey(name))
			{
				return 0;
			}
			string[] array = dictionary[name].Split('/');
			if (!(array[4] == "female"))
			{
				return 0;
			}
			return 1;
		}

		public bool isMarriedToAnyone(string name)
		{
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.spouse == name && allFarmer.isMarried())
				{
					return true;
				}
			}
			return false;
		}

		public void updateSlots()
		{
			int num = scrollArea.getYOffsetForScroll() + mainBox.Y + slotsYStart;
			for (int i = 0; i < sprites.Count; i++)
			{
				sprites[i].bounds.Y = num + slotHeight * i;
			}
		}

		public void addTabsToClickableComponents()
		{
			if (Game1.activeClickableMenu is GameMenu && !allClickableComponents.Contains((Game1.activeClickableMenu as GameMenu).tabs[0]))
			{
				allClickableComponents.AddRange((Game1.activeClickableMenu as GameMenu).tabs);
			}
		}

		protected void _SelectSlot(ClickableComponent slot_component)
		{
			if (slot_component != null && characterSlots.Contains(slot_component))
			{
				int num = characterSlots.IndexOf(slot_component as ClickableTextureComponent);
				currentlySnappedComponent = slot_component;
				if (num < slotPosition)
				{
					slotPosition = num;
				}
				else if (num >= slotPosition + slotsOnPage)
				{
					slotPosition = num - slotsOnPage + 1;
				}
				setScrollBarToCurrentIndex();
				updateSlots();
				if (Game1.options.snappyMenus && Game1.options.gamepadControls)
				{
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public void ConstrainSelectionToVisibleSlots()
		{
			if (characterSlots.Contains(currentlySnappedComponent))
			{
				int num = characterSlots.IndexOf(currentlySnappedComponent as ClickableTextureComponent);
				if (num < slotPosition)
				{
					num = slotPosition;
				}
				else if (num >= slotPosition + slotsOnPage)
				{
					num = slotPosition + slotsOnPage - 1;
				}
				currentlySnappedComponent = characterSlots[num];
				if (Game1.options.snappyMenus && Game1.options.gamepadControls)
				{
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public override void snapCursorToCurrentSnappedComponent()
		{
			if (currentlySnappedComponent != null && characterSlots.Contains(currentlySnappedComponent))
			{
				Game1.setMousePosition(currentlySnappedComponent.bounds.Left + 64, currentlySnappedComponent.bounds.Center.Y);
			}
			else
			{
				base.snapCursorToCurrentSnappedComponent();
			}
		}

		public override void applyMovementKey(int direction)
		{
			if (direction == 0 && slotPosition > 0)
			{
				upArrowPressed();
			}
			else if (direction == 2 && slotPosition < sprites.Count - slotsOnPage)
			{
				downArrowPressed();
			}
			else if (direction == 3 || direction == 1)
			{
				base.applyMovementKey(direction);
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			bool panelScrolling = scrollArea.panelScrolling;
			scrollArea.leftClickHeld(x, y);
			if (scrollArea.panelScrolling || panelScrolling)
			{
				updateSlots();
			}
			if (scrolling)
			{
				clickedEntry = -1;
				float num = newScrollbar.setY(y);
				scrollArea.setYOffsetForScroll(-(int)(num * (float)scrollArea.getMaxYOffset() / 100f));
				updateSlots();
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			scrollArea.releaseLeftClick(x, y);
			if (clickedEntry >= 0 && clickedEntry < sprites.Count && new Rectangle(sprites[clickedEntry].bounds.X, sprites[clickedEntry].bounds.Y, sprites[clickedEntry].bounds.Width, sprites[clickedEntry].bounds.Height + 4).Contains(x, y))
			{
				bool flag = true;
				if (names[clickedEntry] is string)
				{
					Character characterFromName = Game1.getCharacterFromName((string)names[clickedEntry]);
					if (characterFromName != null && Game1.player.friendshipData.ContainsKey(characterFromName.name))
					{
						flag = false;
						Game1.playSound("bigSelect");
						int num = slotPosition;
						ProfileMenu profileMenu = new ProfileMenu(characterFromName);
						profileMenu.exitFunction = delegate
						{
							GameMenu gameMenu = (GameMenu)(Game1.activeClickableMenu = new GameMenu(2));
							SocialPage socialPage = gameMenu.GetCurrentPage() as SocialPage;
						};
						Game1.activeClickableMenu = profileMenu;
						if (Game1.options.SnappyMenus)
						{
							profileMenu.snapToDefaultClickableComponent();
						}
					}
				}
				if (flag)
				{
					Game1.playSound("shiny4");
				}
			}
			scrolling = false;
		}

		private void setScrollBarToCurrentIndex()
		{
		}

		public override void receiveGamePadButton(Buttons b)
		{
			switch (b)
			{
			case Buttons.DPadDown:
			case Buttons.LeftThumbstickDown:
			{
				int yOffsetForScroll2 = scrollArea.getYOffsetForScroll();
				int num3 = (int)Math.Floor((float)yOffsetForScroll2 / (float)slotHeight);
				int yOffsetForScroll3 = Math.Max(-scrollArea.maxYOffset, num3 * slotHeight - slotHeight);
				scrollArea.setYOffsetForScroll(yOffsetForScroll3);
				updateSlots();
				Game1.playSound("shiny4");
				break;
			}
			case Buttons.DPadUp:
			case Buttons.LeftThumbstickUp:
			{
				int num = scrollArea.getYOffsetForScroll() + mainBox.Y + slotsYStart;
				int num2 = (int)Math.Floor((float)num / (float)slotHeight);
				int yOffsetForScroll = Math.Min(0, num2 * slotHeight + slotHeight);
				scrollArea.setYOffsetForScroll(yOffsetForScroll);
				updateSlots();
				Game1.playSound("shiny4");
				break;
			}
			}
		}

		public override void receiveScrollWheelAction(int direction)
		{
			direction *= slotHeight;
			base.receiveScrollWheelAction(direction);
			scrollArea.receiveScrollWheelAction(direction);
			updateSlots();
		}

		public void upArrowPressed()
		{
		}

		public void downArrowPressed()
		{
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (newScrollbar.sliderContains(x, y))
			{
				scrolling = true;
			}
			else if (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y))
			{
				float num = newScrollbar.setY(y);
				scrollArea.setYOffsetForScroll(-(int)(num * (float)scrollArea.getMaxYOffset() / 100f));
				updateSlots();
				Game1.playSound("shwip");
			}
			else
			{
				scrollArea.receiveLeftClick(x, y);
			}
			for (int i = 0; i < sprites.Count; i++)
			{
				if (new Rectangle(sprites[i].bounds.X, sprites[i].bounds.Y, sprites[i].bounds.Width, sprites[i].bounds.Height + 4).Contains(x, y))
				{
					clickedEntry = i;
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			descriptionText = "";
			hoverText = "";
		}

		private void drawNPCSlot(SpriteBatch b, int i)
		{
			if (sprites[i].bounds.Y < yPositionOnScreen - slotHeight || sprites[i].bounds.Y > yPositionOnScreen + height)
			{
				return;
			}
			sprites[i].draw(b);
			string text = names[i] as string;
			int friendshipHeartLevelForNPC = Game1.player.getFriendshipHeartLevelForNPC(text);
			bool flag = isDatable(text);
			Friendship friendship = getFriendship(text);
			bool flag2 = friendship.IsMarried();
			bool flag3 = flag2 && isRoommateOfAnyone(text);
			float y = Game1.smallFont.MeasureString("W").Y;
			float num = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? ((0f - y) / 2f) : 0f);
			Utility.drawTextWithShadow(b, npcNames[text], (npcNames[text].Length <= 10) ? Game1.dialogueFont : ((Game1.uiViewport.Width > 1400) ? Game1.dialogueFont : Game1.smallFont), new Vector2(nameX, (float)(sprites[i].bounds.Y + 48) + num - (float)((!flag) ? 20 : (Game1.options.bigFonts ? 40 : 24))), Game1.textColor);
			for (int j = 0; j < Math.Max(Utility.GetMaximumHeartsForCharacter(Game1.getCharacterFromName(text)), 10); j++)
			{
				int x = ((j < friendshipHeartLevelForNPC) ? 211 : 218);
				if (flag && !friendship.IsDating() && !flag2 && j >= 8)
				{
					x = 211;
				}
				if (j < 10)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(mainBox.X + heartsX) + (float)(j * 32) * widthMod, sprites[i].bounds.Y + 64 - 28), new Rectangle(x, 428, 7, 6), (flag && !friendship.IsDating() && !sprites[i].hoverText.Split('_')[0].Equals("true") && !flag2 && j >= 8) ? (Color.Black * 0.35f) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				}
				else
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(mainBox.X + heartsX) + (float)((j - 10) * 32) * widthMod, sprites[i].bounds.Y + 64), new Rectangle(x, 428, 7, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				}
			}
			if (flag || flag3)
			{
				string text2 = ((!Game1.content.ShouldUseGenderedCharacterTranslations()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635") : ((getGender(text) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').First() : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').Last()));
				if (flag3)
				{
					text2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Housemate");
				}
				else if (flag2)
				{
					text2 = ((getGender(text) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11636") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11637"));
				}
				else if (isMarriedToAnyone(text))
				{
					text2 = ((getGender(text) == 0) ? Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_MaleNPC") : Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_FemaleNPC"));
				}
				else if (!Game1.player.isMarried() && friendship.IsDating())
				{
					text2 = ((getGender(text) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11639") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11640"));
				}
				else if (getFriendship(text).IsDivorced())
				{
					text2 = ((getGender(text) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11642") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11643"));
				}
				int num2 = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
				text2 = Game1.parseText(text2, Game1.smallFont, num2);
				Vector2 vector = Game1.smallFont.MeasureString(text2);
				Utility.drawTextWithShadow(b, text2, Game1.smallFont, new Vector2(nameX, (float)sprites[i].bounds.Bottom - (vector.Y - y) - (float)(Game1.options.bigFonts ? 16 : 0)), Game1.textColor);
			}
			if (!getFriendship(text).IsMarried() && !kidsNames.Contains(text))
			{
				b.Draw(Game1.mouseCursors, Utility.To4(new Vector2(mainBox.X + giftsX, sprites[i].bounds.Y - 4)), new Rectangle(229, 410, 14, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				b.Draw(Game1.mouseCursors, Utility.To4(new Vector2(mainBox.X + giftsX - 12, sprites[i].bounds.Y + 32 + 20)), new Rectangle(227 + ((getFriendship(text).GiftsThisWeek == 2) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				b.Draw(Game1.mouseCursors, Utility.To4(new Vector2(mainBox.X + giftsX + 32, sprites[i].bounds.Y + 32 + 20)), new Rectangle(227 + ((getFriendship(text).GiftsThisWeek >= 1) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				b.Draw(Game1.mouseCursors2, Utility.To4(new Vector2(mainBox.X + talkX, sprites[i].bounds.Y)), new Rectangle(180, 175, 13, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				b.Draw(Game1.mouseCursors, Utility.To4(new Vector2(mainBox.X + talkX + 8, sprites[i].bounds.Y + 32 + 20)), new Rectangle(227 + (getFriendship(text).TalkedToToday ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			}
			if (flag2)
			{
				if (!flag3 || text == "Krobus")
				{
					b.Draw(Game1.objectSpriteSheet, Utility.To4(new Vector2((float)nameX + Game1.dialogueFont.MeasureString(text).X, sprites[i].bounds.Y)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, flag3 ? 808 : 460, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
				}
			}
			else if (friendship.IsDating())
			{
				b.Draw(Game1.objectSpriteSheet, Utility.To4(new Vector2((float)nameX + Game1.dialogueFont.MeasureString(text).X, sprites[i].bounds.Y)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, flag3 ? 808 : 458, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
			}
		}

		private int rowPosition(int i)
		{
			int num = i - slotPosition;
			int num2 = 112;
			return yPositionOnScreen + IClickableMenu.borderWidth + 160 + 4 + num * num2;
		}

		private void drawFarmerSlot(SpriteBatch b, int i)
		{
			long num = (long)names[i];
			Farmer farmerMaybeOffline = Game1.getFarmerMaybeOffline(num);
			if (farmerMaybeOffline != null)
			{
				int num2 = ((!farmerMaybeOffline.IsMale) ? 1 : 0);
				ClickableTextureComponent clickableTextureComponent = sprites[i];
				int x = clickableTextureComponent.bounds.X;
				int y = clickableTextureComponent.bounds.Y;
				Rectangle scissorRectangle = b.GraphicsDevice.ScissorRectangle;
				Rectangle scissorRectangle2 = scissorRectangle;
				scissorRectangle2.Height = Math.Min(scissorRectangle2.Bottom, rowPosition(i)) - scissorRectangle2.Y - 4;
				b.GraphicsDevice.ScissorRectangle = scissorRectangle2;
				FarmerRenderer.isDrawingForUI = true;
				try
				{
					farmerMaybeOffline.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(farmerMaybeOffline.bathingClothes ? 108 : 0, 0, secondaryArm: false, flip: false), farmerMaybeOffline.bathingClothes ? 108 : 0, new Rectangle(0, farmerMaybeOffline.bathingClothes ? 576 : 0, 16, 32), new Vector2(x, y), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, farmerMaybeOffline);
				}
				finally
				{
					b.GraphicsDevice.ScissorRectangle = scissorRectangle;
				}
				FarmerRenderer.isDrawingForUI = false;
				Friendship friendship = Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, num);
				bool flag = friendship.IsMarried();
				float y2 = Game1.smallFont.MeasureString("W").Y;
				float num3 = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? ((0f - y2) / 2f) : 0f);
				string text = ChatBox.formattedUserName(farmerMaybeOffline);
				b.DrawString(Game1.dialogueFont, text, new Vector2(mainBox.X + nameX, (float)(sprites[i].bounds.Y + 48) + num3 - 24f), Game1.textColor);
				string text2 = ((!Game1.content.ShouldUseGenderedCharacterTranslations()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635") : ((num2 == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').First() : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').Last()));
				if (flag)
				{
					text2 = ((num2 == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11636") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11637"));
				}
				else if (farmerMaybeOffline.isMarried() && !farmerMaybeOffline.hasRoommate())
				{
					text2 = ((num2 == 0) ? Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_MaleNPC") : Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_FemaleNPC"));
				}
				else if (!Game1.player.isMarried() && friendship.IsDating())
				{
					text2 = ((num2 == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11639") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11640"));
				}
				else if (friendship.IsDivorced())
				{
					text2 = ((num2 == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11642") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11643"));
				}
				int num4 = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
				text2 = Game1.parseText(text2, Game1.smallFont, num4);
				Vector2 vector = Game1.smallFont.MeasureString(text2);
				b.DrawString(Game1.smallFont, text2, new Vector2((float)(mainBox.X + nameX + 192 + 8) - vector.X / 2f, (float)sprites[i].bounds.Bottom - (vector.Y - y2)), Game1.textColor);
				if (flag)
				{
					b.Draw(Game1.objectSpriteSheet, new Vector2((float)(mainBox.X + nameX) + Game1.dialogueFont.MeasureString(text2).X / 2f, sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 801, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
				}
				else if (friendship.IsDating())
				{
					b.Draw(Game1.objectSpriteSheet, new Vector2((float)(mainBox.X + nameX) + Game1.dialogueFont.MeasureString(text2).X / 2f, sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 458, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (!TutorialManager.Instance.socialHasBeenSeen)
			{
				TutorialManager.Instance.socialHasBeenSeen = true;
				TutorialManager.Instance.completeTutorial(tutorialType.DUMMY_SOCIAL);
			}
			IClickableMenu.drawTextureBox(b, mainBox.X, mainBox.Y, mainBox.Width, mainBox.Height, Color.White);
			int num = sprites[slotPosition].bounds.Y + 4 + sprites[slotPosition].bounds.Height - slotHeight;
			scrollArea.setUpForScrollBoxDrawing(b);
			drawMobileVerticalPartition(b, mainBox.X + divider0X - 32, num + 32, slotHeight * sprites.Count());
			drawMobileVerticalPartition(b, mainBox.X + divider1X, num + 32, slotHeight * sprites.Count());
			drawMobileVerticalPartition(b, mainBox.X + divider2X, num + 32, slotHeight * sprites.Count());
			drawMobileVerticalPartition(b, mainBox.X + divider3X, num + 32, slotHeight * sprites.Count());
			drawMobileVerticalPartition(b, mainBox.X + divider4X - 32, num + 32, slotHeight * sprites.Count());
			drawMobileHorizontalPartition(b, mainBox.X + divider0X, num, divider4X - divider0X, small: true);
			b.End();
			b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
			for (int i = 0; i < sprites.Count; i++)
			{
				if (names[i] is string)
				{
					drawNPCSlot(b, i);
				}
				else if (names[i] is long)
				{
					drawFarmerSlot(b, i);
				}
				drawMobileHorizontalPartition(b, mainBox.X + divider0X, sprites[i].bounds.Y + 4 + sprites[i].bounds.Height, divider4X - divider0X, small: true);
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			scrollArea.finishScrollBoxDrawing(b);
			newScrollbar.draw(b);
		}

		public override void update(GameTime time)
		{
			bool scrollingWithMomentum = scrollArea.scrollingWithMomentum;
			scrollArea.update(time);
			if (scrollArea.scrollingWithMomentum || scrollingWithMomentum)
			{
				updateSlots();
			}
		}
	}
}
