using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class TransferredItemSprite
	{
		public Item item;

		public Vector2 position;

		public float age;

		public float alpha = 1f;

		public TransferredItemSprite(Item transferred_item, int start_x, int start_y)
		{
			item = transferred_item;
			position.X = start_x;
			position.Y = start_y;
		}

		public bool Update(GameTime time)
		{
			float num = 0.15f;
			position.Y -= (float)time.ElapsedGameTime.TotalSeconds * 128f;
			age += (float)time.ElapsedGameTime.TotalSeconds;
			alpha = 1f - age / num;
			if (age >= num)
			{
				return true;
			}
			return false;
		}

		public void Draw(SpriteBatch b)
		{
			item.drawInMenu(b, position, 1f, alpha, 0.9f, StackDrawType.Hide, Color.White, drawShadow: false);
		}
	}
}
