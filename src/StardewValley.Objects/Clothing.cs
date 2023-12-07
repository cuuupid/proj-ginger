using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Objects
{
	public class Clothing : Item
	{
		public enum ClothesType
		{
			SHIRT,
			PANTS,
			ACCESSORY
		}

		public const int SHIRT_SHEET_WIDTH = 128;

		[XmlElement("price")]
		public readonly NetInt price = new NetInt();

		[XmlElement("indexInTileSheet")]
		public readonly NetInt indexInTileSheetMale = new NetInt();

		[XmlElement("indexInTileSheetFemale")]
		public readonly NetInt indexInTileSheetFemale = new NetInt();

		[XmlIgnore]
		public string description;

		[XmlIgnore]
		public string displayName;

		[XmlElement("clothesType")]
		public readonly NetInt clothesType = new NetInt();

		[XmlElement("dyeable")]
		public readonly NetBool dyeable = new NetBool(value: false);

		[XmlElement("clothesColor")]
		public readonly NetColor clothesColor = new NetColor(new Color(255, 255, 255));

		[XmlElement("otherData")]
		public readonly NetString otherData = new NetString("");

		protected List<string> _otherDataList;

		[XmlElement("isPrismatic")]
		public readonly NetBool isPrismatic = new NetBool(value: false);

		[XmlIgnore]
		protected bool _loadedData;

		internal static int _maxShirtValue = -1;

		internal static int _maxPantsValue = -1;

		public int Price
		{
			get
			{
				return price.Value;
			}
			set
			{
				price.Value = value;
			}
		}

		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				if (!_loadedData)
				{
					LoadData();
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

		public Clothing()
		{
			base.Category = -100;
			base.NetFields.AddFields(price, indexInTileSheetMale, indexInTileSheetFemale, clothesType, dyeable, clothesColor, otherData, isPrismatic);
		}

		public static int GetMaxShirtValue()
		{
			if (_maxShirtValue < 0)
			{
				foreach (string value in Game1.clothingInformation.Values)
				{
					if (value == null)
					{
						continue;
					}
					string[] array = value.Split('/');
					if (array.Length >= 9 && !(array[8] != "Shirt"))
					{
						int num = int.Parse(array[3]);
						int num2 = int.Parse(array[4]);
						if (_maxShirtValue < num)
						{
							_maxShirtValue = num;
						}
						if (_maxShirtValue < num2)
						{
							_maxShirtValue = num2;
						}
					}
				}
			}
			return _maxShirtValue;
		}

		public static int GetMaxPantsValue()
		{
			if (_maxPantsValue < 0)
			{
				foreach (string value in Game1.clothingInformation.Values)
				{
					if (value == null)
					{
						continue;
					}
					string[] array = value.Split('/');
					if (array.Length >= 9 && !(array[8] != "Pants"))
					{
						int num = int.Parse(array[3]);
						int maxPantsValue = int.Parse(array[4]);
						if (_maxPantsValue < num)
						{
							_maxPantsValue = num;
						}
						if (_maxPantsValue < num)
						{
							_maxPantsValue = maxPantsValue;
						}
					}
				}
			}
			return _maxPantsValue;
		}

		public virtual List<string> GetOtherData()
		{
			if (otherData.Value != null)
			{
				if (_otherDataList == null)
				{
					_otherDataList = new List<string>(otherData.Value.Split(','));
					for (int i = 0; i < _otherDataList.Count; i++)
					{
						string text = _otherDataList[i].Trim();
						if (text == "")
						{
							_otherDataList.RemoveAt(i);
							i--;
						}
					}
				}
				return _otherDataList;
			}
			return new List<string>();
		}

		public Clothing(int item_index)
			: this()
		{
			Name = "Clothing";
			base.Category = -100;
			base.ParentSheetIndex = item_index;
			LoadData(initialize_color: true);
		}

		public virtual void LoadData(bool initialize_color = false)
		{
			if (_loadedData)
			{
				return;
			}
			int num = base.ParentSheetIndex;
			base.Category = -100;
			if (Game1.clothingInformation.ContainsKey(num))
			{
				string[] array = Game1.clothingInformation[num].Split('/');
				Name = array[0];
				price.Value = Convert.ToInt32(array[5]);
				indexInTileSheetMale.Value = Convert.ToInt32(array[3]);
				indexInTileSheetFemale.Value = Convert.ToInt32(array[4]);
				dyeable.Value = Convert.ToBoolean(array[7]);
				if (initialize_color)
				{
					string[] array2 = array[6].Split(' ');
					clothesColor.Value = new Color(Convert.ToInt32(array2[0]), Convert.ToInt32(array2[1]), Convert.ToInt32(array2[2]));
				}
				displayName = array[1];
				description = array[2];
				switch (array[8].ToLower().Trim())
				{
				case "pants":
					clothesType.Set(1);
					break;
				case "shirt":
					clothesType.Set(0);
					break;
				case "accessory":
					clothesType.Set(2);
					break;
				}
				if (array.Length >= 10)
				{
					otherData.Set(array[9]);
				}
				else
				{
					otherData.Set("");
				}
				if (GetOtherData().Contains("Prismatic"))
				{
					isPrismatic.Set(newValue: true);
				}
			}
			else
			{
				base.ParentSheetIndex = num;
				string[] array3 = Game1.clothingInformation[-1].Split('/');
				clothesType.Set(1);
				if (num >= 1000)
				{
					array3 = Game1.clothingInformation[-2].Split('/');
					clothesType.Set(0);
					num -= 1000;
				}
				if (initialize_color)
				{
					clothesColor.Set(new Color(1f, 1f, 1f));
				}
				if (clothesType.Value == 1)
				{
					dyeable.Set(newValue: true);
				}
				else
				{
					dyeable.Set(newValue: false);
				}
				displayName = array3[1];
				description = array3[2];
				indexInTileSheetMale.Set(num);
				indexInTileSheetFemale.Set(-1);
			}
			if (dyeable.Value)
			{
				description = description + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Clothes_Dyeable");
			}
			_loadedData = true;
		}

		public override string getCategoryName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:category_clothes");
		}

		public override int salePrice()
		{
			return price;
		}

		public virtual void Dye(Color color, float strength = 0.5f)
		{
			if (dyeable.Value)
			{
				Color value = clothesColor.Value;
				clothesColor.Value = new Color(Utility.MoveTowards((float)(int)value.R / 255f, (float)(int)color.R / 255f, strength), Utility.MoveTowards((float)(int)value.G / 255f, (float)(int)color.G / 255f, strength), Utility.MoveTowards((float)(int)value.B / 255f, (float)(int)color.B / 255f, strength), Utility.MoveTowards((float)(int)value.A / 255f, (float)(int)color.A / 255f, strength));
			}
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			Color a = clothesColor;
			if (isPrismatic.Value)
			{
				a = Utility.GetPrismaticColor();
			}
			if (clothesType.Value == 0)
			{
				float num = 1E-07f;
				if (layerDepth >= 1f - num)
				{
					layerDepth = 1f - num;
				}
				spriteBatch.Draw(FarmerRenderer.shirtsTexture, location + new Vector2(base.itemSlotSize / 2, base.itemSlotSize / 2), new Rectangle(indexInTileSheetMale.Value * 8 % 128, indexInTileSheetMale.Value * 8 / 128 * 32, 8, 8), color * transparency, 0f, new Vector2(4f, 4f), scaleSize * 4f, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(FarmerRenderer.shirtsTexture, location + new Vector2(base.itemSlotSize / 2, base.itemSlotSize / 2), new Rectangle(indexInTileSheetMale.Value * 8 % 128 + 128, indexInTileSheetMale.Value * 8 / 128 * 32, 8, 8), Utility.MultiplyColor(a, color) * transparency, 0f, new Vector2(4f, 4f), scaleSize * 4f, SpriteEffects.None, layerDepth + num);
			}
			else if (clothesType.Value == 1)
			{
				spriteBatch.Draw(FarmerRenderer.pantsTexture, location + new Vector2(base.itemSlotSize / 2, base.itemSlotSize / 2), new Rectangle(192 * (indexInTileSheetMale.Value % (FarmerRenderer.pantsTexture.Width / 192)), 688 * (indexInTileSheetMale.Value / (FarmerRenderer.pantsTexture.Width / 192)) + 672, 16, 16), Utility.MultiplyColor(a, color) * transparency, 0f, new Vector2(8f, 8f), scaleSize * 4f, SpriteEffects.None, layerDepth);
			}
		}

		public override int maximumStackSize()
		{
			return 1;
		}

		public override int addToStack(Item stack)
		{
			return 1;
		}

		public override string getDescription()
		{
			if (!_loadedData)
			{
				LoadData();
			}
			return Game1.parseText(description, Game1.smallFont, getDescriptionWidth());
		}

		public override bool isPlaceable()
		{
			return false;
		}

		public override Item getOne()
		{
			Clothing clothing = new Clothing(base.ParentSheetIndex);
			clothing.clothesColor.Value = clothesColor.Value;
			clothing._GetOneFrom(this);
			return clothing;
		}
	}
}
