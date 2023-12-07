using StardewValley.BellsAndWhistles;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class FishShop : ShopLocation
	{
		public FishShop()
		{
		}

		public FishShop(string map, string name)
			: base(map, name)
		{
		}

		public override string getPurchasedItemDialogueForNPC(Object i, NPC n)
		{
			string result = "...";
			string[] array = Game1.content.LoadString("Strings\\Lexicon:GenericPlayerTerm").Split('^');
			string text = array[0];
			if (array.Length > 1 && !Game1.player.isMale)
			{
				text = array[1];
			}
			string text2 = ((Game1.random.NextDouble() < (double)(Game1.player.getFriendshipLevelForNPC(n.Name) / 1250)) ? Game1.player.Name : text);
			if (n.Age != 0)
			{
				text2 = Game1.player.Name;
			}
			string text3 = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? Lexicon.getProperArticleForWord(i.name) : "");
			if ((i.Category == -4 || i.Category == -75 || i.Category == -79) && Game1.random.NextDouble() < 0.5)
			{
				text3 = Game1.content.LoadString("Strings\\StringsFromCSFiles:SeedShop.cs.9701");
			}
			int num = Game1.random.Next(5);
			if (n.Manners == 2)
			{
				num = 2;
			}
			switch (num)
			{
			case 0:
			case 4:
				result = ((!(Game1.random.NextDouble() < (double)(int)i.quality * 0.5 + 0.2)) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_1_QualityLow_Willy", text2, text3, i.DisplayName, Lexicon.getRandomNegativeFoodAdjective(n)) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_1_QualityHigh_Willy", text2, text3, i.DisplayName, Lexicon.getRandomDeliciousAdjective(n)));
				break;
			case 1:
				result = (((int)i.quality != 0) ? ((!n.Name.Equals("Jodi")) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityHigh_Willy", text2, text3, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityHigh_Jodi_Willy", text2, text3, i.DisplayName)) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityLow_Willy", text2, text3, i.DisplayName));
				break;
			case 2:
				if (n.Manners == 2)
				{
					if ((int)i.quality != 2)
					{
						result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_QualityLow_Rude_Willy", text2, text3, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomNegativeFoodAdjective(n), Lexicon.getRandomNegativeItemSlanderNoun());
					}
					else
					{
						Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_QualityHigh_Rude_Willy", text2, text3, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomSlightlyPositiveAdjectiveForEdibleNoun(n));
					}
				}
				else
				{
					Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_NonRude_Willy", text2, text3, i.DisplayName, i.salePrice() / 2);
				}
				break;
			case 3:
				result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_4_Willy", text2, text3, i.DisplayName);
				break;
			}
			string text4 = n.Name;
			if (text4 == "Willy")
			{
				result = (((int)i.quality != 0) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Pierre_QualityHigh_Willy", text2, text3, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Pierre_QualityLow_Willy", text2, text3, i.DisplayName));
			}
			return result;
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action == "WarpBoatTunnel")
			{
				if (Game1.player.mailReceived.Contains("willyBackRoomInvitation"))
				{
					Game1.warpFarmer("BoatTunnel", 6, 12, flip: false);
					playSound("doorClose");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
				}
			}
			return base.performAction(action, who, tileLocation);
		}
	}
}
