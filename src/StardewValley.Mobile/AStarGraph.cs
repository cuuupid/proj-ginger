using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using xTile;

namespace StardewValley.Mobile
{
	public class AStarGraph
	{
		public GameLocation gameLocation;

		public Map map;

		private AStarNode[,] _aStarNodeArray;

		protected List<AStarNode> _nodes = new List<AStarNode>();

		public AStarNode FarmerAStarNode => FetchAStarNode((int)Math.Floor(Game1.player.position.X / 64f), (int)Math.Floor(Game1.player.position.Y / 64f));

		public AStarNode FarmerAStarNodeOffset
		{
			get
			{
				Vector2 vector = new Vector2(Game1.player.position.X + 32f, Game1.player.position.Y + 32f);
				AStarNode aStarNode = FetchAStarNode((int)(vector.X / 64f), (int)(vector.Y / 64f));
				if (aStarNode == null && Game1.currentLocation is FarmHouse)
				{
					aStarNode = FetchNeighbourNodeThatIsPassible((int)(vector.X / 64f), (int)(vector.Y / 64f));
				}
				return aStarNode;
			}
		}

		public virtual List<AStarNode> Nodes => _nodes;

		public void Init(GameLocation gameLocation)
		{
			this.gameLocation = gameLocation;
			map = gameLocation.map;
			int layerWidth = map.Layers[0].LayerWidth;
			int layerHeight = map.Layers[0].LayerHeight;
			_aStarNodeArray = new AStarNode[layerWidth, layerHeight];
			for (int i = 0; i < layerWidth; i++)
			{
				for (int j = 0; j < layerHeight; j++)
				{
					AStarNode aStarNode = new AStarNode(this, i, j);
					_aStarNodeArray[i, j] = aStarNode;
				}
			}
		}

		public AStarNode FetchAStarNode(int x, int y)
		{
			if (x >= 0 && x < _aStarNodeArray.GetLength(0) && y >= 0 && y < _aStarNodeArray.GetLength(1))
			{
				return _aStarNodeArray[x, y];
			}
			return null;
		}

		public AStarNode FetchNeighbourNodeThatIsPassible(int x, int y)
		{
			AStarNode aStarNode = FetchAStarNode(x + 1, y);
			if (aStarNode != null && aStarNode.isTilePassable() && aStarNode.TileClear)
			{
				return aStarNode;
			}
			aStarNode = FetchAStarNode(x - 1, y);
			if (aStarNode != null && aStarNode.isTilePassable() && aStarNode.TileClear)
			{
				return aStarNode;
			}
			aStarNode = FetchAStarNode(x, y + 1);
			if (aStarNode != null && aStarNode.isTilePassable() && aStarNode.TileClear)
			{
				return aStarNode;
			}
			aStarNode = FetchAStarNode(x, y - 1);
			if (aStarNode != null && aStarNode.isTilePassable() && aStarNode.TileClear)
			{
				return aStarNode;
			}
			return null;
		}

		public void AddNode(AStarNode node)
		{
			_nodes.Add(node);
		}

