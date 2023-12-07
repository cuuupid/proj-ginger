using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Quests
{
	public class LostItemQuest : Quest
	{
		[XmlElement("npcName")]
		public readonly NetString npcName = new NetString();

		[XmlElement("locationOfItem")]
		public readonly NetString locationOfItem = new NetString();

		[XmlElement("itemIndex")]
		public readonly NetInt itemIndex = new NetInt();

		[XmlElement("tileX")]
		public readonly NetInt tileX = new NetInt();

		[XmlElement("tileY")]
		public readonly NetInt tileY = new NetInt();

		[XmlElement("itemFound")]
		public readonly NetBool itemFound = new NetBool();

		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

		public LostItemQuest()
		{
		}

		public LostItemQuest(string npcName, string locationOfItem, int itemIndex, int tileX, int tileY)
		{
			this.npcName.Value = npcName;
			this.locationOfItem.Value = locationOfItem;
			this.itemIndex.Value = itemIndex;
			this.tileX.Value = tileX;
			this.tileY.Value = tileY;
			questType.Value = 9;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(objective, npcName, locationOfItem, itemIndex, tileX, tileY, itemFound);
		}

		public override void adjustGameLocation(GameLocation location)
		{
			if (!itemFound && location.name.Equals(locationOfItem.Value))
			{
				Vector2 vector = new Vector2((int)tileX, (int)tileY);
				if (location.overlayObjects.ContainsKey(vector))
				{
					location.overlayObjects.Remove(vector);
				}
				Object @object = new Object(vector, itemIndex, 1);
				@object.questItem.Value = true;
				@object.questId.Value = id;
				@object.isSpawnedObject.Value = true;
				location.overlayObjects.Add(vector, @object);
			}
		}

		public new void reloadObjective()
		{
			if (objective.Value != null)
			{
				base.currentObjective = objective.Value.loadDescriptionElement();
			}
		}

		public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -2, Item item = null, string str = null)
		{
			if ((bool)completed)
			{
				return false;
			}
			if (item != null && item is Object && (item as Object).parentSheetIndex.Value == itemIndex.Value && !itemFound)
			{
				itemFound.Value = true;
				string sub = npcName;
				NPC characterFromName = Game1.getCharacterFromName(npcName);
				if (characterFromName != null)
				{
					sub = characterFromName.displayName;
				}
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Quests:MessageFoundLostItem", item.DisplayName, sub));
				objective.Value = new DescriptionElement("Strings\\Quests:ObjectiveReturnToNPC", characterFromName);
				Game1.playSound("jingle1");
			}
			else if (n != null && n.Name.Equals(npcName.Value) && n.isVillager() && (bool)itemFound && Game1.player.hasItemInInventory(itemIndex, 1))
			{
				questComplete();
				Dictionary<int, string> dictionary = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
				string s = ((dictionary[id].Length > 9) ? dictionary[id].Split('/')[9] : Game1.content.LoadString("Data\\ExtraDialogue:LostItemQuest_DefaultThankYou"));
				n.setNewDialogue(s);
				Game1.drawDialogue(n);
				Game1.player.changeFriendship(250, n);
				Game1.player.removeFirstOfThisItemFromInventory(itemIndex);
				return true;
			}
			return false;
		}
	}
}
