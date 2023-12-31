using System;
using System.Collections.Generic;
using StardewValley.Locations;

namespace StardewValley
{
	public class Stats
	{
		public uint seedsSown;

		public uint itemsShipped;

		public uint itemsCooked;

		public uint itemsCrafted;

		public uint chickenEggsLayed;

		public uint duckEggsLayed;

		public uint cowMilkProduced;

		public uint goatMilkProduced;

		public uint rabbitWoolProduced;

		public uint sheepWoolProduced;

		public uint cheeseMade;

		public uint goatCheeseMade;

		public uint trufflesFound;

		public uint stoneGathered;

		public uint rocksCrushed;

		public uint dirtHoed;

		public uint giftsGiven;

		public uint timesUnconscious;

		public uint averageBedtime;

		public uint timesFished;

		public uint fishCaught;

		public uint bouldersCracked;

		public uint stumpsChopped;

		public uint stepsTaken;

		public uint monstersKilled;

		public uint diamondsFound;

		public uint prismaticShardsFound;

		public uint otherPreciousGemsFound;

		public uint caveCarrotsFound;

		public uint copperFound;

		public uint ironFound;

		public uint coalFound;

		public uint coinsFound;

		public uint goldFound;

		public uint iridiumFound;

		public uint barsSmelted;

		public uint beveragesMade;

		public uint preservesMade;

		public uint piecesOfTrashRecycled;

		public uint mysticStonesCrushed;

		public uint daysPlayed;

		public uint weedsEliminated;

		public uint sticksChopped;

		public uint notesFound;

		public uint questsCompleted;

		public uint starLevelCropsShipped;

		public uint cropsShipped;

		public uint itemsForaged;

		public uint slimesKilled;

		public uint geodesCracked;

		public uint goodFriends;

		public uint totalMoneyGifted;

		public uint individualMoneyEarned;

		public SerializableDictionary<string, int> specificMonstersKilled = new SerializableDictionary<string, int>();

		public SerializableDictionary<string, uint> stat_dictionary = new SerializableDictionary<string, uint>();

		public uint GoodFriends
		{
			get
			{
				return goodFriends;
			}
			set
			{
				goodFriends = value;
			}
		}

		public uint CropsShipped
		{
			get
			{
				return cropsShipped;
			}
			set
			{
				cropsShipped = value;
			}
		}

		public uint ItemsForaged
		{
			get
			{
				return itemsForaged;
			}
			set
			{
				itemsForaged = value;
			}
		}

		public uint GeodesCracked
		{
			get
			{
				return geodesCracked;
			}
			set
			{
				geodesCracked = value;
			}
		}

		public uint SlimesKilled
		{
			get
			{
				return slimesKilled;
			}
			set
			{
				slimesKilled = value;
			}
		}

		public uint StarLevelCropsShipped
		{
			get
			{
				return starLevelCropsShipped;
			}
			set
			{
				starLevelCropsShipped = value;
				checkForStarCropsAchievements();
			}
		}

		public uint StoneGathered
		{
			get
			{
				return stoneGathered;
			}
			set
			{
				stoneGathered = value;
			}
		}

		public uint QuestsCompleted
		{
			get
			{
				return questsCompleted;
			}
			set
			{
				questsCompleted = value;
				checkForQuestAchievements();
			}
		}

		public uint FishCaught
		{
			get
			{
				return fishCaught;
			}
			set
			{
				fishCaught = value;
			}
		}

		public uint NotesFound
		{
			get
			{
				return notesFound;
			}
			set
			{
				notesFound = value;
			}
		}

		public uint SticksChopped
		{
			get
			{
				return sticksChopped;
			}
			set
			{
				sticksChopped = value;
			}
		}

		public uint WeedsEliminated
		{
			get
			{
				return weedsEliminated;
			}
			set
			{
				weedsEliminated = value;
			}
		}

		public uint DaysPlayed
		{
			get
			{
				return daysPlayed;
			}
			set
			{
				daysPlayed = value;
			}
		}

