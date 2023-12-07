namespace StardewValley
{
	public class ArtifactSpotWatchActivity : FarmActivity
	{
		protected override bool _AttemptActivity(Farm farm)
		{
			Object randomObject = GetRandomObject(farm, (Object o) => Utility.IsNormalObjectAtParentSheetIndex(o, 595));
			if (randomObject != null)
			{
				activityPosition = GetNearbyTile(farm, randomObject.TileLocation);
				return true;
			}
			return false;
		}
	}
}
