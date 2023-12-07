using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Projectiles;

namespace StardewValley.Tools
{
	public class Slingshot : Tool
	{
		public const int basicDamage = 5;

		public const int basicSlingshot = 32;

		public const int masterSlingshot = 33;

		public const int galaxySlingshot = 34;

		public const int drawBackSoundThreshold = 8;

		[XmlIgnore]
		public int recentClickX;

		[XmlIgnore]
		public int recentClickY;

		[XmlIgnore]
		public int lastClickX;

		[XmlIgnore]
		public int lastClickY;

		[XmlIgnore]
		public int mouseDragAmount;

		[XmlIgnore]
		public Vector2 initialClick;

		[XmlIgnore]
		public double pullStartTime = -1.0;

		[XmlIgnore]
		public float nextAutoFire = -1f;

		private bool canPlaySound = true;

		[XmlIgnore]
		private readonly NetEvent0 finishEvent = new NetEvent0();

		[XmlIgnore]
		public readonly NetPoint aimPos = new NetPoint().Interpolated(interpolate: true, wait: true);

		public Slingshot()
		{
			base.InitialParentTileIndex = 32;
			base.CurrentParentTileIndex = base.InitialParentTileIndex;
			base.IndexOfMenuItemView = base.CurrentParentTileIndex;
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
			string[] array = dictionary[initialParentTileIndex].Split('/');
			base.BaseName = array[0];
			numAttachmentSlots.Value = 1;
			attachments.SetCount(1);
		}

		public override Item getOne()
		{
			Slingshot slingshot = new Slingshot(base.InitialParentTileIndex);
			CopyEnchantments(this, slingshot);
			slingshot._GetOneFrom(this);
			return slingshot;
		}

		protected override string loadDisplayName()
		{
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
			string[] array = dictionary[initialParentTileIndex].Split('/');
			if (LocalizedContentManager.CurrentLanguageCode != 0)
			{
				return array[array.Length - 1];
			}
			return Name;
		}

		protected override string loadDescription()
		{
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
			string[] array = dictionary[initialParentTileIndex].Split('/');
			return array[1];
		}

		public override bool doesShowTileLocationMarker()
		{
			return false;
		}

		public Slingshot(int which = 32)
		{
			base.InitialParentTileIndex = which;
			base.CurrentParentTileIndex = base.InitialParentTileIndex;
			base.IndexOfMenuItemView = base.CurrentParentTileIndex;
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
			string[] array = dictionary[initialParentTileIndex].Split('/');
			base.BaseName = array[0];
			numAttachmentSlots.Value = 1;
			attachments.SetCount(1);
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(finishEvent, aimPos);
			finishEvent.onEvent += doFinish;
		}

		public int GetBackArmDistance(Farmer who)
		{
			if (CanAutoFire() && nextAutoFire > 0f)
			{
				return (int)Utility.Lerp(20f, 0f, nextAutoFire / GetAutoFireRate());
			}
			if (!Game1.options.useLegacySlingshotFiring)
			{
				return (int)(20f * GetSlingshotChargeTime());
			}
			return Math.Min(20, (int)Vector2.Distance(who.getStandingPosition(), new Vector2(aimPos.X, aimPos.Y)) / 20);
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.IndexOfMenuItemView = base.InitialParentTileIndex;
			if (!CanAutoFire())
			{
				PerformFire(location, who);
			}
			finish();
		}

