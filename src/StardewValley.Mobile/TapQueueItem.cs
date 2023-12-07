namespace StardewValley.Mobile
{
	public struct TapQueueItem
	{
		public int mouseX;

		public int mouseY;

		public int viewportX;

		public int viewportY;

		public int tileX;

		public int tileY;

		public TapQueueItem(int mouseX, int mouseY, int viewportX, int viewportY, int tileX, int tileY)
		{
			this.mouseX = mouseX;
			this.mouseY = mouseY;
			this.viewportX = viewportX;
			this.viewportY = viewportY;
			this.tileX = tileX;
			this.tileY = tileY;
		}
	}
}
