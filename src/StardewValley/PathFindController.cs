using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley
{
	[InstanceStatics]
	public class PathFindController
	{
		public delegate bool isAtEnd(PathNode currentNode, Point endPoint, GameLocation location, Character c);

		public delegate void endBehavior(Character c, GameLocation location);

		public const byte impassable = 255;

		public const int timeToWaitBeforeCancelling = 5000;

		private Character character;

		public GameLocation location;

		public Stack<Point> pathToEndPoint;

		public Point endPoint;

		public int finalFacingDirection;

		public int pausedTimer;

		public int limit;

		private isAtEnd endFunction;

		public endBehavior endBehaviorFunction;

		public bool nonDestructivePathing;

		public bool allowPlayerPathingInEvent;

		public bool NPCSchedule;

		private static readonly sbyte[,] Directions = new sbyte[4, 2]
		{
			{ -1, 0 },
			{ 1, 0 },
			{ 0, 1 },
			{ 0, -1 }
		};

		internal static PriorityQueue _openList = new PriorityQueue();

		internal static HashSet<int> _closedList = new HashSet<int>();

		internal static int _counter = 0;

		public int timerSinceLastCheckPoint;

		public PathFindController(Character c, GameLocation location, Point endPoint, int finalFacingDirection)
			: this(c, location, isAtEndPoint, finalFacingDirection, eraseOldPathController: false, null, 10000, endPoint)
		{
		}

		public PathFindController(Character c, GameLocation location, Point endPoint, int finalFacingDirection, endBehavior endBehaviorFunction)
			: this(c, location, isAtEndPoint, finalFacingDirection, eraseOldPathController: false, null, 10000, endPoint)
		{
			this.endPoint = endPoint;
			this.endBehaviorFunction = endBehaviorFunction;
		}

		public PathFindController(Character c, GameLocation location, Point endPoint, int finalFacingDirection, endBehavior endBehaviorFunction, int limit)
			: this(c, location, isAtEndPoint, finalFacingDirection, eraseOldPathController: false, null, limit, endPoint)
		{
			this.endPoint = endPoint;
			this.endBehaviorFunction = endBehaviorFunction;
		}

		public PathFindController(Character c, GameLocation location, Point endPoint, int finalFacingDirection, bool eraseOldPathController, bool clearMarriageDialogues = true)
			: this(c, location, isAtEndPoint, finalFacingDirection, eraseOldPathController, null, 10000, endPoint, clearMarriageDialogues)
		{
		}

		public static bool isAtEndPoint(PathNode currentNode, Point endPoint, GameLocation location, Character c)
		{
			if (currentNode.x == endPoint.X)
			{
				return currentNode.y == endPoint.Y;
			}
			return false;
		}

		public PathFindController(Stack<Point> pathToEndPoint, GameLocation location, Character c, Point endPoint)
		{
			this.pathToEndPoint = pathToEndPoint;
			this.location = location;
			character = c;
			this.endPoint = endPoint;
		}

		public PathFindController(Stack<Point> pathToEndPoint, Character c, GameLocation l)
		{
			this.pathToEndPoint = pathToEndPoint;
			character = c;
			location = l;
			NPCSchedule = true;
		}

		public PathFindController(Character c, GameLocation location, isAtEnd endFunction, int finalFacingDirection, bool eraseOldPathController, endBehavior endBehaviorFunction, int limit, Point endPoint, bool clearMarriageDialogues = true)
		{
			this.limit = limit;
			character = c;
			NPC nPC = c as NPC;
			if (nPC != null && nPC.CurrentDialogue.Count > 0 && nPC.CurrentDialogue.Peek().removeOnNextMove)
			{
				nPC.CurrentDialogue.Pop();
			}
			if (nPC != null && clearMarriageDialogues)
			{
				if (nPC.currentMarriageDialogue.Count > 0)
				{
					nPC.currentMarriageDialogue.Clear();
				}
				nPC.shouldSayMarriageDialogue.Value = false;
			}
			this.location = location;
			this.endFunction = ((endFunction == null) ? new isAtEnd(isAtEndPoint) : endFunction);
			this.endBehaviorFunction = endBehaviorFunction;
			if (endPoint == Point.Zero)
			{
				endPoint = new Point((int)c.getTileLocation().X, (int)c.getTileLocation().Y);
			}
			this.finalFacingDirection = finalFacingDirection;
			if (!(character is NPC) && !isPlayerPresent() && endFunction == new isAtEnd(isAtEndPoint) && endPoint.X > 0 && endPoint.Y > 0)
			{
				character.Position = new Vector2(endPoint.X * 64, endPoint.Y * 64 - 32);
				return;
			}
			pathToEndPoint = findPath(new Point((int)c.getTileLocation().X, (int)c.getTileLocation().Y), endPoint, endFunction, location, character, limit);
			if (pathToEndPoint == null)
			{
				_ = location is FarmHouse;
			}
		}

		public bool isPlayerPresent()
		{
			return location.farmers.Any();
		}

		public bool update(GameTime time)
		{
			if (pathToEndPoint == null || pathToEndPoint.Count == 0)
			{
				return true;
			}
			if (!NPCSchedule && !isPlayerPresent() && endPoint.X > 0 && endPoint.Y > 0)
			{
				character.Position = new Vector2(endPoint.X * 64, endPoint.Y * 64 - 32);
				return true;
			}
			if (Game1.activeClickableMenu == null || Game1.IsMultiplayer)
			{
				timerSinceLastCheckPoint += time.ElapsedGameTime.Milliseconds;
				Vector2 position = character.Position;
				moveCharacter(time);
				if (character.Position.Equals(position))
				{
					pausedTimer += time.ElapsedGameTime.Milliseconds;
				}
				else
				{
					pausedTimer = 0;
				}
				if (!NPCSchedule && pausedTimer > 5000)
				{
					return true;
				}
			}
			return false;
		}

		public static Stack<Point> findPath(Point startPoint, Point endPoint, isAtEnd endPointFunction, GameLocation location, Character character, int limit)
		{
			int num = Interlocked.Increment(ref _counter);
			if (num != 1)
			{
				throw new Exception();
			}
			try
			{
				bool flag = false;
				if (character is FarmAnimal && (character as FarmAnimal).type.Value == "Duck" && (character as FarmAnimal).isSwimming.Value)
				{
					flag = true;
				}
				_openList.Clear();
				_closedList.Clear();
				PriorityQueue openList = _openList;
				HashSet<int> closedList = _closedList;
				int num2 = 0;
				openList.Enqueue(new PathNode(startPoint.X, startPoint.Y, 0, null), Math.Abs(endPoint.X - startPoint.X) + Math.Abs(endPoint.Y - startPoint.Y));
				int layerWidth = location.map.Layers[0].LayerWidth;
				int layerHeight = location.map.Layers[0].LayerHeight;
				while (!openList.IsEmpty())
				{
					PathNode pathNode = openList.Dequeue();
					if (endPointFunction(pathNode, endPoint, location, character))
					{
						return reconstructPath(pathNode);
					}
					closedList.Add(pathNode.id);
					int num3 = (byte)(pathNode.g + 1);
					for (int i = 0; i < 4; i++)
					{
						int num4 = pathNode.x + Directions[i, 0];
						int num5 = pathNode.y + Directions[i, 1];
						int item = PathNode.ComputeHash(num4, num5);
						if (closedList.Contains(item))
						{
							continue;
						}
						if ((num4 != endPoint.X || num5 != endPoint.Y) && (num4 < 0 || num5 < 0 || num4 >= layerWidth || num5 >= layerHeight))
						{
							closedList.Add(item);
							continue;
						}
						PathNode pathNode2 = new PathNode(num4, num5, pathNode);
						pathNode2.g = (byte)(pathNode.g + 1);
						if (!flag && location.isCollidingPosition(new Rectangle(pathNode2.x * 64 + 1, pathNode2.y * 64 + 1, 62, 62), Game1.viewport, character is Farmer, 0, glider: false, character, pathfinding: true))
						{
							closedList.Add(item);
							continue;
						}
						int priority = num3 + (Math.Abs(endPoint.X - num4) + Math.Abs(endPoint.Y - num5));
						closedList.Add(item);
						openList.Enqueue(pathNode2, priority);
					}
					num2++;
					if (num2 >= limit)
					{
						return null;
					}
				}
				return null;
			}
			finally
			{
				if (Interlocked.Decrement(ref _counter) != 0)
				{
					throw new Exception();
				}
			}
		}

		public static Stack<Point> reconstructPath(PathNode finalNode)
		{
			Stack<Point> stack = new Stack<Point>();
			stack.Push(new Point(finalNode.x, finalNode.y));
			for (PathNode parent = finalNode.parent; parent != null; parent = parent.parent)
			{
				stack.Push(new Point(parent.x, parent.y));
			}
			return stack;
		}

		private byte[,] createMapGrid(GameLocation location, Point endPoint)
		{
			byte[,] array = new byte[location.map.Layers[0].LayerWidth, location.map.Layers[0].LayerHeight];
			for (int i = 0; i < location.map.Layers[0].LayerWidth; i++)
			{
				for (int j = 0; j < location.map.Layers[0].LayerHeight; j++)
				{
					if (!location.isCollidingPosition(new Rectangle(i * 64 + 1, j * 64 + 1, 62, 62), Game1.viewport, isFarmer: false, 0, glider: false, character, pathfinding: true))
					{
						array[i, j] = (byte)(Math.Abs(endPoint.X - i) + Math.Abs(endPoint.Y - j));
					}
					else
					{
						array[i, j] = 255;
					}
				}
			}
			return array;
		}

		private void moveCharacter(GameTime time)
		{
			Point point = pathToEndPoint.Peek();
			Rectangle rectangle = new Rectangle(point.X * 64, point.Y * 64, 64, 64);
			rectangle.Inflate(-2, 0);
			Rectangle boundingBox = character.GetBoundingBox();
			if ((rectangle.Contains(boundingBox) || (boundingBox.Width > rectangle.Width && rectangle.Contains(boundingBox.Center))) && rectangle.Bottom - boundingBox.Bottom >= 2)
			{
				timerSinceLastCheckPoint = 0;
				pathToEndPoint.Pop();
				character.stopWithoutChangingFrame();
				if (pathToEndPoint.Count == 0)
				{
					character.Halt();
					if (finalFacingDirection != -1)
					{
						character.faceDirection(finalFacingDirection);
					}
					if (NPCSchedule)
					{
						NPC nPC = character as NPC;
						nPC.DirectionsToNewLocation = null;
						nPC.endOfRouteMessage.Value = nPC.nextEndOfRouteMessage;
					}
					if (endBehaviorFunction != null)
					{
						endBehaviorFunction(character, location);
					}
				}
				return;
			}
			if (character is Farmer farmer)
			{
				farmer.movementDirections.Clear();
			}
			else if (!(location is MovieTheater))
			{
				string name = character.Name;
				foreach (NPC character in location.characters)
				{
					if (!character.Equals(this.character) && character.GetBoundingBox().Intersects(boundingBox) && character.isMoving() && string.Compare(character.Name, name, StringComparison.Ordinal) < 0)
					{
						this.character.Halt();
						return;
					}
				}
			}
			if (boundingBox.Left < rectangle.Left && boundingBox.Right < rectangle.Right)
			{
				this.character.SetMovingRight(b: true);
			}
			else if (boundingBox.Right > rectangle.Right && boundingBox.Left > rectangle.Left)
			{
				this.character.SetMovingLeft(b: true);
			}
			else if (boundingBox.Top <= rectangle.Top)
			{
				this.character.SetMovingDown(b: true);
			}
			else if (boundingBox.Bottom >= rectangle.Bottom - 2)
			{
				this.character.SetMovingUp(b: true);
			}
			this.character.MovePosition(time, Game1.viewport, location);
			if (nonDestructivePathing)
			{
				if (rectangle.Intersects(this.character.nextPosition(this.character.facingDirection)))
				{
					Vector2 vector = this.character.nextPositionVector2();
					Object objectAt = location.getObjectAt((int)vector.X, (int)vector.Y);
					if (objectAt != null)
					{
						if (objectAt is Fence fence && (bool)fence.isGate)
						{
							fence.toggleGate(location, open: true);
						}
						else if (!objectAt.isPassable())
						{
							this.character.Halt();
							this.character.controller = null;
							return;
						}
					}
				}
				handleWarps(this.character.nextPosition(this.character.getDirection()));
			}
			else if (NPCSchedule)
			{
				handleWarps(this.character.nextPosition(this.character.getDirection()));
			}
		}

		public void handleWarps(Rectangle position)
		{
			Warp warp = location.isCollidingWithWarpOrDoor(position, character);
			if (warp == null)
			{
				return;
			}
			if (warp.TargetName == "Trailer" && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				warp = new Warp(warp.X, warp.Y, "Trailer_Big", 13, 24, flipFarmer: false);
			}
			if (character is NPC && (character as NPC).isMarried() && (character as NPC).followSchedule)
			{
				NPC nPC = character as NPC;
				if (location is FarmHouse)
				{
					warp = new Warp(warp.X, warp.Y, "BusStop", 0, 23, flipFarmer: false);
				}
				if (location is BusStop && warp.X <= 0)
				{
					warp = new Warp(warp.X, warp.Y, nPC.getHome().name, (nPC.getHome() as FarmHouse).getEntryLocation().X, (nPC.getHome() as FarmHouse).getEntryLocation().Y, flipFarmer: false);
				}
				if (nPC.temporaryController != null && nPC.controller != null)
				{
					nPC.controller.location = Game1.getLocationFromName(warp.TargetName);
				}
			}
			location = Game1.getLocationFromName(warp.TargetName);
			if (character is NPC && (warp.TargetName == "FarmHouse" || warp.TargetName == "Cabin") && (character as NPC).isMarried() && (character as NPC).getSpouse() != null)
			{
				location = Utility.getHomeOfFarmer((character as NPC).getSpouse());
				warp = new Warp(warp.X, warp.Y, location.name, (location as FarmHouse).getEntryLocation().X, (location as FarmHouse).getEntryLocation().Y, flipFarmer: false);
				if ((character as NPC).temporaryController != null && (character as NPC).controller != null)
				{
					(character as NPC).controller.location = location;
				}
				Game1.warpCharacter(character as NPC, location, new Vector2(warp.TargetX, warp.TargetY));
			}
			else
			{
				Game1.warpCharacter(character as NPC, warp.TargetName, new Vector2(warp.TargetX, warp.TargetY));
			}
			if (isPlayerPresent() && location.doors.ContainsKey(new Point(warp.X, warp.Y)))
			{
				location.playSoundAt("doorClose", new Vector2(warp.X, warp.Y), NetAudio.SoundContext.NPC);
			}
			if (isPlayerPresent() && location.doors.ContainsKey(new Point(warp.TargetX, warp.TargetY - 1)))
			{
				location.playSoundAt("doorClose", new Vector2(warp.TargetX, warp.TargetY), NetAudio.SoundContext.NPC);
			}
			if (pathToEndPoint.Count > 0)
			{
				pathToEndPoint.Pop();
			}
			while (pathToEndPoint.Count > 0 && (Math.Abs(pathToEndPoint.Peek().X - character.getTileX()) > 1 || Math.Abs(pathToEndPoint.Peek().Y - character.getTileY()) > 1))
			{
				pathToEndPoint.Pop();
			}
		}

		public static bool IsPositionImpassableOnFarm(GameLocation loc, int x, int y)
		{
			if (loc is Farm farm)
			{
				NPC.isCheckingSpouseTileOccupancy = true;
				if (farm.isTileOccupied(new Vector2(x, y), "", ignoreAllCharacters: true))
				{
					NPC.isCheckingSpouseTileOccupancy = false;
					return true;
				}
				NPC.isCheckingSpouseTileOccupancy = false;
				if (farm.getBuildingAt(new Vector2(x, y)) != null)
				{
					return true;
				}
			}
			return isPositionImpassableForNPCSchedule(loc, x, y);
		}

		public static Stack<Point> FindPathOnFarm(Point startPoint, Point endPoint, GameLocation location, int limit)
		{
			Dictionary<Vector2, int> weight_map = new Dictionary<Vector2, int>();
			PriorityQueue priorityQueue = new PriorityQueue();
			HashSet<int> hashSet = new HashSet<int>();
			int num = 0;
			priorityQueue.Enqueue(new PathNode(startPoint.X, startPoint.Y, 0, null), Math.Abs(endPoint.X - startPoint.X) + Math.Abs(endPoint.Y - startPoint.Y));
			PathNode pathNode = (PathNode)priorityQueue.Peek();
			int layerWidth = location.map.Layers[0].LayerWidth;
			int layerHeight = location.map.Layers[0].LayerHeight;
			while (!priorityQueue.IsEmpty())
			{
				PathNode pathNode2 = priorityQueue.Dequeue();
				if (pathNode2.x == endPoint.X && pathNode2.y == endPoint.Y)
				{
					return reconstructPath(pathNode2);
				}
				hashSet.Add(pathNode2.id);
				for (int i = 0; i < 4; i++)
				{
					int num2 = pathNode2.x + Directions[i, 0];
					int num3 = pathNode2.y + Directions[i, 1];
					int item = PathNode.ComputeHash(num2, num3);
					if (hashSet.Contains(item))
					{
						continue;
					}
					PathNode pathNode3 = new PathNode(num2, num3, pathNode2);
					pathNode3.g = (byte)(pathNode2.g + 1);
					if ((pathNode3.x == endPoint.X && pathNode3.y == endPoint.Y) || (pathNode3.x >= 0 && pathNode3.y >= 0 && pathNode3.x < layerWidth && pathNode3.y < layerHeight && !IsPositionImpassableOnFarm(location, pathNode3.x, pathNode3.y)))
					{
						int num4 = pathNode3.g + GetFarmTileWeight(location, pathNode3.x, pathNode3.y, weight_map) + (Math.Abs(endPoint.X - pathNode3.x) + Math.Abs(endPoint.Y - pathNode3.y));
						if (pathNode.x - pathNode2.x == pathNode2.x - num2 && pathNode.y - pathNode2.y == pathNode2.y - num3)
						{
							num4 -= 25;
						}
						if (!priorityQueue.Contains(pathNode3, num4))
						{
							priorityQueue.Enqueue(pathNode3, num4);
						}
					}
				}
				pathNode = pathNode2;
				num++;
				if (num >= limit)
				{
					return null;
				}
			}
			return null;
		}

		public static int GetFarmTileWeight(GameLocation location, int x, int y, Dictionary<Vector2, int> weight_map)
		{
			Vector2 key = new Vector2(x, y);
			if (weight_map.ContainsKey(key))
			{
				return weight_map[key];
			}
			int num = 0;
			Object objectAtTile = location.getObjectAtTile(x, y);
			if (objectAtTile != null && !objectAtTile.isPassable() && (!(objectAtTile is Fence fence) || !fence.isGate))
			{
				num = 9999;
			}
			if (location.terrainFeatures.TryGetValue(key, out var value) && value is Flooring)
			{
				num -= 50;
			}
			weight_map[key] = num;
			return num;
		}

		public static int CheckClearance(GameLocation location, Rectangle rect)
		{
			int num = 0;
			for (num = 0; num < 5; num++)
			{
				bool flag = false;
				for (int i = rect.Left; i < rect.Right; i++)
				{
					for (int j = rect.Top; j < rect.Bottom; j++)
					{
						Object objectAtTile = location.getObjectAtTile(i, j);
						if (objectAtTile != null && !objectAtTile.isPassable())
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					return num;
				}
			}
			return num;
		}

		public static Stack<Point> findPathForNPCSchedules(Point startPoint, Point endPoint, GameLocation location, int limit)
		{
			PriorityQueue priorityQueue = new PriorityQueue();
			HashSet<int> hashSet = new HashSet<int>();
			int num = 0;
			priorityQueue.Enqueue(new PathNode(startPoint.X, startPoint.Y, 0, null), Math.Abs(endPoint.X - startPoint.X) + Math.Abs(endPoint.Y - startPoint.Y));
			PathNode pathNode = (PathNode)priorityQueue.Peek();
			int layerWidth = location.map.Layers[0].LayerWidth;
			int layerHeight = location.map.Layers[0].LayerHeight;
			while (!priorityQueue.IsEmpty())
			{
				PathNode pathNode2 = priorityQueue.Dequeue();
				if (pathNode2.x == endPoint.X && pathNode2.y == endPoint.Y)
				{
					return reconstructPath(pathNode2);
				}
				hashSet.Add(pathNode2.id);
				for (int i = 0; i < 4; i++)
				{
					int x = pathNode2.x + Directions[i, 0];
					int y = pathNode2.y + Directions[i, 1];
					int item = PathNode.ComputeHash(x, y);
					if (hashSet.Contains(item))
					{
						continue;
					}
					PathNode pathNode3 = new PathNode(x, y, pathNode2);
					pathNode3.g = (byte)(pathNode2.g + 1);
					if ((pathNode3.x == endPoint.X && pathNode3.y == endPoint.Y) || (pathNode3.x >= 0 && pathNode3.y >= 0 && pathNode3.x < layerWidth && pathNode3.y < layerHeight && !isPositionImpassableForNPCSchedule(location, pathNode3.x, pathNode3.y)))
					{
						int priority = pathNode3.g + getPreferenceValueForTerrainType(location, pathNode3.x, pathNode3.y) + (Math.Abs(endPoint.X - pathNode3.x) + Math.Abs(endPoint.Y - pathNode3.y) + (((pathNode3.x == pathNode2.x && pathNode3.x == pathNode.x) || (pathNode3.y == pathNode2.y && pathNode3.y == pathNode.y)) ? (-2) : 0));
						if (!priorityQueue.Contains(pathNode3, priority))
						{
							priorityQueue.Enqueue(pathNode3, priority);
						}
					}
				}
				pathNode = pathNode2;
				num++;
				if (num >= limit)
				{
					return null;
				}
			}
			return null;
		}

		private static bool isPositionImpassableForNPCSchedule(GameLocation loc, int x, int y)
		{
			Layer layer = loc.Map.GetLayer("Buildings");
			Tile tile = layer.Tiles[x, y];
			if (tile != null && tile.TileIndex != -1)
			{
				PropertyValue value = null;
				string text = null;
				tile.TileIndexProperties.TryGetValue("Action", out value);
				if (value == null)
				{
					tile.Properties.TryGetValue("Action", out value);
				}
				if (value != null)
				{
					text = value.ToString();
					if (text.StartsWith("LockedDoorWarp"))
					{
						return true;
					}
					if (!text.Contains("Door") && !text.Contains("Passable"))
					{
						return true;
					}
				}
				else if (loc.doesTileHaveProperty(x, y, "Passable", "Buildings") == null && loc.doesTileHaveProperty(x, y, "NPCPassable", "Buildings") == null)
				{
					return true;
				}
			}
			if (loc.doesTileHaveProperty(x, y, "NoPath", "Back") != null)
			{
				return true;
			}
			foreach (Warp warp in loc.warps)
			{
				if (warp.X == x && warp.Y == y)
				{
					return true;
				}
			}
			if (loc.isTerrainFeatureAt(x, y))
			{
				return true;
			}
			return false;
		}

		private static int getPreferenceValueForTerrainType(GameLocation l, int x, int y)
		{
			string text = l.doesTileHaveProperty(x, y, "Type", "Back");
			if (text != null)
			{
				switch (text.ToLower())
				{
				case "stone":
					return -7;
				case "wood":
					return -4;
				case "dirt":
					return -2;
				case "grass":
					return -1;
				}
			}
			return 0;
		}
	}
}