		public uint BouldersCracked
		{
			get
			{
				return bouldersCracked;
			}
			set
			{
				bouldersCracked = value;
			}
		}

		public uint MysticStonesCrushed
		{
			get
			{
				return mysticStonesCrushed;
			}
			set
			{
				mysticStonesCrushed = value;
			}
		}

		public uint GoatCheeseMade
		{
			get
			{
				return goatCheeseMade;
			}
			set
			{
				goatCheeseMade = value;
			}
		}

		public uint CheeseMade
		{
			get
			{
				return cheeseMade;
			}
			set
			{
				cheeseMade = value;
			}
		}

		public uint PiecesOfTrashRecycled
		{
			get
			{
				return piecesOfTrashRecycled;
			}
			set
			{
				piecesOfTrashRecycled = value;
			}
		}

		public uint PreservesMade
		{
			get
			{
				return preservesMade;
			}
			set
			{
				preservesMade = value;
			}
		}

		public uint BeveragesMade
		{
			get
			{
				return beveragesMade;
			}
			set
			{
				beveragesMade = value;
			}
		}

		public uint BarsSmelted
		{
			get
			{
				return barsSmelted;
			}
			set
			{
				barsSmelted = value;
			}
		}

		public uint IridiumFound
		{
			get
			{
				return iridiumFound;
			}
			set
			{
				iridiumFound = value;
			}
		}

		public uint GoldFound
		{
			get
			{
				return goldFound;
			}
			set
			{
				goldFound = value;
			}
		}

		public uint CoinsFound
		{
			get
			{
				return coinsFound;
			}
			set
			{
				coinsFound = value;
			}
		}

		public uint CoalFound
		{
			get
			{
				return coalFound;
			}
			set
			{
				coalFound = value;
			}
		}

		public uint IronFound
		{
			get
			{
				return ironFound;
			}
			set
			{
				ironFound = value;
			}
		}

		public uint CopperFound
		{
			get
			{
				return copperFound;
			}
			set
			{
				copperFound = value;
			}
		}

		public uint CaveCarrotsFound
		{
			get
			{
				return caveCarrotsFound;
			}
			set
			{
				caveCarrotsFound = value;
			}
		}

		public uint OtherPreciousGemsFound
		{
			get
			{
				return otherPreciousGemsFound;
			}
			set
			{
				otherPreciousGemsFound = value;
			}
		}

		public uint PrismaticShardsFound
		{
			get
			{
				return prismaticShardsFound;
			}
			set
			{
				prismaticShardsFound = value;
			}
		}

		public uint DiamondsFound
		{
			get
			{
				return diamondsFound;
			}
			set
			{
				diamondsFound = value;
			}
		}

		public uint MonstersKilled
		{
			get
			{
				return monstersKilled;
			}
			set
			{
				monstersKilled = value;
			}
		}

		public uint StepsTaken
		{
			get
			{
				return stepsTaken;
			}
			set
			{
				stepsTaken = value;
			}
		}

		public uint StumpsChopped
		{
			get
			{
				return stumpsChopped;
			}
			set
			{
				stumpsChopped = value;
			}
		}

		public uint TimesFished
		{
			get
			{
				return timesFished;
			}
			set
			{
				timesFished = value;
			}
		}

		public uint AverageBedtime
		{
			get
			{
				return averageBedtime;
			}
			set
			{
				averageBedtime = (averageBedtime * (daysPlayed - 1) + value) / Math.Max(1u, daysPlayed);
			}
		}

		public uint TimesUnconscious
		{
			get
			{
				return timesUnconscious;
			}
			set
			{
				timesUnconscious = value;
			}
		}

		public uint GiftsGiven
		{
			get
			{
				return giftsGiven;
			}
			set
			{
				giftsGiven = value;
			}
		}

		public uint DirtHoed
		{
			get
			{
				return dirtHoed;
			}
			set
			{
				dirtHoed = value;
			}
		}

