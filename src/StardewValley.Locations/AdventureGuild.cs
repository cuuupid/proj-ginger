using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class AdventureGuild : GameLocation
	{
		private NPC Gil = new NPC(null, new Vector2(-1000f, -1000f), "AdventureGuild", 2, "Gil", datable: false, null, Game1.content.Load<Texture2D>("Portraits\\Gil"));

		private bool talkedToGil;

		public AdventureGuild()
		{
		}

		public AdventureGuild(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			switch ((map.GetLayer("Buildings").Tiles[tileLocation] != null) ? map.GetLayer("Buildings").Tiles[tileLocation].TileIndex : (-1))
			{
			case 1306:
				showMonsterKillList();
				return true;
			case 1291:
			case 1292:
			case 1355:
			case 1356:
			case 1357:
			case 1358:
				gil();
				return true;
			default:
				return base.checkAction(tileLocation, viewport, who);
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			talkedToGil = false;
			if (!Game1.player.mailReceived.Contains("guildMember"))
			{
				Game1.player.mailReceived.Add("guildMember");
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (!Game1.player.mailReceived.Contains("checkedMonsterBoard"))
			{
				float num = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(504f, 464f + num)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.064801f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(544f, 504f + num)), new Microsoft.Xna.Framework.Rectangle(175, 425, 12, 12), Color.White * 0.75f, 0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, 0.06481f);
			}
		}

		private string killListLine(string monsterType, int killCount, int target)
		{
			string sub = Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_" + monsterType);
			if (killCount == 0)
			{
				return Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat_None", killCount, target, sub) + "^";
			}
			if (killCount >= target)
			{
				return Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat_OverTarget", killCount, target, sub) + "^";
			}
			return Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat", killCount, target, sub) + "^";
		}

		public void showMonsterKillList()
		{
			if (!Game1.player.mailReceived.Contains("checkedMonsterBoard"))
			{
				Game1.player.mailReceived.Add("checkedMonsterBoard");
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_Header").Replace('\n', '^') + "^");
			int killCount = Game1.stats.getMonstersKilled("Green Slime") + Game1.stats.getMonstersKilled("Frost Jelly") + Game1.stats.getMonstersKilled("Sludge") + Game1.stats.getMonstersKilled("Tiger Slime");
			int killCount2 = Game1.stats.getMonstersKilled("Shadow Guy") + Game1.stats.getMonstersKilled("Shadow Shaman") + Game1.stats.getMonstersKilled("Shadow Brute") + Game1.stats.getMonstersKilled("Shadow Sniper");
			int killCount3 = Game1.stats.getMonstersKilled("Skeleton") + Game1.stats.getMonstersKilled("Skeleton Mage");
			int killCount4 = Game1.stats.getMonstersKilled("Rock Crab") + Game1.stats.getMonstersKilled("Lava Crab") + Game1.stats.getMonstersKilled("Iridium Crab");
			int killCount5 = Game1.stats.getMonstersKilled("Grub") + Game1.stats.getMonstersKilled("Fly") + Game1.stats.getMonstersKilled("Bug");
			int killCount6 = Game1.stats.getMonstersKilled("Bat") + Game1.stats.getMonstersKilled("Frost Bat") + Game1.stats.getMonstersKilled("Lava Bat") + Game1.stats.getMonstersKilled("Iridium Bat");
			int killCount7 = Game1.stats.getMonstersKilled("Duggy") + Game1.stats.getMonstersKilled("Magma Duggy");
			int monstersKilled = Game1.stats.getMonstersKilled("Dust Spirit");
			int monstersKilled2 = Game1.stats.getMonstersKilled("Mummy");
			int monstersKilled3 = Game1.stats.getMonstersKilled("Pepper Rex");
			int killCount8 = Game1.stats.getMonstersKilled("Serpent") + Game1.stats.getMonstersKilled("Royal Serpent");
			int killCount9 = Game1.stats.getMonstersKilled("Magma Sprite") + Game1.stats.getMonstersKilled("Magma Sparker");
			stringBuilder.Append(killListLine("Slimes", killCount, 1000));
			stringBuilder.Append(killListLine("VoidSpirits", killCount2, 150));
			stringBuilder.Append(killListLine("Bats", killCount6, 200));
			stringBuilder.Append(killListLine("Skeletons", killCount3, 50));
			stringBuilder.Append(killListLine("CaveInsects", killCount5, 125));
			stringBuilder.Append(killListLine("Duggies", killCount7, 30));
			stringBuilder.Append(killListLine("DustSprites", monstersKilled, 500));
			stringBuilder.Append(killListLine("RockCrabs", killCount4, 60));
			stringBuilder.Append(killListLine("Mummies", monstersKilled2, 100));
			stringBuilder.Append(killListLine("PepperRex", monstersKilled3, 50));
			stringBuilder.Append(killListLine("Serpent", killCount8, 250));
			stringBuilder.Append(killListLine("MagmaSprite", killCount9, 150));
			stringBuilder.Append(Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_Footer").Replace('\n', '^'));
			Game1.drawLetterMessage(stringBuilder.ToString());
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			Game1.changeMusicTrack("none");
		}

		public static bool areAllMonsterSlayerQuestsComplete()
		{
			int num = Game1.stats.getMonstersKilled("Green Slime") + Game1.stats.getMonstersKilled("Frost Jelly") + Game1.stats.getMonstersKilled("Sludge") + Game1.stats.getMonstersKilled("Tiger Slime");
			int num2 = Game1.stats.getMonstersKilled("Shadow Guy") + Game1.stats.getMonstersKilled("Shadow Shaman") + Game1.stats.getMonstersKilled("Shadow Brute") + Game1.stats.getMonstersKilled("Shadow Sniper");
			int num3 = Game1.stats.getMonstersKilled("Skeleton") + Game1.stats.getMonstersKilled("Skeleton Mage");
			int num4 = Game1.stats.getMonstersKilled("Rock Crab") + Game1.stats.getMonstersKilled("Lava Crab") + Game1.stats.getMonstersKilled("Iridium Crab");
			int num5 = Game1.stats.getMonstersKilled("Grub") + Game1.stats.getMonstersKilled("Fly") + Game1.stats.getMonstersKilled("Bug");
			int num6 = Game1.stats.getMonstersKilled("Bat") + Game1.stats.getMonstersKilled("Frost Bat") + Game1.stats.getMonstersKilled("Lava Bat") + Game1.stats.getMonstersKilled("Iridium Bat");
			int num7 = Game1.stats.getMonstersKilled("Duggy") + Game1.stats.getMonstersKilled("Magma Duggy");
			int monstersKilled = Game1.stats.getMonstersKilled("Metal Head");
			int monstersKilled2 = Game1.stats.getMonstersKilled("Stone Golem");
			int monstersKilled3 = Game1.stats.getMonstersKilled("Dust Spirit");
			int monstersKilled4 = Game1.stats.getMonstersKilled("Mummy");
			int monstersKilled5 = Game1.stats.getMonstersKilled("Pepper Rex");
			int num8 = Game1.stats.getMonstersKilled("Serpent") + Game1.stats.getMonstersKilled("Royal Serpent");
			int num9 = Game1.stats.getMonstersKilled("Magma Sprite") + Game1.stats.getMonstersKilled("Magma Sparker");
			if (num < 1000)
			{
				return false;
			}
			if (num2 < 150)
			{
				return false;
			}
			if (num3 < 50)
			{
				return false;
			}
			if (num5 < 125)
			{
				return false;
			}
			if (num6 < 200)
			{
				return false;
			}
			if (num7 < 30)
			{
				return false;
			}
			if (monstersKilled3 < 500)
			{
				return false;
			}
			if (num4 < 60)
			{
				return false;
			}
			if (monstersKilled4 < 100)
			{
				return false;
			}
			if (monstersKilled5 < 50)
			{
				return false;
			}
			if (num8 < 250)
			{
				return false;
			}
			if (num9 < 150)
			{
				return false;
			}
			return true;
		}

		public static bool willThisKillCompleteAMonsterSlayerQuest(string nameOfMonster)
		{
			int num = Game1.stats.getMonstersKilled("Green Slime") + Game1.stats.getMonstersKilled("Frost Jelly") + Game1.stats.getMonstersKilled("Sludge") + Game1.stats.getMonstersKilled("Tiger Slime");
			int num2 = Game1.stats.getMonstersKilled("Shadow Guy") + Game1.stats.getMonstersKilled("Shadow Shaman") + Game1.stats.getMonstersKilled("Shadow Brute") + Game1.stats.getMonstersKilled("Shadow Sniper");
			int num3 = Game1.stats.getMonstersKilled("Skeleton") + Game1.stats.getMonstersKilled("Skeleton Mage");
			int num4 = Game1.stats.getMonstersKilled("Rock Crab") + Game1.stats.getMonstersKilled("Lava Crab") + Game1.stats.getMonstersKilled("Iridium Crab");
			int num5 = Game1.stats.getMonstersKilled("Grub") + Game1.stats.getMonstersKilled("Fly") + Game1.stats.getMonstersKilled("Bug");
			int num6 = Game1.stats.getMonstersKilled("Bat") + Game1.stats.getMonstersKilled("Frost Bat") + Game1.stats.getMonstersKilled("Lava Bat") + Game1.stats.getMonstersKilled("Iridium Bat");
			int num7 = Game1.stats.getMonstersKilled("Duggy") + Game1.stats.getMonstersKilled("Magma Duggy");
			int monstersKilled = Game1.stats.getMonstersKilled("Metal Head");
			int monstersKilled2 = Game1.stats.getMonstersKilled("Stone Golem");
			int monstersKilled3 = Game1.stats.getMonstersKilled("Dust Spirit");
			int monstersKilled4 = Game1.stats.getMonstersKilled("Mummy");
			int monstersKilled5 = Game1.stats.getMonstersKilled("Pepper Rex");
			int num8 = Game1.stats.getMonstersKilled("Serpent") + Game1.stats.getMonstersKilled("Royal Serpent");
			int num9 = Game1.stats.getMonstersKilled("Magma Sprite") + Game1.stats.getMonstersKilled("Magma Sparker");
			int num10 = num + ((nameOfMonster.Equals("Green Slime") || nameOfMonster.Equals("Frost Jelly") || nameOfMonster.Equals("Sludge") || nameOfMonster.Equals("Tiger Slime")) ? 1 : 0);
			int num11 = num2 + ((nameOfMonster.Equals("Shadow Guy") || nameOfMonster.Equals("Shadow Shaman") || nameOfMonster.Equals("Shadow Brute") || nameOfMonster.Equals("Shadow Sniper")) ? 1 : 0);
			int num12 = num3 + ((nameOfMonster.Equals("Skeleton") || nameOfMonster.Equals("Skeleton Mage")) ? 1 : 0);
			int num13 = num4 + ((nameOfMonster.Equals("Rock Crab") || nameOfMonster.Equals("Lava Crab") || nameOfMonster.Equals("Iridium Crab")) ? 1 : 0);
			int num14 = num5 + ((nameOfMonster.Equals("Grub") || nameOfMonster.Equals("Fly") || nameOfMonster.Equals("Bug")) ? 1 : 0);
			int num15 = num6 + ((nameOfMonster.Equals("Bat") || nameOfMonster.Equals("Frost Bat") || nameOfMonster.Equals("Lava Bat")) ? 1 : 0);
			int num16 = num7 + (nameOfMonster.Contains("Duggy") ? 1 : 0);
			int num17 = monstersKilled + (nameOfMonster.Equals("Metal Head") ? 1 : 0);
			int num18 = monstersKilled2 + (nameOfMonster.Equals("Stone Golem") ? 1 : 0);
			int num19 = monstersKilled3 + (nameOfMonster.Equals("Dust Spirit") ? 1 : 0);
			int num20 = monstersKilled4 + (nameOfMonster.Equals("Mummy") ? 1 : 0);
			int num21 = monstersKilled5 + (nameOfMonster.Equals("Pepper Rex") ? 1 : 0);
			int num22 = num8 + (nameOfMonster.Contains("Serpent") ? 1 : 0);
			int num23 = num9 + ((nameOfMonster.Equals("Magma Sprite") || nameOfMonster.Equals("Magma Sparker")) ? 1 : 0);
			if (num < 1000 && num10 >= 1000 && !Game1.player.mailReceived.Contains("Gil_Slime Charmer Ring"))
			{
				return true;
			}
			if (num2 < 150 && num11 >= 150 && !Game1.player.mailReceived.Contains("Gil_Savage Ring"))
			{
				return true;
			}
			if (num3 < 50 && num12 >= 50 && !Game1.player.mailReceived.Contains("Gil_Skeleton Mask"))
			{
				return true;
			}
			if (num5 < 125 && num14 >= 125 && !Game1.player.mailReceived.Contains("Gil_Insect Head"))
			{
				return true;
			}
			if (num6 < 200 && num15 >= 200 && !Game1.player.mailReceived.Contains("Gil_Vampire Ring"))
			{
				return true;
			}
			if (num7 < 30 && num16 >= 30 && !Game1.player.mailReceived.Contains("Gil_Hard Hat"))
			{
				return true;
			}
			if (monstersKilled3 < 500 && num19 >= 500 && !Game1.player.mailReceived.Contains("Gil_Burglar's Ring"))
			{
				return true;
			}
			if (num4 < 60 && num13 >= 60 && !Game1.player.mailReceived.Contains("Gil_Crabshell Ring"))
			{
				return true;
			}
			if (monstersKilled4 < 100 && num20 >= 100 && !Game1.player.mailReceived.Contains("Gil_Arcane Hat"))
			{
				return true;
			}
			if (monstersKilled5 < 50 && num21 >= 50 && !Game1.player.mailReceived.Contains("Gil_Knight's Helmet"))
			{
				return true;
			}
			if (num8 < 250 && num22 >= 250 && !Game1.player.mailReceived.Contains("Gil_Napalm Ring"))
			{
				return true;
			}
			if (num9 < 150 && num23 >= 150 && !Game1.player.mailReceived.Contains("Gil_Telephone"))
			{
				return true;
			}
			return false;
		}

		public void onRewardCollected(Item item, Farmer who)
		{
			if (item != null && !who.hasOrWillReceiveMail("Gil_" + item.Name))
			{
				who.mailReceived.Add("Gil_" + item.Name);
			}
		}

		private void gil()
		{
			List<Item> list = new List<Item>();
			int num = Game1.stats.getMonstersKilled("Green Slime") + Game1.stats.getMonstersKilled("Frost Jelly") + Game1.stats.getMonstersKilled("Sludge") + Game1.stats.getMonstersKilled("Tiger Slime");
			int num2 = Game1.stats.getMonstersKilled("Shadow Guy") + Game1.stats.getMonstersKilled("Shadow Shaman") + Game1.stats.getMonstersKilled("Shadow Brute") + Game1.stats.getMonstersKilled("Shadow Sniper");
			int num3 = Game1.stats.getMonstersKilled("Skeleton") + Game1.stats.getMonstersKilled("Skeleton Mage");
			int num4 = Game1.stats.getMonstersKilled("Goblin Warrior") + Game1.stats.getMonstersKilled("Goblin Wizard");
			int num5 = Game1.stats.getMonstersKilled("Rock Crab") + Game1.stats.getMonstersKilled("Lava Crab") + Game1.stats.getMonstersKilled("Iridium Crab");
			int num6 = Game1.stats.getMonstersKilled("Grub") + Game1.stats.getMonstersKilled("Fly") + Game1.stats.getMonstersKilled("Bug");
			int num7 = Game1.stats.getMonstersKilled("Bat") + Game1.stats.getMonstersKilled("Frost Bat") + Game1.stats.getMonstersKilled("Lava Bat") + Game1.stats.getMonstersKilled("Iridium Bat");
			int num8 = Game1.stats.getMonstersKilled("Duggy") + Game1.stats.getMonstersKilled("Magma Duggy");
			int monstersKilled = Game1.stats.getMonstersKilled("Metal Head");
			int monstersKilled2 = Game1.stats.getMonstersKilled("Stone Golem");
			int monstersKilled3 = Game1.stats.getMonstersKilled("Dust Spirit");
			int monstersKilled4 = Game1.stats.getMonstersKilled("Mummy");
			int monstersKilled5 = Game1.stats.getMonstersKilled("Pepper Rex");
			int num9 = Game1.stats.getMonstersKilled("Serpent") + Game1.stats.getMonstersKilled("Royal Serpent");
			int num10 = Game1.stats.getMonstersKilled("Magma Sprite") + Game1.stats.getMonstersKilled("Magma Sparker");
			if (num >= 1000 && !Game1.player.mailReceived.Contains("Gil_Slime Charmer Ring"))
			{
				list.Add(new Ring(520));
			}
			if (num2 >= 150 && !Game1.player.mailReceived.Contains("Gil_Savage Ring"))
			{
				list.Add(new Ring(523));
			}
			if (num3 >= 50 && !Game1.player.mailReceived.Contains("Gil_Skeleton Mask"))
			{
				list.Add(new Hat(8));
			}
			if (num4 >= 50)
			{
				Game1.player.specialItems.Contains(9);
			}
			if (num5 >= 60 && !Game1.player.mailReceived.Contains("Gil_Crabshell Ring"))
			{
				list.Add(new Ring(810));
			}
			if (num6 >= 125 && !Game1.player.mailReceived.Contains("Gil_Insect Head"))
			{
				list.Add(new MeleeWeapon(13));
			}
			if (num7 >= 200 && !Game1.player.mailReceived.Contains("Gil_Vampire Ring"))
			{
				list.Add(new Ring(522));
			}
			if (num8 >= 30 && !Game1.player.mailReceived.Contains("Gil_Hard Hat"))
			{
				list.Add(new Hat(27));
			}
			if (monstersKilled >= 50)
			{
				Game1.player.specialItems.Contains(519);
			}
			if (monstersKilled2 >= 50)
			{
				Game1.player.specialItems.Contains(517);
			}
			if (monstersKilled3 >= 500 && !Game1.player.mailReceived.Contains("Gil_Burglar's Ring"))
			{
				list.Add(new Ring(526));
			}
			if (monstersKilled4 >= 100 && !Game1.player.mailReceived.Contains("Gil_Arcane Hat"))
			{
				list.Add(new Hat(60));
			}
			if (monstersKilled5 >= 50 && !Game1.player.mailReceived.Contains("Gil_Knight's Helmet"))
			{
				list.Add(new Hat(50));
			}
			if (num9 >= 250 && !Game1.player.mailReceived.Contains("Gil_Napalm Ring"))
			{
				list.Add(new Ring(811));
			}
			if (num10 >= 150 && !Game1.player.mailReceived.Contains("Gil_Telephone"))
			{
				Game1.addMail("Gil_Telephone", noLetter: true, sendToEveryone: true);
				Game1.drawDialogue(Gil, Game1.content.LoadString("Strings\\Locations:Gil_Telephone"));
				return;
			}
			foreach (Item item in list)
			{
				if (item is Object)
				{
					(item as Object).specialItem = true;
				}
			}
			if (list.Count > 0)
			{
				Game1.activeClickableMenu = new ItemGrabMenu(list, this)
				{
					behaviorOnItemGrab = onRewardCollected
				};
				return;
			}
			if (talkedToGil)
			{
				Game1.drawDialogue(Gil, Game1.content.LoadString("Characters\\Dialogue\\Gil:Snoring"));
			}
			else
			{
				Game1.drawDialogue(Gil, Game1.content.LoadString("Characters\\Dialogue\\Gil:ComeBackLater"));
			}
			talkedToGil = true;
		}
	}
}
