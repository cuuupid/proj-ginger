using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.GameData.HomeRenovations;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewValley
{
	public class HouseRenovation : ISalable
	{
		public enum AnimationType
		{
			Build,
			Destroy
		}

		protected string _displayName;

		protected string _name;

		protected string _description;

		public AnimationType animationType;

		public List<List<Rectangle>> renovationBounds = new List<List<Rectangle>>();

		public string placementText = "";

		public GameLocation location;

		public bool requireClearance = true;

		public Action<HouseRenovation, int> onRenovation;

		public Func<HouseRenovation, int, bool> validate;

		public string DisplayName => _displayName;

		public string Name => _name;

		public int Stack
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		public bool ShouldDrawIcon()
		{
			return false;
		}

		public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
		}

		public string getDescription()
		{
			return _description;
		}

		public int maximumStackSize()
		{
			return 1;
		}

		public int addToStack(Item stack)
		{
			return 0;
		}

		public int salePrice()
		{
			return 0;
		}

		public bool actionWhenPurchased()
		{
			return false;
		}

		public bool canStackWith(ISalable other)
		{
			return false;
		}

		public bool CanBuyItem(Farmer farmer)
		{
			return true;
		}

		public bool IsInfiniteStock()
		{
			return true;
		}

		public ISalable GetSalableInstance()
		{
			return this;
		}

		public static void ShowRenovationMenu()
		{
			Game1.activeClickableMenu = new ShopMenu(GetAvailableRenovations(), 0, null, OnPurchaseRenovation);
		}

		public static List<ISalable> GetAvailableRenovations()
		{
			FarmHouse farmhouse = Game1.getLocationFromName(Game1.player.homeLocation) as FarmHouse;
			List<ISalable> list = new List<ISalable>();
			HouseRenovation houseRenovation = null;
			Dictionary<string, HomeRenovation> dictionary = Game1.content.Load<Dictionary<string, HomeRenovation>>("Data\\HomeRenovations");
			NetInt field;
			foreach (string key in dictionary.Keys)
			{
				HomeRenovation homeRenovation = dictionary[key];
				bool flag = true;
				foreach (RenovationValue requirement in homeRenovation.Requirements)
				{
					if (requirement.Type == "Value")
					{
						string text = requirement.Value;
						bool flag2 = true;
						if (text.Length > 0 && text[0] == '!')
						{
							text = text.Substring(1);
							flag2 = false;
						}
						int num = int.Parse(text);
						try
						{
							NetInt netInt = (NetInt)farmhouse.GetType().GetField(requirement.Key).GetValue(farmhouse);
							if (netInt == null)
							{
								flag = false;
								break;
							}
							if (netInt.Value == num != flag2)
							{
								flag = false;
								break;
							}
							continue;
						}
						catch (Exception)
						{
							flag = false;
						}
						break;
					}
					if (requirement.Type == "Mail")
					{
						string value = requirement.Value;
						if (Game1.player.hasOrWillReceiveMail(requirement.Key) != (requirement.Value == "1"))
						{
							flag = false;
							break;
						}
					}
				}
				if (!flag)
				{
					continue;
				}
				houseRenovation = new HouseRenovation();
				houseRenovation.location = farmhouse;
				houseRenovation._name = key;
				string text2 = Game1.content.LoadString(homeRenovation.TextStrings);
				string[] array = text2.Split('/');
				try
				{
					houseRenovation._displayName = array[0];
					houseRenovation._description = array[1];
					houseRenovation.placementText = array[2];
				}
				catch (Exception)
				{
					houseRenovation._displayName = "?";
					houseRenovation._description = "?";
					houseRenovation.placementText = "?";
				}
				if (homeRenovation.CheckForObstructions)
				{
					HouseRenovation houseRenovation2 = houseRenovation;
					houseRenovation2.validate = (Func<HouseRenovation, int, bool>)Delegate.Combine(houseRenovation2.validate, new Func<HouseRenovation, int, bool>(EnsureNoObstructions));
				}
				if (homeRenovation.AnimationType == "destroy")
				{
					houseRenovation.animationType = AnimationType.Destroy;
				}
				else
				{
					houseRenovation.animationType = AnimationType.Build;
				}
				if (homeRenovation.SpecialRect != null && homeRenovation.SpecialRect != "")
				{
					if (homeRenovation.SpecialRect == "crib")
					{
						Rectangle? cribBounds = farmhouse.GetCribBounds();
						if (!farmhouse.CanModifyCrib() || !cribBounds.HasValue)
						{
							continue;
						}
						houseRenovation.AddRenovationBound(cribBounds.Value);
					}
				}
				else
				{
					foreach (RectGroup rectGroup in homeRenovation.RectGroups)
					{
						List<Rectangle> list2 = new List<Rectangle>();
						foreach (Rect rect in rectGroup.Rects)
						{
							Rectangle item = default(Rectangle);
							item.X = rect.X;
							item.Y = rect.Y;
							item.Width = rect.Width;
							item.Height = rect.Height;
							list2.Add(item);
						}
						houseRenovation.AddRenovationBound(list2);
					}
				}
				foreach (RenovationValue action_data in homeRenovation.RenovateActions)
				{
					if (action_data.Type == "Value")
					{
						try
						{
							field = (NetInt)farmhouse.GetType().GetField(action_data.Key).GetValue(farmhouse);
							if (field == null)
							{
								flag = false;
								break;
							}
							Action<HouseRenovation, int> b2 = delegate(HouseRenovation selected_renovation, int index)
							{
								if (action_data.Value == "selected")
								{
									field.Value = index;
								}
								else
								{
									int value2 = int.Parse(action_data.Value);
									field.Value = value2;
								}
							};
							HouseRenovation houseRenovation3 = houseRenovation;
							houseRenovation3.onRenovation = (Action<HouseRenovation, int>)Delegate.Combine(houseRenovation3.onRenovation, b2);
							continue;
						}
						catch (Exception)
						{
							flag = false;
						}
						break;
					}
					if (!(action_data.Type == "Mail"))
					{
						continue;
					}
					Action<HouseRenovation, int> b3 = delegate
					{
						if (action_data.Value == "0")
						{
							Game1.player.mailReceived.Remove(action_data.Key);
						}
						else
						{
							Game1.player.mailReceived.Add(action_data.Key);
						}
					};
					HouseRenovation houseRenovation4 = houseRenovation;
					houseRenovation4.onRenovation = (Action<HouseRenovation, int>)Delegate.Combine(houseRenovation4.onRenovation, b3);
				}
				if (flag)
				{
					HouseRenovation houseRenovation5 = houseRenovation;
					houseRenovation5.onRenovation = (Action<HouseRenovation, int>)Delegate.Combine(houseRenovation5.onRenovation, (Action<HouseRenovation, int>)delegate
					{
						farmhouse.UpdateForRenovation();
					});
					list.Add(houseRenovation);
				}
			}
			return list;
		}

		public static bool EnsureNoObstructions(HouseRenovation renovation, int selected_index)
		{
			if (renovation.location != null)
			{
				foreach (Rectangle item in renovation.renovationBounds[selected_index])
				{
					for (int i = item.Left; i < item.Right; i++)
					{
						for (int j = item.Top; j < item.Bottom; j++)
						{
							if (renovation.location.isTileOccupiedByFarmer(new Vector2(i, j)) != null)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:RenovationBlocked"));
								return false;
							}
							if (renovation.location.isTileOccupied(new Vector2(i, j)))
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:RenovationBlocked"));
								return false;
							}
						}
					}
					Rectangle value = new Rectangle(item.X * 64, item.Y * 64, item.Width * 64, item.Height * 64);
					if (!(renovation.location is DecoratableLocation decoratableLocation))
					{
						continue;
					}
					foreach (Furniture item2 in decoratableLocation.furniture)
					{
						if (item2.getBoundingBox(item2.tileLocation).Intersects(value))
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:RenovationBlocked"));
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		public static void BuildCrib(HouseRenovation renovation, int selected_index)
		{
			if (renovation.location is FarmHouse farmHouse)
			{
				farmHouse.cribStyle.Value = 1;
			}
		}

		public static void RemoveCrib(HouseRenovation renovation, int selected_index)
		{
			if (renovation.location is FarmHouse farmHouse)
			{
				farmHouse.cribStyle.Value = 0;
			}
		}

		public static void OpenBedroom(HouseRenovation renovation, int selected_index)
		{
			if (renovation.location is FarmHouse farmHouse)
			{
				Game1.player.mailReceived.Add("renovation_bedroom_open");
				farmHouse.UpdateForRenovation();
			}
		}

		public static void CloseBedroom(HouseRenovation renovation, int selected_index)
		{
			if (renovation.location is FarmHouse farmHouse)
			{
				Game1.player.mailReceived.Remove("renovation_bedroom_open");
				farmHouse.UpdateForRenovation();
			}
		}

		public static void OpenSouthernRoom(HouseRenovation renovation, int selected_index)
		{
			if (renovation.location is FarmHouse farmHouse)
			{
				Game1.player.mailReceived.Add("renovation_southern_open");
				farmHouse.UpdateForRenovation();
			}
		}

		public static void CloseSouthernRoom(HouseRenovation renovation, int selected_index)
		{
			if (renovation.location is FarmHouse farmHouse)
			{
				Game1.player.mailReceived.Remove("renovation_southern_open");
				farmHouse.UpdateForRenovation();
			}
		}

		public static void OpenCornernRoom(HouseRenovation renovation, int selected_index)
		{
			if (renovation.location is FarmHouse farmHouse)
			{
				Game1.player.mailReceived.Add("renovation_corner_open");
				farmHouse.UpdateForRenovation();
			}
		}

		public static void CloseCornerRoom(HouseRenovation renovation, int selected_index)
		{
			if (renovation.location is FarmHouse farmHouse)
			{
				Game1.player.mailReceived.Remove("renovation_corner_open");
				farmHouse.UpdateForRenovation();
			}
		}

		public static bool OnPurchaseRenovation(ISalable salable, Farmer who, int amount)
		{
			if (salable is HouseRenovation renovation)
			{
				Game1.activeClickableMenu = new RenovateMenu(renovation);
				return true;
			}
			return false;
		}

		public virtual void AddRenovationBound(Rectangle bound)
		{
			List<Rectangle> list = new List<Rectangle>();
			list.Add(bound);
			renovationBounds.Add(list);
		}

		public virtual void AddRenovationBound(List<Rectangle> bounds)
		{
			renovationBounds.Add(bounds);
		}
	}
}
