using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	public class MapPage : IClickableMenu
	{
		public const int region_desert = 1001;

		public const int region_farm = 1002;

		public const int region_backwoods = 1003;

		public const int region_busstop = 1004;

		public const int region_wizardtower = 1005;

		public const int region_marnieranch = 1006;

		public const int region_leahcottage = 1007;

		public const int region_samhouse = 1008;

		public const int region_haleyhouse = 1009;

		public const int region_townsquare = 1010;

		public const int region_harveyclinic = 1011;

		public const int region_generalstore = 1012;

		public const int region_blacksmith = 1013;

		public const int region_saloon = 1014;

		public const int region_manor = 1015;

		public const int region_museum = 1016;

		public const int region_elliottcabin = 1017;

		public const int region_sewer = 1018;

		public const int region_graveyard = 1019;

		public const int region_trailer = 1020;

		public const int region_alexhouse = 1021;

		public const int region_sciencehouse = 1022;

		public const int region_tent = 1023;

		public const int region_mines = 1024;

		public const int region_adventureguild = 1025;

		public const int region_quarry = 1026;

		public const int region_jojamart = 1027;

		public const int region_fishshop = 1028;

		public const int region_spa = 1029;

		public const int region_secretwoods = 1030;

		public const int region_ruinedhouse = 1031;

		public const int region_communitycenter = 1032;

		public const int region_sewerpipe = 1033;

		public const int region_railroad = 1034;

		public const int region_island = 1035;

		private string descriptionText = "";

		private string hoverText = "";

		private string playerLocationName;

		private Texture2D map;

		private int mapX;

		private int mapY;

		public List<ClickableComponent> points = new List<ClickableComponent>();

		public ClickableTextureComponent okButton;

		private bool drawPamHouseUpgrade;

		private bool drawMovieTheaterJoja;

		private bool drawMovieTheater;

		private bool drawIsland;

		private ClickableComponent _selectedLocation;

		private ClickableComponent _playerLocation;

		private float widthMod;

		private float heightMod;

		private Vector2 mapScale = new Vector2(3.4f, 3.4f);

		private int mapXCrop;

		private int infoX;

		private int infoWidth;

		private string headerString;

		private string bodyString;

		private int infoTextHeight;

		public MapPage(int x, int y, int width, int height, float wMod, float hMod)
			: base(x, y, width, height)
		{
			mapXCrop = 25;
			widthMod = wMod;
			heightMod = hMod;
			mapScale.Y = (mapScale.X = (float)(height - 32) / 180f);
			float num = mapScale.X * (float)(300 - 2 * mapXCrop);
			if (num / (float)width > 0.75f)
			{
				mapScale.X = ((float)width * 0.75f - 32f) / (float)(300 - 2 * mapXCrop);
			}
			else if (num / (float)width < 0.65f)
			{
				mapXCrop = 25 * (int)(num / (float)width);
				mapScale.X = ((float)width * 0.75f - 32f) / (float)(300 - 2 * mapXCrop);
			}
			okButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11059"), new Rectangle(xPositionOnScreen + width + 64, yPositionOnScreen + height - IClickableMenu.borderWidth - 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
			map = Game1.content.Load<Texture2D>("LooseSprites\\map");
			Vector2 vector = new Vector2((width - map.Bounds.Width) / 2, (float)(height - map.Bounds.Height) / (4f * mapScale.Y));
			mapX = (int)vector.X;
			mapY = (int)vector.Y + y;
			infoX = xPositionOnScreen + (int)((float)(300 - 2 * mapXCrop) * mapScale.X + 32f);
			infoWidth = xPositionOnScreen + width - infoX - 16;
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(-mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 0f * mapScale.Y), (int)(73f * mapScale.X), (int)(38f * mapScale.Y)), Game1.player.mailReceived.Contains("ccVault") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11062") : "???"));
			drawPamHouseUpgrade = Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade");
			drawMovieTheaterJoja = Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja");
			drawMovieTheater = Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater");
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(81 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 63f * mapScale.Y), (int)(47f * mapScale.X), (int)(33f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11064", Game1.player.farmName)));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(90 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 24f * mapScale.Y), (int)(47f * mapScale.X), (int)(33f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11065")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(129 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 56f * mapScale.Y), (int)(19f * mapScale.X), (int)(25f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11066")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(49 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 88f * mapScale.Y), (int)(9f * mapScale.X), (int)(19f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11067")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(105 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 98f * mapScale.Y), (int)(19f * mapScale.X), (int)(10f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11068") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11069")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(113 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 109f * mapScale.Y), (int)(8f * mapScale.X), (int)(6f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11070")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(153 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 99f * mapScale.Y), (int)(9f * mapScale.X), (int)(13f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11071") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11072")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(163 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 102f * mapScale.Y), (int)(10f * mapScale.X), (int)(9f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11073") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11074")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(168 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 85f * mapScale.Y), (int)(11f * mapScale.X), (int)(15f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11075")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(170 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 76f * mapScale.Y), (int)(4f * mapScale.X), (int)(8f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11076") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11077")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(174 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 74f * mapScale.Y), (int)(7f * mapScale.X), (int)(10f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11078") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11079") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11080")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(213 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 97f * mapScale.Y), (int)(10f * mapScale.X), (int)(9f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11081") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11082")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(179 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 88f * mapScale.Y), (int)(7f * mapScale.X), (int)(10f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11083") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11084")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(192 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 97f * mapScale.Y), (int)(11f * mapScale.X), (int)(14f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11085")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(223 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 104f * mapScale.Y), (int)(8f * mapScale.X), (int)(7f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11086") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11087")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(206 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 141f * mapScale.Y), (int)(7f * mapScale.X), (int)(5f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11088")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(174 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 112f * mapScale.Y), (int)(6f * mapScale.X), (int)(4f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11089")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(181 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 106f * mapScale.Y), (int)(10f * mapScale.X), (int)(8f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11090")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(195 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 90f * mapScale.Y), (int)(6f * mapScale.X), (int)(5f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11091")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(187 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 79f * mapScale.Y), (int)(9f * mapScale.X), (int)(9f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11092") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11093")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(183 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 37f * mapScale.Y), (int)(12f * mapScale.X), (int)(8f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11094") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11095") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11096")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(196 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 32f * mapScale.Y), (int)(3f * mapScale.X), (int)(4f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11097")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(220 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 24f * mapScale.Y), (int)(4f * mapScale.X), (int)(6f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11098")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(225 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 27f * mapScale.Y), (int)(8f * mapScale.X), (int)(9f * mapScale.Y)), (Game1.stats.DaysPlayed >= 5) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11099") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11100")) : "???"));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(242 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 29f * mapScale.Y), (int)(22f * mapScale.X), (int)(19f * mapScale.Y)), Game1.player.mailReceived.Contains("ccCraftsRoom") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11103") : "???"));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(211 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 152f * mapScale.Y), (int)(9f * mapScale.X), (int)(10f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11107") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11108")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(144 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 15f * mapScale.Y), (int)(12f * mapScale.X), (int)(9f * mapScale.Y)), Game1.isLocationAccessible("Railroad") ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11110") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11111")) : "???"));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(-mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 68f * mapScale.Y), (int)(49f * mapScale.X), (int)(44f * mapScale.Y)), Game1.player.mailReceived.Contains("beenToWoods") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11114") : "???"));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(65 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 143f * mapScale.Y), (int)(5f * mapScale.X), (int)(5f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11116")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(95 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 149f * mapScale.Y), (int)(6f * mapScale.X), (int)(8f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11118")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(161 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 16f * mapScale.Y), (int)(4f * mapScale.X), (int)(2f * mapScale.Y)), Game1.isLocationAccessible("Railroad") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11119") : "???"));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(182 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 163f * mapScale.Y), (int)(7f * mapScale.X), (int)(7f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11122")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(195 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 90f * mapScale.Y), (int)(6f * mapScale.X), (int)(5f * mapScale.Y)), Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade") ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.PamHouse") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.PamHouseHomeOf")) : Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11091")));
			hoverText = playerLocationName;
			setInfoText(hoverText);
			string text = "";
			text = ((Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater") && !Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja")) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:MovieTheater_Map") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MovieTheater_Hours")) : ((!Utility.HasAnyPlayerSeenEvent(191393)) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11105") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11106")) : Game1.content.LoadString("Strings\\StringsFromCSFiles:AbandonedJojaMart")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(218 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 70f * mapScale.Y), (int)(13f * mapScale.X), (int)(13f * mapScale.Y)), text)
			{
				myID = 1027,
				upNeighborID = 1025,
				leftNeighborID = 1021,
				downNeighborID = 1013
			});
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(211 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 152f * mapScale.Y), (int)(9f * mapScale.X), (int)(10f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11107") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11108"))
			{
				myID = 1028,
				upNeighborID = 1017,
				rightNeighborID = (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("Visited_Island") ? 1035 : (-1))
			});
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(144 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 15f * mapScale.Y), (int)(12f * mapScale.X), (int)(9f * mapScale.Y)), Game1.isLocationAccessible("Railroad") ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11110") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11111")) : "???")
			{
				myID = 1029,
				rightNeighborID = 1034,
				downNeighborID = 1003,
				leftNeighborID = 1001
			});
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(-mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 68f * mapScale.Y), (int)(49f * mapScale.X), (int)(44f * mapScale.Y)), Game1.player.mailReceived.Contains("beenToWoods") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11114") : "???")
			{
				myID = 1030,
				upNeighborID = 1001,
				rightNeighborID = 1005
			});
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(65 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 143f * mapScale.Y), (int)(5f * mapScale.X), (int)(5f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11116"))
			{
				myID = 1031,
				rightNeighborID = 1033,
				upNeighborID = 1005
			});
			if (Game1.MasterPlayer.hasOrWillReceiveMail("Visited_Island"))
			{
				drawIsland = true;
				points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(242 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 158f * mapScale.Y), (int)(36f * mapScale.X), (int)(24f * mapScale.Y)), Game1.content.LoadString("Strings\\StringsFromCSFiles:IslandName"))
				{
					myID = 1035,
					downNeighborID = -1,
					upNeighborID = 1013,
					leftNeighborID = 1028
				});
			}
			string text2 = "";
			text2 = ((!Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater") || !Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja")) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11117") : (Game1.content.LoadString("Strings\\StringsFromCSFiles:MovieTheater_Map") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MovieTheater_Hours")));
			points.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + 16) + (float)(173 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 51f * mapScale.Y), (int)(11f * mapScale.X), (int)(9f * mapScale.Y)), text2)
			{
				myID = 1032,
				downNeighborID = 1012,
				upNeighborID = 1022,
				leftNeighborID = 1004
			});
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(1002);
			snapCursorToCurrentSnappedComponent();
		}

		public Vector2 getPlayerMapPosition(Farmer player)
		{
			Vector2 result = new Vector2(-999f, -999f);
			if (player.currentLocation == null)
			{
				return result;
			}
			string text = player.currentLocation.Name;
			if (text.StartsWith("UndergroundMine") || text == "Mine")
			{
				text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11098");
				if (player.currentLocation is MineShaft && (player.currentLocation as MineShaft).mineLevel > 120 && (player.currentLocation as MineShaft).mineLevel != 77377)
				{
					text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11062");
				}
			}
			if (player.currentLocation is IslandLocation)
			{
				text = Game1.content.LoadString("Strings\\StringsFromCSFiles:IslandName");
			}
			string name = player.currentLocation.Name;
			if (name != null)
			{
				switch (name.Length)
				{
				case 8:
				{
					char c = name[0];
					if ((uint)c <= 72u)
					{
						if (c != 'F')
						{
							if (c != 'H' || !(name == "Hospital"))
							{
								break;
							}
							goto IL_0595;
						}
						if (name == "FishShop")
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11107");
						}
						break;
					}
					if (c != 'R')
					{
						if (c == 'S' && name == "SeedShop")
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11078") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11079") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11080");
						}
						break;
					}
					if (!(name == "Railroad"))
					{
						break;
					}
					goto IL_0707;
				}
				case 9:
				{
					char c = name[2];
					if ((uint)c <= 110u)
					{
						if (c == 'c')
						{
							if (name == "Backwoods")
							{
							}
							break;
						}
						if (c != 'n' || !(name == "SandyShop"))
						{
							break;
						}
					}
					else
					{
						if (c == 's')
						{
							if (name == "JoshHouse")
							{
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11092") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11093");
							}
							break;
						}
						if (c != 'u' || !(name == "SkullCave"))
						{
							break;
						}
					}
					goto IL_056b;
				}
				case 4:
				{
					char c = name[0];
					if (c != 'C')
					{
						if (c == 'T' && name == "Temp" && player.currentLocation.Map.Id.Contains("Town"))
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
						}
						break;
					}
					if (!(name == "Club"))
					{
						break;
					}
					goto IL_056b;
				}
				case 10:
				{
					char c = name[0];
					if ((uint)c <= 72u)
					{
						if (c != 'A')
						{
							if (c != 'H' || !(name == "HarveyRoom"))
							{
								break;
							}
							goto IL_0595;
						}
						if (name == "AnimalShop")
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11068");
						}
						break;
					}
					if (c != 'M')
					{
						if (c != 'S' || !(name == "SandyHouse"))
						{
							break;
						}
						goto IL_056b;
					}
					if (name == "ManorHouse")
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11085");
					}
					break;
				}
				case 11:
				{
					char c = name[0];
					if (c != 'T')
					{
						if (c != 'W' || !(name == "WizardHouse"))
						{
							break;
						}
						goto IL_062f;
					}
					if (name == "Trailer_Big")
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.PamHouse");
					}
					break;
				}
				case 14:
				{
					char c = name[0];
					if (c != 'A')
					{
						if (c != 'B' || !(name == "BathHouse_Pool"))
						{
							break;
						}
						goto IL_0644;
					}
					if (name == "AdventureGuild")
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11099");
					}
					break;
				}
				case 15:
				{
					char c = name[0];
					if (c != 'B')
					{
						if (c == 'C' && name == "CommunityCenter")
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11117");
						}
						break;
					}
					if (!(name == "BathHouse_Entry"))
					{
						break;
					}
					goto IL_0644;
				}
				case 13:
				{
					char c = name[0];
					if (c != 'S')
					{
						if (c != 'W' || !(name == "WitchWarpCave"))
						{
							break;
						}
						goto IL_0707;
					}
					if (!(name == "SebastianRoom"))
					{
						break;
					}
					goto IL_0687;
				}
				case 12:
				{
					char c = name[0];
					if (c != 'E')
					{
						if (c != 'S' || !(name == "ScienceHouse"))
						{
							break;
						}
						goto IL_0687;
					}
					if (name == "ElliottHouse")
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11088");
					}
					break;
				}
				case 5:
					if (name == "Woods")
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11114");
					}
					break;
				case 6:
					if (!(name == "Desert"))
					{
						break;
					}
					goto IL_056b;
				case 19:
					if (!(name == "WizardHouseBasement"))
					{
						break;
					}
					goto IL_062f;
				case 20:
					if (!(name == "BathHouse_MensLocker"))
					{
						break;
					}
					goto IL_0644;
				case 22:
					if (!(name == "BathHouse_WomensLocker"))
					{
						break;
					}
					goto IL_0644;
				case 16:
					{
						if (name == "ArchaeologyHouse")
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11086");
						}
						break;
					}
					IL_0707:
					text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11119");
					break;
					IL_0595:
					text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11076") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11077");
					break;
					IL_056b:
					text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11062");
					break;
					IL_0644:
					text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11110") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11111");
					break;
					IL_0687:
					text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11094") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11095");
					break;
					IL_062f:
					text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11067");
					break;
				}
			}
			foreach (ClickableComponent point in points)
			{
				string text2 = point.name.Replace(" ", "");
				int length = point.name.IndexOf(Environment.NewLine);
				int length2 = text2.IndexOf(Environment.NewLine);
				string value = text.Substring(0, text.Contains(Environment.NewLine) ? text.IndexOf(Environment.NewLine) : text.Length);
				if (point.name.Equals(text) || text2.Equals(text) || (point.name.Contains(Environment.NewLine) && (point.name.Substring(0, length).Equals(value) || text2.Substring(0, length2).Equals(value))))
				{
					result = new Vector2(point.bounds.Center.X, point.bounds.Center.Y);
					_playerLocation = point;
					if (player.IsLocalPlayer)
					{
						playerLocationName = (point.name.Contains(Environment.NewLine) ? point.name.Substring(0, point.name.IndexOf(Environment.NewLine)) : point.name);
					}
					return result;
				}
			}
			int tileX = player.getTileX();
			int tileY = player.getTileY();
			string text3 = player.currentLocation.name;
			if (text3 != null)
			{
				switch (text3.Length)
				{
				case 6:
				{
					char c = text3[0];
					if (c != 'F')
					{
						if (c != 'S')
						{
							if (c != 'T' || !(text3 == "Tunnel"))
							{
								break;
							}
							goto IL_0e1d;
						}
						if (text3 == "Saloon" && player.IsLocalPlayer)
						{
							playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11172");
						}
						break;
					}
					if (!(text3 == "Forest"))
					{
						break;
					}
					if (tileY > 51)
					{
						if (player.IsLocalPlayer)
						{
							playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11186");
						}
						result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(70 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 135f * mapScale.Y));
					}
					else if (tileX < 58)
					{
						if (player.IsLocalPlayer)
						{
							playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11186");
						}
						result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(63 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 104f * mapScale.Y));
					}
					else
					{
						if (player.IsLocalPlayer)
						{
							playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11188");
						}
						result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(63 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 104f * mapScale.Y));
					}
					break;
				}
				case 5:
				{
					char c = text3[0];
					if (c != 'B')
					{
						if (c != 'C' || !(text3 == "Cabin"))
						{
							break;
						}
						goto IL_0e8a;
					}
					if (text3 == "Beach")
					{
						if (player.IsLocalPlayer)
						{
							playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11174");
						}
						result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(202 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 141f * mapScale.Y));
					}
					break;
				}
				case 8:
				{
					char c = text3[6];
					if ((uint)c <= 105u)
					{
						if (c != 'e')
						{
							if (c != 'i' || !(text3 == "Mountain"))
							{
								break;
							}
							if (tileX < 38)
							{
								if (player.IsLocalPlayer)
								{
									playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11176");
								}
								result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(185 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 36f * mapScale.Y));
							}
							else if (tileX < 96)
							{
								if (player.IsLocalPlayer)
								{
									playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11177");
								}
								result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(220 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 38f * mapScale.Y));
							}
							else
							{
								if (player.IsLocalPlayer)
								{
									playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11178");
								}
								result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(253 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 40f * mapScale.Y));
							}
							break;
						}
						if (!(text3 == "Big Shed"))
						{
							break;
						}
					}
					else if (c != 'o')
					{
						if (c != 'r')
						{
							if (c != 'v' || !(text3 == "FarmCave"))
							{
								break;
							}
						}
						else if (!(text3 == "Big Barn"))
						{
							break;
						}
					}
					else if (!(text3 == "Big Coop"))
					{
						break;
					}
					goto IL_0e8a;
				}
				case 9:
				{
					char c = text3[0];
					if (c != 'B')
					{
						if (c != 'F' || !(text3 == "FarmHouse"))
						{
							break;
						}
						goto IL_0e8a;
					}
					if (!(text3 == "Backwoods"))
					{
						break;
					}
					goto IL_0e1d;
				}
				case 4:
					switch (text3[0])
					{
					case 'B':
						break;
					case 'C':
						goto IL_0b79;
					case 'S':
						goto IL_0b8f;
					case 'F':
						goto IL_0ba5;
					case 'T':
						goto IL_0bbb;
					default:
						goto end_IL_0915;
					}
					if (!(text3 == "Barn"))
					{
						break;
					}
					goto IL_0e8a;
				case 11:
				{
					char c = text3[7];
					if (c != 'B')
					{
						if (c != 'C')
						{
							if (c != 'u' || !(text3 == "Slime Hutch"))
							{
								break;
							}
						}
						else if (!(text3 == "Deluxe Coop"))
						{
							break;
						}
					}
					else if (!(text3 == "Deluxe Barn"))
					{
						break;
					}
					goto IL_0e8a;
				}
				case 10:
					{
						if (!(text3 == "Greenhouse"))
						{
							break;
						}
						goto IL_0e8a;
					}
					IL_0b79:
					if (!(text3 == "Coop"))
					{
						break;
					}
					goto IL_0e8a;
					IL_0b8f:
					if (!(text3 == "Shed"))
					{
						break;
					}
					goto IL_0e8a;
					IL_0e1d:
					result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(109 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 47f * mapScale.Y));
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11180");
					}
					break;
					IL_0ba5:
					if (!(text3 == "Farm"))
					{
						break;
					}
					goto IL_0e8a;
					IL_0bbb:
					if (!(text3 == "Town"))
					{
						if (!(text3 == "Temp") || !player.currentLocation.Map.Id.Contains("Town"))
						{
							break;
						}
						if (tileX > 84 && tileY < 68)
						{
							result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(225 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 81f * mapScale.Y));
							if (player.IsLocalPlayer)
							{
								playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
							}
						}
						else if (tileX > 80 && tileY >= 68)
						{
							result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(220 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 108f * mapScale.Y));
							if (player.IsLocalPlayer)
							{
								playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
							}
						}
						else if (tileY <= 42)
						{
							result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(178 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 64f * mapScale.Y));
							if (player.IsLocalPlayer)
							{
								playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
							}
						}
						else if (tileY > 42 && tileY < 76)
						{
							result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(175 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 88f * mapScale.Y));
							if (player.IsLocalPlayer)
							{
								playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
							}
						}
						else
						{
							result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(182 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 109f * mapScale.Y));
							if (player.IsLocalPlayer)
							{
								playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
							}
						}
					}
					else if (tileX > 84 && tileY < 68)
					{
						result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(225 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 81f * mapScale.Y));
						if (player.IsLocalPlayer)
						{
							playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
						}
					}
					else if (tileX > 80 && tileY >= 68)
					{
						result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(220 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 108f * mapScale.Y));
						if (player.IsLocalPlayer)
						{
							playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
						}
					}
					else if (tileY <= 42)
					{
						result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(178 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 64f * mapScale.Y));
						if (player.IsLocalPlayer)
						{
							playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
						}
					}
					else if (tileY > 42 && tileY < 76)
					{
						result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(175 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 88f * mapScale.Y));
						if (player.IsLocalPlayer)
						{
							playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
						}
					}
					else
					{
						result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(182 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 109f * mapScale.Y));
						if (player.IsLocalPlayer)
						{
							playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
						}
					}
					break;
					IL_0e8a:
					result = new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(96 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 72f * mapScale.Y));
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11064", player.farmName);
					}
					break;
					end_IL_0915:
					break;
				}
			}
			if (hoverText == null || hoverText == "")
			{
				hoverText = playerLocationName;
				setInfoText(hoverText);
			}
			return result;
		}

		private void setInfoText(string text)
		{
			if (text != null && text != "")
			{
				headerString = Utility.getHeaderFromString(text);
				bodyString = Utility.getBodyFromString(text, 1);
				int num = height;
				int num2 = yPositionOnScreen + 32;
				Vector2 position = new Vector2(width * 2, 0f);
				infoTextHeight = Utility.drawMultiLineTextWithShadow(null, headerString, Game1.dialogueFont, position, infoWidth - 16, 128, Game1.textColor, centreY: false, actuallyDrawIt: false);
				infoTextHeight += Utility.drawMultiLineTextWithShadow(null, bodyString, Game1.smallFont, position, infoWidth - 16, num - num2, Game1.textColor, centreY: false, actuallyDrawIt: false);
			}
		}

		private void drawInfoText(SpriteBatch b, string text, SpriteFont font)
		{
			if (text != null && text != "")
			{
				SpriteFont dialogueFont = Game1.dialogueFont;
				int num = height;
				int num2 = yPositionOnScreen + 32;
				num2 = Utility.drawMultiLineTextWithShadow(position: new Vector2(infoX + 8, yPositionOnScreen + (num - infoTextHeight) / 2), b: b, text: headerString, font: Game1.dialogueFont, width: infoWidth - 16, height: 128, col: Game1.textColor, centreY: false);
				Utility.drawMultiLineTextWithShadow(position: new Vector2(infoX + 8, num2), b: b, text: bodyString, font: Game1.smallFont, width: infoWidth - 16, height: num - num2, col: Game1.textColor, centreY: false);
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (_selectedLocation == null && (b == Buttons.DPadUp || b == Buttons.LeftThumbstickUp || b == Buttons.DPadDown || b == Buttons.LeftThumbstickDown || b == Buttons.DPadLeft || b == Buttons.LeftThumbstickLeft || b == Buttons.DPadRight || b == Buttons.LeftThumbstickRight))
			{
				Vector2 playerMapPosition = getPlayerMapPosition(Game1.player);
				Point value = new Point((int)playerMapPosition.X, (int)playerMapPosition.Y);
				for (int i = 0; i < points.Count; i++)
				{
					if (points[i].bounds.Contains(value))
					{
						SetSelectedLocation(points[i]);
						break;
					}
				}
				if (_selectedLocation == null)
				{
					SetSelectedLocation(points[0]);
				}
				return;
			}
			int num = 0;
			for (int j = 0; j < points.Count; j++)
			{
				if (points[j] == _selectedLocation)
				{
					num = j;
					break;
				}
			}
			switch (b)
			{
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
				num--;
				if (num < 0)
				{
					num = points.Count - 1;
				}
				SetSelectedLocation(points[num]);
				break;
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickRight:
				num++;
				if (num >= points.Count)
				{
					num = 0;
				}
				SetSelectedLocation(points[num]);
				break;
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (Game1.options.doesInputListContain(Game1.options.mapButton, key))
			{
				exitThisMenu();
			}
		}

		private void SetHovertext(ClickableComponent c)
		{
			Log.It("hovertext:" + hoverText + "..... c.name:" + c.name);
			if (c != null && hoverText != c.name)
			{
				hoverText = c.name;
				Game1.playSound("smallSelect");
				setInfoText(hoverText);
				string name = c.name;
				if (name == "Lonely Stone")
				{
					Game1.playSound("stoneCrack");
				}
			}
		}

		private void SetSelectedLocation(ClickableComponent c)
		{
			_selectedLocation = c;
			SetHovertext(c);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (okButton.containsPoint(x, y))
			{
				okButton.scale -= 0.25f;
				okButton.scale = Math.Max(0.75f, okButton.scale);
				(Game1.activeClickableMenu as GameMenu).changeTab(0);
			}
			foreach (ClickableComponent point in points)
			{
				if (point.containsPoint(x, y))
				{
					SetSelectedLocation(point);
				}
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			receiveLeftClick(x, y);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
		}

		protected virtual void drawMiniPortraits(SpriteBatch b)
		{
			Dictionary<Vector2, int> dictionary = new Dictionary<Vector2, int>();
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				Vector2 vector = getPlayerMapPosition(onlineFarmer) - new Vector2(32f, 32f);
				int value = 0;
				dictionary.TryGetValue(vector, out value);
				dictionary[vector] = value + 1;
				vector += new Vector2(48 * (value % 2), 48 * (value / 2));
				onlineFarmer.FarmerRenderer.drawMiniPortrat(b, vector, 0.00011f, 4f, 2, onlineFarmer);
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (!TutorialManager.Instance.mapHasBeenSeen)
			{
				TutorialManager.Instance.mapHasBeenSeen = true;
				TutorialManager.Instance.completeTutorial(tutorialType.DUMMY_MAP);
			}
			try
			{
				IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
			}
			catch (Exception)
			{
			}
			try
			{
				IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, (int)((float)(300 - 2 * mapXCrop) * mapScale.X + 32f), height, Color.White);
			}
			catch (Exception)
			{
			}
			b.Draw(map, Utility.To4(new Vector2(xPositionOnScreen + 16, yPositionOnScreen + 16)), new Rectangle(mapXCrop, 0, 300 - 2 * mapXCrop, 180), Color.White, 0f, Vector2.Zero, mapScale, SpriteEffects.None, 0.86f);
			switch (Game1.whichFarm)
			{
			case 1:
				b.Draw(map, Utility.To4(new Vector2(xPositionOnScreen + 16, (float)(yPositionOnScreen + 16) + 43f * mapScale.Y)), new Rectangle(mapXCrop, 180, 131 - mapXCrop, 61), Color.White, 0f, Vector2.Zero, mapScale, SpriteEffects.None, 0.861f);
				break;
			case 2:
				b.Draw(map, Utility.To4(new Vector2(xPositionOnScreen + 16, (float)(yPositionOnScreen + 16) + 43f * mapScale.Y)), new Rectangle(131 + mapXCrop, 180, 131 - mapXCrop, 61), Color.White, 0f, Vector2.Zero, mapScale, SpriteEffects.None, 0.861f);
				break;
			case 3:
				b.Draw(map, Utility.To4(new Vector2(xPositionOnScreen + 16, (float)(yPositionOnScreen + 16) + 43f * mapScale.Y)), new Rectangle(mapXCrop, 241, 131 - mapXCrop, 61), Color.White, 0f, Vector2.Zero, mapScale, SpriteEffects.None, 0.861f);
				break;
			case 4:
				b.Draw(map, Utility.To4(new Vector2(xPositionOnScreen + 16, (float)(yPositionOnScreen + 16) + 43f * mapScale.Y)), new Rectangle(131 + mapXCrop, 241, 131 - mapXCrop, 61), Color.White, 0f, Vector2.Zero, mapScale, SpriteEffects.None, 0.861f);
				break;
			case 5:
				b.Draw(map, Utility.To4(new Vector2(xPositionOnScreen + 16, (float)(yPositionOnScreen + 16) + 43f * mapScale.Y)), new Rectangle(mapXCrop, 302, 131 - mapXCrop, 61), Color.White, 0f, Vector2.Zero, mapScale, SpriteEffects.None, 0.861f);
				break;
			case 6:
				b.Draw(map, Utility.To4(new Vector2(xPositionOnScreen + 16, (float)(yPositionOnScreen + 16) + 43f * mapScale.Y)), new Rectangle(131 + mapXCrop, 302, 131 - mapXCrop, 61), Color.White, 0f, Vector2.Zero, mapScale, SpriteEffects.None, 0.861f);
				break;
			case 7:
				if (Game1.whichModFarm != null && Game1.whichModFarm.WorldMapTexture != null)
				{
					Texture2D texture = Game1.content.Load<Texture2D>(Game1.whichModFarm.WorldMapTexture);
					b.Draw(texture, Utility.To4(new Vector2(xPositionOnScreen + 16, (float)(yPositionOnScreen + 16) + 43f * mapScale.Y)), null, Color.White, 0f, Vector2.Zero, mapScale, SpriteEffects.None, 0.861f);
				}
				else
				{
					b.Draw(map, Utility.To4(new Vector2(xPositionOnScreen + 16, (float)(yPositionOnScreen + 16) + 43f * mapScale.Y)), new Rectangle(mapXCrop, 180, 131 - mapXCrop, 61), Color.White, 0f, Vector2.Zero, mapScale, SpriteEffects.None, 0.861f);
				}
				break;
			}
			if (drawPamHouseUpgrade)
			{
				b.Draw(map, new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(195 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 87f * mapScale.Y)), new Rectangle(263, 181, 8, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.861f);
			}
			if (drawMovieTheater)
			{
				b.Draw(map, new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(213 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 70f * mapScale.Y)), new Rectangle(271, 181, 29, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.861f);
			}
			if (drawMovieTheaterJoja)
			{
				b.Draw(map, new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(171 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 48f * mapScale.Y)), new Rectangle(276, 181, 13, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.861f);
			}
			if (drawIsland)
			{
				b.Draw(map, new Vector2((int)((float)(xPositionOnScreen + 16) + (float)(242 - mapXCrop) * mapScale.X), (int)((float)(yPositionOnScreen + 16) + 158f * mapScale.Y)), new Rectangle(208, 363, 40, 30), Color.White, 0f, Vector2.Zero, new Vector2(0.825f * mapScale.X, 4f), SpriteEffects.None, 0.861f);
			}
			if (_selectedLocation != null)
			{
				IClickableMenu.DrawRedBox(b, _selectedLocation.bounds.X, _selectedLocation.bounds.Y, _selectedLocation.bounds.Width, _selectedLocation.bounds.Height, 8);
			}
			drawMiniPortraits(b);
			if (hoverText != null && !hoverText.Equals(""))
			{
				drawInfoText(b, hoverText, Game1.smallFont);
			}
			if (playerLocationName != null)
			{
				float num = yPositionOnScreen + height + 32 + 16;
				float num2 = num + 80f;
				if (num2 > (float)Game1.uiViewport.Height)
				{
					num -= num2 - (float)Game1.uiViewport.Height;
				}
				SpriteText.drawStringWithScrollCenteredAt(b, playerLocationName, xPositionOnScreen + width / 2, (int)num);
			}
		}
	}
}
