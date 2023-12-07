using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Objects
{
	public class Hat : Item
	{
		public enum HairDrawType
		{
			DrawFullHair,
			DrawObscuredHair,
			HideHair
		}

		public const int widthOfTileSheetSquare = 20;

		public const int heightOfTileSheetSquare = 20;

		[XmlElement("which")]
		public readonly NetInt which = new NetInt();

		[XmlElement("skipHairDraw")]
		public bool skipHairDraw;

		[XmlElement("ignoreHairstyleOffset")]
		public readonly NetBool ignoreHairstyleOffset = new NetBool();

		[XmlElement("hairDrawType")]
		public readonly NetInt hairDrawType = new NetInt();

		[XmlElement("isPrismatic")]
		public readonly NetBool isPrismatic = new NetBool(value: false);

		[XmlIgnore]
		protected int _isMask = -1;

		[XmlIgnore]
		public string displayName;

		[XmlIgnore]
		public string description;

		[XmlIgnore]
		public bool isMask
		{
			get
			{
				if (_isMask == -1)
				{
					if (Name.Contains("Mask"))
					{
						_isMask = 1;
					}
					else
					{
						_isMask = 0;
					}
					if ((int)hairDrawType == 2)
					{
						_isMask = 0;
					}
				}
				return _isMask == 1;
			}
		}

		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				if (displayName == null)
				{
					loadDisplayFields();
				}
				return displayName;
			}
			set
			{
				displayName = value;
			}
		}

		[XmlIgnore]
		public override int Stack
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		public Hat()
		{
			base.NetFields.AddFields(which, ignoreHairstyleOffset, hairDrawType, isPrismatic);
			load(which);
		}

		public void load(int which)
		{
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\hats");
			if (!dictionary.ContainsKey(which))
			{
				which = 0;
			}
			string[] array = dictionary[which].Split('/');
			Name = array[0];
			if (array[2] == "hide")
			{
				hairDrawType.Set(2);
			}
			else if (Convert.ToBoolean(array[2]))
			{
				hairDrawType.Set(0);
			}
			else
			{
				hairDrawType.Set(1);
			}
			if (skipHairDraw)
			{
				skipHairDraw = false;
				hairDrawType.Set(0);
			}
			if (array.Length > 4)
			{
				string[] array2 = array[4].Split(' ');
				foreach (string text in array2)
				{
					if (text == "Prismatic")
					{
						isPrismatic.Value = true;
					}
				}
			}
			ignoreHairstyleOffset.Value = Convert.ToBoolean(array[3]);
			base.Category = -95;
		}

		public Hat(int which)
			: this()
		{
			this.which.Value = which;
			load(which);
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			spriteBatch.Draw(sourceRectangle: new Rectangle((int)which * 20 % 240, (int)which * 20 / 240 * 20 * 4, 20, 20), texture: FarmerRenderer.hatsTexture, position: location + new Vector2((int)((float)(base.itemSlotSize / 2) * scaleSize), (int)((float)(base.itemSlotSize / 2) * scaleSize)), color: isPrismatic ? (Utility.GetPrismaticColor() * transparency) : (color * transparency), rotation: 0f, origin: new Vector2(10f, 10f) * scaleSize, scale: 4f * scaleSize, effects: SpriteEffects.None, layerDepth: layerDepth);
		}

		public void draw(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, int direction)
		{
			switch (direction)
			{
			case 0:
				direction = 3;
				break;
			case 2:
				direction = 0;
				break;
			case 3:
				direction = 2;
				break;
			}
			spriteBatch.Draw(FarmerRenderer.hatsTexture, location + new Vector2(10f, 10f), new Rectangle((int)which * 20 % FarmerRenderer.hatsTexture.Width, (int)which * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4 + direction * 20, 20, 20), isPrismatic ? (Utility.GetPrismaticColor() * transparency) : (Color.White * transparency), 0f, new Vector2(3f, 3f), 3f * scaleSize, SpriteEffects.None, layerDepth);
		}

		public override string getDescription()
		{
			if (description == null)
			{
				loadDisplayFields();
			}
			return Game1.parseText(description, Game1.smallFont, getDescriptionWidth());
		}

		public override int maximumStackSize()
		{
			return 1;
		}

		public override int addToStack(Item stack)
		{
			return 1;
		}

		public override bool isPlaceable()
		{
			return false;
		}

		public override Item getOne()
		{
			Hat hat = new Hat(which);
			hat._GetOneFrom(this);
			return hat;
		}

		private bool loadDisplayFields()
		{
			if (Name != null)
			{
				Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\hats");
				foreach (KeyValuePair<int, string> item in dictionary)
				{
					string[] array = item.Value.Split('/');
					if (array[0] == Name)
					{
						displayName = Name;
						if (LocalizedContentManager.CurrentLanguageCode != 0)
						{
							displayName = array[array.Length - 1];
						}
						description = array[1];
						return true;
					}
				}
			}
			return false;
		}
	}
}
