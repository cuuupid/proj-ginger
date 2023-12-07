using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	public class DialogueBox : IClickableMenu
	{
		public List<string> dialogues = new List<string>();

		public Dialogue characterDialogue;

		public Stack<string> characterDialoguesBrokenUp = new Stack<string>();

		public List<Response> responses = new List<Response>();

		public const int portraitBoxSize = 74;

		public const int nameTagWidth = 102;

		public const int nameTagHeight = 18;

		public const int portraitPlateWidth = 115;

		public const int nameTagSideMargin = 5;

		public const float transitionRate = 3f;

		public const int characterAdvanceDelay = 30;

		public const int safetyDelay = 750;

		public int questionFinishPauseTimer;

		protected bool _showedOptions;

		public Rectangle friendshipJewel = Rectangle.Empty;

		public List<ClickableComponent> responseCC;

		private bool closeButton = true;

		private bool hasBeenClicked;

		private bool responseMade;

		public const int TEXT_PADDING_LEFT = 8;

		public const int TEXT_PADDING_RIGHT = 16;

		public const int TEXT_PADDING_TOP = 12;

		public const int TEXT_PADDING_BOTTOM = 12;

		public int x;

		public int y;

		public int transitionX = -1;

		public int transitionY;

		public int transitionWidth;

		public int transitionHeight;

		public int characterAdvanceTimer;

		public int characterIndexInDialogue;

		public int safetyTimer = 750;

		public int heightForQuestions;

		public int selectedResponse = -1;

		public int newPortaitShakeTimer;

		public bool transitionInitialized;

		public bool transitioning = true;

		public bool transitioningBigger = true;

		public bool dialogueContinuedOnNextPage;

		public bool dialogueFinished;

		public bool isQuestion;

		public TemporaryAnimatedSprite dialogueIcon;

		public TemporaryAnimatedSprite aboveDialogueImage;

		public string fullDialogue;

		private string hoverText = "";

		public DialogueBox(int x, int y, int width, int height)
		{
			this.x = x;
			this.y = y;
			base.width = width;
			base.width = GetWidth();
			base.height = height;
		}

		public DialogueBox(string dialogue, bool closeButton = true)
		{
			this.closeButton = closeButton;
			fullDialogue = dialogue;
			Game1.mouseCursorTransparency = 0f;
			if (dialogue.Contains("#44'") || dialogue.Contains("#45'") || dialogue.Contains("#173'"))
			{
				dialogues.Add(dialogue);
			}
			else
			{
				dialogues.AddRange(dialogue.Split('#'));
			}
			width = GetWidth();
			height = SpriteText.getHeightOfString(dialogue, width - 8 - 16) + 12 + 12;
			x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
			y = Game1.uiViewport.Height - height - 32;
			setUpIcons();
		}

		public DialogueBox(string dialogue, List<Response> responses, int width = 1200)
		{
			base.width = GetWidth();
			if (TestToShrinkFont())
			{
				responseMade = false;
			}
			dialogues.Add(dialogue);
			this.responses = responses;
			isQuestion = true;
			setUpQuestions();
			height = heightForQuestions;
			SpriteText.shrinkFont(shrink: false);
			x = (Game1.uiViewport.Width - base.width) / 2;
			y = Game1.uiViewport.Height - height - 32;
			setUpIcons();
			characterIndexInDialogue = dialogue.Length - 1;
			if (responses == null)
			{
				return;
			}
			foreach (Response response in responses)
			{
				if (response.responseText.Contains("¦"))
				{
					if (Game1.player.IsMale)
					{
						response.responseText = response.responseText.Substring(0, response.responseText.IndexOf("¦"));
					}
					else
					{
						response.responseText = response.responseText.Substring(response.responseText.IndexOf("¦") + 1);
					}
				}
			}
		}

		public DialogueBox(Dialogue dialogue)
		{
			characterDialogue = dialogue;
			Game1.mouseCursorTransparency = 0f;
			width = 1200;
			width = GetWidth();
			height = 384;
			x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
			y = Game1.uiViewport.Height - height - 32;
			friendshipJewel = new Rectangle(x + width - 64, y + 256, 44, 44);
			dialogue.prepareDialogueForDisplay();
			characterDialoguesBrokenUp.Push(dialogue.getCurrentDialogue());
			checkDialogue(dialogue);
			newPortaitShakeTimer = ((characterDialogue.getPortraitIndex() == 1) ? 250 : 0);
			setUpForGamePadMode();
		}

		public DialogueBox(List<string> dialogues)
		{
			Game1.mouseCursorTransparency = 0f;
			this.dialogues = dialogues;
			width = GetWidth();
			height = SpriteText.getHeightOfString(dialogues[0], width - 8 - 16) + 12 + 12;
			x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
			y = Game1.uiViewport.Height - height - 32;
			setUpIcons();
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public override bool autoCenterMouseCursorForGamepad()
		{
			return false;
		}

		private void playOpeningSound()
		{
			Game1.playSound("breathin");
		}

		public override void setUpForGamePadMode()
		{
			if (!Game1.options.gamepadControls || Game1.lastCursorMotionWasMouse)
			{
				return;
			}
			gamePadControlsImplemented = true;
			if (isQuestion)
			{
				int num = 0;
				string currentString = getCurrentString();
				if (currentString != null && currentString.Length > 0)
				{
					num = SpriteText.getHeightOfString(currentString);
				}
				if (!Game1.options.snappyMenus)
				{
					Game1.setMousePosition(x + width - 128, y + num + 64);
				}
			}
			else
			{
				Game1.mouseCursorTransparency = 0f;
			}
		}

		public void closeDialogue()
		{
			if (Game1.activeClickableMenu != null && Game1.activeClickableMenu.Equals(this))
			{
				Game1.exitActiveMenu();
				Game1.dialogueUp = false;
				if (characterDialogue != null && characterDialogue.speaker != null && characterDialogue.speaker.CurrentDialogue.Count > 0 && dialogueFinished && characterDialogue.speaker.CurrentDialogue.Count > 0)
				{
					characterDialogue.speaker.CurrentDialogue.Pop();
				}
				if (Game1.messagePause)
				{
					Game1.pauseTime = 500f;
				}
				if (Game1.currentObjectDialogue.Count > 0)
				{
					Game1.currentObjectDialogue.Dequeue();
				}
				Game1.currentDialogueCharacterIndex = 0;
				if (Game1.currentObjectDialogue.Count > 0)
				{
					Game1.dialogueUp = true;
					Game1.questionChoices.Clear();
					Game1.dialogueTyping = true;
				}
				Game1.tvStation = -1;
				if (characterDialogue != null && characterDialogue.speaker != null && !characterDialogue.speaker.Name.Equals("Gunther") && !Game1.eventUp && !characterDialogue.speaker.doingEndOfRouteAnimation)
				{
					characterDialogue.speaker.doneFacingPlayer(Game1.player);
				}
				Game1.currentSpeaker = null;
				if (!Game1.eventUp)
				{
					Game1.player.CanMove = true;
					Game1.player.movementDirections.Clear();
				}
				else if (Game1.currentLocation.currentEvent.CurrentCommand > 0 || Game1.currentLocation.currentEvent.specialEventVariable1)
				{
					if (!Game1.isFestival() || !Game1.currentLocation.currentEvent.canMoveAfterDialogue())
					{
						Game1.currentLocation.currentEvent.CurrentCommand++;
					}
					else
					{
						Game1.player.CanMove = true;
					}
				}
				Game1.questionChoices.Clear();
			}
			if (Game1.afterDialogues != null)
			{
				Game1.afterFadeFunction afterDialogues = Game1.afterDialogues;
				Game1.afterDialogues = null;
				afterDialogues();
			}
		}

		public void finishTyping()
		{
			characterIndexInDialogue = getCurrentString().Length - 1;
		}

		public bool hasFinishedTyping()
		{
			int length = getCurrentString().Length;
			return characterIndexInDialogue >= length;
		}

		public void beginOutro()
		{
			transitionWidth = width;
			transitioning = true;
			transitioningBigger = false;
			Game1.playSound("breathout");
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			receiveLeftClick(x, y, playSound);
		}

		private void tryOutro()
		{
			if (Game1.activeClickableMenu != null && Game1.activeClickableMenu.Equals(this))
			{
				beginOutro();
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (transitioning || Game1.isAnyGamePadButtonBeingPressed())
			{
				return;
			}
			if (isQuestion)
			{
				switch (key)
				{
				case Keys.W:
					selectedResponse--;
					if (selectedResponse < 0)
					{
						selectedResponse = responses.Count - 1;
					}
					Game1.playSound("smallSelect");
					break;
				case Keys.S:
					selectedResponse++;
					if (selectedResponse >= responses.Count)
					{
						selectedResponse = 0;
					}
					Game1.playSound("smallSelect");
					break;
				default:
					if (selectedResponse >= 0 && (key == Keys.E || key == Keys.C || key == Keys.X))
					{
						hasBeenClicked = true;
						releaseLeftClick(0, 0);
					}
					break;
				}
			}
			else if (key == Keys.E || key == Keys.C || key == Keys.X)
			{
				hasBeenClicked = true;
				releaseLeftClick(0, 0);
			}
			else if ((!Game1.options.SnappyMenus || !isQuestion) && (Game1.options.doesInputListContain(Game1.options.actionButton, key) || Game1.options.doesInputListContain(Game1.options.menuButton, key)) && Game1.options.SnappyMenus && !Game1.options.doesInputListContain(Game1.options.actionButton, key))
			{
				receiveLeftClick(0, 0);
			}
			else if (isQuestion && !Game1.eventUp && characterDialogue == null)
			{
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key))
				{
					if (responses != null && responses.Count > 0 && Game1.currentLocation.answerDialogue(responses[responses.Count - 1]))
					{
						Game1.playSound("smallSelect");
					}
					selectedResponse = -1;
					tryOutro();
				}
				else if (Game1.options.SnappyMenus)
				{
					base.receiveKeyPress(key);
				}
				else if (key == Keys.Y && responses != null && responses.Count > 0 && responses[0].responseKey.Equals("Yes") && Game1.currentLocation.answerDialogue(responses[0]))
				{
					Game1.playSound("smallSelect");
					selectedResponse = -1;
					tryOutro();
				}
			}
			else if (Game1.options.SnappyMenus && isQuestion && !Game1.options.doesInputListContain(Game1.options.menuButton, key))
			{
				base.receiveKeyPress(key);
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (responses.Count > 0)
			{
				switch (b)
				{
				case Buttons.DPadUp:
				case Buttons.LeftThumbstickUp:
					selectedResponse--;
					if (selectedResponse < 0)
					{
						selectedResponse = responses.Count - 1;
					}
					Game1.playSound("Cowboy_gunshot");
					break;
				case Buttons.DPadDown:
				case Buttons.LeftThumbstickDown:
					selectedResponse++;
					if (selectedResponse >= responses.Count)
					{
						selectedResponse = 0;
					}
					Game1.playSound("Cowboy_gunshot");
					break;
				}
			}
			if (b != Buttons.A)
			{
				return;
			}
			if (responses.Count > 0)
			{
				if (selectedResponse > -1)
				{
					hasBeenClicked = true;
					releaseLeftClick(0, 0);
				}
			}
			else
			{
				hasBeenClicked = true;
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			hasBeenClicked = true;
			leftClickHeld(x, y);
		}

		public override void leftClickHeld(int mouseX, int mouseY)
		{
			if (!hasBeenClicked)
			{
				return;
			}
			TestToShrinkFont();
			hoverText = "";
			if (!transitioning && characterIndexInDialogue >= getCurrentString().Length - 1 && isQuestion)
			{
				int num = selectedResponse;
				selectedResponse = -1;
				int num2 = y - (heightForQuestions - height) + SpriteText.getHeightOfString(getCurrentString(), width - 48) + 48;
				for (int i = 0; i < responses.Count; i++)
				{
					int heightOfString = SpriteText.getHeightOfString(responses[i].responseText, width - 80);
					int heightOfString2 = SpriteText.getHeightOfString(responses[i].responseText, width - 80);
					if (mouseX >= x && mouseX <= x + width && mouseY >= num2 - 16 && mouseY < num2 + heightOfString2 + 16)
					{
						selectedResponse = i;
						break;
					}
					num2 += SpriteText.getHeightOfString(responses[i].responseText, width - 48) + 16 + 32;
				}
				if (selectedResponse != num)
				{
					Game1.playSound("Cowboy_gunshot");
				}
			}
			if (!Game1.eventUp && !friendshipJewel.Equals(Rectangle.Empty) && friendshipJewel.Contains(mouseX, mouseY) && characterDialogue != null && characterDialogue.speaker != null && Game1.player.friendshipData.ContainsKey(characterDialogue.speaker.Name))
			{
				hoverText = Game1.player.getFriendshipHeartLevelForNPC(characterDialogue.speaker.name) + "/" + (characterDialogue.speaker.name.Equals(Game1.player.spouse) ? "12" : "10") + "<";
			}
			SpriteText.shrinkFont(shrink: false);
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (!hasBeenClicked || transitioning)
			{
				return;
			}
			hasBeenClicked = false;
			if (characterIndexInDialogue < getCurrentString().Length - 1)
			{
				characterIndexInDialogue = getCurrentString().Length - 1;
				return;
			}
			if (isQuestion)
			{
				if (selectedResponse == -1)
				{
					return;
				}
				responseMade = true;
				questionFinishPauseTimer = (Game1.eventUp ? 600 : 200);
				transitioning = true;
				transitionX = -1;
				transitioningBigger = true;
				if (characterDialogue == null)
				{
					Game1.dialogueUp = false;
					if (Game1.eventUp)
					{
						Game1.playSound("smallSelect");
						Game1.currentLocation.currentEvent.answerDialogue(Game1.currentLocation.lastQuestionKey, selectedResponse);
						selectedResponse = -1;
						tryOutro();
						return;
					}
					if (Game1.currentLocation == null)
					{
						Game1.playSound("smallSelect");
						tryOutro();
						return;
					}
					if (Game1.currentLocation.answerDialogue(responses[selectedResponse]))
					{
						Game1.playSound("smallSelect");
					}
					selectedResponse = -1;
					tryOutro();
					return;
				}
				characterDialoguesBrokenUp.Pop();
				characterDialogue.chooseResponse(responses[selectedResponse]);
				characterDialoguesBrokenUp.Push("");
				Game1.playSound("smallSelect");
			}
			else if (characterDialogue == null)
			{
				dialogues.RemoveAt(0);
				if (dialogues.Count == 0)
				{
					closeDialogue();
				}
				else
				{
					width = GetWidth();
					if (responseMade)
					{
						SpriteText.shrinkFont(shrink: false);
					}
					height = SpriteText.getHeightOfString(dialogues[0], width - 8 - 16) + 12 + 12;
					this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
					this.y = Game1.uiViewport.Height - height - 32;
					xPositionOnScreen = x;
					yPositionOnScreen = y;
					setUpIcons();
				}
			}
			characterIndexInDialogue = 0;
			if (characterDialogue != null)
			{
				int portraitIndex = characterDialogue.getPortraitIndex();
				if (characterDialoguesBrokenUp.Count == 0)
				{
					beginOutro();
					return;
				}
				characterDialoguesBrokenUp.Pop();
				if (characterDialoguesBrokenUp.Count == 0)
				{
					if (!characterDialogue.isCurrentStringContinuedOnNextScreen)
					{
						beginOutro();
					}
					characterDialogue.exitCurrentDialogue();
				}
				if (!characterDialogue.isDialogueFinished() && characterDialogue.getCurrentDialogue().Length > 0 && characterDialoguesBrokenUp.Count == 0)
				{
					characterDialoguesBrokenUp.Push(characterDialogue.getCurrentDialogue());
				}
				checkDialogue(characterDialogue);
				if (characterDialogue.getPortraitIndex() != portraitIndex)
				{
					newPortaitShakeTimer = ((characterDialogue.getPortraitIndex() == 1) ? 250 : 50);
				}
			}
			if (!transitioning)
			{
				Game1.playSound("smallSelect");
			}
			setUpIcons();
			safetyTimer = 750;
			if (getCurrentString() != null && getCurrentString().Length <= 20)
			{
				safetyTimer -= 200;
			}
		}

		public bool isTransitioning()
		{
			string currentString = getCurrentString();
			int num = currentString.Length - 1;
			if (!transitioning)
			{
				return characterIndexInDialogue < num;
			}
			return true;
		}

		public int getSelectedResponse()
		{
			return selectedResponse;
		}

		private void setUpIcons()
		{
			dialogueIcon = null;
			if (isQuestion)
			{
				setUpQuestionIcon();
			}
			else if (characterDialogue != null && (characterDialogue.isCurrentStringContinuedOnNextScreen || characterDialoguesBrokenUp.Count > 1))
			{
				setUpNextPageIcon();
			}
			else if (dialogues != null && dialogues.Count > 1)
			{
				setUpNextPageIcon();
			}
			else if (closeButton)
			{
				setUpCloseDialogueIcon();
			}
			setUpForGamePadMode();
			if (getCurrentString() != null && getCurrentString().Length <= 20)
			{
				safetyTimer -= 200;
			}
		}

		public override void performHoverAction(int mouseX, int mouseY)
		{
		}

		private void setUpQuestionIcon()
		{
			dialogueIcon = new TemporaryAnimatedSprite(position: new Vector2(x + width - 40, y + height - 44), textureName: "LooseSprites\\Cursors", sourceRect: new Rectangle(330, 357, 7, 13), animationInterval: 100f, animationLength: 6, numberOfLoops: 999999, flicker: false, flipped: false, layerDepth: 0.089f, alphaFade: 0f, color: Color.White, scale: 4f, scaleChange: 0f, rotation: 0f, rotationChange: 0f, local: true)
			{
				yPeriodic = true,
				yPeriodicLoopTime = 1500f,
				yPeriodicRange = 8f
			};
		}

		private void setUpCloseDialogueIcon()
		{
			Vector2 position = new Vector2(x + width - 40, y + height - 44);
			if (isPortraitBox())
			{
				position.X -= 492f;
			}
			dialogueIcon = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(289, 342, 11, 12), 80f, 11, 999999, position, flicker: false, flipped: false, 0.089f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true);
		}

		private void setUpNextPageIcon()
		{
			Vector2 position = new Vector2(x + width - 40, y + height - 40);
			if (isPortraitBox())
			{
				position.X -= 492f;
			}
			dialogueIcon = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(232, 346, 9, 9), 90f, 6, 999999, position, flicker: false, flipped: false, 0.089f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				yPeriodic = true,
				yPeriodicLoopTime = 1500f,
				yPeriodicRange = 8f
			};
		}

		private void checkDialogue(Dialogue d)
		{
			isQuestion = false;
			string text = "";
			if (characterDialoguesBrokenUp.Count == 1)
			{
				text = SpriteText.getSubstringBeyondHeight(characterDialoguesBrokenUp.Peek(), width - 460 - 20, height - 16);
			}
			if (text.Length > 0)
			{
				string text2 = characterDialoguesBrokenUp.Pop().Replace(Environment.NewLine, "");
				characterDialoguesBrokenUp.Push(text.Trim());
				characterDialoguesBrokenUp.Push(text2.Substring(0, text2.Length - text.Length + 1).Trim());
			}
			if (d.getCurrentDialogue().Length == 0)
			{
				dialogueFinished = true;
			}
			if (d.isCurrentStringContinuedOnNextScreen || characterDialoguesBrokenUp.Count > 1)
			{
				dialogueContinuedOnNextPage = true;
			}
			else if (d.getCurrentDialogue().Length == 0)
			{
				beginOutro();
			}
			if (d.isCurrentDialogueAQuestion())
			{
				responses = d.getResponseOptions();
				isQuestion = true;
				TestToShrinkFont();
				setUpQuestions();
				height = heightForQuestions;
				SpriteText.shrinkFont(shrink: false);
				x = (Game1.uiViewport.Width - width) / 2;
				y = Game1.uiViewport.Height - height - 32;
				setUpIcons();
			}
			else
			{
				width = GetWidth();
				height = 384;
				x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
				y = Game1.uiViewport.Height - height - 32;
			}
		}

		private void setUpQuestions()
		{
			int widthConstraint = width - 48;
			heightForQuestions = SpriteText.getHeightOfString(getCurrentString(), widthConstraint);
			foreach (Response response in responses)
			{
				int num = SpriteText.getHeightOfString(response.responseText, width - 80) + 16;
				heightForQuestions += num;
				heightForQuestions += 32;
			}
			heightForQuestions += 40;
			if (responses != null)
			{
				int num2 = y - (heightForQuestions - height) + SpriteText.getHeightOfString(getCurrentString(), width - 48) + 48;
				for (int i = 0; i < responses.Count; i++)
				{
					num2 += SpriteText.getHeightOfString(responses[i].responseText, width - 80) + 16 + 32;
				}
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public bool isPortraitBox()
		{
			if (characterDialogue != null && characterDialogue.speaker != null && characterDialogue.speaker.Portrait != null && characterDialogue.showPortrait)
			{
				return Game1.options.showPortraits;
			}
			return false;
		}

		public void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
		{
			if (xPos > 0)
			{
				b.Draw(Game1.mouseCursors, new Rectangle(xPos - 8, yPos - 1, boxWidth + 16, boxHeight + 1), new Rectangle(306, 320, 16, 16), Color.White);
				b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 16, boxWidth, 24), new Rectangle(275, 313, 1, 6), Color.White);
				b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle(275, 328, 1, 8), Color.White);
				b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle(264, 325, 8, 1), Color.White);
				b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle(293, 324, 7, 1), Color.White);
				b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 24), new Rectangle(261, 311, 14, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.087f);
				b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 24), new Rectangle(291, 311, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.087f);
				b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle(291, 326, 12, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.087f);
				b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle(261, 327, 14, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.087f);
			}
		}

		private bool shouldPortraitShake(Dialogue d)
		{
			int portraitIndex = d.getPortraitIndex();
			if (d.speaker.Name.Equals("Pam") && portraitIndex == 3)
			{
				return true;
			}
			if (d.speaker.Name.Equals("Abigail") && portraitIndex == 7)
			{
				return true;
			}
			if (d.speaker.Name.Equals("Haley") && portraitIndex == 5)
			{
				return true;
			}
			if (d.speaker.Name.Equals("Maru") && portraitIndex == 9)
			{
				return true;
			}
			return newPortaitShakeTimer > 0;
		}

		public void drawPortrait(SpriteBatch b)
		{
			if (width >= 642)
			{
				int num = x + width - 448 + 4;
				int num2 = x + width - num;
				b.Draw(Game1.mouseCursors, new Rectangle(num - 40, y, 36, height), new Rectangle(278, 324, 9, 1), Color.White);
				b.Draw(Game1.mouseCursors, new Vector2(num - 40, y - 16), new Rectangle(278, 313, 10, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.088f);
				b.Draw(Game1.mouseCursors, new Vector2(num - 40, y + height), new Rectangle(278, 328, 10, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.088f);
				int num3 = num + 76;
				int num4 = y + height / 2 - 148 - 36;
				b.Draw(Game1.mouseCursors, new Vector2(num - 8, y), new Rectangle(583, 411, 115, 97), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.088f);
				int portraitIndex = characterDialogue.getPortraitIndex();
				Rectangle value = new Rectangle(portraitIndex * 64 % characterDialogue.speaker.Portrait.Width, portraitIndex * 64 / characterDialogue.speaker.Portrait.Width * 64, 64, 64);
				if (!characterDialogue.speaker.Portrait.Bounds.Contains(value))
				{
					value = new Rectangle(0, 0, 64, 64);
				}
				int num5 = (shouldPortraitShake(characterDialogue) ? Game1.random.Next(-1, 2) : 0);
				b.Draw(characterDialogue.speaker.Portrait, new Vector2(num3 + 16 + num5, num4 + 24), value, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.088f);
				SpriteText.drawStringHorizontallyCenteredAt(b, characterDialogue.speaker.getName(), num + num2 / 2, num4 + 296 + 16);
				if (!Game1.eventUp && !friendshipJewel.Equals(Rectangle.Empty) && characterDialogue != null && characterDialogue.speaker != null && Game1.player.friendshipData.ContainsKey(characterDialogue.speaker.Name))
				{
					friendshipJewel.X = x + width - 64;
					friendshipJewel.Y = y + 256;
					b.Draw(Game1.mouseCursors, new Vector2(friendshipJewel.X, friendshipJewel.Y), new Rectangle(Math.Max(140, 140 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 / 250.0) * 11), Math.Max(532, 532 + Math.Min(4, Game1.player.getFriendshipHeartLevelForNPC(characterDialogue.speaker.Name) / 2) * 11), 11, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.088f);
				}
			}
		}

		public string getCurrentString()
		{
			if (characterDialogue != null)
			{
				string text = ((characterDialoguesBrokenUp.Count <= 0) ? characterDialogue.getCurrentDialogue().Trim().Replace(Environment.NewLine, "") : characterDialoguesBrokenUp.Peek().Trim().Replace(Environment.NewLine, ""));
				if (!Game1.options.showPortraits)
				{
					text = characterDialogue.speaker.getName() + ": " + text;
				}
				return text;
			}
			if (dialogues.Count > 0)
			{
				return dialogues[0].Trim().Replace(Environment.NewLine, "");
			}
			return "";
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (Game1.options.SnappyMenus && !Game1.lastCursorMotionWasMouse)
			{
				Game1.mouseCursorTransparency = 0f;
			}
			else
			{
				Game1.mouseCursorTransparency = 1f;
			}
			if (safetyTimer > 0)
			{
				safetyTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (questionFinishPauseTimer > 0)
			{
				questionFinishPauseTimer -= time.ElapsedGameTime.Milliseconds;
				return;
			}
			if (transitioning)
			{
				if (transitionX == -1)
				{
					transitionX = x + width / 2;
					transitionY = y + height / 2;
					transitionWidth = 0;
					transitionHeight = 0;
				}
				if (transitioningBigger)
				{
					int num = transitionWidth;
					transitionX -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f);
					transitionY -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)(isQuestion ? heightForQuestions : height) / (float)width));
					transitionX = Math.Max(x, transitionX);
					transitionY = Math.Max(isQuestion ? (y + height - heightForQuestions) : y, transitionY);
					transitionWidth += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * 2f);
					transitionHeight += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)(isQuestion ? heightForQuestions : height) / (float)width) * 2f);
					transitionWidth = Math.Min(width, transitionWidth);
					transitionHeight = Math.Min(isQuestion ? heightForQuestions : height, transitionHeight);
					transitionY = y + (height - transitionHeight) / 2;
					if (num == 0 && transitionWidth > 0)
					{
						playOpeningSound();
					}
					if (transitionX <= x && transitionY <= (isQuestion ? (y + height - heightForQuestions) : y))
					{
						transitioning = false;
						characterAdvanceTimer = 90;
						setUpIcons();
						transitionX = x;
						transitionY = y;
						transitionWidth = width;
						transitionHeight = height;
					}
				}
				else
				{
					transitionX += (int)((float)time.ElapsedGameTime.Milliseconds * 3f);
					transitionY += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)height / (float)width));
					transitionX = Math.Min(x + width / 2, transitionX);
					transitionY = Math.Min(y + height / 2, transitionY);
					transitionWidth -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * 2f);
					transitionHeight -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)height / (float)width) * 2f);
					transitionWidth = Math.Max(0, transitionWidth);
					transitionHeight = Math.Max(0, transitionHeight);
					float num2 = (float)transitionWidth / (float)width;
					transitionX = x + (width - (int)(num2 * (float)width)) / 2;
					transitionY = y + (height - (int)(num2 * (float)height)) / 2;
					if (transitionWidth == 0 && transitionHeight == 0)
					{
						closeDialogue();
					}
				}
			}
			if (!transitioning && characterIndexInDialogue < getCurrentString().Length)
			{
				characterAdvanceTimer -= time.ElapsedGameTime.Milliseconds;
				if (characterAdvanceTimer <= 0)
				{
					characterAdvanceTimer = 30;
					int num3 = characterIndexInDialogue;
					characterIndexInDialogue = Math.Min(characterIndexInDialogue + 1, getCurrentString().Length);
					if (characterIndexInDialogue != num3 && characterIndexInDialogue == getCurrentString().Length)
					{
						Game1.playSound("dialogueCharacterClose");
					}
					if (characterIndexInDialogue > 1 && characterIndexInDialogue < getCurrentString().Length && Game1.options.dialogueTyping)
					{
						Game1.playSound("dialogueCharacter");
					}
				}
			}
			if (!transitioning && dialogueIcon != null)
			{
				dialogueIcon.update(time);
			}
			if (!transitioning && newPortaitShakeTimer > 0)
			{
				newPortaitShakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			width = GetWidth();
			height = 384;
			x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
			y = Game1.uiViewport.Height - height - 32;
			friendshipJewel = new Rectangle(x + width - 64, y + 256, 44, 44);
			setUpIcons();
		}

		public override void draw(SpriteBatch b)
		{
			if (transitioning)
			{
				drawBox(b, transitionX, transitionY, transitionWidth, transitionHeight);
				SpriteText.shrinkFont(shrink: false);
				return;
			}
			if (isQuestion)
			{
				TestToShrinkFont();
				drawBox(b, x, y - (heightForQuestions - height), width, heightForQuestions);
				SpriteText.drawString(b, getCurrentString(), x + 8, y + 12 - (heightForQuestions - height), characterIndexInDialogue, width - 16);
				if (characterIndexInDialogue >= getCurrentString().Length - 1)
				{
					int num = y - (heightForQuestions - height) + SpriteText.getHeightOfString(getCurrentString(), width - 48) + 48;
					for (int i = 0; i < responses.Count; i++)
					{
						if (i == selectedResponse)
						{
							IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(267, 256, 10, 10), x + 4 - 4 + 12, num - 16, width - 8 - 24, SpriteText.getHeightOfString(responses[i].responseText, width - 80) + 32, Color.White, 4f, drawShadow: false);
							SpriteText.drawString(b, responses[i].responseText, Utility.To4(x + 40), Utility.To4(num + 4 - ((responses.Count > 2) ? 4 : 0)), 999999, width - 80, 999999, 0.6f);
							num += SpriteText.getHeightOfString(responses[i].responseText, width - 80) + 16 + 32;
						}
						else
						{
							IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(256, 256, 10, 10), x + 4 + 12, num - 16, width - 8 - 24, SpriteText.getHeightOfString(responses[i].responseText, width - 80) + 32, Color.White, 4f);
							SpriteText.drawString(b, responses[i].responseText, Utility.To4(x + 44), Utility.To4(num - ((responses.Count > 2) ? 4 : 0)), 999999, width - 80);
							num += SpriteText.getHeightOfString(responses[i].responseText, width - 80) + 16 + 32;
						}
					}
				}
			}
			else
			{
				drawBox(b, x, y, width, height);
				if (!isPortraitBox() && !isQuestion)
				{
					SpriteText.shrinkFont(shrink: false);
					SpriteText.drawString(b, getCurrentString(), x + 8, y + 12, characterIndexInDialogue, width - 8 - 16 + 12);
				}
			}
			if (isPortraitBox() && !isQuestion)
			{
				SpriteText.shrinkFont(shrink: false);
				drawPortrait(b);
				if (!isQuestion)
				{
					SpriteText.drawString(b, getCurrentString(), x + 8, y + 12, characterIndexInDialogue, width - 8 - 16 - 460);
				}
			}
			if (aboveDialogueImage != null)
			{
				drawBox(b, x + width / 2 - (int)((float)(aboveDialogueImage.sourceRect.Width / 2) * aboveDialogueImage.scale), y - 64 - 4 - (int)((float)aboveDialogueImage.sourceRect.Height * aboveDialogueImage.scale), (int)((float)aboveDialogueImage.sourceRect.Width * aboveDialogueImage.scale), (int)((float)aboveDialogueImage.sourceRect.Height * aboveDialogueImage.scale) + 8);
				Utility.drawWithShadow(b, aboveDialogueImage.texture, new Vector2((float)(x + width / 2) - (float)(aboveDialogueImage.sourceRect.Width / 2) * aboveDialogueImage.scale, y - 64 - (int)((float)aboveDialogueImage.sourceRect.Height * aboveDialogueImage.scale)), aboveDialogueImage.sourceRect, Color.White, 0f, Vector2.Zero, aboveDialogueImage.scale, flipped: false, 1f);
			}
			if (hoverText.Length > 0)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, hoverText, friendshipJewel.Center.X - SpriteText.getWidthOfString(hoverText) / 2, friendshipJewel.Y - 64);
			}
			SpriteText.shrinkFont(shrink: false);
		}

		private bool TestToShrinkFont()
		{
			if (Game1.clientBounds.Height <= 1080 || (responses != null && responses.Count > 2))
			{
				SpriteText.shrinkFont(shrink: true);
				return true;
			}
			return false;
		}

		public static int GetWidth()
		{
			if (Game1.clientBounds.Width >= 1280 && Game1.toolbar != null && Game1.options.verticalToolbar && Game1.displayHUD)
			{
				if (Game1.clientBounds.Width > 2000)
				{
					return Math.Min((int)((double)Game1.clientBounds.Width * 0.5), Game1.uiViewport.Width - (Game1.toolbarPaddingX + Game1.toolbar.itemSlotSize + 28) * 2);
				}
				return Game1.uiViewport.Width - (Game1.toolbarPaddingX + Game1.toolbar.itemSlotSize + 28) * 2;
			}
			return Game1.uiViewport.Width - Game1.xEdge * 2 - 64;
		}

		public Rectangle getBounds()
		{
			return new Rectangle(x, y, width, height);
		}
	}
}
