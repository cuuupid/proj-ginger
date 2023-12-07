using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	public class ProfileMenu : IClickableMenu
	{
		public class ProfileItemCategory
		{
			public string categoryName;

			public int[] validCategories;

			public ProfileItemCategory(string name, int[] valid_categories)
			{
				categoryName = name;
				validCategories = valid_categories;
			}
		}

		public const int region_backButton = 101;

		public const int region_forwardButton = 102;

		public int letterWidth = 320;

		public int letterHeight = 180;

		public Texture2D letterTexture;

		public Texture2D secretNoteImageTexture;

		protected string hoverText = "";

		protected List<ProfileItem> _profileItems;

		protected Character _target;

		public Item hoveredItem;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent forwardButton;

		public ClickableTextureComponent nextCharacterButton;

		public ClickableTextureComponent previousCharacterButton;

		protected Rectangle characterSpriteBox;

		protected int _currentCategory;

		protected AnimatedSprite _animatedSprite;

		protected float _directionChangeTimer;

		protected float _hiddenEmoteTimer = -1f;

		protected int _currentDirection;

		protected int _hideTooltipTime;

		protected SocialPage _socialPage;

		protected string _status = "";

		protected string _printedName = "";

		protected Vector2 _characterEntrancePosition = new Vector2(0f, 0f);

		public List<ClickableComponent> clickableProfileItems;

		protected List<Character> _charactersList;

		protected Friendship _friendship;

		protected Vector2 _characterNamePosition;

		protected Vector2 _heartDisplayPosition;

		protected Vector2 _birthdayHeadingDisplayPosition;

		protected Vector2 _birthdayDisplayPosition;

		protected Vector2 _statusHeadingDisplayPosition;

		protected Vector2 _statusDisplayPosition;

		protected Vector2 _giftLogHeadingDisplayPosition;

		protected Vector2 _giftLogCategoryDisplayPosition;

		protected Vector2 _errorMessagePosition;

		protected Vector2 _characterSpriteDrawPosition;

		protected Rectangle _characterStatusDisplayBox;

		protected List<ClickableTextureComponent> _clickableTextureComponents;

		private MobileScrollbox scrollArea;

		private Rectangle storedGiftLogRect;

		public static ProfileItemCategory[] itemCategories = new ProfileItemCategory[10]
		{
			new ProfileItemCategory("Profile_Gift_Category_LikedGifts", null),
			new ProfileItemCategory("Profile_Gift_Category_FruitsAndVegetables", new int[2] { -75, -79 }),
			new ProfileItemCategory("Profile_Gift_Category_AnimalProduce", new int[4] { -6, -5, -14, -18 }),
			new ProfileItemCategory("Profile_Gift_Category_ArtisanItems", new int[1] { -26 }),
			new ProfileItemCategory("Profile_Gift_Category_CookedItems", new int[1] { -7 }),
			new ProfileItemCategory("Profile_Gift_Category_ForagedItems", new int[4] { -80, -81, -23, -17 }),
			new ProfileItemCategory("Profile_Gift_Category_Fish", new int[1] { -4 }),
			new ProfileItemCategory("Profile_Gift_Category_Ingredients", new int[2] { -27, -25 }),
			new ProfileItemCategory("Profile_Gift_Category_MineralsAndGems", new int[3] { -15, -12, -2 }),
			new ProfileItemCategory("Profile_Gift_Category_Misc", null)
		};

		protected Dictionary<int, List<Item>> _sortedItems;

		private bool drawBackPanel;

		private int _characterSpriteRandomInt;

		public ProfileMenu(Character character)
		{
			if (Game1.uiViewport.Width < 1280)
			{
				letterWidth = Game1.uiViewport.Width / 4;
				letterHeight = Game1.uiViewport.Height / 4;
				drawBackPanel = false;
				initialize(Game1.xEdge, 0, Game1.uiViewport.Width - Game1.xEdge * 2, Game1.uiViewport.Height, showUpperRightCloseButton: true);
			}
			else
			{
				drawBackPanel = false;
				initialize(Game1.xEdge, 0, Game1.uiViewport.Width - Game1.xEdge * 2, Game1.uiViewport.Height, showUpperRightCloseButton: true);
			}
			_charactersList = new List<Character>();
			_socialPage = new SocialPage(0, 0, 0, 0);
			_printedName = "";
			_characterEntrancePosition = new Vector2(0f, 4f);
			foreach (object name in _socialPage.names)
			{
				if (!(name is long) && name is string)
				{
					NPC characterFromName = Game1.getCharacterFromName((string)name);
					if (characterFromName != null && Game1.player.friendshipData.ContainsKey(characterFromName.name))
					{
						_charactersList.Add(characterFromName);
					}
				}
			}
			_profileItems = new List<ProfileItem>();
			clickableProfileItems = new List<ClickableComponent>();
			UpdateButtons();
			letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
			_SetCharacter(character);
		}

		public Character GetCharacter()
		{
			return _target;
		}

		public NPC GetTemporaryCharacter(Character character)
		{
			Texture2D texture2D = null;
			try
			{
				texture2D = Game1.content.Load<Texture2D>("Portraits\\" + (character as NPC).getTextureName());
			}
			catch (Exception)
			{
				return null;
			}
			int num = ((character.name.Contains("Dwarf") || character.name.Equals("Krobus")) ? 96 : 128);
			return new NPC(new AnimatedSprite("Characters\\" + (character as NPC).getTextureName(), 0, 16, num / 4), new Vector2(0f, 0f), character.Name, character.facingDirection, character.name, null, texture2D, eventActor: true);
		}

		protected void _SetCharacter(Character character)
		{
			_target = character;
			_sortedItems = new Dictionary<int, List<Item>>();
			if (_target is NPC)
			{
				_friendship = _socialPage.getFriendship(_target.name);
				NPC temporaryCharacter = GetTemporaryCharacter(_target);
				_animatedSprite = temporaryCharacter.Sprite.Clone();
				_animatedSprite.tempSpriteHeight = -1;
				_animatedSprite.faceDirection(2);
				foreach (int key in Game1.objectInformation.Keys)
				{
					Object @object = new Object(key, 1);
					if (!Game1.player.hasGiftTasteBeenRevealed(temporaryCharacter, key) || (@object.Name == "Stone" && key != 390))
					{
						continue;
					}
					for (int i = 0; i < itemCategories.Length; i++)
					{
						if (itemCategories[i].categoryName == "Profile_Gift_Category_LikedGifts")
						{
							int giftTasteForThisItem = temporaryCharacter.getGiftTasteForThisItem(@object);
							if (giftTasteForThisItem == 2 || giftTasteForThisItem == 0)
							{
								if (!_sortedItems.ContainsKey(i))
								{
									_sortedItems[i] = new List<Item>();
								}
								_sortedItems[i].Add(@object);
							}
						}
						else if (itemCategories[i].categoryName == "Profile_Gift_Category_Misc")
						{
							bool flag = false;
							for (int j = 0; j < itemCategories.Length; j++)
							{
								if (itemCategories[j].validCategories != null && itemCategories[j].validCategories.Contains(@object.Category))
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								int giftTasteForThisItem2 = temporaryCharacter.getGiftTasteForThisItem(@object);
								if (!_sortedItems.ContainsKey(i))
								{
									_sortedItems[i] = new List<Item>();
								}
								_sortedItems[i].Add(@object);
							}
						}
						else if (itemCategories[i].validCategories.Contains(@object.Category))
						{
							if (!_sortedItems.ContainsKey(i))
							{
								_sortedItems[i] = new List<Item>();
							}
							_sortedItems[i].Add(@object);
						}
					}
				}
				bool flag2 = SocialPage.isDatable(_target.name);
				bool flag3 = _friendship.IsMarried();
				bool flag4 = flag3 && SocialPage.isRoommateOfAnyone(_target.name);
				_status = "";
				if (flag2 || flag4)
				{
					string text = ((LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.pt) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635") : ((_socialPage.getGender(_target.name) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').First() : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').Last()));
					if (flag4)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:Housemate");
					}
					else if (flag3)
					{
						text = ((_socialPage.getGender(_target.name) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11636") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11637"));
					}
					else if (_socialPage.isMarriedToAnyone(_target.name))
					{
						text = ((_socialPage.getGender(_target.name) == 0) ? Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_MaleNPC") : Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_FemaleNPC"));
					}
					else if (!Game1.player.isMarried() && _friendship.IsDating())
					{
						text = ((_socialPage.getGender(_target.name) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11639") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11640"));
					}
					else if (_socialPage.getFriendship(_target.name).IsDivorced())
					{
						text = ((_socialPage.getGender(_target.name) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11642") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11643"));
					}
					text = Game1.parseText(text, Game1.smallFont, width);
					string s = text.Replace("(", "").Replace(")", "").Replace("（", "")
						.Replace("）", "");
					s = (_status = Utility.capitalizeFirstLetter(s));
				}
				_UpdateList();
			}
			_directionChangeTimer = 2000f;
			_currentDirection = 2;
			_hiddenEmoteTimer = -1f;
		}

		public void ChangeCharacter(int offset)
		{
			int num = _charactersList.IndexOf(_target);
			if (num == -1)
			{
				if (_charactersList.Count > 0)
				{
					_SetCharacter(_charactersList[0]);
				}
				return;
			}
			for (num += offset; num < 0; num += _charactersList.Count)
			{
			}
			while (num >= _charactersList.Count)
			{
				num -= _charactersList.Count;
			}
			_SetCharacter(_charactersList[num]);
			Game1.playSound("smallSelect");
			_printedName = "";
			_characterEntrancePosition = new Vector2(Math.Sign(offset) * -4, 0f);
		}

		protected void _UpdateList()
		{
			for (int i = 0; i < _profileItems.Count; i++)
			{
				_profileItems[i].Unload();
			}
			_profileItems.Clear();
			if (!(_target is NPC))
			{
				return;
			}
			NPC nPC = _target as NPC;
			List<Item> list = new List<Item>();
			List<Item> list2 = new List<Item>();
			List<Item> list3 = new List<Item>();
			List<Item> list4 = new List<Item>();
			List<Item> list5 = new List<Item>();
			if (_sortedItems.ContainsKey(_currentCategory))
			{
				foreach (Item item2 in _sortedItems[_currentCategory])
				{
					switch (nPC.getGiftTasteForThisItem(item2))
					{
					case 0:
						list.Add(item2);
						break;
					case 2:
						list2.Add(item2);
						break;
					case 8:
						list3.Add(item2);
						break;
					case 4:
						list4.Add(item2);
						break;
					case 6:
						list5.Add(item2);
						break;
					}
				}
			}
			PI_ItemList item = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Loved"), list);
			_profileItems.Add(item);
			item = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Liked"), list2);
			_profileItems.Add(item);
			item = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Neutral"), list3);
			_profileItems.Add(item);
			item = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Disliked"), list4);
			_profileItems.Add(item);
			item = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Hated"), list5);
			_profileItems.Add(item);
			SetupLayout();
			populateClickableComponentList();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls && (currentlySnappedComponent == null || !allClickableComponents.Contains(currentlySnappedComponent)))
			{
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			if (clickableProfileItems.Count > 0)
			{
				currentlySnappedComponent = clickableProfileItems[0];
			}
			else
			{
				currentlySnappedComponent = backButton;
			}
			snapCursorToCurrentSnappedComponent();
		}

		public void UpdateButtons()
		{
			_clickableTextureComponents = new List<ClickableTextureComponent>();
			backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 32 - 64, 80, 76), Game1.mobileSpriteSheet, new Rectangle(80, 0, 20, 19), 4f)
			{
				myID = 0,
				name = "Back Button",
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998
			};
			_clickableTextureComponents.Add(backButton);
			forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 32 - 64, 80, 76), Game1.mobileSpriteSheet, new Rectangle(100, 0, 20, 19), 4f)
			{
				myID = 0,
				name = "Forward Button",
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998
			};
			_clickableTextureComponents.Add(forwardButton);
			previousCharacterButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, drawBackPanel ? (yPositionOnScreen + height - 32 - 64) : forwardButton.bounds.Y, 80, 76), Game1.mobileSpriteSheet, new Rectangle(80, 0, 20, 19), 4f)
			{
				myID = 0,
				name = "Previous Char",
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998
			};
			_clickableTextureComponents.Add(previousCharacterButton);
			nextCharacterButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, drawBackPanel ? (yPositionOnScreen + height - 32 - 64) : forwardButton.bounds.Y, 80, 76), Game1.mobileSpriteSheet, new Rectangle(100, 0, 20, 19), 4f)
			{
				myID = 0,
				name = "Next Char",
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998
			};
			_clickableTextureComponents.Add(nextCharacterButton);
		}

		public void ChangePage(int offset)
		{
			_currentCategory += offset;
			while (_currentCategory < 0)
			{
				_currentCategory += itemCategories.Length;
			}
			while (_currentCategory >= itemCategories.Length)
			{
				_currentCategory -= itemCategories.Length;
			}
			Game1.playSound("shwip");
			_UpdateList();
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(letterWidth * 4, letterHeight * 4).X;
			yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(letterWidth * 4, letterHeight * 4).Y;
			UpdateButtons();
			SetupLayout();
			initializeUpperRightCloseButton();
			populateClickableComponentList();
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			switch (b)
			{
			case Buttons.LeftTrigger:
				ChangePage(-1);
				break;
			case Buttons.RightTrigger:
				ChangePage(1);
				break;
			case Buttons.RightShoulder:
				ChangeCharacter(1);
				break;
			case Buttons.LeftShoulder:
				ChangeCharacter(-1);
				break;
			case Buttons.Back:
				PlayHiddenEmote();
				break;
			}
		}

		public override void receiveKeyPress(Keys key)
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

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			scrollArea.receiveScrollWheelAction(direction);
			updateLayout();
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			scrollArea.releaseLeftClick(x, y);
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			scrollArea.leftClickHeld(x, y);
			updateLayout();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			scrollArea.receiveLeftClick(x, y);
			if (upperRightCloseButton != null && readyToClose() && upperRightCloseButton.containsPoint(x, y))
			{
				exitThisMenu();
			}
			else if (Game1.activeClickableMenu == null && Game1.currentMinigame == null)
			{
				unload();
			}
			else if (backButton.containsPoint(x, y))
			{
				ChangePage(-1);
			}
			else if (forwardButton.containsPoint(x, y))
			{
				ChangePage(1);
			}
			else if (previousCharacterButton.containsPoint(x, y))
			{
				ChangeCharacter(-1);
			}
			else if (nextCharacterButton.containsPoint(x, y))
			{
				ChangeCharacter(1);
			}
			else if (characterSpriteBox.Contains(x, y))
			{
				PlayHiddenEmote();
			}
		}

		public void PlayHiddenEmote()
		{
			if (GetCharacter() != null)
			{
				string text = GetCharacter().name;
				if (Game1.player.getFriendshipHeartLevelForNPC(GetCharacter().name) >= 4)
				{
					_currentDirection = 2;
					_hiddenEmoteTimer = 4000f;
					_characterSpriteRandomInt = Game1.random.Next(4);
					Game1.playSound("drumkit6");
				}
				else
				{
					_currentDirection = 2;
					_directionChangeTimer = 5000f;
					Game1.playSound("Cowboy_Footstep");
				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			hoveredItem = null;
			foreach (ProfileItem profileItem in _profileItems)
			{
				profileItem.performHover(x, y);
			}
			backButton.tryHover(x, y, 0.6f);
			forwardButton.tryHover(x, y, 0.6f);
			nextCharacterButton.tryHover(x, y, 0.6f);
			previousCharacterButton.tryHover(x, y, 0.6f);
		}

		public override void update(GameTime time)
		{
			base.update(time);
			bool scrollingWithMomentum = scrollArea.scrollingWithMomentum;
			scrollArea.update(time);
			if (scrollArea.scrollingWithMomentum || scrollingWithMomentum)
			{
				updateLayout();
			}
			if (_target != null && _target.displayName != null && _printedName.Length < _target.displayName.Length)
			{
				_printedName += _target.displayName[_printedName.Length];
			}
			if (_hideTooltipTime > 0)
			{
				_hideTooltipTime -= time.ElapsedGameTime.Milliseconds;
				if (_hideTooltipTime < 0)
				{
					_hideTooltipTime = 0;
				}
			}
			if (_characterEntrancePosition.X != 0f)
			{
				_characterEntrancePosition.X -= (float)Math.Sign(_characterEntrancePosition.X) * 0.25f;
			}
			if (_characterEntrancePosition.Y != 0f)
			{
				_characterEntrancePosition.Y -= (float)Math.Sign(_characterEntrancePosition.Y) * 0.25f;
			}
			if (_animatedSprite == null)
			{
				return;
			}
			if (_hiddenEmoteTimer > 0f)
			{
				_hiddenEmoteTimer += time.ElapsedGameTime.Milliseconds;
				if (_hiddenEmoteTimer <= 0f)
				{
					_hiddenEmoteTimer = -1f;
					_currentDirection = 2;
					_directionChangeTimer = 2000f;
				}
			}
			else if (_directionChangeTimer > 0f)
			{
				_directionChangeTimer -= time.ElapsedGameTime.Milliseconds;
				if (_directionChangeTimer <= 0f)
				{
					_directionChangeTimer = 2000f;
					_currentDirection = (_currentDirection + 1) % 4;
				}
			}
			if (_characterEntrancePosition != Vector2.Zero)
			{
				if (_characterEntrancePosition.X < 0f)
				{
					_animatedSprite.AnimateRight(time, 2);
				}
				else if (_characterEntrancePosition.X > 0f)
				{
					_animatedSprite.AnimateLeft(time, 2);
				}
				else if (_characterEntrancePosition.Y > 0f)
				{
					_animatedSprite.AnimateUp(time, 2);
				}
				else if (_characterEntrancePosition.Y < 0f)
				{
					_animatedSprite.AnimateDown(time, 2);
				}
			}
			else if (_hiddenEmoteTimer > 0f)
			{
				string text = GetCharacter().name;
				if (text != null)
				{
					switch (text.Length)
					{
					case 7:
						switch (text[0])
						{
						case 'A':
							if (!(text == "Abigail"))
							{
								break;
							}
							_animatedSprite.Animate(time, 16, 4, 200f);
							return;
						case 'E':
							if (!(text == "Elliott"))
							{
								break;
							}
							_animatedSprite.Animate(time, 33, 2, 800f);
							return;
						case 'V':
							if (!(text == "Vincent"))
							{
								break;
							}
							_animatedSprite.Animate(time, 18, 2, 600f);
							return;
						}
						break;
					case 5:
						switch (text[0])
						{
						case 'P':
							if (!(text == "Penny"))
							{
								break;
							}
							_animatedSprite.Animate(time, 18, 2, 1000f);
							return;
						case 'H':
							if (!(text == "Haley"))
							{
								break;
							}
							_animatedSprite.Animate(time, 26, 1, 200f);
							return;
						case 'E':
							if (!(text == "Emily"))
							{
								break;
							}
							_animatedSprite.Animate(time, 16 + _characterSpriteRandomInt * 2, 2, 300f);
							return;
						case 'S':
							if (!(text == "Shane"))
							{
								if (!(text == "Sandy"))
								{
									break;
								}
								_animatedSprite.Animate(time, 16, 2, 200f);
								return;
							}
							_animatedSprite.Animate(time, 28, 2, 500f);
							return;
						case 'L':
							if (!(text == "Lewis"))
							{
								if (!(text == "Linus"))
								{
									break;
								}
								_animatedSprite.Animate(time, 22, 1, 200f);
								return;
							}
							_animatedSprite.Animate(time, 24, 1, 170f);
							return;
						case 'W':
							if (!(text == "Willy"))
							{
								break;
							}
							_animatedSprite.Animate(time, 28, 4, 200f);
							return;
						case 'R':
							if (!(text == "Robin"))
							{
								break;
							}
							_animatedSprite.Animate(time, 32, 2, 120f);
							return;
						case 'C':
							if (!(text == "Clint"))
							{
								break;
							}
							_animatedSprite.Animate(time, 39, 1, 200f);
							return;
						case 'D':
							if (!(text == "Dwarf"))
							{
								break;
							}
							_animatedSprite.Animate(time, 16, 4, 100f);
							return;
						}
						break;
					case 4:
						switch (text[0])
						{
						case 'M':
							if (!(text == "Maru"))
							{
								break;
							}
							_animatedSprite.Animate(time, 16, 8, 150f);
							return;
						case 'L':
							if (!(text == "Leah"))
							{
								break;
							}
							_animatedSprite.Animate(time, 16, 4, 200f);
							return;
						case 'A':
							if (!(text == "Alex"))
							{
								break;
							}
							_animatedSprite.Animate(time, 16, 8, 170f);
							return;
						case 'K':
							if (!(text == "Kent"))
							{
								break;
							}
							_animatedSprite.Animate(time, 16, 1, 200f);
							return;
						case 'J':
							if (!(text == "Jodi"))
							{
								break;
							}
							_animatedSprite.Animate(time, 16, 2, 200f);
							return;
						}
						break;
					case 3:
						switch (text[0])
						{
						case 'S':
							if (!(text == "Sam"))
							{
								break;
							}
							_animatedSprite.Animate(time, 20, 2, 300f);
							return;
						case 'J':
							if (!(text == "Jas"))
							{
								break;
							}
							_animatedSprite.Animate(time, 16, 4, 100f);
							return;
						case 'G':
							if (!(text == "Gus"))
							{
								break;
							}
							_animatedSprite.Animate(time, 18, 3, 200f);
							return;
						case 'P':
							if (!(text == "Pam"))
							{
								break;
							}
							_animatedSprite.Animate(time, 28, 2, 200f);
							return;
						}
						break;
					case 9:
						switch (text[0])
						{
						case 'S':
							if (!(text == "Sebastian"))
							{
								break;
							}
							_animatedSprite.Animate(time, 16, 8, 180f);
							return;
						case 'D':
							if (!(text == "Demetrius"))
							{
								break;
							}
							_animatedSprite.Animate(time, 30, 2, 200f);
							return;
						}
						break;
					case 6:
						switch (text[0])
						{
						case 'H':
							if (!(text == "Harvey"))
							{
								break;
							}
							_animatedSprite.Animate(time, 20, 2, 800f);
							return;
						case 'W':
							if (!(text == "Wizard"))
							{
								break;
							}
							_animatedSprite.Animate(time, 16, 1, 170f);
							return;
						case 'M':
							if (!(text == "Marnie"))
							{
								break;
							}
							_animatedSprite.Animate(time, 28, 4, 120f);
							return;
						case 'G':
							if (!(text == "George"))
							{
								break;
							}
							_animatedSprite.Animate(time, 16, 4, 400f);
							return;
						case 'P':
							if (!(text == "Pierre"))
							{
								break;
							}
							_animatedSprite.Animate(time, 23, 1, 200f);
							return;
						case 'K':
							if (!(text == "Krobus"))
							{
								break;
							}
							_animatedSprite.Animate(time, 20, 4, 200f);
							return;
						case 'E':
							if (!(text == "Evelyn"))
							{
								break;
							}
							_animatedSprite.Animate(time, 20, 1, 200f);
							return;
						}
						break;
					case 8:
						if (!(text == "Caroline"))
						{
							break;
						}
						_animatedSprite.Animate(time, 19, 1, 200f);
						return;
					}
				}
				_animatedSprite.AnimateDown(time, 2);
			}
			else
			{
				switch (_currentDirection)
				{
				case 0:
					_animatedSprite.AnimateUp(time, 2);
					break;
				case 2:
					_animatedSprite.AnimateDown(time, 2);
					break;
				case 3:
					_animatedSprite.AnimateLeft(time, 2);
					break;
				case 1:
					_animatedSprite.AnimateRight(time, 2);
					break;
				}
			}
		}

		public void SetupLayout()
		{
			int x = xPositionOnScreen + 64 - 12;
			int y = yPositionOnScreen + IClickableMenu.borderWidth;
			Rectangle rectangle = new Rectangle(x, y, 400, letterHeight * 4 - IClickableMenu.borderWidth * 2);
			Rectangle content_rectangle = new Rectangle(x, y, letterWidth * 4 - 64 - 12 - Game1.xEdge * 2, letterHeight * 4 - IClickableMenu.borderWidth * 2);
			content_rectangle.X += rectangle.Width;
			content_rectangle.Width -= rectangle.Width;
			_characterStatusDisplayBox = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			if (drawBackPanel)
			{
				rectangle.Y += 32;
				rectangle.Height -= 32;
			}
			_characterSpriteDrawPosition = new Vector2(rectangle.X + (rectangle.Width - Game1.nightbg.Width) / 2, rectangle.Y);
			characterSpriteBox = new Rectangle(xPositionOnScreen + 64 - 12 + (400 - Game1.nightbg.Width) / 2, yPositionOnScreen + IClickableMenu.borderWidth, Game1.nightbg.Width, Game1.nightbg.Height);
			previousCharacterButton.bounds.X = (int)_characterSpriteDrawPosition.X - 64 - previousCharacterButton.bounds.Width / 2;
			previousCharacterButton.bounds.Y = (int)_characterSpriteDrawPosition.Y + Game1.nightbg.Height / 2 - previousCharacterButton.bounds.Height / 2;
			nextCharacterButton.bounds.X = (int)_characterSpriteDrawPosition.X + Game1.nightbg.Width + 64 - nextCharacterButton.bounds.Width / 2;
			nextCharacterButton.bounds.Y = (int)_characterSpriteDrawPosition.Y + Game1.nightbg.Height / 2 - nextCharacterButton.bounds.Height / 2;
			rectangle.Y += Game1.daybg.Height + 32;
			rectangle.Height -= Game1.daybg.Height + 32;
			_characterNamePosition = new Vector2(rectangle.Center.X, rectangle.Top);
			rectangle.Y += 96;
			rectangle.Height -= 96;
			_heartDisplayPosition = new Vector2(rectangle.Center.X, rectangle.Top);
			if (_target is NPC)
			{
				rectangle.Y += 56;
				rectangle.Height -= 48;
				_birthdayHeadingDisplayPosition = new Vector2(rectangle.Center.X, rectangle.Top);
				if ((_target as NPC).birthday_Season.Value != null)
				{
					int seasonNumber = Utility.getSeasonNumber((_target as NPC).birthday_Season);
					if (seasonNumber >= 0)
					{
						rectangle.Y += 48;
						rectangle.Height -= 48;
						_birthdayDisplayPosition = new Vector2(rectangle.Center.X, rectangle.Top);
						string text = (_target as NPC).Birthday_Day + " " + Utility.getSeasonNameFromNumber(seasonNumber);
						rectangle.Y += 32;
						rectangle.Height -= 32;
					}
				}
				if (_status != "")
				{
					_statusDisplayPosition = new Vector2(rectangle.Center.X, rectangle.Top);
					rectangle.Y += 32;
					rectangle.Height -= 32;
				}
			}
			content_rectangle.Height -= 96;
			content_rectangle.Y -= 8;
			_giftLogHeadingDisplayPosition = new Vector2(content_rectangle.Center.X, content_rectangle.Top);
			content_rectangle.Y += 80;
			content_rectangle.Height -= 70;
			backButton.bounds.X = content_rectangle.Left + 64 - forwardButton.bounds.Width / 2;
			backButton.bounds.Y = content_rectangle.Top;
			forwardButton.bounds.X = content_rectangle.Right - 64 - forwardButton.bounds.Width / 2;
			forwardButton.bounds.Y = content_rectangle.Top;
			if (!drawBackPanel)
			{
				nextCharacterButton.bounds.Y = (previousCharacterButton.bounds.Y = forwardButton.bounds.Y);
			}
			content_rectangle.Width -= 250;
			content_rectangle.X += 125;
			_giftLogCategoryDisplayPosition = new Vector2(content_rectangle.Center.X, content_rectangle.Top);
			content_rectangle.Y += 64;
			content_rectangle.Height -= 64;
			float num = content_rectangle.Top;
			storedGiftLogRect = content_rectangle;
			_errorMessagePosition = new Vector2(content_rectangle.Center.X, content_rectangle.Center.Y);
			scrollArea = new MobileScrollbox(content_rectangle.X, content_rectangle.Y, content_rectangle.Width, content_rectangle.Height, content_rectangle.Height * 3, new Rectangle(content_rectangle.X, content_rectangle.Y + 8, content_rectangle.Width, height - content_rectangle.Y - 16));
			if (_profileItems.Count > 0)
			{
				for (int i = 0; i < _profileItems.Count; i++)
				{
					ProfileItem profileItem = _profileItems[i];
					if (profileItem.ShouldDraw())
					{
						num = profileItem.HandleLayout(num, content_rectangle);
					}
				}
			}
			scrollArea.setMaxYOffset((int)num - content_rectangle.Y - content_rectangle.Height);
		}

		public void updateLayout()
		{
			int yOffsetForScroll = scrollArea.getYOffsetForScroll();
			float draw_y = storedGiftLogRect.Top + yOffsetForScroll;
			if (_profileItems.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < _profileItems.Count; i++)
			{
				ProfileItem profileItem = _profileItems[i];
				if (profileItem.ShouldDraw())
				{
					draw_y = profileItem.HandleLayout(draw_y, storedGiftLogRect);
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.8f);
			if (drawBackPanel)
			{
				Texture2D texture = letterTexture;
				Rectangle destinationRectangle = new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height);
				_ = letterWidth;
				b.Draw(texture, destinationRectangle, new Rectangle(0, 0, letterWidth, letterHeight), Color.White);
				Game1.DrawBox(_characterStatusDisplayBox.X, _characterStatusDisplayBox.Y, _characterStatusDisplayBox.Width, _characterStatusDisplayBox.Height);
			}
			else
			{
				Texture2D texture2 = letterTexture;
				Rectangle destinationRectangle2 = new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height);
				_ = letterWidth;
				b.Draw(texture2, destinationRectangle2, new Rectangle(0, 0, 320, 180), Color.White);
			}
			b.Draw((Game1.timeOfDay >= 1900) ? Game1.nightbg : Game1.daybg, _characterSpriteDrawPosition, Color.White);
			Vector2 vector = new Vector2(0f, (32 - _animatedSprite.SpriteHeight) * 4);
			vector += _characterEntrancePosition * 4f;
			if (!(_target is Farmer) && _target is NPC)
			{
				_animatedSprite.draw(b, new Vector2(_characterSpriteDrawPosition.X + 32f + vector.X, _characterSpriteDrawPosition.Y + 32f + vector.Y), 0.8f);
				int friendshipHeartLevelForNPC = Game1.player.getFriendshipHeartLevelForNPC(_target.name);
				bool flag = SocialPage.isDatable(_target.name);
				bool flag2 = _friendship.IsMarried();
				bool flag3 = flag2 && SocialPage.isRoommateOfAnyone(_target.name);
				int num = Math.Max(10, Utility.GetMaximumHeartsForCharacter(_target));
				float num2 = _heartDisplayPosition.X - (float)(Math.Min(10, num) * 32 / 2);
				float num3 = ((num > 10) ? (-16f) : 0f);
				for (int i = 0; i < num; i++)
				{
					int x = ((i < friendshipHeartLevelForNPC) ? 211 : 218);
					if (flag && !_friendship.IsDating() && !flag2 && i >= 8)
					{
						x = 211;
					}
					if (i < 10)
					{
						b.Draw(Game1.mouseCursors, new Vector2(num2 + (float)(i * 32), _heartDisplayPosition.Y + num3), new Rectangle(x, 428, 7, 6), (flag && !_friendship.IsDating() && !flag2 && i >= 8) ? (Color.Black * 0.35f) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, new Vector2(num2 + (float)((i - 10) * 32), _heartDisplayPosition.Y + num3 + 32f), new Rectangle(x, 428, 7, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
					}
				}
			}
			if (_printedName.Length < _target.displayName.Length)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, "", (int)_characterNamePosition.X, (int)_characterNamePosition.Y, _printedName);
			}
			else
			{
				SpriteText.drawStringWithScrollCenteredAt(b, _target.displayName, (int)_characterNamePosition.X, (int)_characterNamePosition.Y);
			}
			if ((_target as NPC).birthday_Season.Value != null)
			{
				int seasonNumber = Utility.getSeasonNumber((_target as NPC).birthday_Season);
				if (seasonNumber >= 0)
				{
					SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_Birthday"), (int)_birthdayHeadingDisplayPosition.X, (int)_birthdayHeadingDisplayPosition.Y);
					string text = (_target as NPC).Birthday_Day + " " + Utility.getSeasonNameFromNumber(seasonNumber);
					b.DrawString(Game1.smallFont, text, new Vector2((0f - Game1.smallFont.MeasureString(text).X) / 2f + _birthdayDisplayPosition.X, _birthdayDisplayPosition.Y), Game1.textColor);
				}
				if (_status != "")
				{
					b.DrawString(Game1.smallFont, _status, new Vector2((0f - Game1.smallFont.MeasureString(_status).X) / 2f + _statusDisplayPosition.X, _statusDisplayPosition.Y), Game1.textColor);
				}
			}
			SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_GiftLog"), (int)_giftLogHeadingDisplayPosition.X, (int)_giftLogHeadingDisplayPosition.Y);
			Utility.drawMultiLineTextWithShadow(b, Game1.content.LoadString("Strings\\UI:" + itemCategories[_currentCategory].categoryName, _target.displayName), Game1.smallFont, new Vector2(backButton.bounds.X + backButton.bounds.Width + 24, backButton.bounds.Y + 20), forwardButton.bounds.X - (backButton.bounds.X + backButton.bounds.Width + 48), 300, Game1.textColor, centreY: false, actuallyDrawIt: true, drawShadows: true, centerX: true, bold: false, close: true, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 0.75f : 1f);
			bool flag4 = false;
			scrollArea.setUpForScrollBoxDrawing(b);
			if (_profileItems.Count > 0)
			{
				for (int j = 0; j < _profileItems.Count; j++)
				{
					ProfileItem profileItem = _profileItems[j];
					if (profileItem.ShouldDraw())
					{
						flag4 = true;
						profileItem.Draw(b);
					}
				}
			}
			scrollArea.finishScrollBoxDrawing(b);
			if (!flag4)
			{
				string text2 = Game1.content.LoadString("Strings\\UI:Profile_GiftLog_NoGiftsGiven");
				b.DrawString(Game1.smallFont, text2, new Vector2((0f - Game1.smallFont.MeasureString(text2).X) / 2f + _errorMessagePosition.X, _errorMessagePosition.Y), Game1.textColor);
			}
			foreach (ClickableTextureComponent clickableTextureComponent in _clickableTextureComponents)
			{
				clickableTextureComponent.draw(b);
			}
			base.draw(b);
			if (hoveredItem != null)
			{
				bool flag5 = true;
				if (Game1.options.snappyMenus && Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse && _hideTooltipTime > 0)
				{
					flag5 = false;
				}
				if (flag5)
				{
					IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem);
				}
			}
		}

		public void unload()
		{
			_socialPage = null;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			receiveLeftClick(x, y, playSound);
		}

		public void RegisterClickable(ClickableComponent clickable)
		{
			clickableProfileItems.Add(clickable);
		}

		public void UnregisterClickable(ClickableComponent clickable)
		{
			clickableProfileItems.Remove(clickable);
		}
	}
}
