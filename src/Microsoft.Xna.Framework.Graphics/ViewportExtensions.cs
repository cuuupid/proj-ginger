using xTile.Dimensions;

namespace Microsoft.Xna.Framework.Graphics
{
	public static class ViewportExtensions
	{
		public static Rectangle GetTitleSafeArea(this Viewport vp)
		{
			return vp.Bounds;
		}

		public static Rectangle ToXna(this xTile.Dimensions.Rectangle xrect)
		{
			return new Rectangle(xrect.X, xrect.Y, xrect.Width, xrect.Height);
		}

		public static Vector2 Size(this Viewport vp)
		{
			return new Vector2(vp.Width, vp.Height);
		}

		public static int GetElementCount(this Texture2D texure)
		{
			return texure.ActualWidth * texure.ActualHeight;
		}

		public static int GetActualWidth(this Texture2D texure)
		{
			return texure.ActualWidth;
		}

		public static int GetActualHeight(this Texture2D texure)
		{
			return texure.ActualHeight;
		}
	}
}
