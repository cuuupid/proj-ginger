using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using StardewValley.GameData;

namespace StardewValley
{
	public class BundleGenerator
	{
		public List<RandomBundleData> randomBundleData;

		public Dictionary<string, string> bundleData;

		public Dictionary<string, int> itemNameLookup;

		public Random random;

		public Dictionary<string, string> Generate(string bundle_data_path, Random rng)
		{
			random = rng;
			randomBundleData = Game1.content.Load<List<RandomBundleData>>(bundle_data_path);
			this.bundleData = new Dictionary<string, string>();
			Dictionary<string, string> dictionary = Game1.content.LoadBase<Dictionary<string, string>>("Data\\Bundles");
			foreach (string key in dictionary.Keys)
			{
				this.bundleData[key] = dictionary[key];
			}
			foreach (RandomBundleData randomBundleDatum in randomBundleData)
			{
				List<int> list = new List<int>();
				string[] array = randomBundleDatum.Keys.Trim().Split(' ');
				Dictionary<int, BundleData> dictionary2 = new Dictionary<int, BundleData>();
				string[] array2 = array;
				foreach (string s in array2)
				{
					list.Add(int.Parse(s));
				}
				BundleSetData bundleSetData = Utility.GetRandom(randomBundleDatum.BundleSets, random);
				if (bundleSetData != null)
				{
					foreach (BundleData bundle in bundleSetData.Bundles)
					{
						dictionary2[bundle.Index] = bundle;
					}
				}
				List<BundleData> list2 = new List<BundleData>();
				foreach (BundleData bundle2 in randomBundleDatum.Bundles)
				{
					list2.Add(bundle2);
				}
				for (int j = 0; j < list.Count; j++)
				{
					if (dictionary2.ContainsKey(j))
					{
						continue;
					}
					List<BundleData> list3 = new List<BundleData>();
					foreach (BundleData item2 in list2)
					{
						if (item2.Index == j)
						{
							list3.Add(item2);
						}
					}
					if (list3.Count > 0)
					{
						BundleData bundleData = Utility.GetRandom(list3, random);
						list2.Remove(bundleData);
						dictionary2[j] = bundleData;
						continue;
					}
					foreach (BundleData item3 in list2)
					{
						if (item3.Index == -1)
						{
							list3.Add(item3);
						}
					}
					if (list3.Count > 0)
					{
						BundleData bundleData2 = Utility.GetRandom(list3, random);
						list2.Remove(bundleData2);
						dictionary2[j] = bundleData2;
					}
				}
				foreach (int key2 in dictionary2.Keys)
				{
					BundleData bundleData3 = dictionary2[key2];
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(bundleData3.Name);
					stringBuilder.Append("/");
					string text = bundleData3.Reward;
					if (text.Length > 0)
					{
						try
						{
							if (char.IsDigit(text[0]))
							{
								string[] array3 = text.Split(' ');
								int stack_count = int.Parse(array3[0]);
								string query = string.Join(" ", array3, 1, array3.Length - 1);
								Item item = Utility.fuzzyItemSearch(query, stack_count);
								if (item != null)
								{
									text = Utility.getStandardDescriptionFromItem(item, item.Stack);
								}
							}
						}
						catch (Exception)
						{
							Console.WriteLine("ERROR: Malformed reward string in bundle: " + text);
							text = bundleData3.Reward;
						}
					}
					stringBuilder.Append(text);
					stringBuilder.Append("/");
					int color = 0;
					if (bundleData3.Color == "Red")
					{
						color = 4;
					}
					else if (bundleData3.Color == "Blue")
					{
						color = 5;
					}
					else if (bundleData3.Color == "Green")
					{
						color = 0;
					}
					else if (bundleData3.Color == "Orange")
					{
						color = 2;
					}
					else if (bundleData3.Color == "Purple")
					{
						color = 1;
					}
					else if (bundleData3.Color == "Teal")
					{
						color = 6;
					}
					else if (bundleData3.Color == "Yellow")
					{
						color = 3;
					}
					ParseItemList(stringBuilder, bundleData3.Items, bundleData3.Pick, bundleData3.RequiredItems, color);
					stringBuilder.Append("/");
					stringBuilder.Append(bundleData3.Sprite);
					this.bundleData[randomBundleDatum.AreaName + "/" + list[key2]] = stringBuilder.ToString();
				}
			}
			return this.bundleData;
		}

		public string ParseRandomTags(string data)
		{
			int num = 0;
			do
			{
				num = data.LastIndexOf('[');
				if (num >= 0)
				{
					int num2 = data.IndexOf(']', num);
					if (num2 == -1)
					{
						return data;
					}
					string text = data.Substring(num + 1, num2 - num - 1);
					string value = Utility.GetRandom(new List<string>(text.Split('|')), random);
					data = data.Remove(num, num2 - num + 1);
					data = data.Insert(num, value);
				}
			}
			while (num >= 0);
			return data;
		}

		public Item ParseItemString(string item_string)
		{
			string text = item_string.Trim();
			string[] array = text.Split(' ');
			int num = 0;
			int num2 = int.Parse(array[num]);
			num++;
			int quality = 0;
			if (array[num] == "NQ")
			{
				quality = 0;
				num++;
			}
			else if (array[num] == "SQ")
			{
				quality = 1;
				num++;
			}
			else if (array[num] == "GQ")
			{
				quality = 2;
				num++;
			}
			else if (array[num] == "IQ")
			{
				quality = 3;
				num++;
			}
			string text2 = string.Join(" ", array, num, array.Length - num);
			if (char.IsDigit(text2[0]))
			{
				Item item = new Object(int.Parse(text2), num2);
				(item as Object).Quality = quality;
				return item;
			}
			Item item2 = null;
			if (text2.ToLowerInvariant().EndsWith("category"))
			{
				try
				{
					FieldInfo field = typeof(Object).GetField(text2);
					if (field != null)
					{
						int parentSheetIndex = (int)field.GetValue(null);
						item2 = new Object(Vector2.Zero, parentSheetIndex, 1);
					}
				}
				catch (Exception)
				{
				}
			}
			if (item2 == null)
			{
				item2 = Utility.fuzzyItemSearch(text2);
				if (item2 is Object)
				{
					(item2 as Object).Quality = quality;
				}
			}
			if (item2 == null)
			{
				throw new Exception("Invalid item name '" + text2 + "' encountered while generating a bundle.");
			}
			item2.Stack = num2;
			return item2;
		}

		public void ParseItemList(StringBuilder builder, string item_list, int pick_count, int required_items, int color)
		{
			item_list = ParseRandomTags(item_list);
			string[] array = item_list.Split(',');
			List<string> list = new List<string>();
			for (int i = 0; i < array.Length; i++)
			{
				Item item = ParseItemString(array[i]);
				list.Add(item.ParentSheetIndex + " " + item.Stack + " " + (item as Object).Quality);
			}
			if (pick_count < 0)
			{
				pick_count = list.Count;
			}
			if (required_items < 0)
			{
				required_items = pick_count;
			}
			while (list.Count > pick_count)
			{
				int index = random.Next(list.Count);
				list.RemoveAt(index);
			}
			for (int j = 0; j < list.Count; j++)
			{
				builder.Append(list[j]);
				if (j < list.Count - 1)
				{
					builder.Append(" ");
				}
			}
			builder.Append("/");
			builder.Append(color);
			builder.Append("/");
			builder.Append(required_items);
		}
	}
}