		public uint RocksCrushed
		{
			get
			{
				return rocksCrushed;
			}
			set
			{
				rocksCrushed = value;
			}
		}

		public uint TrufflesFound
		{
			get
			{
				return trufflesFound;
			}
			set
			{
				trufflesFound = value;
			}
		}

		public uint SheepWoolProduced
		{
			get
			{
				return sheepWoolProduced;
			}
			set
			{
				sheepWoolProduced = value;
			}
		}

		public uint RabbitWoolProduced
		{
			get
			{
				return rabbitWoolProduced;
			}
			set
			{
				rabbitWoolProduced = value;
			}
		}

		public uint GoatMilkProduced
		{
			get
			{
				return goatMilkProduced;
			}
			set
			{
				goatMilkProduced = value;
			}
		}

		public uint CowMilkProduced
		{
			get
			{
				return cowMilkProduced;
			}
			set
			{
				cowMilkProduced = value;
			}
		}

		public uint DuckEggsLayed
		{
			get
			{
				return duckEggsLayed;
			}
			set
			{
				duckEggsLayed = value;
			}
		}

		public uint ItemsCrafted
		{
			get
			{
				return itemsCrafted;
			}
			set
			{
				itemsCrafted = value;
				checkForCraftingAchievements();
			}
		}

		public uint ChickenEggsLayed
		{
			get
			{
				return chickenEggsLayed;
			}
			set
			{
				chickenEggsLayed = value;
			}
		}

		public uint ItemsCooked
		{
			get
			{
				return itemsCooked;
			}
			set
			{
				itemsCooked = value;
			}
		}

		public uint ItemsShipped
		{
			get
			{
				return itemsShipped;
			}
			set
			{
				itemsShipped = value;
			}
		}

		public uint SeedsSown
		{
			get
			{
				return seedsSown;
			}
			set
			{
				seedsSown = value;
			}
		}

		public uint IndividualMoneyEarned
		{
			get
			{
				return individualMoneyEarned;
			}
			set
			{
				uint num = individualMoneyEarned;
				individualMoneyEarned = value;
				if (num < 1000000 && individualMoneyEarned >= 1000000)
				{
					Game1.multiplayer.globalChatInfoMessage("SoloEarned1mil_" + (Game1.player.isMale ? "Male" : "Female"), Game1.player.Name);
				}
				else if (num < 100000 && individualMoneyEarned >= 100000)
				{
					Game1.multiplayer.globalChatInfoMessage("SoloEarned100k_" + (Game1.player.isMale ? "Male" : "Female"), Game1.player.Name);
				}
				else if (num < 10000 && individualMoneyEarned >= 10000)
				{
					Game1.multiplayer.globalChatInfoMessage("SoloEarned10k_" + (Game1.player.isMale ? "Male" : "Female"), Game1.player.Name);
				}
				else if (num < 1000 && individualMoneyEarned >= 1000)
				{
					Game1.multiplayer.globalChatInfoMessage("SoloEarned1k_" + (Game1.player.isMale ? "Male" : "Female"), Game1.player.Name);
				}
			}
		}

		public uint getStat(string label)
		{
			if (stat_dictionary.ContainsKey(label))
			{
				return stat_dictionary[label];
			}
			return 0u;
		}

		public void incrementStat(string label, int amount)
		{
			if (stat_dictionary.ContainsKey(label))
			{
				stat_dictionary[label] += (uint)amount;
			}
			else
			{
				stat_dictionary.Add(label, (uint)amount);
			}
		}

