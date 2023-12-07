using System;
using System.Collections.Generic;

namespace StardewValley.Mobile
{
	public class AStarPath
	{
		protected List<AStarNode> _nodeList = new List<AStarNode>();

		protected float _length;

		public virtual List<AStarNode> nodes
		{
			get
			{
				return _nodeList;
			}
			set
			{
				_nodeList = value;
			}
		}

		public virtual float length => _length;

		public virtual void Bake()
		{
			List<AStarNode> list = new List<AStarNode>();
			_length = 0f;
			for (int i = 0; i < _nodeList.Count; i++)
			{
				AStarNode aStarNode = _nodeList[i];
				for (int j = 0; j < aStarNode.NeighbouringNodeList.Count; j++)
				{
					AStarNode aStarNode2 = aStarNode.NeighbouringNodeList[j];
					if (_nodeList.Contains(aStarNode2) && !list.Contains(aStarNode2))
					{
						_length += Distance(aStarNode.x, aStarNode.y, aStarNode2.x, aStarNode2.y);
					}
				}
				list.Add(aStarNode);
			}
		}

		private float Distance(int x1, int y1, int x2, int y2)
		{
			return (float)(Math.Pow(x1 - x2, 2.0) + Math.Pow(y1 - y2, 2.0));
		}

		public override string ToString()
		{
			try
			{
				if (nodes == null || nodes.Count == 0)
				{
					return "No path";
				}
				string text = "[";
				for (int i = 0; i < nodes.Count; i++)
				{
					text = text + "(" + nodes[i].x + "," + nodes[i].y + "), ";
				}
				return text.Substring(0, text.Length - 2) + "], Length:" + nodes.Count;
			}
			catch
			{
				Log.It("AStarPath.ToString ERROR -> Path is null");
				return "No path ERROR";
			}
		}

		public AStarNode containsClosedGate()
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i].isGate() && !nodes[i].isGateOpen())
				{
					return nodes[i];
				}
			}
			return null;
		}

		public AStarNode ContainsGate()
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i].isGate())
				{
					return nodes[i];
				}
			}
			return null;
		}
	}
}
