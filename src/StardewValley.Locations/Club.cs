using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Locations
{
	public class Club : GameLocation
	{
		public static int timesPlayedCalicoJack;

		public static int timesPlayedSlots;

		private string coinBuffer;

		public Club()
		{
		}

		public Club(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			lightGlows.Clear();
			coinBuffer = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? "     " : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? "\u3000\u3000" : "  "));
		}

		public override void checkForMusic(GameTime time)
		{
			if (Game1.random.NextDouble() < 0.002)
			{
				localSound("boop");
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			Game1.changeMusicTrack("none");
			base.cleanupBeforePlayerExit();
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			b.End();
			Game1.PushUIMode();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			int num = 64;
			int num2 = 16;
			if (Game1.options.verticalToolbar)
			{
				num += (int)((float)(Math.Max(Game1.xEdge, Game1.toolbarPaddingX) + Game1.toolbar.itemSlotSize + 20) / Game1.options.uiScale);
			}
			if (Game1.activeClickableMenu == null)
			{
				SpriteText.drawStringWithScrollBackground(b, coinBuffer + Game1.player.clubCoins, num, num2);
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num, num2 + 4), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			}
			b.End();
			Game1.PopUIMode();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		}
	}
}
