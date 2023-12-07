using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace StardewValley
{
	public class CropWatchActivity : FarmActivity
	{
		protected Object _cropObject;

		protected override bool _AttemptActivity(Farm farm)
		{
			_cropObject = null;
			HoeDirt randomCrop = GetRandomCrop(farm, (Crop crop) => crop.currentPhase.Value <= 0 && new Object(Vector2.Zero, crop.indexOfHarvest.Value, 1).category.Value != -80);
			if (randomCrop != null)
			{
				_cropObject = new Object(Vector2.Zero, randomCrop.crop.indexOfHarvest.Value, 1);
				activityPosition = randomCrop.currentTileLocation;
				return true;
			}
			return false;
		}

		protected override void _EndActivity()
		{
			_cropObject = null;
		}

		protected override void _BeginActivity()
		{
			if (_cropObject != null)
			{
				switch (_character.getGiftTasteForThisItem(_cropObject))
				{
				case 6:
					_character.doEmote(20, nextEventCommand: false);
					break;
				case 2:
					_character.doEmote(32, nextEventCommand: false);
					break;
				default:
					_character.doEmote(12, nextEventCommand: false);
					break;
				case 8:
					break;
				}
			}
		}
	}
}
