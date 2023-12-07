using Microsoft.Xna.Framework;

namespace StardewValley
{
	public class DelayedAction
	{
		public delegate void delayedBehavior();

		public int timeUntilAction;

		public float floatData;

		public string stringData;

		public Point pointData;

		public NPC character;

		public GameLocation location;

		public delayedBehavior behavior;

		public Game1.afterFadeFunction afterFadeBehavior;

		public bool waitUntilMenusGone;

		public TemporaryAnimatedSprite temporarySpriteData;

		public DelayedAction(int timeUntilAction)
		{
			this.timeUntilAction = timeUntilAction;
		}

		public DelayedAction(int timeUntilAction, delayedBehavior behavior)
		{
			this.timeUntilAction = timeUntilAction;
			this.behavior = behavior;
		}

		public bool update(GameTime time)
		{
			if (!waitUntilMenusGone || Game1.activeClickableMenu == null)
			{
				timeUntilAction -= time.ElapsedGameTime.Milliseconds;
				if (timeUntilAction <= 0)
				{
					behavior();
				}
			}
			return timeUntilAction <= 0;
		}

		public static void warpAfterDelay(string nameToWarpTo, Point pointToWarp, int timer)
		{
			DelayedAction delayedAction = new DelayedAction(timer);
			delayedAction.behavior = delayedAction.warp;
			delayedAction.stringData = nameToWarpTo;
			delayedAction.pointData = pointToWarp;
			Game1.delayedActions.Add(delayedAction);
		}

		public static void addTemporarySpriteAfterDelay(TemporaryAnimatedSprite t, GameLocation l, int timer, bool waitUntilMenusGone = false)
		{
			DelayedAction delayedAction = new DelayedAction(timer);
			delayedAction.behavior = delayedAction.addTempSprite;
			delayedAction.temporarySpriteData = t;
			delayedAction.location = l;
			delayedAction.waitUntilMenusGone = waitUntilMenusGone;
			Game1.delayedActions.Add(delayedAction);
		}

		public static void playSoundAfterDelay(string soundName, int timer, GameLocation location = null, int pitch = -1)
		{
			DelayedAction delayedAction = new DelayedAction(timer);
			delayedAction.behavior = delayedAction.playSound;
			delayedAction.stringData = soundName;
			delayedAction.location = location;
			delayedAction.floatData = pitch;
			Game1.delayedActions.Add(delayedAction);
		}

		public static void removeTemporarySpriteAfterDelay(GameLocation location, float idOfTempSprite, int timer)
		{
			DelayedAction delayedAction = new DelayedAction(timer);
			delayedAction.behavior = delayedAction.removeTemporarySprite;
			delayedAction.location = location;
			delayedAction.floatData = idOfTempSprite;
			Game1.delayedActions.Add(delayedAction);
		}

		public static DelayedAction playMusicAfterDelay(string musicName, int timer, bool interruptable = true)
		{
			DelayedAction delayedAction = new DelayedAction(timer);
			delayedAction.behavior = delayedAction.changeMusicTrack;
			delayedAction.stringData = musicName;
			if (interruptable)
			{
				delayedAction.floatData = 1f;
			}
			else
			{
				delayedAction.floatData = 0f;
			}
			Game1.delayedActions.Add(delayedAction);
			return delayedAction;
		}

		public static void textAboveHeadAfterDelay(string text, NPC who, int timer)
		{
			DelayedAction delayedAction = new DelayedAction(timer);
			delayedAction.behavior = delayedAction.showTextAboveHead;
			delayedAction.stringData = text;
			delayedAction.character = who;
			Game1.delayedActions.Add(delayedAction);
		}

		public static void stopFarmerGlowing(int timer)
		{
			DelayedAction delayedAction = new DelayedAction(timer);
			delayedAction.behavior = delayedAction.stopGlowing;
			Game1.delayedActions.Add(delayedAction);
		}

		public static void showDialogueAfterDelay(string dialogue, int timer)
		{
			DelayedAction delayedAction = new DelayedAction(timer);
			delayedAction.behavior = delayedAction.showDialogue;
			delayedAction.stringData = dialogue;
			Game1.delayedActions.Add(delayedAction);
		}

		public static void screenFlashAfterDelay(float intensity, int timer, string sound = "")
		{
			DelayedAction delayedAction = new DelayedAction(timer);
			delayedAction.behavior = delayedAction.screenFlash;
			delayedAction.stringData = sound;
			delayedAction.floatData = intensity;
			Game1.delayedActions.Add(delayedAction);
		}

		public static void removeTileAfterDelay(int x, int y, int timer, GameLocation l, string whichLayer)
		{
			DelayedAction delayedAction = new DelayedAction(timer);
			delayedAction.behavior = delayedAction.removeBuildingsTile;
			delayedAction.pointData = new Point(x, y);
			delayedAction.location = l;
			delayedAction.stringData = whichLayer;
			Game1.delayedActions.Add(delayedAction);
		}

		public static void fadeAfterDelay(Game1.afterFadeFunction behaviorAfterFade, int timer)
		{
			DelayedAction delayedAction = new DelayedAction(timer);
			delayedAction.behavior = delayedAction.doGlobalFade;
			delayedAction.afterFadeBehavior = behaviorAfterFade;
			Game1.delayedActions.Add(delayedAction);
		}

		public static void functionAfterDelay(delayedBehavior func, int timer)
		{
			DelayedAction delayedAction = new DelayedAction(timer);
			delayedAction.behavior = func;
			Game1.delayedActions.Add(delayedAction);
		}

		public void doGlobalFade()
		{
			Game1.globalFadeToBlack(afterFadeBehavior);
		}

		public void showTextAboveHead()
		{
			if (character != null && stringData != null)
			{
				character.showTextAboveHead(stringData);
			}
		}

		public void addTempSprite()
		{
			if (location != null && temporarySpriteData != null)
			{
				location.TemporarySprites.Add(temporarySpriteData);
			}
		}

		public void stopGlowing()
		{
			Game1.player.stopGlowing();
			Game1.player.stopJittering();
			Game1.screenGlowHold = false;
			if (Game1.isFestival() && Game1.currentSeason.Equals("fall"))
			{
				Game1.changeMusicTrack("fallFest");
			}
		}

		public void showDialogue()
		{
			Game1.drawObjectDialogue(stringData);
		}

		public void warp()
		{
			if (stringData != null)
			{
				_ = pointData;
				Game1.warpFarmer(stringData, pointData.X, pointData.Y, flip: false);
			}
		}

		public void removeBuildingsTile()
		{
			_ = pointData;
			if (location != null && stringData != null)
			{
				location.removeTile(pointData.X, pointData.Y, stringData);
			}
		}

		public void removeTemporarySprite()
		{
			if (location != null)
			{
				location.removeTemporarySpritesWithID(floatData);
			}
		}

		public void playSound()
		{
			if (stringData == null)
			{
				return;
			}
			if (location == null)
			{
				if (floatData != -1f)
				{
					Game1.playSoundPitched(stringData, (int)floatData);
				}
				else
				{
					Game1.playSound(stringData);
				}
			}
			else if (floatData != -1f)
			{
				location.playSoundPitched(stringData, (int)floatData);
			}
			else
			{
				location.playSound(stringData);
			}
		}

		public void changeMusicTrack()
		{
			if (stringData != null)
			{
				Game1.changeMusicTrack(stringData, floatData > 0f);
			}
		}

		public void screenFlash()
		{
			if (stringData != null && stringData.Length > 0)
			{
				Game1.playSound(stringData);
			}
			Game1.flashAlpha = floatData;
		}
	}
}