		public virtual void PerformFire(GameLocation location, Farmer who)
		{
			if (attachments[0] != null)
			{
				updateAimPos();
				int x = aimPos.X;
				int y = aimPos.Y;
				int backArmDistance = GetBackArmDistance(who);
				Vector2 shootOrigin = GetShootOrigin(who);
				Vector2 velocityTowardPoint = Utility.getVelocityTowardPoint(GetShootOrigin(who), AdjustForHeight(new Vector2(x, y)), (float)(15 + Game1.random.Next(4, 6)) * (1f + who.weaponSpeedModifier));
				if (backArmDistance > 4 && !canPlaySound)
				{
					Object @object = (Object)attachments[0].getOne();
					attachments[0].Stack--;
					if (attachments[0].Stack <= 0)
					{
						attachments[0] = null;
					}
					int num = 1;
					BasicProjectile.onCollisionBehavior collisionBehavior = null;
					string collisionSound = "hammer";
					float num2 = 1f;
					if (base.InitialParentTileIndex == 33)
					{
						num2 = 2f;
					}
					else if (base.InitialParentTileIndex == 34)
					{
						num2 = 4f;
					}
					switch (@object.ParentSheetIndex)
					{
					case 388:
						num = 2;
						@object.ParentSheetIndex++;
						break;
					case 390:
						num = 5;
						@object.ParentSheetIndex++;
						break;
					case 378:
						num = 10;
						@object.ParentSheetIndex++;
						break;
					case 380:
						num = 20;
						@object.ParentSheetIndex++;
						break;
					case 384:
						num = 30;
						@object.ParentSheetIndex++;
						break;
					case 382:
						num = 15;
						@object.ParentSheetIndex++;
						break;
					case 386:
						num = 50;
						@object.ParentSheetIndex++;
						break;
					case 441:
						num = 20;
						collisionBehavior = BasicProjectile.explodeOnImpact;
						collisionSound = "explosion";
						break;
					}
					int num3 = @object.Category;
					if (num3 == -5)
					{
						collisionSound = "slimedead";
					}
					if (!Game1.options.useLegacySlingshotFiring)
					{
						velocityTowardPoint.X *= -1f;
						velocityTowardPoint.Y *= -1f;
					}
					location.projectiles.Add(new BasicProjectile((int)(num2 * (float)(num + Game1.random.Next(-(num / 2), num + 2)) * (1f + who.attackIncreaseModifier)), @object.ParentSheetIndex, 0, 0, (float)(Math.PI / (double)(64f + (float)Game1.random.Next(-63, 64))), 0f - velocityTowardPoint.X, 0f - velocityTowardPoint.Y, shootOrigin - new Vector2(32f, 32f), collisionSound, "", explode: false, damagesMonsters: true, location, who, spriteFromObjectSheet: true, collisionBehavior)
					{
						IgnoreLocationCollision = (Game1.currentLocation.currentEvent != null || Game1.currentMinigame != null)
					});
				}
			}
			else
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14254"));
			}
			canPlaySound = true;
		}

		public Vector2 GetShootOrigin(Farmer who)
		{
			return AdjustForHeight(new Vector2(who.getStandingX(), who.getStandingY()), for_cursor: false);
		}

		public Vector2 AdjustForHeight(Vector2 position, bool for_cursor = true)
		{
			if (!Game1.options.useLegacySlingshotFiring && for_cursor)
			{
				return new Vector2(position.X, position.Y);
			}
			return new Vector2(position.X, position.Y - 32f - 8f);
		}

		public void finish()
		{
			finishEvent.Fire();
		}

		private void doFinish()
		{
			if (lastUser != null)
			{
				lastUser.usingSlingshot = false;
				lastUser.canReleaseTool = true;
				lastUser.UsingTool = false;
				lastUser.canMove = true;
				lastUser.Halt();
				if (lastUser == Game1.player && Game1.options.gamepadControls)
				{
					Game1.game1.controllerSlingshotSafeTime = 0.2f;
				}
			}
		}

		public override bool canThisBeAttached(Object o)
		{
			if (o == null || (!o.bigCraftable && (((int)o.parentSheetIndex >= 378 && (int)o.parentSheetIndex <= 390) || o.Category == -5 || o.Category == -79 || o.Category == -75 || (int)o.parentSheetIndex == 441)))
			{
				return true;
			}
			return false;
		}

		public override Object attach(Object o)
		{
			Object result = attachments[0];
			attachments[0] = o;
			Game1.playSound("button1");
			return result;
		}

		public override string getHoverBoxText(Item hoveredItem)
		{
			return null;
		}

