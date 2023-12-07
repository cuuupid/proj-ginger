using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Tools;

namespace StardewValley
{
	[XmlInclude(typeof(MagnifyingGlass))]
	[XmlInclude(typeof(Shears))]
	[XmlInclude(typeof(MilkPail))]
	[XmlInclude(typeof(Axe))]
	[XmlInclude(typeof(Wand))]
	[XmlInclude(typeof(Hoe))]
	[XmlInclude(typeof(FishingRod))]
	[XmlInclude(typeof(MeleeWeapon))]
	[XmlInclude(typeof(Pan))]
	[XmlInclude(typeof(Pickaxe))]
	[XmlInclude(typeof(WateringCan))]
	[XmlInclude(typeof(Slingshot))]
	[XmlInclude(typeof(GenericTool))]
	public abstract class Tool : Item
	{
		public const int standardStaminaReduction = 2;

		public const int nonUpgradeable = -1;

		public const int stone = 0;

		public const int copper = 1;

		public const int steel = 2;

		public const int gold = 3;

		public const int iridium = 4;

		public const int parsnipSpriteIndex = 0;

		public const int hoeSpriteIndex = 21;

		public const int hammerSpriteIndex = 105;

		public const int axeSpriteIndex = 189;

		public const int wateringCanSpriteIndex = 273;

		public const int fishingRodSpriteIndex = 8;

		public const int batteredSwordSpriteIndex = 67;

		public const int axeMenuIndex = 215;

		public const int hoeMenuIndex = 47;

		public const int pickAxeMenuIndex = 131;

		public const int wateringCanMenuIndex = 296;

		public const int startOfNegativeWeaponIndex = -10000;

		public const string weaponsTextureName = "TileSheets\\weapons";

		public static Texture2D weaponsTexture;

		[XmlElement("initialParentTileIndex")]
		public readonly NetInt initialParentTileIndex = new NetInt();

		[XmlElement("currentParentTileIndex")]
		public readonly NetInt currentParentTileIndex = new NetInt();

		[XmlElement("indexOfMenuItemView")]
		public readonly NetInt indexOfMenuItemView = new NetInt();

		[XmlElement("stackable")]
		public readonly NetBool stackable = new NetBool();

		[XmlElement("instantUse")]
		public readonly NetBool instantUse = new NetBool();

		[XmlElement("isEfficient")]
		public readonly NetBool isEfficient = new NetBool();

		[XmlElement("animationSpeedModifier")]
		public readonly NetFloat animationSpeedModifier = new NetFloat(1f);

		[XmlIgnore]
		private string _description;

		public static Color copperColor = new Color(198, 108, 43);

		public static Color steelColor = new Color(197, 226, 222);

		public static Color goldColor = new Color(248, 255, 73);

		public static Color iridiumColor = new Color(144, 135, 181);

		[XmlElement("upgradeLevel")]
		public readonly NetInt upgradeLevel = new NetInt();

		[XmlElement("numAttachmentSlots")]
		public readonly NetInt numAttachmentSlots = new NetInt();

		protected Farmer lastUser;

		public readonly NetObjectArray<Object> attachments = new NetObjectArray<Object>();

		[XmlIgnore]
		protected string displayName;

		[XmlElement("enchantments")]
		public readonly NetList<BaseEnchantment, NetRef<BaseEnchantment>> enchantments = new NetList<BaseEnchantment, NetRef<BaseEnchantment>>();

		[XmlElement("previousEnchantments")]
		public readonly NetStringList previousEnchantments = new NetStringList();

		[XmlIgnore]
		public string description
		{
			get
			{
				if (_description == null)
				{
					_description = loadDescription();
				}
				return _description;
			}
			set
			{
				_description = value;
			}
		}