		public AStarPath GetShortestPathDijkstra(AStarNode startNode, AStarNode endNode)
		{
			if (startNode == null || endNode == null)
			{
				throw new ArgumentNullException();
			}
			AStarPath aStarPath = new AStarPath();
			if (startNode == endNode)
			{
				aStarPath.nodes.Add(startNode);
				return aStarPath;
			}
			List<AStarNode> list = new List<AStarNode>();
			Dictionary<AStarNode, AStarNode> dictionary = new Dictionary<AStarNode, AStarNode>();
			Dictionary<AStarNode, float> distances = new Dictionary<AStarNode, float>();
			for (int i = 0; i < _nodes.Count; i++)
			{
				AStarNode aStarNode = _nodes[i];
				list.Add(aStarNode);
				distances.Add(aStarNode, 3.4028235E+38f);
			}
			distances[startNode] = 0f;
			while (list.Count != 0)
			{
				list = list.OrderBy((AStarNode node) => distances[node]).ToList();
				AStarNode aStarNode2 = list[0];
				list.Remove(aStarNode2);
				if (aStarNode2 == endNode)
				{
					while (dictionary.ContainsKey(aStarNode2))
					{
						aStarPath.nodes.Insert(0, aStarNode2);
						aStarNode2 = dictionary[aStarNode2];
					}
					aStarPath.nodes.Insert(0, aStarNode2);
					break;
				}
				for (int j = 0; j < aStarNode2.NeighbouringNodeList.Count; j++)
				{
					AStarNode aStarNode3 = aStarNode2.NeighbouringNodeList[j];
					float num = Distance(aStarNode2.x, aStarNode2.y, aStarNode3.x, aStarNode3.y);
					float num2 = distances[aStarNode2] + num;
					if (num2 < distances[aStarNode3])
					{
						distances[aStarNode3] = num2;
						dictionary[aStarNode3] = aStarNode2;
					}
				}
			}
			aStarPath.Bake();
			return aStarPath;
		}

		public AStarPath GetShortestPathAStar(AStarNode startNode, AStarNode endNode)
		{
			if (startNode == null || endNode == null)
			{
				return null;
			}
			List<AStarNode> list = new List<AStarNode>();
			HashSet<AStarNode> hashSet = new HashSet<AStarNode>();
			list.Add(startNode);
			bool flag = gameLocation is DecoratableLocation && !endNode.isBlockingBedTile();
			while (list.Count > 0)
			{
				AStarNode aStarNode = list[0];
				for (int i = 1; i < list.Count; i++)
				{
					if ((list[i].fCost < aStarNode.fCost || list[i].fCost == aStarNode.fCost) && list[i].hCost < aStarNode.hCost)
					{
						aStarNode = list[i];
					}
				}
				list.Remove(aStarNode);
				hashSet.Add(aStarNode);
				if (aStarNode == endNode)
				{
					return RetracePath(startNode, endNode);
				}
				foreach (AStarNode neighbouringNode in aStarNode.NeighbouringNodeList)
				{
					if (hashSet.Contains(neighbouringNode) || (flag && neighbouringNode.isBlockingBedTile()))
					{
						continue;
					}
					float num = aStarNode.gCost + 1f;
					if (num < neighbouringNode.gCost || !list.Contains(neighbouringNode))
					{
						neighbouringNode.gCost = num;
						neighbouringNode.hCost = Distance(neighbouringNode.x, neighbouringNode.y, endNode.x, endNode.y);
						neighbouringNode.parentNode = aStarNode;
						if (!list.Contains(neighbouringNode))
						{
							list.Add(neighbouringNode);
						}
					}
				}
			}
			return null;
		}

		public AStarPath RetracePath(AStarNode startNode, AStarNode endNode)
		{
			AStarPath aStarPath = new AStarPath();
			for (AStarNode aStarNode = endNode; aStarNode != startNode; aStarNode = aStarNode.parentNode)
			{
				aStarPath.nodes.Add(aStarNode);
			}
			aStarPath.nodes.Reverse();
			return aStarPath;
		}