		public override bool onRelease(GameLocation location, int x, int y, Farmer who)
		{
			DoFunction(location, x, y, 1, who);
			return true;
		}

		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			who.usingSlingshot = true;
			who.canReleaseTool = false;
			mouseDragAmount = 0;
			int num = ((who.FacingDirection == 3 || who.FacingDirection == 1) ? 1 : ((who.FacingDirection == 0) ? 2 : 0));
			who.FarmerSprite.setCurrentFrame(42 + num);
			if (!who.IsLocalPlayer)
			{
				return true;
			}
			Game1.oldMouseState = Game1.input.GetMouseState();
			Game1.lastMousePositionBeforeFade = Game1.getMousePosition();
			lastClickX = Game1.getOldMouseX() + Game1.viewport.X;
			lastClickY = Game1.getOldMouseY() + Game1.viewport.Y;
			initialClick = new Vector2(lastClickX, lastClickY);
			pullStartTime = Game1.currentGameTime.TotalGameTime.TotalSeconds;
			if (CanAutoFire())
			{
				nextAutoFire = -1f;
			}
			updateAimPos();
			return true;
		}

		public virtual float GetAutoFireRate()
		{
			return 0.3f;
		}

		public virtual bool CanAutoFire()
		{
			return false;
		}

		internal void updateAimPos()
		{
			if (lastUser == null || !lastUser.IsLocalPlayer)
			{
				return;
			}
			Point point = Game1.getMousePosition();
			if (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse)
			{
				Vector2 vector = Game1.oldPadState.ThumbSticks.Left;
				if (vector.Length() < 0.25f)
				{
					vector.X = 0f;
					vector.Y = 0f;
					if (Game1.oldPadState.DPad.Down == ButtonState.Pressed)
					{
						vector.Y = -1f;
					}
					else if (Game1.oldPadState.DPad.Up == ButtonState.Pressed)
					{
						vector.Y = 1f;
					}
					if (Game1.oldPadState.DPad.Left == ButtonState.Pressed)
					{
						vector.X = -1f;
					}
					if (Game1.oldPadState.DPad.Right == ButtonState.Pressed)
					{
						vector.X = 1f;
					}
					if (vector.X != 0f && vector.Y != 0f)
					{
						vector.Normalize();
						vector *= 1f;
					}
				}
				Vector2 shootOrigin = GetShootOrigin(lastUser);
				if (!Game1.options.useLegacySlingshotFiring && vector.Length() < 0.25f)
				{
					if ((int)lastUser.facingDirection == 3)
					{
						vector = new Vector2(-1f, 0f);
					}
					else if ((int)lastUser.facingDirection == 1)
					{
						vector = new Vector2(1f, 0f);
					}
					else if ((int)lastUser.facingDirection == 0)
					{
						vector = new Vector2(0f, 1f);
					}
					else if ((int)lastUser.facingDirection == 2)
					{
						vector = new Vector2(0f, -1f);
					}
				}
				point = Utility.Vector2ToPoint(shootOrigin + new Vector2(vector.X, 0f - vector.Y) * 600f);
				point.X -= Game1.viewport.X;
				point.Y -= Game1.viewport.Y;
			}
			int x = point.X + Game1.viewport.X;
			int y = point.Y + Game1.viewport.Y;
			aimPos.X = x;
			aimPos.Y = y;
		}

		public override void tickUpdate(GameTime time, Farmer who)
		{
			lastUser = who;
			finishEvent.Poll();
			if (!who.usingSlingshot)
			{
				return;
			}
			if (who.IsLocalPlayer)
			{
				updateAimPos();
				int x = aimPos.X;
				int y = aimPos.Y;
				Game1.debugOutput = "playerPos: " + who.getStandingPosition().ToString() + ", mousePos: " + x + ", " + y;
				mouseDragAmount++;
				if (!Game1.options.useLegacySlingshotFiring)
				{
					Vector2 shootOrigin = GetShootOrigin(who);
					Vector2 vector = AdjustForHeight(new Vector2(x, y));
					Vector2 vector2 = vector - shootOrigin;
					if (Math.Abs(vector2.X) > Math.Abs(vector2.Y))
					{
						if (vector2.X < 0f)
						{
							who.faceDirection(3);
						}
						if (vector2.X > 0f)
						{
							who.faceDirection(1);
						}
					}
					else
					{
						if (vector2.Y < 0f)
						{
							who.faceDirection(0);
						}
						if (vector2.Y > 0f)
						{
							who.faceDirection(2);
						}
					}
				}
				else
				{
					who.faceGeneralDirection(new Vector2(x, y + 48), 0, opposite: true);
				}
				if (!Game1.options.useLegacySlingshotFiring)
				{
					if (canPlaySound && GetSlingshotChargeTime() >= 1f)
					{
						who.currentLocation.playSound("slingshot");
						canPlaySound = false;
					}
				}
				else if (canPlaySound && (Math.Abs(x - lastClickX) > 8 || Math.Abs(y - lastClickY) > 8) && mouseDragAmount > 4)
				{
					who.currentLocation.playSound("slingshot");
					canPlaySound = false;
				}
				if (!CanAutoFire())
				{
					lastClickX = x;
					lastClickY = y;
				}
				if (Game1.options.useLegacySlingshotFiring)
				{
					Game1.mouseCursor = -1;
				}
				if (CanAutoFire())
				{
					bool flag = false;
					if (GetBackArmDistance(who) >= 20 && nextAutoFire < 0f)
					{
						nextAutoFire = 0f;
						flag = true;
					}
					if (nextAutoFire > 0f || flag)
					{
						nextAutoFire -= (float)time.ElapsedGameTime.TotalSeconds;
						if (nextAutoFire <= 0f)
						{
							PerformFire(who.currentLocation, who);
							nextAutoFire = GetAutoFireRate();
						}
					}
				}
			}
			int num = ((who.FacingDirection == 3 || who.FacingDirection == 1) ? 1 : ((who.FacingDirection == 0) ? 2 : 0));
			who.FarmerSprite.setCurrentFrame(42 + num);
		}

		public override void drawAttachments(SpriteBatch b, int x, int y)
		{
			if (attachments[0] == null)
			{
				b.Draw(Game1.menuTexture, new Vector2(x, y), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 43), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
				return;
			}
			b.Draw(Game1.menuTexture, new Vector2(x, y), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
			attachments[0].drawInMenu(b, new Vector2(x, y), 1f);
		}

		public float GetSlingshotChargeTime()
		{
			if (pullStartTime < 0.0)
			{
				return 0f;
			}
			return Utility.Clamp((float)((Game1.currentGameTime.TotalGameTime.TotalSeconds - pullStartTime) / (double)GetRequiredChargeTime()), 0f, 1f);
		}

		public float GetRequiredChargeTime()
		{
			return 0.3f;
		}

		public override void draw(SpriteBatch b)
		{
			if (lastUser.usingSlingshot && lastUser.IsLocalPlayer && !canPlaySound && mouseDragAmount > 4)
			{
				int x = aimPos.X;
				int y = aimPos.Y;
				Vector2 shootOrigin = GetShootOrigin(lastUser);
				Vector2 velocityTowardPoint = Utility.getVelocityTowardPoint(shootOrigin, AdjustForHeight(new Vector2(x, y)), 256f);
				double num = Math.Sqrt(velocityTowardPoint.X * velocityTowardPoint.X + velocityTowardPoint.Y * velocityTowardPoint.Y);
				double num2 = num - 181.0;
				double num3 = velocityTowardPoint.X / 256f;
				double num4 = velocityTowardPoint.Y / 256f;
				int num5 = (int)((double)velocityTowardPoint.X - num2 * num3);
				int num6 = (int)((double)velocityTowardPoint.Y - num2 * num4);
				if (!Game1.options.useLegacySlingshotFiring)
				{
					num5 *= -1;
					num6 *= -1;
				}
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(shootOrigin.X - (float)num5, shootOrigin.Y - (float)num6)), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 43), Color.White, 0f, new Vector2(32f, 32f), 1f, SpriteEffects.None, 0.999999f);
			}
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			if (base.IndexOfMenuItemView == 0 || base.IndexOfMenuItemView == 21 || base.IndexOfMenuItemView == 47 || base.CurrentParentTileIndex == 47)
			{
				switch (base.BaseName)
				{
				case "Slingshot":
					base.CurrentParentTileIndex = 32;
					break;
				case "Master Slingshot":
					base.CurrentParentTileIndex = 33;
					break;
				case "Galaxy Slingshot":
					base.CurrentParentTileIndex = 34;
					break;
				}
				base.IndexOfMenuItemView = base.CurrentParentTileIndex;
			}
			int num = (int)(64f * scaleSize);
			int num2 = 4;
			Vector2 zero = Vector2.Zero;
			Rectangle sourceRectForStandardTileSheet = Game1.getSourceRectForStandardTileSheet(Tool.weaponsTexture, base.IndexOfMenuItemView, 16, 16);
			spriteBatch.Draw(Tool.weaponsTexture, location + new Vector2(num2, num2), sourceRectForStandardTileSheet, color * transparency, 0f, zero, 4f * scaleSize, SpriteEffects.None, layerDepth);
			if (drawStackNumber != 0 && attachments != null && attachments[0] != null)
			{
				Utility.drawTinyDigits(attachments[0].Stack, spriteBatch, location + new Vector2((float)(base.itemSlotSize - Utility.getWidthOfTinyDigitString(attachments[0].Stack, 3f * scaleSize)) - 3f * scaleSize, (float)base.itemSlotSize - 24f * scaleSize), 3f * scaleSize, 1f, Color.White);
			}
		}
	}
}