		public void monsterKilled(string name)
		{
			if (specificMonstersKilled.ContainsKey(name))
			{
				if (AdventureGuild.willThisKillCompleteAMonsterSlayerQuest(name))
				{
					specificMonstersKilled[name]++;
					Game1.player.hasCompletedAllMonsterSlayerQuests.Value = AdventureGuild.areAllMonsterSlayerQuestsComplete();
					string value = name;
					if (Game1.content.Load<Dictionary<string, string>>("Data\\Monsters").TryGetValue(name, out value))
					{
						string[] array = value.Split('/');
						value = ((array.Length <= 14) ? name : array[14]);
					}
					else
					{
						value = name;
					}
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Stats.cs.5129"));
					Game1.multiplayer.globalChatInfoMessage("MonsterSlayer" + Game1.random.Next(4), Game1.player.Name, value);
					if (AdventureGuild.areAllMonsterSlayerQuestsComplete())
					{
						Game1.getSteamAchievement("Achievement_KeeperOfTheMysticRings");
					}
				}
				else
				{
					specificMonstersKilled[name]++;
				}
			}
			else
			{
				specificMonstersKilled.Add(name, 1);
			}
		}

		public int getMonstersKilled(string name)
		{
			if (specificMonstersKilled.ContainsKey(name))
			{
				return specificMonstersKilled[name];
			}
			return 0;
		}

		public void onMoneyGifted(uint amount)
		{
			uint num = totalMoneyGifted;
			totalMoneyGifted += amount;
			if (num <= 1000000 && totalMoneyGifted > 1000000)
			{
				Game1.multiplayer.globalChatInfoMessage("Gifted1mil", Game1.player.Name);
			}
			else if (num <= 100000 && totalMoneyGifted > 100000)
			{
				Game1.multiplayer.globalChatInfoMessage("Gifted100k", Game1.player.Name);
			}
			else if (num <= 10000 && totalMoneyGifted > 10000)
			{
				Game1.multiplayer.globalChatInfoMessage("Gifted10k", Game1.player.Name);
			}
			else if (num <= 1000 && totalMoneyGifted > 1000)
			{
				Game1.multiplayer.globalChatInfoMessage("Gifted1k", Game1.player.Name);
			}
		}

		public void takeStep()
		{
			StepsTaken++;
			if (StepsTaken == 10000)
			{
				Game1.multiplayer.globalChatInfoMessage("Walked10k", Game1.player.Name);
			}
			else if (StepsTaken == 100000)
			{
				Game1.multiplayer.globalChatInfoMessage("Walked100k", Game1.player.Name);
			}
			else if (StepsTaken == 1000000)
			{
				Game1.multiplayer.globalChatInfoMessage("Walked1m", Game1.player.Name);
			}
			else if (StepsTaken == 10000000)
			{
				Game1.multiplayer.globalChatInfoMessage("Walked10m", Game1.player.Name);
			}
		}

		public void checkForCookingAchievements()
		{
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\CookingRecipes");
			int num = 0;
			int num2 = 0;
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				if (Game1.player.cookingRecipes.ContainsKey(item.Key))
				{
					int key = Convert.ToInt32(item.Value.Split('/')[2].Split(' ')[0]);
					if (Game1.player.recipesCooked.ContainsKey(key))
					{
						num2 += Game1.player.recipesCooked[key];
						num++;
					}
				}
			}
			itemsCooked = (uint)num2;
			if (num == dictionary.Count)
			{
				Game1.getAchievement(17);
			}
			if (num >= 25)
			{
				Game1.getAchievement(16);
			}
			if (num >= 10)
			{
				Game1.getAchievement(15);
			}
		}

		public void checkForCraftingAchievements()
		{
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\CraftingRecipes");
			int num = 0;
			int num2 = 0;
			foreach (string key in dictionary.Keys)
			{
				if (!(key == "Wedding Ring") && Game1.player.craftingRecipes.ContainsKey(key))
				{
					num2 += Game1.player.craftingRecipes[key];
					if (Game1.player.craftingRecipes[key] > 0)
					{
						num++;
					}
				}
			}
			itemsCrafted = (uint)num2;
			if (num >= dictionary.Count - 1)
			{
				Game1.getAchievement(22);
			}
			if (num >= 30)
			{
				Game1.getAchievement(21);
			}
			if (num >= 15)
			{
				Game1.getAchievement(20);
			}
		}

