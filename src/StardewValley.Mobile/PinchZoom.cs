using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Mobile
{
	public sealed class PinchZoom
	{
		private const float MAX_ZOOM = 4f;

		private float _startDistanceBetweenTouchPoints = 3.4028235E+38f;

		private float _startRealDistance;

		private float _pinchZoomLevel = 1f;

		private bool _isPinching;

		private float _farmerStartX = 3.4028235E+38f;

		private float _farmerStartY = 3.4028235E+38f;

		private float _screenCenterX = 3.4028235E+38f;

		private float _screenCenterY = 3.4028235E+38f;

		private float _pinchCenterX = 3.4028235E+38f;

		private float _pinchCenterY = 3.4028235E+38f;

		private float _startPinchPercentX;

		private float _startPinchPercentY;

		private Vector2 _pinchPointA;

		private Vector2 _pinchPointB;

		private Vector2 _startPinchMidPoint;

		private Vector2 _pinchMidPoint;

		private float _dragDistanceX;

		private float _dragDistanceY;

		private float _newViewportWidth;

		private float _newViewportHeight;

		private float _lastPinchZoomLevel;

		private static PinchZoom _instance = null;

		private static readonly object _padlock = new object();

		public bool justPinchZoomed;

		private float _dragDistanceXSinceLastUpdate;

		private float _dragDistanceYSinceLastUpdate;

		private Vector2 _oldViewportTarget;

		public static PinchZoom Instance
		{
			get
			{
				lock (_padlock)
				{
					if (_instance == null)
					{
						_instance = new PinchZoom();
						if (Game1.options != null)
						{
							_instance._pinchZoomLevel = Game1.options.zoomLevel;
						}
					}
					return _instance;
				}
			}
		}

		public float MinZoom => Math.Max((float)Game1.clientBounds.Width / 4096f, (float)Game1.clientBounds.Height / 4096f);

		public float ZoomLevel
		{
			get
			{
				return _pinchZoomLevel;
			}
			set
			{
				if (ZoomingAllowed)
				{
					_pinchZoomLevel = Math.Max(MinZoom, Math.Min(value, 4f));
				}
			}
		}

		public bool Pinching => _isPinching;

		private bool ZoomingAllowed
		{
			get
			{
				if ((Game1.options.weaponControl == 2 || Game1.options.weaponControl == 3) && Game1.virtualJoypad.TouchingTwoOrMoreButtons)
				{
					_startDistanceBetweenTouchPoints = 3.4028235E+38f;
					return false;
				}
				if (TutorialManager.Instance.attackDialog != null)
				{
					return false;
				}
				if (Game1.currentMinigame != null)
				{
					return false;
				}
				if (Game1.virtualJoypad.TouchingJoystickOrButton && Game1.options.weaponControl != 2 && Game1.options.weaponControl != 3)
				{
					_startDistanceBetweenTouchPoints = 3.4028235E+38f;
					return false;
				}
				if (Game1.CurrentEvent != null && !Game1.CurrentEvent.isFestival)
				{
					return false;
				}
				if (!(Game1.activeClickableMenu is CarpenterMenu) && !(Game1.activeClickableMenu is AnimalQueryMenu) && !(Game1.activeClickableMenu is PurchaseAnimalsMenu) && (!Game1.options.pinchZoom || Game1.viewportFreeze || Game1.currentLocation is MermaidHouse))
				{
					return false;
				}
				return true;
			}
		}

		public void SetZoomLevel(float zoom)
		{
			_pinchZoomLevel = zoom;
		}

		public bool CheckForPinchZoom()
		{
			bool isPinching = _isPinching;
			_isPinching = false;
			if (!ZoomingAllowed)
			{
				return false;
			}
			bool flag = false;
			Vector2 vector = Vector2.Zero;
			Vector2 vector2 = Vector2.Zero;
			TouchCollection getTouchState = Game1.input.GetTouchState;
			flag = getTouchState.Count == 2;
			if (flag)
			{
				vector = getTouchState[0].Position;
				vector2 = getTouchState[1].Position;
			}
			if (flag)
			{
				float num = Vector2.Distance(vector, vector2);
				if (_startDistanceBetweenTouchPoints == 3.4028235E+38f)
				{
					_startDistanceBetweenTouchPoints = num;
					_startRealDistance = _startDistanceBetweenTouchPoints / Game1.options.zoomLevel;
					_farmerStartX = Game1.player.Position.X;
					_farmerStartY = Game1.player.Position.Y;
					_screenCenterX = Game1.viewport.X + Game1.viewport.Width / 2;
					_screenCenterY = Game1.viewport.Y + Game1.viewport.Height / 2;
					float num2 = (vector.X + vector2.X) / 2f;
					float num3 = (vector.Y + vector2.Y) / 2f;
					_pinchCenterX = (float)Game1.viewport.X + num2 / Game1.options.zoomLevel;
					_pinchCenterY = (float)Game1.viewport.Y + num3 / Game1.options.zoomLevel;
					_pinchPointA = vector;
					_pinchPointB = vector2;
					_startPinchMidPoint = (_pinchMidPoint = new Vector2((vector.X + vector2.X) / 2f, (vector.Y + vector2.Y) / 2f));
					_startPinchPercentX = _startPinchMidPoint.X / (float)Game1.clientBounds.Width;
					_startPinchPercentY = _startPinchMidPoint.Y / (float)Game1.clientBounds.Height;
					_lastPinchZoomLevel = Game1.options.zoomLevel;
				}
				else
				{
					_pinchZoomLevel = num / _startRealDistance;
					_pinchZoomLevel = Math.Max(MinZoom, Math.Min(_pinchZoomLevel, 4f));
					Vector2 pinchMidPoint = _pinchMidPoint;
					_pinchMidPoint = new Vector2((vector.X + vector2.X) / 2f, (vector.Y + vector2.Y) / 2f);
					_dragDistanceX = (_startPinchMidPoint.X - _pinchMidPoint.X) / _pinchZoomLevel;
					_dragDistanceY = (_startPinchMidPoint.Y - _pinchMidPoint.Y) / _pinchZoomLevel;
					_dragDistanceXSinceLastUpdate = (_pinchMidPoint.X - pinchMidPoint.X) / _pinchZoomLevel;
					_dragDistanceYSinceLastUpdate = (_pinchMidPoint.Y - pinchMidPoint.Y) / _pinchZoomLevel;
					int x = Game1.viewport.X;
					int y = Game1.viewport.Y;
					float lastPinchZoomLevel = _lastPinchZoomLevel;
					if (_pinchZoomLevel != _lastPinchZoomLevel)
					{
						_lastPinchZoomLevel = _pinchZoomLevel;
						Game1.options.desiredBaseZoomLevel = _pinchZoomLevel;
						_isPinching = false;
						Game1.game1.refreshWindowSettings();
						_isPinching = true;
					}
					Center();
				}
				_isPinching = true;
				justPinchZoomed = true;
				if (Game1.currentLocation != null && Game1.currentLocation.tapToMove.Moving)
				{
					Game1.currentLocation.tapToMove.Reset();
				}
				return true;
			}
			if (_startDistanceBetweenTouchPoints != 3.4028235E+38f)
			{
				_startDistanceBetweenTouchPoints = 3.4028235E+38f;
			}
			return false;
		}

		public void Center()
		{
			_newViewportWidth = (float)Game1.clientBounds.Width / Game1.options.zoomLevel;
			_newViewportHeight = (float)Game1.clientBounds.Height / Game1.options.zoomLevel;
			_oldViewportTarget = Game1.currentViewportTarget;
			CenterOnPinch();
			Game1.currentViewportTarget = Game1.ClampViewportCornerToGameMap(Game1.currentViewportTarget);
			Game1.viewportPositionLerp = (Game1.previousViewportPosition = Game1.currentViewportTarget);
			Game1.viewport.X = (int)Game1.currentViewportTarget.X;
			Game1.viewport.Y = (int)Game1.currentViewportTarget.Y;
			Game1.forceSnapOnNextViewportUpdate = true;
		}

		public void CenterOnPinch()
		{
			Game1.currentViewportTarget.X = _pinchCenterX - _newViewportWidth * _startPinchPercentX + _dragDistanceX;
			Game1.currentViewportTarget.Y = _pinchCenterY - _newViewportHeight * _startPinchPercentY + _dragDistanceY;
		}

		public void CenterOnScreen()
		{
			Game1.currentViewportTarget.X = _screenCenterX - _newViewportWidth * 0.5f + _dragDistanceX;
			Game1.currentViewportTarget.Y = _screenCenterY - _newViewportHeight * 0.5f + _dragDistanceY;
		}

		public void CenterOnFarmer()
		{
			Game1.currentViewportTarget.X = _farmerStartX - _newViewportWidth * 0.5f + _dragDistanceX;
			Game1.currentViewportTarget.Y = _farmerStartY - _newViewportHeight * 0.5f + _dragDistanceY;
		}
	}
}
