using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class CollectionsPage : IClickableMenu
	{
		public const int region_sideTabShipped = 7001;

		public const int region_sideTabFish = 7002;

		public const int region_sideTabArtifacts = 7003;

		public const int region_sideTabMinerals = 7004;

		public const int region_sideTabCooking = 7005;

		public const int region_sideTabAchivements = 7006;

		public const int region_sideTabSecretNotes = 7007;

		public const int region_sideTabLetters = 7008;

		public const int region_forwardButton = 707;

		public const int region_backButton = 706;

		public static int widthToMoveActiveTab = 8;

		public const int organicsTab = 0;

		public const int fishTab = 1;

		public const int archaeologyTab = 2;

		public const int mineralsTab = 3;

		public const int cookingTab = 4;

		public const int achievementsTab = 5;

		public const int secretNotesTab = 7;

		public const int lettersTab = 6;

		public const int distanceFromMenuBottomBeforeNewPage = 128;

		public LetterViewerMenu letterviewerSubMenu;

		private string descriptionText = "";

		private string hoverText = "";

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent forwardButton;

		public Dictionary<int, ClickableTextureComponent> sideTabs = new Dictionary<int, ClickableTextureComponent>();

		public int currentTab;

		public int currentPage;

		public int secretNoteImage = -1;

		public Dictionary<int, List<List<ClickableTextureComponent>>> collections = new Dictionary<int, List<List<ClickableTextureComponent>>>();

		private Dictionary<int, string> secretNotesData;

		private Texture2D secretNoteImageTexture;

		private bool changePanelHeight;

		private float widthMod;

		private float heightMod;

		private Rectangle mainBox;

		private Rectangle infoBox;

		private string headerText;

		private MobileScrollbar newScrollbar;

		private int numTabs;

		private int numInRow;

		private int numRows;

		private Rectangle[] mobSideTabs;

		private int xSpace = 128;

		private int ySpace = 128;

		private int[] col;

		private int[] row;

		private bool[] sliderVisible;

		private bool scrolling;

		private float[] sliderPercent;

		private string infoHeader = "";

		private ClickableTextureComponent[] currentlySelectedComponent;

		private ClickableTextureComponent highlightTexture;

		private MobileScrollbox scrollArea;

		private MobileScrollbox notesScrollArea;

		private int storedSecretPanelHeight;

		private int sideTabHeight;

		private int sideTabWidth;

		private int headerX;

		private int _selectedItemIndex;

		private int _lineNumber;

		private int value;

		private int selectedItemIndex
		{
			get
			{
				int num = 0;
				if (currentlySelectedComponent[currentTab] != null)
				{
					for (int i = 0; i < collections[currentTab].Count; i++)
					{
						for (int j = 0; j < collections[currentTab][i].Count; j++)
						{
							if (collections[currentTab][i][j] == currentlySelectedComponent[currentTab])
							{
								return num;
							}
							num++;
						}
					}
				}
				return 0;
			}
		}

		public CollectionsPage(int x, int y, int width, int height, float wMod, float hMod, int topTabX)
			: base(x, y, width, height)
		{
			numTabs = 6 + ((Game1.player.secretNotesSeen.Count > 0) ? 1 : 0);
			widthMod = wMod;
			heightMod = hMod;
			numTabs++;
			sideTabHeight = height / numTabs;
			sideTabWidth = 80;
			mobSideTabs = new Rectangle[numTabs];
			sliderVisible = new bool[numTabs];
			currentlySelectedComponent = new ClickableTextureComponent[numTabs];
			infoBox = new Rectangle(topTabX, y, x + width - topTabX, height);
			mainBox = new Rectangle(x + sideTabWidth, y, topTabX - (x + sideTabWidth) + 12, height);
			headerX = topTabX;
			headerText = Game1.content.LoadString("Strings\\UI:GameMenu_Collections");
			newScrollbar = new MobileScrollbar(infoBox.X - 44, mainBox.Y + 88, 1, mainBox.Height - 20 - 84, 0, 40);
			highlightTexture = new ClickableTextureComponent("", new Rectangle(0, 0, xSpace + 80, ySpace + 80), null, "", Game1.mobileSpriteSheet, new Rectangle(20, 96, 20, 20), 7.2f);
			scrollArea = new MobileScrollbox(mainBox.X + 16, mainBox.Y + 16, infoBox.X - 64 - mainBox.X - 24, mainBox.Height - 32, mainBox.Height - 32, new Rectangle(mainBox.X, mainBox.Y + 90, mainBox.Width - 16, mainBox.Height - 64 - 38), newScrollbar);
			for (int i = 0; i < numTabs; i++)
			{
				mobSideTabs[i] = new Rectangle(x, y + i * sideTabHeight, sideTabWidth, sideTabHeight);
			}
			mobSideTabs[numTabs - 1].Height = y + height - mobSideTabs[numTabs - 1].Y;
			sideTabs.Add(0, new ClickableTextureComponent("", new Rectangle(x + (sideTabWidth - 40) / 2, y + sideTabHeight - (sideTabHeight + 32) / 2, 40, 32), "", Game1.content.LoadString("Strings\\UI:Collections_Shipped"), Game1.mobileSpriteSheet, new Rectangle(0, 68, 10, 8), 4f)
			{
				myID = 7001
			});
			sideTabs.Add(1, new ClickableTextureComponent("", new Rectangle(x + (sideTabWidth - 44) / 2, y + 2 * sideTabHeight - (sideTabHeight + 28) / 2, 44, 28), "", Game1.content.LoadString("Strings\\UI:Collections_Fish"), Game1.mobileSpriteSheet, new Rectangle(10, 67, 11, 7), 4f)
			{
				myID = 7002
			});
			sideTabs.Add(2, new ClickableTextureComponent("", new Rectangle(x + (sideTabWidth - 28) / 2, y + 3 * sideTabHeight - (sideTabHeight + 32) / 2, 28, 32), "", Game1.content.LoadString("Strings\\UI:Collections_Artifacts"), Game1.mobileSpriteSheet, new Rectangle(21, 67, 7, 8), 4f)
			{
				myID = 7003
			});
			sideTabs.Add(3, new ClickableTextureComponent("", new Rectangle(x + (sideTabWidth - 40) / 2, y + 4 * sideTabHeight - (sideTabHeight + 32) / 2, 40, 32), "", Game1.content.LoadString("Strings\\UI:Collections_Minerals"), Game1.mobileSpriteSheet, new Rectangle(28, 67, 10, 8), 4f)
			{
				myID = 7004
			});
			sideTabs.Add(4, new ClickableTextureComponent("", new Rectangle(x + (sideTabWidth - 36) / 2, y + 5 * sideTabHeight - (sideTabHeight + 32) / 2, 36, 32), "", Game1.content.LoadString("Strings\\UI:Collections_Cooking"), Game1.mobileSpriteSheet, new Rectangle(38, 67, 9, 8), 4f)
			{
				myID = 7005
			});
			sideTabs.Add(5, new ClickableTextureComponent("", new Rectangle(x + (sideTabWidth - 36) / 2 - 4, y + 6 * sideTabHeight - (sideTabHeight + 36) / 2, 36, 32), "", Game1.content.LoadString("Strings\\UI:Collections_Achievements"), Game1.mobileSpriteSheet, new Rectangle(47, 67, 9, 9), 4f)
			{
				myID = 7006
			});
			sideTabs.Add(6, new ClickableTextureComponent("", new Rectangle(x + (sideTabWidth - 44) / 2, y + 7 * sideTabHeight - (sideTabHeight + 32) / 2, 36, 32), "", Game1.content.LoadString("Strings\\UI:Collections_Letters"), Game1.mobileSpriteSheet, new Rectangle(108, 67, 11, 8), 4f)
			{
				myID = 7008
			});
			collections.Add(0, new List<List<ClickableTextureComponent>>());
			collections.Add(1, new List<List<ClickableTextureComponent>>());
			collections.Add(2, new List<List<ClickableTextureComponent>>());
			collections.Add(3, new List<List<ClickableTextureComponent>>());
			collections.Add(4, new List<List<ClickableTextureComponent>>());
			collections.Add(5, new List<List<ClickableTextureComponent>>());
			collections.Add(6, new List<List<ClickableTextureComponent>>());
			if (Game1.player.secretNotesSeen.Count > 0)
			{
				sideTabs.Add(7, new ClickableTextureComponent("", new Rectangle(x + (sideTabWidth - 36) / 2, y + 8 * sideTabHeight - (sideTabHeight + 32) / 2, 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_SecretNotes"), Game1.mobileSpriteSheet, new Rectangle(98, 67, 9, 8), 4f));
				collections.Add(7, new List<List<ClickableTextureComponent>>());
				storedSecretPanelHeight = infoBox.Height + 64;
				notesScrollArea = new MobileScrollbox(infoBox.X, infoBox.Y, infoBox.Width, infoBox.Height, storedSecretPanelHeight, new Rectangle(infoBox.X + 16, infoBox.Y + 16, infoBox.Width - 32, infoBox.Height - 32));
			}
			if (collections[6].Count == 0)
			{
				collections[6].Add(new List<ClickableTextureComponent>());
			}
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
			int[] array = new int[sideTabs.Count];
			int num = xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
			numInRow = Math.Max(4, mainBox.Width / 128);
			numRows = (mainBox.Height - sideTabHeight - 32) / 128;
			int num2 = numInRow;
			col = new int[sideTabs.Count];
			row = new int[sideTabs.Count];
			sliderPercent = new float[sideTabs.Count];
			for (int j = 0; j < sideTabs.Count; j++)
			{
				col[j] = 0;
				row[j] = 0;
				sliderPercent[j] = 0f;
			}
			if (collections[6].Count == 0)
			{
				collections[6].Add(new List<ClickableTextureComponent>());
			}
			foreach (string item in Game1.player.mailReceived)
			{
				if (dictionary.ContainsKey(item))
				{
					int num3 = 128;
					int num4 = numInRow * num3;
					num = mainBox.X + (mainBox.Width - num4) / 2 + num3 / 4 - (sliderVisible[6] ? 12 : 0);
					int x2 = num + array[6] % num2 * num3;
					int y2 = 0;
					if (col[6] >= 1)
					{
						col[6] = 0;
						row[6]++;
						collections[6].Add(new List<ClickableTextureComponent>());
						array[6] = 0;
						x2 = num;
					}
					string[] array2 = dictionary[item].Split(new string[1] { "[#]" }, StringSplitOptions.None);
					collections[6].Last().Add(new ClickableTextureComponent(item + " true " + ((array2.Count() > 1) ? array2[1] : "???"), new Rectangle(x2, y2, 64, 64), null, "", Game1.mouseCursors, new Rectangle(190, 423, 14, 11), 4f, drawShadow: true));
					array[6]++;
					col[6]++;
				}
			}
			foreach (KeyValuePair<int, string> item2 in Game1.objectInformation)
			{
				string text = item2.Value.Split('/')[3];
				int num5 = 0;
				bool drawShadow = false;
				if (text.Contains("Arch"))
				{
					num5 = 2;
					if (Game1.player.archaeologyFound.ContainsKey(item2.Key))
					{
						drawShadow = true;
					}
				}
				else if (text.Contains("Fish"))
				{
					if ((item2.Key >= 167 && item2.Key < 173) || (item2.Key >= 898 && item2.Key <= 902))
					{
						continue;
					}
					num5 = 1;
					if (Game1.player.fishCaught.ContainsKey(item2.Key))
					{
						drawShadow = true;
					}
				}
				else if (text.Contains("Mineral") || text.Substring(text.Length - 3).Equals("-2"))
				{
					num5 = 3;
					if (Game1.player.mineralsFound.ContainsKey(item2.Key))
					{
						drawShadow = true;
					}
				}
				else if (text.Contains("Cooking") || text.Substring(text.Length - 3).Equals("-7"))
				{
					num5 = 4;
					if (Game1.player.recipesCooked.ContainsKey(item2.Key))
					{
						drawShadow = true;
					}
					if (item2.Key == 217 || item2.Key == 772 || item2.Key == 773 || item2.Key == 279 || item2.Key == 873)
					{
						continue;
					}
				}
				else
				{
					if (!Object.isPotentialBasicShippedCategory(item2.Key, text.Substring(text.Length - 3)))
					{
						continue;
					}
					num5 = 0;
					if (Game1.player.basicShipped.ContainsKey(item2.Key))
					{
						drawShadow = true;
					}
				}
				int num6 = 128;
				int num7 = numInRow * num6;
				num = mainBox.X + (mainBox.Width - num7) / 2 + num6 / 4 - (sliderVisible[num5] ? 12 : 0);
				int x3 = num + array[num5] % num2 * num6;
				int y3 = 0;
				if (col[num5] >= numInRow)
				{
					col[num5] = 0;
					row[num5]++;
					collections[num5].Add(new List<ClickableTextureComponent>());
					array[num5] = 0;
					x3 = num;
				}
				if (collections[num5].Count == 0)
				{
					collections[num5].Add(new List<ClickableTextureComponent>());
				}
				collections[num5].Last().Add(new ClickableTextureComponent(item2.Key + " " + drawShadow, new Rectangle(x3, y3, 64, 64), null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, item2.Key, 16, 16), 4f, drawShadow));
				array[num5]++;
				col[num5]++;
			}
			if (collections[5].Count == 0)
			{
				collections[5].Add(new List<ClickableTextureComponent>());
			}
			foreach (KeyValuePair<int, string> achievement in Game1.achievements)
			{
				bool flag = Game1.player.achievements.Contains(achievement.Key);
				string[] array3 = achievement.Value.Split('^');
				if (flag || (array3[2].Equals("true") && (array3[3].Equals("-1") || farmerHasAchievements(array3[3]))))
				{
					int num8 = 128;
					int num9 = numInRow * num8;
					num = mainBox.X + (mainBox.Width - num9) / 2 + num8 / 4 - (sliderVisible[5] ? 12 : 0);
					int x4 = num + array[5] % num2 * num8;
					int y4 = 0;
					if (col[5] >= numInRow)
					{
						col[5] = 0;
						row[5]++;
						collections[5].Add(new List<ClickableTextureComponent>());
						array[5] = 0;
						x4 = num;
					}
					if (collections[5].Count == 0)
					{
						collections[5].Add(new List<ClickableTextureComponent>());
					}
					collections[5].Last().Add(new ClickableTextureComponent(achievement.Key + " " + flag, new Rectangle(x4, y4, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25), 1f));
					array[5]++;
					col[5]++;
				}
			}
			if (Game1.player.secretNotesSeen.Count > 0)
			{
				secretNotesData = Game1.content.Load<Dictionary<int, string>>("Data\\SecretNotes");
				secretNoteImageTexture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\SecretNotesImages");
				bool flag2 = Game1.player.secretNotesSeen.Contains(GameLocation.JOURNAL_INDEX + 1);
				foreach (int key in secretNotesData.Keys)
				{
					if (key >= GameLocation.JOURNAL_INDEX)
					{
						if (!flag2)
						{
							continue;
						}
					}
					else if (!Game1.player.hasMagnifyingGlass)
					{
						continue;
					}
					int num10 = 128;
					int num11 = numInRow * num10;
					num = x + (sliderVisible[7] ? 18 : 40) + (mainBox.Width - num11 + num10) / 2;
					int x5 = num + array[7] % num2 * num10;
					int y5 = 0;
					if (col[7] >= numInRow)
					{
						col[7] = 0;
						row[7]++;
						collections[7].Add(new List<ClickableTextureComponent>());
						array[7] = 0;
						x5 = num;
					}
					if (collections[7].Count == 0)
					{
						collections[7].Add(new List<ClickableTextureComponent>());
					}
					collections[7].Last().Add(new ClickableTextureComponent(key + " " + Game1.player.secretNotesSeen.Contains(key), new Rectangle(x5, y5, 64, 64), null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (key >= GameLocation.JOURNAL_INDEX) ? 842 : 79, 16, 16), 4f, Game1.player.secretNotesSeen.Contains(key)));
					array[7]++;
					col[7]++;
				}
			}
			for (int k = 0; k < sideTabs.Count; k++)
			{
				if (row[k] >= numRows)
				{
					sliderVisible[k] = true;
					foreach (List<ClickableTextureComponent> item3 in collections[k])
					{
						foreach (ClickableTextureComponent item4 in item3)
						{
							item4.bounds.X -= 20;
						}
					}
				}
				else
				{
					sliderVisible[k] = false;
				}
				if (collections[k].Count <= 0)
				{
					continue;
				}
				if (k == 6)
				{
					if (collections[k].First().Count > 0)
					{
						currentlySelectedComponent[k] = collections[k].First().First();
					}
				}
				else
				{
					currentlySelectedComponent[k] = collections[k].First().First();
				}
			}
			currentTab = 0;
			if (currentlySelectedComponent[currentTab] != null)
			{
				if (currentTab == 5 || Convert.ToBoolean(currentlySelectedComponent[currentTab].name.Split(' ')[1]))
				{
					hoverText = createDescription(Convert.ToInt32(currentlySelectedComponent[currentTab].name.Split(' ')[0]));
				}
				else
				{
					hoverText = "???";
					infoHeader = "???";
					value = -1;
				}
			}
			newScrollbar.setPercentage(sliderPercent[0]);
			int num12 = (row[0] - numRows + 1) * 64 * 2;
			scrollArea.setMaxYOffset(num12);
			scrollArea.setYOffsetForScroll(-(int)(sliderPercent[0] * (float)num12 / 100f));
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			base.customSnapBehavior(direction, oldRegion, oldID);
			switch (direction)
			{
			case 2:
				if (currentPage > 0)
				{
					currentlySnappedComponent = getComponentWithID(706);
				}
				else if (currentPage == 0 && collections[currentTab].Count > 1)
				{
					currentlySnappedComponent = getComponentWithID(707);
				}
				backButton.upNeighborID = oldID;
				forwardButton.upNeighborID = oldID;
				break;
			case 3:
				if (oldID == 707 && currentPage > 0)
				{
					currentlySnappedComponent = getComponentWithID(706);
				}
				break;
			case 1:
				if (oldID == 706 && collections[currentTab].Count > currentPage + 1)
				{
					currentlySnappedComponent = getComponentWithID(707);
				}
				break;
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			base.snapToDefaultClickableComponent();
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		private bool farmerHasAchievements(string listOfAchievementNumbers)
		{
			string[] array = listOfAchievementNumbers.Split(' ');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!Game1.player.achievements.Contains(Convert.ToInt32(text)))
				{
					return false;
				}
			}
			return true;
		}

		public override void update(GameTime time)
		{
			if (sliderVisible[currentTab])
			{
				scrollArea.update(time);
			}
			if (currentTab == 7)
			{
				notesScrollArea.update(time);
			}
			if (letterviewerSubMenu == null)
			{
				return;
			}
			letterviewerSubMenu.update(time);
			if (letterviewerSubMenu.destroy)
			{
				letterviewerSubMenu = null;
				if (Game1.options.SnappyMenus)
				{
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.options.SnappyMenus)
			{
				return;
			}
			base.receiveLeftClick(x, y);
			if (letterviewerSubMenu != null)
			{
				letterviewerSubMenu.receiveLeftClick(x, y);
				return;
			}
			foreach (List<ClickableTextureComponent> item in collections[currentTab])
			{
				if (currentTab == 6)
				{
					Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
					foreach (ClickableTextureComponent item2 in item)
					{
						if (item2.containsPoint(x, y))
						{
							letterviewerSubMenu = new LetterViewerMenu(dictionary[item2.name.Split(' ')[0]], item2.name.Split(' ')[0], fromCollection: true);
						}
					}
					continue;
				}
				foreach (ClickableTextureComponent item3 in item)
				{
					if (item3.containsPoint(x, y))
					{
						secretNoteImage = -1;
						if (notesScrollArea != null)
						{
							notesScrollArea.setYOffsetForScroll(0);
						}
						Game1.playSound("smallSelect");
						currentlySelectedComponent[currentTab] = item3;
						ShowSelectectItemInfo(item3.name);
					}
				}
			}
			if (sliderVisible[currentTab])
			{
				scrollArea.receiveLeftClick(x, y);
				if (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y))
				{
					scrolling = true;
				}
			}
			if (currentTab == 7)
			{
				notesScrollArea.receiveLeftClick(x, y);
			}
			for (int i = 0; i < sideTabs.Count; i++)
			{
				if (mobSideTabs[i].Contains(x, y) && currentTab != i)
				{
					currentTab = i;
					OnChangeCollectionsTab();
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void receiveGamePadButton(Buttons b)
		{
			try
			{
				_lineNumber = 0;
				_selectedItemIndex = selectedItemIndex;
				_lineNumber = 1;
				int count = collections[currentTab][0].Count;
				_lineNumber = 2;
				int count2 = collections[currentTab].Count;
				_lineNumber = 3;
				int num = (count2 - 1) * count + collections[currentTab][count2 - 1].Count;
				_lineNumber = 4;
				if (b == Buttons.A)
				{
					currentTab++;
					if (currentTab >= numTabs)
					{
						currentTab = 0;
					}
					OnChangeCollectionsTab();
				}
				if (currentTab == 6 && letterviewerSubMenu != null)
				{
					letterviewerSubMenu.receiveGamePadButton(b);
				}
				if (b == Buttons.B && currentTab == 6)
				{
					letterviewerSubMenu = null;
				}
				_lineNumber = 5;
				if (b == Buttons.X && currentTab == 6 && currentlySelectedComponent[currentTab] != null)
				{
					_lineNumber = 6;
					Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
					ClickableTextureComponent clickableTextureComponent = collections[currentTab][_selectedItemIndex][0];
					_lineNumber = 7;
					letterviewerSubMenu = new LetterViewerMenu(dictionary[currentlySelectedComponent[currentTab].name.Split(' ')[0]], currentlySelectedComponent[currentTab].name.Split(' ')[0], fromCollection: true);
					_lineNumber = 8;
				}
				switch (b)
				{
				case Buttons.DPadUp:
				case Buttons.LeftThumbstickUp:
					_selectedItemIndex -= count;
					break;
				case Buttons.DPadDown:
				case Buttons.LeftThumbstickDown:
					_selectedItemIndex += count;
					break;
				case Buttons.DPadLeft:
				case Buttons.LeftThumbstickLeft:
					_selectedItemIndex--;
					break;
				case Buttons.DPadRight:
				case Buttons.LeftThumbstickRight:
					_selectedItemIndex++;
					break;
				}
				if (_selectedItemIndex < 0)
				{
					_selectedItemIndex = 0;
				}
				else if (_selectedItemIndex >= num)
				{
					_selectedItemIndex = num - 1;
				}
				int num2 = (int)Math.Floor((float)_selectedItemIndex / (float)count);
				int index = _selectedItemIndex - num2 * count;
				_lineNumber = 9;
				ClickableTextureComponent clickableTextureComponent2 = collections[currentTab][num2][index];
				_lineNumber = 10;
				currentlySelectedComponent[currentTab] = clickableTextureComponent2;
				_lineNumber = 11;
				ShowSelectectItemInfo(clickableTextureComponent2.name);
				_lineNumber = 12;
				int num3 = 128;
				int num4 = (int)Math.Floor((0f - (float)scrollArea.getYOffsetForScroll()) / (float)num3);
				int num5 = num4 + (int)Math.Floor((float)scrollArea.Bounds.Height / (float)num3) - 1;
				if (num2 >= num5)
				{
					int yOffsetForScroll = Math.Max(-scrollArea.maxYOffset, scrollArea.getYOffsetForScroll() - num3);
					scrollArea.setYOffsetForScroll(yOffsetForScroll);
				}
				else if (num2 <= num4)
				{
					int yOffsetForScroll2 = Math.Min(0, scrollArea.getYOffsetForScroll() + num3);
					scrollArea.setYOffsetForScroll(yOffsetForScroll2);
				}
			}
			catch (Exception)
			{
			}
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public string createDescription(int index)
		{
			string text = "";
			if (currentTab == 5)
			{
				string[] array = Game1.achievements[index].Split('^');
				infoHeader = array[0];
				text = array[1];
				value = -1;
			}
			else if (currentTab == 7)
			{
				if (secretNotesData != null)
				{
					infoHeader = Game1.content.LoadString("Strings\\Locations:Secret_Note_Name") + " #" + index;
					if (index >= GameLocation.JOURNAL_INDEX)
					{
						infoHeader = Game1.content.LoadString("Strings\\Locations:Journal_Name") + " #" + (index - GameLocation.JOURNAL_INDEX);
					}
					text = "";
					secretNoteImage = -1;
					if (secretNotesData[index][0] == '!')
					{
						text = " ";
						secretNoteImage = Convert.ToInt32(secretNotesData[index].Split(' ')[1]);
						changePanelHeight = true;
					}
					else
					{
						text += Utility.ParseGiftReveals(secretNotesData[index]).TrimStart(' ', '^').Replace("^", Environment.NewLine)
							.Replace("@", Game1.player.name);
						changePanelHeight = true;
					}
				}
			}
			else
			{
				string[] array2 = Game1.objectInformation[index].Split('/');
				string text2 = (infoHeader = array2[4]);
				text = text + array2[5] + Environment.NewLine + Environment.NewLine;
				if (array2[3].Contains("Arch"))
				{
					text += (Game1.player.archaeologyFound.ContainsKey(index) ? Game1.content.LoadString("Strings\\UI:Collections_Description_ArtifactsFound", Game1.player.archaeologyFound[index][0]) : "");
				}
				else if (array2[3].Contains("Cooking"))
				{
					text += (Game1.player.recipesCooked.ContainsKey(index) ? Game1.content.LoadString("Strings\\UI:Collections_Description_RecipesCooked", Game1.player.recipesCooked[index]) : "");
				}
				else if (!array2[3].Contains("Fish"))
				{
					text = ((!array2[3].Contains("Minerals") && !array2[3].Substring(array2[3].Length - 3).Equals("-2")) ? (text + Game1.content.LoadString("Strings\\UI:Collections_Description_NumberShipped", Game1.player.basicShipped.ContainsKey(index) ? Game1.player.basicShipped[index] : 0)) : (text + Game1.content.LoadString("Strings\\UI:Collections_Description_MineralsFound", Game1.player.mineralsFound.ContainsKey(index) ? Game1.player.mineralsFound[index] : 0)));
				}
				else
				{
					text += Game1.content.LoadString("Strings\\UI:Collections_Description_FishCaught", Game1.player.fishCaught.ContainsKey(index) ? Game1.player.fishCaught[index][0] : 0);
					if (Game1.player.fishCaught.ContainsKey(index) && Game1.player.fishCaught[index][1] > 0)
					{
						text = text + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Collections_Description_BiggestCatch", (LocalizedContentManager.CurrentLanguageCode != 0) ? Math.Round((double)Game1.player.fishCaught[index][1] * 2.54) : ((double)Game1.player.fishCaught[index][1]));
					}
				}
				value = Convert.ToInt32(array2[1]);
			}
			return text;
		}

		public void drawInfoPanel(SpriteBatch b)
		{
			if (!(hoverText != "") && !(infoHeader != ""))
			{
				return;
			}
			int num = 0;
			if (currentTab == 7)
			{
				notesScrollArea.setUpForScrollBoxDrawing(b);
				if (secretNoteImage != -1)
				{
					IClickableMenu.drawTextureBox(b, infoBox.X + (infoBox.Width - 256 - 32) / 2, infoBox.Y + (infoBox.Height - 256 - 32) / 2, 288, 288, Color.White);
					b.Draw(secretNoteImageTexture, new Vector2(infoBox.X + (infoBox.Width - 256 - 32) / 2 + 16, infoBox.Y + 16 + (infoBox.Height - 256 - 32) / 2), new Rectangle(secretNoteImage * 64 % secretNoteImageTexture.Width, secretNoteImage * 64 / secretNoteImageTexture.Width * 64, 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
				}
				else
				{
					num = notesScrollArea.getYOffsetForScroll();
				}
				int num2 = IClickableMenu.drawMobileTextPanel(b, hoverText, Game1.smallFont, infoBox.X + 8, infoBox.Y + 8 + num, infoBox.Width - 16, infoBox.Height - 16, 34, -1, infoHeader, -1, null, null, -1, -1, -1, -1, -1, 1f, null, -1, inStockAndBuyable: false, drawBackgroundBox: false, avoidOffscreenCull: true) - (infoBox.Y + 8 + num + 64);
				notesScrollArea.finishScrollBoxDrawing(b);
				if (changePanelHeight)
				{
					storedSecretPanelHeight = num2;
					notesScrollArea.setMaxYOffset(storedSecretPanelHeight - infoBox.Height);
					changePanelHeight = false;
				}
			}
			else
			{
				IClickableMenu.drawMobileTextPanel(b, hoverText, Game1.smallFont, infoBox.X + 8, infoBox.Y + 8 + num, infoBox.Width - 16, infoBox.Height - 16, 34, value, infoHeader);
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (letterviewerSubMenu != null)
			{
				if (Game1.activeClickableMenu is GameMenu)
				{
					(Game1.activeClickableMenu as GameMenu).invisible = true;
				}
				letterviewerSubMenu.draw(b);
				return;
			}
			if (Game1.activeClickableMenu is GameMenu)
			{
				(Game1.activeClickableMenu as GameMenu).invisible = false;
			}
			if (!TutorialManager.Instance.collectionsHasBeenSeen)
			{
				TutorialManager.Instance.collectionsHasBeenSeen = true;
				TutorialManager.Instance.completeTutorial(tutorialType.DUMMY_COLLECTIONS);
			}
			IClickableMenu.drawTextureBox(b, xPositionOnScreen, mainBox.Y, mainBox.Width + mainBox.X, mainBox.Height, Color.White);
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), infoBox.X, infoBox.Y, infoBox.Width, infoBox.Height, Color.White, 1f, drawShadow: false);
			b.Draw(Game1.mobileSpriteSheet, new Vector2(xPositionOnScreen, mainBox.Y), new Rectangle(68, 76, 1, 1), Color.White, 0f, Vector2.Zero, new Vector2(12f, mainBox.Height - 16), SpriteEffects.None, 0.865f);
			if (currentTab != 6)
			{
				drawInfoPanel(b);
			}
			for (int i = 0; i < numTabs; i++)
			{
				if (i != currentTab)
				{
					IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), mobSideTabs[i].X, mobSideTabs[i].Y, mobSideTabs[i].Width, mobSideTabs[i].Height, Color.DarkGray);
				}
			}
			Vector2 vector = Game1.dialogueFont.MeasureString(sideTabs[currentTab].hoverText);
			SpriteFont spriteFont = ((!(vector.X > (float)(mainBox.Width - 64))) ? Game1.dialogueFont : Game1.smallFont);
			int num = mainBox.X + Game1.xEdge / 2 + 32;
			int num2 = mainBox.Y + 8 + 16 + 4;
			int num3 = 128;
			int num4 = numInRow * num3;
			IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(76, 80, 10, 15), mobSideTabs[currentTab].X, mobSideTabs[currentTab].Y, mobSideTabs[currentTab].Width - 20, mobSideTabs[currentTab].Height, Color.White, 4f, drawShadow: false);
			if (currentTab != 0)
			{
				b.Draw(Game1.mobileSpriteSheet, new Vector2(mobSideTabs[currentTab].X + mobSideTabs[currentTab].Width - 24, mobSideTabs[currentTab].Y - 12), new Rectangle(85, 89, 6, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
				b.Draw(Game1.mobileSpriteSheet, new Vector2(mobSideTabs[currentTab].X + mobSideTabs[currentTab].Width - 24, mainBox.Y), new Rectangle(85, 85, 6, 1), Color.White, 0f, Vector2.Zero, new Vector2(4f, mobSideTabs[currentTab].Y - mainBox.Y), SpriteEffects.None, 0.865f);
				b.Draw(Game1.mobileSpriteSheet, new Vector2(mobSideTabs[currentTab].X + mobSideTabs[currentTab].Width - 12, mainBox.Y), new Rectangle(76, 80, 5, 5), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
			}
			else
			{
				b.Draw(Game1.mobileSpriteSheet, new Vector2(mobSideTabs[currentTab].X + mobSideTabs[currentTab].Width - 40, mobSideTabs[currentTab].Y), new Rectangle(79, 80, 1, 3), Color.White, 0f, Vector2.Zero, new Vector2(80f, 4f), SpriteEffects.None, 0.865f);
			}
			if (currentTab != numTabs - 1)
			{
				b.Draw(Game1.mobileSpriteSheet, new Vector2(mobSideTabs[currentTab].X + mobSideTabs[currentTab].Width - 24, mobSideTabs[currentTab].Y + mobSideTabs[currentTab].Height - 12), new Rectangle(85, 80, 6, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
				int num5 = mainBox.Height - (mobSideTabs[currentTab].Y + mobSideTabs[currentTab].Height) + yPositionOnScreen - 24;
				b.Draw(Game1.mobileSpriteSheet, new Vector2(mobSideTabs[currentTab].X + mobSideTabs[currentTab].Width - 24, mobSideTabs[currentTab].Y + mobSideTabs[currentTab].Height + 12), new Rectangle(85, 85, 6, 1), Color.White, 0f, Vector2.Zero, new Vector2(4f, num5), SpriteEffects.None, 0.865f);
				b.Draw(Game1.mobileSpriteSheet, new Vector2(mobSideTabs[currentTab].X + mobSideTabs[currentTab].Width - 12, mainBox.Y + mainBox.Height - 20), new Rectangle(76, 90, 5, 5), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
			}
			else
			{
				b.Draw(Game1.mobileSpriteSheet, new Vector2(mobSideTabs[currentTab].X + mobSideTabs[currentTab].Width - 40, mainBox.Y + mainBox.Height - 12), new Rectangle(81, 92, 1, 3), Color.White, 0f, Vector2.Zero, new Vector2(80f, 4f), SpriteEffects.None, 0.865f);
			}
			b.Draw(Game1.menuTexture, new Rectangle(num, (int)((float)(mainBox.Y + 4) + vector.Y + 32f), infoBox.X - 64 - num, 4), new Rectangle(44, 300, 4, 4), Color.White);
			b.DrawString(spriteFont, sideTabs[currentTab].hoverText, new Vector2(num, num2) + new Vector2(2f, 2f), Game1.textShadowColor);
			b.DrawString(spriteFont, sideTabs[currentTab].hoverText, new Vector2(num, num2) + new Vector2(0f, 2f), Game1.textShadowColor);
			b.DrawString(spriteFont, sideTabs[currentTab].hoverText, new Vector2(num, num2), Game1.textColor);
			scrollArea.setUpForScrollBoxDrawing(b);
			int num6 = numRows * 64 * 2;
			int num7 = (int)((float)num2 + vector.Y + 64f);
			for (int j = 0; j <= row[currentTab]; j++)
			{
				foreach (ClickableTextureComponent item in collections[currentTab][j])
				{
					bool flag = Convert.ToBoolean(item.name.Split(' ')[1]);
					item.bounds.Y = num7 + (sliderVisible[currentTab] ? scrollArea.getYOffsetForScroll() : 0) + j * 64 * 2;
					item.draw(b, flag ? Color.White : (Color.Black * 0.2f), 0.86f);
					if (currentTab == 6)
					{
						string text = Game1.parseText(item.name.Substring(item.name.IndexOf(' ', item.name.IndexOf(' ') + 1) + 1), Game1.smallFont, mainBox.Width - (item.bounds.X + item.bounds.Width - mainBox.X) * 2);
						b.DrawString(Game1.smallFont, text, new Vector2(item.bounds.X + item.bounds.Width + 16, item.bounds.Y) + new Vector2(2f, 10f), Game1.textShadowColor);
						b.DrawString(Game1.smallFont, text, new Vector2(item.bounds.X + item.bounds.Width + 16, item.bounds.Y) + new Vector2(0f, 10f), Game1.textShadowColor);
						b.DrawString(Game1.smallFont, text, new Vector2(item.bounds.X + item.bounds.Width + 16, item.bounds.Y) + new Vector2(0f, 8f), Game1.textColor);
					}
					if (currentTab == 5 && flag)
					{
						int num8 = new Random(Convert.ToInt32(item.name.Split(' ')[0])).Next(12);
						b.Draw(Game1.mouseCursors, new Vector2(item.bounds.X + 16 + 16, item.bounds.Y + 20 + 16), new Rectangle(256 + num8 % 6 * 64 / 2, 128 + num8 / 6 * 64 / 2, 32, 32), Color.White, 0f, new Vector2(16f, 16f), item.scale, SpriteEffects.None, 0.88f);
					}
				}
			}
			if (currentlySelectedComponent[currentTab] != null)
			{
				if (currentTab != 6)
				{
					highlightTexture.bounds.X = currentlySelectedComponent[currentTab].bounds.X - 32 - 8;
					highlightTexture.bounds.Y = currentlySelectedComponent[currentTab].bounds.Y - 32 - 8;
					highlightTexture.scale = 4f;
					highlightTexture.draw(b);
				}
				else if (Game1.options.gamepadControls)
				{
					highlightTexture.bounds.X = currentlySelectedComponent[currentTab].bounds.X - 32 - 8 - 4;
					highlightTexture.bounds.Y = currentlySelectedComponent[currentTab].bounds.Y - 32 - 8 - 8;
					highlightTexture.scale = 4f;
					highlightTexture.draw(b);
				}
			}
			scrollArea.finishScrollBoxDrawing(b);
			if (sliderVisible[currentTab])
			{
				newScrollbar.draw(b);
			}
			int num9 = 0;
			foreach (ClickableTextureComponent value in sideTabs.Values)
			{
				Color c = ((num9 == currentTab) ? Color.White : Color.DarkGray);
				value.draw(b, c, 0.865f);
				num9++;
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (letterviewerSubMenu != null)
			{
				letterviewerSubMenu.releaseLeftClick(x, y);
			}
			if (scrollArea != null && scrollArea != null)
			{
				_ = scrollArea.havePanelScrolled;
			}
			if (sliderVisible[currentTab])
			{
				scrollArea.releaseLeftClick(x, y);
			}
			scrolling = false;
			if (currentTab == 7)
			{
				notesScrollArea.releaseLeftClick(x, y);
			}
			_ = currentTab;
			_ = 6;
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (letterviewerSubMenu != null)
			{
				letterviewerSubMenu.leftClickHeld(x, y);
			}
			if (scrollArea != null)
			{
				scrollArea.leftClickHeld(x, y);
			}
			if (sliderVisible[currentTab] && scrolling && (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y)))
			{
				sliderPercent[currentTab] = newScrollbar.setY(y);
				if (sliderPercent[currentTab] < 0f)
				{
					sliderPercent[currentTab] = 0f;
				}
				if (sliderPercent[currentTab] > 100f)
				{
					sliderPercent[currentTab] = 100f;
				}
				scrollArea.setYOffsetForScroll(-(int)(sliderPercent[currentTab] * (float)scrollArea.getMaxYOffset() / 100f));
			}
			if (currentTab == 7)
			{
				notesScrollArea.leftClickHeld(x, y);
			}
		}

		private void OnChangeCollectionsTab()
		{
			secretNoteImage = -1;
			Game1.playSound("smallSelect");
			if (sliderVisible[currentTab])
			{
				newScrollbar.setPercentage(sliderPercent[currentTab]);
				int num = (row[currentTab] - numRows + 1) * 64 * 2;
				scrollArea.setMaxYOffset(num);
				scrollArea.setYOffsetForScroll(-(int)(sliderPercent[currentTab] * (float)num / 100f));
			}
			if (currentlySelectedComponent[currentTab] != null)
			{
				ShowSelectectItemInfo(currentlySelectedComponent[currentTab].name);
			}
			_selectedItemIndex = 0;
		}

		private void ShowSelectectItemInfo(string dataStr)
		{
			if (currentTab == 5 || Convert.ToBoolean(dataStr.Split(' ')[1]))
			{
				if (currentTab == 6)
				{
					hoverText = Game1.parseText(dataStr.Substring(dataStr.IndexOf(' ', dataStr.IndexOf(' ') + 1) + 1), Game1.smallFont, 256);
				}
				else
				{
					hoverText = createDescription(Convert.ToInt32(dataStr.Split(' ')[0]));
				}
			}
			else
			{
				hoverText = "???";
				infoHeader = "???";
				value = -1;
			}
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			if ((Game1.options.gamepadControls && direction == -1) || direction == 1)
			{
				direction *= 128;
			}
			if (currentTab == 7 && notesScrollArea != null)
			{
				notesScrollArea.receiveScrollWheelAction(direction);
			}
			else
			{
				scrollArea.receiveScrollWheelAction(direction);
			}
			if (currentlySelectedComponent[currentTab] != null)
			{
				if (currentlySelectedComponent[currentTab].bounds.Top < scrollArea.Bounds.Top + 128)
				{
					_selectedItemIndex += numInRow;
				}
				else if (currentlySelectedComponent[currentTab].bounds.Bottom > scrollArea.Bounds.Height)
				{
					_selectedItemIndex -= numInRow;
				}
				int count = collections[currentTab][0].Count;
				int num = (int)Math.Floor((float)_selectedItemIndex / (float)count);
				int index = _selectedItemIndex - num * count;
				ClickableTextureComponent clickableTextureComponent = collections[currentTab][num][index];
				currentlySelectedComponent[currentTab] = clickableTextureComponent;
				ShowSelectectItemInfo(clickableTextureComponent.name);
			}
		}
	}
}
