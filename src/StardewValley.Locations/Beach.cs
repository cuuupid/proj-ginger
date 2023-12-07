using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Network;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class Beach : GameLocation
	{
		internal NPC oldMariner;

		[XmlElement("bridgeFixed")]
		public readonly NetBool bridgeFixed = new NetBool();

		private bool hasShownCCUpgrade;

		public Beach()
		{
		}

		public Beach(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(bridgeFixed);
			bridgeFixed.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && mapPath.Value != null)
				{
					fixBridge(this);
				}
			};
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (wasUpdated)
			{
				return;
			}
			base.UpdateWhenCurrentLocation(time);
			if (oldMariner != null)
			{
				oldMariner.update(time, this);
			}
			if (Game1.eventUp || !(Game1.random.NextDouble() < 1E-06))
			{
				return;
			}
			Vector2 position = new Vector2(Game1.random.Next(15, 47) * 64, Game1.random.Next(29, 42) * 64);
			bool flag = true;
			for (float num = position.Y / 64f; num < (float)map.GetLayer("Back").LayerHeight; num += 1f)
			{
				if (doesTileHaveProperty((int)position.X / 64, (int)num, "Water", "Back") == null || doesTileHaveProperty((int)position.X / 64 - 1, (int)num, "Water", "Back") == null || doesTileHaveProperty((int)position.X / 64 + 1, (int)num, "Water", "Back") == null)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				temporarySprites.Add(new SeaMonsterTemporarySprite(250f, 4, Game1.random.Next(7), position));
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (!Game1.isRaining && !Game1.isFestival())
			{
				Game1.changeMusicTrack("none");
			}
			oldMariner = null;
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			bool flag = IsUsingMagicBait(who);
			float num = 0f;
			if (who != null && who.CurrentTool is FishingRod && (who.CurrentTool as FishingRod).getBobberAttachmentIndex() == 856)
			{
				num += 0.07f;
			}
			if (who.getTileX() >= 82 && who.FishingLevel >= 5 && waterDepth >= 3 && Game1.random.NextDouble() < 0.18 + (double)num)
			{
				if (Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY"))
				{
					return new Object(898, 1);
				}
				if (!who.fishCaught.ContainsKey(159) && (Game1.currentSeason.Equals("summer") || flag))
				{
					return new Object(159, 1);
				}
			}
			if (flag && bobberTile.X < 12f && bobberTile.Y > 31f && waterDepth >= 3 && Game1.random.NextDouble() < 0.1 + (double)(num * 1.5f))
			{
				return new Object(798 + Game1.random.Next(3), 1);
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(65, 11, 25, 12);
			float num = 1f;
			while (Game1.random.NextDouble() < (double)num)
			{
				int parentSheetIndex = 393;
				if (Game1.random.NextDouble() < 0.2)
				{
					parentSheetIndex = 397;
				}
				Vector2 vector = new Vector2(Game1.random.Next(rectangle.X, rectangle.Right), Game1.random.Next(rectangle.Y, rectangle.Bottom));
				if (isTileLocationTotallyClearAndPlaceable(vector))
				{
					dropObject(new Object(parentSheetIndex, 1), vector * 64f, Game1.viewport, initialPlacement: true);
				}
				num /= 2f;
			}
			Microsoft.Xna.Framework.Rectangle rectangle2 = new Microsoft.Xna.Framework.Rectangle(66, 24, 19, 1);
			num = 0.25f;
			while (Game1.random.NextDouble() < (double)num)
			{
				if (Game1.random.NextDouble() < 0.1)
				{
					Vector2 vector2 = new Vector2(Game1.random.Next(rectangle2.X, rectangle2.Right), Game1.random.Next(rectangle2.Y, rectangle2.Bottom));
					if (isTileLocationTotallyClearAndPlaceable(vector2))
					{
						dropObject(new Object(152, 1), vector2 * 64f, Game1.viewport, initialPlacement: true);
					}
				}
				num /= 2f;
			}
			if (!Game1.currentSeason.Equals("summer") || Game1.dayOfMonth < 12 || Game1.dayOfMonth > 14)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				spawnObjects();
			}
			num = 1.5f;
			while (Game1.random.NextDouble() < (double)num)
			{
				int parentSheetIndex2 = 393;
				if (Game1.random.NextDouble() < 0.2)
				{
					parentSheetIndex2 = 397;
				}
				Vector2 randomTile = getRandomTile();
				randomTile.Y /= 2f;
				string text = doesTileHaveProperty((int)randomTile.X, (int)randomTile.Y, "Type", "Back");
				if (isTileLocationTotallyClearAndPlaceable(randomTile) && (text == null || !text.Equals("Wood")))
				{
					dropObject(new Object(parentSheetIndex2, 1), randomTile * 64f, Game1.viewport, initialPlacement: true);
				}
				num /= 1.1f;
			}
		}

		public void doneWithBridgeFix()
		{
			Game1.globalFadeToClear();
			Game1.viewportFreeze = false;
			Game1.freezeControls = false;
		}

		public void fadedForBridgeFix()
		{
			Game1.freezeControls = true;
			DelayedAction.playSoundAfterDelay("crafting", 1000);
			DelayedAction.playSoundAfterDelay("crafting", 1500);
			DelayedAction.playSoundAfterDelay("crafting", 2000);
			DelayedAction.playSoundAfterDelay("crafting", 2500);
			DelayedAction.playSoundAfterDelay("axchop", 3000);
			DelayedAction.playSoundAfterDelay("Ship", 3200);
			Game1.viewportFreeze = true;
			Game1.viewport.X = -10000;
			bridgeFixed.Value = true;
			Game1.pauseThenDoFunction(4000, doneWithBridgeFix);
			fixBridge(this);
		}

		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer != null && questionAndAnswer.Equals("BeachBridge_Yes"))
			{
				Game1.globalFadeToBlack(fadedForBridgeFix);
				Game1.player.removeItemsFromInventory(388, 300);
				return true;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			switch ((map.GetLayer("Buildings").Tiles[tileLocation] != null) ? map.GetLayer("Buildings").Tiles[tileLocation].TileIndex : (-1))
			{
			case 284:
				if (who.hasItemInInventory(388, 300))
				{
					createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Beach_FixBridge_Question"), createYesNoResponses(), "BeachBridge");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Beach_FixBridge_Hint"));
				}
				break;
			case 496:
				if (!Game1.MasterPlayer.mailReceived.Contains("spring_2_1"))
				{
					Game1.drawLetterMessage(Game1.content.LoadString("Strings\\Locations:Beach_GoneFishingMessage").Replace('\n', '^'));
					return false;
				}
				break;
			}
			if (oldMariner != null && oldMariner.getTileX() == tileLocation.X && oldMariner.getTileY() == tileLocation.Y)
			{
				string sub = Game1.content.LoadString("Strings\\Locations:Beach_Mariner_Player_" + (who.IsMale ? "Male" : "Female"));
				if (!who.isMarried() && who.specialItems.Contains(460) && !Utility.doesItemWithThisIndexExistAnywhere(460))
				{
					for (int num = who.specialItems.Count - 1; num >= 0; num--)
					{
						if (who.specialItems[num] == 460)
						{
							who.specialItems.RemoveAt(num);
						}
					}
				}
				if (who.isMarried())
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerMarried", sub)));
				}
				else if (who.specialItems.Contains(460))
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerHasItem", sub)));
				}
				else if (who.hasAFriendWithHeartLevel(10, datablesOnly: true) && (int)who.houseUpgradeLevel == 0)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerNotUpgradedHouse", sub)));
				}
				else if (who.hasAFriendWithHeartLevel(10, datablesOnly: true))
				{
					Response[] answerChoices = new Response[2]
					{
						new Response("Buy", Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_AnswerYes")),
						new Response("Not", Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_AnswerNo"))
					};
					createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_Question", sub)), answerChoices, "mariner");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerNoRelationship", sub)));
				}
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			if (oldMariner != null && position.Intersects(oldMariner.GetBoundingBox()))
			{
				return true;
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		public override void checkForMusic(GameTime time)
		{
			if (Game1.random.NextDouble() < 0.003 && Game1.timeOfDay < 1900)
			{
				localSound("seagulls");
			}
			base.checkForMusic(time);
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (Game1.currentSeason.Equals("summer") && Game1.dayOfMonth >= 12 && Game1.dayOfMonth <= 14)
			{
				waterColor.Value = new Color(0, 255, 0) * 0.4f;
			}
		}

		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (force)
			{
				hasShownCCUpgrade = false;
			}
			if ((bool)bridgeFixed)
			{
				fixBridge(this);
			}
			if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				showCommunityUpgradeShortcuts(this, ref hasShownCCUpgrade);
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (!Game1.isRaining && !Game1.isFestival())
			{
				Game1.changeMusicTrack("ocean");
			}
			int number = Game1.random.Next(6);
			List<Vector2> positionsInClusterAroundThisTile = Utility.getPositionsInClusterAroundThisTile(new Vector2(Game1.random.Next(map.DisplayWidth / 64), Game1.random.Next(12, map.DisplayHeight / 64)), number);
			foreach (Vector2 item in positionsInClusterAroundThisTile)
			{
				if (!isTileOnMap(item) || (!isTileLocationTotallyClearAndPlaceable(item) && doesTileHaveProperty((int)item.X, (int)item.Y, "Water", "Back") == null))
				{
					continue;
				}
				int startingState = 3;
				if (doesTileHaveProperty((int)item.X, (int)item.Y, "Water", "Back") != null)
				{
					startingState = 2;
					if (Game1.random.NextDouble() < 0.5)
					{
						continue;
					}
				}
				critters.Add(new Seagull(item * 64f + new Vector2(32f, 32f), startingState));
			}
			if (Game1.isRaining && Game1.timeOfDay < 1900)
			{
				oldMariner = new NPC(new AnimatedSprite("Characters\\Mariner", 0, 16, 32), new Vector2(80f, 5f) * 64f, 2, "Old Mariner");
			}
		}

		public static void showCommunityUpgradeShortcuts(GameLocation location, ref bool flag)
		{
			if (flag)
			{
				return;
			}
			flag = true;
			location.warps.Add(new Warp(-1, 4, "Forest", 119, 35, flipFarmer: false));
			location.warps.Add(new Warp(-1, 5, "Forest", 119, 35, flipFarmer: false));
			location.warps.Add(new Warp(-1, 6, "Forest", 119, 36, flipFarmer: false));
			location.warps.Add(new Warp(-1, 7, "Forest", 119, 36, flipFarmer: false));
			for (int i = 0; i < 5; i++)
			{
				for (int j = 4; j < 7; j++)
				{
					location.removeTile(i, j, "Buildings");
				}
			}
			location.removeTile(7, 6, "Buildings");
			location.removeTile(5, 6, "Buildings");
			location.removeTile(6, 6, "Buildings");
			location.setMapTileIndex(3, 7, 107, "Back");
			location.removeTile(67, 5, "Buildings");
			location.removeTile(67, 4, "Buildings");
			location.removeTile(67, 3, "Buildings");
			location.removeTile(67, 2, "Buildings");
			location.removeTile(67, 1, "Buildings");
			location.removeTile(67, 0, "Buildings");
			location.removeTile(66, 3, "Buildings");
			location.removeTile(68, 3, "Buildings");
		}

		public static void fixBridge(GameLocation location)
		{
			if (!NetWorldState.checkAnywhereForWorldStateID("beachBridgeFixed"))
			{
				NetWorldState.addWorldStateIDEverywhere("beachBridgeFixed");
			}
			location.updateMap();
			int whichTileSheet = ((!location.name.Value.Contains("Market")) ? 1 : 2);
			location.setMapTile(58, 13, 301, "Buildings", null, whichTileSheet);
			location.setMapTile(59, 13, 301, "Buildings", null, whichTileSheet);
			location.setMapTile(60, 13, 301, "Buildings", null, whichTileSheet);
			location.setMapTile(61, 13, 301, "Buildings", null, whichTileSheet);
			location.setMapTile(58, 14, 336, "Back", null, whichTileSheet);
			location.setMapTile(59, 14, 336, "Back", null, whichTileSheet);
			location.setMapTile(60, 14, 336, "Back", null, whichTileSheet);
			location.setMapTile(61, 14, 336, "Back", null, whichTileSheet);
		}

		public override void draw(SpriteBatch b)
		{
			if (oldMariner != null)
			{
				oldMariner.draw(b);
			}
			base.draw(b);
			if (!bridgeFixed)
			{
				float num = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3704f, 720f + num)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.095401f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3744f, 760f + num)), new Microsoft.Xna.Framework.Rectangle(175, 425, 12, 12), Color.White * 0.75f, 0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, 0.09541f);
			}
		}
	}
}