		public void checkForShippingAchievements()
		{
			if (farmerShipped(24, 15) && farmerShipped(188, 15) && farmerShipped(190, 15) && farmerShipped(192, 15) && farmerShipped(248, 15) && farmerShipped(250, 15) && farmerShipped(252, 15) && farmerShipped(254, 15) && farmerShipped(256, 15) && farmerShipped(258, 15) && farmerShipped(260, 15) && farmerShipped(262, 15) && farmerShipped(264, 15) && farmerShipped(266, 15) && farmerShipped(268, 15) && farmerShipped(270, 15) && farmerShipped(272, 15) && farmerShipped(274, 15) && farmerShipped(276, 15) && farmerShipped(278, 15) && farmerShipped(280, 15) && farmerShipped(282, 15) && farmerShipped(284, 15) && farmerShipped(300, 15) && farmerShipped(304, 15) && farmerShipped(398, 15) && farmerShipped(400, 15) && farmerShipped(433, 15))
			{
				Game1.getAchievement(31);
			}
			if (farmerShipped(24, 300) || farmerShipped(188, 300) || farmerShipped(190, 300) || farmerShipped(192, 300) || farmerShipped(248, 300) || farmerShipped(250, 300) || farmerShipped(252, 300) || farmerShipped(254, 300) || farmerShipped(256, 300) || farmerShipped(258, 300) || farmerShipped(260, 300) || farmerShipped(262, 300) || farmerShipped(264, 300) || farmerShipped(266, 300) || farmerShipped(268, 300) || farmerShipped(270, 300) || farmerShipped(272, 300) || farmerShipped(274, 300) || farmerShipped(276, 300) || farmerShipped(278, 300) || farmerShipped(280, 300) || farmerShipped(282, 300) || farmerShipped(284, 300) || farmerShipped(454, 300) || farmerShipped(300, 300) || farmerShipped(304, 300) || (farmerShipped(398, 300) | farmerShipped(433, 300)) || farmerShipped(400, 300) || farmerShipped(591, 300) || farmerShipped(593, 300) || farmerShipped(595, 300) || farmerShipped(597, 300))
			{
				Game1.getAchievement(32);
			}
		}

		public void checkForStarCropsAchievements()
		{
			if (StarLevelCropsShipped >= 100)
			{
				Game1.getAchievement(77);
			}
		}

		private bool farmerShipped(int index, int number)
		{
			if (Game1.player.basicShipped.ContainsKey(index) && Game1.player.basicShipped[index] >= number)
			{
				return true;
			}
			return false;
		}

