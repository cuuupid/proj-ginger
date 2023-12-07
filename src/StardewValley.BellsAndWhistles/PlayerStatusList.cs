using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.BellsAndWhistles
{
	public class PlayerStatusList : INetObject<NetFields>
	{
		public enum SortMode
		{
			None,
			NumberSort,
			NumberSortDescending,
			AlphaSort,
			AlphaSortDescending
		}

		public enum DisplayMode
		{
			Text,
			LocalizedText,
			Icons
		}

		public enum VerticalAlignment
		{
			Top,
			Bottom
		}

		public enum HorizontalAlignment
		{
			Left,
			Right
		}

		protected readonly NetLongDictionary<string, NetString> _statusList = new NetLongDictionary<string, NetString>();

		protected Dictionary<long, string> _formattedStatusList = new Dictionary<long, string>();

		protected Dictionary<string, Texture2D> _iconSprites = new Dictionary<string, Texture2D>();

		protected List<Farmer> _sortedFarmers = new List<Farmer>();

		public int iconAnimationFrames = 1;

		public int largestSpriteWidth;

		public int largestSpriteHeight;

		public SortMode sortMode;

		public DisplayMode displayMode;

		protected Dictionary<string, KeyValuePair<string, Rectangle>> _iconDefinitions = new Dictionary<string, KeyValuePair<string, Rectangle>>();

		public NetFields NetFields { get; } = new NetFields();


		public PlayerStatusList()
		{
			InitNetFields();
			_iconDefinitions = new Dictionary<string, KeyValuePair<string, Rectangle>>();
			_formattedStatusList = new Dictionary<long, string>();
		}

		public void InitNetFields()
		{
			_statusList.InterpolationWait = false;
			NetFields.AddFields(_statusList);
			_statusList.OnConflictResolve += delegate
			{
				_OnValueChanged();
			};
			_statusList.OnValueAdded += delegate
			{
				_OnValueChanged();
			};
			_statusList.OnValueRemoved += delegate
			{
				_OnValueChanged();
			};
		}

		public void AddSpriteDefinition(string key, string file, int x, int y, int width, int height)
		{
			if (!_iconSprites.ContainsKey(file) || _iconSprites[file].IsDisposed)
			{
				_iconSprites[file] = Game1.content.Load<Texture2D>(file);
			}
			_iconDefinitions[key] = new KeyValuePair<string, Rectangle>(file, new Rectangle(x, y, width, height));
			if (width > largestSpriteWidth)
			{
				largestSpriteWidth = width;
			}
			if (height > largestSpriteHeight)
			{
				largestSpriteHeight = height;
			}
		}

		public void UpdateState(string new_state)
		{
			if (!_statusList.ContainsKey(Game1.player.UniqueMultiplayerID) || _statusList[Game1.player.UniqueMultiplayerID] != new_state)
			{
				_statusList.Remove(Game1.player.UniqueMultiplayerID);
				_statusList.Add(Game1.player.UniqueMultiplayerID, new_state);
			}
		}

		public void WithdrawState()
		{
			if (_statusList.ContainsKey(Game1.player.UniqueMultiplayerID))
			{
				_statusList.Remove(Game1.player.UniqueMultiplayerID);
			}
		}

		protected void _OnValueChanged()
		{
			foreach (long key in _statusList.Keys)
			{
				_formattedStatusList[key] = GetStatusText(key);
			}
			_ResortList();
		}

		protected void _ResortList()
		{
			_sortedFarmers.Clear();
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				_sortedFarmers.Add(onlineFarmer);
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (Game1.IsMasterGame && !_sortedFarmers.Contains(allFarmer) && _statusList.ContainsKey(allFarmer.UniqueMultiplayerID))
				{
					_statusList.Remove(allFarmer.UniqueMultiplayerID);
				}
				if (!_statusList.ContainsKey(allFarmer.UniqueMultiplayerID))
				{
					_sortedFarmers.Remove(allFarmer);
				}
			}
			if (sortMode == SortMode.AlphaSort || sortMode == SortMode.AlphaSortDescending)
			{
				_sortedFarmers.Sort((Farmer a, Farmer b) => GetStatusText(a.UniqueMultiplayerID).CompareTo(GetStatusText(b.UniqueMultiplayerID)));
				if (sortMode == SortMode.AlphaSortDescending)
				{
					_sortedFarmers.Reverse();
				}
			}
			else if (sortMode == SortMode.NumberSort || sortMode == SortMode.NumberSortDescending)
			{
				_sortedFarmers.Sort((Farmer a, Farmer b) => int.Parse(GetStatusText(a.UniqueMultiplayerID)).CompareTo(int.Parse(GetStatusText(b.UniqueMultiplayerID))));
				if (sortMode == SortMode.NumberSortDescending)
				{
					_sortedFarmers.Reverse();
				}
			}
		}

		public string GetStatusText(long id)
		{
			if (_statusList.ContainsKey(id))
			{
				if (displayMode == DisplayMode.LocalizedText)
				{
					return Game1.content.LoadString(_statusList[id]);
				}
				return _statusList[id];
			}
			return "";
		}

		public void Draw(SpriteBatch b, Vector2 draw_position, float draw_scale = 4f, float draw_layer = 0.45f, HorizontalAlignment horizontal_origin = HorizontalAlignment.Left, VerticalAlignment vertical_origin = VerticalAlignment.Top)
		{
			float num = 12f;
			if (displayMode == DisplayMode.Icons && (float)largestSpriteHeight > num)
			{
				num = largestSpriteHeight;
			}
			if (horizontal_origin == HorizontalAlignment.Right)
			{
				float num2 = 0f;
				if (displayMode == DisplayMode.Icons)
				{
					draw_position.X -= (float)largestSpriteWidth * draw_scale;
				}
				else
				{
					foreach (Farmer sortedFarmer in _sortedFarmers)
					{
						if (_formattedStatusList.ContainsKey(sortedFarmer.UniqueMultiplayerID))
						{
							float x = Game1.dialogueFont.MeasureString(_formattedStatusList[sortedFarmer.UniqueMultiplayerID]).X;
							if (num2 < x)
							{
								num2 = x;
							}
						}
					}
					draw_position.X -= (num2 + 16f) * draw_scale;
				}
			}
			if (vertical_origin == VerticalAlignment.Bottom)
			{
				draw_position.Y -= num * (float)_statusList.Count() * draw_scale;
			}
			foreach (Farmer sortedFarmer2 in _sortedFarmers)
			{
				float num3 = ((!Game1.isUsingBackToFrontSorting) ? 1 : (-1));
				if (_formattedStatusList.ContainsKey(sortedFarmer2.UniqueMultiplayerID))
				{
					Vector2 zero = Vector2.Zero;
					sortedFarmer2.FarmerRenderer.drawMiniPortrat(b, draw_position, draw_layer, draw_scale * 0.75f, 2, sortedFarmer2);
					if (displayMode == DisplayMode.Icons && _iconDefinitions.ContainsKey(_formattedStatusList[sortedFarmer2.UniqueMultiplayerID]))
					{
						zero.X += 12f * draw_scale;
						KeyValuePair<string, Rectangle> keyValuePair = _iconDefinitions[_formattedStatusList[sortedFarmer2.UniqueMultiplayerID]];
						Rectangle value = keyValuePair.Value;
						value.Y = (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % (double)(iconAnimationFrames * 100) / 100.0) * 16;
						b.Draw(_iconSprites[keyValuePair.Key], draw_position + zero, value, Color.White, 0f, Vector2.Zero, draw_scale, SpriteEffects.None, draw_layer - 0.0001f * num3);
					}
					else
					{
						zero.X += 16f * draw_scale;
						zero.Y += 2f * draw_scale;
						string text = _formattedStatusList[sortedFarmer2.UniqueMultiplayerID];
						b.DrawString(Game1.dialogueFont, text, draw_position + zero + Vector2.One * draw_scale, Color.Black, 0f, Vector2.Zero, draw_scale / 4f, SpriteEffects.None, draw_layer - 0.0001f * num3);
						b.DrawString(Game1.dialogueFont, text, draw_position + zero, Color.White, 0f, Vector2.Zero, draw_scale / 4f, SpriteEffects.None, draw_layer);
					}
					draw_position.Y += num * draw_scale;
				}
			}
		}
	}
}
