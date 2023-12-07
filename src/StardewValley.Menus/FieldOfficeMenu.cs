using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	public class FieldOfficeMenu : MenuWithInventory
	{
		private Texture2D fieldOfficeMenuTexture;

		private IslandFieldOffice office;

		private bool madeADonation;

		private bool gotReward;

		public List<ClickableComponent> pieceHolders = new List<ClickableComponent>();

		private new int width;

		private new int height;

		private Rectangle donationRec;

		private float panelWidthRatio;

		private float panelHightRatio;

		private float bearTimer;

		private float snakeTimer;

		private float batTimer;

		private float frogTimer;

		public FieldOfficeMenu(IslandFieldOffice office)
			: base(highlightBones, okButton: true, trashCan: true, Game1.xEdge)
		{
			FieldOfficeMenu fieldOfficeMenu = this;
			this.office = office;
			fieldOfficeMenuTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\FieldOfficeDonationMenu");
			width = Game1.uiViewport.Width - Game1.xEdge * 2;
			height = Game1.uiViewport.Height;
			float num = 2.55f;
			int num2 = height / 2 - 20;
			int num3 = (int)((float)num2 * num);
			Point point = new Point(width / 2, height / 4);
			donationRec = new Rectangle(point.X - num3 / 2 - 10, point.Y - num2 / 2 + 10, num3, num2);
			panelWidthRatio = (float)donationRec.Width / 204f;
			panelHightRatio = (float)num2 / 80f;
			pieceHolders.Add(new ClickableComponent(new Rectangle((int)((float)donationRec.X + 19f * panelWidthRatio), (int)((float)donationRec.Y + 45f * panelHightRatio), (int)(15f * panelWidthRatio), (int)(15f * panelWidthRatio)), office.piecesDonated[0] ? new Object(823, 1) : null)
			{
				label = "823"
			});
			pieceHolders.Add(new ClickableComponent(new Rectangle((int)((float)donationRec.X + 36f * panelWidthRatio), (int)((float)donationRec.Y + 45f * panelHightRatio), (int)(15f * panelWidthRatio), (int)(15f * panelWidthRatio)), office.piecesDonated[1] ? new Object(824, 1) : null)
			{
				label = "824"
			});
			pieceHolders.Add(new ClickableComponent(new Rectangle((int)((float)donationRec.X + 53f * panelWidthRatio), (int)((float)donationRec.Y + 45f * panelHightRatio), (int)(15f * panelWidthRatio), (int)(15f * panelWidthRatio)), office.piecesDonated[2] ? new Object(823, 1) : null)
			{
				label = "823"
			});
			pieceHolders.Add(new ClickableComponent(new Rectangle((int)((float)donationRec.X + 19f * panelWidthRatio), (int)((float)donationRec.Y + 28f * panelHightRatio), (int)(15f * panelWidthRatio), (int)(15f * panelWidthRatio)), office.piecesDonated[3] ? new Object(822, 1) : null)
			{
				label = "822"
			});
			pieceHolders.Add(new ClickableComponent(new Rectangle((int)((float)donationRec.X + 36f * panelWidthRatio), (int)((float)donationRec.Y + 28f * panelHightRatio), (int)(15f * panelWidthRatio), (int)(15f * panelWidthRatio)), office.piecesDonated[4] ? new Object(821, 1) : null)
			{
				label = "821"
			});
			pieceHolders.Add(new ClickableComponent(new Rectangle((int)((float)donationRec.X + 53f * panelWidthRatio), (int)((float)donationRec.Y + 28f * panelHightRatio), (int)(15f * panelWidthRatio), (int)(15f * panelWidthRatio)), office.piecesDonated[5] ? new Object(820, 1) : null)
			{
				label = "820"
			});
			pieceHolders.Add(new ClickableComponent(new Rectangle((int)((float)donationRec.X + 103f * panelWidthRatio), (int)((float)donationRec.Y + 12f * panelHightRatio), (int)(15f * panelWidthRatio), (int)(15f * panelWidthRatio)), office.piecesDonated[6] ? new Object(826, 1) : null)
			{
				label = "826"
			});
			pieceHolders.Add(new ClickableComponent(new Rectangle((int)((float)donationRec.X + 103f * panelWidthRatio), (int)((float)donationRec.Y + 32f * panelHightRatio), (int)(15f * panelWidthRatio), (int)(15f * panelWidthRatio)), office.piecesDonated[7] ? new Object(826, 1) : null)
			{
				label = "826"
			});
			pieceHolders.Add(new ClickableComponent(new Rectangle((int)((float)donationRec.X + 103f * panelWidthRatio), (int)((float)donationRec.Y + 52f * panelHightRatio), (int)(15f * panelWidthRatio), (int)(15f * panelWidthRatio)), office.piecesDonated[8] ? new Object(825, 1) : null)
			{
				label = "825"
			});
			pieceHolders.Add(new ClickableComponent(new Rectangle((int)((float)donationRec.X + 154f * panelWidthRatio), (int)((float)donationRec.Y + 9f * panelHightRatio), (int)(15f * panelWidthRatio), (int)(15f * panelWidthRatio)), office.piecesDonated[9] ? new Object(827, 1) : null)
			{
				label = "827"
			});
			pieceHolders.Add(new ClickableComponent(new Rectangle((int)((float)donationRec.X + 156f * panelWidthRatio), (int)((float)donationRec.Y + 39f * panelHightRatio), (int)(15f * panelWidthRatio), (int)(15f * panelWidthRatio)), office.piecesDonated[10] ? new Object(828, 1) : null)
			{
				label = "828"
			});
			if (Game1.activeClickableMenu == null)
			{
				Game1.playSound("bigSelect");
			}
			for (int i = 0; i < pieceHolders.Count; i++)
			{
				ClickableComponent clickableComponent = pieceHolders[i];
				clickableComponent.upNeighborID = (clickableComponent.downNeighborID = (clickableComponent.rightNeighborID = (clickableComponent.leftNeighborID = -99998)));
				clickableComponent.myID = 1000 + i;
			}
			foreach (ClickableComponent item in inventory.GetBorder(InventoryMenu.BorderSide.Top))
			{
				item.upNeighborID = -99998;
			}
			foreach (ClickableComponent item2 in inventory.GetBorder(InventoryMenu.BorderSide.Right))
			{
				item2.rightNeighborID = 4857;
				item2.rightNeighborImmutable = true;
			}
			populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				snapToDefaultClickableComponent();
			}
			if (trashCan != null)
			{
				trashCan.leftNeighborID = (okButton.leftNeighborID = 11);
			}
			exitFunction = delegate
			{
				if (fieldOfficeMenu.madeADonation)
				{
					string text = (fieldOfficeMenu.gotReward ? ("#$b#" + Game1.content.LoadString("Strings\\Locations:FieldOfficeDonated_Reward")) : "");
					Game1.drawDialogue(office.getSafariGuy(), Game1.content.LoadString("Strings\\Locations:FieldOfficeDonated_" + Game1.random.Next(4)) + text);
					if (fieldOfficeMenu.gotReward)
					{
						Game1.multiplayer.globalChatInfoMessage("FieldOfficeCompleteSet", Game1.player.Name);
					}
				}
			};
		}

		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (b.myID == 5948 && b.myID != 4857)
			{
				return false;
			}
			return base.IsAutomaticSnapValid(direction, a, b);
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public static bool highlightBones(Item i)
		{
			if (i != null)
			{
				IslandFieldOffice islandFieldOffice = (IslandFieldOffice)Game1.getLocationFromName("IslandFieldOffice");
				switch ((int)i.parentSheetIndex)
				{
				case 820:
					if (!islandFieldOffice.piecesDonated[5])
					{
						return true;
					}
					break;
				case 821:
					if (!islandFieldOffice.piecesDonated[4])
					{
						return true;
					}
					break;
				case 822:
					if (!islandFieldOffice.piecesDonated[3])
					{
						return true;
					}
					break;
				case 823:
					if (!islandFieldOffice.piecesDonated[0] || !islandFieldOffice.piecesDonated[2])
					{
						return true;
					}
					break;
				case 824:
					if (!islandFieldOffice.piecesDonated[1])
					{
						return true;
					}
					break;
				case 825:
					if (!islandFieldOffice.piecesDonated[8])
					{
						return true;
					}
					break;
				case 826:
					if (!islandFieldOffice.piecesDonated[7] || !islandFieldOffice.piecesDonated[6])
					{
						return true;
					}
					break;
				case 827:
					if (!islandFieldOffice.piecesDonated[9])
					{
						return true;
					}
					break;
				case 828:
					if (!islandFieldOffice.piecesDonated[10])
					{
						return true;
					}
					break;
				}
			}
			return false;
		}

		public static int getPieceIndexForDonationItem(int itemIndex)
		{
			return itemIndex switch
			{
				820 => 5, 
				821 => 4, 
				822 => 3, 
				823 => 0, 
				824 => 1, 
				825 => 8, 
				826 => 7, 
				827 => 9, 
				828 => 10, 
				_ => -1, 
			};
		}

		public static int getDonationPieceIndexNeededForSpot(int donationSpotIndex)
		{
			switch (donationSpotIndex)
			{
			case 5:
				return 820;
			case 4:
				return 821;
			case 3:
				return 822;
			case 0:
			case 2:
				return 823;
			case 1:
				return 824;
			case 8:
				return 825;
			case 6:
			case 7:
				return 826;
			case 9:
				return 827;
			case 10:
				return 828;
			default:
				return -1;
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (heldItem == null)
			{
				return;
			}
			int pieceIndexForDonationItem = getPieceIndexForDonationItem(heldItem.parentSheetIndex);
			if (pieceIndexForDonationItem == -1)
			{
				return;
			}
			if ((int)heldItem.parentSheetIndex == 823)
			{
				if (!donate(0, x, y))
				{
					donate(2, x, y);
				}
			}
			else if ((int)heldItem.parentSheetIndex == 826)
			{
				if (!donate(7, x, y))
				{
					donate(6, x, y);
				}
			}
			else
			{
				donate(pieceIndexForDonationItem, x, y);
			}
		}

		protected override void cleanupBeforeExit()
		{
			base.cleanupBeforeExit();
			if (office != null && office.isRangeAllTrue(0, 11) && office.plantsRestoredRight.Value && office.plantsRestoredLeft.Value && !Game1.player.hasOrWillReceiveMail("fieldOfficeFinale"))
			{
				office.triggerFinaleCutscene();
			}
		}

		private bool donate(int index, int x, int y)
		{
			if (pieceHolders[index].containsPoint(x, y) && pieceHolders[index].item == null)
			{
				pieceHolders[index].item = new Object(heldItem.parentSheetIndex, 1);
				heldItem.Stack--;
				Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
				inventory.currentlySelectedItem = -1;
				if (heldItem.Stack <= 0)
				{
					heldItem = null;
				}
				Game1.playSound("newArtifact");
				checkForSetFinish();
				gotReward = office.donatePiece(index);
				Game1.multiplayer.globalChatInfoMessage("FieldOfficeDonation", Game1.player.Name, pieceHolders[index].item.DisplayName);
				madeADonation = true;
				return true;
			}
			return false;
		}

		public void checkForSetFinish()
		{
			if (!office.centerSkeletonRestored.Value && pieceHolders[0].item != null && pieceHolders[1].item != null && pieceHolders[2].item != null && pieceHolders[3].item != null && pieceHolders[4].item != null && pieceHolders[5].item != null)
			{
				DelayedAction.functionAfterDelay(delegate
				{
					bearTimer = 500f;
					Game1.playSound("camel");
				}, 700);
			}
			if (!office.snakeRestored.Value && pieceHolders[6].item != null && pieceHolders[7].item != null && pieceHolders[8].item != null)
			{
				DelayedAction.functionAfterDelay(delegate
				{
					snakeTimer = 1500f;
					Game1.playSound("steam");
				}, 700);
			}
			if (!office.batRestored.Value && pieceHolders[9].item != null)
			{
				DelayedAction.functionAfterDelay(delegate
				{
					batTimer = 1500f;
					Game1.playSound("batScreech");
				}, 700);
			}
			if (!office.frogRestored.Value && pieceHolders[10].item != null)
			{
				DelayedAction.functionAfterDelay(delegate
				{
					frogTimer = 1000f;
					Game1.playSound("croak");
				}, 700);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (bearTimer > 0f)
			{
				bearTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (snakeTimer > 0f)
			{
				snakeTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (batTimer > 0f)
			{
				batTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (frogTimer > 0f)
			{
				frogTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.75f);
			Game1.drawDialogueBox(-16, Game1.uiViewport.Height / 2 - 80, Game1.uiViewport.Width + 32, Game1.uiViewport.Height / 2 + 96, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe: true, 0, 80, 80);
			Game1.drawDialogueBox(donationRec.X - 32, -76, donationRec.Width + 64, donationRec.Height + 128, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe: true, 0, 80, 80);
			b.Draw(fieldOfficeMenuTexture, donationRec, new Rectangle(0, 0, 204, 80), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.1f);
			base.draw(b, drawUpperPortion: true, drawDescriptionArea: false, 0, 80, 80);
			b.Draw(fieldOfficeMenuTexture, new Vector2((float)(width - donationRec.X) - 35f * panelWidthRatio, (float)donationRec.Y + 7f * panelHightRatio + ((batTimer > 0f) ? ((float)Math.Sin((1500f - batTimer) / 80f) * panelWidthRatio / 4f) : 0f)), new Rectangle(68, 84, 30, 20), Color.White, 0f, Vector2.Zero, panelWidthRatio, SpriteEffects.None, 0.1f);
			foreach (ClickableComponent pieceHolder in pieceHolders)
			{
				if (pieceHolder.item != null)
				{
					Vector2 location = new Vector2(pieceHolder.bounds.X, pieceHolder.bounds.Y);
					float scaleSize = (float)pieceHolder.bounds.Width / (float)pieceHolder.item.itemSlotSize * 3f / 4f;
					pieceHolder.item.itemSlotSize = pieceHolder.bounds.Width;
					pieceHolder.item.drawInMenu(b, location, scaleSize, 1f, 0.0865f, StackDrawType.Draw);
					pieceHolder.item.itemSlotSize = 64;
				}
			}
			if (bearTimer > 0f)
			{
				b.Draw(fieldOfficeMenuTexture, new Vector2((float)donationRec.X + 60f * panelWidthRatio, (float)donationRec.Y + 9f * panelWidthRatio), new Rectangle(0, 81, 37, 29), Color.White, 0f, Vector2.Zero, panelWidthRatio, SpriteEffects.None, 0.1f);
			}
			else if (snakeTimer > 0f && snakeTimer / 300f % 2f != 0f)
			{
				b.Draw(fieldOfficeMenuTexture, new Vector2((float)donationRec.X + 121f * panelWidthRatio, (float)donationRec.Y + 58f * panelWidthRatio), new Rectangle(47, 84, 19, 19), Color.White, 0f, Vector2.Zero, panelWidthRatio, SpriteEffects.None, 0.1f);
			}
			else if (frogTimer > 0f)
			{
				b.Draw(fieldOfficeMenuTexture, new Vector2((float)donationRec.X + 177f * panelWidthRatio, (float)donationRec.Y + 35f * panelWidthRatio), new Rectangle(100, 89, 18, 7), Color.White, 0f, Vector2.Zero, panelWidthRatio, SpriteEffects.None, 0.1f);
			}
			if (heldItem != null)
			{
				int pieceIndexForDonationItem = getPieceIndexForDonationItem(heldItem.parentSheetIndex);
				if (pieceIndexForDonationItem != -1)
				{
					drawHighlightedSquare(pieceIndexForDonationItem, b);
				}
			}
			drawMouse(b);
			if (heldItem != null)
			{
				heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
			}
		}

		private void drawHighlightedSquare(int index, SpriteBatch b)
		{
			Rectangle value = default(Rectangle);
			switch ((int)heldItem.parentSheetIndex)
			{
			case 820:
			case 821:
			case 822:
			case 823:
			case 824:
				value = new Rectangle(119, 86, 18, 18);
				break;
			case 825:
			case 826:
				value = new Rectangle(138, 86, 18, 18);
				break;
			case 827:
				value = new Rectangle(157, 86, 18, 18);
				break;
			case 828:
				value = new Rectangle(176, 86, 18, 18);
				break;
			}
			if (pieceHolders[index].item == null)
			{
				b.Draw(fieldOfficeMenuTexture, Utility.PointToVector2(pieceHolders[index].bounds.Location) + new Vector2(-1f, -1f) * 4f, value, Color.White, 0f, Vector2.Zero, panelWidthRatio, SpriteEffects.None, 0.1f);
			}
			if ((int)heldItem.parentSheetIndex == 823 && index == 0 && pieceHolders[2].item == null)
			{
				b.Draw(fieldOfficeMenuTexture, Utility.PointToVector2(pieceHolders[2].bounds.Location) + new Vector2(-1f, -1f) * 4f, value, Color.White, 0f, Vector2.Zero, panelWidthRatio, SpriteEffects.None, 0.1f);
			}
			if ((int)heldItem.parentSheetIndex == 826 && index == 7 && pieceHolders[6].item == null)
			{
				b.Draw(fieldOfficeMenuTexture, Utility.PointToVector2(pieceHolders[6].bounds.Location) + new Vector2(-1f, -1f) * 4f, value, Color.White, 0f, Vector2.Zero, panelWidthRatio, SpriteEffects.None, 0.1f);
			}
		}
	}
}