		public void checkForFishingAchievements()
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (KeyValuePair<int, string> item in Game1.objectInformation)
			{
				if (item.Value.Split('/')[3].Contains("Fish") && (item.Key < 167 || item.Key > 172) && (item.Key < 898 || item.Key > 902))
				{
					num3++;
					if (Game1.player.fishCaught.ContainsKey(item.Key))
					{
						num += Game1.player.fishCaught[item.Key][0];
						num2++;
					}
				}
			}
			fishCaught = (uint)num;
			if (num >= 100)
			{
				Game1.getAchievement(27);
			}
			if (num2 == num3)
			{
				Game1.getAchievement(26);
				if (!Game1.player.hasOrWillReceiveMail("CF_Fish"))
				{
					Game1.addMailForTomorrow("CF_Fish");
				}
			}
			if (num2 >= 24)
			{
				Game1.getAchievement(25);
			}
			if (num2 >= 10)
			{
				Game1.getAchievement(24);
			}
		}

		public void checkForArchaeologyAchievements()
		{
			int num = Game1.netWorldState.Value.MuseumPieces.Count();
			if (num >= 95)
			{
				Game1.getAchievement(5);
			}
			if (num >= 40)
			{
				Game1.getAchievement(28);
			}
		}

		public void checkForMoneyAchievements()
		{
			if (Game1.player.totalMoneyEarned >= 10000000)
			{
				Game1.getAchievement(4);
			}
			if (Game1.player.totalMoneyEarned >= 1000000)
			{
				Game1.getAchievement(3);
			}
			if (Game1.player.totalMoneyEarned >= 250000)
			{
				Game1.getAchievement(2);
			}
			if (Game1.player.totalMoneyEarned >= 50000)
			{
				Game1.getAchievement(1);
			}
			if (Game1.player.totalMoneyEarned >= 15000)
			{
				Game1.getAchievement(0);
			}
		}

		public void checkForBuildingUpgradeAchievements()
		{
			if (Game1.player.HouseUpgradeLevel == 2)
			{
				Game1.getAchievement(19);
			}
			if (Game1.player.HouseUpgradeLevel == 1)
			{
				Game1.getAchievement(18);
			}
		}

		public void checkForQuestAchievements()
		{
			if (QuestsCompleted >= 40)
			{
				Game1.getAchievement(30);
				Game1.addMailForTomorrow("quest35");
			}
			if (QuestsCompleted >= 10)
			{
				Game1.getAchievement(29);
				Game1.addMailForTomorrow("quest10");
			}
		}

		public void checkForFriendshipAchievements()
		{
			uint num = 0u;
			uint num2 = 0u;
			uint num3 = 0u;
			foreach (Friendship value in Game1.player.friendshipData.Values)
			{
				if (value.Points >= 2500)
				{
					num3++;
				}
				if (value.Points >= 2000)
				{
					num2++;
				}
				if (value.Points >= 1250)
				{
					num++;
				}
			}
			GoodFriends = num2;
			if (num >= 20)
			{
				Game1.getAchievement(13);
			}
			if (num >= 10)
			{
				Game1.getAchievement(12);
			}
			if (num >= 4)
			{
				Game1.getAchievement(11);
			}
			if (num >= 1)
			{
				Game1.getAchievement(6);
			}
			if (num3 >= 8)
			{
				Game1.getAchievement(9);
			}
			if (num3 >= 1)
			{
				Game1.getAchievement(7);
			}
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\CookingRecipes");
			foreach (string key in dictionary.Keys)
			{
				string[] array = dictionary[key].Split('/')[3].Split(' ');
				if (array[0].Equals("f") && Game1.player.friendshipData.ContainsKey(array[1]) && Game1.player.friendshipData[array[1]].Points >= Convert.ToInt32(array[2]) * 250 && !Game1.player.cookingRecipes.ContainsKey(key) && !Game1.player.hasOrWillReceiveMail(array[1] + "Cooking"))
				{
					Game1.addMailForTomorrow(array[1] + "Cooking");
				}
			}
			Dictionary<string, string> dictionary2 = Game1.content.Load<Dictionary<string, string>>("Data\\CraftingRecipes");
			foreach (string key2 in dictionary2.Keys)
			{
				string[] array2 = dictionary2[key2].Split('/')[4].Split(' ');
				if (array2[0].Equals("f") && Game1.player.friendshipData.ContainsKey(array2[1]) && Game1.player.friendshipData[array2[1]].Points >= Convert.ToInt32(array2[2]) * 250 && !Game1.player.craftingRecipes.ContainsKey(key2) && !Game1.player.hasOrWillReceiveMail(array2[1] + "Crafting"))
				{
					Game1.addMailForTomorrow(array2[1] + "Crafting");
				}
			}
		}

		public bool isSharedAchievement(int which)
		{
			if ((uint)which <= 5u || which == 28)
			{
				return true;
			}
			return false;
		}

		public void checkForAchievements()
		{
			checkForCookingAchievements();
			checkForCraftingAchievements();
			checkForShippingAchievements();
			checkForStarCropsAchievements();
			checkForFishingAchievements();
			checkForArchaeologyAchievements();
			checkForMoneyAchievements();
			checkForBuildingUpgradeAchievements();
			checkForQuestAchievements();
			checkForFriendshipAchievements();
			Game1.player.hasCompletedAllMonsterSlayerQuests.Value = AdventureGuild.areAllMonsterSlayerQuestsComplete();
		}
	}
}
