using Netcode;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Locations
{
	public class ShopLocation : GameLocation
	{
		public const int maxItemsToSellFromPlayer = 11;

		public readonly NetObjectList<Item> itemsFromPlayerToSell = new NetObjectList<Item>();

		public readonly NetObjectList<Item> itemsToStartSellingTomorrow = new NetObjectList<Item>();

		public ShopLocation()
		{
		}

		public ShopLocation(string map, string name)
			: base(map, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(itemsFromPlayerToSell, itemsToStartSellingTomorrow);
		}

		public virtual string getPurchasedItemDialogueForNPC(Object i, NPC n)
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
				result = ((!(Game1.random.NextDouble() < (double)(int)i.quality * 0.5 + 0.2)) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_1_QualityLow", text2, text3, i.DisplayName, Lexicon.getRandomNegativeFoodAdjective(n)) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_1_QualityHigh", text2, text3, i.DisplayName, Lexicon.getRandomDeliciousAdjective(n)));
				break;
			case 1:
				result = (((int)i.quality != 0) ? ((!n.Name.Equals("Jodi")) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityHigh", text2, text3, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityHigh_Jodi", text2, text3, i.DisplayName)) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityLow", text2, text3, i.DisplayName));
				break;
			case 2:
				if (n.Manners == 2)
				{
					if ((int)i.quality != 2)
					{
						result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_QualityLow_Rude", text2, text3, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomNegativeFoodAdjective(n), Lexicon.getRandomNegativeItemSlanderNoun());
					}
					else
					{
						Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_QualityHigh_Rude", text2, text3, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomSlightlyPositiveAdjectiveForEdibleNoun(n));
					}
				}
				else
				{
					Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_NonRude", text2, text3, i.DisplayName, i.salePrice() / 2);
				}
				break;
			case 3:
				result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_4", text2, text3, i.DisplayName);
				break;
			case 4:
				if (i.Category == -75 || i.Category == -79)
				{
					result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_5_VegetableOrFruit", text2, text3, i.DisplayName);
				}
				else if (i.Category == -7)
				{
					string randomPositiveAdjectiveForEventOrPerson = Lexicon.getRandomPositiveAdjectiveForEventOrPerson(n);
					result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_5_Cooking", text2, text3, i.DisplayName, Lexicon.getProperArticleForWord(randomPositiveAdjectiveForEventOrPerson), randomPositiveAdjectiveForEventOrPerson);
				}
				else
				{
					result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_5_Foraged", text2, text3, i.DisplayName);
				}
				break;
			}
			if (n.Age == 1 && Game1.random.NextDouble() < 0.6)
			{
				result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Teen", text2, text3, i.DisplayName);
			}
			string text4 = n.Name;
			if (text4 != null)
			{
				switch (text4.Length)
				{
				case 7:
					switch (text4[0])
					{
					case 'A':
						if (text4 == "Abigail")
						{
							result = (((int)i.quality != 0) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Abigail_QualityHigh", text2, text3, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Abigail_QualityLow", text2, text3, i.DisplayName, Lexicon.getRandomNegativeItemSlanderNoun()));
						}
						break;
					case 'E':
						if (text4 == "Elliott")
						{
							result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Elliott", text2, text3, i.DisplayName);
						}
						break;
					}
					break;
				case 4:
					switch (text4[0])
					{
					case 'A':
						if (text4 == "Alex")
						{
							result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Alex", text2, text3, i.DisplayName);
						}
						break;
					case 'L':
						if (text4 == "Leah")
						{
							result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Leah", text2, text3, i.DisplayName);
						}
						break;
					}
					break;
				case 8:
					if (text4 == "Caroline")
					{
						result = (((int)i.quality != 0) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Caroline_QualityHigh", text2, text3, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Caroline_QualityLow", text2, text3, i.DisplayName));
					}
					break;
				case 6:
					if (text4 == "Pierre")
					{
						result = (((int)i.quality != 0) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Pierre_QualityHigh", text2, text3, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Pierre_QualityLow", text2, text3, i.DisplayName));
					}
					break;
				case 5:
					if (text4 == "Haley")
					{
						result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Haley", text2, text3, i.DisplayName);
					}
					break;
				}
			}
			return result;
		}

		public override void DayUpdate(int dayOfMonth)
		{
			for (int num = itemsToStartSellingTomorrow.Count - 1; num >= 0; num--)
			{
				Item item = itemsToStartSellingTomorrow[num];
				if (itemsFromPlayerToSell.Count < 11)
				{
					bool flag = false;
					foreach (Item item2 in itemsFromPlayerToSell)
					{
						if (item2.Name.Equals(item.Name) && (item2 as Object).quality == (item as Object).quality)
						{
							item2.Stack += item.Stack;
							flag = true;
							break;
						}
					}
					itemsToStartSellingTomorrow.RemoveAt(num);
					if (!flag)
					{
						itemsFromPlayerToSell.Add(item);
					}
				}
			}
			base.DayUpdate(dayOfMonth);
		}
	}
}
