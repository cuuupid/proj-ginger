using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	public class TutorialItem : IClickableMenu
	{
		public HandPointer hand;

		public string text;

		public string location;

		public bool hasPointer;

		private bool showing;

		private bool alreadyShown;

		private bool completed;

		private bool timeOut;

		private bool delayCountingDown;

		private bool requiresNoAction;

		private List<tutorialType> prerequisites;

		private List<tutorialType> ignoreifthere;

		private float widthMod;

		private float heightMod;

		private int startX;

		private int startY;

		private int targetX;

		private int targetY;

		private int pointerType;

		public tutorialType tType;

		public DialogueBox dialog;

		private float timeLeft;

		private float delay;

		private float elapsedTime;

		private const float expiryTime = 6000f;

		private float minimumTimeToDisplay = 500f;

		public Type menuType;

		public ClickableTextureComponent buttonTarget;

		private string toolbarItem;

		public TutorialItem(string location, tutorialType typeOfTutorial, bool hasPointer, Type menuType = null, int pointerType = -1, int targetX = -1, int targetY = -1, int startX = -1, int startY = -1, string text = null, bool timeOut = false, ClickableTextureComponent button_target = null, string toolbarItem = "")
		{
			this.startX = startX;
			this.startY = startY;
			this.targetX = targetX;
			this.targetY = targetY;
			this.hasPointer = hasPointer;
			this.pointerType = pointerType;
			this.location = location;
			tType = typeOfTutorial;
			this.menuType = menuType;
			buttonTarget = button_target;
			this.toolbarItem = toolbarItem;
			widthMod = (float)Game1.viewport.Width / 1280f;
			heightMod = (float)Game1.viewport.Height / 720f;
			prerequisites = new List<tutorialType>();
			ignoreifthere = new List<tutorialType>();
			this.text = text;
			alreadyShown = false;
			completed = false;
			showing = (delayCountingDown = false);
			this.timeOut = timeOut;
			if (timeOut)
			{
				timeLeft = 6000f;
			}
		}

		public void setComplete()
		{
			completed = true;
			alreadyShown = true;
			hand = null;
			text = "";
		}

		public bool isComplete()
		{
			return completed;
		}

		public void addPrerequisite(tutorialType pre)
		{
			prerequisites.Add(pre);
		}

		public void addDelay(float milliSeconds)
		{
			delay = milliSeconds;
		}

		public List<tutorialType> getPrerequisites()
		{
			return prerequisites;
		}

		public void addIgnoreIfThere(tutorialType i)
		{
			ignoreifthere.Add(i);
		}

		public List<tutorialType> getIgnoreIfThere()
		{
			return ignoreifthere;
		}

		public bool hasBeenShown()
		{
			return alreadyShown;
		}

		public bool isShowing()
		{
			return showing;
		}

		public void unShow()
		{
			if (showing)
			{
				showing = false;
				TutorialManager.menuUp = false;
				if (dialog != null)
				{
					dialog = null;
				}
			}
		}

		public bool willTimeout()
		{
			return timeOut;
		}

		public bool isInDialogBounds(int x, int y)
		{
			if (((dialog == null) ? new Rectangle(-1, -1, 0, 0) : dialog.getBounds()).Contains(x, y))
			{
				return true;
			}
			return false;
		}

		public void show()
		{
			if (!TutorialManager.Instance.showTheTutorials)
			{
				return;
			}
			alreadyShown = true;
			if (delay > 0f)
			{
				delayCountingDown = true;
			}
			else
			{
				if (showing)
				{
					return;
				}
				TutorialManager.menuUp = true;
				showing = true;
				elapsedTime = 0f;
				if (text != null && text != "")
				{
					dialog = new DialogueBox(text, closeButton: false);
				}
				if (!hasPointer)
				{
					return;
				}
				if (buttonTarget != null)
				{
					hand = new HandPointer(0, 0, pointerType, 0, 0, buttonTarget);
				}
				else if (toolbarItem != "")
				{
					if (toolbarItem.Equals("closeButton"))
					{
						hand = null;
						hasPointer = false;
					}
					else
					{
						Vector2 iconPosition = Game1.toolbar.getIconPosition(toolbarItem);
						if (iconPosition.X != -999f)
						{
							hand = new HandPointer((int)iconPosition.X, (int)iconPosition.Y, pointerType, (int)(iconPosition.X - 32f), (int)(iconPosition.Y - 32f));
						}
						else
						{
							hand = null;
							hasPointer = false;
						}
					}
				}
				else
				{
					int num = pointerType;
					if (num == 1 || num == 3)
					{
						hand = new HandPointer(targetX, targetY, pointerType, startX, startY);
					}
					else
					{
						hand = new HandPointer(Utility.To4(targetX), Utility.To4(targetY), pointerType, Utility.To4(startX), Utility.To4(startY));
					}
				}
				if (hand != null)
				{
					hand.start();
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (!Game1.eventUp && hand != null && (pointerType == 1 || pointerType == 3) && (Game1.activeClickableMenu == null || Game1.activeClickableMenu.GetType().Equals(menuType)))
			{
				hand.draw(b);
			}
		}

		public void drawDialogueBox(SpriteBatch b)
		{
			if (!Game1.eventUp && dialog != null)
			{
				if (Game1.clientBounds.Width >= 1280 && Game1.toolbar != null && Game1.options.verticalToolbar && Game1.displayHUD)
				{
					dialog.width = (int)((float)Game1.clientBounds.Width / Game1.NativeZoomLevel) - (Game1.toolbarPaddingX + Game1.toolbar.itemSlotSize + 28) * 2;
				}
				else
				{
					dialog.width = (int)((float)Game1.clientBounds.Width / Game1.NativeZoomLevel) - Game1.xEdge * 2 - 64;
				}
				if (dialog.fullDialogue != null && dialog.fullDialogue != "")
				{
					dialog.height = SpriteText.getHeightOfString(dialog.fullDialogue, dialog.width - 8 - 16) + 12 + 12;
				}
				dialog.x = (Game1.uiViewport.Width - dialog.width) / 2;
				dialog.y = Game1.uiViewport.Height - dialog.height - 32;
				dialog.draw(b);
			}
		}

		public void drawHandForUI(SpriteBatch spriteBatch)
		{
			if (!Game1.eventUp && hand != null && buttonTarget == null && pointerType != 1 && pointerType != 3 && !Game1.globalFade)
			{
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				hand.draw(spriteBatch);
			}
		}

		public void drawButtonHands(SpriteBatch b)
		{
			if (!Game1.eventUp && hand != null && buttonTarget != null && pointerType != 1 && pointerType != 3 && !Game1.globalFade)
			{
				hand.draw(b);
			}
		}

		public bool dontAllowExit()
		{
			if (dialog != null)
			{
				return dialog.isTransitioning();
			}
			return false;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (dialog != null && !dialog.hasFinishedTyping())
			{
				dialog.finishTyping();
			}
			else if (elapsedTime > minimumTimeToDisplay)
			{
				if (timeOut)
				{
					TutorialManager.Instance.completeTutorial(tType);
				}
				dialog = null;
				TutorialManager.menuUp = false;
			}
		}

		public override void update(GameTime time)
		{
			elapsedTime += time.ElapsedGameTime.Milliseconds;
			if (delayCountingDown)
			{
				delay -= time.ElapsedGameTime.Milliseconds;
				if (!(delay > 0f))
				{
					delayCountingDown = false;
					show();
				}
				return;
			}
			if (dialog != null && showing && !Game1.game1.IsSaving)
			{
				dialog.update(time);
			}
			if (hand != null && showing)
			{
				hand.update(time);
			}
			if (timeOut)
			{
				timeLeft -= time.ElapsedGameTime.Milliseconds;
				if (timeLeft < 0f)
				{
					TutorialManager.Instance.completeTutorial(tType);
					dialog = null;
					TutorialManager.menuUp = false;
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			throw new NotImplementedException();
		}
	}
}