		public string BaseName
		{
			get
			{
				return netName;
			}
			set
			{
				netName.Set(value);
			}
		}

		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				displayName = loadDisplayName();
				return (int)upgradeLevel switch
				{
					1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14299", displayName), 
					2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14300", displayName), 
					3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14301", displayName), 
					4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14302", displayName), 
					_ => displayName, 
				};
			}
			set
			{
				displayName = value;
			}
		}

		public override string Name
		{
			get
			{
				return (int)upgradeLevel switch
				{
					1 => "Copper " + BaseName, 
					2 => "Steel " + BaseName, 
					3 => "Gold " + BaseName, 
					4 => "Iridium " + BaseName, 
					_ => BaseName, 
				};
			}
			set
			{
				BaseName = value;
			}
		}

		public override int Stack
		{
			get
			{
				if ((bool)stackable)
				{
					return ((Stackable)this).NumberInStack;
				}
				return 1;
			}
			set
			{
				if ((bool)stackable)
				{
					((Stackable)this).Stack = Math.Min(Math.Max(0, value), maximumStackSize());
				}
			}
		}

		public string Description => description;

		[XmlIgnore]
		public int CurrentParentTileIndex
		{
			get
			{
				return currentParentTileIndex;
			}
			set
			{
				currentParentTileIndex.Set(value);
			}
		}

		public int InitialParentTileIndex
		{
			get
			{
				return initialParentTileIndex;
			}
			set
			{
				initialParentTileIndex.Set(value);
			}
		}

		public int IndexOfMenuItemView
		{
			get
			{
				return indexOfMenuItemView;
			}
			set
			{
				indexOfMenuItemView.Set(value);
			}
		}

		[XmlIgnore]
		public int UpgradeLevel
		{
			get
			{
				return upgradeLevel;
			}
			set
			{
				upgradeLevel.Value = value;
				setNewTileIndexForUpgradeLevel();
			}
		}

		public bool InstantUse
		{
			get
			{
				return instantUse;
			}
			set
			{
				instantUse.Value = value;
			}
		}

		public bool IsEfficient
		{
			get
			{
				return isEfficient;
			}
			set
			{
				isEfficient.Value = value;
			}
		}

		public float AnimationSpeedModifier
		{
			get
			{
				return animationSpeedModifier;
			}
			set
			{
				animationSpeedModifier.Value = value;
			}
		}

		public bool Stackable
		{
			get
			{
				return stackable;
			}
			set
			{
				stackable.Value = value;
			}
		}

		public Tool()
		{
			initNetFields();
			base.Category = -99;
		}

		public Tool(string name, int upgradeLevel, int initialParentTileIndex, int indexOfMenuItemView, bool stackable, int numAttachmentSlots = 0)
			: this()
		{
			BaseName = name;
			this.initialParentTileIndex.Value = initialParentTileIndex;
			IndexOfMenuItemView = indexOfMenuItemView;
			Stackable = stackable;
			currentParentTileIndex.Value = initialParentTileIndex;
			this.numAttachmentSlots.Value = numAttachmentSlots;
			if (numAttachmentSlots > 0)
			{
				attachments.SetCount(numAttachmentSlots);
			}
			base.Category = -99;
		}

		protected virtual void initNetFields()
		{
			base.NetFields.AddFields(initialParentTileIndex, currentParentTileIndex, indexOfMenuItemView, stackable, instantUse, upgradeLevel, numAttachmentSlots, attachments, enchantments, isEfficient, animationSpeedModifier, previousEnchantments);
		}

		protected abstract string loadDisplayName();

		protected abstract string loadDescription();

		public override string getCategoryName()
		{
			if (this is MeleeWeapon && !(this as MeleeWeapon).isScythe(IndexOfMenuItemView))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14303", (this as MeleeWeapon).getItemLevel(), ((int)(this as MeleeWeapon).type == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14304") : (((int)(this as MeleeWeapon).type == 2) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14305") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14306")));
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14307");
		}

		public override Color getCategoryColor()
		{
			return Color.DarkSlateGray;
		}

		public virtual void draw(SpriteBatch b)
		{
			if (lastUser == null || lastUser.toolPower <= 0 || !lastUser.canReleaseTool)
			{
				return;
			}
			foreach (Vector2 item in tilesAffected(lastUser.GetToolLocation() / 64f, lastUser.toolPower, lastUser))
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((int)item.X * 64, (int)item.Y * 64)), new Rectangle(194, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
			}
		}

		public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
		{
			base.drawTooltip(spriteBatch, ref x, ref y, font, alpha, overrideText);
			foreach (BaseEnchantment enchantment in enchantments)
			{
				if (enchantment.ShouldBeDisplayed())
				{
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors2, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(127, 35, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
					Utility.drawTextWithShadow(spriteBatch, BaseEnchantment.hideEnchantmentName ? "???" : enchantment.GetDisplayName(), font, new Vector2(x + 16 + 52, y + 16 + 12), new Color(120, 0, 210) * 0.9f * alpha);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
			}
		}

		public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
		{
			Point extraSpaceNeededForTooltipSpecialIcons = base.getExtraSpaceNeededForTooltipSpecialIcons(font, minWidth, horizontalBuffer, startingHeight, descriptionText, boldTitleText, moneyAmountToDisplayAtBottom);
			extraSpaceNeededForTooltipSpecialIcons.Y = startingHeight;
			foreach (BaseEnchantment enchantment in enchantments)
			{
				if (enchantment.ShouldBeDisplayed())
				{
					extraSpaceNeededForTooltipSpecialIcons.Y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
			}
			return extraSpaceNeededForTooltipSpecialIcons;
		}

		public virtual void tickUpdate(GameTime time, Farmer who)
		{
		}

		public bool isHeavyHitter()
		{
			if (!(this is MeleeWeapon) && !(this is Hoe) && !(this is Axe))
			{
				return this is Pickaxe;
			}
			return true;
		}

		public void Update(int direction, int farmerMotionFrame, Farmer who)
		{
			int num = 0;
			if (this is WateringCan)
			{
				switch (direction)
				{
				case 0:
					num = 4;
					break;
				case 1:
					num = 2;
					break;
				case 2:
					num = 0;
					break;
				case 3:
					num = 2;
					break;
				}
			}
			else if (this is FishingRod)
			{
				switch (direction)
				{
				case 0:
					num = 3;
					break;
				case 1:
					num = 0;
					break;
				case 3:
					num = 0;
					break;
				}
			}
			else
			{
				switch (direction)
				{
				case 0:
					num = 3;
					break;
				case 1:
					num = 2;
					break;
				case 3:
					num = 2;
					break;
				}
			}
			if (!Name.Equals("Watering Can"))
			{
				if (farmerMotionFrame < 1)
				{
					CurrentParentTileIndex = InitialParentTileIndex;
				}
				else if (who.FacingDirection == 0 || (who.FacingDirection == 2 && farmerMotionFrame >= 2))
				{
					CurrentParentTileIndex = InitialParentTileIndex + 1;
				}
			}
			else if (farmerMotionFrame < 5 || direction == 0)
			{
				CurrentParentTileIndex = InitialParentTileIndex;
			}
			else
			{
				CurrentParentTileIndex = InitialParentTileIndex + 1;
			}
			CurrentParentTileIndex += num;
		}

		public override int attachmentSlots()
		{
			return numAttachmentSlots;
		}

		public Farmer getLastFarmerToUse()
		{
			return lastUser;
		}

		public virtual void leftClick(Farmer who)
		{
		}

		public virtual void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			lastUser = who;
			short seed = (short)Game1.random.Next(-32768, 32768);
			Game1.recentMultiplayerRandom = new Random(seed);
			ToolDescription indexFromTool = ToolFactory.getIndexFromTool(this);
			if (isHeavyHitter() && !(this is MeleeWeapon))
			{
				float num = 0.1f + (float)(Game1.random.NextDouble() / 4.0);
				location.damageMonster(new Rectangle(x - 32, y - 32, 64, 64), (int)upgradeLevel + 1, ((int)upgradeLevel + 1) * 3, isBomb: false, who);
			}
			if (this is MeleeWeapon && (!who.UsingTool || Game1.mouseClickPolling >= 50 || (int)(this as MeleeWeapon).type == 1 || (this as MeleeWeapon).InitialParentTileIndex == 47 || MeleeWeapon.timedHitTimer > 0 || who.FarmerSprite.currentAnimationIndex != 5 || !(who.FarmerSprite.timer < who.FarmerSprite.interval / 4f)))
			{
				if ((int)(this as MeleeWeapon).type == 2 && (this as MeleeWeapon).isOnSpecial)
				{
					(this as MeleeWeapon).triggerClubFunction(who);
				}
				else if (who.FarmerSprite.currentAnimationIndex > 0)
				{
					MeleeWeapon.timedHitTimer = 500;
				}
			}
		}

		public virtual void endUsing(GameLocation location, Farmer who)
		{
			who.stopJittering();
			who.canReleaseTool = false;
			int num = ((!(who.Stamina <= 0f)) ? 1 : 2);
			if (Game1.isAnyGamePadButtonBeingPressed() || !who.IsLocalPlayer)
			{
				who.lastClick = who.GetToolLocation();
			}
			if (Name.Equals("Seeds"))
			{
				switch (who.FacingDirection)
				{
				case 2:
					((FarmerSprite)who.Sprite).animateOnce(200, 150f, 4);
					break;
				case 1:
					((FarmerSprite)who.Sprite).animateOnce(204, 150f, 4);
					break;
				case 0:
					((FarmerSprite)who.Sprite).animateOnce(208, 150f, 4);
					break;
				case 3:
					((FarmerSprite)who.Sprite).animateOnce(212, 150f, 4);
					break;
				}
			}
			else if (this is WateringCan)
			{
				if ((this as WateringCan).WaterLeft > 0 && who.ShouldHandleAnimationSound())
				{
					who.currentLocation.localSound("wateringCan");
				}
				switch (who.FacingDirection)
				{
				case 2:
					((FarmerSprite)who.Sprite).animateOnce(164, 125f * (float)num, 3);
					break;
				case 1:
					((FarmerSprite)who.Sprite).animateOnce(172, 125f * (float)num, 3);
					break;
				case 0:
					((FarmerSprite)who.Sprite).animateOnce(180, 125f * (float)num, 3);
					break;
				case 3:
					((FarmerSprite)who.Sprite).animateOnce(188, 125f * (float)num, 3);
					break;
				}
			}
			else if (this is FishingRod && who.IsLocalPlayer && Game1.activeClickableMenu == null)
			{
				if (!(this as FishingRod).hit)
				{
					DoFunction(who.currentLocation, (int)who.lastClick.X, (int)who.lastClick.Y, 1, who);
				}
			}
			else if (!(this is MeleeWeapon) && !(this is Pan) && !(this is Shears) && !(this is MilkPail) && !(this is Slingshot))
			{
				switch (who.FacingDirection)
				{
				case 0:
					((FarmerSprite)who.Sprite).animateOnce(176, 60f * (float)num, 8);
					break;
				case 1:
					((FarmerSprite)who.Sprite).animateOnce(168, 60f * (float)num, 8);
					break;
				case 2:
					((FarmerSprite)who.Sprite).animateOnce(160, 60f * (float)num, 8);
					break;
				case 3:
					((FarmerSprite)who.Sprite).animateOnce(184, 60f * (float)num, 8);
					break;
				}
			}
		}

		public virtual bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			lastUser = who;
			if (!instantUse)
			{
				who.Halt();
				Update(who.FacingDirection, 0, who);
				if ((!(this is FishingRod) && (int)upgradeLevel <= 0 && !(this is MeleeWeapon)) || this is Pickaxe)
				{
					who.EndUsingTool();
					return true;
				}
			}
			if (Name.Equals("Wand"))
			{
				if (((Wand)this).charged)
				{
					Game1.toolAnimationDone(who);
					who.canReleaseTool = false;
					if (!who.IsLocalPlayer || !Game1.fadeToBlack)
					{
						who.CanMove = true;
						who.UsingTool = false;
					}
				}
				else
				{
					if (who.IsLocalPlayer)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3180")));
					}
					who.UsingTool = false;
					who.canReleaseTool = false;
				}
			}
			else if ((bool)instantUse)
			{
				Game1.toolAnimationDone(who);
				who.canReleaseTool = false;
				who.UsingTool = false;
			}
			else if (Name.Equals("Seeds"))
			{
				switch (who.FacingDirection)
				{
				case 0:
					who.Sprite.currentFrame = 208;
					Update(0, 0, who);
					break;
				case 1:
					who.Sprite.currentFrame = 204;
					Update(1, 0, who);
					break;
				case 2:
					who.Sprite.currentFrame = 200;
					Update(2, 0, who);
					break;
				case 3:
					who.Sprite.currentFrame = 212;
					Update(3, 0, who);
					break;
				}
			}
			else if (this is WateringCan && location.CanRefillWateringCanOnTile((int)who.GetToolLocation().X / 64, (int)who.GetToolLocation().Y / 64))
			{
				switch (who.FacingDirection)
				{
				case 2:
					((FarmerSprite)who.Sprite).animateOnce(166, 250f, 2);
					Update(2, 1, who);
					break;
				case 1:
					((FarmerSprite)who.Sprite).animateOnce(174, 250f, 2);
					Update(1, 0, who);
					break;
				case 0:
					((FarmerSprite)who.Sprite).animateOnce(182, 250f, 2);
					Update(0, 1, who);
					break;
				case 3:
					((FarmerSprite)who.Sprite).animateOnce(190, 250f, 2);
					Update(3, 0, who);
					break;
				}
				who.canReleaseTool = false;
			}
			else if (this is WateringCan && ((WateringCan)this).WaterLeft <= 0)
			{
				Game1.toolAnimationDone(who);
				who.CanMove = true;
				who.canReleaseTool = false;
			}
			else if (this is WateringCan)
			{
				who.jitterStrength = 0.25f;
				switch (who.FacingDirection)
				{
				case 0:
					who.FarmerSprite.setCurrentFrame(180);
					Update(0, 0, who);
					break;
				case 1:
					who.FarmerSprite.setCurrentFrame(172);
					Update(1, 0, who);
					break;
				case 2:
					who.FarmerSprite.setCurrentFrame(164);
					Update(2, 0, who);
					break;
				case 3:
					who.FarmerSprite.setCurrentFrame(188);
					Update(3, 0, who);
					break;
				}
			}
			else if (this is FishingRod)
			{
				switch (who.FacingDirection)
				{
				case 0:
					((FarmerSprite)who.Sprite).animateOnce(295, 35f, 8, FishingRod.endOfAnimationBehavior);
					Update(0, 0, who);
					break;
				case 1:
					((FarmerSprite)who.Sprite).animateOnce(296, 35f, 8, FishingRod.endOfAnimationBehavior);
					Update(1, 0, who);
					break;
				case 2:
					((FarmerSprite)who.Sprite).animateOnce(297, 35f, 8, FishingRod.endOfAnimationBehavior);
					Update(2, 0, who);
					break;
				case 3:
					((FarmerSprite)who.Sprite).animateOnce(298, 35f, 8, FishingRod.endOfAnimationBehavior);
					Update(3, 0, who);
					break;
				}
				who.canReleaseTool = false;
			}
			else if (this is MeleeWeapon)
			{
				((MeleeWeapon)this).setFarmerAnimating(who);
			}
			else
			{
				switch (who.FacingDirection)
				{
				case 0:
					who.FarmerSprite.setCurrentFrame(176);
					Update(0, 0, who);
					break;
				case 1:
					who.FarmerSprite.setCurrentFrame(168);
					Update(1, 0, who);
					break;
				case 2:
					who.FarmerSprite.setCurrentFrame(160);
					Update(2, 0, who);
					break;
				case 3:
					who.FarmerSprite.setCurrentFrame(184);
					Update(3, 0, who);
					break;
				}
			}
			return false;
		}

		public virtual bool onRelease(GameLocation location, int x, int y, Farmer who)
		{
			return false;
		}

		public override bool canBeDropped()
		{
			return false;
		}

		public virtual bool canThisBeAttached(Object o)
		{
			if (attachments != null)
			{
				for (int i = 0; i < attachments.Length; i++)
				{
					if (attachments[i] == null)
					{
						return true;
					}
				}
			}
			return false;
		}

		public virtual Object attach(Object o)
		{
			for (int i = 0; i < attachments.Length; i++)
			{
				if (attachments[i] == null)
				{
					attachments[i] = o;
					Game1.playSound("button1");
					return null;
				}
			}
			return o;
		}

		public void colorTool(int level)
		{
			int num = 0;
			int num2 = 0;
			switch (BaseName.Split(' ').Last())
			{
			case "Hoe":
				num = 69129;
				num2 = 65536;
				break;
			case "Pickaxe":
				num = 100749;
				num2 = 98304;
				break;
			case "Axe":
				num = 134681;
				num2 = 131072;
				break;
			case "Can":
				num = 168713;
				num2 = 163840;
				break;
			}
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			switch (level)
			{
			case 1:
				num3 = 198;
				num4 = 108;
				num5 = 43;
				break;
			case 2:
				num3 = 197;
				num4 = 226;
				num5 = 222;
				break;
			case 3:
				num3 = 248;
				num4 = 255;
				num5 = 73;
				break;
			case 4:
				num3 = 144;
				num4 = 135;
				num5 = 181;
				break;
			}
			if (num2 > 0 && level > 0)
			{
				if (BaseName.Contains("Can"))
				{
					ColorChanger.swapColor(Game1.toolSpriteSheet, num + 36, num3 * 5 / 4, num4 * 5 / 4, num5 * 5 / 4, num2, num2 + 32768);
				}
				ColorChanger.swapColor(Game1.toolSpriteSheet, num + 8, num3, num4, num5, num2, num2 + 32768);
				ColorChanger.swapColor(Game1.toolSpriteSheet, num + 4, num3 * 3 / 4, num4 * 3 / 4, num5 * 3 / 4, num2, num2 + 32768);
				ColorChanger.swapColor(Game1.toolSpriteSheet, num, num3 * 3 / 8, num4 * 3 / 8, num5 * 3 / 8, num2, num2 + 32768);
			}
		}

		public virtual void actionWhenClaimed()
		{
			if (this is GenericTool)
			{
				int num = indexOfMenuItemView;
				if ((uint)(num - 13) <= 3u)
				{
					Game1.player.trashCanLevel++;
				}
			}
		}

		public override bool CanBuyItem(Farmer who)
		{
			if (Game1.player.toolBeingUpgraded.Value == null && (this is Axe || this is Pickaxe || this is Hoe || this is WateringCan || (this is GenericTool && (int)indexOfMenuItemView >= 13 && (int)indexOfMenuItemView <= 16)))
			{
				return true;
			}
			return base.CanBuyItem(who);
		}

		public override bool actionWhenPurchased()
		{
			if (Game1.player.toolBeingUpgraded.Value == null)
			{
				if (this is Axe || this is Pickaxe || this is Hoe || this is WateringCan)
				{
					Tool toolFromName = Game1.player.getToolFromName(BaseName);
					toolFromName.UpgradeLevel++;
					Game1.player.removeItemFromInventory(toolFromName);
					Game1.player.toolBeingUpgraded.Value = toolFromName;
					Game1.player.daysLeftForToolUpgrade.Value = 2;
					Game1.playSound("parry");
					Game1.exitActiveMenu();
					Game1.drawDialogue(Game1.getCharacterFromName("Clint"), Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14317"));
					return true;
				}
				if (this is GenericTool)
				{
					int num = indexOfMenuItemView;
					if ((uint)(num - 13) <= 3u)
					{
						Game1.player.toolBeingUpgraded.Value = this;
						Game1.player.daysLeftForToolUpgrade.Value = 2;
						Game1.playSound("parry");
						Game1.exitActiveMenu();
						Game1.drawDialogue(Game1.getCharacterFromName("Clint"), Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14317"));
						return true;
					}
				}
			}
			return base.actionWhenPurchased();
		}

		protected List<Vector2> tilesAffected(Vector2 tileLocation, int power, Farmer who)
		{
			power++;
			List<Vector2> list = new List<Vector2>();
			list.Add(tileLocation);
			Vector2 vector = Vector2.Zero;
			if (who.FacingDirection == 0)
			{
				if (power >= 6)
				{
					vector = new Vector2(tileLocation.X, tileLocation.Y - 2f);
				}
				else
				{
					if (power >= 2)
					{
						list.Add(tileLocation + new Vector2(0f, -1f));
						list.Add(tileLocation + new Vector2(0f, -2f));
					}
					if (power >= 3)
					{
						list.Add(tileLocation + new Vector2(0f, -3f));
						list.Add(tileLocation + new Vector2(0f, -4f));
					}
					if (power >= 4)
					{
						list.RemoveAt(list.Count - 1);
						list.RemoveAt(list.Count - 1);
						list.Add(tileLocation + new Vector2(1f, -2f));
						list.Add(tileLocation + new Vector2(1f, -1f));
						list.Add(tileLocation + new Vector2(1f, 0f));
						list.Add(tileLocation + new Vector2(-1f, -2f));
						list.Add(tileLocation + new Vector2(-1f, -1f));
						list.Add(tileLocation + new Vector2(-1f, 0f));
					}
					if (power >= 5)
					{
						for (int num = list.Count - 1; num >= 0; num--)
						{
							list.Add(list[num] + new Vector2(0f, -3f));
						}
					}
				}
			}
			else if (who.FacingDirection == 1)
			{
				if (power >= 6)
				{
					vector = new Vector2(tileLocation.X + 2f, tileLocation.Y);
				}
				else
				{
					if (power >= 2)
					{
						list.Add(tileLocation + new Vector2(1f, 0f));
						list.Add(tileLocation + new Vector2(2f, 0f));
					}
					if (power >= 3)
					{
						list.Add(tileLocation + new Vector2(3f, 0f));
						list.Add(tileLocation + new Vector2(4f, 0f));
					}
					if (power >= 4)
					{
						list.RemoveAt(list.Count - 1);
						list.RemoveAt(list.Count - 1);
						list.Add(tileLocation + new Vector2(0f, -1f));
						list.Add(tileLocation + new Vector2(1f, -1f));
						list.Add(tileLocation + new Vector2(2f, -1f));
						list.Add(tileLocation + new Vector2(0f, 1f));
						list.Add(tileLocation + new Vector2(1f, 1f));
						list.Add(tileLocation + new Vector2(2f, 1f));
					}
					if (power >= 5)
					{
						for (int num2 = list.Count - 1; num2 >= 0; num2--)
						{
							list.Add(list[num2] + new Vector2(3f, 0f));
						}
					}
				}
			}
			else if (who.FacingDirection == 2)
			{
				if (power >= 6)
				{
					vector = new Vector2(tileLocation.X, tileLocation.Y + 2f);
				}
				else
				{
					if (power >= 2)
					{
						list.Add(tileLocation + new Vector2(0f, 1f));
						list.Add(tileLocation + new Vector2(0f, 2f));
					}
					if (power >= 3)
					{
						list.Add(tileLocation + new Vector2(0f, 3f));
						list.Add(tileLocation + new Vector2(0f, 4f));
					}
					if (power >= 4)
					{
						list.RemoveAt(list.Count - 1);
						list.RemoveAt(list.Count - 1);
						list.Add(tileLocation + new Vector2(1f, 2f));
						list.Add(tileLocation + new Vector2(1f, 1f));
						list.Add(tileLocation + new Vector2(1f, 0f));
						list.Add(tileLocation + new Vector2(-1f, 2f));
						list.Add(tileLocation + new Vector2(-1f, 1f));
						list.Add(tileLocation + new Vector2(-1f, 0f));
					}
					if (power >= 5)
					{
						for (int num3 = list.Count - 1; num3 >= 0; num3--)
						{
							list.Add(list[num3] + new Vector2(0f, 3f));
						}
					}
				}
			}
			else if (who.FacingDirection == 3)
			{
				if (power >= 6)
				{
					vector = new Vector2(tileLocation.X - 2f, tileLocation.Y);
				}
				else
				{
					if (power >= 2)
					{
						list.Add(tileLocation + new Vector2(-1f, 0f));
						list.Add(tileLocation + new Vector2(-2f, 0f));
					}
					if (power >= 3)
					{
						list.Add(tileLocation + new Vector2(-3f, 0f));
						list.Add(tileLocation + new Vector2(-4f, 0f));
					}
					if (power >= 4)
					{
						list.RemoveAt(list.Count - 1);
						list.RemoveAt(list.Count - 1);
						list.Add(tileLocation + new Vector2(0f, -1f));
						list.Add(tileLocation + new Vector2(-1f, -1f));
						list.Add(tileLocation + new Vector2(-2f, -1f));
						list.Add(tileLocation + new Vector2(0f, 1f));
						list.Add(tileLocation + new Vector2(-1f, 1f));
						list.Add(tileLocation + new Vector2(-2f, 1f));
					}
					if (power >= 5)
					{
						for (int num4 = list.Count - 1; num4 >= 0; num4--)
						{
							list.Add(list[num4] + new Vector2(-3f, 0f));
						}
					}
				}
			}
			if (power >= 6)
			{
				list.Clear();
				for (int i = (int)vector.X - 2; (float)i <= vector.X + 2f; i++)
				{
					for (int j = (int)vector.Y - 2; (float)j <= vector.Y + 2f; j++)
					{
						list.Add(new Vector2(i, j));
					}
				}
			}
			return list;
		}

		public virtual bool doesShowTileLocationMarker()
		{
			return true;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			int num = (int)(64f * scaleSize);
			int num2 = (base.itemSlotSize - num) / 2;
			Vector2 zero = Vector2.Zero;
			spriteBatch.Draw(Game1.toolSpriteSheet, location + new Vector2(num2, num2), Game1.getSquareSourceRectForNonStandardTileSheet(Game1.toolSpriteSheet, 16, 16, IndexOfMenuItemView), color * transparency, 0f, zero, 4f * scaleSize, SpriteEffects.None, layerDepth);
			if ((bool)stackable)
			{
				Game1.drawWithBorder(((Stackable)this).NumberInStack.ToString() ?? "", Color.Black, Color.White, location + new Vector2(64f - Game1.dialogueFont.MeasureString(((Stackable)this).NumberInStack.ToString() ?? "").X, 64f - Game1.dialogueFont.MeasureString(((Stackable)this).NumberInStack.ToString() ?? "").Y * 3f / 4f), 0f, 0.5f, 1f);
			}
		}

		public override bool isPlaceable()
		{
			return false;
		}

		public override int maximumStackSize()
		{
			if ((bool)stackable)
			{
				return 99;
			}
			return -1;
		}

		public virtual void setNewTileIndexForUpgradeLevel()
		{
			if (this is MeleeWeapon || this is MagnifyingGlass || this is MilkPail || this is Shears || this is Pan || this is Slingshot || this is Wand)
			{
				return;
			}
			int num = 21;
			if (this is FishingRod)
			{
				InitialParentTileIndex = 8 + (int)upgradeLevel;
				CurrentParentTileIndex = InitialParentTileIndex;
				IndexOfMenuItemView = InitialParentTileIndex;
				return;
			}
			if (this is Axe)
			{
				num = 189;
			}
			else if (this is Hoe)
			{
				num = 21;
			}
			else if (this is Pickaxe)
			{
				num = 105;
			}
			else if (this is WateringCan)
			{
				num = 273;
			}
			num += (int)upgradeLevel * 7;
			if ((int)upgradeLevel > 2)
			{
				num += 21;
			}
			InitialParentTileIndex = num;
			CurrentParentTileIndex = InitialParentTileIndex;
			IndexOfMenuItemView = InitialParentTileIndex + ((this is WateringCan) ? 2 : 5) + 21;
		}

		public override int addToStack(Item stack)
		{
			if ((bool)stackable)
			{
				((Stackable)this).NumberInStack += stack.Stack;
				if (((Stackable)this).NumberInStack > 99)
				{
					int result = ((Stackable)this).NumberInStack - 99;
					((Stackable)this).NumberInStack = 99;
					return result;
				}
				return 0;
			}
			return stack.Stack;
		}

		public override string getDescription()
		{
			return Game1.parseText(description, Game1.smallFont, getDescriptionWidth());
		}

		public virtual void ClearEnchantments()
		{
			for (int num = enchantments.Count - 1; num >= 0; num--)
			{
				enchantments[num].UnapplyTo(this);
			}
			enchantments.Clear();
		}

		public virtual int GetMaxForges()
		{
			return 0;
		}

		public int GetSecondaryEnchantmentCount()
		{
			int num = 0;
			foreach (BaseEnchantment enchantment in enchantments)
			{
				if (enchantment != null && enchantment.IsSecondaryEnchantment())
				{
					num++;
				}
			}
			return num;
		}

		public virtual bool CanAddEnchantment(BaseEnchantment enchantment)
		{
			if (!enchantment.IsForge() && !enchantment.IsSecondaryEnchantment())
			{
				return true;
			}
			if (GetTotalForgeLevels() >= GetMaxForges() && !enchantment.IsSecondaryEnchantment())
			{
				return false;
			}
			if (enchantment != null)
			{
				foreach (BaseEnchantment enchantment2 in enchantments)
				{
					if (enchantment.GetType() == enchantment2.GetType())
					{
						if (enchantment2.GetMaximumLevel() < 0 || enchantment2.GetLevel() < enchantment2.GetMaximumLevel())
						{
							return true;
						}
						return false;
					}
				}
				return true;
			}
			return false;
		}

		public virtual void CopyEnchantments(Tool source, Tool destination)
		{
			foreach (BaseEnchantment enchantment in enchantments)
			{
				destination.enchantments.Add(enchantment.GetOne());
				enchantment.GetOne().ApplyTo(destination);
			}
			destination.previousEnchantments.Clear();
			destination.previousEnchantments.AddRange(source.previousEnchantments);
		}

		public int GetTotalForgeLevels(bool for_unforge = false)
		{
			int num = 0;
			foreach (BaseEnchantment enchantment in enchantments)
			{
				if (enchantment is DiamondEnchantment)
				{
					if (for_unforge)
					{
						return num;
					}
				}
				else if (enchantment.IsForge())
				{
					num += enchantment.GetLevel();
				}
			}
			return num;
		}

		public virtual bool AddEnchantment(BaseEnchantment enchantment)
		{
			if (enchantment != null)
			{
				if (this is MeleeWeapon && (enchantment.IsForge() || enchantment.IsSecondaryEnchantment()))
				{
					foreach (BaseEnchantment enchantment2 in enchantments)
					{
						if (enchantment.GetType() == enchantment2.GetType())
						{
							if (enchantment2.GetMaximumLevel() < 0 || enchantment2.GetLevel() < enchantment2.GetMaximumLevel())
							{
								enchantment2.SetLevel(this, enchantment2.GetLevel() + 1);
								return true;
							}
							return false;
						}
					}
					enchantments.Add(enchantment);
					enchantment.ApplyTo(this, lastUser);
					return true;
				}
				for (int num = enchantments.Count - 1; num >= 0; num--)
				{
					if (!enchantments[num].IsForge() && !enchantments[num].IsSecondaryEnchantment())
					{
						enchantments.ElementAt(num).UnapplyTo(this);
						enchantments.RemoveAt(num);
					}
				}
				enchantments.Add(enchantment);
				enchantment.ApplyTo(this, lastUser);
				return true;
			}
			return false;
		}

		public bool hasEnchantmentOfType<T>()
		{
			foreach (BaseEnchantment enchantment in enchantments)
			{
				if (enchantment is T)
				{
					return true;
				}
			}
			return false;
		}

		public virtual void RemoveEnchantment(BaseEnchantment enchantment)
		{
			if (enchantment != null)
			{
				enchantments.Remove(enchantment);
				enchantment.UnapplyTo(this, lastUser);
			}
		}

		public override void actionWhenBeingHeld(Farmer who)
		{
			base.actionWhenBeingHeld(who);
			if (!who.IsLocalPlayer)
			{
				return;
			}
			foreach (BaseEnchantment enchantment in enchantments)
			{
				enchantment.OnEquip(who);
			}
		}

		public override void actionWhenStopBeingHeld(Farmer who)
		{
			base.actionWhenStopBeingHeld(who);
			if (who.UsingTool)
			{
				who.UsingTool = false;
				if (who.FarmerSprite.PauseForSingleAnimation)
				{
					who.FarmerSprite.PauseForSingleAnimation = false;
				}
			}
			if (!who.IsLocalPlayer)
			{
				return;
			}
			foreach (BaseEnchantment enchantment in enchantments)
			{
				enchantment.OnUnequip(who);
			}
		}

		public virtual bool CanUseOnStandingTile()
		{
			return false;
		}

		public virtual bool CanForge(Item item)
		{
			BaseEnchantment enchantmentFromItem = BaseEnchantment.GetEnchantmentFromItem(this, item);
			if (enchantmentFromItem != null && CanAddEnchantment(enchantmentFromItem))
			{
				return true;
			}
			return false;
		}

		public T GetEnchantmentOfType<T>() where T : BaseEnchantment
		{
			foreach (BaseEnchantment enchantment in enchantments)
			{
				if (enchantment.GetType() == typeof(T))
				{
					return enchantment as T;
				}
			}
			return null;
		}

		public int GetEnchantmentLevel<T>() where T : BaseEnchantment
		{
			int num = 0;
			foreach (BaseEnchantment enchantment in enchantments)
			{
				if (enchantment.GetType() == typeof(T))
				{
					num += enchantment.GetLevel();
				}
			}
			return num;
		}

		public virtual bool Forge(Item item, bool count_towards_stats = false)
		{
			BaseEnchantment enchantmentFromItem = BaseEnchantment.GetEnchantmentFromItem(this, item);
			if (enchantmentFromItem != null && AddEnchantment(enchantmentFromItem))
			{
				if (enchantmentFromItem is DiamondEnchantment)
				{
					int num = GetMaxForges() - GetTotalForgeLevels();
					List<int> list = new List<int>();
					if (!hasEnchantmentOfType<EmeraldEnchantment>())
					{
						list.Add(0);
					}
					if (!hasEnchantmentOfType<AquamarineEnchantment>())
					{
						list.Add(1);
					}
					if (!hasEnchantmentOfType<RubyEnchantment>())
					{
						list.Add(2);
					}
					if (!hasEnchantmentOfType<AmethystEnchantment>())
					{
						list.Add(3);
					}
					if (!hasEnchantmentOfType<TopazEnchantment>())
					{
						list.Add(4);
					}
					if (!hasEnchantmentOfType<JadeEnchantment>())
					{
						list.Add(5);
					}
					for (int i = 0; i < num; i++)
					{
						if (list.Count == 0)
						{
							break;
						}
						int index = Game1.random.Next(list.Count);
						int num2 = list[index];
						list.RemoveAt(index);
						switch (num2)
						{
						case 0:
							AddEnchantment(new EmeraldEnchantment());
							break;
						case 1:
							AddEnchantment(new AquamarineEnchantment());
							break;
						case 2:
							AddEnchantment(new RubyEnchantment());
							break;
						case 3:
							AddEnchantment(new AmethystEnchantment());
							break;
						case 4:
							AddEnchantment(new TopazEnchantment());
							break;
						case 5:
							AddEnchantment(new JadeEnchantment());
							break;
						}
					}
				}
				else if (enchantmentFromItem is GalaxySoulEnchantment && this is MeleeWeapon && (this as MeleeWeapon).isGalaxyWeapon() && (this as MeleeWeapon).GetEnchantmentLevel<GalaxySoulEnchantment>() >= 3)
				{
					int num3 = (this as MeleeWeapon).InitialParentTileIndex;
					int num4 = -1;
					switch (num3)
					{
					case 4:
						num4 = 62;
						break;
					case 29:
						num4 = 63;
						break;
					case 23:
						num4 = 64;
						break;
					}
					if (num4 != -1)
					{
						(this as MeleeWeapon).transform(num4);
						if (count_towards_stats)
						{
							DelayedAction.playSoundAfterDelay("discoverMineral", 400);
							Game1.multiplayer.globalChatInfoMessage("InfinityWeapon", Game1.player.name, DisplayName);
						}
					}
					GalaxySoulEnchantment enchantmentOfType = GetEnchantmentOfType<GalaxySoulEnchantment>();
					if (enchantmentOfType != null)
					{
						RemoveEnchantment(enchantmentOfType);
					}
				}
				if (count_towards_stats && !enchantmentFromItem.IsForge())
				{
					previousEnchantments.Insert(0, enchantmentFromItem.GetName());
					while (previousEnchantments.Count > 2)
					{
						previousEnchantments.RemoveAt(previousEnchantments.Count - 1);
					}
					Game1.stats.incrementStat("timesEnchanted", 1);
				}
				return true;
			}
			return false;
		}
	}
}