		public AStarPath SmoothRightAngles(AStarPath path, int endNodesToLeave = 1)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < path.nodes.Count - 1 - endNodesToLeave; i++)
			{
				if (DiagonalWalkDirection(path, i) != 0)
				{
					i++;
					list.Add(i);
				}
			}
			if (list.Count > 0)
			{
				List<AStarNode> list2 = new List<AStarNode>(path.nodes);
				for (int num = list.Count - 1; num >= 0; num--)
				{
					int index = list[num];
					list2.RemoveAt(index);
				}
				path.nodes = list2;
			}
			return path;
		}

		private float Distance(int x1, int y1, int x2, int y2)
		{
			return (float)(Math.Pow(x1 - x2, 2.0) + Math.Pow(y1 - y2, 2.0));
		}

		public bool IsNeighbouringNode(AStarNode startNode, AStarNode endNode)
		{
			if (startNode == null || endNode == null)
			{
				return false;
			}
			if (endNode.x >= startNode.x - 1 && endNode.x <= startNode.x + 1 && endNode.y >= startNode.y - 1 && endNode.y <= startNode.y + 1)
			{
				return !IsSameNode(startNode, endNode);
			}
			return false;
		}

		public bool IsNeighbouringNodeNoDiagonals(AStarNode startNode, AStarNode endNode)
		{
			if (startNode == null || endNode == null)
			{
				return false;
			}
			if (endNode.x != startNode.x || (endNode.y != startNode.y + 1 && endNode.y != startNode.y - 1))
			{
				if (endNode.y == startNode.y)
				{
					if (endNode.x != startNode.x + 1)
					{
						return endNode.x == startNode.x - 1;
					}
					return true;
				}
				return false;
			}
			return true;
		}

		public bool IsNeighbouringNodeOnDiagonal(AStarNode startNode, AStarNode endNode)
		{
			if (startNode == null || endNode == null)
			{
				return false;
			}
			if (endNode.x == startNode.x - 1 || endNode.x == startNode.x + 1)
			{
				if (endNode.y != startNode.y - 1)
				{
					return endNode.y == startNode.y + 1;
				}
				return true;
			}
			return false;
		}

		public bool IsSameNode(AStarNode startNode, AStarNode endNode)
		{
			if (startNode == null || endNode == null)
			{
				return false;
			}
			if (endNode.x == startNode.x)
			{
				return endNode.y == startNode.y;
			}
			return false;
		}

		public WalkDirection OppositeWalkDirection(WalkDirection walkDirection)
		{
			return walkDirection switch
			{
				WalkDirection.Up => WalkDirection.Down, 
				WalkDirection.Down => WalkDirection.Up, 
				WalkDirection.Left => WalkDirection.Right, 
				WalkDirection.Right => WalkDirection.Left, 
				WalkDirection.UpLeft => WalkDirection.DownRight, 
				WalkDirection.UpRight => WalkDirection.DownLeft, 
				WalkDirection.DownLeft => WalkDirection.UpRight, 
				WalkDirection.DownRight => WalkDirection.UpLeft, 
				_ => WalkDirection.None, 
			};
		}

		public bool AreOppositeWalkDirection(WalkDirection walkDirectionA, WalkDirection walkDirectionB)
		{
			if ((walkDirectionA == WalkDirection.Up || walkDirectionA == WalkDirection.UpLeft || walkDirectionA == WalkDirection.UpRight) && (walkDirectionB == WalkDirection.Down || walkDirectionB == WalkDirection.DownLeft || walkDirectionB == WalkDirection.DownRight))
			{
				return true;
			}
			if ((walkDirectionA == WalkDirection.Left || walkDirectionA == WalkDirection.UpLeft || walkDirectionA == WalkDirection.DownLeft) && (walkDirectionB == WalkDirection.Right || walkDirectionB == WalkDirection.UpRight || walkDirectionB == WalkDirection.DownRight))
			{
				return true;
			}
			if ((walkDirectionA == WalkDirection.Right || walkDirectionA == WalkDirection.UpRight || walkDirectionA == WalkDirection.DownRight) && (walkDirectionB == WalkDirection.Left || walkDirectionB == WalkDirection.UpLeft || walkDirectionB == WalkDirection.DownLeft))
			{
				return true;
			}
			if ((walkDirectionA == WalkDirection.Down || walkDirectionA == WalkDirection.DownLeft || walkDirectionA == WalkDirection.DownRight) && (walkDirectionB == WalkDirection.Up || walkDirectionB == WalkDirection.UpLeft || walkDirectionB == WalkDirection.UpRight))
			{
				return true;
			}
			return false;
		}

		public WalkDirection WalkDirectionToNextNode(AStarNode startNode, AStarNode endNode)
		{
			if (startNode == null || endNode == null)
			{
				return WalkDirection.None;
			}
			if (startNode.x == endNode.x + 1 && startNode.y == endNode.y + 1)
			{
				return WalkDirection.UpLeft;
			}
			if (startNode.x == endNode.x - 1 && startNode.y == endNode.y + 1)
			{
				return WalkDirection.UpRight;
			}
			if (startNode.x == endNode.x + 1 && startNode.y == endNode.y - 1)
			{
				return WalkDirection.DownLeft;
			}
			if (startNode.x == endNode.x - 1 && startNode.y == endNode.y - 1)
			{
				return WalkDirection.DownRight;
			}
			if (startNode.x == endNode.x && startNode.y == endNode.y - 1)
			{
				return WalkDirection.Down;
			}
			if (startNode.x == endNode.x && startNode.y == endNode.y + 1)
			{
				return WalkDirection.Up;
			}
			if (startNode.x == endNode.x + 1 && startNode.y == endNode.y)
			{
				return WalkDirection.Left;
			}
			if (startNode.x == endNode.x - 1 && startNode.y == endNode.y)
			{
				return WalkDirection.Right;
			}
			return WalkDirection.None;
		}

		public WalkDirection WalkDirectionBetweenNodes(AStarNode startNode, AStarNode endNode)
		{
			if (startNode.x > endNode.x && startNode.y > endNode.y)
			{
				return WalkDirection.UpLeft;
			}
			if (startNode.x < endNode.x && startNode.y > endNode.y)
			{
				return WalkDirection.UpRight;
			}
			if (startNode.x > endNode.x && startNode.y < endNode.y)
			{
				return WalkDirection.DownLeft;
			}
			if (startNode.x < endNode.x && startNode.y < endNode.y)
			{
				return WalkDirection.DownRight;
			}
			if (startNode.x == endNode.x && startNode.y < endNode.y)
			{
				return WalkDirection.Down;
			}
			if (startNode.x == endNode.x && startNode.y > endNode.y)
			{
				return WalkDirection.Up;
			}
			if (startNode.x > endNode.x && startNode.y == endNode.y)
			{
				return WalkDirection.Left;
			}
			if (startNode.x < endNode.x && startNode.y == endNode.y)
			{
				return WalkDirection.Right;
			}
			return WalkDirection.None;
		}

		public WalkDirection WalkDirectionBetweenTwoPoints(Vector2 start, Vector2 end, float threshold = 0f)
		{
			float num = Math.Abs(start.X - end.X);
			float num2 = Math.Abs(start.Y - end.Y);
			if (start.X > end.X && start.Y > end.Y && num >= threshold && num2 >= threshold)
			{
				return WalkDirection.UpLeft;
			}
			if (start.X < end.X && start.Y > end.Y && num >= threshold && num2 >= threshold)
			{
				return WalkDirection.UpRight;
			}
			if (start.X > end.X && start.Y < end.Y && num >= threshold && num2 >= threshold)
			{
				return WalkDirection.DownLeft;
			}
			if (start.X < end.X && start.Y < end.Y && num >= threshold && num2 >= threshold)
			{
				return WalkDirection.DownRight;
			}
			if (start.Y > end.Y && num2 > num)
			{
				return WalkDirection.Up;
			}
			if (start.Y < end.Y && num2 > num)
			{
				return WalkDirection.Down;
			}
			if (start.X > end.X)
			{
				return WalkDirection.Left;
			}
			if (start.X < end.X)
			{
				return WalkDirection.Right;
			}
			return WalkDirection.None;
		}

		public WalkDirection WalkDirectionBetweenTwoPointsNoDiagonals(Vector2 start, Vector2 end)
		{
			float num = Math.Abs(start.X - end.X);
			float num2 = Math.Abs(start.Y - end.Y);
			if (start.Y > end.Y && num2 > num)
			{
				return WalkDirection.Up;
			}
			if (start.Y < end.Y && num2 > num)
			{
				return WalkDirection.Down;
			}
			if (start.X > end.X)
			{
				return WalkDirection.Left;
			}
			if (start.X < end.X)
			{
				return WalkDirection.Right;
			}
			return WalkDirection.None;
		}

		public WalkDirection WalkDirectionBetweenTwoNodes(AStarNode start, AStarNode end)
		{
			float num = Math.Abs(start.x - end.x);
			float num2 = Math.Abs(start.y - end.y);
			if (start.y > end.y && num2 > num)
			{
				return WalkDirection.Up;
			}
			if (start.y < end.y && num2 > num)
			{
				return WalkDirection.Down;
			}
			if (start.x > end.x)
			{
				return WalkDirection.Left;
			}
			if (start.x < end.x)
			{
				return WalkDirection.Right;
			}
			return WalkDirection.None;
		}

		public WalkDirection WalkDirectionBetweenTwoTiles(Vector2 start, Vector2 end)
		{
			float num = end.X - start.X;
			float num2 = end.Y - start.Y;
			float num3 = 32f;
			if (num2 < 0f - num3 && Math.Abs(num) < num3)
			{
				return WalkDirection.Up;
			}
			if (num2 > num3 && Math.Abs(num) < num3)
			{
				return WalkDirection.Down;
			}
			if (num < 0f - num3 && Math.Abs(num2) < num3)
			{
				return WalkDirection.Left;
			}
			if (num > num3 && Math.Abs(num2) < num3)
			{
				return WalkDirection.Right;
			}
			if (num2 < 0f - num3 && num < 0f - num3)
			{
				return WalkDirection.UpLeft;
			}
			if (num2 < 0f - num3 && num > num3)
			{
				return WalkDirection.UpRight;
			}
			if (num2 > num3 && num < 0f - num3)
			{
				return WalkDirection.DownLeft;
			}
			if (num2 > num3 && num > num3)
			{
				return WalkDirection.DownRight;
			}
			if (Math.Abs(num) > Math.Abs(num2))
			{
				if (num < 0f)
				{
					return WalkDirection.Left;
				}
				return WalkDirection.Right;
			}
			if (num2 < 0f)
			{
				return WalkDirection.Up;
			}
			return WalkDirection.Down;
		}

		public WalkDirection WalkDirectionBetweenTwoPointsWithLastDirection(Vector2 start, Vector2 end, WalkDirection lastDirection, float threshold = 0f)
		{
			float num = Math.Abs(start.X - end.X);
			float num2 = Math.Abs(start.Y - end.Y);
			if (start.X > end.X && start.Y > end.Y && num >= threshold && num2 >= threshold && (lastDirection == WalkDirection.UpLeft || lastDirection == WalkDirection.Up || lastDirection == WalkDirection.Left || lastDirection == WalkDirection.None))
			{
				return WalkDirection.UpLeft;
			}
			if (start.X < end.X && start.Y > end.Y && num >= threshold && num2 >= threshold && (lastDirection == WalkDirection.UpRight || lastDirection == WalkDirection.Up || lastDirection == WalkDirection.Right || lastDirection == WalkDirection.None))
			{
				return WalkDirection.UpRight;
			}
			if (start.X > end.X && start.Y < end.Y && num >= threshold && num2 >= threshold && (lastDirection == WalkDirection.DownLeft || lastDirection == WalkDirection.Down || lastDirection == WalkDirection.Left || lastDirection == WalkDirection.None))
			{
				return WalkDirection.DownLeft;
			}
			if (start.X < end.X && start.Y < end.Y && num >= threshold && num2 >= threshold && (lastDirection == WalkDirection.DownRight || lastDirection == WalkDirection.Down || lastDirection == WalkDirection.Right || lastDirection == WalkDirection.None))
			{
				return WalkDirection.DownRight;
			}
			if (start.Y > end.Y && num2 >= threshold && (lastDirection == WalkDirection.Up || lastDirection == WalkDirection.UpLeft || lastDirection == WalkDirection.UpRight || lastDirection == WalkDirection.None))
			{
				return WalkDirection.Up;
			}
			if (start.Y < end.Y && num2 >= threshold && (lastDirection == WalkDirection.Down || lastDirection == WalkDirection.DownLeft || lastDirection == WalkDirection.DownRight || lastDirection == WalkDirection.None))
			{
				return WalkDirection.Down;
			}
			if (start.X > end.X && (lastDirection == WalkDirection.Left || lastDirection == WalkDirection.UpLeft || lastDirection == WalkDirection.DownLeft || lastDirection == WalkDirection.None))
			{
				return WalkDirection.Left;
			}
			if (start.X < end.X && (lastDirection == WalkDirection.Right || lastDirection == WalkDirection.UpRight || lastDirection == WalkDirection.DownRight || lastDirection == WalkDirection.None))
			{
				return WalkDirection.Right;
			}
			return WalkDirection.None;
		}

		private WalkDirection DiagonalWalkDirection(AStarPath path, int i)
		{
			if (((path.nodes[i + 1].x == path.nodes[i].x - 1 && path.nodes[i + 1].y == path.nodes[i].y) || (path.nodes[i + 1].x == path.nodes[i].x && path.nodes[i + 1].y == path.nodes[i].y + 1)) && path.nodes[i + 2].x == path.nodes[i].x - 1 && path.nodes[i + 2].y == path.nodes[i].y + 1)
			{
				int num = 0;
				for (int j = 0; j < path.nodes[i].NeighbouringNodeList.Count; j++)
				{
					if ((path.nodes[i].NeighbouringNodeList[j].x == path.nodes[i].x - 1 && path.nodes[i].NeighbouringNodeList[j].y == path.nodes[i].y) || (path.nodes[i].NeighbouringNodeList[j].x == path.nodes[i].x && path.nodes[i].NeighbouringNodeList[j].y == path.nodes[i].y + 1))
					{
						num++;
					}
				}
				if (num == 2)
				{
					return WalkDirection.DownLeft;
				}
			}
			else if (((path.nodes[i + 1].x == path.nodes[i].x + 1 && path.nodes[i + 1].y == path.nodes[i].y) || (path.nodes[i + 1].x == path.nodes[i].x && path.nodes[i + 1].y == path.nodes[i].y + 1)) && path.nodes[i + 2].x == path.nodes[i].x + 1 && path.nodes[i + 2].y == path.nodes[i].y + 1)
			{
				int num2 = 0;
				for (int k = 0; k < path.nodes[i].NeighbouringNodeList.Count; k++)
				{
					if ((path.nodes[i].NeighbouringNodeList[k].x == path.nodes[i].x + 1 && path.nodes[i].NeighbouringNodeList[k].y == path.nodes[i].y) || (path.nodes[i].NeighbouringNodeList[k].x == path.nodes[i].x && path.nodes[i].NeighbouringNodeList[k].y == path.nodes[i].y + 1))
					{
						num2++;
					}
				}
				if (num2 == 2)
				{
					return WalkDirection.DownRight;
				}
			}
			else if (((path.nodes[i + 1].x == path.nodes[i].x - 1 && path.nodes[i + 1].y == path.nodes[i].y) || (path.nodes[i + 1].x == path.nodes[i].x && path.nodes[i + 1].y == path.nodes[i].y - 1)) && path.nodes[i + 2].x == path.nodes[i].x - 1 && path.nodes[i + 2].y == path.nodes[i].y - 1)
			{
				int num3 = 0;
				for (int l = 0; l < path.nodes[i].NeighbouringNodeList.Count; l++)
				{
					if ((path.nodes[i].NeighbouringNodeList[l].x == path.nodes[i].x - 1 && path.nodes[i].NeighbouringNodeList[l].y == path.nodes[i].y) || (path.nodes[i].NeighbouringNodeList[l].x == path.nodes[i].x && path.nodes[i].NeighbouringNodeList[l].y == path.nodes[i].y - 1))
					{
						num3++;
					}
				}
				if (num3 == 2)
				{
					return WalkDirection.UpLeft;
				}
			}
			else if (((path.nodes[i + 1].x == path.nodes[i].x + 1 && path.nodes[i + 1].y == path.nodes[i].y) || (path.nodes[i + 1].x == path.nodes[i].x && path.nodes[i + 1].y == path.nodes[i].y - 1)) && path.nodes[i + 2].x == path.nodes[i].x + 1 && path.nodes[i + 2].y == path.nodes[i].y - 1)
			{
				int num4 = 0;
				for (int m = 0; m < path.nodes[i].NeighbouringNodeList.Count; m++)
				{
					if ((path.nodes[i].NeighbouringNodeList[m].x == path.nodes[i].x + 1 && path.nodes[i].NeighbouringNodeList[m].y == path.nodes[i].y) || (path.nodes[i].NeighbouringNodeList[m].x == path.nodes[i].x && path.nodes[i].NeighbouringNodeList[m].y == path.nodes[i].y - 1))
					{
						num4++;
					}
				}
				if (num4 == 2)
				{
					return WalkDirection.UpRight;
				}
			}
			return WalkDirection.None;
		}

		public void RefreshBubbles()
		{
			try
			{
				ResetBubbles(one: true, two: true);
				if (FarmerAStarNode != null)
				{
					FarmerAStarNodeOffset?.SetBubbleIDRecursively(0);
				}
			}
			catch
			{
				Console.WriteLine("AStarGraph.RefreshBubbles ERROR refreshing bubbles.");
			}
		}

		public void ResetBubbles(bool one = true, bool two = false)
		{
			if (map == null)
			{
				return;
			}
			int layerWidth = map.Layers[0].LayerWidth;
			int layerHeight = map.Layers[0].LayerHeight;
			for (int i = 0; i < layerWidth; i++)
			{
				for (int j = 0; j < layerHeight; j++)
				{
					_aStarNodeArray[i, j].bubbleChecked = false;
					if (one)
					{
						_aStarNodeArray[i, j].bubbleID = -1;
					}
					if (two)
					{
						_aStarNodeArray[i, j].bubbleID2 = -1;
					}
				}
			}
		}

		public void mergeBubbleID2IntoBubbleID()
		{
			int layerWidth = map.Layers[0].LayerWidth;
			int layerHeight = map.Layers[0].LayerHeight;
			for (int i = 0; i < layerWidth; i++)
			{
				for (int j = 0; j < layerHeight; j++)
				{
					if (_aStarNodeArray[i, j].bubbleID2 == 0)
					{
						_aStarNodeArray[i, j].bubbleID = 0;
						_aStarNodeArray[i, j].bubbleID2 = -1;
					}
					_aStarNodeArray[i, j].bubbleChecked = false;
				}
			}
		}

		public AStarPath GetShortestPathToNeighbouringDiagonalAStarWithBubbleCheck(AStarNode startNode, AStarNode endNode)
		{
			AStarPath shortestPathAStarWithBubbleCheck = GetShortestPathAStarWithBubbleCheck(startNode, endNode);
			if (shortestPathAStarWithBubbleCheck != null)
			{
				return shortestPathAStarWithBubbleCheck;
			}
			if (endNode.FakeTileClear)
			{
				AStarNode aStarNode = FetchAStarNode(endNode.x - 1, endNode.y - 1);
				AStarNode aStarNode2 = FetchAStarNode(endNode.x + 1, endNode.y - 1);
				AStarNode aStarNode3 = FetchAStarNode(endNode.x - 1, endNode.y + 1);
				AStarNode aStarNode4 = FetchAStarNode(endNode.x + 1, endNode.y + 1);
				double num = 1.7976931348623157E+308;
				double num2 = 1.7976931348623157E+308;
				double num3 = 1.7976931348623157E+308;
				double num4 = 1.7976931348623157E+308;
				if (aStarNode != null)
				{
					num = distance(startNode.x, aStarNode.x, startNode.y, aStarNode.y);
				}
				if (aStarNode2 != null)
				{
					num2 = distance(startNode.x, aStarNode2.x, startNode.y, aStarNode2.y);
				}
				if (aStarNode3 != null)
				{
					num3 = distance(startNode.x, aStarNode3.x, startNode.y, aStarNode3.y);
				}
				if (aStarNode4 != null)
				{
					num4 = distance(startNode.x, aStarNode4.x, startNode.y, aStarNode4.y);
				}
				if (aStarNode != null && aStarNode.TileClear && num < num2 && num < num3 && num < num4)
				{
					return GetShortestPathAStarWithBubbleCheck(startNode, aStarNode);
				}
				if (aStarNode2 != null && aStarNode2.TileClear && num2 < num && num2 < num3 && num2 < num4)
				{
					return GetShortestPathAStarWithBubbleCheck(startNode, aStarNode2);
				}
				if (aStarNode3 != null && aStarNode3.TileClear && num3 < num && num3 < num2 && num3 < num4)
				{
					return GetShortestPathAStarWithBubbleCheck(startNode, aStarNode3);
				}
				if (aStarNode4 != null && aStarNode4.TileClear)
				{
					return GetShortestPathAStarWithBubbleCheck(startNode, aStarNode4);
				}
			}
			return null;
		}

		private double distance(int x1, int x2, int y1, int y2)
		{
			return Math.Sqrt(Math.Pow(x1 - x2, 2.0) + Math.Pow(y1 - y2, 2.0));
		}

		public AStarPath GetShortestPathAStarWithBubbleCheck(AStarNode startNode, AStarNode endNode)
		{
			if (startNode == null || endNode == null)
			{
				return null;
			}
			if (endNode.bubbleID == 0)
			{
				return GetShortestPathAStar(startNode, endNode);
			}
			startNode.bubbleID = 0;
			if (endNode.bubbleID == -1 && PathBetweenNodesExists(startNode, endNode))
			{
				return GetShortestPathAStar(startNode, endNode);
			}
			ResetBubbles(one: false, two: true);
			endNode.SetBubbleIDRecursively(0, two: true);
			if (startNode.bubbleID2 == endNode.bubbleID2)
			{
				mergeBubbleID2IntoBubbleID();
				return GetShortestPathAStar(startNode, endNode);
			}
			return null;
		}

		public bool PathBetweenNodesExists(AStarNode start, AStarNode end)
		{
			if (start.bubbleID == end.bubbleID)
			{
				return true;
			}
			if (end.bubbleID == -1 && end.fakeTileClear)
			{
				AStarNode aStarNode = FetchAStarNode(end.x - 1, end.y);
				if (aStarNode != null && aStarNode.bubbleID == start.bubbleID)
				{
					return true;
				}
				aStarNode = FetchAStarNode(end.x + 1, end.y);
				if (aStarNode != null && aStarNode.bubbleID == start.bubbleID)
				{
					return true;
				}
				aStarNode = FetchAStarNode(end.x, end.y - 1);
				if (aStarNode != null && aStarNode.bubbleID == start.bubbleID)
				{
					return true;
				}
				aStarNode = FetchAStarNode(end.x, end.y + 1);
				if (aStarNode != null && aStarNode.bubbleID == start.bubbleID)
				{
					return true;
				}
			}
			return false;
		}
	}
}
