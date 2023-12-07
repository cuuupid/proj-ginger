using StardewValley.Menus;

namespace StardewValley.Tools
{
	public class Seeds : Stackable
	{
		private string seedType;

		private int numberInStack;

		public new int NumberInStack
		{
			get
			{
				return numberInStack;
			}
			set
			{
				numberInStack = value;
			}
		}

		public string SeedType
		{
			get
			{
				return seedType;
			}
			set
			{
				seedType = value;
			}
		}

		public Seeds()
		{
		}

		public Seeds(string seedType, int numberInStack)
			: base("Seeds", 0, 0, 0, stackable: true)
		{
			this.seedType = seedType;
			this.numberInStack = numberInStack;
			setCurrentTileIndexToSeedType();
			base.IndexOfMenuItemView = base.CurrentParentTileIndex;
		}

		public override Item getOne()
		{
			Seeds seeds = new Seeds(SeedType, 1);
			seeds._GetOneFrom(this);
			return seeds;
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Seeds.cs.14209");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Seeds.cs.14210");
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			who.Stamina -= 2f - (float)who.FarmingLevel * 0.1f;
			numberInStack--;
			setCurrentTileIndexToSeedType();
			location.playSound("seeds");
			if (TutorialManager.Instance.numberOfSeedsSown < 6)
			{
				TutorialManager.Instance.numberOfSeedsSown++;
				if (TutorialManager.Instance.numberOfSeedsSown >= 6)
				{
					TutorialManager.Instance.completeTutorial(tutorialType.PLANT_SEEDS);
				}
			}
		}

		private void setCurrentTileIndexToSeedType()
		{
			string text = seedType;
			if (text == null)
			{
				return;
			}
			switch (text.Length)
			{
			case 7:
				switch (text[1])
				{
				case 'a':
					if (text == "Parsnip")
					{
						base.CurrentParentTileIndex = 0;
					}
					break;
				case 'h':
					if (text == "Rhubarb")
					{
						base.CurrentParentTileIndex = 6;
					}
					break;
				case 'u':
					if (text == "Pumpkin")
					{
						base.CurrentParentTileIndex = 58;
					}
					break;
				}
				break;
			case 10:
				switch (text[1])
				{
				case 'r':
					if (text == "Green Bean")
					{
						base.CurrentParentTileIndex = 1;
					}
					break;
				case 'p':
					if (text == "Spring Mix")
					{
						base.CurrentParentTileIndex = 63;
					}
					break;
				case 'u':
					if (text == "Summer Mix")
					{
						base.CurrentParentTileIndex = 64;
					}
					break;
				case 'i':
					if (text == "Winter Mix")
					{
						base.CurrentParentTileIndex = 66;
					}
					break;
				}
				break;
			case 11:
				switch (text[1])
				{
				case 'a':
					if (text == "Cauliflower")
					{
						base.CurrentParentTileIndex = 2;
					}
					break;
				case 'e':
					if (text == "Red Cabbage")
					{
						base.CurrentParentTileIndex = 13;
					}
					break;
				case 'r':
					if (text == "Cranberries")
					{
						base.CurrentParentTileIndex = 61;
					}
					break;
				}
				break;
			case 6:
				switch (text[0])
				{
				case 'P':
					if (text == "Potato")
					{
						base.CurrentParentTileIndex = 3;
					}
					break;
				case 'G':
					if (text == "Garlic")
					{
						base.CurrentParentTileIndex = 4;
					}
					break;
				case 'T':
					if (text == "Tomato")
					{
						base.CurrentParentTileIndex = 8;
					}
					break;
				case 'R':
					if (text == "Radish")
					{
						base.CurrentParentTileIndex = 12;
					}
					break;
				}
				break;
			case 4:
				switch (text[0])
				{
				case 'K':
					if (text == "Kale")
					{
						base.CurrentParentTileIndex = 5;
					}
					break;
				case 'C':
					if (text == "Corn")
					{
						base.CurrentParentTileIndex = 15;
					}
					break;
				case 'B':
					if (text == "Beet")
					{
						base.CurrentParentTileIndex = 62;
					}
					break;
				}
				break;
			case 5:
				switch (text[0])
				{
				case 'M':
					if (text == "Melon")
					{
						base.CurrentParentTileIndex = 7;
					}
					break;
				case 'W':
					if (text == "Wheat")
					{
						base.CurrentParentTileIndex = 11;
					}
					break;
				}
				break;
			case 9:
				switch (text[0])
				{
				case 'B':
					if (text == "Blueberry")
					{
						base.CurrentParentTileIndex = 9;
					}
					break;
				case 'S':
					if (text == "Starfruit")
					{
						base.CurrentParentTileIndex = 14;
					}
					break;
				case 'A':
					if (text == "Artichoke")
					{
						base.CurrentParentTileIndex = 57;
					}
					break;
				}
				break;
			case 13:
				switch (text[0])
				{
				case 'Y':
					if (text == "Yellow Pepper")
					{
						base.CurrentParentTileIndex = 10;
					}
					break;
				case 'A':
					if (text == "Ancient Fruit")
					{
						base.CurrentParentTileIndex = 72;
					}
					break;
				}
				break;
			case 8:
				switch (text[0])
				{
				case 'E':
					if (text == "Eggplant")
					{
						base.CurrentParentTileIndex = 56;
					}
					break;
				case 'B':
					if (text == "Bok Choy")
					{
						base.CurrentParentTileIndex = 59;
					}
					break;
				case 'F':
					if (text == "Fall Mix")
					{
						base.CurrentParentTileIndex = 65;
					}
					break;
				case 'C':
				case 'D':
					break;
				}
				break;
			case 3:
				if (text == "Yam")
				{
					base.CurrentParentTileIndex = 60;
				}
				break;
			case 12:
				break;
			}
		}
	}
}
