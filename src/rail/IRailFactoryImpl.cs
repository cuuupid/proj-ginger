using System;

namespace rail
{
	public class IRailFactoryImpl : RailObject, IRailFactory
	{
		internal IRailFactoryImpl(IntPtr cPtr)
		{
			swigCPtr_ = cPtr;
		}

		~IRailFactoryImpl()
		{
		}

		public virtual IRailPlayer RailPlayer()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailPlayer(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailPlayerImpl(intPtr);
		}

		public virtual IRailUsersHelper RailUsersHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailUsersHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailUsersHelperImpl(intPtr);
		}

		public virtual IRailFriends RailFriends()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailFriends(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailFriendsImpl(intPtr);
		}

		public virtual IRailFloatingWindow RailFloatingWindow()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailFloatingWindow(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailFloatingWindowImpl(intPtr);
		}

		public virtual IRailBrowserHelper RailBrowserHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailBrowserHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailBrowserHelperImpl(intPtr);
		}

		public virtual IRailInGamePurchase RailInGamePurchase()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailInGamePurchase(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailInGamePurchaseImpl(intPtr);
		}

		public virtual IRailInGameCoin RailInGameCoin()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailInGameCoin(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailInGameCoinImpl(intPtr);
		}

		public virtual IRailRoomHelper RailRoomHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailRoomHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailRoomHelperImpl(intPtr);
		}

		public virtual IRailGameServerHelper RailGameServerHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailGameServerHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailGameServerHelperImpl(intPtr);
		}

		public virtual IRailStorageHelper RailStorageHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailStorageHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailStorageHelperImpl(intPtr);
		}

		public virtual IRailUserSpaceHelper RailUserSpaceHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailUserSpaceHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailUserSpaceHelperImpl(intPtr);
		}

		public virtual IRailStatisticHelper RailStatisticHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailStatisticHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailStatisticHelperImpl(intPtr);
		}

		public virtual IRailLeaderboardHelper RailLeaderboardHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailLeaderboardHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailLeaderboardHelperImpl(intPtr);
		}

		public virtual IRailAchievementHelper RailAchievementHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailAchievementHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailAchievementHelperImpl(intPtr);
		}

		public virtual IRailNetwork RailNetworkHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailNetworkHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailNetworkImpl(intPtr);
		}

		public virtual IRailApps RailApps()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailApps(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailAppsImpl(intPtr);
		}

		public virtual IRailGame RailGame()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailGame(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailGameImpl(intPtr);
		}

		public virtual IRailUtils RailUtils()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailUtils(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailUtilsImpl(intPtr);
		}

		public virtual IRailAssetsHelper RailAssetsHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailAssetsHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailAssetsHelperImpl(intPtr);
		}

		public virtual IRailDlcHelper RailDlcHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailDlcHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailDlcHelperImpl(intPtr);
		}

		public virtual IRailScreenshotHelper RailScreenshotHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailScreenshotHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailScreenshotHelperImpl(intPtr);
		}

		public virtual IRailVoiceHelper RailVoiceHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailVoiceHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailVoiceHelperImpl(intPtr);
		}

		public virtual IRailSystemHelper RailSystemHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailSystemHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailSystemHelperImpl(intPtr);
		}

		public virtual IRailTextInputHelper RailTextInputHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailTextInputHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailTextInputHelperImpl(intPtr);
		}

		public virtual IRailIMEHelper RailIMETextInputHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailIMETextInputHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailIMEHelperImpl(intPtr);
		}

		public virtual IRailHttpSessionHelper RailHttpSessionHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailHttpSessionHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailHttpSessionHelperImpl(intPtr);
		}

		public virtual IRailSmallObjectServiceHelper RailSmallObjectServiceHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailSmallObjectServiceHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailSmallObjectServiceHelperImpl(intPtr);
		}

		public virtual IRailZoneServerHelper RailZoneServerHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailZoneServerHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailZoneServerHelperImpl(intPtr);
		}

		public virtual IRailGroupChatHelper RailGroupChatHelper()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailFactory_RailGroupChatHelper(swigCPtr_);
			return (intPtr == IntPtr.Zero) ? null : new IRailGroupChatHelperImpl(intPtr);
		}
	}
}
