using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Events
{
	public class DiaryEvent : FarmEvent, INetObject<NetFields>
	{
		public string NPCname;

		public NetFields NetFields { get; } = new NetFields();


		public bool setUp()
		{
			if (Game1.player.isMarried())
			{
				return true;
			}
			foreach (string item in Game1.player.mailReceived)
			{
				if (item.Contains("diary"))
				{
					string text = item.Split('_')[1];
					if (!Game1.player.mailReceived.Contains("diary_" + text + "_finished"))
					{
						int num = Convert.ToInt32(text.Split('/')[1]);
						NPCname = text.Split('/')[0];
						NPC characterFromName = Game1.getCharacterFromName(NPCname);
						string text2 = ((characterFromName.Gender == 0) ? "him" : "her");
						Game1.player.mailReceived.Add("diary_" + text + "_finished");
						string text3 = (Game1.player.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6658") : Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6660")) + Environment.NewLine + Environment.NewLine + "-" + Utility.capitalizeFirstLetter(Game1.CurrentSeasonDisplayName) + " " + Game1.dayOfMonth + "-" + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6664", NPCname);
						Response[] answerChoices = new Response[3]
						{
							new Response("...We're", Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6667")),
							new Response("...I", ((int)characterFromName.gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6669") : Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6670")),
							new Response("(Write", Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6672"))
						};
						Game1.currentLocation.createQuestionDialogue(Game1.parseText(text3), answerChoices, "diary");
						Game1.messagePause = true;
						return false;
					}
				}
			}
			return true;
		}

		public bool tickUpdate(GameTime time)
		{
			return !Game1.dialogueUp;
		}

		public void draw(SpriteBatch b)
		{
		}

		public void makeChangesToLocation()
		{
			Game1.messagePause = false;
		}

		public void drawAboveEverything(SpriteBatch b)
		{
		}
	}
}
