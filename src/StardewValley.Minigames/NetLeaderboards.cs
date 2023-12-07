using System.Collections.Generic;
using Netcode;

namespace StardewValley.Minigames
{
	public class NetLeaderboards : INetObject<NetFields>
	{
		public NetObjectList<NetLeaderboardsEntry> entries = new NetObjectList<NetLeaderboardsEntry>();

		public NetInt maxEntries = new NetInt(10);

		public NetFields NetFields { get; } = new NetFields();


		public void InitNetFields()
		{
			NetFields.AddFields(entries, maxEntries);
		}

		public NetLeaderboards()
		{
			InitNetFields();
		}

		public void AddScore(string name, int score)
		{
			List<NetLeaderboardsEntry> list = new List<NetLeaderboardsEntry>(entries);
			list.Add(new NetLeaderboardsEntry(name, score));
			list.Sort((NetLeaderboardsEntry a, NetLeaderboardsEntry b) => a.score.Value.CompareTo(b.score.Value));
			list.Reverse();
			while (list.Count > maxEntries.Value)
			{
				list.RemoveAt(list.Count - 1);
			}
			entries.Set(list);
		}

		public List<KeyValuePair<string, int>> GetScores()
		{
			List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
			foreach (NetLeaderboardsEntry entry in entries)
			{
				list.Add(new KeyValuePair<string, int>(entry.name.Value, entry.score.Value));
			}
			list.Sort((KeyValuePair<string, int> a, KeyValuePair<string, int> b) => a.Value.CompareTo(b.Value));
			list.Reverse();
			return list;
		}

		public void LoadScores(List<KeyValuePair<string, int>> scores)
		{
			entries.Clear();
			foreach (KeyValuePair<string, int> score in scores)
			{
				AddScore(score.Key, score.Value);
			}
		}
	}
}
