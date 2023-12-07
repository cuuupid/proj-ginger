namespace StardewValley.Mobile
{
	public class MobileKeyStates
	{
		public WalkDirection lastWalkDirection;

		public bool realTapHeld;

		public bool useToolButtonPressed;

		public bool useToolButtonReleased;

		public bool useToolHeld;

		public bool actionButtonPressed;

		public bool moveUpPressed;

		public bool moveDownPressed;

		public bool moveLeftPressed;

		public bool moveRightPressed;

		public bool moveUpReleased;

		public bool moveRightReleased;

		public bool moveDownReleased;

		public bool moveLeftReleased;

		public bool moveUpHeld;

		public bool moveRightHeld;

		public bool moveDownHeld;

		public bool moveLeftHeld;

		public void Reset()
		{
			useToolButtonPressed = false;
			useToolButtonReleased = true;
			useToolHeld = false;
			actionButtonPressed = false;
			StopMoving();
			realTapHeld = false;
		}

		public void StopMoving()
		{
			moveUpReleased = moveUpHeld;
			moveRightReleased = moveRightHeld;
			moveDownReleased = moveDownHeld;
			moveLeftReleased = moveLeftHeld;
			moveUpHeld = false;
			moveRightHeld = false;
			moveDownHeld = false;
			moveLeftHeld = false;
			moveUpPressed = false;
			moveDownPressed = false;
			moveLeftPressed = false;
			moveRightPressed = false;
		}

		public void ResetDirections()
		{
			StopMoving();
			moveUpReleased = true;
			moveRightReleased = true;
			moveDownReleased = true;
			moveLeftReleased = true;
		}

		public void ResetLeftOrRightClickButtons()
		{
			useToolButtonPressed = false;
			useToolButtonReleased = false;
			useToolHeld = false;
			actionButtonPressed = false;
			realTapHeld = false;
		}

		public void Debug()
		{
			Log.It("MobileKeyStates.Debug -> actionButtonPressed:" + actionButtonPressed + ", realTapHeld:" + realTapHeld + "\n\tup -> pressed:" + moveUpPressed + ", held:" + moveUpHeld + ", released:" + moveUpReleased + "\n\tdown -> pressed:" + moveDownPressed + ", held:" + moveDownHeld + ", released:" + moveDownReleased + "\n\tleft -> pressed:" + moveLeftPressed + ", held:" + moveLeftHeld + ", released:" + moveLeftReleased + "\n\tright -> pressed:" + moveRightPressed + ", held:" + moveRightHeld + ", released:" + moveRightReleased + "\n\tuseTool -> pressed:" + useToolButtonPressed + ", held:" + useToolHeld + ", released:" + useToolButtonReleased);
		}

		public void DebugLine()
		{
			Log.It("MobileKeyStates.Debug -> actionButtonPressed:" + actionButtonPressed + ", realTapHeld:" + realTapHeld + ", up -> pressed:" + moveUpPressed + ", held:" + moveUpHeld + ", released:" + moveUpReleased + ", down -> pressed:" + moveDownPressed + ", held:" + moveDownHeld + ", released:" + moveDownReleased + ", left -> pressed:" + moveLeftPressed + ", held:" + moveLeftHeld + ", released:" + moveLeftReleased + ", right -> pressed:" + moveRightPressed + ", held:" + moveRightHeld + ", released:" + moveRightReleased + ", useTool -> pressed:" + useToolButtonPressed + ", held:" + useToolHeld + ", released:" + useToolButtonReleased);
		}

		public void SetMovePressed(WalkDirection walkDirection)
		{
			switch (walkDirection)
			{
			case WalkDirection.Up:
				SetPressed(up: true, down: false, left: false, right: false);
				break;
			case WalkDirection.Down:
				SetPressed(up: false, down: true, left: false, right: false);
				break;
			case WalkDirection.Left:
				SetPressed(up: false, down: false, left: true, right: false);
				break;
			case WalkDirection.Right:
				SetPressed(up: false, down: false, left: false, right: true);
				break;
			case WalkDirection.UpLeft:
				SetPressed(up: true, down: false, left: true, right: false);
				break;
			case WalkDirection.UpRight:
				SetPressed(up: true, down: false, left: false, right: true);
				break;
			case WalkDirection.DownLeft:
				SetPressed(up: false, down: true, left: true, right: false);
				break;
			case WalkDirection.DownRight:
				SetPressed(up: false, down: true, left: false, right: true);
				break;
			default:
				SetPressed(up: false, down: false, left: false, right: false);
				break;
			}
			lastWalkDirection = walkDirection;
		}

		public void SetPressed(bool up, bool down, bool left, bool right)
		{
			SetUp(up);
			SetDown(down);
			SetLeft(left);
			SetRight(right);
		}

		public void SetUp(bool up)
		{
			moveUpPressed = up && !moveUpHeld;
			moveUpReleased = !up && moveUpHeld;
			moveUpHeld = up;
		}

		public void SetDown(bool down)
		{
			moveDownPressed = down && !moveDownHeld;
			moveDownReleased = !down && moveDownHeld;
			moveDownHeld = down;
		}

		public void SetLeft(bool left)
		{
			moveLeftPressed = left && !moveLeftHeld;
			moveLeftReleased = !left && moveLeftHeld;
			moveLeftHeld = left;
		}

		public void SetRight(bool right)
		{
			moveRightPressed = right && !moveRightHeld;
			moveRightReleased = !right && moveRightHeld;
			moveRightHeld = right;
		}

		public void SetUseTool(bool useTool)
		{
			useToolButtonPressed = useTool && !useToolHeld;
			useToolButtonReleased = !useTool && useToolHeld;
			useToolHeld = useTool;
		}

		public void UpdateReleasedStates()
		{
			if (useToolButtonReleased)
			{
				useToolButtonReleased = false;
			}
			if (moveUpReleased)
			{
				moveUpReleased = false;
			}
			if (moveDownReleased)
			{
				moveDownReleased = false;
			}
			if (moveLeftReleased)
			{
				moveLeftReleased = false;
			}
			if (moveRightReleased)
			{
				moveRightReleased = false;
			}
		}
	}
}
