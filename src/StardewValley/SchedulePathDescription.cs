using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley
{
	public class SchedulePathDescription
	{
		public Stack<Point> route;

		public Stack<string> locationNames;

		public int facingDirection;

		public string endOfRouteBehavior;

		public string endOfRouteMessage;

		public SchedulePathDescription(Stack<Point> route, int facingDirection, string endBehavior, string endMessage, Stack<string> locationNames)
		{
			endOfRouteMessage = endMessage;
			this.route = route;
			this.locationNames = locationNames;
			this.facingDirection = facingDirection;
			endOfRouteBehavior = endBehavior;
		}
	}
}
