using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace StardewValley.Events
{
	public class QuestionEvent : FarmEvent, INetObject<NetFields>
	{
		public const int pregnancyQuestion = 1;

		public const int barnBirth = 2;

		public const int playerPregnancyQuestion = 3;

		private int whichQuestion;

		private AnimalHouse animalHouse;

		public FarmAnimal animal;

		public bool forceProceed;

		public NetFields NetFields { get; } = new NetFields();


		public QuestionEvent(int whichQuestion)
		{
			this.whichQuestion = whichQuestion;
		}

		public bool setUp()
		{
			switch (whichQuestion)
			{
			case 1:
			{
				Response[] answerChoices2 = new Response[2]
				{
					new Response("Yes", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_Yes")),
					new Response("Not", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_No"))
				};
				if (!Game1.getCharacterFromName(Game1.player.spouse).isGaySpouse())
				{
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HaveBabyQuestion", Game1.player.Name), answerChoices2, answerPregnancyQuestion, Game1.getCharacterFromName(Game1.player.spouse));
				}
				else
				{
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HaveBabyQuestion_Adoption", Game1.player.Name), answerChoices2, answerPregnancyQuestion, Game1.getCharacterFromName(Game1.player.spouse));
				}
				Game1.messagePause = true;
				return false;
			}
			case 2:
			{
				FarmAnimal farmAnimal = null;
				foreach (Building building in Game1.getFarm().buildings)
				{
					if ((building.owner.Equals(Game1.player.UniqueMultiplayerID) || !Game1.IsMultiplayer) && building.buildingType.Contains("Barn") && !building.buildingType.Equals("Barn") && !(building.indoors.Value as AnimalHouse).isFull() && Game1.random.NextDouble() < (double)(building.indoors.Value as AnimalHouse).animalsThatLiveHere.Count * 0.0055)
					{
						farmAnimal = Utility.getAnimal((building.indoors.Value as AnimalHouse).animalsThatLiveHere[Game1.random.Next((building.indoors.Value as AnimalHouse).animalsThatLiveHere.Count)]);
						animalHouse = building.indoors.Value as AnimalHouse;
						break;
					}
				}
				if (farmAnimal != null && !farmAnimal.isBaby() && (bool)farmAnimal.allowReproduction && farmAnimal.CanHavePregnancy())
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Events:AnimalBirth", farmAnimal.displayName, farmAnimal.shortDisplayType()));
					Game1.messagePause = true;
					animal = farmAnimal;
					return false;
				}
				break;
			}
			case 3:
			{
				Response[] answerChoices = new Response[2]
				{
					new Response("Yes", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_Yes")),
					new Response("Not", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_No"))
				};
				long value = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
				Farmer farmer = Game1.otherFarmers[value];
				if (farmer.IsMale != Game1.player.IsMale)
				{
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HavePlayerBabyQuestion", farmer.Name), answerChoices, answerPlayerPregnancyQuestion);
				}
				else
				{
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HavePlayerBabyQuestion_Adoption", farmer.Name), answerChoices, answerPlayerPregnancyQuestion);
				}
				Game1.messagePause = true;
				return false;
			}
			}
			return true;
		}

		private void answerPregnancyQuestion(Farmer who, string answer)
		{
			if (answer.Equals("Yes"))
			{
				WorldDate worldDate = new WorldDate(Game1.Date);
				worldDate.TotalDays += 14;
				who.GetSpouseFriendship().NextBirthingDate = worldDate;
				Game1.getCharacterFromName(who.spouse).isGaySpouse();
			}
		}

		private void answerPlayerPregnancyQuestion(Farmer who, string answer)
		{
			if (answer.Equals("Yes"))
			{
				long value = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
				Farmer receiver = Game1.otherFarmers[value];
				Game1.player.team.SendProposal(receiver, ProposalType.Baby);
			}
		}

		public bool tickUpdate(GameTime time)
		{
			if (forceProceed)
			{
				return true;
			}
			if (whichQuestion == 2 && !Game1.dialogueUp)
			{
				if (Game1.activeClickableMenu == null)
				{
					Game1.activeClickableMenu = new NamingMenu(animalHouse.addNewHatchedAnimal, (animal != null) ? Game1.content.LoadString("Strings\\Events:AnimalNamingTitle", animal.displayType) : Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestionEvent.cs.6692"));
				}
				return false;
			}
			return !Game1.dialogueUp;
		}

		public void draw(SpriteBatch b)
		{
		}

		public void drawAboveEverything(SpriteBatch b)
		{
		}

		public void makeChangesToLocation()
		{
			Game1.messagePause = false;
		}
	}
}
